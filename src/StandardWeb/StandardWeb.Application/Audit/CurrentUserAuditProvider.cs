using MiCake.Audit.Core;
using StandardWeb.Domain.Common;

namespace StandardWeb.Application.Audit;

public class CurrentUserAuditProvider(ICurrentUser currentUser) : IAuditProvider
{
    private readonly ICurrentUser _currentUser = currentUser;

    public void ApplyAudit(AuditOperationContext context)
    {
        if (context.EntityState == MiCake.DDD.Infrastructure.RepositoryEntityStates.Added)
        {
            if (context.Entity is IHasCreatedBy hasCreatedBy)
            {
                hasCreatedBy.CreatedBy = _currentUser.GetCurrentUserId();
            }
        }
        else if (context.EntityState == MiCake.DDD.Infrastructure.RepositoryEntityStates.Modified)
        {
            if (context.Entity is IHasModifiedBy hasModifiedBy)
            {
                hasModifiedBy.ModifiedBy = _currentUser.GetCurrentUserId();
            }
        }
    }
}
