using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Application.DTOs;
// Mario Cesar Alves Júnior
public class MatriculaDto
{
    public int Id { get; set; }

    public int AlunoId { get; set; }

    public AppMatriculaPlano Plano { get; set; }

    public DateOnly DataInicio { get; set; }

    public DateOnly DataFinal { get; set; }

    public required string Objetivo { get; set; }

    public AppMatriculaRestricoes Restricoes { get; set; }

    public string? ObservacoesRestricoes { get; set; }

    public ArquivoDto? LaudoMedico { get; set; }
}