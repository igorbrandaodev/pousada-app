using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PousadaApp.API.DTOs;
using PousadaApp.Domain.Entities;
using PousadaApp.Infrastructure.Data;

namespace PousadaApp.API.Controllers;

[Authorize]
public class HospedesController(PousadaDbContext db) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var empresaId = GetEmpresaId();

        var hospedes = await db.Hospedes
            .Where(h => h.IdEmpresa == empresaId)
            .Select(h => new HospedeDto(h.Id, h.Nome, h.Documento, h.Telefone, h.Email, h.Cidade, h.Observacoes))
            .ToListAsync();

        return Ok(hospedes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var empresaId = GetEmpresaId();

        var hospede = await db.Hospedes
            .Where(h => h.Id == id && h.IdEmpresa == empresaId)
            .Select(h => new HospedeDto(h.Id, h.Nome, h.Documento, h.Telefone, h.Email, h.Cidade, h.Observacoes))
            .FirstOrDefaultAsync();

        if (hospede is null)
            return NotFound();

        return Ok(hospede);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHospedeDto dto)
    {
        var empresaId = GetEmpresaId();

        var hospede = new Hospede
        {
            IdEmpresa = empresaId,
            Nome = dto.Nome,
            Documento = dto.Documento,
            Telefone = dto.Telefone,
            Email = dto.Email,
            Cidade = dto.Cidade,
            Observacoes = dto.Observacoes
        };

        db.Hospedes.Add(hospede);
        await db.SaveChangesAsync();

        var result = new HospedeDto(hospede.Id, hospede.Nome, hospede.Documento, hospede.Telefone, hospede.Email, hospede.Cidade, hospede.Observacoes);
        return CreatedAtAction(nameof(GetById), new { id = hospede.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateHospedeDto dto)
    {
        var empresaId = GetEmpresaId();
        var hospede = await db.Hospedes.FirstOrDefaultAsync(h => h.Id == id && h.IdEmpresa == empresaId);

        if (hospede is null)
            return NotFound();

        hospede.Nome = dto.Nome;
        hospede.Documento = dto.Documento;
        hospede.Telefone = dto.Telefone;
        hospede.Email = dto.Email;
        hospede.Cidade = dto.Cidade;
        hospede.Observacoes = dto.Observacoes;
        hospede.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var result = new HospedeDto(hospede.Id, hospede.Nome, hospede.Documento, hospede.Telefone, hospede.Email, hospede.Cidade, hospede.Observacoes);
        return Ok(result);
    }
}
