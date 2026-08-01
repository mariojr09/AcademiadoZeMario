namespace AcademiadoZE.Domain.Entities;
//Mario Cesar alves Júnior
public abstract class Entity
{
    public int Id { get; protected set; }

    protected Entity(int id = 0)
    {
        if (id < 0) throw new Exception("ID_NEGATIVO");
        Id = id;
    }
}