using AcademiaDoZe.Infrastructure.Exceptions;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Reflection;

namespace AcademiaDoZe.Infrastructure.Data;

public static class DbInitializer
{
    private static readonly ConcurrentDictionary<string, bool> _bancosInicializados = new();

    public static async Task InicializarAsync(
        string connectionString,
        DatabaseType databaseType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        var key = $"{databaseType}:{connectionString}";

        if (_bancosInicializados.ContainsKey(key))
            return;

        var scriptSql = ObterScript(databaseType);

        try
        {
            await using var connection =
                DbProvider.CreateConnection(connectionString, databaseType);

            await connection.OpenAsync(cancellationToken);

            await using var command =
                DbProvider.CreateCommand(scriptSql, connection);

            await command.ExecuteNonQueryAsync(cancellationToken);

            if (databaseType == DatabaseType.MySql)
            {
                await ValidarEsquemaMySqlAsync(
                    connection,
                    cancellationToken);
            }

            _bancosInicializados.TryAdd(key, true);
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_INICIALIZAR_BANCO",
                $"Erro ao inicializar banco de dados: {ex.Message}",
                ex);
        }
    }

    private static async Task ValidarEsquemaMySqlAsync(
        DbConnection connection,
        CancellationToken cancellationToken)
    {
        var esperadas = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["tb_logradouro"] = ["id_logradouro", "cep", "nome", "bairro", "cidade", "estado", "pais"],
            ["tb_aluno"] = ["id_aluno", "cpf", "nome", "nascimento", "telefone", "email", "logradouro_id", "numero", "complemento", "senha", "foto"],
            ["tb_colaborador"] = ["id_colaborador", "cpf", "nome", "nascimento", "telefone", "email", "logradouro_id", "numero", "complemento", "senha", "foto", "admissao", "tipo", "vinculo"]
        };

        const string sql = """
            SELECT TABLE_NAME, COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME IN ('tb_logradouro', 'tb_aluno', 'tb_colaborador')
            """;

        await using var command = DbProvider.CreateCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var encontradas = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        while (await reader.ReadAsync(cancellationToken))
        {
            var tabela = reader.GetString(0);
            var coluna = reader.GetString(1);
            if (!encontradas.TryGetValue(tabela, out var colunas))
            {
                colunas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                encontradas[tabela] = colunas;
            }
            colunas.Add(coluna);
        }

        var ausentes = esperadas
            .SelectMany(item => item.Value
                .Where(coluna => !encontradas.TryGetValue(item.Key, out var colunas) || !colunas.Contains(coluna))
                .Select(coluna => $"{item.Key}.{coluna}"))
            .ToArray();

        if (ausentes.Length > 0)
        {
            throw new InfrastructureException(
                "SCHEMA_MYSQL_INCOMPATIVEL",
                $"Estrutura MySQL incompatível. Tabelas/colunas ausentes: {string.Join(", ", ausentes)}.");
        }
    }

    public static string ObterScript(DatabaseType databaseType)
    {
        var nomeScript = databaseType switch
        {
            DatabaseType.SqlServer => "script_sqlserver.sql",
            DatabaseType.MySql => "script_mysql.sql",
            DatabaseType.Sqlite => "script_sqlite.sql",
            _ => throw new InfrastructureException(
                "SGDB_NAO_SUPORTADO",
                $"SGDB não suportado: {databaseType}")
        };

        var assembly = Assembly.GetExecutingAssembly();

        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(r =>
                r.EndsWith(nomeScript, StringComparison.OrdinalIgnoreCase))
            ?? throw new InfrastructureException(
                "SCRIPT_EMBARCADO_NAO_ENCONTRADO",
                $"Script SQL embarcado '{nomeScript}' não encontrado.");

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InfrastructureException(
                "ERRO_LEITURA_SCRIPT",
                $"Erro ao carregar o fluxo do script embarcado '{nomeScript}'.");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
