// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiadoZE.Domain.Tests.ValueObjects;

public class CpfTests
{
    [Theory]
    [InlineData("52998224725", "52998224725")] [InlineData("529.982.247-25", "52998224725")]
    [InlineData("11144477735", "11144477735")] [InlineData(" 111.444.777-35 ", "11144477735")]
    public void Criar_ComCpfValido_DeveNormalizar(string entrada, string esperado) { var r = Cpf.Criar(entrada); Assert.True(r.IsSuccess); Assert.Equal(esperado, r.Value!.Valor); Assert.Equal(esperado, r.Value.ToString()); }

    [Theory]
    [InlineData(null, "CPF_OBRIGATORIO")] [InlineData("", "CPF_OBRIGATORIO")] [InlineData("   ", "CPF_OBRIGATORIO")]
    [InlineData("123", "CPF_DIGITOS")] [InlineData("123456789012", "CPF_DIGITOS")]
    [InlineData("00000000000", "CPF_INVALIDO")] [InlineData("11111111111", "CPF_INVALIDO")]
    [InlineData("52998224724", "CPF_INVALIDO")] [InlineData("12345678900", "CPF_INVALIDO")]
    public void Criar_ComCpfInvalido_DeveFalhar(string? entrada, string mensagem) { var r = Cpf.Criar(entrada); Assert.True(r.IsFailure); Assert.Equal(mensagem, Assert.Single(r.Notifications).Mensagem); }

    [Fact] public void CpfsComMesmoValor_DevemSerIguais() => Assert.Equal(Cpf.Criar("52998224725").Value, Cpf.Criar("529.982.247-25").Value);
}
