using System;
using System.Collections.Concurrent;
using System.Threading;
using Volo.Abp.DependencyInjection;

namespace Cyberjuice.Companies;

public class CurrentCompany : ICurrentCompany, ISingletonDependency
{
    private readonly AsyncLocal<WorkspaceCacheItem> _currentWorkspaceCacheItem = new AsyncLocal<WorkspaceCacheItem>();
    private readonly ConcurrentDictionary<string, WorkspaceCacheItem> _workspaceConfigurations = new ConcurrentDictionary<string, WorkspaceCacheItem>();
    /// <summary>
    /// Gets current Company's Id.
    /// </summary>
    public virtual Guid? Id => _currentWorkspaceCacheItem.Value?.WorkspaceId;
    /// <summary>
    /// Gets current Company's name.
    /// </summary>
    public virtual string Name => _currentWorkspaceCacheItem.Value?.Name;
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
        var workspaceCacheItem = _currentWorkspaceCacheItem.Value;
        var previousWorkspaceId = workspaceCacheItem?.WorkspaceId;
        var previousWorkspaceName = workspaceCacheItem?.Name;
        if (id == previousWorkspaceId && name == previousWorkspaceName)
        {
            return NullWorkspaceRestore.Instance;
        }
        _currentWorkspaceCacheItem.Value = new WorkspaceCacheItem(id, name);
        return new WorkspaceRestore(this, previousWorkspaceId, previousWorkspaceName);
    }
    private class WorkspaceCacheItem
    {
        public Guid? WorkspaceId { get; }
        public string Name { get; }
        public WorkspaceCacheItem(Guid? workspaceId, string name = null)
        {
            WorkspaceId = workspaceId;
            Name = name;
        }
    }
    private class WorkspaceRestore : IDisposable
    {
        private readonly CurrentCompany _currentWorkspace;
        private readonly Guid? _workspaceId;
        private readonly string _workspaceName;
        public WorkspaceRestore(CurrentCompany currentWorkspace, Guid? workspaceId, string workspaceName = null)
        {
            _currentWorkspace = currentWorkspace;
            _workspaceId = workspaceId;
            _workspaceName = workspaceName;
        }
        public void Dispose()
        {
            _currentWorkspace._currentWorkspaceCacheItem.Value = new WorkspaceCacheItem(_workspaceId, _workspaceName);
        }
    }
    private class NullWorkspaceRestore : IDisposable
    {
        public static readonly NullWorkspaceRestore Instance = new NullWorkspaceRestore();
        private NullWorkspaceRestore()
        {
        }
        public void Dispose()
        {
        }
    }
}
