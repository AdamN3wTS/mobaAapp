using System;
using api.DbModels;

namespace api.IDbServices;

public interface IMatchDbService 
{
    public  Task<bool> IsMatchExistByMatchIdAsync(string matchId,CancellationToken ct);
    public  Task<bool> AddMatchAsync(Match match, CancellationToken ct);

}
