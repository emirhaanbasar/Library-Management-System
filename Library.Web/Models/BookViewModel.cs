using Library.Core;
using System.Collections.Generic;

namespace Library.Web.Models
{
    public class BookViewModel
    {
        public int? BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public List<int> SelectedCategoryIds { get; set; } = new();
        public List<Category>? AllCategories { get; set; }
    }
} 