using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoseEmDia.Models.db
{
    [Table("ContadorRequisicoes")]
    public class ContadorRequisicoes
    {
        [Key]
        public int Id { get; set; } = 1;
        public int Requisicoes { get; set; }
    }
}