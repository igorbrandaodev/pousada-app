namespace PousadaApp.API.DTOs;

public record ResumoFinanceiroDto(decimal TotalHospedagem, decimal TotalRestaurante, decimal Total);

public record TransacaoDto(DateTime Data, string Descricao, string Tipo, decimal Valor);
