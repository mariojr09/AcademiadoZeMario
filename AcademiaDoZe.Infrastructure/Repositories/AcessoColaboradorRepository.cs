// Mario Cesar Alves Júnior

using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Enums;
using AcademiadoZE.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoColaboradorRepository : BaseRepository, IAcessoColaboradorRepository
{
    private const int PessoaTipo = (int)AcessoPessoaTipo.Colaborador;

    private const string BaseSelectQuery = """
        SELECT
            ac.id_acesso,
            ac.data_hora_chegada,
            ac.data_hora_saida,
            c.id_colaborador,
            c.cpf,
            c.nome,
            c.nascimento,
            c.telefone,
            c.email,
            c.logradouro_id,
            c.numero,
            c.complemento,
            c.senha,
            c.foto,
            c.admissao,
            c.tipo,
            c.vinculo,
            l.id_logradouro,
            l.cep,
            l.nome AS logradouro_nome,
            l.bairro,
            l.cidade,
            l.estado,
            l.pais
        FROM tb_acesso ac
        INNER JOIN tb_colaborador c
            ON ac.pessoa_id = c.id_colaborador
        INNER JOIN tb_logradouro l
            ON c.logradouro_id = l.id_logradouro
        WHERE ac.pessoa_tipo = 1
        """;

    public AcessoColaboradorRepository(
        string connectionString,
        DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    public async Task<AcessoColaborador?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        string sql = $"{BaseSelectQuery} AND ac.id_acesso = @Id";

        await using var command = await CreateCommandAsync(sql, cancellationToken);
        DbProvider.AddParameter(command, "@Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<IReadOnlyCollection<AcessoColaborador>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        string sql = $"{BaseSelectQuery} ORDER BY ac.data_hora_chegada DESC";
        return await ListarComandoAsync(sql, null, cancellationToken);
    }

    public async Task AdicionarAsync(
        AcessoColaborador entidade,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO tb_acesso
                (pessoa_tipo, pessoa_id, data_hora_chegada, data_hora_saida)
            SELECT
                @PessoaTipo, @PessoaId, @DataHoraChegada, NULL
            WHERE NOT EXISTS
            (
                SELECT 1
                FROM tb_acesso
                WHERE pessoa_tipo = @PessoaTipo
                  AND pessoa_id = @PessoaId
                  AND data_hora_saida IS NULL
            )
            """;

        await using var command = await CreateCommandAsync(sql, cancellationToken);
        DbProvider.AddParameter(command, "@PessoaTipo", PessoaTipo);
        DbProvider.AddParameter(command, "@PessoaId", entidade.Colaborador.Id);
        DbProvider.AddParameter(command, "@DataHoraChegada", entidade.DataHoraChegada);

        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0)
        {
            throw new InfrastructureException(
                "ACESSO_COLABORADOR_JA_ABERTO",
                "O colaborador já possui uma entrada sem saída registrada.");
        }
    }

    public void Atualizar(AcessoColaborador entidade)
    {
        const string sql = """
            UPDATE tb_acesso
            SET data_hora_saida = @DataHoraSaida
            WHERE id_acesso = @Id
              AND pessoa_tipo = @PessoaTipo
              AND pessoa_id = @PessoaId
              AND data_hora_saida IS NULL
            """;

        using var command = CreateCommandAsync(sql).GetAwaiter().GetResult();
        DbProvider.AddParameter(command, "@DataHoraSaida", entidade.DataHoraSaida);
        DbProvider.AddParameter(command, "@Id", entidade.Id);
        DbProvider.AddParameter(command, "@PessoaTipo", PessoaTipo);
        DbProvider.AddParameter(command, "@PessoaId", entidade.Colaborador.Id);

        if (command.ExecuteNonQuery() == 0)
        {
            throw new InfrastructureException(
                "ACESSO_COLABORADOR_NAO_ATUALIZADO",
                "O acesso do colaborador não está aberto ou não foi encontrado.");
        }
    }

    public void Remover(AcessoColaborador entidade)
    {
        const string sql = """
            DELETE FROM tb_acesso
            WHERE id_acesso = @Id
              AND pessoa_tipo = @PessoaTipo
            """;

        using var command = CreateCommandAsync(sql).GetAwaiter().GetResult();
        DbProvider.AddParameter(command, "@Id", entidade.Id);
        DbProvider.AddParameter(command, "@PessoaTipo", PessoaTipo);
        command.ExecuteNonQuery();
    }

    public async Task<IReadOnlyCollection<AcessoColaborador>> ListarPorColaboradorAsync(
        int colaboradorId,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
            {BaseSelectQuery}
              AND ac.pessoa_id = @PessoaId
            ORDER BY ac.data_hora_chegada DESC
            """;

        return await ListarComandoAsync(sql, colaboradorId, cancellationToken);
    }

    public async Task<AcessoColaborador?> ObterAbertoPorColaboradorAsync(
        int colaboradorId,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
            {BaseSelectQuery}
              AND ac.pessoa_id = @PessoaId
              AND ac.data_hora_saida IS NULL
            ORDER BY ac.data_hora_chegada DESC
            """;

        await using var command = await CreateCommandAsync(sql, cancellationToken);
        DbProvider.AddParameter(command, "@PessoaId", colaboradorId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    private async Task<IReadOnlyCollection<AcessoColaborador>> ListarComandoAsync(
        string sql,
        int? colaboradorId,
        CancellationToken cancellationToken)
    {
        await using var command = await CreateCommandAsync(sql, cancellationToken);
        if (colaboradorId.HasValue)
            DbProvider.AddParameter(command, "@PessoaId", colaboradorId.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var acessos = new List<AcessoColaborador>();

        while (await reader.ReadAsync(cancellationToken))
            acessos.Add(Map(reader));

        return acessos;
    }

    private static AcessoColaborador Map(DbDataReader reader)
    {
        var colaborador = ColaboradorRepository.Map(reader);
        var result = AcessoColaborador.Criar(
            reader.GetInt32Value("id_acesso"),
            colaborador,
            reader.GetDateTimeValue("data_hora_chegada"));

        if (result.IsFailure)
        {
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_ACESSO_COLABORADOR",
                "Não foi possível mapear o acesso do colaborador.");
        }

        var acesso = result.Value!;
        var dataHoraSaida = reader.GetNullableDateTime("data_hora_saida");

        if (dataHoraSaida.HasValue && acesso.RegistrarSaida(dataHoraSaida.Value).IsFailure)
        {
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_SAIDA_COLABORADOR",
                "Não foi possível mapear a saída do colaborador.");
        }

        return acesso;
    }
}
