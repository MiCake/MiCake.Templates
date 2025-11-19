namespace StandardWeb.Common.Auth
{
    /// <summary>
    /// Defines standard JWT claim type names used throughout the application.
    /// Ensures consistent claim naming across token generation and validation.
    /// </summary>
    public class JwtClaimTypes
    {
        /// <summary>
        /// Claim type for user ID (primary key).
        /// </summary>
        public const string UserId = "userid";

        /// <summary>
        /// Claim type for user's phone number.
        /// </summary>
        public const string PhoneNumber = "phonenumber";
    }
}