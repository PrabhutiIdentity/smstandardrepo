using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
   
    public class ParentApiPaymentDetailParamModel
    {
        public string UUID { get; set; }
        public int Status { get; set; }
        public int ID { get; set; }
        public int StudentID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
    public class ParentApiPaymentParamModel
    {
        public string UUID { get; set; }
        public int Status { get; set; }
        public int ID { get; set; }
    }
    public class ParentApiParamModel
    {
        public int OCID { get; set; }
        public int ID { get; set; }
        public int PageID { get; set; }
        public int ID2 { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string UUID { get; set; }
        public int DayID { get; set; }
        public int Status { get; set; }
        public int SBranchID { get; set; }
        public string Password { get; set; }
        public string EmailID { get; set; }
        public string MobileNumber { get; set; }
        public int UpdateDetailsFor { get; set; }
        public DateTime RDate { get; set; }
    }
    public class TransportEmployeeModel
    {
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Photo { get; set; }
    }
}