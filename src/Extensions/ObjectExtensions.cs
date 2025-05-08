namespace Markwardt;

public static class ObjectExtensions
{
    public static bool ValueEquals(this object? x, object? y)
        => (x is null && y is null) || (x is not null && y is not null && x.Equals(y));

    public static T Do<T>(this T target, Action<T> action)
    {
        action(target);
        return target;
    }
}