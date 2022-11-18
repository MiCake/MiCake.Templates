using MiCakeTemplate.Domain.AuthContext.Enums;
using MiCakeTemplate.Domain.AuthContext.ErrorConstants;
using MiCakeTemplate.Domain.Constants;
using MiCakeTemplate.Util.Common;
using MiCakeTemplate.Util.Cryptography;
using MiCakeTemplate.Util.Validation;

namespace MiCakeTemplate.Domain.AuthContext
{
    public class User : AppUser<int>
    {
        public string? UserName { get; protected set; }

        public string? Password { get; protected set; }

        public bool IsLocked { get; protected set; }

        public DateTime? LockedDateTime { get; private set; }

        public int LoginAttempt { get; private set; }

        public DateTime? LastLoginTime { get; protected set; }

        public UserSecurityToken? UserToken { get; protected set; }

        public List<UserIdentification> UserIdentifications { get; protected set; } = new();

        public static User Create(string userName, string password)
        {
            CheckUserNameFormat(userName);
            CheckPasswordFormat(password);

            return new User()
            {
                UserName = userName,
                Password = EncryptPwd(password),
            };
        }

        public static User CreateByPhone(string phone, string password, bool isVerified = false)
        {
            if (!ValidationHelper.IsPhoneNumber(phone))
            {
                throw DomainException.Create(CommonErrorCodes.PHONE_FORMAT_ERROR);
            }
            CheckPasswordFormat(password);

            var user = new User()
            {
                Password = EncryptPwd(password),
            };

            var phoneRecord = UserIdentification.Create(Enums.UserIdentificationType.Phone, phone);
            if (isVerified)
            {
                phoneRecord.SetVerified();
            }
            user.UserIdentifications.Add(phoneRecord);

            return user;
        }

        public static User CreateByEmail(string email, string password, bool isVerified = false)
        {
            if (!ValidationHelper.IsEmail(email))
            {
                throw DomainException.Create(CommonErrorCodes.EMAIL_FORMAT_ERROR);
            }
            CheckPasswordFormat(password);

            var user = new User()
            {
                Password = EncryptPwd(password),
            };

            var emailRecord = UserIdentification.Create(Enums.UserIdentificationType.Email, email);
            if (isVerified)
            {
                emailRecord.SetVerified();
            }
            user.UserIdentifications.Add(emailRecord);

            return user;
        }

        public void UpdateUserName(string userName, bool force = false)
        {
            if (!force && !string.IsNullOrWhiteSpace(userName))
            {
                throw DomainException.Create(UserErrorCodes.USER_NAME_EXIST);
            }
            UserName = userName;
        }

        public void UpdateEmail(string email, bool existThenThrow = true)
        {
            if (!ValidationHelper.IsEmail(email))
            {
                throw DomainException.Create(CommonErrorCodes.EMAIL_FORMAT_ERROR);
            }

            var currentEmail = UserIdentifications.FirstOrDefault(s => s.Type == Enums.UserIdentificationType.Email);
            if (currentEmail is null)
            {
                var emailId = UserIdentification.Create(Enums.UserIdentificationType.Email, email);
                emailId.SetVerified();
                UserIdentifications.Add(emailId);
            }
            else
            {
                if (existThenThrow)
                {
                    throw DomainException.Create(UserErrorCodes.EMAIL_EXIST);
                }

                currentEmail.UpdateValue(email);
                currentEmail.SetVerified();
            }
        }

        public void UpdatePhone(string phone, bool existThenThrow = true)
        {
            if (!ValidationHelper.IsPhoneNumber(phone))
            {
                throw DomainException.Create(CommonErrorCodes.PHONE_FORMAT_ERROR);
            }

            var currentPhone = UserIdentifications.FirstOrDefault(s => s.Type == Enums.UserIdentificationType.Phone);
            if (currentPhone is null)
            {
                var phoneRecord = UserIdentification.Create(Enums.UserIdentificationType.Phone, phone);
                phoneRecord.SetVerified();
                UserIdentifications.Add(phoneRecord);
            }
            else
            {
                if (existThenThrow)
                {
                    throw DomainException.Create(UserErrorCodes.USER_PHONE_EXIST);
                }

                currentPhone.UpdateValue(phone);
                currentPhone.SetVerified();
            }
        }

        public void ChangePassword(string password)
        {
            CheckPasswordFormat(password);
            Password = EncryptPwd(password);

            LoginAttempt = 0;
            IsLocked = false;
        }

        public string CreateForgotPasswordToken()
        {
            UserToken ??= new();
            return UserToken.GenerateResetPasswordToken();
        }

        public void ResetPasswordByToken(string token, string password)
        {
            if (UserToken is null || !UserToken.IsValidResetPasswordToken(token))
            {
                throw DomainException.Create(UserErrorCodes.RESET_PASSWORD_TOKEN_INVALID);
            }
            ChangePassword(password);
            UserToken.InvalidateResetPasswordToken();
        }

        public void Lock()
        {
            IsLocked = true;
            LockedDateTime = DateTime.UtcNow;
        }

        public void Unlock()
        {
            IsLocked = false;
            LockedDateTime = null;
        }

        public (bool result, ErrorDefinition? error) LoginByPassword(string pwd)
        {
            return TryLogin(() =>
            {
                ErrorDefinition? error = null;
                bool loginResult = false;

                if (Password != EncryptPwd(pwd))
                {
                    error = UserErrorCodes.PASSWORD_ERROR;
                }
                else
                {
                    loginResult = true;
                }

                return (loginResult, error);
            });
        }

        public (bool result, ErrorDefinition? error) LoginByCustomValidation(Func<(bool result, ErrorDefinition? error)> validationFunc)
        {
            return TryLogin(validationFunc);
        }

        public bool CheckIdentityVerified(UserIdentificationType type, string value)
        {
            var record = UserIdentifications.FirstOrDefault(s => s.Type == type && s.Value == value);
            return record?.IsVerified ?? false;
        }

        private (bool result, ErrorDefinition? error) TryLogin(Func<(bool result, ErrorDefinition? error)> loginMethodVerify)
        {
            if (IsLocked)
            {
                throw DomainException.Create(UserErrorCodes.USER_LOCKED);
            }

            bool loginResult;
            var (result, error) = loginMethodVerify();
            if (!result)
            {
                LoginAttempt++;
                if (LoginAttempt >= 8)
                {
                    Lock();
                }
                loginResult = false;
            }
            else
            {
                loginResult = true;
                LoginAttempt = 0;
                LastLoginTime = DateTime.UtcNow;
            }

            return (loginResult, error);
        }

        /// <summary>
        /// verify user_name format
        /// </summary>
        /// <param name="userName"></param>
        private static void CheckUserNameFormat(string userName)
        {
            if (ValidationHelper.IsPhoneNumber(userName))
            {
                throw DomainException.Create(UserErrorCodes.USER_NAME_INVALID);
            }
            if (ValidationHelper.IsEmail(userName))
            {
                throw DomainException.Create(UserErrorCodes.USER_NAME_INVALID);
            }
            if (userName.Length < 3 || userName.Length > 20)
            {
                throw DomainException.Create(UserErrorCodes.USER_NAME_LENGTH_ERROR);
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(userName, @"^[a-zA-Z0-9_]{3,20}$"))
            {
                throw DomainException.Create(UserErrorCodes.USER_NAME_SPECIAL_LETTER);
            }
        }

        private static void CheckPasswordFormat(string pwd)
        {
            if (pwd.Length < 6 || pwd.Length > 20)
            {
                throw DomainException.Create(UserErrorCodes.PASSWORD_LENGTH_ERROR);
            }
        }

        private static string EncryptPwd(string pwd)
        {
            return HashHelper.GetHash(pwd);
        }
    }
}
