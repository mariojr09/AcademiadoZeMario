// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiadoZE.Domain.Repositories;

public interface IAlunoRepository : IRepository<Aluno>
{
    Task<Aluno?> ObterPorCpfAsync(Cpf cpf, CancellationToken cancellationToken = default);
    Task<Aluno?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<bool> ExisteCpfAsync(Cpf cpf, CancellationToken cancellationToken = default);
    Task<bool> ExisteEmailAsync(Email email, CancellationToken cancellationToken = default);
}
