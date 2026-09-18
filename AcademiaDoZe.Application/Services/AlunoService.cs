using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Security;
using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Repositories;
using AcademiadoZE.Domain.Services;
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;
// Mario Cesar Alves Júnior
public class AlunoService : IAlunoService
{
    private readonly IAlunoRepository _alunoRepository;
    private readonly ILogradouroRepository _logradouroRepository;

    public AlunoService(
        IAlunoRepository alunoRepository,
        ILogradouroRepository logradouroRepository)
    {
        _alunoRepository = alunoRepository;
        _logradouroRepository = logradouroRepository;
    }

    public async Task<AlunoDto?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var aluno = await _alunoRepository.ObterPorIdAsync(
            id,
            cancellationToken);

        return aluno?.ToDto();
    }

    public async Task<IEnumerable<AlunoDto>> ObterTodosAsync(
        CancellationToken cancellationToken = default)
    {
        var alunos = await _alunoRepository.ListarAsync(
            cancellationToken);

        return alunos.Select(aluno => aluno.ToDto());
    }

    public async Task<AlunoDto?> ObterPorCpfAsync(
        string cpf,
        CancellationToken cancellationToken = default)
    {
        var cpfResult = Cpf.Criar(cpf);

        if (cpfResult.IsFailure)
            return null;

        var aluno = await _alunoRepository.ObterPorCpfAsync(
            cpfResult.Value!,
            cancellationToken);

        return aluno?.ToDto();
    }

    public async Task<AlunoDto?> ObterPorEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Criar(email);

        if (emailResult.IsFailure)
            return null;

        var aluno = await _alunoRepository.ObterPorEmailAsync(
            emailResult.Value!,
            cancellationToken);

        return aluno?.ToDto();
    }

    public async Task<IEnumerable<AlunoDto>> ObterPorNomeAsync(
        string nome,
        CancellationToken cancellationToken = default)
    {
        var alunos = await _alunoRepository.ListarAsync(
            cancellationToken);

        return alunos
            .Where(aluno =>
                aluno.Nome.Contains(
                    nome,
                    StringComparison.OrdinalIgnoreCase))
            .Select(aluno => aluno.ToDto());
    }

    public async Task<bool> CpfJaExisteAsync(
        string cpf,
        int? id = null,
        CancellationToken cancellationToken = default)
    {
        var cpfResult = Cpf.Criar(cpf);

        if (cpfResult.IsFailure)
            return false;

        var aluno = await _alunoRepository.ObterPorCpfAsync(
            cpfResult.Value!,
            cancellationToken);

        if (aluno is null)
            return false;

        return !id.HasValue || aluno.Id != id.Value;
    }

    public async Task<bool> EmailJaExisteAsync(
        string email,
        int? id = null,
        CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Criar(email);

        if (emailResult.IsFailure)
            return false;

        var aluno = await _alunoRepository.ObterPorEmailAsync(
            emailResult.Value!,
            cancellationToken);

        if (aluno is null)
            return false;

        return !id.HasValue || aluno.Id != id.Value;
    }

    public async Task<AlunoDto> AdicionarAsync(
        AlunoDto alunoDto,
        CancellationToken cancellationToken = default)
    {
        if (await CpfJaExisteAsync(
            alunoDto.Cpf,
            null,
            cancellationToken))
        {
            throw new InvalidOperationException(
                "CPF_JA_CADASTRADO");
        }

        if (!string.IsNullOrWhiteSpace(alunoDto.Email) &&
            await EmailJaExisteAsync(
                alunoDto.Email,
                null,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "EMAIL_JA_CADASTRADO");
        }

        if (alunoDto.Endereco is null)
        {
            throw new InvalidOperationException(
                "LOGRADOURO_OBRIGATORIO");
        }

        var logradouro =
            await _logradouroRepository.ObterPorIdAsync(
                alunoDto.Endereco.Id,
                cancellationToken);

        if (logradouro is null)
        {
            throw new InvalidOperationException(
                "LOGRADOURO_NAO_ENCONTRADO");
        }

        if (string.IsNullOrWhiteSpace(alunoDto.Senha))
        {
            throw new InvalidOperationException(
                "SENHA_OBRIGATORIA");
        }

        var senhaResult = PoliticaSenha.Validar(
            alunoDto.Senha);

        if (senhaResult.IsFailure)
        {
            throw new InvalidOperationException(
                "SENHA_INVALIDA");
        }

        string senhaHash =
            PasswordHasher.Hash(alunoDto.Senha);

        Arquivo? foto = null;

        if (alunoDto.Foto?.Conteudo is { Length: > 0 })
        {
            var fotoResult =
                Arquivo.Criar(alunoDto.Foto.Conteudo);

            if (fotoResult.IsFailure)
            {
                throw new InvalidOperationException(
                    "FOTO_INVALIDA");
            }

            foto = fotoResult.Value;
        }

        var result = Aluno.Criar(
            0,
            alunoDto.Nome,
            alunoDto.Cpf,
            alunoDto.DataNascimento,
            alunoDto.Telefone,
            alunoDto.Email,
            logradouro,
            alunoDto.Numero,
            alunoDto.Complemento,
            senhaHash,
            foto);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                "DADOS_ALUNO_INVALIDOS");
        }

        var aluno = result.Value!;

        await _alunoRepository.AdicionarAsync(
            aluno,
            cancellationToken);

        return aluno.ToDto();
    }

    public async Task<AlunoDto> AtualizarAsync(
        AlunoDto alunoDto,
        CancellationToken cancellationToken = default)
    {
        var existente =
            await _alunoRepository.ObterPorIdAsync(
                alunoDto.Id,
                cancellationToken);

        if (existente is null)
        {
            throw new InvalidOperationException(
                "ALUNO_NAO_ENCONTRADO");
        }

        if (await CpfJaExisteAsync(
            alunoDto.Cpf,
            alunoDto.Id,
            cancellationToken))
        {
            throw new InvalidOperationException(
                "CPF_JA_CADASTRADO");
        }

        if (!string.IsNullOrWhiteSpace(alunoDto.Email) &&
            await EmailJaExisteAsync(
                alunoDto.Email,
                alunoDto.Id,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "EMAIL_JA_CADASTRADO");
        }

        if (alunoDto.Endereco is null)
        {
            throw new InvalidOperationException(
                "LOGRADOURO_OBRIGATORIO");
        }

        var logradouro =
            await _logradouroRepository.ObterPorIdAsync(
                alunoDto.Endereco.Id,
                cancellationToken);

        if (logradouro is null)
        {
            throw new InvalidOperationException(
                "LOGRADOURO_NAO_ENCONTRADO");
        }

        string senhaHash = existente.Senha.Hash;

        if (!string.IsNullOrWhiteSpace(alunoDto.Senha))
        {
            var senhaResult =
                PoliticaSenha.Validar(alunoDto.Senha);

            if (senhaResult.IsFailure)
            {
                throw new InvalidOperationException(
                    "SENHA_INVALIDA");
            }

            senhaHash =
                PasswordHasher.Hash(alunoDto.Senha);
        }

        Arquivo? foto = existente.Foto;

        if (alunoDto.Foto?.Conteudo is { Length: > 0 })
        {
            var fotoResult =
                Arquivo.Criar(alunoDto.Foto.Conteudo);

            if (fotoResult.IsFailure)
            {
                throw new InvalidOperationException(
                    "FOTO_INVALIDA");
            }

            foto = fotoResult.Value;
        }

        var result = Aluno.Criar(
            alunoDto.Id,
            alunoDto.Nome,
            alunoDto.Cpf,
            alunoDto.DataNascimento,
            alunoDto.Telefone,
            alunoDto.Email,
            logradouro,
            alunoDto.Numero,
            alunoDto.Complemento,
            senhaHash,
            foto);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                "DADOS_ALUNO_INVALIDOS");
        }

        var aluno = result.Value!;

        _alunoRepository.Atualizar(aluno);

        return aluno.ToDto();
    }

    public async Task<bool> RemoverAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var aluno =
            await _alunoRepository.ObterPorIdAsync(
                id,
                cancellationToken);

        if (aluno is null)
            return false;

        _alunoRepository.Remover(aluno);

        return true;
    }
}