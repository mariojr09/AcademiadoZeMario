namespace AcademiadoZE.Domain.ValueObjects;
//Mario Cesar alves Júnior
public record Email
{
    public string Valor { get; }

    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor) || !valor.Contains('@'))
            throw new Exception("EMAIL_INVALIDO");

        Valor = valor;
    }
}
