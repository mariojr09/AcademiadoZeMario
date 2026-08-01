namespace AcademiadoZE.Domain.ValueObjects;
//Mario Cesar alves Júnior
public record Cep
{
    public string Valor { get; }

    public Cep(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new Exception("CEP_INVALIDO");

        Valor = valor;
    }
}