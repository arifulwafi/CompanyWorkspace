using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cyberjuice.Employees;
using Cyberjuice.Employees.Dtos;
using Cyberjuice.Companies;
using Cyberjuice.Companies.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Cyberjuice.Web.Pages.Employees;

public class CreateModalModel : AbpPageModel
{
    [BindProperty]
    public CreateUpdateEmployeeInput Employee { get; set; }

    public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();

    private readonly IEmployeeAppService _employeeAppService;
    private readonly ICompanyAppService _companyAppService;

    public CreateModalModel(IEmployeeAppService employeeAppService, ICompanyAppService companyAppService)
    {
        _employeeAppService = employeeAppService;
        _companyAppService = companyAppService;
    }

    public async Task OnGetAsync()
    {
        Employee = new CreateUpdateEmployeeInput();
        await LoadCompaniesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _employeeAppService.CreateAsync(Employee);
        return NoContent();
    }

    private async Task LoadCompaniesAsync()
    {
        var companies = await _companyAppService.GetAllAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 1000 });
        
        Companies = companies.Items.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        }).ToList();
    }
} 