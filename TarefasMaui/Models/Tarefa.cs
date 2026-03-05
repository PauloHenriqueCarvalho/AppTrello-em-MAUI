using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TarefasAPI_v2.Models;

namespace TarefasMaui.Models
{
    public class Tarefa
    {
        public int Id { get; set; }

        public int BoardId { get; set; }

        public int ColunaId { get; set; }

        public int UsuarioCriadorId { get; set; }

        public string Titulo { get; set; } = null!;

        public string Descricao { get; set; } = null!;

        public DateTime? DataVencimento { get; set; }

        public DateTime DataCadastro { get; set; }

        public virtual Board Board { get; set; } = null!;

        public virtual Coluna Coluna { get; set; } = null!;
        public bool Arquivada { get; set; }

        public virtual Usuarios UsuarioCriador { get; set; } = null!;

        public virtual ICollection<Usuarios> Usuarios { get; set; } = new List<Usuarios>();

    }
}
