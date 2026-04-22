using PousadaApp.Domain.Enums;

namespace PousadaApp.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public int IdEmpresa { get; set; }
    public required string Email { get; set; }
    public required string SenhaHash { get; set; }
    public required string Nome { get; set; }
    public UsuarioRole Role { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Empresa Empresa { get; set; } = null!;
}
