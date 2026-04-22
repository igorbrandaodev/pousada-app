using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PousadaApp.API.Controllers;
using PousadaApp.API.DTOs;
using PousadaApp.Domain.Enums;
using PousadaApp.Tests.Helpers;

namespace PousadaApp.Tests.Controllers;

public class ComandasControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsAllComandasForEmpresa()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetAll(null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var comandas = Assert.IsType<List<ComandaDto>>(okResult.Value);
        // Extra seed adds comandas 100 (Aberta) and 101 (Fechada) for empresa 1
        Assert.Equal(2, comandas.Count);
    }

    [Fact]
    public async Task GetAll_FilterByStatus_ReturnsFiltered()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetAll(ComandaStatus.Aberta);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var comandas = Assert.IsType<List<ComandaDto>>(okResult.Value);
        Assert.Single(comandas);
        Assert.Equal(ComandaStatus.Aberta, comandas[0].Status);
    }

    [Fact]
    public async Task GetById_ExistingComanda_ReturnsComanda()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetById(100);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var comanda = Assert.IsType<ComandaDto>(okResult.Value);
        Assert.Equal(100, comanda.Id);
        Assert.Equal(ComandaStatus.Aberta, comanda.Status);
    }

    [Fact]
    public async Task GetById_NonExistingComanda_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ValidComanda_ReturnsComanda()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new CreateComandaDto(IdReserva: 100);

        var result = await controller.Create(dto);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var comanda = Assert.IsType<ComandaDto>(createdResult.Value);
        Assert.Equal(100, comanda.IdReserva);
        Assert.Equal(ComandaStatus.Aberta, comanda.Status);
        Assert.Equal(0, comanda.Total);
        Assert.Empty(comanda.Itens);
    }

    [Fact]
    public async Task Create_InvalidReserva_ReturnsBadRequest()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new CreateComandaDto(IdReserva: 999);

        var result = await controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddItem_ValidItem_UpdatesTotal()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Produto 1 = "Agua Mineral 500ml" at 6.00
        var dto = new AddItemDto(IdProduto: 1, Quantidade: 2, Observacao: "Gelada");

        var result = await controller.AddItem(100, dto);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var item = Assert.IsType<ItemComandaDto>(createdResult.Value);
        Assert.Equal(1, item.IdProduto);
        Assert.Equal(2, item.Quantidade);
        Assert.Equal(6.00m, item.PrecoUnitario);
        Assert.Equal("Gelada", item.Observacao);
        Assert.Equal("Agua Mineral 500ml", item.ProdutoNome);

        var comanda = await db.Comandas.FindAsync(100);
        Assert.Equal(12.00m, comanda!.Total);
    }

    [Fact]
    public async Task AddItem_MultipleItems_AccumulatesTotal()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Produto 1 = Agua Mineral at 6.00, Produto 5 = File de Peixe Grelhado at 58.00
        await controller.AddItem(100, new AddItemDto(1, 2, null)); // 2 * 6.00 = 12.00
        await controller.AddItem(100, new AddItemDto(5, 1, null)); // 1 * 58.00 = 58.00

        var comanda = await db.Comandas.FindAsync(100);
        Assert.Equal(70.00m, comanda!.Total);
    }

    [Fact]
    public async Task AddItem_ToClosedComanda_ReturnsBadRequest()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Comanda 101 is closed
        var dto = new AddItemDto(1, 1, null);

        var result = await controller.AddItem(101, dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddItem_InvalidProduto_ReturnsBadRequest()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new AddItemDto(999, 1, null);

        var result = await controller.AddItem(100, dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddItem_ProdutoFromOtherEmpresa_ReturnsBadRequest()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Produto 100 belongs to empresa 100
        var dto = new AddItemDto(100, 1, null);

        var result = await controller.AddItem(100, dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RemoveItem_UpdatesTotal()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Add two items to comanda 100
        await controller.AddItem(100, new AddItemDto(1, 2, null)); // 12.00
        await controller.AddItem(100, new AddItemDto(5, 1, null)); // 58.00

        var comanda = await db.Comandas.Include(c => c.Itens).FirstAsync(c => c.Id == 100);
        var firstItemId = comanda.Itens.First(i => i.IdProduto == 1).Id;

        var result = await controller.RemoveItem(100, firstItemId);

        Assert.IsType<NoContentResult>(result);

        await db.Entry(comanda).ReloadAsync();
        Assert.Equal(58.00m, comanda.Total);
    }

    [Fact]
    public async Task RemoveItem_NonExistingComanda_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.RemoveItem(999, 1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task RemoveItem_NonExistingItem_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.RemoveItem(100, 999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Fechar_SetsStatusFechada()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.Fechar(100);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var comanda = Assert.IsType<ComandaDto>(okResult.Value);
        Assert.Equal(ComandaStatus.Fechada, comanda.Status);
        Assert.NotNull(comanda.DataFechamento);
    }

    [Fact]
    public async Task Fechar_AlreadyClosed_ReturnsBadRequest()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Comanda 101 is already closed
        var result = await controller.Fechar(101);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Fechar_NonExistingComanda_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ComandasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.Fechar(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
