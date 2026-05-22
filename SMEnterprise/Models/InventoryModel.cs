using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class InventoryModel
    {
    }
    public class GSTStateModel
    {
        public int StateID { get; set; }
        public string StateName { get; set; }
        public string StateCode { get; set; }
    }
    public class VendorPageModel
    {
        public List<GSTStateModel> GSTStates { get; set; }
        public List<VendorModel> Vendors { get; set; }
        public VendorModel Vendor { get; set; }
    }
    public class VendorModel
    {
        public int VendorID { get; set; }
        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public int StateID { get; set; }
        public string StateName { get; set; }
        public string PinCode { get; set; }
        public string CompanyContact { get; set; }
        public string EmailID { get; set; }
        public string BranchName { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public int AccountType { get; set; }
        public string IFSCCode { get; set; }
        public string OtherDetails { get; set; }
        public string GSTIN { get; set; }
        public string PANNumber { get; set; }
        public string TINNumber { get; set; }
        public string CINNumber { get; set; }
        public string RegistrationNumber { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
    }

    public class DayBookReportModel
    {
        public string UUID { get; set; }
        public SBranchModel Branch { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int ReportType { get; set; }
        public int PaymentMode { get; set; }
        public int SBranchID { get; set; }
        public List<FeePaymentModel> Report { get; set; }
        public List<NameIDModel> FeeType { get; set; }
        public List<PaymentModeModel> PaymentModes { get; set; }
        public List<PaymentDetailsModel> FeeReportType { get; set; }
        public List<ExpenseDetailsModel> ExpenseReportType { get; set; }
        public int QuarterID { get; set; }
    }

    public class ExpenseDetailsModel1
    {
        public int CollectionID { get; set; }
        public int ExpenceID { get; set; }
        public int ExpenceTypeID { get; set; }
        public string ExpenceTypeName { get; set; }
        public string ExpenseNoRange { get; set; }
        public int EmployeeID { get; set; }
        public decimal ExpenceAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal SalryAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal NetPayble { get; set; }
        public decimal GrossEarning { get; set; }
        public decimal TotalDeduction { get; set; }
        public string Year { get; set; }
        public int Month { get; set; }
        public DateTime PaymentDate { get; set; }
        public int SBranchID { get; set; }
    }
    public class DayBookPageModel
    {
        public int PaymentMode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int SBranchID { get; set; }
        public int DayBookID { get; set; }
        public DayBookModel DayBook { get; set; }
        public List<DayBookModel> DayBooks { get; set; }
        public List<DayBookHeadModel> DayBookHead { get; set; }
    }

    public class DayBookModel
    {
           
        public int UserID { get; set; }
        public int ID { get; set; }
        public int DBookID { get; set; }
        public int SBranchID { get; set; }
        public string Title { get; set; }
        public string IncomeExpenceBy { get; set; }
        public int FY { get; set; }
        public int Type { get; set; }
        public int DayBookID { get; set; }
        public string HeadName { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RecievedBy { get; set; }
        public int PaymentMode { get; set; }
        public string ReferanceNo { get; set; }
       
        public int OpType { get; set; }
    }
    public class DayBookHeadModel
    {
        public int ID { get; set; }
        public int DayBookID { get; set; }        
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }       
        public int OpType { get; set; }
        public int SBranchID { get; set; }
        public int HeadCount { get; set; }
    }
    public class ProductCategoryModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string HSNCode { get; set; }
        public decimal SGST { get; set; }
        public decimal CGST { get; set; }
        public decimal IGST { get; set; }
        public int OpType { get; set; }
        public int SBranchID { get; set; }
        public int ProductCount { get; set; }
    }
    public class ProductsPageModel
    {
        public int SBranchID { get; set; }
        public int CategoryID { get; set; }
        public ProductModel Product { get; set; }
        public List<ProductModel> Products { get; set; }
        public List<ProductCategoryModel> Categories { get; set; }
    }
    public class ProductModel
    {
        public int ProductID { get; set; }
        public int ProductCategoryID { get; set; }
        public string ProductCategoryName { get; set; }
        public decimal Quantity { get; set; }
        public decimal Available { get; set; }
        public decimal MinQty { get; set; }
        public int Status { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public decimal MRP { get; set; }
        public decimal Price { get; set; }
        public decimal SGST { get; set; }
        public decimal CGST { get; set; }
        public decimal IGST { get; set; }
        public HttpPostedFileBase ProductPhoto { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
        public int ProductTranCount { get; set; }
    }
    public class StockManagementModel
    {
        public int TrType { get; set; }
        public int SBranchID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<StockTransaferMasterModel> Transactions { get; set; }
        public SBranchModel Branch { get; set; }
    }





    public class StockTransactionPaymentModel
    {
        public int StockPaymentID { get; set; }
        public int STID { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public int PaymentMode { get; set; }
        public string PaymentReferanceNo { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public int PaymentStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public int SBranchID { get; set; }
        public int CreatedBy { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string CancelRemark { get; set; }
        public int CancelledBy { get; set; }
        public string PaymentModeText
        {
            get
            {
                switch (PaymentMode)
                {
                    case 0:
                        return "Cash";
                    case 1:
                        return "Cheque";
                    case 2:
                        return "NEFT";
                    case 4:
                        return "Google Pay";
                    case 5:
                        return "PayTm";
                    case 6:
                        return "PhonePe";
                    case 3:
                        return "Other";
                    default:
                        return "";
                }
            }
        }
        public string PaymentStatusText
        {
            get
            {
                if (IsCancelled || PaymentStatus == 3)
                {
                    return "Cancelled";
                }
                switch (PaymentStatus)
                {
                    case 2:
                        return "Paid";
                    case 1:
                        return "Partial";
                    case 3:
                        return "Cancelled";
                    default:
                        return PaymentAmount > 0 ? "Partial" : "Unpaid";
                }
            }
        }
    }
    public class StockSaleReportItemModel
    {
        public int STID { get; set; }
        public string InvoiceNumber { get; set; }
        public string LastReceiptNumber { get; set; }
        public DateTime TrDate { get; set; }
        public string CustomerName { get; set; }
        public string ClassSection { get; set; }
        public string ProductNames { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public int PaymentStatus { get; set; }
        public string PaymentStatusText { get; set; }
        public int LastPaymentID { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public int LastPaymentStatus { get; set; }
        public DateTime? LastReceiptCancelledDate { get; set; }
    }
    public class StockSaleReportPageModel
    {
        public int SBranchID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ProductID { get; set; }
        public List<NameIDModel> Products { get; set; }
        public List<StockSaleReportItemModel> Report { get; set; }
        public SBranchModel Branch { get; set; }
    }




    public class StockTransaferMasterModel
    {
        public string InvoiceNumber { get; set; }
        public int PaymentMode { get; set; }
        public string PaymentReferanceNo { get; set; }





        public DateTime? PaymentDate { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public int PaymentStatus { get; set; }
        public string CancelRemark { get; set; }




        public string ReferanceNo { get; set; }
        public int STID { get; set; }
        public int VendorID { get; set; }
        public DateTime TrDate { get; set; }
        public int TrType { get; set; }
        public string RefName { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int SessionID { get; set; }
        public int RefID { get; set; }
        public int RefType { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }

        public int LastPaymentID { get; set; }
        public int SelectedPaymentID { get; set; }
        public int Status { get; set; }
        public int EmployeeTypeID { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }





        public bool IsPaymentLocked
        {
            get
            {
                return Status != 0 && PaidAmount > 0;
            }
        }
        public string PaymentModeText
        {
            get
            {
                switch (PaymentMode)
                {
                    case 0:
                        return "Cash";
                    case 1:
                        return "Cheque";
                    case 2:
                        return "NEFT";
                    case 4:
                        return "Google Pay";
                    case 5:
                        return "PayTm";
                    case 6:
                        return "PhonePe";
                    case 3:
                        return "Other";
                    default:
                        return "";
                }
            }
        }
        public string PaymentStatusText
        {
            get
            {
                if (Status == 0)
                {
                    return "Cancelled";
                }
                switch (PaymentStatus)
                {
                    case 2:
                        return "Paid";
                    case 1:
                        return "Partial";
                    default:
                        return PaidAmount > 0 ? "Partial" : "Unpaid";
                }
            }
        }




        public List<NameIDModel> EmployeeTypes { get; set; }
        public List<VendorModel> Vendors { get; set; }
        public List<NameIDModel> Employees { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> Students { get; set; }
        public List<ProductModel> Products { get; set; }
        public List<StockTransaferDetailModel> Details { get; set; }

        public List<StockTransactionPaymentModel> PaymentHistory { get; set; }

        public DataTable GetDetailsDataTable()
        {

            DataTable dtExpenceDetails = new DataTable();
            dtExpenceDetails.SetTypeName("ut_StockTransactionDetails");
            dtExpenceDetails.Columns.Add("STDID");
            dtExpenceDetails.Columns.Add("STID");
            dtExpenceDetails.Columns.Add("STType");
            dtExpenceDetails.Columns.Add("ProductID");
            dtExpenceDetails.Columns.Add("Quantity");
            dtExpenceDetails.Columns.Add("MRP");
            dtExpenceDetails.Columns.Add("Cost");
            dtExpenceDetails.Columns.Add("SBranchID");
            dtExpenceDetails.Columns.Add("SGST");
            dtExpenceDetails.Columns.Add("CGST");
            dtExpenceDetails.Columns.Add("IGST");

            foreach (StockTransaferDetailModel e in Details)
            {
                if (e.OpType != -1)
                {
                    DataRow dr = dtExpenceDetails.NewRow();
                    dr["STDID"] = e.STDID;
                    dr["STID"] = e.STID;
                    dr["STType"] = e.STType;
                    dr["ProductID"] = e.ProductID;
                    dr["Quantity"] = e.Quantity;
                    dr["Cost"] = e.Cost;
                    dr["MRP"] = e.MRP;
                    dr["SGST"] = e.SGST;
                    dr["CGST"] = e.CGST;
                    dr["IGST"] = e.IGST;
                    dr["SBranchID"] = SBranchID;
                    dtExpenceDetails.Rows.Add(dr);
                }
            }
            return dtExpenceDetails;
        }
    }
    public class StockTransaferDetailModel
    {
        public int STDID { get; set; }
        public int STID { get; set; }
        public int STType { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal Cost { get; set; }
        public decimal MRP { get; set; }
        public decimal SGST { get; set; }
        public decimal CGST { get; set; }
        public decimal IGST { get; set; }
        public decimal Available { get; set; }
        public decimal Quantity { get; set; }
        public int OpType { get; set; }
    }
    public class SaleReceiptCustomerModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string GSTIN { get; set; }
        public string StateName { get; set; }
        public string ContactNo { get; set; }
        public int StateID { get; set; }
        public string ExtraData { get; set; }

        public string ClassName { get; set; }
        public string FatherName { get; set; }

    }
    public class PrintSaleReceiptModel
    {
        public SBranchModel SBranchDetails { get; set; }
        public List<StockTransaferDetailModel> Products { get; set; }
        public StockTransaferMasterModel Transfer { get; set; }
        public SaleReceiptCustomerModel Customer { get; set; }


        public List<StockTransactionPaymentModel> PaymentHistory { get; set; }
        public StockTransactionPaymentModel SelectedPayment { get; set; }
    }
}




