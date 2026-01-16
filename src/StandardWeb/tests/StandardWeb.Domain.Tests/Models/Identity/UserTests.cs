using StandardWeb.Domain.Enums.Identity;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Domain.Tests.Models.Identity;

/// <summary>
/// Unit tests for User domain model to validate business logic and invariants.
/// </summary>
public class UserTests
{
    [Fact]
    public void RegisterNewUser_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var phoneNumber = "13800138000";
        var passwordHash = "hashedPassword123";
        var salt = "randomSalt";

        // Act
        var user = User.RegisterNewUser(phoneNumber, passwordHash, salt);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(phoneNumber, user.PhoneNumber);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(salt, user.Salt);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.False(user.LockoutEnabled);
        Assert.Equal(0, user.AccessFailedCount);
    }

    [Fact]
    public void RegisterNewUser_WithEmptyPhoneNumber_ShouldThrowArgumentException()
    {
        // Arrange
        var phoneNumber = "";
        var passwordHash = "hashedPassword123";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => User.RegisterNewUser(phoneNumber, passwordHash));
        Assert.Contains("Phone number cannot be empty", exception.Message);
    }

    [Fact]
    public void RegisterNewUser_WithEmptyPasswordHash_ShouldThrowArgumentException()
    {
        // Arrange
        var phoneNumber = "13800138000";
        var passwordHash = "";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => User.RegisterNewUser(phoneNumber, passwordHash));
        Assert.Contains("Password hash cannot be empty", exception.Message);
    }

    [Fact]
    public void UpdateProfile_WithValidData_ShouldUpdateUserProfile()
    {
        // Arrange
        var user = User.RegisterNewUser("13800138000", "hashedPassword");
        var firstName = "John";
        var lastName = "Doe";
        var displayName = "JohnD";
        var dateOfBirth = new DateTime(1990, 1, 1);

        // Act
        user.UpdateProfile(firstName, lastName, displayName, dateOfBirth);

        // Assert
        Assert.Equal(firstName, user.FirstName);
        Assert.Equal(lastName, user.LastName);
        Assert.Equal(displayName, user.DisplayName);
        Assert.Equal(dateOfBirth, user.DateOfBirth);
    }

    [Fact]
    public void LockAccount_WithValidDuration_ShouldLockUser()
    {
        // Arrange
        var user = User.RegisterNewUser("13800138000", "hashedPassword");
        var lockDuration = TimeSpan.FromHours(24);

        // Act
        user.LockAccount(lockDuration);

        // Assert
        Assert.True(user.LockoutEnabled);
        Assert.NotNull(user.LockoutEnd);
        Assert.True(user.IsLockedOut());
    }

    [Fact]
    public void LockAccount_WithNegativeDuration_ShouldThrowArgumentException()
    {
        // Arrange
        var user = User.RegisterNewUser("13800138000", "hashedPassword");
        var lockDuration = TimeSpan.FromHours(-1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => user.LockAccount(lockDuration));
        Assert.Contains("Lock duration must be positive", exception.Message);
    }

    [Fact]
    public void UnlockAccount_ShouldResetLockoutState()
    {
        // Arrange
        var user = User.RegisterNewUser("13800138000", "hashedPassword");
        user.LockAccount(TimeSpan.FromHours(24));
        user.IncrementAccessFailedCount();
        user.IncrementAccessFailedCount();

        // Act
        user.UnlockAccount();

        // Assert
        Assert.False(user.LockoutEnabled);
        Assert.Null(user.LockoutEnd);
        Assert.Equal(0, user.AccessFailedCount);
        Assert.False(user.IsLockedOut());
    }

    [Fact]
    public void IncrementAccessFailedCount_ShouldIncreaseCounter()
    {
        // Arrange
        var user = User.RegisterNewUser("13800138000", "hashedPassword");

        // Act
        user.IncrementAccessFailedCount();
        user.IncrementAccessFailedCount();

        // Assert
        Assert.Equal(2, user.AccessFailedCount);
    }

    [Fact]
    public void IncrementAccessFailedCount_WhenReachingMaxAttempts_ShouldMarkDangerousLogin()
    {
        // Arrange
        var user = User.RegisterNewUser("13800138000", "hashedPassword");

        // Act
        for (int i = 0; i < 5; i++)
        {
            user.IncrementAccessFailedCount();
        }

        // Assert
        Assert.Equal(5, user.AccessFailedCount);
        Assert.True(user.ForceOTPOnLogin);
    }

    [Fact]
    public void ResetAccessFailedCount_ShouldResetCounterAndMarkSafe()
    {
        // Arrange
        var user = User.RegisterNewUser("13800138000", "hashedPassword");
        user.IncrementAccessFailedCount();
        user.IncrementAccessFailedCount();
        user.MarkDangerousLogin();

        // Act
        user.ResetAccessFailedCount();

        // Assert
        Assert.Equal(0, user.AccessFailedCount);
        Assert.False(user.ForceOTPOnLogin);
    }

    [Fact]
    public void ChangePassword_WithValidHash_ShouldUpdatePassword()
    {
        // Arrange
        var user = User.RegisterNewUser("13800138000", "oldPasswordHash");
        var newPasswordHash = "newPasswordHash";
        var newSalt = "newSalt";

        // Act
        user.ChangePassword(newPasswordHash, newSalt);

        // Assert
        Assert.Equal(newPasswordHash, user.PasswordHash);
        Assert.Equal(newSalt, user.Salt);
    }

    [Fact]
    public void ChangePassword_WithEmptyHash_ShouldThrowArgumentException()
    {
        // Arrange
        var user = User.RegisterNewUser("13800138000", "oldPasswordHash");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => user.ChangePassword("", null));
        Assert.Contains("Password hash cannot be empty", exception.Message);
    }

    [Fact]
    public void UpdateEmail_ShouldSetEmailAddress()
    {
        // Arrange
        var user = User.RegisterNewUser("13800138000", "hashedPassword");
        var email = "john@example.com";

        // Act
        user.UpdateEmail(email);

        // Assert
        Assert.Equal(email, user.Email);
    }

    [Fact]
    public void SetProfilePicture_WithValidUrl_ShouldSetPictureUrl()
    {
        // Arrange
        var user = User.RegisterNewUser("13800138000", "hashedPassword");
        var pictureUrl = "https://example.com/avatar.jpg";

        // Act
        user.SetProfilePicture(pictureUrl);

        // Assert
        Assert.Equal(pictureUrl, user.ProfilePictureUrl);
    }

    [Fact]
    public void SetProfilePicture_WithEmptyUrl_ShouldThrowArgumentException()
    {
        // Arrange
        var user = User.RegisterNewUser("13800138000", "hashedPassword");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => user.SetProfilePicture(""));
        Assert.Contains("Picture URL cannot be empty", exception.Message);
    }

    [Fact]
    public void UpdateStatus_ShouldChangeUserStatus()
    {
        // Arrange
        var user = User.RegisterNewUser("13800138000", "hashedPassword");

        // Act
        user.UpdateStatus(UserStatus.Frozen);

        // Assert
        Assert.Equal(UserStatus.Frozen, user.Status);
    }
}
