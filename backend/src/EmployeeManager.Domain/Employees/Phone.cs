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

        if (!IsDigits(number) || number.Length != 9)
            throw new ArgumentException("Phone number must have exactly 9 digits.");

        Number = number;
        Type = string.IsNullOrWhiteSpace(type) ? null : type.Trim();
    }

    private static bool IsDigits(string value)
        => value.All(char.IsDigit);
}