namespace PousadaApp.API.DTOs;

public record ProdutoDto(
    int Id,
    int IdCategoria,
    string Nome,
    string? Descricao,
    decimal Preco,
    string? FotoUrl,
    bool Disponivel,
    string? Categoria
);

public record CreateProdutoDto(
    int IdCategoria,
    string Nome,
    string? Descricao,
    decimal Preco,
    string? FotoUrl
);
