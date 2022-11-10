using MiCakeTemplate.Domain.AuthContext.Enums;

namespace MiCakeTemplate.Domain.AuthContext
{
    public class UserIdentification : AuditTimeEntity
    {
        public UserIdentificationType Type { get; protected set; }

        public string? Value { get; protected set; }

        public bool IsVerified { get; protected set; }

        public DateTime? VerifiedTime { get; protected set; }

        public bool IsPrimary { get; protected set; }

        public static UserIdentification Create(UserIdentificationType type, string value)
        {
            return new UserIdentification()
            {
                Type = type,
                Value = value
            };
        }

        public void UpdateValue(string value)
        {
            Value = value;
        }

        public void SetPrimary()
        {
            IsPrimary = true;
        }

        public void CancelPrimary()
        {
            IsPrimary = false;
        }

        public void SetVerified()
        {
            IsVerified = true;
            VerifiedTime = DateTime.UtcNow;
        }
    }
}
