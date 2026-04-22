using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PousadaApp.API.DTOs;
using PousadaApp.Domain.Enums;
using PousadaApp.Infrastructure.Data;

namespace PousadaApp.API.Controllers;

[Authorize]
public class FinanceiroController(PousadaDbContext db) : BaseController
{
    [HttpGet("resumo")]
    public async Task<IActionResult> GetResumo()
    {
        var empresaId = GetEmpresaId();

        var totalHospedagem = await db.Reservas
            .Where(r => r.IdEmpresa == empresaId && r.Status == ReservaStatus.Checkout)
            .SumAsync(r => r.ValorTotal);

        var totalRestaurante = await db.Comandas
            .Where(c => c.IdEmpresa == empresaId && c.Status == ComandaStatus.Fechada)
            .SumAsync(c => c.Total);

        return Ok(new ResumoFinanceiroDto(totalHospedagem, totalRestaurante, totalHospedagem + totalRestaurante));
    }

    [HttpGet("transacoes")]
    public async Task<IActionResult> GetTransacoes()
    {
        var empresaId = GetEmpresaId();

        var reservaTransacoes = await db.Reservas
            .Include(r => r.Hospede)
            .Include(r => r.Quarto)
            .Where(r => r.IdEmpresa == empresaId && r.Status == ReservaStatus.Checkout)
            .Select(r => new TransacaoDto(
                r.DataCheckout,
                $"Hospedagem - {r.Hospede.Nome} (Quarto {r.Quarto.Numero})",
                "Hospedagem",
                r.ValorTotal
            ))
            .ToListAsync();

        var comandaTransacoes = await db.Comandas
            .Include(c => c.Reserva).ThenInclude(r => r.Hospede)
            .Where(c => c.IdEmpresa == empresaId && c.Status == ComandaStatus.Fechada)
            .Select(c => new TransacaoDto(
                c.DataFechamento!.Value,
                $"Comanda - {c.Reserva.Hospede.Nome}",
                "Restaurante",
                c.Total
            ))
            .ToListAsync();

        var transacoes = reservaTransacoes
            .Concat(comandaTransacoes)
            .OrderByDescending(t => t.Data)
            .ToList();

        return Ok(transacoes);
    }
}
