// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiadoZE.Domain.Tests.ValueObjects;

public class EnderecoEArquivoTests
{
    [Fact]
    public void EnderecoValido_DeveSerCriado()
        => Assert.True(
            Endereco.Criar(
                TestData.LogradouroValido(),
                "10",
                null
            ).IsSuccess);

    [Fact]
    public void Endereco_DeveNormalizarNumero()
        => Assert.Equal(
            "10 A",
            Endereco.Criar(
                TestData.LogradouroValido(),
                " 10   A ",
                null
            ).Value!.Numero);

    [Fact]
    public void Endereco_DeveNormalizarComplemento()
        => Assert.Equal(
            "Apto 2",
            Endereco.Criar(
                TestData.LogradouroValido(),
                "10",
                " Apto   2 "
            ).Value!.Complemento);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Endereco_ComComplementoVazio_DeveConverterParaNulo(
        string? valor)
        => Assert.Null(
            Endereco.Criar(
                TestData.LogradouroValido(),
                "10",
                valor
            ).Value!.Complemento);

    [Fact]
    public void Endereco_SemLogradouro_DeveFalhar()
        => Assert.Contains(
            Endereco.Criar(
                null,
                "10",
                null
            ).Notifications,
            n => n.Mensagem == "LOGRADOURO_OBRIGATORIO");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Endereco_SemNumero_DeveFalhar(string? numero)
        => Assert.Contains(
            Endereco.Criar(
                TestData.LogradouroValido(),
                numero,
                null
            ).Notifications,
            n => n.Mensagem == "NUMERO_OBRIGATORIO");

    [Fact]
    public void Endereco_DeveAcumularErros()
        => Assert.Equal(
            2,
            Endereco.Criar(
                null,
                null,
                null
            ).Notifications.Count);

    [Fact]
    public void ArquivoValido_DeveSerCriado()
    {
        var bytes = new byte[] { 1, 2, 3, 4 };

        var result = Arquivo.Criar(bytes);

        Assert.True(result.IsSuccess);
        Assert.Equal(bytes, result.Value!.Conteudo);
    }

    [Fact]
    public void Arquivo_DeveManterConteudo()
    {
        var bytes = new byte[] { 10, 20, 30 };

        var arquivo = Arquivo.Criar(bytes).Value!;

        Assert.Equal(bytes, arquivo.Conteudo);
    }

    [Fact]
    public void Arquivo_Nulo_DeveFalhar()
    {
        var result = Arquivo.Criar(null);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "ARQUIVO_CONTEUDO_OBRIGATORIO");
    }

    [Fact]
    public void Arquivo_Vazio_DeveFalhar()
    {
        var result = Arquivo.Criar(Array.Empty<byte>());

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "ARQUIVO_CONTEUDO_OBRIGATORIO");
    }

    [Fact]
    public void Arquivo_ComUmByte_DeveSerCriado()
    {
        var result = Arquivo.Criar(new byte[] { 1 });

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Conteudo);
    }
}