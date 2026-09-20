using rsit.ViewModels.Admin;

namespace rsit.Services.Interfaces;

public interface IDepartmentService
{
    Task<DepartmentListViewModel> GetListAsync(DepartmentListViewModel query);

    Task<DepartmentFormViewModel?> GetForEditAsync(int departmentId);

    Task<ServiceResult> CreateAsync(DepartmentFormViewModel model);

    Task<ServiceResult> UpdateAsync(DepartmentFormViewModel model);

    Task<ServiceResult> SetStatusAsync(int departmentId, bool activate);
}
