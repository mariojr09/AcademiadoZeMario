using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiadoZE.Domain.Entities;

namespace AcademiaDoZe.Application.Mappings;
// Mario Cesar Alves Júnior
public static class MatriculaMappingExtensions
{
    public static MatriculaDto ToDto(this Matricula matricula)
    {
        return new MatriculaDto
        {
            Id = matricula.Id,
            AlunoId = matricula.Aluno.Id,
            Plano = (AppMatriculaPlano)(int)matricula.Plano,
            DataInicio = matricula.DataInicio,
            DataFinal = matricula.DataFinal,
            Objetivo = matricula.Objetivo,
            Restricoes = (AppMatriculaRestricoes)(int)matricula.Restricoes,
            ObservacoesRestricoes = matricula.ObservacoesRestricoes,

            LaudoMedico = matricula.LaudoMedico is null
                ? null
                : new ArquivoDto
                {
                    Conteudo = matricula.LaudoMedico.Conteudo
                }
        };
    }
}