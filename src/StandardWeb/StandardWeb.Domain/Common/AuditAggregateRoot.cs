using System.ComponentModel.DataAnnotations;
using MiCake.Audit;

namespace StandardWeb.Domain.Common;

/// <summary>
/// Base class for aggregate roots with audit tracking capabilities.
/// Automatically tracks creation and modification times and actors.
/// Inherits from MiCake AggregateRoot with long ID type.
/// </summary>
public class AuditAggregateRoot : AggregateRoot<long>, IHasAuditTimestamps<DateTimeOffset>, IHasCreatedBy, IHasModifiedBy
{
    /// <summary>
    /// Identifier of the user or system that created this entity.
    /// </summary>
    [MaxLength(50)]
    public long? CreatedBy { get; set; }

    /// <summary>
    /// Timestamp when this entity was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Identifier of the user or system that last modified this entity.
    /// </summary>
    [MaxLength(50)]
    public long? ModifiedBy { get; set; }

    /// <summary>
    /// Timestamp of the last modification to this entity.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
