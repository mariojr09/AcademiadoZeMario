using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class LogradouroInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _repository;

    public LogradouroInfrastructureTests()
    {
        _repository = new LogradouroRepository(
            ConnectionString,
            DatabaseType);
    }

    [Fact]
    public async Task Logradouro_Adicionar_E_Listar_Sucesso()
    {
        var cep = GerarCep();

        var result = Logradouro.Criar(
            0,
            cep,
            "Brasil",
            "SC",
            "SQLite",
            "Alves",
            "Mario");

        Assert.True(result.IsSuccess);

        await _repository.AdicionarAsync(result.Value!);

        var lista = await _repository.ListarAsync();

        Assert.NotNull(lista);
        Assert.NotEmpty(lista);
    }
    [Fact]
    public async Task Logradouro_ObterPorId_Sucesso()
    {
        var cep = GerarCep();

        var result = Logradouro.Criar(
            0,
            cep,
            "Brasil",
            "SC",
            "SQLite",
            "Alves",
            "Mario");

        Assert.True(result.IsSuccess);

        await _repository.AdicionarAsync(result.Value!);

        var cepResult = Cep.Criar(cep);
        Assert.True(cepResult.IsSuccess);

        var encontrados = await _repository.ListarPorCepAsync(cepResult.Value!);
        var inserido = encontrados.Single();

        var obtido = await _repository.ObterPorIdAsync(inserido.Id);

        Assert.NotNull(obtido);
        Assert.Equal(inserido.Id, obtido.Id);
        Assert.Equal(cep, obtido.Cep.Valor);
    }
    [Fact]
    public async Task Logradouro_ListarPorCep_Sucesso()
    {
        var cep = GerarCep();

        var result = Logradouro.Criar(
            0,
            cep,
            "Brasil",
            "SC",
            "SQLite",
            "Alves",
            "Mario");

        Assert.True(result.IsSuccess);

        await _repository.AdicionarAsync(result.Value!);

        var cepResult = Cep.Criar(cep);

        Assert.True(cepResult.IsSuccess);

        var encontrados = await _repository.ListarPorCepAsync(cepResult.Value!);

        Assert.NotEmpty(encontrados);
        Assert.Contains(encontrados, x => x.Cep.Valor == cep);
    }
    [Fact]
    public async Task Logradouro_Atualizar_Sucesso()
    {
        var cep = GerarCep();

        var result = Logradouro.Criar(
            0,
            cep,
            "Brasil",
            "SC",
            "SQLite",
            "Alves",
            "Mario");

        Assert.True(result.IsSuccess);

        await _repository.AdicionarAsync(result.Value!);

        var lista = await _repository.ListarAsync();
        var inserido = lista.Last();

        var atualizadoResult = Logradouro.Criar(
            inserido.Id,
            inserido.Cep.Valor,
            "Brasil",
            "SC",
            "SQLite",
            "Alves",
            "Mario Atualizado");

        Assert.True(atualizadoResult.IsSuccess);

        _repository.Atualizar(atualizadoResult.Value!);

        var obtido = await _repository.ObterPorIdAsync(inserido.Id);

        Assert.NotNull(obtido);
        Assert.Equal("Mario Atualizado", obtido.NomeLogradouro);
    }
    [Fact]
    public async Task Logradouro_Remover_Sucesso()
    {
        var cep = GerarCep();

        var result = Logradouro.Criar(
            0,
            cep,
            "Brasil",
            "SC",
            "SQLite",
            "Alves",
            "Mario");

        Assert.True(result.IsSuccess);

        await _repository.AdicionarAsync(result.Value!);

        var lista = await _repository.ListarAsync();
        var inserido = lista.Last();

        _repository.Remover(inserido);

        var obtido = await _repository.ObterPorIdAsync(inserido.Id);

        Assert.Null(obtido);
    }
}