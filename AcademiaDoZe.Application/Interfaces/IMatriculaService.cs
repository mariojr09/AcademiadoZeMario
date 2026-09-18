using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;
// Mario Cesar Alves Júnior
/// <summary>
/// Contrato de serviço para operações de negócios relacionadas a Matrículas.
/// </summary>
public interface IMatriculaService
{
    Task<MatriculaDto?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<MatriculaDto>> ObterTodosAsync(
        CancellationToken cancellationToken = default);

    Task<MatriculaDto> AdicionarAsync(
        MatriculaDto matriculaDto,
        CancellationToken cancellationToken = default);

    Task<MatriculaDto> AtualizarAsync(
        MatriculaDto matriculaDto,
        CancellationToken cancellationToken = default);

    Task<bool> RemoverAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<MatriculaDto>> ObterPorAlunoAsync(
        int alunoId,
        CancellationToken cancellationToken = default);

    Task<MatriculaDto?> ObterVigentePorAlunoAsync(
        int alunoId,
        DateOnly dataReferencia,
        CancellationToken cancellationToken = default);
}