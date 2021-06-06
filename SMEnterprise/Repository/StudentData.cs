using Dapper;
using SMEnterprise.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SMEnterprise.Repository
{
    public class StudentData
    {
        public async Task<IEnumerable<AssignmentModel>> GetStudentAssignments(int StudentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                return (await con.QueryAsync<AssignmentModel>("spn_GetStudentAssignments", paramater, null,  0, CommandType.StoredProcedure)).ToList();
            }
        }
        public async Task<IEnumerable<AssignmentModel>> GetStudentAssignments(int StudentID,int SubjectID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SubjectID", SubjectID);
                return (await con.QueryAsync<AssignmentModel>("spn_GetStudentAssignments", paramater, null,  0, CommandType.StoredProcedure)).ToList();
            }
        }
        #region Holiday List
        public List<HolidayModel> GetHolidays(int StudentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                return con.Query<HolidayModel>("sp_GetStudentHolidays", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        #endregion
        #region Leave List
        public async Task<List<StudentLeaveModel>> GetStudentLeaves(int StudentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                return (await con.QueryAsync<StudentLeaveModel>("sp_GetLeavesByStudent", paramater, null,  0, CommandType.StoredProcedure)).ToList();
            }
        }
        #endregion
    }

}