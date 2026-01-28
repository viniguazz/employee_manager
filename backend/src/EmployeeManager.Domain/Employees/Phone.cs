namespace EmployeeManager.Domain.Employees;

public sealed class Phone
{
    public string Number { get; }
    public string? Type { get; }

    public Phone(string number, string? type = null)
    {
        number = (number ?? "").Trim();
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Phone number is required.");

        Number = number;
        Type = string.IsNullOrWhiteSpace(type) ? null : type.Trim();
    }
}