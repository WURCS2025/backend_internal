using Internal_API.model;

namespace Internal_API.service
{
    public interface IUserInfoDao
    {
        Task CreateUserAsync(UserInfo user);
        Task DeleteUserAsync(UserInfo user);

        Task<UserInfo> GetUserByUsernameAsync(string username);

        Task<Boolean> VerifyUserPassword(UserInfo user);
    }
}
