namespace AcademiadoZE.Domain.ValueObjects;
//Mario Cesar alves Júnior
public record Senha
{
    public string Hash { get; }

    public Senha(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new Exception("SENHA_INVALIDA");

        Hash = hash;
    }
}