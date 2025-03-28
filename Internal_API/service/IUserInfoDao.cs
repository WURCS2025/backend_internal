using Internal_API.model;
using Internal_API.models;

namespace Internal_API.service
{
    public interface IUserInfoDao
    {
        Task CreateUserAsync(UserInfo user);
        Task DeleteUserAsync(UserInfo user);

        Task<UserInfo> GetUserByUsernameAsync(string username);

        Task<Boolean> VerifyUserPassword(Login user);

        Task UpdateUserAsync(UserInfo user);

        void DeleteUser(string username);

        Task<IList<UserInfo>> GetUserList();

        IQueryable<UserInfo> getQuery();
    }
}
