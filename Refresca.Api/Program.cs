using Microsoft.EntityFrameworkCore;
using Refresca.Api.Application.Interfaces;
using Refresca.Api.Infrastructure.Clients;
using Refresca.Api.Infrastructure.Persistence;
using Refresca.Api.Infrastructure.Repositories;
using Refresca.Api.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Refresca")
    ?? "Data Source=refresca.db";

builder.Services.AddDbContext<RefrescaDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRefrescaService, RefrescaService>();

// Serviço de previsão (repositório de SOA). A variável de ambiente tem prioridade sobre o appsettings.
var previsaoUrl = builder.Configuration["REFRESCA_PREVISAO_URL"]
    ?? builder.Configuration["Servicos:PrevisaoUrl"]
    ?? "http://localhost:8000";
builder.Services.AddHttpClient<IPrevisaoClient, PrevisaoClient>(client =>
{
    client.BaseAddress = new Uri(previsaoUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("App", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseMiddleware<Refresca.Api.Infrastructure.Middleware.ExceptionHandlingMiddleware>();
app.UseCors("App");

// Swagger sempre ligado: a documentação dos endpoints faz parte da entrega, e quem clona o
// repositório roda em ambiente Production (o launchSettings.json não vai para o Git).
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RefrescaDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedData.InitializeAsync(db, scope.ServiceProvider.GetRequiredService<TimeProvider>());
}

app.Run();

public partial class Program { }
