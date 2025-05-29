using Cyberjuice.Companies;
using System;
using Volo.Abp.Domain.Entities;

namespace Cyberjuice.Employees
{
    public class CompanyEmployee : Entity
    {
        public Guid EmployeeId { get; set; }
        public Guid CompanyId { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Company Company { get; set; }

        protected CompanyEmployee()
        {
            // Required by EF Core
        }

        public CompanyEmployee(Guid employeeId, Guid companyId)
        {
            EmployeeId = employeeId;
            CompanyId = companyId;
        }

        public override object[] GetKeys()
        {
            return new object[] { EmployeeId, CompanyId };
        }
    }
}
