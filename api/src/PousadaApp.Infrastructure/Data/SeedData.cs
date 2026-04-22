using Microsoft.EntityFrameworkCore;
using PousadaApp.Domain.Entities;
using PousadaApp.Domain.Enums;

namespace PousadaApp.Infrastructure.Data;

public static class SeedData
{
    // Pre-computed BCrypt hash for "admin123"
    private const string AdminPasswordHash = "$2a$11$uPCh9a1iJzHlzjuoZgtGIeVjCloKvE4fhPXEysmJpwqIiQI0M/51K";

    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedEmpresas(modelBuilder);
        SeedUsuarios(modelBuilder);
        SeedQuartos(modelBuilder);
        SeedHospedes(modelBuilder);
        SeedCategoriasCardapio(modelBuilder);
        SeedProdutos(modelBuilder);
    }

    private static void SeedEmpresas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>().HasData(new Empresa
        {
            Id = 1,
            Nome = "Pousada Bella Vista",
            Fantasia = "Bella Vista",
            UrlSlug = "bella-vista",
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }

    private static void SeedUsuarios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasData(new Usuario
        {
            Id = 1,
            IdEmpresa = 1,
            Email = "admin@pousada.com",
            SenhaHash = AdminPasswordHash,
            Nome = "Administrador",
            Role = UsuarioRole.Admin,
            Ativo = true,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }

    private static void SeedQuartos(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Quarto>().HasData(
            new Quarto { Id = 1, IdEmpresa = 1, Numero = "101", Tipo = QuartoTipo.Standard, Capacidade = 2, PrecoDiaria = 250.00m, Status = QuartoStatus.Disponivel, Andar = 1, Ativo = true },
            new Quarto { Id = 2, IdEmpresa = 1, Numero = "102", Tipo = QuartoTipo.Standard, Capacidade = 2, PrecoDiaria = 250.00m, Status = QuartoStatus.Disponivel, Andar = 1, Ativo = true },
            new Quarto { Id = 3, IdEmpresa = 1, Numero = "103", Tipo = QuartoTipo.Luxo, Capacidade = 3, PrecoDiaria = 400.00m, Status = QuartoStatus.Disponivel, Andar = 1, Ativo = true },
            new Quarto { Id = 4, IdEmpresa = 1, Numero = "104", Tipo = QuartoTipo.Suite, Capacidade = 4, PrecoDiaria = 600.00m, Status = QuartoStatus.Disponivel, Andar = 1, Ativo = true },
            new Quarto { Id = 5, IdEmpresa = 1, Numero = "201", Tipo = QuartoTipo.Standard, Capacidade = 2, PrecoDiaria = 280.00m, Status = QuartoStatus.Disponivel, Andar = 2, Ativo = true },
            new Quarto { Id = 6, IdEmpresa = 1, Numero = "202", Tipo = QuartoTipo.Luxo, Capacidade = 3, PrecoDiaria = 450.00m, Status = QuartoStatus.Disponivel, Andar = 2, Ativo = true },
            new Quarto { Id = 7, IdEmpresa = 1, Numero = "203", Tipo = QuartoTipo.Luxo, Capacidade = 3, PrecoDiaria = 450.00m, Status = QuartoStatus.Disponivel, Andar = 2, Ativo = true },
            new Quarto { Id = 8, IdEmpresa = 1, Numero = "204", Tipo = QuartoTipo.Suite, Capacidade = 4, PrecoDiaria = 650.00m, Status = QuartoStatus.Disponivel, Andar = 2, Ativo = true }
        );
    }

    private static void SeedHospedes(ModelBuilder modelBuilder)
    {
        var createdAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Hospede>().HasData(
            new Hospede { Id = 1, IdEmpresa = 1, Nome = "Carlos Alberto Silva", Documento = "123.456.789-00", Telefone = "(11) 98765-4321", Email = "carlos.silva@email.com", Cidade = "Sao Paulo - SP", CreatedAt = createdAt },
            new Hospede { Id = 2, IdEmpresa = 1, Nome = "Maria Fernanda Oliveira", Documento = "987.654.321-00", Telefone = "(21) 97654-3210", Email = "maria.oliveira@email.com", Cidade = "Rio de Janeiro - RJ", CreatedAt = createdAt },
            new Hospede { Id = 3, IdEmpresa = 1, Nome = "Joao Pedro Santos", Documento = "456.789.123-00", Telefone = "(31) 96543-2109", Email = "joao.santos@email.com", Cidade = "Belo Horizonte - MG", CreatedAt = createdAt },
            new Hospede { Id = 4, IdEmpresa = 1, Nome = "Ana Carolina Pereira", Documento = "321.654.987-00", Telefone = "(41) 95432-1098", Email = "ana.pereira@email.com", Cidade = "Curitiba - PR", CreatedAt = createdAt },
            new Hospede { Id = 5, IdEmpresa = 1, Nome = "Rafael Mendes Costa", Documento = "654.321.987-00", Telefone = "(51) 94321-0987", Email = "rafael.costa@email.com", Cidade = "Porto Alegre - RS", CreatedAt = createdAt },
            new Hospede { Id = 6, IdEmpresa = 1, Nome = "Juliana Martins Rocha", Documento = "789.123.456-00", Telefone = "(71) 93210-9876", Email = "juliana.rocha@email.com", Cidade = "Salvador - BA", CreatedAt = createdAt },
            new Hospede { Id = 7, IdEmpresa = 1, Nome = "Fernando Augusto Lima", Documento = "234.567.890-00", Telefone = "(81) 92109-8765", Email = "fernando.lima@email.com", Cidade = "Recife - PE", CreatedAt = createdAt },
            new Hospede { Id = 8, IdEmpresa = 1, Nome = "Patricia Souza Almeida", Documento = "567.890.234-00", Telefone = "(48) 91098-7654", Email = "patricia.almeida@email.com", Cidade = "Florianopolis - SC", CreatedAt = createdAt }
        );
    }

    private static void SeedCategoriasCardapio(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoriaCardapio>().HasData(
            new CategoriaCardapio { Id = 1, IdEmpresa = 1, Nome = "Bebidas", Icone = "pi pi-glass-water" },
            new CategoriaCardapio { Id = 2, IdEmpresa = 1, Nome = "Pratos Principais", Icone = "pi pi-sun" },
            new CategoriaCardapio { Id = 3, IdEmpresa = 1, Nome = "Petiscos", Icone = "pi pi-bolt" },
            new CategoriaCardapio { Id = 4, IdEmpresa = 1, Nome = "Sobremesas", Icone = "pi pi-heart" },
            new CategoriaCardapio { Id = 5, IdEmpresa = 1, Nome = "Drinks", Icone = "pi pi-star" }
        );
    }

    private static void SeedProdutos(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>().HasData(
            // Bebidas (Categoria 1)
            new Produto { Id = 1, IdEmpresa = 1, IdCategoria = 1, Nome = "Agua Mineral 500ml", Preco = 6.00m, Disponivel = true },
            new Produto { Id = 2, IdEmpresa = 1, IdCategoria = 1, Nome = "Refrigerante Lata", Preco = 8.00m, Disponivel = true },
            new Produto { Id = 3, IdEmpresa = 1, IdCategoria = 1, Nome = "Suco Natural de Laranja", Preco = 12.00m, Disponivel = true },
            new Produto { Id = 4, IdEmpresa = 1, IdCategoria = 1, Nome = "Cerveja Long Neck", Preco = 14.00m, Disponivel = true },

            // Pratos Principais (Categoria 2)
            new Produto { Id = 5, IdEmpresa = 1, IdCategoria = 2, Nome = "Filé de Peixe Grelhado", Descricao = "Filé de peixe grelhado com legumes e arroz", Preco = 58.00m, Disponivel = true },
            new Produto { Id = 6, IdEmpresa = 1, IdCategoria = 2, Nome = "Picanha na Chapa", Descricao = "Picanha grelhada com arroz, feijao e farofa", Preco = 72.00m, Disponivel = true },
            new Produto { Id = 7, IdEmpresa = 1, IdCategoria = 2, Nome = "Moqueca de Camarao", Descricao = "Moqueca com camarao, leite de coco e dende", Preco = 68.00m, Disponivel = true },
            new Produto { Id = 8, IdEmpresa = 1, IdCategoria = 2, Nome = "Frango Parmegiana", Descricao = "Filé de frango empanado com molho e queijo", Preco = 48.00m, Disponivel = true },

            // Petiscos (Categoria 3)
            new Produto { Id = 9, IdEmpresa = 1, IdCategoria = 3, Nome = "Porcao de Batata Frita", Descricao = "Batata frita crocante com cheddar e bacon", Preco = 32.00m, Disponivel = true },
            new Produto { Id = 10, IdEmpresa = 1, IdCategoria = 3, Nome = "Bolinho de Bacalhau", Descricao = "6 unidades de bolinho de bacalhau artesanal", Preco = 38.00m, Disponivel = true },
            new Produto { Id = 11, IdEmpresa = 1, IdCategoria = 3, Nome = "Bruschetta Italiana", Descricao = "Torradas com tomate, manjericao e azeite", Preco = 28.00m, Disponivel = true },
            new Produto { Id = 12, IdEmpresa = 1, IdCategoria = 3, Nome = "Tabua de Frios", Descricao = "Selecao de queijos e embutidos artesanais", Preco = 55.00m, Disponivel = true },

            // Sobremesas (Categoria 4)
            new Produto { Id = 13, IdEmpresa = 1, IdCategoria = 4, Nome = "Pudim de Leite", Descricao = "Pudim caseiro de leite condensado", Preco = 18.00m, Disponivel = true },
            new Produto { Id = 14, IdEmpresa = 1, IdCategoria = 4, Nome = "Petit Gateau", Descricao = "Bolo de chocolate com sorvete de baunilha", Preco = 28.00m, Disponivel = true },
            new Produto { Id = 15, IdEmpresa = 1, IdCategoria = 4, Nome = "Acai na Tigela", Descricao = "Acai com granola, banana e leite condensado", Preco = 24.00m, Disponivel = true },
            new Produto { Id = 16, IdEmpresa = 1, IdCategoria = 4, Nome = "Mousse de Maracuja", Descricao = "Mousse cremoso de maracuja", Preco = 16.00m, Disponivel = true },

            // Drinks (Categoria 5)
            new Produto { Id = 17, IdEmpresa = 1, IdCategoria = 5, Nome = "Caipirinha de Limao", Descricao = "Cachaca artesanal, limao e acucar", Preco = 22.00m, Disponivel = true },
            new Produto { Id = 18, IdEmpresa = 1, IdCategoria = 5, Nome = "Pina Colada", Descricao = "Rum, leite de coco e suco de abacaxi", Preco = 28.00m, Disponivel = true },
            new Produto { Id = 19, IdEmpresa = 1, IdCategoria = 5, Nome = "Mojito", Descricao = "Rum, hortela, limao, acucar e agua com gas", Preco = 26.00m, Disponivel = true },
            new Produto { Id = 20, IdEmpresa = 1, IdCategoria = 5, Nome = "Gin Tonica", Descricao = "Gin, agua tonica, limao e especiarias", Preco = 30.00m, Disponivel = true }
        );
    }
}
