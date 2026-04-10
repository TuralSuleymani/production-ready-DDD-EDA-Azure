namespace VOEConsulting.Flame.Domain.Common.Errors;

public interface IDomainError
{
    string? ErrorMessage { get; }
    ErrorType ErrorType { get; }
    IReadOnlyList<string>? Errors { get; }
}
