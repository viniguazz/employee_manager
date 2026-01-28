namespace EmployeeManager.Infrastructure.Data.Entities;

public sealed class EmployeePhoneEntity
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public EmployeeEntity Employee { get; set; } = default!;

    public string Number { get; set; } = default!;
    public string? Type { get; set; }
}