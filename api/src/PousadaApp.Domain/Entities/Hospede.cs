namespace PousadaApp.Domain.Entities;

public class Hospede
{
    public int Id { get; set; }
    public int IdEmpresa { get; set; }
    public required string Nome { get; set; }
    public string? Documento { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Cidade { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
