using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Application.DTOs;
// Mario Cesar Alves Júnior
public class ColaboradorDto : PessoaDto
{
    public DateOnly DataAdmissao { get; set; }

    public AppColaboradorTipo Tipo { get; set; }

    public AppColaboradorVinculo Vinculo { get; set; }
}