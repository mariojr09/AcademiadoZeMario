// Mario Cesar Alves Júnior

using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Enums;
using AcademiadoZE.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class ColaboradorInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly ColaboradorRepository _colaboradorRepo;

    public ColaboradorInfrastructureTests()
    {
        _logradouroRepo =
            new LogradouroRepository(ConnectionString, DatabaseType);

        _colaboradorRepo =
            new ColaboradorRepository(ConnectionString, DatabaseType);
    }

    private async Task<Colaborador> CriarColaboradorAsync()
    {
        var logradouro =
     await CriarEInserirLogradouroAsync();

        var foto =
            Arquivo.Criar(
                new byte[] { 1, 2, 3, 4 }
            ).Value!;

        var resultado = Colaborador.Criar(
            id: 0,

            // Requisito do professor: seu nome
            nome: "Mario Cesar Alves Junior",

            cpf: GerarCpf(),

            dataNascimento:
                new DateOnly(1990, 5, 10),

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
                foto,

            dataAdmissao:
                new DateOnly(2024, 1, 10),

            tipo:
                ColaboradorTipo.Instrutor,

            vinculo:
                ColaboradorVinculo.Clt
        );

        Assert.True(
            resultado.IsSuccess,
            string.Join(
                ", ",
                resultado.Notifications
                    .Select(n => n.Mensagem)));

        var colaborador =
            resultado.Value!;

        await _colaboradorRepo
            .AdicionarAsync(colaborador);

        return colaborador;
    }

    [Fact]
    public async Task Colaborador_Adicionar_E_ObterPorCpf_Sucesso()
    {
        var colaborador =
            await CriarColaboradorAsync();

        var obtido =
            await _colaboradorRepo
                .ObterPorCpfAsync(colaborador.Cpf);

        Assert.NotNull(obtido);

        Assert.Equal(
            colaborador.Cpf.Valor,
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
    public async Task Colaborador_ObterPorId_Sucesso()
    {
        var colaborador =
            await CriarColaboradorAsync();

        var inserido =
            await _colaboradorRepo
                .ObterPorCpfAsync(colaborador.Cpf);

        Assert.NotNull(inserido);
        Assert.True(inserido.Id > 0);

        var obtido =
            await _colaboradorRepo
                .ObterPorIdAsync(inserido.Id);

        Assert.NotNull(obtido);

        Assert.Equal(
            inserido.Id,
            obtido.Id);
    }

    [Fact]
    public async Task Colaborador_ObterPorId_Inexistente_DeveRetornarNull()
    {
        var obtido =
            await _colaboradorRepo
                .ObterPorIdAsync(999999);

        Assert.Null(obtido);
    }

    [Fact]
    public async Task Colaborador_Listar_Sucesso()
    {
        var colaborador =
            await CriarColaboradorAsync();

        var colaboradores =
            await _colaboradorRepo
                .ListarAsync();

        Assert.NotNull(colaboradores);
        Assert.NotEmpty(colaboradores);

        Assert.Contains(
            colaboradores,
            c => c.Cpf.Valor == colaborador.Cpf.Valor);
    }

    [Fact]
    public async Task Colaborador_ObterPorCpf_Sucesso()
    {
        var colaborador =
            await CriarColaboradorAsync();

        var obtido =
            await _colaboradorRepo
                .ObterPorCpfAsync(colaborador.Cpf);

        Assert.NotNull(obtido);

        Assert.Equal(
            colaborador.Cpf.Valor,
            obtido.Cpf.Valor);
    }

    [Fact]
    public async Task Colaborador_ObterPorEmail_Sucesso()
    {
        var colaborador =
            await CriarColaboradorAsync();

        var obtido =
            await _colaboradorRepo
                .ObterPorEmailAsync(colaborador.Email);

        Assert.NotNull(obtido);

        Assert.Equal(
            colaborador.Email.Valor,
            obtido.Email.Valor);
    }

    [Fact]
    public async Task Colaborador_ExisteCpf_DeveRetornarTrue()
    {
        var colaborador =
            await CriarColaboradorAsync();

        var existe =
            await _colaboradorRepo
                .ExisteCpfAsync(colaborador.Cpf);

        Assert.True(existe);
    }

    [Fact]
    public async Task Colaborador_ExisteEmail_DeveRetornarTrue()
    {
        var colaborador =
            await CriarColaboradorAsync();

        var existe =
            await _colaboradorRepo
                .ExisteEmailAsync(colaborador.Email);

        Assert.True(existe);
    }

    [Fact]
    public async Task Colaborador_Atualizar_Sucesso()
    {
        var colaborador =
            await CriarColaboradorAsync();

        var inserido =
            await _colaboradorRepo
                .ObterPorCpfAsync(colaborador.Cpf);

        Assert.NotNull(inserido);

        var atualizadoResult =
            Colaborador.Criar(
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
                    inserido.Foto,

                dataAdmissao:
                    inserido.DataAdmissao,

                tipo:
                    ColaboradorTipo.Administrador,

                vinculo:
                    ColaboradorVinculo.Clt
            );

        Assert.True(atualizadoResult.IsSuccess);

        var atualizado =
            atualizadoResult.Value!;

        _colaboradorRepo.Atualizar(atualizado);

        var noBanco =
            await _colaboradorRepo
                .ObterPorIdAsync(inserido.Id);

        Assert.NotNull(noBanco);

        Assert.Equal(
            "200",
            noBanco.Endereco.Numero);

        Assert.Equal(
            ColaboradorTipo.Administrador,
            noBanco.Tipo);

        Assert.Equal(
            "NovaSenhaMySQL123",
            noBanco.Senha.Hash);
    }

    [Fact]
    public async Task Colaborador_Remover_Sucesso()
    {
        var colaborador =
            await CriarColaboradorAsync();

        var inserido =
            await _colaboradorRepo
                .ObterPorCpfAsync(colaborador.Cpf);

        Assert.NotNull(inserido);

        _colaboradorRepo.Remover(inserido);

        var noBanco =
            await _colaboradorRepo
                .ObterPorIdAsync(inserido.Id);

        Assert.Null(noBanco);
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
}