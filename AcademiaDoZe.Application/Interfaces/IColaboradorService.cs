using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;
// Mario Cesar Alves Júnior
/// <summary>
/// Contrato de serviço para operações de negócios relacionadas a Colaboradores.
/// </summary>
public interface IColaboradorService
{
    Task<ColaboradorDto?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ColaboradorDto>> ObterTodosAsync(
        CancellationToken cancellationToken = default);

    Task<ColaboradorDto> AdicionarAsync(
        ColaboradorDto colaboradorDto,
        CancellationToken cancellationToken = default);

    Task<ColaboradorDto> AtualizarAsync(
        ColaboradorDto colaboradorDto,
        CancellationToken cancellationToken = default);

    Task<bool> RemoverAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ColaboradorDto?> ObterPorCpfAsync(
        string cpf,
        CancellationToken cancellationToken = default);

    Task<ColaboradorDto?> ObterPorEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ColaboradorDto>> ObterPorNomeAsync(
        string nome,
        CancellationToken cancellationToken = default);

    Task<bool> CpfJaExisteAsync(
        string cpf,
        int? id = null,
        CancellationToken cancellationToken = default);

    Task<bool> EmailJaExisteAsync(
        string email,
        int? id = null,
        CancellationToken cancellationToken = default);
}