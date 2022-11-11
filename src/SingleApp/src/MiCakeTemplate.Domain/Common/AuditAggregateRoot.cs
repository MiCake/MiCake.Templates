using MiCake.Audit;
using MiCake.DDD.Domain;

/*
 * Tips: This folder is used to store some common base classes
 */

namespace MiCakeTemplate.Domain
{
    public abstract class AuditTimeAggregateRoot : AggregateRoot, IHasCreatedTime, IHasUpdatedTime
    {
        public DateTime? UpdatedTime { get; protected set; }

        public DateTime CreatedTime { get; protected set; }
    }

    public abstract class AuditUserAggregateRoot : AuditTimeAggregateRoot, IHasCreatedUser<int>, IHasUpdatedUser<int>
    {
        public int? CreatedBy { get; protected set; }

        public int? UpdatedBy { get; protected set; }
    }

    public abstract class AuditTimeEntity : Entity, IHasCreatedTime, IHasUpdatedTime
    {
        public DateTime? UpdatedTime { get; protected set; }

        public DateTime CreatedTime { get; protected set; }
    }

    public abstract class AuditUserEntity : AuditTimeEntity, IHasCreatedUser<int>, IHasUpdatedUser<int>
    {
        public int? CreatedBy { get; protected set; }

        public int? UpdatedBy { get; protected set; }
    }
}
