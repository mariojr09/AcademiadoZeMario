// Mario Cesar Alves Júnior
using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Exceptions;

namespace AcademiadoZE.Domain.Tests.Entities;

public class EntityTests
{
    private sealed class EntidadeA(int id) : Entity(id);
    private sealed class EntidadeB(int id) : Entity(id);

    [Fact] public void IdZero_DeveSerTransiente() => Assert.True(new EntidadeA(0).EhTransiente);
    [Fact] public void IdPositivo_NaoDeveSerTransiente() => Assert.False(new EntidadeA(1).EhTransiente);
    [Fact] public void IdNegativo_DeveLancarExcecao() => Assert.Equal("ID_NEGATIVO", Assert.Throws<DomainException>(() => new EntidadeA(-1)).Message);
    [Fact] public void MesmaReferencia_DeveSerIgual() { var a = new EntidadeA(1); Assert.True(a.Equals(a)); }
    [Fact] public void MesmoTipoEId_DevemSerIguais() => Assert.Equal(new EntidadeA(1), new EntidadeA(1));
    [Fact] public void TiposDiferentesMesmoId_NaoDevemSerIguais() => Assert.NotEqual<Entity>(new EntidadeA(1), new EntidadeB(1));
    [Fact] public void EntidadesTransientesDiferentes_NaoDevemSerIguais() => Assert.NotEqual(new EntidadeA(0), new EntidadeA(0));
    [Fact] public void EntidadesComIdsDiferentes_NaoDevemSerIguais() => Assert.NotEqual(new EntidadeA(1), new EntidadeA(2));
    [Fact] public void OperadorIgual_DeveTratarDoisNulos() { EntidadeA? a = null; EntidadeA? b = null; Assert.True(a == b); }
    [Fact] public void OperadorDiferente_DeveTratarUmNulo() { EntidadeA? a = new(1); EntidadeA? b = null; Assert.True(a != b); }
    [Fact] public void EntidadesIguais_DevemTerMesmoHashCode() => Assert.Equal(new EntidadeA(1).GetHashCode(), new EntidadeA(1).GetHashCode());
}
