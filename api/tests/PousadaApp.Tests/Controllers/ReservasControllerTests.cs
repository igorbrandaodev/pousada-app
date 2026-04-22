using Microsoft.AspNetCore.Mvc;
using PousadaApp.API.Controllers;
using PousadaApp.API.DTOs;
using PousadaApp.Domain.Enums;
using PousadaApp.Tests.Helpers;

namespace PousadaApp.Tests.Controllers;

public class ReservasControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsAllReservasForEmpresa()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ReservasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetAll(null, null, null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var reservas = Assert.IsType<List<ReservaDto>>(okResult.Value);
        // Extra seed adds reservas 100 (Confirmada) and 101 (Checkout) for empresa 1
        Assert.Equal(2, reservas.Count);
    }

    [Fact]
    public async Task GetAll_FilterByStatus_ReturnsFiltered()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ReservasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetAll(ReservaStatus.Confirmada, null, null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var reservas = Assert.IsType<List<ReservaDto>>(okResult.Value);
        Assert.Single(reservas);
        Assert.Equal(ReservaStatus.Confirmada, reservas[0].Status);
    }

    [Fact]
    public async Task GetAll_FilterByDateRange_ReturnsFiltered()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ReservasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Only reserva 100 has DataCheckin >= 2024-05-15
        var result = await controller.GetAll(null, new DateTime(2024, 5, 15), null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var reservas = Assert.IsType<List<ReservaDto>>(okResult.Value);
        Assert.Single(reservas);
        Assert.Equal(100, reservas[0].Id);
    }

    [Fact]
    public async Task GetById_ExistingReserva_ReturnsReserva()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ReservasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetById(100);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var reserva = Assert.IsType<ReservaDto>(okResult.Value);
        Assert.Equal(100, reserva.Id);
        Assert.Equal(1000.00m, reserva.ValorTotal);
        Assert.NotNull(reserva.Hospede);
        Assert.NotNull(reserva.Quarto);
    }

    [Fact]
    public async Task GetById_NonExistingReserva_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ReservasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ValidReserva_CalculatesValorTotal()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ReservasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Quarto 1 = "101", PrecoDiaria = 250.00
        var dto = new CreateReservaDto(
            IdHospede: 1,
            IdQuarto: 1,
            DataCheckin: new DateTime(2024, 7, 1),
            DataCheckout: new DateTime(2024, 7, 4),
            Observacoes: "Reserva teste"
        );

        var result = await controller.Create(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var reserva = Assert.IsType<ReservaDto>(createdResult.Value);
        // 3 nights * 250.00 = 750.00
        Assert.Equal(750.00m, reserva.ValorTotal);
        Assert.Equal(ReservaStatus.Pendente, reserva.Status);
        Assert.Equal("Reserva teste", reserva.Observacoes);
    }

    [Fact]
    public async Task Create_InvalidQuarto_ReturnsBadRequest()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ReservasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new CreateReservaDto(1, 999, new DateTime(2024, 7, 1), new DateTime(2024, 7, 4), null);

        var result = await controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_InvalidHospede_ReturnsBadRequest()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ReservasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new CreateReservaDto(999, 1, new DateTime(2024, 7, 1), new DateTime(2024, 7, 4), null);

        var result = await controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_CheckoutBeforeCheckin_ReturnsBadRequest()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ReservasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new CreateReservaDto(1, 1, new DateTime(2024, 7, 5), new DateTime(2024, 7, 1), null);

        var result = await controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_QuartoFromOtherEmpresa_ReturnsBadRequest()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ReservasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Quarto 100 belongs to empresa 100
        var dto = new CreateReservaDto(1, 100, new DateTime(2024, 7, 1), new DateTime(2024, 7, 4), null);

        var result = await controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_ToCheckin_SetsQuartoOcupado()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ReservasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new UpdateReservaStatusDto(ReservaStatus.Checkin);

        var result = await controller.UpdateStatus(100, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var reserva = Assert.IsType<ReservaDto>(okResult.Value);
        Assert.Equal(ReservaStatus.Checkin, reserva.Status);

        var quarto = await db.Quartos.FindAsync(1);
        Assert.Equal(QuartoStatus.Ocupado, quarto!.Status);
    }

    [Fact]
    public async Task UpdateStatus_ToCheckout_SetsQuartoLimpeza()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ReservasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // First do checkin on reserva 100 (linked to quarto 1)
        await controller.UpdateStatus(100, new UpdateReservaStatusDto(ReservaStatus.Checkin));

        // Then checkout
        var result = await controller.UpdateStatus(100, new UpdateReservaStatusDto(ReservaStatus.Checkout));

        var okResult = Assert.IsType<OkObjectResult>(result);
        var reserva = Assert.IsType<ReservaDto>(okResult.Value);
        Assert.Equal(ReservaStatus.Checkout, reserva.Status);

        var quarto = await db.Quartos.FindAsync(1);
        Assert.Equal(QuartoStatus.Limpeza, quarto!.Status);
    }

    [Fact]
    public async Task UpdateStatus_CanceladaFromCheckin_SetsQuartoLimpeza()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ReservasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // First do checkin
        await controller.UpdateStatus(100, new UpdateReservaStatusDto(ReservaStatus.Checkin));

        // Then cancel
        var result = await controller.UpdateStatus(100, new UpdateReservaStatusDto(ReservaStatus.Cancelada));

        var okResult = Assert.IsType<OkObjectResult>(result);
        var reserva = Assert.IsType<ReservaDto>(okResult.Value);
        Assert.Equal(ReservaStatus.Cancelada, reserva.Status);

        var quarto = await db.Quartos.FindAsync(1);
        Assert.Equal(QuartoStatus.Limpeza, quarto!.Status);
    }

    [Fact]
    public async Task UpdateStatus_NonExistingReserva_Returns404()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ReservasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        var dto = new UpdateReservaStatusDto(ReservaStatus.Checkin);

        var result = await controller.UpdateStatus(999, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_ExistingReserva_RecalculatesValorTotal()
    {
        var db = TestDbContext.CreateSeeded();
        var controller = new ReservasController(db);
        TestDbContext.SetupControllerContext(controller, empresaId: 1);

        // Move reserva 100 to quarto 3 (Luxo, 400/night) for 2 nights
        var dto = new UpdateReservaDto(
            IdHospede: 1,
            IdQuarto: 3, // Luxo room at 400.00/night (app seed quarto 3)
            DataCheckin: new DateTime(2024, 7, 1),
            DataCheckout: new DateTime(2024, 7, 3),
            Observacoes: "Atualizada"
        );

        var result = await controller.Update(100, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var reserva = Assert.IsType<ReservaDto>(okResult.Value);
        // 2 nights * 400.00 = 800.00
        Assert.Equal(800.00m, reserva.ValorTotal);
        Assert.Equal("Atualizada", reserva.Observacoes);
    }
}
