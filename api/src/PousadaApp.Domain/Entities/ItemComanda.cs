using PousadaApp.Domain.Enums;

namespace PousadaApp.Domain.Entities;

public class ItemComanda
{
    public int Id { get; set; }
    public int IdComanda { get; set; }
    public int IdProduto { get; set; }
    public int Quantidade { get; set; } = 1;
    public decimal PrecoUnitario { get; set; }
    public ItemComandaStatus Status { get; set; } = ItemComandaStatus.Pendente;
    public string? Observacao { get; set; }

    public Comanda Comanda { get; set; } = null!;
    public Produto Produto { get; set; } = null!;
}
