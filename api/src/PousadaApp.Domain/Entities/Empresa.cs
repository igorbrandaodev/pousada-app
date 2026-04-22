namespace PousadaApp.Domain.Entities;

public class Empresa
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public string? Fantasia { get; set; }
    public string? LogoUrl { get; set; }
    public required string UrlSlug { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<Quarto> Quartos { get; set; } = new List<Quarto>();
    public ICollection<Hospede> Hospedes { get; set; } = new List<Hospede>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<CategoriaCardapio> CategoriasCardapio { get; set; } = new List<CategoriaCardapio>();
    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
    public ICollection<Comanda> Comandas { get; set; } = new List<Comanda>();
}
