namespace AcademiaDoZe.Presentation.Helpers;
//Mario Cesar Alves Júnior

internal static class LogradouroErrorMessages
{
    public static string From(Exception exception, string fallback)
    {
        var messages = EnumerateMessages(exception).ToArray();

        if (Contains(messages, "CEP_JA_CADASTRADO"))
            return "Já existe um logradouro cadastrado com este CEP.";

        if (Contains(messages, "DADOS_LOGRADOURO_INVALIDOS"))
            return "Verifique os dados informados.";

        if (Contains(messages, "LOGRADOURO_NAO_ENCONTRADO"))
            return "O logradouro não foi encontrado.";

        if (Contains(messages, "foreign key constraint") ||
            Contains(messages, "Cannot delete or update a parent row") ||
            Contains(messages, "1451"))
        {
            return "Este logradouro está sendo utilizado e não pode ser excluído.";
        }

        if (Contains(messages, "FALHA_ABRIR_CONEXAO") ||
            Contains(messages, "ERRO_INICIALIZAR_BANCO") ||
            Contains(messages, "Unable to connect") ||
            Contains(messages, "Connection refused") ||
            Contains(messages, "Connect Timeout"))
        {
            return "Não foi possível acessar o banco MySQL. Verifique se o servidor está disponível.";
        }

        return fallback;
    }

    private static IEnumerable<string> EnumerateMessages(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException!)
            yield return current.Message;
    }

    private static bool Contains(IEnumerable<string> messages, string value) =>
        messages.Any(message => message.Contains(value, StringComparison.OrdinalIgnoreCase));
}
