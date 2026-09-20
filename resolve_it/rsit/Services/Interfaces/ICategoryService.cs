using rsit.ViewModels.Admin;

namespace rsit.Services.Interfaces;

public interface ICategoryService
{
    Task<CategoryListViewModel> GetListAsync(CategoryListViewModel query);

    Task<CategoryFormViewModel?> GetForEditAsync(int categoryId);

    Task<ServiceResult> CreateAsync(CategoryFormViewModel model);

    Task<ServiceResult> UpdateAsync(CategoryFormViewModel model);

    Task<ServiceResult> SetStatusAsync(int categoryId, bool activate);
}
