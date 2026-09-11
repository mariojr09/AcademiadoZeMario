// Mario Cesar Alves Júnior

using System.Data;
using System.Data.Common;
using AcademiadoZE.Domain.Entities;
using AcademiadoZE.Domain.Enums;
using AcademiadoZE.Domain.Repositories;
using AcademiadoZE.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class ColaboradorRepository : BaseRepository, IColaboradorRepository
{
    public ColaboradorRepository(
        string connectionString,
        DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    private static string BaseSelectQuery => @"
        SELECT
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
        FROM tb_colaborador c
        INNER JOIN tb_logradouro l
            ON c.logradouro_id = l.id_logradouro";

    public async Task<Colaborador?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        string sql =
            $"{BaseSelectQuery} WHERE c.id_colaborador = @Id";

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        DbProvider.AddParameter(command, "@Id", id);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return Map(reader);
    }

    public async Task<IReadOnlyCollection<Colaborador>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        string sql =
            $"{BaseSelectQuery} ORDER BY c.nome";

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var colaboradores = new List<Colaborador>();

        while (await reader.ReadAsync(cancellationToken))
        {
            colaboradores.Add(Map(reader));
        }

        return colaboradores;
    }

    public async Task AdicionarAsync(
        Colaborador entidade,
        CancellationToken cancellationToken = default)
    {
        string sql = @"
            INSERT INTO tb_colaborador
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
                foto,
                admissao,
                tipo,
                vinculo
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
                @Foto,
                @Admissao,
                @Tipo,
                @Vinculo
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

        DbProvider.AddParameter(
            command,
            "@Admissao",
            entidade.DataAdmissao);

        DbProvider.AddParameter(
            command,
            "@Tipo",
            (int)entidade.Tipo);

        DbProvider.AddParameter(
            command,
            "@Vinculo",
            (int)entidade.Vinculo);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public void Atualizar(Colaborador entidade)
    {
        string sql = @"
            UPDATE tb_colaborador
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
                foto = @Foto,
                admissao = @Admissao,
                tipo = @Tipo,
                vinculo = @Vinculo
            WHERE id_colaborador = @Id";

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

        DbProvider.AddParameter(
            command,
            "@Admissao",
            entidade.DataAdmissao);

        DbProvider.AddParameter(
            command,
            "@Tipo",
            (int)entidade.Tipo);

        DbProvider.AddParameter(
            command,
            "@Vinculo",
            (int)entidade.Vinculo);

        command.ExecuteNonQuery();
    }

    public void Remover(Colaborador entidade)
    {
        string sql = @"
            DELETE FROM tb_colaborador
            WHERE id_colaborador = @Id";

        using var command =
            CreateCommandAsync(sql).GetAwaiter().GetResult();

        DbProvider.AddParameter(
            command,
            "@Id",
            entidade.Id);

        command.ExecuteNonQuery();
    }

    public async Task<Colaborador?> ObterPorCpfAsync(
        Cpf cpf,
        CancellationToken cancellationToken = default)
    {
        string sql =
            $"{BaseSelectQuery} WHERE c.cpf = @Cpf";

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

    public async Task<Colaborador?> ObterPorEmailAsync(
        Email email,
        CancellationToken cancellationToken = default)
    {
        string sql =
            $"{BaseSelectQuery} WHERE c.email = @Email";

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
            FROM tb_colaborador
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
            FROM tb_colaborador
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

    private static Colaborador Map(DbDataReader reader)
    {
        int idLogradouro =
            reader.GetInt32Value("id_logradouro");

        var logradouroResult = Logradouro.Criar(
            idLogradouro,
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
                "Não foi possível mapear o logradouro do colaborador.");
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
                    "Não foi possível mapear a foto do colaborador.");
            }

            foto = fotoResult.Value;
        }

        var colaboradorResult = Colaborador.Criar(
            reader.GetInt32Value("id_colaborador"),
            reader.GetStringValue("nome"),
            reader.GetStringValue("cpf"),
            reader.GetDateOnlyValue("nascimento"),
            reader.GetStringValue("telefone"),
            reader.GetStringValue("email"),
            logradouroResult.Value!,
            reader.GetStringValue("numero"),
            reader.GetNullableString("complemento"),
            reader.GetStringValue("senha"),
            foto,
            reader.GetDateOnlyValue("admissao"),
            (ColaboradorTipo)reader.GetInt32Value("tipo"),
            (ColaboradorVinculo)reader.GetInt32Value("vinculo")
        );

        if (colaboradorResult.IsFailure)
        {
            throw new InfrastructureException(
                "ERRO_MAPEAMENTO_COLABORADOR",
                "Não foi possível transformar os dados do banco em um Colaborador.");
        }

        return colaboradorResult.Value!;
    }
}