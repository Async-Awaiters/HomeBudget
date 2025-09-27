namespace HomeBudget.AuthService.ValidationHelpers.Interfaces
{
    public interface IUpdateRequestValidator<TRequest> where TRequest : class
    {
        Dictionary<string, object?> Validate(TRequest request);
    }
}
