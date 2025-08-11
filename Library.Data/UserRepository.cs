using Library.Core;

namespace Library.Data
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(LibraryDbContext context) : base(context) { }
        // User'a özel metotlar burada eklenebilir
    }
} 