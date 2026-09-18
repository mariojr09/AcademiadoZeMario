using AcademiaDoZe.Application.DTOs;
using AcademiadoZE.Domain.Entities;

namespace AcademiaDoZe.Application.Mappings;
// Mario Cesar Alves Júnior
public static class LogradouroMappingExtensions
{
    public static LogradouroDto ToDto(this Logradouro logradouro)
    {
        return new LogradouroDto
        {
            Id = logradouro.Id,
            Cep = logradouro.Cep.Valor,
            Nome = logradouro.NomeLogradouro,
            Bairro = logradouro.Bairro,
            Cidade = logradouro.Cidade,
            Estado = logradouro.Estado,
            Pais = logradouro.Pais
        };
    }
}