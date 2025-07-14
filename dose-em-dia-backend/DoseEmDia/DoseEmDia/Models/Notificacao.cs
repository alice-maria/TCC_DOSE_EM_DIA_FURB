using DoseEmDia.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DoseEmDia.Models
{
    [Table("Notificacoes")]
    public class Notificacao
    {
        [Key]
        [Column("IdNotificacao")]
        public int IdNotificacao { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
        public TipoNotificacao Tipo { get; set; }
        public DateTime DataEnvio { get; set; }
        public bool Visualizada { get; set; } = false;

        [Column("EmailEnviado")]
        public bool EmailEnviado { get; set; } = false; //atualização: validação de Email enviado

        public Notificacao() { }
    }
}
