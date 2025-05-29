using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Volo.Abp.EntityFrameworkCore;

namespace Cyberjuice.Companies;

public abstract class CompanyDbContextBase<TSelf>
: AbpDbContext<TSelf>
where TSelf : DbContext
{
    protected CompanyDbContextBase(DbContextOptions<TSelf> options)
    : base(options)
    {
    }

    protected ICurrentCompany CurrentWorkspace =>
    LazyServiceProvider.LazyGetRequiredService<ICurrentCompany>();

    protected ICompanyFilter MultiWorkspaceFilter =>
        LazyServiceProvider.LazyGetRequiredService<ICompanyFilter>();

    protected bool IsMultiWorkspaceFilterEnabled =>
        DataFilter?.IsEnabled<ICompany>() ?? false;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyCurrentWorkspaceId();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyCurrentWorkspaceId();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyCurrentWorkspaceId()
    {
        if (CurrentWorkspace?.Id == null) return;

        var currentWorkspaceId = CurrentWorkspace.Id.Value;

        foreach (var entry in ChangeTracker.Entries()
            .Where(e =>
                e.Entity is ICompany &&
                (e.State == EntityState.Added || e.State == EntityState.Modified)))
        {
            // Stamp the FK column via EF Core API
            entry.Property(nameof(ICompany.CompanyId)).CurrentValue = currentWorkspaceId;

            if (entry.State == EntityState.Modified)
            {
                // Prevent accidental overwrites
                entry.Property(nameof(ICompany.CompanyId)).IsModified = false;
            }
        }
    }

    protected override bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType)
    {
        if (typeof(ICompany).IsAssignableFrom(typeof(TEntity)))
        {
            return true;
        }

        return base.ShouldFilterEntity<TEntity>(entityType);
    }

    protected override Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>(ModelBuilder modelBuilder)
    {
        var baseExpression = base.CreateFilterExpression<TEntity>(modelBuilder);

        if (!typeof(ICompany).IsAssignableFrom(typeof(TEntity)))
        {
            return baseExpression;
        }

        var prop = modelBuilder
        .Entity<TEntity>()
            .Metadata
            .FindProperty(nameof(ICompany.CompanyId))!;

        var columnName = prop.GetColumnName() ?? prop.Name;

        Expression<Func<TEntity, bool>> workspaceFilter = e =>
            !IsMultiWorkspaceFilterEnabled
            || CurrentWorkspace.Id == null
            || EF.Property<Guid?>(e, columnName) == CurrentWorkspace.Id;

        if (baseExpression == null) return workspaceFilter;

        return QueryFilterExpressionHelper.CombineExpressions(baseExpression, workspaceFilter);
    }
}
