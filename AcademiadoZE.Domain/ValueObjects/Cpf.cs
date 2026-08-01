namespace AcademiadoZE.Domain.ValueObjects;
//Mario Cesar alves Júnior
public record Cpf
{
    public string Valor { get; }

    public Cpf(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new Exception("CPF_INVALIDO");

        Valor = valor;
    }
}