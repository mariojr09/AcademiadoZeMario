// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Entities;

namespace AcademiadoZE.Domain.Repositories;

public interface IAcessoColaboradorRepository : IRepository<AcessoColaborador>
{
    Task<IReadOnlyCollection<AcessoColaborador>> ListarPorColaboradorAsync(int colaboradorId, CancellationToken cancellationToken = default);
    Task<AcessoColaborador?> ObterAbertoPorColaboradorAsync(int colaboradorId, CancellationToken cancellationToken = default);
}
