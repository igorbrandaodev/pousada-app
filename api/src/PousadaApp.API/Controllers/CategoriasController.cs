using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PousadaApp.API.DTOs;
using PousadaApp.Domain.Entities;
using PousadaApp.Infrastructure.Data;

namespace PousadaApp.API.Controllers;

[Authorize]
public class CategoriasController(PousadaDbContext db) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var empresaId = GetEmpresaId();

        var categorias = await db.CategoriasCardapio
            .Where(c => c.IdEmpresa == empresaId)
            .Select(c => new CategoriaDto(c.Id, c.Nome, c.Icone))
            .ToListAsync();

        return Ok(categorias);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoriaDto dto)
    {
        var empresaId = GetEmpresaId();

        var categoria = new CategoriaCardapio
        {
            IdEmpresa = empresaId,
            Nome = dto.Nome,
            Icone = dto.Icone
        };

        db.CategoriasCardapio.Add(categoria);
        await db.SaveChangesAsync();

        return Created("", new CategoriaDto(categoria.Id, categoria.Nome, categoria.Icone));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCategoriaDto dto)
    {
        var empresaId = GetEmpresaId();
        var categoria = await db.CategoriasCardapio.FirstOrDefaultAsync(c => c.Id == id && c.IdEmpresa == empresaId);

        if (categoria is null)
            return NotFound();

        categoria.Nome = dto.Nome;
        categoria.Icone = dto.Icone;

        await db.SaveChangesAsync();

        return Ok(new CategoriaDto(categoria.Id, categoria.Nome, categoria.Icone));
    }
}
