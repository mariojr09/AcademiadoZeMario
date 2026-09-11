using AcademiadoZE.Domain.Common;

namespace AcademiadoZE.Domain.ValueObjects;

// Mario Cesar Alves Júnior
public record Arquivo
{
    public byte[] Conteudo { get; }

    private Arquivo(byte[] conteudo)
    {
        Conteudo = conteudo;
    }

    public static Result<Arquivo> Criar(byte[]? conteudo)
    {
        if (conteudo == null || conteudo.Length == 0)
        {
            return Result<Arquivo>.Failure(
                "Arquivo",
                "ARQUIVO_CONTEUDO_OBRIGATORIO");
        }

        return Result<Arquivo>.Success(
            new Arquivo(conteudo));
    }
}