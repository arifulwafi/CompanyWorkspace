using System.Threading.Tasks;
using Cyberjuice.Employees;
using Cyberjuice.Employees.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Cyberjuice.Web.Pages.Employees;

public class CreateModalModel : AbpPageModel
{
    [BindProperty]
    public CreateUpdateEmployeeInput Employee { get; set; }

    private readonly IEmployeeAppService _employeeAppService;

    public CreateModalModel(IEmployeeAppService employeeAppService)
    {
        _employeeAppService = employeeAppService;
    }

    public void OnGet()
    {
        Employee = new CreateUpdateEmployeeInput();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _employeeAppService.CreateAsync(Employee);
        return NoContent();
    }
} 