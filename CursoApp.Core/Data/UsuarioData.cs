using CursoApp.Core.Models;

namespace CursoApp.Core.Data
{
    public class UsuarioData
    {
        public Task<List<Usuario>> GetUsersAsync()
        {
            var lista = new List<Usuario>
            {
                new Usuario { Id = 1, UserName = "Victor", Password = "1234"}
            };
            return Task.FromResult(lista);
        }
    }
}
