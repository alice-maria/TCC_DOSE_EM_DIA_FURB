namespace DoseEmDia.Models
{
    public class PostoVacinacao
    {
        public string Nome { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string Telefone { get; set; }
        public string HorarioFuncionamento { get; set; }

        public PostoVacinacao(string nome, string cidade, string estado, string telefone, string horarioFuncionamento)
        {
            Nome = nome;
            Cidade = cidade;
            Estado = estado;
            Telefone = telefone;
            HorarioFuncionamento = horarioFuncionamento;
        }
    }
}
