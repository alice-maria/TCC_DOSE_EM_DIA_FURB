namespace DoseEmDia.Models.Dto
{
    public class UsuarioRequest
    {
        public string Nome { get; set; }
        public DateTime DataNascimento { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string CPF { get; set; }
        public string Senha { get; set; }
        public Endereco Endereco { get; set; }
    }
}
