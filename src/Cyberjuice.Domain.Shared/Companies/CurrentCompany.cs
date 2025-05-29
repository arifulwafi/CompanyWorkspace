using System;
using System.Collections.Concurrent;
using System.Threading;
using Volo.Abp.DependencyInjection;

namespace Cyberjuice.Companies;

public class CurrentCompany : ICurrentCompany, ISingletonDependency
{
    private readonly AsyncLocal<CompanyCacheItem> _currentCompanyCacheItem = new AsyncLocal<CompanyCacheItem>();
    private readonly ConcurrentDictionary<string, CompanyCacheItem> _workspaceConfigurations = new ConcurrentDictionary<string, CompanyCacheItem>();
    /// <summary>
    /// Gets current Company's Id.
    /// </summary>
    public virtual Guid? Id => _currentCompanyCacheItem.Value?.CompanyId;
    /// <summary>
    /// Gets current Company's name.
    /// </summary>
    public virtual string Name => _currentCompanyCacheItem.Value?.Name;
    /// <summary>
    /// Gets a value indicates that current Company is available.
    /// </summary>
    public virtual bool IsAvailable => Id.HasValue;
    /// <summary>
    /// Changes current Company Id.
    /// </summary>
    /// <param name="id">Company Id</param>
    /// <returns>A disposable object to restore Company Id when disposed.</returns>
    public virtual IDisposable Change(Guid? id)
    {
        return Change(id, null);
    }
    /// <summary>
    /// Changes current Company Id and Name.
    /// </summary>
    /// <param name="id">Company Id</param>
    /// <param name="name">Company Name</param>
    /// <returns>A disposable object to restore Company values when disposed.</returns>
    public virtual IDisposable Change(Guid? id, string name)
    {
        var workspaceCacheItem = _currentCompanyCacheItem.Value;
        var previousWorkspaceId = workspaceCacheItem?.CompanyId;
        var previousCopanyName = workspaceCacheItem?.Name;
        if (id == previousWorkspaceId && name == previousCopanyName)
        {
            return NullCompanyRestore.Instance;
        }
        _currentCompanyCacheItem.Value = new CompanyCacheItem(id, name);
        return new CompanyRestore(this, previousWorkspaceId, previousCopanyName);
    }
    private class CompanyCacheItem
    {
        public Guid? CompanyId { get; }
        public string Name { get; }
        public CompanyCacheItem(Guid? companyId, string name = null)
        {
            CompanyId = companyId;
            Name = name;
        }
    }
    private class CompanyRestore : IDisposable
    {
        private readonly CurrentCompany _currentCompany;
        private readonly Guid? _copmanyId;
        private readonly string _workspaceName;
        public CompanyRestore(CurrentCompany currentCompany, Guid? companyId, string workspaceName = null)
        {
            _currentCompany = currentCompany;
            _copmanyId = companyId;
            _workspaceName = workspaceName;
        }
        public void Dispose()
        {
            _currentCompany._currentCompanyCacheItem.Value = new CompanyCacheItem(_copmanyId, _workspaceName);
        }
    }
    private class NullCompanyRestore : IDisposable
    {
        public static readonly NullCompanyRestore Instance = new NullCompanyRestore();
        private NullCompanyRestore()
        {
        }
        public void Dispose()
        {
        }
    }
}
