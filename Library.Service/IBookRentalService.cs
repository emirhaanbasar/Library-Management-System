using Library.Core;

namespace Library.Service
{
    public interface IBookRentalService
    {
        Task<IEnumerable<BookRental>> GetAllRentalsAsync();
        Task<IEnumerable<BookRental>> GetActiveRentalsByUserAsync(int userId);
        Task<int> GetActiveRentalCountByUserAsync(int userId);
        Task RentBookAsync(int bookId, int userId, DateTime startDate, DateTime dueDate, string userRole);
        Task ReturnBookAsync(int rentalId);
    }
} 