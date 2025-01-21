using System.Net;
using JoyGame.CaseStudy.API.Extensions;
using JoyGame.CaseStudy.API.Models;
using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.Exceptions;

namespace JoyGame.CaseStudy.API.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;
    private readonly IHostEnvironment _environment = environment;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An unhandled exception occurred while processing {Path}. Method: {Method}",
                context.Request.Path,
                context.Request.Method);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            EntityNotFoundException ex => OperationResult<object>.Failure(ErrorCode.EntityNotFound, ex.Message)
                .ToApiResponse(),
            BusinessRuleException ex => OperationResult<object>.Failure(ErrorCode.BusinessRuleViolation, ex.Message)
                .ToApiResponse(),
            _ => CreateUnexpectedErrorResponse(exception)
        };

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await context.Response.WriteAsJsonAsync(response);
    }

    private ApiResponse<object> CreateUnexpectedErrorResponse(Exception exception)
    {
        var response = OperationResult<object>.Failure(ErrorCode.InternalServerError,
            _environment.IsDevelopment() ? exception.ToString() : "An unexpected error occurred").ToApiResponse();

        return response;
    }
}