using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiCakeTemplate.Domain.AuthContext
{
    public class UserSecurityToken : AuditTimeEntity
    {
        public string? ResetPasswordToken { get; protected set; }

        public DateTime? ResetPasswordTokenCreatedTime { get; protected set; }

        public string? AccountCreationToken { get; protected set; }

        public DateTime? AccountCreationTokenCreatedTime { get; protected set; }

        public static UserSecurityToken Create()
        {
            return new UserSecurityToken();
        }

        public string GenerateResetPasswordToken()
        {
            var token = Guid.NewGuid().ToString();

            ResetPasswordToken = token;
            ResetPasswordTokenCreatedTime = DateTime.UtcNow;

            return token;
        }

        public string GenerateAccountCreationToken()
        {
            var token = Guid.NewGuid().ToString();

            AccountCreationToken = token;
            AccountCreationTokenCreatedTime = DateTime.UtcNow;

            return token;
        }

        internal bool IsValidResetPasswordToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }
            if (ResetPasswordToken != token)
            {
                return false;
            }
            if (ResetPasswordTokenCreatedTime == null)
            {
                return false;
            }
            if (ResetPasswordTokenCreatedTime.Value.AddHours(1) < DateTime.UtcNow)
            {
                return false;
            }
            return true;
        }

        internal void InvalidateResetPasswordToken()
        {
            ResetPasswordToken = string.Empty;
            ResetPasswordTokenCreatedTime = null;
        }
    }
}
