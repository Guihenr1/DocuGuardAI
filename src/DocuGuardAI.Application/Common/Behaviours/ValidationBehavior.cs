using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace DocuGuardAI.Application.Common.Behaviours;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            var validationErrors = failures.Select(f => 
                new ValidationError
                {
                    Identifier = f.PropertyName,
                    ErrorMessage = f.ErrorMessage,
                    Severity = f.Severity switch
                    {
                        Severity.Error => ValidationSeverity.Error,
                        Severity.Warning => ValidationSeverity.Warning,
                        Severity.Info => ValidationSeverity.Info,
                        _ => ValidationSeverity.Error
                    }
                }).ToList();
            
            var invalidMethod = typeof(Result<>)
                .MakeGenericType(typeof(TResponse).GenericTypeArguments[0])
                .GetMethod(nameof(Result.Invalid), new[] { typeof(List<ValidationError>) });

            var invalidResult = invalidMethod!.Invoke(null, new object[] { validationErrors });

            return (TResponse)invalidResult!;
        }

        return await next();
    }
}