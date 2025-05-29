using Cyberjuice.Employees;
using Cyberjuice.Employees.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Cyberjuice.Web.Pages.Employees;

public class EditModalModel : AbpPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateEmployeeInput Employee { get; set; }
    
    public int RemainingLeaveDays { get; set; }

    private readonly IEmployeeAppService _employeeAppService;

    public EditModalModel(IEmployeeAppService employeeAppService)
    {
        _employeeAppService = employeeAppService;
    }

    public async Task OnGetAsync()
    {
        var employee = await _employeeAppService.GetAsync(Id);
        
        Employee = new CreateUpdateEmployeeInput
        {
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            DateOfBirth = employee.DateOfBirth,
            JoiningDate = employee.JoiningDate,
            TotalLeaveDays = employee.TotalLeaveDays
        };
        
        RemainingLeaveDays = employee.RemainingLeaveDays;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _employeeAppService.UpdateAsync(Id, Employee);
        return NoContent();
    }
} 