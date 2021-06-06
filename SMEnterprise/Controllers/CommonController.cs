using SMEnterprise.Models;
using SMEnterprise.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SMEnterprise.Controllers
{
    public class CommonController : Controller
    {
        [HttpPost]
        public ActionResult Upload(UploadViewModel model)
        {
            var path = "";
            if (model.Type == "Assignment")
            {
                path = Path.Combine(Server.MapPath(CommonUsage.AssignmentAttachmentBasePath),
                  model.ID + "_" + model.File.FileName.Replace(" ", "-"));
            }


            model.File.SaveAs(path);
            //model.File.
            return Json(1, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteFile(DeleteFileModel objModel = null)
        {
            var path = "";
            try { 
            if (objModel.Type == "Assignment")
            {
                path = Path.Combine(Server.MapPath(CommonUsage.AssignmentAttachmentBasePath),
                  objModel.ID + "_" + objModel.FileName.Replace(" ", "-"));
            }
            System.IO.File.Delete(path);
            }
            catch(Exception ex)
            {

            }
            //model.File.
            return Json(1, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdatePassword(string ID = null)
        {
            string Password = ID;
            int UserID = PermissionManager.GetLoggedInUser().UserID;
            int UserType = PermissionManager.GetLoggedInUser().RoleID;
            CommonData objCommonData = new CommonData();
            objCommonData.UpdatePassword(UserID, UserType, Password);
            PermissionManager.logout();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
        
    }
}