using Internal_API.model;
using Internal_API.models;
using Internal_API.service;
using Internal_API.utility;
using Microsoft.EntityFrameworkCore;

namespace Internal_API.dao
{
    public class UserInfoDao : IUserInfoDao
    {
        private readonly AppDbContext _context;

        public UserInfoDao(AppDbContext context)
        {
            _context = context;
        }
        public async Task CreateUserAsync(UserInfo user)
        {
            await _context.UserInfo.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(UserInfo user)
        {
            var delUser = _context.UserInfo.Where(a => a.Id == user.Id).FirstOrDefault();
            if (delUser != null)
            {
                _context.UserInfo.Remove(delUser);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<UserInfo?> GetUserByUsernameAsync(string username)
        {
            return await _context.UserInfo.FirstOrDefaultAsync(a=>a.Username == username);
        }

        public async Task<bool> VerifyUserPassword(UserInfo user)
        {
            var existingUser = await GetUserByUsernameAsync(user.Username);

            if (existingUser == null || !PasswordHelper.VerifyPassword(user.PasswordHash, existingUser.PasswordHash))
                return false;

            return true;
        }
    }
}
