
using System;
using System.Globalization;
using System.IO;
using AcademiaDoZe.Application.DTOs;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.Converters;

// Mario Cesar Alves Júnior
public class FotoToImageSourceConverter : IValueConverter
{
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is ArquivoDto foto &&
            foto.Conteudo is { Length: > 0 })
        {
            return ImageSource.FromStream(
                () => new MemoryStream(foto.Conteudo));
        }

        return null;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
