using System;
using api.IServices;

namespace api.DatabaseCleaner;

public class DatabaseCleaner : BackgroundService
{
    private readonly ILogger<DatabaseCleaner> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    public DatabaseCleaner(IServiceScopeFactory scopeFactory,ILogger<DatabaseCleaner> logger)
    {
        _logger=logger;
        _scopeFactory=scopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("dbCleaner starting...");
        var time = int.Parse(Environment.GetEnvironmentVariable("CLEANER_TIME_MINUTES")!);

        await Task.Yield();
        while (!stoppingToken.IsCancellationRequested)
        {
                _logger.LogInformation("dbCleaner started");
            
               
                try
                {
                    await using(var scope = _scopeFactory.CreateAsyncScope()){
                        
                        var _cleanerService = scope.ServiceProvider.GetRequiredService<IDbCleanerService>();
                        await _cleanerService.CleanDatabase(stoppingToken);
                        await Task.Delay(TimeSpan.FromMinutes(time), stoppingToken); 
                        
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
