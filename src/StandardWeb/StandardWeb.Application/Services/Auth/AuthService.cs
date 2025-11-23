using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StandardWeb.Application.ErrorCodes;
using StandardWeb.Application.Providers;
using StandardWeb.Common.Helpers;
using StandardWeb.Contracts.Dtos.Identity;
using StandardWeb.Domain.Models.Identity;
using StandardWeb.Domain.Repositories;

namespace StandardWeb.Application.Services.Auth;

/// <summary>
/// Provides authentication services including user registration, login, and token management.
/// Handles password validation, account locking, and JWT token generation.
/// </summary>
[InjectService(Lifetime = MiCakeServiceLifetime.Scoped)]
public class AuthService : BaseLoginService
{
    public AuthService(JwtProvider jwtProvider,
                       IUserRepo userRepo,
                       IMapper mapper,
                       ILogger<AuthService> logger) : base(jwtProvider, userRepo, mapper, logger)
    {
    }

    /// <summary>
    /// Registers a new user with phone number and password.
    /// Validates phone format and checks for existing accounts.
    /// </summary>
    /// <param name="data">Registration data including credentials and profile info</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Operation result with created user or error details</returns>
    public async Task<OperationResult<User?>> RegisterAsync(UserRegistrationDto data, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Registering user with phone number: {PhoneNumber}", data.PhoneNumber);

        // Validate phone number format (Chinese mobile format)
        if (FormatChecker.IsValidPhoneNumber(data.PhoneNumber) == false)
        {
            return OperationResult<User?>.Failure("Invalid phone number format.", BaseErrorCodes.InvalidInput);
        }

        // Check if phone number already exists
        var existingUser = await UserRepo.GetByPhoneNumberAsync(data.PhoneNumber!, false, cancellationToken: cancellationToken);
        if (existingUser is not null)
        {
            return OperationResult<User?>.Failure("User with the given phone number already exists.", AuthErrorCodes.UserAlreadyExists);
        }

        // Validate password is provided
        if (string.IsNullOrWhiteSpace(data.Password))
        {
            return OperationResult<User?>.Failure("Password cannot be empty.", BaseErrorCodes.InvalidInput);
        }

        // Hash password with BCrypt
        var (hash, salt) = EncryptionHelper.HashContent(data.Password);
        var newUser = User.RegisterNewUser(data.PhoneNumber!, hash, salt);
        newUser.UpdateProfile(data.FirstName, data.LastName, data.DisplayName);

        // Persist user to database
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
    /// Authenticates a user with phone number and password.
    /// Handles account locking after failed attempts and OTP requirement for suspicious accounts.
    /// </summary>
    /// <param name="data">Login credentials and optional OTP code</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Operation result with login tokens and user info, or error details</returns>
    public async Task<OperationResult<LoginResultDto>> LoginAsync(LoginRequestDto data, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Logging in user with phone number: {PhoneNumber}", data.PhoneNumber);

        // Load user with related tokens for refresh token validation
        var user = await UserRepo.GetByPhoneNumberWithIncludesAsync(data.PhoneNumber!, s => s.Include(j => j.UserTokens), cancellationToken: cancellationToken);
        if (user is null)
        {
            return OperationResult<LoginResultDto>.Failure("User not found.", AuthErrorCodes.UserNotFound);
        }

        // Validate account status (locked, suspended, etc.)
        var accountValidation = ValidateUserAccountStatus(user);
        if (!accountValidation.IsSuccess)
        {
            return OperationResult<LoginResultDto>.Failure(accountValidation.ErrorMessage ?? "Account validation failed", accountValidation.ErrorCode);
        }

        // Check if OTP is required for this account
        if (user.ForceOTPOnLogin && string.IsNullOrWhiteSpace(data.OtpCode))
        {
            return OperationResult<LoginResultDto>.Success(CreateOtpRequiredResult());
        }

        // Verify password hash
        if (!EncryptionHelper.VerifyHash(data.Password!, user.PasswordHash, user.Salt ?? ""))
        {
            await HandleFailedLoginAttemptAsync(user, cancellationToken);
            return OperationResult<LoginResultDto>.Failure("Invalid credentials.", AuthErrorCodes.InvalidCredentials);
        }

        // Validate OTP code if provided (placeholder for actual OTP validation)
        if (user.ForceOTPOnLogin && string.IsNullOrWhiteSpace(data.OtpCode) == false)
        {
            // TODO: Implement actual OTP validation logic here
        }

        // Reset failed login counter on successful authentication
        HandleSuccessfulLogin(user);

        // Generate JWT access and refresh tokens
        var tokenResult = await GenerateJwtTokenAsync(user, cancellationToken);
        await UserRepo.SaveChangesAsync(cancellationToken);

        var loginResult = CreateLoginResult(user, tokenResult);

        Logger.LogInformation("User {UserId} logged in successfully", user.Id);
        return OperationResult<LoginResultDto>.Success(loginResult);
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// Validates refresh token and generates new token pair.
    /// </summary>
    /// <param name="refreshToken">Current refresh token</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Operation result with new tokens or error details</returns>
    public async Task<OperationResult<LoginResultDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await ValidateAndRefreshTokenAsync(refreshToken, cancellationToken);
    }
}