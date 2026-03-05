using Microsoft.EntityFrameworkCore;
using TarefasAPI_v2.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

if (string.IsNullOrEmpty(connectionString))
{
    // Isso vai fazer o log do Render gritar exatamente o que está faltando
    throw new Exception("ERRO FATAL: A VARIÁVEL 'DB_CONNECTION_STRING' NÃO FOI ENCONTRADA!");
}

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

// 3. Aplica migrações automaticamente ao iniciar (Ideal para subir no Render)
// 3. Aplica migrações com tratamento de erro
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        Console.WriteLine("Tentando aplicar migrações...");
        db.Database.Migrate();
        Console.WriteLine("Migrações aplicadas com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERRO CRÍTICO AO CONECTAR NO BANCO: {ex.Message}");
        // Opcional: throw; // Descomente se quiser que a API pare de subir caso o banco não conecte
    }
}

// 4. Swagger sempre disponível para testes
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();