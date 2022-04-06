using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class NotificationSMSModel
    {
        public string Name { get; set; }
        public string Number { get; set; }
        public string SMSText { get; set; }
        public string FCMToken { get; set; }
        public int RecieverID { get; set; }
        public int RecieverType { get; set; }
        public string ContentID { get; set; }
    }
    public class ClassFeeCollectionChartModel
    {
        public string ClassName { get; set; }
        public decimal FeeCollected { get; set; }
    }
    public class AccountDashboardModel
    {
        public List<NameIDModel> Students { get; set; }
        public List<NameIDModel> Employees { get; set; }
        public decimal MonthlySale { get; set; }
        public decimal MonthlyPurchase { get; set; }
        public int DayID { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int SBranchID { get; set; }
        public List<ClassFeeCollectionChartModel> Collection { get; set; }
        public ChartModel FeeCollection
        {
            get
            {
                ChartModel objCM = new ChartModel();
                objCM.labels = new List<string>();
                DataSetsModel dsTotal = new DataSetsModel();
                dsTotal.label = "Fee Collection";
                dsTotal.data = new List<decimal>();
                dsTotal.backgroundColor = "rgba(26, 187, 156, 0.21)";
                dsTotal.borderColor = "rgba(26, 187, 156, 0.7)";
                dsTotal.pointBorderColor = "rgba(26, 187, 156, 0.7)";
                dsTotal.pointBackgroundColor = "rgba(26, 187, 156, 0.7)";
                dsTotal.pointHoverBackgroundColor = "#fff";
                dsTotal.pointHoverBorderColor = "rgba(220,220,220,1)";

                foreach (ClassFeeCollectionChartModel att in Collection)
                {
                    objCM.labels.Add(att.ClassName);
                    dsTotal.data.Add(Convert.ToDecimal(att.FeeCollected));
                }
                objCM.datasets = new List<DataSetsModel>();
                objCM.datasets.Add(dsTotal);
                return objCM;
            }
        }

    }
    public class AccountModel
    {
    }
    public class StudentsPageModel
    {
        public int ParentID { get; set; }

        public List<StudentModel> Students { get; set; }
        public int ClassID { get; set; }

        public string ClassName { get; set; }
        public int SectionID { get; set; }
        public string SectionName { get; set; }
        public int StudentID { get; set; }
        public int SessionID { get; set; }
        public string SessionName { get; set; }
        public SBranchModel BranchDetails { get; set; }
        public int SBranchID { get; set; }
        public List<ClassModel> Classes { get; set; }
        public List<SectionModel> Sections { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<ActiveInactiveStudentModel> ActiveInactive { get; set; }
        public DataTable GetStudentsDataTable()
        {

            DataTable dtStudentDetails = new DataTable();
            dtStudentDetails.SetTypeName("ut_QuickStudentEdit");
            dtStudentDetails.Columns.Add("StudentID");
            dtStudentDetails.Columns.Add("StudentSessionUID");
            dtStudentDetails.Columns.Add("ParentID");
            dtStudentDetails.Columns.Add("SchoolUID");
            dtStudentDetails.Columns.Add("RollNo");
            dtStudentDetails.Columns.Add("DOB");
            dtStudentDetails.Columns.Add("DOJ");
            dtStudentDetails.Columns.Add("FatherMobileNo");
            dtStudentDetails.Columns.Add("MotherMobileNo");
            dtStudentDetails.Columns.Add("AadharCardNo");

            foreach (StudentModel e in Students)
            {
                DataRow dr = dtStudentDetails.NewRow();
                dr["StudentID"] = e.StudentID;
                dr["StudentSessionUID"] = e.StudentSessionUID;
                dr["ParentID"] = e.ParentID;
                dr["SchoolUID"] = e.SchoolUID;
                dr["RollNo"] = e.RollNo;
                dr["DOB"] = e.DOB;
                dr["DOJ"] = e.DOJ;
                dr["FatherMobileNo"] = e.FatherMobileNo;
                dr["MotherMobileNo"] = e.MotherMobileNo;
                dr["AadharCardNo"] = e.AadharCardNo;
                dtStudentDetails.Rows.Add(dr);
            }

            return dtStudentDetails;
        }
    }
    public class StudentsFeePageModel
    {
        public List<StudentModel> Students { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public DateTime FeeMonth { get; set; }
        public List<ClassModel> Classes { get; set; }
        public List<SectionModel> Sections { get; set; }
    }
    public class AbsentStudentReportModel
    {
        public DateTime SelectedDate { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int SBranchID { get; set; }
        public List<ClassModel> Classes { get; set; }
        public List<StudentModel> Students { get; set; }
    }
    public class StudentsMonthlyAttendancePageModel
    {
        public List<StudentModel> Students { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int StudentID { get; set; }
        public int SessionID { get; set; }
        public DateTime ReportDate { get; set; }
        public List<ClassModel> Classes { get; set; }
        public List<SectionModel> Sections { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<SMEnterpriseDB.Models.StudentAttendanceMasterT> Attendances { get; set; }

       

    }
    #region Bulk Student Data Upload
    public class BulkStudentUploadModel
    {
        public HttpPostedFileBase ExcelFile { get; set; }
        public List<StudentBulkUploadModel> Students { get; set; }
        public List<ClassModel> Classes { get; set; }
        public List<SectionModel> Sections { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int SessionID { get; set; }
        public int SBranchID { get; set; }
        public DataTable GetStudentsDataTable()
        {

            DataTable dtStudentDetails = new DataTable();
            dtStudentDetails.SetTypeName("ut_BulkStudentUpload");
            dtStudentDetails.Columns.Add("Name");
            dtStudentDetails.Columns.Add("RollNo");
            dtStudentDetails.Columns.Add("SchoolUID");
            dtStudentDetails.Columns.Add("BloodGroup");
            dtStudentDetails.Columns.Add("DOB");
            dtStudentDetails.Columns.Add("DOJ");
            dtStudentDetails.Columns.Add("AadharCardNo");
            dtStudentDetails.Columns.Add("FatherName");
            dtStudentDetails.Columns.Add("MotherName");
            dtStudentDetails.Columns.Add("GuardianMobileNo");
            dtStudentDetails.Columns.Add("MiniAddress");
            dtStudentDetails.Columns.Add("FamilyID");

            foreach (StudentBulkUploadModel e in Students)
            {
                DataRow dr = dtStudentDetails.NewRow();
                dr["Name"] = e.Name;
                dr["RollNo"] = e.RollNo;
                dr["SchoolUID"] = e.SchoolUID;
                dr["BloodGroup"] = e.BloodGroup;
                dr["DOB"] = e.DOB;
                dr["DOJ"] = e.DOJ.Year == 1 ? DateTime.Now : e.DOJ;
                dr["AadharCardNo"] = e.AadharCardNo;
                dr["FatherName"] = e.FatherName;
                dr["MotherName"] = e.MotherName;
                dr["GuardianMobileNo"] = e.GuardianMobileNo;
                dr["MiniAddress"] = e.MiniAddress;
                dr["FamilyID"] = e.FamilyID;

                dtStudentDetails.Rows.Add(dr);
            }

            return dtStudentDetails;
        }
        public DataTable GetStudentsDataTableEnt()
        {

            DataTable dtStudentDetailsEnt = new DataTable();
            dtStudentDetailsEnt.SetTypeName("ut_BulkStudentUploadEnt");
            dtStudentDetailsEnt.Columns.Add("Name");
            dtStudentDetailsEnt.Columns.Add("RollNo");
            dtStudentDetailsEnt.Columns.Add("SchoolUID");
            dtStudentDetailsEnt.Columns.Add("BloodGroup");
            dtStudentDetailsEnt.Columns.Add("Gender");
            dtStudentDetailsEnt.Columns.Add("DOB");
            dtStudentDetailsEnt.Columns.Add("DOJ");
            dtStudentDetailsEnt.Columns.Add("AadharCardNo");
            dtStudentDetailsEnt.Columns.Add("FatherName");
            dtStudentDetailsEnt.Columns.Add("FatherMobileNo");
            dtStudentDetailsEnt.Columns.Add("FatherEmailID");
            dtStudentDetailsEnt.Columns.Add("MotherName");
            dtStudentDetailsEnt.Columns.Add("MotherMobileNo");
            dtStudentDetailsEnt.Columns.Add("MotherEmailID");
            dtStudentDetailsEnt.Columns.Add("GuardianName");
            dtStudentDetailsEnt.Columns.Add("GuardianMobileNo");
            dtStudentDetailsEnt.Columns.Add("GuardianEmail");
            dtStudentDetailsEnt.Columns.Add("MiniAddress");
            dtStudentDetailsEnt.Columns.Add("ReligionID");
            dtStudentDetailsEnt.Columns.Add("Category");
            dtStudentDetailsEnt.Columns.Add("QuotaID");
         //   dtStudentDetailsEnt.Columns.Add("FamilyID");

            foreach (StudentBulkUploadModel e in Students)
            {
                DataRow dr = dtStudentDetailsEnt.NewRow();
                dr["Name"] = e.Name;
                dr["RollNo"] = e.RollNo;
                dr["SchoolUID"] = e.SchoolUID;
                dr["BloodGroup"] = e.BloodGroup;
                dr["Gender"] = e.Gender;
                dr["DOB"] = e.DOB;
                dr["DOJ"] = e.DOJ.Year == 1 ? DateTime.Now : e.DOJ;
                dr["AadharCardNo"] = e.AadharCardNo;
                dr["FatherName"] = e.FatherName;
                dr["FatherMobileNo"] = e.FatherMobileNo;
                dr["FatherEmailID"] = e.FatherEmailID;
                dr["MotherName"] = e.MotherName;
                dr["MotherMobileNo"] = e.MotherMobileNo;
                dr["MotherEmailID"] = e.MotherEmailID;
                dr["GuardianName"] = e.GuardianName;
                dr["GuardianMobileNo"] = e.GuardianMobileNo;
                dr["GuardianEmail"] = e.GuardianEmail;
                dr["MiniAddress"] = e.MiniAddress;
                dr["ReligionID"] = e.ReligionID;
                dr["Category"] = e.Category;
                dr["QuotaID"] = e.QuotaID;
              //  dr["FamilyID"] = e.FamilyID;

                dtStudentDetailsEnt.Rows.Add(dr);
            }

            return dtStudentDetailsEnt;
        }
    }

    #endregion Bulk Student Data Upload


}