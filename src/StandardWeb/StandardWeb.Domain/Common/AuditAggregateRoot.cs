using System.ComponentModel.DataAnnotations;
using MiCake.Audit;
using MiCake.DDD.Domain;

namespace StandardWeb.Domain.Common;

/// <summary>
/// Base class for aggregate roots with audit tracking (creation and modification times/users)
/// </summary>
public class AuditAggregateRoot : AggregateRoot<long>, IHasCreationTime, IHasModificationTime
{
    /// <summary>
    /// Gets or sets the user who created this entity
    /// </summary>
    [MaxLength(50)]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when this entity was created
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified this entity
    /// </summary>
    [MaxLength(50)]
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this entity was last modified
    /// </summary>
    public DateTime? ModificationTime { get; set; }

}
