namespace Library.Data
{
    using Microsoft.EntityFrameworkCore;
    using Library.Core;

    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }
        public LibraryDbContext() { }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<SeatReservation> SeatReservations { get; set; }
        public DbSet<BookRental> BookRentals { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TC ve Email için unique constraint
            modelBuilder.Entity<User>()
                .HasIndex(u => u.TC)
                .IsUnique()
                .HasFilter("[TC] IS NOT NULL");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Koltuk numarası unique olmalı
            modelBuilder.Entity<Seat>()
                .HasIndex(s => s.SeatNumber)
                .IsUnique();

            // BookRental ilişkileri
            modelBuilder.Entity<BookRental>()
                .HasOne(br => br.Book)
                .WithMany(b => b.BookRentals)
                .HasForeignKey(br => br.BookId);

            modelBuilder.Entity<BookRental>()
                .HasOne(br => br.User)
                .WithMany(u => u.BookRentals)
                .HasForeignKey(br => br.UserId);

            // SeatReservation ilişkileri
            modelBuilder.Entity<SeatReservation>()
                .HasOne(sr => sr.Seat)
                .WithMany(s => s.SeatReservations)
                .HasForeignKey(sr => sr.SeatId);

            modelBuilder.Entity<SeatReservation>()
                .HasOne(sr => sr.User)
                .WithMany(u => u.SeatReservations)
                .HasForeignKey(sr => sr.UserId);

            // BookCategory composite key
            modelBuilder.Entity<BookCategory>()
                .HasKey(bc => new { bc.BookId, bc.CategoryId });
            modelBuilder.Entity<BookCategory>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BookCategories)
                .HasForeignKey(bc => bc.BookId);
            modelBuilder.Entity<BookCategory>()
                .HasOne(bc => bc.Category)
                .WithMany(c => c.BookCategories)
                .HasForeignKey(bc => bc.CategoryId);
        }
    }
} 