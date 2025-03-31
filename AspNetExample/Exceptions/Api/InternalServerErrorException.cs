﻿using System.Net;

namespace AspNetExample.Exceptions.Api;

public class InternalServerErrorException : ApiExceptionBase
{
    public override HttpStatusCode Code => HttpStatusCode.InternalServerError;

    public InternalServerErrorException(string? message = null, Exception? exception = null)
        : base(message, exception)
    {
    }
}