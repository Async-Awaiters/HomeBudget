using HomeBudget.Directories.EF.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;


namespace HomeBudget.Directories.EF.DAL
{

    public class CategoriesRepository : IRepository<Categories>
    {
        private readonly DirectoriesContext _context;
        public CategoriesRepository(DirectoriesContext context)
        {
            this._context = new DirectoriesContext();
        }

        public IEnumerable<Categories> GetAll()
        {
            return _context.Categories;
        }

        public Categories GetById(Guid id)
        {
            return _context.Categories.Find(id);
        }

        public void Create(Categories category)
        {
            _context.Categories.Add(category);
        }

        public void Delete(Guid id)
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }
        }

        public void Update(Categories category)
        {
            _context.Entry(category).State = EntityState.Modified;
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
