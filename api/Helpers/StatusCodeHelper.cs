using System;
using System.Net;
using api.Models;
using Microsoft.AspNetCore.Mvc;

namespace api.Helpers;

public static class StatusCodeHelper
{
    public static IActionResult SafeResponse<T>(ControllerBase c,ResponseDto<T?> serviceResponse)
    {
        switch (serviceResponse.StatusCode)
            {
                case (HttpStatusCode.OK):
                    return c.Ok(serviceResponse);
                case (HttpStatusCode.Unauthorized):
                    return c.Unauthorized(serviceResponse);
                case (HttpStatusCode.Forbidden):
                    return c.Forbid();
                case (HttpStatusCode.NotFound):
                    return c.NotFound(serviceResponse);
                case (HttpStatusCode.TooManyRequests):
                    return c.StatusCode(429,serviceResponse);
                case (HttpStatusCode.InternalServerError):
                    return c.StatusCode(500,serviceResponse);
                case (HttpStatusCode.BadGateway):
                    return c.StatusCode(502,serviceResponse);
                case (HttpStatusCode.ServiceUnavailable):
                    return c.StatusCode(503,serviceResponse);
                case (HttpStatusCode.GatewayTimeout):
                    return c.StatusCode(504,serviceResponse);
                default:
                    return c.NoContent();
            }
    }
}

