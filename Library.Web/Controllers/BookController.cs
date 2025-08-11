using Library.Core;
using Library.Service;
using Library.Web.Models;
using Library.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ICategoryService _categoryService;
        private readonly BookCsvImportService _csvImportService;
        private static bool _imported = false;
        public BookController(IBookService bookService, ICategoryService categoryService, BookCsvImportService csvImportService)
        {
            _bookService = bookService;
            _categoryService = categoryService;
            _csvImportService = csvImportService;
        }
        public async Task<IActionResult> Index()
        {
            if (!_imported)
            {
                var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "Books_data - Sheet1.csv");
                _csvImportService.ImportBooksFromCsv(csvPath);
                _imported = true;
            }
            var books = await _bookService.GetAllBooksAsync();
            return View(books);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var model = new BookViewModel { AllCategories = categories.ToList() };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Create(BookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllCategories = (await _categoryService.GetAllCategoriesAsync()).ToList();
                return View(model);
            }
            var book = new Book
            {
                Title = model.Title,
                Author = model.Author,
                Genre = model.Genre,
                Quantity = model.Quantity
            };
            await _bookService.AddBookAsync(book, model.SelectedCategoryIds);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null) return NotFound();
            var categories = await _categoryService.GetAllCategoriesAsync();
            var model = new BookViewModel
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                Genre = book.Genre,
                Quantity = book.Quantity,
                SelectedCategoryIds = book.BookCategories?.Select(bc => bc.CategoryId).ToList() ?? new(),
                AllCategories = categories.ToList()
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(BookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllCategories = (await _categoryService.GetAllCategoriesAsync()).ToList();
                return View(model);
            }
            var book = new Book
            {
                BookId = model.BookId ?? 0,
                Title = model.Title,
                Author = model.Author,
                Genre = model.Genre,
                Quantity = model.Quantity
            };
            await _bookService.UpdateBookAsync(book, model.SelectedCategoryIds);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _bookService.DeleteBookAsync(id);
            return RedirectToAction("Index");
        }
    }
} 