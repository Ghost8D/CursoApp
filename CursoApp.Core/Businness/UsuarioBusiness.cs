using CursoApp.Core.Data;
using CursoApp.Core.Interfaces;
using CursoApp.Core.Models;

namespace CursoApp.Core.Businness
{
    public class UsuarioBusiness : IUserInterface
    {
        public async Task<List<Usuario>> GetUsuariosAsync()
        {
           UsuarioData datos = new UsuarioData();
           return await datos.GetUsersAsync();
        }
    }
}
