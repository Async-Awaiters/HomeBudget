using HomeBudget.Directories.EF.DAL.Interfaces;
using HomeBudget.Directories.EF.DAL.Models;
using HomeBudget.Directories.EF.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HomeBudget.Directories.EF.DAL
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly DirectoriesContext _context;

        public CategoriesRepository(DirectoriesContext context)
        {
            _context = context;
        }

        public IQueryable<Category> GetAll(CancellationToken cancellationToken)
        {
            var query = _context.Categories.Where(category => !category.IsDeleted);

            return query;
        }

        public async Task<Category?> GetById(Guid id, CancellationToken cancellationToken)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(category => category.Id == id);
            return category is not null && !category.IsDeleted
                ? category
                : null;
        }

        public async Task Create(Category category, CancellationToken cancellationToken)
        {
            var currencyDb = _context.Categories.AnyAsync(x => string.Equals(x.Name, category.Name) && Equals(x.ParentId, category.ParentId) && Equals(x.UserId, category.UserId));
            if (currencyDb.Result)
            {
                throw new EntityNotFoundException("Такая категория уже существует");
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
            else
            {
                throw new EntityNotFoundException("Категория не найдена.");
            }
        }

        public async Task Update(Category category, CancellationToken cancellationToken)
        {
            var categoryBD = await _context.Categories.FirstOrDefaultAsync(x => x.Id == category.Id);
            if (categoryBD != null)
            {
                _context.Entry(categoryBD).CurrentValues.SetValues(category);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new EntityNotFoundException("Категория не найдена.");
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
