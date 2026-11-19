using System;
using api.DbModels;

namespace api.IDbServices;

public interface IMatchDbService 
{
    public  Task<bool> IsMatchExistByMatchIdAsync(string matchId,CancellationToken ct);
    public  Task<bool> AddMatchAsync(Match match, CancellationToken ct);
    public  Task<Match?> GetLatestMatch(CancellationToken ct);
    public  Task<HashSet<string>> GetExistingMatchIdsAsync(List<string> ids, CancellationToken ct);
    public  Task DeleteMatchesBeforePatchAsync(string patch,CancellationToken ct);
    public  Task<long> GetFirstMatchInCurrnetPatch(string riotPatch);
    public  Task<int> GetMatchesCountAsync(CancellationToken ct);



}
