# Dynamic Configuration Settings Feature - Architecture Design

**Created**: 2026-01-16  
**Architect**: MiCake Architect  
**Status**: 📋 Ready for Review  
**Last Updated**: 2026-01-16  
**Version**: 1.3

---

## Revision History

### v1.3 (2026-01-16) - Latest
**Architecture refinement**:
1. ✅ **Removed ConfigurationProvider**: Merged functionality into `AppSettingService` to eliminate redundancy
2. ✅ **Moved IDataProtectionService**: Relocated to `StandardWeb.Common/Security/` as a general-purpose encryption service
3. ✅ **Unified configuration service**: `AppSettingService` now handles both CRUD and cached reading

### v1.2 (2026-01-16)
**Refinements based on user feedback**:
1. ✅ **ValidationPattern support**: Regex-based value validation before storage
2. ✅ **Application-layer encryption**: `IDataProtectionService` in Application layer
3. ✅ **Resolved design questions**: Encryption and validation approaches decided

### v1.1 (2026-01-16)
**Changes based on user feedback**:
1. ✅ **Removed redundant audit fields**: Inherit from `AuditAggregateRoot`
2. ✅ **SettingGroup enum**: Type-safe groups instead of free-form strings
3. ✅ **Updated signatures**: Enum parameters throughout

---

## 1. Overview

### 1.1 Business Context
This feature enables runtime configuration management, allowing users to modify application settings dynamically without redeployment. Configuration items are organized into strongly-typed setting groups (e.g., Email, Sms, Payment) with individual properties that can be modified through UI or API.

### 1.2 Design Goals
- **Type Safety**: Strong typing via enums and generic methods
- **Security**: Transparent encryption for sensitive values
- **Validation**: Regex pattern matching for data integrity
- **Performance**: Intelligent caching with configurable expiration
- **Auditability**: Automatic tracking of all changes
- **Extensibility**: Easy addition of new setting groups

---

## 2. Domain Model Design

### 2.1 Aggregate: AppSetting

**Aggregate Root**: `AppSetting` (extends `AuditAggregateRoot`)

```
AppSetting (Aggregate Root) : AuditAggregateRoot
├── Id: long (inherited from AggregateRoot<long>)
├── SettingGroup: SettingGroup enum
├── Key: string
├── Value: string (already encrypted if IsEncrypted=true)
├── DataType: SettingDataType enum
├── Description: string?
├── IsEncrypted: bool
├── ValidationPattern: string? (regex)
└── [Audit Fields - Inherited]
    ├── CreatedBy: long?
    ├── CreatedAt: DateTime
    ├── ModifiedBy: long?
    └── UpdatedAt: DateTime?
```

**Invariants**:
- `SettingGroup + Key` must be unique
- Value must be valid for specified `DataType`
- Value must match `ValidationPattern` if specified
- Encrypted values are already encrypted when stored
- System-required settings cannot be deleted

**Factory Method**:
```csharp
public static AppSetting Create(
    SettingGroup settingGroup, 
    string key, 
    string value,                    // Already encrypted if isEncrypted=true
    SettingDataType dataType, 
    bool isEncrypted = false,
    string? description = null,
    string? validationPattern = null)
{
    // Validates parameters, checks pattern if provided
    // Returns new AppSetting instance
}
```

**Business Methods**:
```csharp
public void UpdateValue(string newValue)      // newValue already encrypted if needed
public bool ValidateValue(string value)       // Validate against ValidationPattern
public bool IsValidFormat()                    // Check current value matches pattern
```

**Key Design Point**: Encryption/decryption happens in **Application layer**, not Domain. Aggregate only stores encrypted values.

---

### 2.2 Enum: SettingGroup

```csharp
/// <summary>
/// Defines logical grouping of application settings.
/// Provides strong typing and prevents arbitrary group creation.
/// </summary>
public enum SettingGroup
{
    /// <summary>Email service settings (SMTP, port, authentication)</summary>
    Email = 1,
    
    /// <summary>SMS service settings (provider, API keys, templates)</summary>
    Sms = 2,
    
    /// <summary>Payment gateway settings (API credentials, callbacks)</summary>
    Payment = 3,
    
    /// <summary>General system settings (maintenance mode, feature flags)</summary>
    System = 4,
    
    /// <summary>Security and authentication settings (password policy, lockout)</summary>
    Security = 5,
    
    /// <summary>File storage settings (upload limits, allowed types, paths)</summary>
    Storage = 6,
    
    /// <summary>Notification settings (push, email, in-app)</summary>
    Notification = 7,
    
    /// <summary>Integration with external services (APIs, webhooks)</summary>
    Integration = 8
}
```

---

### 2.3 Enum: SettingDataType

```csharp
/// <summary>
/// Specifies the data type of a setting value for validation and parsing.
/// </summary>
public enum SettingDataType
{
    String = 1,
    Integer = 2,
    Boolean = 3,
    Decimal = 4,
    Json = 5
}
```

---

### 2.4 Domain Event

```csharp
/// <summary>
/// Published when a setting value is changed.
/// Enables cache invalidation, audit logging, and notifications.
/// </summary>
public record SettingValueChangedEvent(
    long SettingId,
    SettingGroup SettingGroup,
    string Key,
    string OldValue,
    string NewValue,
    long? ChangedBy,
    DateTime ChangedAt) : IDomainEvent;
```

---

## 3. Layer Architecture

### 3.1 Common Layer (Infrastructure)

#### 3.1.1 Data Protection Service ✨

**Location**: `StandardWeb.Common/Security/`

**Why Common Layer?**
- General-purpose encryption service, not specific to Configuration module
- Can be reused for user passwords, tokens, sensitive data across modules
- Infrastructure concern, not business logic

**Interface**: `StandardWeb.Common/Security/IDataProtectionService.cs`

```csharp
namespace StandardWeb.Common.Security;

/// <summary>
/// Provides encryption and decryption for sensitive data.
/// Uses ASP.NET Core Data Protection API.
/// Can be used across multiple modules (Configuration, User, Auth, etc.).
/// </summary>
public interface IDataProtectionService
{
    /// <summary>Encrypts plain text for secure storage.</summary>
    string Protect(string plainText, string purpose = "AppData");
    
    /// <summary>Decrypts encrypted value.</summary>
    string Unprotect(string encryptedText, string purpose = "AppData");
    
    /// <summary>Checks if a string appears to be encrypted.</summary>
    bool IsEncrypted(string value);
}
```

**Implementation**: `StandardWeb.Common/Security/DataProtectionService.cs`

```csharp
using Microsoft.AspNetCore.DataProtection;

namespace StandardWeb.Common.Security;

[InjectService(ServiceTypes = [typeof(IDataProtectionService)], 
               Lifetime = MiCakeServiceLifetime.Singleton)]  // Singleton for performance
public class DataProtectionService : IDataProtectionService
{
    private readonly IDataProtectionProvider _protectionProvider;
    private readonly ILogger<DataProtectionService> _logger;
    
    public DataProtectionService(
        IDataProtectionProvider protectionProvider,
        ILogger<DataProtectionService> logger)
    {
        _protectionProvider = protectionProvider;
        _logger = logger;
    }
    
    public string Protect(string plainText, string purpose = "AppData")
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be empty", nameof(plainText));
            
        var protector = _protectionProvider.CreateProtector(purpose);
        return protector.Protect(plainText);
    }
    
    public string Unprotect(string encryptedText, string purpose = "AppData")
    {
        if (string.IsNullOrEmpty(encryptedText))
            throw new ArgumentException("Encrypted text cannot be empty", nameof(encryptedText));
            
        var protector = _protectionProvider.CreateProtector(purpose);
        return protector.Unprotect(encryptedText);
    }
    
    public bool IsEncrypted(string value)
    {
        // Heuristic: Data Protection output is Base64-like
        return !string.IsNullOrEmpty(value) && 
               value.Length > 50 && 
               !value.Any(char.IsWhiteSpace);
    }
}
```

**Registration**: Auto-registered via `[InjectService]`. ASP.NET Core's `IDataProtectionProvider` is available from DI.

**Usage Across Modules**:
```csharp
// In Configuration module
var encrypted = _dataProtectionService.Protect(apiKey, "AppSettings");

// In User module (future)
var encryptedSsn = _dataProtectionService.Protect(ssn, "UserData");

// In Auth module (future)
var encryptedToken = _dataProtectionService.Protect(token, "AuthTokens");
```

---

### 3.2 Domain Layer

**Location**: `StandardWeb.Domain/`

```
Models/Configuration/
├── AppSetting.cs              (Aggregate Root extends AuditAggregateRoot)
└── Events/
    └── SettingValueChangedEvent.cs

Enums/Configuration/
├── SettingGroup.cs            (Enum - Email, Sms, Payment, etc.)
└── SettingDataType.cs         (Enum - String, Integer, Boolean, etc.)

Repositories/Configuration/
└── IAppSettingRepository.cs   (Repository interface)
```

**Repository Interface**:
```csharp
public interface IAppSettingRepository : IRepository<AppSetting>
{
    Task<AppSetting?> GetByKeyAsync(
        SettingGroup settingGroup, 
        string key, 
        CancellationToken ct = default);
        
    Task<IReadOnlyList<AppSetting>> GetByGroupAsync(
        SettingGroup settingGroup, 
        CancellationToken ct = default);
        
    Task<Dictionary<string, string>> GetGroupAsDictionaryAsync(
        SettingGroup settingGroup, 
        CancellationToken ct = default);
        
    Task<bool> ExistsAsync(
        SettingGroup settingGroup, 
        string key, 
        CancellationToken ct = default);
}
```

---

### 3.3 Application Layer

#### 3.3.1 Application Service (Unified)

**Location**: `StandardWeb.Application/Services/Configuration/AppSettingService.cs`

```csharp
/// <summary>
/// Unified configuration service for managing and reading dynamic settings.
/// Handles CRUD operations, caching, encryption, validation, and type-safe access.
/// Combines management and consumption into a single cohesive service.
/// </summary>
[InjectService(Lifetime = MiCakeServiceLifetime.Scoped)]
public class AppSettingService
{
    private readonly IAppSettingRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly IDataProtectionService _dataProtectionService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppSettingService> _logger;
    
    // ==================== CRUD Operations ====================
    
    /// <summary>Creates a new setting with validation and encryption.</summary>
    Task<OperationResult<AppSettingDto>> CreateSettingAsync(
        CreateAppSettingDto dto, 
        CancellationToken ct = default);
        
    /// <summary>Updates an existing setting value with validation.</summary>
    Task<OperationResult> UpdateSettingAsync(
        UpdateAppSettingDto dto, 
        long? modifiedBy, 
        CancellationToken ct = default);
        
    /// <summary>Deletes a setting and invalidates cache.</summary>
    Task<OperationResult> DeleteSettingAsync(
        SettingGroup settingGroup, 
        string key, 
        CancellationToken ct = default);
    
    // ==================== Reading Operations (Cached) ====================
    
    /// <summary>
    /// Gets a single setting value with automatic decryption and type conversion.
    /// Uses cache-aside pattern for performance.
    /// </summary>
    Task<T?> GetSettingValueAsync<T>(
        SettingGroup settingGroup, 
        string key, 
        CancellationToken ct = default);
    
    /// <summary>
    /// Gets all settings in a group as a dictionary (decrypted).
    /// Cached at group level.
    /// </summary>
    Task<Dictionary<string, object>> GetGroupValuesAsync(
        SettingGroup settingGroup, 
        CancellationToken ct = default);
    
    /// <summary>
    /// Gets all settings in a group as a strongly-typed object (RECOMMENDED).
    /// Automatically maps setting keys to object properties.
    /// </summary>
    Task<TSettings> GetGroupAsObjectAsync<TSettings>(
        SettingGroup settingGroup, 
        CancellationToken ct = default) 
        where TSettings : class, new();
    
    /// <summary>Gets setting metadata (for admin UI).</summary>
    Task<OperationResult<AppSettingDto>> GetSettingAsync(
        SettingGroup settingGroup, 
        string key, 
        CancellationToken ct = default);
    
    /// <summary>Gets all settings in a group with metadata.</summary>
    Task<OperationResult<List<AppSettingDto>>> GetGroupSettingsAsync(
        SettingGroup settingGroup, 
        CancellationToken ct = default);
    
    // ==================== Cache Management ====================
    
    /// <summary>Invalidates cache for a specific setting group.</summary>
    Task InvalidateCacheAsync(
        SettingGroup settingGroup, 
        CancellationToken ct = default);
    
    // ==================== Private Helpers ====================
    
    private string PrepareValueForStorage(string value, bool shouldEncrypt)
    {
        return shouldEncrypt 
            ? _dataProtectionService.Protect(value) 
            : value;
    }
    
    private string GetDecryptedValue(AppSetting setting)
    {
        return setting.IsEncrypted 
            ? _dataProtectionService.Unprotect(setting.Value) 
            : setting.Value;
    }
    
    private bool ValidateValueAgainstPattern(string value, string? pattern)
    {
        if (string.IsNullOrEmpty(pattern)) return true;
        return Regex.IsMatch(value, pattern);
    }
    
    private string GetCacheKey(SettingGroup group, string key) 
        => string.Format(CacheKeys.AppSettingByKey, group, key);
    
    private string GetGroupCacheKey(SettingGroup group) 
        => string.Format(CacheKeys.AppSettingGroup, group);
}
```

**Service Responsibilities**:

1. **Management (CRUD)**:
   - Create/Update/Delete settings
   - Validation (pattern matching)
   - Encryption before storage
   - Cache invalidation on changes

2. **Consumption (Reading)**:
   - Type-safe value retrieval
   - Automatic decryption
   - Caching for performance
   - Strongly-typed group access

**Cache Keys**:
- Individual: `AppSetting:{SettingGroup}:{Key}` (e.g., `AppSetting:Email:SmtpServer`)
- Group: `AppSetting:Group:{SettingGroup}` (e.g., `AppSetting:Group:Email`)

**Service Flow**:
1. **Create**: Validate → Encrypt if needed → Create aggregate → Save → Invalidate cache
2. **Update**: Load → Validate → Encrypt if needed → Update → Save → Invalidate cache
3. **Read (cached)**: Check cache → If miss, load from DB → Decrypt → Cache → Return
4. **Read (metadata)**: Load from DB → Return DTO with metadata

---

### 3.4 Contracts Layer

**Location**: `StandardWeb.Contracts/Dtos/Configuration/`

```csharp
// AppSettingDto.cs
public record AppSettingDto(
    long Id,
    string SettingGroup,        // Enum as string for API compatibility
    string Key,
    string Value,               // Decrypted when read from API
    string DataType,            // Enum as string
    string? Description,
    bool IsEncrypted,
    string? ValidationPattern,  // NEW
    long? CreatedBy,
    DateTime CreatedAt,
    long? ModifiedBy,
    DateTime? UpdatedAt);

// CreateAppSettingDto.cs
public record CreateAppSettingDto(
    string SettingGroup,        // Parsed to SettingGroup enum
    string Key,
    string Value,               // Plain text - encrypted by service if needed
    string DataType,            // Parsed to SettingDataType enum
    string? Description,
    bool IsEncrypted = false,
    string? ValidationPattern = null);  // NEW

// UpdateAppSettingDto.cs
public record UpdateAppSettingDto(
    string SettingGroup,        // Parsed to enum
    string Key,
    string Value);              // Plain text
```

---

### 3.5 Web Layer

**Location**: `StandardWeb.Web/Controllers/Configuration/AppSettingsController.cs`

```csharp
/// <summary>
/// API controller for managing dynamic configuration settings.
/// Provides CRUD operations with authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]  // Restrict to admins
public class AppSettingsController : BaseApiController
{
    private readonly AppSettingService _service;
    
    public AppSettingsController(
        AppSettingService service,
        InfrastructureTools tools) : base(tools)
    {
        _service = service;
        ModuleCode = ModuleCodes.Configuration; // "05"
    }

    // GET /api/appsettings/email
    [HttpGet("{settingGroup}")]
    public async Task<IActionResult> GetGroupSettings(
        string settingGroup, 
        CancellationToken ct) { ... }
    
    // GET /api/appsettings/email/smtpserver
    [HttpGet("{settingGroup}/{key}")]
    public async Task<IActionResult> GetSetting(
        string settingGroup, 
        string key, 
        CancellationToken ct) { ... }
    
    // POST /api/appsettings
    [HttpPost]
    public async Task<IActionResult> CreateSetting(
        [FromBody] CreateAppSettingDto dto, 
        CancellationToken ct) { ... }
    
    // PUT /api/appsettings/email/smtpserver
    [HttpPut("{settingGroup}/{key}")]
    public async Task<IActionResult> UpdateSetting(
        string settingGroup, 
        string key, 
        [FromBody] UpdateAppSettingDto dto, 
        CancellationToken ct) { ... }
    
    // DELETE /api/appsettings/email/smtpserver
    [HttpDelete("{settingGroup}/{key}")]
    public async Task<IActionResult> DeleteSetting(
        string settingGroup, 
        string key, 
        CancellationToken ct) { ... }
}
```

**API Examples**:
- `GET /api/appsettings/email` → All Email settings
- `GET /api/appsettings/email/smtpserver` → Specific setting
- `PUT /api/appsettings/sms/apikey` → Update encrypted API key

---

## 4. Database Design

### 4.1 Table Schema

```sql
CREATE TABLE AppSettings (
    Id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    SettingGroup INT NOT NULL,                     -- Enum as int
    Key VARCHAR(100) NOT NULL,
    Value TEXT NOT NULL,                           -- Encrypted if IsEncrypted=true
    DataType INT NOT NULL,                         -- Enum as int
    Description VARCHAR(500),
    IsEncrypted BOOLEAN NOT NULL DEFAULT FALSE,
    ValidationPattern VARCHAR(500),                -- NEW: Regex pattern
    
    -- Audit fields (from AuditAggregateRoot)
    CreatedBy BIGINT,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedBy BIGINT,
    UpdatedAt TIMESTAMP,
    
    -- Constraints
    CONSTRAINT UQ_AppSettings_SettingGroup_Key UNIQUE (SettingGroup, Key),
    CONSTRAINT CK_AppSettings_SettingGroup CHECK (SettingGroup BETWEEN 1 AND 8),
    CONSTRAINT CK_AppSettings_DataType CHECK (DataType BETWEEN 1 AND 5)
);

-- Indexes
CREATE INDEX IX_AppSettings_SettingGroup ON AppSettings(SettingGroup);
CREATE INDEX IX_AppSettings_UpdatedAt ON AppSettings(UpdatedAt DESC);
```

---

### 4.2 EF Core Configuration

**Location**: `StandardWeb.Domain/AppDbContext.cs`

Add to `OnModelCreating`:

```csharp
#region Configuration Module

modelBuilder.Entity<AppSetting>(builder =>
{
    builder.ToTable("AppSettings");
    builder.HasKey(x => x.Id);
    
    // Unique constraint
    builder.HasIndex(x => new { x.SettingGroup, x.Key }).IsUnique();
    builder.HasIndex(x => x.SettingGroup);
    
    // Properties
    builder.Property(x => x.SettingGroup).IsRequired();
    builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
    builder.Property(x => x.Value).IsRequired();
    builder.Property(x => x.DataType).IsRequired();
    builder.Property(x => x.Description).HasMaxLength(500);
    builder.Property(x => x.IsEncrypted).IsRequired();
    builder.Property(x => x.ValidationPattern).HasMaxLength(500);  // NEW
    
    // Note: Audit fields auto-configured by MiCake
});

#endregion
```

---

## 5. Cache Configuration

### 5.1 AppSettings.json

```json
{
  "CacheSettings": {
    "AppSettingsCacheDurationMinutes": 30,
    "EnableDistributedCache": true
  }
}
```

### 5.2 Cache Keys

**Location**: `StandardWeb.Application/Constants/CacheKeys.cs`

```csharp
public static class CacheKeys
{
    // AppSetting cache keys
    public const string AppSettingByKey = "AppSetting:{0}:{1}";   // SettingGroup:Key
    public const string AppSettingGroup = "AppSetting:Group:{0}"; // SettingGroup
}
```

### 5.3 Invalidation Strategy

```csharp
private async Task InvalidateCacheForSettingAsync(SettingGroup settingGroup, string key)
{
    var settingKey = string.Format(CacheKeys.AppSettingByKey, settingGroup, key);
    var groupKey = string.Format(CacheKeys.AppSettingGroup, settingGroup);
    
    await _cacheService.RemoveAsync(settingKey);
    await _cacheService.RemoveAsync(groupKey);
}
```

---

## 6. Usage Examples

### 6.1 Reading Configuration (Type-Safe)

```csharp
using StandardWeb.Domain.Enums.Configuration;
using StandardWeb.Application.Services.Configuration;

public class EmailService
{
    private readonly AppSettingService _appSettings;
    
    public EmailService(AppSettingService appSettings)
    {
        _appSettings = appSettings;
    }
    
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // Option 1: Get individual settings (type-safe enum)
        var smtpServer = await _appSettings.GetSettingValueAsync<string>(
            SettingGroup.Email, "SmtpServer");
        var port = await _appSettings.GetSettingValueAsync<int>(
            SettingGroup.Email, "Port");
        var useSsl = await _appSettings.GetSettingValueAsync<bool>(
            SettingGroup.Email, "UseSsl");
        
        // Option 2: Get entire group as strongly-typed object (RECOMMENDED)
        var emailSettings = await _appSettings.GetGroupAsObjectAsync<EmailSettingsConfig>(
            SettingGroup.Email);
        
        // Values are automatically decrypted and cached
        await SendViaSmtp(emailSettings.SmtpServer, emailSettings.Port);
    }
}

// Strongly-typed configuration class
public class EmailSettingsConfig
{
    public string SmtpServer { get; set; } = null!;
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }  // Automatically decrypted
}
```

---

### 6.2 Creating Settings with Encryption & Validation

```csharp
using StandardWeb.Common.Security;
using StandardWeb.Domain.Enums.Configuration;

// In seed data or setup
var dataProtection = serviceProvider.GetRequiredService<IDataProtectionService>();

var emailSettings = new[]
{
    AppSetting.Create(
        SettingGroup.Email, 
        "SmtpServer", 
        "smtp.gmail.com", 
        SettingDataType.String,
        isEncrypted: false,
        description: "SMTP server address",
        validationPattern: @"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"),  // Domain pattern
    
    AppSetting.Create(
        SettingGroup.Email, 
        "Port", 
        "587", 
        SettingDataType.Integer,
        isEncrypted: false,
        description: "SMTP port",
        validationPattern: @"^(25|465|587|2525)$"),  // Valid SMTP ports
    
    AppSetting.Create(
        SettingGroup.Sms, 
        "ApiKey", 
        dataProtection.Protect("sensitive-api-key-12345"),  // Pre-encrypted!
        SettingDataType.String,
        isEncrypted: true,
        description: "SMS provider API key (encrypted)")
};

await context.AppSettings.AddRangeAsync(emailSettings);
await context.SaveChangesAsync();
```

---

### 6.3 API Usage

```http
### Get all email settings
GET /api/appsettings/email

### Get specific setting (returns decrypted value)
GET /api/appsettings/email/smtpserver

### Update setting (provide plain text, service encrypts if needed)
PUT /api/appsettings/email/port
Content-Type: application/json

{
  "settingGroup": "Email",
  "key": "Port",
  "value": "465"
}

### Create new encrypted setting
POST /api/appsettings
Content-Type: application/json

{
  "settingGroup": "Sms",
  "key": "ApiKey",
  "value": "my-secret-key",
  "dataType": "String",
  "isEncrypted": true,
  "description": "SMS API key",
  "validationPattern": "^[a-zA-Z0-9]{32,}$"
}
```

---

## 7. Error Codes

**Location**: `StandardWeb.Application/ErrorCodes/ConfigurationErrorCodes.cs`

```csharp
public static class ConfigurationErrorCodes
{
    private const string Prefix = "05"; // ModuleCodes.Configuration
    
    public const string SettingNotFound = Prefix + "01";
    public const string SettingAlreadyExists = Prefix + "02";
    public const string InvalidDataType = Prefix + "03";
    public const string InvalidValue = Prefix + "04";
    public const string GroupNotFound = Prefix + "05";
    public const string CannotDeleteRequiredSetting = Prefix + "06";
    public const string DecryptionFailed = Prefix + "07";
    public const string ValidationPatternMismatch = Prefix + "08";  // NEW
    public const string EncryptionFailed = Prefix + "09";            // NEW
}
```

**Location**: `StandardWeb.Web/Constants/ModuleCodes.cs`

```csharp
public static class ModuleCodes
{
    public const string Auth = "01";
    public const string User = "02";
    // ... existing
    public const string Configuration = "05";  // NEW
}
```

---

## 8. Security & Validation

### 8.1 Encryption ✅

**Implementation**: Common-layer `IDataProtectionService` (in `StandardWeb.Common/Security/`)

```csharp
// AppSettingService encrypts BEFORE creating aggregate
var encryptedApiKey = _dataProtectionService.Protect("sensitive-api-key");
var setting = AppSetting.Create(
    SettingGroup.Sms, 
    "ApiKey", 
    encryptedApiKey,  // Already encrypted!
    SettingDataType.String,
    isEncrypted: true);

// AppSettingService decrypts when reading
var apiKey = await _appSettings.GetSettingValueAsync<string>(
    SettingGroup.Sms, "ApiKey");
// Returns decrypted value automatically
```

**Key Points**:
- Encryption in Common layer (infrastructure service)
- Application service coordinates encryption/decryption
- Aggregate stores encrypted values only
- Uses ASP.NET Core Data Protection API
- Automatic decryption on read

---

### 8.2 Validation Pattern ✅

**Implementation**: Regex-based validation

```csharp
// Email format
validationPattern: @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"

// Port range
validationPattern: @"^(25|465|587|2525)$"

// URL
validationPattern: @"^https?://[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}.*$"

// Alphanumeric
validationPattern: @"^[a-zA-Z0-9]+$"
```

**Validation occurs**:
1. Before aggregate creation (in `AppSettingService`)
2. Before value update (in `AppSetting.UpdateValue`)
3. Returns `ValidationPatternMismatch` error if fails

---

### 8.3 Authorization

```csharp
[Authorize(Roles = "Admin")]  // On controller
public class AppSettingsController : BaseApiController
```

- Only admins can modify settings
- Consider separate read/write permissions
- API keys should have encrypted storage

---

### 8.4 Audit Trail

- Automatic via `AuditAggregateRoot`
- `CreatedBy`, `CreatedAt`, `ModifiedBy`, `UpdatedAt` auto-populated
- Domain event for external audit logging

---

## 9. Testing Strategy

### 9.1 Domain Tests
**Location**: `tests/StandardWeb.Domain.Tests/Models/Configuration/`
- Aggregate invariants
- Factory methods
- Value validation
- Pattern matching

### 9.2 Application Tests
**Location**: `tests/StandardWeb.Application.Tests/Services/Configuration/`
- Encryption/decryption
- Service operations
- Mock repository and cache
- Error scenarios

### 9.3 Integration Tests
**Location**: `tests/StandardWeb.Web.IntegrationTests/Controllers/Configuration/`
- API endpoints
- Cache behavior
- Database persistence
- End-to-end encryption

---

## 10. Implementation Roadmap

### Phase 1: Core Domain (Days 1-2)
1. Create `SettingGroup` enum
2. Create `SettingDataType` enum  
3. Create `AppSetting` aggregate with ValidationPattern
4. Create `SettingValueChangedEvent`
5. Define `IAppSettingRepository`
6. Implement repository
7. EF Core configuration
8. Database migration

### Phase 2: Common Layer - Security (Day 3)
9. `IDataProtectionService` interface in `StandardWeb.Common/Security/`
10. `DataProtectionService` implementation
11. Unit tests for encryption
12. Register in CommonModule

### Phase 3: Application Layer - Unified Service (Days 4-5)
13. `AppSettingService` with CRUD + cached reading
14. Implement caching logic (cache-aside pattern)
15. Implement encryption/decryption integration
16. Implement validation (regex pattern)
17. DTOs with ValidationPattern
18. Error codes
19. AutoMapper profiles

### Phase 4: API Layer (Day 6)
20. `AppSettingsController`
21. Authorization attributes
22. FluentValidation (if used)
23. OpenAPI documentation

### Phase 5: Configuration & Data (Day 7)
24. Cache configuration in appsettings
25. Seed data with patterns and encryption
26. Documentation

### Phase 6: Testing (Days 8-9)
27. Domain tests
28. Application tests (including unified service tests)
29. Integration tests
30. README updates

---

## 11. Architectural Decision Records

### ADR-001: Single Aggregate for All Settings

**Decision**: One `AppSetting` aggregate for all configuration items.

**Rationale**: 
- Settings are simple key-value pairs
- No complex cross-setting invariants
- Simpler repository and caching
- SettingGroup enum provides logical separation

**Consequences**: 
- (+) Simple, flexible
- (-) Cannot enforce cross-setting validation (acceptable)

---

### ADR-002: SettingGroup as Enum (Not String)

**Decision**: Use `SettingGroup` enum instead of free-form string.

**Rationale**: 
- Compile-time safety (no typos)
- IDE autocomplete and refactoring
- Controlled growth
- Self-documenting

**Consequences**: 
- (+) Type-safe, maintainable
- (+) Better IDE support
- (-) Adding group requires code change (desired)

---

### ADR-003: Inherit from AuditAggregateRoot

**Decision**: Inherit audit fields instead of defining custom ones.

**Rationale**: 
- DRY principle
- Consistent with other aggregates
- Framework auto-populates
- Type-safe user IDs (`long?`)

**Consequences**: 
- (+) Less code, automatic tracking
- (+) Consistent pattern

---

### ADR-004: String-Based Value Storage

**Decision**: Store all values as strings with DataType metadata.

**Rationale**:
- Maximum flexibility
- Supports JSON configurations
- Consistent schema
- Easy serialization

**Consequences**:
- (+) Flexible, no schema changes
- (-) Runtime parsing required

---

### ADR-005: Common-Layer Encryption (Not Domain/Application) ✨

**Decision**: Place `IDataProtectionService` in `StandardWeb.Common/Security/`, NOT in Domain or Application Configuration module.

**Rationale**: 
- **Reusability**: Encryption is needed across multiple modules (Configuration, User, Auth, etc.)
- **Domain Purity**: No infrastructure dependencies in domain
- **Separation of Concerns**: Encryption is infrastructure, not business logic
- **Single Location**: One encryption service for entire application
- **Testability**: Easy to mock in all layers

**Consequences**: 
- (+) Reusable across all modules
- (+) Clean separation of concerns
- (+) Easy to swap encryption implementation
- (+) Common infrastructure pattern
- (-) Requires Common layer dependency in Application

**Implementation**:
```csharp
// In AppSettingService (Application layer)
private string PrepareValueForStorage(string value, bool shouldEncrypt)
{
    return shouldEncrypt 
        ? _dataProtectionService.Protect(value, "AppSettings") 
        : value;
}

private string GetDecryptedValue(AppSetting setting)
{
    return setting.IsEncrypted 
        ? _dataProtectionService.Unprotect(setting.Value, "AppSettings") 
        : setting.Value;
}

// Can also be used in other modules
// UserService: _dataProtectionService.Protect(ssn, "UserData");
// AuthService: _dataProtectionService.Protect(token, "AuthTokens");
```

---

### ADR-006: Regex Validation Pattern Support ✨

**Decision**: Add optional `ValidationPattern` field for value validation.

**Rationale**: 
- Data integrity (prevent invalid formats)
- Early validation (before storage)
- Flexible (per-setting rules)
- Self-documenting (pattern shows format)

**Consequences**: 
- (+) Stronger validation
- (+) Clear feedback on errors
- (-) Requires regex knowledge

---

### ADR-007: Unified Service (No Separate Provider) ✨

**Decision**: Use single `AppSettingService` for both management (CRUD) and consumption (reading), instead of separate `AppSettingService` + `ConfigurationProvider`.

**Rationale**:
- **Simplicity**: One service to inject, not two
- **Cohesion**: Related operations in one place
- **Reduced Redundancy**: No duplicate caching/decryption logic
- **Clear API**: Single point of entry for all configuration needs
- **Easier Testing**: Mock one service instead of two

**Consequences**:
- (+) Simpler dependency injection
- (+) Less code duplication
- (+) Easier to maintain
- (+) Clear responsibility boundary
- (-) Service has more methods (acceptable - well organized)

**API Design**:
```csharp
// For business services (reading configuration)
await _appSettings.GetSettingValueAsync<string>(SettingGroup.Email, "SmtpServer");
await _appSettings.GetGroupAsObjectAsync<EmailConfig>(SettingGroup.Email);

// For admin UI (managing configuration)
await _appSettings.CreateSettingAsync(dto);
await _appSettings.UpdateSettingAsync(dto, userId);
await _appSettings.GetGroupSettingsAsync(SettingGroup.Email); // With metadata
```

---

### ADR-008: Cache-Aside Pattern

**Decision**: Use cache-aside (lazy loading) within `AppSettingService`.

**Rationale**:
- Read-heavy workload
- Simpler implementation
- Easy invalidation
- Leverages existing `ICacheService`

**Consequences**:
- (+) Simple, predictable
- (-) Cache stampede risk (acceptable)

---

## 12. Remaining Design Questions

### ✅ Resolved
1. ~~Encryption Implementation~~ → **DECIDED**: `IDataProtectionService` in Common layer (`StandardWeb.Common/Security/`)
2. ~~Validation Rules~~ → **DECIDED**: Regex `ValidationPattern` field
3. ~~Service Architecture~~ → **DECIDED**: Unified `AppSettingService` (no separate Provider)

### 🤔 For Future Consideration

3. **Versioning**: Maintain change history?
   - Option A: `SettingHistory` table
   - Option B: Domain events for audit
   - Option C: Audit fields sufficient

4. **Multi-Tenancy**: Tenant-specific settings?
   - Add `TenantId` field if needed
   - Default: Global settings

5. **UI Requirements**: Admin interface?
   - Web panel, config file, or API-only

6. **Notifications**: Alert on critical changes?
   - Domain event handler → notifications
   - Define "critical" settings

---

## 13. Next Steps

### ✅ Design Complete - Ready for Implementation

This design is now complete with:
- ✅ Enum-based type safety (SettingGroup, SettingDataType)
- ✅ Audit field inheritance (AuditAggregateRoot)
- ✅ Common-layer encryption (IDataProtectionService in StandardWeb.Common/Security/)
- ✅ Regex validation support (ValidationPattern field)
- ✅ Unified service architecture (AppSettingService handles CRUD + cached reading)
- ✅ Comprehensive caching strategy (cache-aside pattern)

### After Your Approval

I can provide:
1. **Implementation tasks** for Developer agent
2. **Code templates** for aggregate and services
3. **Migration scripts** for database
4. **Integration points** documentation

**Are you ready to proceed with implementation, or would you like to discuss any aspect further?**

---

*This is an architectural design document. Implementation by Developer agent will follow approval.*
