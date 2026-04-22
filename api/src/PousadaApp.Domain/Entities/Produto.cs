namespace PousadaApp.Domain.Entities;

public class Produto
{
    public int Id { get; set; }
    public int IdEmpresa { get; set; }
    public int IdCategoria { get; set; }
    public required string Nome { get; set; }
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public string? FotoUrl { get; set; }
    public bool Disponivel { get; set; } = true;

    public Empresa Empresa { get; set; } = null!;
    public CategoriaCardapio Categoria { get; set; } = null!;
    public ICollection<ItemComanda> ItensComanda { get; set; } = new List<ItemComanda>();
}
