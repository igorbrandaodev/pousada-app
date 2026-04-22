using Microsoft.AspNetCore.Mvc;
using PousadaApp.API.Controllers;
using PousadaApp.API.DTOs;
using PousadaApp.Domain.Enums;
using PousadaApp.Tests.Helpers;

namespace PousadaApp.Tests.Controllers;

public class QuartosControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsAllQuartosForEmpresa()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new QuartosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var quartos = Assert.IsType<List<QuartoDto>>(okResult.Value);
        // App seed has 8 quartos for empresa 1
        Assert.Equal(8, quartos.Count);
        Assert.All(quartos, q => Assert.NotEqual("X01", q.Numero));
    }

    [Fact]
    public async Task GetAll_DoesNotReturnQuartosFromOtherEmpresa()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new QuartosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 100);

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var quartos = Assert.IsType<List<QuartoDto>>(okResult.Value);
        Assert.Single(quartos);
        Assert.Equal("X01", quartos[0].Numero);
    }

    [Fact]
    public async Task GetById_ExistingQuarto_ReturnsQuarto()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new QuartosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var quarto = Assert.IsType<QuartoDto>(okResult.Value);
        Assert.Equal("101", quarto.Numero);
        Assert.Equal(QuartoTipo.Standard, quarto.Tipo);
        Assert.Equal(250.00m, quarto.PrecoDiaria);
    }

    [Fact]
    public async Task GetById_NonExistingQuarto_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new QuartosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_QuartoFromOtherEmpresa_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new QuartosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Quarto 100 belongs to empresa 100
        var result = await controller.GetById(100);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ValidQuarto_Returns201()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new QuartosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new CreateQuartoDto("301", QuartoTipo.Suite, 4, 700.00m, 3);

        var result = await controller.Create(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var quarto = Assert.IsType<QuartoDto>(createdResult.Value);
        Assert.Equal("301", quarto.Numero);
        Assert.Equal(QuartoTipo.Suite, quarto.Tipo);
        Assert.Equal(4, quarto.Capacidade);
        Assert.Equal(700.00m, quarto.PrecoDiaria);
        Assert.Equal(3, quarto.Andar);
        Assert.Equal(QuartoStatus.Disponivel, quarto.Status);
        Assert.True(quarto.Ativo);
    }

    [Fact]
    public async Task Update_ExistingQuarto_ReturnsUpdated()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new QuartosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new UpdateQuartoDto("101-A", QuartoTipo.Luxo, 3, 350.00m, QuartoStatus.Manutencao, 1, true);

        var result = await controller.Update(1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var quarto = Assert.IsType<QuartoDto>(okResult.Value);
        Assert.Equal("101-A", quarto.Numero);
        Assert.Equal(QuartoTipo.Luxo, quarto.Tipo);
        Assert.Equal(3, quarto.Capacidade);
        Assert.Equal(350.00m, quarto.PrecoDiaria);
        Assert.Equal(QuartoStatus.Manutencao, quarto.Status);
    }

    [Fact]
    public async Task Update_NonExistingQuarto_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new QuartosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new UpdateQuartoDto("999", QuartoTipo.Standard, 2, 100m, QuartoStatus.Disponivel, 1, true);

        var result = await controller.Update(999, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_ChangesStatus()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new QuartosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new UpdateQuartoStatusDto(QuartoStatus.Ocupado);

        var result = await controller.UpdateStatus(1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var quarto = Assert.IsType<QuartoDto>(okResult.Value);
        Assert.Equal(QuartoStatus.Ocupado, quarto.Status);
    }

    [Fact]
    public async Task UpdateStatus_NonExistingQuarto_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new QuartosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new UpdateQuartoStatusDto(QuartoStatus.Limpeza);

        var result = await controller.UpdateStatus(999, dto);

        Assert.IsType<NotFoundResult>(result);
    }
}
