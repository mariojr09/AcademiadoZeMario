// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Entities;

namespace AcademiadoZE.Domain.Repositories;

public interface IMatriculaRepository : IRepository<Matricula>
{
    Task<IReadOnlyCollection<Matricula>> ListarPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<Matricula?> ObterVigentePorAlunoAsync(int alunoId, DateOnly dataReferencia, CancellationToken cancellationToken = default);
}
