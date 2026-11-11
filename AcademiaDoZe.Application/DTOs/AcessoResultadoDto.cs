namespace AcademiaDoZe.Application.DTOs;

public record AcessoResultadoDto(string Mensagem, TimeSpan? Duracao = null, int? DiasRestantesPlano = null);
