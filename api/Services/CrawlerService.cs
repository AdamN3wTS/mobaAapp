using System;
using api.Client;
using api.DbModels;
using api.Helpers;
using api.IDbServices;
using api.IServices;

namespace api.Services;

public class CrawlerService : ICrawlerService
{
    private readonly ILogger<CrawlerService> _logger;

    private readonly IRiotClient _riotClient;
    private readonly ICrawlPlayerDbService _crawlPlayerDbService;
    private readonly IMatchDbService _matchDbService;

    public CrawlerService(IMatchDbService matchDbService,ILogger<CrawlerService> logger,IRiotClient riotClient,ICrawlPlayerDbService crawlPlayerDbService)
    {
        _logger=logger;
        _riotClient=riotClient;
        
        _crawlPlayerDbService=crawlPlayerDbService;
        _matchDbService=matchDbService;
    }
    public async Task<List<CrawlPlayer>> GetPlayerList(string region,string tier,string queue,string division,int page,CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Statring GetPlayerList");
            _logger.LogInformation($"Statring _riotClient.GetSummonersByRegionQueueTierDivisionAsync with params region:{region},tier:{tier},queue:{queue},division:{division},page:{page}");

            var clientResponse = await _riotClient.GetSummonersByRegionQueueTierDivisionAsync(region,queue,tier,division,page,true);
            _logger.LogInformation($"_riotClient.GetSummonersByRegionQueueTierDivisionAsync StatusCode: {clientResponse.StatusCode}");

            if (clientResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return new List<CrawlPlayer>();
            }
            _logger.LogInformation($"Zebrałem {clientResponse.Data.Count()} graczy");
            var players = clientResponse.Data;
            var playersIds = players.Select(s=>s.Puuid).ToList();
            
            var selectedIds = await _crawlPlayerDbService.GetExistingPuuidListAsync(playersIds,ct); 

            var selectedPlayers = players.Where(p=>!selectedIds.Contains(p.Puuid)).Select(s=>new CrawlPlayer
                {
                    Puuid=s.Puuid,
                    LastCrawledAt=null,
                    Platform=region,
                    Tier=tier
                }).ToList();
            _logger.LogInformation($"Nowych graczy poza baza: {selectedPlayers.Count()} ");

            var addService = await _crawlPlayerDbService.AddListPlayersAsync(selectedPlayers,ct);
            _logger.LogInformation($"Udalo sie dodac: {addService}");

            return selectedPlayers;
        }
        catch(Exception ex)
        {
            return new List<CrawlPlayer>();
        }
    }
    private async Task DiscoverNewPlayersAsync(Match match, CancellationToken ct)
    {
        var playersIds = match.Participants.Select(p => p.Puuid).ToList();
        var existingIds = await _crawlPlayerDbService.GetExistingPuuidListAsync(playersIds, ct);
        var newIds = playersIds.Where(p => !existingIds.Contains(p)).ToList();

        var crawls = new List<CrawlPlayer>();
        foreach (var puuid in newIds)
        {
            var ranks = await _riotClient.GetRanksByPuuidAsync("eun1", puuid, true);
            if (ranks.Data == null) continue;

            crawls.Add(new CrawlPlayer
            {
                Puuid = puuid,
                LastCrawledAt = null,
                Platform = "eun1",
                Tier = ranks.Data
                    .Where(r => r.QueueType == "RANKED_SOLO_5x5")
                    .Select(r => r.Tier)
                    .FirstOrDefault() ?? "nothing"
            });
        }

        _logger.LogInformation("Nowych graczy poza bazą: {Count}", crawls.Count);

        if (crawls.Count > 0)
            await _crawlPlayerDbService.AddListPlayersAsync(crawls, ct);
    }
    public async Task InsertMatches(CancellationToken ct)
    {
        var players = await _crawlPlayerDbService.GetActivePlayersToInsertMatchesAsync("eun1", ct);
        var matchesIds = new List<string>();

        foreach (var player in players)
        {
            ct.ThrowIfCancellationRequested();

            var matches = await _riotClient.GetMatchIdsForCrawlerAsync("europe", player.Puuid, player.LastCrawledAt, IsBcg: true);
            if (matches.StatusCode != System.Net.HttpStatusCode.OK || matches.Data == null)
            {
                _logger.LogWarning("Nie udało się pobrać meczów dla {Puuid}: {Status}", player.Puuid, matches.StatusCode);
                continue;
            }

            await _crawlPlayerDbService.SetCrawlTimeByPuuidAsync(player.Puuid, ct);
            matchesIds.AddRange(matches.Data);
        }

        var uniqueMatchesIds = matchesIds.Distinct().ToList();

        var existing = await _matchDbService.GetExistingMatchIdsAsync(uniqueMatchesIds, ct);
        var toFetch = uniqueMatchesIds.Where(id => !existing.Contains(id)).ToList();

        _logger.LogInformation("{Unique} unikalnych ID, {New} nowych do pobrania", uniqueMatchesIds.Count, toFetch.Count);

        var added = 0;
        foreach (var matchId in toFetch)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var matchInfo = await _riotClient.GetMatchAsync(matchId, true);
                if (matchInfo.StatusCode != System.Net.HttpStatusCode.OK || matchInfo.Data == null)
                {
                    _logger.LogWarning("Nie udało się pobrać meczu {MatchId}: {Status}", matchId, matchInfo.StatusCode);
                    continue;
                }

                var match = MatchMapper.ToEntity(matchInfo.Data);
                await _matchDbService.AddMatchAsync(match, ct);
                added++;

                // await DiscoverNewPlayersAsync(match, ct);  // wyłączone +/-10 dodatkowych requestów na mecz
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd przy zapisie meczu {MatchId}", matchId);
            }
        }

        _logger.LogInformation("Dodano {Added} nowych meczów", added);
    }
}
