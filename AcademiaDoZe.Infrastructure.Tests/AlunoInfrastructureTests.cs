// Mario Cesar Alves Júnior

using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class AlunoInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly AlunoRepository _alunoRepo;

    public AlunoInfrastructureTests()
    {
        _logradouroRepo =
            new LogradouroRepository(ConnectionString, DatabaseType);

        _alunoRepo =
            new AlunoRepository(ConnectionString, DatabaseType);
    }

    private async Task<Logradouro> CriarEInserirLogradouroAsync()
    {
        var cep = GerarCep();

        var result = Logradouro.Criar(
            0,
            cep,
            "Brasil",
            "SC",
            "Lages",
            "Alves",
            "Mario");

        Assert.True(result.IsSuccess);

        await _logradouroRepo.AdicionarAsync(result.Value!);

        var cepResult = Cep.Criar(cep);

        Assert.True(cepResult.IsSuccess);

        var encontrados =
            await _logradouroRepo.ListarPorCepAsync(cepResult.Value!);

        return encontrados.Single();
    }

    private async Task<Aluno> CriarAlunoAsync()
    {
        var logradouro =
            await CriarEInserirLogradouroAsync();

        var foto =
            Arquivo.Criar(
                new byte[] { 1, 2, 3, 4 }
            ).Value!;

        var resultado = Aluno.Criar(
            id: 0,

            // requisito: seu nome
            nome: "Mario Cesar Alves Junior",

            cpf: GerarCpf(),

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

            // requisito: sobrenome
            complemento:
                "Alves Junior",

            // requisito: conter o SGBD
            senha:
                "SenhaMySQL123",

            foto:
                foto
        );

        Assert.True(
            resultado.IsSuccess,
            string.Join(
                ", ",
                resultado.Notifications
                    .Select(n => n.Mensagem)));

        var aluno =
            resultado.Value!;

        await _alunoRepo.AdicionarAsync(aluno);

        return aluno;
    }

    [Fact]
    public async Task Aluno_Adicionar_E_ObterPorCpf_Sucesso()
    {
        var aluno =
            await CriarAlunoAsync();

        var obtido =
            await _alunoRepo
                .ObterPorCpfAsync(aluno.Cpf);

        Assert.NotNull(obtido);

        Assert.Equal(
            aluno.Cpf.Valor,
            obtido.Cpf.Valor);

        Assert.Equal(
            "Mario Cesar Alves Junior",
            obtido.Nome);

        Assert.Equal(
            "Alves Junior",
            obtido.Endereco.Complemento);

        Assert.Equal(
            "SenhaMySQL123",
            obtido.Senha.Hash);
    }

    [Fact]
    public async Task Aluno_ObterPorId_Sucesso()
    {
        var aluno =
            await CriarAlunoAsync();

        var inserido =
            await _alunoRepo
                .ObterPorCpfAsync(aluno.Cpf);

        Assert.NotNull(inserido);
        Assert.True(inserido.Id > 0);

        var obtido =
            await _alunoRepo
                .ObterPorIdAsync(inserido.Id);

        Assert.NotNull(obtido);

        Assert.Equal(
            inserido.Id,
            obtido.Id);
    }

    [Fact]
    public async Task Aluno_ObterPorId_Inexistente_DeveRetornarNull()
    {
        var obtido =
            await _alunoRepo
                .ObterPorIdAsync(999999);

        Assert.Null(obtido);
    }

    [Fact]
    public async Task Aluno_Listar_Sucesso()
    {
        var aluno =
            await CriarAlunoAsync();

        var alunos =
            await _alunoRepo
                .ListarAsync();

        Assert.NotNull(alunos);
        Assert.NotEmpty(alunos);

        Assert.Contains(
            alunos,
            x => x.Cpf.Valor == aluno.Cpf.Valor);
    }

    [Fact]
    public async Task Aluno_ObterPorCpf_Sucesso()
    {
        var aluno =
            await CriarAlunoAsync();

        var obtido =
            await _alunoRepo
                .ObterPorCpfAsync(aluno.Cpf);

        Assert.NotNull(obtido);

        Assert.Equal(
            aluno.Cpf.Valor,
            obtido.Cpf.Valor);
    }

    [Fact]
    public async Task Aluno_ObterPorEmail_Sucesso()
    {
        var aluno =
            await CriarAlunoAsync();

        var obtido =
            await _alunoRepo
                .ObterPorEmailAsync(aluno.Email);

        Assert.NotNull(obtido);

        Assert.Equal(
            aluno.Email.Valor,
            obtido.Email.Valor);
    }

    [Fact]
    public async Task Aluno_ExisteCpf_DeveRetornarTrue()
    {
        var aluno =
            await CriarAlunoAsync();

        var existe =
            await _alunoRepo
                .ExisteCpfAsync(aluno.Cpf);

        Assert.True(existe);
    }

    [Fact]
    public async Task Aluno_ExisteEmail_DeveRetornarTrue()
    {
        var aluno =
            await CriarAlunoAsync();

        var existe =
            await _alunoRepo
                .ExisteEmailAsync(aluno.Email);

        Assert.True(existe);
    }

    [Fact]
    public async Task Aluno_Atualizar_Sucesso()
    {
        var aluno =
            await CriarAlunoAsync();

        var inserido =
            await _alunoRepo
                .ObterPorCpfAsync(aluno.Cpf);

        Assert.NotNull(inserido);

        var atualizadoResult =
            Aluno.Criar(
                id: inserido.Id,

                nome:
                    "Mario Cesar Alves Junior",

                cpf:
                    inserido.Cpf.Valor,

                dataNascimento:
                    inserido.DataNascimento,

                telefone:
                    inserido.Telefone.Valor,

                email:
                    inserido.Email.Valor,

                logradouro:
                    inserido.Endereco.Logradouro,

                numero:
                    "200",

                complemento:
                    "Alves Junior",

                senha:
                    "NovaSenhaMySQL123",

                foto:
                    inserido.Foto
            );

        Assert.True(atualizadoResult.IsSuccess);

        var atualizado =
            atualizadoResult.Value!;

        _alunoRepo.Atualizar(atualizado);

        var noBanco =
            await _alunoRepo
                .ObterPorIdAsync(inserido.Id);

        Assert.NotNull(noBanco);

        Assert.Equal(
            "200",
            noBanco.Endereco.Numero);

        Assert.Equal(
            "NovaSenhaMySQL123",
            noBanco.Senha.Hash);
    }

    [Fact]
    public async Task Aluno_Remover_Sucesso()
    {
        var aluno =
            await CriarAlunoAsync();

        var inserido =
            await _alunoRepo
                .ObterPorCpfAsync(aluno.Cpf);

        Assert.NotNull(inserido);

        _alunoRepo.Remover(inserido);

        var noBanco =
            await _alunoRepo
                .ObterPorIdAsync(inserido.Id);

        Assert.Null(noBanco);
    }
}