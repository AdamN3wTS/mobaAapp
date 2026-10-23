using System;
using api.Data;
using api.DbModels;
using api.IDbServices;
using Microsoft.EntityFrameworkCore;

namespace api.DbServices;

public class MatchDbService : IMatchDbService
{
    private readonly AppDbContext _dbContext;
    public MatchDbService(AppDbContext appDbContext)
    {
        _dbContext=appDbContext;
    }
    public async Task<bool> IsMatchExistByMatchIdAsync(string matchId,CancellationToken ct)
    {
        var match= await _dbContext.matches.FirstOrDefaultAsync(p=>p.MatchId==matchId,ct);
        return match==null ? false :true;
    }
    public async Task<bool> AddMatchAsync(Match match, CancellationToken ct)
    {
        
        await _dbContext.matches.AddAsync(match,ct);            
        await _dbContext.SaveChangesAsync(ct);    
        return true;
        
        
    }

}
