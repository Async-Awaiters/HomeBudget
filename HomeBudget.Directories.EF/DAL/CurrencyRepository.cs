using HomeBudget.Directories.EF.DAL.Interfaces;
using HomeBudget.Directories.EF.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeBudget.Directories.EF.DAL
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly DirectoriesContext _context;

        public CurrencyRepository(DirectoriesContext context)
        {
            _context = context;
        }

        public IQueryable<Currency> GetAll(CancellationToken cancellationToken)
        {
            IQueryable<Currency> query = _context.Сurrencies.AsNoTracking();

            return query;
        }

        public async Task<Currency?> GetById(Guid id, CancellationToken cancellationToken)
        {
            var currency = await _context.Сurrencies.FirstOrDefaultAsync(currency => currency.Id == id);
            return currency is not null ? currency : null;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
