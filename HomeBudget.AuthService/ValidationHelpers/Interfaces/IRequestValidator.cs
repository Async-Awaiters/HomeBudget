namespace HomeBudget.AuthService.ValidationHelpers.Interfaces
{
    public interface IRequestValidator<TRequest> where TRequest : class
    {
        void Validate(TRequest request);
    }
}