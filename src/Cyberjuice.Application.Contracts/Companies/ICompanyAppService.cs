using Cyberjuice.Companies.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Cyberjuice.Companies;

public interface ICompanyAppService : IApplicationService
{
    Task<CompanyDto> CreateAsync(string name);
    Task<CompanyDto> UpdateAsync(Guid id, string name);
    Task<CompanyDto> GetAsync(Guid id);
    Task<PagedResultDto<CompanyDto>> GetAllAsync(PagedAndSortedResultRequestDto filter);
    Task DeleteAsync(Guid id);
}
