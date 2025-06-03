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
            this._context = context;
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

        public async Task Create(Currency currency, CancellationToken cancellationToken)
        {
            var currencyDb = _context.Сurrencies.AnyAsync(cur => String.Equals(cur.Name, currency.Name) && String.Equals(cur.Code, currency.Code) && String.Equals(cur.Country, cur.Country));
            if (currencyDb.Result)
            {
                throw new Exception("Нет такой валюты");
            }

            await _context.Сurrencies.AddAsync(currency);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Update(Currency currency, CancellationToken cancellationToken) 
        {
            var currencyBD = await _context.Сurrencies.FindAsync(currency.Id);
            if (currencyBD != null)
            {
                _context.Entry(currencyBD).CurrentValues.SetValues(currency);
                await _context.SaveChangesAsync();
                return true;
            }
            else return false;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
