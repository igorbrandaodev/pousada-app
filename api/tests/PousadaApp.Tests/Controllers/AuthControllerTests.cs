using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PousadaApp.API.Auth;
using PousadaApp.API.Controllers;
using PousadaApp.API.DTOs;
using PousadaApp.Tests.Helpers;

namespace PousadaApp.Tests.Controllers;

public class AuthControllerTests
{
    private static JwtService CreateJwtService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsATestSecretKeyThatIsLongEnough12345",
                ["Jwt:Issuer"] = "TestIssuer"
            })
            .Build();

        return new JwtService(config);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // The app seed creates admin@pousada.com with password "admin123"
        var db = TestDbContext.CreateSeeded();
        var jwtService = CreateJwtService();
        var controller = new AuthController(db, jwtService);

        var request = new LoginRequest("admin@pousada.com", "admin123");

        var result = await controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.NotEmpty(response.Token);
        Assert.Equal("Administrador", response.Nome);
        Assert.Equal("admin@pousada.com", response.Email);
        Assert.Equal("Admin", response.Role);
        Assert.Equal(1, response.EmpresaId);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401()
    {
        var db = TestDbContext.CreateSeeded();
        var jwtService = CreateJwtService();
        var controller = new AuthController(db, jwtService);

        var request = new LoginRequest("admin@pousada.com", "senhaerrada");

        var result = await controller.Login(request);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_Returns401()
    {
        var db = TestDbContext.CreateSeeded();
        var jwtService = CreateJwtService();
        var controller = new AuthController(db, jwtService);

        var request = new LoginRequest("naoexiste@teste.com", "senha123");

        var result = await controller.Login(request);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithInactiveUser_Returns401()
    {
        var db = TestDbContext.CreateSeeded();
        var jwtService = CreateJwtService();
        var controller = new AuthController(db, jwtService);

        // inativo@teste.com is added by our extra seed with Ativo = false
        var request = new LoginRequest("inativo@teste.com", "senha123");

        var result = await controller.Login(request);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Register_WithNewUser_CreatesEmpresaAndUser()
    {
        var db = TestDbContext.CreateSeeded();
        var jwtService = CreateJwtService();
        var controller = new AuthController(db, jwtService);

        var request = new RegisterRequest(
            "Novo Admin",
            "novo@pousada.com",
            "novaSenha123",
            "Nova Pousada",
            "nova-pousada"
        );

        var result = await controller.Register(request);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var response = Assert.IsType<LoginResponse>(createdResult.Value);
        Assert.NotEmpty(response.Token);
        Assert.Equal("Novo Admin", response.Nome);
        Assert.Equal("novo@pousada.com", response.Email);
        Assert.Equal("Admin", response.Role);

        var empresa = db.Empresas.First(e => e.UrlSlug == "nova-pousada");
        Assert.Equal("Nova Pousada", empresa.Nome);

        var usuario = db.Usuarios.First(u => u.Email == "novo@pousada.com");
        Assert.Equal(empresa.Id, usuario.IdEmpresa);
        Assert.True(BCrypt.Net.BCrypt.Verify("novaSenha123", usuario.SenhaHash));
    }

    [Fact]
    public async Task Register_WithDuplicateSlug_ReturnsBadRequest()
    {
        var db = TestDbContext.CreateSeeded();
        var jwtService = CreateJwtService();
        var controller = new AuthController(db, jwtService);

        // "bella-vista" is the slug from the app seed
        var request = new RegisterRequest(
            "Admin",
            "outro@email.com",
            "senha",
            "Empresa",
            "bella-vista"
        );

        var result = await controller.Register(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        var db = TestDbContext.CreateSeeded();
        var jwtService = CreateJwtService();
        var controller = new AuthController(db, jwtService);

        // "admin@pousada.com" exists in the app seed
        var request = new RegisterRequest(
            "Admin",
            "admin@pousada.com",
            "senha",
            "Empresa Nova",
            "slug-novo"
        );

        var result = await controller.Register(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
