namespace MiCakeTemplate.Util.Common
{
    /// <summary>
    /// Represent a defined error.
    /// </summary>
    public struct ErrorDefinition
    {
        public string Code { get; set; }
        public string Message { get; set; }

        public ErrorDefinition(string code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
