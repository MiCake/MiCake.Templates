using RBACWeb.Domain.Enums.Identity;

namespace RBACWeb.Web.Dtos.Identity;

// A example to show how to use dynamic query object and paging filter
[DynamicFilterJoin(JoinType = FilterJoinType.And)]
public class QueryUserListDto : IDynamicQueryModel
{
    [DynamicFilter(OperatorType = ValueOperatorType.StartsWith)]
    public string? PhoneNumber { get; set; }

    // if the property name is same as the model property, no need to set PropertyName. 
    // otherwise, need to set it.
    [DynamicFilter(OperatorType = ValueOperatorType.Equal, PropertyName = "Status")]
    public UserStatus UserStatus { get; set; } = UserStatus.Active;
}
