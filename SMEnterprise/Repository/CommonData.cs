using Dapper;
using SMEnterprise.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace SMEnterprise.Repository
{
    public class CommonData
    {
        public PerformanceParameterDetailModel GetStudentPerformanceDetails(DateTime DOB, string StudentSID)
        {
            PerformanceParameterDetailModel model = new PerformanceParameterDetailModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                //StartupModel objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];
                var paramater = new DynamicParameters();
                paramater.Add("@DOB", DOB);
                paramater.Add("@StudentSID", StudentSID);
                using (var multi = con.QueryMultiple("sp_GetStudentReportByDOBID", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    model.PerformanceParameters = multi.Read<PerformanceParameterModel>().ToList();
                    model.MainEvaluations = multi.Read<EvaluationModel>().ToList();
                    model.SubEvaluations = multi.Read<EvaluationModel>().ToList();
                    model.Result = multi.Read<ExamResultDetailModel>().ToList();
                    model.Student = multi.Read<StudentModel>().SingleOrDefault();
                    model.CGPA = multi.Read<decimal>().SingleOrDefault();
                    model.EvaluationName = multi.Read<string>().SingleOrDefault();
                    model.FilledParameter = multi.Read<int>().SingleOrDefault();
                    model.TotalParameter = multi.Read<int>().SingleOrDefault();
                    model.EvaluationTypes = multi.Read<EvaluationTypeModel>().ToList();
                    model.SBranchDetails = multi.Read<SBranchModel>().SingleOrDefault();
                    try
                    {
                        model.SessionName = multi.Read<string>().SingleOrDefault();

                    }
                    catch { }
                }
            }
            return model;
        }
        public async Task< EventCalendarModel> GetEventCalander(int Month, int Year, int SBranchID, int Status,int ParentID=0)
        {
            EventCalendarModel objModel = new EventCalendarModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@Month", Month);
                paramater.Add("@Year", Year);
                paramater.Add("@Status", Status);
                paramater.Add("@ParentID", ParentID);
                using (var multi = await con.QueryMultipleAsync("sp_GetEventCalendar", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Events = multi.Read<EventModel>().ToList();
                    objModel.Holidays = multi.Read<HolidayModel>().ToList();
                }
            }
            return objModel;
        }
        public async Task<int> GetTransportLocationMode(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                DateTime curDate = CommonUsage.GetCurrentDate();
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return (await con.QueryAsync<int>("tsp_GetTransportLocationMode", paramater, null, 0, CommandType.StoredProcedure)).FirstOrDefault();
            }
        }
        //public static void InitializeSMSConfiguration(string Path)
        //{
        //    try
        //    {
        //        using (FileStream xmlStream = new FileStream(Path + "SMSParams.xml", FileMode.Open))
        //        {
        //            using (XmlReader xmlReader = XmlReader.Create(xmlStream))
        //            {
        //                XmlSerializer serializer = new XmlSerializer(typeof(SMSParams));
        //                CommonUsage.SMSConfig = serializer.Deserialize(xmlReader) as SMSParams;
        //            }
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        SMSStatusModel objModel = new SMSStatusModel();
        //        objModel.MobileNumber = "";
        //        objModel.SMSDateTime = CommonUsage.GetCurrentDate();
        //        objModel.ReasonFailure = ex.Message;
        //        (new CommonData()).UpdateSMSFailure(objModel);
        //    }
        //}
        public string GetNotificationServerKey(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<string>("sp_GetNotificationServerKey", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int AddYoutubeVideo(YouTubeVideoModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", oModel.TeacherID);
                paramater.Add("@UploadDate", oModel.UploadDate);
                paramater.Add("@SubjectID", oModel.SubjectID);
                paramater.Add("@Title", oModel.Title);
                paramater.Add("@Description", oModel.Description);
                paramater.Add("@YoutubeID", oModel.YouTubeID);
                paramater.Add("@ClassID", oModel.ClassID);
                paramater.Add("@SectionID", oModel.SectionID);
                paramater.Add("@SequenceNo", oModel.SequenceNo);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@VideoDate", oModel.VideoDate);
                return con.Query<int>("sp_AddYouTubeVideo", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int DeleteYoutubeVideo(YouTubeVideoModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", oModel.TeacherID);
                paramater.Add("@VideoID", oModel.VideoID);
                return con.Query<int>("sp_DeleteYouTubeVideo", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public List<YouTubeVideoModel>  GetYoutubeVideos(int SBranchID,int ClassID,int SectionID, int SubjectID,int TeacherID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@SubjectID", SubjectID);
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<YouTubeVideoModel>("sp_GetYouTubeVideo", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        public static void InitializeCommonConfiguration()
        {
            //try
            //{
            //    using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            //    {
            //        using (var multi = con.QueryMultiple("spn_GetCommonStartupModel", null, null, 0, commandType: CommandType.StoredProcedure))
            //        {
            //            CommonUsage.NotificationServerKey = multi.Read<string>().SingleOrDefault();

            //        }
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
        }
        public void InitializeStartupSettings(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                StartupModel objStartupModel;
                if (HttpContext.Current.Session["StartupModel"] == null)
                {
                    objStartupModel = new StartupModel();
                }
                else
                {
                    objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];

                }
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetStartupModel", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objStartupModel.SessionStartDate = multi.Read<DateTime>().SingleOrDefault();
                    objStartupModel.SessionEndDate = multi.Read<DateTime>().SingleOrDefault();
                    objStartupModel.HostelFeeMode = multi.Read<int>().SingleOrDefault();
                    objStartupModel.TransportFeeMode = multi.Read<int>().SingleOrDefault();
                    objStartupModel.EvaluationMode = multi.Read<int>().SingleOrDefault();
                    objStartupModel.AbsentNotificationTime = multi.Read<string>().SingleOrDefault();
                    objStartupModel.AbsentSMSTemplate = multi.Read<string>().SingleOrDefault();
                    objStartupModel.StudentInOutSMSNotification = multi.Read<int>().SingleOrDefault();
                    objStartupModel.SchemaPrefix = multi.Read<string>().SingleOrDefault();
                    objStartupModel.StudentInSMSTemplate = multi.Read<string>().SingleOrDefault();
                    objStartupModel.StudentOutSMSTemplate = multi.Read<string>().SingleOrDefault();
                    objStartupModel.SMSForAbsentStudents = multi.Read<int>().SingleOrDefault();
                    objStartupModel.FeePaymentReminderDate = multi.Read<int>().SingleOrDefault();
                    objStartupModel.FeePaymentReminderSMS = multi.Read<int>().SingleOrDefault();
                    objStartupModel.FeePaymentReminderSMSTemplate = multi.Read<string>().SingleOrDefault();
                    objStartupModel.FeePaymentNotificationSMSTemplate = multi.Read<string>().SingleOrDefault();
                    //PermissionManager.GetLoggedInUser().NotificationServerKey = multi.Read<string>().SingleOrDefault();
                }
                HttpContext.Current.Session["StartupModel"] = objStartupModel;
            }
        }
        public static int InsertApplicationLog(string OperationTime)
        {
            //using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            //{
            //    DateTime OperationDate = CommonUsage.GetCurrentDate();
            //    var paramater = new DynamicParameters();
            //    paramater.Add("@OperationType", OperationTime);
            //    paramater.Add("@CreatedDate", OperationDate);

            //    return con.Query<int>("sp_AddApplicationLog", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            //}
            return 1;
        }
        public int InsertLog(int UserID, string URL, string Data)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@LogDate", CommonUsage.GetCurrentDate());
                paramater.Add("@UserID", UserID);
                paramater.Add("@URL", URL);
                paramater.Add("@Data", Data);

                try
                {
                    return con.Query<int>("sp_AddOperationLog", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
                }
                catch
                {
                    return 1;
                }

            }
        }
        public int InsertError(int UserID, string URL, string Data)
        {
<<<<<<< HEAD
            using (SqlConnection con = new SqlConnection("Data Source=173.249.36.15;Initial Catalog=PSchoolonline;Persist Security Info=True;User ID=Pschool;Password=P@school@"))
=======
            using (SqlConnection con = new SqlConnection("Data Source=173.249.36.15,1405;Initial Catalog=PSchoolonline;Persist Security Info=True;User ID=Pschool;Password=P@school@"))
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
            {
                var paramater = new DynamicParameters();
                paramater.Add("@LogDate", CommonUsage.GetCurrentDate());
                paramater.Add("@UserID", UserID);
                paramater.Add("@URL", URL);
                paramater.Add("@Data", Data);

                try
                {
                    return con.Query<int>("sp_AddErrorLog", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
                }
                catch(Exception ex)
                {
                    return 1;
                }

            }
        }
        public static int InsertTestLog(string Log)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                DateTime OperationDate = CommonUsage.GetCurrentDate();
                var paramater = new DynamicParameters();
                paramater.Add("@OperationType", Log);
                paramater.Add("@CreatedDate", OperationDate);

                return con.Query<int>("sp_AddApplicationLog", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
            return 1;
        }
        public static int InsertTestingAttandance()
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                DateTime OperationDate = CommonUsage.GetCurrentDate();

                return con.Query<int>("sp_AddAttandance", null, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
            //return 1;
        }
        public int UpdateSMSCount(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                DateTime OperationDate = CommonUsage.GetCurrentDate();
                int Month = OperationDate.Month;
                int Year = OperationDate.Year;
                var paramater = new DynamicParameters();
                paramater.Add("@Year", Year);
                paramater.Add("@Month", Month);
                paramater.Add("@SBranchID", SBranchID);

                return con.Query<int>("sp_UpdateSMSMonthlyCount", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public int UpdateSMSFailure(SMSStatusModel data)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SMSType", data.SMSType);
                paramater.Add("@RecieverType", data.RecieverType);
                paramater.Add("@RecieverID", data.RecieverID);
                paramater.Add("@MobileNumber", data.MobileNumber);
                paramater.Add("@SMSText", data.SMSText);
                paramater.Add("@ReasonFailure", data.ReasonFailure);
                paramater.Add("@SMSDateTime", data.SMSDateTime);

                return con.Query<int>("sp_InsertSMSFailure", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public async Task<int> UpdatePassword(int UserID, int UserType, string Password)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@UserID", UserID);
                paramater.Add("@UserType", UserType);
                paramater.Add("@Password", Password);

                return (await con.QueryAsync<int>("sp_ChangePassword", paramater, null,  0, commandType: CommandType.StoredProcedure)).SingleOrDefault();

            }
        }
        public IEnumerable<NotificationModel> GetNotifications(int SBranchID, int RecieverType, int RecieverID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@RecieverID", RecieverID);
                paramater.Add("@RecieverType", RecieverType);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<NotificationModel>("sp_GetNotifications", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int InsertUpdateAppUser(ApiAuthenticationModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@UserType", objModel.UserType);
                paramater.Add("@UserID", objModel.UserID);
                paramater.Add("@FCMToken", objModel.deviceToken);
                paramater.Add("@UUID", objModel.UUID);
                paramater.Add("@LastActive", objModel.LastLoginDate);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@OpType", objModel.OpType);
                paramater.Add("@DynamicSalt", objModel.DynamicSalt);

                return con.Query<int>("spn_InsertUpdateAppUser", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public List<ApiAuthenticationModel> GetAppUser(string uuid)
        {
            ApiAuthenticationModel user = new ApiAuthenticationModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@UUID", uuid);

                return con.Query<ApiAuthenticationModel>("select UserID,UserType,FCMToken as deviceToken,SBranchID,Status,LastActive as LastLoginDate,UUID,DynamicSalt from [dbo].[AppUsers] where UUID =@UUID", paramater, null, true, 0, commandType: CommandType.Text).ToList();

            }
        }
        public int InsertNotification(NotificationModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@RecieverType", objModel.RecieverType);
                paramater.Add("@NotificationType", objModel.NotificationType);
                paramater.Add("@NotificationText", objModel.NotificationText);
                paramater.Add("@NotificationDateTime", objModel.NotificationDateTime);
                paramater.Add("@RecieverID", objModel.RecieverID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@Recievers", objModel.GetRecieverList());

                return con.Query<int>("sp_InsertNotification", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public async Task<List<YouTubeVideoModel>> GetYoutubeVideosParent(int StudentID, int SubjectID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SubjectID", SubjectID);
                return (await con.QueryAsync<YouTubeVideoModel>("sp_GetYouTubeVideoParent", paramater, null,  0, commandType: CommandType.StoredProcedure)).ToList();
            }
        }
    }
}