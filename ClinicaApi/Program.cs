using Microsoft.EntityFrameworkCore;
using ClinicaApi.Data;
using ClinicaApi.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// EF Core: conectar el DbContext a la base de datos ClinicaBootcamp
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSQL")));

// Dapper: repositorio de lecturas (CQRS Lite)
builder.Services.AddScoped<IClinicaLecturaRepository, ClinicaLecturaRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Al iniciar, comprobar que la API llega a la base de datos
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (db.Database.CanConnect())
        Console.WriteLine("Conexión a ClinicaBootcamp: OK");
    else
        Console.WriteLine("No se pudo conectar. Revisa ConexionSQL en appsettings.json");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
