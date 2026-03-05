using System;
using System.Collections.Generic;

namespace TarefasAPI_v2.Models;

public partial class Tarefa
{
    public int Id { get; set; }

    public int BoardId { get; set; }

    public int ColunaId { get; set; }

    public int UsuarioCriadorId { get; set; }

    public string Titulo { get; set; } = null!;

    public string Descricao { get; set; } = null!;

    public DateTime? DataVencimento { get; set; }

    public DateTime DataCadastro { get; set; }

    public bool Arquivada { get; set; }

    public virtual Board Board { get; set; } = null!;

    public virtual Coluna Coluna { get; set; } = null!;

    public virtual Usuario UsuarioCriador { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
