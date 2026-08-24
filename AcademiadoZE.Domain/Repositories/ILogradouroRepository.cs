// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiadoZE.Domain.Repositories;

public interface ILogradouroRepository : IRepository<Logradouro>
{
    Task<IReadOnlyCollection<Logradouro>> ListarPorCepAsync(Cep cep, CancellationToken cancellationToken = default);
}
