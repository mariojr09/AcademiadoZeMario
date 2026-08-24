// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiadoZE.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("mario@example.com", "mario@example.com")] [InlineData("MARIO@EXAMPLE.COM", "mario@example.com")]
    [InlineData("  mario@example.com  ", "mario@example.com")] [InlineData("nome.sobrenome@sub.example.com", "nome.sobrenome@sub.example.com")]
    public void Criar_ComEmailValido_DeveNormalizar(string entrada, string esperado) { var r = Email.Criar(entrada); Assert.True(r.IsSuccess); Assert.Equal(esperado, r.Value!.Valor); Assert.Equal(esperado, r.Value.ToString()); }

    [Theory]
    [InlineData(null)] [InlineData("")] [InlineData("   ")] [InlineData("sem-arroba")]
    [InlineData("@example.com")] [InlineData("nome@")]
    [InlineData("nome@example")] [InlineData("nome@.example.com")]
    [InlineData("nome@example.com.")] [InlineData("nome@example..com")] [InlineData("a@b@c.com")]
    public void Criar_ComEmailInvalido_DeveFalhar(string? entrada) { var r = Email.Criar(entrada); Assert.True(r.IsFailure); Assert.Equal("EMAIL_FORMATO", Assert.Single(r.Notifications).Mensagem); }

    [Fact] public void EmailsNormalizados_DevemSerIguais() => Assert.Equal(Email.Criar("A@EXAMPLE.COM").Value, Email.Criar("a@example.com").Value);
}
