using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using SMEnterprise.Filters;
using Newtonsoft.Json;
using System.Threading.Tasks;
using BigBlueButtonAPI.Core;

namespace SMEnterprise.Controllers
{
    public class ParentController : Controller
    {
        private readonly BigBlueButtonAPIClient client;
        public ParentController()
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
        [PermissionFilter]
        public ActionResult LandingPage()
        {
            int ParentID = PermissionManager.GetLoggedInUser().UserID;
            ParentLandingPageModel objModel = objParentData.GetParentLandingPageData(ParentID);
            return View(objModel);
        }
        ParentData objParentData = new ParentData();
        AccountData objAccountData = new AccountData();
        TeacherData objTeacherData = new TeacherData();
        [ChildActionOnly]
        public ActionResult Childs()
        {
            int ParentID = PermissionManager.GetLoggedInUser().UserID;
            IEnumerable<StudentModel> objModel = objParentData.GetChilds(ParentID);
            if (Session["SChildID"] == null)
            {
                Session["SChildID"] = objModel.ElementAt(0).StudentID;
            }
            return PartialView("~/Views/Shared/_ChildListPartial.cshtml", objModel);
        }
        [HttpPost]
        public ActionResult SChildChanged(string ID)
        {
            Session["SChildID"] = ID;
            return Json(1, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult ChangePassword()
        {
            return View();
        }
        // GET: Parent
        #region Dashboard
        [PermissionFilter]
        public ActionResult Dashboard()
        {

            int ParentID = PermissionManager.GetLoggedInUser().UserID;
            if (Session["SChildID"] == null)
            {
                IEnumerable<StudentModel> objSModel = objParentData.GetChilds(ParentID);
                Session["SChildID"] = objSModel.ElementAt(0).StudentID;

            }

            int StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            DashboardModel objModel = objParentData.GetDashboardPageData(StudentID, ParentID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GetDashboardData()
        {
            int StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            DashboardModel objModel = objParentData.GetDashboardData(StudentID);

            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region MailBox
        MessageData objMessageData = new MessageData();
        [PermissionFilter]
        public ActionResult MailBox()
        {
            int ParentID = PermissionManager.GetLoggedInUser().UserID;
            int UserType = PermissionManager.GetLoggedInUser().RoleID;
            MailBoxModel objModel = objMessageData.GetMailBoxModel(UserType, ParentID, 3);
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
            MailBoxModel objModel = objMessageData.GetSentMailBoxModel(UserType, ParentID, 3);
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
        #region Calendar
        [PermissionFilter]
        public async Task<ActionResult> Calendar()
        {
            int year = CommonUsage.GetCurrentDate().Year;
            int month = CommonUsage.GetCurrentDate().Month;
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            CommonData objCommonData = new CommonData();
            EventCalendarModel model =await objCommonData.GetEventCalander(month, year, SBranchID, 0);
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
        #region Fee Payments
        [PermissionFilter]
        public async Task<ActionResult> FeePayment()
        {
            int ParentID = PermissionManager.GetLoggedInUser().UserID;
            int Year = CommonUsage.GetCurrentDate().Year;
            int Month = CommonUsage.GetCurrentDate().Month;
            int Day = CommonUsage.GetCurrentDate().Day;
            List<PaymentModel> Payments = await objParentData.GetParentPayments(ParentID, Day, Month, Year);
            return View(Payments);
        }
        [PermissionFilter]
        public ActionResult FeeSummery()
        {
            StudentSessionFeeStatusPageModel oModel = new StudentSessionFeeStatusPageModel();
            oModel.StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            objAccountData.GetStudentMonthWiseSessionFeeDetails(oModel);
            return View(oModel);
        }
        [PermissionFilter]
        public async Task<ActionResult> GetPaymentDetail(ParentApiPaymentDetailParamModel Model = null)
        {
            int PaymentID = Model.ID;
            int Day = CommonUsage.GetCurrentDate().Day;
            List<PaymentDetailsModel> model =await objParentData.GetPaymentDetails(Model.ID, Day, Model.Month, Model.Year, Model.StudentID);

            return PartialView("_ParentPaymentDetails", model);
        }
        [PermissionFilter]
        public async Task<ActionResult> GetFeeReciept(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            FeePaymentModel model =await objAccountData.GetFeePaymentReciptDetails(iID);
            return PartialView("_PrintFeeRecipt", model);
        }
        #endregion
        #region Performance
        [PermissionFilter]
        public ActionResult StudentPerformance(ParentStudentPerformancePage objModel)
        {
            objModel.StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            objModel = objParentData.GetStudentPerformancePage(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult PerformanceDetails(PerformanceParameterDetailModel objModel)
        {
            objModel.StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());

            objModel = objParentData.GetStudentPerformanceDetails(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult StudentPerformanceDetails(string ID = null, string ID2 = null, string ID3 = null)
        {
            PerformanceParameterDetailModel objModel = new PerformanceParameterDetailModel();
            objModel.StudentSessionUID = CommonUsage.ConvertToInt(ID);
            objModel.EvaluationID = CommonUsage.ConvertToInt(ID2);
            objModel.SessionID = CommonUsage.ConvertToInt(ID3);
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objTeacherData.GetStudentPerformanceDetails(objModel);
            return View(objModel);
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
        #region Notices
        [PermissionFilter]
        public async Task<ActionResult> Notices()
        {
            int ParentID = PermissionManager.GetLoggedInUser().UserID;

            List<NoticeModel> objModel =await objParentData.GetNotices(ParentID);
            return View(objModel);
        }
        #endregion
        #region ParentDiary
        [PermissionFilter]
        public async Task<ActionResult> ParentDiary()
        {
            int ParentID = PermissionManager.GetLoggedInUser().UserID;

            List<ParentDiaryModel> objModel =await objParentData.GetParentDiary(ParentID);
            return View(objModel);
        }
        #endregion

        [PermissionFilter]
        public ActionResult TimeTable(string id = null)
        {

            int StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            DateTime CurrDate = CommonUsage.GetCurrentDate();
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            TimeTablePageModel objModel = objParentData.GetStudentTimeTable(StudentID, CurrDate, SBranchID);
            return View(objModel);

        }
        [PermissionFilter]
        public async Task<ActionResult> Assignments()
        {

            int StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            DateTime CurrDate = CommonUsage.GetCurrentDate();
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            StudentData objCommonData = new StudentData();
            List<AssignmentModel> objModel =(await objCommonData.GetStudentAssignments(StudentID)).ToList();
            return View(objModel);

        }
        #region Online Exams
        [PermissionFilter]
        public ActionResult OnlineExams(StudentOnlineExamListPageModel oModel)
        {
            oModel.StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            objParentData.GetStudentOnlineExams(oModel);
            return View(oModel);
        }
        [PermissionFilter]
        public ActionResult StartOnlineExam(OnlineExamModel oModel)
        {
            OnlineExamEditModel objModel = objParentData.GetStudentOnlineExamDetails(oModel.OExamID);
            if (objModel.Exam.OnlineExamType == 1)
            {
                TempData["OExamID"] = oModel.OExamID;
                return RedirectToAction("StartOnlineExamNew");
            }
            else
            {
                return View(objModel);
            }
        }
        [PermissionFilter]
        public ActionResult StartOnlineExamNew(OnlineExamModel oModel)
        {
            if (TempData["OExamID"] != null)
            {
                oModel.OExamID = (int)TempData["OExamID"];
            }
            OnlineExamEditModel objModel = objParentData.GetStudentOnlineExamDetails(oModel.OExamID);

            return View(objModel);

        }
        [PermissionFilter]
        public ActionResult SubmitAnswer(StudentOnlineExamSubmitModel oModel)
        {
            oModel.StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            oModel.SubmissionDate = CommonUsage.GetCurrentDate();
            StudentOnlineExamSubmitModel obj = objParentData.SubmitOnlineExamAnswerSheetNew(oModel);
            int res = obj.SubAnsID;
            TempData["SubmissionID"] = obj.SubmissionID;
            TempData["OExamID"] = oModel.OExamID;
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult SubmitOnlineTest(StudentOnlineExamSubmitModel oModel)
        {
            oModel.StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            oModel.SubmissionDate = CommonUsage.GetCurrentDate();
            int SubmissionID = objParentData.SubmitOnlineExamAnswerSheet(oModel);
            TempData["SubmissionID"] = SubmissionID;
            TempData["OExamID"] = oModel.OExamID;
            return RedirectToAction("ViewAnswerSheet");
        }
        [PermissionFilter]
        public ActionResult ViewAnswerSheet(StudentOnlineExamSubmitModel oModel)
        {
            if(TempData["SubmissionID"]!=null)
            {
                oModel.SubmissionID = (int)TempData["SubmissionID"];
                oModel.OExamID = (int)TempData["OExamID"];
            }
            oModel.StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            OnlineExamEditModel objModel = objParentData.GetStudentOnlineExamAnswerSheet(oModel.OExamID,oModel.SubmissionID,oModel.StudentID);
            return View(objModel);
        }
        #endregion
        #region OnlineClasses
        [HttpPost]
        [PermissionFilter]
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
        public async Task<ActionResult> OnlineClasses(BBBOnlineClassStudentPageModel oModel)
        {

            int StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            if (oModel.ClassDate.Year == 1)
            {
                oModel.ClassDate = CommonUsage.GetCurrentDate();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            oModel.Classes = await (new BBBOnlineClassData()).GetStudentOnlineClassesSchedules(StudentID, oModel.ClassDate);
            return View(oModel);

        }
        [PermissionFilter]
        public async Task<ActionResult> JoinClass(string ID = null)
        {
            var onlienClassData = new BBBOnlineClassData();
            var user = PermissionManager.GetLoggedInUser(); 
            int ParentID = PermissionManager.GetLoggedInUser().UserID;
            int OCID = CommonUsage.ConvertToInt(ID);
            int StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            BBBOnlineClassModel cModel = onlienClassData.GetOnlineClassDetailsByOCID(OCID);

            string basepath = $"{this.Request.Url.Scheme}://{this.Request.Url.Host}";
            var meetingStatus = await client.GetMeetingInfoAsync(new GetMeetingInfoRequest { meetingID = cModel.MeetingID });
            if (meetingStatus.returncode != Returncode.FAILED)
            {
                var joinDate = CommonUsage.GetCurrentDate();
                NameIDModel student = await onlienClassData.StudentJoinOnlineClass(StudentID, joinDate, OCID, ParentID, 0);
               // StudentModel Student = objStudents.Where(x => x.StudentID == StudentID).FirstOrDefault();
                string avatar = basepath + student.Extra1;
                var requestJoin = new JoinMeetingRequest { meetingID = cModel.MeetingID };
                requestJoin.userID = student.ID.ToString();
                requestJoin.fullName = student.Name;
                requestJoin.password = cModel.AttPassword;
                var setConfigRequest = new SetConfigXMLRequest
                {
                    meetingID = cModel.MeetingID,
                    configXML = "<config><modules><localeversion supressWarning=\"false\">0.9.0</localeversion></modules></config>"
                };
                var setConfigResult = await client.SetConfigXMLAsync(setConfigRequest);
                if (setConfigResult.returncode == Returncode.FAILED) return Json(0);
                requestJoin.configToken = setConfigResult.configToken;
                requestJoin.avatarURL = avatar;
                var url = client.GetJoinMeetingUrl(requestJoin);
                ViewBag.URL = url;
                return View();
            }
            else
            {
               
                return View(-1);
            }
        }
        #endregion
    }
}