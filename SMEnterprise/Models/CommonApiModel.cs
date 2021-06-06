using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace SMEnterprise.Models
{
    public class CommonApiWraperModel
    {
        public int IsStaffMeeting { get; set; }
        public DateTime RDate { get; set; }
        public int IsOnlineClass { get; set; }
        public int Code { get; set; }
        [AllowHtml]
        public string Message { get; set; }
        public List<object> List { get; set; }
        public List<object> List2 { get; set; }
        public object Data { get; set; }
        public string base_url { get; set; }
      
    }
    public class AppAttandanceModel
    {
        public string EmployeeID { get; set; }
        public string serial_no { get; set; }
        public DateTime logdatetime { get; set; }
        public int AttandanceType { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string Location { get; set; }
        public string UUID { get; set; }
        public double Status { get; set; }
        public string Comment { get; set; }
        public string Time
        {
            get
            {
                return logdatetime.Hour.ToString().PadLeft(2, '0') + ":" + logdatetime.Minute.ToString().PadLeft(2, '0');
            }
        }
        public int InOut { get; set; }
    }
    public class EmployeeAttandanceReviewPageModel
    {
        public int EmployeeType { get; set; }
        public int SBranchID { get; set; }
        public List<NameIDModel> EmployeeTypes { get; set; }
        public List<EmployeeAttandanceReviewModel> Attandances { get; set; }
        public DateTime AttandanceDate { get; set; }
    }
    public class EmployeeAttandanceReviewModel
    {
        public int Gender { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Photo { get; set; }
        public string EmployeeSID { get; set; }
        public string serial_no { get; set; }
        public DateTime logdatetime { get; set; }
        public int AttandanceType { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string Location { get; set; }
        public string UUID { get; set; }
        public double Status { get; set; }
        public string Comment { get; set; }
        public string Details { get; set; }

    }
    public class ApiAuthenticationModel
    {
        public int SchoolID { get; set; }
        public int UserID { get; set; }
        public int SBranchID { get; set; }
        public int UserType { get; set; }
        public string UUID { get; set; }
        public string UserName { get; set; }
        public string DynamicSalt { get; set; }
        public DateTime LastLoginDate { get; set; }
        public string deviceType { get; set; }
        public string deviceToken { get; set; }
        public int OpType { get; set; }
        public string Name { get; set; }
        public string MobileNumber { get; set; }
    }
    public class ForgetPasswordApiModel
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public string MobileNumber { get; set; }
        public DateTime DOB { get; set; }
        public string Name { get; set; }
        public int UserType { get; set; }
        public string Password { get; set; }
    }
}