using AcademiadoZE.Domain.ValueObjects;

namespace AcademiadoZE.Domain.Entities;
//Mario Cesar alves Júnior
public class Aluno : Pessoa
{
    public Aluno(
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
}