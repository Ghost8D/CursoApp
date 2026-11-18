using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CursoApp.Core.Businness;

namespace CursoApp.ViewsModel
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly UsuarioBusiness _UserBusiness;
        [ObservableProperty]
        private string username;
        [ObservableProperty]
        private string password;

        public LoginViewModel(UsuarioBusiness userbusinnes)
        {
            _UserBusiness = userbusinnes;
        }

        [RelayCommand]
        private async Task Login()
        {
            if(string.IsNullOrEmpty(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await App.Current.MainPage.DisplayAlertAsync("Error", "Por favor ingrese sus credenciales", "OK");
                return;
            }
            var listaUsuarios = await _UserBusiness.GetUsuariosAsync();

            var UsuarioEncontrado = listaUsuarios.FirstOrDefault(u => 
            u.UserName.Equals(Username, StringComparison.OrdinalIgnoreCase) && u.Password == Password);

            if (UsuarioEncontrado != null)
            {
                await App.Current.MainPage.DisplayAlertAsync("Exito", "Inicio de Sesion exitoso", "OK");
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                await App.Current.MainPage.DisplayAlertAsync("Error", "Usuario o contraseña incorrectos", "OK");
            }
        }
    }
}
