namespace AcademiaDoZe.Infrastructure.Tests;

/// <summary>
/// Mantém os testes de integração seguros: nenhuma credencial é versionada e
/// o runner informa corretamente quando o MySQL local não foi configurado.
/// </summary>
public sealed class FactAttribute : Xunit.FactAttribute
{
    public FactAttribute()
    {
        if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable("ACADEMIA_DB_CONNECTION_STRING")))
        {
            Skip = "Defina ACADEMIA_DB_CONNECTION_STRING para executar a integração MySQL.";
        }
    }
}
