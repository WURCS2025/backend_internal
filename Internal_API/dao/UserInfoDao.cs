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

        public void DeleteUser(string username)
        {
            var user = dbcontext.UserInfo.Where(a=>a.username == username).FirstOrDefault();
            if (user != null)
            {
                dbcontext.UserInfo.Remove(user);
                dbcontext.SaveChanges();
            }
        }

        public IQueryable<UserInfo> getQuery()
        {
            var query = dbcontext.UserInfo.AsQueryable();
            return query;
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

        public async Task<IList<UserInfo>> GetUserList()
        {
            var list = await dbcontext.UserInfo.ToListAsync<UserInfo>();
            return list;
        }
    }
}
