using PousadaApp.Domain.Enums;

namespace PousadaApp.Domain.Entities;

public class Reserva
{
    public int Id { get; set; }
    public int IdEmpresa { get; set; }
    public int IdHospede { get; set; }
    public int IdQuarto { get; set; }
    public DateTime DataCheckin { get; set; }
    public DateTime DataCheckout { get; set; }
    public ReservaStatus Status { get; set; } = ReservaStatus.Pendente;
    public decimal ValorTotal { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public Hospede Hospede { get; set; } = null!;
    public Quarto Quarto { get; set; } = null!;
    public ICollection<Comanda> Comandas { get; set; } = new List<Comanda>();
}
