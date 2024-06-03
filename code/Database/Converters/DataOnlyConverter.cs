using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Database.Converters;

public class DateOnlyConverter : ValueConverter<DateOnly?, string>
{
    public DateOnlyConverter() : base(
        dateOnly => ConvertFromDateOnlyToString(dateOnly), 
        dateOnlyAsString => ConvertFromStringToDateOnly(dateOnlyAsString))
         { }

    private static string ConvertFromDateOnlyToString(DateOnly? dateOnly)
    {
        return dateOnly?.ToString("yyyy-MM-dd");
    }
    
    private static DateOnly? ConvertFromStringToDateOnly(string dateOnlyAsString)
    {
        if (string.IsNullOrWhiteSpace(dateOnlyAsString))
        {
            return null;
        }

        return DateOnly.ParseExact(dateOnlyAsString, "yyyy-MM-dd");
    }
}
