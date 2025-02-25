using Internal_API.model;

namespace Internal_API.service
{
    public interface IJwtTokenService
    {
        string GenerateToken(UserInfo user);
    }
}
