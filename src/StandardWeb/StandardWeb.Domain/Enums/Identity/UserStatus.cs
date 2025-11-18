namespace StandardWeb.Domain.Enums.Identity;

public enum UserStatus
{
    /// <summary>
    /// 禁用
    /// </summary>
    Inactive = 0,

    /// <summary>
    /// 正常
    /// </summary>
    Active = 1,

    /// <summary>
    /// 冻结
    /// </summary>
    Frozen = 2
}