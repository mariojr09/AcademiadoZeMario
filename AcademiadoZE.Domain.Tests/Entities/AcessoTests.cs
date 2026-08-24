// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Entities;

namespace AcademiadoZE.Domain.Tests.Entities;

public class AcessoTests
{
    private static readonly DateTime Chegada = new(2026, 1, 10, 8, 0, 0);

    [Fact] public void AcessoAlunoValido_DeveSerCriadoAberto() { var a = AcessoAluno.Criar(1, TestData.AlunoValido(), Chegada).Value!; Assert.Equal(Chegada, a.DataHoraChegada); Assert.Null(a.DataHoraSaida); }
    [Fact] public void AcessoAluno_SemAluno_DeveFalhar() => Assert.Contains(AcessoAluno.Criar(1, null, Chegada).Notifications, n => n.Mensagem == "ALUNO_OBRIGATORIO");
    [Fact] public void AcessoAluno_SemChegada_DeveFalhar() => Assert.Contains(AcessoAluno.Criar(1, TestData.AlunoValido(), default).Notifications, n => n.Mensagem == "DATA_HORA_CHEGADA_OBRIGATORIA");
    [Fact] public void AcessoAluno_DeveAcumularErrosDeCriacao() => Assert.Equal(2, AcessoAluno.Criar(1, null, default).Notifications.Count);
    [Fact] public void AcessoAluno_DeveRegistrarSaidaPosterior() { var a = AcessoAluno.Criar(1, TestData.AlunoValido(), Chegada).Value!; Assert.True(a.RegistrarSaida(Chegada.AddHours(1)).IsSuccess); Assert.Equal(Chegada.AddHours(1), a.DataHoraSaida); }
    [Fact] public void AcessoAluno_NaoDeveRegistrarSaidaAnterior() { var a = AcessoAluno.Criar(1, TestData.AlunoValido(), Chegada).Value!; Assert.Equal("SAIDA_ANTERIOR_A_CHEGADA", Assert.Single(a.RegistrarSaida(Chegada.AddSeconds(-1)).Notifications).Mensagem); Assert.Null(a.DataHoraSaida); }
    [Fact] public void AcessoAluno_NaoDeveRegistrarDuasSaidas() { var a = AcessoAluno.Criar(1, TestData.AlunoValido(), Chegada).Value!; a.RegistrarSaida(Chegada.AddHours(1)); Assert.Equal("SAIDA_JA_REGISTRADA", Assert.Single(a.RegistrarSaida(Chegada.AddHours(2)).Notifications).Mensagem); }

    [Fact] public void AcessoColaboradorValido_DeveSerCriadoAberto() { var a = AcessoColaborador.Criar(1, TestData.ColaboradorValido(), Chegada).Value!; Assert.Equal(Chegada, a.DataHoraChegada); Assert.Null(a.DataHoraSaida); }
    [Fact] public void AcessoColaborador_SemColaborador_DeveFalhar() => Assert.Contains(AcessoColaborador.Criar(1, null, Chegada).Notifications, n => n.Mensagem == "COLABORADOR_OBRIGATORIO");
    [Fact] public void AcessoColaborador_SemChegada_DeveFalhar() => Assert.Contains(AcessoColaborador.Criar(1, TestData.ColaboradorValido(), default).Notifications, n => n.Mensagem == "DATA_HORA_CHEGADA_OBRIGATORIA");
    [Fact] public void AcessoColaborador_DeveAcumularErrosDeCriacao() => Assert.Equal(2, AcessoColaborador.Criar(1, null, default).Notifications.Count);
    [Fact] public void AcessoColaborador_DeveRegistrarSaidaPosterior() { var a = AcessoColaborador.Criar(1, TestData.ColaboradorValido(), Chegada).Value!; Assert.True(a.RegistrarSaida(Chegada.AddHours(1)).IsSuccess); Assert.Equal(Chegada.AddHours(1), a.DataHoraSaida); }
    [Fact] public void AcessoColaborador_NaoDeveRegistrarSaidaAnterior() { var a = AcessoColaborador.Criar(1, TestData.ColaboradorValido(), Chegada).Value!; Assert.Equal("SAIDA_ANTERIOR_A_CHEGADA", Assert.Single(a.RegistrarSaida(Chegada.AddSeconds(-1)).Notifications).Mensagem); Assert.Null(a.DataHoraSaida); }
    [Fact] public void AcessoColaborador_NaoDeveRegistrarDuasSaidas() { var a = AcessoColaborador.Criar(1, TestData.ColaboradorValido(), Chegada).Value!; a.RegistrarSaida(Chegada.AddHours(1)); Assert.Equal("SAIDA_JA_REGISTRADA", Assert.Single(a.RegistrarSaida(Chegada.AddHours(2)).Notifications).Mensagem); }
}
