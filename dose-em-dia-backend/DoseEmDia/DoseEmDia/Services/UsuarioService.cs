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

    private static string SomenteDigitos(string? s)
        => new string((s ?? string.Empty).Where(char.IsDigit).ToArray());

    private static string NormalizaCep(string? cep)
    {
        var cod = SomenteDigitos(cep);
        if (cod.Length != 8) throw new ArgumentException("CEP inválido (esperado 8 dígitos).");
        return cod;
    }

    private async Task<Cep> EnsureCepAsync(string cepCodigo, Cidade cidade, string? bairro, CancellationToken ct) // <<<
    {
        var tracked = _context.ChangeTracker.Entries<Cep>()
            .FirstOrDefault(e => e.Entity.Codigo == cepCodigo)?.Entity;
        if (tracked is not null)
        {
            if (cidade is not null && tracked.CidadeId != cidade.IdCidade && cidade.IdCidade != 0)
                tracked.CidadeId = cidade.IdCidade;
            if (!string.IsNullOrWhiteSpace(bairro) && !string.Equals(tracked.Bairro, bairro, StringComparison.Ordinal))
                tracked.Bairro = bairro;
            return tracked;
        }

        var existente = await _context.Cep.FirstOrDefaultAsync(c => c.Codigo == cepCodigo, ct);
        if (existente is not null)
        {
            if (cidade is not null && existente.CidadeId != cidade.IdCidade && cidade.IdCidade != 0)
                existente.CidadeId = cidade.IdCidade;
            if (!string.IsNullOrWhiteSpace(bairro) && !string.Equals(existente.Bairro, bairro, StringComparison.Ordinal))
                existente.Bairro = bairro;
            return existente;
        }

        var novo = new Cep
        {
            Codigo = cepCodigo,
            Bairro = string.IsNullOrWhiteSpace(bairro) ? null : bairro,
            Cidade = cidade
        };
        _context.Cep.Add(novo);
        return novo;
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
        var (ufEstado, estadoNomeExtenso) = ResolverEstado(dto.Uf);
        var cidadeNome = dto.Cidade.Trim();
        var cepCodigo = NormalizaCep(dto.Cep);
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
                    paisNome, ufEstado, estadoNomeExtenso, cidadeNome,
                    cepCodigo, logradouro, numero, bairro, ct);

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
                    Endereco = endereco,
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
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync(ct);

            var usuario = await _context.Usuario
                .Include(u => u.Endereco)
                    .ThenInclude(e => e.Cep)
                .FirstOrDefaultAsync(u => u.IdUser == id, ct);

            if (usuario is null)
                throw new UsuarioException.UsuarioNaoEncontradoException(id);

            // Dados básicos
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

            // Endereço
            if (request.Endereco is not null)
            {
                var reqCep = request.Endereco.CEP;
                var reqLograd = request.Endereco.Logradouro;
                var reqNumero = request.Endereco.Numero;
                var reqBairro = request.Endereco.Bairro;

                var houveAlgumCampo =
                    !string.IsNullOrWhiteSpace(reqCep) ||
                    !string.IsNullOrWhiteSpace(reqLograd) ||
                    !string.IsNullOrWhiteSpace(reqNumero) ||
                    !string.IsNullOrWhiteSpace(reqBairro);

                if (houveAlgumCampo)
                {
                    if (usuario.Endereco is null)
                    {
                        // Primeiro cadastro de endereço exige CEP e Número
                        if (string.IsNullOrWhiteSpace(reqCep) || string.IsNullOrWhiteSpace(reqNumero))
                            throw new Exception("Para cadastrar o endereço pela primeira vez, informe CEP e Número.");

                        var cepCodigo = NormalizaCep(reqCep);

                        // Garante que CidadeId válido seja usado (exemplo: herda da cidade padrão ou precisa resolver via ViaCEP)
                        var cidadeDefault = await _context.Cidade.FirstOrDefaultAsync(ct)
                            ?? throw new Exception("Nenhuma cidade padrão encontrada. Informe cidade/UF ao cadastrar CEP.");

                        var cepAlvo = await _context.Cep
                            .FirstOrDefaultAsync(c => c.Codigo == cepCodigo, ct);

                        if (cepAlvo is null)
                        {
                            cepAlvo = new Cep
                            {
                                Codigo = cepCodigo,
                                Bairro = reqBairro?.Trim(),
                                CidadeId = cidadeDefault.IdCidade
                            };
                            _context.Cep.Add(cepAlvo);
                        }

                        var novoEnd = new Endereco
                        {
                            Cep = cepAlvo,
                            Logradouro = (reqLograd ?? string.Empty).Trim(),
                            Numero = reqNumero.Trim()
                        };

                        usuario.Endereco = novoEnd;
                        _context.Endereco.Add(novoEnd);
                    }
                    else
                    {
                        // Atualização in-place
                        var end = usuario.Endereco;

                        if (!string.IsNullOrWhiteSpace(reqCep))
                        {
                            var cepCodigo = NormalizaCep(reqCep);
                            var cepAlvo = await _context.Cep
                                .FirstOrDefaultAsync(c => c.Codigo == cepCodigo, ct);

                            if (cepAlvo is null)
                            {
                                // Herda cidade do CEP atual
                                var cidadeId = end.Cep?.CidadeId;
                                if (cidadeId is null || cidadeId == 0)
                                    throw new Exception("Não foi possível associar o CEP a uma cidade válida.");

                                cepAlvo = new Cep
                                {
                                    Codigo = cepCodigo,
                                    Bairro = reqBairro?.Trim(),
                                    CidadeId = cidadeId.Value
                                };
                                _context.Cep.Add(cepAlvo);
                            }
                            else if (!string.IsNullOrWhiteSpace(reqBairro))
                            {
                                cepAlvo.Bairro = reqBairro.Trim();
                            }

                            end.Cep = cepAlvo;
                        }
                        else if (!string.IsNullOrWhiteSpace(reqBairro))
                        {
                            end.Cep.Bairro = reqBairro.Trim();
                        }

                        if (!string.IsNullOrWhiteSpace(reqLograd))
                            end.Logradouro = reqLograd.Trim();

                        if (!string.IsNullOrWhiteSpace(reqNumero))
                            end.Numero = reqNumero.Trim();

                        _context.Endereco.Update(end);
                    }
                }
            }

            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });
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

    private async Task<Endereco> CriarEnderecoAsync(string paisNome, string ufEstado, string estadoNomeExtenso, string cidadeNome, string cepCodigo, string logradouro, string numero, string? bairro, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(paisNome)) 
            throw new ArgumentException("País é obrigatório.", nameof(paisNome));
        if (string.IsNullOrWhiteSpace(cidadeNome)) 
            throw new ArgumentException("Cidade é obrigatória.", nameof(cidadeNome));
        if (string.IsNullOrWhiteSpace(cepCodigo)) 
            throw new ArgumentException("CEP é obrigatório.", nameof(cepCodigo));
        if (string.IsNullOrWhiteSpace(numero)) 
            throw new ArgumentException("Número é obrigatório.", nameof(numero));

        var pais = await _context.Pais.FirstOrDefaultAsync(p => EF.Functions.ILike(p.Nome, paisNome), ct);
        if (pais is null)
        {
            pais = new Pais { Nome = paisNome };
            _context.Pais.Add(pais);
        }

        var estado = await _context.Estado.FirstOrDefaultAsync(
            e => e.PaisId == pais.IdPais &&
                 (EF.Functions.ILike(e.Nome, estadoNomeExtenso) || e.Uf == ufEstado || EF.Functions.ILike(e.Nome, ufEstado)), ct);

        if (estado is null)
        {
            estado = new Estado { Pais = pais, Nome = estadoNomeExtenso, Uf = ufEstado };
            _context.Estado.Add(estado);
        }
        else
        {
            if (!string.Equals(estado.Nome, estadoNomeExtenso, StringComparison.Ordinal))
                estado.Nome = estadoNomeExtenso;
            if (!string.Equals(estado.Uf, ufEstado, StringComparison.OrdinalIgnoreCase))
                estado.Uf = ufEstado;
        }

        var cidade = await _context.Cidade
            .FirstOrDefaultAsync(c => c.EstadoId == estado.IdEstado && EF.Functions.ILike(c.Nome, cidadeNome), ct);

        if (cidade is null)
        {
            cidade = new Cidade { Estado = estado, Nome = cidadeNome };
            _context.Cidade.Add(cidade);
        }

        var cep = await EnsureCepAsync(cepCodigo, cidade, bairro, ct);

        var end = new Endereco
        {
            Cep = cep,
            Logradouro = logradouro?.Trim(),
            Numero = (numero?.Trim() ?? string.Empty) is var n && n.Length > 20 ? n[..20] : n
        };

        _context.Endereco.Add(end);
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