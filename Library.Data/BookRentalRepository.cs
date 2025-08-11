using Library.Core;
using Microsoft.EntityFrameworkCore;

namespace Library.Data
{
    public class BookRentalRepository : Repository<BookRental>, IBookRentalRepository
    {
        private readonly LibraryDbContext _context;
        public BookRentalRepository(LibraryDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<int> GetActiveRentalCountByUserAsync(int userId)
        {
            return await _context.BookRentals.CountAsync(r => r.UserId == userId && r.ReturnDate == null);
        }
        public async Task<IEnumerable<BookRental>> GetActiveRentalsByUserAsync(int userId)
        {
            return await _context.BookRentals.Where(r => r.UserId == userId && r.ReturnDate == null).ToListAsync();
        }
    }
} 