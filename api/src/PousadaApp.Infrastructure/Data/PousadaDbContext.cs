using Microsoft.EntityFrameworkCore;
using PousadaApp.Domain.Entities;
using PousadaApp.Domain.Enums;

namespace PousadaApp.Infrastructure.Data;

public class PousadaDbContext(DbContextOptions<PousadaDbContext> options) : DbContext(options)
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Quarto> Quartos => Set<Quarto>();
    public DbSet<Hospede> Hospedes => Set<Hospede>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<CategoriaCardapio> CategoriasCardapio => Set<CategoriaCardapio>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Comanda> Comandas => Set<Comanda>();
    public DbSet<ItemComanda> ItensComanda => Set<ItemComanda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureEmpresa(modelBuilder);
        ConfigureUsuario(modelBuilder);
        ConfigureQuarto(modelBuilder);
        ConfigureHospede(modelBuilder);
        ConfigureReserva(modelBuilder);
        ConfigureCategoriaCardapio(modelBuilder);
        ConfigureProduto(modelBuilder);
        ConfigureComanda(modelBuilder);
        ConfigureItemComanda(modelBuilder);

        SeedData.Seed(modelBuilder);
    }

    private static void ConfigureEmpresa(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.ToTable("empresas");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Fantasia).HasColumnName("fantasia").HasMaxLength(200);
            entity.Property(e => e.LogoUrl).HasColumnName("logo_url").HasMaxLength(500);
            entity.Property(e => e.UrlSlug).HasColumnName("url_slug").HasMaxLength(100).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.UrlSlug).IsUnique();
        });
    }

    private static void ConfigureUsuario(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200).IsRequired();
            entity.Property(e => e.SenhaHash).HasColumnName("senha_hash").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50).HasConversion<string>();
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Empresa)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureQuarto(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Quarto>(entity =>
        {
            entity.ToTable("quartos");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.Numero).HasColumnName("numero").HasMaxLength(10).IsRequired();
            entity.Property(e => e.Tipo).HasColumnName("tipo").HasMaxLength(50).HasConversion<string>();
            entity.Property(e => e.Capacidade).HasColumnName("capacidade");
            entity.Property(e => e.PrecoDiaria).HasColumnName("preco_diaria").HasPrecision(10, 2);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasConversion<string>();
            entity.Property(e => e.Andar).HasColumnName("andar");
            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.HasOne(e => e.Empresa)
                .WithMany(e => e.Quartos)
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureHospede(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hospede>(entity =>
        {
            entity.ToTable("hospedes");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Documento).HasColumnName("documento").HasMaxLength(20);
            entity.Property(e => e.Telefone).HasColumnName("telefone").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200);
            entity.Property(e => e.Cidade).HasColumnName("cidade").HasMaxLength(100);
            entity.Property(e => e.Observacoes).HasColumnName("observacoes").HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Empresa)
                .WithMany(e => e.Hospedes)
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureReserva(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.ToTable("reservas");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdHospede).HasColumnName("id_hospede");
            entity.Property(e => e.IdQuarto).HasColumnName("id_quarto");
            entity.Property(e => e.DataCheckin).HasColumnName("data_checkin");
            entity.Property(e => e.DataCheckout).HasColumnName("data_checkout");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasConversion<string>();
            entity.Property(e => e.ValorTotal).HasColumnName("valor_total").HasPrecision(10, 2);
            entity.Property(e => e.Observacoes).HasColumnName("observacoes").HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Empresa)
                .WithMany(e => e.Reservas)
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Hospede)
                .WithMany(e => e.Reservas)
                .HasForeignKey(e => e.IdHospede)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Quarto)
                .WithMany(e => e.Reservas)
                .HasForeignKey(e => e.IdQuarto)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCategoriaCardapio(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoriaCardapio>(entity =>
        {
            entity.ToTable("categorias_cardapio");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Icone).HasColumnName("icone").HasMaxLength(50);

            entity.HasOne(e => e.Empresa)
                .WithMany(e => e.CategoriasCardapio)
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureProduto(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("produtos");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Descricao).HasColumnName("descricao").HasMaxLength(500);
            entity.Property(e => e.Preco).HasColumnName("preco").HasPrecision(10, 2);
            entity.Property(e => e.FotoUrl).HasColumnName("foto_url").HasMaxLength(500);
            entity.Property(e => e.Disponivel).HasColumnName("disponivel");

            entity.HasOne(e => e.Empresa)
                .WithMany(e => e.Produtos)
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Categoria)
                .WithMany(e => e.Produtos)
                .HasForeignKey(e => e.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureComanda(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comanda>(entity =>
        {
            entity.ToTable("comandas");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasConversion<string>();
            entity.Property(e => e.DataAbertura).HasColumnName("data_abertura");
            entity.Property(e => e.DataFechamento).HasColumnName("data_fechamento");
            entity.Property(e => e.Total).HasColumnName("total").HasPrecision(10, 2);

            entity.HasOne(e => e.Empresa)
                .WithMany(e => e.Comandas)
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Reserva)
                .WithMany(e => e.Comandas)
                .HasForeignKey(e => e.IdReserva)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureItemComanda(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ItemComanda>(entity =>
        {
            entity.ToTable("itens_comanda");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdComanda).HasColumnName("id_comanda");
            entity.Property(e => e.IdProduto).HasColumnName("id_produto");
            entity.Property(e => e.Quantidade).HasColumnName("quantidade");
            entity.Property(e => e.PrecoUnitario).HasColumnName("preco_unitario").HasPrecision(10, 2);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasConversion<string>();
            entity.Property(e => e.Observacao).HasColumnName("observacao").HasMaxLength(500);

            entity.HasOne(e => e.Comanda)
                .WithMany(e => e.Itens)
                .HasForeignKey(e => e.IdComanda)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Produto)
                .WithMany(e => e.ItensComanda)
                .HasForeignKey(e => e.IdProduto)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
