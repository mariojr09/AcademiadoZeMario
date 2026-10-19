using AcademiaDoZe.Infrastructure.Data;

[assembly: CollectionBehavior(
    CollectionBehavior.CollectionPerAssembly,
    DisableTestParallelization = true)]

namespace AcademiaDoZe.Infrastructure.Tests;

public abstract class TestBase
{
    protected string ConnectionString { get; }
    protected DatabaseType DatabaseType { get; }

    protected TestBase()
    {
        DatabaseType = DatabaseType.MySql;
        ConnectionString = Environment.GetEnvironmentVariable(
            "ACADEMIA_DB_CONNECTION_STRING") ?? string.Empty;

    }

    private static int _counter = 10000;

    protected static string GerarCep() =>
        (80000000 +
        ((int)(DateTime.UtcNow.Ticks % 8000000)) +
        Interlocked.Increment(ref _counter))
        .ToString("D8")[..8];

    protected static string GerarCpf()
    {
        long valor =
            100000000L +
            ((DateTime.UtcNow.Ticks +
              Interlocked.Increment(ref _counter))
             % 899999999L);

        string baseCpf = valor.ToString("D9");

        int soma = 0;

        for (int i = 0; i < 9; i++)
            soma += (baseCpf[i] - '0') * (10 - i);

        int resto = soma % 11;

        int primeiroDigito =
            resto < 2 ? 0 : 11 - resto;

        string cpfComPrimeiro =
            baseCpf + primeiroDigito;

        soma = 0;

        for (int i = 0; i < 10; i++)
            soma += (cpfComPrimeiro[i] - '0') * (11 - i);

        resto = soma % 11;

        int segundoDigito =
            resto < 2 ? 0 : 11 - resto;

        return cpfComPrimeiro + segundoDigito;
    }

    protected static string GerarEmail() =>
        $"user_{Guid.NewGuid().ToString("N")[..8]}@test.com";

    protected static string GerarTelefone() =>
        (49990000000L +
        ((DateTime.UtcNow.Ticks % 8000000000L)) +
        Interlocked.Increment(ref _counter))
        .ToString("D11")[..11];
}
