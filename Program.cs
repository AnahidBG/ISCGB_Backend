using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models; // Ajusta según tu namespace si cambia

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar el DbContext (Asegúrate de cambiar TuDbContext por el nombre real de tu archivo Context)
builder.Services.AddDbContext<TuDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Habilitar controladores y herramientas de documentación
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // <--- Esto genera la documentación para Swagger

var app = builder.Build();

// 3. ACTIVAR SWAGGER VISUAL (Dejalo afuera de cualquier 'if' para que ande siempre local)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoGestión Docente API v1");
});

// app.UseHttpsRedirection();

// 4. Mapear las rutas de tus controladores (como AuthController)
app.MapControllers();

// Dejamos el endpoint de clima por defecto por si querés verificar que funcione
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}