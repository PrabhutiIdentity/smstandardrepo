using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class StudentCustomFeeModel
    {
        public int FeeTypeApplicable { get; set; }
        public int SFID { get; set; }
        public int StudentID { get; set; }
        public int SessionID { get; set; }
        public string FeeTypeName { get; set; }
        public int FeeTypeID { get; set; }
        public decimal ClassFee { get; set; }
        public decimal FeeAmount { get; set; }
        public int IsApplicable { get; set; }
        public int OpType { get; set; }
    }

    public class StudentTransportHostelAllocationDelocationModel
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
    public class StudentPromotionModel
    {
        public int SessionID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int SBranchID { get; set; }
        public List<StudentPromoteModel> Students { get; set; }
        public List<ClassModel> Classes { get; set; }
        public List<SectionModel> Sections { get; set; }
        public List<NameIDModel> Branches { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<NameIDModel> ForwardSession { get; set; }
        public DataTable GetPromotedStudentsDataTable()
        {

            DataTable dtPromotedStudents = new DataTable();
            dtPromotedStudents.SetTypeName("ut_StudentSession");
            dtPromotedStudents.Columns.Add("StudentID");
            dtPromotedStudents.Columns.Add("QuotaID");
            dtPromotedStudents.Columns.Add("FeePaymentMode");
            dtPromotedStudents.Columns.Add("HouseID");
            dtPromotedStudents.Columns.Add("Status");
            if (Students == null)
            {
                Students = new List<StudentPromoteModel>();
            }
            foreach (StudentPromoteModel e in Students)
            {
                DataRow dr = dtPromotedStudents.NewRow();
                dr["StudentID"] = e.StudentID;
                dr["QuotaID"] = e.QuotaID;
                dr["FeePaymentMode"] = e.FeePaymentMode;
                dr["HouseID"] = e.HouseID;
                dr["Status"] = e.IsSelected;
                dtPromotedStudents.Rows.Add(dr);
            }

            return dtPromotedStudents;
        }
    }
    public class StudentPromoteModel
    {
        public int StudentID { get; set; }
        public int QuotaID { get; set; }
        public int FeePaymentMode { get; set; }
        public int Status { get; set; }
        public string Name { get; set; }
        public string StudentSID { get; set; }
        public string FatherName { get; set; }
        public string Photo { get; set; }
        public int HouseID { get; set; }
        public int IsSelected { get; set; }
        public int Gender { get; set; }
    }
    public class StudentSearchListModel
    {
        public string SearchText { get; set; }
        public int SessionID { get; set; }
        public List<StudentSearchModel> Students { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public int TCType { get; set; }
    }
    public class ActiveInactiveStudentModel
    {
        public string TypeName { get; set; }
        public int AllStudent { get; set; }
        public int Boys { get; set; }
        public int Girls { get; set; }
    }

    public class StudentSearchModel
    {
        public string FatherName { get; set; }
        public int SLCGenerated { get; set; }
        public int CSGenerated { get; set; }
        public int DOBCGenerated { get; set; }
        public int NDCGenerated { get; set; }
        public int TFCGenerated { get; set; }
        public string SchoolUID { get; set; }
        public int StudentID { get; set; }
        public string StudentSID { get; set; }
        public DateTime DOB { get; set; }
        public int Gender { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public string Photo { get; set; }
        public string Photo1 { get; set; }
        public string Name { get; set; }
        public string RollNo { get; set; }
        public string BloodGroup { get; set; }
        public string AccessCardNo { get; set; }
        public string AadharCardNo { get; set; }        
        public string GuardianMobileNo { get; set; }
        public string FatherMobileNo { get; set; }
        public string MotherMobileNo { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string SSSID { get; set; }
        public string FamilyID { get; set; }
        public string MotherName { get; set; }
        public string GuardianName { get; set; }
        public string MiniAddress { get; set; }
    }
    public class StudentModel
    {
        public string PSchoolName { get; set; }
        public string PSchoolMedium { get; set; }
        public string PClassName { get; set; }
        public string PSResult { get; set; }
        public string PSchoolCity { get; set; }
        public string PSchoolState { get; set; }
        public string SParentID { get; set; }
        public int IsCustomFee { get; set; }
        public int IsBlock { get; set; }
        public int StudentSessionUID { get; set; }
        public string SchoolUID { get; set; }
        public string SubReligion { get; set; }
        public string SessionName { get; set; }
        public DateTime SessionStartDate { get; set; }
        public DateTime SessionEndDate { get; set; }
        public DateTime FatherDOB { get; set; }
        //For whereve Students are Being selected
        public int IsSelected { get; set; }
        public int AttandanceStatus { get; set; }
        public string FatherMobileNo { get; set; }
        public string MotherMobileNo { get; set; }
        public int NotificationSMSTo { get; set; }
        public int StudentID { get; set; }
        public int ParentID { get; set; }
        public string StudentSID { get; set; }
        public string Name { get; set; }
        public DateTime DOB { get; set; }
        public int Age { get; set; }
        public int Gender { get; set; }
        public int Nationality { get; set; }
        public string NationalityText { get; set; }
        public int ReligionID { get; set; }
        public string Religion { get; set; }
        public DateTime DOJ { get; set; }
        public string EmailID { get; set; }
        public int Category { get; set; }
        public string CategoryName { get; set; }
        public int Disability { get; set; }
        public int AdmissionRoute { get; set; }
        public int MotherTongueID { get; set; }
        public string PassportNo { get; set; }
        public int House { get; set; }
        public string HouseName { get; set; }
        public string BusNo { get; set; }
        public string GuardianName { get; set; }
        public int RelationWithGuardian { get; set; }
        public string GuardianMobileNo { get; set; }
        public string GuardianEmail { get; set; }
        public string Cadd_HouseNo { get; set; }
        public string Cadd_Street { get; set; }
        public int Cadd_AreaCode { get; set; }
        public string Cadd_Sector { get; set; }
        public string Cadd_PinCode { get; set; }
        public int Cadd_DistrictCode { get; set; }
        public int Cadd_StateCode { get; set; }
        public int Cadd_CountryCode { get; set; }
        public string RoleModel { get; set; }
        public string Ambition { get; set; }
        public string ExtraCurricular { get; set; }
        public string Allergic_Medicine { get; set; }
        public string VehicleNo { get; set; }
        public string DriverName { get; set; }
        public string DriverMobileNo { get; set; }
        public string AddressProof { get; set; }
        public string BirthCertificate { get; set; }
        public string CategoryCertificate { get; set; }
        public string Photo { get; set; }
        public string Photo1 { get; set; }
        public string TransferCertificate { get; set; }
        public int SBranchID { get; set; }
        public int IsAdmissionComplete { get; set; }
        public int AdmissionBy { get; set; }
        public DateTime OperationDate { get; set; }
        public int OpType { get; set; }
        public string Password { get; set; }
        public int StopID { get; set; }
        public string FatherName { get; set; }
        public string FatherImage { get; set; }
        public string MotherName { get; set; }
        public string MotherImage { get; set; }
        public string AadharCardNo { get; set; }
        public string FatherOccupationID { get; set; }
        public string FatherEducationID { get; set; }
        public string MotherOccupationID { get; set; }
        public string MotherEducationID { get; set; }
        public string MotherAadhaar { get; set; }
        public string FatherAadhaar { get; set; }
      
        public string RollNo { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string BloodGroup { get; set; }
        public string AccessCardNo { get; set; }
        public int HostelRoomID { get; set; }
      
        public string StopName { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public string MiniAddress { get; set; }
        public string MiniAddress2 { get; set; }
        public string SSSID { get; set; }
        public string FamilyID { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IFSCCode { get; set; }
        public string BranchName { get; set; }
        public string ReasonforInactive { get; set; }
        public HttpPostedFileBase StudentImageUploader { get; set; }
        public HttpPostedFileBase BirthCertificateUploader { get; set; }
        public HttpPostedFileBase AddressCertificateUploader { get; set; }
        public HttpPostedFileBase CategoryCertificateUploader { get; set; }
        public HttpPostedFileBase TransferCertificateUploader { get; set; }
        public string EducationLevel { get; set; }
    }
    public class StudentEditModel
    {
        public int StudentSessionUID { get; set; }
        public string SchoolUID { get; set; }
        public int IsCustomFee { get; set; }
        public int StudentID { get; set; }
        public int CurrentTab { get; set; }
        public string SessionName { get; set; }
        public StudentModel Student { get; set; }
        public ParentModel ParantDetails { get; set; }
        public List<NameIDModel> Countries { get; set; }
        public List<NameIDModel> Religions { get; set; }
        public List<NameIDModel> Nationalities { get; set; }
        public List<NameIDModel> SocialCategories { get; set; }
        public List<NameIDModel> MotherTongues { get; set; }
        public List<NameIDModel> SubReligions { get; set; }
        public List<NameIDModel> States { get; set; }
        public List<NameIDModel> Cities { get; set; }
        public List<NameIDModel> Areas { get; set; }
        public List<NameIDModel> Houses { get; set; }
        public List<SessionModel> Sessions { get; set; }
        public List<SchoolSessionModel> SchoolSessions { get; set; }
        public StudentTransportModel Transport = new StudentTransportModel();
        public StudentHostelModel Hostel = new StudentHostelModel();
        public List<StudentCustomFeeModel> CustomeFee { get; set; }
        public List<StudentTransportHostelAllocationDelocationModel> THADDetails { get; set; }
        public SBranchModel SBranchDetails { get; set; }
        public DataTable GetFeeStructureDataTable()
        {

            DataTable dtFeeStructureDetails = new DataTable();
            dtFeeStructureDetails.SetTypeName("ut_StudentFeeDetails");
            dtFeeStructureDetails.Columns.Add("SFID");
            dtFeeStructureDetails.Columns.Add("StudentID");
            dtFeeStructureDetails.Columns.Add("FeeTypeID");
            dtFeeStructureDetails.Columns.Add("SessionID");
            dtFeeStructureDetails.Columns.Add("FeeAmount");
            dtFeeStructureDetails.Columns.Add("IsApplicable");
            dtFeeStructureDetails.Columns.Add("OpType");

            foreach (StudentCustomFeeModel e in CustomeFee)
            {
                DataRow dr = dtFeeStructureDetails.NewRow();
                dr["SFID"] = e.SFID;
                dr["StudentID"] = StudentID;
                dr["FeeTypeID"] = e.FeeTypeID;
                dr["SessionID"] = e.SessionID;
                dr["FeeAmount"] = e.FeeAmount;
                dr["IsApplicable"] = e.IsApplicable;
                dr["OpType"] = e.OpType;

                dtFeeStructureDetails.Rows.Add(dr);
            }

            return dtFeeStructureDetails;
        }
    }
    public class SessionModel
    {
        public DateTime DateOfPromotion { get; set; }
        public DateTime DateOfRemoval { get; set; }
        public string ConductWork { get; set; }
        public string Work { get; set; }
        public string CauseOfRemoval { get; set; }
        public int IsLeft { get; set; }
        public int StudentSessionUID { get; set; }
        public int SessionID { get; set; }
        public string SessionName { get; set; }
        public string HouseName { get; set; }
        public int StudentID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int Status { get; set; }
        public string RollNo { get; set; }
        public int SBranchID { get; set; }
        public int QuotaID { get; set; }
        public int FeePaymentMode { get; set; }
        public string QuotaName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public int HouseID { get; set; }
        public int IsAdmissionFeeApplicable { get; set; }
        public string ReasonforInactive { get; set; }
        public List<NameIDModel> SubjectsOpted { get; set; }
        
        public DataTable GetSubjectOptedDataTable()
        {

            DataTable dtSubjectOpted = new DataTable();
            dtSubjectOpted.SetTypeName("ut_Name_ID_Utility");
            dtSubjectOpted.Columns.Add("ID");
            dtSubjectOpted.Columns.Add("Name");
            dtSubjectOpted.Columns.Add("Extra1");
            dtSubjectOpted.Columns.Add("Extra2");
            dtSubjectOpted.Columns.Add("Extra3");
            if(SubjectsOpted==null)
            {
                SubjectsOpted = new List<NameIDModel>();
            }
            foreach (NameIDModel e in SubjectsOpted)
            {
                DataRow dr = dtSubjectOpted.NewRow();
                dr["ID"] = e.ID;
                dr["Name"] = e.Name;
                dr["Extra1"] = e.Extra1;
                dr["Extra2"] = e.Extra2;
                dr["Extra3"] = e.Extra3;

                dtSubjectOpted.Rows.Add(dr);
            }

            return dtSubjectOpted;
        }
    }
    public class SessionEditModel
    {
        public int StudentID { get; set; }
        public int SessionID { get; set; }
        public SessionModel Session { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> Quotas { get; set; }
        public List<SchoolSessionModel> SchoolSessions { get; set; }
        public DateTime SessionStartDate { get; set; }
        public DateTime SessionEndDate { get; set; }
        public List<NameIDModel> Houses { get; set; }
        public List<NameIDModel> OptionalSubjects { get; set; }
        public List<NameIDModel> SubjectsOpted { get; set; }
    }
    public class StudentTransportModel
    {
        public StudentTransportHostelAllocationDelocationModel THADetails { get; set; }
        public DateTime ChangeDate { get; set; }
        public int Applicable { get; set; }
        public int TransportStop { get; set; }
        public int StudentID { get; set; }
        public int VehicleRouteID { get; set; }
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
    public class StudentHostelModel
    {
        public DateTime ChangedDate { get; set; }
        public int Applicable { get; set; }
        public int HostelID { get; set; }
        public int FloorID { get; set; }
        public int RoomID { get; set; }
        public int StudentID { get; set; }
        public int SBranchID { get; set; }
        public List<NameIDModel> Hostels { get; set; }
        public List<NameIDModel> Floors { get; set; }
        public List<HostalRoomModel> Rooms { get; set; }
    }
    public class StudentFeeModel
    {
        public int Day { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int ParentID { get; set; }
        public int Month { get; set; }
        public int SessionID { get; set; }
        public int Year { get; set; }             
        public DateTime SelectedDate { get; set; }
        public int SBranchID { get; set; }
        public int SchoolID { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
        public List<FeePaymentModel> FeePayments { get; set; }       
        public SBranchModel Branch { get; set; }
    }
    public class StudentLeaveModel
    {
        public string UUID { get; set; }
        public int LeaveID { get; set; }
        public int LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveReason { get; set; }
        public string StudentSID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public string ClassSection { get; set; }
        public string RollNo { get; set; }
        public int Gender { get; set; }
        public string Photo { get; set; }
        public int IsApproved { get; set; }
        public int ApplicantType { get; set; }
        public int SBranchID { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class StudentAttandancePageModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string Day { get; set; }
        public int SBranchID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int TeacherID { get; set; }
        public string UUID { get; set; }
        public DateTime SelectedDate { get; set; }
        public List<StudentAttandanceModel> StudentAttandances { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public DataTable GetAttandanceDetailsDataTable()
        {

            DataTable dtAttandanceDetails = new DataTable();
            dtAttandanceDetails.SetTypeName("AttandanceDetails");
            dtAttandanceDetails.Columns.Add("ID");
            dtAttandanceDetails.Columns.Add("Status");
            dtAttandanceDetails.Columns.Add("IsExist");

            foreach (StudentAttandanceModel e in StudentAttandances)
            {
                DataRow dr = dtAttandanceDetails.NewRow();
                dr["ID"] = e.StudentID;
                dr["Status"] = e.Status;
                dr["IsExist"] = e.IsExist;

                dtAttandanceDetails.Rows.Add(dr);
            }

            return dtAttandanceDetails;
        }
    }
    public class StudentAttandanceModel
    {
        public int Gender { get; set; }
        public int StudentID { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public string StudentSID { get; set; }
        public string SchoolUID { get; set; }        
        public string RollNo { get; set; }
        public int LeaveID { get; set; }
        public double Status { get; set; }
        public int IsExist { get; set; }

    }
    public class StudentLeavePageModel
    {
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<StudentLeaveModel> Students { get; set; }
    }
    public class StudentLeaveAddModel
    {
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> Students { get; set; }
    }
    #region for bulk upload
    public class StudentBulkUploadModel
    {
        public string MiniAddress { get; set; }
        public decimal AnnualIncome { get; set; }
        public string SchoolUID { get; set; }
        public string AadharCardNo { get; set; }
        public int StudentID { get; set; }
        public string Name { get; set; }
        public DateTime DOB { get; set; }
        public int Gender { get; set; }
        public DateTime DOJ { get; set; }
        public string GuardianName { get; set; }
        public string GuardianMobileNo { get; set; }
        public string GuardianEmail { get; set; }
        public string FatherName { get; set; }
        public string FatherMobileNo { get; set; }
        public string FatherEmailID { get; set; }
        public string MotherName { get; set; }
        public string MotherMobileNo { get; set; }
        public string MotherEmailID { get; set; }
        public int SBranchID { get; set; }
        public int QuotaID { get; set; }

        public int ReligionID { get; set; }
        public int Category { get; set; }
        public string RollNo { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string BloodGroup { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int FamilyID { get; set; }
    }

    #endregion

    public class BulkUploadInfoModel
    {
        public int ID { get; set; }
        public List<NameIDModel> Religion { get; set; }
        public List<NameIDModel> Categroy { get; set; }
        public List<NameIDModel> Quota { get; set; }
        public int SBranchID { get; set; }
    }
   }