using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DoseEmDia.Controllers.DTO
{
    public class CriarUsuarioRequest
    {
        // Dados do usuário
        [Required, StringLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Senha { get; set; } = string.Empty;

        [Required]
        public DateTime DataNascimento { get; set; }

        [StringLength(20)]
        public string Telefone { get; set; } = string.Empty;

        [StringLength(14)]
        public string CPF { get; set; } = string.Empty;

        [StringLength(20)] 
        public string Sexo { get; set; } = string.Empty;

        public bool ReceberNotificacoes { get; set; }

        // Dados do endereço
        [JsonPropertyName("pais")]
        [StringLength(120)]
        public string Pais { get; set; } = string.Empty;

        [JsonPropertyName("uf")]
        [StringLength(2)]
        public string Uf { get; set; } = string.Empty;

        [JsonPropertyName("cidade")]
        [StringLength(120)]
        public string Cidade { get; set; } = string.Empty;

        [JsonPropertyName("cep")] 
        [StringLength(9)] 
        public string Cep { get; set; } = string.Empty;

        [JsonPropertyName("logradouro")]
        [StringLength(200)]
        public string Logradouro { get; set; } = string.Empty;

        [JsonPropertyName("numero")]
        [StringLength(20)]
        public string Numero { get; set; } = string.Empty;

        [JsonPropertyName("bairro")]
        [StringLength(120)]
        public string Bairro { get; set; } = string.Empty;
    }
}
