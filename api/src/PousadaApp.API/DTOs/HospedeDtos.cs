namespace PousadaApp.API.DTOs;

public record HospedeDto(
    int Id,
    string Nome,
    string? Documento,
    string? Telefone,
    string? Email,
    string? Cidade,
    string? Observacoes
);

public record CreateHospedeDto(
    string Nome,
    string? Documento,
    string? Telefone,
    string? Email,
    string? Cidade,
    string? Observacoes
);
