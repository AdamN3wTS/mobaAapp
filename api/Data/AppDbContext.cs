using System;
using api.DbModels;
using Microsoft.EntityFrameworkCore;

namespace api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}
    public DbSet<CrawlPlayer> crawlPlayers {get;set;}
    public DbSet<Match> matches {get;set;}
    public DbSet<MatchParticipant> matchParticipants {get;set;}
    public DbSet<MatchBan> matchBans {get;set;}
    public DbSet<ChampionStatsInfo> championStatsInfos {get;set;}
    public DbSet<ChampionStatsItems> championStatsItems {get;set;}

    public DbSet<ChampionStatsSpells> championStatsSpells {get;set;}
    public DbSet<ParticipantRunes> participantRunes {get;set;}
    public List<ChampionStatsRunePage> RunePages { get; set; } = new();
    public List<ChampionStatsRune> Runes { get; set; } = new();
    public List<ChampionStatsShard> Shards { get; set; } = new();





    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<CrawlPlayer>(p=>p.HasKey(p=>p.Puuid));
        modelBuilder.Entity<MatchParticipant>()
        .HasOne(p=>p.Match)
        .WithMany(o=>o.Participants)
        .HasForeignKey(p=>p.MatchId)
        .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<MatchBan>()
        .HasOne(p=>p.Match)
        .WithMany(o=>o.Bans)
        .HasForeignKey(p=>p.MatchId)
        .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ParticipantRunes>()
        .HasOne(p=>p.MatchParticipant)
        .WithMany(o=>o.Runes)
        .HasForeignKey(p=>p.MatchParticipantId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
