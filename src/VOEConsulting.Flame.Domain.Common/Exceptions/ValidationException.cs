namespace VOEConsulting.Flame.Domain.Common.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(IReadOnlyList<string> errors)
        : base("Validation failed.")
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}

