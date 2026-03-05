using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TarefasAPI.DTO.User;
using TarefasAPI_v2.Models;

namespace TarefasAPI_v2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        protected readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }



        public class CreateUserDTO
        {
            public string Nome { get; set; }
            public string Login { get; set; }
            public string Senha { get; set; }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var u = await _context.Usuarios.FirstOrDefaultAsync(x => x.Login == dto.Login);
            Console.WriteLine(u == null ? "Usuário não encontrado" : "Usuário encontrado");
            if (u == null)
                return Unauthorized("Esse usuario não existe!");

            bool senha = BCrypt.Net.BCrypt.Verify(dto.Senha, u.SenhaHash);
            if (!senha)
                return Unauthorized("Credenciais invalidas.");

            return Ok(new UserResponseDTO
            {
                Login = u.Login,
                Id = u.Id,
                Nome = u.Nome,
            });

        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Login) || string.IsNullOrWhiteSpace(dto.Senha))
                return BadRequest("Dados inválidos.");

            var existe = await _context.Usuarios.AnyAsync(x => x.Login == dto.Login);
            if (existe)
                return Conflict("Login já existente.");

            var usuario = new Usuario
            {
                Nome = dto.Nome,
                Login = dto.Login,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsers), new { id = usuario.Id }, new UserResponseDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Login = usuario.Login
            });
        }
        public class UsersResponseDTO
        {
            public string Nome { get; set; }
            public int Id { get; set; }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUsuario([FromBody] CreateUserDTO dto, int id)
        {
            var u = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
            if (u == null)
                return BadRequest("Usuario não encontrado!");
            u.Login = dto.Login;
            u.Nome = dto.Nome;
            u.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
            await _context.SaveChangesAsync();
            return Ok(u);
        }


        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var usuario = await _context.Usuarios.Select(u => new UsersResponseDTO
            {
                Id = u.Id,
                Nome = u.Nome
            }).ToListAsync();

            return Ok(usuario);
        }

    }
}
