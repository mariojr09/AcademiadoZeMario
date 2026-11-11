using AcademiadoZE.Domain.Enums;

namespace AcademiaDoZe.Application.DTOs;

public class AcessoPessoaDto
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public ArquivoDto? Foto { get; init; }
    public AcessoPessoaTipo Tipo { get; init; }
    public ColaboradorVinculo? Vinculo { get; init; }
    public TimeSpan? JornadaDiaria { get; init; }

    public string Descricao => $"Cadastro #{Id} - {Nome} - CPF {Cpf}";
    public string CadastroDescricao => $"Cadastro #{Id}";
    public string DetalhesProfissionais => Vinculo.HasValue && JornadaDiaria.HasValue
        ? $"{(Vinculo == ColaboradorVinculo.Clt ? "CLT" : "Estágio")} - jornada de {(int)JornadaDiaria.Value.TotalHours}h/dia"
        : string.Empty;
}
