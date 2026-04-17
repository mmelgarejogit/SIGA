using SIGA.Application.Common;
using SIGA.Application.DTOs.Users;

namespace SIGA.Application.Interfaces;

public interface IUserService
{
    Task<Result<IEnumerable<UserResponse>>> GetAllAsync();
}
