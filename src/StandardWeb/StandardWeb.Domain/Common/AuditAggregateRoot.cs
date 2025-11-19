using System.ComponentModel.DataAnnotations;
using MiCake.Audit;
using MiCake.DDD.Domain;

namespace StandardWeb.Domain.Common;

/// <summary>
/// Base class for aggregate roots with audit tracking capabilities.
/// Automatically tracks creation and modification times and actors.
/// Inherits from MiCake AggregateRoot with long ID type.
/// </summary>
public class AuditAggregateRoot : AggregateRoot<long>, IHasCreationTime, IHasModificationTime
{
    /// <summary>
    /// Identifier of the user or system that created this entity.
    /// </summary>
    [MaxLength(50)]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when this entity was created.
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// Identifier of the user or system that last modified this entity.
    /// </summary>
    [MaxLength(50)]
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Timestamp of the last modification to this entity.
    /// </summary>
    public DateTime? ModificationTime { get; set; }
}
