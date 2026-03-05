using System.Collections.ObjectModel;
using TarefasAPI_v2.Models;
using TarefasMaui.Helpers;
using TarefasMaui.Services;
using static TarefasMaui.Services.PadraoService;

namespace TarefasMaui.View;

public partial class Boards : ContentPage
{
    public ObservableCollection<Board> BoardsList { get; set; } = new();


    public Boards()
    {
        InitializeComponent();
        BindingContext = this;
        Carregar();

    }

    public async void Carregar()
    {
        var l = await new PadraoService().GetBoardsUsuario();
        if (l == null || !l.Any()) return;

        BoardsList.Clear();
        foreach (var board in l)
        {
            BoardsList.Add(board);
        }

        OnPropertyChanged(nameof(BoardsList));

    }

    private async void OnBoardSelecionado(object sender, TappedEventArgs e)
    {
        var board = e.Parameter as Board;
        if (board == null) return;
        Global.board = board;
        await Shell.Current.GoToAsync("TarefasView");
    }

    private async void OnNovoBoard(object sender, EventArgs e)
    {
        string nomeBoard = await DisplayPromptAsync(
            "Novo Quadro",
            "Digite o nome do quadro:",
            "Criar",

            "Cancelar",
            "Ex: Projeto de vendas");
        if (!string.IsNullOrWhiteSpace(nomeBoard))
        {
            try
            {
                var dto = new CreateBoardDTO
                {
                    nome = nomeBoard,
                    IdCriador = Global.user.Id
                };
                var s = await new PadraoService().CreateBoard(dto);
                await DisplayAlert("Secesso", "Quadro criado com sucesso!", "OK");
                Carregar();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Não foi possivel criar o quadro", "OK");
            }
        }
    }

    private async void OnOpcoes(object sender, EventArgs e)
    {
    }

    private async Task ProcessarTransferencia(Board b)
    {
        var boardsUsuario = await new PadraoService().GetBoardsUsuario();


        var destino = boardsUsuario.Where(x => x.Id != b.Id).ToList();

        string[] nomesBoards = destino.Select(x => x.Nome).ToArray();

        string escolha = await DisplayActionSheet("Escolha o quadro de destino:", "Cancelar", null, nomesBoards);
        if (escolha != "Cancelar" && !string.IsNullOrEmpty(escolha))
        {
            var boardDestino = destino.FirstOrDefault(x => x.Nome == escolha);

            await new PadraoService().TransferirTarefas(b.Id, boardDestino.Id);
            await new PadraoService().ExcluirtBoard(b.Id);
        }

    }

    private async void OnExcluir(object sender, TappedEventArgs e)
    {
        var b = e.Parameter as Board;
        if (b == null) return;
        Global.board = b;
        var tarefasQuadro = await new PadraoService().GetTarefas();

        if (tarefasQuadro.Count == 0)
        {
            bool con = await DisplayAlert("Atenção", "Deseja excluir esse quadro?", "Sim", "Nao");
            if (con)
            {
                var r = await new PadraoService().ExcluirtBoard(b.Id);
                if (r)
                    await DisplayAlert("Sucesso", "Quadro excluido com sucesso!", "OK");
                else
                    await DisplayAlert("Erro", "Erro ao excluir!", "OK");
                Carregar();
                OnPropertyChanged(nameof(BoardsList));
            }
            return;
        }
        var boardsUsuario = await new PadraoService().GetBoardsUsuario();
        if (boardsUsuario.Count == 1)
        {
            await DisplayAlert("Erro", "Impossivel excluir esse board com tarefas pendentes! Nenhum outro board disponivel", "OK");
            return;
        }

        var destino = boardsUsuario.Where(x => x.Id != b.Id).ToList();

        if (destino.Count == 0)
        {
            await DisplayAlert("Erro", "Impossivel excluir esse board com tarefas pendentes! Nenhum outro board disponivel", "OK");
            return;
        }


        string action = await DisplayActionSheet($"O quadro '{b.Nome}' tem {tarefasQuadro.Count} tarefas. O que deseja fazer?", "Cancelar", null, "Excluir tudo", "Transferir tarefas para outro quadro");
        if (action.Equals("Excluir tudo"))
        {
            var r = await new PadraoService().ExcluirtBoard(b.Id);
            if (r)
                await DisplayAlert("Sucesso", "Quadro excluido com sucesso!", "OK");
            else
                await DisplayAlert("Erro", "Erro ao excluir!", "OK");
            Carregar();
            OnPropertyChanged(nameof(BoardsList));
        }
        else if (action.Equals("Transferir tarefas para outro quadro"))
        {
            //Transferir para outro quadro
            await ProcessarTransferencia(b);
            Carregar();
            OnPropertyChanged(nameof(BoardsList));
        }


    }

    private async void OnEditarNome(object sender, TappedEventArgs e)
    {
        var b = e.Parameter as Board;
        if (b == null) return;

        string novoNome = await DisplayPromptAsync("Editar board", "Digite o novo nome do quadro", "Salvar", "Cancelar", initialValue: b.Nome);
        if (!string.IsNullOrWhiteSpace(novoNome) && novoNome != b.Nome)
        {
            var res = await new PadraoService().UpdateName(b.Id, novoNome);
            if (res)
                await DisplayAlert("Sucesso", "Nome alterado com sucesso!", "OK");
            else
                await DisplayAlert("Erro", "Erro ao editar nome!", "OK");
            Carregar();
            OnPropertyChanged(nameof(BoardsList));
        }
    }
}