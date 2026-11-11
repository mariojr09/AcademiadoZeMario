using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoService
{
    Task<IReadOnlyCollection<AcessoPessoaDto>> ListarAlunosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AcessoPessoaDto>> ListarColaboradoresAsync(CancellationToken cancellationToken = default);
    Task<AcessoRegistroDto?> ObterAbertoAlunoAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<AcessoRegistroDto?> ObterAbertoColaboradorAsync(int colaboradorId, CancellationToken cancellationToken = default);
    Task<AcessoResultadoDto> RegistrarEntradaAlunoAsync(int alunoId, DateTime dataHora, CancellationToken cancellationToken = default);
    Task<AcessoResultadoDto> RegistrarSaidaAlunoAsync(int alunoId, DateTime dataHora, CancellationToken cancellationToken = default);
    Task<AcessoResultadoDto> RegistrarEntradaColaboradorAsync(int colaboradorId, DateTime dataHora, CancellationToken cancellationToken = default);
    Task<AcessoResultadoDto> RegistrarSaidaColaboradorAsync(int colaboradorId, DateTime dataHora, CancellationToken cancellationToken = default);
    Task<ResumoDiarioColaboradorDto> ObterResumoDiarioColaboradorAsync(int colaboradorId, DateTime dataHoraReferencia, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AcessoRegistroDto>> ListarAcessosDoDiaAsync(DateOnly data, CancellationToken cancellationToken = default);
}
