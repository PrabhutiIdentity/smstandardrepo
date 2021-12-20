using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class StudentAdmissionReportModel
    {
        public int ID { get; set; }
        public int SBranchID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int SessionID { get; set; }
        public int Month { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<StudentAdmissionDetail> StudentDetail { get; set; }


    }
    public class StudentAdmissionDetail
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

    }
}