// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiadoZE.Domain.Tests.ValueObjects;

public class SenhaTests
{
    [Theory]
    [InlineData("hash")] [InlineData("$2a$12$hashcalculado")] [InlineData(" valor-com-espaco ")]
    public void Criar_ComHashPreenchido_DevePreservarValor(string hash) { var r = Senha.Criar(hash); Assert.True(r.IsSuccess); Assert.Equal(hash, r.Value!.Hash); }

    [Theory]
    [InlineData(null)] [InlineData("")] [InlineData("   ")]
    public void Criar_SemHash_DeveFalhar(string? hash) => Assert.Equal("SENHA_HASH_OBRIGATORIO", Assert.Single(Senha.Criar(hash).Notifications).Mensagem);

    [Theory]
    [InlineData("Abcdefg1")] [InlineData("SenhaMuitoLonga9")] [InlineData("Ábcdefg2")]
    public void Politica_ComSenhaValida_DeveTerSucesso(string senha) { var r = PoliticaSenha.Validar(senha); Assert.True(r.IsSuccess); Assert.Equal(senha, r.Value); }

    [Theory]
    [InlineData(null, "SENHA_OBRIGATORIA")] [InlineData("", "SENHA_OBRIGATORIA")] [InlineData("       ", "SENHA_OBRIGATORIA")]
    public void Politica_SemSenha_DeveFalhar(string? senha, string mensagem) => Assert.Equal(mensagem, Assert.Single(PoliticaSenha.Validar(senha).Notifications).Mensagem);

    [Fact] public void Politica_SenhaCurta_DeveNotificarTamanho() => Assert.Contains(PoliticaSenha.Validar("Abc1").Notifications, n => n.Mensagem == "SENHA_MUITO_CURTA");
    [Fact] public void Politica_SemMaiuscula_DeveNotificar() => Assert.Contains(PoliticaSenha.Validar("abcdefg1").Notifications, n => n.Mensagem == "SENHA_SEM_MAIUSCULA");
    [Fact] public void Politica_SemNumero_DeveNotificar() => Assert.Contains(PoliticaSenha.Validar("Abcdefgh").Notifications, n => n.Mensagem == "SENHA_SEM_NUMERO");
    [Fact] public void Politica_PodeAcumularTresErros() => Assert.Equal(3, PoliticaSenha.Validar("abc").Notifications.Count);
}
