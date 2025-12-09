namespace Perigon.AspNetCore.Abstraction;

/// <summary>
/// entity base interface
/// </summary>
public interface IEntityBase
{
    Guid Id { get; set; }
    DateTimeOffset CreatedTime { get; }
    DateTimeOffset UpdatedTime { get; set; }
    bool IsDeleted { get; set; }
}

/// <summary>
/// entity base interface with tenant support
/// </summary>
public interface ITenantEntityBase : IEntityBase
{
    Guid TenantId { get; set; }
}