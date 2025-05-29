using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using System;
using System.Linq;
using Cyberjuice.Departments.Dtos;
using Cyberjuice.Companies;

namespace Cyberjuice.Departments
{
    public class DepartmentAppService : ApplicationService, IDepartmentAppService
    {
        private readonly IRepository<Department, Guid> _departmentRepository;
        private readonly DepartmentManager _departmentManager;
        private readonly ICurrentCompany _currentCompany;

        public DepartmentAppService(
            IRepository<Department, Guid> departmentRepository,
            DepartmentManager departmentManager,
            ICurrentCompany currentCompany)
        {
            _departmentRepository = departmentRepository;
            _departmentManager = departmentManager;
            _currentCompany = currentCompany;
        }

        public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto input)
        {
            var department = await _departmentManager.CreateAsync(
                input.Name,
                input.Description,
                input.EmployeeCount,
                _currentCompany.Id
            );

            var createdDepartment = await _departmentRepository.InsertAsync(department);

            return ObjectMapper.Map<Department, DepartmentDto>(createdDepartment);
        }

        public async Task<DepartmentDto> UpdateAsync(Guid id, UpdateDepartmentDto input)
        {
            var department = await _departmentRepository.GetAsync(id);

            await _departmentManager.UpdateAsync(
                department,
                input.Name,
                input.Description,
                input.EmployeeCount
            );

            var updatedDepartment = await _departmentRepository.UpdateAsync(department);

            return ObjectMapper.Map<Department, DepartmentDto>(updatedDepartment);
        }

        public async Task<DepartmentDto> GetAsync(Guid id)
        {
            var department = await _departmentRepository.GetAsync(id);
            return ObjectMapper.Map<Department, DepartmentDto>(department);
        }

        public async Task<PagedResultDto<DepartmentDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            string sortBy = !string.IsNullOrWhiteSpace(input.Sorting) ? input.Sorting : nameof(Department.CreationTime);

            var queryable = (await _departmentRepository.GetQueryableAsync())
                .AsNoTracking()
                .Where(d => d.CompanyId == _currentCompany.Id);

            var totalCount = await queryable.CountAsync();

            var query = from d in queryable
                        select new DepartmentDto
                        {
                            Id = d.Id,
                            CompanyId = d.CompanyId,
                            Name = d.Name,
                            Description = d.Description,
                            EmployeeCount = d.EmployeeCount,
                            CreationTime = d.CreationTime,
                            CreatorId = d.CreatorId,
                            LastModificationTime = d.LastModificationTime,
                            LastModifierId = d.LastModifierId,
                            IsDeleted = d.IsDeleted,
                            DeleterId = d.DeleterId,
                            DeletionTime = d.DeletionTime
                        };

            var result = await query
                .OrderBy(sortBy)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<DepartmentDto>(totalCount, result);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _departmentRepository.DeleteAsync(id);
        }
    }
} 