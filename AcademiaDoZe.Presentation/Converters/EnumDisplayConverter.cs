
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.Converters;

// Mario Cesar Alves Júnior
public class EnumDisplayConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not Enum enumValue)
            return string.Empty;

        var member = enumValue.GetType()
            .GetMember(enumValue.ToString());

        if (member.Length == 0)
            return enumValue.ToString();

        var display = member[0]
            .GetCustomAttribute<DisplayAttribute>();

        return display?.GetName() ?? enumValue.ToString();
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
