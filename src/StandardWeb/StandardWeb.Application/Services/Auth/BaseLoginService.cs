using StandardWeb.Application.CodeDefines;
using StandardWeb.Application.Models;
using StandardWeb.Application.Providers;
using StandardWeb.Common;
using StandardWeb.Common.Auth;
using StandardWeb.Domain.Enums.Identity;
using StandardWeb.Domain.Models.Identity;
using StandardWeb.Domain.Repositories;

namespace StandardWeb.Application.Services.Auth;

/// <summary>
/// Abstract base class for login services providing centralized JWT token generation, 
/// refresh token management, and common authentication workflows
/// </summary>
public abstract class BaseLoginService
{
    protected readonly JwtProvider JwtProvider;
    protected readonly IUserRepo UserRepo;
    protected readonly ILogger Logger;

    protected BaseLoginService(JwtProvider jwtProvider, IUserRepo userRepo, ILogger logger)
    {
        JwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
        UserRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Token Generation & Management

    /// <summary>
    /// Generates JWT token and adds/updates the refresh token for the user
    /// </summary>
    protected Task<JwtTokenModel> GenerateJwtTokenAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        var tokenResult = JwtProvider.GenerateToken(user, []);

        var refreshToken = UserToken.Create(
            UserTokenType.JwtRefreshToken,
            tokenResult.RefreshToken,
            tokenResult.RefreshTokenExpiration);

        user.AddOrUpdateUserToken(refreshToken);

        return Task.FromResult(tokenResult);
    }

    /// <summary>
    /// Validates and refreshes an expired JWT token
    /// </summary>
    protected async Task<OperationResult<UserLoginResult>> ValidateAndRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Attempting to refresh token");

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return OperationResult<UserLoginResult>.Failure("Refresh token cannot be empty.", AuthErrorCodes.InvalidInput);
        }

        var userRecord = await UserRepo.FindByUserTokenAsync(UserTokenType.JwtRefreshToken, refreshToken, cancellationToken);
        if (userRecord == null)
        {
            return OperationResult<UserLoginResult>.Failure("Invalid refresh token.", AuthErrorCodes.InvalidToken);
        }

        var refreshTokenRecord = userRecord.GetUserToken(UserTokenType.JwtRefreshToken);
        if (refreshTokenRecord == null || refreshTokenRecord.HasExpired())
        {
            return OperationResult<UserLoginResult>.Failure("Refresh token has expired.", AuthErrorCodes.InvalidToken);
        }

        var user = await UserRepo.FindAsync(userRecord.Id, cancellationToken);
        if (user == null)
        {
            return OperationResult<UserLoginResult>.Failure("User not found.", AuthErrorCodes.UserNotFound);
        }

        if (user.IsLockedOut())
        {
            return OperationResult<UserLoginResult>.Failure("User account is locked out.", AuthErrorCodes.UserInvalidStatus);
        }

        var tokenResult = await GenerateJwtTokenAsync(user, cancellationToken);
        await UserRepo.SaveChangesAsync(cancellationToken);

        var loginResult = new UserLoginResult
        {
            User = user,
            Token = tokenResult.JwtToken,
            Expiration = tokenResult.Expiration,
            RefreshToken = tokenResult.RefreshToken,
            RefreshTokenExpiration = tokenResult.RefreshTokenExpiration,
            LoginPassed = true
        };

        Logger.LogInformation("Token refreshed successfully for user {UserId}", user.Id);
        return OperationResult<UserLoginResult>.Success(loginResult);
    }

    #endregion

    #region User Validation & Lockout

    /// <summary>
    /// Validates if user account is in valid state for login
    /// </summary>
    protected OperationResult<bool> ValidateUserAccountStatus(User user)
    {
        if (user.IsLockedOut())
        {
            return OperationResult<bool>.Failure("User account is locked out.", AuthErrorCodes.UserInvalidStatus);
        }

        return OperationResult<bool>.Success(true);
    }

    /// <summary>
    /// Handles failed login attempt by incrementing access failed count
    /// </summary>
    protected async Task HandleFailedLoginAttemptAsync(User user, CancellationToken cancellationToken = default)
    {
        user.IncrementAccessFailedCount();
        await UserRepo.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Handles successful login by resetting access failed count
    /// </summary>
    protected void HandleSuccessfulLogin(User user)
    {
        user.ResetAccessFailedCount();
    }

    #endregion

    #region Result Building

    /// <summary>
    /// Creates a successful login result with token information
    /// </summary>
    protected UserLoginResult CreateLoginResult(
        User user,
        JwtTokenModel tokenResult)
    {
        return new UserLoginResult
        {
            User = user,
            Token = tokenResult.JwtToken,
            Expiration = tokenResult.Expiration,
            RefreshToken = tokenResult.RefreshToken,
            RefreshTokenExpiration = tokenResult.RefreshTokenExpiration,
            LoginPassed = true
        };
    }

    /// <summary>
    /// Creates a login result requiring OTP verification
    /// </summary>
    protected UserLoginResult CreateOtpRequiredResult()
    {
        return new UserLoginResult
        {
            NeedOtpVerification = true,
            LoginPassed = false
        };
    }

    #endregion
}
