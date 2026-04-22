using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PousadaApp.API.DTOs;
using PousadaApp.Domain.Entities;
using PousadaApp.Domain.Enums;
using PousadaApp.Infrastructure.Data;

namespace PousadaApp.API.Controllers;

[Authorize]
public class ReservasController(PousadaDbContext db) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] ReservaStatus? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var empresaId = GetEmpresaId();

        var query = db.Reservas
            .Include(r => r.Hospede)
            .Include(r => r.Quarto)
            .Where(r => r.IdEmpresa == empresaId);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (from.HasValue)
            query = query.Where(r => r.DataCheckin >= from.Value);

        if (to.HasValue)
            query = query.Where(r => r.DataCheckout <= to.Value);

        var reservas = await query
            .OrderByDescending(r => r.DataCheckin)
            .Select(r => new ReservaDto(
                r.Id, r.IdHospede, r.IdQuarto,
                r.DataCheckin, r.DataCheckout,
                r.Status, r.ValorTotal, r.Observacoes,
                new HospedeDto(r.Hospede.Id, r.Hospede.Nome, r.Hospede.Documento, r.Hospede.Telefone, r.Hospede.Email, r.Hospede.Cidade, r.Hospede.Observacoes),
                new QuartoDto(r.Quarto.Id, r.Quarto.Numero, r.Quarto.Tipo, r.Quarto.Capacidade, r.Quarto.PrecoDiaria, r.Quarto.Status, r.Quarto.Andar, r.Quarto.Ativo)
            ))
            .ToListAsync();

        return Ok(reservas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var empresaId = GetEmpresaId();

        var reserva = await db.Reservas
            .Include(r => r.Hospede)
            .Include(r => r.Quarto)
            .Where(r => r.Id == id && r.IdEmpresa == empresaId)
            .Select(r => new ReservaDto(
                r.Id, r.IdHospede, r.IdQuarto,
                r.DataCheckin, r.DataCheckout,
                r.Status, r.ValorTotal, r.Observacoes,
                new HospedeDto(r.Hospede.Id, r.Hospede.Nome, r.Hospede.Documento, r.Hospede.Telefone, r.Hospede.Email, r.Hospede.Cidade, r.Hospede.Observacoes),
                new QuartoDto(r.Quarto.Id, r.Quarto.Numero, r.Quarto.Tipo, r.Quarto.Capacidade, r.Quarto.PrecoDiaria, r.Quarto.Status, r.Quarto.Andar, r.Quarto.Ativo)
            ))
            .FirstOrDefaultAsync();

        if (reserva is null)
            return NotFound();

        return Ok(reserva);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservaDto dto)
    {
        var empresaId = GetEmpresaId();

        var quarto = await db.Quartos.FirstOrDefaultAsync(q => q.Id == dto.IdQuarto && q.IdEmpresa == empresaId);
        if (quarto is null)
            return BadRequest(new { message = "Quarto nao encontrado" });

        var hospede = await db.Hospedes.AnyAsync(h => h.Id == dto.IdHospede && h.IdEmpresa == empresaId);
        if (!hospede)
            return BadRequest(new { message = "Hospede nao encontrado" });

        var nights = (dto.DataCheckout - dto.DataCheckin).Days;
        if (nights <= 0)
            return BadRequest(new { message = "Data de checkout deve ser posterior ao checkin" });

        var reserva = new Reserva
        {
            IdEmpresa = empresaId,
            IdHospede = dto.IdHospede,
            IdQuarto = dto.IdQuarto,
            DataCheckin = dto.DataCheckin,
            DataCheckout = dto.DataCheckout,
            Observacoes = dto.Observacoes,
            ValorTotal = quarto.PrecoDiaria * nights
        };

        db.Reservas.Add(reserva);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = reserva.Id }, new ReservaDto(
            reserva.Id, reserva.IdHospede, reserva.IdQuarto,
            reserva.DataCheckin, reserva.DataCheckout,
            reserva.Status, reserva.ValorTotal, reserva.Observacoes,
            null, null
        ));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReservaDto dto)
    {
        var empresaId = GetEmpresaId();
        var reserva = await db.Reservas.FirstOrDefaultAsync(r => r.Id == id && r.IdEmpresa == empresaId);

        if (reserva is null)
            return NotFound();

        var quarto = await db.Quartos.FirstOrDefaultAsync(q => q.Id == dto.IdQuarto && q.IdEmpresa == empresaId);
        if (quarto is null)
            return BadRequest(new { message = "Quarto nao encontrado" });

        var nights = (dto.DataCheckout - dto.DataCheckin).Days;
        if (nights <= 0)
            return BadRequest(new { message = "Data de checkout deve ser posterior ao checkin" });

        reserva.IdHospede = dto.IdHospede;
        reserva.IdQuarto = dto.IdQuarto;
        reserva.DataCheckin = dto.DataCheckin;
        reserva.DataCheckout = dto.DataCheckout;
        reserva.Observacoes = dto.Observacoes;
        reserva.ValorTotal = quarto.PrecoDiaria * nights;
        reserva.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Ok(new ReservaDto(
            reserva.Id, reserva.IdHospede, reserva.IdQuarto,
            reserva.DataCheckin, reserva.DataCheckout,
            reserva.Status, reserva.ValorTotal, reserva.Observacoes,
            null, null
        ));
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReservaStatusDto dto)
    {
        var empresaId = GetEmpresaId();
        var reserva = await db.Reservas
            .Include(r => r.Quarto)
            .FirstOrDefaultAsync(r => r.Id == id && r.IdEmpresa == empresaId);

        if (reserva is null)
            return NotFound();

        var previousStatus = reserva.Status;
        reserva.Status = dto.Status;
        reserva.UpdatedAt = DateTime.UtcNow;

        switch (dto.Status)
        {
            case ReservaStatus.Checkin:
                reserva.Quarto.Status = QuartoStatus.Ocupado;
                break;
            case ReservaStatus.Checkout:
                reserva.Quarto.Status = QuartoStatus.Limpeza;
                break;
            case ReservaStatus.Cancelada when previousStatus == ReservaStatus.Checkin:
                reserva.Quarto.Status = QuartoStatus.Limpeza;
                break;
        }

        await db.SaveChangesAsync();

        return Ok(new ReservaDto(
            reserva.Id, reserva.IdHospede, reserva.IdQuarto,
            reserva.DataCheckin, reserva.DataCheckout,
            reserva.Status, reserva.ValorTotal, reserva.Observacoes,
            null, null
        ));
    }
}

public record UpdateReservaStatusDto(ReservaStatus Status);
