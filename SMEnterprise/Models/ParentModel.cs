using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace SMEnterprise.Models
{
    public class ParentDiaryPageModel
    {
        public int SBranchID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int SubjectID { get; set; }
        public int TeacherID { get; set; }
        public DateTime PBDate { get; set; }
        public List<StudentModel> Students { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> Subjects { get; set; }
        public DataTable GetStudentsDataTable()
        {

            DataTable dtAttandanceDetails = new DataTable();
            dtAttandanceDetails.SetTypeName("ut_ParentDiary");
            dtAttandanceDetails.Columns.Add("TeacherID");
            dtAttandanceDetails.Columns.Add("TeacherType");
            dtAttandanceDetails.Columns.Add("StudentID");
            dtAttandanceDetails.Columns.Add("PBDate");
            dtAttandanceDetails.Columns.Add("Title");
            dtAttandanceDetails.Columns.Add("Description");
            dtAttandanceDetails.Columns.Add("Priority");
            dtAttandanceDetails.Columns.Add("IsRead");
            dtAttandanceDetails.Columns.Add("MasterID");
            dtAttandanceDetails.Columns.Add("ClassID");
            dtAttandanceDetails.Columns.Add("SectionID");
            dtAttandanceDetails.Columns.Add("SubjectID");

            foreach (StudentModel e in Students)
            {
                DataRow dr = dtAttandanceDetails.NewRow();
              
                dr["StudentID"] = e.StudentID;

                dtAttandanceDetails.Rows.Add(dr);
            }

            return dtAttandanceDetails;
        }
    }
    public class ParentDiaryDetailModel
    {
        public ParentDiaryModel Details;
        public List<ParentDiaryModel> Students { get; set; }

    }
    public class ParentDiaryModel
    {
        public int ID { get; set; }
        public string UUID { get; set; }
        public int SBranchID { get; set; }
        public int PBID { get; set; }
        public int TeacherID { get; set; }
        public string TeacherName { get; set; }
        public int TeacherType { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int SubjectID { get; set; }
        public string SectionName { get; set; }
        public string SubjectName { get; set; }
        public string ClassName { get; set; }
        public DateTime PBDate { get; set; }
        public DateTime EntryDate { get; set; }
        public string Title { get; set; }
        [AllowHtml]
        public string Description { get; set; }
        public int Priority { get; set; }
        public int IsRead { get; set; }
        public int MasterID { get; set; }
        public int Reciever { get; set; }

        public List<StudentModel> Students { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> Subjects { get; set; }
        public DataTable GetStudentsDataTable()
        {

            DataTable dtAttandanceDetails = new DataTable();
            dtAttandanceDetails.SetTypeName("ut_ParentDiary");
            dtAttandanceDetails.Columns.Add("TeacherID");
            dtAttandanceDetails.Columns.Add("TeacherType");
            dtAttandanceDetails.Columns.Add("StudentID");
            dtAttandanceDetails.Columns.Add("PBDate");
            dtAttandanceDetails.Columns.Add("Title");
            dtAttandanceDetails.Columns.Add("Description");
            dtAttandanceDetails.Columns.Add("Priority");
            dtAttandanceDetails.Columns.Add("IsRead");
            dtAttandanceDetails.Columns.Add("MasterID");
            dtAttandanceDetails.Columns.Add("ClassID");
            dtAttandanceDetails.Columns.Add("SectionID");
            dtAttandanceDetails.Columns.Add("SubjectID");

            foreach (StudentModel e in Students)
            {
                if (e.IsSelected == 1)
                {
                    DataRow dr = dtAttandanceDetails.NewRow();

                    dr["StudentID"] = e.StudentID;
                    dr["PBDate"] = PBDate;

                    dtAttandanceDetails.Rows.Add(dr);
                }
            }

            return dtAttandanceDetails;
        }
    }
    public class AttandanceCalenderModel
    {
        public int DayID { get; set; }
        public decimal Status { get; set; }
    }
    public class ParentPageModel
    {
        public string SText { get; set; }
        public int SBranchID { get; set; }
        public List<ParentModel> Parents { get; set; }
        public int SessionID { get; set; }
        public List<NameIDModel> Sessions { get; set; }
    }
    public class ParentModel
    {
        public int NotificationSMSTo { get; set; }
        public string GuardianName { get; set; }
        public string GuardianMobileNo { get; set; }
        public string StudentName { get; set; }
        public int IsSelected { get; set; }
        public int StudentID { get; set; }
        public int ParentID { get; set; }
        public string ParentSID { get; set; }
        public string FatherName { get; set; }
        public string FatherOccupationID { get; set; }
        public string FatherEducationID { get; set; }
        public string FatherMobileNo { get; set; }
        public string FatherEmailID { get; set; }
        public string FatherBloodGroup { get; set; }
        public string FatherImage { get; set; }
        public string FatherAadhaar { get; set; }
        public DateTime FatherDOB { get; set; }
        public string MotherName { get; set; }
        public string MotherOccupationID { get; set; }
        public string MotherEducationID { get; set; }
        public string MotherMobileNo { get; set; }
        public string MotherEmailID { get; set; }
        public string MotherBloodGroup { get; set; }
        public string MotherImage { get; set; }
        public string MotherAadhaar { get; set; }
        public DateTime MotherDOB { get; set; }
        public string Padd_HouseNo { get; set; }
        public string Padd_Street { get; set; }
        public string Padd_Area { get; set; }
        public string Padd_Sector { get; set; }
        public string Padd_PinCode { get; set; }
        public string Padd_District { get; set; }
        public string Padd_State { get; set; }
        public string Padd_Country { get; set; }
        public string HomeLandLineNo { get; set; }
        public int SBranchID { get; set; }
        public string Password { get; set; }
        public string AccessCardNo { get; set; }
        public HttpPostedFileBase FatherImageUploader { get; set; }
        public HttpPostedFileBase MotherImageUploader { get; set; }


        public string Cadd_HouseNo { get; set; }
        public string Cadd_Street { get; set; }
        public int Cadd_AreaCode { get; set; }
        public string Cadd_Sector { get; set; }
        public string Cadd_PinCode { get; set; }
        public int Cadd_DistrictCode { get; set; }
        public int Cadd_StateCode { get; set; }
        public int Cadd_CountryCode { get; set; }
        public string SchoolUID { get; set; }
        
    }
    public class AttandanceModel
    {
        private string[] Months = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        public decimal Total { get; set; }
        public decimal HalfDay { get; set; }
        public decimal Absent { get; set; }
        public decimal TotalDays { get; set; }
        public int FYear { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public string MonthName { get; set; }
        public string LegendName { get; set; }

    }
    public class ParentLandingPageModel
    {
        public List<StudentModel> Students { get; set; }
    }
    public class DashboardModel
    {
        public List<AttandanceModel> Attandance { get; set; }
        public ChartModel AttandanceChart
        {
            get
            {
                ChartModel objCM = new ChartModel();
                objCM.labels = new List<string>();
                DataSetsModel dsTotal = new DataSetsModel();
                dsTotal.label = "School Days";
                dsTotal.data = new List<decimal>();
                dsTotal.backgroundColor = "rgba(74, 88, 106, 0.21)";
                dsTotal.borderColor = "rgba(74, 88, 106, 0.7)";
                dsTotal.pointBorderColor = "rgba(74, 88, 106, 0.7)";
                dsTotal.pointBackgroundColor = "rgba(74, 88, 106, 0.7)";
                dsTotal.pointHoverBackgroundColor = "#fff";
                dsTotal.pointHoverBorderColor = "rgba(220,220,220,1)";
                DataSetsModel dsHalfDay = new DataSetsModel();
                dsHalfDay.label = "Half Day";
                dsHalfDay.data = new List<decimal>();
                dsHalfDay.backgroundColor = "rgba(255, 216, 0, 0.2)";
                dsHalfDay.borderColor = "rgba(255, 216, 0, 0.7)";
                dsHalfDay.pointBorderColor = "rgba(255, 216, 0, 0.7)";
                dsHalfDay.pointBackgroundColor = "rgba(255, 216, 0, 0.7)";
                dsHalfDay.pointHoverBackgroundColor = "#fff";
                dsHalfDay.pointHoverBorderColor = "rgba(220,220,220,1)";
                DataSetsModel dsAbsent = new DataSetsModel();
                dsAbsent.label = "Absent";
                dsAbsent.data = new List<decimal>();
                dsAbsent.backgroundColor = "rgba(234, 12, 12, 0.2)";
                dsAbsent.borderColor = "rgba(234, 12, 12, 0.7)";
                dsAbsent.pointBorderColor = "rgba(234, 12, 12, 0.7)";
                dsAbsent.pointBackgroundColor = "rgba(234, 12, 12, 0.7)";
                dsAbsent.pointHoverBackgroundColor = "#fff";
                dsAbsent.pointHoverBorderColor = "rgba(220,220,220,1)";
                DataSetsModel dsPresent = new DataSetsModel();
                dsPresent.label = "Present";
                dsPresent.data = new List<decimal>();
                dsPresent.backgroundColor = "rgba(38, 185, 154, 0.2)";
                dsPresent.borderColor = "rgba(38, 185, 154, 0.7)";
                dsPresent.pointBorderColor = "rgba(38, 185, 154, 0.7)";
                dsPresent.pointBackgroundColor = "rgba(38, 185, 154, 0.7)";
                dsPresent.pointHoverBackgroundColor = "#fff";
                dsPresent.pointHoverBorderColor = "rgba(220,220,220,1)";
                foreach (AttandanceModel att in Attandance)
                {
                    objCM.labels.Add(att.LegendName);
                    dsTotal.data.Add(att.TotalDays);
                    dsAbsent.data.Add(att.Absent);
                    dsPresent.data.Add(att.Total);
                    dsHalfDay.data.Add(att.HalfDay);
                }
                objCM.datasets = new List<DataSetsModel>();
                objCM.datasets.Add(dsTotal);
                objCM.datasets.Add(dsPresent);
                objCM.datasets.Add(dsHalfDay);
                objCM.datasets.Add(dsAbsent);
                return objCM;
            }
        }
        public ChartModel MonthlyAttandanceChart
        {
            get
            {
                ChartModel objCM = new ChartModel();
                objCM.labels = new List<string>();
                DataSetsModel dsTotal = new DataSetsModel();
                dsTotal.label = "School Days";
                dsTotal.data = new List<decimal>();
                dsTotal.backgroundColor = "rgba(74, 88, 106, 0.21)";
                dsTotal.borderColor = "rgba(74, 88, 106, 0.7)";
                dsTotal.pointBorderColor = "rgba(74, 88, 106, 0.7)";
                dsTotal.pointBackgroundColor = "rgba(74, 88, 106, 0.7)";
                dsTotal.pointHoverBackgroundColor = "#fff";
                dsTotal.pointHoverBorderColor = "rgba(220,220,220,1)";
                DataSetsModel dsHalfDay = new DataSetsModel();
                dsHalfDay.label = "Half Day";
                dsHalfDay.data = new List<decimal>();
                dsHalfDay.backgroundColor = "rgba(255, 216, 0, 0.2)";
                dsHalfDay.borderColor = "rgba(255, 216, 0, 0.7)";
                dsHalfDay.pointBorderColor = "rgba(255, 216, 0, 0.7)";
                dsHalfDay.pointBackgroundColor = "rgba(255, 216, 0, 0.7)";
                dsHalfDay.pointHoverBackgroundColor = "#fff";
                dsHalfDay.pointHoverBorderColor = "rgba(220,220,220,1)";
                DataSetsModel dsAbsent = new DataSetsModel();
                dsAbsent.label = "Absent";
                dsAbsent.data = new List<decimal>();
                dsAbsent.backgroundColor = "rgba(234, 12, 12, 0.2)";
                dsAbsent.borderColor = "rgba(234, 12, 12, 0.7)";
                dsAbsent.pointBorderColor = "rgba(234, 12, 12, 0.7)";
                dsAbsent.pointBackgroundColor = "rgba(234, 12, 12, 0.7)";
                dsAbsent.pointHoverBackgroundColor = "#fff";
                dsAbsent.pointHoverBorderColor = "rgba(220,220,220,1)";
                DataSetsModel dsPresent = new DataSetsModel();
                dsPresent.label = "Present";
                dsPresent.data = new List<decimal>();
                dsPresent.backgroundColor = "rgba(38, 185, 154, 0.2)";
                dsPresent.borderColor = "rgba(38, 185, 154, 0.7)";
                dsPresent.pointBorderColor = "rgba(38, 185, 154, 0.7)";
                dsPresent.pointBackgroundColor = "rgba(38, 185, 154, 0.7)";
                dsPresent.pointHoverBackgroundColor = "#fff";
                dsPresent.pointHoverBorderColor = "rgba(220,220,220,1)";
                foreach (AttandanceModel att in Attandance)
                {
                    objCM.labels.Add(att.LegendName);
                    dsTotal.data.Add(att.TotalDays);
                    dsAbsent.data.Add(att.Absent);
                    dsPresent.data.Add(att.Total);
                    dsHalfDay.data.Add(att.HalfDay);
                }
                objCM.datasets = new List<DataSetsModel>();
                objCM.datasets.Add(dsTotal);
                objCM.datasets.Add(dsPresent);
                objCM.datasets.Add(dsHalfDay);
                objCM.datasets.Add(dsAbsent);
                return objCM;
            }
        }
        public StudentModel StudentDetail { get; set; }
        public List<MessageModel> Messages { get; set; }
        public List<EventModel> Events { get; set; }
        public List<EvaluationModel> Evaluations { get; set; }
        public List<NoticeModel> Notices { get; set; }
        public List<AttandanceCalenderModel> MonthAttandance { get; set; }
    }
}