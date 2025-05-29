namespace HomeBudget.Directories.Services.Implementations
{
    public class ServiceTimeoutsOptions
    {
        public int CategoryService { get; set; } = 30000;
        public int CurrencyService { get; set; } = 30000;
    }
}
