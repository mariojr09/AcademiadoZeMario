// Mario Cesar Alves Júnior

using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Enums;
using AcademiadoZE.Domain.Repositories;
using AcademiadoZE.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class MatriculaRepository : BaseRepository, IMatriculaRepository
{
    private const string BaseSelectQuery = """
        SELECT
            m.id_matricula,
            m.aluno_id,
            m.plano,
            m.data_inicio,
            m.data_fim,
            m.objetivo,
            m.restricao_medica,
            m.obs_restricao,
            m.laudo_medico,

            a.id_aluno,
            a.cpf,
            a.nome AS aluno_nome,
            a.nascimento,
            a.telefone,
            a.email,
            a.logradouro_id,
            a.numero,
            a.complemento,
            a.senha,
            a.foto,

            l.id_logradouro,
            l.cep,
            l.nome AS logradouro_nome,
            l.bairro,
            l.cidade,
            l.estado,
            l.pais

        FROM tb_matricula m

        INNER JOIN tb_aluno a
            ON m.aluno_id = a.id_aluno

        INNER JOIN tb_logradouro l
            ON a.logradouro_id = l.id_logradouro
        """;

    public MatriculaRepository(
        string connectionString,
        DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    private static Matricula Map(DbDataReader reader)
    {
        var aluno =
            AlunoRepository.Map(
                reader,
                "aluno_nome");

        var plano =
            (MatriculaPlano)reader.GetInt32Value(
                "plano");

        var restricoes =
            (Restricao)reader.GetInt32Value(
                "restricao_medica");

        Arquivo? laudoMedico = null;

        int laudoOrdinal =
            reader.GetOrdinal(
                "laudo_medico");

        if (!reader.IsDBNull(laudoOrdinal))
        {
            var laudoBytes =
                (byte[])reader.GetValue(
                    laudoOrdinal);

            if (laudoBytes.Length > 0)
            {
                var laudoResult =
                    Arquivo.Criar(
                        laudoBytes);

                if (laudoResult.IsFailure)
                {
                    throw new InfrastructureException(
                        "ERRO_MAPEAMENTO_LAUDO",
                        "Não foi possível mapear o laudo médico da matrícula.");
                }

                laudoMedico =
                    laudoResult.Value;
            }
        }

        var matriculaResult =
            Matricula.Criar(
                reader.GetInt32Value(
                    "id_matricula"),
                aluno,
                plano,
                reader.GetDateOnlyValue(
                    "data_inicio"),
                reader.GetDateOnlyValue(
                    "data_fim"),
                reader.GetStringValue(
                    "objetivo"),
                restricoes,
                reader.GetNullableString(
                    "obs_restricao"),
                laudoMedico);

        if (matriculaResult.IsFailure)
        {
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_MATRICULA",
                "Não foi possível transformar os dados do banco em uma Matrícula.");
        }

        return matriculaResult.Value!;
    }

    public async Task<Matricula?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string filtro = """
            WHERE m.id_matricula = @id
            """;

        await using var command =
            await CreateCommandAsync(
                $"{BaseSelectQuery} {filtro}",
                cancellationToken);

        DbProvider.AddParameter(
            command,
            "@id",
            id);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (await reader.ReadAsync(
            cancellationToken))
        {
            return Map(reader);
        }

        return null;
    }

    public async Task<IReadOnlyCollection<Matricula>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
            {BaseSelectQuery}
            ORDER BY m.id_matricula
            """;

        await using var command =
            await CreateCommandAsync(
                sql,
                cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        var matriculas =
            new List<Matricula>();

        while (await reader.ReadAsync(
            cancellationToken))
        {
            matriculas.Add(
                Map(reader));
        }

        return matriculas;
    }

    public async Task AdicionarAsync(
        Matricula entidade,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO tb_matricula
            (
                aluno_id,
                plano,
                data_inicio,
                data_fim,
                objetivo,
                restricao_medica,
                obs_restricao,
                laudo_medico
            )
            VALUES
            (
                @aluno_id,
                @plano,
                @data_inicio,
                @data_fim,
                @objetivo,
                @restricao_medica,
                @obs_restricao,
                @laudo_medico
            )
            """;

        await using var command =
            await CreateCommandAsync(
                sql,
                cancellationToken);

        DbProvider.AddParameter(
            command,
            "@aluno_id",
            entidade.Aluno.Id);

        DbProvider.AddParameter(
            command,
            "@plano",
            (int)entidade.Plano);

        DbProvider.AddParameter(
            command,
            "@data_inicio",
            entidade.DataInicio);

        DbProvider.AddParameter(
            command,
            "@data_fim",
            entidade.DataFinal);

        DbProvider.AddParameter(
            command,
            "@objetivo",
            entidade.Objetivo);

        DbProvider.AddParameter(
            command,
            "@restricao_medica",
            (int)entidade.Restricoes);

        DbProvider.AddParameter(
            command,
            "@obs_restricao",
            entidade.ObservacoesRestricoes);

        DbProvider.AddParameter(
            command,
            "@laudo_medico",
            entidade.LaudoMedico?.Conteudo);

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }

    public void Atualizar(
        Matricula entidade)
    {
        const string sql = """
            UPDATE tb_matricula
            SET
                aluno_id = @aluno_id,
                plano = @plano,
                data_inicio = @data_inicio,
                data_fim = @data_fim,
                objetivo = @objetivo,
                restricao_medica = @restricao_medica,
                obs_restricao = @obs_restricao,
                laudo_medico = @laudo_medico
            WHERE id_matricula = @id
            """;

        using var command =
            CreateCommandAsync(sql)
                .GetAwaiter()
                .GetResult();

        DbProvider.AddParameter(
            command,
            "@aluno_id",
            entidade.Aluno.Id);

        DbProvider.AddParameter(
            command,
            "@plano",
            (int)entidade.Plano);

        DbProvider.AddParameter(
            command,
            "@data_inicio",
            entidade.DataInicio);

        DbProvider.AddParameter(
            command,
            "@data_fim",
            entidade.DataFinal);

        DbProvider.AddParameter(
            command,
            "@objetivo",
            entidade.Objetivo);

        DbProvider.AddParameter(
            command,
            "@restricao_medica",
            (int)entidade.Restricoes);

        DbProvider.AddParameter(
            command,
            "@obs_restricao",
            entidade.ObservacoesRestricoes);

        DbProvider.AddParameter(
            command,
            "@laudo_medico",
            entidade.LaudoMedico?.Conteudo);

        DbProvider.AddParameter(
            command,
            "@id",
            entidade.Id);

        command.ExecuteNonQuery();
    }

    public void Remover(
        Matricula entidade)
    {
        const string sql = """
            DELETE FROM tb_matricula
            WHERE id_matricula = @id
            """;

        using var command =
            CreateCommandAsync(sql)
                .GetAwaiter()
                .GetResult();

        DbProvider.AddParameter(
            command,
            "@id",
            entidade.Id);

        command.ExecuteNonQuery();
    }

    public async Task<IReadOnlyCollection<Matricula>> ListarPorAlunoAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
            {BaseSelectQuery}
            WHERE m.aluno_id = @aluno_id
            ORDER BY m.data_inicio
            """;

        await using var command =
            await CreateCommandAsync(
                sql,
                cancellationToken);

        DbProvider.AddParameter(
            command,
            "@aluno_id",
            alunoId);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        var matriculas =
            new List<Matricula>();

        while (await reader.ReadAsync(
            cancellationToken))
        {
            matriculas.Add(
                Map(reader));
        }

        return matriculas;
    }

    public async Task<Matricula?> ObterVigentePorAlunoAsync(
        int alunoId,
        DateOnly dataReferencia,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
            {BaseSelectQuery}
            WHERE m.aluno_id = @aluno_id
              AND m.data_inicio <= @data_referencia
              AND m.data_fim >= @data_referencia
            ORDER BY m.data_inicio DESC
            LIMIT 1
            """;

        await using var command =
            await CreateCommandAsync(
                sql,
                cancellationToken);

        DbProvider.AddParameter(
            command,
            "@aluno_id",
            alunoId);

        DbProvider.AddParameter(
            command,
            "@data_referencia",
            dataReferencia);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (await reader.ReadAsync(
            cancellationToken))
        {
            return Map(reader);
        }

        return null;
    }
}