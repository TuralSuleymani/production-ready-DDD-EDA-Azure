namespace VOEConsulting.Flame.OrderManagement.Api;

internal static class DomainErrorHttp
{
    public static IResult Map(DomainError error)
    {
        if (error.ErrorType == ErrorType.NotFound)
            return Results.NotFound();
        if (error.ErrorType == ErrorType.Conflict)
            return Results.Conflict();
        if (error.ErrorType == ErrorType.BadRequest)
            return Results.BadRequest(new { error = error.ErrorMessage });
        if (error.ErrorType == ErrorType.Validation)
            return Results.BadRequest(new { error = error.ErrorMessage, errors = error.Errors });
        return Results.Problem(error.ErrorMessage);
    }
}
