using SMEnterprise.Filters;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using System;
using System.Web;
using System.Web.Http;
using System.IO;

namespace SMEnterprise.Controllers
{
    public class SMSApiController : ApiController
    {
        [HttpPost]
 
        public bool SendSMSList(SMSSendTaskModel objModel)
        {
            if (String.IsNullOrEmpty(objModel.StartTime))
            {
                objModel.StartTime = "00:00:00";
            }
            if (String.IsNullOrEmpty(objModel.EndTime))
            {
                objModel.EndTime = "00:00:00";
            }
            AccountData objAccountData = new AccountData();
            objModel.SMSSendDate = CommonUsage.GetCurrentDate();
            objModel.SMSSendingID = objAccountData.InsertSMSSending(objModel);
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
           
            if (HttpContext.Current.Session == null || HttpContext.Current.Session["SMSConfiguration"] == null)
            {
                HttpContext.Current.Session["SMSConfiguration"] = (new AdminData()).GetDefaultSMSConfigurationDetails(objModel.SBranchID);
            }
            SMSConfigirationModel SMSConfigiration = (SMSConfigirationModel)HttpContext.Current.Session["SMSConfiguration"];

            //SMSConfigirationModel SMSConfigiration = (new AdminData()).GetDefaultSMSConfigurationDetails(SBranchID);
            DeligateTasks objDT = new DeligateTasks();
            objDT.StartSending(objModel, SMSConfigiration);
            //return Redirect("/Account/SendSMS/"+ objModel.SMSSendingID);
            return true;
        }
    }
}
