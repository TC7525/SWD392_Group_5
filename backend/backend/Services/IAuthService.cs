using System.Threading.Tasks;
using backend.Models.DTOs;

namespace backend.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}
