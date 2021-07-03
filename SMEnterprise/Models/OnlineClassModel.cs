using BigBlueButtonAPI.Common;
using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class OnlineStaffMeetingModel
    {
        public string UUID { get; set; }
        public string JitsiMeetingID { get; set; }

        public int MeetingID { get; set; }
        public string BBBMeetingID { get; set; }
        public string UpdatedOn { get; set; }
        public DateTime MeetingDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string MeetingTitle { get; set; }
        public string InternalMeetingID { get; set; }
        public string ModPassword { get; set; }
        public string AttPassword { get; set; }
        public string Attendees { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime EndedOn { get; set; }
        public int Status { get; set; }
        public int SBranchID { get; set; }
        public string RecordingJSON { get; set; }
        public string AttendeesJSON { get; set; }
        public List<BBBOnlineClassRecording> Recordings
        {
            get
            {
                if (RecordingJSON != null)
                {
                    return JsonConvert.DeserializeObject<List<BBBOnlineClassRecording>>(RecordingJSON);
                }
                else
                {
                    return new List<BBBOnlineClassRecording>();
                }
            }
        }
        public List<Attendee> AttendeeList
        {
            get
            {
                if (AttendeesJSON != null)
                {
                    return JsonConvert.DeserializeObject<List<Attendee>>(AttendeesJSON);
                }
                else
                {
                    return new List<Attendee>();
                }
            }
        }
    }
    public class OnlineClassNotificationModel
    {
        public int type { get; set; }
        public string message { get; set; }
        public string image
        {
            get
            {
                return "https://node1.pschoolonline.com/images/pSchoolBanner.jpg";
            }
        }
        public int typeid { get; set; }
        public string Data { get; set; }
        public string base_url { get; set; }

    }
    public class BBBOnlineClassStudentPageModel
    {
        public DateTime ClassDate { get; set; }
        public int StudentID { get; set; }
        public List<BBBOnlineClassModel> Classes { get; set; }
    }
    public class BBBOnlineClassListModel
    {
        public DateTime ClassDate { get; set; }
        public string OnlineClassURL { get; set; }
        public List<BBBOnlineClassModel> Classes { get; set; }
        public List<NameIDModel> ClassList { get; set; }
        public List<NameIDModel> SubjectList { get; set; }
        public List<NameIDModel> Sessions { get; set; }
    }
    public class BBBOnlineClassModel
    {
        public string UUID { get; set; }
        public int OCID { get; set; }
        public string ModPassword { get; set; }
        public string AttPassword { get; set; }
        public int GroupID { get; set; }
        public string ClassSection { get; set; }
        public string SubjectName { get; set; }
        public int TeacherID { get; set; }
        public string TeacherName { get; set; }
        public int SectionID { get; set; }
        public int SubjectID { get; set; }
        public int SessionID { get; set; }
        public int SBranchID { get; set; }
        public int Status { get; set; }
        public DateTime ClassDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MeetingID { get; set; }
        public string InternalMeetingID { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime EndedOn { get; set; }
        public int IsRecordingReady { get; set; }
        [JsonIgnore]
        public string RecordingJSON { get; set; }
        public string AttendeesJSON { get; set; }
        public List<BBBOnlineClassRecording> Recordings
        {
            get
            {
                if (RecordingJSON != null)
                {
                    return JsonConvert.DeserializeObject<List<BBBOnlineClassRecording>>(RecordingJSON);
                }
                else
                {
                    return new List<BBBOnlineClassRecording>();
                }
            }
        }
        public List<BBBAttendees> Attendees
        {
            get
            {
                if (AttendeesJSON != null)
                {
                    return JsonConvert.DeserializeObject<List<BBBAttendees>>(AttendeesJSON);
                }
                else
                {
                    return new List<BBBAttendees>();
                }
            }
        }
    }
    public class BBBAttendees
    {
        public int StudentID { get; set; }
        public string Name { get; set; }
        public int AppType { get; set; }
        public string AppTypeText
        {
            get
            {
                if (AppType == 0)
                {
                    return "Web Panel";
                }
                else
                {
                    return "Mobile App";
                }
            }
        }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
    public class BBBOnlineClassRecording
    {
        public int RecID { get; set; }
        public string MeetingID { get; set; }
        public string PlaybackURL { get; set; }
        public string RecordingState { get; set; }
        public int RawRecordingSize { get; set; }
        public int ProcessedRecordingSize { get; set; }
        [JsonIgnore]
        public string Thumbnail { get; set; }
        public string[] Thumbnails
        {
            get
            {
                if (Thumbnail == null)
                {
                    return null;
                }
                else
                {
                    return Thumbnail.Split(",".ToCharArray());
                }
            }
        }
        public long RecordingStartTime { get; set; }
        public long RecordingEndTime { get; set; }
    }
    public class OnlineClassModel
    {
        public string UUID { get; set; }
        public int OCID { get; set; }
        public int TeacherID { get; set; }
        public string TeacherName { get; set; }
        public int SectionID { get; set; }
        public int GroupID { get; set; }
        public int SubjectID { get; set; }
        public int Status { get; set; }
        public string MeetingID { get; set; }
        public DateTime ClassDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ChangeLog { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime EndedOn { get; set; }
        public string ClassSection { get; set; }
        public string SubjectName { get; set; }
    }
    public class OnlineClassAttendeeModel
    {
        public int OCID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    //public class OnlineStaffMeetingModel
    //{
    //    public int MeetingID { get; set; }
    //    public string UUID { get; set; }
    //    public DateTime MeetingDate { get; set; }
    //    public string JitsiMeetingID
    //    {
    //        get
    //        {
    //            return MeetingDate.ToString("yyyy-mm-dd") + "-" + MeetingID;
    //        }
    //    }
    //    public string StartTime { get; set; }
    //    public string EndTime { get; set; }
    //    public string MeetingTitle { get; set; }
    //    public int Status { get; set; }
    //    public int SBranchID { get; set; }
    //}

}