using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TarefasAPI_v2.Models;

namespace TarefasMaui.Models
{
    public class Usuarios
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
}
