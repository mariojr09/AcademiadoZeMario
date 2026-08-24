using AcademiaDoZe.Infrastructure.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Data;

public enum DatabaseType
{
    SqlServer,
    MySql,
    Sqlite
}

public static class DbProvider
{
    public const int DefaultCommandTimeout = 30;

    public static DbConnection CreateConnection(
        string connectionString,
        DatabaseType dbType)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InfrastructureException(
                "CONEXAO_STRING_VAZIA",
                "String de conexão não pode ser vazia.");

        try
        {
            DbConnection connection = dbType switch
            {
                DatabaseType.SqlServer => new SqlConnection(connectionString),
                DatabaseType.MySql => new MySqlConnection(connectionString),
                DatabaseType.Sqlite => new SqliteConnection(connectionString),

                _ => throw new InfrastructureException(
                    "SGDB_NAO_SUPORTADO",
                    $"SGDB não suportado: {dbType}")
            };

            return connection;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException(
                "FALHA_CONEXAO",
                $"Falha ao abrir conexão para {dbType}.",
                ex);
        }
    }

    public static DbCommand CreateCommand(
        string commandText,
        DbConnection connection,
        CommandType commandType = CommandType.Text)
    {
        if (connection == null)
            throw new InfrastructureException(
                "CONEXAO_NULA",
                "Conexão não pode ser nula para criar um comando.");

        if (string.IsNullOrWhiteSpace(commandText))
            throw new InfrastructureException(
                "COMANDO_TEXTO_VAZIO",
                "Texto do comando não pode ser vazio.");

        try
        {
            var command = connection.CreateCommand()
                ?? throw new InfrastructureException(
                    "FALHA_CRIAR_COMANDO",
                    "Falha ao criar o comando no banco de dados.");

            command.CommandText = commandText;
            command.CommandType = commandType;
            command.CommandTimeout = DefaultCommandTimeout;

            return command;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException(
                "FALHA_CRIAR_COMANDO",
                "Falha ao criar o comando no banco de dados.",
                ex);
        }
    }

    public static void AddParameter(
    DbCommand command,
    string parameterName,
    object? value)
    {
        if (command == null)
            throw new InfrastructureException(
                "COMANDO_NULO",
                "Comando não pode ser nulo.");

        var parameter = command.CreateParameter();
        parameter.ParameterName = parameterName;
        parameter.Value = value ?? DBNull.Value;

        command.Parameters.Add(parameter);
    }
}