using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TarefasAPI_v2.Models;

namespace TarefasAPI_v2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TarefasController : ControllerBase
    {
        protected readonly AppDbContext context;

        public TarefasController(AppDbContext context)
        {
            this.context = context;
        }

        public class CreateTarefaDTO
        {
            public string Titulo { get; set; }
            public string Descricao { get; set; }
            public int BoardId { get; set; }
            public int ColunaId { get; set; }
            public DateTime Vencimento { get; set; }
            public int UsuarioCriador { get; set; }
            public int UsuarioDestino { get; set; }
        }

        [HttpGet("board/{id}")]
        public async Task<IActionResult> GetTasksBoard(int id)
        {
            var t = await context.Tarefas
                .Include(t => t.Usuarios)
                .Where(x => x.BoardId == id).ToListAsync();
            return Ok(t);
        }


        [HttpPost]
        public async Task<IActionResult> CreateTarefa([FromBody] CreateTarefaDTO dto)
        {

            var tarefa = new Tarefa
            {
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                DataVencimento = dto.Vencimento,
                BoardId = dto.BoardId,
                ColunaId = dto.ColunaId,
                UsuarioCriadorId = dto.UsuarioCriador,

            };

            var u = await context.Usuarios.FindAsync(dto.UsuarioDestino);
            tarefa.Usuarios.Add(u);
            await context.Tarefas.AddAsync(tarefa);
            await context.SaveChangesAsync();
            return Ok();

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var l = await context.Tarefas
                .Include(t => t.Usuarios)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (l == null) return NotFound("Nenhuma tarefa encontrada");
            return Ok(l);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await context.Tarefas.FirstOrDefaultAsync(x => x.Id == id);
            if (t == null) return NotFound("Tarefa não encontrada!");
            context.Tarefas.Remove(t);
            await context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> AlterarTarefa(int id, CreateTarefaDTO dto)
        {
            var t = await context.Tarefas
                .Include(t => t.Usuarios)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (t == null) return NotFound("Tarefa não encontrada");

            var u = await context.Usuarios.FirstOrDefaultAsync(x => x.Id == dto.UsuarioDestino);
            if (u == null) return NotFound("Usuario não existe");

            t.Titulo = dto.Titulo;
            t.Descricao = dto.Descricao;
            t.ColunaId = dto.ColunaId;
            t.Usuarios.Clear();
            t.Usuarios.Add(u);
            t.DataVencimento = dto.Vencimento;
            await context.SaveChangesAsync();
            return NoContent();
        }


        [HttpPut("transferir-tarefas/{origemId}/{destinoId}")]
        public async Task<IActionResult> TransferirTarefas(int origemId, int destinoId)
        {
            var t = await context.Tarefas
                .Include(t => t.Usuarios)
                .Where(x => x.BoardId == origemId).ToListAsync();

            if (!t.Any()) return NotFound("Tarefas não encontrada");


            var colunaDestino = await context.Colunas
                .Where(x => x.BoardId == destinoId)
                .OrderBy(c => c.Ordem)
                .FirstOrDefaultAsync();

            if (colunaDestino == null)
            {
                colunaDestino = new Coluna
                {
                    Nome = "Backlog",
                    BoardId = destinoId,
                };
                context.Colunas.Add(colunaDestino);
                await context.SaveChangesAsync();
            }

            foreach (var i in t)
            {
                i.BoardId = destinoId;
                i.ColunaId = colunaDestino.Id;
            }

            await context.SaveChangesAsync();
            return NoContent();
        }


        [HttpPut("arquivar/{id}")]
        public async Task<IActionResult> Arquivar(int id)
        {
            var t = await context.Tarefas
                .Include(t => t.Usuarios)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (t == null) return NotFound("Tarefa não encontrada");


            t.Arquivada = true;
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
