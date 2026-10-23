using System;
using api.DbModels;

namespace api.IDbServices;

public interface ICrawlPlayerDbService
{
    public  Task<List<CrawlPlayer>> GetPlayersAsync(CancellationToken ct);

    public Task<bool> AddListPlayersAsync(List<CrawlPlayer> crawlPlayers,CancellationToken ct);

    public Task<List<string>> GetExistingPuuidListAsync(List<string> puuids,CancellationToken ct);
    public Task<bool> SetCrawlTimeByPuuidAsync(string puuid,CancellationToken ct);

    public Task<CrawlPlayer?> IsPlayerExistByPuuidAsync(string puuid,CancellationToken ct);
    public  Task<bool> AddPlayerAsync(CrawlPlayer crawlPlayer,CancellationToken ct);
    public Task<List<CrawlPlayer>> GetActivePlayersToInsertMatchesAsync(CancellationToken ct);
    public Task<int> HowManyPlayersWhoDoesntCrowledOrCrowledLogTimeAgoAsync(CancellationToken ct);



}
