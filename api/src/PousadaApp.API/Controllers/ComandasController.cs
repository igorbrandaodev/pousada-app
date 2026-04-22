using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PousadaApp.API.DTOs;
using PousadaApp.Domain.Entities;
using PousadaApp.Domain.Enums;
using PousadaApp.Infrastructure.Data;

namespace PousadaApp.API.Controllers;

[Authorize]
public class ComandasController(PousadaDbContext db) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ComandaStatus? status)
    {
        var empresaId = GetEmpresaId();

        var query = db.Comandas
            .Include(c => c.Itens).ThenInclude(i => i.Produto)
            .Where(c => c.IdEmpresa == empresaId);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        var comandas = await query
            .OrderByDescending(c => c.DataAbertura)
            .Select(c => new ComandaDto(
                c.Id, c.IdReserva, c.Status, c.DataAbertura, c.DataFechamento, c.Total,
                c.Itens.Select(i => new ItemComandaDto(
                    i.Id, i.IdProduto, i.Quantidade, i.PrecoUnitario, i.Status, i.Observacao, i.Produto.Nome
                )).ToList()
            ))
            .ToListAsync();

        return Ok(comandas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var empresaId = GetEmpresaId();

        var comanda = await db.Comandas
            .Include(c => c.Itens).ThenInclude(i => i.Produto)
            .Where(c => c.Id == id && c.IdEmpresa == empresaId)
            .Select(c => new ComandaDto(
                c.Id, c.IdReserva, c.Status, c.DataAbertura, c.DataFechamento, c.Total,
                c.Itens.Select(i => new ItemComandaDto(
                    i.Id, i.IdProduto, i.Quantidade, i.PrecoUnitario, i.Status, i.Observacao, i.Produto.Nome
                )).ToList()
            ))
            .FirstOrDefaultAsync();

        if (comanda is null)
            return NotFound();

        return Ok(comanda);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateComandaDto dto)
    {
        var empresaId = GetEmpresaId();

        var reservaExists = await db.Reservas.AnyAsync(r => r.Id == dto.IdReserva && r.IdEmpresa == empresaId);
        if (!reservaExists)
            return BadRequest(new { message = "Reserva nao encontrada" });

        var comanda = new Comanda
        {
            IdEmpresa = empresaId,
            IdReserva = dto.IdReserva
        };

        db.Comandas.Add(comanda);
        await db.SaveChangesAsync();

        return Created("", new ComandaDto(
            comanda.Id, comanda.IdReserva, comanda.Status,
            comanda.DataAbertura, comanda.DataFechamento, comanda.Total,
            new List<ItemComandaDto>()
        ));
    }

    [HttpPatch("{id}/fechar")]
    public async Task<IActionResult> Fechar(int id)
    {
        var empresaId = GetEmpresaId();
        var comanda = await db.Comandas
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(c => c.Id == id && c.IdEmpresa == empresaId);

        if (comanda is null)
            return NotFound();

        if (comanda.Status == ComandaStatus.Fechada)
            return BadRequest(new { message = "Comanda ja esta fechada" });

        comanda.Status = ComandaStatus.Fechada;
        comanda.DataFechamento = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Ok(new ComandaDto(
            comanda.Id, comanda.IdReserva, comanda.Status,
            comanda.DataAbertura, comanda.DataFechamento, comanda.Total,
            new List<ItemComandaDto>()
        ));
    }

    [HttpPost("{id}/itens")]
    public async Task<IActionResult> AddItem(int id, [FromBody] AddItemDto dto)
    {
        var empresaId = GetEmpresaId();
        var comanda = await db.Comandas
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(c => c.Id == id && c.IdEmpresa == empresaId);

        if (comanda is null)
            return NotFound();

        if (comanda.Status == ComandaStatus.Fechada)
            return BadRequest(new { message = "Nao e possivel adicionar itens a uma comanda fechada" });

        var produto = await db.Produtos.FirstOrDefaultAsync(p => p.Id == dto.IdProduto && p.IdEmpresa == empresaId);
        if (produto is null)
            return BadRequest(new { message = "Produto nao encontrado" });

        var item = new ItemComanda
        {
            IdComanda = comanda.Id,
            IdProduto = dto.IdProduto,
            Quantidade = dto.Quantidade,
            PrecoUnitario = produto.Preco,
            Observacao = dto.Observacao
        };

        comanda.Itens.Add(item);
        comanda.Total = comanda.Itens.Sum(i => i.PrecoUnitario * i.Quantidade);

        await db.SaveChangesAsync();

        return Created("", new ItemComandaDto(
            item.Id, item.IdProduto, item.Quantidade,
            item.PrecoUnitario, item.Status, item.Observacao, produto.Nome
        ));
    }

    [HttpDelete("{id}/itens/{itemId}")]
    public async Task<IActionResult> RemoveItem(int id, int itemId)
    {
        var empresaId = GetEmpresaId();
        var comanda = await db.Comandas
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(c => c.Id == id && c.IdEmpresa == empresaId);

        if (comanda is null)
            return NotFound();

        var item = comanda.Itens.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return NotFound();

        comanda.Itens.Remove(item);
        db.ItensComanda.Remove(item);
        comanda.Total = comanda.Itens.Sum(i => i.PrecoUnitario * i.Quantidade);

        await db.SaveChangesAsync();

        return NoContent();
    }
}
