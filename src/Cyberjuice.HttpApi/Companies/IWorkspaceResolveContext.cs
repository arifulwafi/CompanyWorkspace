using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cyberjuice.Companies;

public interface IWorkspaceResolveContext
{
    Guid? WorkspaceId { get; set; }

    string WorkspaceName { get; set; }

    IServiceProvider ServiceProvider { get; }

    HttpContext GetHttpContext();
}
