using System.Net.Mail;
using System.Net;
using DoseEmDia.Models.Enums;

namespace DoseEmDia.Models
{
    public class Notificacao
    {
            public int Id { get; set; }
            public int UsuarioId { get; set; }
            public Usuario Usuario { get; set; }
            public string Titulo { get; set; }
            public string Mensagem { get; set; }
            public TipoNotificacao Tipo { get; set; }
            public DateTime DataEnvio { get; set; }
            public bool Visualizada { get; set; } = false;

            public Notificacao() { }
    }
}
