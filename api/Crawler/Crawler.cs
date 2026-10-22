using System;
using api.IServices;
using api.Services;

namespace api.Crawler;

public class Crawler : BackgroundService
{
    private readonly ILogger<Crawler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    public Crawler(IServiceScopeFactory scopeFactory,ILogger<Crawler> logger)
    {
        _logger=logger;
        _scopeFactory=scopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Crawler starting...");
        await Task.Yield();
        while (!stoppingToken.IsCancellationRequested)
        {
                _logger.LogInformation("Crawler started");
            
                int page = 1;
                try
                {
                    await using(var scope = _scopeFactory.CreateAsyncScope()){
                        var _crawlerService = scope.ServiceProvider.GetRequiredService<ICrawlerService>();
                        var players = await _crawlerService.GetPlayerList("eun1","PLATINUM","RANKED_SOLO_5x5","II",page,stoppingToken);
                        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); 
                        page++;
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break; 
                }
                catch (Exception ex)
                {
                    
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); 
                }
        }
    }
    
}
