// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiadoZE.Domain.Tests.ValueObjects;

public class ContatoECepTests
{
    [Theory]
    [InlineData("11987654321", "11987654321")] [InlineData("(11) 98765-4321", "11987654321")]
    [InlineData("11 98765 4321", "11987654321")]
    public void TelefoneValido_DeveSerNormalizado(string entrada, string esperado) { var r = Telefone.Criar(entrada); Assert.True(r.IsSuccess); Assert.Equal(esperado, r.Value!.Valor); Assert.Equal(esperado, r.Value.ToString()); }

    [Theory]
    [InlineData(null, "TELEFONE_OBRIGATORIO")] [InlineData("", "TELEFONE_OBRIGATORIO")]
    [InlineData("   ", "TELEFONE_OBRIGATORIO")] [InlineData("1198765432", "TELEFONE_DIGITOS")]
    [InlineData("119876543210", "TELEFONE_DIGITOS")] [InlineData("abcdefghijk", "TELEFONE_DIGITOS")]
    public void TelefoneInvalido_DeveFalhar(string? entrada, string mensagem) { var r = Telefone.Criar(entrada); Assert.Equal(mensagem, Assert.Single(r.Notifications).Mensagem); }

    [Theory]
    [InlineData("01001000", "01001000")] [InlineData("01001-000", "01001000")] [InlineData(" 01001-000 ", "01001000")]
    public void CepValido_DeveSerNormalizado(string entrada, string esperado) { var r = Cep.Criar(entrada); Assert.True(r.IsSuccess); Assert.Equal(esperado, r.Value!.Valor); Assert.Equal(esperado, r.Value.ToString()); }

    [Theory]
    [InlineData(null, "CEP_OBRIGATORIO")] [InlineData("", "CEP_OBRIGATORIO")] [InlineData(" ", "CEP_OBRIGATORIO")]
    [InlineData("1234567", "CEP_DIGITOS")] [InlineData("123456789", "CEP_DIGITOS")] [InlineData("abcdefgh", "CEP_DIGITOS")]
    public void CepInvalido_DeveFalhar(string? entrada, string mensagem) { var r = Cep.Criar(entrada); Assert.Equal(mensagem, Assert.Single(r.Notifications).Mensagem); }

    [Fact] public void TelefonesNormalizados_DevemSerIguais() => Assert.Equal(Telefone.Criar("11987654321").Value, Telefone.Criar("(11)98765-4321").Value);
    [Fact] public void CepsNormalizados_DevemSerIguais() => Assert.Equal(Cep.Criar("01001000").Value, Cep.Criar("01001-000").Value);
}
