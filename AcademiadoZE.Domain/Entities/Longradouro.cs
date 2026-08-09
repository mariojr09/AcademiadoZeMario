using AcademiadoZE.Domain.Common;
using AcademiadoZE.Domain.Services;
using AcademiadoZE.Domain.ValueObjects;

namespace AcademiadoZE.Domain.Entities;
//Mario Cesar alves Júnior

public sealed class Logradouro : Entity
{
    public Cep Cep { get; }
    public string Pais { get; }
    public string Estado { get; }
    public string Cidade { get; }
    public string Bairro { get; }
    public string NomeLogradouro { get; }

    private Logradouro(int id, Cep cep, string pais, string estado, string cidade, string bairro, string nomeLogradouro)
        : base(id)
    {
        Cep = cep;
        Pais = pais;
        Estado = estado;
        Cidade = cidade;
        Bairro = bairro;
        NomeLogradouro = nomeLogradouro;
    }

    public static Result<Logradouro> Criar(
        int id, string? cep, string? pais, string? estado, string? cidade, string? bairro, string? nomeLogradouro)
    {
        var notifications = new List<Notification>();

        var cepResult = Cep.Criar(cep);
        if (cepResult.IsFailure)
            notifications.AddRange(cepResult.Notifications);

        if (NormalizadoService.TextoVazioOuNulo(pais))
            notifications.Add(new Notification("Pais", "PAIS_OBRIGATORIO"));
        else
            pais = NormalizadoService.LimparEspacos(pais);

        if (NormalizadoService.TextoVazioOuNulo(estado))
            notifications.Add(new Notification("Estado", "ESTADO_OBRIGATORIO"));
        else
            estado = NormalizadoService.ParaMaiusculo(NormalizadoService.LimparTodosEspacos(estado));

        if (NormalizadoService.TextoVazioOuNulo(cidade))
            notifications.Add(new Notification("Cidade", "CIDADE_OBRIGATORIO"));
        else
            cidade = NormalizadoService.LimparEspacos(cidade);

        if (NormalizadoService.TextoVazioOuNulo(bairro))
            notifications.Add(new Notification("Bairro", "BAIRRO_OBRIGATORIO"));
        else
            bairro = NormalizadoService.LimparEspacos(bairro);

        if (NormalizadoService.TextoVazioOuNulo(nomeLogradouro))
            notifications.Add(new Notification("NomeLogradouro", "NOME_LOGRADOURO_OBRIGATORIO"));
        else
            nomeLogradouro = NormalizadoService.LimparEspacos(nomeLogradouro);

        if (notifications.Count != 0)
            return Result<Logradouro>.Failure(notifications);

        var logradouro = new Logradouro(id, cepResult.Value!, pais!, estado!, cidade!, bairro!, nomeLogradouro!);
        return Result<Logradouro>.Success(logradouro);
    }
}
