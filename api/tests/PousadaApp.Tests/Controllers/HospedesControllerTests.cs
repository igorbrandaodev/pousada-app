using Microsoft.AspNetCore.Mvc;
using PousadaApp.API.Controllers;
using PousadaApp.API.DTOs;
using PousadaApp.Tests.Helpers;

namespace PousadaApp.Tests.Controllers;

public class HospedesControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsAllHospedesForEmpresa()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new HospedesController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var hospedes = Assert.IsType<List<HospedeDto>>(okResult.Value);
        // App seed has 8 hospedes for empresa 1
        Assert.Equal(8, hospedes.Count);
        Assert.All(hospedes, h => Assert.NotEqual("Hospede Outra Empresa", h.Nome));
    }

    [Fact]
    public async Task GetAll_OnlyReturnsHospedesForRequestedEmpresa()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new HospedesController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 100);

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var hospedes = Assert.IsType<List<HospedeDto>>(okResult.Value);
        Assert.Single(hospedes);
        Assert.Equal("Hospede Outra Empresa", hospedes[0].Nome);
    }

    [Fact]
    public async Task GetById_ExistingHospede_ReturnsHospede()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new HospedesController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var hospede = Assert.IsType<HospedeDto>(okResult.Value);
        Assert.Equal("Carlos Alberto Silva", hospede.Nome);
        Assert.Equal("123.456.789-00", hospede.Documento);
        Assert.Equal("carlos.silva@email.com", hospede.Email);
    }

    [Fact]
    public async Task GetById_NonExistingHospede_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new HospedesController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_HospedeFromOtherEmpresa_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new HospedesController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Hospede 100 belongs to empresa 100
        var result = await controller.GetById(100);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ValidHospede_Returns201()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new HospedesController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new CreateHospedeDto(
            Nome: "Novo Hospede",
            Documento: "999.888.777-66",
            Telefone: "(11) 91234-5678",
            Email: "novo@email.com",
            Cidade: "Campinas - SP",
            Observacoes: "VIP"
        );

        var result = await controller.Create(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var hospede = Assert.IsType<HospedeDto>(createdResult.Value);
        Assert.Equal("Novo Hospede", hospede.Nome);
        Assert.Equal("999.888.777-66", hospede.Documento);
        Assert.Equal("(11) 91234-5678", hospede.Telefone);
        Assert.Equal("novo@email.com", hospede.Email);
        Assert.Equal("Campinas - SP", hospede.Cidade);
        Assert.Equal("VIP", hospede.Observacoes);
    }

    [Fact]
    public async Task Create_MinimalHospede_Returns201()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new HospedesController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new CreateHospedeDto(
            Nome: "Hospede Simples",
            Documento: null,
            Telefone: null,
            Email: null,
            Cidade: null,
            Observacoes: null
        );

        var result = await controller.Create(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var hospede = Assert.IsType<HospedeDto>(createdResult.Value);
        Assert.Equal("Hospede Simples", hospede.Nome);
        Assert.Null(hospede.Documento);
    }

    [Fact]
    public async Task Update_ExistingHospede_ReturnsUpdated()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new HospedesController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new CreateHospedeDto(
            Nome: "Carlos Atualizado",
            Documento: "111.222.333-44",
            Telefone: "(11) 99999-9999",
            Email: "carlos.novo@email.com",
            Cidade: "Santos - SP",
            Observacoes: "Atualizado"
        );

        var result = await controller.Update(1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var hospede = Assert.IsType<HospedeDto>(okResult.Value);
        Assert.Equal("Carlos Atualizado", hospede.Nome);
        Assert.Equal("111.222.333-44", hospede.Documento);
        Assert.Equal("carlos.novo@email.com", hospede.Email);
        Assert.Equal("Santos - SP", hospede.Cidade);
    }

    [Fact]
    public async Task Update_NonExistingHospede_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new HospedesController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new CreateHospedeDto("Nome", null, null, null, null, null);

        var result = await controller.Update(999, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_HospedeFromOtherEmpresa_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new HospedesController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new CreateHospedeDto("Nome", null, null, null, null, null);

        // Hospede 100 belongs to empresa 100
        var result = await controller.Update(100, dto);

        Assert.IsType<NotFoundResult>(result);
    }
}
