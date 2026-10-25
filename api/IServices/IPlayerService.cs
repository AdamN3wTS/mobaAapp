using System;
using api.Models;
using Api.DTOs;

namespace api.IServices;

public interface IPlayerService
{
    public Task<ResponseDto<PlayerProfileDTO>> GetProfileAsync(string gameName,string tagLine);
    Task<ResponseDto<PaginatedMatchHistoryDTO?>> GetMatchHistoryAsync(string puuid, int page, int pageSize);

}
