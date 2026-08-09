using AcademiadoZE.Domain.Exceptions;

namespace AcademiadoZE.Domain.Entities;
//Mario Cesar alves Júnior


public abstract class Entity
{
    public int Id { get; protected set; }

    protected Entity(int id = 0)
    {
        if (id < 0) throw new DomainException("ID_NEGATIVO");
        Id = id;
    }

    public bool EhTransiente => Id == 0;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity outraEntidade) return false;
        if (ReferenceEquals(this, outraEntidade)) return true;
        if (GetType() != outraEntidade.GetType()) return false;
        if (EhTransiente || outraEntidade.EhTransiente) return false;

        return Id == outraEntidade.Id;
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? esquerda, Entity? direita)
    {
        if (esquerda is null && direita is null) return true;
        if (esquerda is null || direita is null) return false;
        return esquerda.Equals(direita);
    }

    public static bool operator !=(Entity? esquerda, Entity? direita) => !(esquerda == direita);
}
