using Cyberjuice.Companies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyberjuice.Employees
{
    public class CompanyEmployee
    {
        public Guid EmployeeId { get; set; }
        public Guid CompanyId { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Company Company { get; set; }
    }
}
