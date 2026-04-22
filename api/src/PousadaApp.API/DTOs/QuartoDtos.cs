using PousadaApp.Domain.Enums;

namespace PousadaApp.API.DTOs;

public record QuartoDto(
    int Id,
    string Numero,
    QuartoTipo Tipo,
    int Capacidade,
    decimal PrecoDiaria,
    QuartoStatus Status,
    int Andar,
    bool Ativo
);

public record CreateQuartoDto(
    string Numero,
    QuartoTipo Tipo,
    int Capacidade,
    decimal PrecoDiaria,
    int Andar
);

public record UpdateQuartoDto(
    string Numero,
    QuartoTipo Tipo,
    int Capacidade,
    decimal PrecoDiaria,
    QuartoStatus Status,
    int Andar,
    bool Ativo
);
