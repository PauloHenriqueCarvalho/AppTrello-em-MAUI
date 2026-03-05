using Microsoft.EntityFrameworkCore;
using TarefasAPI_v2.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração de Banco: Prioriza Variável de Ambiente (Render), se não acha no appsettings.json
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// 4. Swagger sempre disponível para testes
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();