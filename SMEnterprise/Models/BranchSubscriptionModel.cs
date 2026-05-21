using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class BranchSubscriptionModel
    {
        public int SBranchID { get; set; }
<<<<<<< HEAD
        public string BranchName { get; set; }
        public string BranchAddress { get; set; }
=======
>>>>>>> master
        public bool IsDue { get; set; }
        public decimal DueAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public string PlanName { get; set; }
        public DateTime? LastPaidDate { get; set; }
        public string LastPaymentRef { get; set; }
<<<<<<< HEAD
        public int GraceDays { get; set; }
        public bool AllowPartialPayment { get; set; }
        public int MaxPartialPayments { get; set; }
        public int PartialPaymentCount { get; set; }
        public DateTime? NextDueDate { get; set; }
        public decimal NextDueAmount { get; set; }
        public int PartialCycleDays { get; set; }
    }
    public class SystemPaymentGatewayModel
    {
        public int GatewayID { get; set; }
        public string GatewayName { get; set; }
        public string RazorpayKeyId { get; set; }
        public string RazorpaySecret { get; set; }
        public bool UseForSubscription { get; set; }
        public bool IsActive { get; set; }
=======
    }
    public class BranchGatewayModel
    {
        public int SBranchID { get; set; }
        public string RazorpayKeyId { get; set; }
        public string RazorpaySecret { get; set; }
        public bool UseForSubscription { get; set; }
>>>>>>> master
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
    public class SubscriptionReceiptModel
    {
        public string OrderID { get; set; }
        public string PaymentID { get; set; }
        public int SBranchID { get; set; }
        public decimal AmountPaid { get; set; }
        public string PlanName { get; set; }
        public DateTime PaidOn { get; set; }
    }
<<<<<<< HEAD
    public class BranchSubscriptionPaymentModel
    {
        public int PaymentID { get; set; }
        public int SBranchID { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentRef { get; set; }
        public DateTime PaymentDate { get; set; }
        public int? CreatedBy { get; set; }
    }
    public class BranchSubscriptionAccountModel
    {
        public BranchSubscriptionModel Subscription { get; set; }
        public List<BranchSubscriptionPaymentModel> Payments { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal CurrentDueAmount { get; set; }
        public bool CanPayNow { get; set; }
    }
}
=======
}
>>>>>>> master
