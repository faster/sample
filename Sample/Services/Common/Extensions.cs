namespace Services.Common;

public static class Extensions
{
    public static bool IsTypeA(this string name)
    {
        return name.StartsWith("A") ? true : false;
    }
}
