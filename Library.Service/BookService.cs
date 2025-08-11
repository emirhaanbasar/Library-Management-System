using Library.Core;
using Library.Data;

namespace Library.Service
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly LibraryDbContext _context;
        public BookService(IBookRepository bookRepository, ICategoryRepository categoryRepository, LibraryDbContext context)
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
            _context = context;
        }
        public async Task<IEnumerable<Book>> GetAllBooksAsync() => await _bookRepository.GetAllAsync();
        public async Task<Book?> GetBookByIdAsync(int id) => await _bookRepository.GetByIdAsync(id);
        public async Task AddBookAsync(Book book, IEnumerable<int> categoryIds)
        {
            book.BookRentals = new List<BookRental>();
            await _bookRepository.AddAsync(book);
            await _bookRepository.SaveAsync();
            foreach (var catId in categoryIds)
            {
                _context.Set<BookCategory>().Add(new BookCategory { BookId = book.BookId, CategoryId = catId });
            }
            await _context.SaveChangesAsync();
        }
        public async Task UpdateBookAsync(Book book, IEnumerable<int> categoryIds)
        {
            _bookRepository.Update(book);
            await _bookRepository.SaveAsync();
            var existing = _context.Set<BookCategory>().Where(bc => bc.BookId == book.BookId);
            _context.Set<BookCategory>().RemoveRange(existing);
            foreach (var catId in categoryIds)
            {
                _context.Set<BookCategory>().Add(new BookCategory { BookId = book.BookId, CategoryId = catId });
            }
            await _context.SaveChangesAsync();
        }
        public async Task DeleteBookAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book != null)
            {
                _bookRepository.Delete(book);
                await _bookRepository.SaveAsync();
            }
        }
    }
} 