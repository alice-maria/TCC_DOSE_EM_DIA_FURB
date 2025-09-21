using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoseEmDia.Models;
using DoseEmDia.Models.db;
using DoseEmDia.Controllers.Helpers;
using DoseEmDia.Helpers;
using DoseEmDia.Models.Exceptions;
using DoseEmDia.Controllers.DTO;
using DoseEmDia.Controllers;
using DoseEmDia.Models.Enums;
using Npgsql;
using DoseEmDia.Models.Localizacao;

public class UsuarioService
{
    private readonly ApplicationDbContext _context;
    private readonly EnvioEmail _envioEmail;
    private readonly VacinaService _vacinaService;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(ApplicationDbContext context, EnvioEmail envioEmail, VacinaService vacinaService, ILogger<UsuarioService> logger)
    {
        _context = context;
        _envioEmail = envioEmail;
        _vacinaService = vacinaService;
        _logger = logger;
    }

    public async Task<Usuario> BuscarPorCpf(string cpf)
    {
        var usuario = await _context.Usuario
            .Include(u => u.Endereco)
            .FirstOrDefaultAsync(u => u.CPF == cpf);

        return usuario;
    }

    public async Task<Usuario> CriarUsuario(Usuario request, string paisNome, string estadoUf, string cidadeNome, string cepCodigo, string logradouro, string numero, string? bairro, CancellationToken ct = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("E-mail é obrigatório.");
        if (string.IsNullOrWhiteSpace(request.Senha)) throw new ArgumentException("Senha é obrigatória.");

        request.Nome = request.Nome?.Trim();
        request.Email = request.Email.Trim().ToLowerInvariant();
        request.CPF = FormatacaoHelper.FormataCPF(request.CPF);
        request.Telefone = FormatacaoHelper.FormataTelefone(request.Telefone);

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var emailExiste = await _context.Usuario
                    .AnyAsync(u => EF.Functions.ILike(u.Email, request.Email), ct);
                if (emailExiste)
                    throw new UsuarioException.EmailJaCadastradoException(request.Email);

                var endereco = await CriarEnderecoAsync(
                    paisNome, estadoUf, cidadeNome, cepCodigo,
                    logradouro, numero, bairro, ct);

                var salt = CriptografiaHelper.GerarSalt();
                var hash = CriptografiaHelper.GerarHash(request.Senha, salt);

                var usuario = new Usuario
                {
                    Nome = request.Nome,
                    DataNascimento = request.DataNascimento,
                    Email = request.Email,
                    Telefone = request.Telefone,
                    CPF = request.CPF,
                    Sexo = request.Sexo,
                    Senha = hash,
                    Salt = salt,
                    EnderecoId = endereco.IdEndereco,
                    ReceberNotificacoes = request.ReceberNotificacoes
                };

                _context.Usuario.Add(usuario);
                await _context.SaveChangesAsync(ct);

                var idade = CalcularIdade(usuario.DataNascimento);
                await _vacinaService.GerarEVincularVacinas(usuario.IdUser, idade, usuario.Sexo);

                await tx.CommitAsync(ct);

                usuario.Senha = null!;
                usuario.Salt = null!;
                return usuario;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
            {
                await tx.RollbackAsync(ct);
                throw new UsuarioException.EmailJaCadastradoException(request.Email);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }

    public async Task<Usuario> Login(LoginRequest request)
    {
        var usuario = await _context.Usuario
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (usuario == null)
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

        bool senhaValida = CriptografiaHelper.VerificarSenha(request.Senha, usuario.Senha, usuario.Salt);

        if (!senhaValida)
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

        return usuario;
    }

    public async Task ExcluirConta(ExcluirContaRequest request)
    {
        var usuario = await _context.Usuario
            .Include(u => u.Endereco)
            .Include(u => u.Vacinas)
            .Include(u => u.Notificacoes)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (usuario == null)
            throw new UsuarioException.UsuarioNaoEncontradoException(request.Email);

        bool senhaValida = CriptografiaHelper.VerificarSenha(request.Senha, usuario.Senha, usuario.Salt);
        if (!senhaValida)
            throw new UnauthorizedAccessException("Senha incorreta.");

        if (usuario.Notificacoes?.Any() == true)
            _context.Notificacao.RemoveRange(usuario.Notificacoes);

        if (usuario.Vacinas?.Any() == true)
            _context.Vacina.RemoveRange(usuario.Vacinas);

        if (usuario.Endereco != null)
            _context.Endereco.Remove(usuario.Endereco);

        _context.Usuario.Remove(usuario);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> EsqueciSenha(string email, CancellationToken ct = default)
    {
        var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == email, ct);

        if (usuario is null)
            return true;

        var token = _envioEmail.GerarToken();
        usuario.TokenRedefinicaoSenha = token;
        var expiresUtc = DateTime.UtcNow.AddMinutes(15);
        usuario.TokenExpiracao = DateTime.SpecifyKind(expiresUtc, DateTimeKind.Unspecified);

        await _context.SaveChangesAsync(ct);

        var emailEnviado = false;
        try
        {
            var envioTask = _envioEmail.EnviarEmailRedefinicaoSenhaAsync(usuario.Email, token, ct);
            var completed = await Task.WhenAny(envioTask, Task.Delay(TimeSpan.FromSeconds(15), ct)) == envioTask;

            if (completed)
            {
                await envioTask;
                emailEnviado = true;
            }
            else
            {
                _logger.LogWarning("Timeout ao enviar e-mail de redefinição para {Email}", email);
            }
        }
        catch (EmailException ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail de redefinição para {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar esqueciSenha para {Email}", email);
        }

        var cincoMinutosAtras = DateTime.UtcNow.AddMinutes(-5);

        var haRegistroRecente = await _context.Notificacao.AnyAsync(n =>
            n.UsuarioId == usuario.IdUser &&
            n.Tipo == TipoNotificacao.RedefinicaoSenha &&
            n.DataEnvio > cincoMinutosAtras, ct);

        if (!haRegistroRecente)
        {
            await RegistrarNotificacaoAsync(
                usuario.IdUser,
                TipoNotificacao.RedefinicaoSenha,
                "Redefinição de senha solicitada",
                "Enviamos instruções para redefinir sua senha (verifique também o spam).",
                emailEnviado
            );
        }

        return true; // resposta sempre “ok” para o chamador
    }

    public async Task RedefinirSenha(RedefinirSenhaRequest request)
    {
        var usuario = await _context.Usuario
            .FirstOrDefaultAsync(u => u.TokenRedefinicaoSenha == request.Token && u.TokenExpiracao > DateTime.UtcNow);

        if (usuario == null)
            throw new UsuarioException.TokenInvalidoOuExpiradoException();

        var novoSalt = CriptografiaHelper.GerarSalt();
        var novoHash = CriptografiaHelper.GerarHash(request.NovaSenha, novoSalt);

        usuario.Senha = novoHash;
        usuario.Salt = novoSalt;

        usuario.TokenRedefinicaoSenha = null;
        usuario.TokenExpiracao = null;

        await _context.SaveChangesAsync();
    }

    public async Task AlterarSenha(AlterarSenhaRequest request)
    {
        var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (usuario == null)
            throw new UsuarioException.UsuarioNaoEncontradoException(request.Email);

        var hashSenhaAtual = CriptografiaHelper.GerarHash(request.SenhaAtual, usuario.Salt);
        if (usuario.Senha != hashSenhaAtual)
            throw new UnauthorizedAccessException("Senha incorreta.");

        var hashNovaSenhaComSaltAntigo = CriptografiaHelper.GerarHash(request.NovaSenha, usuario.Salt);
        if (usuario.Senha == hashNovaSenhaComSaltAntigo)
            throw new InvalidOperationException("A nova senha deve ser diferente da atual.");

        var novoSalt = CriptografiaHelper.GerarSalt();
        var novoHash = CriptografiaHelper.GerarHash(request.NovaSenha, novoSalt);

        usuario.Salt = novoSalt;
        usuario.Senha = novoHash;

        await _context.SaveChangesAsync();
    }


    public async Task AtualizarUsuario(int id, AtualizarUsuario request)
    {
        var usuario = await _context.Usuario
            .Include(u => u.Endereco)
            .FirstOrDefaultAsync(u => u.IdUser == id);

        if (usuario == null)
            throw new UsuarioException.UsuarioNaoEncontradoException(id);

        if (!string.IsNullOrWhiteSpace(request.Nome))
            usuario.Nome = request.Nome;

        if (request.DataNascimento.HasValue)
            usuario.DataNascimento = request.DataNascimento.Value;

        if (!string.IsNullOrWhiteSpace(request.Telefone))
            usuario.Telefone = FormatacaoHelper.FormataTelefone(request.Telefone);

        if (!string.IsNullOrWhiteSpace(request.Email))
            usuario.Email = request.Email;

        if (!string.IsNullOrWhiteSpace(request.Sexo))
            usuario.Sexo = request.Sexo;

        if (request.Endereco != null)
        {
            if (!string.IsNullOrWhiteSpace(request.Endereco.CEP))
            {
                var cepCodigo = FormatacaoHelper.FormataCEP(request.Endereco.CEP);

                var cep = await _context.Cep
                    .FirstOrDefaultAsync(c => c.Codigo == cepCodigo);

                if (cep is null)
                    throw new CepNaoEncontradoException(cepCodigo);

                if (usuario.Endereco is null)
                    usuario.Endereco = new Endereco();

                usuario.Endereco.CepId = cep.IdCep;
            }

            if (usuario.Endereco is null)
                usuario.Endereco = new Endereco();

            if (!string.IsNullOrWhiteSpace(request.Endereco.Numero))
                usuario.Endereco.Numero = request.Endereco.Numero;

            if (!string.IsNullOrWhiteSpace(request.Endereco.Logradouro))
                usuario.Endereco.Logradouro = request.Endereco.Logradouro;
        }

        await _context.SaveChangesAsync();
    }

    [HttpGet("{id}")]
    public async Task<Usuario> ObterUsuarioPorId(int id)
    {
        var usuario = await _context.Usuario.FindAsync(id);
        if (usuario == null)
            throw new UsuarioException.UsuarioNaoEncontradoException(id);

        return usuario;
    }

    private int CalcularIdade(DateTime dataNascimento)
    {
        var hoje = DateTime.Today;
        var idade = hoje.Year - dataNascimento.Year;
        if (dataNascimento > hoje.AddYears(-idade)) idade--;
        return idade;
    }

    private async Task RegistrarNotificacaoAsync(int usuarioId, TipoNotificacao tipo, string titulo, string mensagem, bool emailEnviado)
    {
        _context.Notificacao.Add(new Notificacao
        {
            UsuarioId = usuarioId,
            Tipo = tipo,
            Titulo = titulo,
            Mensagem = mensagem,
            DataEnvio = DateTime.UtcNow,
            Visualizada = false,
            EmailEnviado = emailEnviado
        });

        await _context.SaveChangesAsync();
    }

    private async Task<Endereco> CriarEnderecoAsync(
    string paisNome,
    string estadoUf,
    string cidadeNome,
    string cepCodigoRaw,
    string logradouro,
    string numero,
    string? bairro,
    CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(paisNome)) throw new ArgumentException("País é obrigatório.", nameof(paisNome));
        if (string.IsNullOrWhiteSpace(estadoUf) || estadoUf.Trim().Length != 2)
            throw new ArgumentException("UF deve ter 2 letras.", nameof(estadoUf));
        if (string.IsNullOrWhiteSpace(cidadeNome)) throw new ArgumentException("Cidade é obrigatória.", nameof(cidadeNome));
        if (string.IsNullOrWhiteSpace(cepCodigoRaw)) throw new ArgumentException("CEP é obrigatório.", nameof(cepCodigoRaw));
        if (string.IsNullOrWhiteSpace(numero)) throw new ArgumentException("Número é obrigatório.", nameof(numero));

        var cepCodigo = FormatacaoHelper.FormataCEP(cepCodigoRaw);
        var uf = estadoUf.Trim().ToUpperInvariant();

        var pais = await _context.Pais
            .FirstOrDefaultAsync(p => EF.Functions.ILike(p.Nome, paisNome.Trim()), ct)
            ?? (await _context.Pais.AddAsync(new Pais { Nome = paisNome.Trim() }, ct)).Entity;
        await _context.SaveChangesAsync(ct);

        var estado = await _context.Estado
            .FirstOrDefaultAsync(e => e.PaisId == pais.IdPais && EF.Functions.ILike(e.Nome, uf), ct);
        if (estado is null)
        {
            estado = new Estado { PaisId = pais.IdPais, Nome = uf, Uf = uf };
            _context.Estado.Add(estado);
            await _context.SaveChangesAsync(ct);
        }
        else if (!string.Equals(estado.Uf, uf, StringComparison.OrdinalIgnoreCase))
        {
            estado.Uf = uf;
            _context.Estado.Update(estado);
            await _context.SaveChangesAsync(ct);
        }

        var cidade = await _context.Cidade
            .FirstOrDefaultAsync(c => c.EstadoId == estado.IdEstado && EF.Functions.ILike(c.Nome, cidadeNome.Trim()), ct)
            ?? (await _context.Cidade.AddAsync(new Cidade { EstadoId = estado.IdEstado, Nome = cidadeNome.Trim() }, ct)).Entity;
        await _context.SaveChangesAsync(ct);

        var cep = await _context.Cep.FirstOrDefaultAsync(c => c.Codigo == cepCodigo, ct);
        if (cep is null)
        {
            cep = new Cep { Codigo = cepCodigo, CidadeId = cidade.IdCidade, Bairro = bairro?.Trim() };
            _context.Cep.Add(cep);
            await _context.SaveChangesAsync(ct);
        }
        else
        {
            bool mudou = false;
            if (cep.CidadeId != cidade.IdCidade) { cep.CidadeId = cidade.IdCidade; mudou = true; }
            if (!string.IsNullOrWhiteSpace(bairro) && !string.Equals(cep.Bairro, bairro, StringComparison.Ordinal))
            { cep.Bairro = bairro.Trim(); mudou = true; }
            if (mudou) { _context.Cep.Update(cep); await _context.SaveChangesAsync(ct); }
        }

        var end = new Endereco
        {
            CepId = cep.IdCep,
            Logradouro = logradouro?.Trim(),
            Numero = (numero?.Trim() ?? string.Empty).Length > 20 ? numero.Trim()[..20] : numero?.Trim()
        };
        _context.Endereco.Add(end);
        await _context.SaveChangesAsync(ct);

        return end;
    }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Senha { get; set; }
}

public class EsqueciSenhaRequest
{
    public string Email { get; set; }
}

public class RedefinirSenhaRequest
{
    public string Token { get; set; }
    public string NovaSenha { get; set; }
}

public class ExcluirContaRequest
{
    public string Email { get; set; }
    public string Senha { get; set; }
}

public class AlterarSenhaRequest
{
    public string Email { get; set; }
    public string SenhaAtual { get; set; }
    public string NovaSenha { get; set; }
}


