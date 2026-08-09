using AcademiadoZE.Domain.Common;
using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Services;

namespace AcademiadoZE.Domain.ValueObjects;
//Mario Cesar alves Júnior

public record Endereco
{
    public Logradouro Logradouro { get; }
    public string Numero { get; }
    public string? Complemento { get; }

    private Endereco(Logradouro logradouro, string numero, string? complemento)
    {
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
    }

    public static Result<Endereco> Criar(Logradouro? logradouro, string? numero, string? complemento)
    {
        var notifications = new List<Notification>();

        if (logradouro == null)
            notifications.Add(new Notification("Endereco", "LOGRADOURO_OBRIGATORIO"));

        if (NormalizadoService.TextoVazioOuNulo(numero))
            notifications.Add(new Notification("Numero", "NUMERO_OBRIGATORIO"));
        else
            numero = NormalizadoService.LimparEspacos(numero);

        
        complemento = NormalizadoService.TextoVazioOuNulo(complemento) ? null : NormalizadoService.LimparEspacos(complemento);

        if (notifications.Count != 0)
            return Result<Endereco>.Failure(notifications);

        return Result<Endereco>.Success(new Endereco(logradouro!, numero!, complemento));
    }
}
