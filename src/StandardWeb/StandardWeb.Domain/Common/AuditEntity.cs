using System.ComponentModel.DataAnnotations;
using MiCake.Audit;
using MiCake.DDD.Domain;

namespace StandardWeb.Domain.Common;

public class AuditEntity : Entity<long>, IHasCreationTime, IHasModificationTime
{
    [MaxLength(50)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    [MaxLength(50)]
    public string? ModifiedBy { get; set; }

    public DateTime? ModificationTime { get; set; }

}
