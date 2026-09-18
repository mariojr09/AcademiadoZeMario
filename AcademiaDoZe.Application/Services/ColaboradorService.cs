using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Security;
using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Enums;
using AcademiadoZE.Domain.Repositories;
using AcademiadoZE.Domain.Services;
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;
// Mario Cesar Alves Júnior
public class ColaboradorService : IColaboradorService
{
    private readonly IColaboradorRepository _colaboradorRepository;
    private readonly ILogradouroRepository _logradouroRepository;

    public ColaboradorService(
        IColaboradorRepository colaboradorRepository,
        ILogradouroRepository logradouroRepository)
    {
        _colaboradorRepository = colaboradorRepository;
        _logradouroRepository = logradouroRepository;
    }

    public async Task<ColaboradorDto?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var colaborador =
            await _colaboradorRepository.ObterPorIdAsync(
                id,
                cancellationToken);

        return colaborador?.ToDto();
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterTodosAsync(
        CancellationToken cancellationToken = default)
    {
        var colaboradores =
            await _colaboradorRepository.ListarAsync(
                cancellationToken);

        return colaboradores
            .Select(colaborador => colaborador.ToDto());
    }

    public async Task<ColaboradorDto?> ObterPorCpfAsync(
        string cpf,
        CancellationToken cancellationToken = default)
    {
        var cpfResult = Cpf.Criar(cpf);

        if (cpfResult.IsFailure)
            return null;

        var colaborador =
            await _colaboradorRepository.ObterPorCpfAsync(
                cpfResult.Value!,
                cancellationToken);

        return colaborador?.ToDto();
    }

    public async Task<ColaboradorDto?> ObterPorEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Criar(email);

        if (emailResult.IsFailure)
            return null;

        var colaborador =
            await _colaboradorRepository.ObterPorEmailAsync(
                emailResult.Value!,
                cancellationToken);

        return colaborador?.ToDto();
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterPorNomeAsync(
        string nome,
        CancellationToken cancellationToken = default)
    {
        var colaboradores =
            await _colaboradorRepository.ListarAsync(
                cancellationToken);

        return colaboradores
            .Where(colaborador =>
                colaborador.Nome.Contains(
                    nome,
                    StringComparison.OrdinalIgnoreCase))
            .Select(colaborador => colaborador.ToDto());
    }

    public async Task<bool> CpfJaExisteAsync(
        string cpf,
        int? id = null,
        CancellationToken cancellationToken = default)
    {
        var cpfResult = Cpf.Criar(cpf);

        if (cpfResult.IsFailure)
            return false;

        var colaborador =
            await _colaboradorRepository.ObterPorCpfAsync(
                cpfResult.Value!,
                cancellationToken);

        if (colaborador is null)
            return false;

        return !id.HasValue ||
               colaborador.Id != id.Value;
    }

    public async Task<bool> EmailJaExisteAsync(
        string email,
        int? id = null,
        CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Criar(email);

        if (emailResult.IsFailure)
            return false;

        var colaborador =
            await _colaboradorRepository.ObterPorEmailAsync(
                emailResult.Value!,
                cancellationToken);

        if (colaborador is null)
            return false;

        return !id.HasValue ||
               colaborador.Id != id.Value;
    }

    public async Task<ColaboradorDto> AdicionarAsync(
        ColaboradorDto colaboradorDto,
        CancellationToken cancellationToken = default)
    {
        if (await CpfJaExisteAsync(
            colaboradorDto.Cpf,
            null,
            cancellationToken))
        {
            throw new InvalidOperationException(
                "CPF_JA_CADASTRADO");
        }

        if (!string.IsNullOrWhiteSpace(colaboradorDto.Email) &&
            await EmailJaExisteAsync(
                colaboradorDto.Email,
                null,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "EMAIL_JA_CADASTRADO");
        }

        if (colaboradorDto.Endereco is null)
        {
            throw new InvalidOperationException(
                "LOGRADOURO_OBRIGATORIO");
        }

        var logradouro =
            await _logradouroRepository.ObterPorIdAsync(
                colaboradorDto.Endereco.Id,
                cancellationToken);

        if (logradouro is null)
        {
            throw new InvalidOperationException(
                "LOGRADOURO_NAO_ENCONTRADO");
        }

        if (string.IsNullOrWhiteSpace(colaboradorDto.Senha))
        {
            throw new InvalidOperationException(
                "SENHA_OBRIGATORIA");
        }

        var senhaResult =
            PoliticaSenha.Validar(colaboradorDto.Senha);

        if (senhaResult.IsFailure)
        {
            throw new InvalidOperationException(
                "SENHA_INVALIDA");
        }

        string senhaHash =
            PasswordHasher.Hash(colaboradorDto.Senha);

        Arquivo? foto = null;

        if (colaboradorDto.Foto?.Conteudo is { Length: > 0 })
        {
            var fotoResult =
                Arquivo.Criar(
                    colaboradorDto.Foto.Conteudo);

            if (fotoResult.IsFailure)
            {
                throw new InvalidOperationException(
                    "FOTO_INVALIDA");
            }

            foto = fotoResult.Value;
        }

        var result = Colaborador.Criar(
            0,
            colaboradorDto.Nome,
            colaboradorDto.Cpf,
            colaboradorDto.DataNascimento,
            colaboradorDto.Telefone,
            colaboradorDto.Email,
            logradouro,
            colaboradorDto.Numero,
            colaboradorDto.Complemento,
            senhaHash,
            foto,
            colaboradorDto.DataAdmissao,
            (ColaboradorTipo)(int)colaboradorDto.Tipo,
            (ColaboradorVinculo)(int)colaboradorDto.Vinculo);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                "DADOS_COLABORADOR_INVALIDOS");
        }

        var colaborador = result.Value!;

        await _colaboradorRepository.AdicionarAsync(
            colaborador,
            cancellationToken);

        return colaborador.ToDto();
    }

    public async Task<ColaboradorDto> AtualizarAsync(
        ColaboradorDto colaboradorDto,
        CancellationToken cancellationToken = default)
    {
        var existente =
            await _colaboradorRepository.ObterPorIdAsync(
                colaboradorDto.Id,
                cancellationToken);

        if (existente is null)
        {
            throw new InvalidOperationException(
                "COLABORADOR_NAO_ENCONTRADO");
        }

        if (await CpfJaExisteAsync(
            colaboradorDto.Cpf,
            colaboradorDto.Id,
            cancellationToken))
        {
            throw new InvalidOperationException(
                "CPF_JA_CADASTRADO");
        }

        if (!string.IsNullOrWhiteSpace(colaboradorDto.Email) &&
            await EmailJaExisteAsync(
                colaboradorDto.Email,
                colaboradorDto.Id,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "EMAIL_JA_CADASTRADO");
        }

        if (colaboradorDto.Endereco is null)
        {
            throw new InvalidOperationException(
                "LOGRADOURO_OBRIGATORIO");
        }

        var logradouro =
            await _logradouroRepository.ObterPorIdAsync(
                colaboradorDto.Endereco.Id,
                cancellationToken);

        if (logradouro is null)
        {
            throw new InvalidOperationException(
                "LOGRADOURO_NAO_ENCONTRADO");
        }

        string senhaHash = existente.Senha.Hash;

        if (!string.IsNullOrWhiteSpace(colaboradorDto.Senha))
        {
            var senhaResult =
                PoliticaSenha.Validar(
                    colaboradorDto.Senha);

            if (senhaResult.IsFailure)
            {
                throw new InvalidOperationException(
                    "SENHA_INVALIDA");
            }

            senhaHash =
                PasswordHasher.Hash(
                    colaboradorDto.Senha);
        }

        Arquivo? foto = existente.Foto;

        if (colaboradorDto.Foto?.Conteudo is { Length: > 0 })
        {
            var fotoResult =
                Arquivo.Criar(
                    colaboradorDto.Foto.Conteudo);

            if (fotoResult.IsFailure)
            {
                throw new InvalidOperationException(
                    "FOTO_INVALIDA");
            }

            foto = fotoResult.Value;
        }

        var result = Colaborador.Criar(
            colaboradorDto.Id,
            colaboradorDto.Nome,
            colaboradorDto.Cpf,
            colaboradorDto.DataNascimento,
            colaboradorDto.Telefone,
            colaboradorDto.Email,
            logradouro,
            colaboradorDto.Numero,
            colaboradorDto.Complemento,
            senhaHash,
            foto,
            colaboradorDto.DataAdmissao,
            (ColaboradorTipo)(int)colaboradorDto.Tipo,
            (ColaboradorVinculo)(int)colaboradorDto.Vinculo);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                "DADOS_COLABORADOR_INVALIDOS");
        }

        var colaborador = result.Value!;

        _colaboradorRepository.Atualizar(
            colaborador);

        return colaborador.ToDto();
    }

    public async Task<bool> RemoverAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var colaborador =
            await _colaboradorRepository.ObterPorIdAsync(
                id,
                cancellationToken);

        if (colaborador is null)
            return false;

        _colaboradorRepository.Remover(
            colaborador);

        return true;
    }
}