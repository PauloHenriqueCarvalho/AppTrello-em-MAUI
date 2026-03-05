using TarefasAPI_v2.Models;
using TarefasMaui.Helpers;
using TarefasMaui.Models;
using TarefasMaui.Services;
using static TarefasMaui.Services.PadraoService;

namespace TarefasMaui.View;


[QueryProperty(nameof(TarefaId), "Id")]
public partial class AdicionarTarefaView : ContentPage
{
    private int _tarefaId { get; set; }
    private Task _carregarPickersTask;
    public int TarefaId { get => _tarefaId; set { _tarefaId = value; CarregarTarefaEditar(_tarefaId); } }

    public AdicionarTarefaView()
    {
        InitializeComponent();
        _carregarPickersTask = Carregar();
    }

    private async void CarregarTarefaEditar(int id)
    {
        await _carregarPickersTask;

        Title = "Editar Tarefa";
        btnCadastrar.Text = "Atualizar";

        var tarefa = await new PadraoService().GetTarefasUma(id);
        if (tarefa == null) return;

        txtTitulo.Text = tarefa.Titulo;
        txtDescricao.Text = tarefa.Descricao;
        datePicker.Date = tarefa.DataVencimento ?? DateTime.Now;
        timePicker.Time = tarefa.DataVencimento?.TimeOfDay ?? DateTime.Now.TimeOfDay;


        var listaStatus = pckStatus.ItemsSource as System.Collections.Generic.IEnumerable<Coluna>;
        if (listaStatus != null)
        {
            pckStatus.SelectedItem = listaStatus.FirstOrDefault(s => s.Id == tarefa.ColunaId);
        }

        var listaUsuarios = pckUsuario.ItemsSource as System.Collections.Generic.IEnumerable<Usuarios>;
        if (listaUsuarios != null)
        {
            pckUsuario.SelectedItem = listaUsuarios.FirstOrDefault(s => s.Id == tarefa.Usuarios.First().Id);
        }
    }



    private async Task Carregar()
    {
        var status = await new PadraoService().GetColunas();
        var usuarios = await new PadraoService().GetUsuarios();


        var newList = usuarios.Where(x => Global.board.Usuarios.Any(y => y.Id == x.Id)).ToList();

        if (newList.Count == 0)
        {
            newList.Add(Global.user);
        }
        pckStatus.ItemsSource = status;
        pckUsuario.ItemsSource = newList;
    }
    public async void OnCriar(object sender, EventArgs e)
    {
        var st = pckStatus.SelectedItem as Coluna;
        var ut = pckUsuario.SelectedItem as Usuarios;

        if (st == null)
        {
            await DisplayAlert("Erro", "Selecione um Status válido.", "OK");
            return;
        }

        if (ut == null)
        {
            await DisplayAlert("Erro", "O Destinatário selecionado é inválido (Tipo incorreto).", "OK");
            return;
        }

        if (datePicker.Date < DateTime.Today)
        {
            await DisplayAlert("Erro", "A data não pode ser anterior a hoje!", "OK");
            return;
        }

        if (datePicker.Date == DateTime.Today && timePicker.Time < DateTime.Now.TimeOfDay)
        {
            await DisplayAlert("Erro", "O horário selecionado já passou!", "OK");
            return;
        }
        try
        {

            var dataSelecionada = datePicker.Date;
            var horaSelecionada = timePicker.Time;

            DateTime dataCompleta = new DateTime(
                dataSelecionada.Year,
                dataSelecionada.Month,
                dataSelecionada.Day,
                horaSelecionada.Hours,
                horaSelecionada.Minutes,
                0);

            var tarefa = new CreateTarefaDTO
            {
                Titulo = txtTitulo.Text,
                Descricao = txtDescricao.Text,
                Vencimento = dataCompleta,
                BoardId = Global.board.Id,
                ColunaId = st.Id,
                UsuarioCriador = Global.user.Id,
                UsuarioDestino = ut.Id

            };

            if (TarefaId == 0)
            {
                await new PadraoService().CadastrarTarefas(tarefa);
                await DisplayAlert("Sucesso", "Salvo com sucesso!", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                var t = await new PadraoService().AlterarTarefa(tarefa, TarefaId);
                if (t)
                {
                    await DisplayAlert("Sucesso", "Alterado com sucesso!", "OK");

                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await DisplayAlert("Erro", "Erro ao alterar! " + TarefaId, "OK");

                }
            }

        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }
}