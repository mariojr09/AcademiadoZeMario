using AcademiadoZE.Domain.Common;

namespace AcademiadoZE.Domain.Entities;
//Mario Cesar alves Júnior

public class AcessoAluno : Entity
{
    public Aluno Aluno { get; private set; }
    public DateTime DataHoraChegada { get; private set; }
    public DateTime? DataHoraSaida { get; private set; }

    
    private AcessoAluno(int id, Aluno aluno, DateTime dataHoraChegada, DateTime? dataHoraSaida)
        : base(id)
    {
        Aluno = aluno;
        DataHoraChegada = dataHoraChegada;
        DataHoraSaida = dataHoraSaida;
    }

    public static Result<AcessoAluno> Criar(int id, Aluno? aluno, DateTime dataHoraChegada)
    {
        var notifications = new List<Notification>();

        if (aluno == null)
            notifications.Add(new Notification("Aluno", "ALUNO_OBRIGATORIO"));

        if (dataHoraChegada == default)
            notifications.Add(new Notification("DataHoraChegada", "DATA_HORA_CHEGADA_OBRIGATORIA"));

        if (notifications.Count != 0)
            return Result<AcessoAluno>.Failure(notifications);

        return Result<AcessoAluno>.Success(new AcessoAluno(id, aluno!, dataHoraChegada, dataHoraSaida: null));
    }

    
    public Result<bool> RegistrarSaida(DateTime dataHoraSaida)
    {
        if (DataHoraSaida is not null)
            return Result<bool>.Failure("DataHoraSaida", "SAIDA_JA_REGISTRADA");

        if (dataHoraSaida < DataHoraChegada)
            return Result<bool>.Failure("DataHoraSaida", "SAIDA_ANTERIOR_A_CHEGADA");

        DataHoraSaida = dataHoraSaida;
        return Result<bool>.Success(true);
    }
}
