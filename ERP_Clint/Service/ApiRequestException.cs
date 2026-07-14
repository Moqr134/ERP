using System.Net;

namespace ERP_Clint.Service;

public class ApiRequestException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public ApiRequestException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
