// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Common;
using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Enums;

namespace AcademiadoZE.Domain.Tests.Entities;

public class ColaboradorTests
{
    private static Result<Colaborador> Criar(string? nome = "João Silva", string? cpf = "11144477735", DateOnly? nascimento = null, string? telefone = "11987654321", string? email = "joao@example.com", string? senha = "hash", DateOnly? admissao = null, ColaboradorTipo tipo = ColaboradorTipo.Instrutor, ColaboradorVinculo vinculo = ColaboradorVinculo.Clt) =>
        Colaborador.Criar(1, nome, cpf, nascimento ?? new DateOnly(1990, 1, 1), telefone, email, TestData.LogradouroValido(), "10", null, senha, null, admissao ?? new DateOnly(2020, 1, 1), tipo, vinculo);

    [Fact] public void Criar_ComDadosValidos_DevePreencherCampos() { var c = Criar().Value!; Assert.Equal(ColaboradorTipo.Instrutor, c.Tipo); Assert.Equal(ColaboradorVinculo.Clt, c.Vinculo); Assert.Equal(new DateOnly(2020, 1, 1), c.DataAdmissao); }
    [Fact] public void Criar_DeveNormalizarNome() => Assert.Equal("João Silva", Criar(nome: " João   Silva ").Value!.Nome);
    [Theory] [InlineData(null)] [InlineData("")] [InlineData(" ")]
    public void Criar_SemNome_DeveFalhar(string? nome) => Assert.Contains(Criar(nome: nome).Notifications, n => n.Mensagem == "NOME_OBRIGATORIO");
    [Fact] public void Criar_SemNascimento_DeveFalhar() => Assert.Contains(Criar(nascimento: default(DateOnly)).Notifications, n => n.Mensagem == "DATA_NASCIMENTO_OBRIGATORIO");
    [Fact] public void Criar_MenorDeDezoitoAnos_DeveFalhar() => Assert.Contains(Criar(nascimento: DateOnly.FromDateTime(DateTime.Today.AddYears(-18).AddDays(1))).Notifications, n => n.Mensagem == "DATA_NASCIMENTO_MINIMA_INVALIDA");
    [Fact] public void Criar_ExatamenteComDezoitoAnos_DeveTerSucesso() => Assert.True(Criar(nascimento: DateOnly.FromDateTime(DateTime.Today.AddYears(-18))).IsSuccess);
    [Fact] public void Criar_SemAdmissao_DeveFalhar() => Assert.Contains(Criar(admissao: default(DateOnly)).Notifications, n => n.Mensagem == "DATA_ADMISSAO_OBRIGATORIO");
    [Fact] public void Criar_ComAdmissaoFutura_DeveFalhar() => Assert.Contains(Criar(admissao: DateOnly.FromDateTime(DateTime.Today.AddDays(1))).Notifications, n => n.Mensagem == "DATA_ADMISSAO_MAIOR_ATUAL");
    [Theory] [InlineData(99)] [InlineData(-1)]
    public void Criar_ComTipoInvalido_DeveFalhar(int tipo) => Assert.Contains(Criar(tipo: (ColaboradorTipo)tipo).Notifications, n => n.Mensagem == "TIPO_COLABORADOR_INVALIDO");
    [Theory] [InlineData(99)] [InlineData(-1)]
    public void Criar_ComVinculoInvalido_DeveFalhar(int vinculo) => Assert.Contains(Criar(vinculo: (ColaboradorVinculo)vinculo).Notifications, n => n.Mensagem == "VINCULO_COLABORADOR_INVALIDO");
    [Fact] public void AdministradorClt_DeveSerValido() => Assert.True(Criar(tipo: ColaboradorTipo.Administrador, vinculo: ColaboradorVinculo.Clt).IsSuccess);
    [Fact] public void AdministradorEstagiario_DeveFalhar() => Assert.Contains(Criar(tipo: ColaboradorTipo.Administrador, vinculo: ColaboradorVinculo.Estagio).Notifications, n => n.Mensagem == "ADMINISTRADOR_CLT_INVALIDO");
    [Fact] public void InstrutorEstagiario_DeveSerValido() => Assert.True(Criar(tipo: ColaboradorTipo.Instrutor, vinculo: ColaboradorVinculo.Estagio).IsSuccess);
    [Fact] public void Criar_ComCpfInvalido_DevePropagarErro() => Assert.Contains(Criar(cpf: "123").Notifications, n => n.Mensagem == "CPF_DIGITOS");
    [Fact] public void Criar_ComContatoInvalido_DeveAcumularErros() => Assert.True(Criar(telefone: null, email: null, senha: null).Notifications.Count >= 3);
}
