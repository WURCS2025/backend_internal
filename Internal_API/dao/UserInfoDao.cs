using Internal_API.model;
using Internal_API.models;
using Internal_API.service;
using Internal_API.utility;
using Microsoft.EntityFrameworkCore;

namespace Internal_API.dao
{
    public class UserInfoDao : IUserInfoDao
    {
        private readonly AppDbContext dbcontext;

        public UserInfoDao(AppDbContext context)
        {
            dbcontext = context;
        }
        public async Task CreateUserAsync(UserInfo user)
        {
            await dbcontext.UserInfo.AddAsync(user);
            await dbcontext.SaveChangesAsync();

        }

        public async Task UpdateUserAsync(UserInfo user)
        {
            dbcontext.UserInfo.Update(user);
            await dbcontext.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(UserInfo user)
        {
            var delUser = dbcontext.UserInfo.Where(a => a.id == user.id).FirstOrDefault();
            if (delUser != null)
            {
                dbcontext.UserInfo.Remove(delUser);
                await dbcontext.SaveChangesAsync();
            }
        }

        public async Task<UserInfo?> GetUserByUsernameAsync(string username)
        {
            return await dbcontext.UserInfo.FirstOrDefaultAsync(a=>a.username == username);
        }

        public async Task<bool> VerifyUserPassword(Login user)
        {
            var existingUser = await GetUserByUsernameAsync(user.username);

            if (existingUser == null || !PasswordHelper.VerifyPassword(user.password, existingUser.passwordhash))
                return false;

            return true;
        }
    }
}
