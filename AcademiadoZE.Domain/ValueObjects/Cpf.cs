using AcademiadoZE.Domain.Common;
using AcademiadoZE.Domain.Services;

namespace AcademiadoZE.Domain.ValueObjects;
//Mario Cesar alves Júnior

public record Cpf
{
    public string Valor { get; }

    private Cpf(string valor)
    {
        Valor = valor;
    }

    public static Result<Cpf> Criar(string? valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Cpf>.Failure("Cpf", "CPF_OBRIGATORIO");

   
        var textoLimpo = NormalizadoService.LimparEDigitos(valor);

        if (textoLimpo.Length != 11)
            return Result<Cpf>.Failure("Cpf", "CPF_DIGITOS");

        if (TodosDigitosIguais(textoLimpo) || !DigitosVerificadoresConferem(textoLimpo))
            return Result<Cpf>.Failure("Cpf", "CPF_INVALIDO");

        return Result<Cpf>.Success(new Cpf(textoLimpo));
    }

    private static bool TodosDigitosIguais(string cpf) => cpf.Distinct().Count() == 1;

    private static bool DigitosVerificadoresConferem(string cpf)
    {
        var digito1 = CalcularDigitoVerificador(cpf[..9], pesoInicial: 10);
        var digito2 = CalcularDigitoVerificador(cpf[..9] + digito1, pesoInicial: 11);
        return cpf[9] - '0' == digito1 && cpf[10] - '0' == digito2;
    }

    private static int CalcularDigitoVerificador(string baseNumerica, int pesoInicial)
    {
        var soma = 0;
        var peso = pesoInicial;
        foreach (var caractere in baseNumerica)
        {
            soma += (caractere - '0') * peso;
            peso--;
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public override string ToString() => Valor;
}
