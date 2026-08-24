// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Entities;

namespace AcademiadoZE.Domain.Tests.Entities;

public class LogradouroTests
{
    [Fact] public void Criar_ComDadosValidos_DevePreencherCampos() { var l = TestData.LogradouroValido(5); Assert.Equal(5, l.Id); Assert.Equal("01001000", l.Cep.Valor); Assert.Equal("Brasil", l.Pais); Assert.Equal("SP", l.Estado); }
    [Fact] public void Criar_DeveNormalizarTextos() { var l = Logradouro.Criar(1, "01001000", " Brasil  Federal ", " s p ", " São   Paulo ", " Praça   da Sé ", " Rua   Um ").Value!; Assert.Equal("Brasil Federal", l.Pais); Assert.Equal("SP", l.Estado); Assert.Equal("São Paulo", l.Cidade); Assert.Equal("Praça da Sé", l.Bairro); Assert.Equal("Rua Um", l.NomeLogradouro); }
    [Theory]
    [InlineData(null, "CEP_OBRIGATORIO")] [InlineData("123", "CEP_DIGITOS")]
    public void Criar_ComCepInvalido_DeveFalhar(string? cep, string mensagem) => Assert.Contains(Logradouro.Criar(1, cep, "Brasil", "SP", "Cidade", "Bairro", "Rua").Notifications, n => n.Mensagem == mensagem);
    [Theory]
    [InlineData(null, "PAIS_OBRIGATORIO")] [InlineData("", "PAIS_OBRIGATORIO")]
    public void Criar_SemPais_DeveFalhar(string? pais, string mensagem) => Assert.Contains(Logradouro.Criar(1, "01001000", pais, "SP", "Cidade", "Bairro", "Rua").Notifications, n => n.Mensagem == mensagem);
    [Fact] public void Criar_SemEstado_DeveFalhar() => Assert.Contains(Logradouro.Criar(1, "01001000", "Brasil", null, "Cidade", "Bairro", "Rua").Notifications, n => n.Mensagem == "ESTADO_OBRIGATORIO");
    [Fact] public void Criar_SemCidade_DeveFalhar() => Assert.Contains(Logradouro.Criar(1, "01001000", "Brasil", "SP", null, "Bairro", "Rua").Notifications, n => n.Mensagem == "CIDADE_OBRIGATORIO");
    [Fact] public void Criar_SemBairro_DeveFalhar() => Assert.Contains(Logradouro.Criar(1, "01001000", "Brasil", "SP", "Cidade", null, "Rua").Notifications, n => n.Mensagem == "BAIRRO_OBRIGATORIO");
    [Fact] public void Criar_SemNomeLogradouro_DeveFalhar() => Assert.Contains(Logradouro.Criar(1, "01001000", "Brasil", "SP", "Cidade", "Bairro", null).Notifications, n => n.Mensagem == "NOME_LOGRADOURO_OBRIGATORIO");
    [Fact] public void Criar_ComTudoAusente_DeveAcumularSeisErros() => Assert.Equal(6, Logradouro.Criar(1, null, null, null, null, null, null).Notifications.Count);
}
