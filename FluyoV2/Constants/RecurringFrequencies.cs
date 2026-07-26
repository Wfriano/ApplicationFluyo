namespace FluyoV2.Constants;

public static class RecurringFrequencies
{
    public const string Weekly = "Weekly";
    public const string Biweekly = "Biweekly";
    public const string Monthly = "Monthly";

    public static bool IsValid(string frequency)
    {
        return frequency == Weekly
            || frequency == Biweekly
            || frequency == Monthly;
    }
}