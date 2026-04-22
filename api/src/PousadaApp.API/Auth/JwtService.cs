using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PousadaApp.Domain.Entities;

namespace PousadaApp.API.Auth;

public class JwtService
{
    private readonly string _key;
    private readonly string _issuer;

    public JwtService(IConfiguration configuration)
    {
        _key = configuration["Jwt:Key"]!;
        _issuer = configuration["Jwt:Issuer"]!;
    }

    public string GenerateToken(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim("userId", usuario.Id.ToString()),
            new Claim("empresaId", usuario.IdEmpresa.ToString()),
            new Claim(ClaimTypes.Role, usuario.Role.ToString()),
            new Claim("nome", usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
