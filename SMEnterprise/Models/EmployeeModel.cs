using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class EmployeeDocumentDownloadModel
    {
        public int DocType { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public List<NameIDModel> Employees { get; set; }
    }
    public class EmployeeSalarySlipModel
    {
        public EmployeeModel Employee { get; set; }
        public decimal CLApplied { get; set; }
        public decimal ELApplied { get; set; }
        public decimal SLApplied { get; set; }
        public decimal LWPApplied { get; set; }
        public decimal TotalDays { get; set; }
        public decimal Absent { get; set; }
        public decimal DaysPayble { get; set; }
        public List<SalaryCategorySlipModel> Earnings { get; set; }
        public List<SalaryCategorySlipModel> Deductions { get; set; }
    }
    public class SalaryCategorySlipModel
    {
        public int TypeID { get; set; }
        public int TypeApplicable { get; set; }
        public string TypeName { get; set; }
        public decimal Amount { get; set; }
            }
    public class EmployeeSalaryListPageModel
    {
        public DateTime StartDate { get; set; }
        public int EmployeeTypeID { get; set; }
        public int SBranchID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public List<EmployeeSalaryListModel> Employees { get; set; }
        public List<EmployeeTypeModel>  EmployeeTypes { get; set; }
    }
    public class EmployeeSalaryListModel
    {
        public int Gender { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Photo { get; set; }
        public string EmployeeSID { get; set; }
        public int EmployeeType { get; set; }
        public decimal Amount { get; set; }
        public int PaymentID { get; set; }
        public string ReferanceNumber { get; set; }
        public int PaymentStatus { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal PresentDays { get; set; }
        public DateTime PaymentDate { get; set; }
    }
    public class EmployeeAdvancePaymentDeductionModel
    {
        public int ID { get; set; }
        public int AdPayID { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int Mode { get; set; }
        public string Remark { get; set; }
        public int OpType { get; set; }
    }
    public class EmployeeAdvancePaymentEditModel
    {
        public List<EmployeeModel> Employees { get; set; }
        public EmployeeAdvancePaymentModel Payment { get; set; }
    }
    public class EmployeeAdvancePaymentModel
    {
        public int EmployeeID { get; set; }
        public int EmployeeType { get; set; }
        public int Gender { get; set; }
        public int AdPaymentID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal EMI { get; set; }
        public int Months { get; set; }
        public int DeductionsDone { get; set; }
        public decimal DeductedAmount { get; set; }
        public int Status { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string EmployeeName { get; set; }
        public string Photo { get; set; }
        public string EmployeeSID { get; set; }
        public int SBranchID { get; set; }
        public List<EmployeeAdvancePaymentDeductionModel> Deductions { get; set; }

    }
    public class EmployeeAdvancePaymentListModel
    {
        public int SBranchID { get; set; }
        public int Status { get; set; }
        public List<EmployeeAdvancePaymentModel> Employees { get; set; }
    }
    public class TeacherSubstitutionSMSModel
    {
        public EmployeeModel ReplacingTeacher { get; set; }
        public EmployeeModel ReplacedTeacher { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string SubjectName { get; set; }
        public string PeriodName { get; set; }
        public string DayName { get; set; }
        public string SubstitutionDate { get; set; }
        public string FCMToken { get; set; }
    }
    public class EmployeeModel
    {
        public string DeviceGroup { get; set; }
        public int BusShiftID { get; set; }
        public string DevicePassword { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeSID { get; set; }
        public string EmployeeName { get; set; }
        public int OpType { get; set; }
        public DateTime OperationDate { get; set; }
        public int SBranchID { get; set; }
        public DateTime DOB { get; set; }
        public int Gender { get; set; }
        public int Nationality { get; set; }
        public int CategoryID { get; set; }
        public int ReligionID { get; set; }
        public DateTime DOJ { get; set; }
        public DateTime DOR { get; set; }
        public string EmailID { get; set; }
        public string BloodGroup { get; set; }
        public int MaritalStatus { get; set; }
        public int MotherTongueID { get; set; }
        public string MobileNumber { get; set; }
        public string LandLineNumber { get; set; }
        public string PassportNo { get; set; }
        public string FatherHubName { get; set; }
        public string Padd_HouseNo { get; set; }
        public string Padd_Street { get; set; }
        public string Padd_Area { get; set; }
        public string Padd_Sector { get; set; }
        public string Padd_PinCode { get; set; }
        public string Padd_District { get; set; }
        public string Padd_State { get; set; }
        public string Padd_Country { get; set; }
        public string Cadd_HouseNo { get; set; }
        public string Cadd_Street { get; set; }
        public int Cadd_Area { get; set; }
        public string Cadd_Sector { get; set; }
        public string Cadd_PinCode { get; set; }
        public int Cadd_DistrictCode { get; set; }
        public int Cadd_StateCode { get; set; }
        public int Cadd_CountryCode { get; set; }
        public int DepartmentID { get; set; }
        public string Role { get; set; }
        public string EmployeeTypeName { get; set; }
        public string AccessCardNo { get; set; }
        public string VehicleNo { get; set; }
        public string DriverName { get; set; }
        public string DriverMobileNo { get; set; }
        public int DesignationID { get; set; }
        public string AddressProof { get; set; }
        public string BirthCertificate { get; set; }
        public string CategoryCertificate { get; set; }
        public string Photo { get; set; }
        public string ExperienceCertificate { get; set; }
        public string RelievingCertificate { get; set; }
        public string PFDeclaration { get; set; }
        public string PANCardCopy { get; set; }
        public string BankAccountProof { get; set; }
        public string MedicalCertificate { get; set; }
        public string EmpSignature { get; set; }
        public string AppointmentLetter { get; set; }
        public string PANnumber { get; set; }
        public string PFNumber { get; set; }
        public string BankName { get; set; }
        public string IFSCCode { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public int VehicleRouteID { get; set; }
        public int StopID { get; set; }
        public int Status { get; set; }
        public int IsLeft { get; set; }
        public int EmployeeType { get; set; }
        public int EmployeeTypeID { get; set; }
        public string Password { get; set; }
        public string AadharNumber { get; set; }
        public string LicenceNumber { get; set; }
        public string EducationLevels { get; set; }
        
        public HttpPostedFileBase EmployeeImageUploader { get; set; }
        public HttpPostedFileBase BirthCertificateUploader { get; set; }
        public HttpPostedFileBase AddressCertificateUploader { get; set; }
        public HttpPostedFileBase CategoryCertificateUploader { get; set; }
        public HttpPostedFileBase MedicleCertificateUploader { get; set; }
        public HttpPostedFileBase ExperienceCertificateUploader { get; set; }
        public HttpPostedFileBase RelievingCertificateUploader { get; set; }
        public HttpPostedFileBase PFDeclarationUploader { get; set; }
        public HttpPostedFileBase AppointmentCertificateUploader { get; set; }
        public HttpPostedFileBase PANCardUploader { get; set; }
        public HttpPostedFileBase BankAccountProofUploader { get; set; }
        public HttpPostedFileBase EmpSignatureUploader { get; set; }
    }
    public class EmployeeTypeModel
    {
        public int EmployeeTypeID { get; set; }
        public string EmployeeTypeName { get; set; }
        public int SBranchID { get; set; }
        public int IsDefault { get; set; }
        public decimal NetSalary { get; set; }
        public int EmployeeCount { get; set; }
        public int OpType { get; set; }
        public List<EmployeeTypeSalaryModel> Salaries { get; set; }

        public DataTable GetFeeStructureDataTable()
        {

            DataTable dtFeeStructureDetails = new DataTable();
            dtFeeStructureDetails.SetTypeName("ut_SalaryStructureDetail");
            dtFeeStructureDetails.Columns.Add("ETypeSTypeID");
            dtFeeStructureDetails.Columns.Add("EmployeeTypeID");
            dtFeeStructureDetails.Columns.Add("SalaryTypeID");
            dtFeeStructureDetails.Columns.Add("Amount");
            dtFeeStructureDetails.Columns.Add("SBranchID");

            foreach (EmployeeTypeSalaryModel e in Salaries)
            {
                if(e.TypeApplicable==4 && e.Amount>0)
                {
                    e.Amount = e.Amount * -1;
                }
                DataRow dr = dtFeeStructureDetails.NewRow();
                dr["ETypeSTypeID"] = e.ETypeSTypeID;
                dr["EmployeeTypeID"] = e.EmployeeTypeID;
                dr["SalaryTypeID"] = e.TypeID;
                dr["Amount"] = e.Amount;
                dr["SBranchID"] = e.SBranchID;

                dtFeeStructureDetails.Rows.Add(dr);
            }

            return dtFeeStructureDetails;
        }
    }
    public class EmployeeListPageModel
    {
        public int SBranchID { get; set; }
        public int EmployeeType { get; set; }
        public int EmployeeID { get; set; }
        public List<EmployeeModel> Employees { get; set; }
        public List<EmployeeTypeModel> EmployeeTypes { get; set; }
    }
    public class EmployeeAttandancePageModel
    {
        public string UUID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string Day { get; set; }
        public int SBranchID { get; set; }
        public int EmployeeType { get; set; }
        public DateTime SelectedDate { get; set; }
        public List<EmployeeAttandanceModel> EmployeeAttandances { get; set; }
        public List<NameIDModel> EmployeeTypes { get; set; }
        public DataTable GetAttandanceDetailsDataTable()
        {

            DataTable dtAttandanceDetails = new DataTable();
            dtAttandanceDetails.SetTypeName("AttandanceDetails");
            dtAttandanceDetails.Columns.Add("ID");
            dtAttandanceDetails.Columns.Add("Status");
            dtAttandanceDetails.Columns.Add("IsExist");

            foreach (EmployeeAttandanceModel e in EmployeeAttandances)
            {
                DataRow dr = dtAttandanceDetails.NewRow();
                dr["ID"] = e.EmployeeID;
                dr["Status"] = e.Status;
                dr["IsExist"] = e.IsExist;

                dtAttandanceDetails.Rows.Add(dr);
            }

            return dtAttandanceDetails;
        }
    }
    public class EmployeeAttandanceModel
    {
        public int Gender { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Photo { get; set; }
        public string EmployeeSID { get; set; }
        public int LeaveID { get; set; }
        public decimal CLs { get; set; }
        public decimal ELs { get; set; }
        public decimal PLs { get; set; }
        public double Status { get; set; }
        public int IsExist { get; set; }

    }
    public class EmployeeEducationModel
    {
        public int OpType { get; set; }
        public int EmployeeID { get; set; }
        public int EmpEduID { get; set; }
        public string Qualification { get; set; }
        public string Stream { get; set; }
        public string InstituteName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PassingYear { get; set; }
        public string Grade { get; set; }
        public decimal Percentage { get; set; }
    }
    public class EmployeeExperienceModel
    {
        public int OpType { get; set; }
        public int EmployeeID { get; set; }
        public int EmpExpID { get; set; }
        public string Designation { get; set; }
        public string SubjectsClasses { get; set; }
        public string InstituteName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class EmployeeTransportHostelAllocationDelocationModel
    {
        public int SBranchID { get; set; }
        public int THChangeID { get; set; }
        public int VehicleRouteID { get; set; }
        public int ChangeType { get; set; }
        public int UserType { get; set; }
        public int UserID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int SessionID { get; set; }
        public int KeyID { get; set; }
        public string Name { get; set; }
        public int Applicable { get; set; }
        public int OpType { get; set; }
    }
    public class EmployeeTransportModel
    {
        public EmployeeTransportHostelAllocationDelocationModel THADetails { get; set; }
        public DateTime ChangeDate { get; set; }
        public int Applicable { get; set; }
        public int TransportStop { get; set; }
        public int EmployeeID { get; set; }
        public int VehicleRouteID { get; set; }
        public int BusShiftID { get; set; }
        public int SBranchID { get; set; }
        public int StopID { get; set; }
        public RouteStoppageModel StopDetails { get; set; }
        public List<NameIDModel> Countries { get; set; }
        public List<NameIDModel> States { get; set; }
        public List<NameIDModel> Cities { get; set; }
        public List<NameIDModel> Areas { get; set; }
        public List<NameIDModel> Routes { get; set; }
        public List<NameIDModel> Vehicles { get; set; }
        public List<RouteStoppageModel> Stops { get; set; }
    }
    public class EmployeeEditModel
    {
        public int EmployeeID { get; set; }
        public int CurrentTab { get; set; }
        public EmployeeModel EmployeeDetails { get; set; }
        public List<EmployeeTypeModel> EmployeeTypes { get; set; }
        public List<NameIDModel> Countries { get; set; }
        public List<NameIDModel> Religions { get; set; }
        public List<NameIDModel> Nationalities { get; set; }
        public List<NameIDModel> SocialCategories { get; set; }
        public List<NameIDModel> MotherTongues { get; set; }
        public List<NameIDModel> SubReligions { get; set; }
       
        public List<NameIDModel> States { get; set; }
        public List<NameIDModel> Cities { get; set; }
        public List<NameIDModel> Areas { get; set; }
        public List<EmployeeSalaryModel> Salary { get; set; }
        public List<EmployeeEducationModel> Education { get; set; }
        public List<EmployeeExperienceModel> Experience { get; set; }
        public List<EmployeeLeaveQuotaModel> Leaves { get; set; }
        public List<Teacher_SubjectModel> Subjects { get; set; }
        public List<NameIDModel> EducationLevels { get; set; }
        public List<NameIDModel> Groups { get; set; }
        public List<NameIDModel> GroupSubjects { get; set; }
        public EmployeeTransportModel Transport = new EmployeeTransportModel();
        public List<EmployeeTransportHostelAllocationDelocationModel> THADDetails { get; set; }
        public DataTable GetSalaryDataTable()
        {

            DataTable dtFeePaymentDetails = new DataTable();
            dtFeePaymentDetails.SetTypeName("EmployeeSalaryDetails");
            dtFeePaymentDetails.Columns.Add("EmployeeSalaryID");
            dtFeePaymentDetails.Columns.Add("EmployeeID");
            dtFeePaymentDetails.Columns.Add("EmployeeTypeID");
            dtFeePaymentDetails.Columns.Add("SalaryTypeID");
            dtFeePaymentDetails.Columns.Add("Amount");

            foreach (EmployeeSalaryModel e in Salary)
            {
                DataRow dr = dtFeePaymentDetails.NewRow();
                dr["EmployeeSalaryID"] = e.EmployeeSalaryID;
                dr["EmployeeID"] = e.EmployeeID;
                dr["EmployeeTypeID"] = e.EmployeeTypeID;
                dr["SalaryTypeID"] = e.SalaryTypeID;
                dr["Amount"] = e.Amount;

                dtFeePaymentDetails.Rows.Add(dr);
            }

            return dtFeePaymentDetails;
        }
        public DataTable GetLeaveDetailsDataTable()
        {

            DataTable dtLeaveDetails = new DataTable();
            dtLeaveDetails.SetTypeName("EmployeeLeaveDetails");
            dtLeaveDetails.Columns.Add("ELID");
            dtLeaveDetails.Columns.Add("EmployeeID");
            dtLeaveDetails.Columns.Add("LeaveTypeID");
            dtLeaveDetails.Columns.Add("MonthlyQuota");
            dtLeaveDetails.Columns.Add("YearlyQuota");

            foreach (EmployeeLeaveQuotaModel e in Leaves)
            {
                DataRow dr = dtLeaveDetails.NewRow();
                dr["ELID"] = e.ELID;
                dr["EmployeeID"] = e.EmployeeID;
                dr["LeaveTypeID"] = e.LeaveTypeID;
                dr["MonthlyQuota"] = e.MonthlyQuota;
                dr["YearlyQuota"] = e.YearlyQuota;

                dtLeaveDetails.Rows.Add(dr);
            }

            return dtLeaveDetails;
        }
    }
    public class Teacher_SubjectModel
    {
        public int OpType { get; set; }
        public int TSID { get; set; }
        public int SubjectID { get; set; }
        public int EducationLevelID { get; set; }
        public string SubjectName { get; set; }
        public string EducationLevel { get; set; }
        public int EmployeeID { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; }
    }
    public class EmployeeLeaveQuotaModel
    {
        public int ELID { get; set; }
        public int LeaveTypeID { get; set; }
        public string LeaveTypeName { get; set; }
        public decimal YearlyQuota { get; set; }
        public decimal MonthlyQuota { get; set; }
        public int EmployeeID { get; set; }
    }
    public class EmployeeLeaveAddModel
    {
        public List<EmployeeModel> Employees { get; set; }
        public decimal CLYearlyQuota { get; set; }
        public decimal CLMonthlyQuota { get; set; }
        public decimal CLMonthlyApplied { get; set; }
        public decimal CLYearlyApplied { get; set; }

        public decimal PLYearlyQuota { get; set; }
        public decimal PLMonthlyQuota { get; set; }
        public decimal PLMonthlyApplied { get; set; }
        public decimal PLYearlyApplied { get; set; }

        public decimal ELYearlyQuota { get; set; }
        public decimal ELMonthlyQuota { get; set; }
        public decimal ELMonthlyApplied { get; set; }
        public decimal ELYearlyApplied { get; set; }

        public decimal LWPApplied { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}