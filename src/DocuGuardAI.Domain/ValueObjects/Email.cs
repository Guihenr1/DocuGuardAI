using DocuGuardAI.SharedKernel.BaseClasses;
using DocuGuardAI.SharedKernel.Exceptions;

namespace DocuGuardAI.Domain.ValueObjects;

public sealed record Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email From(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty");

        if (!email.Contains('@') || !email.Contains('.'))
            throw new DomainException("Invalid email format");

        var normalized = email.Trim().ToLowerInvariant();

        try
        {
            _ = new System.Net.Mail.MailAddress(normalized);
        }
        catch
        {
            throw new DomainException($"Email '{normalized}' is not valid");
        }

        return new Email(normalized);
    }

    public static implicit operator string(Email email) => email.Value;

    private Email() => Value = null!;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}