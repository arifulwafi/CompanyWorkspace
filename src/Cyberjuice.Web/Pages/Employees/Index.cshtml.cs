using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Cyberjuice.Web.Pages.Employees;


public class IndexModel : AbpPageModel
{
    public async Task OnGetAsync()
    {
        await Task.CompletedTask;
    }
}
