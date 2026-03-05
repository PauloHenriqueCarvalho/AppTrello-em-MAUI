using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TarefasAPI_v2.Models;
using TarefasMaui.Helpers;
using TarefasMaui.Models;

namespace TarefasMaui.Services
{
    public class PadraoService
    {
        private HttpClient c;

        public PadraoService()
        {
            c = new HttpClient();
            c.BaseAddress = new Uri("https://apptrello-em-maui.onrender.com/api/");
            c.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }
        public class LoginDTO
        {
            public string Login { get; set; }
            public string Senha { get; set; }

        }
        public class CreateUserDTO
        {
            public string Login { get; set; }
            public string Senha { get; set; }
            public string Nome { get; set; }

        }
        public class CreateBoardDTO
        {
            public string nome { get; set; }
            public int IdCriador { get; set; }
        }
        public async Task<CreateBoardDTO> CreateBoard(CreateBoardDTO dto)
        {
            var u = JsonConvert.SerializeObject(dto);
            var con = new StringContent(u, Encoding.UTF8, "application/json");
            var res = await c.PostAsync("Board", con);
            if (!res.IsSuccessStatusCode)
                return null;

            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CreateBoardDTO>(json);
        }

        public async Task CreateStatus(Coluna dto)
        {
            var u = JsonConvert.SerializeObject(dto);
            var con = new StringContent(u, Encoding.UTF8, "application/json");
            var res = await c.PostAsync("Coluna", con);


        }
        public async Task TransferirTarefas(int idOrigem, int idDestino)
        {
            await c.PutAsync($"Tarefas/transferir-tarefas/{idOrigem}/{idDestino}", null);


        }

        public async Task<bool> AtualizarOrdensColunas(List<StatusColuna> dto)
        {

            var dados = dto.Select(c => new
            {
                Id = c.Coluna.Id,
                Ordem = c.Coluna.Ordem,
            }).ToList();

            var json = JsonConvert.SerializeObject(dados);
            var con = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await c.PostAsync("Coluna/Ordenar", con);
            return res.IsSuccessStatusCode;


        }


        public class AddUserBoardDTO
        {
            public int IdUsuario { get; set; }
            public int idBoard { get; set; }
        }
        public async Task AddUserBoard(AddUserBoardDTO dto)
        {
            var u = JsonConvert.SerializeObject(dto);
            var con = new StringContent(u, Encoding.UTF8, "application/json");
            var res = await c.PostAsync($"Board/user", con);

        }
        public async Task TrocarLider(int idBoard, int idUsuario)
        {
            await c.PutAsync($"Board/trocar-lider/{idBoard}/{idUsuario}", null);

        }

        public async Task RemoveUserBoard(AddUserBoardDTO dto)
        {

            var res = await c.DeleteAsync($"Board/{dto.idBoard}/user/{dto.IdUsuario}");
            Console.WriteLine(res.RequestMessage);
        }
        public async Task<bool> UpdateName(int id, string nome)
        {
            var u = JsonConvert.SerializeObject(new { Id = id, Name = nome });
            var con = new StringContent(u, Encoding.UTF8, "application/json");
            var res = await c.PutAsync($"Board", con);
            return res.IsSuccessStatusCode;
        }
        public async Task<bool> ExcluirtBoard(int id)
        {
            var res = await c.DeleteAsync($"Board/{id}");
            return res.IsSuccessStatusCode;

        }
        public class UserUpdateStatusDTO
        {
            public int IdTarefa { get; set; }
            public int idColuna { get; set; }

        }
        public async Task<Tarefa> AtualizarStatusTarefa(int idTarefa, int statusId)
        {
            try
            {
                var u = JsonConvert.SerializeObject(new UserUpdateStatusDTO { idColuna = statusId, IdTarefa = idTarefa });
                var con = new StringContent(u, Encoding.UTF8, "application/json");

                var res = await c.PutAsync("Coluna/status", con);

                if (!res.IsSuccessStatusCode)
                {
                    var erro = await res.Content.ReadAsStringAsync();
                    Console.WriteLine($"Erro na API: {res.StatusCode} - {erro}");
                    return null;
                }

                var json = await res.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Tarefa>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção crítica: {ex.Message}");
                return null;
            }
        }



        public async Task<Usuarios> Login(LoginDTO dto)
        {
            var u = JsonConvert.SerializeObject(dto);
            var con = new StringContent(u, Encoding.UTF8, "application/json");
            try
            {
                var res = await c.PostAsync("Usuario/Login", con);
                System.Diagnostics.Debug.WriteLine(res.StatusCode);
                if (!res.IsSuccessStatusCode)
                    return null;

                var json = await res.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Usuarios>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }

        }

        public async Task<Usuarios> CadastrarUsuario(CreateUserDTO dto)
        {
            var u = JsonConvert.SerializeObject(dto);
            var con = new StringContent(u, Encoding.UTF8, "application/json");
            var res = await c.PostAsync("Usuario", con);
            if (!res.IsSuccessStatusCode)
                return null;
            else if (res.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return null;
            }
            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Usuarios>(json);
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


        public async Task<CreateTarefaDTO> CadastrarTarefas(CreateTarefaDTO dto)
        {
            var u = JsonConvert.SerializeObject(dto);
            var con = new StringContent(u, Encoding.UTF8, "application/json");
            var res = await c.PostAsync("Tarefas", con);
            if (!res.IsSuccessStatusCode)
            {
                var erroApi = await res.Content.ReadAsStringAsync();
                Debug.WriteLine($"Erro na API: {res.StatusCode} - {erroApi}");

                return null;
            }
            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CreateTarefaDTO>(json);
        }

        public async Task<bool> AlterarTarefa(CreateTarefaDTO dto, int id)
        {
            var u = JsonConvert.SerializeObject(dto);
            var con = new StringContent(u, Encoding.UTF8, "application/json");
            var res = await c.PutAsync($"Tarefas/{id}", con);
            Console.WriteLine(res.RequestMessage);
            return res.IsSuccessStatusCode;


        }
        public async Task<List<Board>> GetBoardsUsuario()
        {
            var res = await c.GetAsync($"Board/User/{Global.user.Id}");
            if (!res.IsSuccessStatusCode)
                return null;

            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Board>>(json);
        }


        public async Task<List<Tarefa>> GetTarefas()
        {
            var res = await c.GetAsync($"Tarefas/board/{Global.board.Id}");
            if (!res.IsSuccessStatusCode)
                return null;

            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Tarefa>>(json);
        }
        public async Task<List<Tarefa>> GetTarefasAtribuidas()
        {
            var res = await c.GetAsync($"Tarefas/Enviadas/{Global.user.Id}");
            if (!res.IsSuccessStatusCode)
                return null;

            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Tarefa>>(json);
        }

        public async Task<Tarefa> GetTarefasUma(int id)
        {
            var res = await c.GetAsync($"Tarefas/{id}");
            if (!res.IsSuccessStatusCode)
                return null;

            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Tarefa>(json);
        }


        public async Task<List<Coluna>> GetColunas()
        {
            var res = await c.GetAsync($"Coluna/board/{Global.board.Id}");
            if (!res.IsSuccessStatusCode)
                return null;

            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Coluna>>(json);
        }
        public async Task DeletarTarefa(int id)
        {
            var res = await c.DeleteAsync($"Tarefas/{id}");
            if (!res.IsSuccessStatusCode)
            {
                throw new Exception("Erro ao excluir tarefa.");
            }
        }
        public async Task<bool> DeletarColuna(int id)
        {
            var res = await c.DeleteAsync($"coluna/{id}");
            if (!res.IsSuccessStatusCode)
            {
                throw new Exception("Erro ao excluir coluna.");
            }
            return res.IsSuccessStatusCode;
        }
        public async Task DeletarStatus(int id)
        {
            var res = await c.DeleteAsync($"Status/{id}");
            if (!res.IsSuccessStatusCode)
            {
                throw new Exception("Existe tarefas com esse status!");
            }
        }
        public async Task ArquivarTarefa(int id)
        {
            var res = await c.PutAsync($"Tarefas/arquivar/{id}", null);

        }
        public async Task<List<Usuarios>> GetUsuarios()
        {
            var res = await c.GetAsync($"Usuario");
            if (!res.IsSuccessStatusCode)
                return null;

            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Usuarios>>(json);
        }

    }
}
