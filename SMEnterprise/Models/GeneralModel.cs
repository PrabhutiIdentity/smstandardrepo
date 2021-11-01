using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace SMEnterprise.Models
{
    public class AppMenuModel
    {
        public int MenuID { get; set; }
        public string MenuIcon { get; set; }
        public string MenuName { get; set; }
        public string MenuTitle { get; set; }
        public string MenuURL { get; set; }
        public int MenuIndex { get; set; }
        public int IsDefault { get; set; }
        public int SBranchID { get; set; }
    }
    public class ParentAppSMSPageModel
    {
        public string Password { get; set; }
        public string EncryptedPassword { get; set; }
        public string SMSTemplate { get; set; }
        public int SessionID { get; set; }
        public int SBranchID { get; set; }
        public int ClassID { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<ParentModel> Parents { get; set; }
    }
    public class YoutubeAdminEditPageData
    {
        public int VideoID { get; set; }
        public int SBranchID { get; set; }
        public int TeacherID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int SessionID { get; set; }
        public int OpType { get; set; }
        public int SubjectID { get; set; }
        public string TeacherName { get; set; }
        public YouTubeVideoModel Video { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> Subjects { get; set; }
        public List<NameIDModel> Students { get; set; }
        public List<YouTubeVideoModel> Videos { get; set; }
        public DataTable GetStudentsDataTable()
        {
            DataTable dtSubjectOpted = new DataTable();
            dtSubjectOpted.SetTypeName("ut_Name_ID_Utility");
            dtSubjectOpted.Columns.Add("ID");
            dtSubjectOpted.Columns.Add("Name");
            dtSubjectOpted.Columns.Add("Extra1");
            dtSubjectOpted.Columns.Add("Extra2");
            dtSubjectOpted.Columns.Add("Extra3");
            if (Students == null)
            {
                Students = new List<NameIDModel>();
            }
            foreach (NameIDModel e in Students.Where(c => c.Extra2 == "1"))
            {
                DataRow dr = dtSubjectOpted.NewRow();
                dr["ID"] = e.ID;
                dr["Name"] = e.Name;
                dr["Extra1"] = e.Extra1;
                dr["Extra2"] = e.Extra2;
                dr["Extra3"] = e.Extra3;

                dtSubjectOpted.Rows.Add(dr);
            }

            return dtSubjectOpted;
        }
    }
    public class YouTubeAdminPageModel
    {
        public int SBranchID { get; set; }
        public int TeacherID { get; set; }
        public DateTime UploadDate { get; set; }
        public List<YouTubeVideoModel> Videos { get; set; }
        public List<NameIDModel> Teachers { get; set; }
    }
    public class YouTubeVideoModel
    {
        public int SessionID { get; set; }
        public int IsSpecificStudent { get; set; }
        public string TeacherName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string SubjectName { get; set; }
        public int StudentID { get; set; }
        public string UUID { get; set; }
        public int VideoID { get; set; }
        public int TeacherID { get; set; }
        public int SubjectID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public string YouTubeID { get; set; }
        public int SequenceNo { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime VideoDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int SBranchID { get; set; }

        public string YVideoID
        {
            get
            {
                string vid = "";
                if (YouTubeID.Contains("watch"))
                {
                    if (YouTubeID != null && YouTubeID != "")
                    {
                        int eqIndex = YouTubeID.IndexOf('=');
                        vid = YouTubeID.Substring(eqIndex + 1);

                    }
                }
                else
                {
                    int eqIndex = YouTubeID.LastIndexOf(".be/");
                    int endIndex = YouTubeID.IndexOf('?');
                    if (endIndex == -1)
                    {
                        endIndex = YouTubeID.Length;
                    }
                    vid = YouTubeID.Substring(eqIndex + 4, endIndex - eqIndex - 4);
                }
                return vid;
            }
        }
    }
    public class PaymentModeModel
    {
        public int PaymentModeID { get; set; }
        public string PaymentModeName { get; set; }
        public decimal Amount { get; set; }
    }
    public class ImportantContactModel
    {
        public int ICID { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
    }
    public class NotificationDataWraperModel
    {
        public string message { get; set; }
        public int type { get; set; }
        public int studentid { get; set; }
        public int subjectid { get; set; }
        public int primaryid { get; set; }
    }
    //public class OnlineClassNotificationModel
    //{
    //    public int type { get; set; }
    //    public string message { get; set; }
    //    public string image
    //    {
    //        get
    //        {
    //            return "https://node1.pschoolonline.com/images/pSchoolBanner.jpg";
    //        }
    //    }
    //    public int typeid { get; set; }
    //    public string Data { get; set; }
    //    public string base_url { get; set; }

    //}
    public class NotificationModel
    {
        public NotificationDataWraperModel RouteData { get; set; }
        public int RecieverID { get; set; }
        public bool other_key
        {
            get
            {
                return true;
            }
        }
        public string Type { get; set; }
        public int RecieverType { get; set; }
        public int NotificationType { get; set; }
        public int NotificationID { get; set; }
        public string Message { get; set; }
        public string NotificationText { get; set; }
        public DateTime NotificationDateTime { get; set; }
        public int SBranchID { get; set; }
        public List<NotificationRecieverModel> Recievers { get; set; }
        public DataTable GetRecieverList()
        {

            DataTable dtRecievers = new DataTable();
            dtRecievers.SetTypeName("ut_NotificationRecievers");
            dtRecievers.Columns.Add("NotificationRecieverID");
            dtRecievers.Columns.Add("NotificationID");
            dtRecievers.Columns.Add("RecieverID");
            dtRecievers.Columns.Add("RecieverType");
            dtRecievers.Columns.Add("NotificationText");
            dtRecievers.Columns.Add("DeviceToken");
            dtRecievers.Columns.Add("MasterID");
            dtRecievers.Columns.Add("OpType");

            foreach (NotificationRecieverModel e in Recievers)
            {
                DataRow dr = dtRecievers.NewRow();
                dr["NotificationRecieverID"] = e.NotificationRecieverID;
                dr["NotificationID"] = NotificationID;
                dr["RecieverID"] = e.RecieverID;
                dr["NotificationText"] = NotificationText;
                dr["DeviceToken"] = e.DeviceToken;
                dtRecievers.Rows.Add(dr);
            }

            return dtRecievers;
        }
    }
    public class NotificationRecieverModel
    {
        public int NotificationRecieverID { get; set; }
        public int NotificationID { get; set; }
        public int RecieverID { get; set; }
        public int RecieverType { get; set; }
        public string NotificationText { get; set; }
        public string DeviceToken { get; set; }
        public string priority
        {
            get
            {
                return "high";
            }
        }
    }
    public class NotificationSendModel
    {
        public NotificationModel data { get; set; }
        public string[] registration_ids { get; set; }
    }
    public class NewsModel
    {
        public int NewsID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ActiveDate { get; set; }
        public string Attachment { get; set; }
        public HttpPostedFileBase AttachmentFile { get; set; }
        public int Status { get; set; }
        public int OpType { get; set; }
    }
    public class GalleryModel
    {
        public int GalleryID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public int SBranchID { get; set; }
        public int Status { get; set; }
        public int OpType { get; set; }
        public int ImagesCount { get; set; }
        public string FeatureImage { get; set; }
        public List<GalleryImageModel> Images { get; set; }

        public DataTable GetImages()
        {

            DataTable dtImages = new DataTable();
            dtImages.SetTypeName("ut_GalleryImages");
            dtImages.Columns.Add("GalleryImageID");
            dtImages.Columns.Add("GalleryID");
            dtImages.Columns.Add("Title");
            dtImages.Columns.Add("Description");
            dtImages.Columns.Add("Status");
            dtImages.Columns.Add("ImagePath");
            dtImages.Columns.Add("ImageDate");
            dtImages.Columns.Add("OpType");

            foreach (GalleryImageModel e in Images)
            {
                DataRow dr = dtImages.NewRow();
                dr["GalleryImageID"] = e.GalleryImageID;
                dr["GalleryID"] = GalleryID;
                dr["Title"] = e.Title;
                dr["Description"] = e.Description;
                dr["Status"] = e.Status;
                dr["ImagePath"] = e.ImagePath;
                dr["ImageDate"] = e.ImageDate;
                dr["OpType"] = e.OpType;

                dtImages.Rows.Add(dr);
            }

            return dtImages;
        }
    }
    public class GalleryImageModel
    {
        public int GalleryImageID { get; set; }
        public int GalleryID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ImageDate { get; set; }
        public string OldImageName { get; set; }
        public int Status { get; set; }
        public int OpType { get; set; }
        public string ImagePath { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
    }
    public class Select2OptionsModel
    {
        public int id { get; set; }
        public string text { get; set; }
    }
    public class SchoolSessionModel
    {
        public int SessionID { get; set; }
        public DateTime SessionStartDate { get; set; }
        public DateTime SessionEndDate { get; set; }
        public int SessionStatus { get; set; }
        public string SessionName { get; set; }
        public int SBranchID { get; set; }
    }
    public class LocationListModel
    {
        public List<LocationModel> List { get; set; }
        public int Type { get; set; }
        public int MasterID { get; set; }
    }
    public class LocationModel
    {
        public int SBranchID { get; set; }
        public int MasterID { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public string Other { get; set; }
        public int Type { get; set; }
        public int IsApproved { get; set; }
        public int OpType { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
    public class SMSSendingModel
    {
        public int ID { get; set; }
        public int RecieverType { get; set; }
        public int ShiftID { get; set; }
        public string AssociatedIDs { get; set; }
        public string SMSText { get; set; }
    }
    public class DeleteFileModel
    {
        public string Type { get; set; }
        public int ID { get; set; }
        public string FileName { get; set; }
    }
    public class UploadViewModel
    {
        [Required]
        public HttpPostedFileBase File { get; set; }
        public string Type { get; set; }
        public int ID { get; set; }
    }
    public class GeneralModel
    {
    }
    public class UserModel
    {
        public string Token { get; set; }
        public int StateID { get; set; }
        public string NotificationServerKey { get; set; }
        public string BranchName { get; set; }
        public string BranchSchoolName { get; set; }
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public int OpType { get; set; }
        public string UserName { get; set; }
        public string EmailID { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }
        public string UserImage { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public int SavedBlogsCount { get; set; }
        public int Blogs { get; set; }
        public string Profession { get; set; }
        public string Password { get; set; }
        public string BranchLogo { get; set; }
        public int SBranchID { get; set; }
        public string AppToken { get; set; }
        public Role Role
        {
            get
            {
                return Role.GetById(this.RoleID);
            }
            set
            {
                this.Role = value;
                this.RoleID = value.ID;
            }
        }
        public List<Feature> AllowedFeatures = new List<Feature>();
        public List<string> SavedBlogs = new List<string>();
    }
    public class SBranchModel
    {
        public string PlayStoreLink { get; set; }
        public string NotificationServerKey { get; set; }
        public string PrincipalSignature { get; set; }
        public string Longitude { get; set; }
        public int StateID { get; set; }
        public string Latitude { get; set; }
        public string TagLine { get; set; }
        public string AccountUserName { get; set; }
        public string AccountPassword { get; set; }
        public string LibraryUserName { get; set; }
        public string LibraryPassword { get; set; }
        public int SBranchID { get; set; }
        public string BranchName { get; set; }
        public string Logo { get; set; }
        public string ContactNo { get; set; }
        public string EmailID { get; set; }
        public string Address { get; set; }
        public string PrincipalName { get; set; }
        public string PrincipalMobile { get; set; }
        public string PrincipalEmail { get; set; }
        public string BranchSchoolName { get; set; }
        public int Students { get; set; }
        public HttpPostedFileBase BranchLogo { get; set; }
        public HttpPostedFileBase PrincipalSignatureFile { get; set; }
        public string SchoolNo { get; set; }
        public string AffiliationNo { get; set; }
        public string BookNo { get; set; }
        public string RenewedUpto { get; set; }
        public int OpType { get; set; }
    }
    public class SubSubjectTypeEditModel
    {
        public int GroupID { get; set; }
        public List<SubSubjectTypeModel> Types { get; set; }
    }
    public class SubSubjectEditModel
    {
        public int MainSubjectID { get; set; }
        public int GroupID { get; set; }
        public List<SubSubjectTypeModel> Types { get; set; }
        public List<SubjectModel> Subjects { get; set; }
    }
    public class SubSubjectTypeModel
    {
        public int TypeID { get; set; }
        public int IsDefault { get; set; }
        public string TypeName { get; set; }
        public int SBranchID { get; set; }
        public int GroupID { get; set; }
        public int OpType { get; set; }
    }
    public class SubjectModel
    {
        public int GSubjectID { get; set; }
        public string GSubjectName { get; set; }
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }
        public int IsOptionalsubject { get; set; }
        public int SubjectType { get; set; }
        public string SubjectTypeName { get; set; }
        public int Status { get; set; }
        public int GroupID { get; set; }
        public int OpType { get; set; }
        public int EducationLevelID { get; set; }
        public string SubjectCode { get; set; }
        public int MainSubID { get; set; }
        public int SubSubjectCount { get; set; }
        public int IsAdditionalSubject { get; set; }
        public int IsGradeMarking { get; set; }
        public int IsGradeType { get; set; }
        public int EVSectionID { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal ScoredMarks { get; set; }
    }
    public class HouseModel
    {
        public int ID { get; set; }
        public int SBranchID { get; set; }
        public string Name { get; set; }
        public string HouseColor { get; set; }
        public int StudentCount { get; set; }
        public List<StudentModel> Students { get; set; }
        public int OpType { get; set; }
        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }
    }
    public class NameIDModel
    {
        public int OpType { get; set; }
        public int SBranchID { get; set; }
        public int SchoolID { get; set; }
        public int ID { get; set; }
        [AllowHtml]
        public string Name { get; set; }
        public string Extra1 { get; set; }
        public string Extra2 { get; set; }
        public string Extra3 { get; set; }
        public decimal Sum { get; set; }
        
    }
    public class EventModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int OpType { get; set; }
        private int eventID;

        public int EventID
        {
            get { return eventID; }
            set { eventID = value; }
        }


        private string _title;

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
        public string title
        {
            get { return _title; }
            set { _title = value; }
        }
        private string description;

        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        private DateTime startDate;

        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value; }
        }
        public DateTime start
        {
            get { return startDate; }
            set { startDate = value; }
        }
        private DateTime endDate;

        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; }
        }
        public DateTime end
        {
            get { return endDate; }
            set { endDate = value; }
        }
        private int eventTypeID;

        public int EventTypeID
        {
            get { return eventTypeID; }
            set { eventTypeID = value; }
        }
        private string classesIncludedIDs;
        private string classesIncluded;

        public string ClassesIncluded
        {
            get { return classesIncluded; }
            set { classesIncluded = value; }
        }
        private int sBranchID;

        public int SBranchID
        {
            get { return sBranchID; }
            set { sBranchID = value; }
        }

        private int isApproved;

        public int IsApproved
        {
            get { return isApproved; }
            set { isApproved = value; }
        }
        private string classNames;

        public string ClassNames
        {
            get { return classNames; }
            set { classNames = value; }
        }
        private string eventTypename;

        public string EventTypeName
        {
            get { return eventTypename; }
            set { eventTypename = value; }
        }
        private string _backgroundColor;

        public string backgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }
        private string _textColor;

        public string textColor
        {
            get { return _textColor; }
            set { _textColor = value; }
        }

        public string ClassesIncludedIDs
        {
            get
            {
                return classesIncludedIDs;
            }

            set
            {
                classesIncludedIDs = value;
            }
        }
    }
    public class EventCategoryModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string BackGroundColor { get; set; }
        public string TextColor { get; set; }
        public int SBranchID { get; set; }
        public int Count { get; set; }
        public int OpType { get; set; }
    }
    public class EventCalendarModel
    {
        public List<HolidayModel> Holidays { get; set; }
        public List<EventModel> Events { get; set; }
        public List<EventCategoryModel> EventTypes { get; set; }
        public List<ClassModel> Classes { get; set; }
    }
    public class MasterSettingModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Details { get; set; }
        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }
        public int Status { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
    }
    public class EmailSettingModel
    {
        public int SettingID { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int SSL { get; set; }
        public DateTime OperationDate { get; set; }
        public int OpType { get; set; }
    }
    public class CommonEvents
    {
        public string UUID { get; set; }
        public int OpType { get; set; }
        private int eventID;

        public int id
        {
            get { return eventID; }
            set { eventID = value; }
        }


        private string _title;

        public string title
        {
            get { return _title; }
            set { _title = value; }
        }
        private string description;

        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        private string startDate;

        public string start
        {
            get { return startDate; }
            set { startDate = value; }
        }
        private string endDate;

        public string end
        {
            get { return endDate; }
            set { endDate = value; }
        }
        private int eventTypeID;

        public int EventTypeID
        {
            get { return eventTypeID; }
            set { eventTypeID = value; }
        }
        private string classesIncludedIDs;
        private string classesIncluded;

        public string ClassesIncluded
        {
            get { return classesIncluded; }
            set { classesIncluded = value; }
        }
        private int sBranchID;

        public int SBranchID
        {
            get { return sBranchID; }
            set { sBranchID = value; }
        }

        private int isApproved;

        public int IsApproved
        {
            get { return isApproved; }
            set { isApproved = value; }
        }
        private string classNames;

        public string ClassNames
        {
            get { return classNames; }
            set { classNames = value; }
        }
        private string eventTypename;

        public string EventTypeName
        {
            get { return eventTypename; }
            set { eventTypename = value; }
        }
        private string _backgroundColor;

        public string backgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }
        private string _textColor;

        public string textColor
        {
            get { return _textColor; }
            set { _textColor = value; }
        }

        public string ClassesIncludedIDs
        {
            get
            {
                return classesIncludedIDs;
            }

            set
            {
                classesIncludedIDs = value;
            }
        }
    }
}