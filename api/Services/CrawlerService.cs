using System;
using api.Client;
using api.DbModels;
using api.IDbServices;
using api.IServices;

namespace api.Services;

public class CrawlerService : ICrawlerService
{
    private readonly ILogger<CrawlerService> _logger;

    private readonly IRiotClient _riotClient;
    private readonly ICrawlPlayerDbService _crawlPlayerDbService;
    public CrawlerService(ILogger<CrawlerService> logger,IRiotClient riotClient,ICrawlPlayerDbService crawlPlayerDbService)
    {
        _logger=logger;
        _riotClient=riotClient;
        _crawlPlayerDbService=crawlPlayerDbService;
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
}
