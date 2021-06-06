using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class HolidayModel
    {
        public int IsSMSSent { get; set; }
        public int IsStudents { get; set; }
        public int IsEmployee { get; set; }
        public string AssociatedIDs { get; set; }
        public string EmployeeTypes { get; set; }
        public string SMSSentTo { get; set; }
        private string holidayTypeName;

        public string HolidayTypeName
        {
            get { return holidayTypeName; }
            set { holidayTypeName = value; }
        }
        private int holidayID;

        public int HolidayID
        {
            get { return holidayID; }
            set { holidayID = value; }
        }

        private string classes;

        public string Classes
        {
            get { return classes; }
            set { classes = value; }
        }

        private int status;

        public int Status
        {
            get { return status; }
            set { status = value; }
        }

        private int holidayType;

        public int HolidayType
        {
            get { return holidayType; }
            set { holidayType = value; }
        }

        private int dayDuration;

        public int DayDuration
        {
            get { return dayDuration; }
            set { dayDuration = value; }
        }
        private int days;

        public int Days
        {
            get { return days; }
            set { days = value; }
        }
        private DateTime endDate;

        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; }
        }

        private DateTime startDate;

        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value; }
        }
        private string dayDurationName;

        public string DayDurationName
        {
            get { return dayDurationName; }
            set { dayDurationName = value; }
        }

        private string description;

        public string HolidayDescription
        {
            get { return description; }
            set { description = value; }
        }

        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        private string classesIncludedIDs;

        public string ClassesIncludedIDs
        {
            get { return classesIncludedIDs; }
            set { classesIncludedIDs = value; }
        }
        private string classesIncluded;

        public string ClassesIncluded
        {
            get { return classesIncluded; }
            set { classesIncluded = value; }
        }

        private int userID;

        public int UserID
        {
            get { return userID; }
            set { userID = value; }
        }
        
        private DateTime operationDate;

        public DateTime OperationDate
        {
            get { return operationDate; }
            set { operationDate = value; }
        }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
    }
    public class HolidayDetailModel
    {
       public HolidayModel Holiday ;
       public List<string> ClassesIncluded;
       public List<ClassModel> Classes;
        public List<EmployeeTypeModel> EmployeeTypes;

    }
    public class HolidayPrintModel
    {
        public HolidayModel Holiday;
        public SchoolDetailModel SchoolDetail { get; set; }
        public List<EmployeeModel> Staff { get; set; }
    }
}