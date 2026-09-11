// Mario Cesar Alves Júnior

using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Enums;
using AcademiadoZE.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class MatriculaInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly AlunoRepository _alunoRepo;
    private readonly MatriculaRepository _matriculaRepo;

    public MatriculaInfrastructureTests()
    {
        _logradouroRepo =
            new LogradouroRepository(
                ConnectionString,
                DatabaseType);

        _alunoRepo =
            new AlunoRepository(
                ConnectionString,
                DatabaseType);

        _matriculaRepo =
            new MatriculaRepository(
                ConnectionString,
                DatabaseType);
    }

    private static MatriculaPlano ObterPlanoValido()
    {
        return Enum
            .GetValues<MatriculaPlano>()
            .First();
    }

    private async Task<Logradouro> CriarEInserirLogradouroAsync()
    {
        var cep = GerarCep();

        var result =
            Logradouro.Criar(
                0,
                cep,
                "Brasil",
                "SC",
                "Lages",
                "Alves",
                "Mario");

        Assert.True(result.IsSuccess);

        await _logradouroRepo
            .AdicionarAsync(result.Value!);

        var cepResult =
            Cep.Criar(cep);

        Assert.True(cepResult.IsSuccess);

        var encontrados =
            await _logradouroRepo
                .ListarPorCepAsync(
                    cepResult.Value!);

        return encontrados.Single();
    }

    private async Task<Aluno> CriarEInserirAlunoAsync()
    {
        var logradouro =
            await CriarEInserirLogradouroAsync();

        var foto =
            Arquivo.Criar(
                new byte[] { 1, 2, 3, 4 }
            ).Value!;

        var resultado =
            Aluno.Criar(
                id: 0,

                nome:
                    "Mario Cesar Alves Junior",

                cpf:
                    GerarCpf(),

                dataNascimento:
                    new DateOnly(2000, 5, 10),

                telefone:
                    GerarTelefone(),

                email:
                    GerarEmail(),

                logradouro:
                    logradouro,

                numero:
                    "100",

                complemento:
                    "Alves Junior",

                senha:
                    "SenhaMySQL123",

                foto:
                    foto);

        Assert.True(
            resultado.IsSuccess,
            string.Join(
                ", ",
                resultado.Notifications
                    .Select(n => n.Mensagem)));

        await _alunoRepo
            .AdicionarAsync(
                resultado.Value!);

        var alunoInserido =
            await _alunoRepo
                .ObterPorCpfAsync(
                    resultado.Value!.Cpf);

        Assert.NotNull(alunoInserido);
        Assert.True(alunoInserido.Id > 0);

        return alunoInserido;
    }

    private async Task<Matricula> CriarEInserirMatriculaAsync()
    {
        var aluno =
            await CriarEInserirAlunoAsync();

        var laudo =
            Arquivo.Criar(
                new byte[] { 10, 20, 30, 40 }
            ).Value!;

        var resultado =
            Matricula.Criar(
                id: 0,

                aluno:
                    aluno,

                plano:
                    ObterPlanoValido(),

                dataInicio:
                    new DateOnly(2026, 1, 1),

                dataFinal:
                    new DateOnly(2026, 12, 31),

                // requisito da atividade
                objetivo:
                    "Mario Cesar Alves Junior",

                restricoes:
                    Restricao.Diabetes |
                    Restricao.Alergias,

                // requisito da atividade
                observacoesRestricoes:
                    "MySQL - teste de matrícula",

                laudoMedico:
                    laudo);

        Assert.True(
            resultado.IsSuccess,
            string.Join(
                ", ",
                resultado.Notifications
                    .Select(n => n.Mensagem)));

        await _matriculaRepo
            .AdicionarAsync(
                resultado.Value!);

        var matriculas =
            await _matriculaRepo
                .ListarPorAlunoAsync(
                    aluno.Id);

        var inserida =
            matriculas.Single();

        Assert.True(inserida.Id > 0);

        return inserida;
    }

    [Fact]
    public async Task Matricula_Adicionar_E_ObterPorId_Sucesso()
    {
        var matricula =
            await CriarEInserirMatriculaAsync();

        var obtida =
            await _matriculaRepo
                .ObterPorIdAsync(
                    matricula.Id);

        Assert.NotNull(obtida);

        Assert.Equal(
            matricula.Id,
            obtida.Id);

        Assert.Equal(
            "Mario Cesar Alves Junior",
            obtida.Objetivo);

        Assert.Contains(
            "MySQL",
            obtida.ObservacoesRestricoes!);

        Assert.Equal(
            Restricao.Diabetes |
            Restricao.Alergias,
            obtida.Restricoes);

        Assert.NotNull(
            obtida.LaudoMedico);
    }

    [Fact]
    public async Task Matricula_ObterPorId_Inexistente_DeveRetornarNull()
    {
        var obtida =
            await _matriculaRepo
                .ObterPorIdAsync(
                    999999);

        Assert.Null(obtida);
    }

    [Fact]
    public async Task Matricula_Listar_Sucesso()
    {
        var matricula =
            await CriarEInserirMatriculaAsync();

        var matriculas =
            await _matriculaRepo
                .ListarAsync();

        Assert.NotNull(matriculas);
        Assert.NotEmpty(matriculas);

        Assert.Contains(
            matriculas,
            x => x.Id == matricula.Id);
    }

    [Fact]
    public async Task Matricula_ListarPorAluno_Sucesso()
    {
        var matricula =
            await CriarEInserirMatriculaAsync();

        var matriculas =
            await _matriculaRepo
                .ListarPorAlunoAsync(
                    matricula.Aluno.Id);

        Assert.NotNull(matriculas);
        Assert.NotEmpty(matriculas);

        Assert.Contains(
            matriculas,
            x => x.Id == matricula.Id);

        Assert.All(
            matriculas,
            x => Assert.Equal(
                matricula.Aluno.Id,
                x.Aluno.Id));
    }

    [Fact]
    public async Task Matricula_ObterVigentePorAluno_Sucesso()
    {
        var matricula =
            await CriarEInserirMatriculaAsync();

        var vigente =
            await _matriculaRepo
                .ObterVigentePorAlunoAsync(
                    matricula.Aluno.Id,
                    new DateOnly(
                        2026,
                        6,
                        15));

        Assert.NotNull(vigente);

        Assert.Equal(
            matricula.Id,
            vigente.Id);
    }

    [Fact]
    public async Task Matricula_ObterVigentePorAluno_ForaDoPeriodo_DeveRetornarNull()
    {
        var matricula =
            await CriarEInserirMatriculaAsync();

        var vigente =
            await _matriculaRepo
                .ObterVigentePorAlunoAsync(
                    matricula.Aluno.Id,
                    new DateOnly(
                        2027,
                        1,
                        1));

        Assert.Null(vigente);
    }

    [Fact]
    public async Task Matricula_Atualizar_Sucesso()
    {
        var matricula =
            await CriarEInserirMatriculaAsync();

        var atualizadoResult =
            Matricula.Criar(
                id:
                    matricula.Id,

                aluno:
                    matricula.Aluno,

                plano:
                    matricula.Plano,

                dataInicio:
                    matricula.DataInicio,

                dataFinal:
                    matricula.DataFinal,

                objetivo:
                    "Mario Cesar Alves Junior - Atualizado",

                restricoes:
                    matricula.Restricoes,

                observacoesRestricoes:
                    "MySQL - matrícula atualizada",

                laudoMedico:
                    matricula.LaudoMedico);

        Assert.True(
            atualizadoResult.IsSuccess,
            string.Join(
                ", ",
                atualizadoResult.Notifications
                    .Select(n => n.Mensagem)));

        _matriculaRepo
            .Atualizar(
                atualizadoResult.Value!);

        var noBanco =
            await _matriculaRepo
                .ObterPorIdAsync(
                    matricula.Id);

        Assert.NotNull(noBanco);

        Assert.Equal(
            "Mario Cesar Alves Junior - Atualizado",
            noBanco.Objetivo);

        Assert.Equal(
            "MySQL - matrícula atualizada",
            noBanco.ObservacoesRestricoes);
    }

    [Fact]
    public async Task Matricula_Remover_Sucesso()
    {
        var matricula =
            await CriarEInserirMatriculaAsync();

        _matriculaRepo
            .Remover(
                matricula);

        var noBanco =
            await _matriculaRepo
                .ObterPorIdAsync(
                    matricula.Id);

        Assert.Null(noBanco);
    }
}