using Estoque.API.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Linq; 
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
// 🔑 USINGS NECESSÁRIOS PARA JWT E SEGURANÇA
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;


var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao contêiner.
builder.Services.AddControllers();

// ** BLOCO REMOVIDO: Configuração do Swagger/OpenAPI e AddSwaggerGen **
builder.Services.AddEndpointsApiExplorer(); // Mantido para futuras reativações do Swagger


// Configuração do DbContext para o Entity Framework Core.
builder.Services.AddDbContext<EstoqueContext>(options =>
     options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// 2. CONFIGURAÇÃO DA VALIDAÇÃO JWT (FORÇANDO O ESQUEMA)
string jwtKey = builder.Configuration["Jwt:Key"] ?? 
                throw new InvalidOperationException("Jwt:Key não encontrado na configuração.");
                
Console.WriteLine($"DEBUG: Chave JWT lida: {jwtKey.Substring(0, 5)}...");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => 
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)) 
    };
});


var app = builder.Build();

// Configura o pipeline de requisições HTTP.
if (app.Environment.IsDevelopment())
{
    // ** LINHAS REMOVIDAS: app.UseSwagger() e app.UseSwaggerUI() **
}

app.UseHttpsRedirection();

// ***************************************************************
// 3. PIPELINE DE SEGURANÇA (Ordem Correta e Essencial)
// ***************************************************************
Console.WriteLine("DEBUG: 1. Chamando UseRouting");
app.UseRouting();

Console.WriteLine("DEBUG: 2. Chamando UseAuthentication");
app.UseAuthentication();

Console.WriteLine("DEBUG: 3. Chamando UseAuthorization");
app.UseAuthorization();


// Mapeamento dos Controllers (DEVE VIR DEPOIS da segurança)
Console.WriteLine("DEBUG: 4. Chamando MapControllers");
app.MapControllers(); 


// Aplica migrações na inicialização.
using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
    dataContext.Database.Migrate(); 
}

app.Run();