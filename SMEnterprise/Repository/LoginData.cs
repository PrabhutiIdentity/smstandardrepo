using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using SMEnterprise.Models;
using Dapper;

namespace SMEnterprise.Repository
{
    public class LoginData 
    {
        public Role GetRoleByUserID(string UserId)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var para = new DynamicParameters();
                para.Add("@UserId", UserId);
                return con.Query<Role>("Usp_getRoleByUserID", para, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public IEnumerable<Users> GetAllUsers()
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                return con.Query<Users>("Usp_GetAllUsers", null, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public UserModel GetUserByUserName(string UserName)
        {
            UserModel UM;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var para = new DynamicParameters();
                para.Add("@LoginName", UserName);
                using (var multi = con.QueryMultiple("sp_GetUserByLoginName", para, null, 0, commandType: CommandType.StoredProcedure))
                {
                    UM = multi.Read<UserModel>().SingleOrDefault();
                }
            }
            return UM;
        }
        public ForgetPasswordApiModel GetUserByMobileDOB(ForgetPasswordApiModel UD)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var para = new DynamicParameters();
                para.Add("@UserID", UD.UserID);
                para.Add("@MobileNumber", UD.MobileNumber);
                para.Add("@DOB", UD.DOB);
                return con.Query<ForgetPasswordApiModel>("sp_GetUserByMobileDOB", para, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public string GetSavedPasswordByUserName(string UserName)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var para = new DynamicParameters();
                para.Add("@LoginName", UserName);
                return con.Query<string>("sp_GetPasswordByLoginName", para, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public string Get_checkUsernameExits(string username)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var para = new DynamicParameters();
                para.Add("@UserName", username);
                return con.Query<string>("Usp_checkUsernameExits", para, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public string Get_checkEmailExits(string EmailID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var para = new DynamicParameters();
                para.Add("@EmailID", EmailID);
                return con.Query<string>("Usp_checkEmailExits", para, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public bool Get_CheckUserRoles(string UserId)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var para = new DynamicParameters();
                para.Add("@UserId", UserId);
                return con.Query<bool>("Usp_CheckUserRoles", para, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public string GetUserName_BY_UserID(string UserId)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var para = new DynamicParameters();
                para.Add("@UserId", UserId);
                return con.Query<string>("Usp_UserNamebyUserID", para, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int InsertUser(UserModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@FullName", objModel.FullName);
                paramater.Add("@UserName", objModel.UserName);
                paramater.Add("@EmailID", objModel.EmailID);
                paramater.Add("@Password", objModel.Password);
                paramater.Add("@CreatedDate", objModel.CreatedDate);


               return con.Query<int>("sp_InsertUser", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }

    }
}