namespace HomeBudget.Directories.EF.Exceptions
{
    public class EntityAlreadyExistsException : Exception
    {
        public EntityAlreadyExistsException(string message) 
            : base(message) { }
    }
}
