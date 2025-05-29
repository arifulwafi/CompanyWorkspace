using Cyberjuice.Employees.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Cyberjuice.Employees;

public interface IEmployeeAppService : IApplicationService
{
    Task<EmployeeDto> GetAsync(Guid id);
    Task<List<EmployeeDto>> GetListAsync();
    Task<EmployeeDto> CreateAsync(CreateUpdateEmployeeInput input);
    Task<EmployeeDto> UpdateAsync(Guid id, CreateUpdateEmployeeInput input);
    Task DeleteAsync(Guid id);
    Task<PagedResultDto<EmployeeDto>> GetPagedListAsync(EmployeeFilter filter);
}
