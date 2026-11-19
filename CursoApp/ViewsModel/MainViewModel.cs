using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CursoApp.Core.Interfaces;
using static CursoApp.Core.Enums.ListaAccionesLens;

namespace CursoApp.ViewsModel
{
    public partial class MainViewModel : ObservableObject
    {
        #region Variables..
        private readonly IGeminiService _geminiService;
        private byte[] _imageData;
        private bool PuedeEnviarMensaje() => TieneImagen && !Cargando;
        private bool PuedeLeerVoz() => !string.IsNullOrEmpty(ResultadoTexto);
        #endregion
        //Propiedades
        [ObservableProperty]
        private ImageSource? _imagenCapturada;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LeerVozCommand))]
        private string _resultadoTexto = "Toma una foto o selecciona una imagen para analizarla.";
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EnviarMensajeChatCommand))]
        private bool _cargando;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EnviarMensajeChatCommand))]
        private bool _tieneImagen;
        [ObservableProperty]
        private string _textoPreguntaChat = string.Empty;
        [ObservableProperty]
        private string _ModeloSeleccionado = string.Empty;
        [ObservableProperty]
        private List<string> _listaModelos = new List<string>();

        public IRelayCommand CapturarFotoCommand { get; }
        public IRelayCommand<string> EjecutarAccionCommand { get; }
        public IRelayCommand EnviarMensajeChatCommand { get; }
        public IRelayCommand LeerVozCommand { get; }
        public IRelayCommand SeleccionaImagenGaleria { get; }

        #region Constructor...
        public MainViewModel(IGeminiService geminiService)
        {
            _geminiService = geminiService;
            CapturarFotoCommand = new AsyncRelayCommand(CapturarFotoAsync);
            EjecutarAccionCommand = new AsyncRelayCommand<string>(ProcesarAccionAsync);
            EnviarMensajeChatCommand = new AsyncRelayCommand(EnviarMensajeChatAsync, PuedeEnviarMensaje);
            LeerVozCommand = new AsyncRelayCommand(LeerTextoAsync, PuedeLeerVoz);
            SeleccionaImagenGaleria = new AsyncRelayCommand(SeleccionarImagenGaleriaAsync);
            _ = InicializarModelosAsync();
        }
        #endregion

        #region Private Methods...
        private async Task InicializarModelosAsync()
        {
            try
            {
                ListaModelos = await _geminiService.ObtenerModelosDisponiblesAsync();

                ModeloSeleccionado = ListaModelos.FirstOrDefault(m => m.Contains("flash"))
                                     ?? ListaModelos.FirstOrDefault()
                                     ?? string.Empty;
            }
            catch (Exception)
            {
                ListaModelos = new List<string> { "gemini-1.5-flash", "gemini-1.5-pro" };
                ModeloSeleccionado = "gemini-1.5-flash";
            }
        }
        private async Task CapturarFotoAsync()
        {
            try
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo == null) return;
                using (var stream = await photo.OpenReadAsync())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        _imageData = memoryStream.ToArray();
                    }
                }
                if (_imageData == null || _imageData.Length == 0)
                {
                    ResultadoTexto = "No se pudo obtener los datos de la imagen.";
                    return;
                }
                _geminiService.ReiniciarConversacion();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ImagenCapturada = ImageSource.FromStream(() => new MemoryStream(_imageData));
                    TieneImagen = true;
                    ResultadoTexto = "Selecciona qué deseas hacer con la imagen (Explicar, Traducir, Buscar...).";
                    TextoPreguntaChat = string.Empty;
                });
            }
            catch (Exception ex)
            {
                ResultadoTexto = $"Ocurrió un error al procesar la foto. {ex.Message}";
            }
        }
        private async Task ProcesarAccionAsync(string? accion)
        {
            if (_imageData == null || _imageData.Length == 0)
            {
                ResultadoTexto = "No hay imagen para procesar. Por favor, captura una foto primero.";
                return;
            }

            try
            {
                if (!Enum.TryParse<AccionesLens>(accion, true, out var acciones))
                    acciones = AccionesLens.Explicar;

                Cargando = true;
                ResultadoTexto = "Procesando la imagen con Gemini IA la imagen Capturada. Espere un Momento...";

                var resultado = await _geminiService.AnalizarImagenAsync(_imageData, acciones, ModeloSeleccionado);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ResultadoTexto = resultado;
                    Cargando = false;
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ResultadoTexto = $"Error al procesar la acción: {ex.Message}";
                    Cargando = false;
                });
            }
        }
        private async Task EnviarMensajeChatAsync()
        {
            if (string.IsNullOrWhiteSpace(TextoPreguntaChat))
            {
                ResultadoTexto = "Por favor, ingresa un mensaje para enviar.";
                return;
            }
            try
            {
                Cargando = true;
                ResultadoTexto = "Pensando respuesta...";
                var respuesta = await _geminiService.EnviarMensajeAsync(TextoPreguntaChat, ModeloSeleccionado);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ResultadoTexto = respuesta;
                    Cargando = false;
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ResultadoTexto = $"Error al enviar el mensaje: {ex.Message}";
                    Cargando = false;
                });
            }
        }
        private async Task SeleccionarImagenGaleriaAsync()
        {
            try
            {
                var photo = await MediaPicker.Default.PickPhotosAsync();
                if (photo == null || !photo.Any())
                {
                    ResultadoTexto = "No se seleccionó ninguna imagen.";
                    return;
                }
                var stream = await photo.First().OpenReadAsync();
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    _imageData = memoryStream.ToArray();
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ImagenCapturada = ImageSource.FromStream(() => new MemoryStream(_imageData));
                    TieneImagen = true;
                    ResultadoTexto = "Selecciona qué deseas hacer con la imagen (Explicar, Traducir, Buscar...).";
                    TextoPreguntaChat = string.Empty;
                });
            }
            catch (Exception ex)
            {
                ResultadoTexto = $"Ocurrió un error al seleccionar la imagen. {ex.Message}";
            }
        }
        private async Task LeerTextoAsync()
        {
            if (string.IsNullOrWhiteSpace(ResultadoTexto))
            {
                ResultadoTexto = "No hay texto para leer.";
                return;
            }
            try
            {
                var locales = await TextToSpeech.Default.GetLocalesAsync();
                var localeEspanol = locales.FirstOrDefault(l => l.Language.StartsWith("es", StringComparison.OrdinalIgnoreCase));

                var settings = new SpeechOptions
                {
                    Volume = 1.0f,
                    Pitch = 1.0f,
                    Locale = localeEspanol ?? locales.FirstOrDefault()
                };

                await TextToSpeech.Default.SpeakAsync(ResultadoTexto, settings);
            }
            catch (Exception ex)
            {
                ResultadoTexto = $"Error al leer el texto: {ex.Message}";
            }
        }
        #endregion
    }
}
