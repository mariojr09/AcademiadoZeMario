using AcademiadoZE.Domain.Enums;

namespace AcademiaDoZe.Application.DTOs;

public class AcessoRegistroDto
{
    public int Id { get; init; }
    public int PessoaId { get; init; }
    public string Nome { get; init; } = string.Empty;
    public ArquivoDto? Foto { get; init; }
    public AcessoPessoaTipo Tipo { get; init; }
    public DateTime DataHoraChegada { get; init; }
    public DateTime? DataHoraSaida { get; init; }
    public ColaboradorVinculo? Vinculo { get; init; }

    public bool EstaAberto => DataHoraSaida is null;
    public string CadastroDescricao => $"Cadastro #{PessoaId}";
    public string TipoDescricao => Tipo == AcessoPessoaTipo.Aluno ? "Aluno" : "Colaborador";
    public string VinculoDescricao => Vinculo.HasValue
        ? Vinculo == ColaboradorVinculo.Clt ? "CLT" : "Estágio"
        : string.Empty;
    public string TipoEVinculoDescricao => string.IsNullOrEmpty(VinculoDescricao)
        ? TipoDescricao
        : $"{TipoDescricao} - {VinculoDescricao}";
    public string HorarioDescricao => DataHoraSaida.HasValue
        ? $"{DataHoraChegada:HH:mm:ss} - {DataHoraSaida:HH:mm:ss}"
        : $"Entrada às {DataHoraChegada:HH:mm:ss} - dentro da academia";
    public string PermanenciaDescricao => DataHoraSaida.HasValue
        ? $"Permanência: {FormatarDuracao(DataHoraSaida.Value - DataHoraChegada)}"
        : string.Empty;

    private static string FormatarDuracao(TimeSpan duracao) =>
        $"{(int)duracao.TotalHours:00}:{duracao.Minutes:00}:{duracao.Seconds:00}";
}
