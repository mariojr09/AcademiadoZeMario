using AcademiadoZE.Domain.Entities;

namespace AcademiadoZE.Domain.ValueObjects;
//Mario Cesar alves Júnior
public record Endereco
{
    public Logradouro Logradouro { get; }
    public string Numero { get; }
    public string? Complemento { get; }

    public Endereco(Logradouro logradouro, string numero, string? complemento)
    {
        if (logradouro is null)
            throw new Exception("LOGRADOURO_INVALIDO");

        if (string.IsNullOrWhiteSpace(numero))
            throw new Exception("NUMERO_INVALIDO");

        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
    }
}