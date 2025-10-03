using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;

[Route("api/[controller]")] // Rota: /api/Auth
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    // Injeção de Dependência da IConfiguration para ler o appsettings.json
    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Simula o processo de login e gera um JWT válido para autenticação.
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login(string username = "admin_user")
    {
        // 1. Chave e Credenciais (Usando a chave secreta do appsettings)
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // 2. Claims (Payload) - Informações de identidade e permissões
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Admin") // Adiciona a permissão de Admin
        };

        // 3. Criação do Objeto Token
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(60), // Expira em 60 minutos
            signingCredentials: credentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        
        // 4. Retorna o Token para o Cliente
        return Ok(new { 
            token = tokenHandler.WriteToken(token),
            expires = token.ValidTo
        });
    }
}