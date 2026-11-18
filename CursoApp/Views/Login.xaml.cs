using CursoApp.ViewsModel;

namespace CursoApp.Views;

public partial class Login : ContentPage
{
	public Login(LoginViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}