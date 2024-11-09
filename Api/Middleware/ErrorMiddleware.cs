using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Api.Middleware;

public class ErrorMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);

            if (context.Response.StatusCode == (int)HttpStatusCode.BadRequest && !context.Response.HasStarted)
            {
                await HandleValidationErrorResponseAsync(context);
            }
        }
        catch (Exception ex)
        {
            if (!context.Response.HasStarted)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        if (context.Response.StatusCode >= 400 && context.Response.StatusCode != (int)HttpStatusCode.BadRequest && !context.Response.HasStarted)
        {
            await HandleErrorResponseAsync(context);
        }
    }

    private static async Task HandleValidationErrorResponseAsync(HttpContext context)
    {
        var body = await GetResponseBodyAsync(context.Response);
        if (string.IsNullOrEmpty(body))
            return;

        var validationProblem = JsonSerializer.Deserialize<ValidationProblemDetails>(body);

        var result = JsonSerializer.Serialize(new
        {
            error = validationProblem?.Title ?? "Validation Error",
            details = validationProblem?.Errors,
            status = context.Response.StatusCode,
            traceId = context.TraceIdentifier
        });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        await context.Response.WriteAsync(result);
    }

    private static Task HandleErrorResponseAsync(HttpContext context)
    {
        int statusCode = context.Response.StatusCode;
        string message = GetDefaultMessageForStatusCode(statusCode);

        var result = JsonSerializer.Serialize(new { error = message, status = statusCode });

        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(result);
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        int statusCode;
        string message;

        switch (exception)
        {
            case ArgumentException _:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = exception.Message;
                break;
            case KeyNotFoundException _:
                statusCode = (int)HttpStatusCode.NotFound;
                message = exception.Message;
                break;
            case UnauthorizedAccessException _:
                statusCode = (int)HttpStatusCode.Unauthorized;
                message = exception.Message;
                break;
            case InvalidOperationException _:
                statusCode = (int)HttpStatusCode.Conflict;
                message = exception.Message;
                break;
            case NotSupportedException _:
                statusCode = (int)HttpStatusCode.UnsupportedMediaType;
                message = exception.Message;
                break;
            case TimeoutException _:
                statusCode = (int)HttpStatusCode.RequestTimeout;
                message = exception.Message;
                break;
            case ValidationException _:
                statusCode = (int)HttpStatusCode.UnprocessableEntity;
                message = exception.Message;
                break;
            case AccessViolationException _:
                statusCode = (int)HttpStatusCode.Forbidden;
                message = "Access violation.";
                break;
            case NotImplementedException _:
                statusCode = (int)HttpStatusCode.NotImplemented;
                message = exception.Message;
                break;
            case NullReferenceException _:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "A null reference occurred.";
                break;
            case DivideByZeroException _:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "A division by zero occurred.";
                break;
            case FormatException _:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = exception.Message;
                break;
            case SqlException _:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "A database error occurred.";
                break;
            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred";
                break;
        }

        var result = JsonSerializer.Serialize(new { error = message, status = statusCode });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsync(result);
    }

    private static string GetDefaultMessageForStatusCode(int statusCode)
    {
        var statusDescription = ((HttpStatusCode)statusCode).ToString().Replace("_", " ");
        return statusDescription;
    }

    private static async Task<string> GetResponseBodyAsync(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        string body = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);
        return body;
    }
}
