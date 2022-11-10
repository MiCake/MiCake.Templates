using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiCakeTemplate.Domain.AuthContext.ErrorConstants
{
    internal static class UserErrorCodes
    {
        public static readonly ErrorDefinition USER_NAME_EMPTY = new("001", "用户名为空");
        public static readonly ErrorDefinition USER_NAME_EXIST = new("002", "用户名已存在");
        public static readonly ErrorDefinition USER_NAME_INVALID = new("003", "用户名无效");
        public static readonly ErrorDefinition USER_NAME_LENGTH_ERROR = new("004", "用户名长度错误");
        public static readonly ErrorDefinition USER_NAME_SPECIAL_LETTER = new("005", "用户名包含特殊字符");
        public static readonly ErrorDefinition USER_PHONE_EXIST = new("008", "手机号已存在");
        public static readonly ErrorDefinition PASSWORD_LENGTH_ERROR = new("009", "密码长度错误");
        public static readonly ErrorDefinition EMAIL_EXIST = new("010", "邮箱已经存在");
        public static readonly ErrorDefinition RESET_PASSWORD_TOKEN_INVALID = new("011", "重置密码token无效");
        public static readonly ErrorDefinition EMAIL_NOTEXIST = new("012", "邮箱不存在");
        public static readonly ErrorDefinition USER_PHONE_NOTEXIST = new("013", "手机号不存在");

        public static readonly ErrorDefinition USER_NOT_EXIST = new("101", "用户不存在");
        public static readonly ErrorDefinition PASSWORD_ERROR = new("102", "密码错误");
        public static readonly ErrorDefinition USER_LOCKED = new("103", "用户已锁定");
        public static readonly ErrorDefinition USER_DISABLED = new("104", "用户被禁用");
    }
}
