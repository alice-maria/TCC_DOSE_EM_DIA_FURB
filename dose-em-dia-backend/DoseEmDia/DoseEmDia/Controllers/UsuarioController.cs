using Microsoft.AspNetCore.Mvc;
using DoseEmDia.Models.Exceptions;
using DoseEmDia.Controllers.DTO;

[Route("api/usuario")]
[ApiController]
public class UsuarioController : ControllerBase
{
    private readonly UsuarioService _usuarioService;
    public UsuarioController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet("buscarPorCpf/{cpf}")]
    public async Task<IActionResult> BuscarPorCpf(string cpf)
    {
        var usuario = await _usuarioService.BuscarPorCpf(cpf);
        return usuario == null ? NotFound() : Ok(usuario);
    }

    [HttpPost("criar-conta")]
    public async Task<IActionResult> CriarUsuario([FromBody] Usuario request)
    {
        try
        {
            var usuario = await _usuarioService.CriarUsuario(request);
            return CreatedAtAction(nameof(ObterUsuarioPorId), new { id = usuario.IdUser }, usuario);
        }
        catch (UsuarioException.EmailJaCadastradoException ex)
        {
            return Conflict(ex.Message);
        }
        catch
        {
            return StatusCode(500, "Erro interno ao criar o usuário.");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var usuario = await _usuarioService.Login(request);
            return Ok(new
            {
                mensagem = "Login realizado com sucesso.",
                nome = usuario.Nome,
                cpf = usuario.CPF,
                id = usuario.IdUser
            });
        }
        catch
        {
            return Unauthorized("Usuário ou senha inválidos.");
        }

    }

    [HttpPost("excluir-conta")]
    public async Task<IActionResult> ExcluirConta([FromBody] ExcluirContaRequest request)
    {
        try
        {
            await _usuarioService.ExcluirConta(request);
            return Ok("Conta excluída com sucesso.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch
        {
            return NotFound("Usuário não encontrado.");
        }
    }

    [HttpPost("esqueciSenha")]
    public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaRequest request)
    {
        try
        {
            await _usuarioService.EsqueciSenha(request.Email);
            return Ok("Se o e-mail estiver cadastrado, um link será enviado.");
        }
        catch
        {
            return NotFound("E-mail não encontrado.");
        }
    }

    [HttpPost("redefinir-senha")]
    public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequest request)
    {
        try
        {
            await _usuarioService.RedefinirSenha(request);
            return Ok("Senha redefinida com sucesso.");
        }
        catch
        {
            return BadRequest("Token inválido ou expirado.");
        }
    }

    [HttpPatch("alterarDados/{id}")]
    public async Task<IActionResult> AtualizarUsuario(int id, [FromBody] AtualizarUsuario request)
    {
        try
        {
            await _usuarioService.AtualizarUsuario(id, request);
            return Ok("Dados atualizados.");
        }
        catch
        {
            return NotFound("Usuário não encontrado.");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterUsuarioPorId(int id)
    {
        try
        {
            var usuario = await _usuarioService.ObterUsuarioPorId(id);
            return Ok(usuario);
        }
        catch
        {
            return NotFound("Usuário não encontrado.");
        }
    }
}
