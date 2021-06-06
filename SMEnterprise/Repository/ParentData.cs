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
    public class ParentData
    {
        public IEnumerable<StudentModel> GetChilds(int ParentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                DateTime curDate = CommonUsage.GetCurrentDate();
                var paramater = new DynamicParameters();
                paramater.Add("@ParentID", ParentID);
                paramater.Add("@Year", curDate.Year);
                paramater.Add("@Month", curDate.Month);
                paramater.Add("@DayID","D"+ curDate.Day);
                return con.Query<StudentModel>("spnp_GetStudentsForParant", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public async Task<StudentStopsScreenModel> GetStudentRouteDetails(int StudentID,DateTime CurDate)
        {
            StudentStopsScreenModel objModel = new StudentStopsScreenModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@CurDate", CurDate);
                using (var multi = await con.QueryMultipleAsync("spn_GetStudentRouteStoppages", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Stops = multi.Read<RouteStoppageModel>().ToList();
                    objModel.Conductor = multi.Read<object>().SingleOrDefault();
                    objModel.VehicleRouteID = multi.Read<int>().SingleOrDefault();
                }
            }
            return  objModel;
        }
        public async Task<IEnumerable<object>> GetChildsObject(int ParentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                DateTime curDate = CommonUsage.GetCurrentDate();
                var paramater = new DynamicParameters();
                paramater.Add("@ParentID", ParentID);
                paramater.Add("@Year", curDate.Year);
                paramater.Add("@Month", curDate.Month);
                paramater.Add("@DayID", "D" + curDate.Day);
                return (await con.QueryAsync<object>("spnp_GetStudentsForParantApp", paramater, null,  0, CommandType.StoredProcedure)).ToList();
            }
        }
        #region Dashboard
        public DashboardModel GetDashboardData(int StudentID)
        {
            DashboardModel objModel = new DashboardModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                objModel.Attandance= con.Query<AttandanceModel>("sp_GetParentDashboardData", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
           
            return objModel;
        }
        public DashboardModel GetDashboardPageData(int StudentID,int ParentID)
        {
            DashboardModel objModel = new DashboardModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@ParentID", ParentID);
                paramater.Add("@CurrDate", CommonUsage.GetCurrentDate());
                using (var multi = con.QueryMultiple("sp_GetParentDashboardPageData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.StudentDetail = multi.Read<StudentModel>().SingleOrDefault();
                    objModel.Messages = multi.Read<MessageModel>().ToList();
                    objModel.Events = multi.Read<EventModel>().ToList();
                    objModel.Evaluations = multi.Read<EvaluationModel>().ToList();
                    objModel.Notices = multi.Read<NoticeModel>().ToList();
                    objModel.MonthAttandance = multi.Read<AttandanceCalenderModel>().ToList();
                }
            }
            return objModel;
        }
        public async Task<List<AttandanceCalenderModel>> GetStudentMonthlyAttandance(int StudentID, int Month,int Year)
        {
            DashboardModel objModel = new DashboardModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@Year", Year);
                paramater.Add("@Month", Month);

                return (await con.QueryAsync<AttandanceCalenderModel>("sp_GetMonthStudentAttandance", paramater, null, 0, CommandType.StoredProcedure)).ToList();

            }
        }
        public ParentLandingPageModel GetParentLandingPageData(int ParentID)
        {
            ParentLandingPageModel objModel = new ParentLandingPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ParentID", ParentID);
                paramater.Add("@CurrDate", CommonUsage.GetCurrentDate());
                using (var multi = con.QueryMultiple("sp_GetParentLandingPageData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentModel>().ToList();
                }
            }
            return objModel;
        }
        #endregion
        #region Payments
        public List<FeePaymentsModel> GetPayments(int ParentID,int Day,int Month,int Year)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Day", Day);
                paramater.Add("@Month", Month);
                paramater.Add("@Year", Year);
                paramater.Add("@ParentID", ParentID);
                return con.Query<FeePaymentsModel>("spnp_GetParentFeePayments", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
            
        }
        public async Task< List<object>> GetPaymentsNew(int ParentID, int Day, int Month, int Year)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ParentID", ParentID);
                paramater.Add("@QDate", new DateTime(Year,Month,Day));
                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());
                return (await con.QueryAsync<object>("spn_GetParentWiseStudentFeeSummeryNew", paramater, null,  0, CommandType.StoredProcedure)).ToList();
            }

        }
        public async Task< List<PaymentModel>> GetParentPayments(int ParentID, int Day, int Month, int Year)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ParentID", ParentID);
                return ( await con.QueryAsync<PaymentModel>("spn_GetParentFeePayments", paramater, null,  0, CommandType.StoredProcedure)).ToList();
            }

        }
        public async Task<List<PaymentDetailsModel>> GetPaymentDetails(int PaymentID,int Day,int Month,int Year,int StudentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@PaymentID", PaymentID);
                paramater.Add("@Day", Day);
                paramater.Add("@Month", Month);
                paramater.Add("@Year", Year);
                paramater.Add("@StudentID", StudentID);
                return (await con.QueryAsync<PaymentDetailsModel>("sp_GetParentFeePaymentDetails", paramater, null,  0, CommandType.StoredProcedure)).ToList();
            }

        }
        #endregion
        #region Performance
        public PerformanceParameterDetailModel GetStudentPerformanceDetails(PerformanceParameterDetailModel model)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                StartupModel objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", model.StudentID);
                paramater.Add("@StudentSessionUID", model.StudentSessionUID);
                paramater.Add("@EvaluationID", model.EvaluationID);
                paramater.Add("@EvaluationMode", objStartupModel.EvaluationMode);
                using (var multi = con.QueryMultiple("sp_GetPerentStudentPerformanceDetail", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    model.PerformanceParameters = multi.Read<PerformanceParameterModel>().ToList();
                    model.MainEvaluations = multi.Read<EvaluationModel>().ToList();
                    model.SubEvaluations = multi.Read<EvaluationModel>().ToList();
                    model.Result = multi.Read<ExamResultDetailModel>().ToList();
                    model.Student = multi.Read<StudentModel>().SingleOrDefault();
                    model.CGPA = multi.Read<decimal>().SingleOrDefault();
                    model.ClassName = multi.Read<string>().SingleOrDefault();
                    model.SectionName = multi.Read<string>().SingleOrDefault();
                    model.EvaluationName = multi.Read<string>().SingleOrDefault();
                    model.FilledParameter = multi.Read<int>().SingleOrDefault();
                    model.TotalParameter = multi.Read<int>().SingleOrDefault();
                    model.StudentSessionUID = multi.Read<int>().SingleOrDefault();
                    model.EvaluationID = multi.Read<int>().SingleOrDefault();
                    model.StudentSessions= multi.Read<NameIDModel>().ToList();
                }
            }
            return model;
        }
        public ParentStudentPerformancePage GetStudentPerformancePage(ParentStudentPerformancePage model)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", model.StudentID);
                paramater.Add("@StudentSessionUID", model.StudentSessionUID);
                using (var multi = con.QueryMultiple("sp_GetParentStudentResultPage", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    model.Evaluations = multi.Read<EvaluationModel>().ToList();
                    model.Exams = multi.Read<NameIDModel>().ToList();
                    model.Student = multi.Read<StudentModel>().SingleOrDefault();
                    model.ClassName = multi.Read<string>().SingleOrDefault();
                    model.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return model;
        }
        public TimeTablePageModel GetStudentTimeTable(int StudentID, DateTime CurrDate, int SBranchID)
        {
            TimeTablePageModel objModel = new TimeTablePageModel();
            objModel.TimeTable = new TimeTableModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@CurrDate", CurrDate);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetStudentFullTimeTable", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                   
                    if (objModel.TimeTable == null)
                    {
                        objModel.TimeTable = new TimeTableModel();
                    }
                    else
                    {
                        objModel.TimeTable = multi.Read<TimeTableModel>().SingleOrDefault();
                        objModel.TimeTable.Periods = multi.Read<PeriodModel>().ToList();
                        objModel.TimeTable.Subjects = multi.Read<SubjectModel>().ToList();
                        objModel.TimeTable.Lactures = multi.Read<TimeTablePeriodModel>().ToList();
                      
                            objModel.TimeTable.Substitutions = multi.Read<TeacherSubstitutionModel>().ToList();
                       
                       
                            objModel.TimeTable.MergedClasses = multi.Read<TimeTableMergedClassModel>().ToList();
                        
                    }
                   
                   
                }
            }
            return objModel;
        }
        #endregion
        #region
        public async Task<List<NoticeModel>> GetNotices(int ParentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ParentID", ParentID);
                return (await con.QueryAsync<NoticeModel>("sp_GetParentNotices", paramater, null,  0, CommandType.StoredProcedure)).ToList();
            }

        }
        public async Task<List<ParentDiaryModel>> GetParentDiary(int ParentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ParentID", ParentID);
                return (await con.QueryAsync<ParentDiaryModel>("sp_GetParentDiary", paramater, null,  0, CommandType.StoredProcedure)).ToList();
            }

        }
        public async Task< ParentAPITimeTableModel> GetParentApiTTDayLactures(ParentApiParamModel objParam)
        {
            ParentAPITimeTableModel objData = new ParentAPITimeTableModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", objParam.ID);
                paramater.Add("@CurrDate", CommonUsage.GetCurrentDate());
                paramater.Add("@DayID", objParam.DayID);
                using (var multi = await con.QueryMultipleAsync("spn_GetStudentTimeTable", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objData.Days = multi.Read<int>().SingleOrDefault();
                    objData.Substitutions = multi.Read<ParentAPITTSubstitutions>().ToList();
                    objData.Periods = multi.Read<PeriodModel>().ToList();
                    objData.MergedClasses = multi.Read<TimeTableMergedClassModel>().ToList();
                }
            }
            return objData;
        }
        #endregion
        #region Leave management
        public async Task<int> AddStudentLeave(StudentLeaveModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@LeaveType", objData.LeaveType);
                paramater.Add("@StartDate", objData.StartDate);
                paramater.Add("@EndDate", objData.EndDate);
                paramater.Add("@LeaveReason", objData.LeaveReason);
                paramater.Add("@ApplicantID", objData.StudentID);
                paramater.Add("@IsApproved", objData.IsApproved);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@CreatedDate", objData.CreatedDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                return (await con.QueryAsync<int>("sp_InsertStudentLeave", paramater, null,  0, CommandType.StoredProcedure)).SingleOrDefault();
            }

        }
        #endregion
        #region Update Contact Details
        public async Task<int> UpdateContactDetails(ParentApiParamModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ParentID", objData.ID);
                paramater.Add("@EmailID", objData.EmailID);
                paramater.Add("@MobileNo", objData.MobileNumber);
                paramater.Add("@UpdateFor", objData.UpdateDetailsFor);
                return (await con.QueryAsync<int>("sp_UpdateParentContactDetails", paramater, null,  0, CommandType.StoredProcedure)).SingleOrDefault();
            }
        }
        #endregion
        public async Task<List<HolidayModel>> GetParentHolidays(int ParentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ParentID", ParentID);
                return (await con.QueryAsync<HolidayModel>("sp_GetParentHolidays", paramater, null,  0, CommandType.StoredProcedure)).ToList();
            }
        }
        public List<HolidayModel> GetTeacherHolidays(int TeacherID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                return con.Query<HolidayModel>("sp_GetTeacherHolidays", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public async Task<TransportMapModel> GetStudentTransportMap(int StudentID)
        {
            TransportMapModel objModel = new TransportMapModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                using (var multi = await con.QueryMultipleAsync("sp_GetStudentTransportMapDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<TransportMapModel>().SingleOrDefault();
                    if (objModel != null)
                    {
                        objModel.Stops = multi.Read<RouteStoppageModel>().ToList();
                        objModel.Driver = multi.Read<TransportEmployeeModel>().SingleOrDefault();
                        objModel.Conductor = multi.Read<TransportEmployeeModel>().SingleOrDefault();
                    }
                }
                
            }
            return objModel;
        }
        public async Task<TeacherBlackBoardPageModel> GetBlackBoardListModel(int StudentID,int SubjectID,int PageID)
        {
            TeacherBlackBoardPageModel objModel = new TeacherBlackBoardPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SubjectID", SubjectID);
                paramater.Add("@PageID", PageID);
                using (var multi = await con.QueryMultipleAsync("sp_GetBlackBoardByStudent", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.BlackBoardEntries = multi.Read<object>().ToList();
                }
            }
            return objModel;
        }
        public async Task<List<NameIDModel>> GetSubjectsForStudent(int StudentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                return (await con.QueryAsync<NameIDModel>("sp_GetSubjectsForStudent", paramater, null,  0, CommandType.StoredProcedure)).ToList();
            }
        }
        public async Task<int> SubmitAssignmentResponse(AssignmentSubmissionModel oModel)
        {
            TeacherBlackBoardPageModel objModel = new TeacherBlackBoardPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@AssResponseID", oModel.AssResponseID);
                paramater.Add("@AssignmentID", oModel.AssignmentID);
                paramater.Add("@StudentID", oModel.StudentID);
                paramater.Add("@SubmissionDate", oModel.SubmissionDate);
                paramater.Add("@Description", oModel.Description);
                paramater.Add("@Status", oModel.Status);
                paramater.Add("@Reason", oModel.Reason);
                paramater.Add("@Attachments", oModel.Attachments);
                return ( await con.QueryAsync<int>("sp_InsertParentAssignmentSubmission", paramater, null,  0, CommandType.StoredProcedure)).SingleOrDefault();
            }
        }
        public int UpdateAssignmentResponseAttachment(string AssResponseID, string Attachments)
        {
            TeacherBlackBoardPageModel objModel = new TeacherBlackBoardPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@AssResponseID",AssResponseID);
                paramater.Add("@Attachments", Attachments);
                return con.Query<int>("sp_UpdateParentAssignmentSubmissionAttachment", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public async Task<AssignmentSubmissionModel> GetStudentAssignmentResponse(int StudentID, int AssignmentID)
        {
            TeacherBlackBoardPageModel objModel = new TeacherBlackBoardPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@AssignmentID", AssignmentID);
                paramater.Add("@StudentID", StudentID);
                return (await con.QueryAsync<AssignmentSubmissionModel>("sp_GetStudentAssignmentResponse", paramater, null,  0, CommandType.StoredProcedure)).SingleOrDefault();
            }
        }
        #region Online Exams
        public void GetStudentOnlineExams(StudentOnlineExamListPageModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", oModel.StudentID);
                paramater.Add("@SubjectID", oModel.SubjectID);
                using (var multi = con.QueryMultiple("sp_GetStudentOnlineExams", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Exams = multi.Read<OnlineExamModel>().ToList();
                    oModel.Subjects = multi.Read<NameIDModel>().ToList();
                    oModel.SubjectID = multi.Read<int>().SingleOrDefault();
                }
            }
        }
        public OnlineExamEditModel GetStudentOnlineExamDetails(int OExamID)
        {
            OnlineExamEditModel oData = new OnlineExamEditModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OExamID", OExamID);
                using (var multi = con.QueryMultiple("sp_GetStudentOnlineExamsDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oData.Exam = multi.Read<OnlineExamModel>().SingleOrDefault();
                    oData.ExamQuestions = multi.Read<QuestionBankModel>().ToList();
                }
            }
            return oData;
        }
        public int SubmitOnlineExamAnswerSheet(StudentOnlineExamSubmitModel oModel)
        {
            TeacherBlackBoardPageModel objModel = new TeacherBlackBoardPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OExamID", oModel.OExamID);
                paramater.Add("@SubmissionDate", oModel.SubmissionDate);
                paramater.Add("@StudentID", oModel.StudentID);
                paramater.Add("@Answers", oModel.GetAnswersDatatable());
                return con.Query<int>("sp_StudentSubmitOnlineAnswerSheet", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public OnlineExamEditModel GetStudentOnlineExamAnswerSheet(int OExamID,int SubmissionID,int StudentID)
        {
            OnlineExamEditModel oData = new OnlineExamEditModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SubmissionID", SubmissionID);
                paramater.Add("@OExamID", OExamID);
                paramater.Add("@StudentID", StudentID);
                using (var multi = con.QueryMultiple("sp_GetStudentOnlineExamsAnswerSheet", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oData.Exam = multi.Read<OnlineExamModel>().SingleOrDefault();
                    oData.ExamQuestions = multi.Read<QuestionBankModel>().ToList();
                }
            }
            return oData;
        }
        #endregion
    }

}