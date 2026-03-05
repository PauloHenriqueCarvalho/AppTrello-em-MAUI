using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TarefasAPI_v2.Models;

namespace TarefasAPI_v2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColunaController : ControllerBase
    {
        protected readonly AppDbContext context;

        public ColunaController(AppDbContext context)
        {
            this.context = context;
        }

        [HttpGet("board/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var l = await context.Colunas.Where(x => x.BoardId == id).ToListAsync();
            if (!l.Any()) return NotFound("Nenhuma coluna encontrada");
            return Ok(l);
        }

        public class CreateColunaDTO
        {
            public string Nome { get; set; }
            public string Cor { get; set; }
            public int Ordem { get; set; }
            public int BoardId { get; set; }
        }

        public class UpdateStatusDTO
        {
            public int idTarefa { get; set; }
            public int idColuna { get; set; }
        }

        [HttpPut("status")]
        public async Task<IActionResult> AlterarStatus([FromBody] UpdateStatusDTO dto)
        {
            var t = await context.Tarefas.FirstOrDefaultAsync(x => x.Id == dto.idTarefa);
            var s = await context.Colunas.FirstOrDefaultAsync(x => x.Id == dto.idColuna);
            if (t == null) return NotFound("Tarefa não encontrada!");
            if (s == null) return NotFound("Coluna não encontrada!");

            t.Coluna = s;
            await context.SaveChangesAsync();
            return Ok(t);

        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await context.Colunas.FirstOrDefaultAsync(x => x.Id == id);
            if (t == null) return NotFound("Coluna não encontrada!");
            context.Colunas.Remove(t);
            await context.SaveChangesAsync();
            return NoContent();
        }


        [HttpPost]
        public async Task<IActionResult> CreateColuna(CreateColunaDTO dto)
        {
            var c = new Coluna
            {
                BoardId = dto.BoardId,
                Cor = dto.Cor,
                Nome = dto.Nome,
                Ordem = dto.Ordem,
            };

            await context.Colunas.AddAsync(c);
            await context.SaveChangesAsync();
            return Ok();
        }


        public class ColunaOrdemDTO
        {
            public int Id { get; set; }
            public int Ordem
            {
                get; set;
            }
        }
        [HttpPost("Ordenar")]
        public async Task<IActionResult> OrdenarColunas([FromBody] List<ColunaOrdemDTO> dto)
        {
            foreach (var item in dto)
            {
                var c = await context.Colunas.FindAsync(item.Id);
                if (c != null)
                {
                    c.Ordem = item.Ordem;
                }
            }
            await context.SaveChangesAsync();
            return Ok();
        }

    }



}
