using System.Text.RegularExpressions;
using AcademiaDoZe.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace AcademiaDoZe.Presentation;

internal static partial class AppErrorMessages
{
    public static string From(
        Exception exception,
        ILogger? logger = null,
        string operation = "operação")
    {
        var infrastructure = Find<InfrastructureException>(exception);
        var mysql = Find<MySqlException>(exception);
        var diagnostic = BuildDiagnostic(exception, infrastructure, mysql);

        logger?.LogError(
            "Falha em {Operation}. Diagnóstico sanitizado: {Diagnostic}",
            operation,
            diagnostic);

        var code = exception.Message;
        var businessMessage = code switch
        {
            "CPF_JA_CADASTRADO" => "Já existe uma pessoa cadastrada com este CPF.",
            "EMAIL_JA_CADASTRADO" => "Já existe uma pessoa cadastrada com este e-mail.",
            "LOGRADOURO_OBRIGATORIO" => "Selecione um logradouro.",
            "LOGRADOURO_NAO_ENCONTRADO" => "O logradouro selecionado não existe mais.",
            "SENHA_OBRIGATORIA" => "Informe uma senha.",
            "SENHA_INVALIDA" => "A senha deve ter no mínimo 8 caracteres, uma letra maiúscula e um número.",
            "COLABORADOR_NAO_ENCONTRADO" => "Colaborador não encontrado.",
            "ALUNO_NAO_ENCONTRADO" => "Aluno não encontrado.",
            "DADOS_COLABORADOR_INVALIDOS" => "Os dados do colaborador não atendem às regras de cadastro.",
            "DADOS_ALUNO_INVALIDOS" => "Os dados do aluno não atendem às regras de cadastro.",
            _ => null
        };

        if (businessMessage is not null)
            return businessMessage;

        if (mysql is not null || infrastructure is not null)
            return $"Não foi possível concluir a operação no MySQL.\n\nDiagnóstico técnico: {diagnostic}";

        return string.IsNullOrWhiteSpace(code)
            ? "Ocorreu um erro inesperado."
            : Sanitize(code);
    }

    private static string BuildDiagnostic(
        Exception exception,
        InfrastructureException? infrastructure,
        MySqlException? mysql)
    {
        if (infrastructure?.ErrorCode == "SCHEMA_MYSQL_INCOMPATIVEL")
            return $"{infrastructure.ErrorCode} — {Sanitize(infrastructure.Message)}";

        if (mysql is not null)
        {
            var description = mysql.Number switch
            {
                1044 => "usuário sem permissão para o banco informado",
                1045 => "autenticação rejeitada pelo servidor",
                1049 => "banco de dados não encontrado",
                1054 => "coluna esperada não encontrada",
                1146 => "tabela esperada não encontrada",
                2003 => "servidor indisponível ou porta incorreta",
                _ => "erro retornado pelo provedor MySQL"
            };
            return $"MySQL {mysql.Number} / SQLSTATE {mysql.SqlState ?? "n/d"} — {description}. Mensagem: {Sanitize(mysql.Message)}";
        }

        if (infrastructure is not null)
            return $"{infrastructure.ErrorCode ?? "INFRAESTRUTURA"} — {Sanitize(infrastructure.Message)}";

        return $"{exception.GetType().Name} — {Sanitize(exception.Message)}";
    }

    private static T? Find<T>(Exception? exception) where T : Exception
    {
        while (exception is not null)
        {
            if (exception is T found) return found;
            exception = exception.InnerException;
        }
        return null;
    }

    private static string Sanitize(string value) =>
        CredentialRegex().Replace(value, "$1=<oculto>");

    [GeneratedRegex("(?i)(password|pwd|senha)\\s*=\\s*[^;,\\s]+")]
    private static partial Regex CredentialRegex();
}
