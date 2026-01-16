using System.ComponentModel.DataAnnotations;
using MiCake.Audit;

namespace StandardWeb.Domain.Common;

public class AuditEntity : Entity<long>, IHasCreatedAt, IHasUpdatedAt
{
    [MaxLength(50)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    [MaxLength(50)]
    public string? ModifiedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

}
