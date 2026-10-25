using System.Net;

namespace api.Models
{
    public class ResponseDto<T>
    {
        public bool IsSuccess { get; set; } = true;
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
    public class ResponseRiotDto<T>
    {
        public HttpStatusCode StatusCode {get;set;}
        public T? Data {get;set;}
    }
}