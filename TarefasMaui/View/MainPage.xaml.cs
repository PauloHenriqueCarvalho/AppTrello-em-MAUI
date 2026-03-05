using System.Threading.Tasks;
using TarefasMaui.Helpers;
using TarefasMaui.Services;
using TarefasMaui.View;
using static TarefasMaui.Services.PadraoService;

namespace TarefasMaui
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }



        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var l = txtLogin.Text;
            var s = txtSenha.Text;

            if (string.IsNullOrWhiteSpace(l) || string.IsNullOrWhiteSpace(s))
            {
                await DisplayAlert("Erro", "Preencha todos os campos.", "OK");
                return;
            }
            var dto = new LoginDTO
            {
                Login = l,
                Senha = s,
            };

            try
            {
                var u = await new PadraoService().Login(dto);
                if (u == null)
                {
                    await DisplayAlert("Erro", "Login ou senha incorreto!", "OK");
                    return;
                }

                Global.user = u;
                await DisplayAlert("Sucesso", "Bem Vindo!", "OK");
                await Shell.Current.GoToAsync("BoardsView");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Erro na API: " + ex.Message, "OK");
            }




        }


        private async void OnCadastroClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("CadastroView");
        }
    }

}
