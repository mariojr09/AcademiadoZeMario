// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiadoZE.Domain.Repositories;

public interface IColaboradorRepository : IRepository<Colaborador>
{
    Task<Colaborador?> ObterPorCpfAsync(Cpf cpf, CancellationToken cancellationToken = default);
    Task<Colaborador?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<bool> ExisteCpfAsync(Cpf cpf, CancellationToken cancellationToken = default);
    Task<bool> ExisteEmailAsync(Email email, CancellationToken cancellationToken = default);
}
