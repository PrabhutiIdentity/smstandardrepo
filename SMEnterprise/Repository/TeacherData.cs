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
    public class TeacherData
    {
        public ParentAPITimeTableModel GetParentApiTTDayLactures(ParentApiParamModel objParam)
        {
            ParentAPITimeTableModel objData = new ParentAPITimeTableModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objParam.ID);
                paramater.Add("@CurrDate", CommonUsage.GetCurrentDate());
                paramater.Add("@DayID", objParam.DayID);
                using (var multi = con.QueryMultiple("spn_GetStudentTimeTable", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objData.Days = multi.Read<int>().SingleOrDefault();
                    objData.Substitutions = multi.Read<ParentAPITTSubstitutions>().ToList();
                    objData.Periods = multi.Read<PeriodModel>().ToList();
                    objData.MergedClasses = multi.Read<TimeTableMergedClassModel>().ToList();
                }
            }
            return objData;
        }
        public TimeTablePageModel GetTeacherTimeTable(int TeacherID, DateTime CurrDate, int SBranchID)
        {
            TimeTablePageModel objModel = new TimeTablePageModel();
            objModel.TimeTable = new TimeTableModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@CurrDate", CurrDate);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetTeacherTimeTableByTeacher", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.TimeTable = multi.Read<TimeTableModel>().SingleOrDefault();
                    objModel.TimeTable.Periods = multi.Read<PeriodModel>().ToList();
                    objModel.TimeTable.Subjects = multi.Read<SubjectModel>().ToList();
                    objModel.TimeTable.Lactures = multi.Read<TimeTablePeriodModel>().ToList();
                    objModel.TimeTable.Substitutions = multi.Read<TeacherSubstitutionModel>().ToList();
                    objModel.TeacherID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        #region Teacher Student Details
        public StudentsPageModel GetStudentClassReport(StudentsPageModel objModel)
        {
            
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@SessionID", objModel.SessionID);
                using (var multi = con.QueryMultiple("spn_GetTeacherStudentClassSectionDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.BranchDetails = multi.Read<SBranchModel>().SingleOrDefault();
                    
                }
            }
            return objModel;
        }
        

        #endregion
        #region Fee Module
        public StudentAttandancePageModel GetStudentAttandance(StudentAttandancePageModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@Day", objModel.Day);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@TeacherID", objModel.TeacherID);
                using (var multi = con.QueryMultiple("sp_GetStudentAttandanceDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.StudentAttandances = multi.Read<StudentAttandanceModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    if (objModel.Classes.Count != 0)
                    {
                        objModel.ClassID = multi.Read<int>().SingleOrDefault();
                        objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    }
                }
            }
            return objModel;
        }
        public int UpdateStudentAttandance(StudentAttandancePageModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Day", objModel.Day);
                paramater.Add("@List", objModel.GetAttandanceDetailsDataTable());
                paramater.Add("@SBranchID", objModel.SBranchID);
                return con.Query<int>("sp_UpdateStudentAttandance", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public IEnumerable<NameIDModel> GetTeacherClassSections(int TeacherID, int ClassID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@ClassID", ClassID);
                return con.Query<NameIDModel>("sp_GetSectionsByTeacher", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        #endregion
        #region Assignment Management
        public AssignmentPageModel GetTeacherAssignments(AssignmentPageModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@ChapterID", objModel.ChapterID);
                paramater.Add("@TeacherID", objModel.TeacherID);
                using (var multi = con.QueryMultiple("sp_GetAssignments", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Subjects = multi.Read<NameIDModel>().ToList();
                    objModel.Chapters = multi.Read<NameIDModel>().ToList();
                    objModel.Assignments = multi.Read<AssignmentModel>().ToList();
                    if (objModel.Classes.Count > 0)
                    {
                        objModel.ClassID = multi.Read<int>().SingleOrDefault();
                        objModel.SectionID = multi.Read<int>().SingleOrDefault();
                        objModel.SubjectID = multi.Read<int>().SingleOrDefault();
                    }
                }
            }
            return objModel;
        }
        public List<AssignmentModel> GetTeacherAssignmentList(AssignmentPageModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@ChapterID", objModel.ChapterID);
                paramater.Add("@TeacherID", objModel.TeacherID);
                return con.Query<AssignmentModel>("sp_GetAssignmentList", paramater, null, true, 0, CommandType.StoredProcedure).ToList();

            }
        }
        //For getting sections in which Teacher Teaches
        public IEnumerable<NameIDModel> GetSectionsForTeacherClass(int TeacherID, int ClassID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@ClassID", ClassID);
                return con.Query<NameIDModel>("spn_GetSectionsForTeacherClass", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        //For getting sections in which Teacher Teaches
        public IEnumerable<NameIDModel> GetSubjectsForTeacherSection(int TeacherID, int SectionID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@SectionID", SectionID);
                return con.Query<NameIDModel>("spn_GetSubjectsForTeacherSections", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        //For getting Chapters for Subjects
        public IEnumerable<NameIDModel> GetChaptersForSubject(int ClassID, int SubjectID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SubjectID", SubjectID);
                return con.Query<NameIDModel>("spn_GetChaptersForSubject", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public IEnumerable<NameIDModel> GetChaptersTopics(int ChapterID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ChapterID", ChapterID);
                return con.Query<NameIDModel>("sp_GetChapterTopicNames", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int UpdateAssignment(AssignmentModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", objModel.ID);
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@ChapterID", objModel.ChapterID);
                paramater.Add("@TopicID", objModel.TopicID);
                paramater.Add("@StartDate", objModel.StartDate);
                paramater.Add("@EndDate", objModel.EndDate);
                paramater.Add("@GracedDays", objModel.GracedDays);
                paramater.Add("@TaskType", objModel.TaskType);
                paramater.Add("@Title", objModel.Title);
                paramater.Add("@Detail", objModel.Detail);
                paramater.Add("@Attachments", objModel.Attachments);
                paramater.Add("@SendMail", objModel.SendMail);
                paramater.Add("@OperationDate", objModel.OperationDate);
                paramater.Add("@OpType", objModel.OpType);
                return con.Query<int>("spn_InsertUpdateAssignment", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public int UpdateAssignmentAttachment(string ID, string Attachments)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", ID);
                paramater.Add("@Attachments", Attachments);
                return con.Query<int>("spn_InsertUpdateAssignmentFiles", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public AssignmentSubmissionListPage GetAssignmentSubmissions(int AssignmentID)
        {
            AssignmentSubmissionListPage objModel = new AssignmentSubmissionListPage();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@AssignmentID", AssignmentID);
                using (var multi = con.QueryMultiple("sp_GetAssignmentSubmissions", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.ClassName = multi.Read<string>().SingleOrDefault();
                    objModel.SectionName = multi.Read<string>().SingleOrDefault();
                    objModel.SubjectName = multi.Read<string>().SingleOrDefault();
                    objModel.ChapterName = multi.Read<string>().SingleOrDefault();
                    objModel.TopicName = multi.Read<string>().SingleOrDefault();
                    objModel.AssignmentTitle = multi.Read<string>().SingleOrDefault();
                    objModel.Submissions = multi.Read<AssignmentSubmissionModel>().ToList();
                }
            }
            return objModel;
        }
        public AssignmentSubmissionModel GetAssignmentSubmissionDetails(int AssResponseID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@AssResponseID", AssResponseID);
                return con.Query<AssignmentSubmissionModel>("sp_GetAssignmentSubmissionDetails", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int UpdateTeacherAssSubmissionResponse(AssignmentSubmissionModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@AssignmentSubmissionID", objModel.AssResponseID);
                paramater.Add("@Status", objModel.Status);
                paramater.Add("@Comments", objModel.Comments);
                paramater.Add("@Grade", objModel.Grade);
                paramater.Add("@Marks", objModel.Marks);
                return con.Query<int>("sp_UpdateTeacherAssignmentSubmissions", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #endregion
        #region Exam Result Module
        public TeacherResultPageModel GetTeacherExamResults(TeacherResultPageModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                StartupModel objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@EvaluationID", objModel.EvaluationID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@EvaluationMode",0);
              //  paramater.Add("@EvaluationMode", objStartupModel.EvaluationMode);
                using (var multi = con.QueryMultiple("sp_GetClassGroupWiseExamResults", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Subjects = multi.Read<NameIDModel>().ToList();
                    objModel.Evaluations = multi.Read<NameIDModel>().ToList();
                    objModel.ExamResults = multi.Read<ExamResultDetailModel>().ToList();
                    if (objModel.Classes.Count > 0)
                    {
                        objModel.ClassID = multi.Read<int>().SingleOrDefault();
                        objModel.SectionID = multi.Read<int>().SingleOrDefault();
                        objModel.SubjectID = multi.Read<int>().SingleOrDefault();
                        objModel.EvaluationID = multi.Read<int>().SingleOrDefault();
                        //objModel.IsLocked = multi.Read<int>().SingleOrDefault();
                    }
                    try
                    {
                        objModel.MarkingScheme = multi.Read<int>().SingleOrDefault();

                    }
                    catch
                    {

                    }
                }
            }
            return objModel;
        }
        public int UpdateStudentResults(TeacherResultPageModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                DataTable dtResult = objModel.GetResultDetailsDataTable();
                paramater.Add("@ExamResultDetails", dtResult);
                return con.Query<int>("sp_UpdateExamResults", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public StudentPerformanceListModel GetStudentsPerformance(StudentPerformanceListModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                StartupModel objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@EvaluationID", objModel.EvaluationID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@EvaluationMode", objStartupModel.EvaluationMode);
                using (var multi = con.QueryMultiple("sp_GetClassSectionWiseStudentPerformances", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Evaluations = multi.Read<NameIDModel>().ToList();
                    objModel.StudentPerformances = multi.Read<StudentPerformanceModel>().ToList();
                    if (objModel.Classes.Count > 0)
                    {
                        objModel.ClassID = multi.Read<int>().SingleOrDefault();
                        objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    }
                    objModel.EvaluationID = multi.Read<int>().SingleOrDefault();
                    objModel.TotalParameters = multi.Read<int>().SingleOrDefault();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.SubEvaluations = multi.Read<NameIDModel>().ToList();
                }
            }
            return objModel;
        }
<<<<<<< HEAD
        public PerformanceParameterDetailModel GetStudentPerformanceDetails(PerformanceParameterDetailModel model)
=======
    
        public PerformanceParameterDetailModel GetStudentPerformanceDetails(PerformanceParameterDetailModel model)
         {
             using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
             {
                 StartupModel objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];
                 var paramater = new DynamicParameters();
                 paramater.Add("@StudentSessionUID", model.StudentSessionUID);
                 paramater.Add("@EvaluationID", model.EvaluationID);
                 paramater.Add("@SBranchID", model.SBranchID);
                 paramater.Add("@EvaluationMode", objStartupModel.EvaluationMode);
                 using (var multi = con.QueryMultiple("sp_GetStudentEvaluationPerformanceDetail", paramater, null, 0, commandType: CommandType.StoredProcedure))
                 {
                     model.PerformanceParameters = multi.Read<PerformanceParameterModel>().ToList();
                     model.MainEvaluations = multi.Read<EvaluationModel>().ToList();
                     model.SubEvaluations = multi.Read<EvaluationModel>().ToList();
                     model.Result = multi.Read<ExamResultDetailModel>().ToList();
                     //try
                     //{

                     //    model.ResultGrade = multi.Read<ExamResultDetailModel>().ToList();
                     //}
                     //catch { }


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
       
        // for rank
       
      /*  public PerformanceParameterDetailModel GetStudentPerformanceDetails(PerformanceParameterDetailModel model)
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                StartupModel objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];
                var paramater = new DynamicParameters();
                paramater.Add("@StudentSessionUID", model.StudentSessionUID);
                paramater.Add("@EvaluationID", model.EvaluationID);
                paramater.Add("@SBranchID", model.SBranchID);
                paramater.Add("@EvaluationMode", objStartupModel.EvaluationMode);
                using (var multi = con.QueryMultiple("sp_GetStudentEvaluationPerformanceDetail", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    model.PerformanceParameters = multi.Read<PerformanceParameterModel>().ToList();
                    model.MainEvaluations = multi.Read<EvaluationModel>().ToList();
                    model.SubEvaluations = multi.Read<EvaluationModel>().ToList();
                    model.Result = multi.Read<ExamResultDetailModel>().ToList();
                    //try
                    //{
<<<<<<< HEAD
                       
=======

>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
                    //    model.ResultGrade = multi.Read<ExamResultDetailModel>().ToList();
                    //}
                    //catch { }


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
<<<<<<< HEAD
                       
                    }
                    catch { }
                }
            }
            return model;
        }
=======

                    }
                    catch { }
                }
                    // ── new: rank SP call ────────────────────────────────
                    // SessionID comes from the existing SP result via Student,
                    // but Student.SessionID may be 0 — use model.SessionID directly
                    var rankParams = new DynamicParameters();
                rankParams.Add("@StudentSessionUID", model.StudentSessionUID);
                // rankParams.Add("@EvaluationID", model.EvaluationID == 0 ? -1 : model.EvaluationID);
                rankParams.Add("@EvaluationID", -1);   // always load ALL terms
                rankParams.Add("@SBranchID", model.SBranchID);
                rankParams.Add("@SessionID", model.SessionID);

                using (var rankMulti = con.QueryMultiple("sp_GetStudentRankInSection_test",rankParams,commandType: CommandType.StoredProcedure))
                {
                    model.TermRanks = rankMulti.Read<StudentTermRankModel>().ToList();
                    model.OverallRank = rankMulti.Read<StudentOverallRankModel>().SingleOrDefault();
                }
            }
            return model;
        }
        */
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
        // for shine
        public StudentPerformanceResultModel GetStudentPerformanceResult(StudentPerformanceResultModel model)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                StartupModel objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];
                var paramater = new DynamicParameters();
                paramater.Add("@StudentSessionUID", model.StudentSessionUID);
                paramater.Add("@EvaluationID", model.EvaluationID);
                paramater.Add("@SBranchID", model.SBranchID);
                paramater.Add("@EvaluationMode", objStartupModel.EvaluationMode);
                paramater.Add("@SessionID", model.SessionID);
                using ( var multi = con.QueryMultiple("sp_GetStudentEvaluationPerformanceDetail2", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    model.MainEvaluations = multi.Read<EvaluationModel>().ToList();
                    model.SubEvaluations = multi.Read<EvaluationModel>().ToList();
                    model.Subjects = multi.Read<SubjectModel>().ToList();
                    model.SubjectType = multi.Read<SubSubjectTypeModel>().ToList();
                    model.Exams = multi.Read<ExamModel>().ToList();
                    model.Result = multi.Read<ExamResultDetailModel>().ToList();
                    model.Student = multi.Read<StudentModel>().SingleOrDefault();
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
        public TotalResultModel GetStudentTotalResult(int StudentID,int SessionID)
        {
            TotalResultModel model = new TotalResultModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("sp_getStudentTotalResult", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    model.Subjects = multi.Read<SubjectModel>().ToList();
                    model.Evaluations = multi.Read<EvaluationModel>().ToList();
                    model.Exams = multi.Read<ExamModel>().ToList();
                    model.Result = multi.Read<ExamResultDetailModel>().ToList();
                    model.Student = multi.Read<StudentModel>().SingleOrDefault();
                    model.ClassName = multi.Read<string>().SingleOrDefault();
                    model.SectionName = multi.Read<string>().SingleOrDefault();
                    model.Session= multi.Read<SchoolSessionModel>().SingleOrDefault();
                }
            }
            return model;
        }
        public int UpdateStudentPerformance(PerformanceParameterDetailModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@PerformanceDetails", objModel.GetPerformanceValuesDatatable());
                return con.Query<int>("sp_UpdateStudentPerformanceDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public PerformanceParameterDetailModel GetStudentResultDetails(PerformanceParameterDetailModel model)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentSessionUID", model.StudentSessionUID);
                paramater.Add("@EvaluationID", model.EvaluationID);
                paramater.Add("@SBranchID", model.SBranchID);
                paramater.Add("@SessionID", model.SessionID);
                using (var multi = con.QueryMultiple("sp_GetStudentEvaluationResult", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    model.Result = multi.Read<ExamResultDetailModel>().ToList();
                    model.Student = multi.Read<StudentModel>().SingleOrDefault();
                    model.EvaluationName = multi.Read<string>().SingleOrDefault();
                    model.SBranchDetails = multi.Read<SBranchModel>().SingleOrDefault();
                    model.Subjects = multi.Read<SubjectModel>().ToList();
                    model.SubjectTypes = multi.Read<NameIDModel>().ToList();
                    try
                    {
                        model.SessionName = multi.Read<string>().SingleOrDefault();
                        model.PerformanceParameters = multi.Read<PerformanceParameterModel>().ToList();
                    }
                    catch { }
                }
            }
            return model;
        }
        #endregion
        #region ParentDiary Module
        public List<ParentDiaryModel> GetTeacherParentDiaryEntries(int TeacherID, int ClassID, int SectionID,int SubjectID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SubjectID", SubjectID);
                return con.Query<ParentDiaryModel>("sp_GetTeacherParentDiaryList", paramater, null, true, 0, CommandType.StoredProcedure).AsList();
            }
        }
        public List<StudentModel> GetTeacherParentDiaryStudentList(int ClassID, int SectionID, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SearchText", "");
                return con.Query<StudentModel>("sp_GetStudentsByClassSection", paramater, null, true, 0, CommandType.StoredProcedure).AsList();
            }
        }
        public ParentDiaryDetailModel GetTeacherParentDiaryDetails(int TeacherID, int MasterID)
        {
            ParentDiaryDetailModel objModel = new ParentDiaryDetailModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@MasterID", MasterID);
                using (var multi = con.QueryMultiple("sp_GetTeacherParentDiaryRecieverList", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Details= multi.Read<ParentDiaryModel>().SingleOrDefault();
                    objModel.Students = multi.Read<ParentDiaryModel>().ToList();
                }
            }
            return objModel;
        }
        public ParentDiaryModel GetStudentParentDiarySubject(ParentDiaryModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_GetSubjectParentDiaryList", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList(); 
                    objModel.Subjects = multi.Read<NameIDModel>().ToList();
                    objModel.Students = multi.Read<StudentModel>().ToList();
                    if (objModel.Classes.Count != 0)
                    {
                        objModel.ClassID = multi.Read<int>().SingleOrDefault();
                        objModel.SectionID = multi.Read<int>().SingleOrDefault();
                        objModel.SubjectID = multi.Read<int>().SingleOrDefault();
                    }
                }
            }
            return objModel;
        }
        public ParentDiaryModel GetStudentParentDiaryClass(ParentDiaryModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_GetClassParentDiaryList", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Students = multi.Read<StudentModel>().ToList();
                    if (objModel.Classes.Count != 0)
                    {
                        objModel.ClassID = multi.Read<int>().SingleOrDefault();
                        objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    }
                }
            }
            return objModel;
        }
        public int DeleteParentDiary(int PBID, int TeacherID)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@PBID", PBID);
                paramater.Add("@TeacherID", TeacherID);
                return con.Query<int>("sp_DeleteParentDiary", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public int UpdateParentDiaryBulk(ParentDiaryModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                DataTable dtResult = objModel.GetStudentsDataTable();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@TeacherType", objModel.TeacherType);
                paramater.Add("@PBDate", objModel.PBDate);
                paramater.Add("@Title", objModel.Title);
                paramater.Add("@Description", objModel.Description);
                paramater.Add("@Priority", objModel.Priority);
                paramater.Add("@IsRead", objModel.IsRead);
                paramater.Add("@MasterID", objModel.MasterID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@Students", dtResult);
                return con.Query<int>("sp_AddBulkParentDiary", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public int UpdateParentDiarySingle(ParentDiaryModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@TeacherType", objModel.TeacherType);
                paramater.Add("@StudentID", objModel.StudentID);
                paramater.Add("@PBDate", objModel.PBDate);
                paramater.Add("@Title", objModel.Title);
                paramater.Add("@Description", objModel.Description);
                paramater.Add("@Priority", objModel.Priority);
                paramater.Add("@IsRead", objModel.IsRead);
                paramater.Add("@MasterID", objModel.MasterID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                return con.Query<int>("sp_AddParentDiaryNote", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        #endregion
        #region Attandance 
        public IEnumerable<ClassSectionModel> GetClassTeacherClassSections(int TeacherID, int Day, int Month, int Year)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Day", Day);
                paramater.Add("@Month", Month);
                paramater.Add("@Year", Year);
                paramater.Add("@TeacherID", TeacherID);
                return con.Query<ClassSectionModel>("sp_GetClassTeacherClassSections", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public IEnumerable<StudentAttandanceModel> GetAppStudentAttandanceList(int ClassID, int SectionID, int Day, int Month, int Year, int SBranchID)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Month", Month);
                paramater.Add("@Year", Year);
                paramater.Add("@Day", Day);
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<StudentAttandanceModel>("sp_GetTAppStudentAttandanceListOnClassSection", paramater, null, true, 0, CommandType.StoredProcedure).ToList();

            }
        }
        public IEnumerable<NoticeModel> GetNotices(int SBranchID)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<NoticeModel>("sp_GetTeacherNotices", paramater, null, true, 0, CommandType.StoredProcedure).ToList();

            }
        }
        public IEnumerable<EventModel> GetEvents(int SBranchID,int TeacherID)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@TeacherID", TeacherID);
                return con.Query<EventModel>("sp_GetTeacherEvents", paramater, null, true, 0, CommandType.StoredProcedure).ToList();

            }
        }
        #endregion
        #region API Assignment
        public IEnumerable<ClassSectionModel> GetTeacherTeachingClassSections(int TeacherID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                return con.Query<ClassSectionModel>("sp_GetTeacherTeachingClassSections", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int DeleteAssignment(int ID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", ID);   
                return con.Query<int>("sp_DeleteAssignment", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public IEnumerable<NameIDModel> GetTeacherTeachingSubjectsOnClassSections(int TeacherID, int ClassID, int SectionID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@TeacherID", TeacherID);
                return con.Query<NameIDModel>("sp_GetTeacherTeachingSubjectsOnClassSection", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        #endregion
        #region EmployeeLeaves
        public IEnumerable<EmployeeLeaveModel> GetEmployeeLeaves(int EmployeeID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", EmployeeID);
                return con.Query<EmployeeLeaveModel>("sp_GetLeavesOnEmployee", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public List<EmployeeLeaveSummery> GetEmployeeLeavesEditDetails(int SBranchID, int Month, int Year, int EmployeeID)
        {
            List<EmployeeLeaveSummery> objModel = new List<EmployeeLeaveSummery>(); 
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@Month", Month);
                paramater.Add("@Year", Year);
                paramater.Add("@EmployeeID", EmployeeID);
                using (var multi = con.QueryMultiple("sp_GetEmployeeLeavesEmployee", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<EmployeeLeaveSummery>().ToList();
                }
            }
            return objModel;
        }
        public ParentAPITimeTableModel GetTeacherApiTTDayLactures(ParentApiParamModel objParam)
        {
            ParentAPITimeTableModel objData = new ParentAPITimeTableModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objParam.ID);
                paramater.Add("@CurrDate", CommonUsage.GetCurrentDate());
                paramater.Add("@DayID", objParam.DayID);
                using (var multi = con.QueryMultiple("spn_GetTeacherTimeTableDayLactures", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objData.Days = multi.Read<int>().SingleOrDefault();
                    objData.Periods = multi.Read<PeriodModel>().ToList();
                }
            }
            return objData;
        }
        public TransportMapModel GetTeacherTransportMap(int TeacherID)
        {
            TransportMapModel objModel = new TransportMapModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                using (var multi = con.QueryMultiple("sp_GetTeacherTransportMapDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
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

        #endregion
        #region API Attandance
        public IEnumerable<AppAttandanceModel> GetAttandanceStatus(int EmployeeID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", EmployeeID);
                paramater.Add("@CurrDate", CommonUsage.GetCurrentDate());
                return con.Query<AppAttandanceModel>("spn_GetAppAttandanceStatus", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public IEnumerable<AppAttandanceModel> UpdateAppAttandance(AppAttandanceModel model)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", model.EmployeeID);
                paramater.Add("@CurrDate", CommonUsage.GetCurrentDate());
                paramater.Add("@Longitude", model.Longitude);
                paramater.Add("@Latitude", model.Latitude);
                paramater.Add("@Location", model.Location);
                paramater.Add("@UUID", model.UUID);
                return con.Query<AppAttandanceModel>("spn_UpdateAppAttandance", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        #endregion
        #region BlackBoardSection
        public TeacherBlackBoardPageModel GetBlackBoardPageModel(TeacherBlackBoardPageModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@EntryDate", objModel.EntryDate);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@PeriodID", objModel.PeriodID);
                using (var multi = con.QueryMultiple("sp_GetTeacherBlackBoardEditData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<TeacherBlackBoardPageModel>().SingleOrDefault();
                    objModel.SelectionList = multi.Read<object>().ToList();
                }
            }
            return objModel;
        }
        public async Task<BlackBoardModel> GetBlackBoardDetail(string BlackBoardID)
        {
            BlackBoardModel objModel = new BlackBoardModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@BlackBoardID", BlackBoardID);
                using (var multi = await con.QueryMultipleAsync("sp_GetBlackBoardDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<BlackBoardModel>().SingleOrDefault();
                    objModel.BBImages = multi.Read<BlackBoardImageModel>().ToList();
                }
            }
            return objModel;
        }
        public TeacherBlackBoardPageModel GetBlackBoardListModel(TeacherBlackBoardPageModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@EntryDate", objModel.EntryDate);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@PeriodID", objModel.PeriodID);
                using (var multi = con.QueryMultiple("sp_GetTeacherBlackBoardEntryList", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<TeacherBlackBoardPageModel>().SingleOrDefault();
                    objModel.BlackBoardEntries = multi.Read<object>().ToList();
                }
            }
            return objModel;
        }
        public int AddBlackBoardEntry(BlackBoardModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@BlackBoardID", oModel.BlackBoardID);
                paramater.Add("@TeacherID", oModel.TeacherID);
                paramater.Add("@SectionID", oModel.SectionID);
                paramater.Add("@ClassID", oModel.ClassID);
                paramater.Add("@PeriodID", oModel.PeriodID);
                paramater.Add("@SubjectID", oModel.SubjectID);
                paramater.Add("@Title", oModel.Title);
                paramater.Add("@BDate", oModel.BDate);
                paramater.Add("@SBranchID", oModel.SBranchID);

                return con.Query<int>("sp_InsertBlackBoardEntry", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int DeleteBlackBoardEntry(string ID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@BlackBoardID", ID);
                return con.Query<int>("sp_DeleteBlackBoardEntry", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int AddBlackBoardImage(BlackBoardImageModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@BlackBoardID", oModel.BlackBoardID);
                paramater.Add("@BBMIID", oModel.BBMIID);
                paramater.Add("@Photo", oModel.Photo);
                paramater.Add("@Detail", oModel.Detail);
                paramater.Add("@SBranchID", oModel.SBranchID);

                return con.Query<int>("sp_InsertBlackBoardImage", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int DeleteBlackBoardImage(string ID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@BBMIID", ID);
                return con.Query<int>("sp_DeleteBlackBoardImage", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #endregion
        #region YouTube and BlackBoard
        public YoutubeAdminEditPageData GetTeacherSectionSubjectAndStudents(int SectionID, int SessionID, int VideoID, int TeacherID ,int ClassID)
        {
            YoutubeAdminEditPageData oModel = new YoutubeAdminEditPageData();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SessionID", SessionID);
                paramater.Add("@VideoID", VideoID);
                using (var multi = con.QueryMultiple("sp_GetTeacherSectionSubjectsAndStudents", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Sections = multi.Read<NameIDModel>().ToList();
                    oModel.Subjects = multi.Read<NameIDModel>().ToList();
                    oModel.SubjectID = multi.Read<int>().SingleOrDefault();
                    oModel.SectionID = multi.Read<int>().SingleOrDefault();
                    oModel.Students = multi.Read<NameIDModel>().ToList();
                }
            }
            return oModel;
        }
        public YoutubeAdminEditPageData GetYouTubeVideosDetails(int SBranchID, int VideoID, int TeacherID)
        {
            YoutubeAdminEditPageData oModel = new YoutubeAdminEditPageData();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@VideoID", VideoID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetTeacherYouTubeVideoEditData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Classes = multi.Read<NameIDModel>().ToList();
                    oModel.Sections = multi.Read<NameIDModel>().ToList();
                    oModel.Subjects = multi.Read<NameIDModel>().ToList();
                    oModel.Video = multi.Read<YouTubeVideoModel>().SingleOrDefault();
                    oModel.Students = multi.Read<NameIDModel>().ToList();
                    oModel.Sessions = multi.Read<NameIDModel>().ToList();

                    oModel.SessionID = multi.Read<int>().SingleOrDefault();
                    oModel.ClassID = multi.Read<int>().SingleOrDefault();
                    oModel.SectionID = multi.Read<int>().SingleOrDefault();
                    oModel.SubjectID = multi.Read<int>().SingleOrDefault();

                    if (oModel.Video == null)
                    {
                        oModel.Video = new YouTubeVideoModel();
                    }
                }
            }
            return oModel;
        }
        public YoutubeAdminEditPageData GetTeacherYouTubeVideos(YoutubeAdminEditPageData objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_GetTeacherPanelYouTubeVideo", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Subjects = multi.Read<NameIDModel>().ToList();
                    if (objModel.Classes.Count != 0)
                    {
                        objModel.ClassID = multi.Read<int>().SingleOrDefault();
                        objModel.SectionID = multi.Read<int>().SingleOrDefault();
                        objModel.SubjectID = multi.Read<int>().SingleOrDefault();
                        objModel.Videos = multi.Read<YouTubeVideoModel>().ToList();
                    }
                }
            }
            return objModel;
        }
        #endregion
        #region Question Bank
        public int UpdateQuestionBank(QuestionBankModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@QuestionID", oModel.QuestionID);
                paramater.Add("@TeacherID", oModel.TeacherID);
                paramater.Add("@ClassID", oModel.ClassID);
                paramater.Add("@GroupID", oModel.GroupID);
                paramater.Add("@SubjectID", oModel.SubjectID);
                paramater.Add("@ChapterID", oModel.ChapterID);
                paramater.Add("@TopicID", oModel.TopicID);
                paramater.Add("@QuestionType", oModel.QuestionType);
                paramater.Add("@Complexity", oModel.Complexity);
                paramater.Add("@QuestionText", oModel.QuestionText);
                paramater.Add("@QuestionImage", oModel.QuestionImage);
                paramater.Add("@Option1", oModel.Option1);
                paramater.Add("@Option1Image", oModel.Option1Image);
                paramater.Add("@Option2", oModel.Option2);
                paramater.Add("@Option2Image", oModel.Option2Image);
                paramater.Add("@Option3", oModel.Option3);
                paramater.Add("@Option3Image", oModel.Option3Image);
                paramater.Add("@Option4", oModel.Option4);
                paramater.Add("@Option4Image", oModel.Option4Image);
                paramater.Add("@Answer", oModel.Answer);
                paramater.Add("@Explaination", oModel.Explaination);
                paramater.Add("@ExplainationImage", oModel.ExplainationImage);
                paramater.Add("@CreatedDate", oModel.CreatedDate);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@OpType", oModel.OpType);

                return con.Query<int>("sp_UpdateQuestionBank", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public void GetTeacherQuestionBank(TeacherQuestionBankModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@QuestionsBy", objModel.QuestionsBy);
                using (var multi = con.QueryMultiple("sp_GetTeacherQuestionBank", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Questions = multi.Read<QuestionBankModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.Subjects = multi.Read<NameIDModel>().ToList();
                }
            }
        }
        public void GetTeacherOnlineExams(OnlineExamPageModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_GetTeacherOnlineExams", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Exams = multi.Read<OnlineExamModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.Subjects = multi.Read<NameIDModel>().ToList();
                    objModel.SubjectID = multi.Read<int>().SingleOrDefault();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
        }
        public int DeleteOnlineExam(int OExamID,int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OExamID", OExamID);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<int>("spn_DeleteOnlineExam", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        
        public OnlineExamEditModel GetTeacherOnlineExamDetails(OnlineExamModel objModel)
        {
            OnlineExamEditModel oData = new OnlineExamEditModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@OExamID", objModel.OExamID);
                using (var multi = con.QueryMultiple("sp_GetTeacherOnlineExamsDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oData.Exam = multi.Read<OnlineExamModel>().SingleOrDefault();
                    oData.ExamQuestions = multi.Read<QuestionBankModel>().ToList();
                    oData.Questions = multi.Read<QuestionBankModel>().ToList();
                }
            }
            if(oData.Exam.OExamID==0)
            {
                oData.Exam.OExamStartDate = CommonUsage.GetCurrentDate();
                oData.Exam.OExamEndDate = oData.Exam.OExamStartDate.AddMinutes(30);
                oData.Exam.Status = 1;
            }
            return oData;
        }
        public int UpdateOnlineExam(OnlineExamModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OExamID", oModel.OExamID);
                paramater.Add("@OExamStartDate", oModel.OExamStartDate);
                paramater.Add("@OExamEndDate", oModel.OExamEndDate);
                paramater.Add("@SessionID", oModel.SessionID);
                paramater.Add("@ClassID", oModel.ClassID);
                paramater.Add("@SectionID", oModel.SectionID);
                paramater.Add("@SubjectID", oModel.SubjectID);
                paramater.Add("@TeacherID", oModel.TeacherID);
                paramater.Add("@CreatedDate", oModel.CreatedDate);
                paramater.Add("@Status", oModel.Status);
                paramater.Add("@ExamTitle", oModel.ExamTitle);
                paramater.Add("@ExamDescription", oModel.ExamDescription);
                paramater.Add("@ExamPriority", oModel.ExamPriority);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@Questions", oModel.GetQuestionsDatatable());
                paramater.Add("@OpType", oModel.OpType);

                return con.Query<int>("sp_UpdateOnlineExam", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public StudentOnlineExamSubmissionPageModel GetOnlineExamSubmissions(string OExamID)
        {
            StudentOnlineExamSubmissionPageModel oModel = new StudentOnlineExamSubmissionPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OExamID", OExamID);
                using (var multi = con.QueryMultiple("sp_GetTeacherOnlineExamSubmissions", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Exam = multi.Read<OnlineExamModel>().SingleOrDefault();
                    oModel.Submissions = multi.Read<StudentOnlineExamSubmitModel>().ToList();
                }
            }
            return oModel;
        }
        public OnlineExamEditModel GetStudentOnlineExamAnswerSheet(int SubmissionID)
        {
            OnlineExamEditModel oData = new OnlineExamEditModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SubmissionID", SubmissionID);
                using (var multi = con.QueryMultiple("sp_GetTeacherStudentOnlineExamsAnswerSheet", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oData.Exam = multi.Read<OnlineExamModel>().SingleOrDefault();
                    oData.ExamQuestions = multi.Read<QuestionBankModel>().ToList();
                }
            }
            return oData;
        }
        public int UpdateOnlineExamSubmissionStatus(StudentOnlineExamSubmitModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SubmissionID", oModel.SubmissionID);
                paramater.Add("@CheckDate", oModel.CheckDate);
                paramater.Add("@TeacherID", oModel.TeacherID);
                paramater.Add("@Status", oModel.Status);
                paramater.Add("@TeacherRemark", oModel.TeacherRemark);
                paramater.Add("@AnswerRemarks", oModel.GetAnswersDatatable()) ;

                return con.Query<int>("sp_UpdateRemarkOnStudentOnlineExamSubmission", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        #endregion
        public async Task<StudentStopsScreenModel> GetEmployeeRouteDetails(int EmployeeID, DateTime CurDate)
        {
            StudentStopsScreenModel objModel = new StudentStopsScreenModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", EmployeeID);
                paramater.Add("@CurDate", CurDate);
                using (var multi = await con.QueryMultipleAsync("spn_GetEmployeeRouteStoppages", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Stops = multi.Read<RouteStoppageModel>().ToList();
                    objModel.Conductor = multi.Read<object>().SingleOrDefault();
                    objModel.VehicleRouteID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }

    }
}