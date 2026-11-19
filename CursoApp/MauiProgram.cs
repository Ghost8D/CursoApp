using CursoApp.Core.Businness;
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

            builder.Services.AddSingleton<UsuarioBusiness>();
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<Login>();
            builder.Services.AddSingleton<MainPage>();

            return builder.Build();
        }
    }
}
