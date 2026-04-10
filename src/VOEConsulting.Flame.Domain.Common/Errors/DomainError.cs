namespace VOEConsulting.Flame.Domain.Common.Errors;

public sealed record DomainError : IDomainError
{
    public static DomainError Conflict(string? message = null) =>
        new(message ?? "The data provided conflicts with existing data.", ErrorType.Conflict, null);

    public static DomainError NotFound(string? message = null) =>
        new(message ?? "The requested item could not be found.", ErrorType.NotFound, null);

    public static DomainError BadRequest(string? message = null) =>
        new(message ?? "Invalid request or parameters.", ErrorType.BadRequest, null);

    public static DomainError Validation(string? message = null, IReadOnlyList<string>? errors = null) =>
        new(message ?? "Validation failed.", ErrorType.Validation, errors);

    public static DomainError Unexpected(string? message = null) =>
        new(message ?? "Unexpected error happened.", ErrorType.Unexpected, null);

    private DomainError(string? message, ErrorType errorType, IReadOnlyList<string>? errors)
    {
        ErrorMessage = message;
        ErrorType = errorType;
        Errors = errors;
    }

    public string? ErrorMessage { get; }
    public ErrorType ErrorType { get; }
    public IReadOnlyList<string>? Errors { get; }
}
