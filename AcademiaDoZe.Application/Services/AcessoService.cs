using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Enums;
using AcademiadoZE.Domain.Repositories;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;

namespace AcademiaDoZe.Application.Services;

public class AcessoService : IAcessoService
{
    private readonly IAlunoRepository _alunoRepository;
    private readonly IColaboradorRepository _colaboradorRepository;
    private readonly IMatriculaRepository _matriculaRepository;
    private readonly IAcessoAlunoRepository _acessoAlunoRepository;
    private readonly IAcessoColaboradorRepository _acessoColaboradorRepository;

    public AcessoService(
        IAlunoRepository alunoRepository,
        IColaboradorRepository colaboradorRepository,
        IMatriculaRepository matriculaRepository,
        IAcessoAlunoRepository acessoAlunoRepository,
        IAcessoColaboradorRepository acessoColaboradorRepository)
    {
        _alunoRepository = alunoRepository;
        _colaboradorRepository = colaboradorRepository;
        _matriculaRepository = matriculaRepository;
        _acessoAlunoRepository = acessoAlunoRepository;
        _acessoColaboradorRepository = acessoColaboradorRepository;
    }

    public async Task<IReadOnlyCollection<AcessoPessoaDto>> ListarAlunosAsync(
        CancellationToken cancellationToken = default) =>
        (await _alunoRepository.ListarAsync(cancellationToken))
            .Select(a => new AcessoPessoaDto
            {
                Id = a.Id,
                Nome = a.Nome,
                Cpf = a.Cpf.Valor,
                Foto = a.Foto is null ? null : new ArquivoDto { Conteudo = a.Foto.Conteudo },
                Tipo = AcessoPessoaTipo.Aluno
            })
            .ToArray();

    public async Task<IReadOnlyCollection<AcessoPessoaDto>> ListarColaboradoresAsync(
        CancellationToken cancellationToken = default) =>
        (await _colaboradorRepository.ListarAsync(cancellationToken))
            .Select(c => new AcessoPessoaDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Cpf = c.Cpf.Valor,
                Foto = c.Foto is null ? null : new ArquivoDto { Conteudo = c.Foto.Conteudo },
                Tipo = AcessoPessoaTipo.Colaborador,
                Vinculo = c.Vinculo,
                JornadaDiaria = c.JornadaDiaria
            })
            .ToArray();

    public async Task<AcessoRegistroDto?> ObterAbertoAlunoAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var acesso = await _acessoAlunoRepository.ObterAbertoPorAlunoAsync(alunoId, cancellationToken);
        return acesso is null ? null : Map(acesso);
    }

    public async Task<AcessoRegistroDto?> ObterAbertoColaboradorAsync(
        int colaboradorId,
        CancellationToken cancellationToken = default)
    {
        var acesso = await _acessoColaboradorRepository.ObterAbertoPorColaboradorAsync(colaboradorId, cancellationToken);
        return acesso is null ? null : Map(acesso);
    }

    public async Task<AcessoResultadoDto> RegistrarEntradaAlunoAsync(
        int alunoId,
        DateTime dataHora,
        CancellationToken cancellationToken = default)
    {
        var aluno = await _alunoRepository.ObterPorIdAsync(alunoId, cancellationToken)
            ?? throw new InvalidOperationException("ALUNO_NAO_ENCONTRADO");

        var data = DateOnly.FromDateTime(dataHora);
        var matricula = await _matriculaRepository.ObterVigentePorAlunoAsync(alunoId, data, cancellationToken)
            ?? throw new InvalidOperationException("ALUNO_SEM_MATRICULA_ATIVA");

        if (await _acessoAlunoRepository.ObterAbertoPorAlunoAsync(alunoId, cancellationToken) is not null)
            throw new InvalidOperationException("ALUNO_JA_ESTA_NA_ACADEMIA");

        var acessoResult = AcessoAluno.Criar(0, aluno, dataHora);
        if (acessoResult.IsFailure)
            throw new InvalidOperationException("DADOS_ACESSO_ALUNO_INVALIDOS");

        await _acessoAlunoRepository.AdicionarAsync(acessoResult.Value!, cancellationToken);

        int diasRestantes = Math.Max(0, matricula.DataFinal.DayNumber - data.DayNumber);
        return new AcessoResultadoDto(
            $"Entrada autorizada. Restam {diasRestantes} dia(s) do plano.",
            DiasRestantesPlano: diasRestantes);
    }

    public async Task<AcessoResultadoDto> RegistrarSaidaAlunoAsync(
        int alunoId,
        DateTime dataHora,
        CancellationToken cancellationToken = default)
    {
        var acesso = await _acessoAlunoRepository.ObterAbertoPorAlunoAsync(alunoId, cancellationToken)
            ?? throw new InvalidOperationException("ALUNO_NAO_ESTA_NA_ACADEMIA");

        var resultado = acesso.RegistrarSaida(dataHora);
        if (resultado.IsFailure)
            throw new InvalidOperationException(resultado.Notifications.First().Mensagem);

        _acessoAlunoRepository.Atualizar(acesso);
        return new AcessoResultadoDto(
            $"Saída registrada. Permanência: {FormatarDuracao(acesso.TempoPermanencia!.Value)}.",
            acesso.TempoPermanencia);
    }

    public async Task<AcessoResultadoDto> RegistrarEntradaColaboradorAsync(
        int colaboradorId,
        DateTime dataHora,
        CancellationToken cancellationToken = default)
    {
        var colaborador = await _colaboradorRepository.ObterPorIdAsync(colaboradorId, cancellationToken)
            ?? throw new InvalidOperationException("COLABORADOR_NAO_ENCONTRADO");

        if (await _acessoColaboradorRepository.ObterAbertoPorColaboradorAsync(colaboradorId, cancellationToken) is not null)
            throw new InvalidOperationException("COLABORADOR_JA_ESTA_NA_ACADEMIA");

        var acessos = await _acessoColaboradorRepository.ListarPorColaboradorAsync(colaboradorId, cancellationToken);
        var totalDoDia = SomarDuracoesDoDia(acessos, DateOnly.FromDateTime(dataHora));

        if (totalDoDia >= colaborador.JornadaDiaria)
            throw new InvalidOperationException("JORNADA_DIARIA_JA_CUMPRIDA");

        var acessoResult = AcessoColaborador.Criar(0, colaborador, dataHora);
        if (acessoResult.IsFailure)
            throw new InvalidOperationException("DADOS_ACESSO_COLABORADOR_INVALIDOS");

        await _acessoColaboradorRepository.AdicionarAsync(acessoResult.Value!, cancellationToken);

        var restante = colaborador.JornadaDiaria - totalDoDia;
        return new AcessoResultadoDto(
            $"Entrada autorizada. Jornada cumprida hoje: {FormatarDuracao(totalDoDia)}; restante: {FormatarDuracao(restante)}.",
            totalDoDia);
    }

    public async Task<AcessoResultadoDto> RegistrarSaidaColaboradorAsync(
        int colaboradorId,
        DateTime dataHora,
        CancellationToken cancellationToken = default)
    {
        var acesso = await _acessoColaboradorRepository.ObterAbertoPorColaboradorAsync(colaboradorId, cancellationToken)
            ?? throw new InvalidOperationException("COLABORADOR_NAO_ESTA_NA_ACADEMIA");

        var resultado = acesso.RegistrarSaida(dataHora);
        if (resultado.IsFailure)
            throw new InvalidOperationException(resultado.Notifications.First().Mensagem);

        _acessoColaboradorRepository.Atualizar(acesso);

        var acessos = await _acessoColaboradorRepository.ListarPorColaboradorAsync(colaboradorId, cancellationToken);
        var totalDoDia = SomarDuracoesDoDia(acessos, DateOnly.FromDateTime(dataHora));

        return new AcessoResultadoDto(
            $"Saída registrada. Permanência atual: {FormatarDuracao(acesso.TempoPermanencia!.Value)}; total do dia: {FormatarDuracao(totalDoDia)}.",
            totalDoDia);
    }

    public async Task<IReadOnlyCollection<AcessoRegistroDto>> ListarAcessosDoDiaAsync(
        DateOnly data,
        CancellationToken cancellationToken = default)
    {
        var alunos = await _acessoAlunoRepository.ListarAsync(cancellationToken);
        var colaboradores = await _acessoColaboradorRepository.ListarAsync(cancellationToken);

        return alunos
            .Where(a => DateOnly.FromDateTime(a.DataHoraChegada) == data)
            .Select(Map)
            .Concat(colaboradores
                .Where(a => DateOnly.FromDateTime(a.DataHoraChegada) == data)
                .Select(Map))
            .OrderByDescending(a => a.DataHoraChegada)
            .ToArray();
    }

    public async Task<ResumoDiarioColaboradorDto> ObterResumoDiarioColaboradorAsync(
        int colaboradorId,
        DateTime dataHoraReferencia,
        CancellationToken cancellationToken = default)
    {
        var colaborador = await _colaboradorRepository.ObterPorIdAsync(colaboradorId, cancellationToken)
            ?? throw new InvalidOperationException("COLABORADOR_NAO_ENCONTRADO");

        var acessos = await _acessoColaboradorRepository.ListarPorColaboradorAsync(
            colaboradorId,
            cancellationToken);

        var horasTrabalhadas = SomarDuracoesDoDia(
            acessos,
            DateOnly.FromDateTime(dataHoraReferencia),
            dataHoraReferencia);

        return new ResumoDiarioColaboradorDto
        {
            ColaboradorId = colaborador.Id,
            Vinculo = colaborador.Vinculo,
            HorasTrabalhadas = horasTrabalhadas,
            JornadaPrevista = colaborador.JornadaDiaria,
            SaldoHoras = horasTrabalhadas - colaborador.JornadaDiaria
        };
    }

    private static TimeSpan SomarDuracoesDoDia(
        IEnumerable<AcessoColaborador> acessos,
        DateOnly data,
        DateTime? dataHoraReferencia = null) =>
        acessos
            .Where(a => DateOnly.FromDateTime(a.DataHoraChegada) == data)
            .Aggregate(TimeSpan.Zero, (total, acesso) =>
            {
                if (acesso.TempoPermanencia.HasValue)
                    return total + acesso.TempoPermanencia.Value;

                if (dataHoraReferencia.HasValue && dataHoraReferencia.Value >= acesso.DataHoraChegada)
                    return total + (dataHoraReferencia.Value - acesso.DataHoraChegada);

                return total;
            });

    private static AcessoRegistroDto Map(AcessoAluno acesso) => new()
    {
        Id = acesso.Id,
        PessoaId = acesso.Aluno.Id,
        Nome = acesso.Aluno.Nome,
        Foto = acesso.Aluno.Foto is null ? null : new ArquivoDto { Conteudo = acesso.Aluno.Foto.Conteudo },
        Tipo = AcessoPessoaTipo.Aluno,
        DataHoraChegada = acesso.DataHoraChegada,
        DataHoraSaida = acesso.DataHoraSaida
    };

    private static AcessoRegistroDto Map(AcessoColaborador acesso) => new()
    {
        Id = acesso.Id,
        PessoaId = acesso.Colaborador.Id,
        Nome = acesso.Colaborador.Nome,
        Foto = acesso.Colaborador.Foto is null ? null : new ArquivoDto { Conteudo = acesso.Colaborador.Foto.Conteudo },
        Tipo = AcessoPessoaTipo.Colaborador,
        Vinculo = acesso.Colaborador.Vinculo,
        DataHoraChegada = acesso.DataHoraChegada,
        DataHoraSaida = acesso.DataHoraSaida
    };

    private static string FormatarDuracao(TimeSpan duracao) =>
        $"{(int)duracao.TotalHours:00}:{duracao.Minutes:00}:{duracao.Seconds:00}";
}
