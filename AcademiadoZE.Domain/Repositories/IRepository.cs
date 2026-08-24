// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Entities;

namespace AcademiadoZE.Domain.Repositories;

public interface IRepository<TEntity> where TEntity : Entity
{
    Task<TEntity?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TEntity>> ListarAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(TEntity entidade, CancellationToken cancellationToken = default);
    void Atualizar(TEntity entidade);
    void Remover(TEntity entidade);
}
