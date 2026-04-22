namespace PousadaApp.Domain.Entities;

public class CategoriaCardapio
{
    public int Id { get; set; }
    public int IdEmpresa { get; set; }
    public required string Nome { get; set; }
    public string? Icone { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
