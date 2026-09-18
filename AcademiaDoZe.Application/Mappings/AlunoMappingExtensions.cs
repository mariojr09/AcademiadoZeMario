using AcademiaDoZe.Application.DTOs;
using AcademiadoZE.Domain.Entities;

namespace AcademiaDoZe.Application.Mappings;
// Mario Cesar Alves Júnior
public static class AlunoMappingExtensions
{
    public static AlunoDto ToDto(this Aluno aluno)
    {
        return new AlunoDto
        {
            Id = aluno.Id,
            Nome = aluno.Nome,
            Cpf = aluno.Cpf.Valor,
            DataNascimento = aluno.DataNascimento,
            Telefone = aluno.Telefone.Valor,
            Email = aluno.Email.Valor,

            Endereco = aluno.Endereco.Logradouro.ToDto(),
            Numero = aluno.Endereco.Numero,
            Complemento = aluno.Endereco.Complemento,

            Foto = aluno.Foto is null
                ? null
                : new ArquivoDto
                {
                    Conteudo = aluno.Foto.Conteudo
                },

            Senha = null,
            Responsavel = null
        };
    }
}