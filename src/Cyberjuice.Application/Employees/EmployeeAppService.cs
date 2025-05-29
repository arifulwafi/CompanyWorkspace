using Cyberjuice.Employees.Dtos;
using Cyberjuice.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Cyberjuice.Employees;

public class EmployeeAppService(
    IRepository<Employee, Guid> employeeRepository,
    EmployeeManager employeeManager)
    : ApplicationService, IEmployeeAppService
{
    [Authorize(CyberjuicePermissions.Employees.Default)]
    public async Task<EmployeeDto> GetAsync(Guid id)
    {
        var employeeQueryable = (await employeeRepository.GetQueryableAsync()).AsNoTracking();

        var employeeDto = await employeeQueryable
            .Where(e => e.Id == id)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                DateOfBirth = e.DateOfBirth,
                JoiningDate = e.JoiningDate,
                TotalLeaveDays = e.TotalLeaveDays,
                RemainingLeaveDays = e.RemainingLeaveDays,
                CompanyIds = e.Companies.Select(c => c.Id).ToList()
            }).SingleOrDefaultAsync();

        return employeeDto;
    }


    [Authorize(CyberjuicePermissions.Employees.Default)]
    public async Task<List<EmployeeDto>> GetListAsync()
    {
        var employees = await employeeRepository.GetListAsync(includeDetails: true);
        var employeeDtos = ObjectMapper.Map<List<Employee>, List<EmployeeDto>>(employees);

        // Set company IDs from navigation property
        foreach (var dto in employeeDtos)
        {
            var employee = employees.First(e => e.Id == dto.Id);
            dto.CompanyIds = [.. employee.Companies.Select(ce => ce.Id)];
        }

        return employeeDtos;
    }

    [Authorize(CyberjuicePermissions.Employees.Default)]
    public async Task<PagedResultDto<EmployeeDto>> GetPagedListAsync(EmployeeFilter input)
    {
        string sortBy = !string.IsNullOrWhiteSpace(input.Sorting) ? input.Sorting : nameof(Employee.JoiningDate);

        var queryable = (await employeeRepository.GetQueryableAsync())
            .Include(e => e.Companies)
            .AsNoTracking();

        // Apply search filter
        if (!string.IsNullOrEmpty(input.Filter))
        {
            queryable = queryable.Where(e =>
                e.FirstName.Contains(input.Filter) ||
                e.LastName.Contains(input.Filter) ||
                e.Email.Contains(input.Filter) ||
                e.PhoneNumber.Contains(input.Filter));
        }

        var totalCount = await queryable.CountAsync();

        var employees = await queryable
            .OrderBy(sortBy)
            .PageBy(input)
            .ToListAsync();

        var employeeDtos = ObjectMapper.Map<List<Employee>, List<EmployeeDto>>(employees);

        // Set company IDs from navigation property
        foreach (var dto in employeeDtos)
        {
            var employee = employees.First(e => e.Id == dto.Id);
            dto.CompanyIds = employee.Companies.Select(ce => ce.Id).ToList();
        }

        return new PagedResultDto<EmployeeDto>(totalCount, employeeDtos);
    }

    [Authorize(CyberjuicePermissions.Employees.Create)]
    public async Task<bool> CreateAsync(CreateUpdateEmployeeInput input)
    {
        var employee = await employeeManager.CreateAsync(
            input.FirstName,
            input.LastName,
            input.Email,
            input.PhoneNumber,
            input.DateOfBirth,
            input.JoiningDate,
            input.TotalLeaveDays,
            input.CompanyIds
        );

        await employeeRepository.InsertAsync(employee, autoSave: true);

        return true;
    }

    [Authorize(CyberjuicePermissions.Employees.Edit)]
    public async Task<bool> UpdateAsync(Guid id, CreateUpdateEmployeeInput input)
    {
        var employee = await (await employeeRepository.GetQueryableAsync())
                            .Include(e => e.Companies)
                            .SingleOrDefaultAsync(e => e.Id == id);

        await employeeManager.UpdateAsync(
            employee,
            input.FirstName,
            input.LastName,
            input.Email,
            input.PhoneNumber,
            input.DateOfBirth,
            input.JoiningDate,
            input.TotalLeaveDays,
            input.CompanyIds
        );

        return true;
    }

    [Authorize(CyberjuicePermissions.Employees.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        // CompanyEmployee entities will be automatically deleted due to cascade delete
        await employeeRepository.DeleteAsync(id);
    }

    public async Task<bool> GetEmployeeHasAccessToCompanyAsync(Guid? currentCompanyId)
    {
        var employeeId = CurrentUser.Id;

        if (employeeId is null || currentCompanyId is null)
        {
            return false; // No access if context is incomplete
        }

        var employeeQueryable = await employeeRepository.GetQueryableAsync();

        var hasAccess = await employeeQueryable
            .AsNoTracking()
            .Where(e => e.Id == employeeId)
            .SelectMany(e => e.Companies)
            .AnyAsync(c => c.Id == currentCompanyId.Value);

        return hasAccess;
    }
}
