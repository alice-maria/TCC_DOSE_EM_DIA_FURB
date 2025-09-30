namespace DoseEmDia.Controllers.DTO
{
#nullable enable
    public sealed class AtualizarUsuario
    {
        public string? Nome { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Telefone { get; set; }
        public string? Sexo { get; set; }
        public string? Email { get; set; }
        public AtualizarEndereco? Endereco { get; set; }
    }

    public sealed class AtualizarEndereco
    {
        public string? CEP { get; set; }          
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }       
        public string? Bairro { get; set; }

        public long? CidadeId { get; set; }        
        public string? CidadeNome { get; set; }   
        public string? Uf { get; set; }           
    }
#nullable restore
}
