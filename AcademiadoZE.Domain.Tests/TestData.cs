// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Enums;
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiadoZE.Domain.Tests;

internal static class TestData
{
    internal static Logradouro LogradouroValido(int id = 1) =>
        Logradouro.Criar(id, "01001-000", "Brasil", "SP", "São Paulo", "Sé", "Praça da Sé").Value!;

    internal static Arquivo ArquivoValido() => Arquivo.Criar("laudo.pdf", "/docs/laudo.pdf").Value!;

    internal static Aluno AlunoValido(int id = 1) =>
        Aluno.Criar(id, "Maria Silva", "529.982.247-25", new DateOnly(2000, 1, 1),
            "11987654321", "maria@example.com", LogradouroValido(), "100", null,
            "hash-calculado", null).Value!;

    internal static Colaborador ColaboradorValido(int id = 1) =>
        Colaborador.Criar(id, "João Silva", "111.444.777-35", new DateOnly(1990, 1, 1),
            "11987654321", "joao@example.com", LogradouroValido(), "100", null,
            "hash-calculado", null, new DateOnly(2020, 1, 1), ColaboradorTipo.Instrutor,
            ColaboradorVinculo.Clt).Value!;
}
