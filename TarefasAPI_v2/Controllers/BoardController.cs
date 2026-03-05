using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TarefasAPI_v2.Models;

namespace TarefasAPI_v2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardController : ControllerBase
    {
        protected readonly AppDbContext context;

        public BoardController(AppDbContext context)
        {
            this.context = context;
        }

        [HttpPost("User")]
        public async Task<IActionResult> AddUserBoard([FromBody] AddUserBoardDTO dto)
        {
            var b = await context.Boards.FirstOrDefaultAsync(x => x.Id == dto.idBoard);
            if (b == null) return NotFound("Esse board não existe");
            var u = await context.Usuarios.FirstOrDefaultAsync(x => x.Id == dto.IdUsuario);
            if (u == null) return NotFound("Esse usuario não existe");

            var board = await context.Boards.Include(x => x.Usuarios).FirstOrDefaultAsync(x => x.Id == dto.idBoard);
            if (board.Usuarios.Any(x => x.Id == dto.IdUsuario)) return BadRequest("Usuario ja esta no board!");
            board.Usuarios.Add(u);
            await context.SaveChangesAsync();
            return Ok("Adicionado com sucesso!");

        }

        [HttpDelete("{idBoard}/User/{idUsuario}")]
        public async Task<IActionResult> RemoveUserBoard(int idBoard, int idUsuario)
        {
            var board = await context.Boards
                .Include(x => x.Usuarios)
                .FirstOrDefaultAsync(x => x.Id == idBoard);

            if (board == null) return NotFound("Esse board não existe");

            var usuario = board.Usuarios.FirstOrDefault(x => x.Id == idUsuario);

            if (usuario == null)
            {
                return BadRequest("O usuário não faz parte deste board ou não existe.");
            }


            var tarefasDoUsuario = await context.Tarefas
              .Where(t => t.BoardId == idBoard && t.Usuarios.First().Id == idUsuario)
              .ToListAsync();

            foreach (var item in tarefasDoUsuario)
            {
                item.Usuarios.Clear();
            }

            board.Usuarios.Remove(usuario);

            await context.SaveChangesAsync();
            return Ok("Usuário removido e tarefas liberadas.");
        }


        [HttpPut("trocar-lider/{idBoard}/{idUsuario}")]
        public async Task<IActionResult> TrocarLider(int idBoard, int idUsuario)
        {
            var board = await context.Boards.FirstOrDefaultAsync(x => x.Id == idBoard);
            if (board == null) return NotFound("Board não encontrado!");

            board.UsuarioCriadorId = idUsuario;
            await context.SaveChangesAsync();
            return NoContent();

        }

        [HttpGet("User/{id:int}")]
        public async Task<IActionResult> GetBoardsUser(int id)
        {
            var board = await context.Boards
                .Where(x => x.UsuarioCriadorId == id || x.Usuarios.Any(c => c.Id == id))
                .Select(a => new
                {
                    a.Id,
                    a.Nome,
                    a.DataCadastro,
                    UsuarioCriador = new
                    {
                        a.UsuarioCriador.Id,
                        a.UsuarioCriador.Nome
                    },
                    Usuarios = a.Usuarios.Select(u => new
                    {
                        u.Id,
                        u.Nome
                    })
                })
                .ToListAsync();
            if (!board.Any()) return NotFound("Nenhum board encontrado!");

            return Ok(board);
        }

        public class CreateBoardDTO
        {
            public string nome { get; set; }
            public int IdCriador { get; set; }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarBoard(int id)
        {
            var b = await context.Boards.FindAsync(id);
            if (b == null) return NotFound("Quadro não encontrado");

            context.Boards.Remove(b);
            await context.SaveChangesAsync();
            return NoContent();
        }

        public class UpdateNameDTO
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        [HttpPut]
        public async Task<IActionResult> AlterarNome(UpdateNameDTO dto)
        {
            var b = await context.Boards.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (b == null) return NotFound("Board não encontrado");
            b.Nome = dto.Name;

            await context.SaveChangesAsync();
            return Ok();


        }

        [HttpPost]
        public async Task<IActionResult> CriarBoard([FromBody] CreateBoardDTO dto)
        {

            var user = await context.Boards.FirstOrDefaultAsync(x => x.Nome == dto.nome && dto.IdCriador == x.UsuarioCriadorId);
            if (user != null) return Conflict("Ja existe um board com esse nome para esse usuario!");
            var b = new Board
            {
                Nome = dto.nome,
                UsuarioCriadorId = dto.IdCriador,
                DataCadastro = DateTime.Now
            };


            await context.Boards.AddAsync(b);
            await context.SaveChangesAsync();
            return Ok(dto);
        }

        public class BoardDTO
        {
            public string Nome { get; set; }
            public int Id { get; set; }
            public int QtdTarefas { get; set; }
        }

        //[HttpGet]
        //public async Task<IActionResult> Get()
        //{

        //}
        public class AddUserBoardDTO
        {
            public int IdUsuario { get; set; }
            public int idBoard { get; set; }
        }





    }
}
