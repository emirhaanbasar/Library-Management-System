using Library.Core;

namespace Library.Data
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(LibraryDbContext context) : base(context) { }
        // Kategorilere özel metotlar burada eklenebilir
    }
} 