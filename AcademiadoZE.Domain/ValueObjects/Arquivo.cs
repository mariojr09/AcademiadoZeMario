namespace AcademiadoZE.Domain.ValueObjects;
//Mario Cesar alves Júnior
public record Arquivo
{
    public string NomeArquivo { get; }
    public string Caminho { get; }

    public Arquivo(string nomeArquivo, string caminho)
    {
        if (string.IsNullOrWhiteSpace(nomeArquivo))
            throw new Exception("NOME_ARQUIVO_INVALIDO");

        if (string.IsNullOrWhiteSpace(caminho))
            throw new Exception("CAMINHO_ARQUIVO_INVALIDO");

        NomeArquivo = nomeArquivo;
        Caminho = caminho;
    }
}
