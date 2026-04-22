using PousadaApp.Domain.Enums;

namespace PousadaApp.Domain.Entities;

public class Comanda
{
    public int Id { get; set; }
    public int IdEmpresa { get; set; }
    public int IdReserva { get; set; }
    public ComandaStatus Status { get; set; } = ComandaStatus.Aberta;
    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;
    public DateTime? DataFechamento { get; set; }
    public decimal Total { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public Reserva Reserva { get; set; } = null!;
    public ICollection<ItemComanda> Itens { get; set; } = new List<ItemComanda>();
}
