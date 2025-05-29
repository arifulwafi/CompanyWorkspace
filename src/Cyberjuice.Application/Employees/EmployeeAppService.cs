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

public class EmployeeAppService : ApplicationService, IEmployeeAppService
{
    private readonly IRepository<Employee, Guid> _employeeRepository;
    private readonly EmployeeManager _employeeManager;

    public EmployeeAppService(
        IRepository<Employee, Guid> employeeRepository,
        EmployeeManager employeeManager)
    {
        _employeeRepository = employeeRepository;
        _employeeManager = employeeManager;
    }

    [Authorize(CyberjuicePermissions.Employees.Default)]
    public async Task<EmployeeDto> GetAsync(Guid id)
    {
        var employee = await _employeeRepository.GetAsync(id, includeDetails: true);
        var employeeDto = ObjectMapper.Map<Employee, EmployeeDto>(employee);

        // Get company IDs from navigation property
        employeeDto.CompanyIds = employee.CompanyEmployees.Select(ce => ce.CompanyId).ToList();

        return employeeDto;
    }

    [Authorize(CyberjuicePermissions.Employees.Default)]
    public async Task<List<EmployeeDto>> GetListAsync()
    {
        var employees = await _employeeRepository.GetListAsync(includeDetails: true);
        var employeeDtos = ObjectMapper.Map<List<Employee>, List<EmployeeDto>>(employees);

        // Set company IDs from navigation property
        foreach (var dto in employeeDtos)
        {
            var employee = employees.First(e => e.Id == dto.Id);
            dto.CompanyIds = [.. employee.CompanyEmployees.Select(ce => ce.CompanyId)];
        }

        return employeeDtos;
    }

    [Authorize(CyberjuicePermissions.Employees.Default)]
    public async Task<PagedResultDto<EmployeeDto>> GetPagedListAsync(EmployeeFilter input)
    {
        string sortBy = !string.IsNullOrWhiteSpace(input.Sorting) ? input.Sorting : nameof(Employee.JoiningDate);

        var queryable = (await _employeeRepository.GetQueryableAsync())
            .Include(e => e.CompanyEmployees)
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
            dto.CompanyIds = employee.CompanyEmployees.Select(ce => ce.CompanyId).ToList();
        }

        return new PagedResultDto<EmployeeDto>(totalCount, employeeDtos);
    }

    [Authorize(CyberjuicePermissions.Employees.Create)]
    public async Task<EmployeeDto> CreateAsync(CreateUpdateEmployeeInput input)
    {
        var employee = await _employeeManager.CreateAsync(
            input.FirstName,
            input.LastName,
            input.Email,
            input.PhoneNumber,
            input.DateOfBirth,
            input.JoiningDate,
            input.TotalLeaveDays,
            input.CompanyIds
        );

        var createdEmployee = await _employeeRepository.InsertAsync(employee, autoSave: true);

        var employeeDto = ObjectMapper.Map<Employee, EmployeeDto>(createdEmployee);
        employeeDto.CompanyIds = input.CompanyIds.ToList();

        return employeeDto;
    }

    [Authorize(CyberjuicePermissions.Employees.Edit)]
    public async Task<EmployeeDto> UpdateAsync(Guid id, CreateUpdateEmployeeInput input)
    {
        var employee = await _employeeRepository.GetAsync(id, includeDetails: true);

        await _employeeManager.UpdateAsync(
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

        var updatedEmployee = await _employeeRepository.UpdateAsync(employee, autoSave: true);

        var employeeDto = ObjectMapper.Map<Employee, EmployeeDto>(updatedEmployee);
        employeeDto.CompanyIds = input.CompanyIds.ToList();

        return employeeDto;
    }

    [Authorize(CyberjuicePermissions.Employees.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        // CompanyEmployee entities will be automatically deleted due to cascade delete
        await _employeeRepository.DeleteAsync(id);
    }
}
