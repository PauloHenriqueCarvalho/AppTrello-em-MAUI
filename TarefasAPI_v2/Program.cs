using Microsoft.EntityFrameworkCore;
using TarefasAPI_v2.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "Server=tarefas_db.mssql.somee.com;Database=tarefas_db;User Id=paulohenrique7_SQLLogin_1;Password=33uh5jyazi;TrustServerCertificate=True;Encrypt=False;Connect Timeout=60;";

Console.WriteLine($"DEBUG: Usando string forçada: {connectionString}");

builder.Services.AddDbContext<AppDbContext>(o =>
{
    o.UseSqlServer(connectionString);
});
// 2. Unificado a configuração de Controllers e JSON
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Isso aplica automaticamente as migrações pendentes no banco de dados
    dbContext.Database.Migrate();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        Console.WriteLine("DEBUG: Iniciando verificação de banco...");

        // Verifica se o banco consegue ser aberto
        var canConnect = await db.Database.CanConnectAsync();
        Console.WriteLine($"DEBUG: Conexão bem sucedida: {canConnect}");

        // Força a aplicação das migrações
        Console.WriteLine("DEBUG: Tentando rodar Migrate()...");
        db.Database.Migrate();
        Console.WriteLine("DEBUG: Migrações aplicadas com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERRO CRÍTICO NO BANCO: {ex.Message}");
        Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
    }
}

// 4. Swagger sempre disponível para testes
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();