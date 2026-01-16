namespace StandardWeb.Domain.Common;

public interface IHasCreatedBy
{
    long? CreatedBy { get; set; }
}
