using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PousadaApp.Domain.Entities;
using PousadaApp.Domain.Enums;
using PousadaApp.Infrastructure.Data;

namespace PousadaApp.Tests.Helpers;

/// <summary>
/// The application's SeedData.Seed() runs inside OnModelCreating, so EnsureCreated()
/// already populates empresa 1 ("Pousada Bella Vista"), 1 admin user, 8 quartos,
/// 8 hospedes, 5 categorias, and 20 produtos. This helper adds extra entities
/// on top of that seed for multi-tenant and edge-case testing.
/// </summary>
public static class TestDbContext
{
    public static PousadaDbContext Create(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<PousadaDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var context = new PousadaDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static PousadaDbContext CreateSeeded(string? dbName = null)
    {
        var context = Create(dbName);
        SeedExtraTestData(context);
        return context;
    }

    private static void SeedExtraTestData(PousadaDbContext context)
    {
        if (context.Empresas.Any(e => e.Id == 100))
            return;

        // Second empresa for multi-tenant isolation tests
        var empresa2 = new Empresa
        {
            Id = 100,
            Nome = "Outra Pousada",
            UrlSlug = "outra-pousada",
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        context.Empresas.Add(empresa2);

        // Inactive user for login failure test
        var usuarioInativo = new Usuario
        {
            Id = 100,
            IdEmpresa = 1,
            Email = "inativo@teste.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
            Nome = "Usuario Inativo",
            Role = UsuarioRole.Staff,
            Ativo = false
        };
        context.Usuarios.Add(usuarioInativo);

        // Quarto belonging to empresa 100 (for isolation tests)
        var quartoOutraEmpresa = new Quarto
        {
            Id = 100,
            IdEmpresa = 100,
            Numero = "X01",
            Tipo = QuartoTipo.Suite,
            Capacidade = 4,
            PrecoDiaria = 600.00m,
            Status = QuartoStatus.Disponivel,
            Andar = 1
        };
        context.Quartos.Add(quartoOutraEmpresa);

        // Hospede belonging to empresa 100
        var hospedeOutraEmpresa = new Hospede
        {
            Id = 100,
            IdEmpresa = 100,
            Nome = "Hospede Outra Empresa",
            Documento = "000.000.000-00",
            Telefone = "(00) 00000-0000",
            Email = "outro@email.com",
            Cidade = "Outra Cidade"
        };
        context.Hospedes.Add(hospedeOutraEmpresa);

        // Categoria + Produto belonging to empresa 100
        var categoriaOutra = new CategoriaCardapio
        {
            Id = 100,
            IdEmpresa = 100,
            Nome = "Categoria Outra",
            Icone = "pi pi-star"
        };
        context.CategoriasCardapio.Add(categoriaOutra);

        var produtoOutraEmpresa = new Produto
        {
            Id = 100,
            IdEmpresa = 100,
            IdCategoria = 100,
            Nome = "Produto Outra Empresa",
            Preco = 10.00m,
            Disponivel = true
        };
        context.Produtos.Add(produtoOutraEmpresa);

        // Reservas for empresa 1
        var reservaConfirmada = new Reserva
        {
            Id = 100,
            IdEmpresa = 1,
            IdHospede = 1,
            IdQuarto = 1,
            DataCheckin = new DateTime(2024, 6, 1),
            DataCheckout = new DateTime(2024, 6, 5),
            Status = ReservaStatus.Confirmada,
            ValorTotal = 1000.00m
        };

        var reservaCheckout = new Reserva
        {
            Id = 101,
            IdEmpresa = 1,
            IdHospede = 2,
            IdQuarto = 2,
            DataCheckin = new DateTime(2024, 5, 1),
            DataCheckout = new DateTime(2024, 5, 3),
            Status = ReservaStatus.Checkout,
            ValorTotal = 800.00m
        };

        context.Reservas.AddRange(reservaConfirmada, reservaCheckout);

        // Comandas for empresa 1
        var comandaAberta = new Comanda
        {
            Id = 100,
            IdEmpresa = 1,
            IdReserva = 100,
            Status = ComandaStatus.Aberta,
            DataAbertura = new DateTime(2024, 6, 1),
            Total = 0
        };

        var comandaFechada = new Comanda
        {
            Id = 101,
            IdEmpresa = 1,
            IdReserva = 101,
            Status = ComandaStatus.Fechada,
            DataAbertura = new DateTime(2024, 5, 1),
            DataFechamento = new DateTime(2024, 5, 3),
            Total = 50.00m
        };

        context.Comandas.AddRange(comandaAberta, comandaFechada);
        context.SaveChanges();
    }

    public static void SetupControllerContext(ControllerBase controller, int empresaId = 1)
    {
        var claims = new List<Claim>
        {
            new("empresaId", empresaId.ToString()),
            new("userId", "1"),
            new(ClaimTypes.Role, "Admin"),
            new("nome", "Admin Teste"),
            new(ClaimTypes.Email, "admin@teste.com")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
}
