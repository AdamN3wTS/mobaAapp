using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using api.Models;

namespace api.Client;

public class RiotClient(HttpClient httpClient) : IRiotClient
{
    
    
    public async Task<ResponseRiotClientDto<T?>> GetAsync<T>(string url, CancellationToken ct)
    {
        string apikey = Environment.GetEnvironmentVariable("RIOT_API_KEY")!;
        httpClient.DefaultRequestHeaders.Add("X-Riot-Token",apikey);
   
        var response = await httpClient.GetAsync(url,ct);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Console.WriteLine(response.StatusCode);
            var responseDtoNotValid = new ResponseRiotClientDto<T?>()
            {
                StatusCode=response.StatusCode,
            };
            return responseDtoNotValid;
        }
        var responseBody = await response.Content.ReadFromJsonAsync<T>();
        var responseDto = new ResponseRiotClientDto<T?>()
        {
            StatusCode=response.StatusCode,
            Data=responseBody
        };
        return responseDto;
    }
}
