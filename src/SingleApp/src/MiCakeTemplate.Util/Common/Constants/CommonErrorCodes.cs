namespace MiCakeTemplate.Util.Common.Constants
{
    /*
     * You can put your common error codes here. all project can use these error codes.
     */
    public static class CommonErrorCodes
    {
        public static readonly ErrorDefinition PHONE_FORMAT_ERROR = new("901", "手机号错误");
        public static readonly ErrorDefinition EMAIL_FORMAT_ERROR = new("902", "邮箱错误");

        public static readonly ErrorDefinition INPUT_TEXT_ERROR = new("990", "输入错误");
        public static readonly ErrorDefinition System_ERROR = new("999", "系统错误，请稍后再试");
    }
}
