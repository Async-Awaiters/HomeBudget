using HomeBudget.Directories.EF.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeBudget.Directories.EF.DAL
{
    public class CurrencyRepository: IRepository<Currency>
    {
        private readonly DirectoriesContext _context;
        public CurrencyRepository(DirectoriesContext context) 
        {
            this._context = new DirectoriesContext();
        }

        public IEnumerable<Currency> GetAll()
        {
            return _context.Сurrencies;
        }

        public Currency GetById(Guid id)
        {
            return _context.Сurrencies.Find(id);
        }

        public void Create(Currency currency)
        {
            _context.Сurrencies.Add(currency);
        }

        public void Delete(Guid id)
        {
            var currency = _context.Сurrencies.Find(id);
            if (currency != null) 
            {
                _context.Сurrencies.Remove(currency);
            }
        }

        public void Update(Currency currency) 
        {
            _context.Entry(currency).State = EntityState.Modified;
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
