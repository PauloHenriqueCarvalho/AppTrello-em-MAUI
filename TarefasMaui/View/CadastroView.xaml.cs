using TarefasMaui.Services;
using static TarefasMaui.Services.PadraoService;

namespace TarefasMaui.View;

public partial class CadastroView : ContentPage
{
    public CadastroView()
    {
        InitializeComponent();
    }
    public async void OnCriar(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtLogin.Text) ||
            string.IsNullOrWhiteSpace(txtNome.Text) ||
            string.IsNullOrWhiteSpace(txtSenha.Text))
        {
            await DisplayAlert("Erro", "Preencha todos os campos.", "OK");
            return;
        }

        var dto = new CreateUserDTO
        {
            Login = txtLogin.Text,
            Nome = txtNome.Text,
            Senha = txtSenha.Text,
        };

        var u = await new PadraoService().CadastrarUsuario(dto);
        if (u == null)
        {
            await DisplayAlert("Erro", "Ja existe usuario com esse login.", "OK");
            return;
        }
        await DisplayAlert("Sucesso", "Cadastrado com sucesso.", "OK");
        await Shell.Current.GoToAsync("..");
    }

}