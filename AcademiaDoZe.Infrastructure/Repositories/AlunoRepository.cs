// Mario Cesar Alves Júnior

using System.Data;
using System.Data.Common;
using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Repositories;
using AcademiadoZE.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AlunoRepository : BaseRepository, IAlunoRepository
{
    public AlunoRepository(
        string connectionString,
        DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    private static string BaseSelectQuery => @"
        SELECT
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
        FROM tb_aluno a
        INNER JOIN tb_logradouro l
            ON a.logradouro_id = l.id_logradouro";

    public async Task<Aluno?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        string sql =
            $"{BaseSelectQuery} WHERE a.id_aluno = @Id";

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        DbProvider.AddParameter(command, "@Id", id);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return Map(reader);
    }

    public async Task<IReadOnlyCollection<Aluno>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        string sql =
            $"{BaseSelectQuery} ORDER BY a.nome";

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var alunos = new List<Aluno>();

        while (await reader.ReadAsync(cancellationToken))
        {
            alunos.Add(Map(reader));
        }

        return alunos;
    }

    public async Task AdicionarAsync(
        Aluno entidade,
        CancellationToken cancellationToken = default)
    {
        string sql = @"
            INSERT INTO tb_aluno
            (
                cpf,
                nome,
                nascimento,
                telefone,
                email,
                logradouro_id,
                numero,
                complemento,
                senha,
                foto
            )
            VALUES
            (
                @Cpf,
                @Nome,
                @Nascimento,
                @Telefone,
                @Email,
                @LogradouroId,
                @Numero,
                @Complemento,
                @Senha,
                @Foto
            )";

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        DbProvider.AddParameter(
            command,
            "@Cpf",
            entidade.Cpf.Valor);

        DbProvider.AddParameter(
            command,
            "@Nome",
            entidade.Nome);

        DbProvider.AddParameter(
            command,
            "@Nascimento",
            entidade.DataNascimento);

        DbProvider.AddParameter(
            command,
            "@Telefone",
            entidade.Telefone.Valor);

        DbProvider.AddParameter(
            command,
            "@Email",
            entidade.Email.Valor);

        DbProvider.AddParameter(
            command,
            "@LogradouroId",
            entidade.Endereco.Logradouro.Id);

        DbProvider.AddParameter(
            command,
            "@Numero",
            entidade.Endereco.Numero);

        DbProvider.AddParameter(
            command,
            "@Complemento",
            entidade.Endereco.Complemento ?? (object)DBNull.Value);

        DbProvider.AddParameter(
            command,
            "@Senha",
            entidade.Senha.Hash);

        DbProvider.AddParameter(
            command,
            "@Foto",
            entidade.Foto?.Conteudo ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public void Atualizar(Aluno entidade)
    {
        string sql = @"
            UPDATE tb_aluno
            SET
                cpf = @Cpf,
                nome = @Nome,
                nascimento = @Nascimento,
                telefone = @Telefone,
                email = @Email,
                logradouro_id = @LogradouroId,
                numero = @Numero,
                complemento = @Complemento,
                senha = @Senha,
                foto = @Foto
            WHERE id_aluno = @Id";

        using var command =
            CreateCommandAsync(sql).GetAwaiter().GetResult();

        DbProvider.AddParameter(
            command,
            "@Id",
            entidade.Id);

        DbProvider.AddParameter(
            command,
            "@Cpf",
            entidade.Cpf.Valor);

        DbProvider.AddParameter(
            command,
            "@Nome",
            entidade.Nome);

        DbProvider.AddParameter(
            command,
            "@Nascimento",
            entidade.DataNascimento);

        DbProvider.AddParameter(
            command,
            "@Telefone",
            entidade.Telefone.Valor);

        DbProvider.AddParameter(
            command,
            "@Email",
            entidade.Email.Valor);

        DbProvider.AddParameter(
            command,
            "@LogradouroId",
            entidade.Endereco.Logradouro.Id);

        DbProvider.AddParameter(
            command,
            "@Numero",
            entidade.Endereco.Numero);

        DbProvider.AddParameter(
            command,
            "@Complemento",
            entidade.Endereco.Complemento ?? (object)DBNull.Value);

        DbProvider.AddParameter(
            command,
            "@Senha",
            entidade.Senha.Hash);

        DbProvider.AddParameter(
            command,
            "@Foto",
            entidade.Foto?.Conteudo ?? (object)DBNull.Value);

        command.ExecuteNonQuery();
    }

    public void Remover(Aluno entidade)
    {
        string sql = @"
            DELETE FROM tb_aluno
            WHERE id_aluno = @Id";

        using var command =
            CreateCommandAsync(sql).GetAwaiter().GetResult();

        DbProvider.AddParameter(
            command,
            "@Id",
            entidade.Id);

        command.ExecuteNonQuery();
    }

    public async Task<Aluno?> ObterPorCpfAsync(
        Cpf cpf,
        CancellationToken cancellationToken = default)
    {
        string sql =
            $"{BaseSelectQuery} WHERE a.cpf = @Cpf";

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        DbProvider.AddParameter(
            command,
            "@Cpf",
            cpf.Valor);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return Map(reader);
    }

    public async Task<Aluno?> ObterPorEmailAsync(
        Email email,
        CancellationToken cancellationToken = default)
    {
        string sql =
            $"{BaseSelectQuery} WHERE a.email = @Email";

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        DbProvider.AddParameter(
            command,
            "@Email",
            email.Valor);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return Map(reader);
    }

    public async Task<bool> ExisteCpfAsync(
        Cpf cpf,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM tb_aluno
            WHERE cpf = @Cpf";

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        DbProvider.AddParameter(
            command,
            "@Cpf",
            cpf.Valor);

        var resultado =
            await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(resultado) > 0;
    }

    public async Task<bool> ExisteEmailAsync(
        Email email,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM tb_aluno
            WHERE email = @Email";

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        DbProvider.AddParameter(
            command,
            "@Email",
            email.Valor);

        var resultado =
            await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(resultado) > 0;
    }

    public static Aluno Map(
     DbDataReader reader,
     string nomeColumn = "nome")
    {
        var logradouroResult = Logradouro.Criar(
            reader.GetInt32Value("id_logradouro"),
            reader.GetStringValue("cep"),
            reader.GetStringValue("pais"),
            reader.GetStringValue("estado"),
            reader.GetStringValue("cidade"),
            reader.GetStringValue("bairro"),
            reader.GetStringValue("logradouro_nome")
        );

        if (logradouroResult.IsFailure)
        {
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_LOGRADOURO",
                "Não foi possível mapear o logradouro do aluno.");
        }

        byte[]? fotoBytes = null;

        int fotoOrdinal =
            reader.GetOrdinal("foto");

        if (!reader.IsDBNull(fotoOrdinal))
        {
            fotoBytes =
                (byte[])reader.GetValue(fotoOrdinal);
        }

        Arquivo? foto = null;

        if (fotoBytes is { Length: > 0 })
        {
            var fotoResult =
                Arquivo.Criar(fotoBytes);

            if (fotoResult.IsFailure)
            {
                throw new InfrastructureException(
                    "ERRO_MAPEAMENTO_FOTO",
                    "Não foi possível mapear a foto do aluno.");
            }

            foto = fotoResult.Value;
        }

        var alunoResult = Aluno.Criar(
            reader.GetInt32Value("id_aluno"),
            reader.GetStringValue(nomeColumn),
            reader.GetStringValue("cpf"),
            reader.GetDateOnlyValue("nascimento"),
            reader.GetStringValue("telefone"),
            reader.GetStringValue("email"),
            logradouroResult.Value!,
            reader.GetStringValue("numero"),
            reader.GetNullableString("complemento"),
            reader.GetStringValue("senha"),
            foto
        );

        if (alunoResult.IsFailure)
        {
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_ALUNO",
                "Não foi possível transformar os dados do banco em um Aluno.");
        }

        return alunoResult.Value!;
    }
}