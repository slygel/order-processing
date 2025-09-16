using FluentValidation;
using MediatR;
using ProductService.Application.Common;

namespace ProductService.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                var errorMessages = failures
                    .Select(failure => failure.ErrorMessage)
                    .Distinct()
                    .ToList();

                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Results<>))
                {
                    var errorMessage = string.Join(", ", errorMessages);
                    var resultType = typeof(TResponse).GetGenericArguments()[0];
                    var result = typeof(Results<>)
                        .MakeGenericType(resultType)
                        .GetMethod("Failure", new[] { typeof(string), typeof(int) })
                        ?.Invoke(null, new object[] { errorMessage, 400 });

                    return result as TResponse ?? throw new InvalidOperationException("Could not create failure result");
                }

            }
        }

        return await next(cancellationToken);
    }
}

