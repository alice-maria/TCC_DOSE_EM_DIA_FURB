using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoseEmDia.Models;
using DoseEmDia.Models.db;
using DoseEmDia.Controllers.Helpers;
using DoseEmDia.Helpers;
using DoseEmDia.Models.Exceptions;
using DoseEmDia.Controllers.DTO;
using DoseEmDia.Controllers;

public class UsuarioService
{
    private readonly ApplicationDbContext _context;
    private readonly EnvioEmail _envioEmail;
    private readonly VacinaService _vacinaService;

    public UsuarioService(ApplicationDbContext context, EnvioEmail envioEmail, VacinaService vacinaService)
    {
        _context = context;
        _envioEmail = envioEmail;
        _vacinaService = vacinaService; 
    }

    public async Task<Usuario> BuscarPorCpf(string cpf)
    {
        var usuario = await _context.Usuario
            .Include(u => u.Endereco)
            .FirstOrDefaultAsync(u => u.CPF == cpf);

        return usuario;
    }

    public async Task<Usuario> CriarUsuario(Usuario request)
    {
        if (_context.Usuario.Any(u => u.Email == request.Email))
            throw new UsuarioException.EmailJaCadastradoException(request.Email);

        var endereco = new Endereco(
            request.Endereco.Logradouro,
            request.Endereco.Bairro,
            request.Endereco.Cidade,
            request.Endereco.Estado,
            FormatacaoHelper.FormataCEP(request.Endereco.CEP),
            request.Endereco.Pais
        );

        _context.Endereco.Add(endereco);

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

        int idade = CalcularIdade(usuario.DataNascimento);
        await _vacinaService.GerarEVincularVacinas(usuario.IdUser, idade, usuario.Sexo);

        return usuario;
    }

    public async Task<Usuario> Login(LoginRequest request)
    {
        var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == request.Email);
        bool senhaValida = CriptografiaHelper.VerificarSenha(request.Senha, usuario.Senha, usuario.Salt);

        if (usuario == null || !senhaValida)
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


    public async Task EsqueciSenha(string email)
    {
        var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == email);
        if (usuario == null)
            throw new UsuarioException.UsuarioNaoEncontradoException(email);

        usuario.TokenRedefinicaoSenha = _envioEmail.GerarToken();
        usuario.TokenExpiracao = DateTime.UtcNow.AddMinutes(15);

        await _context.SaveChangesAsync();

        await _envioEmail.EnviarEmailRedefinicaoSenhaAsync(usuario.Email, usuario.TokenRedefinicaoSenha);
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
            throw new UsuarioException.UsuarioNaoEncontradoException(usuario.IdUser);

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


