using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class WebsiteModel
    {
    }
    public class BannerImageModel
    {
        public int ABID { get; set; }
        public int SBranchID { get; set; }
        public string Title { get; set; }
        public string BImage { get; set; }
        public int Status { get; set; }
        public DateTime BannerDate { get; set; }
        public int OpType { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
    }
    public class AppHomeModel
    {
        public List<BannerImageModel> Banners { get; set; }
        public string AboutString { get; set; }
    }
}