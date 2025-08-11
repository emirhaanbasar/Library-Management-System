using Library.Core;

namespace Library.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<IEnumerable<User>> GetAllUsersAsync() => await _userRepository.GetAllAsync();
        public async Task<User?> GetUserByIdAsync(int id) => await _userRepository.GetByIdAsync(id);
        public async Task AddUserAsync(User user)
        {
            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();
        }
        public async Task UpdateUserAsync(User user)
        {
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
        }
        public async Task DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user != null)
            {
                _userRepository.Delete(user);
                await _userRepository.SaveAsync();
            }
        }
    }
} 