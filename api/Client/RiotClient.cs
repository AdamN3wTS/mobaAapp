using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace api.Client;

public class RiotClient(HttpClient httpClient) : IRiotClient
{
    
    
    public async Task<T?> GetAsync<T>(string url, CancellationToken ct)
    {
        string apikey = Environment.GetEnvironmentVariable("RIOT_API_KEY")!;
        httpClient.DefaultRequestHeaders.Add("X-Riot-Token",apikey);
   
        var response = await httpClient.GetAsync(url,ct);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            // trzeba wykminic czy jaksi ResponseDto zwracamy mozliwe warianty brak/blad/zly klucz/rate limmit
            throw new Exception($"statuscode: {response.StatusCode}");
        }
        var responseBody = await response.Content.ReadFromJsonAsync<T>();
        return responseBody;
    }
}
