using minimaltodo.Data;
using minimaltodo.Models;
using Microsoft.EntityFrameworkCore;
using minimaltodo;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using minimaltodo.Repositories;
using minimaltodo.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SqliteConnectionString") ?? "Data Source=tarefas.db";
var key = Encoding.ASCII.GetBytes(Settings.Secret);

builder.Services.AddAuthentication(x => 
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x => 
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("admin"));
    options.AddPolicy("Employee", policy => policy.RequireRole("employee"));
});

builder.Services.AddSqlite<AppDbContext>(connectionString);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "miniAPI Todo",
                        Version = "v1",
                        Description = "Minimal API para gerenciar Tarefas"
                    });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = @"JWT Authorization",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                          {
                              Reference = new OpenApiReference
                              {
                                  Type = ReferenceType.SecurityScheme,
                                  Id = "Bearer"
                              }
                          },
                         new string[] {}
                    }
                });
            });



var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

await AppDbContext.VerificaDBExiste(app.Services, app.Logger, connectionString);

app.MapPost("/tarefas", async (CriarTarefaViewModel model, AppDbContext db) =>
{
    var tarefa = model.MapTo();
    if (!model.IsValid)
    {
        return Results.BadRequest(model.Notifications);
    };

    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}", tarefa);
}
)
    .RequireAuthorization()
    .WithName("CriarTarefa")
    .WithTags("Tarefas")
    .Produces<Flunt.Notifications.Notification>(StatusCodes.Status400BadRequest)
    .Produces<Tarefa>(StatusCodes.Status201Created);

app.MapGet("/tarefas", async (AppDbContext db) =>
    await db.Tarefas.ToListAsync())
    .RequireAuthorization()
    .WithTags("Tarefas")
    .WithName("GetTarefas")
    .Produces<List<Tarefa>>(StatusCodes.Status200OK);


app.MapGet("/tarefas/{id}", async (Guid id, AppDbContext db) =>
    await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound())
    .RequireAuthorization()
    .WithName("GetTarefaById")
    .WithTags("Tarefas")
    .Produces<Tarefa>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app.MapPut("/tarefas/{id}", async (Guid id, Tarefa inputTarefa, AppDbContext db) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);
    if (tarefa is null) 
        return Results.NotFound();

    tarefa.Titulo = inputTarefa.Titulo;
    tarefa.Descricao = inputTarefa.Descricao;
    tarefa.Feito = inputTarefa.Feito;

    await db.SaveChangesAsync();

    return Results.NoContent();
})
    .RequireAuthorization()
    .WithName("UpdateTarefa")
    .WithTags("Tarefas")
    .ProducesValidationProblem()
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);


app.MapDelete("/tarefas/{id}", async (Guid id, AppDbContext db) =>
{
    if (await db.Tarefas.FindAsync(id) is Tarefa tarefa)
    {
        db.Tarefas.Remove(tarefa);
        await db.SaveChangesAsync();
        return Results.Ok(tarefa);
    }
    return Results.NotFound();
})
    .RequireAuthorization()
    .WithName("DeleteTarefa")
    .WithTags("Tarefas")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

app.MapDelete("/tarefas/delete-tarefas", async (AppDbContext db) =>
    Results.Ok(await db.Database.ExecuteSqlRawAsync("DELETE FROM Tarefas")))
    .RequireAuthorization()
    .WithName("DeleteTarefas")
    .WithTags("Tarefas")
    .Produces<int>(StatusCodes.Status200OK);

app.MapPost("/login", (UsuarioLogin model) =>
{
    var user = UsuarioRepository.Get(model.Username, model.Password);

    if (user == null)
        return Results.NotFound(new { message = "Usuário ou senha inválido" });

    var token = TokenService.GenerateToken(user);

    user.Password = "";

    return Results.Ok(new
    {
        user = user,
        token = token
    });
})
    .WithTags("Usuarios");

app.MapSwagger();
app.UseSwaggerUI();

app.Run();
