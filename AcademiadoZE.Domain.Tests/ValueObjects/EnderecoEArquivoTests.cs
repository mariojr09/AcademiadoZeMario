// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiadoZE.Domain.Tests.ValueObjects;

public class EnderecoEArquivoTests
{
    [Fact] public void EnderecoValido_DeveSerCriado() => Assert.True(Endereco.Criar(TestData.LogradouroValido(), "10", null).IsSuccess);
    [Fact] public void Endereco_DeveNormalizarNumero() => Assert.Equal("10 A", Endereco.Criar(TestData.LogradouroValido(), " 10   A ", null).Value!.Numero);
    [Fact] public void Endereco_DeveNormalizarComplemento() => Assert.Equal("Apto 2", Endereco.Criar(TestData.LogradouroValido(), "10", " Apto   2 ").Value!.Complemento);
    [Theory] [InlineData(null)] [InlineData("")] [InlineData("  ")]
    public void Endereco_ComComplementoVazio_DeveConverterParaNulo(string? valor) => Assert.Null(Endereco.Criar(TestData.LogradouroValido(), "10", valor).Value!.Complemento);
    [Fact] public void Endereco_SemLogradouro_DeveFalhar() => Assert.Contains(Endereco.Criar(null, "10", null).Notifications, n => n.Mensagem == "LOGRADOURO_OBRIGATORIO");
    [Theory] [InlineData(null)] [InlineData("")] [InlineData(" ")]
    public void Endereco_SemNumero_DeveFalhar(string? numero) => Assert.Contains(Endereco.Criar(TestData.LogradouroValido(), numero, null).Notifications, n => n.Mensagem == "NUMERO_OBRIGATORIO");
    [Fact] public void Endereco_DeveAcumularErros() => Assert.Equal(2, Endereco.Criar(null, null, null).Notifications.Count);

    [Fact] public void ArquivoValido_DeveSerCriado() => Assert.True(Arquivo.Criar("foto.png", "/img/foto.png").IsSuccess);
    [Fact] public void Arquivo_DeveNormalizarNomeECaminho() { var a = Arquivo.Criar(" foto   final.png ", " /img/foto   final.png ").Value!; Assert.Equal("foto final.png", a.NomeArquivo); Assert.Equal("/img/foto final.png", a.Caminho); }
    [Theory] [InlineData(null)] [InlineData("")] [InlineData(" ")]
    public void Arquivo_SemNome_DeveFalhar(string? nome) => Assert.Contains(Arquivo.Criar(nome, "/tmp/a").Notifications, n => n.Mensagem == "ARQUIVO_NOME_OBRIGATORIO");
    [Theory] [InlineData(null)] [InlineData("")] [InlineData(" ")]
    public void Arquivo_SemCaminho_DeveFalhar(string? caminho) => Assert.Contains(Arquivo.Criar("a.pdf", caminho).Notifications, n => n.Mensagem == "ARQUIVO_CAMINHO_OBRIGATORIO");
    [Theory] [InlineData("foto.PNG")] [InlineData("documento.pdf")]
    public void Arquivo_ComExtensaoPermitida_DeveTerSucesso(string nome) => Assert.True(Arquivo.Criar(nome, "/tmp/a", [".png", "PDF"]).IsSuccess);
    [Fact] public void Arquivo_ComExtensaoNaoPermitida_DeveFalhar() => Assert.Contains(Arquivo.Criar("virus.exe", "/tmp/a", ["pdf"]).Notifications, n => n.Mensagem == "ARQUIVO_EXTENSAO_NAO_PERMITIDA");
    [Fact] public void Arquivo_SemListaDeExtensoes_DeveAceitarQualquerExtensao() => Assert.True(Arquivo.Criar("a.xyz", "/tmp/a").IsSuccess);
    [Fact] public void Arquivo_DeveAcumularNomeECaminhoAusentes() => Assert.Equal(2, Arquivo.Criar(null, null).Notifications.Count);
}
