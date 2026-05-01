using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class OnlinePaymentModel
    {
        public string Name { get; set; }
        public string EmailID { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public int Amount { get; set; }
        public string OrderID { get; set; }
    }
    public class OrderModel
    {
        public string Name { get; set; }
        public string EmailID { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public int Amount { get; set; }
        public string PGOrderID { get; set; }
        public string PGPaymentID { get; set; }
        public string OrderID { get; set; }
        public int Status { get; set; }
        public DateTime Date { get; set; }
        public string razorpayKey { get; set; }
        public string razorpaySecret { get; set; }
        
        public string currency { get; set; }
        public string Description { get; set; }
        public int StudentID { get; set; }
        public DateTime CurDate { get; set; }
        public DateTime QDate { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Remark { get; set; }
        public decimal PaymentAmount { get; set; }
        public string ReferanceNumber { get; set; }
        public int PaymentMode { get; set; }
        public string CollectedBy { get; set; }
        public int FeeMonth { get; set; }
        public int FeeYear { get; set; }
        public decimal ApplicableFee { get; set; }
        public int SessionID { get; set; }
        public int SBranchID { get; set; }
        public string Hash { get; set; }
        public List<StudentOnlineFeeDetailModel> FeeDetail { get; set; }

        // ── NEW: multi-month support ──────────────────────────────
        /// <summary>
        /// JSON string of selected months, e.g.
        /// [{"Month":4,"Year":2025,"Amount":81.00},{"Month":5,"Year":2025,"Amount":80.00}]
        /// </summary>
        public string SelectedMonthsJson { get; set; }

        /// <summary>True when the order covers more than one month.</summary>
        public bool IsMultiMonth { get; set; }
        // for school subscription plan
        public bool IsSubscriptionOrder { get; set; } = false;


    }
    public class PaymentDetailModel
    {
        public string PaymentNo { get; set; }
        public string PaymentID { get; set; }
        public string TransactionID { get; set; }
        public DateTime DateAdded { get; set; }
        public int PaymentMode { get; set; }
        public decimal Amount { get; set; }
    }
    public class StudentOnlineFeeDetailModel
    {
        public int ID { get; set; }
        public int FeeTypeID { get; set; }
        public string FeeTypeName { get; set; }
        public int FeeMonth { get; set; }
        public int FeeYear { get; set; }

    }
    public class StudentDetailModel
    {
        public StudentModel StudentDetail { get; set; }
        public OrderModel OrderDetail { get; set; }
    }
}