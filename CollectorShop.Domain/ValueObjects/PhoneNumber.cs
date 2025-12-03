using System.Text.RegularExpressions;
using CollectorShop.Domain.Common;

namespace CollectorShop.Domain.ValueObjects;

public partial class PhoneNumber : ValueObject
{
    public string Value { get; private set; }

    private PhoneNumber()
    {
        Value = null!;
    }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty", nameof(value));

        var cleanedNumber = PhoneCleanupRegex().Replace(value, "");

        if (cleanedNumber.Length < 10 || cleanedNumber.Length > 15)
            throw new ArgumentException("Invalid phone number format", nameof(value));

        Value = cleanedNumber;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;

    [GeneratedRegex(@"[^\d+]", RegexOptions.Compiled)]
    private static partial Regex PhoneCleanupRegex();
}
