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
    /// <param name="ct">Cancellation token for async operation</param>
    /// <returns>Operation result with created user or error details</returns>
    public async Task<OperationResult<UserDto?>> RegisterAsync(UserRegistrationDto data, CancellationToken ct = default)
    {
        Logger.LogInformation("Registering user with phone number: {PhoneNumber}", data.PhoneNumber);

        // Validate that at least one contact method is provided
        if (string.IsNullOrWhiteSpace(data.PhoneNumber) && string.IsNullOrWhiteSpace(data.Email))
        {
            return OperationResult<UserDto?>.Failure("At least one contact method (phone or email) is required.", BaseErrorCodes.InvalidInput);
        }

        // Validate phone number format if provided
        if (!string.IsNullOrWhiteSpace(data.PhoneNumber) && !FormatChecker.IsValidPhoneNumber(data.PhoneNumber))
        {
            return OperationResult<UserDto?>.Failure("Invalid phone number format.", BaseErrorCodes.InvalidInput);
        }

        // Check if user already exists (by phone or email)
        User? existingUser = null;
        if (!string.IsNullOrWhiteSpace(data.PhoneNumber))
        {
            existingUser = await UserRepo.GetByPhoneNumberAsync(data.PhoneNumber, false, cancellationToken: ct);
        }
        if (existingUser is null && !string.IsNullOrWhiteSpace(data.Email))
        {
            existingUser = await UserRepo.GetByEmailAsync(data.Email, false, cancellationToken: ct);
        }

        if (existingUser is not null)
        {
            return OperationResult<UserDto?>.Failure("User with the given contact information already exists.", AuthErrorCodes.UserAlreadyExists);
        }

        // Validate password is provided
        if (string.IsNullOrWhiteSpace(data.Password))
        {
            return OperationResult<UserDto?>.Failure("Password cannot be empty.", BaseErrorCodes.InvalidInput);
        }

        // Hash password with BCrypt
        var (hash, salt) = EncryptionHelper.HashContent(data.Password);

        // Create user with appropriate factory method
        User newUser;
        if (!string.IsNullOrWhiteSpace(data.PhoneNumber) && !string.IsNullOrWhiteSpace(data.Email))
        {
            newUser = User.RegisterWithBoth(data.PhoneNumber, data.Email, hash, salt);
        }
        else if (!string.IsNullOrWhiteSpace(data.PhoneNumber))
        {
            newUser = User.RegisterWithPhoneNumber(data.PhoneNumber, hash, salt);
        }
        else
        {
            newUser = User.RegisterWithEmail(data.Email!, hash, salt);
        }

        newUser.UpdateProfile(PersonalInfo.Create(data.FirstName, data.LastName, data.DisplayName));

        await UserRepo.AddAsync(newUser, ct);
        var result = await UserRepo.SaveChangesAsync(ct);
        if (result < 0)
        {
            return OperationResult<UserDto?>.Failure("Failed to register user.", BaseErrorCodes.InternalError);
        }

        Logger.LogInformation("User {UserId} registered successfully", newUser.Id);
        return OperationResult<UserDto?>.Success(Mapper.Map<User, UserDto>(newUser));
    }

    /// <summary>
    /// Authenticates a user with phone number and password.
    /// Handles account locking after failed attempts and OTP requirement for suspicious accounts.
    /// </summary>
    /// <param name="data">Login credentials and optional OTP code</param>
    /// <param name="ct">Cancellation token for async operation</param>
    /// <returns>Operation result with login tokens and user info, or error details</returns>
    public async Task<OperationResult<LoginResultDto>> LoginByPhoneAsync(LoginRequestDto data, CancellationToken ct = default)
    {
        Logger.LogInformation("Logging in user with contact: {Contact}", data.PhoneNumber);

        // Validate that at least one contact method is provided
        if (string.IsNullOrWhiteSpace(data.PhoneNumber))
        {
            return OperationResult<LoginResultDto>.Failure("Phone number or email is required.", AuthErrorCodes.InvalidInput);
        }

        // Load user with related tokens for refresh token validation
        // Try to find by phone
        var user = await UserRepo.GetByPhoneNumberWithIncludesAsync(data.PhoneNumber!, s => s.Include(j => j.UserTokens), needTracking: true, cancellationToken: ct);
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

        // Check if user has a password (external-only users cannot login with password)
        if (!user.HasPassword())
        {
            return OperationResult<LoginResultDto>.Failure("This account requires external login (no password set).", AuthErrorCodes.InvalidCredentials);
        }

        // Check if OTP is required for this account
        if (user.ForceOTPOnLogin && string.IsNullOrWhiteSpace(data.OtpCode))
        {
            return OperationResult<LoginResultDto>.Success(CreateOtpRequiredResult());
        }

        // Verify password hash
        if (!EncryptionHelper.VerifyHash(data.Password!, user.Credential!.Hash, user.Credential.Salt ?? ""))
        {
            await HandleFailedLoginAttemptAsync(user, ct);
            return OperationResult<LoginResultDto>.Failure("Invalid credentials.", AuthErrorCodes.InvalidCredentials);
        }

        // Validate OTP code if provided (placeholder for actual OTP validation)
        if (user.ForceOTPOnLogin && !string.IsNullOrWhiteSpace(data.OtpCode))
        {
            // TODO: Implement actual OTP validation logic here
        }

        // Reset failed login counter on successful authentication
        HandleSuccessfulLogin(user);

        // Generate JWT access and refresh tokens
        var tokenResult = await GenerateJwtTokenAsync(user, ct);
        await UserRepo.SaveChangesAsync(ct);

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