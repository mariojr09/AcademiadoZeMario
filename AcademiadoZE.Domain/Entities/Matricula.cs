using AcademiadoZE.Domain.Enums;
using AcademiadoZE.Domain.ValueObjects;

using AcademiadoZE.Domain.Common;
using AcademiadoZE.Domain.Enums;
using AcademiadoZE.Domain.Services;
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiadoZE.Domain.Entities;
//Mario Cesar alves Júnior

public class Matricula : Entity
{
    
    private const Restricao TodasAsRestricoesValidas =
        Restricao.Diabetes | Restricao.PressaoAlta | Restricao.Labirintite |
        Restricao.Alergias | Restricao.ProblemasRespiratorios | Restricao.RemedioContinuo;

    public Aluno Aluno { get; private set; }
    public MatriculaPlano Plano { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFinal { get; private set; }
    public string Objetivo { get; private set; }
    public Restricao Restricoes { get; private set; }
    public string? ObservacoesRestricoes { get; private set; }
    public Arquivo? LaudoMedico { get; private set; }

    private Matricula(
        int id, Aluno aluno, MatriculaPlano plano, DateOnly dataInicio, DateOnly dataFinal,
        string objetivo, Restricao restricoes, string? observacoesRestricoes, Arquivo? laudoMedico)
        : base(id)
    {
        Aluno = aluno;
        Plano = plano;
        DataInicio = dataInicio;
        DataFinal = dataFinal;
        Objetivo = objetivo;
        Restricoes = restricoes;
        ObservacoesRestricoes = observacoesRestricoes;
        LaudoMedico = laudoMedico;
    }
    public static Result<Matricula> Criar(
        int id, Aluno? aluno, MatriculaPlano plano, DateOnly dataInicio, DateOnly dataFinal,
        string? objetivo, Restricao restricoes, string? observacoesRestricoes, Arquivo? laudoMedico)
    {
        var notifications = new List<Notification>();

        if (aluno == null)
            notifications.Add(new Notification("Aluno", "ALUNO_OBRIGATORIO"));

        if (!Enum.IsDefined(plano))
            notifications.Add(new Notification("Plano", "PLANO_INVALIDO"));

        if (dataInicio == default)
            notifications.Add(new Notification("DataInicio", "DATA_INICIO_OBRIGATORIA"));

        if (dataFinal == default)
            notifications.Add(new Notification("DataFinal", "DATA_FINAL_OBRIGATORIA"));
        else if (dataInicio != default && dataFinal <= dataInicio)
            notifications.Add(new Notification("DataFinal", "DATA_FINAL_ANTERIOR_A_INICIO"));

        if (NormalizadoService.TextoVazioOuNulo(objetivo))
            notifications.Add(new Notification("Objetivo", "OBJETIVO_OBRIGATORIO"));
        else
            objetivo = NormalizadoService.LimparEspacos(objetivo);

       
        if ((TodasAsRestricoesValidas | restricoes) != TodasAsRestricoesValidas)
            notifications.Add(new Notification("Restricoes", "RESTRICAO_INVALIDA"));

        observacoesRestricoes = NormalizadoService.TextoVazioOuNulo(observacoesRestricoes)
            ? null
            : NormalizadoService.LimparEspacos(observacoesRestricoes);

        if (restricoes != Restricao.None && observacoesRestricoes is null && laudoMedico is null)
            notifications.Add(new Notification("ObservacoesRestricoes", "RESTRICAO_SEM_JUSTIFICATIVA"));

        if (notifications.Count != 0)
            return Result<Matricula>.Failure(notifications);

        var matricula = new Matricula(
            id, aluno!, plano, dataInicio, dataFinal, objetivo!, restricoes, observacoesRestricoes, laudoMedico);

        return Result<Matricula>.Success(matricula);
    }

    public void AnexarLaudoMedico(Arquivo laudoMedico)
    {
        LaudoMedico = laudoMedico;
    }
}
