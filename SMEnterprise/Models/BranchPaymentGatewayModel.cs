using System;

namespace SMEnterprise.Models
{
    public class BranchPaymentGatewayModel
    {
        public int SBranchID { get; set; }
        public string KeyId { get; set; }
        public string Secret { get; set; }
        public string PaymentGatewayName { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string BranchName { get; set; }
        public string BranchSchoolName { get; set; }
        public string EmailID { get; set; }
        public string ContactNo { get; set; }
        public string Address { get; set; }
        public string Logo { get; set; }
        public string ImageUrl { get; set; }
    }
}
