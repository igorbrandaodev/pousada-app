using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PousadaApp.API.DTOs;
using PousadaApp.Domain.Entities;
using PousadaApp.Domain.Enums;
using PousadaApp.Infrastructure.Data;

namespace PousadaApp.API.Controllers;

[Authorize]
public class QuartosController(PousadaDbContext db) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var empresaId = GetEmpresaId();

        var quartos = await db.Quartos
            .Where(q => q.IdEmpresa == empresaId)
            .Select(q => new QuartoDto(q.Id, q.Numero, q.Tipo, q.Capacidade, q.PrecoDiaria, q.Status, q.Andar, q.Ativo))
            .ToListAsync();

        return Ok(quartos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var empresaId = GetEmpresaId();

        var quarto = await db.Quartos
            .Where(q => q.Id == id && q.IdEmpresa == empresaId)
            .Select(q => new QuartoDto(q.Id, q.Numero, q.Tipo, q.Capacidade, q.PrecoDiaria, q.Status, q.Andar, q.Ativo))
            .FirstOrDefaultAsync();

        if (quarto is null)
            return NotFound();

        return Ok(quarto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuartoDto dto)
    {
        var empresaId = GetEmpresaId();

        var quarto = new Quarto
        {
            IdEmpresa = empresaId,
            Numero = dto.Numero,
            Tipo = dto.Tipo,
            Capacidade = dto.Capacidade,
            PrecoDiaria = dto.PrecoDiaria,
            Andar = dto.Andar
        };

        db.Quartos.Add(quarto);
        await db.SaveChangesAsync();

        var result = new QuartoDto(quarto.Id, quarto.Numero, quarto.Tipo, quarto.Capacidade, quarto.PrecoDiaria, quarto.Status, quarto.Andar, quarto.Ativo);
        return CreatedAtAction(nameof(GetById), new { id = quarto.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateQuartoDto dto)
    {
        var empresaId = GetEmpresaId();
        var quarto = await db.Quartos.FirstOrDefaultAsync(q => q.Id == id && q.IdEmpresa == empresaId);

        if (quarto is null)
            return NotFound();

        quarto.Numero = dto.Numero;
        quarto.Tipo = dto.Tipo;
        quarto.Capacidade = dto.Capacidade;
        quarto.PrecoDiaria = dto.PrecoDiaria;
        quarto.Status = dto.Status;
        quarto.Andar = dto.Andar;
        quarto.Ativo = dto.Ativo;

        await db.SaveChangesAsync();

        var result = new QuartoDto(quarto.Id, quarto.Numero, quarto.Tipo, quarto.Capacidade, quarto.PrecoDiaria, quarto.Status, quarto.Andar, quarto.Ativo);
        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateQuartoStatusDto dto)
    {
        var empresaId = GetEmpresaId();
        var quarto = await db.Quartos.FirstOrDefaultAsync(q => q.Id == id && q.IdEmpresa == empresaId);

        if (quarto is null)
            return NotFound();

        quarto.Status = dto.Status;
        await db.SaveChangesAsync();

        var result = new QuartoDto(quarto.Id, quarto.Numero, quarto.Tipo, quarto.Capacidade, quarto.PrecoDiaria, quarto.Status, quarto.Andar, quarto.Ativo);
        return Ok(result);
    }
}

public record UpdateQuartoStatusDto(QuartoStatus Status);
