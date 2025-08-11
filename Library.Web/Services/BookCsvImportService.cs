using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Library.Core;
using Library.Data;

namespace Library.Web.Services
{
    public class BookCsvImportService
    {
        private readonly LibraryDbContext _context;
        public BookCsvImportService(LibraryDbContext context)
        {
            _context = context;
        }

        public void ImportBooksFromCsv(string csvPath)
        {
            if (!File.Exists(csvPath)) return;
            using var reader = new StreamReader(csvPath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                PrepareHeaderForMatch = args => args.Header.ToLower(),
            };
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<BookCsvRow>().ToList();
            foreach (var row in records)
            {
                // Kategori ekle
                var category = _context.Categories.FirstOrDefault(c => c.Name == row.category);
                if (category == null)
                {
                    category = new Category { Name = row.category };
                    _context.Categories.Add(category);
                    _context.SaveChanges();
                }
                // Kitap ekle
                var book = _context.Books.FirstOrDefault(b => b.Title == row.title && b.Author == row.author);
                if (book == null)
                {
                    book = new Book
                    {
                        Title = row.title,
                        Author = row.author,
                        Genre = row.category,
                        Quantity = 1, // Her kitap için 1 adet
                        BookCategories = new List<BookCategory>()
                    };
                    _context.Books.Add(book);
                    _context.SaveChanges();
                }
                // Kitap-Kategori ilişkisi ekle
                if (!_context.BookCategories.Any(bc => bc.BookId == book.BookId && bc.CategoryId == category.CategoryId))
                {
                    _context.BookCategories.Add(new BookCategory { BookId = book.BookId, CategoryId = category.CategoryId });
                    _context.SaveChanges();
                }
            }
        }

        private class BookCsvRow
        {
            public string title { get; set; } = string.Empty;
            public string author { get; set; } = string.Empty;
            public string category { get; set; } = string.Empty;
            // bid ve status kullanılmaz
        }
    }
} 