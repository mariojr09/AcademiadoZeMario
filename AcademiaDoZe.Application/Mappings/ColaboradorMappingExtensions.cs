using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiadoZE.Domain.Entities;

namespace AcademiaDoZe.Application.Mappings;
// Mario Cesar Alves Júnior
public static class ColaboradorMappingExtensions
{
    public static ColaboradorDto ToDto(this Colaborador colaborador)
    {
        return new ColaboradorDto
        {
            Id = colaborador.Id,
            Nome = colaborador.Nome,
            Cpf = colaborador.Cpf.Valor,
            DataNascimento = colaborador.DataNascimento,
            Telefone = colaborador.Telefone.Valor,
            Email = colaborador.Email.Valor,

            Endereco = colaborador.Endereco.Logradouro.ToDto(),
            Numero = colaborador.Endereco.Numero,
            Complemento = colaborador.Endereco.Complemento,

            Foto = colaborador.Foto is null
                ? null
                : new ArquivoDto
                {
                    Conteudo = colaborador.Foto.Conteudo
                },

            Senha = null,

            DataAdmissao = colaborador.DataAdmissao,
            Tipo = (AppColaboradorTipo)(int)colaborador.Tipo,
            Vinculo = (AppColaboradorVinculo)(int)colaborador.Vinculo
        };
    }
}