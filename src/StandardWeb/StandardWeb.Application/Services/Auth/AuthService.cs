using MiCake.Core.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using StandardWeb.Application.CodeDefines;
using StandardWeb.Application.Models;
using StandardWeb.Application.Providers;
using StandardWeb.Common;
using StandardWeb.Common.Helpers;
using StandardWeb.Domain.Models.Identity;
using StandardWeb.Domain.Repositories;

namespace StandardWeb.Application.Services.Auth;

/// <summary>
/// Provides authentication services including user registration, login, and token refresh
/// </summary>
[InjectService(Lifetime = MiCakeServiceLifetime.Scoped)]
public class AuthService : BaseLoginService
{

    public AuthService(JwtProvider jwtProvider, IUserRepo userRepo, ILogger<AuthService> logger)
        : base(jwtProvider, userRepo, logger)
    {
    }

    /// <summary>
    /// Registers a new user with phone number and password
    /// </summary>
    /// <param name="data">User registration data including phone number, password, and profile information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result containing the created user or error information</returns>
    public async Task<OperationResult<User?>> RegisterAsync(UserRegistrationModel data, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Registering user with phone number: {PhoneNumber}", data.PhoneNumber);

        if (FormatChecker.IsValidPhoneNumber(data.PhoneNumber) == false)
        {
            return OperationResult<User?>.Failure("Invalid phone number format.", BaseErrorCodes.InvalidInput);
        }

        var existingUser = await UserRepo.GetByPhoneNumberAsync(data.PhoneNumber!, cancellationToken: cancellationToken);
        if (existingUser != null)
        {
            return OperationResult<User?>.Failure("User with the given phone number already exists.", AuthErrorCodes.UserAlreadyExists);
        }

        if (string.IsNullOrWhiteSpace(data.Password))
        {
            return OperationResult<User?>.Failure("Password cannot be empty.", BaseErrorCodes.InvalidInput);
        }

        var (hash, salt) = EncryptionHelper.HashContent(data.Password);
        var newUser = User.RegisterNewUser(data.PhoneNumber!, hash, salt);
        newUser.UpdateProfile(data.FirstName, data.LastName, data.DisplayName);

        await UserRepo.AddAsync(newUser, cancellationToken);
        var result = await UserRepo.SaveChangesAsync(cancellationToken);
        if (result < 0)
        {
            return OperationResult<User?>.Failure("Failed to register user.", BaseErrorCodes.InternalError);
        }

        Logger.LogInformation("User {UserId} registered successfully", newUser.Id);
        return OperationResult<User?>.Success(newUser);
    }

    /// <summary>
    /// Authenticates a user with phone number and password, optionally requiring OTP verification
    /// </summary>
    /// <param name="data">Login credentials including phone number, password, and optional OTP code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result containing login result with JWT tokens or error information</returns>
    public async Task<OperationResult<UserLoginResult>> LoginAsync(UserLoginModel data, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Logging in user with phone number: {PhoneNumber}", data.PhoneNumber);

        var user = await UserRepo.GetByPhoneNumberWithIncludesAsync(data.PhoneNumber!, s => s.Include(j => j.UserTokens), cancellationToken: cancellationToken);
        if (user == null)
        {
            return OperationResult<UserLoginResult>.Failure("User not found.", AuthErrorCodes.UserNotFound);
        }

        var accountValidation = ValidateUserAccountStatus(user);
        if (!accountValidation.IsSuccess)
        {
            return OperationResult<UserLoginResult>.Failure(accountValidation.ErrorMessage ?? "Account validation failed", accountValidation.ErrorCode);
        }

        if (user.ForceOTPOnLogin && string.IsNullOrWhiteSpace(data.OtpCode))
        {
            return OperationResult<UserLoginResult>.Success(CreateOtpRequiredResult());
        }

        if (!EncryptionHelper.VerifyHash(data.Password!, user.PasswordHash, user.Salt ?? ""))
        {
            await HandleFailedLoginAttemptAsync(user, cancellationToken);
            return OperationResult<UserLoginResult>.Failure("Invalid credentials.", AuthErrorCodes.InvalidCredentials);
        }

        if (user.ForceOTPOnLogin && string.IsNullOrWhiteSpace(data.OtpCode) == false)
        {
            // Here you would normally validate the OTP code.
        }

        HandleSuccessfulLogin(user);

        var tokenResult = await GenerateJwtTokenAsync(user, cancellationToken);
        await UserRepo.SaveChangesAsync(cancellationToken);

        var loginResult = CreateLoginResult(user, tokenResult);

        Logger.LogInformation("User {UserId} logged in successfully", user.Id);
        return OperationResult<UserLoginResult>.Success(loginResult);
    }

    /// <summary>
    /// Refreshes an expired JWT token using a valid refresh token
    /// </summary>
    /// <param name="refreshToken">The refresh token to use for generating new tokens</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result containing new JWT tokens or error information</returns>
    public async Task<OperationResult<UserLoginResult>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await ValidateAndRefreshTokenAsync(refreshToken, cancellationToken);
    }
}