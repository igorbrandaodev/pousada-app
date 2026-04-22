using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PousadaApp.API.Auth;
using PousadaApp.API.DTOs;
using PousadaApp.Domain.Entities;
using PousadaApp.Domain.Enums;
using PousadaApp.Infrastructure.Data;

namespace PousadaApp.API.Controllers;

[AllowAnonymous]
public class AuthController(PousadaDbContext db, JwtService jwtService) : BaseController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Ativo);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            return Unauthorized(new { message = "Email ou senha invalidos" });

        var token = jwtService.GenerateToken(usuario);

        return Ok(new LoginResponse(
            token,
            usuario.Nome,
            usuario.Email,
            usuario.Role.ToString(),
            usuario.IdEmpresa
        ));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var slugExists = await db.Empresas.AnyAsync(e => e.UrlSlug == request.UrlSlug);
        if (slugExists)
            return BadRequest(new { message = "URL slug ja esta em uso" });

        var emailExists = await db.Usuarios.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
            return BadRequest(new { message = "Email ja esta em uso" });

        var empresa = new Empresa
        {
            Nome = request.NomeEmpresa,
            UrlSlug = request.UrlSlug
        };

        db.Empresas.Add(empresa);
        await db.SaveChangesAsync();

        var usuario = new Usuario
        {
            IdEmpresa = empresa.Id,
            Email = request.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            Nome = request.Nome,
            Role = UsuarioRole.Admin
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        var token = jwtService.GenerateToken(usuario);

        return Created("", new LoginResponse(
            token,
            usuario.Nome,
            usuario.Email,
            usuario.Role.ToString(),
            usuario.IdEmpresa
        ));
    }
}
