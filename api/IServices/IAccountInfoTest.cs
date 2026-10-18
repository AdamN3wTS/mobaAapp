using System;
using api.Models;
using api.Models.Account;

namespace api.IServices;

public interface IAccountInfoTest
{
    public Task<ResponseDto<RiotAccount?>> GetRiotAccountAsync(string name,string tag,CancellationToken ct);

}
