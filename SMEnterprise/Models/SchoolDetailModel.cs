using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class SchoolDetailModel
    {
        public int SchoolID { get; set; }
        public string PrincipalName { get; set; }
        public string SchoolName { get; set; }
        public string Address { get; set; }
        public string PhoneNo { get; set; }
        public string EmailID { get; set; }
    }
}