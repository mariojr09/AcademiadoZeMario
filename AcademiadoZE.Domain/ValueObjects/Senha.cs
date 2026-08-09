using AcademiadoZE.Domain.Common;
using AcademiadoZE.Domain.Services;

namespace AcademiadoZE.Domain.ValueObjects;
//Mario Cesar alves Júnior


public record Senha
{
    public string Hash { get; }

    private Senha(string hash)
    {
        Hash = hash;
    }

    public static Result<Senha> Criar(string? hashJaCalculado)
    {
        if (NormalizadoService.TextoVazioOuNulo(hashJaCalculado))
            return Result<Senha>.Failure("Senha", "SENHA_HASH_OBRIGATORIO");

        return Result<Senha>.Success(new Senha(hashJaCalculado!));
    }
}

public static class PoliticaSenha
{
    public const int TamanhoMinimo = 8;

    public static Result<string> Validar(string? senhaEmTextoPuro)
    {
        var notifications = new List<Notification>();

        if (NormalizadoService.TextoVazioOuNulo(senhaEmTextoPuro))
        {
            notifications.Add(new Notification("Senha", "SENHA_OBRIGATORIA"));
            return Result<string>.Failure(notifications);
        }

        if (senhaEmTextoPuro!.Length < TamanhoMinimo)
            notifications.Add(new Notification("Senha", "SENHA_MUITO_CURTA"));

        if (!senhaEmTextoPuro.Any(char.IsUpper))
            notifications.Add(new Notification("Senha", "SENHA_SEM_MAIUSCULA"));

        if (!senhaEmTextoPuro.Any(char.IsDigit))
            notifications.Add(new Notification("Senha", "SENHA_SEM_NUMERO"));

        if (notifications.Count != 0)
            return Result<string>.Failure(notifications);

        return Result<string>.Success(senhaEmTextoPuro);
    }
}
