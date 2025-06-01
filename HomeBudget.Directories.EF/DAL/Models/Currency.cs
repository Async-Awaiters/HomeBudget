namespace HomeBudget.Directories.EF.DAL.Models
{
    public class Currency
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Country { get; set; }
    }
}
