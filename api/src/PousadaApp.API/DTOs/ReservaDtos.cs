using PousadaApp.Domain.Enums;

namespace PousadaApp.API.DTOs;

public record ReservaDto(
    int Id,
    int IdHospede,
    int IdQuarto,
    DateTime DataCheckin,
    DateTime DataCheckout,
    ReservaStatus Status,
    decimal ValorTotal,
    string? Observacoes,
    HospedeDto? Hospede,
    QuartoDto? Quarto
);

public record CreateReservaDto(
    int IdHospede,
    int IdQuarto,
    DateTime DataCheckin,
    DateTime DataCheckout,
    string? Observacoes
);

public record UpdateReservaDto(
    int IdHospede,
    int IdQuarto,
    DateTime DataCheckin,
    DateTime DataCheckout,
    string? Observacoes
);
