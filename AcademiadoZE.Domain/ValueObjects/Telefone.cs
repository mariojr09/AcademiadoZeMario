namespace AcademiadoZE.Domain.ValueObjects;
//Mario Cesar alves Júnior
public record Telefone
{
    public string Valor { get; }

    public Telefone(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new Exception("TELEFONE_INVALIDO");

        Valor = valor;
    }
}