using System;

namespace api.DbModels;

public class CrawlPlayer
{
    public string Puuid {get;set;}
    public string? Tier {get;set;}
    public string Platform {get;set;}
    public DateTime? LastCrawledAt {get;set;}
}
