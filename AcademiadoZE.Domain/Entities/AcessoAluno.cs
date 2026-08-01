namespace AcademiadoZE.Domain.Entities;
//Mario Cesar alves Júnior
public class AcessoAluno : Entity
{
    public Aluno Aluno { get; protected set; }
    public DateTime DataHoraChegada { get; protected set; }
    public DateTime? DataHoraSaida { get; protected set; }

    public AcessoAluno(int id, Aluno aluno, DateTime dataHoraChegada, DateTime? dataHoraSaida)
        : base(id)
    {
        Aluno = aluno;
        DataHoraChegada = dataHoraChegada;
        DataHoraSaida = dataHoraSaida;
    }
}