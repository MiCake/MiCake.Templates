namespace RBACWeb.Domain.Common;

public interface IHasModifiedBy
{
    long? ModifiedBy { get; set; }
}
