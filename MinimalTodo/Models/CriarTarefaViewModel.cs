using Flunt.Notifications;
using Flunt.Validations;

namespace minimaltodo.Models
{
    public class CriarTarefaViewModel : Notifiable<Notification>
    {
        public string? Titulo { get; set; }
        public string? Descricao { get; set; }

        public Tarefa MapTo()
        {
            var contract = new Contract<Notification>()
                .Requires()
                .IsNotNull(Titulo, "Informe um t�tulo para a tarefa que deseja cadastrar")
                .IsGreaterThan(Titulo,5, "O t�tulo deve conter mais de 5 caracteres")
                .IsNotNull(Descricao, "Informe uma descri��o para a tarefa que deseja cadastrar")
                .IsGreaterThan(Descricao,15, "A descri��o deve conter mais de 15 caracteres");
            
            AddNotifications(contract);

            Tarefa t = new Tarefa();
            t.Id = Guid.NewGuid();
            t.Titulo = Titulo;
            t.Descricao = Descricao;
            t.Feito = false;

            return t;
        }
    }
}