using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class ReceptionStudentModel
    {
        public List<AttandanceCalenderModel> MonthAttandance { get; set; }
    }
    public class AdmissionEnquiryPageModel
    {
        public int UserID { get; set; }
        public int EStatus { get; set; }
        public int EPossibility { get; set; }
        public int SBranchID { get; set; }
        public int SessionID { get; set; }
        public int ClassID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<NameIDModel> Receptionists { get; set; }
        public List<AdmissionEnquiryMasterModel> Enquiries { get; set; }
        public AdmissionEnquiryMasterModel Enquiry { get; set; }
    }
    public class AdmissionEnquiryFollowupModel
    {
        public int FollowupID { get; set; }
        public int EnquiryID { get; set; }
        public int FollowupBy { get; set; }
        public string FollowupByName { get; set; }
        public string Remark { get; set; }
        public DateTime FDate { get; set; }
        public int Status { get; set; }
        public int Possibility { get; set; }
        public DateTime NextFollowupDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class AdmissionEnquiryMasterModel
    {
        public int SrNo { get; set; }
        public string SerailNO { get; set; }
        public int AssignedTo { get; set; }
        public int Followups { get; set; }
        public string SessionName { get; set; }
        public string ClassName { get; set; }
        public string CurrentClassName { get; set; }
        public string ReciptionistName { get; set; }
        public int EnquiryID { get; set; }
        public DateTime EDate { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string FatherMobileNo { get; set; }
        public string MotherMobileNo { get; set; }
        public string FatherEmailID { get; set; }
        public string MotherEmailID { get; set; }
        public string FOccupation { get; set; }
        public string MOccupation { get; set; }
        public decimal FamilyIncome { get; set; }   
        public string Address { get; set; }
        public string StudentName { get; set; }
        public int AppliedForSession { get; set; }
        public int AppliedForClass { get; set; }
        public int CurrentClass { get; set; }
        public int ESource { get; set; }
        public string CurrentSchool { get; set; }
        public string CurrentSchoolAddress { get; set; }
        public string ReasonForChange { get; set; }
        public int Gender { get; set; }
        public string Nationality { get; set; }
        public string CurrentEducationSystem { get; set; }
        public DateTime StudentDOB { get; set; }
        public int EStatus { get; set; }
        public int EPossibility { get; set; }
        public DateTime NextFollowUpDate { get; set; }
        public int SBranchID { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedDate { get; set; }
        public int StudentID { get; set; }
        public int PaymentStatus { get; set; }
        public string PaymentDetails { get; set; }
        public string Image { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public string StudentAadharNo { get; set; }
        public string FatherAadhaarNo { get; set; }
        public string MotherAadhaarNo { get; set; }
        public string Caste { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string TelephoneNoReg { get; set; }
        public string TelephoneNoOff { get; set; }
        public string Sibling { get; set; }
        public string SiblingName { get; set; }
        public string SiblingClass { get; set; }
        public string MotherQuaAndOcc { get; set; }
        public string PermanentAddress { get; set; }
        public string TemporaryAddress { get; set; }
        public string FatherQuaAndOcc { get; set; }
        public int BranchID { get; set; }
        public List<SBranchModel> BranchData { get; set; }
        public int ReligionID { get; set; }
        public List<NameIDModel> Religions { get; set; }
        public List<ClassModel> Classes { get; set; }
        public List<NameIDModel> Casts { get; set; }

    }
    public class ToDoItemModel {
        public int OpType { get; set; }
        public int UserType { get; set; }
        public int ToDoID { get; set; }
        public int Priority { get; set; }
        public DateTime ScheduledDateTime { get; set; }
        public int Status { get; set; }
        public string Details { get; set; }
        public int UserID { get; set; }
        public int SBranchID { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class ScheduledEnquiryModel
    {
        public int EnquiryID { get; set; }
        public string Remark { get; set; }
        public DateTime FDate { get; set; }
        public DateTime NextFollowupDate { get; set; }
        public int EStatus { get; set; }
        public int Possibility { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string FatherMobileNo { get; set; }
        public string MotherMobileNo { get; set; }
        public string SessionName { get; set; }
        public string ClassName { get; set; }
        public string FollowupByName { get; set; }
        public int Gender { get; set; }
    }
    public class ReceptionDashboardModel
    {
        public List<ScheduledEnquiryModel> ScheduledEnquiries { get; set; }
        public List<ToDoItemModel> ToDoList { get; set; }
        public int ScheduledToday { get; set; }
        public int TotalEnquiries { get; set; }
        public int TotalFollowups { get; set; }
        public int TotalConverted { get; set; }
        public int TotalFailed { get; set; }
        public int TotalPending { get; set; }
    }
    public class ReceptionStudentSearchModel
    {

        public StudentModel StudentDetail { get; set; }
        public int TodayAttandanceStatus { get; set; }
        public List<MessageModel> Messages { get; set; }
        public List<EventModel> Events { get; set; }
        public List<EvaluationModel> Evaluations { get; set; }
        public List<NoticeModel> Notices { get; set; }
        public List<AttandanceCalenderModel> MonthAttandance { get; set; }
    }

    public class ReceptionEmployeeSearchModel
    {

        public EmployeeModel Employee { get; set; }
        public int TodayAttandanceStatus { get; set; }
        public List<MessageModel> Messages { get; set; }
        public List<EventModel> Events { get; set; }
        public List<EvaluationModel> Evaluations { get; set; }
        public List<NoticeModel> Notices { get; set; }
        public List<AttandanceCalenderModel> MonthAttandance { get; set; }
    }
}