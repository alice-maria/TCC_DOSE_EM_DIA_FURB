namespace DoseEmDia.Controllers.DTO
{
    public class AtualizarUsuario
    {
        public string? Nome { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Telefone { get; set; }
        public AtualizarEndereco? Endereco { get; set; }
        public string? Email { get; set; }
    }

    public class AtualizarEndereco
    {
        public string? Logradouro { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? CEP { get; set; }
        public string? Pais { get; set; }
    }
}
