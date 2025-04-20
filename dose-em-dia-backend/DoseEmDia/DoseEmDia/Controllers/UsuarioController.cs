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
                request.Endereco.CEP,
                request.Endereco.Pais
            );

            _context.Endereco.Add( endereco );

            var salt = CriptografiaHelper.GerarSalt();

            var usuario = new Usuario
            {
                Nome = request.Nome,
                DataNascimento = request.DataNascimento,
                Email = request.Email,
                Telefone = request.Telefone,
                CPF = request.CPF.Replace(".", "").Replace("-", ""),
                Senha = CriptografiaHelper.GerarHash(request.Senha, salt),
                Salt = salt,
                Endereco = endereco
            };

            _context.Usuario.Add(usuario);
            await _context.SaveChangesAsync();

            // Criar vacinas fictícias
            var vacinasFicticias = new List<Vacina>
            {
                new("Covid19 - 3 dose", "Pfizer", 3, 123456, "120 dias", DateTime.Today.AddDays(-5), StatusVacina.EmAtraso),
                new("Influenza", "Butantan", 1, 654321, "Anual", DateTime.Today.AddDays(3), StatusVacina.AVencer),
                new("Covid19 - 2 dose", "Pfizer", 2, 111111, "30 dias", DateTime.Today.AddMonths(-4), StatusVacina.Aplicada),
                new("HPV", "MSD", 3, 222222, "6 meses", DateTime.Today.AddYears(-5), StatusVacina.Aplicada),
                new("Febre Amarela", "Fiocruz", 1, 333333, "Única", DateTime.Today.AddYears(-8), StatusVacina.Aplicada),
            };

            foreach (var vacina in vacinasFicticias)
            {
                vacina.UsuarioId = usuario.Id;
                _context.Vacina.Add(vacina);
            }

            await _context.SaveChangesAsync();


            return CreatedAtAction(nameof(ObterUsuarioPorId), new { id = usuario.Id }, usuario);
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

        return Ok(new { mensagem = "Login realizado com sucesso." });
    }

    [HttpPost("esqueciSenha")]
    public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaRequest request)
    {
        var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (usuario == null)
            return NotFound("E-mail não encontrado.");

        usuario.TokenRedefinicaoSenha = _envioEmail.GerarToken();
        usuario.TokenExpiracao = DateTime.UtcNow.AddHours(1);

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
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null)
            throw new UsuarioException.UsuarioNaoEncontradoException(id);

        if (!string.IsNullOrWhiteSpace(request.Nome))
            usuario.Nome = request.Nome;

        if (request.DataNascimento.HasValue)
            usuario.DataNascimento = request.DataNascimento.Value;

        if (!string.IsNullOrWhiteSpace(request.Telefone))
            usuario.Telefone = request.Telefone;

        if (!string.IsNullOrWhiteSpace(request.Email))
            usuario.Email = request.Email;

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
                usuario.Endereco.CEP = request.Endereco.CEP;

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

