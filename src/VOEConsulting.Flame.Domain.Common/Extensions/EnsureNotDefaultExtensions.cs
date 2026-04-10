namespace VOEConsulting.Flame.Domain.Common.Extensions;

public static class EnsureNotDefaultExtensions
{
    public static Guid EnsureNotDefault(this Guid value, string paramName)
    {
        if (value == default)
            throw new ArgumentException($"{paramName} cannot be default.", paramName);
        return value;
    }
}

