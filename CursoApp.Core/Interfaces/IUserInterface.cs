using CursoApp.Core.Models;

namespace CursoApp.Core.Interfaces
{
    public interface IUserInterface
    {
        Task<List<Usuario>> GetUsuariosAsync();
    }
}
