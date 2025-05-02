using HomeBudget.Directories.EF.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;


namespace HomeBudget.Directories.EF.DAL
{
    public class CategoriesRepository : IGetRepository<Categories>, ICreateRepository<Categories>, IDeleteRepository<Categories>, IUpdateRepository<Categories>
    {
        private readonly DirectoriesContext _context;
        public CategoriesRepository(DirectoriesContext context)
        {
            this._context = new DirectoriesContext();
        }

        public async Task<IEnumerable<Categories>> GetAll()
        {
            return await _context.Categories.Where(category => !category.IsDeleted).ToListAsync();
        }

        public async Task<Categories> GetById(Guid id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(category => category.Id == id);
            return !category.IsDeleted ? category : null;
        }

        public async Task Create(Categories category)
        {
           
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
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
