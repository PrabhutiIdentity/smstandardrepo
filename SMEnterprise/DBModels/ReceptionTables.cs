using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterpriseDB.Models
{
    public class ReceptionTables
    {
    }
    public class AdmissionEnquiryFollowups
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
    public class AdmissionEnquiryMaster
    {
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
    }
}