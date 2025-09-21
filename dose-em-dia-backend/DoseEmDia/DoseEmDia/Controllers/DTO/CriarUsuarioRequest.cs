namespace DoseEmDia.Controllers.DTO
{
    public class CriarUsuarioRequest
    {
        // Dados do usuário
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public string Telefone { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public string Sexo { get; set; } = string.Empty;
        public bool ReceberNotificacoes { get; set; }

        // Dados do endereço
        public string Pais { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public string Logradouro { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Bairro { get; set; }
    }
}