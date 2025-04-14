using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoseEmDia.Models;
using DoseEmDia.Models.db;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using DoseEmDia.Controllers.Seguranca;

[Route("api/usuario")]
[ApiController]
public class UsuarioController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsuarioController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 🔹 Criar Conta (POST)
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


    // 🔹 Login (POST)
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == request.Email);
        bool senhaValida = CriptografiaHelper.VerificarSenha(request.Senha, usuario.Senha, usuario.Salt);

        if (usuario == null || !senhaValida)
            return Unauthorized("Usuário ou senha inválidos.");

        return Ok(new { mensagem = "Login realizado com sucesso." });
    }

    // 🔹 Esqueci Minha Senha (POST)
    [HttpPost("esqueciSenha")]
    public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaRequest request)
    {
        var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (usuario == null)
            return NotFound("E-mail não encontrado.");

        usuario.TokenRedefinicaoSenha = GerarToken();
        usuario.TokenExpiracao = DateTime.UtcNow.AddHours(1); // Token válido por 1 hora

        await _context.SaveChangesAsync();

        // Enviar e-mail com o token
        EnviarEmailRedefinicao(usuario.Email, usuario.TokenRedefinicaoSenha);

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


    private string GerarToken()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] tokenData = new byte[32];
            rng.GetBytes(tokenData);
            return Convert.ToBase64String(tokenData);
        }
    }
    private void EnviarEmailRedefinicao(string email, string token)
    {
        try
        {
            // Obtém o servidor SMTP e a porta com base no domínio do e-mail
            var (smtpServidor, porta) = ObterServidorSmtp(email);

            string remetente = "notificadoseemdia@gmail.com"; // Seu e-mail de envio
            string senha = "Doseemdia2025"; // A senha do seu e-mail de envio

            using (SmtpClient smtpClient = new SmtpClient(smtpServidor))
            {
                smtpClient.Port = porta;
                smtpClient.Credentials = new NetworkCredential(remetente, senha);
                smtpClient.EnableSsl = true;

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(remetente),
                    Subject = "Redefinição de Senha",
                    Body = $"Clique no link para redefinir sua senha: https://doseemdia.com/redefinir-senha?token={token}",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                // Envia o e-mail
                smtpClient.Send(mailMessage);
            }
        }
        catch(SmtpException smtpEx)
        {
            throw new Exception("Falha ao enviar o e-mail. Verifique as configurações de SMTP.", smtpEx);
        }
        catch (Exception ex)
        {
            throw new Exception("Ocorreu um erro inesperado ao tentar enviar o e-mail de redefinição de senha.", ex);
        }
    }
    public static (string servidor, int porta) ObterServidorSmtp(string email)
    {
        string dominio = email.Split('@')[1].ToLower();

        // Verifica o domínio do e-mail e retorna o servidor SMTP apropriado
        return dominio switch
        {
            "gmail.com" => ("smtp.gmail.com", 587),
            "outlook.com" or "hotmail.com" => ("smtp.office365.com", 587),
            "yahoo.com" => ("smtp.mail.yahoo.com", 465),
            "icloud.com" => ("smtp.mail.me.com", 587),
            _ => ("smtp.sendgrid.net", 587) // Default para SendGrid
        };
    }

    // 🔹 Obter Usuário por ID (GET)
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterUsuarioPorId(int id)
    {
        var usuario = await _context.Usuario.FindAsync(id);
        if (usuario == null)
            return NotFound("Usuário não encontrado.");

        return Ok(usuario);
    }
}

// 🔹 Modelos auxiliares para login e recuperação de senha
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

