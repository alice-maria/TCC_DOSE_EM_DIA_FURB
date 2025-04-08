using System.Security.Cryptography;
using System.Text;

namespace DoseEmDia.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public DateTime DataNascimento { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string CPF { get; set; }
        public string SenhaHash{ get; set; }
        public Endereco Endereco { get; set; }
        public int? EnderecoId { get; set; }
        public string? TokenRedefinicaoSenha { get; set; }
        public DateTime? TokenExpiracao { get; set; }

        public Usuario(string nome, DateTime dataNascimento, string email, string telefone, string cpf, string senha, Endereco endereco)
        {
            Nome = nome;
            DataNascimento = dataNascimento;
            Email = email;
            Telefone = telefone;
            CPF = cpf;
            SenhaHash = GerarHashSHA256(senha);
            Endereco = endereco;
        }

        public Usuario () {}

        public bool ValidarSenha(string senha)
        {
            return SenhaHash == GerarHashSHA256(senha);
        }

        public string GerarHashSHA256(string senha)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(senha);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
