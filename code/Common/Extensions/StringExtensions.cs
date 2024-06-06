namespace Common.Extensions;

public static class StringExtensions
{
    public static DateOnly ToDateOnly(this string dateAsString)
    {

        if (dateAsString.Length > 10)
        {
            dateAsString = dateAsString.Substring(0, 10);
        }
        
        return DateOnly.ParseExact(dateAsString, "yyyy-MM-dd");
    }
}
