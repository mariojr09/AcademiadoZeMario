// Mario Cesar Alves Júnior

using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Enums;
using AcademiadoZE.Domain.ValueObjects;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class AcessoInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepository;
    private readonly AlunoRepository _alunoRepository;
    private readonly ColaboradorRepository _colaboradorRepository;
    private readonly MatriculaRepository _matriculaRepository;
    private readonly AcessoAlunoRepository _acessoAlunoRepository;
    private readonly AcessoColaboradorRepository _acessoColaboradorRepository;
    private readonly AcessoService _service;

    public AcessoInfrastructureTests()
    {
        _logradouroRepository = new(ConnectionString, DatabaseType);
        _alunoRepository = new(ConnectionString, DatabaseType);
        _colaboradorRepository = new(ConnectionString, DatabaseType);
        _matriculaRepository = new(ConnectionString, DatabaseType);
        _acessoAlunoRepository = new(ConnectionString, DatabaseType);
        _acessoColaboradorRepository = new(ConnectionString, DatabaseType);
        _service = new(
            _alunoRepository,
            _colaboradorRepository,
            _matriculaRepository,
            _acessoAlunoRepository,
            _acessoColaboradorRepository);
    }

    [Fact]
    public async Task AlunoSemMatricula_DeveTerEntradaNegada()
    {
        var aluno = await CriarAlunoAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegistrarEntradaAlunoAsync(aluno.Id, DateTime.Today.AddHours(9)));

        Assert.Equal("ALUNO_SEM_MATRICULA_ATIVA", exception.Message);
        Assert.Null(await _acessoAlunoRepository.ObterAbertoPorAlunoAsync(aluno.Id));
    }

    [Fact]
    public async Task AlunoMatriculado_DevePersistirEntradaESaidaNaTbAcesso()
    {
        var aluno = await CriarAlunoAsync();
        await CriarMatriculaAtivaAsync(aluno);
        var chegada = DateTime.Today.AddHours(10);
        var saida = chegada.AddHours(1).AddMinutes(30);

        var entrada = await _service.RegistrarEntradaAlunoAsync(aluno.Id, chegada);
        var aberto = await _acessoAlunoRepository.ObterAbertoPorAlunoAsync(aluno.Id);

        Assert.NotNull(aberto);
        Assert.True(aberto.Id > 0);
        Assert.Contains("Restam", entrada.Mensagem);

        var duplicada = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegistrarEntradaAlunoAsync(aluno.Id, chegada.AddMinutes(5)));
        Assert.Equal("ALUNO_JA_ESTA_NA_ACADEMIA", duplicada.Message);

        var resultadoSaida = await _service.RegistrarSaidaAlunoAsync(aluno.Id, saida);
        var persistido = await _acessoAlunoRepository.ObterPorIdAsync(aberto.Id);

        Assert.NotNull(persistido);
        Assert.Equal(saida, persistido.DataHoraSaida);
        Assert.Equal(TimeSpan.FromMinutes(90), persistido.TempoPermanencia);
        Assert.Contains("01:30:00", resultadoSaida.Mensagem);
    }

    [Fact]
    public async Task ColaboradorComNomeDoAutor_DeveSomarAcessosERespeitarJornadaClt()
    {
        var colaborador = await CriarColaboradorAsync(ColaboradorVinculo.Clt);
        var primeiraChegada = DateTime.Today.AddHours(8);

        await _service.RegistrarEntradaColaboradorAsync(colaborador.Id, primeiraChegada);

        var duplicada = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegistrarEntradaColaboradorAsync(colaborador.Id, primeiraChegada.AddMinutes(10)));
        Assert.Equal("COLABORADOR_JA_ESTA_NA_ACADEMIA", duplicada.Message);

        await _service.RegistrarSaidaColaboradorAsync(colaborador.Id, primeiraChegada.AddHours(2));
        await _service.RegistrarEntradaColaboradorAsync(colaborador.Id, primeiraChegada.AddHours(3));
        var saida = await _service.RegistrarSaidaColaboradorAsync(colaborador.Id, primeiraChegada.AddHours(5));

        var resumo = await _service.ObterResumoDiarioColaboradorAsync(
            colaborador.Id,
            primeiraChegada.AddHours(5));
        var persistidos = await _acessoColaboradorRepository.ListarPorColaboradorAsync(colaborador.Id);

        Assert.Equal("Mario Cesar Alves Junior", colaborador.Nome);
        Assert.Equal(2, persistidos.Count);
        Assert.All(persistidos, acesso => Assert.NotNull(acesso.DataHoraSaida));
        Assert.Equal(TimeSpan.FromHours(4), resumo.HorasTrabalhadas);
        Assert.Equal(TimeSpan.FromHours(8), resumo.JornadaPrevista);
        Assert.Equal(TimeSpan.FromHours(-4), resumo.SaldoHoras);
        Assert.Contains("total do dia: 04:00:00", saida.Mensagem);
    }

    [Fact]
    public async Task ColaboradorEstagiario_DeveTerJornadaDeSeisHoras()
    {
        var colaborador = await CriarColaboradorAsync(ColaboradorVinculo.Estagio);

        var resumo = await _service.ObterResumoDiarioColaboradorAsync(
            colaborador.Id,
            DateTime.Today.AddHours(8));

        Assert.Equal(ColaboradorVinculo.Estagio, resumo.Vinculo);
        Assert.Equal(TimeSpan.FromHours(6), resumo.JornadaPrevista);
    }

    private async Task<Logradouro> CriarLogradouroAsync()
    {
        string cep = GerarCep();
        var resultado = Logradouro.Criar(0, cep, "Brasil", "SC", "Lages", "Centro", "Rua do Autor");
        Assert.True(resultado.IsSuccess);
        await _logradouroRepository.AdicionarAsync(resultado.Value!);
        var cepResult = Cep.Criar(cep);
        return (await _logradouroRepository.ListarPorCepAsync(cepResult.Value!)).Single();
    }

    private async Task<Aluno> CriarAlunoAsync()
    {
        var resultado = Aluno.Criar(
            0,
            "Mario Cesar Alves Junior",
            GerarCpf(),
            new DateOnly(2000, 5, 10),
            GerarTelefone(),
            GerarEmail(),
            await CriarLogradouroAsync(),
            "100",
            "Alves Junior",
            "SenhaMySQL123",
            Arquivo.Criar([1, 2, 3, 4]).Value);

        Assert.True(resultado.IsSuccess);
        await _alunoRepository.AdicionarAsync(resultado.Value!);
        return (await _alunoRepository.ObterPorCpfAsync(resultado.Value!.Cpf))!;
    }

    private async Task CriarMatriculaAtivaAsync(Aluno aluno)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var resultado = Matricula.Criar(
            0,
            aluno,
            MatriculaPlano.Mensal,
            hoje.AddDays(-1),
            hoje.AddMonths(1),
            "Avaliação 03 - controle de acesso",
            Restricao.None,
            null,
            null);

        Assert.True(resultado.IsSuccess);
        await _matriculaRepository.AdicionarAsync(resultado.Value!);
    }

    private async Task<Colaborador> CriarColaboradorAsync(ColaboradorVinculo vinculo)
    {
        var resultado = Colaborador.Criar(
            0,
            "Mario Cesar Alves Junior",
            GerarCpf(),
            new DateOnly(1990, 5, 10),
            GerarTelefone(),
            GerarEmail(),
            await CriarLogradouroAsync(),
            "100",
            "Alves Junior",
            "SenhaMySQL123",
            Arquivo.Criar([1, 2, 3, 4]).Value,
            new DateOnly(2024, 1, 10),
            ColaboradorTipo.Instrutor,
            vinculo);

        Assert.True(resultado.IsSuccess);
        await _colaboradorRepository.AdicionarAsync(resultado.Value!);
        return (await _colaboradorRepository.ObterPorCpfAsync(resultado.Value!.Cpf))!;
    }
}
