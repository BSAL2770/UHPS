namespace UHPS.API.Entities;

public class User : BaseEntity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Customer? Customer { get; set; }
    public Employee? Employee { get; set; }
}
