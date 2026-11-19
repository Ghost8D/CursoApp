using CursoApp.Core.Businness;
using CursoApp.Core.Interfaces;
using CursoApp.Core.Services;
using CursoApp.Views;
using CursoApp.ViewsModel;
using Microsoft.Extensions.Logging;

namespace CursoApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            string rutaCarpeta = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string rutaArchivo = Path.Combine(rutaCarpeta, "historial_chat.json");

            builder.Services.AddTransient<IGeminiService>(sp => new GeminiService("", rutaArchivo));

            builder.Services.AddSingleton<UsuarioBusiness>();
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<Login>();

            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainViewModel>();

            return builder.Build();
        }
    }
}
