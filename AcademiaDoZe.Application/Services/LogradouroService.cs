using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Repositories;
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;
// Mario Cesar Alves Júnior
public class LogradouroService : ILogradouroService
{
    private readonly ILogradouroRepository _logradouroRepository;

    public LogradouroService(ILogradouroRepository logradouroRepository)
    {
        _logradouroRepository = logradouroRepository;
    }

    public async Task<LogradouroDto?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var logradouro = await _logradouroRepository.ObterPorIdAsync(
            id,
            cancellationToken);

        return logradouro?.ToDto();
    }

    public async Task<IEnumerable<LogradouroDto>> ObterTodosAsync(
        CancellationToken cancellationToken = default)
    {
        var logradouros = await _logradouroRepository.ListarAsync(
            cancellationToken);

        return logradouros
            .Select(logradouro => logradouro.ToDto());
    }

    public async Task<LogradouroDto?> ObterPorCepAsync(
        string cep,
        CancellationToken cancellationToken = default)
    {
        var cepResult = Cep.Criar(cep);

        if (cepResult.IsFailure)
            return null;

        var logradouros = await _logradouroRepository.ListarPorCepAsync(
            cepResult.Value!,
            cancellationToken);

        return logradouros
            .FirstOrDefault()?
            .ToDto();
    }

    public async Task<bool> CepJaExisteAsync(
        string cep,
        int? id = null,
        CancellationToken cancellationToken = default)
    {
        var cepResult = Cep.Criar(cep);

        if (cepResult.IsFailure)
            return false;

        var logradouros = await _logradouroRepository.ListarPorCepAsync(
            cepResult.Value!,
            cancellationToken);

        return logradouros.Any(logradouro =>
            !id.HasValue || logradouro.Id != id.Value);
    }

    public async Task<IEnumerable<LogradouroDto>> ObterPorCidadeAsync(
        string cidade,
        CancellationToken cancellationToken = default)
    {
        var logradouros = await _logradouroRepository.ListarAsync(
            cancellationToken);

        return logradouros
            .Where(logradouro =>
                logradouro.Cidade.Equals(
                    cidade,
                    StringComparison.OrdinalIgnoreCase))
            .Select(logradouro => logradouro.ToDto());
    }

    public async Task<IEnumerable<LogradouroDto>> ObterPorBairroAsync(
        string cidade,
        string bairro,
        CancellationToken cancellationToken = default)
    {
        var logradouros = await _logradouroRepository.ListarAsync(
            cancellationToken);

        return logradouros
            .Where(logradouro =>
                logradouro.Cidade.Equals(
                    cidade,
                    StringComparison.OrdinalIgnoreCase) &&
                logradouro.Bairro.Equals(
                    bairro,
                    StringComparison.OrdinalIgnoreCase))
            .Select(logradouro => logradouro.ToDto());
    }

    public async Task<LogradouroDto> AdicionarAsync(
        LogradouroDto logradouroDto,
        CancellationToken cancellationToken = default)
    {
        if (await CepJaExisteAsync(
            logradouroDto.Cep,
            null,
            cancellationToken))
        {
            throw new InvalidOperationException(
                "CEP_JA_CADASTRADO");
        }

        var result = Logradouro.Criar(
            0,
            logradouroDto.Cep,
            logradouroDto.Pais,
            logradouroDto.Estado,
            logradouroDto.Cidade,
            logradouroDto.Bairro,
            logradouroDto.Nome);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                "DADOS_LOGRADOURO_INVALIDOS");
        }

        var logradouro = result.Value!;

        await _logradouroRepository.AdicionarAsync(
            logradouro,
            cancellationToken);

        return logradouro.ToDto();
    }

    public async Task<LogradouroDto> AtualizarAsync(
        LogradouroDto logradouroDto,
        CancellationToken cancellationToken = default)
    {
        var existente = await _logradouroRepository.ObterPorIdAsync(
            logradouroDto.Id,
            cancellationToken);

        if (existente is null)
        {
            throw new InvalidOperationException(
                "LOGRADOURO_NAO_ENCONTRADO");
        }

        if (await CepJaExisteAsync(
            logradouroDto.Cep,
            logradouroDto.Id,
            cancellationToken))
        {
            throw new InvalidOperationException(
                "CEP_JA_CADASTRADO");
        }

        var result = Logradouro.Criar(
            logradouroDto.Id,
            logradouroDto.Cep,
            logradouroDto.Pais,
            logradouroDto.Estado,
            logradouroDto.Cidade,
            logradouroDto.Bairro,
            logradouroDto.Nome);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                "DADOS_LOGRADOURO_INVALIDOS");
        }

        var logradouro = result.Value!;

        _logradouroRepository.Atualizar(logradouro);

        return logradouro.ToDto();
    }

    public async Task<bool> RemoverAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var logradouro = await _logradouroRepository.ObterPorIdAsync(
            id,
            cancellationToken);

        if (logradouro is null)
            return false;

        _logradouroRepository.Remover(logradouro);

        return true;
    }
}