using System;
using api.DbModels;

namespace api.IServices;

public interface ICrawlerService
{
    public Task InsertMatches(CancellationToken ct);

    public Task<List<CrawlPlayer>> GetPlayerList(string region,string tier,string queue,string division,int page,CancellationToken ct);
}
