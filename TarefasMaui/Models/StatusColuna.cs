using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TarefasAPI_v2.Models;

namespace TarefasMaui.Models
{
    public class StatusColuna
    {
        public Coluna Coluna { get; set; }
        public List<Tarefa> Tarefas { get; set; }
    }
}
