// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Entities;

namespace AcademiadoZE.Domain.Repositories;

public interface IAcessoAlunoRepository : IRepository<AcessoAluno>
{
    Task<IReadOnlyCollection<AcessoAluno>> ListarPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<AcessoAluno?> ObterAbertoPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default);
}
