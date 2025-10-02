using Estoque.API.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Linq; 
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao contêiner.
builder.Services.AddControllers();

// Configuração do Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração do DbContext para o Entity Framework Core.
builder.Services.AddDbContext<EstoqueContext>(options =>
     options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configura o pipeline de requisições HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Aplica migrações na inicialização para garantir que o DB exista.
using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
    dataContext.Database.Migrate(); 
}

// Habilita o roteamento para os Controllers (seus endpoints RESTful).
app.MapControllers();

app.Run();