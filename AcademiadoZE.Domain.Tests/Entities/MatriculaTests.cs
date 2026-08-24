// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Common;
using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Enums;

namespace AcademiadoZE.Domain.Tests.Entities;

public class MatriculaTests
{
    private static Result<Matricula> Criar(Aluno? aluno = null, MatriculaPlano plano = MatriculaPlano.Mensal, DateOnly? inicio = null, DateOnly? final = null, string? objetivo = "Condicionamento", Restricao restricoes = Restricao.None, string? observacoes = null, AcademiadoZE.Domain.ValueObjects.Arquivo? laudo = null) =>
        Matricula.Criar(1, aluno ?? TestData.AlunoValido(), plano, inicio ?? new DateOnly(2026, 1, 1), final ?? new DateOnly(2026, 2, 1), objetivo, restricoes, observacoes, laudo);

    [Theory] [InlineData(MatriculaPlano.Mensal)] [InlineData(MatriculaPlano.Trimestral)] [InlineData(MatriculaPlano.Semestral)] [InlineData(MatriculaPlano.Anual)]
    public void Criar_DeveAceitarPlanosDefinidos(MatriculaPlano plano) => Assert.True(Criar(plano: plano).IsSuccess);
    [Fact] public void Criar_DevePreencherCampos() { var m = Criar().Value!; Assert.Equal("Condicionamento", m.Objetivo); Assert.Equal(Restricao.None, m.Restricoes); Assert.Equal(new DateOnly(2026, 1, 1), m.DataInicio); }
    [Fact] public void Criar_DeveNormalizarObjetivo() => Assert.Equal("Ganhar massa", Criar(objetivo: " Ganhar   massa ").Value!.Objetivo);
    [Fact] public void Criar_ComPlanoInvalido_DeveFalhar() => Assert.Contains(Criar(plano: (MatriculaPlano)99).Notifications, n => n.Mensagem == "PLANO_INVALIDO");
    [Fact] public void Criar_SemDataInicio_DeveFalhar() => Assert.Contains(Criar(inicio: default(DateOnly)).Notifications, n => n.Mensagem == "DATA_INICIO_OBRIGATORIA");
    [Fact] public void Criar_SemDataFinal_DeveFalhar() => Assert.Contains(Criar(final: default(DateOnly)).Notifications, n => n.Mensagem == "DATA_FINAL_OBRIGATORIA");
    [Fact] public void Criar_ComFinalAnterior_DeveFalhar() => Assert.Contains(Criar(final: new DateOnly(2025, 12, 31)).Notifications, n => n.Mensagem == "DATA_FINAL_ANTERIOR_A_INICIO");
    [Fact] public void Criar_ComFinalIgualAoInicio_DeveFalhar() => Assert.Contains(Criar(final: new DateOnly(2026, 1, 1)).Notifications, n => n.Mensagem == "DATA_FINAL_ANTERIOR_A_INICIO");
    [Theory] [InlineData(null)] [InlineData("")] [InlineData(" ")]
    public void Criar_SemObjetivo_DeveFalhar(string? objetivo) => Assert.Contains(Criar(objetivo: objetivo).Notifications, n => n.Mensagem == "OBJETIVO_OBRIGATORIO");
    [Theory] [InlineData(1)] [InlineData(3)] [InlineData(32)] [InlineData(63)]
    public void Criar_ComRestricoesValidasEObservacao_DeveTerSucesso(int restricoes) => Assert.True(Criar(restricoes: (Restricao)restricoes, observacoes: "Acompanhamento").IsSuccess);
    [Fact] public void Criar_ComRestricaoDesconhecida_DeveFalhar() => Assert.Contains(Criar(restricoes: (Restricao)64, observacoes: "x").Notifications, n => n.Mensagem == "RESTRICAO_INVALIDA");
    [Fact] public void RestricaoSemJustificativa_DeveFalhar() => Assert.Contains(Criar(restricoes: Restricao.Diabetes).Notifications, n => n.Mensagem == "RESTRICAO_SEM_JUSTIFICATIVA");
    [Fact] public void RestricaoComObservacao_DeveTerSucesso() => Assert.True(Criar(restricoes: Restricao.Diabetes, observacoes: "Controlada").IsSuccess);
    [Fact] public void RestricaoComLaudo_DeveTerSucesso() => Assert.True(Criar(restricoes: Restricao.Diabetes, laudo: TestData.ArquivoValido()).IsSuccess);
    [Fact] public void ObservacaoVazia_DeveSerConvertidaParaNulo() => Assert.Null(Criar(observacoes: " ").Value!.ObservacoesRestricoes);
    [Fact] public void AnexarLaudoMedico_DeveSubstituirLaudo() { var m = Criar().Value!; var a = TestData.ArquivoValido(); m.AnexarLaudoMedico(a); Assert.Same(a, m.LaudoMedico); }
}
