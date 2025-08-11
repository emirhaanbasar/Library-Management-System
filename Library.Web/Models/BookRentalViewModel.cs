using Library.Core;
using System;
using System.Collections.Generic;

namespace Library.Web.Models
{
    public class BookRentalViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
        public List<Book>? AvailableBooks { get; set; }
    }
} 