using AutoMapper;
using RBACWeb.Application.ErrorCodes;
using RBACWeb.Contracts.Dtos.Authorization;
using RBACWeb.Domain.Models.Authorization;
using RBACWeb.Domain.Repositories;
using DomainDataScopeType = RBACWeb.Domain.Enums.Authorization.DataScopeType;

namespace RBACWeb.Application.Services.Authorization;

/// <summary>
/// Service for managing data scopes.
/// </summary>
[InjectService(Lifetime = MiCakeServiceLifetime.Scoped)]
public class DataScopeService
{
    private readonly IDataScopeRepo _dataScopeRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<DataScopeService> _logger;

    public DataScopeService(
        IDataScopeRepo dataScopeRepo,
        IMapper mapper,
        ILogger<DataScopeService> logger)
    {
        _dataScopeRepo = dataScopeRepo;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active data scopes.
    /// </summary>
    public async Task<OperationResult<IReadOnlyList<DataScopeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var dataScopes = await _dataScopeRepo.GetActiveDataScopesAsync(cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<DataScopeDto>>(dataScopes);
        return OperationResult<IReadOnlyList<DataScopeDto>>.Success(dtos);
    }

    /// <summary>
    /// Gets a data scope by ID.
    /// </summary>
    public async Task<OperationResult<DataScopeDto?>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var dataScope = await _dataScopeRepo.FindAsync(id, cancellationToken);
        if (dataScope is null)
            return OperationResult<DataScopeDto?>.Failure("Data scope not found", AuthorizationErrorCodes.DataScopeNotFound);

        var dto = _mapper.Map<DataScopeDto>(dataScope);
        return OperationResult<DataScopeDto?>.Success(dto);
    }

    /// <summary>
    /// Creates a new data scope.
    /// </summary>
    public async Task<OperationResult<DataScopeDto?>> CreateAsync(CreateDataScopeDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating data scope with code: {Code}", dto.Code);

        // Check for duplicate code
        if (await _dataScopeRepo.ExistsByCodeAsync(dto.Code, cancellationToken))
            return OperationResult<DataScopeDto?>.Failure("Data scope with this code already exists", AuthorizationErrorCodes.DataScopeAlreadyExists);

        var dataScope = DataScope.Create(
            dto.Code,
            dto.Name,
            (DomainDataScopeType)dto.Type,
            dto.Description,
            dto.Condition,
            dto.Priority);

        await _dataScopeRepo.AddAsync(dataScope, cancellationToken);
        await _dataScopeRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Data scope {DataScopeId} created successfully", dataScope.Id);
        return OperationResult<DataScopeDto?>.Success(_mapper.Map<DataScopeDto>(dataScope));
    }

    /// <summary>
    /// Updates a data scope.
    /// </summary>
    public async Task<OperationResult<DataScopeDto?>> UpdateAsync(long id, UpdateDataScopeDto dto, CancellationToken cancellationToken = default)
    {
        var dataScope = await _dataScopeRepo.FindAsync(id, cancellationToken);
        if (dataScope is null)
            return OperationResult<DataScopeDto?>.Failure("Data scope not found", AuthorizationErrorCodes.DataScopeNotFound);

        dataScope.Update(dto.Name, dto.Description, dto.Condition, dto.Priority);
        await _dataScopeRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Data scope {DataScopeId} updated successfully", id);
        return OperationResult<DataScopeDto?>.Success(_mapper.Map<DataScopeDto>(dataScope));
    }

    /// <summary>
    /// Deactivates a data scope.
    /// </summary>
    public async Task<OperationResult> DeactivateAsync(long id, CancellationToken cancellationToken = default)
    {
        var dataScope = await _dataScopeRepo.FindAsync(id, cancellationToken);
        if (dataScope is null)
            return OperationResult.Failure("Data scope not found", AuthorizationErrorCodes.DataScopeNotFound);

        dataScope.Deactivate();
        await _dataScopeRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Data scope {DataScopeId} deactivated successfully", id);
        return OperationResult.Success();
    }
}
