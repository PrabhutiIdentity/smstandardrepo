using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Dapper;
using SMEnterprise.Repository;

namespace SMEnterprise.Models
{
    public class LeaveModel
    {
        public int LeaveID { get; set; }
        public int ApplicantType { get; set; }
        public int LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveReason { get; set; }
        public int ApplicantID { get; set; }
        public string ApplicantName { get; set; }
        public int IsApproved { get; set; }
        public int OpType { get; set; }
        public int UserID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int EmployeeType { get; set; }
        public DateTime OperationDate { get; set; }
        public int SBranchID { get; set; }
        public string Photo { get; set; }

    }
    public class LeaveDetailModel
    {
        public int LDID { get; set; }
        public int LeaveID { get; set; }
        public int LeaveTypeID { get; set; }
        public string LeaveTypeName { get; set; }
        public int IsSalaryDeduct { get; set; }
        public decimal Applied { get; set; }
        public DateTime LeaveDate { get; set; }
    }
    public class EmployeeLeaveSummery
    {
        public int LeaveTypeID { get; set; }
        public string LeaveTypeName { get; set; }
        public decimal YearlyQuota { get; set; }
        public decimal MonthlyQuota { get; set; }
        public int IsSalaryDeduct { get; set; }
        public decimal YearlyApplied { get; set; }
        public decimal MonthlyApplied { get; set; }
        public decimal Applied { get; set; }
    }
    public class EmployeeLeaveDetailModel
    {
        public int LeaveID { get; set; }
        public int EmployeeID { get; set; }
        public int EmployeeType { get; set; }
        public List<EmployeeLeaveSummery> LeaveBalance { get; set; }
        public List<EmployeeModel> Employees { get; set; }
        public LeaveModel Leave { get; set; }
        public List<LeaveDetailModel> LeaveDetails { get; set; }
    }

    public class EmployeeLeaveModel
    {
        public string UUID { get; set; }
        public int OpType { get; set; }
        public int LeaveID { get; set; }

        public string StatusText { get; set; }
        public string StatusColor { get; set; }

        public int LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveReason { get; set; }
        public int LeaveTypeApplied { get; set; }
        public string LeaveTypeName { get; set; }
        public string EmployeeSID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public int Gender { get; set; }
        public string Photo { get; set; }
        public int EmployeeType { get; set; }
        public int IsApproved { get; set; }
        public int ApplicantType { get; set; }
        public int SBranchID { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<EmployeeLeaveSummery> LeaveBalance { get; set; }

        public DataTable GetLeaveDetailsDataTable()
        {

            DataTable dtLeaveDetails = new DataTable();
            dtLeaveDetails.SetTypeName("ut_EmployeeLeaveDetails");
            dtLeaveDetails.Columns.Add("LDID");
            dtLeaveDetails.Columns.Add("LeaveTypeID");
            dtLeaveDetails.Columns.Add("Applied");

            foreach (EmployeeLeaveSummery e in LeaveBalance)
            {
                DataRow dr = dtLeaveDetails.NewRow();
                dr["LeaveTypeID"] = e.LeaveTypeID;
                dr["Applied"] = e.Applied;

                dtLeaveDetails.Rows.Add(dr);
            }

            return dtLeaveDetails;
        }
    }
    public class LeaveTypeModel
    {
        public int LeaveTypeID { get; set; }
        public string LeaveTypeName { get; set; }
        public decimal YearlyQuota { get; set; }
        public decimal MonthlyQuota { get; set; }
        public int IsSalaryDeduct { get; set; }
        public int IsCarryForward { get; set; }
        public decimal MaxConsecutive { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
    }
    public class LeaveEditModel
    {
        public List<LeaveDetailModel> LeaveDetails { get; set; }
        public List<LeaveTypeModel> LeaveTypes { get; set; }
        public LeaveModel Leave { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> Students { get; set; }
        public List<NameIDModel> Employees { get; set; }
        public string OtherData { get; set; }
        public int classID { get; set; }
        public int sectionID { get; set; }
        public int ClassID
        {
            get {
                if(Leave.ApplicantType==0)
                {
                    return 0;
                }
                else
                {
                    string[] det = OtherData.Split("-".ToCharArray());
                    return CommonUsage.ConvertToInt(det[0]);
                }
            }
            set { classID = value; }
        }
        public int SectionID
        {
            get
            {
                if (Leave.ApplicantType == 0)
                {
                    return 0;
                }
                else
                {
                    string[] det = OtherData.Split("-".ToCharArray());
                    return CommonUsage.ConvertToInt(det[1]);
                }
            }
            set { sectionID = value; }
        }
        //public decimal CLMApplied
        //{
        //    get
        //    {
        //        if (Leave.ApplicantType == 1)
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            string[] det = OtherData.Split(",".ToCharArray());
        //            string[] det2 = det[0].Split("/".ToCharArray());
        //            return CommonUsage.ConvertToDecimal(det2[0]);
        //        }
        //    }
        //}
        //public decimal CLMQuota
        //{
        //    get
        //    {
        //        if (Leave.ApplicantType == 1)
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            string[] det = OtherData.Split(",".ToCharArray());
        //            string[] det2 = det[0].Split("/".ToCharArray());
        //            return CommonUsage.ConvertToDecimal(det2[1]);
        //        }
        //    }
        //}
        //public decimal CLYApplied
        //{
        //    get
        //    {
        //        if (Leave.ApplicantType == 1)
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            string[] det = OtherData.Split(",".ToCharArray());
        //            string[] det2 = det[1].Split("/".ToCharArray());
        //            return CommonUsage.ConvertToDecimal(det2[0]);
        //        }
        //    }
        //}
        //public decimal CLYQuota
        //{
        //    get
        //    {
        //        if (Leave.ApplicantType == 1)
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            string[] det = OtherData.Split(",".ToCharArray());
        //            string[] det2 = det[1].Split("/".ToCharArray());
        //            return CommonUsage.ConvertToDecimal(det2[1]);
        //        }
        //    }
        //}
        //public decimal ELMApplied
        //{
        //    get
        //    {
        //        if (Leave.ApplicantType == 1)
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            string[] det = OtherData.Split(",".ToCharArray());
        //            string[] det2 = det[2].Split("/".ToCharArray());
        //            return CommonUsage.ConvertToDecimal(det2[0]);
        //        }
        //    }
        //}
        //public decimal ELMQuota
        //{
        //    get
        //    {
        //        if (Leave.ApplicantType == 1)
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            string[] det = OtherData.Split(",".ToCharArray());
        //            string[] det2 = det[2].Split("/".ToCharArray());
        //            return CommonUsage.ConvertToDecimal(det2[1]);
        //        }
        //    }
        //}
        //public decimal ELYApplied
        //{
        //    get
        //    {
        //        if (Leave.ApplicantType == 1)
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            string[] det = OtherData.Split(",".ToCharArray());
        //            string[] det2 = det[3].Split("/".ToCharArray());
        //            return CommonUsage.ConvertToDecimal(det2[0]);
        //        }
        //    }
        //}
        //public decimal ELYQuota
        //{
        //    get
        //    {
        //        if (Leave.ApplicantType == 1)
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            string[] det = OtherData.Split(",".ToCharArray());
        //            string[] det2 = det[3].Split("/".ToCharArray());
        //            return CommonUsage.ConvertToDecimal(det2[1]);
        //        }
        //    }
        //}
        //public decimal PLMApplied
        //{
        //    get
        //    {
        //        if (Leave.ApplicantType == 1)
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            string[] det = OtherData.Split(",".ToCharArray());
        //            string[] det2 = det[4].Split("/".ToCharArray());
        //            return CommonUsage.ConvertToDecimal(det2[0]);
        //        }
        //    }
        //}
        //public decimal PLMQuota
        //{
        //    get
        //    {
        //        if (Leave.ApplicantType == 1)
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            string[] det = OtherData.Split(",".ToCharArray());
        //            string[] det2 = det[4].Split("/".ToCharArray());
        //            return CommonUsage.ConvertToDecimal(det2[1]);
        //        }
        //    }
        //}
        //public decimal PLYApplied
        //{
        //    get
        //    {
        //        if (Leave.ApplicantType == 1)
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            string[] det = OtherData.Split(",".ToCharArray());
        //            string[] det2 = det[5].Split("/".ToCharArray());
        //            return CommonUsage.ConvertToDecimal(det2[0]);
        //        }
        //    }
        //}
        //public decimal PLYQuota
        //{
        //    get
        //    {
        //        if (Leave.ApplicantType == 1)
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            string[] det = OtherData.Split(",".ToCharArray());
        //            string[] det2 = det[5].Split("/".ToCharArray());
        //            return CommonUsage.ConvertToDecimal(det2[1]);
        //        }
        //    }
        //}
    }
}