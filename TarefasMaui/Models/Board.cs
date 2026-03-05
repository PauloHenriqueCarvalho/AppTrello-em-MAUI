using System;
using System.Collections.Generic;
using TarefasMaui.Models;

namespace TarefasAPI_v2.Models;

public partial class Board
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public int UsuarioCriadorId { get; set; }

    public DateTime DataCadastro { get; set; }

    public virtual ICollection<Coluna> Colunas { get; set; } = new List<Coluna>();

    public virtual ICollection<Tarefa> Tarefas { get; set; } = new List<Tarefa>();

    public virtual Usuarios UsuarioCriador { get; set; } = null!;

    public virtual ICollection<Usuarios> Usuarios { get; set; } = new List<Usuarios>();
}
