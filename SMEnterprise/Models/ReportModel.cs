using SMEnterpriseDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
   
    public class SectionClassTeacherReportPageModel
    {
        public int SBranchID { get; set; }
        public List<NameIDModel> Teachers { get; set; }
        public List<SectionClassTeacherReportModel> Report { get; set; }
    }
    public class SectionClassTeacherReportModel
    {
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int TeacherID { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string TeacherName { get; set; }
    }
    public class HouseClassStudentListModel
    {
        public int ID { get; set; }
        public int Type { get; set; }
        public List<HouseModel> Houses { get; set; }
        public List<ClassSectionModel> ClassSections { get; set; }
        public List<StudentModel> Students { get; set; }
        public SBranchModel Branch { get; set; }
    }
    public class StopWiseStudentAmountModel
    {
        public string AreaName { get; set; }
        public int AreaID { get; set; }
        public int Students { get; set; }
        public decimal Amount { get; set; }
    }
    public class BusStudentListModel
    {
        public int ShiftID { get; set; }
        public int ID { get; set; }
        public int Type { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int SBranchID { get; set; }
        public int SessionID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime SelectedDate { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
        public List<StopWiseStudentAmountModel> StopWiseReport { get; set; }
        public List<NameIDModel> Busses { get; set; }
        public List<ClassSectionModel> ClassSections { get; set; }
        public List<StudentModel> Students { get; set; }
        public SBranchModel Branch { get; set; }
    }


    public class MonthlyAttendancePageModel
    {
        public int BranchID { get; set; }
        public int EmployeeType { get; set; }
        public DateTime RepoDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<EmployeeModel> Employees { get; set; }
        public List<MonthlyAttendanceLogModel> Attendance { get; set; }
        public List<EmployeeTypeModel> EmployeeTypes { get; set; }
        public List<EmployeeAttendanceMasterT> StatusAttendance { get; set; }
    }
    public class MonthlyAttendanceLogModel
    {
        public string UserID { get; set; }
        public DateTime LogDate { get; set; }
        public int Direction { get; set; }
    }
    public class StudentYearMonthFeeSummeryModel
    {
        public int FeeYear { get; set; }
        public int FeeMonth { get; set; }
        public decimal Discount { get; set; }
        public decimal ApplicableFee { get; set; }
        public decimal PaymentRecieved { get; set; }
    }
    public class StudentYearMonthFeeTypeFeeDetailModel
    {
        public int FeeYear { get; set; }
        public int FeeMonth { get; set; }
        public int FeeTypeID { get; set; }
        public string FeeTypeName { get; set; }
        public decimal RDiscount { get; set; }
        public decimal ApplicableFee { get; set; }
        public decimal PaidAmount { get; set; }
    }
    public class StudentSessionFeeStatusPageModel
    {
        public int StudentID { get; set; }
        public DateTime CurDate { get; set; }
        public List<StudentYearMonthFeeSummeryModel> MonthlySummery { get; set; }
        public List<StudentYearMonthFeeTypeFeeDetailModel> FeeWiseDetails { get; set; }
    }
public class ClassGenderCategoryCountModel
    {
        public int Gender { get; set; }
        public int Category { get; set; }
        public int ClassID { get; set; }
        public int HouseID { get; set; }
        public int GSubjectID { get; set; }
        public int Students { get; set; }
        public int IsExamAttended { get; set; }
        public int AGrade { get; set; }
        public int CNT { get; set; }
        public int BGrade { get; set; }
        public int CGrade { get; set; }
        public int DGrade { get; set; }
        public string ClassName { get; set; }
    }
    public class ClassGenderCategoryCountPageModel
    {
        public List<ClassModel> Classes { get; set; }
        public List<ClassGenderCategoryCountModel> Data { get; set; }
        public List<ClassGenderCategoryCountModel> SubjectData { get; set; }
        public SBranchModel Branch { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<NameIDModel> Evaluations { get; set; }
        public List<NameIDModel> Schemes { get; set; }
        public List<NameIDModel> Subjects { get; set; }
        public List<NameIDModel> EducationLevels { get; set; }
        public List<NameIDModel> RGroupSubjects { get; set; }
        public List<NameIDModel> Houses { get; set; }
        public int SessionID { get; set; }
        public int EvaluationID { get; set; }
        public int SchemeID { get; set; }
        public int EducationLevelID { get; set; }
        public int SBranchID { get; set; }
        public List<NameIDModel> Categories { get; set; }
    }
    public class DailyAttandanceReportModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int SBranchID { get; set; }
        public DateTime ReportDate { get; set; }
        public List<DailyAttandanceModel> Report { get; set; }
        public SBranchModel Branch { get; set; }
    }
    public class DailyAttandanceModel
    {
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public int Total { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Leave { get; set; }
    }
    public class CollectionReportModel
    {
        public string UUID { get; set; }
        public SBranchModel Branch { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int ReportType { get; set; }
        public int PaymentMode { get; set; }
        public int SBranchID { get; set; }
        public List<FeePaymentModel> Report { get; set; }
        public List<NameIDModel> FeeType { get; set; }
        public List<NameIDModel> ExpenceType { get; set; }
        public List<PaymentModeModel> PaymentModes { get; set; }
        public List<PaymentDetailsModel> FeeReportType { get; set; }
        public List<ExpenseDetailsModel> ExpenseReportType { get; set; }
        public int QuarterID { get; set; }
    }


    public class ExpenseDetailsModel
    {
        public int CollectionID { get; set; }
        public int ExpenceID { get; set; }
        public int ExpenceTypeID { get; set; }
        public string ExpenceTypeName { get; set; }
        public string ExpenseNoRange { get; set; }
        public int EmployeeID { get; set; }
        public decimal ExpenceAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal SalryAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal NetPayble { get; set; }
        public decimal GrossEarning { get; set; }
        public decimal TotalDeduction { get; set; }
        public string Year { get; set; }
        public int Month { get; set; }
        public DateTime ExpenceDate { get; set; }
        public int SBranchID { get; set; }
        public decimal Sum { get; set; }
    }

    public class SessionStrengthReport
    {
        public List<ClassModel> Classes { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
        public List<SessionStrengthModel> Strength { get; set; }
    }
    public class SessionStrengthModel
    {
        public int SessionID { get; set; }
        public int ClassID { get; set; }
        public int TotalStrength { get; set; }
        public string ComparativeData { get; set; }
    }
    public class ClassWiseFeeDuePageModel
    {
        public List<SchoolSessionModel> Sessions { get; set; }
        public int SessionID { get; set; }
        public int SBranchID { get; set; }
        public DateTime QDate { get; set; }
        public List<ClassWiseFeeDueModel> ClassWiseDueFee { get; set; }
    }
    public class ClassWiseFeeDueModel
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public decimal LateFee { get; set; }
        public decimal DiscAmt { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal ApplicableFee { get; set; }
        public decimal PreviousDues { get; set; }
    }

}