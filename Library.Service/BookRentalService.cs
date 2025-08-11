using Library.Core;

namespace Library.Service
{
    public class BookRentalService : IBookRentalService
    {
        private readonly IBookRentalRepository _rentalRepository;
        private readonly IBookRepository _bookRepository;
        public BookRentalService(IBookRentalRepository rentalRepository, IBookRepository bookRepository)
        {
            _rentalRepository = rentalRepository;
            _bookRepository = bookRepository;
        }
        public async Task<IEnumerable<BookRental>> GetAllRentalsAsync() => await _rentalRepository.GetAllAsync();
        public async Task<IEnumerable<BookRental>> GetActiveRentalsByUserAsync(int userId) => await _rentalRepository.GetActiveRentalsByUserAsync(userId);
        public async Task<int> GetActiveRentalCountByUserAsync(int userId) => await _rentalRepository.GetActiveRentalCountByUserAsync(userId);
        public async Task RentBookAsync(int bookId, int userId, DateTime startDate, DateTime dueDate, string userRole)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null || book.Quantity < 1)
                throw new Exception("Kitap mevcut değil veya stokta yok.");
            var activeCount = await _rentalRepository.GetActiveRentalCountByUserAsync(userId);
            int maxBooks = userRole == "Student" ? 5 : 3;
            if (activeCount >= maxBooks)
                throw new Exception($"En fazla {maxBooks} kitap kiralayabilirsiniz.");
            // Süre kontrolü (öğrenciye özel daha uzun süre)
            int maxDays = userRole == "Student" ? 14 : 7;
            if ((dueDate - startDate).TotalDays > maxDays)
                throw new Exception($"{maxDays} günden fazla kiralama yapılamaz.");
            book.Quantity--;
            _bookRepository.Update(book);
            await _bookRepository.SaveAsync();
            var rental = new BookRental
            {
                BookId = bookId,
                UserId = userId,
                RentalDate = startDate,
                DueDate = dueDate,
                ReturnDate = null
            };
            await _rentalRepository.AddAsync(rental);
            await _rentalRepository.SaveAsync();
        }
        public async Task ReturnBookAsync(int rentalId)
        {
            var rental = await _rentalRepository.GetByIdAsync(rentalId);
            if (rental != null && rental.ReturnDate == null)
            {
                rental.ReturnDate = DateTime.Now;
                _rentalRepository.Update(rental);
                await _rentalRepository.SaveAsync();
                var book = await _bookRepository.GetByIdAsync(rental.BookId);
                if (book != null)
                {
                    book.Quantity++;
                    _bookRepository.Update(book);
                    await _bookRepository.SaveAsync();
                }
            }
        }
    }
} 