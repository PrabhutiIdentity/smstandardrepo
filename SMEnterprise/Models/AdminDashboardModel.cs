using SMEnterprise.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class AdminDashboardModel
    {
        public int ClassCount { get; set; }
        public int TeacherCount { get; set; }
        public int AbsentTeachers { get; set; }
        public string SessionName { get; set; }
        public int StudentsCount { get; set; }
        public decimal MonthlyCollection { get; set; }
        public decimal MonthlyExpence { get; set; }
        public List<NameIDModel> DailyFTCollection { get; set; }
        public List<StudentBirthdaySMSModel> BirthDays { get; set; }
        public List<PaymentModeModel> DailyFMCollection { get; set; }
        public List<PaymentModeModel> MonthlyFMCollection { get; set; }
        public List<PaymentModeModel> QuarterlyCollection { get; set; }
        public List<PaymentModeModel> AnnualCollection { get; set; }
        public List<ClassSectionChartModel> ClassSectionData { get; set; }
        public List<EmployeeModel> AbsentTeacherList { get; set; }
        public ChartModel ClassSection
        {
            get
            {
                ChartModel objCM = new ChartModel();
                objCM.labels = new List<string>();
                DataSetsModel dsClasses = new DataSetsModel();
                dsClasses.label = "Class";
                dsClasses.data = new List<decimal>();
                dsClasses.backgroundColor = "rgba(74, 88, 106, 0.21)";
                dsClasses.borderColor = "rgba(74, 88, 106, 0.7)";
                dsClasses.pointBorderColor = "rgba(74, 88, 106, 0.7)";
                dsClasses.pointBackgroundColor = "rgba(74, 88, 106, 0.7)";
                dsClasses.pointHoverBackgroundColor = "#fff";
                dsClasses.pointHoverBorderColor = "rgba(220,220,220,1)";
                DataSetsModel dsStudents = new DataSetsModel();
                dsStudents.label = "Students";
                dsStudents.data = new List<decimal>();
                dsStudents.backgroundColor = "rgba(231, 76, 60, 0.2)";
                dsStudents.borderColor = "rgba(231, 76, 60, 0.7)";
                dsStudents.pointBorderColor = "rgba(231, 76, 60, 0.7)";
                dsStudents.pointBackgroundColor = "rgba(231, 76, 60, 0.7)";
                dsStudents.pointHoverBackgroundColor = "#fff";
                dsStudents.pointHoverBorderColor = "rgba(220,220,220,1)";
                
                foreach (ClassSectionChartModel att in ClassSectionData)
                {
                    objCM.labels.Add(att.LegendName);
                    dsClasses.data.Add(att.ClassCapacity);
                    dsStudents.data.Add(att.Students);
                }
                objCM.datasets = new List<DataSetsModel>();
                objCM.datasets.Add(dsClasses);
                objCM.datasets.Add(dsStudents);
                return objCM;
            }
        }

        public List<AttandanceModel> EmployeeAttandance { get; set; }
        public ChartModel EmployeeAttandanceChart
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
                foreach (AttandanceModel att in EmployeeAttandance)
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
        public List<FeeCategoryModel> FeeCategries { get; set; }
        public List<FeeDetailsModel> FeeDetails { get; set; }
    }
    public class ClassSectionChartModel
    {
        public string LegendName { get; set; }
        public int ClassCapacity { get; set; }
        public int Students { get; set; }
        public List<FeeChartDetailsModel> FeeDetails { get; set; }
    }
    public class FeeTypeChartModel
    {
        public int FeeTypeID { get; set; }
        public string FeeTypeName { get; set; }
        public string ChartColor { get; set; }
    }
    public class FeeChartDetailsModel
    {
        public string Label { get; set; }
        public int FeeTypeID { get; set; }
        public decimal Amount { get; set; }
    }
}