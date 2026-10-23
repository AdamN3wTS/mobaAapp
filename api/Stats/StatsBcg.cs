using System;
using api.IServices;

namespace api.Stats;

public class StatsBcg : BackgroundService
{
    private readonly ILogger<StatsBcg> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    public StatsBcg(IServiceScopeFactory scopeFactory,ILogger<StatsBcg> logger)
    {
        _logger=logger;
        _scopeFactory=scopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stats starting...");
        await Task.Yield();
        while (!stoppingToken.IsCancellationRequested)
        {
                _logger.LogInformation("StatsBcg started");
            
                
                try
                {
                    await using(var scope = _scopeFactory.CreateAsyncScope()){
                        
                        var _statsService= scope.ServiceProvider.GetRequiredService<IStatsService>();
                        await _statsService.GenerateStats(stoppingToken);
                        await Task.Delay(TimeSpan.FromMinutes(20), stoppingToken); 
                        
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
