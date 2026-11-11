using AcademiadoZE.Domain.Enums;

namespace AcademiaDoZe.Application.DTOs;

public class ResumoDiarioColaboradorDto
{
    public int ColaboradorId { get; init; }
    public ColaboradorVinculo Vinculo { get; init; }
    public TimeSpan HorasTrabalhadas { get; init; }
    public TimeSpan JornadaPrevista { get; init; }
    public TimeSpan SaldoHoras { get; init; }

    public string VinculoDescricao => Vinculo == ColaboradorVinculo.Clt ? "CLT" : "Estágio";
    public string HorasTrabalhadasDescricao => FormatarDuracao(HorasTrabalhadas);
    public string JornadaPrevistaDescricao => FormatarDuracao(JornadaPrevista);
    public string SaldoHorasDescricao =>
        $"{(SaldoHoras < TimeSpan.Zero ? "-" : "+")}{FormatarDuracao(SaldoHoras.Duration())}";

    private static string FormatarDuracao(TimeSpan duracao) =>
        $"{(int)duracao.TotalHours:00}:{duracao.Minutes:00}:{duracao.Seconds:00}";
}
