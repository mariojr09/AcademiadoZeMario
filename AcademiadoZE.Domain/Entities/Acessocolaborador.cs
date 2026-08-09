using AcademiadoZE.Domain.Common;

namespace AcademiadoZE.Domain.Entities;
//Mario Cesar alves Júnior

public class AcessoColaborador : Entity
{
    public Colaborador Colaborador { get; private set; }
    public DateTime DataHoraChegada { get; private set; }
    public DateTime? DataHoraSaida { get; private set; }

 
    private AcessoColaborador(int id, Colaborador colaborador, DateTime dataHoraChegada, DateTime? dataHoraSaida)
        : base(id)
    {
        Colaborador = colaborador;
        DataHoraChegada = dataHoraChegada;
        DataHoraSaida = dataHoraSaida;
    }

   
    public static Result<AcessoColaborador> Criar(int id, Colaborador? colaborador, DateTime dataHoraChegada)
    {
        var notifications = new List<Notification>();

        if (colaborador == null)
            notifications.Add(new Notification("Colaborador", "COLABORADOR_OBRIGATORIO"));

        if (dataHoraChegada == default)
            notifications.Add(new Notification("DataHoraChegada", "DATA_HORA_CHEGADA_OBRIGATORIA"));

        if (notifications.Count != 0)
            return Result<AcessoColaborador>.Failure(notifications);

        return Result<AcessoColaborador>.Success(new AcessoColaborador(id, colaborador!, dataHoraChegada, dataHoraSaida: null));
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
