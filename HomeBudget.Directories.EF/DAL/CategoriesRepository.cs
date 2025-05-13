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
            this._context = new DirectoriesContext();
        }

        public async Task<List<Categories>> GetAll()
        {
            IQueryable<Categories> query = _context.Categories.AsNoTracking().Where(category => !category.IsDeleted);
            
            return await query.ToListAsync();
        }

        public async Task<Categories> GetById(Guid id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(category => category.Id == id);
            return !category.IsDeleted ? category : null;
        }

        public async Task Create(Categories category)
        {
            var currencyDb = _context.Categories.AnyAsync(x => String.Equals(x.Name, category.Name) && String.Equals(x.ParentId, category.ParentId) && String.Equals(x.UserId, category.UserId));
            if (currencyDb.Result)
            {
                throw new Exception("Нет такой категории");
            }

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(category => category.Id == id);
            if (category != null)
            {
                category.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task Update(Categories category)
        {
            var categoryBD = await _context.Categories.FindAsync(category.Id);
            if (categoryBD != null)
            {
                _context.Entry(categoryBD).CurrentValues.SetValues(category);
                await _context.SaveChangesAsync();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
