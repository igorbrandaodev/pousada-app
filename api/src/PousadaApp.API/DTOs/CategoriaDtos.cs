namespace PousadaApp.API.DTOs;

public record CategoriaDto(int Id, string Nome, string? Icone);

public record CreateCategoriaDto(string Nome, string? Icone);
