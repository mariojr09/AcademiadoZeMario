using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Presentation.Converters;

// Mario Cesar Alves Júnior
public sealed class RestricoesFlagsDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AppMatriculaRestricoes restricoes ||
            restricoes == AppMatriculaRestricoes.None)
        {
            return ObterDescricao(AppMatriculaRestricoes.None);
        }

        return string.Join(", ",
            Enum.GetValues<AppMatriculaRestricoes>()
                .Where(item => item != AppMatriculaRestricoes.None && restricoes.HasFlag(item))
                .Select(ObterDescricao));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static string ObterDescricao(AppMatriculaRestricoes valor) =>
        valor.GetType()
            .GetMember(valor.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()?
            .GetName() ?? valor.ToString();
}
