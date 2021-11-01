using SMEnterprise.Models;
using SMEnterprise.Repository;
using System;
using System.Web;
using System.Web.Http;

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
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (HttpContext.Current.Session["SMSConfiguration"] == null)
            {
                HttpContext.Current.Session["SMSConfiguration"] = (new AdminData()).GetDefaultSMSConfigurationDetails(SBranchID);
            }
            SMSConfigirationModel SMSConfigiration = (SMSConfigirationModel)HttpContext.Current.Session["SMSConfiguration"];
            DeligateTasks objDT = new DeligateTasks();
            objDT.StartSending(objModel, SMSConfigiration);
            //return Redirect("/Account/SendSMS/"+ objModel.SMSSendingID);
            return true;
        }
    }
}
