using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TT.Internet.Framework.Infrastructure.Filters
{
    public class ValidationFilter<T> : IActionFilter where T : class
    {
        private readonly IValidator<T> _validator;
        private readonly ILogger<ValidationFilter<T>> _logger;

        public ValidationFilter(IValidator<T> validator, ILogger<ValidationFilter<T>> logger)
        {
            _validator = validator;
            _logger = logger;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
       
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Find the argument of type T from the action's arguments
            var argument = context.ActionArguments.Values.FirstOrDefault(v => v is T) as T;

            if (argument == null)
            {
                _logger.LogError("Unable to find parameters or body for validation");
                context.Result = new BadRequestObjectResult("Unable to find parameters or body for validation");
                return;
            }

            // Validate the argument
            var validationResult = _validator.Validate(argument);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed: {ValidationErrors}", validationResult.Errors);
                context.Result = new BadRequestObjectResult(validationResult.ToDictionary());
            }
        }
    }
}
