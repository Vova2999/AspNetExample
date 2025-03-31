using System.Net;

namespace AspNetExample.Exceptions.Api;

public class UnauthorizedException : ApiExceptionBase
{
    public override HttpStatusCode Code => HttpStatusCode.Unauthorized;

    public UnauthorizedException(string? message = null, Exception? exception = null)
        : base(message, exception)
    {
    }
}