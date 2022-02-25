using Dapper;
using SMEnterprise.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace SMEnterprise.Repository
{
    public class ReceptionData
    {
        public AdmissionEnquiryPageModel GetEnquiries(AdmissionEnquiryPageModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@StartDate", objData.StartDate);
                paramater.Add("@EndDate", objData.EndDate);
                paramater.Add("@SessionID", objData.SessionID);
                paramater.Add("@ClassID", objData.ClassID);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@EStatus", objData.EStatus);
                paramater.Add("@EPossibility", objData.EPossibility);
                using (var multi = con.QueryMultiple("sp_GetAdmissionEnquiries", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objData.Classes = multi.Read<NameIDModel>().ToList();
                    objData.Sessions = multi.Read<NameIDModel>().ToList();
                    objData.Enquiries = multi.Read<AdmissionEnquiryMasterModel>().ToList();
                    objData.Receptionists = multi.Read<NameIDModel>().ToList();
                }
            }
            return objData;
        }
        public AdmissionEnquiryPageModel GetEnquiryDetails(int EnquiryID, int SBranchID)
        {
            AdmissionEnquiryPageModel objData = new AdmissionEnquiryPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EnquiryID", EnquiryID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetAdmissionEnquiryDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objData.Classes = multi.Read<NameIDModel>().ToList();
                    objData.Sessions = multi.Read<NameIDModel>().ToList();
                    objData.Enquiry = multi.Read<AdmissionEnquiryMasterModel>().SingleOrDefault();
                    objData.Receptionists = multi.Read<NameIDModel>().ToList();
                }
            }
            if (objData.Enquiry == null)
            {
                objData.Enquiry = new AdmissionEnquiryMasterModel();
                objData.Enquiry.EDate = CommonUsage.GetCurrentDate();
                objData.Enquiry.NextFollowUpDate = CommonUsage.GetCurrentDate().AddDays(1);
            }
            return objData;
        }
        public List<AdmissionEnquiryFollowupModel> GetEnquiryFollowups(int EnquiryID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EnquiryID", EnquiryID);

                return con.Query<AdmissionEnquiryFollowupModel>("sp_GetAdmissionEnquiryFollowups", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        public AdmissionEnquiryMasterModel AddEnquiryAssignee(AdmissionEnquiryMasterModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EnquiryID", oModel.EnquiryID);
                paramater.Add("@AssignedTo", oModel.AssignedTo);

                return con.Query<AdmissionEnquiryMasterModel>("sp_AddEnquiryAssignee", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public AdmissionEnquiryMasterModel UpdateEnquiry(AdmissionEnquiryMasterModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EnquiryID", objData.EnquiryID);
                paramater.Add("@EDate", CommonUsage.GetCurrentDate());
                paramater.Add("@FatherName", objData.FatherName);
                paramater.Add("@MotherName", objData.MotherName);
                paramater.Add("@FatherMobileNo", objData.FatherMobileNo);
                paramater.Add("@MotherMobileNo", objData.MotherMobileNo);
                paramater.Add("@FatherEmailID", objData.FatherEmailID);
                paramater.Add("@MotherEmailID", objData.MotherEmailID);
                paramater.Add("@FOccupation", objData.FOccupation);
                paramater.Add("@MOccupation", objData.MOccupation);
                paramater.Add("@FamilyIncome", objData.FamilyIncome);
                paramater.Add("@Address", objData.Address);
                paramater.Add("@StudentName", objData.StudentName);
                paramater.Add("@AppliedForSession", objData.AppliedForSession);
                paramater.Add("@AppliedForClass", objData.AppliedForClass);
                paramater.Add("@CurrentClass", objData.CurrentClass);
                paramater.Add("@ESource", objData.ESource);
                paramater.Add("@CurrentSchool", objData.CurrentSchool);
                paramater.Add("@CurrentSchoolAddress", objData.CurrentSchoolAddress);
                paramater.Add("@ReasonForChange", objData.ReasonForChange);
                paramater.Add("@Gender", objData.Gender);
                paramater.Add("@Nationality", objData.Nationality);
                paramater.Add("@CurrentEducationSystem", objData.CurrentEducationSystem);
                paramater.Add("@StudentDOB", objData.StudentDOB);
                paramater.Add("@EStatus", objData.EStatus);
                paramater.Add("@EPossibility", objData.EPossibility);
                paramater.Add("@NextFollowUpDate", CommonUsage.GetCurrentDate());
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@CreatedDate", CommonUsage.GetCurrentDate());
                paramater.Add("@StudentID", objData.StudentID);
                paramater.Add("@PaymentStatus", objData.PaymentStatus);
                paramater.Add("@PaymentDetails", objData.PaymentDetails);
                paramater.Add("@AssignedTo", objData.AssignedTo);
                paramater.Add("@Image", objData.Image);
                paramater.Add("@StudentAadharNo", objData.StudentAadharNo);
                paramater.Add("@FatherAadhaarNo", objData.FatherAadhaarNo);
                paramater.Add("@MotherAadhaarNo", objData.MotherAadhaarNo);
                paramater.Add("@Caste", objData.Caste);
                paramater.Add("@DateOfBirth", objData.StudentDOB);
                paramater.Add("@TelephoneNoOff", objData.TelephoneNoOff);
                paramater.Add("@TelephoneNoReg", objData.TelephoneNoReg);
                paramater.Add("@Sibling", objData.Sibling);
                paramater.Add("@SiblingName", objData.SiblingName);
                paramater.Add("@SiblingClass", objData.SiblingClass);
                paramater.Add("@MotherQuaAndOcc", objData.MotherQuaAndOcc);
                paramater.Add("@FatherQuaAndOcc", objData.FatherQuaAndOcc);
                paramater.Add("@TemporaryAddress", objData.TemporaryAddress);
                paramater.Add("@PermanentAddress", objData.PermanentAddress);
                paramater.Add("@Religion", objData.ReligionID);

                return con.Query<AdmissionEnquiryMasterModel>("spn_InsertUpdateAdmissionEnquiry", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int InsertAdmissionEnquiryFollowup(AdmissionEnquiryFollowupModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@FollowupID", objData.FollowupID);
                paramater.Add("@EnquiryID", objData.EnquiryID);
                paramater.Add("@FollowupBy", objData.FollowupBy);
                paramater.Add("@FollowupByName", objData.FollowupByName);
                paramater.Add("@Remark", objData.Remark);
                paramater.Add("@FDate", objData.FDate);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@Possibility", objData.Possibility);
                paramater.Add("@NextFollowupDate", objData.NextFollowupDate);
                paramater.Add("@CreatedDate", objData.CreatedDate);
                return con.Query<int>("spn_InsertAdmissionEnquiryFollowup", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int InsertUpdateToDoItem(ToDoItemModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ToDoID", objData.ToDoID);
                paramater.Add("@Priority", objData.Priority);
                paramater.Add("@ScheduledDateTime", objData.ScheduledDateTime);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@Details", objData.Details);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@CreatedDate", objData.CreatedDate);
                paramater.Add("@UserType", objData.UserType);
                paramater.Add("@OpType", objData.OpType);
                return con.Query<int>("spn_InsertUpdateToDoItem", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public ReceptionDashboardModel GetDashboard(int UserID, DateTime CurrentDate, int SBranchID, int UserType)
        {
            ReceptionDashboardModel objData = new ReceptionDashboardModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@UserID", UserID);
                paramater.Add("@CurrentDate", CurrentDate);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@UserType", UserType);
                using (var multi = con.QueryMultiple("spr_GetReceptionDashboard", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objData.ToDoList = multi.Read<ToDoItemModel>().ToList();
                    objData.ScheduledEnquiries = multi.Read<ScheduledEnquiryModel>().ToList();
                    objData.ScheduledToday = objData.ScheduledEnquiries.Count;
                    objData.TotalEnquiries = multi.Read<int>().SingleOrDefault();
                    objData.TotalFollowups = multi.Read<int>().SingleOrDefault();
                    objData.TotalConverted = multi.Read<int>().SingleOrDefault();
                    objData.TotalFailed = multi.Read<int>().SingleOrDefault();
                    objData.TotalPending = multi.Read<int>().SingleOrDefault();
                }
            }
            return objData;
        }
        public ReceptionStudentSearchModel GetStudentSearchData(int StudentID)
        {
            ReceptionStudentSearchModel objModel = new ReceptionStudentSearchModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@CurrDate", CommonUsage.GetCurrentDate());
                using (var multi = con.QueryMultiple("sp_GetReceptionSearchStudentData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.StudentDetail = multi.Read<StudentModel>().SingleOrDefault();
                    objModel.TodayAttandanceStatus= multi.Read<int>().SingleOrDefault();
                    objModel.MonthAttandance = multi.Read<AttandanceCalenderModel>().ToList();
                }
            }
            return objModel;
        }

        public ReceptionEmployeeSearchModel GetEmployeeSearchData(int EmployeeID)
        {
            ReceptionEmployeeSearchModel objModel = new ReceptionEmployeeSearchModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", EmployeeID);
                paramater.Add("@CurrDate", CommonUsage.GetCurrentDate());
                using (var multi = con.QueryMultiple("sp_GetReceptionSearchEmployeeData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Employee = multi.Read<EmployeeModel>().SingleOrDefault();
                    objModel.TodayAttandanceStatus = multi.Read<int>().SingleOrDefault();
                    objModel.MonthAttandance = multi.Read<AttandanceCalenderModel>().ToList();
                }
            }
            return objModel;
        }
        public ParentModel GetStudentParents(int StudentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                return con.Query<ParentModel>("spn_GetStudentParentDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
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
                    objModel.TimeTable = multi.Read<TimeTableModel>().SingleOrDefault();
                    objModel.TimeTable.Periods = multi.Read<PeriodModel>().ToList();
                    objModel.TimeTable.Subjects = multi.Read<SubjectModel>().ToList();
                    objModel.TimeTable.Lactures = multi.Read<TimeTablePeriodModel>().ToList();
                    objModel.TimeTable.Substitutions = multi.Read<TeacherSubstitutionModel>().ToList();
                    objModel.TimeTable.MergedClasses = multi.Read<TimeTableMergedClassModel>().ToList();
                }
            }
            return objModel;
        }

        public List<Teacher_SubjectModel> GetTeacherSubjects(int EmployeeID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", EmployeeID);

                return con.Query<Teacher_SubjectModel>("sp_GetTeacherSubjects", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
    }
}