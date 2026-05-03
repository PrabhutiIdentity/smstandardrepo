using BigBlueButtonAPI.Common;
using BigBlueButtonAPI.Core;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using SMEnterprise.Utilities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SMEnterprise.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<OnlineClassHub> _hubContext;
        private IHostingEnvironment _hostingEnvironment;
        //private readonly BigBlueButtonAPIClient client;

        AccountData oData = new AccountData();

        public HomeController( IHostingEnvironment hostingEnvironment)
        {
              _hostingEnvironment = hostingEnvironment;
        }
        public HomeController()
        {
<<<<<<< HEAD
            CommonData objcd = new CommonData();
            try
            {

                objcd.InsertLog(0, "Home Page", "Before BBB Client");
                objcd.InsertLog(0, "BBBConfig", JsonConvert.SerializeObject(MvcApplication.BigBlueButtonAPISettings));
                // this.client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
                objcd.InsertLog(0, "Home Page", "After BBB Client");
            }
            catch (Exception ex)
            {
                DateTime dt = DateTime.Now;
                string Data = "";
                objcd.InsertLog(0, "Home Page Error", ex.Message);
            }
=======
            //CommonData objcd = new CommonData();
            //try
            //{

            //    objcd.InsertLog(0, "Home Page", "Before BBB Client");
            //    objcd.InsertLog(0, "BBBConfig", JsonConvert.SerializeObject(MvcApplication.BigBlueButtonAPISettings));
            //    // this.client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
            //    objcd.InsertLog(0, "Home Page", "After BBB Client");
            //}
            //catch (Exception ex)
            //{
            //    DateTime dt = DateTime.Now;
            //    string Data = "";
            //    objcd.InsertLog(0, "Home Page Error", ex.Message);
            //}
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
        }
        // GET: Home
        public ActionResult Index()
        {
            ViewBag.Salt = CommonUsage.RandomString(10, false);
            //  SMSSender.SendSMS("Test Message", "8860573641", 1);
            return View();
        }
        public ActionResult SendBirthdayWishes()
        {
            //For each Branch loop Start
                //Get Branch SMS COnfiguration
                //If SMS COnfiguration is valid start
                    //Get List of Brnach Students with todays birthday
                        //For each student start
                            //Send SMS to student
                        //For each student end    
                //If SMS Configuration is Valid end
            //For each Branch loop end
            //  SMSSender.SendSMS("Test Message", "8860573641", 1);
            return View();
        }
        public ActionResult ClassEnded()
        {

            return View();
        }
        public async Task<ActionResult> AdmissionEnquiry(string ID = null)
        
        {
            int BranchID = CommonUsage.ConvertToInt(ID);
            AdmissionEnquiryMasterModel model =await oData.GetBranchDetails(BranchID);
            return View(model);
        }
        public async Task<ActionResult> UpdateAdmission(AdmissionEnquiryMasterModel objData)
        {
            string filePath = "";
            if (objData.ImageFile != null)
            {
                objData.Image = objData.ImageFile.FileName.Replace(" ", "-");
            }
            //obj.SerailNO = Guid.NewGuid().ToString();
           

            objData = oData.UpdateAdmissionEnquiry(objData);
            if (objData.ImageFile != null)
            {

                var path = Path.Combine(Server.MapPath(CommonUsage.StudentDocumentsBasePath), objData.EnquiryID + "_AdmissionEnquiry_" + objData.Image);
                objData.ImageFile.SaveAs(path);
            }

            return Redirect("~/Home/Success/" + objData.EnquiryID);
        }
        public ActionResult Success(string id = null)
        {
            //int EnquiryID = CommonUsage.ConvertToInt(id);
            //AdmissionEnquiryMasterModel model = oData.GetEnquiryDetails(EnquiryID);

            return View();
        }
        public ActionResult StopSchedule()
        {

            return RedirectToAction("Index", "Home");
        }
        public ActionResult StartSchedule()
        {
            //ScheduledTasks.StartSQLDependancy();
            return RedirectToAction("Index", "Home");
        }
        public ActionResult StartAttandanceInsert()
        {
            for (int i = 0; i < 10000; i++)
            {
                CommonData.InsertTestingAttandance();
                System.Threading.Thread.Sleep(500);
            }
            //ScheduledTasks.StartSQLDependancy();
            return RedirectToAction("Index", "Home");
        }
        public ActionResult ReportCards()
        {
            return View();
        }
        public ActionResult StudentReportCard(StudentResultGetModel oModel)
        {

            PerformanceParameterDetailModel objModel = new PerformanceParameterDetailModel();
            objModel = (new CommonData()).GetStudentPerformanceDetails(oModel.DOB, oModel.StudentSID);
            if (objModel.Student == null)
            {
                TempData["Message"] = "Student Not Found";
                return RedirectToAction("ReportCards", "Home");
            }
            else
            {
                objModel.EvaluationID = -1;
                return View(objModel);
            }
        }
<<<<<<< HEAD
        public async Task<ActionResult> bbbrecordingready()
        {
            try
            {
                var encoded = Request.Form["signed_parameters"].ToString();
                var token = new JwtSecurityToken(jwtEncodedString: encoded);
                var MeetingID = "";
                var RecordingID = "";
                foreach (var key in token.Payload.Keys)
                {
                    if (key == "meeting_id")
                    {
                        MeetingID = token.Payload[key].ToString();
                    }
                    else if (key == "record_id")
                    {
                        RecordingID = token.Payload[key].ToString();
                    }
                }

                var request = new GetRecordingsRequest
                {
                    recordID = RecordingID,
                    meetingID = MeetingID
                };
                var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
                var result = await client.GetRecordingsAsync(request);
                string PlaybackURL = "";
                string Images = "";
                var rec = result.recordings[0];
                if (rec.published)
                {
                    foreach (var p in result.recordings[0].playbacks)
                    {
                        if (p.type == "presentation")
                        {
                            PlaybackURL = p.url;
                            if (p.previewImages != null && p.previewImages.Count > 0)
                            {
                                foreach (var image in p.previewImages)
                                {
                                    if (Images != "")
                                    {
                                        Images = Images + ",";
                                    }
                                    Images = Images + image.url;
                                }
                            }
                        }
                    }
                }

                BBBOnlineClassRecording oModel = new BBBOnlineClassRecording();
                oModel.MeetingID = MeetingID;
                oModel.RecordingState = rec.published ? "Published" : "UnPublished";
                oModel.PlaybackURL = PlaybackURL;
                oModel.Thumbnail = Images;
                oModel.RawRecordingSize = rec.rawSize;
                oModel.ProcessedRecordingSize = rec.size;

                (new BBBOnlineClassData()).UpdateRecordingReady(oModel);
            }
            catch (Exception ex)
            {
                (new CommonData()).InsertLog(0, "Recording Exception ", ex.ToString());
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        public async Task<ActionResult> bbbrecordingreadystaff()
        {
            try
            {
                var encoded = Request.Form["signed_parameters"].ToString();
                var token = new JwtSecurityToken(jwtEncodedString: encoded);
                var MeetingID = "";
                var RecordingID = "";
                foreach (var key in token.Payload.Keys)
                {
                    if (key == "meeting_id")
                    {
                        MeetingID = token.Payload[key].ToString();
                    }
                    else if (key == "record_id")
                    {
                        RecordingID = token.Payload[key].ToString();
                    }
                }

                var request = new GetRecordingsRequest
                {
                    recordID = RecordingID,
                    meetingID = MeetingID
                };
                var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
                var result = await client.GetRecordingsAsync(request);
                string PlaybackURL = "";
                string Images = "";
                var rec = result.recordings[0];
                if (rec.published)
                {
                    foreach (var p in result.recordings[0].playbacks)
                    {
                        if (p.type == "presentation")
                        {
                            PlaybackURL = p.url;
                            if (p.previewImages != null && p.previewImages.Count > 0)
                            {
                                foreach (var image in p.previewImages)
                                {
                                    if (Images != "")
                                    {
                                        Images = Images + ",";
                                    }
                                    Images = Images + image.url;
                                }
                            }
                        }
                    }
                }

                BBBOnlineClassRecording oModel = new BBBOnlineClassRecording();
                oModel.MeetingID = MeetingID;
                oModel.RecordingState = rec.published ? "Published" : "UnPublished";
                oModel.PlaybackURL = PlaybackURL;
                oModel.Thumbnail = Images;
                oModel.RawRecordingSize = rec.rawSize;
                oModel.ProcessedRecordingSize = rec.size;

                (new BBBOnlineStaffMeetingData()).UpdateRecordingReady(oModel);
            }
            catch (Exception ex)
            {
                (new CommonData()).InsertLog(0, "Recording Exception ", ex.ToString());
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        public async Task<ActionResult> EndOnlineClasses()
        {
            string MeetingID = Request.QueryString["meetingID"];

            BBBOnlineClassModel oModel = new BBBOnlineClassModel();
            oModel.EndedOn = CommonUsage.GetCurrentDate();
            oModel.Status = 2;
            oModel.MeetingID = MeetingID;
            BBBOnlineClassModel cModel = (new BBBOnlineClassData()).GetOnlineClassDetailsByMeetingID(MeetingID);
            var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
            var meetingStatus = await client.GetMeetingInfoAsync(new GetMeetingInfoRequest { meetingID = MeetingID });
            if (meetingStatus.returncode == Returncode.SUCCESS)
            {
                oModel.AttendeesJSON = Newtonsoft.Json.JsonConvert.SerializeObject(meetingStatus.attendees);
            }
            var List = (new BBBOnlineClassData()).EndOnlineClass(oModel);
            if (List > 0)
            {
                List<SMSRecieverModel> recievers = (new BBBOnlineClassData()).GetRecieverListOnOnlineClass(cModel.OCID);
                OnlineClassNotificationModel message = new OnlineClassNotificationModel();
                message.message = "Online Class Ended.";
                message.type = 2;
                message.typeid = cModel.OCID;
                string jmessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                NotificationModel objModel = new NotificationModel();
                objModel.RecieverID = -1;
                objModel.NotificationType = 10;
                objModel.NotificationDateTime = CommonUsage.GetCurrentDate();
                objModel.Recievers = new List<NotificationRecieverModel>();

                StringBuilder sb = new StringBuilder();
                foreach (SMSRecieverModel r in recievers)
                {
                    if (!String.IsNullOrEmpty(r.deviceToken))
                    {
                        if (r.deviceToken.Contains("\\n"))
                        {
                            r.deviceToken.Replace("\\n", "#");
                        }
                        if (sb != null && sb.ToString() != "")
                        {
                            sb.Append("#");
                        }
                        sb.Append(r.deviceToken);
                    }
                }
                string NotificationServerKey = "AAAAFoTO4zQ:APA91bEUshyzwyxd00uGjDfvaugyo57JfKun7-QaQJkPm7XO70-x31w3BnFKAOtwHkuQnj3eTdKmeSwk2UjkzzXHyJOqUrhV0PpkQuT7_lQUBKElQ5_kv_L2LFKR004CgCOsqtoLuIFZ";
                string[] Recievers = sb.ToString().Trim().Split("#".ToCharArray());
                CommonUsage.SendNotificationFCM(Recievers, jmessage, "10", NotificationServerKey);
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        public async Task<ActionResult> EndOnlineMeeting()
        {
            string MeetingID = Request.QueryString["meetingID"];

            OnlineStaffMeetingModel oModel = new OnlineStaffMeetingModel();
            oModel.EndedOn = CommonUsage.GetCurrentDate();
            oModel.Status = 2;
            oModel.BBBMeetingID = MeetingID;
            BBBOnlineStaffMeetingData bbbdata = new BBBOnlineStaffMeetingData();
            var cModel = bbbdata.GetOnlineStaffMeetingDetailsByMeetingID(MeetingID,0);
            var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
            var meetingStatus = await client.GetMeetingInfoAsync(new GetMeetingInfoRequest { meetingID = MeetingID });
            if (meetingStatus.returncode == Returncode.SUCCESS)
            {
                oModel.AttendeesJSON = Newtonsoft.Json.JsonConvert.SerializeObject(meetingStatus.attendees);
            }
            var List = bbbdata.UpdateOnlineStaffMeetingOnEnd(oModel);
            if (List > 0)
            {
                List<SMSRecieverModel> recievers = bbbdata.GetRecieverListOnStafMeeting(cModel.MeetingID);
                OnlineClassNotificationModel message = new OnlineClassNotificationModel();
                message.message = "Online Meeting Ended.";
                message.type = 2;
                message.typeid = cModel.MeetingID;
                string jmessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                NotificationModel objModel = new NotificationModel();
                objModel.RecieverID = -1;
                objModel.NotificationType = 10;
                objModel.NotificationDateTime = CommonUsage.GetCurrentDate();
                objModel.Recievers = new List<NotificationRecieverModel>();

                StringBuilder sb = new StringBuilder();
                foreach (SMSRecieverModel r in recievers)
                {
                    if (!String.IsNullOrEmpty(r.deviceToken))
                    {
                        if (r.deviceToken.Contains("\\n"))
                        {
                            r.deviceToken.Replace("\\n", "#");
                        }
                        if (sb != null && sb.ToString() != "")
                        {
                            sb.Append("#");
                        }
                        sb.Append(r.deviceToken);
                    }
                }
                string NotificationServerKey = "AAAAFoTO4zQ:APA91bEUshyzwyxd00uGjDfvaugyo57JfKun7-QaQJkPm7XO70-x31w3BnFKAOtwHkuQnj3eTdKmeSwk2UjkzzXHyJOqUrhV0PpkQuT7_lQUBKElQ5_kv_L2LFKR004CgCOsqtoLuIFZ";
                string[] Recievers = sb.ToString().Trim().Split("#".ToCharArray());
                CommonUsage.SendNotificationFCM(Recievers, jmessage, "10", NotificationServerKey);
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
=======
        //public async Task<ActionResult> bbbrecordingready()
        //{
        //    try
        //    {
        //        var encoded = Request.Form["signed_parameters"].ToString();
        //        var token = new JwtSecurityToken(jwtEncodedString: encoded);
        //        var MeetingID = "";
        //        var RecordingID = "";
        //        foreach (var key in token.Payload.Keys)
        //        {
        //            if (key == "meeting_id")
        //            {
        //                MeetingID = token.Payload[key].ToString();
        //            }
        //            else if (key == "record_id")
        //            {
        //                RecordingID = token.Payload[key].ToString();
        //            }
        //        }

        //        var request = new GetRecordingsRequest
        //        {
        //            recordID = RecordingID,
        //            meetingID = MeetingID
        //        };
        //        var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
        //        var result = await client.GetRecordingsAsync(request);
        //        string PlaybackURL = "";
        //        string Images = "";
        //        var rec = result.recordings[0];
        //        if (rec.published)
        //        {
        //            foreach (var p in result.recordings[0].playbacks)
        //            {
        //                if (p.type == "presentation")
        //                {
        //                    PlaybackURL = p.url;
        //                    if (p.previewImages != null && p.previewImages.Count > 0)
        //                    {
        //                        foreach (var image in p.previewImages)
        //                        {
        //                            if (Images != "")
        //                            {
        //                                Images = Images + ",";
        //                            }
        //                            Images = Images + image.url;
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        BBBOnlineClassRecording oModel = new BBBOnlineClassRecording();
        //        oModel.MeetingID = MeetingID;
        //        oModel.RecordingState = rec.published ? "Published" : "UnPublished";
        //        oModel.PlaybackURL = PlaybackURL;
        //        oModel.Thumbnail = Images;
        //        oModel.RawRecordingSize = rec.rawSize;
        //        oModel.ProcessedRecordingSize = rec.size;

        //        (new BBBOnlineClassData()).UpdateRecordingReady(oModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        (new CommonData()).InsertLog(0, "Recording Exception ", ex.ToString());
        //    }
        //    return new HttpStatusCodeResult(HttpStatusCode.OK);
        //}
        //public async Task<ActionResult> bbbrecordingreadystaff()
        //{
        //    try
        //    {
        //        var encoded = Request.Form["signed_parameters"].ToString();
        //        var token = new JwtSecurityToken(jwtEncodedString: encoded);
        //        var MeetingID = "";
        //        var RecordingID = "";
        //        foreach (var key in token.Payload.Keys)
        //        {
        //            if (key == "meeting_id")
        //            {
        //                MeetingID = token.Payload[key].ToString();
        //            }
        //            else if (key == "record_id")
        //            {
        //                RecordingID = token.Payload[key].ToString();
        //            }
        //        }

        //        var request = new GetRecordingsRequest
        //        {
        //            recordID = RecordingID,
        //            meetingID = MeetingID
        //        };
        //        var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
        //        var result = await client.GetRecordingsAsync(request);
        //        string PlaybackURL = "";
        //        string Images = "";
        //        var rec = result.recordings[0];
        //        if (rec.published)
        //        {
        //            foreach (var p in result.recordings[0].playbacks)
        //            {
        //                if (p.type == "presentation")
        //                {
        //                    PlaybackURL = p.url;
        //                    if (p.previewImages != null && p.previewImages.Count > 0)
        //                    {
        //                        foreach (var image in p.previewImages)
        //                        {
        //                            if (Images != "")
        //                            {
        //                                Images = Images + ",";
        //                            }
        //                            Images = Images + image.url;
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        BBBOnlineClassRecording oModel = new BBBOnlineClassRecording();
        //        oModel.MeetingID = MeetingID;
        //        oModel.RecordingState = rec.published ? "Published" : "UnPublished";
        //        oModel.PlaybackURL = PlaybackURL;
        //        oModel.Thumbnail = Images;
        //        oModel.RawRecordingSize = rec.rawSize;
        //        oModel.ProcessedRecordingSize = rec.size;

        //        (new BBBOnlineStaffMeetingData()).UpdateRecordingReady(oModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        (new CommonData()).InsertLog(0, "Recording Exception ", ex.ToString());
        //    }
        //    return new HttpStatusCodeResult(HttpStatusCode.OK);
        //}
        //public async Task<ActionResult> EndOnlineClasses()
        //{
        //    string MeetingID = Request.QueryString["meetingID"];

        //    BBBOnlineClassModel oModel = new BBBOnlineClassModel();
        //    oModel.EndedOn = CommonUsage.GetCurrentDate();
        //    oModel.Status = 2;
        //    oModel.MeetingID = MeetingID;
        //    BBBOnlineClassModel cModel = (new BBBOnlineClassData()).GetOnlineClassDetailsByMeetingID(MeetingID);
        //    var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
        //    var meetingStatus = await client.GetMeetingInfoAsync(new GetMeetingInfoRequest { meetingID = MeetingID });
        //    if (meetingStatus.returncode == Returncode.SUCCESS)
        //    {
        //        oModel.AttendeesJSON = Newtonsoft.Json.JsonConvert.SerializeObject(meetingStatus.attendees);
        //    }
        //    var List = (new BBBOnlineClassData()).EndOnlineClass(oModel);
        //    if (List > 0)
        //    {
        //        List<SMSRecieverModel> recievers = (new BBBOnlineClassData()).GetRecieverListOnOnlineClass(cModel.OCID);
        //        OnlineClassNotificationModel message = new OnlineClassNotificationModel();
        //        message.message = "Online Class Ended.";
        //        message.type = 2;
        //        message.typeid = cModel.OCID;
        //        string jmessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
        //        NotificationModel objModel = new NotificationModel();
        //        objModel.RecieverID = -1;
        //        objModel.NotificationType = 10;
        //        objModel.NotificationDateTime = CommonUsage.GetCurrentDate();
        //        objModel.Recievers = new List<NotificationRecieverModel>();

        //        StringBuilder sb = new StringBuilder();
        //        foreach (SMSRecieverModel r in recievers)
        //        {
        //            if (!String.IsNullOrEmpty(r.deviceToken))
        //            {
        //                if (r.deviceToken.Contains("\\n"))
        //                {
        //                    r.deviceToken.Replace("\\n", "#");
        //                }
        //                if (sb != null && sb.ToString() != "")
        //                {
        //                    sb.Append("#");
        //                }
        //                sb.Append(r.deviceToken);
        //            }
        //        }
        //        string NotificationServerKey = "AAAAFoTO4zQ:APA91bEUshyzwyxd00uGjDfvaugyo57JfKun7-QaQJkPm7XO70-x31w3BnFKAOtwHkuQnj3eTdKmeSwk2UjkzzXHyJOqUrhV0PpkQuT7_lQUBKElQ5_kv_L2LFKR004CgCOsqtoLuIFZ";
        //        string[] Recievers = sb.ToString().Trim().Split("#".ToCharArray());
        //        CommonUsage.SendNotificationFCM(Recievers, jmessage, "10", NotificationServerKey);
        //    }
        //    return new HttpStatusCodeResult(HttpStatusCode.OK);
        //}
        //public async Task<ActionResult> EndOnlineMeeting()
        //{
        //    string MeetingID = Request.QueryString["meetingID"];

        //    OnlineStaffMeetingModel oModel = new OnlineStaffMeetingModel();
        //    oModel.EndedOn = CommonUsage.GetCurrentDate();
        //    oModel.Status = 2;
        //    oModel.BBBMeetingID = MeetingID;
        //    BBBOnlineStaffMeetingData bbbdata = new BBBOnlineStaffMeetingData();
        //    var cModel = bbbdata.GetOnlineStaffMeetingDetailsByMeetingID(MeetingID,0);
        //    var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
        //    var meetingStatus = await client.GetMeetingInfoAsync(new GetMeetingInfoRequest { meetingID = MeetingID });
        //    if (meetingStatus.returncode == Returncode.SUCCESS)
        //    {
        //        oModel.AttendeesJSON = Newtonsoft.Json.JsonConvert.SerializeObject(meetingStatus.attendees);
        //    }
        //    var List = bbbdata.UpdateOnlineStaffMeetingOnEnd(oModel);
        //    if (List > 0)
        //    {
        //        List<SMSRecieverModel> recievers = bbbdata.GetRecieverListOnStafMeeting(cModel.MeetingID);
        //        OnlineClassNotificationModel message = new OnlineClassNotificationModel();
        //        message.message = "Online Meeting Ended.";
        //        message.type = 2;
        //        message.typeid = cModel.MeetingID;
        //        string jmessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
        //        NotificationModel objModel = new NotificationModel();
        //        objModel.RecieverID = -1;
        //        objModel.NotificationType = 10;
        //        objModel.NotificationDateTime = CommonUsage.GetCurrentDate();
        //        objModel.Recievers = new List<NotificationRecieverModel>();

        //        StringBuilder sb = new StringBuilder();
        //        foreach (SMSRecieverModel r in recievers)
        //        {
        //            if (!String.IsNullOrEmpty(r.deviceToken))
        //            {
        //                if (r.deviceToken.Contains("\\n"))
        //                {
        //                    r.deviceToken.Replace("\\n", "#");
        //                }
        //                if (sb != null && sb.ToString() != "")
        //                {
        //                    sb.Append("#");
        //                }
        //                sb.Append(r.deviceToken);
        //            }
        //        }
        //        string NotificationServerKey = "AAAAFoTO4zQ:APA91bEUshyzwyxd00uGjDfvaugyo57JfKun7-QaQJkPm7XO70-x31w3BnFKAOtwHkuQnj3eTdKmeSwk2UjkzzXHyJOqUrhV0PpkQuT7_lQUBKElQ5_kv_L2LFKR004CgCOsqtoLuIFZ";
        //        string[] Recievers = sb.ToString().Trim().Split("#".ToCharArray());
        //        CommonUsage.SendNotificationFCM(Recievers, jmessage, "10", NotificationServerKey);
        //    }
        //    return new HttpStatusCodeResult(HttpStatusCode.OK);
        //}
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
    }
    
    public class StudentResultGetModel
    {
        public DateTime DOB { get; set; }
        public string StudentSID { get; set; }
    }
}