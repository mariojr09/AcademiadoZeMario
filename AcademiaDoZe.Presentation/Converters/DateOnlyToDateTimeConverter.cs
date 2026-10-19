
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.Converters;

// Mario Cesar Alves Júnior
public class DateOnlyToDateTimeConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is DateOnly data
            ? data.ToDateTime(TimeOnly.MinValue)
            : DateTime.Today;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is DateTime data
            ? DateOnly.FromDateTime(data)
            : DateOnly.FromDateTime(DateTime.Today);
    }
}
