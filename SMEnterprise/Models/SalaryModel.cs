using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class SalarySlipModel
    {
        public int SPMID { get; set; }
        public int EmployeeID { get; set; }
        public int EmployeeType { get; set; }
        public int SBranchID { get; set; }
        public int SalaryYear { get; set; }
        public int SalaryMonth { get; set; }
        public SBranchModel Branch { get; set; }
        public EmployeeModel Employee { get; set; }
        public SalaryProcessingModel Salary { get; set; }
        public List<SalaryProcessingDetailModel> Details { get; set; }
    }
    public class EmployeeMonthSalarySummeryModel
    {
        public decimal MonthDays { get; set; }
        public decimal WorkingDays { get; set; }
        public decimal WeekOffs { get; set; }
        public decimal PubHolidays { get; set; }
        public decimal Leaves { get; set; }
        public decimal LWP { get; set; }
        public decimal Present { get; set; }

    }
    public class DayHolidayStatusModel
    {
        public int DayID { get; set; }
        public decimal HStatus { get; set; }
        public int HDuration { get; set; }
    }
    public class DayLeaveStatusModel
    {
        public int DayID { get; set; }
        public decimal LStatus { get; set; }
        public int IsSalaryDeduct { get; set; }
        public int LDuration { get; set; }
    }
    public class EmployeeSalaryDetailsPageModel
    {
        public int EmployeeID { get; set; }
        public int EmployeeType { get; set; }
        public int SalaryMonth { get; set; }
        public int SalaryYear { get; set; }
        public int SBranchID { get; set; }
        public EmployeeModel Employee { get; set; }
        public SBranchModel Branch { get; set; }
        public EmployeeMonthSalarySummeryModel Summery { get; set; }
        public List<DayHolidayStatusModel> Holidays { get; set; }
        public List<DayLeaveStatusModel> Leaves { get; set; }
        public List<EmployeeSalaryDetailsListModel> Categories { get; set; }
        public List<EmployeeSalaryDetailsListModel> AdvancePayments { get; set; }
    }
    public class SalaryProcessingModel
    {
        public int SPMID { get; set; }
        public int EmployeeID { get; set; }
        public int EmployeeType { get; set; }
        public int SMonth { get; set; }
        public int PaymentMode { get; set; }
        public int SYear { get; set; }
        public string SalaryReferanceNumber { get; set; }
        public string ReferanceNumber { get; set; }
        public DateTime ProcessingDate { get; set; }
        public string Remark { get; set; }
        public int SBranchID { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserID { get; set; }
        public int Status { get; set; }
        public decimal SalaryAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal TotalDays { get; set; }
        public decimal PresentDays { get; set; }
        public decimal LWP { get; set; }
        public decimal PaidLeaves { get; set; }
        public decimal Holidays { get; set; }
        public decimal WeekOffs { get; set; }
        public decimal GrossEarning { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPayble { get; set; }
        public List<SalaryProcessingDetailModel> Details { get; set; }
        public DataTable GetSalaryDetailsDataTable()
        {
            DataTable dtFeeStructureDetails = new DataTable();
            dtFeeStructureDetails.SetTypeName("ut_EmpSalaryProcessingDetails");
            dtFeeStructureDetails.Columns.Add("SPDID");
            dtFeeStructureDetails.Columns.Add("SPMID");
            dtFeeStructureDetails.Columns.Add("EmployeeID");
            dtFeeStructureDetails.Columns.Add("SalaryCategoryID");
            dtFeeStructureDetails.Columns.Add("RefID");
            dtFeeStructureDetails.Columns.Add("Amount");
            dtFeeStructureDetails.Columns.Add("Paid");
            dtFeeStructureDetails.Columns.Add("IsDuePaid");
            dtFeeStructureDetails.Columns.Add("SMonth");
            dtFeeStructureDetails.Columns.Add("SYear");
            dtFeeStructureDetails.Columns.Add("Status");
            dtFeeStructureDetails.Columns.Add("Title");
            dtFeeStructureDetails.Columns.Add("Type");
            dtFeeStructureDetails.Columns.Add("OpType");
            if (Details == null)
            {
                Details = new List<SalaryProcessingDetailModel>();
            }

            foreach (SalaryProcessingDetailModel e in Details.Where(c => c.Status == 1))
            {
                if (e.Type != 3)
                {
                    DataRow dr = dtFeeStructureDetails.NewRow();
                    dr["SPDID"] = e.SPDID;
                    dr["SalaryCategoryID"] = e.SalaryCategoryID;
                    dr["Amount"] = e.Amount;
                    dr["Paid"] = e.Paid;
                    dr["Title"] = e.Title;
                    dr["Type"] = e.Type;
                    dtFeeStructureDetails.Rows.Add(dr);
                }
                else if (e.Paid != 0)
                {
                    DataRow dr = dtFeeStructureDetails.NewRow();
                    dr["SPDID"] = e.SPDID;
                    dr["SalaryCategoryID"] = e.SalaryCategoryID;
                    dr["Amount"] = e.Paid;
                    dr["Paid"] = e.Paid;
                    dr["Title"] = e.Title;
                    dr["Type"] = e.Type;
                    dtFeeStructureDetails.Rows.Add(dr);
                }
            }
            return dtFeeStructureDetails;
        }
    }
    public class SalaryProcessingDetailModel
    {
        public int SPDID { get; set; }
        public int SPMID { get; set; }
        public int EmployeeID { get; set; }
        public int SalaryCategoryID { get; set; }
        public int RefID { get; set; }
        public decimal Amount { get; set; }
        public decimal Paid { get; set; }
        public int IsDuePaid { get; set; }
        public int SMonth { get; set; }
        public int SYear { get; set; }
        public int Status { get; set; }
        public string Title { get; set; }
        public int Type { get; set; }
        public int OpType { get; set; }
    }

    public class SalaryDaysModel
    {
        public decimal SalaryDays { get; set; }
        public decimal TotalDays { get; set; }
        public decimal PayableLeaves { get; set; }
        public decimal NonPayableLeaves { get; set; }
    }
    public class EmployeeSalaryDetailsListModel
    {
        public int TypeID { get; set; }
        public string TypeName { get; set; }
        public int IsDeduction { get; set; }
        public int AttendanceType { get; set; }
        public int MinDays { get; set; }
        public decimal Amount { get; set; }
    }
    public class EmployeeSalaryDetailModel
    {
        public int EmployeeID { get; set; }
        public int EmployeeTypeID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int SBranchID { get; set; }
        public SBranchModel Branch { get; set; }
        public EmployeeModel Employee { get; set; }
        public SalaryDaysModel SalaryDays { get; set; }
        public decimal LeaveDeduction { get; set; }
        public decimal DaysPayble { get; set; }
        public List<EmployeeSalaryDetailsListModel> SalaryHeads { get; set; }

    }
    public class EmployeeSalaryModel
    {
        public int IsDeduction { get; set; }
        public int EmployeeID { get; set; }
        public int EmployeeTypeID { get; set; }
        public int EmployeeSalaryID { get; set; }
        public int SalaryTypeID { get; set; }
        public string SalaryTypeName { get; set; }
        public decimal Amount { get; set; }
        public decimal SalaryStructure { get; set; }
    }

    public class EmployeeTypeSalaryModel
    {
        public int ETypeSTypeID { get; set; }
        public int EmployeeTypeID { get; set; }
        public int TypeID { get; set; }
        public int IsDeduction { get; set; }
        public string TypeName { get; set; }
        public decimal Amount { get; set; }
        public int TypeApplicable { get; set; }
        public string TypeApplicableName { get; set; }
        public string Months { get; set; }
        public int SBranchID { get; set; }
    }
    public class SalaryCategoryPageModel
    {
        public List<NameIDModel> SalaryApplicableTypes { get; set; }
        public List<SalaryCategoryModel> SalaryTypes { get; set; }
    }
    public class SalaryCategoryModel
    {
        public int AttendanceType { get; set; }
        public int MinDays { get; set; }
        public int IsDeduction { get; set; }
        public decimal Amount { get; set; }
        public string Months { get; set; }
        public int RefID { get; set; }
        public int SBranchID { get; set; }
        private DateTime operationDate;
        private int userID;
        private int typeID;
        private string typeName;
        private int typeApplicable;
        private string typeApplicableName;
        private string chartColor;
        public int OpType { get; set; }
        public string ChartColor
        {
            get
            {
                return chartColor;
            }

            set
            {
                chartColor = value;
            }
        }

        public string TypeApplicableName
        {
            get
            {
                return typeApplicableName;
            }

            set
            {
                typeApplicableName = value;
            }
        }

        public int TypeApplicable
        {
            get
            {
                return typeApplicable;
            }

            set
            {
                typeApplicable = value;
            }
        }

        public string TypeName
        {
            get
            {
                return typeName;
            }

            set
            {
                typeName = value;
            }
        }

        public int TypeID
        {
            get
            {
                return typeID;
            }

            set
            {
                typeID = value;
            }
        }

        public int UserID
        {
            get
            {
                return userID;
            }

            set
            {
                userID = value;
            }
        }

        public DateTime OperationDate
        {
            get
            {
                return operationDate;
            }

            set
            {
                operationDate = value;
            }
        }
    }
}