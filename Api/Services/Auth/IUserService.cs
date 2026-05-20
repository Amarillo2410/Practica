using System.Threading;
using System.Threading.Tasks;
using Api.Dtos.Auth;

namespace Api.Services.Auth;

public interface IUserService
{
    Task<DataUserDto> RegisterAsync(RegisterDto model);
    Task<DataUserDto> GetTokenAsync(LoginDto model);
    Task<DataUserDto> AddRoleAsync(AddRoleDto model);
    Task<DataUserDto> RefreshTokenAsync(string refreshToken);
}
