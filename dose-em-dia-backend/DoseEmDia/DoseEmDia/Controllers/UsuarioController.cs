using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoseEmDia.Models;
using DoseEmDia.Models.db;
using DoseEmDia.Controllers.Helpers;
using DoseEmDia.Helpers;
using DoseEmDia.Models.Exceptions;
using DoseEmDia.Controllers.DTO;
using DoseEmDia.Models.Enums;

[Route("api/usuario")]
[ApiController]
public class UsuarioController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly EnvioEmail _envioEmail;

    public UsuarioController(ApplicationDbContext context, EnvioEmail envioEmail)
    {
        _context = context;
        _envioEmail = envioEmail;
    }

    [HttpPost("criar")]
    public async Task<IActionResult> CriarUsuario([FromBody] Usuario request)
    {
        try
        {
            if (_context.Usuario.Any(u => u.Email == request.Email))
                return BadRequest("E-mail já cadastrado.");

            var endereco = new Endereco(
                request.Endereco.Logradouro,
                request.Endereco.Bairro,
                request.Endereco.Cidade,
                request.Endereco.Estado,
                FormatacaoHelper.FormataCEP(request.Endereco.CEP),
                request.Endereco.Pais
            );

            _context.Endereco.Add(endereco);

            // Remover espaço final do nome
            request.Nome = request.Nome?.TrimEnd();

            var salt = CriptografiaHelper.GerarSalt();

            var usuario = new Usuario
            {
                Nome = request.Nome,
                DataNascimento = request.DataNascimento,
                Email = request.Email,
                Telefone = FormatacaoHelper.FormataTelefone(request.Telefone),
                CPF = FormatacaoHelper.FormataCPF(request.CPF),
                Sexo = request.Sexo,
                Senha = CriptografiaHelper.GerarHash(request.Senha, salt),
                Salt = salt,
                Endereco = endereco
            };

            _context.Usuario.Add(usuario);
            await _context.SaveChangesAsync();

            var vacinas = GerarVacinasFicticiasParaUsuario();
            foreach (var vacina in vacinas)
            {
                vacina.UsuarioId = usuario.IdUser;
                _context.Vacina.Add(vacina);
            }
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterUsuarioPorId), new { id = usuario.IdUser }, usuario);
        }
        catch (UsuarioException.EmailJaCadastradoException ex)
        {
            return Conflict(ex.Message); // 409
        }
        catch (Exception)
        {
            return StatusCode(500, "Erro interno ao criar o usuário.");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == request.Email);
        bool senhaValida = CriptografiaHelper.VerificarSenha(request.Senha, usuario.Senha, usuario.Salt);

        if (usuario == null || !senhaValida)
            return Unauthorized("Usuário ou senha inválidos.");

        return Ok(new
        {
            mensagem = "Login realizado com sucesso.",
            nome = usuario.Nome,
            cpf = usuario.CPF,
            id = usuario.IdUser
        });

    }

    [HttpPost("esqueciSenha")]
    public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaRequest request)
    {
        var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (usuario == null)
            return NotFound("E-mail não encontrado.");

        usuario.TokenRedefinicaoSenha = _envioEmail.GerarToken();
        usuario.TokenExpiracao = DateTime.UtcNow.AddMinutes(15);

        await _context.SaveChangesAsync();

        await _envioEmail.EnviarEmailRedefinicaoSenhaAsync(usuario.Email, usuario.TokenRedefinicaoSenha);

        return Ok("Se o e-mail estiver cadastrado, um link de redefinição será enviado.");
    }

    [HttpPost("redefinir-senha")]
    public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequest request)
    {
        var usuario = await _context.Usuario
            .FirstOrDefaultAsync(u => u.TokenRedefinicaoSenha == request.Token && u.TokenExpiracao > DateTime.UtcNow);

        if (usuario == null)
            return BadRequest("Token inválido ou expirado.");

        var novoSalt = CriptografiaHelper.GerarSalt();
        var novoHash = CriptografiaHelper.GerarHash(request.NovaSenha, novoSalt);

        usuario.Senha = novoHash;
        usuario.Salt = novoSalt;

        usuario.TokenRedefinicaoSenha = null;
        usuario.TokenExpiracao = null;

        await _context.SaveChangesAsync();

        return Ok("Senha redefinida com sucesso.");
    }

    [HttpPatch("alterarDados/{id}")]
    public async Task<IActionResult> AtualizarUsuario(int id, [FromBody] AtualizarUsuario request)
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
            if (usuario.Endereco == null)
                usuario.Endereco = new Endereco();

            if (!string.IsNullOrWhiteSpace(request.Endereco.Logradouro))
                usuario.Endereco.Logradouro = request.Endereco.Logradouro;

            if (!string.IsNullOrWhiteSpace(request.Endereco.Bairro))
                usuario.Endereco.Bairro = request.Endereco.Bairro;

            if (!string.IsNullOrWhiteSpace(request.Endereco.Cidade))
                usuario.Endereco.Cidade = request.Endereco.Cidade;

            if (!string.IsNullOrWhiteSpace(request.Endereco.Estado))
                usuario.Endereco.Estado = request.Endereco.Estado;

            if (!string.IsNullOrWhiteSpace(request.Endereco.CEP))
                usuario.Endereco.CEP = FormatacaoHelper.FormataCEP(request.Endereco.CEP);

            if (!string.IsNullOrWhiteSpace(request.Endereco.Pais))
                usuario.Endereco.Pais = request.Endereco.Pais;
        }

        await _context.SaveChangesAsync();

        return Ok("Dados do usuário atualizados com sucesso.");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterUsuarioPorId(int id)
    {
        var usuario = await _context.Usuario.FindAsync(id);
        if (usuario == null)
            throw new UsuarioException.UsuarioNaoEncontradoException(id);

        return Ok(usuario);
    }

    private List<Vacina> GerarVacinasFicticiasParaUsuario()
    {
        var listaVacinas = new Dictionary<string, string>
        {
            { "BCG", "Única" },
            { "Hepatite B", "0-1-6 meses" },
            { "Penta", "2, 4 e 6 meses" },
            { "Pólio inativada", "2, 4 e 6 meses" },
            { "Rotavírus", "2 e 4 meses" },
            { "Pneumo 10", "2 e 4 meses + reforço com 12 meses" },
            { "Meningo C", "3 e 5 meses + reforço com 12 meses" },
            { "Febre amarela", "Dose única aos 9 meses" },
            { "Tríplice viral", "12 e 15 meses" },
            { "Tetra viral", "15 meses" },
            { "DTP", "15 meses e 4 anos" },
            { "Hepatite A", "15 meses" },
            { "Varicela", "15 meses" },
            { "Difteria e tétano adulto (dT)", "Reforço a cada 10 anos" },
            { "Meningocócica ACWY", "11 e 12 anos" },
            { "HPV quadrivalente", "2 doses a partir dos 9 anos" },
            { "dTpa", "Gestante / substituto do dT" },
            { "Covid-19", "2 ou 3 doses + reforços" },
            { "Pneumocócica 23-valente (Pneumo 23)", "Única após 60 anos" }
        };

        var validadeMeses = new Dictionary<string, int>
        {
            { "BCG", 999 },
            { "Hepatite B", 6 },
            { "Penta", 6 },
            { "Pólio inativada", 6 },
            { "Rotavírus", 6 },
            { "Pneumo 10", 12 },
            { "Meningo C", 12 },
            { "Febre amarela", 999 },
            { "Tríplice viral", 12 },
            { "Tetra viral", 12 },
            { "DTP", 48 },
            { "Hepatite A", 12 },
            { "Varicela", 12 },
            { "Difteria e tétano adulto (dT)", 120 },
            { "Meningocócica ACWY", 24 },
            { "HPV quadrivalente", 12 },
            { "dTpa", 120 },
            { "Covid-19", 12 },
            { "Pneumocócica 23-valente (Pneumo 23)", 999 }
        };

        var fabricantes = new[] { "Pfizer", "Butantan", "MSD", "Fiocruz", "AstraZeneca", "GSK", "Moderna" };
        var nomesSorteio = listaVacinas.Keys.ToList();
        var rand = new Random();
        var vacinas = new List<Vacina>();

        // Geração das vacinas aplicadas
        for (int i = 0; i < 10; i++)
        {
            var nome = nomesSorteio[rand.Next(nomesSorteio.Count)];
            vacinas.Add(new Vacina(
                nome,
                fabricantes[rand.Next(fabricantes.Length)],
                rand.Next(1, 4),
                rand.Next(100000, 999999),
                listaVacinas[nome],
                DateTime.Today.AddMonths(-rand.Next(1, 48)),
                StatusVacina.Aplicada)
            {
                ValidadeMeses = validadeMeses.ContainsKey(nome) ? validadeMeses[nome] : 12
            });
        }

        // Geração das vacinas em atraso
        for (int i = 0; i < 5; i++)
        {
            var nome = nomesSorteio[rand.Next(nomesSorteio.Count)];
            vacinas.Add(new Vacina(
                nome,
                fabricantes[rand.Next(fabricantes.Length)],
                rand.Next(1, 4),
                rand.Next(100000, 999999),
                listaVacinas[nome],
                DateTime.Today.AddDays(-rand.Next(10, 90)),
                StatusVacina.EmAtraso)
            {
                ValidadeMeses = validadeMeses.ContainsKey(nome) ? validadeMeses[nome] : 12
            });
        }

        // Geração das vacinas a vencer
        for (int i = 0; i < 4; i++)
        {
            var nome = nomesSorteio[rand.Next(nomesSorteio.Count)];
            vacinas.Add(new Vacina(
                nome,
                fabricantes[rand.Next(fabricantes.Length)],
                rand.Next(1, 4),
                rand.Next(100000, 999999),
                listaVacinas[nome],
                DateTime.Today.AddDays(rand.Next(1, 15)),
                StatusVacina.AVencer)
            {
                ValidadeMeses = validadeMeses.ContainsKey(nome) ? validadeMeses[nome] : 12
            });
        }

        return vacinas;
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

