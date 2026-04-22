using Microsoft.AspNetCore.Mvc;
using PousadaApp.API.Controllers;
using PousadaApp.API.DTOs;
using PousadaApp.Tests.Helpers;

namespace PousadaApp.Tests.Controllers;

public class ProdutosControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsAllProdutosForEmpresa()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ProdutosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetAll(null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var produtos = Assert.IsType<List<ProdutoDto>>(okResult.Value);
        // App seed has 20 produtos for empresa 1
        Assert.Equal(20, produtos.Count);
        Assert.All(produtos, p => Assert.NotEqual("Produto Outra Empresa", p.Nome));
    }

    [Fact]
    public async Task GetAll_FilterByCategoria_ReturnsBebidas()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ProdutosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Categoria 1 = Bebidas (4 products in app seed)
        var result = await controller.GetAll(categoriaId: 1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var produtos = Assert.IsType<List<ProdutoDto>>(okResult.Value);
        Assert.Equal(4, produtos.Count);
        Assert.All(produtos, p => Assert.Equal(1, p.IdCategoria));
    }

    [Fact]
    public async Task GetAll_FilterByCategoria_ReturnsPratosPrincipais()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ProdutosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Categoria 2 = Pratos Principais (4 products in app seed)
        var result = await controller.GetAll(categoriaId: 2);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var produtos = Assert.IsType<List<ProdutoDto>>(okResult.Value);
        Assert.Equal(4, produtos.Count);
        Assert.All(produtos, p => Assert.Equal("Pratos Principais", p.Categoria));
    }

    [Fact]
    public async Task GetById_ExistingProduto_ReturnsProduto()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ProdutosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var produto = Assert.IsType<ProdutoDto>(okResult.Value);
        Assert.Equal("Agua Mineral 500ml", produto.Nome);
        Assert.Equal(6.00m, produto.Preco);
        Assert.True(produto.Disponivel);
        Assert.Equal("Bebidas", produto.Categoria);
    }

    [Fact]
    public async Task GetById_NonExistingProduto_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ProdutosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_ProdutoFromOtherEmpresa_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ProdutosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Produto 100 belongs to empresa 100
        var result = await controller.GetById(100);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ValidProduto_Returns201()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ProdutosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new CreateProdutoDto(
            IdCategoria: 1,
            Nome: "Suco de Laranja",
            Descricao: "Natural",
            Preco: 12.00m,
            FotoUrl: "https://img.com/suco.jpg"
        );

        var result = await controller.Create(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var produto = Assert.IsType<ProdutoDto>(createdResult.Value);
        Assert.Equal("Suco de Laranja", produto.Nome);
        Assert.Equal("Natural", produto.Descricao);
        Assert.Equal(12.00m, produto.Preco);
        Assert.True(produto.Disponivel);
        Assert.Equal("Bebidas", produto.Categoria);
    }

    [Fact]
    public async Task Update_ExistingProduto_ReturnsUpdated()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ProdutosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new CreateProdutoDto(
            IdCategoria: 2,
            Nome: "Agua Premium",
            Descricao: "Agua importada",
            Preco: 15.00m,
            FotoUrl: null
        );

        var result = await controller.Update(1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var produto = Assert.IsType<ProdutoDto>(okResult.Value);
        Assert.Equal("Agua Premium", produto.Nome);
        Assert.Equal(15.00m, produto.Preco);
        Assert.Equal(2, produto.IdCategoria);
        Assert.Equal("Pratos Principais", produto.Categoria);
    }

    [Fact]
    public async Task Update_NonExistingProduto_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ProdutosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new CreateProdutoDto(1, "Teste", null, 10m, null);

        var result = await controller.Update(999, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ExistingProduto_Returns204()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ProdutosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);

        var deleted = await db.Produtos.FindAsync(1);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task Delete_NonExistingProduto_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ProdutosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ProdutoFromOtherEmpresa_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ProdutosController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Produto 100 belongs to empresa 100
        var result = await controller.Delete(100);

        Assert.IsType<NotFoundResult>(result);
    }
}
