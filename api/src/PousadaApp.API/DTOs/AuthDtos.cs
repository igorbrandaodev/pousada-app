namespace PousadaApp.API.DTOs;

public record LoginRequest(string Email, string Senha);

public record LoginResponse(string Token, string Nome, string Email, string Role, int EmpresaId);

public record RegisterRequest(string Nome, string Email, string Senha, string NomeEmpresa, string UrlSlug);
