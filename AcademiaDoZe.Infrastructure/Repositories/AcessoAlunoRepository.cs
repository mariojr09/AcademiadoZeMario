// Mario Cesar Alves Júnior

using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Enums;
using AcademiadoZE.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    private const int PessoaTipo = (int)AcessoPessoaTipo.Aluno;

    private const string BaseSelectQuery = """
        SELECT
            ac.id_acesso,
            ac.data_hora_chegada,
            ac.data_hora_saida,
            a.id_aluno,
            a.cpf,
            a.nome,
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
        FROM tb_acesso ac
        INNER JOIN tb_aluno a
            ON ac.pessoa_id = a.id_aluno
        INNER JOIN tb_logradouro l
            ON a.logradouro_id = l.id_logradouro
        WHERE ac.pessoa_tipo = 0
        """;

    public AcessoAlunoRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    public async Task<AcessoAluno?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        string sql = $"{BaseSelectQuery} AND ac.id_acesso = @Id";
        await using var command = await CreateCommandAsync(sql, cancellationToken);
        DbProvider.AddParameter(command, "@Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<IReadOnlyCollection<AcessoAluno>> ListarAsync(CancellationToken cancellationToken = default)
    {
        string sql = $"{BaseSelectQuery} ORDER BY ac.data_hora_chegada DESC";
        return await ListarComandoAsync(sql, null, cancellationToken);
    }

    public async Task AdicionarAsync(AcessoAluno entidade, CancellationToken cancellationToken = default)
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
        DbProvider.AddParameter(command, "@PessoaId", entidade.Aluno.Id);
        DbProvider.AddParameter(command, "@DataHoraChegada", entidade.DataHoraChegada);

        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0)
        {
            throw new InfrastructureException(
                "ACESSO_ALUNO_JA_ABERTO",
                "O aluno já possui uma entrada sem saída registrada.");
        }
    }

    public void Atualizar(AcessoAluno entidade)
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
        DbProvider.AddParameter(command, "@PessoaId", entidade.Aluno.Id);

        if (command.ExecuteNonQuery() == 0)
        {
            throw new InfrastructureException(
                "ACESSO_ALUNO_NAO_ATUALIZADO",
                "O acesso do aluno não está aberto ou não foi encontrado.");
        }
    }

    public void Remover(AcessoAluno entidade)
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

    public async Task<IReadOnlyCollection<AcessoAluno>> ListarPorAlunoAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
            {BaseSelectQuery}
              AND ac.pessoa_id = @PessoaId
            ORDER BY ac.data_hora_chegada DESC
            """;

        return await ListarComandoAsync(sql, alunoId, cancellationToken);
    }

    public async Task<AcessoAluno?> ObterAbertoPorAlunoAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
            {BaseSelectQuery}
              AND ac.pessoa_id = @PessoaId
              AND ac.data_hora_saida IS NULL
            ORDER BY ac.data_hora_chegada DESC
            """;

        await using var command = await CreateCommandAsync(sql, cancellationToken);
        DbProvider.AddParameter(command, "@PessoaId", alunoId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    private async Task<IReadOnlyCollection<AcessoAluno>> ListarComandoAsync(
        string sql,
        int? alunoId,
        CancellationToken cancellationToken)
    {
        await using var command = await CreateCommandAsync(sql, cancellationToken);
        if (alunoId.HasValue)
            DbProvider.AddParameter(command, "@PessoaId", alunoId.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var acessos = new List<AcessoAluno>();

        while (await reader.ReadAsync(cancellationToken))
            acessos.Add(Map(reader));

        return acessos;
    }

    private static AcessoAluno Map(DbDataReader reader)
    {
        var aluno = AlunoRepository.Map(reader);
        var result = AcessoAluno.Criar(
            reader.GetInt32Value("id_acesso"),
            aluno,
            reader.GetDateTimeValue("data_hora_chegada"));

        if (result.IsFailure)
        {
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_ACESSO_ALUNO",
                "Não foi possível mapear o acesso do aluno.");
        }

        var acesso = result.Value!;
        var dataHoraSaida = reader.GetNullableDateTime("data_hora_saida");

        if (dataHoraSaida.HasValue && acesso.RegistrarSaida(dataHoraSaida.Value).IsFailure)
        {
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_SAIDA_ALUNO",
                "Não foi possível mapear a saída do aluno.");
        }

        return acesso;
    }
}
