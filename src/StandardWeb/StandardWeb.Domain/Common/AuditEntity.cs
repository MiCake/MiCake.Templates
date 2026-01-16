using System.ComponentModel.DataAnnotations;
using MiCake.Audit;

namespace StandardWeb.Domain.Common;

public class AuditEntity : Entity<long>, IHasCreatedAt, IHasUpdatedAt, IHasCreatedBy, IHasModifiedBy
{
    [MaxLength(50)]
    public long? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    [MaxLength(50)]
    public long? ModifiedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

}
