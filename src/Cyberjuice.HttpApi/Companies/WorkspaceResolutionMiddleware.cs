using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Cyberjuice.Companies;

public class WorkspaceResolutionMiddleware(
    IOptions<CompanyResolveOptions> options,
    ILogger<WorkspaceResolutionMiddleware> logger,
    ICurrentCompany currentWorkspace) 
    : IMiddleware, ITransientDependency
{
    private readonly CompanyResolveOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var workspaceResolveContext = new WorkspaceResolveContext(context);

        foreach (var workspaceResolver in _options.WorkspaceResolvers)
        {
            await workspaceResolver.ResolveAsync(workspaceResolveContext);

            if (workspaceResolveContext.WorkspaceId.HasValue)
            {
                logger.LogDebug($"Company resolved by {workspaceResolver.Name}: {workspaceResolveContext.WorkspaceId}");
                break;
            }
        }
        if (workspaceResolveContext.WorkspaceId.HasValue)
        {
            // Set current Company using scoped ICurrentWorkspace service 
            using (currentWorkspace.Change(workspaceResolveContext.WorkspaceId.Value, workspaceResolveContext.WorkspaceName))
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

