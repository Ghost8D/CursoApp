using static CursoApp.Core.Enums.ListaAccionesLens;

namespace CursoApp.Core.Interfaces
{
    public interface IGeminiService
    {
        Task<string> AnalizarImagenAsync(byte[] imagen, AccionesLens accion, string modelo);
        Task<string> EnviarMensajeAsync(string mensaje, string modelo);
        Task<byte[]> GenerarRespuestaVoz(string texto, string modelo);
        Task<List<string>> ObtenerModelosDisponiblesAsync();
        void ReiniciarConversacion();
    }
}
