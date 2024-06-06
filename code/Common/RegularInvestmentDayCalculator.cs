namespace Common;

public static class RegularInvestmentDayCalculator
{

    public static bool IsRegularInvestmentDay(DateOnly date)
    {
        if (date.Day == 10 && (
                date.DayOfWeek == DayOfWeek.Monday ||
                date.DayOfWeek == DayOfWeek.Tuesday ||
                date.DayOfWeek == DayOfWeek.Wednesday ||
                date.DayOfWeek == DayOfWeek.Thursday ||
                date.DayOfWeek == DayOfWeek.Friday))
        {
            return true;
        }

        if (  (date.Day == 11 || date.Day == 12) && date.DayOfWeek == DayOfWeek.Monday)
        {
            return true;
        }

        return false;
    }
}
