using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SMEnterprise.Models;
using SMEnterprise.Repository;

namespace SMEnterprise.Services
{
    public interface IUserService
    {
        UserModel Authenticate(string username, string password,string salt);
        IEnumerable<UserModel> GetAll();
    }
    public class AppSettings
    {
        public string Secret { get; set; }
    }

    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<UserModel> _users = new List<UserModel>
        { 
            new UserModel { UserID = 1, FullName = "Test", UserName = "test", Password = "test" } 
        };

        private readonly AppSettings _appSettings;
        LoginData oLoginData = new LoginData();

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }
        public UserService()
        {
        }

        public UserModel Authenticate(string username, string password,string salt)
        {
            var user = oLoginData.GetUserByUserName(username);
            if (user == null)
                return null;
            string NewHash = CommonUsage.EncryptPassword(user.Password + salt);
            if (user != null && NewHash == password)
            {

                // authentication successful so generate jwt token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("Psys@1040@1234@0405");
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Name, user.UserID.ToString()),
                    new Claim(ClaimTypes.Role, user.RoleID.ToString()),
                    new Claim(ClaimTypes.Gender, user.SBranchID.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMonths(12),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                user.Token = tokenHandler.WriteToken(token);

                // remove password before returning
                user.Password = null;
               // user.BasePath = CommonUsage.AssetBasePath.Replace("\\","/");
                return user;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<UserModel> GetAll()
        {
            // return users without passwords
            return _users.Select(x => {
                x.Password = null;
                return x;
            });
        }
    }
}