namespace Entity.SystemMod;

[Index(nameof(RoleId), nameof(MenuId), IsUnique = true)]
public class SystemMenuRole : EntityBase
{
    public Guid MenuId { get; set; }
    public Guid RoleId { get; set; }

    public SystemMenu SystemMenu { get; set; } = default!;
    public SystemRole SystemRole { get; set; } = default!;
}
