using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PousadaApp.API.DTOs;
using PousadaApp.Domain.Entities;
using PousadaApp.Infrastructure.Data;

namespace PousadaApp.API.Controllers;

[Authorize]
public class ProdutosController(PousadaDbContext db) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? categoriaId)
    {
        var empresaId = GetEmpresaId();

        var query = db.Produtos
            .Include(p => p.Categoria)
            .Where(p => p.IdEmpresa == empresaId);

        if (categoriaId.HasValue)
            query = query.Where(p => p.IdCategoria == categoriaId.Value);

        var produtos = await query
            .Select(p => new ProdutoDto(p.Id, p.IdCategoria, p.Nome, p.Descricao, p.Preco, p.FotoUrl, p.Disponivel, p.Categoria.Nome))
            .ToListAsync();

        return Ok(produtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var empresaId = GetEmpresaId();

        var produto = await db.Produtos
            .Include(p => p.Categoria)
            .Where(p => p.Id == id && p.IdEmpresa == empresaId)
            .Select(p => new ProdutoDto(p.Id, p.IdCategoria, p.Nome, p.Descricao, p.Preco, p.FotoUrl, p.Disponivel, p.Categoria.Nome))
            .FirstOrDefaultAsync();

        if (produto is null)
            return NotFound();

        return Ok(produto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProdutoDto dto)
    {
        var empresaId = GetEmpresaId();

        var produto = new Produto
        {
            IdEmpresa = empresaId,
            IdCategoria = dto.IdCategoria,
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Preco = dto.Preco,
            FotoUrl = dto.FotoUrl
        };

        db.Produtos.Add(produto);
        await db.SaveChangesAsync();

        var categoria = await db.CategoriasCardapio.FindAsync(dto.IdCategoria);

        return CreatedAtAction(nameof(GetById), new { id = produto.Id },
            new ProdutoDto(produto.Id, produto.IdCategoria, produto.Nome, produto.Descricao, produto.Preco, produto.FotoUrl, produto.Disponivel, categoria?.Nome));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProdutoDto dto)
    {
        var empresaId = GetEmpresaId();
        var produto = await db.Produtos.FirstOrDefaultAsync(p => p.Id == id && p.IdEmpresa == empresaId);

        if (produto is null)
            return NotFound();

        produto.IdCategoria = dto.IdCategoria;
        produto.Nome = dto.Nome;
        produto.Descricao = dto.Descricao;
        produto.Preco = dto.Preco;
        produto.FotoUrl = dto.FotoUrl;

        await db.SaveChangesAsync();

        var categoria = await db.CategoriasCardapio.FindAsync(dto.IdCategoria);

        return Ok(new ProdutoDto(produto.Id, produto.IdCategoria, produto.Nome, produto.Descricao, produto.Preco, produto.FotoUrl, produto.Disponivel, categoria?.Nome));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var empresaId = GetEmpresaId();
        var produto = await db.Produtos.FirstOrDefaultAsync(p => p.Id == id && p.IdEmpresa == empresaId);

        if (produto is null)
            return NotFound();

        db.Produtos.Remove(produto);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
