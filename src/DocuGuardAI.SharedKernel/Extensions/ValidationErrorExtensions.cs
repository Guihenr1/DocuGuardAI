using Ardalis.Result;
using FluentValidation;
using FluentValidation.Results;

namespace DocuGuardAI.SharedKernel.Extensions;

public static class ValidationErrorExtensions
{
    public static List<ValidationError> ToValidationErrorList(this IEnumerable<ValidationFailure> failures)
    {
        return failures.Select(f => new ValidationError
        {
            Identifier = f.PropertyName,
            ErrorMessage = f.ErrorMessage,
            Severity = f.Severity.ToResultSeverity()
        }).ToList();
    }

    private static ValidationSeverity ToResultSeverity(this Severity severity) => severity switch
    {
        Severity.Error => ValidationSeverity.Error,
        Severity.Warning => ValidationSeverity.Warning,
        Severity.Info => ValidationSeverity.Info,
        _ => ValidationSeverity.Error
    };
}