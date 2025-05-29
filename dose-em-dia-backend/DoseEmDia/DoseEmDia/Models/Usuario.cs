﻿using DoseEmDia.Controllers.Helpers;
using DoseEmDia.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Usuario
{
    [Key]
    [Column("IdUser")]
    public int IdUser { get; set; }
    public string Nome { get; set; }
    public DateTime DataNascimento { get; set; }
    public string Email { get; set; }
    public string Telefone { get; set; }
    public string CPF { get; set; }
    public string? Sexo { get; set; }
    public string? Senha { get; set; }
    public string? Salt { get; set; }
    public Endereco Endereco { get; set; }
    public int EnderecoId { get; set; }
    public List<Vacina>? Vacinas { get; set; } = new List<Vacina>();
    public List<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
    public string? TokenRedefinicaoSenha { get; set; }
    public DateTime? TokenExpiracao { get; set; }
    public bool ReceberNotificacoes { get; set; } = true;

    public Usuario(string nome, DateTime dataNascimento, string email, string telefone, string cpf, string sexo, string senha, Endereco endereco)
    {
        Nome = nome;
        DataNascimento = dataNascimento;
        Email = email;
        Telefone = telefone;
        CPF = cpf;
        var salt = CriptografiaHelper.GerarSalt();
        Senha = senha;
        Salt = salt;
        Endereco = endereco;
    }

    public Usuario() { }

}
