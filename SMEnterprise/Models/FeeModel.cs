using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class FeeDetailsAppResponseModel
    {
        public List<FeeDetailsAppResponseMonthModel> Months { get; set; }
    }
    public class FeeDetailsAppResponseMonthModel
    {
        public int FeeMonth { get; set; }
        public int FeeYear { get; set; }
        public string MonthName { get {
                if (FeeYear == 0)
                {
                    return "Total";
                }
                else
                {
                    DateTime dt = new DateTime(FeeYear, FeeMonth, 1);
                    return dt.ToString("MMM, yyyy");
                }
            } }
        public decimal ApplicableFee { get; set; }
        public decimal PaymentRecieved { get; set; }
        public decimal Waiver { get; set; }
        public List<FeeDetailsAppResponseMonthFeeModel> Summery { get; set; }
    }
    public class FeeDetailsAppResponseMonthFeeModel
    {
        public string FeeTypeName { get; set; }
        public decimal ApplicableFee { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal NetPayable { get; set; }
    }
    public class FeeRefundPrintModel
    {
        public FeeRefundModel RefundDetails { get; set; }
        public SBranchModel SBranchDetails { get; set; }
    }
    public class FeeRefundEditModel
    {
        public int RefundID { get; set; }
        public int SBranchID { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> Students { get; set; }
        public FeeRefundModel RefundDetails { get; set; }
    }
    public class FeeRefundPageModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int Status { get; set; }
        public int SBranchID { get; set; }
        public int SessionID { get; set; }
        public List<FeeRefundModel> Refunds { get; set; }
    }
    public class FeeRefundModel
    {
        public int RefundID { get; set; }
        public int RefundTo { get; set; }
        public string StudentSID { get; set; }
        public string SchoolUID { get; set; }
        public string RefundName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public DateTime RefundDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime SessionStartDate { get; set; }
        public DateTime SessionEndDate { get; set; }
        public string SessionName { get; set; }
        public string FatherName { get; set; }
        public int RefundBy { get; set; }
        public string Reason { get; set; }
        public int SessionID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int StudentSessionUID { get; set; }
        public int SBranchID { get; set; }
        public decimal RefundAmount { get; set; }
        public int Status { get; set; }

    }
    public class ParentFeeDetailsPageModel
    {
        public int ParentID { get; set; }
        public string PaymentRecieptNo { get; set; }
        public int PaymentID { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public string YearMonth { get; set; }
        public string Remark { get; set; }
        public int SBranchID { get; set; }
        public string CollectedBy { get; set; }
        public decimal ApplicableAmount { get; set; }
        public string ReferanceNumber { get; set; }
        public int PaymentMode { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public int Year { get; set; }
        public DateTime QDate { get; set; }
        public int SessionID { get; set; }
        public SchoolSessionModel Session { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
        public ParentModel Parent { get; set; }
        public List<StudentModel> Students { get; set; }
        public List<ParentStudentFeeDetails> FeeDetails { get; set; }

        public DataTable GetFeePaymentsDataTable()
        {

            DataTable dtFeePaymentDetails = new DataTable();
            dtFeePaymentDetails.SetTypeName("ut_PaymentDetails");
            dtFeePaymentDetails.Columns.Add("CollectionID");
            dtFeePaymentDetails.Columns.Add("PaymentID");
            dtFeePaymentDetails.Columns.Add("FeeTypeID");
            dtFeePaymentDetails.Columns.Add("PayeeID");
            dtFeePaymentDetails.Columns.Add("Amount");
            dtFeePaymentDetails.Columns.Add("DiscPer");
            dtFeePaymentDetails.Columns.Add("DiscAmt");
            dtFeePaymentDetails.Columns.Add("NetApplicablePayment");
            dtFeePaymentDetails.Columns.Add("PaymentRecieved");
            dtFeePaymentDetails.Columns.Add("Year");
            dtFeePaymentDetails.Columns.Add("Month");
            dtFeePaymentDetails.Columns.Add("PaymentDate");
            dtFeePaymentDetails.Columns.Add("SBranchID");
            dtFeePaymentDetails.Columns.Add("DuesPaidCount");
            dtFeePaymentDetails.Columns.Add("PaymentDetails");

            foreach (ParentStudentFeeDetails e in FeeDetails)
            {
                if (e.PaymentRecieved > 0)
                {
                    DataRow dr = dtFeePaymentDetails.NewRow();
                    dr["PaymentID"] = PaymentID;
                    dr["FeeTypeID"] = e.FeeTypeID;
                    dr["PayeeID"] = e.PayeeID;
                    dr["Amount"] = e.Amount;
                    dr["DiscAmt"] = e.DiscAmt;
                    dr["NetApplicablePayment"] = e.NetApplicablePayment;
                    dr["PaymentRecieved"] = e.PaymentRecieved;
                    dr["Year"] = e.Year;
                    dr["Month"] = e.Month;
                    dr["SBranchID"] = e.SBranchID;
                    dr["DuesPaidCount"] = e.DuesPaidCount;

                    dtFeePaymentDetails.Rows.Add(dr);
                }
            }

            return dtFeePaymentDetails;
        }
    }

    public class ParentStudentFeeDetails
    {
        public int FeeTypeID { get; set; }
        public string FeeTypeName { get; set; }
        public decimal NetApplicablePayment { get; set; }
        public decimal DiscAmt { get; set; }
        public decimal ApprovedAmount { get; set; }
        public int SBranchID { get; set; }
        public decimal Amount { get; set; }
        public int DuesPaidCount { get; set; }
        public decimal PaymentRecieved { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime PaymentDate { get; set; }
        public int PayeeID { get; set; }
    }
    public class FeePaymentsModel
    {
        public int StudentID { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public int Gender { get; set; }
        public string StudentSID { get; set; }
        public string RollNo { get; set; }
        public int QuotaID { get; set; }
        public int FeePaymentMode { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int GroupID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string QuotaName { get; set; }
        public int PaymentStatus { get; set; }
        public decimal ApplicableFee { get; set; }
        public decimal DiscAmt { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal LateFee { get; set; }
        public DateTime LastDate { get; set; }
        public DateTime PaidDate { get; set; }
        public string MonthNames { get; set; }
        public int IsPayNow { get; set; }
    }

    public class FeeCategoryEditModel
    {
        public List<FeeCategoryModel> FeeCategories { get; set; }
        public List<NameIDModel> FeeApplicables { get; set; }
    }
    public class FeeCategoryModel
    {
        private DateTime operationDate;
        private int userID;
        private int feeTypeID;
        private string feeTypeName;
        private int feeTypeApplicable;
        private string feeTypeApplicableName;
        private string chartColor;
        public string Months { get; set; }
        public int OpType { get; set; }
        public int SBranchID { get; set; }
        public int FeeTypeCount { get; set; }
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

        public string FeeTypeApplicableName
        {
            get
            {
                return feeTypeApplicableName;
            }

            set
            {
                feeTypeApplicableName = value;
            }
        }

        public int FeeTypeApplicable
        {
            get
            {
                return feeTypeApplicable;
            }

            set
            {
                feeTypeApplicable = value;
            }
        }

        public string FeeTypeName
        {
            get
            {
                return feeTypeName;
            }

            set
            {
                feeTypeName = value;
            }
        }

        public int FeeTypeID
        {
            get
            {
                return feeTypeID;
            }

            set
            {
                feeTypeID = value;
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
    public class PayDetailFeeTypesModel
    {
        public int FeeTypeID { get; set; }
        public string FeeTypeName { get; set; }
        public decimal ApplicableFee { get; set; }
        public decimal Waiver { get; set; }
        public decimal PaymentRecieved { get; set; }
    }
    public class PayDetailMonthsModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int FeeMonth { get; set; }
        public int FeeYear { get; set; }
        public decimal ApplicableFee { get; set; }
        public decimal Waiver { get; set; }
        public decimal PaymentRecieved { get; set; }
    }
    public class FeeDetailsModel
    {
        public int FeeMonth { get; set; }
        public int FeeYear { get; set; }
        public decimal QDiscount { get; set; }
        public decimal RDiscount { get; set; }
        public decimal PayApplicableAmount { get; set; }
        public decimal CustomFee { get; set; }
        public decimal PaidAmount { get; set; }
        public int IsCustomFee { get; set; }
        public int FeeTypeApplicable { get; set; }
        public int IsPayment { get; set; }
        public int DuesPaidCount { get; set; }
        public string FeeTypeName { get; set; }
        public decimal FeeAmount { get; set; }
        public int FeePaymentMode { get; set; }
        public decimal ApplicableFee { get; set; }
        public decimal Discount { get; set; }
        private int collectionID;
        private int paymentID;
        private int feeTypeID;
        private int studentID;
        private decimal amount;
        private decimal discPer;
        private decimal discAmt;
        private decimal netApplicablePayment;
        private decimal paymentRecieved;
        private int year;
        private int month;
        private string sBranchID;
        private int sessionID;

        public int Month
        {
            get
            {
                if(month==0)
                {
                    return FeeMonth;
                }
                return month;
            }

            set
            {
                month = value;
            }
        }

        public int Year
        {
            get
            {
                if(year==0)
                {
                    return FeeYear;
                }
                return year;
            }

            set
            {
                year = value;
            }
        }

        public decimal PaymentRecieved
        {
            get
            {
                return paymentRecieved;
            }

            set
            {
                paymentRecieved = value;
            }
        }

        public decimal NetApplicablePayment
        {
            get
            {
                return netApplicablePayment;
            }

            set
            {
                netApplicablePayment = value;
            }
        }

        public decimal DiscAmt
        {
            get
            {
                return discAmt;
            }

            set
            {
                discAmt = value;
            }
        }

        public decimal DiscPer
        {
            get
            {
                return discPer;
            }

            set
            {
                discPer = value;
            }
        }

        public decimal Amount
        {
            get
            {
                return amount;
            }

            set
            {
                amount = value;
            }
        }

        public int StudentID
        {
            get
            {
                return studentID;
            }

            set
            {
                studentID = value;
            }
        }

        public int FeeTypeID
        {
            get
            {
                return feeTypeID;
            }

            set
            {
                feeTypeID = value;
            }
        }

        public int PaymentID
        {
            get
            {
                return paymentID;
            }

            set
            {
                paymentID = value;
            }
        }

        public int CollectionID
        {
            get
            {
                return collectionID;
            }

            set
            {
                collectionID = value;
            }
        }

        public string SBranchID
        {
            get
            {
                return sBranchID;
            }

            set
            {
                sBranchID = value;
            }
        }
        public int SessionID
        {
            get
            {
                return sessionID;
            }

            set
            {
                sessionID = value;
            }
        }
        
    }
    public class FeePaymentRowModel
    {
        public FeePaymentModel StudentDetails { get; set; }
        public int PaymentID { get; set; }
    }

    public class FeePaymentModel
    {
<<<<<<< HEAD
=======
        public decimal RefundAmount { get; set; }
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
        public int IsCustomFee { get; set; }
        public string MotherName { get; set; }
        public int UserID { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal PreviousDue { get; set; }
        public decimal Discounts { get; set; }
        public decimal Paid { get; set; }
        public string PaymentRecieptNo { get; set; }
        public string FatherName { get; set; }
        public string ExcludedFees { get; set; }
        public string SchoolUID { get; set; }
        public int LastPayDay { get; set; }
        public int Day { get; set; }
        public int FeePaymentMode { get; set; }
        public string FeePayMode { get; set; }
        public int SessionID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime FeeDate { get; set; }
        public DateTime QDate { get; set; }
        public string Photo { get; set; }
        public int StudentID { get; set; }
        public string WaiverMonths { get; set; }
        public string CollectedBy { get; set; }
        public int Gender { get; set; }
        public int ClassID { get; set; }
        public string ClassSection { get; set; }
        public int SectionID { get; set; }
        public int GroupID { get; set; }
        public int SBranchID { get; set; }
        public int PayeeType { get; set; }
        public int PaymentType { get; set; }
        public string StudentSID { get; set; }
        public string Name { get; set; }
        public string RollNo { get; set; }
        public string QuotaName { get; set; }
        public int PaymentMode { get; set; }
        public string Remark { get; set; }
        public int PaymentID { get; set; }
        public string ReferanceNumber { get; set; }
        public int PaymentStatus { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal PreviousDues { get; set; }
        public decimal LateFee { get; set; }
        public decimal ApplicableFee { get; set; }
        public decimal DiscAmt { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal TransportFee { get; set; }
        public decimal HostelFee { get; set; }
        public string PayMonths { get; set; }
        public string SessionName { get; set; }
        public List<FeeDetailsModel> PaymentDetails { get; set; }
        public List<FeeDetailsModel> PreviourDuesDetails { get; set; }
        public List<NameIDModel> FeeTypes { get; set; }
        public List<PayDetailFeeTypesModel> FeeTypeSummery { get; set; }
        public List<PayDetailMonthsModel> Months { get; set; }
        public DateTime SessionStartDate { get; set; }
        public DateTime SessionEndDate { get; set; }
        public DateTime SchoolSessionStartDate { get; set; }
        public SBranchModel SBranchDetails { get; set; }
        public StudentModel StudentDetails { get; set; }
        public int FeeTypeID { get; set; }
        public decimal PaymentRecieved { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<FeeDetailsModel> FeeListDetail { get; set; }
        public List<StudentModel> StudentList { get; set; }
        public DateTime DemandMonth { get; set; }
        public string Class { get; set; }
        public string Section { get; set; }
        public string ClassName
        {
            get
            {
                try
                {
                    char[] splitter = { '\\' };
                    if (ClassSection.Split(splitter).Length > 1)
                    {
                        return ClassSection.Split(splitter)[0];
                    }
                    else
                    {
                        return "";
                    }
                }
                catch
                {
                    return "";
                }
            }
        }
        public string SectionName
        {
            get
            {
                try
                {
                    char[] splitter = { '\\' };
<<<<<<< HEAD
                    if (ClassSection.Split(splitter).Length > 1)
=======
                    
                        if (ClassSection.Split(splitter).Length > 1)
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
                    {
                        return ClassSection.Split(splitter)[1];
                    }
                    else
                    {
                        return "";
                    }
                }
                catch
                {
                    return "";
                }
            }
        }
        public DataTable GetFeePaymentsDataTable()
        {

            DataTable dtFeePaymentDetails = new DataTable();
            dtFeePaymentDetails.SetTypeName("StudentFeeDetails");
            dtFeePaymentDetails.Columns.Add("CollectionID");
            dtFeePaymentDetails.Columns.Add("PaymentID");
            dtFeePaymentDetails.Columns.Add("FeeTypeID");
            dtFeePaymentDetails.Columns.Add("PayeeID");
            dtFeePaymentDetails.Columns.Add("Amount");
            dtFeePaymentDetails.Columns.Add("DiscPer");
            dtFeePaymentDetails.Columns.Add("DiscAmt");
            dtFeePaymentDetails.Columns.Add("NetApplicablePayment");
            dtFeePaymentDetails.Columns.Add("PaymentRecieved");
            dtFeePaymentDetails.Columns.Add("Year");
            dtFeePaymentDetails.Columns.Add("Month");
            dtFeePaymentDetails.Columns.Add("PaymentDate");
            dtFeePaymentDetails.Columns.Add("SBranchID");
            dtFeePaymentDetails.Columns.Add("DuesPaidCount");

            foreach (FeeDetailsModel e in PaymentDetails)
            {
                DataRow dr = dtFeePaymentDetails.NewRow();
                dr["CollectionID"] = e.CollectionID;
                dr["PaymentID"] = PaymentID;
                dr["FeeTypeID"] = e.FeeTypeID;
                dr["PayeeID"] = StudentID;
                dr["Amount"] = e.Amount;
                dr["DiscPer"] = e.Discount;
                dr["DiscAmt"] = e.DiscAmt;
                dr["NetApplicablePayment"] = e.NetApplicablePayment;
                dr["PaymentRecieved"] = e.PaymentRecieved;
                dr["Year"] = e.Year;
                dr["Month"] = e.Month;
                dr["PaymentDate"] = PaymentDate;
                dr["SBranchID"] = SBranchID;
                dr["DuesPaidCount"] = e.DuesPaidCount;

                dtFeePaymentDetails.Rows.Add(dr);
            }

            return dtFeePaymentDetails;
        }
    }
    public class ClassFeeStructureEditModel
    {
        public int ClassID { get; set; }
        public int GroupID { get; set; }
        public int SessionID { get; set; }
        public List<ClassFeeStructureModel> FeeStructure { get; set; }
        public int SBranchID { get; set; }
        public DateTime OperationDate { get; set; }
        public int UserID { get; set; }
        public List<ClassModel> Classes { get; set; }
        public List<GroupModel> Groups { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }

        public DataTable GetFeeStructureDataTable()
        {

            DataTable dtFeeStructureDetails = new DataTable();
            dtFeeStructureDetails.SetTypeName("ut_FeeStructureDetail");
            dtFeeStructureDetails.Columns.Add("ID");
            dtFeeStructureDetails.Columns.Add("ClassID");
            dtFeeStructureDetails.Columns.Add("GroupID");
            dtFeeStructureDetails.Columns.Add("FeeTypeID");
            dtFeeStructureDetails.Columns.Add("FeeAmount");
            dtFeeStructureDetails.Columns.Add("SBranchID");
            dtFeeStructureDetails.Columns.Add("UserID");
            dtFeeStructureDetails.Columns.Add("CreatedDate");
            dtFeeStructureDetails.Columns.Add("ModifiedDate");

            foreach (ClassFeeStructureModel e in FeeStructure)
            {
                DataRow dr = dtFeeStructureDetails.NewRow();
                dr["ID"] = e.ID;
                dr["ClassID"] = e.ClassID;
                dr["GroupID"] = e.GroupID;
                dr["FeeTypeID"] = e.FeeTypeID;
                dr["FeeAmount"] = e.FeeAmount;
                dr["SBranchID"] = e.SBranchID;
                dr["UserID"] = e.Status;
                dr["CreatedDate"] = e.CreatedDate;
                dr["ModifiedDate"] = e.ModifiedDate;

                dtFeeStructureDetails.Rows.Add(dr);
            }

            return dtFeeStructureDetails;
        }

    }
    public class ClassFeeStructureModel
    {
        private int iD;
        public int SessionID { get; set; }
        private int classID;
        private int groupID;
        public int Status { get; set; }
        private int feeTypeID;
        private decimal feeAmount;
        private int userID;
        private DateTime operationDate;
        public string FeeTypeName { get; set; }
        public int SBranchID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int FeeTypeApplicable { get; set; }
        public string FeeApplicableName { get; set; }
        public string Months { get; set; }

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

        public decimal FeeAmount
        {
            get
            {
                return feeAmount;
            }

            set
            {
                feeAmount = value;
            }
        }

        public int FeeTypeID
        {
            get
            {
                return feeTypeID;
            }

            set
            {
                feeTypeID = value;
            }
        }

        public int GroupID
        {
            get
            {
                return groupID;
            }

            set
            {
                groupID = value;
            }
        }

        public int ClassID
        {
            get
            {
                return classID;
            }

            set
            {
                classID = value;
            }
        }

        public int ID
        {
            get
            {
                return iD;
            }

            set
            {
                iD = value;
            }
        }
    }
    public class PaymentModel
    {
        private string[] Months = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        public string Mobile { get; set; }
        public int PaymentID { get; set; }
        public int PayeeID { get; set; }
        public string PayeeName { get; set; }
        public string ReferanceNumber { get; set; }
        public int PaymentType { get; set; }
        public int PaymentMode { get; set; }
        public string Remark { get; set; }
        public decimal PaymentAmount { get; set; }
        public int PaymentStatus { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string PayMonths { get; set; }
        public string SessionName { get; set; }
        public string MonthName
        {
            get
            {
                return Months[Month];
            }
        }
        public DateTime PaymentDate { get; set; }
        public int SBranchID { get; set; }
    }
    public class PaymentDetailsModel
    {
        public int CollectionID { get; set; }
        public int PaymentID { get; set; }
        public int FeeTypeID { get; set; }
        public string FeeTypeName { get; set; }
        public string PaymentNoRange { get; set; }
        public int PayeeID { get; set; }
<<<<<<< HEAD
=======

>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
        public decimal Amount { get; set; }
        public decimal DiscPer { get; set; }
        public decimal DiscAmt { get; set; }
        public decimal NetApplicablePayment { get; set; }
        public decimal PaymentRecieved { get; set; }
<<<<<<< HEAD
=======
        
              public decimal RefundAmount { get; set; }
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
        public string Year { get; set; }
        public int Month { get; set; }
        public DateTime PaymentDate { get; set; }
        public int SBranchID { get; set; }
    }

    public class QuotaModel
    {
        public int QuotaID { get; set; }
        public string QuotaName { get; set; }
        public string Description { get; set; }
        public int NumberOfStudents { get; set; }
        public int Students { get; set; }
        public int IsApproved { get; set; }
        public int SBranchID { get; set; }
        public int SequenceNo { get; set; }
        public DateTime OperationDate { get; set; }
        public int UserID { get; set; }
        public int OpType { get; set; }
        public decimal AvgDiscount { get; set; }
        public int SessionID { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<QuotaDiscountsModel> Discounts { get; set; }

        public DataTable GetQuotaDiscountDataTable()
        {

            DataTable dtFeeStructureDetails = new DataTable();
            dtFeeStructureDetails.SetTypeName("ut_QuotaDiscountDetail");
            dtFeeStructureDetails.Columns.Add("ID");
            dtFeeStructureDetails.Columns.Add("QuotaID");
            dtFeeStructureDetails.Columns.Add("FeeTypeID");
            dtFeeStructureDetails.Columns.Add("DiscPer");
            dtFeeStructureDetails.Columns.Add("SBranchID");
            dtFeeStructureDetails.Columns.Add("UserID");
            dtFeeStructureDetails.Columns.Add("CreatedDate");
            dtFeeStructureDetails.Columns.Add("ModifiedDate");

            foreach (QuotaDiscountsModel e in Discounts)
            {
                DataRow dr = dtFeeStructureDetails.NewRow();
                dr["ID"] = e.ID;
                dr["QuotaID"] = e.QuotaID;
                dr["FeeTypeID"] = e.FeeTypeID;
                dr["DiscPer"] = e.DiscPer;
                dr["SBranchID"] = e.SBranchID;
                dr["UserID"] = e.UserID;
                dr["CreatedDate"] = OperationDate;
                dr["ModifiedDate"] = OperationDate;

                dtFeeStructureDetails.Rows.Add(dr);
            }

            return dtFeeStructureDetails;
        }
    }
    public class QuotaDiscountsModel
    {
        public int ID { get; set; }
        public int QuotaID { get; set; }
        public int FeeTypeID { get; set; }
        public string FeeTypeName { get; set; }
        public int FeeTypeApplicable { get; set; }
        public decimal DiscPer { get; set; }
        public int SBranchID { get; set; }
        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }

    }
    public class DemandReciptModel
    {
        public int StudentID { get; set; }
        public int NotificationSMSTo { get; set; }
        public string FatherName { get; set; }
        public string Name { get; set; }
        public string SchoolUID { get; set; }
        public string StudentSID { get; set; }
        public string RollNo { get; set; }
        public string ClassName { get; set; }
        public decimal LateFee { get; set; }
        public decimal DiscAmt { get; set; }
        public decimal ApplicableFee { get; set; }
        public decimal PreviousDues { get; set; }
        public string MobileNo { get; set; }
        public string MonthName { get; set; }
        public decimal PaymentAmount { get; set; }

        public int ExistCount { get; set; }
<<<<<<< HEAD
=======

       // Add this if missing
        public decimal FeeAmount { get; set; }
        public decimal Discounts { get; set; }
        public decimal Paid { get; set; }
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
    }
    public class DemandReciptListModel
    {
        public SBranchModel Branch { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int SessionID { get; set; }
        public DateTime DemandMonth { get; set; }
        public DateTime DemandYear { get; set; }
        public List<DemandReciptModel> Recipts { get; set; }
        public DateTime SelectedDate { get; set; }
    }
  
    public class AdminFeeDiscountRequestMaster
    {
        public int Status { get; set; }
        public List<FeeDiscountRequestMaster> Requests { get; set; }
    }
    public class FeeDiscountRequestMaster
    {
        public int SessionID { get; set; }
        public int SBranchID { get; set; }
        public int DiscRequestID { get; set; }
        public int StudentID { get; set; }
        public string RollNo { get; set; }
        public string StudentSID { get; set; }
        public string ClassSection { get; set; }
        public string Remark { get; set; }
        public DateTime RequestDate { get; set; }
        public int FeeMonth { get; set; }
        public int FeeYear { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public int Status { get; set; }
        public string DirectorRemark { get; set; }
        public int OpType { get; set; }
        public string StudentName { get; set; }
        public string MonthName { get; set; }
        public List<FeeDiscountRequestDetails> Details { get; set; }

        public DataTable GetDiscountDetailsDataTable()
        {

            DataTable dtFeeDiscountDetails = new DataTable();
            dtFeeDiscountDetails.SetTypeName("ut_FeeDiscountRequestDetails");
            dtFeeDiscountDetails.Columns.Add("DiscRequestID");
            dtFeeDiscountDetails.Columns.Add("DiscDetailID");
            dtFeeDiscountDetails.Columns.Add("FeeTypeID");
            dtFeeDiscountDetails.Columns.Add("NetApplicablePayment");
            dtFeeDiscountDetails.Columns.Add("Amount");
            dtFeeDiscountDetails.Columns.Add("ApprovedAmount");
            dtFeeDiscountDetails.Columns.Add("OpType");

            foreach (FeeDiscountRequestDetails e in Details)
            {
                DataRow dr = dtFeeDiscountDetails.NewRow();
                dr["DiscRequestID"] = DiscRequestID;
                dr["DiscDetailID"] = e.DiscDetailID;
                dr["FeeTypeID"] = e.FeeTypeID;
                dr["NetApplicablePayment"] = e.NetApplicablePayment;
                dr["Amount"] = e.Amount;
                dr["ApprovedAmount"] = e.ApprovedAmount;
                dr["OpType"] = e.OpType;

                dtFeeDiscountDetails.Rows.Add(dr);
            }

            return dtFeeDiscountDetails;
        }
    }

    public class FeeDiscountRequestDetails
    {
        public int DiscRequestID { get; set; }
        public int DiscDetailID { get; set; }
        public int FeeTypeID { get; set; }
        public decimal Amount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public int OpType { get; set; }
        public string FeeTypeName { get; set; }
        public decimal NetApplicablePayment { get; set; }
    }
}