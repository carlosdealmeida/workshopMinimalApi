using Microsoft.EntityFrameworkCore;
using minimaltodo.Models;

namespace minimaltodo.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Tarefa> Tarefas { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { 

        }
        public static async Task VerificaDBExiste(IServiceProvider services, ILogger logger, string connectionString)
        {
            logger.LogInformation("Verifica se o banco de dados existe e esteja na string de conexão :" +
                " '{connectionString}'", connectionString);

            using var db = services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }
    }
}