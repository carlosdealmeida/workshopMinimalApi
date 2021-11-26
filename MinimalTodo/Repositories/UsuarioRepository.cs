using minimaltodo.Models;

namespace minimaltodo.Repositories
{
    public class UsuarioRepository
    {
        public static Usuario Get(string username, string password)
        {
            var users = new List<Usuario>
            {
                new Usuario { Id = 1, Username = "chefao", Password = "chefao123", Role = "admin" },
                new Usuario { Id = 2, Username = "aspira", Password = "aspira123", Role = "employee"}
            };

            return users.Where(x => x.Username.ToLower() == username.ToLower() && x.Password == password).FirstOrDefault();
        }
    }
}