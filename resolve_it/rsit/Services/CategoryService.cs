using rsit.Models;
using rsit.Repositories.Interfaces;
using rsit.Services.Interfaces;
using rsit.ViewModels.Admin;

namespace rsit.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<CategoryListViewModel> GetListAsync(CategoryListViewModel query)
    {
        query.Categories = await _repository.GetSummariesAsync(
            query.Search, query.Type, query.Status);
        return query;
    }

    public async Task<CategoryFormViewModel?> GetForEditAsync(int categoryId)
    {
        var category = await _repository.GetByIdAsync(categoryId);

        if (category == null)
            return null;

        return new CategoryFormViewModel
        {
            Id = category.CategoryId,
            Name = category.Name,
            Type = category.Type,
            Description = category.Description,
            Status = category.Status
        };
    }

    public async Task<ServiceResult> CreateAsync(CategoryFormViewModel model)
    {
        var name = model.Name.Trim();

        var validation = await ValidateAsync(name, model.Type, model.Status, excludeId: null);
        if (validation != null)
            return validation;

        await _repository.AddAsync(new Category
        {
            Name = name,
            Type = model.Type,
            Description = model.Description?.Trim() ?? string.Empty,
            Status = model.Status
        });

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(CategoryFormViewModel model)
    {
        var category = await _repository.GetByIdAsync(model.Id);

        if (category == null)
            return ServiceResult.Failure("Category not found.");

        var name = model.Name.Trim();

        var validation = await ValidateAsync(name, model.Type, model.Status, excludeId: model.Id);
        if (validation != null)
            return validation;

        category.Name = name;
        category.Type = model.Type;
        category.Description = model.Description?.Trim() ?? string.Empty;
        category.Status = model.Status;

        await _repository.UpdateAsync(category);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SetStatusAsync(int categoryId, bool activate)
    {
        var category = await _repository.GetByIdAsync(categoryId);

        if (category == null)
            return ServiceResult.Failure("Category not found.");

        category.Status = activate ? RecordStatus.Active : RecordStatus.Inactive;
        await _repository.UpdateAsync(category);

        return ServiceResult.Success();
    }

    private async Task<ServiceResult?> ValidateAsync(
        string name, string type, string status, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult.Failure("Category name is required.");

        if (!CategoryTypes.IsValid(type))
            return ServiceResult.Failure("Please select a valid category type.");

        if (!RecordStatus.IsValid(status))
            return ServiceResult.Failure("Please select a valid status.");

        if (await _repository.NameExistsAsync(name, excludeId))
            return ServiceResult.Failure("A category with this name already exists.");

        return null;
    }
}
