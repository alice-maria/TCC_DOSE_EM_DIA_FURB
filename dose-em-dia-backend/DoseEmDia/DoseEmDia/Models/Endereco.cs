namespace DoseEmDia.Models
{
    public class Endereco
    {
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string CEP { get; set; }
        public string Pais { get; set; }

        public Endereco(string logradouro, string numero, string complemento, string bairro, string cidade, string estado, string cep, string pais)
        {
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Cidade = cidade;
            Estado = estado;
            CEP = cep;
            Pais = pais;
        }
    }
}
