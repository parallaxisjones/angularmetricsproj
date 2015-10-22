using Microsoft.AspNet.Identity;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Playverse.Data;
using System.Collections.Generic;
using System.Data;
using Utilities;
namespace PlayverseMetrics.Models
{
    public class User : IUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
    }

    public class Users : IUserStore<User>
                        ,IUserPasswordStore<User>
    {
        private const string Salt = "{9635218E-20F0-405F-AA68-619AC6AAE5D1}";

        public string GetHashedSaltedPassword(string password)
        {
            // explicitly name types on variables, produces a compile time error when something happens
            byte[] salt = Encoding.UTF8.GetBytes(Salt);
            byte[] bytes = Encoding.UTF8.GetBytes(password);

            HMACMD5 hmacMD5 = new HMACMD5(salt);
            byte[] saltedHash = hmacMD5.ComputeHash(bytes);

            return Encoding.UTF8.GetString(saltedHash);
        }

        public Task CreateAsync(User user)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteAsync(User user)
        {
            throw new System.NotImplementedException();
        }

        public Task<User> FindByIdAsync(string userId)
        {
            string query = @"SELECT * from User where Id = @uname;";

            try
            {
                DataTable res = DBManager.Instance.Query(Datastore.Monitoring, query, new Dictionary<string, string>() { 
                    {"@uname", userId}
                });
                if (res.Rows.Count > 0)
                {
                    User user = new User()
                    {
                        Id = res.Rows[0]["Id"].ToString(),
                        UserName = res.Rows[0]["Username"].ToString(),
                        PasswordHash = res.Rows[0]["PasswordHash"].ToString(),
                        Email = res.Rows[0]["Email"].ToString()
                    };
                    return Task.FromResult<User>(user);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(String.Format("FindUserById Error: {0}", ex.Message), ex.StackTrace);
            }
            //}

            return Task.FromResult<User>(null);
        }

        public Task<User> FindByNameAsync(string userName)
        {
            PasswordHasher passwordHasher = new PasswordHasher();

            string query = @"SELECT * from User where Username = @uname;";

            DataTable res = DBManager.Instance.Query(Datastore.Monitoring, query, new Dictionary<string, string>() { 
                {"@uname", userName}
            });
            if (res.Rows.Count > 0) {
                User user = new User()
                {
                    Id = res.Rows[0]["Id"].ToString(),
                    UserName = res.Rows[0]["Username"].ToString(),
                    PasswordHash = res.Rows[0]["PasswordHash"].ToString(),
                    Email = res.Rows[0]["Email"].ToString()
                };
                return Task.FromResult<User>(user);
            }
            //}

            return Task.FromResult<User>(null);
        }

        public Task UpdateAsync(User user)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            // TODO: May need something?
        }

        public Task<string> GetPasswordHashAsync(User user)
        {
            return Task.FromResult<string>(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task SetPasswordHashAsync(User user, string passwordHash)
        {
            throw new NotImplementedException();
        }
    }
}