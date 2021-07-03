using Dapper;
using SMEnterprise.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SMEnterprise.Repository
{
    public class BBBOnlineStaffMeetingData
    {
        public List<SMSRecieverModel> GetRecieverListOnStafMeeting(int MeetingID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@MeetingID", MeetingID);
                return con.Query<SMSRecieverModel>("spn_GetRecieverListForBBBBStaffMeeting", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public List<OnlineStaffMeetingModel> GetOnlineStaffMeetings(DateTime CurDate, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@CurDate", CurDate);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<OnlineStaffMeetingModel>("sp_GetBBBStaffMeetings", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        public OnlineStaffMeetingModel GetOnlineStaffMeetingDetails(int MeetingId, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@MeetingId", MeetingId);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<OnlineStaffMeetingModel>("sp_GetBBBStaffMeetingDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
        }
        public OnlineStaffMeetingModel GetOnlineStaffMeetingDetailsByMeetingID(string MeetingId, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@MeetingId", MeetingId);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<OnlineStaffMeetingModel>("sp_GetBBBStaffMeetingDetailsByMeetingId", paramater, null, true, 0, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
        }
        public async Task<NameIDModel> JoinOnlineStaffMeetingGetDetail(int TeacherID, DateTime CurDate, int MeetingID, int AppType)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@JoinDate", CurDate);
                paramater.Add("@MeetingID", MeetingID);
                paramater.Add("@AppType", AppType);
                return (await con.QueryAsync<NameIDModel>("sp_JoinBBBStaffMeetingGetDetails", paramater, null, 0, commandType: CommandType.StoredProcedure)).SingleOrDefault();
            }
        }
        public int ScheduleOnlineStaffMeeting(OnlineStaffMeetingModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@MeetingDate", objModel.MeetingDate);
                paramater.Add("@StartTime", objModel.StartTime);
                paramater.Add("@EndTime", objModel.EndTime);
                paramater.Add("@MeetingTitle", objModel.MeetingTitle);
                paramater.Add("@SBranchID", objModel.SBranchID);
                return con.Query<int>("sp_AddBBBStaffMeeting", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public int UpdateOnlineStaffMeeting(OnlineStaffMeetingModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@MeetingID", objModel.MeetingID);
                paramater.Add("@BBBMeetingID", objModel.BBBMeetingID);
                paramater.Add("@InternalMeetingID", objModel.InternalMeetingID);
                paramater.Add("@ModPassword", objModel.ModPassword);
                paramater.Add("@AttPassword", objModel.AttPassword);
                paramater.Add("@Status", objModel.Status);
                paramater.Add("@CurDate", objModel.UpdatedOn);
                return con.Query<int>("sp_UpdateBBBStaffMeetingDetailonStart", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int UpdateOnlineStaffMeetingOnEnd(OnlineStaffMeetingModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@MeetingID", objModel.MeetingID);
                paramater.Add("@BBBMeetingID", objModel.BBBMeetingID);
                paramater.Add("@CurDate", objModel.UpdatedOn);
                paramater.Add("@Attendees", objModel.Attendees);                
                return con.Query<int>("sp_UpdateBBBOnlineMeetionDetailonEnd", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
    }
    public class BBBOnlineClassData
    {

        public List<BBBAttendees> GetOnlineClassAttendees(int OCID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OCID", OCID);
                return con.Query<BBBAttendees>("sp_GetBBBOnlineClassAttendees", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }

        }
        public async Task<NameIDModel> StudentJoinOnlineClass(int StudentID, DateTime CurDate, int OCID, int ParentID,int AppType)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@JoinDate", CurDate);
                paramater.Add("@OCID", OCID);
                paramater.Add("@AppType", AppType);
                paramater.Add("@ParentID", ParentID);
                return (await con.QueryAsync<NameIDModel>("sp_JoinStudentBBBOnlineClass", paramater, null, 0, commandType: CommandType.StoredProcedure)).SingleOrDefault();
            }
        }
        public async Task<StudentModel> GetStudentForOnlineClassByID(int StudentId)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentId", StudentId);
                return (await con.QueryAsync<StudentModel>("sp_GetStudentBasicDetailOnID", paramater, null, 0, CommandType.StoredProcedure)).SingleOrDefault();
            }
        }
        public async Task<StudentModel> GetStudentForOnlineClassByMeetinID(int meetingId,int parentId)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OCID", meetingId);
                paramater.Add("@parentId", parentId);
                return (await con.QueryAsync<StudentModel>("sp_GetStudentBasicDetailOnMeetingID", paramater, null, 0, CommandType.StoredProcedure)).SingleOrDefault();
            }
        }
        public async Task<EmployeeModel> GetTeacherForOnlineClassByID(int teacherId)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@teacherId", teacherId);
                return (await con.QueryAsync<EmployeeModel>("sp_GetTeacherBasicDetailOnID", paramater, null, 0, CommandType.StoredProcedure)).SingleOrDefault();
            }
        }
        public List<BBBOnlineClassModel> GetOnlineClassesList(int TeacherID, DateTime CurDate, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@CurDate", CurDate);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<BBBOnlineClassModel>("sp_GetTeacherBBBWebOnlineClassListOnly", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        public BBBOnlineClassListModel GetOnlineClassesPageData(int TeacherID, DateTime CurDate, int SBranchID)
        {
            BBBOnlineClassListModel objModel = new BBBOnlineClassListModel();
            objModel.ClassDate = CurDate;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@CurDate", CurDate);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetTeacherBBBWebOnlineClasses", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<BBBOnlineClassModel>().ToList();
                    objModel.ClassList = multi.Read<NameIDModel>().ToList();
                    objModel.SubjectList = multi.Read<NameIDModel>().ToList();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                }
            }
            return objModel;
        }
        public List<BBBOnlineClassModel> GetTeacherOnlineClassesSchedules(int TeacherID, DateTime CurDate, int SBranchID)
        {
            BBBOnlineClassListModel objModel = new BBBOnlineClassListModel();
            objModel.ClassDate = CurDate;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@CurDate", CurDate);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<BBBOnlineClassModel>("sp_GetTeacherBBBWebOnlineClassSchedules", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        public int ScheduleOnlineClass(BBBOnlineClassModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@GroupID", objModel.GroupID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@Status", objModel.Status);
                paramater.Add("@ClassDate", objModel.ClassDate);
                paramater.Add("@StartTime", objModel.StartTime);
                paramater.Add("@EndTime", objModel.EndTime);
                paramater.Add("@CreatedDate", objModel.CreatedDate);
                paramater.Add("@MeetingID", objModel.MeetingID);
                paramater.Add("@InternalMeetingID", objModel.InternalMeetingID);
                return con.Query<int>("sp_AddBBBOnlineClass", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public List<SMSRecieverModel> GetRecieverListOnOnlineClass(int OCID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OCID", OCID);
                return con.Query<SMSRecieverModel>("spn_GetRecieverListForBBBOnlineClass", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int UpdateBBBOnlineClassDetails(BBBOnlineClassModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OCID", objModel.OCID);
                paramater.Add("@MeetingID", objModel.MeetingID);
                paramater.Add("@InternalMeetingID", objModel.InternalMeetingID);
                paramater.Add("@AttPassword", objModel.AttPassword);
                paramater.Add("@ModPassword", objModel.ModPassword);
                paramater.Add("@Status", objModel.Status);
                paramater.Add("@CurDate", objModel.StartedOn);
                return con.Query<int>("sp_UpdateBBBOnlineClassDetailonStart", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public BBBOnlineClassModel GetOnlineClassDetailsByOCID(int OCID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OCID", OCID);
                return con.Query<BBBOnlineClassModel>("GetMeetingDetailsOnOCID", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public BBBOnlineClassModel GetOnlineClassDetailsByMeetingID(string MeetingID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@MeetingID", MeetingID);
                return con.Query<BBBOnlineClassModel>("GetMeetingDetailsOnMeetingID", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int StartOnlineClass(BBBOnlineClassModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@OCID", objModel.OCID);
                paramater.Add("@StartedOn", objModel.StartedOn);
                return con.Query<int>("sp_StartBBBOnlineClass", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public async Task<List<BBBOnlineClassModel>> GetPrincipalBBBOnlineClassesSchedules(int SBranchID, DateTime CurDate)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@CurDate", CurDate);
                return (await con.QueryAsync<BBBOnlineClassModel>("sp_GetPrincipalBBBOnlineClass", paramater, null, 0, commandType: CommandType.StoredProcedure)).ToList();

            }
        }
        public int EndOnlineClass(BBBOnlineClassModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OCID", objModel.OCID);
                paramater.Add("@MeetingID", objModel.MeetingID);
                paramater.Add("@Status", objModel.Status);
                paramater.Add("@CurDate", objModel.EndedOn);
                paramater.Add("@Attendees", objModel.AttendeesJSON);
                return con.Query<int>("sp_UpdateBBBOnlineClassDetailonEnd", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public int UpdateRecordingReady(BBBOnlineClassRecording objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@MeetingID", objModel.MeetingID);
                paramater.Add("@PlaybackURL", objModel.PlaybackURL);
                paramater.Add("@RecordingState", objModel.RecordingState);
                paramater.Add("@RawRecordingSize", objModel.RawRecordingSize);
                paramater.Add("@ProcessedRecordingSize", objModel.ProcessedRecordingSize);
                paramater.Add("@Thumbnail", objModel.Thumbnail);
                paramater.Add("@RecordingStartTime", objModel.RecordingStartTime);
                paramater.Add("@RecordingEndTime", objModel.RecordingEndTime);
                return con.Query<int>("sp_UpdateBBBRecordingStatus", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public async Task<List<BBBOnlineClassModel>> GetStudentOnlineClassesSchedules(int StudentID, DateTime CurDate)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@CurDate", CurDate);
                return (await con.QueryAsync<BBBOnlineClassModel>("sp_GetStudentBBBWebOnlineClass", paramater, null, 0, CommandType.StoredProcedure)).ToList();
            }
        }
    }
    public class OnlineClassData
    {
        public List<OnlineClassModel> GetOnlineClassesSchedules(int TeacherID, DateTime CurDate)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@CurDate", CurDate);
                return con.Query<OnlineClassModel>("sp_GetTeacherOnlineClasses", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public NameIDModel ScheduleOnlineClass(OnlineClassModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@GroupID", objModel.GroupID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@ClassDate", objModel.ClassDate);
                paramater.Add("@StartTime", objModel.StartTime);
                paramater.Add("@EndTime", objModel.EndTime);
                paramater.Add("@CreatedDate", objModel.CreatedDate);
                return con.Query<NameIDModel>("sp_ScheduleTeacherOnlineClass", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public int StartOnlineClass(OnlineClassModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@OCID", objModel.OCID);
                paramater.Add("@StartedOn", objModel.StartedOn);
                return con.Query<int>("sp_StartOnlineClass", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public List<OnlineClassAttendeeModel> GetOnlineClassAttendees(int OCID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OCID", OCID);
                return con.Query<OnlineClassAttendeeModel>("sp_GetOnlineClassAttendees", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }

        }
        public int EndOnlineClass(OnlineClassModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@OCID", objModel.OCID);
                paramater.Add("@EndedOn", objModel.EndedOn);
                return con.Query<int>("sp_EndOnlineClass", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public async Task<List<OnlineClassModel>> GetStudentOnlineClassesSchedules(int StudentID, DateTime CurDate)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@CurDate", CurDate);
                return (await con.QueryAsync<OnlineClassModel>("sp_GetStudentOnlineClass", paramater, null, 0, CommandType.StoredProcedure)).ToList();
            }
        }
        public async Task<object> StudentJoinOnlineClass(int StudentID, DateTime CurDate, int OCID, int ParentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@JoinDate", CurDate);
                paramater.Add("@OCID", OCID);
                paramater.Add("@ParentID", ParentID);
                return (await con.QueryAsync<object>("sp_JoinStudentOnlineClass", paramater, null, 0, commandType: CommandType.StoredProcedure)).SingleOrDefault();
            }
        }
        public async Task<int> StudentLeaveOnlineClass(int StudentID, DateTime CurDate, int OCID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@LeaveDate", CurDate);
                paramater.Add("@OCID", OCID);
                return (await con.QueryAsync<int>("sp_LeaveStudentOnlineClass", paramater, null, 0, commandType: CommandType.StoredProcedure)).SingleOrDefault();
            }
        }
        public List<SMSRecieverModel> GetRecieverListOnOnlineClass(int OCID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OCID", OCID);
                return con.Query<SMSRecieverModel>("spn_GetRecieverListForOnlineClass", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public List<NameIDModel> GetSBranchesOnlineClassURLs()
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                return con.Query<NameIDModel>("sp_GetSBranchesOnlineClassURL", null, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        //For Principal

        public List<OnlineClassModel> GetPrincipalOnlineClassesSchedules(int SBranchID, DateTime CurDate)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@CurDate", CurDate);
                return con.Query<OnlineClassModel>("sp_GetPrincipalOnlineClass", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        private static List<NameIDModel> _SBranchesOnlineClassURLs;
        public static List<NameIDModel> SBranchesOnlineClassURLs
        {
            get
            {
                if (_SBranchesOnlineClassURLs == null)
                {
                    _SBranchesOnlineClassURLs = (new OnlineClassData()).GetSBranchesOnlineClassURLs();
                }
                return _SBranchesOnlineClassURLs;
            }
        }
        #region Online Staff Meeting
        public int AddOnlineStaffMeeting(OnlineStaffMeetingModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@MeetingDate", oModel.MeetingDate);
                paramater.Add("@StartTime", oModel.StartTime);
                paramater.Add("@EndTime", oModel.EndTime);
                paramater.Add("@MeetingTitle", oModel.MeetingTitle);
                paramater.Add("@SBranchID", oModel.SBranchID);
                return con.Query<int>("sp_AddStaffMeeting", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int UpdateOnlineStaffMeeting(OnlineStaffMeetingModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@MeetingID", oModel.MeetingID);
                paramater.Add("@Status", oModel.Status);
                return con.Query<int>("sp_UpdateStaffMeeting", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public List<OnlineStaffMeetingModel> GetOnlineStaffMeetings(int SBranchID, DateTime CurDate)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@CurDate", CurDate);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<OnlineStaffMeetingModel>("sp_GetStaffMeetings", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        public List<SMSRecieverModel> GetRecieverListOnStafMeeting(int MeetingID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@MeetingID", MeetingID);
                return con.Query<SMSRecieverModel>("spn_GetRecieverListForStaffMeeting", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        #endregion
    }
}