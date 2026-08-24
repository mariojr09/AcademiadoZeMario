// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Entities;

namespace AcademiadoZE.Domain.Tests.Entities;

public class AlunoTests
{
    private static AcademiadoZE.Domain.Common.Result<Aluno> Criar(string? nome = "Maria Silva", string? cpf = "52998224725", DateOnly? nascimento = null, string? telefone = "11987654321", string? email = "maria@example.com", Logradouro? logradouro = null, string? numero = "10", string? senha = "hash") =>
        Aluno.Criar(1, nome, cpf, nascimento ?? new DateOnly(2000, 1, 1), telefone, email, logradouro ?? TestData.LogradouroValido(), numero, null, senha, null);

    [Fact] public void Criar_ComDadosValidos_DevePreencherPessoa() { var a = Criar().Value!; Assert.Equal("Maria Silva", a.Nome); Assert.Equal("52998224725", a.Cpf.Valor); Assert.Equal("11987654321", a.Telefone.Valor); Assert.Equal("maria@example.com", a.Email.Valor); }
    [Fact] public void Criar_DeveNormalizarNome() => Assert.Equal("Maria da Silva", Criar(nome: " Maria   da Silva ").Value!.Nome);
    [Theory] [InlineData(null)] [InlineData("")] [InlineData(" ")]
    public void Criar_SemNome_DeveFalhar(string? nome) => Assert.Contains(Criar(nome: nome).Notifications, n => n.Mensagem == "NOME_OBRIGATORIO");
    [Fact] public void Criar_SemNascimento_DeveFalhar() => Assert.Contains(Criar(nascimento: default(DateOnly)).Notifications, n => n.Mensagem == "DATA_NASCIMENTO_OBRIGATORIO");
    [Fact] public void Criar_ComMenosDeQuatorzeAnos_DeveFalhar() => Assert.Contains(Criar(nascimento: DateOnly.FromDateTime(DateTime.Today.AddYears(-14).AddDays(1))).Notifications, n => n.Mensagem == "DATA_NASCIMENTO_MINIMA_INVALIDA");
    [Fact] public void Criar_ExatamenteComQuatorzeAnos_DeveTerSucesso() => Assert.True(Criar(nascimento: DateOnly.FromDateTime(DateTime.Today.AddYears(-14))).IsSuccess);
    [Fact] public void Criar_ComCpfInvalido_DevePropagarErro() => Assert.Contains(Criar(cpf: "123").Notifications, n => n.Mensagem == "CPF_DIGITOS");
    [Fact] public void Criar_ComTelefoneInvalido_DevePropagarErro() => Assert.Contains(Criar(telefone: "123").Notifications, n => n.Mensagem == "TELEFONE_DIGITOS");
    [Fact] public void Criar_ComEmailInvalido_DevePropagarErro() => Assert.Contains(Criar(email: "invalido").Notifications, n => n.Mensagem == "EMAIL_FORMATO");
    [Fact] public void Criar_SemSenha_DevePropagarErro() => Assert.Contains(Criar(senha: null).Notifications, n => n.Mensagem == "SENHA_HASH_OBRIGATORIO");
    [Fact] public void Criar_ComVariosDadosInvalidos_DeveAcumularErros() => Assert.True(Criar(nome: null, cpf: null, nascimento: default(DateOnly), telefone: null, email: null, numero: null, senha: null).Notifications.Count >= 7);
}
