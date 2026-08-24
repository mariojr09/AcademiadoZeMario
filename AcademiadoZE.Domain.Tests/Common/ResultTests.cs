// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Common;

namespace AcademiadoZE.Domain.Tests.Common;

public class ResultTests
{
    [Fact] public void Success_DeveConterValor() { var r = Result<int>.Success(42); Assert.Equal(42, r.Value); }
    [Fact] public void Success_DeveIndicarSucesso() => Assert.True(Result<string>.Success("ok").IsSuccess);
    [Fact] public void Success_NaoDeveIndicarFalha() => Assert.False(Result<string>.Success("ok").IsFailure);
    [Fact] public void Success_NaoDeveConterNotificacoes() => Assert.Empty(Result<int>.Success(1).Notifications);
    [Fact] public void Failure_DeveIndicarFalha() => Assert.True(Result<int>.Failure("Campo", "ERRO").IsFailure);
    [Fact] public void Failure_NaoDeveIndicarSucesso() => Assert.False(Result<int>.Failure("Campo", "ERRO").IsSuccess);
    [Fact] public void Failure_DeveTerValorPadrao() => Assert.Equal(0, Result<int>.Failure("Campo", "ERRO").Value);
    [Fact] public void Failure_DeveCriarNotificacao() { var n = Assert.Single(Result<int>.Failure("Campo", "ERRO").Notifications); Assert.Equal("Campo", n.Propriedade); Assert.Equal("ERRO", n.Mensagem); }
    [Fact] public void Failure_DeveAceitarNotificacao() { var n = new Notification("A", "B"); Assert.Same(n, Assert.Single(Result<int>.Failure(n).Notifications)); }
    [Fact] public void Failure_DevePreservarMultiplasNotificacoes() { var r = Result<int>.Failure([new("A", "1"), new("B", "2")]); Assert.Equal(2, r.Notifications.Count); }
}
