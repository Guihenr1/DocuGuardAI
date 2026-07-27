using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;

namespace DocuGuardAI.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        return result.Status switch
        {
            ResultStatus.Ok => controller.Ok(result.Value),

            ResultStatus.Invalid => controller.ValidationProblem(
                new ValidationProblemDetails(
                    result.ValidationErrors
                        .GroupBy(e => e.Identifier)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()))
                {
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest
                }),

            ResultStatus.NotFound => controller.NotFound(new ProblemDetails
            {
                Title = "Resource not found",
                Status = StatusCodes.Status404NotFound,
                Detail = string.Join("; ", result.Errors)
            }),

            ResultStatus.Conflict => controller.Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Status = StatusCodes.Status409Conflict,
                Detail = string.Join("; ", result.Errors)
            }),

            ResultStatus.Unauthorized => controller.Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = string.Join("; ", result.Errors)
            }),

            ResultStatus.Forbidden => controller.StatusCode(
                StatusCodes.Status403Forbidden,
                new ProblemDetails
                {
                    Title = "Forbidden",
                    Status = StatusCodes.Status403Forbidden,
                    Detail = string.Join("; ", result.Errors)
                }),

            _ => controller.BadRequest(new ProblemDetails
            {
                Title = "Bad request",
                Status = StatusCodes.Status400BadRequest,
                Detail = string.Join("; ", result.Errors)
            })
        };
    }
}