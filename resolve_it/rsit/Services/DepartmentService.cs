using rsit.Models;
using rsit.Repositories.Interfaces;
using rsit.Services.Interfaces;
using rsit.ViewModels.Admin;

namespace rsit.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;

    public DepartmentService(IDepartmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<DepartmentListViewModel> GetListAsync(DepartmentListViewModel query)
    {
        query.Departments = await _repository.GetSummariesAsync(query.Search, query.Status);
        return query;
    }

    public async Task<DepartmentFormViewModel?> GetForEditAsync(int departmentId)
    {
        var department = await _repository.GetByIdAsync(departmentId);

        if (department == null)
            return null;

        return new DepartmentFormViewModel
        {
            Id = department.DepartmentId,
            Name = department.Name,
            Description = department.Description,
            Status = department.Status
        };
    }

    public async Task<ServiceResult> CreateAsync(DepartmentFormViewModel model)
    {
        var name = model.Name.Trim();

        var validation = await ValidateAsync(name, model.Status, excludeId: null);
        if (validation != null)
            return validation;

        await _repository.AddAsync(new Department
        {
            Name = name,
            Description = model.Description?.Trim() ?? string.Empty,
            Status = model.Status
        });

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(DepartmentFormViewModel model)
    {
        var department = await _repository.GetByIdAsync(model.Id);

        if (department == null)
            return ServiceResult.Failure("Department not found.");

        var name = model.Name.Trim();

        var validation = await ValidateAsync(name, model.Status, excludeId: model.Id);
        if (validation != null)
            return validation;

        department.Name = name;
        department.Description = model.Description?.Trim() ?? string.Empty;
        department.Status = model.Status;

        await _repository.UpdateAsync(department);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SetStatusAsync(int departmentId, bool activate)
    {
        var department = await _repository.GetByIdAsync(departmentId);

        if (department == null)
            return ServiceResult.Failure("Department not found.");

        department.Status = activate ? RecordStatus.Active : RecordStatus.Inactive;
        await _repository.UpdateAsync(department);

        return ServiceResult.Success();
    }

    private async Task<ServiceResult?> ValidateAsync(string name, string status, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult.Failure("Department name is required.");

        if (!RecordStatus.IsValid(status))
            return ServiceResult.Failure("Please select a valid status.");

        if (await _repository.NameExistsAsync(name, excludeId))
            return ServiceResult.Failure("A department with this name already exists.");

        return null;
    }
}
