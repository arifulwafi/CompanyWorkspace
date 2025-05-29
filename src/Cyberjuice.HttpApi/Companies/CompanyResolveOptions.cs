using System.Collections.Generic;

namespace Cyberjuice.Companies;

public class CompanyResolveOptions
{
    public List<ICompanyResolveContributor> WorkspaceResolvers { get; }

    public CompanyResolveOptions()
    {
        WorkspaceResolvers = [];
    }

    public void AddResolver(ICompanyResolveContributor resolver)
    {
        WorkspaceResolvers.Add(resolver);
    }
}
