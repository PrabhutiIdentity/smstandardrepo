using SMEnterprise.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMEnterprise.Filters;
using SMEnterprise.Models;
using System.IO;
using System.Threading.Tasks;
using BigBlueButtonAPI.Core;
using BigBlueButtonAPI.Common;

namespace SMEnterprise.Controllers
{

    public class TeacherController : Controller
    {
      //  private readonly BigBlueButtonAPIClient client;
        public TeacherController()
        {
           // this.client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
        }
<<<<<<< HEAD
=======
<<<<<<< HEAD
        private async Task<bool> isBigBlueButtonAPISettingsOKAsync()
        {
            try
            {
                var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
                var res = await client.IsMeetingRunningAsync(new IsMeetingRunningRequest { meetingID = Guid.NewGuid().ToString() });
                if (res.returncode == Returncode.FAILED) return false;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
=======
>>>>>>> master
        //private async Task<bool> isBigBlueButtonAPISettingsOKAsync()
        //{
        //    try
        //    {
        //        var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
        //        var res = await client.IsMeetingRunningAsync(new IsMeetingRunningRequest { meetingID = Guid.NewGuid().ToString() });
        //        if (res.returncode == Returncode.FAILED) return false;
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}
<<<<<<< HEAD
=======
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
>>>>>>> master
        TeacherData objTeacherData = new TeacherData();
        // GET: Home
        [PermissionFilter]
        public ActionResult Dashboard()
        {
            return View();
        }
        [PermissionFilter]
        public ActionResult ChangePassword()
        {
            return View();
        }
        [PermissionFilter]
        public ActionResult TimeTable()
        {
            int TeacherID = PermissionManager.GetLoggedInUser().UserID;
            int BranchID = PermissionManager.GetLoggedInUser().SBranchID;
            DateTime CurrentDate = CommonUsage.GetCurrentDate();
            TimeTablePageModel objModel = objTeacherData.GetTeacherTimeTable(TeacherID, CurrentDate, BranchID);
            return View(objModel);
        }
        #region Calendar
        [PermissionFilter]
        public async Task<ActionResult> Calendar()
        {
            int year = CommonUsage.GetCurrentDate().Year;
            int month = CommonUsage.GetCurrentDate().Month;
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            CommonData objCommonData = new CommonData();
            EventCalendarModel model = await objCommonData.GetEventCalander(month, year, SBranchID, 0);
            return View(model);
        }
        [PermissionFilter]
        public async Task<ActionResult> GetEventCalender(string id = null, string id2 = null)
        {
            int year = CommonUsage.ConvertToInt(id2);
            int month = CommonUsage.ConvertToInt(id);

            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            CommonData objCommonData = new CommonData();
            EventCalendarModel model = await objCommonData.GetEventCalander(month, year, SBranchID, 1);

            return Json(model, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region MailBox
        MessageData objMessageData = new MessageData();
        [PermissionFilter]
        public ActionResult MailBox()
        {
            int ParentID = PermissionManager.GetLoggedInUser().UserID;
            int UserType = PermissionManager.GetLoggedInUser().RoleID;
            MailBoxModel objModel = objMessageData.GetMailBoxModel(UserType, ParentID, 4);
            if (objModel.Mail != null)
            {
                objModel.Mail.CurrUserID = ParentID;
                objModel.Mail.CurrUserType = UserType;
            }
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult SentMail()
        {
            int ParentID = PermissionManager.GetLoggedInUser().UserID;
            int UserType = PermissionManager.GetLoggedInUser().RoleID;
            MailBoxModel objModel = objMessageData.GetSentMailBoxModel(UserType, ParentID, 4);
            if (objModel.Mail != null)
            {
                objModel.Mail.CurrUserID = ParentID;
                objModel.Mail.CurrUserType = UserType;
            }
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GetMessageDetail(string ID = null, string ID2 = null)
        {
            int ParentID = PermissionManager.GetLoggedInUser().UserID;
            int UserType = PermissionManager.GetLoggedInUser().RoleID;
            int MessageID = CommonUsage.ConvertToInt(ID);
            int IsSent = CommonUsage.ConvertToInt(ID2);
            MessageModel model = objMessageData.GetMessageDetails(MessageID, IsSent);
            if (model != null)
            {
                model.CurrUserID = ParentID;
                model.CurrUserType = UserType;
            }
            return PartialView("_MailViewPartial", model);
        }
        [PermissionFilter]
        public ActionResult GetRecieverListOptions(string ID = null)
        {
            int RecieverType = CommonUsage.ConvertToInt(ID);
            int ParentID = PermissionManager.GetLoggedInUser().UserID;
            List<NameIDModel> model = objMessageData.GetRecieverList(RecieverType, ParentID, 3);

            return PartialView("_SelectOptionsPartial", model);
        }
        #endregion
        #region student Details
       
        [PermissionFilter]
        public ActionResult StudentDetails(StudentsPageModel objModel)
        {

            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
           
            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
              objModel = objTeacherData.GetStudentClassReport(objModel);
           // objModel = objAccountData.GetAllStudentReport(SBranchID, objModel.SessionID);
        //    objModel = objTeacherData.GetStudentAttandance(objModel);
            return View(objModel);
        }

        #endregion
        #region Student Attandance
        [PermissionFilter]
        public ActionResult StudentAttandance(StudentAttandancePageModel objModel)
        {

            if (objModel.SelectedDate.Year == 1)
            {
                objModel.SelectedDate = CommonUsage.GetCurrentDate();
            }
            objModel.Month = objModel.SelectedDate.Month;
            objModel.Year = objModel.SelectedDate.Year;

            objModel.Day = objModel.SelectedDate.Day.ToString();
            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objTeacherData.GetStudentAttandance(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateStudentAttandance(StudentAttandancePageModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objTeacherData.UpdateStudentAttandance(objModel);
            objModel.StudentAttandances = null;
            int day = CommonUsage.ConvertToInt(objModel.Day.Replace("D", ""));
            objModel.SelectedDate = new DateTime(objModel.Year, objModel.Month, day);
            return RedirectToAction("StudentAttandance", "Teacher", objModel);
        }
        [PermissionFilter]
        public ActionResult GetTeacherClassSections(string ID = null)
        {
            int ClassID = CommonUsage.ConvertToInt(ID);
            int TeacherID = PermissionManager.GetLoggedInUser().UserID;
            IEnumerable<NameIDModel> model = objTeacherData.GetTeacherClassSections(TeacherID, ClassID);

            return PartialView("_SelectOptionsPartial", model);
        }
        #endregion
        #region Assignment
        [PermissionFilter]
        public ActionResult Assignments(AssignmentPageModel objModel)
        {

            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel = objTeacherData.GetTeacherAssignments(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GetSectionsForTeacherClass(string ID = null)
        {
            int ClassID = CommonUsage.ConvertToInt(ID);
            int TeacherID = PermissionManager.GetLoggedInUser().UserID;
            IEnumerable<NameIDModel> model = objTeacherData.GetSectionsForTeacherClass(TeacherID, ClassID);

            return PartialView("_SelectOptionsPartial", model);
        }
        [PermissionFilter]
        public ActionResult GetSubjectsForTeacherSection(string ID = null)
        {
            int SectionID = CommonUsage.ConvertToInt(ID);
            int TeacherID = PermissionManager.GetLoggedInUser().UserID;
            IEnumerable<NameIDModel> model = objTeacherData.GetSubjectsForTeacherSection(TeacherID, SectionID);

            return PartialView("_SelectOptionsPartial", model);
        }
        [PermissionFilter]
        public ActionResult GetChaptersForSubject(string ID = null, string ID2 = null)
        {
            int ClassID = CommonUsage.ConvertToInt(ID);
            int SubjectID = CommonUsage.ConvertToInt(ID2);
            IEnumerable<NameIDModel> model = objTeacherData.GetChaptersForSubject(ClassID, SubjectID);

            return PartialView("_SelectOptionsPartial", model);
        }
        [PermissionFilter]
        public ActionResult GetChaptersTopics(string ID = null)
        {
            int ChapterID = CommonUsage.ConvertToInt(ID);
            IEnumerable<NameIDModel> model = objTeacherData.GetChaptersTopics(ChapterID);

            return PartialView("_SelectOptionsPartial", model);
        }
        [PermissionFilter]
        public ActionResult GetAssignmentDetails(string ID = null)
        {
            int SectionID = CommonUsage.ConvertToInt(ID);
            int TeacherID = PermissionManager.GetLoggedInUser().UserID;

            return PartialView("_AssignmentDetails");
        }
        [PermissionFilter]
        public ActionResult UpdateAssignment(AssignmentModel objModel)
        {
            objModel.OperationDate = CommonUsage.GetCurrentDate();
            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            int id = objTeacherData.UpdateAssignment(objModel);

            return Json(id, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult UpdateAssignmentAttachment(AssignmentModel objModel)
        {
            objModel.OperationDate = CommonUsage.GetCurrentDate();
            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            int id = objTeacherData.UpdateAssignmentAttachment(objModel.ID.ToString(), objModel.Attachments);

            return Json(id, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult DeleteAssignment(string ID = null)
        {
            AssignmentModel objModel = new AssignmentModel();
            objModel.ID = CommonUsage.ConvertToInt(ID);
            objModel.StartDate = CommonUsage.GetCurrentDate();
            objModel.EndDate = CommonUsage.GetCurrentDate();
            objModel.OpType = -1;
            objModel.OperationDate = CommonUsage.GetCurrentDate();
            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            int id = objTeacherData.UpdateAssignment(objModel);

            var dir = new DirectoryInfo(Server.MapPath(CommonUsage.AssignmentAttachmentBasePath));
            foreach (var file in dir.EnumerateFiles(ID + "_*"))
            {
                file.Delete();
            }
            return Json(id, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult AssignmentSubmissions(string ID = null)
        {
            int AssignmentID = CommonUsage.ConvertToInt(ID);
            AssignmentSubmissionListPage objModel = objTeacherData.GetAssignmentSubmissions(AssignmentID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult AssignmentSubmissionDetail(string ID = null)
        {
            int AssSubmissionID = CommonUsage.ConvertToInt(ID);
            AssignmentSubmissionModel objModel = objTeacherData.GetAssignmentSubmissionDetails(AssSubmissionID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateAssignmentTeacherResponse(AssignmentSubmissionModel objModel)
        {
            int id = objTeacherData.UpdateTeacherAssSubmissionResponse(objModel);

            return Json(id, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Exam Resulst
        [PermissionFilter]
        public ActionResult ExamResults(TeacherResultPageModel objModel)
        {

            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objTeacherData.GetTeacherExamResults(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateTeacherResults(TeacherResultPageModel objModel)
        {
            int id = objTeacherData.UpdateStudentResults(objModel);
            objModel.ExamResults = null;
            return RedirectToAction("ExamResults", "Teacher", objModel);
        }
      

        [PermissionFilter]
        public ActionResult StudentPerformance(StudentPerformanceListModel objModel)
        {

            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objTeacherData.GetStudentsPerformance(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult StudentPerformanceDetails(string ID = null, string ID2 = null)
        {
            PerformanceParameterDetailModel objModel = new PerformanceParameterDetailModel();
            objModel.StudentSessionUID = CommonUsage.ConvertToInt(ID);
            objModel.EvaluationID = CommonUsage.ConvertToInt(ID2);
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objTeacherData.GetStudentPerformanceDetails(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateStudentPerformance(PerformanceParameterDetailModel objModel)
        {
            int id = objTeacherData.UpdateStudentPerformance(objModel);
            objModel.PerformanceParameters = null;
            System.Web.Routing.RouteValueDictionary route = new System.Web.Routing.RouteValueDictionary();
            route.Add("ID", objModel.StudentSessionUID);
            route.Add("ID2", objModel.EvaluationID);
            return RedirectToAction("StudentPerformanceDetails", "Teacher", route);
        }
        [PermissionFilter]
        public ActionResult StudentResultDetails(string ID = null, string ID2 = null, string ID3 = null)
        {
            PerformanceParameterDetailModel objModel = new PerformanceParameterDetailModel();
            objModel.StudentSessionUID = CommonUsage.ConvertToInt(ID);
            objModel.EvaluationID = CommonUsage.ConvertToInt(ID2);
            objModel.SessionID = CommonUsage.ConvertToInt(ID3);
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objTeacherData.GetStudentResultDetails(objModel);
            return View(objModel);
        }
        #endregion
        #region Parent Diary
        [PermissionFilter]
        public ActionResult SubjectParentDiary(ParentDiaryModel objModel)
        {

            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objTeacherData.GetStudentParentDiarySubject(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ClassParentDiary(ParentDiaryModel objModel)
        {

            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objTeacherData.GetStudentParentDiaryClass(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult InsertStudentDiary(ParentDiaryModel objModel)
        {
            objModel.PBDate = CommonUsage.GetCurrentDate();
            int id = objTeacherData.UpdateParentDiaryBulk(objModel);
            System.Web.Routing.RouteValueDictionary route = new System.Web.Routing.RouteValueDictionary();
            if (objModel.TeacherType == 0)
            {
                return RedirectToAction("ClassParentDiary", "Teacher", objModel);
            }
            else
            {
                return RedirectToAction("SubjectParentDiary", "Teacher", objModel);
            }
        }
        #endregion
        #region YouTube and BlackBoard
        [PermissionFilter]
        public ActionResult YouTubeVideos(YoutubeAdminEditPageData objModel)
        {

            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objTeacherData.GetTeacherYouTubeVideos(objModel);
            return View(objModel);
        }
        [HttpPost]
        public ActionResult GetSectionSubjectsStudents(string ID = null, string ID2 = null, string ID3 = null, string ID4 = null)
        {
            int SectionID = CommonUsage.ConvertToInt(ID);
            int SessionID = CommonUsage.ConvertToInt(ID2);
            int ClassID = CommonUsage.ConvertToInt(ID4);
            int TeacherID = PermissionManager.GetLoggedInUser().UserID;
            int VideoID = CommonUsage.ConvertToInt(ID3);
            YoutubeAdminEditPageData oModel = objTeacherData.GetTeacherSectionSubjectAndStudents(SectionID, SessionID, VideoID, TeacherID, ClassID);
            return Json(oModel, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult EditYouTubeVideo(string ID = null)
        {
            int TeacherID = PermissionManager.GetLoggedInUser().UserID;
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int VideoID = CommonUsage.ConvertToInt(ID);

            YoutubeAdminEditPageData objModel = objTeacherData.GetYouTubeVideosDetails(SBranchID, VideoID, TeacherID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateYoutubeVideo(YoutubeAdminEditPageData objModel)
        {
            objModel.Video.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel.Video.UploadDate = CommonUsage.GetCurrentDate();
            //objModel.RequestDate = CommonUsage.GetCurrentDate();
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel.Video.VideoID = (new AdminData()).UpdateYoutubeVideo(objModel);
            return Redirect("~/Teacher/EditYouTubeVideo/" + objModel.Video.VideoID);
        }
        #endregion
        #region Question Bank
        [PermissionFilter]

        public ActionResult QuestionBank(TeacherQuestionBankModel objModel)
        {
            if (TempData["QuestionBank"] != null)
            {
                objModel = (TeacherQuestionBankModel)TempData["QuestionBank"];
            }
            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            ViewBag.UserID = objModel.TeacherID;
            objTeacherData.GetTeacherQuestionBank(objModel);
            return View(objModel);
        }
        [PermissionFilter]

        public ActionResult UpdateQuestionBank(QuestionBankModel objModel)
        {
            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel.CreatedDate = CommonUsage.GetCurrentDate();
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int QuestionID = objTeacherData.UpdateQuestionBank(objModel);
            TeacherQuestionBankModel objFD = new TeacherQuestionBankModel();
            objFD.QuestionsBy = objModel.QuestionsBy;
            objFD.ClassID = objModel.ClassID;
            TempData["QuestionBank"] = objFD;
            return RedirectToAction("QuestionBank");
        }
        [PermissionFilter]
        public ActionResult OnlineExams(OnlineExamPageModel objModel)
        {
            if (TempData["OnlineExams"] != null)
            {
                objModel = (OnlineExamPageModel)TempData["OnlineExams"];
            }
            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objTeacherData.GetTeacherOnlineExams(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult DeleteOnlineExam(OnlineExamModel objModel)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objTeacherData.DeleteOnlineExam(objModel.OExamID, SBranchID);
            OnlineExamPageModel objFD = new OnlineExamPageModel();
            TempData["OnlineExams"] = objFD;
            return RedirectToAction("OnlineExams");
        }
        [PermissionFilter]
        public ActionResult OnlineExamDetails(OnlineExamModel objModel)
        {
            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            OnlineExamEditModel oData = objTeacherData.GetTeacherOnlineExamDetails(objModel);
            return View(oData);
        }
        public ActionResult UpdateOnlineExam(OnlineExamModel objModel)
        {
            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel.CreatedDate = CommonUsage.GetCurrentDate();
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int QuestionID = objTeacherData.UpdateOnlineExam(objModel);
            OnlineExamPageModel objFD = new OnlineExamPageModel();
            objFD.SectionID = objModel.SectionID;
            objFD.ClassID = objModel.ClassID;
            objFD.SectionID = objModel.SectionID;
            objFD.SubjectID = objModel.SubjectID;
            TempData["OnlineExams"] = objFD;
            return RedirectToAction("OnlineExams");
        }
        [PermissionFilter]
        public ActionResult GetOnlineExamSubmissions(string ID = null)
        {

            StudentOnlineExamSubmissionPageModel oModel = objTeacherData.GetOnlineExamSubmissions(ID);

            return PartialView("_ViewOnlineExamSubmissions", oModel);
        }
        [PermissionFilter]
        public ActionResult ViewAnswerSheet(string ID = null)
        {
            int SubmissionID = CommonUsage.ConvertToInt(ID);
            OnlineExamEditModel objModel = objTeacherData.GetStudentOnlineExamAnswerSheet(SubmissionID);
            return View(objModel);
        }
        public ActionResult UpdateOnlineExamSubmissionStatus(StudentOnlineExamSubmitModel objModel)
        {
            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel.CheckDate = CommonUsage.GetCurrentDate();
            int QuestionID = objTeacherData.UpdateOnlineExamSubmissionStatus(objModel);
            OnlineExamPageModel objFD = new OnlineExamPageModel();

            return RedirectToAction("OnlineExams");
        }
        #endregion

        [PermissionFilter]
        public ActionResult Question()
        {
            return View();
        }
<<<<<<< HEAD
=======
<<<<<<< HEAD
        [PermissionFilter]
        [HttpPost]
        public ActionResult PlayRecording(string url)
        {
            if (url == null)
            {
                return RedirectToAction("OnlineClasses");
            }
            ViewBag.URL = url;
            return View();
        }
        [PermissionFilter]
        public ActionResult OnlineClasses(BBBOnlineClassListModel oModel)
        {
            if (TempData["OnlineClassState"] != null)
            {
                BBBOnlineClassListModel tmp = (BBBOnlineClassListModel)TempData["OnlineClassState"];
                oModel.ClassDate = tmp.ClassDate;
                TempData["OnlineClassState"] = null;
            }
            if (oModel.ClassDate.Year == 1)
            {
                oModel.ClassDate = CommonUsage.GetCurrentDate();
            }
            var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
            int TeacherID = PermissionManager.GetLoggedInUser().UserID;
            int BranchID = PermissionManager.GetLoggedInUser().SBranchID;
            oModel = (new BBBOnlineClassData()).GetOnlineClassesPageData(TeacherID, oModel.ClassDate, BranchID);
            //oModel.OnlineClassURL = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == BranchID).FirstOrDefault().Name;
            return View(oModel);
        }
        [PermissionFilter]
        public ActionResult UpdateOnlineClass(BBBOnlineClassModel objModel)
        {
            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel.CreatedDate = CommonUsage.GetCurrentDate();
            objModel.MeetingID = Guid.NewGuid().ToString();
            //objModel.RequestDate = CommonUsage.GetCurrentDate();
            int Res = (new BBBOnlineClassData()).ScheduleOnlineClass(objModel);


            BBBOnlineClassListModel mdl = new BBBOnlineClassListModel();
            mdl.ClassDate = objModel.ClassDate;

            TempData["OnlineClassState"] = mdl;
            TempData["OnlineClassDetails"] = Res;
            TempData["Message"] = "Online Class Scheduled Successfully";

            return Redirect("~/Teacher/OnlineClasses/");
        }
        [PermissionFilter]
        public async Task<ActionResult> CreateAndStartClass(string ID = null)
        {
            var user = PermissionManager.GetLoggedInUser();
            int OCID = CommonUsage.ConvertToInt(ID);
            BBBOnlineClassModel cModel = (new BBBOnlineClassData()).GetOnlineClassDetailsByOCID(OCID);
            string basep = $"{ this.Request.Url.Scheme}://{this.Request.Url.Host}";
            if (basep.Contains("localhost"))
            {
                basep = "https://node1.pschoolonline.com";
            }

            string basepath = basep;

            string logo = basepath + "/Images/SBranchLogo/" + user.SBranchID + "_" + user.BranchLogo;
            string avatar = basepath + "/Images/EmployeeImage/" + user.UserID + "_" + user.UserImage;
            MetaData meta = new MetaData();
            meta.Add("BranchID", "1");
            meta.Add("meta_endCallbackUrl", basepath + "/Home/meetingEnded");
            meta.Add("meta_bbb-recording-ready-url", basepath + "/Home/redordingavailable");
            meta.Add("meta_bbb_skip_check_audio", "true");
            meta.Add("meta_bbb_client_title", "P-School");
            meta.Add("meta_bbb_enable_screen_sharing", "false");
            meta.Add("meta_bbb_show_public_chat_on_login", "false");
            //meta.Add("meta_bbb-recording-ready-url", "URL");
            var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
            var result = await client.CreateMeetingAsync(new CreateMeetingRequest
            {
                name = cModel.SubjectName + " (" + cModel.ClassSection + "), by " + cModel.TeacherName,
                meetingID = cModel.MeetingID,
                record = true,
                logoutURL = "https://identitybbb.requestcatcher.com/",
                meta = meta,
                logo = logo,
                allowModsToUnmuteUsers = true,
                //lockSettingsDisableCam=true,
                lockSettingsDisablePrivateChat = true,
                lockSettingsLockOnJoin = true,
                //lockSettingsDisableMic=true,
                //autoStartRecording = true,
                //bannerText = "Online Class for Subject:" + cModel.SubjectName + " Class:" + cModel.ClassSection + " By :" + cModel.TeacherName
            });
            if (result.returncode == Returncode.FAILED && result.messageKey != "idNotUnique") return View("Error", result);
            if (result.returncode != Returncode.FAILED)
            {
                cModel.InternalMeetingID = result.internalMeetingID;
                cModel.Status = 1;
                cModel.StartedOn = CommonUsage.GetCurrentDate();
                cModel.AttPassword = result.attendeePW;
                cModel.ModPassword = result.moderatorPW;
                cModel.InternalMeetingID = result.internalMeetingID;
                (new BBBOnlineClassData()).UpdateBBBOnlineClassDetails(cModel);
            }

            //var requestJoin = new JoinMeetingRequest { meetingID = cModel.MeetingID };
            //requestJoin.userID = user.UserID.ToString();
            //requestJoin.fullName = user.FullName;
            //requestJoin.password = cModel.ModPassword;
            ////requestJoin.avatarURL = avatar;
            //var url = client.GetJoinMeetingUrl(requestJoin);
            return Json(1);
            //return Redirect("~/Teacher/OnlineClasses/");
        }
        [PermissionFilter]
        public async Task<ActionResult> JoinClass(string ID = null)
        {
            var user = PermissionManager.GetLoggedInUser();
            int OCID = CommonUsage.ConvertToInt(ID);
            BBBOnlineClassModel cModel = (new BBBOnlineClassData()).GetOnlineClassDetailsByOCID(OCID);
            string basep = $"{this.Request.Url.Scheme}://{this.Request.Url.Host}";

            DateTime startdatetime = DateTime.Parse("2021-02-01 " + cModel.StartTime);
            DateTime enddatetime = DateTime.Parse("2021-02-01 " + cModel.EndTime);
            int duration = (enddatetime - startdatetime).Minutes;
            if(duration<0)
            {
                duration = 30;
            }
            else if (duration > 60)
            {
                duration = 60;
            }
            if (basep.Contains("localhost"))
            {
                basep = "https://node1.pschoolonline.com";
            }
            string basepath = basep;
            string logo = basepath + "/Images/SBranchLogo/" + user.SBranchID + "_" + user.BranchLogo;
            string avatar = basepath + "/Images/EmployeeImage/" + user.UserID + "_" + user.UserImage;
            var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
            var meetingStatus = await client.GetMeetingInfoAsync(new GetMeetingInfoRequest { meetingID = cModel.MeetingID });
            if (meetingStatus.returncode == Returncode.FAILED)
            {
                MetaData meta = new MetaData();
                meta.Add("BranchID", "1");
                string meu = basepath + "/Home/EndOnlineClasses";
                meta.Add("endCallbackUrl", meu);

                string reccbu = basepath + "/home/bbbrecordingready/";
                meta.Add("bbb-recording-ready-url", reccbu);
                meta.Add("bbb-skip-check-audio", "true");
                meta.Add("bbb_client_title", "P-School");
                meta.Add("bbb_enable_screen_sharing", "false");
                meta.Add("bbb_show_public_chat_on_login", "false");
                var result = await client.CreateMeetingAsync(new CreateMeetingRequest
                {
                    name = cModel.SubjectName + " (" + cModel.ClassSection + "), by " + cModel.TeacherName + " on " + cModel.ClassDate.ToString("dd MMM, yyyy"),
                    meetingID = cModel.MeetingID,
                    record = true,
                    logoutURL = basepath + "/Home/ClassEnded/" + cModel.MeetingID,
                    meta = meta,
                    guestPolicy = "ALWAYS_ACCEPT",
                    logo = logo,
                    lockSettingsDisablePrivateChat = true,
                    lockSettingsDisableNote = false,
                    muteOnStart = true,
                    allowModsToUnmuteUsers = true,
                    autoStartRecording = true,
                    duration = duration + 5
                    //autoStartRecording = true,
                    //bannerText = "Online Class for Subject:" + cModel.SubjectName + " Class:" + cModel.ClassSection + " By :" + cModel.TeacherName
                });
                if (result.returncode == Returncode.FAILED) return View("Error", result);
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
            if (setConfigResult.returncode == Returncode.FAILED) return View("Error", setConfigResult);
            requestJoin.configToken = setConfigResult.configToken;
            requestJoin.avatarURL = avatar;
            var url = client.GetJoinMeetingUrl(requestJoin);
            ViewBag.URL = url;
            return View();
        }
        public async Task<ActionResult> EndClass(string ID = null)
        {
            int OCID = CommonUsage.ConvertToInt(ID);
            BBBOnlineClassModel cModel = (new BBBOnlineClassData()).GetOnlineClassDetailsByOCID(OCID);
            var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
            var result = await client.EndMeetingAsync(new EndMeetingRequest
            {
                meetingID = cModel.MeetingID,
                password = cModel.ModPassword
            });
            if (result.returncode == Returncode.FAILED) return Json(0);
            //cModel.EndedOn = CommonUsage.GetCurrentDate();
            //cModel.Status = 2;
            //cModel.MeetingID = ID;
            //(new BBBOnlineClassData()).EndOnlineClass(cModel);
            return Json(1);
        }
        public ActionResult GetOnlineClassDetails(string ID = null)
        {

            int OCID = CommonUsage.ConvertToInt(ID);
            BBBOnlineClassModel cModel = (new BBBOnlineClassData()).GetOnlineClassDetailsByOCID(OCID);
            return PartialView("_OnlineClassDetailsPartial", cModel);
        }
=======
>>>>>>> master
        //[PermissionFilter]
        //[HttpPost]
        //public ActionResult PlayRecording(string url)
        //{
        //    if (url == null)
        //    {
        //        return RedirectToAction("OnlineClasses");
        //    }
        //    ViewBag.URL = url;
        //    return View();
        //}
        //[PermissionFilter]
        //public ActionResult OnlineClasses(BBBOnlineClassListModel oModel)
        //{
        //    if (TempData["OnlineClassState"] != null)
        //    {
        //        BBBOnlineClassListModel tmp = (BBBOnlineClassListModel)TempData["OnlineClassState"];
        //        oModel.ClassDate = tmp.ClassDate;
        //        TempData["OnlineClassState"] = null;
        //    }
        //    if (oModel.ClassDate.Year == 1)
        //    {
        //        oModel.ClassDate = CommonUsage.GetCurrentDate();
        //    }
        //    var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
        //    int TeacherID = PermissionManager.GetLoggedInUser().UserID;
        //    int BranchID = PermissionManager.GetLoggedInUser().SBranchID;
        //    oModel = (new BBBOnlineClassData()).GetOnlineClassesPageData(TeacherID, oModel.ClassDate, BranchID);
        //    //oModel.OnlineClassURL = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == BranchID).FirstOrDefault().Name;
        //    return View(oModel);
        //}
        //[PermissionFilter]
        //public ActionResult UpdateOnlineClass(BBBOnlineClassModel objModel)
        //{
        //    objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
        //    objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
        //    objModel.CreatedDate = CommonUsage.GetCurrentDate();
        //    objModel.MeetingID = Guid.NewGuid().ToString();
        //    //objModel.RequestDate = CommonUsage.GetCurrentDate();
        //    int Res = (new BBBOnlineClassData()).ScheduleOnlineClass(objModel);


        //    BBBOnlineClassListModel mdl = new BBBOnlineClassListModel();
        //    mdl.ClassDate = objModel.ClassDate;

        //    TempData["OnlineClassState"] = mdl;
        //    TempData["OnlineClassDetails"] = Res;
        //    TempData["Message"] = "Online Class Scheduled Successfully";

        //    return Redirect("~/Teacher/OnlineClasses/");
        //}
        //[PermissionFilter]
        //public async Task<ActionResult> CreateAndStartClass(string ID = null)
        //{
        //    var user = PermissionManager.GetLoggedInUser();
        //    int OCID = CommonUsage.ConvertToInt(ID);
        //    BBBOnlineClassModel cModel = (new BBBOnlineClassData()).GetOnlineClassDetailsByOCID(OCID);
        //    string basep = $"{ this.Request.Url.Scheme}://{this.Request.Url.Host}";
        //    if (basep.Contains("localhost"))
        //    {
        //        basep = "https://node1.pschoolonline.com";
        //    }

        //    string basepath = basep;

        //    string logo = basepath + "/Images/SBranchLogo/" + user.SBranchID + "_" + user.BranchLogo;
        //    string avatar = basepath + "/Images/EmployeeImage/" + user.UserID + "_" + user.UserImage;
        //    MetaData meta = new MetaData();
        //    meta.Add("BranchID", "1");
        //    meta.Add("meta_endCallbackUrl", basepath + "/Home/meetingEnded");
        //    meta.Add("meta_bbb-recording-ready-url", basepath + "/Home/redordingavailable");
        //    meta.Add("meta_bbb_skip_check_audio", "true");
        //    meta.Add("meta_bbb_client_title", "P-School");
        //    meta.Add("meta_bbb_enable_screen_sharing", "false");
        //    meta.Add("meta_bbb_show_public_chat_on_login", "false");
        //    //meta.Add("meta_bbb-recording-ready-url", "URL");
        //    var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
        //    var result = await client.CreateMeetingAsync(new CreateMeetingRequest
        //    {
        //        name = cModel.SubjectName + " (" + cModel.ClassSection + "), by " + cModel.TeacherName,
        //        meetingID = cModel.MeetingID,
        //        record = true,
        //        logoutURL = "https://identitybbb.requestcatcher.com/",
        //        meta = meta,
        //        logo = logo,
        //        allowModsToUnmuteUsers = true,
        //        //lockSettingsDisableCam=true,
        //        lockSettingsDisablePrivateChat = true,
        //        lockSettingsLockOnJoin = true,
        //        //lockSettingsDisableMic=true,
        //        //autoStartRecording = true,
        //        //bannerText = "Online Class for Subject:" + cModel.SubjectName + " Class:" + cModel.ClassSection + " By :" + cModel.TeacherName
        //    });
        //    if (result.returncode == Returncode.FAILED && result.messageKey != "idNotUnique") return View("Error", result);
        //    if (result.returncode != Returncode.FAILED)
        //    {
        //        cModel.InternalMeetingID = result.internalMeetingID;
        //        cModel.Status = 1;
        //        cModel.StartedOn = CommonUsage.GetCurrentDate();
        //        cModel.AttPassword = result.attendeePW;
        //        cModel.ModPassword = result.moderatorPW;
        //        cModel.InternalMeetingID = result.internalMeetingID;
        //        (new BBBOnlineClassData()).UpdateBBBOnlineClassDetails(cModel);
        //    }

        //    //var requestJoin = new JoinMeetingRequest { meetingID = cModel.MeetingID };
        //    //requestJoin.userID = user.UserID.ToString();
        //    //requestJoin.fullName = user.FullName;
        //    //requestJoin.password = cModel.ModPassword;
        //    ////requestJoin.avatarURL = avatar;
        //    //var url = client.GetJoinMeetingUrl(requestJoin);
        //    return Json(1);
        //    //return Redirect("~/Teacher/OnlineClasses/");
        //}
        //[PermissionFilter]
        //public async Task<ActionResult> JoinClass(string ID = null)
        //{
        //    var user = PermissionManager.GetLoggedInUser();
        //    int OCID = CommonUsage.ConvertToInt(ID);
        //    BBBOnlineClassModel cModel = (new BBBOnlineClassData()).GetOnlineClassDetailsByOCID(OCID);
        //    string basep = $"{this.Request.Url.Scheme}://{this.Request.Url.Host}";

        //    DateTime startdatetime = DateTime.Parse("2021-02-01 " + cModel.StartTime);
        //    DateTime enddatetime = DateTime.Parse("2021-02-01 " + cModel.EndTime);
        //    int duration = (enddatetime - startdatetime).Minutes;
        //    if(duration<0)
        //    {
        //        duration = 30;
        //    }
        //    else if (duration > 60)
        //    {
        //        duration = 60;
        //    }
        //    if (basep.Contains("localhost"))
        //    {
        //        basep = "https://node1.pschoolonline.com";
        //    }
        //    string basepath = basep;
        //    string logo = basepath + "/Images/SBranchLogo/" + user.SBranchID + "_" + user.BranchLogo;
        //    string avatar = basepath + "/Images/EmployeeImage/" + user.UserID + "_" + user.UserImage;
        //    var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
        //    var meetingStatus = await client.GetMeetingInfoAsync(new GetMeetingInfoRequest { meetingID = cModel.MeetingID });
        //    if (meetingStatus.returncode == Returncode.FAILED)
        //    {
        //        MetaData meta = new MetaData();
        //        meta.Add("BranchID", "1");
        //        string meu = basepath + "/Home/EndOnlineClasses";
        //        meta.Add("endCallbackUrl", meu);

        //        string reccbu = basepath + "/home/bbbrecordingready/";
        //        meta.Add("bbb-recording-ready-url", reccbu);
        //        meta.Add("bbb-skip-check-audio", "true");
        //        meta.Add("bbb_client_title", "P-School");
        //        meta.Add("bbb_enable_screen_sharing", "false");
        //        meta.Add("bbb_show_public_chat_on_login", "false");
        //        var result = await client.CreateMeetingAsync(new CreateMeetingRequest
        //        {
        //            name = cModel.SubjectName + " (" + cModel.ClassSection + "), by " + cModel.TeacherName + " on " + cModel.ClassDate.ToString("dd MMM, yyyy"),
        //            meetingID = cModel.MeetingID,
        //            record = true,
        //            logoutURL = basepath + "/Home/ClassEnded/" + cModel.MeetingID,
        //            meta = meta,
        //            guestPolicy = "ALWAYS_ACCEPT",
        //            logo = logo,
        //            lockSettingsDisablePrivateChat = true,
        //            lockSettingsDisableNote = false,
        //            muteOnStart = true,
        //            allowModsToUnmuteUsers = true,
        //            autoStartRecording = true,
        //            duration = duration + 5
        //            //autoStartRecording = true,
        //            //bannerText = "Online Class for Subject:" + cModel.SubjectName + " Class:" + cModel.ClassSection + " By :" + cModel.TeacherName
        //        });
        //        if (result.returncode == Returncode.FAILED) return View("Error", result);
        //        if (result.returncode != Returncode.FAILED)
        //        {
        //            cModel.InternalMeetingID = result.internalMeetingID;
        //            cModel.Status = 1;
        //            cModel.StartedOn = CommonUsage.GetCurrentDate();
        //            cModel.AttPassword = result.attendeePW;
        //            cModel.ModPassword = result.moderatorPW;
        //            (new BBBOnlineClassData()).UpdateBBBOnlineClassDetails(cModel);
        //        }
        //    }

        //    var requestJoin = new JoinMeetingRequest { meetingID = cModel.MeetingID };
        //    requestJoin.userID = user.UserID.ToString();
        //    requestJoin.fullName = user.FullName;
        //    requestJoin.password = cModel.ModPassword;
        //    var setConfigRequest = new SetConfigXMLRequest
        //    {
        //        meetingID = cModel.MeetingID,
        //        configXML = "<config><modules><localeversion supressWarning=\"false\">0.9.0</localeversion></modules></config>"
        //    };
        //    var setConfigResult = await client.SetConfigXMLAsync(setConfigRequest);
        //    if (setConfigResult.returncode == Returncode.FAILED) return View("Error", setConfigResult);
        //    requestJoin.configToken = setConfigResult.configToken;
        //    requestJoin.avatarURL = avatar;
        //    var url = client.GetJoinMeetingUrl(requestJoin);
        //    ViewBag.URL = url;
        //    return View();
        //}
        //public async Task<ActionResult> EndClass(string ID = null)
        //{
        //    int OCID = CommonUsage.ConvertToInt(ID);
        //    BBBOnlineClassModel cModel = (new BBBOnlineClassData()).GetOnlineClassDetailsByOCID(OCID);
        //    var client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
        //    var result = await client.EndMeetingAsync(new EndMeetingRequest
        //    {
        //        meetingID = cModel.MeetingID,
        //        password = cModel.ModPassword
        //    });
        //    if (result.returncode == Returncode.FAILED) return Json(0);
        //    //cModel.EndedOn = CommonUsage.GetCurrentDate();
        //    //cModel.Status = 2;
        //    //cModel.MeetingID = ID;
        //    //(new BBBOnlineClassData()).EndOnlineClass(cModel);
        //    return Json(1);
        //}
        //public ActionResult GetOnlineClassDetails(string ID = null)
        //{

        //    int OCID = CommonUsage.ConvertToInt(ID);
        //    BBBOnlineClassModel cModel = (new BBBOnlineClassData()).GetOnlineClassDetailsByOCID(OCID);
        //    return PartialView("_OnlineClassDetailsPartial", cModel);
        //}
<<<<<<< HEAD
=======
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
>>>>>>> master
    }
}