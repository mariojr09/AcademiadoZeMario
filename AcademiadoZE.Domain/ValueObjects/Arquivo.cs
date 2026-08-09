using AcademiadoZE.Domain.Common;
using AcademiadoZE.Domain.Services;

namespace AcademiadoZE.Domain.ValueObjects;
//Mario Cesar alves Júnior

public record Arquivo
{
    public string NomeArquivo { get; }
    public string Caminho { get; }

    private Arquivo(string nomeArquivo, string caminho)
    {
        NomeArquivo = nomeArquivo;
        Caminho = caminho;
    }

  
    public static Result<Arquivo> Criar(string? nomeArquivo, string? caminho, IReadOnlyCollection<string>? extensoesPermitidas = null)
    {
        var notifications = new List<Notification>();

        if (NormalizadoService.TextoVazioOuNulo(nomeArquivo))
            notifications.Add(new Notification("NomeArquivo", "ARQUIVO_NOME_OBRIGATORIO"));
        else
            nomeArquivo = NormalizadoService.LimparEspacos(nomeArquivo);

        if (NormalizadoService.TextoVazioOuNulo(caminho))
            notifications.Add(new Notification("Caminho", "ARQUIVO_CAMINHO_OBRIGATORIO"));
        else
            caminho = NormalizadoService.LimparEspacos(caminho);

        if (extensoesPermitidas is { Count: > 0 } && !NormalizadoService.TextoVazioOuNulo(nomeArquivo))
        {
            var extensao = Path.GetExtension(nomeArquivo).TrimStart('.').ToLowerInvariant();
            var permitida = extensoesPermitidas.Select(e => e.TrimStart('.').ToLowerInvariant()).Contains(extensao);

            if (!permitida)
                notifications.Add(new Notification("NomeArquivo", "ARQUIVO_EXTENSAO_NAO_PERMITIDA"));
        }

        if (notifications.Count != 0)
            return Result<Arquivo>.Failure(notifications);

        return Result<Arquivo>.Success(new Arquivo(nomeArquivo!, caminho!));
    }
}
