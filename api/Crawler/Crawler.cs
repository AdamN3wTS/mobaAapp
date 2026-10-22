using System;

namespace api.Crawler;

public class Crawler : BackgroundService
{
    public Crawler()
    {
        
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
               
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); 
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
