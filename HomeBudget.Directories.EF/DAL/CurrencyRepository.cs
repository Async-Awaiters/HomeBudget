using HomeBudget.Directories.EF.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace HomeBudget.Directories.EF.DAL
{
    public class CurrencyRepository: IGetRepository<Currency>, ICreateRepository<Currency>, IUpdateRepository<Currency>
    {
        private readonly DirectoriesContext _context;
        public CurrencyRepository(DirectoriesContext context) 
        {
            this._context = new DirectoriesContext();
        }

        public async Task<IEnumerable<Currency>> GetAll()
        {
            return await _context.Currency.ToListAsync();
        }

        public async Task<Currency> GetById(Guid id)
        {
            return await _context.Currency.FirstOrDefaultAsync(currency => currency.Id == id);
        }

        public async Task Create(Currency currency)
        {
            await _context.Currency.AddAsync(currency);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Currency currency) 
        {
            _context.Entry(currency).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
