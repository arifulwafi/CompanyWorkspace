using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Cyberjuice.Companies;

/// <summary>
/// Resolves Company from the X-Company-Id header in HTTP requests
/// </summary>
public class CompanyIdHeaderResolveContributor : ICompanyResolveContributor, ITransientDependency
{
    /// <summary>
    /// Default header name: X-Company-ID.
    /// </summary>
    public const string HeaderName = "X-Company-Id";

    /// <summary>
    /// Default contributor name: WorkspaceIdHeader.
    /// </summary>
    public const string ContributorName = "WorkspaceIdHeader";
    /// <summary>
    /// Name of the contributor.
    /// </summary>
    public string Name => ContributorName;
    private readonly ILogger<CompanyIdHeaderResolveContributor> _logger;
    public CompanyIdHeaderResolveContributor(ILogger<CompanyIdHeaderResolveContributor> logger)
    {
        _logger = logger;
    }
    /// <summary>
    /// Tries to resolve current Company from HTTP header.
    /// </summary>
    public Task ResolveAsync(IWorkspaceResolveContext context)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext == null)
        {
            return Task.CompletedTask;
        }
        var WorkspaceIdHeader = httpContext.Request.Headers[HeaderName];
        if (WorkspaceIdHeader.Count == 0 || string.IsNullOrWhiteSpace(WorkspaceIdHeader[0]))
        {
            return Task.CompletedTask;
        }
        if (Guid.TryParse(WorkspaceIdHeader[0], out var workspaceId))
        {
            _logger.LogDebug($"Company Id found in request header: {workspaceId}");
            context.WorkspaceId = workspaceId;
        }
        else
        {
            _logger.LogDebug($"Invalid Company Id format in request header: {WorkspaceIdHeader[0]}");
        }
        return Task.CompletedTask;
    }
}
