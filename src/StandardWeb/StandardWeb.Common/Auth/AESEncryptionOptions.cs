using System.ComponentModel.DataAnnotations;

namespace StandardWeb.Common.Auth;

/// <summary>
/// Options describing how AES encryption keys are provided for shared helpers.
/// </summary>
public class AESEncryptionOptions
{
    [Required]
    [MinLength(16, ErrorMessage = "AES encryption key must be at least 16 characters long.")]
    public string Key { get; set; } = string.Empty;
}
