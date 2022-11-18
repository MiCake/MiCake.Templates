using MiCakeTemplate.Util.Common;

namespace MiCakeTemplate.Api.Constants
{
    /*
     * You can put your business error codes here.
     * 
     */

    internal class UserErrorCodes
    {
        public static readonly ErrorDefinition NOFOUND_USER = new($"001.001", "用户名无效或者密码错误");
    }
}
