using System;
using api.Client;
using api.IServices;
using api.Models;
using api.Models.Account;

namespace api.Services;

public class AccountInfoTest : IAccountInfoTest
{
    private readonly IRiotClient _riotClient;
    public AccountInfoTest(IRiotClient riotClient)
    {
        _riotClient=riotClient;
    }
    public async Task<ResponseDto<RiotAccount?>> GetRiotAccountAsync(string name,string tag,CancellationToken ct)
    {
        try
        {
            var clientResponse = await _riotClient.GetAsync<RiotAccount?>($"https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{name}/{tag}",ct);
            var response =  new ResponseDto<RiotAccount?>()
            {
                StatusCode=clientResponse.StatusCode,
                Data=clientResponse.Data,
                Success=clientResponse.StatusCode==System.Net.HttpStatusCode.OK,
                Message="FAJNIE:)"
            };
            return response;
        }
        catch(Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
