namespace Perigon.AspNetCore.Abstraction;

/// <summary>
/// entity base interface with tenant support
/// </summary>
public interface IEntityBase
{
    Guid Id { get; set; }
    DateTimeOffset CreatedTime { get; }
    DateTimeOffset UpdatedTime { get; set; }
    bool IsDeleted { get; set; }
    Guid TenantId { get; set; }
}
