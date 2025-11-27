using MiCake.Util.Query.Dynamic;
using StandardWeb.Domain.Enums.Identity;

namespace StandardWeb.Web.Dtos.Identity;

// A example to show how to use dynamic query object and paging filter
[DynamicFilterJoin(JoinType = FilterJoinType.And)]
public class QueryUserListDto : IDynamicQueryObj
{
    [DynamicFilter(OperatorType = ValueOperatorType.StartsWith)]
    public string? PhoneNumber { get; set; }

    // if the property name is same as the model property, no need to set PropertyName. 
    // otherwise, need to set it.
    [DynamicFilter(OperatorType = ValueOperatorType.Equal, PropertyName = "Status")]
    public UserStatus UserStatus { get; set; } = UserStatus.Active;
}
