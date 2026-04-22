using PousadaApp.Domain.Enums;

namespace PousadaApp.API.DTOs;

public record ComandaDto(
    int Id,
    int IdReserva,
    ComandaStatus Status,
    DateTime DataAbertura,
    DateTime? DataFechamento,
    decimal Total,
    List<ItemComandaDto> Itens
);

public record CreateComandaDto(int IdReserva);

public record ItemComandaDto(
    int Id,
    int IdProduto,
    int Quantidade,
    decimal PrecoUnitario,
    ItemComandaStatus Status,
    string? Observacao,
    string? ProdutoNome
);

public record AddItemDto(int IdProduto, int Quantidade, string? Observacao);
