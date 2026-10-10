using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutoGestionAPI.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<TuDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
//Esto configura que ASP.NET valide JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        IConfigurationSection jwtSettings =
            builder.Configuration.GetSection("Jwt");

        string? key = jwtSettings["Key"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(key!)),

            ValidateIssuer = false,
            ValidateAudience = false,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Ingresá el token JWT."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirAngular", policy =>
    {
        // Cambiar por el puerto que corresponda desde angular.
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IDocumentacionService, DocumentacionService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<AutoGestionAPI.Workers.NotificadorFaltantesWorker>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoGestión Docente API v1");
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Pedimos la conexión a la base de datos
        var context = services.GetRequiredService<TuDbContext>();

        // Ejecutamos nuestra siembra
        AutoGestionAPI.Data.DbSeeder.Inicializar(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error al insertar las provincias: " + ex.Message);
    }
}

// app.UseHttpsRedirection();


// Ejecutar dotnet ef database update


app.UseCors("PermitirAngular");

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- AUTO-MIGRACIÓN PARA EL EQUIPO ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TuDbContext>();
    try
    {
        context.Database.Migrate(); // Lee la carpeta Migrations y actualiza SQL Server
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error actualizando la base de datos: {ex.Message}");
    }
}

app.Run();

app.Run();