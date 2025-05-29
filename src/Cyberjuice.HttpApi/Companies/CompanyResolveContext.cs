using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyberjuice.Companies
{
    public class CompanyResolveContext : ICompanyResolveContext
    {
        public Guid? CompanyId { get; set; }
        public string CompanyName { get; set; }

        public IServiceProvider ServiceProvider => _httpContext.RequestServices;

        private readonly HttpContext _httpContext;
        public CompanyResolveContext(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }
        public HttpContext GetHttpContext()
        {
            return _httpContext;
        }
    }
}
