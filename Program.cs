using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using AutoGestionAPI.Services;
using Microsoft.OpenApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<TuDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// Clave JWT. En desarrollo viene de appsettings.Development.json (clave de
// prueba, así alcanza con "dotnet run"). En producción se carga con la
// variable de entorno Jwt__Key y NO puede ser la clave de desarrollo.
string? jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Falta configurar 'Jwt:Key' (mínimo 32 caracteres). " +
        "En producción: variable de entorno Jwt__Key. Ver README.");
}
if (!builder.Environment.IsDevelopment() && jwtKey.StartsWith("CLAVE-SOLO-PARA-DESARROLLO"))
{
    throw new InvalidOperationException(
        "Se está usando la clave JWT de desarrollo fuera de desarrollo. " +
        "Configurá una clave propia con la variable de entorno Jwt__Key.");
}

//Esto configura que ASP.NET valide JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),

            ValidateIssuer = false,
            ValidateAudience = false,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Por defecto TODOS los endpoints requieren estar logueado. Los públicos
// (login, recupero de contraseña, etc.) se marcan con [AllowAnonymous].
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Limita los intentos contra login / recupero de contraseña para frenar
// ataques de fuerza bruta: 10 pedidos por minuto por IP.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddEndpointsApiExplorer();

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

// El origen del frontend se configura en appsettings ("Frontend:Url").
string urlFrontend = builder.Configuration["Frontend:Url"] ?? "http://localhost:4200";

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirAngular", policy =>
    {
        policy.WithOrigins(urlFrontend)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IDocumentacionService, DocumentacionService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<AutoGestionAPI.Workers.NotificadorFaltantesWorker>();

WebApplication app = builder.Build();

// Swagger expone todo el mapa de la API: solo en desarrollo.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoGestión Docente API v1");
    });
}

// --- AUTO-MIGRACIÓN Y CARGA INICIAL ---
// Primero se aplican las migraciones y recién después se siembran los datos:
// en una base nueva el seeder necesita que las tablas ya existan.
using (IServiceScope scope = app.Services.CreateScope())
{
    TuDbContext context = scope.ServiceProvider.GetRequiredService<TuDbContext>();
    ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        context.Database.Migrate(); // Lee la carpeta Migrations y actualiza SQL Server
        await AutoGestionAPI.Data.DbSeeder.Inicializar(context);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al migrar o inicializar la base de datos.");
        throw;
    }
}

// app.UseHttpsRedirection();

app.UseCors("PermitirAngular");

app.UseStaticFiles();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
