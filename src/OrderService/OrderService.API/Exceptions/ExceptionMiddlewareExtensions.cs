using System;
using Microsoft.AspNetCore.Diagnostics;

namespace OrderService.API.Exceptions;

public static class ExceptionMiddlewareExtensions
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                if (contextFeature != null)
                {
                    var exception = contextFeature.Error;

                    // Validation exception return 400
                    if (exception is FluentValidation.ValidationException validationException)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        var errors = validationException.Errors
                            .Select(e => new { e.PropertyName, e.ErrorMessage });
                        await context.Response.WriteAsJsonAsync(errors);
                    }
                    else
                    {
                        // Return 500
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            Message = "Internal Server Error",
                            Detail = exception.Message
                        });
                    }
                }
            });
        });
    }
}
