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

            var clientResponse = await _riotClient.GetSummonersByRegionQueueTierDivisionAsync(region,queue,tier,division,page);
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
    public async Task InsertMatches(CancellationToken ct)
    {
        var players = await _crawlPlayerDbService.GetActivePlayersToInsertMatchesAsync(ct);
        var matchesIds = new List<string>();
        foreach(var player in players)
        {
            var matches = await _riotClient.GetMatchIdsForCrawlerAsync("europe",player.Puuid,50);
            if(matches.StatusCode==System.Net.HttpStatusCode.OK)
            {
                var crawled = await _crawlPlayerDbService.SetCrawlTimeByPuuidAsync(player.Puuid,ct);
                matchesIds.AddRange(matches.Data);
            }
        }
        var uniqueMatchesIds = matchesIds.Distinct().ToList();

        foreach(var item in uniqueMatchesIds)
        {
            var isExist = await _matchDbService.IsMatchExistByMatchIdAsync(item,ct);
            if (isExist)
            {
                continue;
            }
            var matchInfo = await _riotClient.GetMatchAsync(item);
            var dtoMapper = MatchMapper.ToEntity(matchInfo.Data);
            var addService = await _matchDbService.AddMatchAsync(dtoMapper,ct);
            
        }
    }
}
