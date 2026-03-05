using System;
using System.Collections.Generic;

namespace TarefasAPI_v2.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Login { get; set; } = null!;

    public string SenhaHash { get; set; } = null!;

    public DateTime DataCadastro { get; set; }

    public virtual ICollection<Board> Boards { get; set; } = new List<Board>();

    public virtual ICollection<Tarefa> Tarefas { get; set; } = new List<Tarefa>();

    public virtual ICollection<Board> BoardsNavigation { get; set; } = new List<Board>();

    public virtual ICollection<Tarefa> TarefasNavigation { get; set; } = new List<Tarefa>();
}
