using System;

namespace Cyberjuice.Companies;

public interface ICurrentCompany
{
    Guid? Id { get; }
    string Name { get; }
    IDisposable Change(Guid? id);
    IDisposable Change(Guid? id, string name);
}
