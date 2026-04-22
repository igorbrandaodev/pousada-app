using PousadaApp.Domain.Enums;

namespace PousadaApp.Domain.Entities;

public class Quarto
{
    public int Id { get; set; }
    public int IdEmpresa { get; set; }
    public required string Numero { get; set; }
    public QuartoTipo Tipo { get; set; }
    public int Capacidade { get; set; } = 2;
    public decimal PrecoDiaria { get; set; }
    public QuartoStatus Status { get; set; } = QuartoStatus.Disponivel;
    public int Andar { get; set; } = 1;
    public bool Ativo { get; set; } = true;

    public Empresa Empresa { get; set; } = null!;
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
