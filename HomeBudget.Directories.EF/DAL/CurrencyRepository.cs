using HomeBudget.Directories.EF.DAL.Interfaces;
using HomeBudget.Directories.EF.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace HomeBudget.Directories.EF.DAL
{
    public class CurrencyRepository: ICurrencyRepository
    {
        private readonly DirectoriesContext _context;
        public CurrencyRepository(DirectoriesContext context) 
        {
            this._context = new DirectoriesContext();
        }

        public async Task<List<Currency>> GetAll()
        {
            IQueryable<Currency> query = _context.Сurrencies.AsNoTracking();

            return await query.ToListAsync();
        }

        public async Task<Currency> GetById(Guid id)
        {
            return await _context.Сurrencies.FirstOrDefaultAsync(currency => currency.Id == id);
        }

        public async Task Create(Currency currency)
        {
            var currencyDb = _context.Сurrencies.AnyAsync(cur => String.Equals(cur.Name, currency.Name) && String.Equals(cur.Code, currency.Code) && String.Equals(cur.Country, cur.Country));
            if (currencyDb.Result)
            {
                throw new Exception("Нет такой валюты");
            }

            await _context.Сurrencies.AddAsync(currency);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Currency currency) 
        {
            var currencyBD = await _context.Сurrencies.FindAsync(currency.Id);
            if (currencyBD != null)
            {
                _context.Entry(currencyBD).CurrentValues.SetValues(currency);
                await _context.SaveChangesAsync();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
