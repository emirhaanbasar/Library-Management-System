namespace Library.Core
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public ICollection<BookRental> BookRentals { get; set; } = new List<BookRental>();
        public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
    }
} 