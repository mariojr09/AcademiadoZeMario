// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Services;

namespace AcademiadoZE.Domain.Tests.Services;

public class NormalizadoServiceTests
{
    [Theory]
    [InlineData(null, true)] [InlineData("", true)] [InlineData(" ", true)]
    [InlineData("\t\r\n", true)] [InlineData("a", false)] [InlineData("  a  ", false)]
    public void TextoVazioOuNulo_DeveClassificarCorretamente(string? entrada, bool esperado) =>
        Assert.Equal(esperado, NormalizadoService.TextoVazioOuNulo(entrada));

    [Theory]
    [InlineData(null, "")] [InlineData("", "")] [InlineData("  Mario   Cesar  ", "Mario Cesar")]
    [InlineData("Mario\tCesar", "Mario Cesar")] [InlineData("Mario\r\nCesar", "Mario Cesar")]
    public void LimparEspacos_DeveNormalizar(string? entrada, string esperado) => Assert.Equal(esperado, NormalizadoService.LimparEspacos(entrada));

    [Theory]
    [InlineData(null, "")] [InlineData("a b c", "abc")] [InlineData("  abc  ", "abc")]
    public void LimparTodosEspacos_DeveRemoverEspacoComum(string? entrada, string esperado) => Assert.Equal(esperado, NormalizadoService.LimparTodosEspacos(entrada));

    [Theory]
    [InlineData(null, "")] [InlineData("Mario", "MARIO")] [InlineData("áÇ", "ÁÇ")]
    public void ParaMaiusculo_DeveConverter(string? entrada, string esperado) => Assert.Equal(esperado, NormalizadoService.ParaMaiusculo(entrada));

    [Theory]
    [InlineData(null, "")] [InlineData("MARIO", "mario")] [InlineData("ÁÇ", "áç")]
    public void ParaMinusculo_DeveConverter(string? entrada, string esperado) => Assert.Equal(esperado, NormalizadoService.ParaMinusculo(entrada));

    [Theory]
    [InlineData(null, "")] [InlineData("(11) 98765-4321", "11987654321")] [InlineData("abc123", "123")]
    [InlineData("sem dígitos", "")] [InlineData("0 1 2", "012")]
    public void LimparEDigitos_DeveManterSomenteDigitos(string? entrada, string esperado) => Assert.Equal(esperado, NormalizadoService.LimparEDigitos(entrada));
}
