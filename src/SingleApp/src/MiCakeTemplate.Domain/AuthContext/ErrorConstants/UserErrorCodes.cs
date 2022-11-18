using MiCakeTemplate.Util.Common;

namespace MiCakeTemplate.Domain.AuthContext.ErrorConstants
{
    /*
     * You can put your domain error codes here. 
     * tips: these error codes only for current domain use. so you should set this class internal to avoid other projects using it.
     */
    internal static class UserErrorCodes
    {
        public static readonly ErrorDefinition USER_NAME_EMPTY = new("01.001", "用户名为空");
        public static readonly ErrorDefinition USER_NAME_EXIST = new("01.002", "用户名已存在");
        public static readonly ErrorDefinition USER_NAME_INVALID = new("01.003", "用户名无效");
        public static readonly ErrorDefinition USER_NAME_LENGTH_ERROR = new("01.004", "用户名长度错误");
        public static readonly ErrorDefinition USER_NAME_SPECIAL_LETTER = new("01.005", "用户名包含特殊字符");
        public static readonly ErrorDefinition USER_PHONE_EXIST = new("01.008", "手机号已存在");
        public static readonly ErrorDefinition PASSWORD_LENGTH_ERROR = new("01.009", "密码长度错误");
        public static readonly ErrorDefinition EMAIL_EXIST = new("01.010", "邮箱已经存在");
        public static readonly ErrorDefinition RESET_PASSWORD_TOKEN_INVALID = new("01.011", "重置密码token无效");
        public static readonly ErrorDefinition EMAIL_NOTEXIST = new("01.012", "邮箱不存在");
        public static readonly ErrorDefinition USER_PHONE_NOTEXIST = new("01.013", "手机号不存在");

        public static readonly ErrorDefinition USER_NOT_EXIST = new("01.101", "用户不存在");
        public static readonly ErrorDefinition PASSWORD_ERROR = new("01.102", "密码错误");
        public static readonly ErrorDefinition USER_LOCKED = new("01.103", "用户已锁定");
        public static readonly ErrorDefinition USER_DISABLED = new("01.104", "用户被禁用");
    }
}
