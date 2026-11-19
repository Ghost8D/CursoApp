using CursoApp.Core.Interfaces;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;
using static CursoApp.Core.Enums.ListaAccionesLens;
using IOFile = System.IO.File;

namespace CursoApp.Core.Services
{
    public class GeminiService : IGeminiService
    {
        #region Variables...
        private readonly List<Content> _historialChat = new();
        private readonly Client _client;
        private readonly string _rutaArchivoJson;
        #endregion

        #region Constructor...
        public GeminiService(string apiKey, string rutaArchivoJson)
        {
            _client = new Client(apiKey: apiKey);
            _rutaArchivoJson = rutaArchivoJson;
        }
        #endregion

        #region Private Methods...
        private List<Content> CargarHistorial()
        {
            if (!IOFile.Exists(_rutaArchivoJson))
            {
                return new List<Content>();
            }
            try
            {
                string jsonString = IOFile.ReadAllText(_rutaArchivoJson);
                return JsonSerializer.Deserialize<List<Content>>(_rutaArchivoJson) ?? new List<Content>();
            }
            catch
            {
                return new List<Content>();
            }
        }
        private void GuardarHistorial(List<Content> historial)
        {
            try
            {
                var opciones = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(historial, opciones);
                IOFile.WriteAllText(_rutaArchivoJson, jsonString);
            }
            catch(Exception ex)
            {
                throw new Exception($"Error al guardar el historial{ex.Message}");
            }
        }
        #endregion

        #region Public Methods...

        public async Task<string> AnalizarImagenAsync(byte[] imagen, AccionesLens accion, string modelo)
        {
            try
            {
                string prompt = accion switch
                {
                    AccionesLens.Explicar => "Explica la siguiente imagen proporcionada",
                    AccionesLens.Traducir => "Traduce el texto de la siguiente imagen proporcionada",
                    AccionesLens.Buscar => "Busca información relacionada con la siguiente imagen proporcionada",
                    _ => throw new ArgumentException("Acción no válida", nameof(accion)),
                };

                try
                {
                    var historial = CargarHistorial();
                    var parts = new List<Part> { Part.FromText(prompt)};

                    if(imagen != null && imagen.Length > 0)
                    {
                        parts.Add(Part.FromBytes(imagen, "image/png"));
                    }
                    var UserContent = new Content { Role = "user", Parts = parts };
                    historial.Add(UserContent);

                    var respuesta = await _client.Models.GenerateContentAsync(
                        model: modelo,
                        contents: historial
                        );

                    var respuestaFinal = string.IsNullOrWhiteSpace(respuesta.Text) ? "Lo siento, no pude generar una respuesta" : respuesta.Text;
                    historial.Add(new Content { Role = "model", Parts = new List<Part> {Part.FromText(respuestaFinal)} });
                    GuardarHistorial(historial);
                    return respuestaFinal;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al generar respuesta: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al analizar la imagen: {ex.Message}");
            }
        }

        public async Task<string> EnviarMensajeAsync(string mensaje, string modelo)
        {
            try
            {
                var historial = CargarHistorial();
                var userContent = new Content
                {
                    Role = "user",
                    Parts = new List<Part> { Part.FromText(mensaje) }
                };
                historial.Add(userContent);

                var response = await _client.Models.GenerateContentAsync(
                    model: modelo,
                    contents: historial
                );

                var respuestaFinal = string.IsNullOrWhiteSpace(response.Text) ? "Lo siento, no pude generar una respuesta." : response.Text;
                historial.Add(new Content { Role = "model", Parts = new List<Part> { Part.FromText(respuestaFinal) } });
                GuardarHistorial(historial);
                return respuestaFinal;
            }
            catch(Exception ex)
            {
                throw new Exception($"Ocurrio un error al enviar el mensaje: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> GenerarRespuestaVoz(string texto, string modelo)
        {
            try
            {
                var respustavoz = await _client.Models.GenerateContentAsync(
                    model: modelo,
                    contents: new List<Content>
                    {
                        new Content
                        {
                            Role = "user",
                            Parts = new List<Part>
                            {
                                Part.FromText($"Lee en voz alta y de forma natural el siguiente texto: {texto}")
                            }
                        }
                    });
                var audio = respustavoz.Candidates?.FirstOrDefault()?.Content.Parts.FirstOrDefault(p => p.InlineData != null);
                return audio?.InlineData?.Data ?? Array.Empty<byte>();
            }
            catch(Exception ex)
            {
                throw new Exception($"Ocurrio un error al generar la voz {ex.Message}");
            }
        }

        public async Task<List<string>> ObtenerModelosDisponiblesAsync()
        {
            var models = await _client.Models.ListAsync();
            var nombres = new List<string>();

            await foreach (var model in models)
            {
                if (string.IsNullOrEmpty(model.Name))
                {
                    nombres.Add(model.Name);
                }
            }
            return nombres;
        }

        public void ReiniciarConversacion()
        {
            if (IOFile.Exists(_rutaArchivoJson))
            {
                IOFile.Delete(_rutaArchivoJson);
            }
        }
        #endregion
    }
}
