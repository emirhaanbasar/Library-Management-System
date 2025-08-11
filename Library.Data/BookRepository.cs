using Library.Core;

namespace Library.Data
{
    public class BookRepository : Repository<Book>, IBookRepository
    {
        public BookRepository(LibraryDbContext context) : base(context) { }
        // Kitaplara özel metotlar burada eklenebilir
    }
} 