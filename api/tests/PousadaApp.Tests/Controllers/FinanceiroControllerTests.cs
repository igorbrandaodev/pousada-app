using Microsoft.AspNetCore.Mvc;
using PousadaApp.API.Controllers;
using PousadaApp.API.DTOs;
using PousadaApp.Tests.Helpers;

namespace PousadaApp.Tests.Controllers;

public class FinanceiroControllerTests
{
    [Fact]
    public async Task GetResumo_ReturnsCorrectTotals()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new FinanceiroController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetResumo();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var resumo = Assert.IsType<ResumoFinanceiroDto>(okResult.Value);

        // Reserva 101 has status Checkout with ValorTotal 800.00
        Assert.Equal(800.00m, resumo.TotalHospedagem);
        // Comanda 101 is Fechada with Total 50.00
        Assert.Equal(50.00m, resumo.TotalRestaurante);
        Assert.Equal(850.00m, resumo.Total);
    }

    [Fact]
    public async Task GetResumo_ExcludesNonCheckoutReservas()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new FinanceiroController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Reserva 100 is Confirmada (not Checkout), so it should NOT be included
        var result = await controller.GetResumo();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var resumo = Assert.IsType<ResumoFinanceiroDto>(okResult.Value);
        // Only reserva 101 (Checkout, 800) counts
        Assert.Equal(800.00m, resumo.TotalHospedagem);
    }

    [Fact]
    public async Task GetResumo_EmptyData_ReturnsZeros()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new FinanceiroController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 100);

        var result = await controller.GetResumo();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var resumo = Assert.IsType<ResumoFinanceiroDto>(okResult.Value);
        Assert.Equal(0m, resumo.TotalHospedagem);
        Assert.Equal(0m, resumo.TotalRestaurante);
        Assert.Equal(0m, resumo.Total);
    }

    [Fact]
    public async Task GetTransacoes_ReturnsTransacoesForEmpresa()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new FinanceiroController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetTransacoes();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var transacoes = Assert.IsType<List<TransacaoDto>>(okResult.Value);

        // 1 checkout reservation (101) + 1 closed comanda (101) = 2 transactions
        Assert.Equal(2, transacoes.Count);

        var hospedagem = transacoes.FirstOrDefault(t => t.Tipo == "Hospedagem");
        Assert.NotNull(hospedagem);
        Assert.Equal(800.00m, hospedagem.Valor);
        Assert.Contains("Maria Fernanda Oliveira", hospedagem.Descricao);
        Assert.Contains("102", hospedagem.Descricao);

        var restaurante = transacoes.FirstOrDefault(t => t.Tipo == "Restaurante");
        Assert.NotNull(restaurante);
        Assert.Equal(50.00m, restaurante.Valor);
    }

    [Fact]
    public async Task GetTransacoes_OrderedByDateDescending()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new FinanceiroController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetTransacoes();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var transacoes = Assert.IsType<List<TransacaoDto>>(okResult.Value);

        if (transacoes.Count > 1)
        {
            for (var i = 0; i < transacoes.Count - 1; i++)
            {
                Assert.True(transacoes[i].Data >= transacoes[i + 1].Data);
            }
        }
    }

    [Fact]
    public async Task GetTransacoes_EmptyForEmpresaWithNoData()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new FinanceiroController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 100);

        var result = await controller.GetTransacoes();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var transacoes = Assert.IsType<List<TransacaoDto>>(okResult.Value);
        Assert.Empty(transacoes);
    }
}
