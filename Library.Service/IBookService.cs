using Library.Core;

namespace Library.Service
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book?> GetBookByIdAsync(int id);
        Task AddBookAsync(Book book, IEnumerable<int> categoryIds);
        Task UpdateBookAsync(Book book, IEnumerable<int> categoryIds);
        Task DeleteBookAsync(int id);
    }
} 