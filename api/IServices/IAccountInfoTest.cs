using System.Threading.Tasks;
using api.Models;
using api.Models.Account;

namespace api.IServices
{
    public interface IAccountInfoTest
    {
        Task<ResponseDto<FullAccountInfoDto>> GetFullAccountInfoAsync(string gameName, string tagLine);
    }
}