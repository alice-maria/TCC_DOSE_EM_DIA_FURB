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

    public async Task<Usuario?> BuscarPorCpf(string cpf)
    {
        return await _context.Usuario
            .Include(u => u.Endereco)
                .ThenInclude(e => e.Cep)
                    .ThenInclude(cep => cep.Cidade)
                        .ThenInclude(c => c.Estado)
                            .ThenInclude(uf => uf.Pais)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.CPF == cpf);
    }

    public async Task<Usuario> CriarUsuario(CriarUsuarioRequest dto, CancellationToken ct = default)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("E-mail é obrigatório.");
        if (string.IsNullOrWhiteSpace(dto.Senha))
            throw new ArgumentException("Senha é obrigatória.");
        if (string.IsNullOrWhiteSpace(dto.Pais))
            throw new ArgumentException("País é obrigatório.");
        if (string.IsNullOrWhiteSpace(dto.Uf))
            throw new ArgumentException("UF é obrigatória.");
        if (string.IsNullOrWhiteSpace(dto.Cidade))
            throw new ArgumentException("Cidade é obrigatória.");
        if (string.IsNullOrWhiteSpace(dto.Cep))
            throw new ArgumentException("CEP é obrigatório.");
        if (string.IsNullOrWhiteSpace(dto.Logradouro))
            throw new ArgumentException("Logradouro é obrigatório.");
        if (string.IsNullOrWhiteSpace(dto.Numero))
            throw new ArgumentException("Número é obrigatório.");

        var nome = dto.Nome?.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();
        var cpf = FormatacaoHelper.FormataCPF(dto.CPF);
        var telefone = FormatacaoHelper.FormataTelefone(dto.Telefone);
        var paisNome = dto.Pais.Trim();
        var estadoUf = dto.Uf.Trim().ToUpperInvariant();
        var cidadeNome = dto.Cidade.Trim();
        var cepCodigo = FormatacaoHelper.FormataCEP(dto.Cep);
        var logradouro = dto.Logradouro.Trim();
        var numero = dto.Numero.Trim();
        var bairro = dto.Bairro?.Trim();
        var sexo = dto.Sexo?.Trim();
        var dataNasc = dto.DataNascimento;
        var receberNotif = dto.ReceberNotificacoes;

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var emailExiste = await _context.Usuario
                    .AnyAsync(u => EF.Functions.ILike(u.Email, email), ct);
                if (emailExiste)
                    throw new UsuarioException.EmailJaCadastradoException(email);

                var endereco = await CriarEnderecoAsync(
                    paisNome, estadoUf, cidadeNome, cepCodigo,
                    logradouro, numero, bairro, ct);

                var salt = CriptografiaHelper.GerarSalt();
                var hash = CriptografiaHelper.GerarHash(dto.Senha, salt);

                var usuario = new Usuario
                {
                    Nome = nome,
                    DataNascimento = dataNasc,
                    Email = email,
                    Telefone = telefone,
                    CPF = cpf,
                    Sexo = sexo,
                    Senha = hash,
                    Salt = salt,
                    EnderecoId = endereco.IdEndereco,
                    ReceberNotificacoes = receberNotif
                };

                _context.Usuario.Add(usuario);
                await _context.SaveChangesAsync(ct);

                var idade = CalcularIdade(usuario.DataNascimento);
                await _vacinaService.GerarEVincularVacinas(usuario.IdUser, idade, usuario.Sexo);

                await tx.CommitAsync(ct);
                return usuario;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
            {
                await tx.RollbackAsync(ct);
                throw new UsuarioException.EmailJaCadastradoException(email);
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

        return true;
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


    public async Task AtualizarUsuario(int id, AtualizarUsuario request, CancellationToken ct = default)
    {
        var usuario = await _context.Usuario
        .Include(u => u.Endereco)
            .ThenInclude(e => e.Cep)
        .FirstOrDefaultAsync(u => u.IdUser == id, ct);

        if (usuario is null)
            throw new UsuarioException.UsuarioNaoEncontradoException(id);

        if (!string.IsNullOrWhiteSpace(request.Nome))
            usuario.Nome = request.Nome.Trim();

        if (request.DataNascimento.HasValue)
            usuario.DataNascimento = request.DataNascimento.Value;

        if (!string.IsNullOrWhiteSpace(request.Telefone))
            usuario.Telefone = FormatacaoHelper.FormataTelefone(request.Telefone);

        if (!string.IsNullOrWhiteSpace(request.Sexo))
            usuario.Sexo = request.Sexo.Trim();

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailNovo = request.Email.Trim().ToLowerInvariant();
            if (!string.Equals(usuario.Email, emailNovo, StringComparison.OrdinalIgnoreCase))
            {
                var existe = await _context.Usuario
                    .AnyAsync(u => u.IdUser != id && EF.Functions.ILike(u.Email, emailNovo), ct);
                if (existe)
                    throw new UsuarioException.EmailJaCadastradoException(emailNovo);

                usuario.Email = emailNovo;
            }
        }

        if (request.Endereco is not null)
        {
            var reqCep = request.Endereco.CEP;
            var reqLogradouro = request.Endereco.Logradouro;
            var reqNumero = request.Endereco.Numero;
            var reqBairro = request.Endereco.Bairro;

            if (!string.IsNullOrWhiteSpace(reqCep))
            {
                var cepCodigo = FormatacaoHelper.FormataCEP(reqCep);
                var cep = await _context.Cep.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Codigo == cepCodigo, ct);

                if (cep is null)
                    throw new CepNaoEncontradoException(cepCodigo);

                if (usuario.Endereco is null)
                {
                    if (string.IsNullOrWhiteSpace(reqNumero))
                        throw new ArgumentException("Para criar endereço é obrigatório informar o Número.");

                    usuario.Endereco = new Endereco
                    {
                        CepId = cep.IdCep,
                        Logradouro = reqLogradouro?.Trim(),
                        Numero = reqNumero.Trim(),
                    };
                }
                else
                {
                    usuario.Endereco.CepId = cep.IdCep;

                    if (!string.IsNullOrWhiteSpace(reqLogradouro))
                        usuario.Endereco.Logradouro = reqLogradouro.Trim();

                    if (!string.IsNullOrWhiteSpace(reqNumero))
                        usuario.Endereco.Numero = reqNumero.Trim();
                }

                if (!string.IsNullOrWhiteSpace(reqBairro))
                {
                    var cepToUpdate = new Cep { IdCep = cep.IdCep, Bairro = reqBairro.Trim(), CidadeId = cep.CidadeId };
                    _context.Cep.Attach(cepToUpdate);
                    _context.Entry(cepToUpdate).Property(x => x.Bairro).IsModified = true;
                }
            }
            else
            {
                if (usuario.Endereco is null)
                {
                    if (!string.IsNullOrWhiteSpace(reqLogradouro) ||
                        !string.IsNullOrWhiteSpace(reqNumero) ||
                        !string.IsNullOrWhiteSpace(reqComplemento) ||
                        !string.IsNullOrWhiteSpace(reqBairro))
                    {
                        throw new ArgumentException("Para criar endereço é obrigatório informar o CEP.");
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(reqLogradouro))
                        usuario.Endereco.Logradouro = reqLogradouro.Trim();

                    if (!string.IsNullOrWhiteSpace(reqNumero))
                        usuario.Endereco.Numero = reqNumero.Trim();

                    if (!string.IsNullOrWhiteSpace(reqBairro) && usuario.Endereco.CepId != 0)
                    {
                        var cepAtualId = usuario.Endereco.CepId;
                        var cepToUpdate = new Cep { IdCep = cepAtualId, Bairro = reqBairro.Trim() };
                        _context.Cep.Attach(cepToUpdate);
                        _context.Entry(cepToUpdate).Property(x => x.Bairro).IsModified = true;
                    }
                }
            }
        }

        await _context.SaveChangesAsync(ct);
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

    private static readonly Dictionary<string, string> _ufParaNome = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AC"] = "Acre",
        ["AL"] = "Alagoas",
        ["AP"] = "Amapá",
        ["AM"] = "Amazonas",
        ["BA"] = "Bahia",
        ["CE"] = "Ceará",
        ["DF"] = "Distrito Federal",
        ["ES"] = "Espírito Santo",
        ["GO"] = "Goiás",
        ["MA"] = "Maranhão",
        ["MT"] = "Mato Grosso",
        ["MS"] = "Mato Grosso do Sul",
        ["MG"] = "Minas Gerais",
        ["PA"] = "Pará",
        ["PB"] = "Paraíba",
        ["PR"] = "Paraná",
        ["PE"] = "Pernambuco",
        ["PI"] = "Piauí",
        ["RJ"] = "Rio de Janeiro",
        ["RN"] = "Rio Grande do Norte",
        ["RS"] = "Rio Grande do Sul",
        ["RO"] = "Rondônia",
        ["RR"] = "Roraima",
        ["SC"] = "Santa Catarina",
        ["SP"] = "São Paulo",
        ["SE"] = "Sergipe",
        ["TO"] = "Tocantins"
    };

    private static (string Uf, string NomeExtenso) ResolverEstado(string estadoUf)
    {
        if (string.IsNullOrWhiteSpace(estadoUf))
            throw new ArgumentException("UF é obrigatória.", nameof(estadoUf));

        var uf = estadoUf.Trim().ToUpperInvariant();
        if (!_ufParaNome.TryGetValue(uf, out var nomeExtenso))
            throw new ArgumentException($"UF inválida: \"{estadoUf}\".", nameof(estadoUf));

        return (uf, nomeExtenso);
    }

    private async Task<Endereco> CriarEnderecoAsync(string pais, string uf, string cidade, string cepCodigoRaw, string logradouro, string numero, string? bairro, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(pais))
            throw new ArgumentException("País é obrigatório.", nameof(pais));
        if (string.IsNullOrWhiteSpace(cidade))
            throw new ArgumentException("Cidade é obrigatória.", nameof(cidade));
        if (string.IsNullOrWhiteSpace(cepCodigoRaw))
            throw new ArgumentException("CEP é obrigatório.", nameof(cepCodigoRaw));
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("Número é obrigatório.", nameof(numero));

        var (ufEstado, estadoNomeExtenso) = ResolverEstado(uf);
        var cepCodigo = FormatacaoHelper.FormataCEP(cepCodigoRaw);
        var paisNomeOk = pais.Trim();
        var cidadeOk = cidade.Trim();
        var logOk = logradouro?.Trim();
        var numOk = (numero?.Trim() ?? string.Empty);
        if (numOk.Length > 20) numOk = numOk[..20];
        var bairroOk = bairro?.Trim();

        var nomePais = await _context.Pais
            .FirstOrDefaultAsync(p => EF.Functions.ILike(p.Nome, paisNomeOk), ct)
            ?? (await _context.Pais.AddAsync(new Pais { Nome = paisNomeOk }, ct)).Entity;
        await _context.SaveChangesAsync(ct);

        var nomeEstado = await _context.Estado.FirstOrDefaultAsync(
            e => e.PaisId == nomePais.IdPais &&
                 (EF.Functions.ILike(e.Nome, estadoNomeExtenso) || e.Uf == ufEstado || EF.Functions.ILike(e.Nome, ufEstado)),
            ct);

        if (nomeEstado is null)
        {
            nomeEstado = new Estado { PaisId = nomePais.IdPais, Nome = estadoNomeExtenso, Uf = ufEstado };
            _context.Estado.Add(nomeEstado);
            await _context.SaveChangesAsync(ct);
        }
        else
        {
            bool mudou = false;
            if (!string.Equals(nomeEstado.Nome, estadoNomeExtenso, StringComparison.Ordinal))
            { nomeEstado.Nome = estadoNomeExtenso; mudou = true; }
            if (!string.Equals(nomeEstado.Uf, ufEstado, StringComparison.OrdinalIgnoreCase))
            { nomeEstado.Uf = ufEstado; mudou = true; }
            if (mudou)
            {
                _context.Estado.Update(nomeEstado);
                await _context.SaveChangesAsync(ct);
            }
        }

        var NomeCidade = await _context.Cidade
            .FirstOrDefaultAsync(c => c.EstadoId == nomeEstado.IdEstado && EF.Functions.ILike(c.Nome, cidadeOk), ct)
            ?? (await _context.Cidade.AddAsync(new Cidade { EstadoId = nomeEstado.IdEstado, Nome = cidadeOk }, ct)).Entity;
        await _context.SaveChangesAsync(ct);

        var cep = await _context.Cep.FirstOrDefaultAsync(c => c.Codigo == cepCodigo, ct);
        if (cep is null)
        {
            cep = new Cep { Codigo = cepCodigo, CidadeId = NomeCidade.IdCidade, Bairro = bairroOk };
            _context.Cep.Add(cep);
            await _context.SaveChangesAsync(ct);
        }
        else
        {
            bool cepMudou = false;
            if (cep.CidadeId != NomeCidade.IdCidade) { cep.CidadeId = NomeCidade.IdCidade; cepMudou = true; }
            if (!string.IsNullOrWhiteSpace(bairroOk) && !string.Equals(cep.Bairro, bairroOk, StringComparison.Ordinal))
            { cep.Bairro = bairroOk; cepMudou = true; }
            if (cepMudou)
            {
                _context.Cep.Update(cep);
                await _context.SaveChangesAsync(ct);
            }
        }

        var end = new Endereco
        {
            CepId = cep.IdCep,
            Logradouro = logOk,
            Numero = numOk
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


