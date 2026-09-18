using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Enums;
using AcademiadoZE.Domain.Repositories;
using AcademiadoZE.Domain.ValueObjects;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;

namespace AcademiaDoZe.Application.Services;
// Mario Cesar Alves Júnior
public class MatriculaService : IMatriculaService
{
    private readonly IMatriculaRepository _matriculaRepository;
    private readonly IAlunoRepository _alunoRepository;

    public MatriculaService(
        IMatriculaRepository matriculaRepository,
        IAlunoRepository alunoRepository)
    {
        _matriculaRepository = matriculaRepository;
        _alunoRepository = alunoRepository;
    }

    public async Task<MatriculaDto?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var matricula =
            await _matriculaRepository.ObterPorIdAsync(
                id,
                cancellationToken);

        return matricula?.ToDto();
    }

    public async Task<IEnumerable<MatriculaDto>> ObterTodosAsync(
        CancellationToken cancellationToken = default)
    {
        var matriculas =
            await _matriculaRepository.ListarAsync(
                cancellationToken);

        return matriculas
            .Select(matricula => matricula.ToDto());
    }

    public async Task<IEnumerable<MatriculaDto>> ObterPorAlunoAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var matriculas =
            await _matriculaRepository.ListarPorAlunoAsync(
                alunoId,
                cancellationToken);

        return matriculas
            .Select(matricula => matricula.ToDto());
    }

    public async Task<MatriculaDto?> ObterVigentePorAlunoAsync(
        int alunoId,
        DateOnly dataReferencia,
        CancellationToken cancellationToken = default)
    {
        var matricula =
            await _matriculaRepository.ObterVigentePorAlunoAsync(
                alunoId,
                dataReferencia,
                cancellationToken);

        return matricula?.ToDto();
    }

    public async Task<MatriculaDto> AdicionarAsync(
        MatriculaDto matriculaDto,
        CancellationToken cancellationToken = default)
    {
        var aluno =
            await _alunoRepository.ObterPorIdAsync(
                matriculaDto.AlunoId,
                cancellationToken);

        if (aluno is null)
        {
            throw new InvalidOperationException(
                "ALUNO_NAO_ENCONTRADO");
        }

        var matriculaVigente =
            await _matriculaRepository.ObterVigentePorAlunoAsync(
                matriculaDto.AlunoId,
                matriculaDto.DataInicio,
                cancellationToken);

        if (matriculaVigente is not null)
        {
            throw new InvalidOperationException(
                "ALUNO_JA_POSSUI_MATRICULA_VIGENTE");
        }

        Arquivo? laudoMedico = null;

        if (matriculaDto.LaudoMedico?.Conteudo is { Length: > 0 })
        {
            var laudoResult =
                Arquivo.Criar(
                    matriculaDto.LaudoMedico.Conteudo);

            if (laudoResult.IsFailure)
            {
                throw new InvalidOperationException(
                    "LAUDO_MEDICO_INVALIDO");
            }

            laudoMedico = laudoResult.Value;
        }

        bool possuiRestricao =
            matriculaDto.Restricoes !=
            AppMatriculaRestricoes.None;

        if (possuiRestricao && laudoMedico is null)
        {
            throw new InvalidOperationException(
                "LAUDO_MEDICO_OBRIGATORIO_PARA_RESTRICAO");
        }

        int idade =
            CalcularIdade(
                aluno.DataNascimento,
                matriculaDto.DataInicio);

        if (idade < 16 && laudoMedico is null)
        {
            throw new InvalidOperationException(
                "LAUDO_MEDICO_OBRIGATORIO_MENOR_16_ANOS");
        }

        var result =
            Matricula.Criar(
                0,
                aluno,
                (MatriculaPlano)(int)matriculaDto.Plano,
                matriculaDto.DataInicio,
                matriculaDto.DataFinal,
                matriculaDto.Objetivo,
                (Restricao)(int)matriculaDto.Restricoes,
                matriculaDto.ObservacoesRestricoes,
                laudoMedico);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                "DADOS_MATRICULA_INVALIDOS");
        }

        var matricula = result.Value!;

        await _matriculaRepository.AdicionarAsync(
            matricula,
            cancellationToken);

        return matricula.ToDto();
    }

    public async Task<MatriculaDto> AtualizarAsync(
        MatriculaDto matriculaDto,
        CancellationToken cancellationToken = default)
    {
        var existente =
            await _matriculaRepository.ObterPorIdAsync(
                matriculaDto.Id,
                cancellationToken);

        if (existente is null)
        {
            throw new InvalidOperationException(
                "MATRICULA_NAO_ENCONTRADA");
        }

        var aluno =
            await _alunoRepository.ObterPorIdAsync(
                matriculaDto.AlunoId,
                cancellationToken);

        if (aluno is null)
        {
            throw new InvalidOperationException(
                "ALUNO_NAO_ENCONTRADO");
        }

        Arquivo? laudoMedico =
            existente.LaudoMedico;

        if (matriculaDto.LaudoMedico?.Conteudo is { Length: > 0 })
        {
            var laudoResult =
                Arquivo.Criar(
                    matriculaDto.LaudoMedico.Conteudo);

            if (laudoResult.IsFailure)
            {
                throw new InvalidOperationException(
                    "LAUDO_MEDICO_INVALIDO");
            }

            laudoMedico = laudoResult.Value;
        }

        bool possuiRestricao =
            matriculaDto.Restricoes !=
            AppMatriculaRestricoes.None;

        if (possuiRestricao && laudoMedico is null)
        {
            throw new InvalidOperationException(
                "LAUDO_MEDICO_OBRIGATORIO_PARA_RESTRICAO");
        }

        int idade =
            CalcularIdade(
                aluno.DataNascimento,
                matriculaDto.DataInicio);

        if (idade < 16 && laudoMedico is null)
        {
            throw new InvalidOperationException(
                "LAUDO_MEDICO_OBRIGATORIO_MENOR_16_ANOS");
        }

        var result =
            Matricula.Criar(
                matriculaDto.Id,
                aluno,
                (MatriculaPlano)(int)matriculaDto.Plano,
                matriculaDto.DataInicio,
                matriculaDto.DataFinal,
                matriculaDto.Objetivo,
                (Restricao)(int)matriculaDto.Restricoes,
                matriculaDto.ObservacoesRestricoes,
                laudoMedico);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                "DADOS_MATRICULA_INVALIDOS");
        }

        var matricula = result.Value!;

        _matriculaRepository.Atualizar(
            matricula);

        return matricula.ToDto();
    }

    public async Task<bool> RemoverAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var matricula =
            await _matriculaRepository.ObterPorIdAsync(
                id,
                cancellationToken);

        if (matricula is null)
            return false;

        _matriculaRepository.Remover(
            matricula);

        return true;
    }

    private static int CalcularIdade(
        DateOnly dataNascimento,
        DateOnly dataReferencia)
    {
        int idade =
            dataReferencia.Year -
            dataNascimento.Year;

        if (dataNascimento >
            dataReferencia.AddYears(-idade))
        {
            idade--;
        }

        return idade;
    }
}