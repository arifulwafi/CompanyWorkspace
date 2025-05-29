using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Cyberjuice.Companies;

public class CompanyResolutionMiddleWare(
    IOptions<CompanyResolveOptions> options,
    ILogger<CompanyResolutionMiddleWare> logger,
    ICurrentCompany currentCompany) 
    : IMiddleware, ITransientDependency
{
    private readonly CompanyResolveOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var companyResolveContext = new CompanyResolveContext(context);

        foreach (var companyResolver in _options.CompanyResolvers)
        {
            await companyResolver.ResolveAsync(companyResolveContext);

            if (companyResolveContext.CompanyId.HasValue)
            {
                logger.LogDebug($"Company resolved by {companyResolver.Name}: {companyResolveContext.CompanyId}");
                break;
            }
        }
        if (companyResolveContext.CompanyId.HasValue)
        {
            // Set current Company using scoped ICurrentWorkspace service 
            using (currentCompany.Change(companyResolveContext.CompanyId.Value, companyResolveContext.CompanyName))
            {
                await next(context);
            }
        }
        else
        {
            await next(context);
        }
    }
}

