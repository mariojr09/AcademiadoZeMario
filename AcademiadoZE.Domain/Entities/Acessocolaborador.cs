namespace AcademiadoZE.Domain.Entities;
//Mario Cesar alves Júnior
public class AcessoColaborador : Entity
{
    public Colaborador Colaborador { get; protected set; }
    public DateTime DataHoraChegada { get; protected set; }
    public DateTime? DataHoraSaida { get; protected set; }

    public AcessoColaborador(int id, Colaborador colaborador, DateTime dataHoraChegada, DateTime? dataHoraSaida)
        : base(id)
    {
        Colaborador = colaborador;
        DataHoraChegada = dataHoraChegada;
        DataHoraSaida = dataHoraSaida;
    }
}