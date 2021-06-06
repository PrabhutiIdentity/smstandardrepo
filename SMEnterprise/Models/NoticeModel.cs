using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class NoticeModel
    {
        public int SendSMS { get; set; }
        public string UUID { get; set; }
        public int NoticeID { get; set; }
        public int ShiftID { get; set; }
        public string NoticeTitle { get; set; }
        public string NoticeDescription { get; set; }
        public DateTime NoticeStartDate { get; set; }
        public DateTime NoticeEndDate { get; set; }
        public int ApplicableFor { get; set; }
        public int NoticeLevel { get; set; }
        public string Classes { get; set; }
        public string ClassNames { get; set; }
        public string PostedBy { get; set; }
        public string ClassesIncludedIDs { get; set; }
        public int Status { get; set; }
        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }
        public int OpType { get; set; }
        public int SBranchID { get; set; }
        public int IsSMSSent { get; set; }
        public string SMSSentTo { get; set; }
    }
    public class NoticePageModel
    {
        public List<NoticeModel> Notices { get; set; }
        public List<NameIDModel> SelectionList { get; set; }
        public List<NameIDModel> BusList { get; set; }

    }
}