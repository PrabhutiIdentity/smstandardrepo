using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMEnterprise.Models;
using SMEnterprise.Repository;
namespace SMEnterprise.Controllers
{
    public class AppController : Controller
    {
        // GET: App
        public ActionResult Home(string ID=null)
        {
            return View();
        }
        public ActionResult About(string ID = null)
        {
            return View();
        }
        public ActionResult Gallery(string ID = null)
        {
            int SBranchID = CommonUsage.ConvertToInt(ID);
            AdminData objAdminData = new AdminData();
            List<GalleryModel> Galleries = objAdminData.GetAppGalleryList(SBranchID);
            return View(Galleries);
        }
        public ActionResult GalleryImages(string ID = null)
        {
            int GalletyID = CommonUsage.ConvertToInt(ID);
            AdminData objAdminData = new AdminData();
            GalleryModel Gallery = objAdminData.GetAppGalleryDetails(GalletyID);
            return View(Gallery);
        }
        public ActionResult ContactUs(string ID = null)
        {
            int SBranchID = CommonUsage.ConvertToInt(ID);
            AdminData objAdminData = new AdminData();
            SBranchModel Branch = objAdminData.GetAppBranchContact(SBranchID);
            return View(Branch);
        }
    }
}