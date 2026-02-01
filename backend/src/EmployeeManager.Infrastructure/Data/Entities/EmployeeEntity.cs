using EmployeeManager.Domain.Roles;

namespace EmployeeManager.Infrastructure.Data.Entities;

public sealed class EmployeeEntity
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string DocNumber { get; set; } = default!;

    public DateOnly BirthDate { get; set; }
    public Role Role { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? DeactivatedAt { get; set; }

    public Guid? ManagerEmployeeId { get; set; }

    public string PasswordHash { get; set; } = default!;

    public List<EmployeePhoneEntity> Phones { get; set; } = new();
}