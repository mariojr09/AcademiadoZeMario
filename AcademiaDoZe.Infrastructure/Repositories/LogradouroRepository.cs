using System.Data;
using System.Data.Common;
using AcademiaDoZe.Infrastructure.Data;
using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Repositories;
using AcademiadoZE.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class LogradouroRepository : BaseRepository, ILogradouroRepository
{
    public LogradouroRepository(
        string connectionString,
        DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    public async Task<Logradouro?> ObterPorIdAsync(
     int id,
     CancellationToken cancellationToken = default)
    {
        string sql = $"{BaseSelectQuery} WHERE id_logradouro = @Id";

        await using var command = await CreateCommandAsync(sql, cancellationToken);

        DbProvider.AddParameter(command, "@Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return Map(reader);
    }

    public async Task<IReadOnlyCollection<Logradouro>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        string sql = $"{BaseSelectQuery} ORDER BY nome";

        await using var command = await CreateCommandAsync(sql, cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var logradouros = new List<Logradouro>();

        while (await reader.ReadAsync(cancellationToken))
        {
            logradouros.Add(Map(reader));
        }

        return logradouros;
    }

    public async Task<IReadOnlyCollection<Logradouro>> ListarPorCepAsync(
        Cep cep,
        CancellationToken cancellationToken = default)
    {
        string sql = $"{BaseSelectQuery} WHERE cep = @Cep";

        await using var command = await CreateCommandAsync(sql, cancellationToken);

        DbProvider.AddParameter(command, "@Cep", cep.Valor);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var logradouros = new List<Logradouro>();

        while (await reader.ReadAsync(cancellationToken))
        {
            logradouros.Add(Map(reader));
        }

        return logradouros;
    }
    private static Logradouro Map(DbDataReader reader)
    {
        int id = reader.GetInt32Value("id_logradouro");

        var result = Logradouro.Criar(
            id,
            reader.GetStringValue("cep"),
            reader.GetStringValue("pais"),
            reader.GetStringValue("estado"),
            reader.GetStringValue("cidade"),
            reader.GetStringValue("bairro"),
            reader.GetStringValue("nome")
        );

        if (result.IsFailure)
        {
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_LOGRADOURO",
                "Não foi possível transformar os dados do banco em um Logradouro.");
        }

        return result.Value!;
    }
    public async Task AdicionarAsync(
    Logradouro entidade,
    CancellationToken cancellationToken = default)
    {
        string sql = @"
        INSERT INTO tb_logradouro
        (cep, nome, bairro, cidade, estado, pais)
        VALUES
        (@Cep, @Nome, @Bairro, @Cidade, @Estado, @Pais)";

        await using var command = await CreateCommandAsync(sql, cancellationToken);

        DbProvider.AddParameter(command, "@Cep", entidade.Cep.Valor);
        DbProvider.AddParameter(command, "@Nome", entidade.NomeLogradouro);
        DbProvider.AddParameter(command, "@Bairro", entidade.Bairro);
        DbProvider.AddParameter(command, "@Cidade", entidade.Cidade);
        DbProvider.AddParameter(command, "@Estado", entidade.Estado);
        DbProvider.AddParameter(command, "@Pais", entidade.Pais);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
    public void Atualizar(Logradouro entidade)
    {
        string sql = @"
        UPDATE tb_logradouro
        SET cep = @Cep,
            nome = @Nome,
            bairro = @Bairro,
            cidade = @Cidade,
            estado = @Estado,
            pais = @Pais
        WHERE id_logradouro = @Id";

        using var command = CreateCommandAsync(sql).GetAwaiter().GetResult();

        DbProvider.AddParameter(command, "@Id", entidade.Id);
        DbProvider.AddParameter(command, "@Cep", entidade.Cep.Valor);
        DbProvider.AddParameter(command, "@Nome", entidade.NomeLogradouro);
        DbProvider.AddParameter(command, "@Bairro", entidade.Bairro);
        DbProvider.AddParameter(command, "@Cidade", entidade.Cidade);
        DbProvider.AddParameter(command, "@Estado", entidade.Estado);
        DbProvider.AddParameter(command, "@Pais", entidade.Pais);

        command.ExecuteNonQuery();
    }
    public void Remover(Logradouro entidade)
    {
        string sql = @"
        DELETE FROM tb_logradouro
        WHERE id_logradouro = @Id";

        using var command = CreateCommandAsync(sql).GetAwaiter().GetResult();

        DbProvider.AddParameter(command, "@Id", entidade.Id);

        command.ExecuteNonQuery();
    }
    private static string BaseSelectQuery =>
        "SELECT id_logradouro, cep, nome, bairro, cidade, estado, pais FROM tb_logradouro";
}