using Library.Core;

namespace Library.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync() => await _categoryRepository.GetAllAsync();
        public async Task<Category?> GetCategoryByIdAsync(int id) => await _categoryRepository.GetByIdAsync(id);
        public async Task AddCategoryAsync(Category category)
        {
            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveAsync();
        }
        public async Task UpdateCategoryAsync(Category category)
        {
            _categoryRepository.Update(category);
            await _categoryRepository.SaveAsync();
        }
        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category != null)
            {
                _categoryRepository.Delete(category);
                await _categoryRepository.SaveAsync();
            }
        }
    }
} 