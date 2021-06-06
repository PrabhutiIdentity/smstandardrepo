using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMEnterprise.Models
{
    public class MessageModel
    {
        public int MessageID { get; set; }
        public string MessageTitle { get; set; }
        [AllowHtml]
        public string MessageBody { get; set; }
        public int SenderID { get; set; }
        public int SenderType { get; set; }
        public int RecieverID { get; set; }
        public int RecieverType { get; set; }
        public DateTime SendDate { get; set; }
        public int IsRead { get; set; }
        public DateTime ReadDate { get; set; }
        public int ParentMessageID { get; set; }
        public int MainMailThreadID { get; set; }
        public int IsShow { get; set; }
        public string Attachments { get; set; }
        public string Name { get; set; }
        public string SenderName { get; set; }
        public int CurrUserType { get; set; }
        public int CurrUserID { get; set; }
        public HttpPostedFileBase Attachment { get; set; }
        public string SenderUrl { get; set; }
    }
    public class MailBoxModel
    {
        public List<MessageModel> Mails { get; set; }
        public int UnreadCount { get; set; }
        public MessageModel Mail { get; set; }
        public List<NameIDModel> RecieverList { get; set; }
    }
    public class AbsentStudentModel
    {
        public string StudentSID { get; set; }
        public int StudentID { get; set; }
        public int NotificationSMSTo { get; set; }
        public string MobileNo { get; set; }
        public string Name { get; set; }
        public int SBranchID { get; set; }
    }
    public class AbsentStudentListModel
    {
        public List<AbsentStudentModel> AbsentStudents { get; set; }
    }
    public class StudentAttandaceNoticeModel
    {
        public string employeeid { get; set; }
        public DateTime logdatetime { get; set; }

    }
}