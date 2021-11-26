namespace minimaltodo.Models
{
    public class Tarefa
    {
        public Guid Id { get; set; }
        public string? Titulo { get; set; }
        public string? Descricao { get; set; }
        public bool Feito { get; set; }
    }
}