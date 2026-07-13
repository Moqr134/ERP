using ERP_API.Infrastructure.Services;
using Infrastructure.AppException;
using Infrastructure.Logger;
using Newtonsoft.Json;
using System.Net;
using Validation;

namespace Infrastructure.Middleware;

public class ErrorHandler
{
    private readonly RequestDelegate _next;
    private readonly IAppLogger _logger;
    private readonly IHostEnvironment _environment;

    public ErrorHandler(RequestDelegate next, IAppLogger logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            HttpResponse response = context.Response;
            response.ContentType = "application/json";

            object objectResult;

            switch (exception)
            {
                case LogicException:
                    response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                    objectResult = new { exception.Message };
                    break;

                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    objectResult = new { Message = $"{exception.Message} {ErrorCode.KeyNotFound}" };
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    objectResult = new { exception.Message };
                    break;

                case DuplicateException:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    objectResult = new { exception.Message };
                    break;

                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    objectResult = new { exception.Message };
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    objectResult = _environment.IsDevelopment()
                        ? new
                        {
                            exception.Message,
                            InnerMessage = exception.InnerException?.Message,
                            exception.Data,
                            exception.Source,
                            exception.HelpLink,
                            exception.HResult
                        }
                        : new { Message = "حدث خطأ داخلي في الخادم" };
                    break;
            }

            await _logger.WriteAsync(exception, context, objectResult);
            await response.WriteAsync(JsonConvert.SerializeObject(objectResult));
        }
    }
}
