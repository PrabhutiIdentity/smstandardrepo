using BigBlueButtonAPI.Common;
using BigBlueButtonAPI.Core;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc.Routing;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;

namespace SMEnterprise.Controllers
{
    public class TeacherApiController : ApiController
    {
        private readonly BigBlueButtonAPIClient client;
        public TeacherApiController()
        {
            this.client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
        }
        private async Task<bool> isBigBlueButtonAPISettingsOKAsync()
        {
            try
            {
                var res = await client.IsMeetingRunningAsync(new IsMeetingRunningRequest { meetingID = Guid.NewGuid().ToString() });
                if (res.returncode == Returncode.FAILED) return false;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private UserModel VerifyUser(string UUID)
        {
            UserModel objUserModel = new UserModel();
            List<ApiAuthenticationModel> objapiModel = (new CommonData()).GetAppUser(UUID);
            bool isFound = false;
            if (objapiModel.Count > 0)
            {
                foreach (ApiAuthenticationModel a in objapiModel)
                {
                    if (a.UserType == (int)RoleType.Teacher)
                    {
                        objUserModel.UserID = a.UserID;
                        objUserModel.RoleID = a.UserType;
                        objUserModel.UserName = a.UserName;
                        objUserModel.SBranchID = a.SBranchID;
                        isFound = true;
                    }
                }
            }
            if (!isFound)
            {
                objUserModel = null;

            }
            return objUserModel;
        }
        public async Task<CommonApiWraperModel> JoinClassNew(string ID = null)
        {
            CommonApiWraperModel response = new CommonApiWraperModel();
            var user = new UserModel
            {
                FullName = "Sourabh Sharma",
                UserID = 1,
                SBranchID = 1,
                BranchLogo = "/images/SBranchLogo/1.jpg",
                UserImage = "/images/SBranchLogo/1.jpg"
            };
            int OCID = CommonUsage.ConvertToInt(ID);
            BBBOnlineClassModel cModel = (new BBBOnlineClassData()).GetOnlineClassDetailsByOCID(OCID);

            string basepath = $"{this.Request.RequestUri.Scheme}://{this.Request.RequestUri.Host}";
            string logo = basepath + "/Images/SBranchLogo/" + user.SBranchID + "_" + user.BranchLogo;
            string avatar = basepath + "/Images/EmployeeImage/" + user.UserID + "_" + user.UserImage;
            var meetingStatus = await client.GetMeetingInfoAsync(new GetMeetingInfoRequest { meetingID = cModel.MeetingID });
            if (meetingStatus.returncode == Returncode.FAILED)
            {
                MetaData meta = new MetaData();
                meta.Add("BranchID", "1");
                string meu = basepath + "/Home/EndOnlineClasses/";
                meta.Add("endCallbackUrl", meu);

                string reccbu = basepath + "/home/bbbrecordingready/";
                meta.Add("bbb-recording-ready-url", reccbu);
                var result = await client.CreateMeetingAsync(new CreateMeetingRequest
                {
                    name = cModel.SubjectName + " (" + cModel.ClassSection + "), by " + cModel.TeacherName + " on " + cModel.ClassDate.ToString("dd MMM, yyyy"),
                    meetingID = cModel.MeetingID,
                    record = true,
                    //logoutURL = basepath + "/Home/LogoutOnlineClasses/" + cModel.MeetingID,
                    meta = meta,
                    guestPolicy = "ALWAYS_ACCEPT",
                    logo = logo,
                    
                    lockSettingsDisablePrivateChat = true,
                    lockSettingsDisableNote = false,
                    lockSettingsLockedLayout = false,
                    muteOnStart = true,
                    allowModsToUnmuteUsers = true,
                    //autoStartRecording = true,
                    //bannerText = "Online Class for Subject:" + cModel.SubjectName + " Class:" + cModel.ClassSection + " By :" + cModel.TeacherName
                });
                if (result.returncode == Returncode.FAILED) return response;
                if (result.returncode != Returncode.FAILED)
                {
                    cModel.InternalMeetingID = result.internalMeetingID;
                    cModel.Status = 1;
                    cModel.StartedOn = CommonUsage.GetCurrentDate();
                    cModel.AttPassword = result.attendeePW;
                    cModel.ModPassword = result.moderatorPW;
                    (new BBBOnlineClassData()).UpdateBBBOnlineClassDetails(cModel);
                }
            }

            var requestJoin = new JoinMeetingRequest { meetingID = cModel.MeetingID };
            requestJoin.userID = user.UserID.ToString();
            requestJoin.fullName = user.FullName;
            requestJoin.password = cModel.ModPassword;
            var setConfigRequest = new SetConfigXMLRequest
            {
                meetingID = cModel.MeetingID,
                configXML = "<config><modules><localeversion supressWarning=\"false\">0.9.0</localeversion></modules></config>"
            };
            var setConfigResult = await client.SetConfigXMLAsync(setConfigRequest);
            if (setConfigResult.returncode == Returncode.FAILED) return response;
            requestJoin.configToken = setConfigResult.configToken;
            requestJoin.avatarURL = avatar;
            var url = client.GetJoinMeetingUrl(requestJoin);
            response.Data = url;
            return response;
        }
        [HttpPost]
        public CommonApiWraperModel GetVehicleLocation(TransportGeoModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            try
            {
                DateTime cDate = CommonUsage.GetCurrentDate();
                try
                {
                    data.GioDate = cDate.ToString("yyyy-MM-dd");
                    if (data.IsAll == 0)
                    {
                        objWraper.Data = GioData.SearchDefault("", data.GioDate, data.VehicleID.ToString()).OrderByDescending(o => Convert.ToDateTime(data.GioDate + " " + o.GioTime)).ToList<object>().FirstOrDefault();
                    }
                    else
                    {
                        objWraper.List = GioData.SearchDefault("", data.GioDate, data.VehicleID.ToString()).OrderByDescending(o => Convert.ToDateTime(data.GioDate + " " + o.GioTime)).ToList<object>();//GioData.GetAllIndexRecords().ToList<object>();//
                        objWraper.Data = objWraper.List.FirstOrDefault();
                    }
                }
                catch
                {

                }
                objWraper.Code = 200;
                objWraper.Message = "Success";
            }
            catch (Exception ex)
            {
                objWraper.Code = 404;
                objWraper.Message = ex.InnerException.ToString();
            }

            return objWraper;
        }
        [HttpPost]
        [Route("api/TeacherApi/UploadAttachment")]
        public CommonApiWraperModel UploadAttachment()
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            // Check if the request contains multipart/form-data.  
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }
                HttpPostedFile postedFile = HttpContext.Current.Request.Files[0];
                if (HttpContext.Current.Request.Form["ID"] != null)
                {
                    string directoryName = String.Empty;
                    string filename = String.Empty;
                    var thisFileName = postedFile.FileName.Trim('\"');
                    string ID = HttpContext.Current.Request.Form["ID"];
                    int Type = CommonUsage.ConvertToInt(HttpContext.Current.Request.Form["Type"]);
                    var path = HttpRuntime.AppDomainAppPath;
                    if (Type == 0)
                    {
                        directoryName = System.IO.Path.Combine(path, "Attachments\\Assignments");
                    }
                    else if (Type == 1)
                    {
                        directoryName = System.IO.Path.Combine(path, "Images\\BlackBoard");
                    }
                    else
                    {
                        directoryName = System.IO.Path.Combine(path, "Attachments");
                    }
                    thisFileName = CommonUsage.GetValidFileName(thisFileName);
                    filename = System.IO.Path.Combine(directoryName, ID + "_" + thisFileName);

                    //Deletion exists file  
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }

                    //Directory.CreateDirectory(@directoryName);  
                    postedFile.SaveAs(filename);

                    if (Type == 0)
                    {
                        (new TeacherData()).UpdateAssignmentAttachment(ID, thisFileName);
                    }
                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    objWraper.Code = 200;
                    objWraper.Message = "Success";
                    objWraper.Data = directoryName + "\\" + ID + "_" + thisFileName;
                }
                else
                {
                    objWraper.Code = 404;
                    objWraper.Message = "Not Uploaded " + HttpContext.Current.Request.Form["ID"];
                }
            }
            catch (Exception ex)
            {
                objWraper.Code = 1;
                objWraper.Message = ex.ToString();

            }
            return objWraper;
        }
        //[HttpPost]
        //[Route("api/TeacherApi/UploadAttachmentOld")]
        //public async Task<CommonApiWraperModel> UploadAttachmentOld()
        //{
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    // Check if the request contains multipart/form-data.  
        //    try
        //    {
        //        if (!Request.Content.IsMimeMultipartContent())
        //        {
        //            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //        }

        //        var provider = await Request.Content.ReadAsMultipartAsync(new InMemoryMultipartFormDataStreamProvider());
        //        //access form data  
        //        NameValueCollection formData = provider.FormData;
        //        //access files  
        //        IList<HttpContent> files = provider.Files;

        //        if (formData["ID"] != null)
        //        {
        //            HttpContent file1 = files[0];

        //            string filename = String.Empty;
        //            Stream input = await file1.ReadAsStreamAsync();
        //            string directoryName = String.Empty;
        //            string URL = String.Empty;
        //            var thisFileName = file1.Headers.ContentDisposition.FileName.Trim('\"');
        //            string ID = formData["ID"];
        //            int Type = CommonUsage.ConvertToInt(formData["Type"]);
        //            var path = HttpRuntime.AppDomainAppPath;
        //            if (Type == 0)
        //            {
        //                directoryName = System.IO.Path.Combine(path, "Attachments\\Assignments");
        //            }
        //            else if (Type == 1)
        //            {
        //                directoryName = System.IO.Path.Combine(path, "Images\\BlackBoard");
        //            }
        //            else
        //            {
        //                directoryName = System.IO.Path.Combine(path, "Attachments");
        //            }
        //            thisFileName = CommonUsage.GetValidFileName(thisFileName);
        //            filename = System.IO.Path.Combine(directoryName, ID + "_" + thisFileName);

        //            //Deletion exists file  
        //            if (File.Exists(filename))
        //            {
        //                File.Delete(filename);
        //            }

        //            //Directory.CreateDirectory(@directoryName);  
        //            using (Stream file = File.OpenWrite(filename))
        //            {
        //                input.CopyTo(file);
        //                //close file  
        //                file.Close();
        //            }
        //            if (Type == 0)
        //            {
        //                (new TeacherData()).UpdateAssignmentAttachment(ID, thisFileName);
        //            }
        //            var response = Request.CreateResponse(HttpStatusCode.OK);
        //            objWraper.Code = 200;
        //            objWraper.Message = "Success";
        //            objWraper.Data = directoryName + "\\" + ID + "_" + thisFileName;
        //        }
        //        else
        //        {
        //            objWraper.Code = 404;
        //            objWraper.Message = "Not Uploaded " + formData["ID"];
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        objWraper.Code = 1;
        //        objWraper.Message = ex.ToString();

        //    }
        //    return objWraper;
        //}
        [HttpPost]
        [Route("api/TeacherApi/GetClassTeacherClassesSections")]
        public CommonApiWraperModel GetClassTeacherClassesSections(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                int Day = CommonUsage.GetCurrentDate().Day;
                int Month = CommonUsage.GetCurrentDate().Month;
                int Year = CommonUsage.GetCurrentDate().Year;
                List<object> List = objTeacherData.GetClassTeacherClassSections(data.ID, Day, Month, Year).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        [Route("api/TeacherApi/GetClassSectionAttandanceList")]
        public CommonApiWraperModel GetClassSectionAttandanceList(TeacherAttandanceParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                int Day = CommonUsage.GetCurrentDate().Day;
                int Month = CommonUsage.GetCurrentDate().Month;
                int Year = CommonUsage.GetCurrentDate().Year;
                List<object> List = objTeacherData.GetAppStudentAttandanceList(data.ClassID, data.SectionID, Day, Month, Year, user.SBranchID).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel UpdateStudentAttandance(StudentAttandancePageModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                data.SBranchID = user.SBranchID;
                data.Day = "D" + CommonUsage.GetCurrentDate().Day;
                data.Month = CommonUsage.GetCurrentDate().Month;
                data.Year = CommonUsage.GetCurrentDate().Year;

                int Data = objTeacherData.UpdateStudentAttandance(data);
                if (Data > 0)
                {
                    objWraper.Code = 200;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetNotices(TeacherAttandanceParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                List<object> List = objTeacherData.GetNotices(user.SBranchID).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetEvents(TeacherAttandanceParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                List<object> List = objTeacherData.GetEvents(user.SBranchID, user.UserID).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        #region Assignment
        [HttpPost]
        public CommonApiWraperModel DeleteAssignment(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                try
                {
                    int res = objTeacherData.DeleteAssignment(data.ID);
                    objWraper.Code = 200;
                    objWraper.Data = "Deleted Successfully!";
                    objWraper.Message = "Deleted Successfully!";
                }
                catch
                {
                    objWraper.Code = 404;
                    objWraper.Message = "Assignment not Deleted!";
                }
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetAssignmentSubmissions(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (VerifyUser(data.UUID) != null)
            {
                TeacherData objCommonData = new TeacherData();
                AssignmentSubmissionListPage oModel = objCommonData.GetAssignmentSubmissions(data.ID);
                oModel.BasePath = "/Attachments/AssignmentSubmissions/";
                if (oModel.Submissions.Count == 0)
                {
                    objWraper.Code = 404;
                }
                else
                {
                    objWraper.Code = 200;
                    objWraper.Data = oModel;
                }
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public async Task<CommonApiWraperModel> GetAssignmentResponse(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (VerifyUser(data.UUID) != null)
            {
                ParentData objCommonData = new ParentData();
                AssignmentSubmissionModel oModel = await objCommonData.GetStudentAssignmentResponse(data.ID, data.ID2);
                oModel.BasePath = "/Attachments/AssignmentSubmissions/";
                if (oModel == null)
                {
                    objWraper.Code = 404;
                }
                else
                {
                    objWraper.Code = 200;
                    objWraper.Data = oModel;
                }
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel UpdateAssSubmissionStatus(AssignmentSubmissionModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();

                int Data = objTeacherData.UpdateTeacherAssSubmissionResponse(data);
                if (Data > 0)
                {
                    objWraper.Code = 200;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetTeacherTeachingClassesSections(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                List<object> List = objTeacherData.GetTeacherTeachingClassSections(user.UserID).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetTeacherSubjectsOnClassSection(TeacherAttandanceParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                List<object> List = objTeacherData.GetTeacherTeachingSubjectsOnClassSections(user.UserID, data.ClassID, data.SectionID).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetAssignments(AssignmentPageModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                data.TeacherID = user.UserID;
                List<AssignmentModel> List = objTeacherData.GetTeacherAssignmentList(data);
                List.ForEach(x => x.Attachments = x.Attachments == null ? x.Attachments : x.Attachments.Replace(",", ";"));
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List.ToList<object>();
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetChaptersForSubjects(TeacherChapterParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                List<object> List = objTeacherData.GetChaptersForSubject(data.ClassID, data.SubjectID).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel UpdateAssignment(AssignmentModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                if (data.Attachments != null)
                {
                    string attachmentnames = "";
                    string[] arratt = data.Attachments.Split(',');
                    foreach (var sa in arratt)
                    {
                        if (attachmentnames != "")
                        {
                            attachmentnames = attachmentnames + ",";
                        }
                        attachmentnames = attachmentnames + CommonUsage.GetValidFileName(sa);
                    }
                    data.Attachments = attachmentnames;
                }
                //data.Attachments = "";
                TeacherData objTeacherData = new TeacherData();
                data.OperationDate = CommonUsage.GetCurrentDate();
                data.TeacherID = user.UserID;
                int Data = objTeacherData.UpdateAssignment(data);
                if (Data > 0)
                {
                    objWraper.Code = 200;
                    objWraper.Data = Data;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        #endregion

        #region Employee Leaves
        [HttpPost]
        public CommonApiWraperModel GetLeaves(TeacherAttandanceParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                List<object> List = objTeacherData.GetEmployeeLeaves(user.UserID).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetLeaveDetails(TeacherLeaveParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                int EmployeeID = user.UserID;
                int SBranchID = user.SBranchID;
                int Month = data.Month;
                int Year = data.Year;


                List<EmployeeLeaveSummery> objModel = objTeacherData.GetEmployeeLeavesEditDetails(SBranchID, Month, Year, EmployeeID);
                if (objModel.Count() > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = objModel.ToList<object>();
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel UpdateLeave(EmployeeLeaveModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                AccountData objAccountData = new AccountData();
                data.SBranchID = user.SBranchID;
                data.UserID = user.UserID;
                data.CreatedDate = CommonUsage.GetCurrentDate();
                data.ApplicantType = 0;
                data.EmployeeType = 3;
                data.IsApproved = 2;
                data.EmployeeID = user.UserID;

                int Data = objAccountData.UpdateEmployeeLeaveNew(data);
                if (Data > 0)
                {
                    objWraper.Code = 200;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        #endregion

        [HttpPost]
        public CommonApiWraperModel GetDayPeriods(ParentApiParamModel data)
        {
            UserModel um = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (um != null)
            {
                data.ID = um.UserID;
                TeacherData objTeacherData = new TeacherData();
                object Data = objTeacherData.GetTeacherApiTTDayLactures(data);
                if (Data != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = Data;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }

            return objWraper;

        }

        [HttpPost]
        public CommonApiWraperModel GetTeacherParentDiaryList(TeacherAttandanceParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.SubjectID = data.SubjectID == 0 ? data.ID : data.SubjectID;
                TeacherData objTeacherData = new TeacherData();
                List<object> List = objTeacherData.GetTeacherParentDiaryEntries(user.UserID, data.ClassID, data.SectionID, data.SubjectID).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetStudentListOnClassSection(TeacherAttandanceParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                List<object> List = objTeacherData.GetTeacherParentDiaryStudentList(data.ClassID, data.SectionID, user.SBranchID).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetParentDiaryDetails(ParentApiParamModel data)
        {
            UserModel um = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (um != null)
            {
                TeacherData objTeacherData = new TeacherData();
                object Data = objTeacherData.GetTeacherParentDiaryDetails(um.UserID, data.ID);
                if (Data != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = Data;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }

            return objWraper;

        }
        [HttpPost]
        public CommonApiWraperModel AddParentDiaryEntry(ParentDiaryModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                data.SBranchID = user.SBranchID;
                data.PBDate = CommonUsage.GetCurrentDate();
                data.TeacherID = user.UserID;
                int Data = objTeacherData.UpdateParentDiaryBulk(data);
                if (Data > 0)
                {
                    objWraper.Code = 200;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel DeleteParentDiaryEntry(ParentDiaryModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                data.SBranchID = user.SBranchID;
                data.PBDate = CommonUsage.GetCurrentDate();
                data.TeacherID = user.UserID;
                int Data = objTeacherData.DeleteParentDiary(data.ID, data.TeacherID);
                if (Data > 0)
                {
                    objWraper.Code = 200;
                    objWraper.Message = "Parent Diary Deleted Successfully.";
                }
                else
                {
                    objWraper.Code = 404;
                    objWraper.Message = "Some Error Occured!.";
                }
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetHolidays(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            UserModel user = VerifyUser(data.UUID);
            if (user != null)
            {

                ParentData objCommonData = new ParentData();
                List<object> List = objCommonData.GetTeacherHolidays(user.UserID).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }

            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetTeacherTransportMap(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (VerifyUser(data.UUID) != null)
            {
                TeacherData objTeacherData = new TeacherData();
                object Data = objTeacherData.GetTeacherTransportMap(data.ID);
                if (Data != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = Data;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }

            return objWraper;

        }
        [HttpPost]
        public CommonApiWraperModel GetAttandanceStatus(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                List<object> List = objTeacherData.GetAttandanceStatus(user.UserID).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel UpdateAttandance(AppAttandanceModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.EmployeeID = user.UserID.ToString();
                TeacherData objTeacherData = new TeacherData();
                List<object> List = objTeacherData.UpdateAppAttandance(data).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public async Task<CommonApiWraperModel> GetImportantContactsAsync(ParentApiParamModel data)
        {
            int SBranchID = VerifyUser(data.UUID).SBranchID;
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (SBranchID != 0)
            {
                AdminData objAdminData = new AdminData();
                List<object> List = (await objAdminData.GetImportantContact(SBranchID)).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        #region 

        #region BlackBoard
        [HttpPost]
        public CommonApiWraperModel GetBlackBoardSelectionList(TeacherBlackBoardPageModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                data.TeacherID = user.UserID;
                if (data.EntryDate.Year == 1)
                {
                    data.EntryDate = CommonUsage.GetCurrentDate();
                }
                data = objTeacherData.GetBlackBoardPageModel(data);
                if (data != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = data;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetBlackBoardEntriesList(TeacherBlackBoardPageModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                data.TeacherID = user.UserID;
                if (data.EntryDate.Year == 1)
                {
                    data.EntryDate = CommonUsage.GetCurrentDate();
                }
                data = objTeacherData.GetBlackBoardListModel(data);
                data.ImageBaseURL = "/images/BlackBoard/";
                if (data != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = data;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public async Task<CommonApiWraperModel> GetBlackBoardDetails(BlackBoardModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                data.TeacherID = user.UserID;

                data = await objTeacherData.GetBlackBoardDetail(data.BlackBoardID);
                data.ImageBaseURL = "/images/BlackBoard/";
                if (data != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = data;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel DeleteBlackBoardEntry(BlackBoardModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();

                int Data = objTeacherData.DeleteBlackBoardEntry(data.BlackBoardID);
                if (Data > 0)
                {
                    objWraper.Code = 200;
                    objWraper.Data = data.BlackBoardID;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel DeleteBlackBoardImage(BlackBoardImageModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();

                int Data = objTeacherData.DeleteBlackBoardImage(data.BBMIID);
                if (Data > 0)
                {
                    objWraper.Code = 200;
                    objWraper.Data = data.BlackBoardID;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel AddBlackBoardEntry(BlackBoardModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                if (data.BDate.Year == 1)
                {
                    data.BDate = CommonUsage.GetCurrentDate();
                }
                data.BlackBoardID = Guid.NewGuid().ToString();
                data.TeacherID = user.UserID;
                data.SBranchID = user.SBranchID;
                int Data = objTeacherData.AddBlackBoardEntry(data);
                if (Data > 0)
                {
                    objWraper.Code = 200;
                    objWraper.Data = data.BlackBoardID;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel AddBlackBoardImage(BlackBoardImageModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                data.Photo = CommonUsage.GetValidFileName(data.Photo);
                data.BBMIID = Guid.NewGuid().ToString();
                int Data = objTeacherData.AddBlackBoardImage(data);
                if (Data > 0)
                {
                    objWraper.Code = 200;
                    objWraper.Data = data.BBMIID;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        #endregion
        #region YouTubeVideo
        [HttpPost]
        public CommonApiWraperModel GetYouTubeVideos(YouTubeVideoModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                CommonData objTeacherData = new CommonData();
                data.TeacherID = user.UserID;
                List<object> List = objTeacherData.GetYoutubeVideos(user.SBranchID, data.ClassID, data.SectionID, data.SubjectID, data.TeacherID).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel AddYouTubeVideos(YouTubeVideoModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.SBranchID = user.SBranchID;
                data.TeacherID = user.UserID;
                data.UploadDate = CommonUsage.GetCurrentDate();
                CommonData objTeacherData = new CommonData();
                int Data = objTeacherData.AddYoutubeVideo(data);
                if (Data > 0)
                {
                    objWraper.Code = 200;
                    objWraper.Data = Data;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel DeleteYouTubeVideo(YouTubeVideoModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.SBranchID = user.SBranchID;
                data.TeacherID = user.UserID;
                data.UploadDate = CommonUsage.GetCurrentDate();
                CommonData objTeacherData = new CommonData();
                int Data = objTeacherData.DeleteYoutubeVideo(data);
                if (Data > 0)
                {
                    objWraper.Code = 200;
                    objWraper.Data = Data;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        #endregion
        #region Online Class Related
        [HttpPost]
        public CommonApiWraperModel GetTeacherOnlineClassesSections(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                List<object> List = objTeacherData.GetTeacherTeachingClassSections(user.UserID).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetTeacherSubjectsOnlineClassSection(TeacherAttandanceParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                TeacherData objTeacherData = new TeacherData();
                List<object> List = objTeacherData.GetTeacherTeachingSubjectsOnClassSections(user.UserID, data.ClassID, data.SectionID).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetBBBOnlineClassSchedules(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.SBranchID = user.SBranchID;
                if (data.RDate.Year == 1)
                {
                    data.RDate = CommonUsage.GetCurrentDate();
                }
                //OnlineClassData OnlineClassData = new OnlineClassData();
                //objWraper.Data = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
                var Data = (new BBBOnlineClassData()).GetOnlineClassesList(user.UserID, data.RDate, data.SBranchID);
                if (Data.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = Data.ToList<object>();
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetOnlineClassSchedules(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.SBranchID = user.SBranchID;
                if(data.RDate.Year==1)
                {
                    data.RDate = CommonUsage.GetCurrentDate();
                }
                OnlineClassData objTeacherData = new OnlineClassData();
                objWraper.Data = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
                List<OnlineClassModel> Data = objTeacherData.GetOnlineClassesSchedules(user.UserID, data.RDate);
                if (Data.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = Data.ToList<object>();
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetOnlineMeetings(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.SBranchID = user.SBranchID;
                var CurDate = CommonUsage.GetCurrentDate();
                OnlineClassData objTeacherData = new OnlineClassData();
                objWraper.Data = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
                List<OnlineStaffMeetingModel> Data = objTeacherData.GetOnlineStaffMeetings(user.SBranchID, CurDate);
                if (Data.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = Data.ToList<object>();
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetBBBOnlineMeetings(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.SBranchID = user.SBranchID;
                var CurDate = CommonUsage.GetCurrentDate();
                BBBOnlineStaffMeetingData objTeacherData = new BBBOnlineStaffMeetingData();
                List<OnlineStaffMeetingModel> Data = objTeacherData.GetOnlineStaffMeetings(CurDate, user.SBranchID);
                if (Data.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = Data.ToList<object>();
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel ScheduleBBBOnlineClass(BBBOnlineClassModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.TeacherID = user.UserID;
                data.SBranchID = user.SBranchID;
                data.CreatedDate = CommonUsage.GetCurrentDate();
                data.MeetingID = Guid.NewGuid().ToString();
                int res = (new BBBOnlineClassData()).ScheduleOnlineClass(data);
                NameIDModel List = new NameIDModel
                {
                    ID = res,
                    Name = data.MeetingID
                };
                if (List.ID != 0)
                {
                    objWraper.Code = 200;
                    objWraper.Data = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel ScheduleOnlineClass(OnlineClassModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.TeacherID = user.UserID;
                data.CreatedDate = CommonUsage.GetCurrentDate();
                OnlineClassData objTeacherData = new OnlineClassData();
                NameIDModel List = objTeacherData.ScheduleOnlineClass(data);
                if (List != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public async Task<CommonApiWraperModel> StartBBBOnlineClass(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                string basep = $"{ this.Request.RequestUri.Scheme}://{this.Request.RequestUri.Host}";
                if(basep.Contains("localhost"))
                {
                    basep = "https://node1.pschoolonline.com";
                }
                var onlineClassData = (new BBBOnlineClassData());
                EmployeeModel teacher = await onlineClassData.GetTeacherForOnlineClassByID(user.UserID);
                BBBOnlineClassModel cModel = onlineClassData.GetOnlineClassDetailsByOCID(data.ID);
                string basepath = basep;// $"{ this.Request.RequestUri.Scheme}://{this.Request.RequestUri.Host}";
                string logo = HttpUtility.UrlEncode(basepath + "/Images/SBranchLogo/" + user.SBranchID + "_" + user.BranchLogo);
                var setupOk = await isBigBlueButtonAPISettingsOKAsync();
                var meetingStatus = await this.client.GetMeetingInfoAsync(new GetMeetingInfoRequest { meetingID = cModel.MeetingID });
                DateTime startdatetime = DateTime.Parse("2021-02-01 "+ cModel.StartTime);
                DateTime enddatetime = DateTime.Parse("2021-02-01 " + cModel.EndTime);
                int duration = (enddatetime - startdatetime).Minutes; 
                
                if (duration < 0)
                {
                    duration = 30;
                }
                else if (duration > 60)
                {
                    duration = 60;
                }
                if (meetingStatus.returncode == Returncode.FAILED)
                {
                    MetaData meta = new MetaData();
                    meta.Add("BranchID", "1");
<<<<<<< HEAD
                    //string meu = basepath+ "/Home/EndOnlineClasses";
                    string meu = basepath + "/Home/EndOnlineClasses";
=======
                    string meu = basepath+ "/Home/EndOnlineClasses";
>>>>>>> master
                    meta.Add("endCallbackUrl", meu);

                    string reccbu = basepath + "/home/bbbrecordingready/";
                    meta.Add("bbb-recording-ready-url", reccbu);
                    meta.Add("bbb_skip_check_audio", "true");
                    meta.Add("bbb_client_title", "P-School");
                    meta.Add("bbb_enable_screen_sharing", "false");
                    meta.Add("bbb_show_public_chat_on_login", "false");
                    var result = await client.CreateMeetingAsync(new CreateMeetingRequest
                    {
                        name = cModel.SubjectName + " (" + cModel.ClassSection + "), by " + cModel.TeacherName + " on " + cModel.ClassDate.ToString("dd MMM, yyyy"),
                        meetingID = cModel.MeetingID,
                        record = true,

                        logoutURL = basepath + "/Home/ClassEnded/" + cModel.MeetingID,
                       // logoutURL = basepath + "/Home/EndOnlineClasses/" + cModel.MeetingID,
                        
                        //logoutURL = basepath + "/Home/LogoutOnlineClasses/" + cModel.MeetingID,
                        meta = meta,
                        guestPolicy = "ALWAYS_ACCEPT",
                        logo = logo,
                        lockSettingsDisablePrivateChat = true,
                        lockSettingsDisableNote = false,
                        muteOnStart = true,
                        allowModsToUnmuteUsers = true,
                        autoStartRecording = true,
                        //duration = duration + 5
                        //welcome="Welcome to class"
                        //autoStartRecording = true,
                        //bannerText = "Online Class for Subject:" + cModel.SubjectName + " Class:" + cModel.ClassSection + " By :" + cModel.TeacherName
                    }); ;
                    if (result.returncode == Returncode.FAILED) objWraper.Data = "BB01";
                    if (result.returncode != Returncode.FAILED)
                    {
                        cModel.InternalMeetingID = result.internalMeetingID;
                        cModel.Status = 1;
                        cModel.StartedOn = CommonUsage.GetCurrentDate();
                        cModel.AttPassword = result.attendeePW;
                        cModel.ModPassword = result.moderatorPW;
                        (new BBBOnlineClassData()).UpdateBBBOnlineClassDetails(cModel);
                    }
                }

                var requestJoin = new JoinMeetingRequest { meetingID = cModel.MeetingID };
                requestJoin.userID = teacher.EmployeeID.ToString();
                requestJoin.fullName = teacher.EmployeeName;
                requestJoin.password = cModel.ModPassword;
                var setConfigRequest = new SetConfigXMLRequest
                {
                    meetingID = cModel.MeetingID,
                    configXML = "<config><modules><localeversion supressWarning=\"false\">0.9.0</localeversion></modules></config>"
                };
                var setConfigResult = await client.SetConfigXMLAsync(setConfigRequest);
                if (setConfigResult.returncode == Returncode.FAILED)
                {
                    objWraper.Data = setConfigResult;
                }
                else
                {
                    requestJoin.configToken = setConfigResult.configToken;
                    //requestJoin.avatarURL = avatar;
                    var url = client.GetJoinMeetingUrl(requestJoin);

                    List<SMSRecieverModel> recievers = (new BBBOnlineClassData()).GetRecieverListOnOnlineClass(data.ID);
                    OnlineClassNotificationModel message = new OnlineClassNotificationModel();
                    //message.base_url = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
                    message.message = "Online Class Started.";
                    message.type = 1;
                    message.typeid = data.ID;
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

                    objWraper.Code = 200;
                    objWraper.Data = url;
                }
            }
            else
            {
                objWraper.Code = 404;
            }
            objWraper.Message = "Success";
            return objWraper;
        }
        [HttpPost]
        public async Task<CommonApiWraperModel> EndBBBOnlineClass(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {

                BBBOnlineClassModel cModel = (new BBBOnlineClassData()).GetOnlineClassDetailsByOCID(data.ID);
                var result = await client.EndMeetingAsync(new EndMeetingRequest
                {
                    meetingID = cModel.MeetingID,
                    password = cModel.ModPassword
                });
                int List = (new BBBOnlineClassData()).EndOnlineClass(cModel);
                if (List > 0)
                {
                    List<SMSRecieverModel> recievers = (new BBBOnlineClassData()).GetRecieverListOnOnlineClass(data.ID);
                    OnlineClassNotificationModel message = new OnlineClassNotificationModel();
                    message.message = "Online Class Ended.";
                    message.type = 2;
                    message.typeid = data.ID;
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

                    objWraper.Code = 200;
                    objWraper.Data = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel StartOnlineClass(OnlineClassModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.TeacherID = user.UserID;
                data.StartedOn = CommonUsage.GetCurrentDate();
                OnlineClassData objTeacherData = new OnlineClassData();
                int List = objTeacherData.StartOnlineClass(data);
                if (List > 0)
                {
                    List<SMSRecieverModel> recievers = objTeacherData.GetRecieverListOnOnlineClass(data.OCID);
                    OnlineClassNotificationModel message = new OnlineClassNotificationModel();
                    message.base_url = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
                    message.message = "Online Class Started.";
                    message.type = 1;
                    message.typeid = data.OCID;
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

                    objWraper.Code = 200;
                    objWraper.Data = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetOnlineClassAttendees(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                BBBOnlineClassData objTeacherData = new BBBOnlineClassData();
                var List = objTeacherData.GetOnlineClassAttendees(data.OCID);
                if (List.Count() > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List.ToList<object>();
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetBBBOnlineClassAttendees(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                BBBOnlineClassData objTeacherData = new BBBOnlineClassData();
               var List = objTeacherData.GetOnlineClassAttendees(data.OCID);
                if (List.Count() > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = List.ToList<object>();
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public async Task<CommonApiWraperModel> JoinBBBOnlineMeeting(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                BBBOnlineStaffMeetingData onlineClassData = new BBBOnlineStaffMeetingData();
                //var Student = await onlineClassData.GetStudentForOnlineClassByMeetinID(data.OCID,user.UserID);
                var cModel = (onlineClassData).GetOnlineStaffMeetingDetails(data.OCID,user.SBranchID);

                var meetingStatus = await client.GetMeetingInfoAsync(new GetMeetingInfoRequest { meetingID = cModel.BBBMeetingID });
                if (meetingStatus.returncode != Returncode.FAILED)
                {

                    var joinDate = CommonUsage.GetCurrentDate();
                    NameIDModel student = await onlineClassData.JoinOnlineStaffMeetingGetDetail(user.UserID, joinDate, data.OCID, 1);
                    var requestJoin = new JoinMeetingRequest { meetingID = cModel.BBBMeetingID };
                    requestJoin.userID = student.ID.ToString();
                    requestJoin.fullName = student.Name;
                    requestJoin.password = cModel.AttPassword;
                    var setConfigRequest = new SetConfigXMLRequest
                    {
                        meetingID = cModel.BBBMeetingID,
                        configXML = "<config><modules><localeversion supressWarning=\"false\">0.9.0</localeversion></modules></config>"
                    };
                    var setConfigResult = await client.SetConfigXMLAsync(setConfigRequest);
                    if (setConfigResult.returncode == Returncode.FAILED)
                    {
                        objWraper.Code = -1;
                    }
                    else
                    {
                        requestJoin.configToken = setConfigResult.configToken;
                        // requestJoin.avatarURL = avatar;
                        var url = client.GetJoinMeetingUrl(requestJoin);
                        objWraper.Data = url;
                    }
                }

                if (objWraper.Data != null)
                {
                    objWraper.Code = 200;
                    objWraper.Message = "Preparing to join class";
                }
                else
                {
                    objWraper.Code = 404;
                    objWraper.Message = "Class is not running now";
                }
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        //[HttpPost]
        //public CommonApiWraperModel GetOnlineClassAttendees(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        OnlineClassData objTeacherData = new OnlineClassData();
        //        List<OnlineClassAttendeeModel> List = objTeacherData.GetOnlineClassAttendees(data.OCID);
        //        if (List.Count() > 0)
        //        {
        //            objWraper.Code = 200;
        //            objWraper.List = List.ToList<object>();
        //        }
        //        else
        //        {
        //            objWraper.Code = 404;
        //        }
        //        objWraper.Message = "Success";
        //    }
        //    else
        //    {
        //        objWraper.Code = 101;
        //        objWraper.Message = "Unauthorized";
        //    }
        //    return objWraper;
        //}
        [HttpPost]
        public CommonApiWraperModel EndOnlineClass(OnlineClassModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.TeacherID = user.UserID;
                data.EndedOn = CommonUsage.GetCurrentDate();
                OnlineClassData objTeacherData = new OnlineClassData();
                int List = objTeacherData.EndOnlineClass(data);
                if (List > 0)
                {
                    List<SMSRecieverModel> recievers = objTeacherData.GetRecieverListOnOnlineClass(data.OCID);
                    OnlineClassNotificationModel message = new OnlineClassNotificationModel();
                    message.message = "Online Class Ended.";
                    message.type = 2;
                    message.typeid = data.OCID;
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

                    objWraper.Code = 200;
                    objWraper.Data = List;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel JoinOnlineClass(YouTubeVideoModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            objWraper.Code = 200;
            objWraper.Data = 1;

            return objWraper;
        }
        #endregion
        //[HttpPost]
        //public CommonApiWraperModel GetGoogleAccessToken(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null || user == null)
        //    {
        //        var path = HttpRuntime.AppDomainAppPath;

        //        string keypath = System.IO.Path.Combine(path, "googleApiKey.json"); ;

        //        string AccessToken = GetAccessTokenFromJSONKey(keypath, "https://www.googleapis.com/auth/userinfo.profile");

        //        if(AccessToken !=null)
        //        {
        //            objWraper.Code = 200;
        //            objWraper.Data = AccessToken;
        //        }
        //        else
        //        {
        //            objWraper.Code = 404;
        //        }
        //        objWraper.Message = "Success";
        //    }
        //    else
        //    {
        //        objWraper.Code = 101;
        //        objWraper.Message = "Unauthorized";
        //    }
        //    return objWraper;
        //}
        #endregion

        ///// <summary>  
        ///// Get Access Token From JSON Key  
        ///// </summary>  
        ///// <param name="jsonKeyFilePath">Path to your JSON Key file</param>  
        ///// <param name="scopes">Scopes required in access token</param>  
        ///// <returns>Access token as string</returns>  
        //public static string GetAccessTokenFromJSONKey(string jsonKeyFilePath, params string[] scopes)
        //{
        //    return GetAccessTokenFromJSONKeyAsync(jsonKeyFilePath, scopes).Result;
        //}

        //public static async Task<string> GetAccessTokenFromJSONKeyAsync(string jsonKeyFilePath, params string[] scopes)
        //{
        //    using (var stream = new FileStream(jsonKeyFilePath, FileMode.Open, FileAccess.Read))
        //    {
        //        return await GoogleCredential
        //            .FromStream(stream) // Loads key file  
        //            .CreateScoped(scopes) // Gathers scopes requested  
        //            .UnderlyingCredential // Gets the credentials  
        //            .GetAccessTokenForRequestAsync(); // Gets the Access Token  
        //    }
        //}
    }

}