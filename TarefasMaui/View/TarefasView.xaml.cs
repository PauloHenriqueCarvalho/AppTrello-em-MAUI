using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using TarefasAPI_v2.Models;
using TarefasMaui.Helpers;
using TarefasMaui.Models;
using TarefasMaui.Services;
using static TarefasMaui.Services.PadraoService;

namespace TarefasMaui.View;


public partial class TarefasView : ContentPage
{
    public List<StatusColuna> ColunasStatus { get; set; } = new List<StatusColuna>();
    List<Tarefa> listaVindaDaApi = new();
    public string nomeQuedro { get; set; } = Global.board.Nome;
    public TarefasView()
    {
        InitializeComponent();

        Carregar();




    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Carregar();
        if (Global.board?.UsuarioCriador.Id == Global.user?.Id)
        {
            btnSair.IsVisible = true;
        }
        else
        {
            btnSair.IsVisible = false; // Sempre garanta o estado oposto
        }
    }

    private async void Carregar()
    {
        var colunas = await new PadraoService().GetColunas();
        listaVindaDaApi = await new PadraoService().GetTarefas();

        colunas ??= new List<Coluna>();
        listaVindaDaApi ??= new List<Tarefa>();

        AtualizarTela(colunas);
    }
    private void AtualizarTela(List<Coluna> colunas = null)
    {
        if (colunas == null)
            colunas = ColunasStatus?.Select(x => x.Coluna).ToList() ?? new List<Coluna>();

        var tarefasFiltradas = chkArquivadas.IsChecked
            ? listaVindaDaApi
            : listaVindaDaApi.Where(x => !x.Arquivada).ToList();

        string termo = txtBusca.Text?.ToLower() ?? "";
        if (!string.IsNullOrWhiteSpace(termo))
        {
            tarefasFiltradas = tarefasFiltradas.Where(x =>
                (x.Titulo != null && x.Titulo.ToLower().Contains(termo)) ||
                (x.Usuarios != null && x.Usuarios.Any(u => u.Nome.ToLower().Contains(termo)))
            ).ToList();
        }

        ColunasStatus = colunas
            .OrderBy(x => x.Ordem)
            .Select(s => new StatusColuna
            {
                Coluna = s,
                Tarefas = tarefasFiltradas.Where(x => x.ColunaId == s.Id).ToList()
            }).ToList();

        BindingContext = null;
        BindingContext = this;
    }

    public async void OnStatus(object sender, EventArgs e)
    {
        string nomeColuna = await DisplayPromptAsync("Nova Coluna", "Qual o nome da nova etapa? (ex: Testes)", "Criar", "Cancelar");
        if (string.IsNullOrEmpty(nomeColuna))
            return;


        int ordem = ColunasStatus.Count + 1;

        if (ColunasStatus.Any(x => x.Coluna.Nome == nomeColuna))
        {
            await DisplayAlert("Erro!", "Ja existe uma coluna com esse nome!", "Ok");
            return;
        }
        var dto = new Coluna
        {
            Cor = "#333",
            Nome = nomeColuna,
            BoardId = Global.board.Id,
            Ordem = ordem,

        };

        await new PadraoService().CreateStatus(dto);

        await DisplayAlert("Sucesso!", "Criado com sucesso!", "Ok");
        Carregar();
    }
    public async void OnCriarTarefa(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("AdicionarTarefaView");
    }
    public async void OnExluirColuna(object sender, TappedEventArgs e)
    {
        var c = e.Parameter as Coluna;
        if (c == null) return;

        bool con = await DisplayAlert("Atenção", "Deseja excluir essa coluna?", "Sim", "Nao");
        if (con)
        {
            var r = await new PadraoService().DeletarColuna(c.Id);
            if (r)
                await DisplayAlert("Sucesso", "Quadro excluido com sucesso!", "OK");
            else
                await DisplayAlert("Erro", "Erro ao excluir!", "OK");
            Carregar();
            OnPropertyChanged(nameof(ColunasStatus));
        }
    }

    public async void OnBuscar(object sender, EventArgs e)
    {
        AtualizarTela();
    }


    Tarefa tarefaSendoArrastada;

    private async void OnMoverTarefaTapped(object sender, TappedEventArgs e)
    {
        var elemento = sender as Element;
        var tarefa = elemento?.BindingContext as Tarefa;

        if (tarefa == null) return;
        if (tarefa.Arquivada) return;


        var colunas = ColunasStatus.Select(x => x.Coluna.Nome).ToList();

        string acao = await DisplayActionSheet($"Tarefa: {tarefa.Titulo}", "Cancelar", "Excluir",
            new string[] { "Editar" }.Concat(colunas).ToArray());

        if (acao == "Cancelar" || string.IsNullOrEmpty(acao)) return;

        if (acao == "Excluir")
        {

            if (tarefa.ColunaId == ColunasStatus.OrderBy(x => x.Coluna.Ordem).LastOrDefault()?.Coluna.Id)
            {

                await DisplayAlert("Arquivado!", "Tarefa arquivada " + tarefa.Id, "Ok");
                //Arquivar
                await new PadraoService().ArquivarTarefa(tarefa.Id);
                Carregar();
                return;
            }

            bool confir = await DisplayAlert("Confirmar", $"Deseja realmente excluir a tarefa '{tarefa.Titulo}'?", "Sim", "Não");
            if (confir)
            {
                try
                {
                    await new PadraoService().DeletarTarefa(tarefa.Id);

                    await DisplayAlert("Sucesso!", "Excluido com sucesso!", "Ok");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro!", "Erro ao excluir!", "Ok");
                }
                Carregar();
            }
            else
                return;
        }
        else if (acao == "Editar")
        {
            await Shell.Current.GoToAsync($"AdicionarTarefaView?Id={tarefa.Id}");
        }
        else
        {
            var novoStatus = ColunasStatus.FirstOrDefault(c => c.Coluna.Nome == acao)?.Coluna;

            if (novoStatus != null && novoStatus.Id != tarefa.ColunaId)
            {
                await new PadraoService().AtualizarStatusTarefa(tarefa.Id, novoStatus.Id);
                Carregar();
            }
        }
    }

    private async void OnAddUser(object sender, EventArgs e)
    {
        var users = await new PadraoService().GetUsuarios();
        if (users == null) return;
        var listaUsuariosBoard = Global.board.Usuarios ?? new List<Usuarios>();
        var userList = users.Where(u =>
         u.Id != Global.user.Id &&
         !listaUsuariosBoard.Any(b => b.Id == u.Id)
     ).ToList();


        var nomes = userList.Select(u => u.Nome).ToArray();
        if (!nomes.Any())
        {
            await DisplayAlert("Atenção", "Todos usuarios ja estao nesse quadro!", "Ok");
            return;
        }
        string escolha = await DisplayActionSheet("Convidar para o quadro", "Cancelar", null, nomes);

        if (string.IsNullOrEmpty(escolha) || escolha == "Cancelar")
            return;

        var userSelecionado = userList.FirstOrDefault(u => u.Nome == escolha);

        if (userSelecionado != null)
        {
            var dto = new AddUserBoardDTO
            {
                IdUsuario = userSelecionado.Id,
                idBoard = Global.board.Id,
            };

            await new PadraoService().AddUserBoard(dto);
            await DisplayAlert("Sucesso", $"{userSelecionado.Nome} adicionado!", "Ok");

            var todosBoards = await new PadraoService().GetBoardsUsuario();
            if (todosBoards != null)
            {
                Global.board = todosBoards.FirstOrDefault(x => x.Id == Global.board.Id);
            }
        }
    }

    StatusColuna colunaSendoArrastada;
    private void OnColunaDragStarting(object sender, DragStartingEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("MÉTODO DE ARRASTAR DISPARADO!");
        var elemento = sender as Element;
        var coluna = colunaSendoArrastada = elemento?.BindingContext as StatusColuna;
        if (coluna == null) return;
        colunaSendoArrastada = coluna;
        e.Data.Properties.Add("ColunaId", coluna);
    }
    private async void OnColunaDrop(object sender, DropEventArgs e)
    {
        var elementoDestino = sender as Element;
        var colunaDestino = elementoDestino?.BindingContext as StatusColuna;

        if (colunaSendoArrastada == null || colunaSendoArrastada == colunaDestino)
            return;

        int indiceOrigem = ColunasStatus.IndexOf(colunaSendoArrastada);
        int indiceDestino = ColunasStatus.IndexOf(colunaDestino);

        if (indiceOrigem != -1 && indiceDestino != -1)
        {
            ColunasStatus.RemoveAt(indiceOrigem);
            ColunasStatus.Insert(indiceDestino, colunaSendoArrastada);

            for (int i = 0; i < ColunasStatus.Count; i++)
            {
                ColunasStatus[i].Coluna.Ordem = i + 1;
            }

            var tempLista = ColunasStatus.ToList();
            ColunasStatus = tempLista;

            BindingContext = null;
            BindingContext = this;
            await SalvarNovaOrdem();
        }
    }

    private async Task SalvarNovaOrdem()
    {
        try
        {
            var ordens = ColunasStatus;
            await new PadraoService().AtualizarOrdensColunas(ordens);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Falha ao salvar ordem: " + ex.Message, "OK");
        }
    }

    private async void OnRemoveUser(object sender, EventArgs e)
    {
        var listaUsuariosBoard = Global.board.Usuarios.Where(x => x.Id != Global.user.Id).ToList();

        if (!listaUsuariosBoard.Any())
        {
            await DisplayAlert("Atenção", "Não existem outros usuários neste quadro!", "Ok");
            return;
        }

        var nomes = listaUsuariosBoard.Select(u => u.Nome).ToArray();

        string escolha = await DisplayActionSheet("Remover do quadro", "Cancelar", null, nomes);

        if (string.IsNullOrEmpty(escolha) || escolha == "Cancelar") return;

        var userSelecionado = listaUsuariosBoard.FirstOrDefault(u => u.Nome == escolha);
        if (userSelecionado == null) return;

        var dto = new AddUserBoardDTO
        {
            IdUsuario = userSelecionado.Id,
            idBoard = Global.board.Id,
        };

        try
        {
            await new PadraoService().RemoveUserBoard(dto);

            await DisplayAlert("Sucesso", $"{userSelecionado.Nome} foi removido!", "Ok");

            var boards = await new PadraoService().GetBoardsUsuario();
            Global.board = boards.FirstOrDefault(x => x.Id == Global.board.Id);

            Carregar();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível remover: " + ex.Message, "Ok");
        }
    }

    private void OnFiltroArquivadas(object sender, CheckedChangedEventArgs e)
    {
        AtualizarTela();
    }

    private void FiltrarExibir()
    {

    }

    private async void OnSairQuadro(object sender, EventArgs e)
    {
        var usuariosBoard = Global.board.Usuarios;
        usuariosBoard = usuariosBoard.Where(x => x.Id != Global.user.Id).ToList();


        if (usuariosBoard.Count == 0)
        {
            await DisplayAlert("Erro", "Não existe outro usuario nesse board! Exclua ele para sair", "OK");
            return;
        }

        string[] nomesuser = usuariosBoard.Select(x => x.Nome).ToArray();
        string escolha = await DisplayActionSheet("Escolha o usuario que deseja transferir a liderança!", "Cancelar", null, nomesuser);

        if (!escolha.Equals("Cancelar") && !string.IsNullOrEmpty(escolha))
        {
            var novoLider = usuariosBoard.FirstOrDefault(x => x.Nome == escolha);

            await new PadraoService().TrocarLider(Global.board.Id, novoLider.Id);
            await Shell.Current.GoToAsync("BoardsView");
        }

    }
}