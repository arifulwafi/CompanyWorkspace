using Cyberjuice.Companies;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using Volo.Abp;

namespace Cyberjuice.Employees;

public class Employee : FullAuditedAggregateRoot<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime JoiningDate { get; set; }
    public int TotalLeaveDays { get; set; }
    public int RemainingLeaveDays { get; set; }

    // Navigation property for many-to-many relationship
    public virtual ICollection<CompanyEmployee> CompanyEmployees { get; set; } = new List<CompanyEmployee>();

    protected Employee()
    {
        // Required by EF Core
    }

    public Employee(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        DateTime dateOfBirth,
        DateTime joiningDate,
        int totalLeaveDays
    ) : base(id)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetEmail(email);
        SetPhoneNumber(phoneNumber);
        DateOfBirth = dateOfBirth;
        JoiningDate = joiningDate;
        SetTotalLeaveDays(totalLeaveDays);
        RemainingLeaveDays = totalLeaveDays;
        CompanyEmployees = new List<CompanyEmployee>();
    }

    public Employee SetFirstName(string firstName)
    {
        FirstName = Check.NotNullOrWhiteSpace(firstName, nameof(firstName), EmployeeConsts.MaxFirstNameLength);
        return this;
    }

    public Employee SetLastName(string lastName)
    {
        LastName = Check.NotNullOrWhiteSpace(lastName, nameof(lastName), EmployeeConsts.MaxLastNameLength);
        return this;
    }

    public Employee SetEmail(string email)
    {
        Email = Check.NotNullOrWhiteSpace(email, nameof(email), EmployeeConsts.MaxEmailLength);
        return this;
    }

    public Employee SetPhoneNumber(string phoneNumber)
    {
        PhoneNumber = Check.NotNullOrWhiteSpace(phoneNumber, nameof(phoneNumber), EmployeeConsts.MaxPhoneNumberLength);
        return this;
    }

    public Employee SetTotalLeaveDays(int totalLeaveDays)
    {
        if (totalLeaveDays < 0)
        {
            throw new ArgumentException("Total leave days cannot be negative.", nameof(totalLeaveDays));
        }
        TotalLeaveDays = totalLeaveDays;
        return this;
    }

    public void UpdateRemainingLeaveDays(int daysUsed)
    {
        if (daysUsed > RemainingLeaveDays)
        {
            throw new BusinessException(CyberjuiceDomainErrorCodes.NotEnoughLeaveDays);
        }
        RemainingLeaveDays -= daysUsed;
    }

    public void AddToCompany(Guid companyId)
    {
        if (CompanyEmployees.Any(ce => ce.CompanyId == companyId))
        {
            return; // Already exists
        }

        CompanyEmployees.Add(new CompanyEmployee(Id, companyId));
    }

    public void RemoveFromCompany(Guid companyId)
    {
        var companyEmployee = CompanyEmployees.FirstOrDefault(ce => ce.CompanyId == companyId);
        if (companyEmployee != null)
        {
            CompanyEmployees.Remove(companyEmployee);
        }
    }

    public void UpdateCompanies(IEnumerable<Guid> companyIds)
    {
        // Clear existing company assignments
        CompanyEmployees.Clear();
        
        // Add new company assignments
        foreach (var companyId in companyIds)
        {
            CompanyEmployees.Add(new CompanyEmployee(Id, companyId));
        }
    }

    public IEnumerable<Guid> GetCompanyIds()
    {
        return CompanyEmployees.Select(ce => ce.CompanyId);
    }
}
