using AcademiadoZE.Domain.Common;
using AcademiadoZE.Domain.Services;
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiadoZE.Domain.Entities;
//Mario Cesar alves Júnior

public class Aluno : Pessoa
{
    private const int IdadeMinimaAnos = 14;

    private Aluno(
        int id,
        string nome,
        Cpf cpf,
        DateOnly dataNascimento,
        Telefone telefone,
        Email email,
        Endereco endereco,
        Senha senha,
        Arquivo? foto)
        : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
    {
    }

    public static Result<Aluno> Criar(
        int id,
        string? nome,
        string? cpf,
        DateOnly dataNascimento,
        string? telefone,
        string? email,
        Logradouro? logradouro,
        string? numero,
        string? complemento,
        string? senha,
        Arquivo? foto)
    {
        var notifications = new List<Notification>();

        if (NormalizadoService.TextoVazioOuNulo(nome))
            notifications.Add(new Notification("Nome", "NOME_OBRIGATORIO"));
        else
            nome = NormalizadoService.LimparEspacos(nome);

        if (dataNascimento == default)
            notifications.Add(new Notification("DataNascimento", "DATA_NASCIMENTO_OBRIGATORIO"));
        else if (dataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-IdadeMinimaAnos)))
            notifications.Add(new Notification("DataNascimento", "DATA_NASCIMENTO_MINIMA_INVALIDA"));

        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) notifications.AddRange(cpfResult.Notifications);

        var telefoneResult = Telefone.Criar(telefone);
        if (telefoneResult.IsFailure) notifications.AddRange(telefoneResult.Notifications);

        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure) notifications.AddRange(emailResult.Notifications);

        var senhaResult = Senha.Criar(senha);
        if (senhaResult.IsFailure) notifications.AddRange(senhaResult.Notifications);

        var enderecoResult = Endereco.Criar(logradouro, numero, complemento);
        if (enderecoResult.IsFailure) notifications.AddRange(enderecoResult.Notifications);

        if (notifications.Count != 0)
            return Result<Aluno>.Failure(notifications);

        var aluno = new Aluno(
            id, nome!, cpfResult.Value!, dataNascimento, telefoneResult.Value!,
            emailResult.Value!, enderecoResult.Value!, senhaResult.Value!, foto);

        return Result<Aluno>.Success(aluno);
    }
}
