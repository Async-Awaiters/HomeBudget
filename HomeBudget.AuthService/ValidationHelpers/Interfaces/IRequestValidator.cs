namespace HomeBudget.AuthService.ValidationHelpers.Interfaces
{
    public interface IRequestValidator<TRequest> where TRequest : class
    {
        Dictionary<string, object?> ValidateRequest(TRequest request);
    }
}
