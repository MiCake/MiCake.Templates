using System.ComponentModel.DataAnnotations;
using MiCake.Audit;

namespace StandardWeb.Domain.Common;

public class AuditEntity : Entity<long>, IHasAuditTimestamps<DateTimeOffset>, IHasCreatedBy, IHasModifiedBy
{
    [MaxLength(50)]
    public long? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    [MaxLength(50)]
    public long? ModifiedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

}
