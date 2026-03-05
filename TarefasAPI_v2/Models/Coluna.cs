using System;
using System.Collections.Generic;

namespace TarefasAPI_v2.Models;

public partial class Coluna
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Cor { get; set; } = null!;

    public int Ordem { get; set; }

    public int BoardId { get; set; }

    public DateTime DataCadastro { get; set; }

    public virtual Board Board { get; set; } = null!;

    public virtual ICollection<Tarefa> Tarefas { get; set; } = new List<Tarefa>();
}
