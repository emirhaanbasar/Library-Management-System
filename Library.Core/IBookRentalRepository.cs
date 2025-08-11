namespace Library.Core
{
    public interface IBookRentalRepository : IRepository<BookRental>
    {
        Task<int> GetActiveRentalCountByUserAsync(int userId);
        Task<IEnumerable<BookRental>> GetActiveRentalsByUserAsync(int userId);
    }
} 