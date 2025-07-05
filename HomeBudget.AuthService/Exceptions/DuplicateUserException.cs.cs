namespace HomeBudget.AuthService.Exceptions
{
    public class DuplicateUserException : Exception
    {
        public DuplicateUserException(string field, string value)
            : base($"User with {field} '{value}' already exists.")
        {
        }
    }
}