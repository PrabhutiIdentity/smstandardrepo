using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class ExtraIncomePageModel
    {
        public List<ExtraIncomeHeadModel> Heads { get; set; }
        public List<ExtraIncomeModel> Incomes { get; set; }
        public ExtraIncomeModel IncomeDetail { get; set; }
        public int SBranchID { get; set; }
        public int PaymentMode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class ExtraIncomeHeadModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
    }
    public class ExtraIncomeModel
    {
        public int UserID { get; set; }
        public int ID { get; set; }
        public int SBranchID { get; set; }
        public string Title { get; set; }
        public string PaidBy { get; set; }
        public int FY { get; set; }
        public int EIHeadID { get; set; }
        public string HeadName { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public DateTime PaidDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RecievedBy { get; set; }
        public int PaymentMode { get; set; }
        public string ReferanceNo { get; set; }
        public string PayerAddress { get; set; }
        public string PayerCity { get; set; }
        public string PayerState { get; set; }
        public string PayerPin { get; set; }
        public string PayerContact { get; set; }
        public string PayerPAN { get; set; }
        public int OpType { get; set; }
    }
    public class ExpenceCategoryModel
    {
        private DateTime operationDate;
        private int userID;
        private int expenceTypeID;
        private string expenceTypeName;
        private int expenceTypeApplicable;
        private string expenceTypeApplicableName;
        private string chartColor;
        public int OpType { get; set; }
        public int SBranchID { get; set; }

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

        public string ExpenceTypeApplicableName
        {
            get
            {
                return expenceTypeApplicableName;
            }

            set
            {
                expenceTypeApplicableName = value;
            }
        }

        public int ExpenceTypeApplicable
        {
            get
            {
                return expenceTypeApplicable;
            }

            set
            {
                expenceTypeApplicable = value;
            }
        }

        public string ExpenceTypeName
        {
            get
            {
                return expenceTypeName;
            }

            set
            {
                expenceTypeName = value;
            }
        }

        public int ExpenceTypeID
        {
            get
            {
                return expenceTypeID;
            }

            set
            {
                expenceTypeID = value;
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
    public class ExpenceManagementModel
    {
        public int ExpenceType { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int SBranchID { get; set; }
        public DateTime StartDate { get; set; }
        public List<NameIDModel> ExpenceTypes { get; set; }
        public List<ExpenceModel> Expences { get; set; }
        public SBranchModel Branch { get; set; }
    }
    public class ExpenceModel
    {
        public int ExpenceID { get; set; }
        public string ReferanceNumber { get; set; }
        public int PaymentType { get; set; }
        public int PaymentMode { get; set; }
        public string Remark { get; set; }
        public decimal PaymentAmount { get; set; }
        public int PaymentStatus { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime PaymentDate { get; set; }
        public int SBranchID { get; set; }
        public int PayeeType { get; set; }
        public string ExpenceTitle { get; set; }
        public DateTime EnteredDate { get; set; }
        public int OpType { get; set; }
        public string ExpenceType { get; set; }
        public string ChartColor { get; set; }
        public int ExpenceTypeID { get; set; }
        public List<ExpenceDetailsModel> ExpenceDetails { get; set; }
        public List<ExpenceBillModel> ExpenceBills { get; set; }
        public List<NameIDModel> ExpenceTypes { get; set; }
        public DataTable GetExpenceDetailsDataTable()
        {

            DataTable dtExpenceDetails = new DataTable();
            dtExpenceDetails.SetTypeName("ut_ExpenceDetails");
            dtExpenceDetails.Columns.Add("CollectionID");
            dtExpenceDetails.Columns.Add("ExpenceID");
            dtExpenceDetails.Columns.Add("Quantity");
            dtExpenceDetails.Columns.Add("Rate");
            dtExpenceDetails.Columns.Add("Amount");
            dtExpenceDetails.Columns.Add("ItemName");
            dtExpenceDetails.Columns.Add("OpType");

            if (ExpenceDetails == null)
            {
                ExpenceDetails = new List<ExpenceDetailsModel>();
            }

            foreach (ExpenceDetailsModel e in ExpenceDetails)
            {
                if (e.Amount != 0)
                {
                    DataRow dr = dtExpenceDetails.NewRow();
                    dr["CollectionID"] = e.CollectionID;
                    dr["ExpenceID"] = ExpenceID;
                    dr["Quantity"] = e.Quantity;
                    dr["Rate"] = e.Rate;
                    dr["Amount"] = e.Amount;
                    dr["ItemName"] = e.ItemName;
                    dr["OpType"] = e.OpType;
                    dtExpenceDetails.Rows.Add(dr);
                }
            }
            return dtExpenceDetails;
        }
        public DataTable GetExpenceBillsDataTable()
        {

            DataTable dtExpenceBills = new DataTable();
            dtExpenceBills.SetTypeName("ut_ExpenceBills");
            dtExpenceBills.Columns.Add("BillID");
            dtExpenceBills.Columns.Add("ExpenceID");
            dtExpenceBills.Columns.Add("BillFor");
            dtExpenceBills.Columns.Add("ImageName");
            dtExpenceBills.Columns.Add("OpType");
            if(ExpenceBills == null)
            {
                ExpenceBills = new List<ExpenceBillModel>();
            }

            foreach (ExpenceBillModel e in ExpenceBills)
            {
                if (!String.IsNullOrEmpty(e.ImageName) || e.OpType == -1)
                {
                    DataRow dr = dtExpenceBills.NewRow();
                    dr["BillID"] = e.BillID;
                    dr["ExpenceID"] = ExpenceID;
                    dr["BillFor"] = e.BillFor;
                    dr["ImageName"] = e.ImageName;
                    dr["OpType"] = e.OpType;
                    dtExpenceBills.Rows.Add(dr);
                }
            }
            return dtExpenceBills;
        }
    }
    public class ExpenceDetailsModel
    {
        public int CollectionID { get; set; }
        public int ExpenceID { get; set; }
        public string ItemName { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public int OpType { get; set; }
    }
    public class ExpenceBillModel
    {
        public int BillID { get; set; }
        public int ExpenceID { get; set; }
        public string BillFor { get; set; }
        public string ImageName { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public string OldImageName { get; set; }
        public int OpType { get; set; }
    }
}