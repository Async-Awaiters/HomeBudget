using HomeBudget.Directories.EF.DAL.Interfaces;
using HomeBudget.Directories.EF.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;

namespace HomeBudget.Directories.EF.DAL
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly DirectoriesContext _context;

        public CategoriesRepository(DirectoriesContext context)
        {
            this._context = context;
        }

        public async Task<IQueryable<Categories>> GetAll(CancellationToken cancellationToken)
        {
            var query = _context.Categories.Where(category => !category.IsDeleted);
            
            return query;
        }

        public async Task<Categories?> GetById(Guid id, CancellationToken cancellationToken)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(category => category.Id == id);
            return category is not null && !category.IsDeleted
                ? category
                : null;
        }

        public async Task Create(Categories category, CancellationToken cancellationToken)
        {
            var currencyDb = _context.Categories.AnyAsync(x => String.Equals(x.Name, category.Name) && String.Equals(x.ParentId, category.ParentId) && String.Equals(x.UserId, category.UserId));
            if (currencyDb.Result)
            {
                throw new Exception("Нет такой категории");
            }

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(category => category.Id == id);
            if (category != null)
            {
                category.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> Update(Categories category, CancellationToken cancellationToken)
        {
            var categoryBD = await _context.Categories.FindAsync(category.Id);
            if (categoryBD != null)
            {
                _context.Entry(categoryBD).CurrentValues.SetValues(category);
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
