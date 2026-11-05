using System;
using api.Models;
using api.Models.Champion;

namespace api.IServices;

public interface IChampionService
{
    Task<ResponseDto<ChampionDto?>> GetChampionStatsAsync(string championName,string TeamPosition, CancellationToken ct);
    Task<ResponseDto<List<ChampionPositionDto>?>> GetChampionPositionsAsync(string championName, CancellationToken ct);
}
