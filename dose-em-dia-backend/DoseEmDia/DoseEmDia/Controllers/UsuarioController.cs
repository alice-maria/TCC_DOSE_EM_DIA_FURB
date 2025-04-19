using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoseEmDia.Models;
using DoseEmDia.Models.db;
using DoseEmDia.Controllers.Helpers;
using DoseEmDia.Helpers;

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

            return CreatedAtAction(nameof(ObterUsuarioPorId), new { id = usuario.Id }, usuario);
        }
        catch (Exception ex)
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

        // Agora usa o serviço
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

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterUsuarioPorId(int id)
    {
        var usuario = await _context.Usuario.FindAsync(id);
        if (usuario == null)
            return NotFound("Usuário não encontrado.");

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

