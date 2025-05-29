using AutoMapper;
using Cyberjuice.Employees;
using Cyberjuice.Employees.Dtos;
using Cyberjuice.Departments;
using Cyberjuice.Departments.Dtos;

namespace Cyberjuice;

public class CyberjuiceApplicationAutoMapperProfile : Profile
{
    public CyberjuiceApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        CreateMap<Employee, EmployeeDto>();
        CreateMap<CreateUpdateEmployeeInput, Employee>();
        
        // Department mappings
        CreateMap<Department, DepartmentDto>();
        CreateMap<CreateDepartmentDto, Department>();
        CreateMap<UpdateDepartmentDto, Department>();
    }
}
