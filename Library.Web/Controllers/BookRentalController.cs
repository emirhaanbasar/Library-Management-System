using Library.Core;
using Library.Service;
using Library.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Library.Web.Services;

namespace Library.Web.Controllers
{
    [Authorize]
    public class BookRentalController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IBookRentalService _rentalService;
        private readonly MailService _mailService;
        private readonly IUserService _userService;
        private readonly ICategoryService _categoryService;
        // Not: Gerçek uygulamada kullanıcı kimliği oturumdan alınır
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        private string CurrentUserRole => User.FindFirstValue(ClaimTypes.Role);

        public BookRentalController(IBookService bookService, IBookRentalService rentalService, MailService mailService, IUserService userService, ICategoryService categoryService)
        {
            _bookService = bookService;
            _rentalService = rentalService;
            _mailService = mailService;
            _userService = userService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId, string? author, string? genre)
        {
            var books = (await _bookService.GetAllBooksAsync()).Where(b => b.Quantity > 0);
            var categories = await _categoryService.GetAllCategoriesAsync();
            var genres = books.Select(b => b.Genre).Distinct().OrderBy(g => g).ToList();
            var authors = books.Select(b => b.Author).Distinct().OrderBy(a => a).ToList();
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                books = books.Where(b => b.BookCategories.Any(bc => bc.CategoryId == categoryId.Value));
            }
            if (!string.IsNullOrEmpty(author))
            {
                books = books.Where(b => b.Author == author);
            }
            if (!string.IsNullOrEmpty(genre))
            {
                books = books.Where(b => b.Genre == genre);
            }
            if (!string.IsNullOrEmpty(search))
            {
                books = books.Where(b => b.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || b.Author.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || b.Genre.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || b.BookCategories.Any(bc => categories.Any(c => c.CategoryId == bc.CategoryId && c.Name.Contains(search, StringComparison.OrdinalIgnoreCase))));
            }
            ViewBag.Categories = categories;
            ViewBag.Genres = genres;
            ViewBag.Authors = authors;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedAuthor = author;
            ViewBag.SelectedGenre = genre;
            return View(books);
        }

        [HttpGet]
        public async Task<IActionResult> Rent(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null || book.Quantity < 1) return NotFound();
            int days = CurrentUserRole == "Student" ? 14 : 7;
            var model = new BookRentalViewModel
            {
                BookId = book.BookId,
                BookTitle = book.Title,
                StartDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(days)
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Rent(BookRentalViewModel model)
        {
            try
            {
                await _rentalService.RentBookAsync(model.BookId, CurrentUserId, model.StartDate, model.DueDate, CurrentUserRole);
                
                // E-posta bildirimi
                try
                {
                    var user = await _userService.GetUserByIdAsync(CurrentUserId);
                    var book = await _bookService.GetBookByIdAsync(model.BookId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        var subject = "Kitap Kiralama Başarılı";
                        var body = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                                <h2 style='color: #28a745;'>Kitap Kiralama Başarılı</h2>
                                <p>Sayın {user.FirstName} {user.LastName},</p>
                                <p>Kitap kiralama işleminiz başarıyla tamamlanmıştır.</p>
                                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                    <h3 style='margin-top: 0; color: #333;'>Kiralama Detayları:</h3>
                                    <ul style='list-style: none; padding: 0;'>
                                        <li><strong>Kitap:</strong> {book?.Title}</li>
                                        <li><strong>Yazar:</strong> {book?.Author}</li>
                                        <li><strong>Kiralama Tarihi:</strong> {model.StartDate:dd.MM.yyyy}</li>
                                        <li><strong>Teslim Tarihi:</strong> {model.DueDate:dd.MM.yyyy}</li>
                                    </ul>
                                </div>
                                <p>Kitabınızı teslim tarihine kadar iade etmeyi unutmayın.</p>
                                <hr style='margin: 20px 0; border: none; border-top: 1px solid #ddd;'>
                                <p style='font-size: 12px; color: #666;'>Bu e-posta otomatik olarak gönderilmiştir.</p>
                            </div>";
                        
                        await _mailService.SendMailAsync(user.Email, subject, body);
                        Console.WriteLine($"Kitap kiralama maili gönderildi: {user.Email}");
                    }
                }
                catch (Exception mailEx)
                {
                    Console.WriteLine($"Mail gönderilemedi: {mailEx.Message}");
                    // Mail hatası kiralama işlemini etkilemesin
                }
                
                TempData["SuccessMessage"] = "Kitap başarıyla kiralandı!";
                return RedirectToAction("MyRentals");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var book = await _bookService.GetBookByIdAsync(model.BookId);
                model.BookTitle = book?.Title ?? "";
                return View(model);
            }
        }

        public async Task<IActionResult> MyRentals()
        {
            var rentals = await _rentalService.GetActiveRentalsByUserAsync(CurrentUserId);
            return View(rentals);
        }

        [HttpPost]
        public async Task<IActionResult> Return(int rentalId)
        {
            try
            {
                var rental = await _rentalService.GetAllRentalsAsync();
                var rentalToReturn = rental.FirstOrDefault(r => r.RentalId == rentalId);
                
                await _rentalService.ReturnBookAsync(rentalId);
                
                // E-posta bildirimi
                try
                {
                    var user = await _userService.GetUserByIdAsync(CurrentUserId);
                    if (user != null && !string.IsNullOrEmpty(user.Email) && rentalToReturn?.Book != null)
                    {
                        var subject = "Kitap İade Edildi";
                        var body = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                                <h2 style='color: #28a745;'>Kitap İade Edildi</h2>
                                <p>Sayın {user.FirstName} {user.LastName},</p>
                                <p>Kitap iade işleminiz başarıyla tamamlanmıştır.</p>
                                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                    <h3 style='margin-top: 0; color: #333;'>İade Detayları:</h3>
                                    <ul style='list-style: none; padding: 0;'>
                                        <li><strong>Kitap:</strong> {rentalToReturn.Book.Title}</li>
                                        <li><strong>Yazar:</strong> {rentalToReturn.Book.Author}</li>
                                        <li><strong>İade Tarihi:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</li>
                                    </ul>
                                </div>
                                <p>Teşekkürler!</p>
                                <hr style='margin: 20px 0; border: none; border-top: 1px solid #ddd;'>
                                <p style='font-size: 12px; color: #666;'>Bu e-posta otomatik olarak gönderilmiştir.</p>
                            </div>";
                        
                        await _mailService.SendMailAsync(user.Email, subject, body);
                        Console.WriteLine($"Kitap iade maili gönderildi: {user.Email}");
                    }
                }
                catch (Exception mailEx)
                {
                    Console.WriteLine($"İade maili gönderilemedi: {mailEx.Message}");
                }
                
                TempData["SuccessMessage"] = "Kitap başarıyla iade edildi!";
                return RedirectToAction("MyRentals");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"İade işlemi başarısız: {ex.Message}";
                return RedirectToAction("MyRentals");
            }
        }
    }
} 