using System;
using System.Net;

namespace api.Models;

public class ResponseDto<T>
{
    public T? Data {get;set;}
    public bool Success {get;set;}
    public string Message {get;set;} = default!;
    public HttpStatusCode StatusCode {get;set;}
}
public class ResponseRiotClientDto<T>
{
    public HttpStatusCode StatusCode {get;set;}
    public T? Data {get;set;}
}
