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

        public IQueryable<Category> GetAll(Guid userId, CancellationToken cancellationToken)
        {
            var query = _context.Categories.Where(category => !category.IsDeleted && (category.UserId == userId || category.UserId == null));
            return query;
        }

        public async Task<Category?> GetById(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(category => category.Id == id && (category.UserId == userId || category.UserId == null));
            return category is not null && !category.IsDeleted
                ? category
                : null;
        }

        public async Task Create(Category category, CancellationToken cancellationToken)
        {
            var currencyDb = await _context.Categories.AnyAsync(x => string.Equals(x.Name, category.Name) && Equals(x.ParentId, category.ParentId) && Equals(x.UserId, category.UserId));
            if (currencyDb)
            {
                throw new EntityAlreadyExistsException("Такая категория уже существует.");
            }

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(category => category.Id == id && category.UserId == userId && !category.IsDeleted);
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

        public async Task Update(Guid userId, Guid id, Category category, CancellationToken cancellationToken)
        {
            var categoryBD = await _context.Categories.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (categoryBD != null)
            {
                categoryBD.Name = category.Name;
                categoryBD.ParentId = category.ParentId;
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
