using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using System.IO;
using SMEnterprise.Filters;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using BigBlueButtonAPI.Core;

namespace SMEnterprise.Controllers
{

    public partial class AdminController : Controller
    {
        AdminData objAdminData = new AdminData();
        AccountData objAccountData = new AccountData();

        private readonly BigBlueButtonAPIClient client;
        public AdminController()
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
        public ActionResult ParentAppInstalSMS(string ID = null)
        {
            ParentAppSMSPageModel objModel = new ParentAppSMSPageModel();
            objModel.SessionID = CommonUsage.ConvertToInt(ID);
            //objModel.ClassID = CommonUsage.ConvertToInt(ID2);
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAdminData.GetParentsForLoginSMS(objModel.SessionID, objModel.SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult SendParentAppInstalSMS(ParentAppSMSPageModel oModel)
        {
            // ParentAppSMSPageModel objModel = new ParentAppSMSPageModel();
            //objModel.SessionID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            oModel.EncryptedPassword = CommonUsage.EncryptPassword(oModel.Password + CommonUsage.FixedPrimaryEncryptionSalt);
          //  objModel = objAdminData.GetParentsForLoginSMS(objModel.SessionID, objModel.SBranchID);
            string PlayLink = objAdminData.GetPlayStoreLink(SBranchID);
            foreach (ParentModel r in oModel.Parents.Where(c => c.IsSelected == 1))
            {
                if (r.FatherMobileNo != null && r.FatherMobileNo != "")
                {
                    string smsText = CommonUsage.ParentAppSMSTemplate.Replace("[Reciever]", r.FatherName).Replace("[PlayStoreLink]", PlayLink)
                        .Replace("[UserName]", r.ParentSID).Replace("[Password]", oModel.Password);
                    SMSSender objSender = new SMSSender();
                    objSender.SendSMSAsync(smsText, r.FatherMobileNo, SBranchID, -1, 4, r.ParentID);
                    objAdminData.UpdateIndividualPassword(SBranchID, r.ParentSID, oModel.EncryptedPassword, r.ParentID);
                }
            }
            TempData["IsQueued"] = 1;
            return RedirectToAction("ParentAppInstalSMS", "Admin");
        }

   

        [HttpPost]
        public ActionResult GetSMSBalance()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            if (Session["SMSConfiguration"] == null)
            {
                Session["SMSConfiguration"] = objAdminData.GetDefaultSMSConfigurationDetails(SBranchID);
            }
            SMSConfigirationModel SMSConfigiration = (SMSConfigirationModel)Session["SMSConfiguration"];
            List<SMSBalanceModel> oModel = SMSSender.GetSMSBalance(SBranchID, SMSConfigiration);
            return Json(oModel, JsonRequestBehavior.AllowGet);
        }
        [ChildActionOnly]
        public ActionResult SBranches()
        {
            int UserID = -1;
            if (PermissionManager.GetLoggedInUser().RoleID == (int)RoleType.Admin || PermissionManager.GetLoggedInUser().RoleID == (int)RoleType.Director)
            {
                UserID = -1;
            }
            else
            {
                UserID = PermissionManager.GetLoggedInUser().UserID;

            }
            if (Session["SBrancheList"] == null)
            {
                Session["SBrancheList"] = objAdminData.GetBranches(UserID).ToList();
            }
            List<SBranchModel> objModel = (List<SBranchModel>)Session["SBrancheList"];
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = objModel.ElementAt(0).SBranchID;
            }
            return PartialView("~/Views/Shared/_BranchListPartial.cshtml", objModel);
        }
        [ChildActionOnly]
        public ActionResult Header()
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            return PartialView("~/Views/Shared/_AdminLayoutHeader.cshtml");
        }
        [PermissionFilter]
        public ActionResult ChangePassword()
        {

            return View();
        }
        [PermissionFilter]
        public ActionResult MasterSetup()
        {
            StartupModel objModel = (StartupModel)Session["StartupModel"];
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateMasterSetup(StartupModelUpdate objModel)
        {
            StartupModel objStartupModel;
            if (Session["StartupModel"] != null)
            {
                objStartupModel = (StartupModel)Session["StartupModel"];
            }
            else
            {
                objStartupModel = new StartupModel();
            }
            foreach (Type_Value m in objModel.MasterSettings)
            {
                if (m.Type == "SessionDate")
                {
                    objStartupModel.SessionStartDate = CommonUsage.ConvertToDateTime(m.Value);
                }
                else if (m.Type == "SessionEndDate")
                {
                    objStartupModel.SessionEndDate = CommonUsage.ConvertToDateTime(m.Value);
                }
                else if (m.Type == "AbsentNotificationTime")
                {
                    objStartupModel.AbsentNotificationTime = m.Value;
                }
                else if (m.Type == "AbsentSMSTemplate")
                {
                    objStartupModel.AbsentSMSTemplate = m.Value;
                }
                else if (m.Type == "EvaluationMode")
                {
                    objStartupModel.EvaluationMode = CommonUsage.ConvertToInt(m.Value);
                }
                else if (m.Type == "HostelFeeMode")
                {
                    objStartupModel.HostelFeeMode = CommonUsage.ConvertToInt(m.Value);
                }
                else if (m.Type == "TransportFeeMode")
                {
                    objStartupModel.TransportFeeMode = CommonUsage.ConvertToInt(m.Value);
                }
                //else if (m.Type == "SchemaPrefix")
                //{
                //    objStartupModel.SchemaPrefix = m.Value;
                //}
                else if (m.Type == "StudentInOutSMSNotification")
                {
                    objStartupModel.StudentInOutSMSNotification = CommonUsage.ConvertToInt(m.Value);
                }
                else if (m.Type == "StudentInSMSTemplate")
                {
                    objStartupModel.StudentInSMSTemplate = m.Value;
                }
                else if (m.Type == "StudentOutSMSTemplate")
                {
                    objStartupModel.StudentOutSMSTemplate = m.Value;
                }
                else if (m.Type == "SMSForAbsentStudents")
                {
                    objStartupModel.SMSForAbsentStudents = CommonUsage.ConvertToInt(m.Value);
                }
                else if (m.Type == "FeePaymentReminderDate")
                {
                    objStartupModel.FeePaymentReminderDate = CommonUsage.ConvertToInt(m.Value);
                }
                else if (m.Type == "FeePaymentReminderSMS")
                {
                    objStartupModel.FeePaymentReminderSMS = CommonUsage.ConvertToInt(m.Value);
                }
                else if (m.Type == "FeePaymentReminderSMSTemplate")
                {
                    objStartupModel.FeePaymentReminderSMSTemplate = m.Value;
                }
                else if (m.Type == "FeePaymentNotificationSMSTemplate")
                {
                    objStartupModel.FeePaymentNotificationSMSTemplate = m.Value;
                }
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objAdminData.InsertUpdateMasterSettings(objModel, SBranchID);
            return RedirectToAction("MasterSetup", "Admin");
        }
        [PermissionFilter]
        public ActionResult Dashboard()
        {
            int UserID = -1;

            if (PermissionManager.GetLoggedInUser().RoleID != 0)
            {
                UserID = PermissionManager.GetLoggedInUser().UserID;
            }
            IEnumerable<SBranchModel> objBranches = objAdminData.GetBranches(UserID);
            if (objBranches.Count() == 0)
            {
                return RedirectToAction("FirstBranch");
            }
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = objBranches.ElementAt(0).SBranchID;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            DateTime CurrentDate = CommonUsage.GetCurrentDate();
            AdminDashboardModel objModel = objAdminData.GetDashBoardData(SBranchID, CurrentDate);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult FirstBranch()
        {

            return View();
        }
        [HttpPost]
        public ActionResult SBranchChanged(string ID)
        {
            Session["SBranchID"] = ID;
            PermissionManager.GetLoggedInUser().SBranchID = CommonUsage.ConvertToInt(ID);
            CommonData objCData = new CommonData();
            objCData.InitializeStartupSettings(CommonUsage.ConvertToInt(ID));
            return Json(1, JsonRequestBehavior.AllowGet);
        }
        #region Building Management
        [PermissionFilter]
        public ActionResult BuildingManagement()
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<BuildingModel> objModel = objAdminData.GetBuildings(-1, SBranchID);
            return View(objModel);
        }
        [HttpPost]
        [PermissionFilter]
        public ActionResult UpdateBuilding(BuildingModel objData)
        {
            objData.CreatedDate = CommonUsage.GetCurrentDate();
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objAdminData.InsertUpdateBuilding(objData);
            return RedirectToAction("BuildingManagement", "Admin");
        }

        [PermissionFilter]
        public ActionResult GetBuildingFloors(string ID = null)
        {
            BuildingModel model = new BuildingModel();
            model.ID = CommonUsage.ConvertToInt(ID);
            model.Floors = objAdminData.GetBuildingFloors(model.ID, -1).ToList();

            return PartialView("_BuildingFloorsPartial", model);
        }
        [PermissionFilter]
        public ActionResult GetBuildingFloorRooms(string ID = null)
        {
            FloorModel model = new FloorModel();
            model.ID = CommonUsage.ConvertToInt(ID);
            model = objAdminData.GetBuildingFloorRooms(model.ID, -1);

            return PartialView("_BuildingFloorRoomsPartial", model);
        }
        [HttpPost]
        [PermissionFilter]
        public ActionResult UpdateFloor(FloorModel objData)
        {
            objData.CreatedDate = CommonUsage.GetCurrentDate();
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objData.UserID = 1;// PermissionManager.GetLoggedInUser().UserID;
            objAdminData.InsertUpdateFloor(objData);


            BuildingModel model = new BuildingModel();
            model.ID = objData.BuildingID;
            model.Floors = objAdminData.GetBuildingFloors(model.ID, -1).ToList();

            return PartialView("_BuildingFloorsPartial", model);
        }
        [HttpPost]
        [PermissionFilter]
        public ActionResult UpdateRoom(RoomModel objData)
        {
            objData.CreatedDate = CommonUsage.GetCurrentDate();
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objData.UserID = 1;// PermissionManager.GetLoggedInUser().UserID;
            objAdminData.InsertUpdateRoom(objData);


            FloorModel model = new FloorModel();
            model.ID = objData.FloorID;
            model = objAdminData.GetBuildingFloorRooms(model.ID, -1);

            return PartialView("_BuildingFloorRoomsPartial", model);
        }
        #endregion
        #region Class-Section
        [PermissionFilter]
        public ActionResult ClassSectionManagement(string ID = null)
        {
            int SessionID = CommonUsage.ConvertToInt(ID);
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            ClassPageModel objModel = objAdminData.GetClasses(-1, SBranchID, SessionID);
            return View(objModel);
        }
        [HttpPost]
        [PermissionFilter]
        public ActionResult UpdateClass(ClassModel objData)
        {
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objAdminData.InsertUpdateClass(objData);
            return RedirectToAction("ClassSectionManagement", "Admin", new { id = objData.SessionID });
        }
        [PermissionFilter]
        public ActionResult GetClassSections(string ID = null, string ID2 = null, string ID3 = null)
        {
            ClassModel model = new ClassModel();
            model.ClassID = CommonUsage.ConvertToInt(ID);
            model.EducationLevelID = CommonUsage.ConvertToInt(ID2);
            int SessionID = CommonUsage.ConvertToInt(ID3);
            model.Sections = objAdminData.GetClassSections(model.ClassID, -1, SessionID).ToList();

            return PartialView("_ClassSectionPartial", model);
        }
        [PermissionFilter]
        public ActionResult GetSectionDetails(string ID = null, string ID2 = null)
        {
            SectionDetailsModel model = new SectionDetailsModel();
            int SectionID = CommonUsage.ConvertToInt(ID);
            int ClassID = CommonUsage.ConvertToInt(ID2);
            model = objAdminData.GetSectionDetails(SectionID, ClassID);

            return PartialView("_ClassSectionEditFormPartial", model);
        }
        [PermissionFilter]
        public ActionResult GetSectionBuildingFloors(string ID = null, string ID2 = null)
        {
            int SectionID = CommonUsage.ConvertToInt(ID);
            int BuildingID = CommonUsage.ConvertToInt(ID2);
            IEnumerable<FloorModel> floors = objAdminData.GetFloorsForSection(SectionID, BuildingID);

            return PartialView("_FloorOptionsPartial", floors);
        }
        [PermissionFilter]
        public ActionResult GetFloorRooms(string ID = null, string ID2 = null)
        {
            int SectionID = CommonUsage.ConvertToInt(ID);
            int FloorID = CommonUsage.ConvertToInt(ID2);
            IEnumerable<RoomModel> floors = objAdminData.GetRoomsForSection(SectionID, FloorID);

            return PartialView("_RoomOptionsPartial", floors);
        }
        [PermissionFilter]
        public ActionResult GetSectionTeachers(string ID = null, string ID2 = null)
        {
            int GroupID = CommonUsage.ConvertToInt(ID);
            int EducationLevelID = CommonUsage.ConvertToInt(ID2);
            IEnumerable<EmployeeModel> floors = objAdminData.GetTeachersForSection(EducationLevelID, GroupID);

            return PartialView("_TeacherOptionsPartial", floors);
        }
        [HttpPost]
        [PermissionFilter]
        public ActionResult UpdateSection(SectionModel objData)
        {
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objAdminData.InsertUpdateSection(objData);


            ClassModel model = new ClassModel();
            model.ClassID = objData.ClassID;
            model.EducationLevelID = objData.EducationLevelID;
            model.Sections = objAdminData.GetClassSections(model.ClassID, -1, objData.SessionID).ToList();

            return PartialView("_ClassSectionPartial", model);
        }
        #endregion
        #region Holiday Management
        [PermissionFilter]
        public ActionResult HolidayManagement()
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<HolidayModel> objModel = objAdminData.GetHolidays(SBranchID);
            return View(objModel);

        }
        [PermissionFilter]
        public ActionResult SendHolidaySMS(SMSSendingModel data)
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            if (data.AssociatedIDs == "")
            {
                data.AssociatedIDs = "0";
            }
            List<SMSRecieverModel> recievers = objAdminData.GetSMSRecieverList(SBranchID, data.ID.ToString(), 4);
            foreach (SMSRecieverModel r in recievers)
            {
                if (r.MobileNo != null && r.MobileNo != "" && String.IsNullOrEmpty(r.deviceToken))
                {
                    SMSSender objSender = new SMSSender();
                    objSender.SendSMSAsync(data.SMSText, r.MobileNo, SBranchID, 2, r.RecieverType, r.ID);
                }
            }
            objAdminData.UpdateSenderSMSSentStatus(2, data.ID, data.AssociatedIDs, data.RecieverType);

            NotificationModel objModel = new NotificationModel();
            CommonData objCommonData = new CommonData();
            objModel.RecieverID = -1;
            objModel.SBranchID = SBranchID;
            objModel.NotificationType = 4;
            objModel.RecieverType = 3;
            objModel.NotificationText = data.SMSText;
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
            string NotificationServerKey = PermissionManager.GetLoggedInUser().NotificationServerKey;
            string[] Recievers = sb.ToString().Trim().Split("#".ToCharArray());
            //NotificationDataWraperModel routeData = new NotificationDataWraperModel();
            //routeData.type = (int)CommonUsage.NotificationTypes.Holiday;
            //routeData.primaryid = data.ID;
            //routeData.studentid=
            CommonUsage.SendNotificationFCM(Recievers, data.SMSText, "4", NotificationServerKey);
            objCommonData.InsertNotification(objModel);

            return RedirectToAction("HolidayManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult GetHolidayDetails(string ID = null)
        {
            HolidayDetailModel model = new HolidayDetailModel();
            int HolidayID = CommonUsage.ConvertToInt(ID);
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            model = objAdminData.GetHolidayDetails(HolidayID, SBranchID);

            return PartialView("_HolidayEditFormPartial", model);
        }
        [PermissionFilter]
        public ActionResult UpdateHoliday(HolidayModel objData)
        {
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            //if (objData.ClassesIncluded != "0")
            //{
            //    objData.Classes = 1;
            //}
            int HolidayID = objAdminData.InsertUpdateHoliday(objData);
            if (objData.Status == 1)
            {

            }
            return RedirectToAction("HolidayManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult DeleteHoliday(HolidayModel objData)
        {
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objData.StartDate = CommonUsage.GetCurrentDate();
            objData.EndDate = CommonUsage.GetCurrentDate();
            objAdminData.InsertUpdateHoliday(objData);
            return RedirectToAction("HolidayManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult HolidayNoticePrint(string ID = null)
        {
            HolidayPrintModel model = new HolidayPrintModel();
            int HolidayID = CommonUsage.ConvertToInt(ID);
            model = objAdminData.GetHolidayPrintDetails(HolidayID);

            return View(model);
        }
        #endregion
        #region Period Management
        [PermissionFilter]
        public ActionResult EducationLevelManagement()
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<EducationLevelModel> objModel = objAdminData.GetEducationLevelsForPeriod(SBranchID);
            return View(objModel);

        }
        [PermissionFilter]
        public ActionResult UpdateEducationLevel(EducationLevelModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objAdminData.InsertUpdateEducationLevel(objData);
            return RedirectToAction("EducationLevelManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult UpdateReportSection(EducationLevelSectionModel objData)
        {
            EducationLevelModel objModel = new EducationLevelModel();
            objModel.ID = objData.EducationLevelID;
            objModel.ReportSections = objAdminData.InsertUpdateELSections(objData);

            return PartialView("_ELReportSections", objModel);
        }
        [PermissionFilter]
        public ActionResult GetELReportSections(string ID = null)
        {
            int EduID = CommonUsage.ConvertToInt(ID);
            EducationLevelModel objModel = new EducationLevelModel();
            objModel.ID = EduID;
            objModel.ReportSections = objAdminData.GetEducationLevelSections(EduID);

            return PartialView("_ELReportSections", objModel);
        }
        [PermissionFilter]
        public ActionResult GetPeriodList(string ID = null)
        {
            int EduID = CommonUsage.ConvertToInt(ID);
            EducationLevelModel objModel = objAdminData.GetPeriodsonEducationLevel(EduID);

            return PartialView("_PeriodListPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdatePeriod(PeriodModel objData)
        {
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objAdminData.InsertUpdatePeriod(objData);

            EducationLevelModel objModel = objAdminData.GetPeriodsonEducationLevel(objData.EducationLevelID);

            return PartialView("_PeriodListPartial", objModel);
        }
        #endregion
        #region Class Time Table
        [PermissionFilter]
        public ActionResult TimeTableManagement(string id = null)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            int SectionID = CommonUsage.ConvertToInt(id);
            DateTime CurrDate = CommonUsage.GetCurrentDate();
            TimeTablePageModel objModel = objAdminData.GetSectionTimeTable(SectionID, CurrDate, SBranchID);
            return View(objModel);

        }
        [PermissionFilter]
        public ActionResult ViewSyllabusSchedule(SyllabusScheduleViewModel objData)
        {
            objData.CurrentDate = CommonUsage.GetCurrentDate();
            objData = objAdminData.GetSyllabusSchedule(objData);

            return PartialView("_ViewRecentSyllabusSchedule", objData);
        }
        [PermissionFilter]
        public ActionResult ViewTeacherSubstitution(TeacherSubstitutionEditModel objData)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objData.CurrentDate = CommonUsage.GetCurrentDate();
            objData = objAdminData.GetSectionSubstitutionDetails(objData);

            return PartialView("_ViewTeacherTimeTableSubstitutionPartial", objData);
        }
        [PermissionFilter]
        public ActionResult TeacherTimeTableManagement(string id = null)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            int TeacherID = CommonUsage.ConvertToInt(id);
            DateTime CurrDate = CommonUsage.GetCurrentDate();
            TimeTablePageModel objModel = new TimeTablePageModel();
            objModel = objAdminData.GetTeacherTimeTable(TeacherID, CurrDate, SBranchID);
            return View(objModel);

        }
        [PermissionFilter]
        public ActionResult UpdateTeacherSubstitution(TeacherSubstitutionModel objModel)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            objModel.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            if (objModel.OpType == -1)
            {
                objModel.FromDate = CommonUsage.GetCurrentDate();
            }
            objModel.CurrentDate = CommonUsage.GetCurrentDate();
            objModel.EndDate = objModel.FromDate.AddHours(12);
            TeacherSubstitutionEditModel objData = objAdminData.UpdateTeacherSubstitutionDetails(objModel);
            TeacherSubstitutionSMSModel objSMS = objAdminData.GetTeacherSubstitutionSMSDetails(objModel);
            string ReplacingSMS = "";
            StartupModel objStartupModel = (StartupModel)Session["StartupModel"];
            if (objModel.OpType == -1)
            {
                ReplacingSMS = objStartupModel.TeacherRemoveSubstitutedSMSTemplate.Replace("[NamePlaceHolder]", objSMS.ReplacingTeacher.EmployeeName)
                   .Replace("[ClassPlaceHolder]", objSMS.ClassName)
                   .Replace("[SectionPlaceHolder]", objSMS.SectionName)
                   .Replace("[DatePlaceHolder]", objModel.FromDate.ToString("dd,MMM")).Replace("[DayPlaceHolder]", AdminData.arrDays[objModel.DayID - 1])
                   .Replace("[PeriodPlaceHolder]", objSMS.PeriodName);
            }
            else
            {
                ReplacingSMS = objStartupModel.TeacherSubstitutedSMSTemplate.Replace("[NamePlaceHolder]", objSMS.ReplacingTeacher.EmployeeName)
                    .Replace("[SubjectPlaceHolder]", objSMS.SubjectName).Replace("[ClassPlaceHolder]", objSMS.ClassName)
                    .Replace("[SectionPlaceHolder]", objSMS.SectionName).Replace("[ReplacedTeacher]", objSMS.ReplacedTeacher.EmployeeName)
                    .Replace("[DatePlaceHolder]", objModel.FromDate.ToString("dd,MMM")).Replace("[DayPlaceHolder]", AdminData.arrDays[objModel.DayID - 1])
                    .Replace("[PeriodPlaceHolder]", objSMS.PeriodName);
            }
            if (objModel.SendSMS == 1)
            {

                if (objSMS.ReplacingTeacher.MobileNumber != null && objSMS.ReplacingTeacher.MobileNumber != "")
                {
                    SMSSender objSender = new SMSSender();
                    objSender.SendSMSAsync(ReplacingSMS, objSMS.ReplacingTeacher.MobileNumber, objModel.SBranchID, 3, 3, objModel.ReplacingTeacherID);
                }

            }

            if (!String.IsNullOrEmpty(objSMS.FCMToken))
            {
                NotificationModel objNModel = new NotificationModel();
                CommonData objCommonData = new CommonData();
                objNModel.RecieverID = objModel.ReplacingTeacherID;
                objNModel.SBranchID = objModel.SBranchID;
                objNModel.NotificationType = 8;
                objNModel.RecieverType = 3;
                objNModel.NotificationText = ReplacingSMS;
                objNModel.NotificationDateTime = CommonUsage.GetCurrentDate();
                objNModel.Recievers = new List<NotificationRecieverModel>();

                string[] Recievers = objSMS.FCMToken.Trim().Split("#".ToCharArray());
                string NotificationServerKey = PermissionManager.GetLoggedInUser().NotificationServerKey;
                CommonUsage.SendNotificationFCM(Recievers, objNModel.NotificationText, "8", NotificationServerKey);
                objCommonData.InsertNotification(objNModel);
            }
            objData.DayID = objModel.DayID;
            objData.PeriodID = objModel.PeriodID;
            objData.SectionID = objModel.SectionID;
            objData.TeacherID = objModel.ReplacedTeacherID;
            objData.SubjectID = objModel.SubjectID;
            objData.ClassID = objModel.ClassID;
            objData.EducationLevelID = objModel.EducationLevelID;
            return PartialView("_EditTeacherTimeTableSubstitutionPartial", objData);

        }
        [PermissionFilter]
        public ActionResult EditLacture(EditTTLactureModel objData)
        {
            objData.CurrentDate = CommonUsage.GetCurrentDate();
            EditTTLactureModel objModel = objAdminData.GetLactureEditDetails(objData);

            return PartialView("_EditTimeTableLacturePartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetSectionSubjectTeachers(EditTTLactureModel objData)
        {
            objData.CurrentDate = CommonUsage.GetCurrentDate();
            IEnumerable<EmployeeModel> Teachers = objAdminData.GetTeacherForSectionSubject(objData);
            return PartialView("_TeacherOptionsPartial", Teachers);
        }
        [PermissionFilter]
        public ActionResult UpdateLacture(EditTTLactureModel objData)
        {
            int res = objAdminData.InsertUpdateLacture(objData);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult UpdateTeacherLacture(EditTTLactureModel objData)
        {
            int res = objAdminData.InsertUpdateTeacherLacture(objData);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult EditMergedClasses(EditMergedClassModel objData)
        {
            EditMergedClassModel objModel = objAdminData.GetEditMergeClasses(objData);

            return PartialView("_MergeSectionPopup", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateMergedClass(EditMergedClassModel objData)
        {
            int res = objAdminData.InsertMergedClasses(objData);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult EditMergedClassSections(EditMergedClassModel objData)
        {
            EditMergedClassModel objModel = objAdminData.GetEditMergeClassSections(objData);

            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult EditMergedClassSectionDetails(EditMergedClassModel objData)
        {
            TeacherSubjectModel objModel = objAdminData.GetEditMergeClassSectionDetail(objData);
            if (objModel == null)
            {
                objModel = new TeacherSubjectModel();
                objModel.SubjectName = "No Subject";
                objModel.TeacherName = "No Teacher";
            }
            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult DeleteMergedClassSection(string id = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            int res = objAdminData.DeleteMergedClasses(ID);

            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult GetClassesOnEducationLevel(string id = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            IEnumerable<ClassModel> objModel = objAdminData.GetEducationLevelClasses(ID);

            return PartialView("_ClassOptionsPartial", objModel);
        }
        public ActionResult GetSectionOnClass(string id = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            IEnumerable<SectionModel> objModel = objAdminData.GetSectionsOnClass(ID);

            return PartialView("_SectionOptionsPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetSectionsOnClassTeacher(string id = null, string id2 = null)
        {
            int TeacherID = CommonUsage.ConvertToInt(id);
            int ClassID = CommonUsage.ConvertToInt(id2);
            IEnumerable<SectionModel> objModel = objAdminData.GetSectionsOnTeacherClass(TeacherID, ClassID);

            return PartialView("_SectionOptionsPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetSectionsOnClassTeacherTT(EditTTLactureModel objData)
        {

            IEnumerable<SectionModel> objModel = objAdminData.GetSectionsOnTeacherClassTT(objData);

            return PartialView("_SectionOptionsPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult EditTeacherLacture(EditTTLactureModel objData)
        {
            objData.CurrentDate = CommonUsage.GetCurrentDate();
            EditTTLactureModel objModel = objAdminData.GetTeacherLactureEditDetails(objData);

            return PartialView("_EditTeacherTimeTableLacturePartial", objModel);
        }
        [PermissionFilter]
        public ActionResult EditTeacherSubstitution(TeacherSubstitutionEditModel objData)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objData.CurrentDate = CommonUsage.GetCurrentDate();
            objData = objAdminData.GetTeacherSubstitutionDetails(objData);

            return PartialView("_EditTeacherTimeTableSubstitutionPartial", objData);
        }

        #endregion
        #region Period Management
        [PermissionFilter]
        public ActionResult HouseManagement()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<HouseModel> objModel = objAdminData.GetHouses(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateHouse(HouseModel objData)
        {
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objData.UserID = 1;// PermissionManager.GetLoggedInUser().UserID;
            int res = objAdminData.InsertUpdateHouse(objData);
            return RedirectToAction("HouseManagement", "Admin");
        }
        #endregion
        #region Transport Management
        [PermissionFilter]
        public ActionResult DriverConductorManagement()
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<DriverConductorModel> objModel = objAdminData.GetDriverConductors(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateDriverConductor(DriverConductorModel objData)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            string OldImageFile = "";
            if (objData.PhotoFile != null)
            {
                OldImageFile = objData.Image;
                objData.Image = objData.PhotoFile.FileName.Replace(" ", "-");
            }
            objData.ID = objAdminData.InsertUpdateDriverConductor(objData);
            if (objData.PhotoFile != null)
            {
                try
                {
                    System.IO.File.Delete(
                        Server.MapPath("~/images/DriverConductor/" + objData.ID + "_" + OldImageFile));
                    System.IO.File.Delete(
                        Server.MapPath("~/images/DriverConductor/Thumb/" + objData.ID + "_" + OldImageFile));
                }
                catch (Exception ex)
                {


                }
                var path = Path.Combine(Server.MapPath(CommonUsage.DriverConductorImageBasePath),
                   objData.ID + "_" + objData.Image);
                objData.PhotoFile.SaveAs(path);
                /**Save Thumb Image **/
                CommonUsage.SaveThumbImage(Server.MapPath(CommonUsage.DriverConductorImageBasePath),
                    objData.ID + "_" + objData.Image);
            }
            return RedirectToAction("DriverConductorManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult DeleteDriverConductor(string id = null)
        {
            DriverConductorModel objData = new DriverConductorModel();
            objData.ID = CommonUsage.ConvertToInt(id);
            objData.OpType = -1;
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.BirthDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            int res = objAdminData.InsertUpdateDriverConductor(objData);
            var dirBlog = new DirectoryInfo(Server.MapPath(CommonUsage.DriverConductorImageBasePath));
            foreach (var file in dirBlog.EnumerateFiles(objData.ID + "_*"))
            {
                try
                {
                    file.Delete();
                    System.IO.File.Delete(dirBlog + "Thumb\\" + file.Name);
                }
                catch (Exception ex)
                {


                }

            }
            return RedirectToAction("DriverConductorManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult VehicleManagement()
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            VehicleEditModel objModel = objAdminData.GetVehicles(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateVehicle(VehicleModel objData)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            string OldImageFile = "";
            if (objData.PhotoFile != null)
            {
                OldImageFile = objData.VehicleImage;
                objData.VehicleImage = objData.PhotoFile.FileName.Replace(" ", "-");
            }
            objData.VehicleID = objAdminData.InsertUpdateVehicle(objData);
            if (objData.PhotoFile != null)
            {
                try
                {
                    System.IO.File.Delete(
                        Server.MapPath("~/images/VehicleImages/" + objData.VehicleID + "_" + OldImageFile));
                    System.IO.File.Delete(
                        Server.MapPath("~/images/VehicleImages/Thumb/" + objData.VehicleID + "_" + OldImageFile));
                }
                catch (Exception ex)
                {


                }
                var path = Path.Combine(Server.MapPath(CommonUsage.VehicleImageBasePath),
                   objData.VehicleID + "_" + objData.VehicleImage);
                objData.PhotoFile.SaveAs(path);
                /**Save Thumb Image **/
                CommonUsage.SaveThumbImage(Server.MapPath(CommonUsage.VehicleImageBasePath),
                    objData.VehicleID + "_" + objData.VehicleImage);
            }
            return RedirectToAction("VehicleManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult VehicleLogs(VehicleLogPageModel objModel)
        {
            if (objModel.StartDate.Year == 1)
            {
                objModel.EndDate = CommonUsage.GetCurrentDate();
                objModel.StartDate = objModel.EndDate.AddMonths(-1);
            }
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAdminData.GetVehicleLogs(objModel);
            return View(objModel);
        }

        [PermissionFilter]
        public ActionResult UpdateVehicleLog(VehicleLogModel oModel)
        {
            oModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            oModel.CreatedDate = CommonUsage.GetCurrentDate();
            int STID = objAdminData.UpdateVehicleLog(oModel);
            return Redirect("/Admin/VehicleLogs");
        }
        [PermissionFilter]
        public ActionResult DeleteVehicle(string id = null)
        {
            VehicleModel objData = new VehicleModel();
            objData.VehicleID = CommonUsage.ConvertToInt(id);
            objData.OpType = -1;
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.InsuranceRenewalDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            int res = objAdminData.InsertUpdateVehicle(objData);
            var dirBlog = new DirectoryInfo(Server.MapPath(CommonUsage.VehicleImageBasePath));
            foreach (var file in dirBlog.EnumerateFiles(objData.VehicleID + "_*"))
            {
                try
                {
                    file.Delete();
                    System.IO.File.Delete(dirBlog + "Thumb\\" + file.Name);
                }
                catch (Exception ex)
                {


                }

            }
            return RedirectToAction("VehicleManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult RouteManagement()
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<RouteModel> objModel = objAdminData.GetTransportRoutes(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateRoute(RouteModel objData)
        {
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objAdminData.InsertUpdateTransportRoute(objData);
            return RedirectToAction("RouteManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult GetTransportRouteStops(string id = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            RouteModel objModel = objAdminData.GetTransportRouteStops(ID);

            return PartialView("_RouteStoppagesPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult EditTransportRouteStop(string id = null, string ID2 = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            int RouteID = CommonUsage.ConvertToInt(ID2);
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            RouteStoppageModel objModel = objAdminData.GetTransportStopEditDetails(ID, RouteID, SBranchID);

            return PartialView("_TransportStopEditFormPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateStop(RouteStoppageModel objData)
        {
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            RouteModel objModel = objAdminData.InsertUpdateTransportStop(objData);

            return PartialView("_RouteStoppagesPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetTransportRouteVehicles(string id = null)
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            int ID = CommonUsage.ConvertToInt(id);
            RouteVehicleListModel objModel = objAdminData.GetTransportRouteVehicles(ID, SBranchID);

            return PartialView("_RouteVehicleListPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetTransportRouteVehicleStops(string id = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            IEnumerable<RouteStoppageModel> objModel = objAdminData.GetTransportRouteVehicleStops(ID);

            return PartialView("_VehicleRouteStoppagesPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateRouteVehicle(RouteVehicleModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            RouteVehicleListModel objModel = objAdminData.InsertUpdateTransportRouteVehicle(objData);

            return PartialView("_RouteVehicleListPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetLocationLists(string id = null, string ID2 = null)
        {

            int ID = CommonUsage.ConvertToInt(id);
            List<NameIDModel> objModel;
            if (ID2 == "0")
            {
                objModel = objAdminData.GetCountryStates(ID);
            }
            else if (ID2 == "1")
            {
                objModel = objAdminData.GetStateCities(ID);
            }
            else
            {
                objModel = objAdminData.GetCitieArea(ID);
            }
            return PartialView("_SelectOptionsPartial", objModel);
        }
        #endregion
        #region Hostal Management
        [PermissionFilter]
        public ActionResult HostalTypeManagement()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<HostalTypeModel> objModel = objAdminData.GetHostalTypes(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateHostalType(HostalTypeModel objData)
        {
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objAdminData.InsertUpdateHostalType(objData);
            return RedirectToAction("HostalTypeManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult HostalManagement()
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            HostelPageModel objModel = objAdminData.GetHostals(-1, SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateHostal(HostalModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = 1;// PermissionManager.GetLoggedInUser().UserID;
            objAdminData.InsertUpdateHostal(objData);
            return RedirectToAction("HostalManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult DeleteHostal(string id = null)
        {
            HostalModel objData = new HostalModel();
            objData.OpType = -1;
            objData.ID = CommonUsage.ConvertToInt(id);
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = 1;// PermissionManager.GetLoggedInUser().UserID;
            objAdminData.InsertUpdateHostal(objData);
            return RedirectToAction("HostalManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult GetHostelRoomTypes(string id = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            HostalModel objModel = objAdminData.GetHostalRoomTypes(ID);

            return PartialView("_RoomTypeManagePartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateHostelRoomType(HostalRoomTypeModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            HostalModel objModel = objAdminData.InsertUpdateHostelRoomType(objData);
            return PartialView("_RoomTypeManagePartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetHostelFloors(string id = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            HostalModel objModel = objAdminData.GetHostalFloors(ID);

            return PartialView("_HostelFloorManagePartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateHostelFloor(HostalFloorModel objData)
        {
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = 1;// PermissionManager.GetLoggedInUser().UserID;
            HostalModel objModel = objAdminData.InsertUpdateHostelFloor(objData);
            return PartialView("_HostelFloorManagePartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetHostelFloorRooms(string id = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            HostalFloorModel objModel = objAdminData.GetHostalFloorRooms(ID);

            return PartialView("_HostelRoomManagePartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateHostelFloorRoom(HostalRoomModel objData)
        {
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = 1;// PermissionManager.GetLoggedInUser().UserID;
            HostalFloorModel objModel = objAdminData.InsertUpdateHostelFloorRoom(objData);
            return PartialView("_HostelRoomManagePartial", objModel);
        }
        #endregion
        #region Notice Management
        [PermissionFilter]
        public ActionResult NoticeManagement()
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            NoticePageModel objModel = objAdminData.GetNotices(SBranchID);
            return View(objModel);
        }

        [PermissionFilter]
        public ActionResult SendNoticeSMS(SMSSendingModel data)
        {
            if (data.AssociatedIDs == "")
            {
                data.AssociatedIDs = "0";
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            List<SMSRecieverModel> recievers = objAdminData.GetSMSRecieverList(SBranchID, data.AssociatedIDs, data.RecieverType);
            foreach (SMSRecieverModel r in recievers)
            {
                if (r.MobileNo != null && r.MobileNo != "" && String.IsNullOrEmpty(r.deviceToken))
                {
                    SMSSender objSender = new SMSSender();
                    objSender.SendSMSAsync(data.SMSText, r.MobileNo, SBranchID, 1, r.RecieverType, r.ID);
                }
            }
            NotificationModel objModel = new NotificationModel();
            CommonData objCommonData = new CommonData();
            objModel.RecieverID = -1;
            objModel.SBranchID = SBranchID;
            objModel.NotificationType = 1;
            objModel.RecieverType = data.RecieverType;
            objModel.NotificationText = data.SMSText;
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
            string NotificationServerKey = PermissionManager.GetLoggedInUser().NotificationServerKey;
            string[] Recievers = sb.ToString().Trim().Split("#".ToCharArray());
            CommonUsage.SendNotificationFCM(Recievers, data.SMSText, "1", NotificationServerKey);
            objCommonData.InsertNotification(objModel);

            objAdminData.UpdateSenderSMSSentStatus(1, data.ID, data.AssociatedIDs, data.RecieverType);
            return RedirectToAction("NoticeManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult UpdateNotice(NoticeModel objData)
        {
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            if (objData.OpType == -1)
            {
                objData.NoticeStartDate = CommonUsage.GetCurrentDate();
                objData.NoticeEndDate = CommonUsage.GetCurrentDate();
            }
            objAdminData.InsertUpdateNotice(objData);

            //List<SMSRecieverModel> recievers = objAdminData.GetSMSRecieverList(objData.SBranchID, objData.ClassesIncludedIDs, objData.ApplicableFor);
            //NotificationModel objModel = new NotificationModel();
            //CommonData objCommonData = new CommonData();
            //objModel.RecieverID = -1;
            //objModel.SBranchID = objData.SBranchID;
            //objModel.NotificationType = 1;
            //objModel.RecieverType = objData.ApplicableFor;
            //objModel.NotificationText = objData.NoticeTitle;
            //objModel.NotificationDateTime = CommonUsage.GetCurrentDate();
            //objModel.Recievers = new List<NotificationRecieverModel>();

            //StringBuilder sb = new StringBuilder();
            //foreach (SMSRecieverModel r in recievers)
            //{
            //    if (!String.IsNullOrEmpty(r.deviceToken))
            //    {
            //        if (r.deviceToken.Contains("\\n"))
            //        {
            //            r.deviceToken.Replace("\\n", "#");
            //        }
            //        if (sb != null && sb.ToString() != "")
            //        {
            //            sb.Append("#");
            //        }
            //        sb.Append(r.deviceToken);
            //    }
            //}
            //string NotificationServerKey = PermissionManager.GetLoggedInUser().NotificationServerKey;
            //string[] Recievers = sb.ToString().Trim().Split("#".ToCharArray());
            //CommonUsage.SendNotificationFCM(Recievers, objData.NoticeTitle, "1", NotificationServerKey);
            //objCommonData.InsertNotification(objModel);

            return RedirectToAction("NoticeManagement", "Admin");
        }
        #endregion
        #region Leave Management
        [PermissionFilter]
        public ActionResult LeaveTypeManagement()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<LeaveTypeModel> objModel = objAdminData.GetLeaveTypes(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateLeaveType(LeaveTypeModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objAdminData.UpdateLeaveType(objData);
            return RedirectToAction("LeaveTypeManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult LeaveManagement()
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<LeaveModel> objModel = objAdminData.GetLeaves(SBranchID, -1, 0);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GetLeaveDetails(string ID = null)
        {
            LeaveEditModel model = new LeaveEditModel();
            int LeaveID = CommonUsage.ConvertToInt(ID);
            model = objAdminData.GetAdminLeaveDetails(LeaveID);

            return PartialView("_LeaveDetailView", model);
        }
        [PermissionFilter]
        public ActionResult UpdateLeave(LeaveModel objData)
        {
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            if (objData.OpType == -1)
            {
                objAdminData.DeleteLeave(objData);
            }
            else if (objData.LeaveID == 0)
            {
                objAdminData.InsertLeave(objData);
            }
            else
            {
                objAdminData.UpdateLeave(objData);
            }
            return RedirectToAction("LeaveManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult UpdateLeaveStatus(LeaveModel objData)
        {
            objAdminData.UpdateLeaveStatus(objData);
            return RedirectToAction("LeaveManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult GetSectionStudents(string ID = null)
        {
            int LeaveID = CommonUsage.ConvertToInt(ID);
            IEnumerable<NameIDModel> model = objAdminData.GetSectionStudents(LeaveID);

            return PartialView("_SelectOptionsPartial", model);
        }
        [PermissionFilter]
        public ActionResult GetEmployeeLeaveDetails(string id = null, string id2 = null)
        {
            int EmpID = CommonUsage.ConvertToInt(id);
            DateTime leaveDate = CommonUsage.ConvertToDateTime(id2);
            LeaveEditModel objModel = objAdminData.GetEmployeeLeaveDetails(EmpID, leaveDate);

            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Email Management
        [PermissionFilter]
        public ActionResult EmailSetting()
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<MasterSettingModel> objModel = objAdminData.GetMasterSettings("Email", -1, SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateEmailSetting(EmailSettingModel objData)
        {
            MasterSettingModel objModel = new MasterSettingModel();
            objModel.OperationDate = CommonUsage.GetCurrentDate();
            objModel.UserID = PermissionManager.GetLoggedInUser().UserID;
            objModel.Description = objData.Description;
            objModel.ID = objData.SettingID;
            objModel.Name = objData.Title;
            objModel.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objModel.Status = objData.Status;
            objModel.Type = "Email";
            objModel.Details = objData.Host + "\n" + objData.Port + "\n" + objData.UserName + "\n" + objData.Password + "\n" + objData.SSL;
            objAdminData.InsertUpdateMasterSetting(objModel);
            return RedirectToAction("EmailSetting", "Admin");
        }
        #endregion
        #region Evaluation Management
        [PermissionFilter]
        public ActionResult GetEvaluationSchemes(string id = null)
        {
            int SessionID = CommonUsage.ConvertToInt(id);
            List<EvaluationSchemeModel> objModel = objAdminData.GetSessionSchemes(SessionID).ToList();

            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult EvaluationSchemeManagement(string ID = null)
        {
            int SessionID = CommonUsage.ConvertToInt(ID);
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            EvaluationSchemePageModel objModel = objAdminData.GetEvaluationSchemes(SBranchID, SessionID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateEvaluationScheme(EvaluationSchemeModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());

            objAdminData.InsertUpdateEvaluationScheme(objData);
            return RedirectToAction("EvaluationSchemeManagement", "Admin", new { id = objData.SessionID });
        }
        [PermissionFilter]
        public ActionResult EvaluationTypeManagement(string ID = null, string ID2 = null)
        {
            int SessionID = CommonUsage.ConvertToInt(ID);
            int EvaluationSchemeID = CommonUsage.ConvertToInt(ID2);
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            EvaluationTypePageModel objModel = objAdminData.GetEvaluationTypes(SBranchID, SessionID, EvaluationSchemeID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateEvaluationType(EvaluationTypeModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());

            objAdminData.InsertUpdateEvaluationType(objData);
            return RedirectToAction("EvaluationTypeManagement", "Admin", new { id = objData.SessionID, id2 = objData.EvaluationSchemeID });
        }
        [PermissionFilter]
        public ActionResult EvaluationManagement(string ID = null, string ID2 = null)
        {
            int SessionID = CommonUsage.ConvertToInt(ID);
            int SchemeID = CommonUsage.ConvertToInt(ID2);
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            EvaluationPageModel objModel = objAdminData.GetEvaluations(SBranchID, SessionID, SchemeID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateEvaluation(EvaluationModel objData)
        {
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            if (objData.OpType == -1)
            {
                objData.ResultDate = CommonUsage.GetCurrentDate();
            }
            objAdminData.InsertUpdateEvaluation(objData);
            return RedirectToAction("EvaluationManagement", "Admin", new { id = objData.SessionID, id2 = objData.EvaluationSchemeID });
        }
        [PermissionFilter]
        public ActionResult GetSubEvaluations(string ID = null)
        {
            SubEvaluationListModel objModel = new SubEvaluationListModel();
            objModel.EvaluationID = CommonUsage.ConvertToInt(ID);

            objAdminData.GetSubEvaluations(objModel);
            return PartialView("_EvaluationDetailsPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateSubEvaluations(EvaluationModel data)
        {
            SubEvaluationListModel objModel = new SubEvaluationListModel();
            objModel.EvaluationID = data.EvaluationID;
            data.OperationDate = CommonUsage.GetCurrentDate();
            data.UserID = PermissionManager.GetLoggedInUser().UserID;
            data.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objModel = objAdminData.InsertUpdateSubEvaluation(data);
            return PartialView("_EvaluationDetailsPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult ExamManagement(ExamPageModel Model = null)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            Model.UserID = PermissionManager.GetLoggedInUser().UserID;
            Model.CurrentDate = CommonUsage.GetCurrentDate();
            Model.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            Model = objAdminData.GetExams(Model);
            return View(Model);
        }
        [PermissionFilter]
        public ActionResult GetClassGroups(string id = null, string id2 = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            int ID2 = CommonUsage.ConvertToInt(id2);
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());

            ExamPageModel objModel = objAdminData.GetClassGroupEvaluations(ID, 2, SBranchID, ID2);
            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult GetClassGroupList(string id = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());

            List<NameIDModel> objModel = objAdminData.GetClassGroups(ID, SBranchID).ToList();
            return PartialView("_SelectOptionsPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateExams(ExamPageModel objData)
        {
            //objData.OperationDate = CommonUsage.GetCurrentDate();
            //objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            //objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objData.CurrentDate = CommonUsage.GetCurrentDate();
            if (objData.Exams != null)
            {
                objAdminData.InsertUpdateExam(objData);
            }
            objData.Exams = null;
            return RedirectToAction("ExamManagement", "Admin", objData);
        }
        #endregion
        #region Group/Subject Management
        [PermissionFilter]
        public ActionResult GroupManagement()
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            GroupEditModel Model = objAdminData.GetGroupsForAdmin(SBranchID);
            return View(Model);
        }
        [PermissionFilter]
        public async Task<ActionResult> ImportantContacts()
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            List<ImportantContactModel> Model = await objAdminData.GetImportantContact(SBranchID);
            return View(Model);
        }
        [PermissionFilter]
        public ActionResult UpdateImportantContact(ImportantContactModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());

            objAdminData.UpdateImportantContacts(objData);

            return RedirectToAction("ImportantContacts", "Admin");
        }
        [PermissionFilter]
        public ActionResult UpdateGroup(GroupModel objData)
        {
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());

            objAdminData.InsertUpdateGroup(objData);

            return RedirectToAction("GroupManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult GetGroupSubjects(string id = null)
        {
            int ID = CommonUsage.ConvertToInt(id);

            GroupModel objModel = objAdminData.GetGroupSubjects(ID);
            return PartialView("_GroupSubjectsPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetGroupSubjectTypes(string id = null)
        {
            SubSubjectTypeEditModel oModel = new SubSubjectTypeEditModel();
            oModel.GroupID = CommonUsage.ConvertToInt(id);
            oModel.Types = objAdminData.GetGroupSubjectTypes(oModel.GroupID);
            return PartialView("_SubSubjectTypesPartial", oModel);
        }
        [PermissionFilter]
        public ActionResult UpdateGroupSubjectType(SubSubjectTypeModel objData)
        {
            SubSubjectTypeEditModel oModel = new SubSubjectTypeEditModel();
            oModel.GroupID = objData.GroupID;
            oModel.Types = objAdminData.UpdateGroupSubjectTypes(objData);
            return PartialView("_SubSubjectTypesPartial", oModel);
        }
        [PermissionFilter]
        public ActionResult UpdateGroupSubject(SubjectModel objData)
        {
            GroupModel objModel = objAdminData.InsertUpdateGroupSubject(objData);
            if (objData.MainSubID == 0)
            {
                return PartialView("_GroupSubjectsPartial", objModel);
            }
            else
            {
                SubSubjectEditModel oModel = objAdminData.GetSubSubjectsPageModel(objData.MainSubID);
                return PartialView("_SubSubjectsPartial", oModel);
            }
        }
        [PermissionFilter]
        public ActionResult GetSubSubjects(string id = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            SubSubjectEditModel oModel = objAdminData.GetSubSubjectsPageModel(ID);
            return PartialView("_SubSubjectsPartial", oModel);
        }
        [PermissionFilter]
        public ActionResult GetSubSubjectsOld(string id = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            ViewBag.MainSubID = ID;
            List<SubjectModel> objModel = objAdminData.GetSubSubjects(ID);
            return PartialView("_SubSubjectsPartial", objModel);
        }
        #endregion
        #region Fee Management
        [PermissionFilter]
        public ActionResult ClassSessionNoFeeMonth(ClassPageModel objModel)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            if (objModel.NoFeeMonths != null)
            {
                objModel.SBranchID = SBranchID;
                objAdminData.UpdateSessionNoFeeMonths(objModel);
            }
            objModel = objAdminData.GetClassesNoFeeMonths(SBranchID, objModel.SessionID);
            return View(objModel);
        }

        [PermissionFilter]
        public ActionResult FeeCategoryManagement()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            FeeCategoryEditModel Model = objAdminData.GetFeeTypes(SBranchID);
            return View(Model);
        }
        [PermissionFilter]
        public ActionResult UpdateFeeType(FeeCategoryModel objData)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objAdminData.InsertUpdateFeeType(objData);
            return RedirectToAction("FeeCategoryManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult FeeStructureManagement(ClassFeeStructureEditModel Model = null)
        {
            if (Model == null)
            {
                Model = new ClassFeeStructureEditModel();
            }
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            Model.OperationDate = CommonUsage.GetCurrentDate();
            Model.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            Model = objAdminData.GetFeeStructure(Model);
            return View(Model);
        }
        [PermissionFilter]
        public ActionResult UpdateClassFee(ClassFeeStructureEditModel objData)
        {
            if (objData.FeeStructure != null)
            {
                objAdminData.InsertUpdateFeeStructure(objData);
            }
            objData.FeeStructure = null;
            return RedirectToAction("FeeStructureManagement", "Admin", objData);
        }
        #endregion
        #region Salary Management
        [PermissionFilter]
        public ActionResult SalaryCategoryManagement()
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            SalaryCategoryPageModel Model = objAdminData.GetSalaryTypes(SBranchID);
            return View(Model);
        }
        [PermissionFilter]
        public ActionResult UpdateSalaryType(SalaryCategoryModel objData)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objAdminData.InsertUpdateSalaryType(objData);
            return RedirectToAction("SalaryCategoryManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult EmployeeTypeManagement()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<EmployeeTypeModel> Model = objAdminData.GetEmployeeTypes(SBranchID);
            return View(Model);
        }
        [PermissionFilter]
        public ActionResult UpdateEmployeeType(EmployeeTypeModel objData)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAdminData.InsertUpdateEmployeeType(objData);
            return RedirectToAction("EmployeeTypeManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult GetEmployeeTypeSalaries(string ID = null)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int EmployeeTypeID = CommonUsage.ConvertToInt(ID);
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            EmployeeTypeModel objModel = objAdminData.GetEmployeeSalaryDetails(EmployeeTypeID, SBranchID);
            return PartialView("_SalaryStructurePartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateSalaryStructure(EmployeeTypeModel objData)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            EmployeeTypeModel objModel = objAdminData.InsertUpdateSalaryStructure(objData);
            return PartialView("_SalaryStructurePartial", objModel);
        }
        #endregion
        #region Expence Management
        [PermissionFilter]
        public ActionResult ExpenceCategoryManagement()

        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            IEnumerable<ExpenceCategoryModel> Model = objAdminData.GetExpenceTypes(SBranchID);
            return View(Model);
        }
        [PermissionFilter]
        public ActionResult UpdateExpenceType(ExpenceCategoryModel objData)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objAdminData.InsertUpdateExpenceType(objData);
            return RedirectToAction("ExpenceCategoryManagement", "Admin");
        }
        #endregion
        #region Quota Management
        [PermissionFilter]
        public ActionResult QuotaManagement()
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<QuotaModel> Model = objAdminData.GetQuotas(SBranchID);
            return View(Model);
        }
        [PermissionFilter]
        public ActionResult UpdateQuota(QuotaModel objData)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objAdminData.InsertUpdateQuota(objData);
            return RedirectToAction("QuotaManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult GetQuotaDiscounts(string ID = null, string ID2 = null)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            QuotaModel Model = new QuotaModel();
            Model.QuotaID = CommonUsage.ConvertToInt(ID);
            Model.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            Model.OperationDate = CommonUsage.GetCurrentDate();
            Model.SessionID = CommonUsage.ConvertToInt(ID2);
            QuotaModel objModel = objAdminData.GetQuotaDiscounts(Model);
            return PartialView("_QuotaDiscountsStructurePartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateQuotaDiscounts(QuotaModel objData)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objData.OperationDate = CommonUsage.GetCurrentDate();
            QuotaModel objModel = objAdminData.InsertUpdateQuotaDiscounts(objData);
            return PartialView("_QuotaDiscountsStructurePartial", objModel);
        }
        #endregion
        #region Expence Management
        [PermissionFilter]
        public ActionResult EventCategoryManagement()
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<EventCategoryModel> Model = objAdminData.GetEventTypes(SBranchID);
            return View(Model);
        }
        [PermissionFilter]
        public ActionResult UpdateEventType(EventCategoryModel objData)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objAdminData.InsertUpdateEventType(objData);
            return RedirectToAction("EventCategoryManagement", "Admin");
        }
        [PermissionFilter]
        public async Task<ActionResult> EventManagement()
        {
            int year = CommonUsage.GetCurrentDate().Year;
            int month = CommonUsage.GetCurrentDate().Month;

            if (Session["SBranchID"] == null)
                if (Session["SBranchID"] == null)
                {
                    Session["SBranchID"] = 1;
                }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            CommonData objCommonData = new CommonData();
            EventCalendarModel model =await objCommonData.GetEventCalander(month, year, SBranchID, 1);
            model.EventTypes = objAdminData.GetEventTypeList(SBranchID);
            model.Classes = objAdminData.GetClasses(1, SBranchID, 0).Classes;
            return View(model);
        }
        [PermissionFilter]
        public async Task<ActionResult> GetEventCalender(string id = null, string id2 = null)
        {
            int year = CommonUsage.ConvertToInt(id2);
            int month = CommonUsage.ConvertToInt(id);
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            CommonData objCommonData = new CommonData();
            EventCalendarModel model = await objCommonData.GetEventCalander(month, year, SBranchID, 1);

            return Json(model, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult UpdateEvent(CommonEvents objModel)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            objModel.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            int EventID = objAdminData.InsertUpdateEvent(objModel);

            if (objModel.IsApproved == 1)
            {
                List<SMSRecieverModel> recievers = objAdminData.GetSMSRecieverList(objModel.SBranchID, objModel.ClassesIncludedIDs, 5);
                NotificationModel objNModel = new NotificationModel();
                CommonData objCommonData = new CommonData();
                objNModel.RecieverID = -1;
                objNModel.SBranchID = objModel.SBranchID;
                objNModel.NotificationType = 6;
                objNModel.RecieverType = 3;
                objNModel.NotificationText = objModel.title;
                objNModel.NotificationDateTime = CommonUsage.GetCurrentDate();
                objNModel.Recievers = new List<NotificationRecieverModel>();

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
                string[] Recievers = sb.ToString().Trim().Split("#".ToCharArray());
                string FCNotificationServerKey = PermissionManager.GetLoggedInUser().NotificationServerKey;
                CommonUsage.SendNotificationFCM(Recievers, objNModel.NotificationText, "6", FCNotificationServerKey);
                objCommonData.InsertNotification(objNModel);
            }
            return Json(1, JsonRequestBehavior.AllowGet);
            //return RedirectToAction("EventCategoryManagement", "Admin", objModel);
        }
        #endregion

        #region Syllabus Management
        [PermissionFilter]
        public ActionResult SyllabusManagement(SyllabusModel Model = null)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            Model.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            Model = objAdminData.GetChapters(Model);
            return View(Model);
        }
        [PermissionFilter]
        public ActionResult GetGroupSubjectList(string ID = null)
        {
            int GroupID = CommonUsage.ConvertToInt(ID);
            IEnumerable<SubjectModel> objModel = objAdminData.GetGroupSubjectList(GroupID);
            return PartialView("_SelectSubjectOptionsPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateChapter(ChapterModel objData)
        {
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objAdminData.InsertUpdateChapter(objData);
            SyllabusModel model = new SyllabusModel();
            model.ClassID = objData.ClassID;
            model.SubjectID = objData.SubjectID;
            model.GroupID = objData.GroupID;
            return RedirectToAction("SyllabusManagement", "Admin", model);
        }
        [PermissionFilter]
        public ActionResult GetChapterTopics(string ID = null)
        {
            ChapterModel Model = new ChapterModel();
            Model.ChapterID = CommonUsage.ConvertToInt(ID);
            Model = objAdminData.GetChapterTopics(Model);
            return PartialView("_TopicTablePartial", Model);
        }
        [PermissionFilter]
        public ActionResult UpdateChapterTopics(TopicModel Model = null)
        {
            ChapterModel objModel = objAdminData.InsertUpdateChapterTopics(Model);
            return PartialView("_TopicTablePartial", objModel);
        }
        #endregion

        #region BranchManagement
        [PermissionFilter]
        public ActionResult BranchManagement()
        {
            var User = PermissionManager.GetLoggedInUser();
            int SBranchID = 0;
            if (User.RoleID == (int)RoleType.Principle)
            {
                SBranchID = User.SBranchID;
            }
            IEnumerable<SBranchModel> objModel = objAdminData.GetAdminBranches(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult DeleteBranch(string ID = null)
        {
            SBranchModel objModel = new SBranchModel();
            objModel.SBranchID = CommonUsage.ConvertToInt(ID);
            objModel.OpType = -1;
            int Res = objAdminData.InsertUpdateAdminBranch(objModel);
            try
            {
                var dir = new DirectoryInfo(Server.MapPath("~/images/SBranchLogo/"));
                foreach (var file in dir.EnumerateFiles(ID + "_*"))
                {
                    file.Delete();
                }

            }
            catch (Exception ex)
            {


            }
            return RedirectToAction("BranchManagement", "Admin");
        }
        public ActionResult CheckUserNameExist(string ID, string ID2)
        {
            int SBranchID = CommonUsage.ConvertToInt(ID2);
            int res = objAdminData.CheckUserNameExist(ID, SBranchID);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [PermissionFilter]
        public ActionResult UpdateBranch(SBranchModel objData)
        {
            string OldImageFile = "";
            string OldSignatureFile = "";
            if (objData.BranchLogo != null)
            {
                OldImageFile = objData.Logo;
                objData.Logo = objData.BranchLogo.FileName.Replace(" ", "-");
            }
            if (objData.PrincipalSignatureFile != null)
            {
                OldSignatureFile = objData.PrincipalSignature;
                objData.PrincipalSignature = objData.PrincipalSignatureFile.FileName.Replace(" ", "-");
            }
            objData.SBranchID = objAdminData.InsertUpdateAdminBranch(objData);
            Session["SBrancheList"] = null;
            if (objData.BranchLogo != null)
            {
                try
                {
                    System.IO.File.Delete(
                        Server.MapPath("~/images/SBranchLogo/" + objData.SBranchID + "_" + OldImageFile));

                }
                catch (Exception ex)
                {


                }
                var path = Path.Combine(Server.MapPath("~/images/SBranchLogo/"),
                   objData.SBranchID + "_" + objData.Logo);
                objData.BranchLogo.SaveAs(path);
            }
            if (objData.PrincipalSignatureFile != null)
            {
                try
                {
                    System.IO.File.Delete(
                        Server.MapPath("~/images/SBranchLogo/" + objData.SBranchID + "_P_" + OldSignatureFile));

                }
                catch (Exception ex)
                {


                }
                var path = Path.Combine(Server.MapPath("~/images/SBranchLogo/"),
                   objData.SBranchID + "_P_" + objData.PrincipalSignature);
                objData.PrincipalSignatureFile.SaveAs(path);
            }
            if (objData.OpType == 1)
            {

                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                return RedirectToAction("BranchManagement", "Admin");
            }
        }
        #endregion
        [PermissionFilter]
        public ActionResult LocationManagement()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<LocationModel> objModel = objAdminData.GetLocations(0, 3, SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GetLocation(string ID = null, string ID2 = null)
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            int MasterID = CommonUsage.ConvertToInt(ID);
            int Type = CommonUsage.ConvertToInt(ID2);
            LocationListModel objModel = new LocationListModel();
            objModel.MasterID = MasterID;
            objModel.Type = Type;
            objModel.List = objAdminData.GetLocations(MasterID, Type, SBranchID).ToList();

            return PartialView("_LocationTablePartial", objModel);
        }
        [PermissionFilter]
        public ActionResult AddLocation(LocationModel objData)
        {
            LocationListModel objModel = new LocationListModel();
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objModel.MasterID = objData.MasterID;
            objModel.Type = objData.Type;
            objModel.List = objAdminData.InsertUpdateLocation(objData).ToList();

            return PartialView("_LocationTablePartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateCity(LocationModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objData.Type = 3;
            objAdminData.InsertUpdateLocation(objData);
            return RedirectToAction("LocationManagement", "Admin");
        }
        #region Session Management
        [PermissionFilter]
        public ActionResult SessionManagement()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<SchoolSessionModel> objModel = objAdminData.GetSessions(-1, SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateSchoolSession(SchoolSessionModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objAdminData.InsertUpdateSessoin(objData);

            return RedirectToAction("SessionManagement", "Admin");
        }
        #endregion

        #region Website Related
        [PermissionFilter]
        public ActionResult NewsManagement()
        {
            IEnumerable<NewsModel> objModel = objAdminData.GetNews();
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateNews(NewsModel objData)
        {

            objData.ActiveDate = CommonUsage.GetCurrentDate();
            if (objData.AttachmentFile != null)
            {
                if (objData.Attachment != null && objData.NewsID != 0)
                {
                    System.IO.File.Delete(
                            Server.MapPath("~/Attachments/News/" + objData.NewsID + "_" + objData.Attachment));
                }
                objData.Attachment = objData.AttachmentFile.FileName.Replace(" ", "");
            }
            objData.NewsID = objAdminData.InsertUpdateNews(objData);
            if (objData.AttachmentFile != null)
            {
                objData.AttachmentFile.SaveAs(Server.MapPath("~/Attachments/News/" + objData.NewsID + "_" + objData.Attachment));
            }
            return RedirectToAction("NewsManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult Gallery()
        {
            IEnumerable<GalleryModel> objModel = objAdminData.GetGallery();
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GetGalleryDetails(string ID = null)
        {
            int GalleryID = CommonUsage.ConvertToInt(ID);
            GalleryModel objModel = objAdminData.GetGalleryDetails(GalleryID);

            return PartialView("_GalleryDetailsPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateGallery(GalleryModel Data)
        {
            Data.GalleryID = objAdminData.InsertUpdateGallery(Data);

            if (Data.OpType == -1)
            {
                var dirBlog = new DirectoryInfo(Server.MapPath("~/images/GalleryImages"));
                foreach (var file in dirBlog.EnumerateFiles(Data.GalleryID + "_*"))
                {
                    try
                    {
                        file.Delete();
                        System.IO.File.Delete(dirBlog + "Thumb\\" + file.Name);
                    }
                    catch (Exception ex)
                    {


                    }

                }
            }
            else
            {
                foreach (GalleryImageModel img in Data.Images)
                {
                    if (img.OpType == -1)
                    {
                        System.IO.File.Delete(
                            Server.MapPath("~/images/GalleryImages/Thumb/" + Data.GalleryID + "_" + img.OldImageName));
                        System.IO.File.Delete(
                            Server.MapPath("~/images/GalleryImages/" + Data.GalleryID + "_" + img.OldImageName));
                    }
                    else if (img.OldImageName != null && img.ImageFile != null)
                    {
                        System.IO.File.Delete(
                           Server.MapPath("~/images/GalleryImages/Thumb/" + Data.GalleryID + "_" + img.OldImageName));
                        System.IO.File.Delete(
                            Server.MapPath("~/images/GalleryImages/" + Data.GalleryID + "_" + img.OldImageName));


                        img.ImageFile.SaveAs(Server.MapPath("~/images/GalleryImages/" + Data.GalleryID + "_" + img.ImagePath));
                        CommonUsage.SaveThumbImage(Server.MapPath("~/images/GalleryImages/"),
                       Data.GalleryID + "_" + img.ImagePath);
                    }
                    else if (img.ImageFile != null)
                    {
                        img.ImageFile.SaveAs(Server.MapPath("~/images/GalleryImages/" + Data.GalleryID + "_" + img.ImagePath));
                        CommonUsage.SaveThumbImage(Server.MapPath("~/images/GalleryImages/"),
                       Data.GalleryID + "_" + img.ImagePath);
                    }
                }
            }
            return RedirectToAction("Gallery", "Admin");
        }
        [PermissionFilter]
        public ActionResult ThoughtOfTheDay()
        {
            IEnumerable<NameIDModel> objModel = objAdminData.GetThoughts();
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateThought(NameIDModel objData)
        {
            objAdminData.InsertUpdateThought(objData);

            return RedirectToAction("ThoughtOfTheDay", "Admin");
        }
        [PermissionFilter]
        public ActionResult HomeBanners()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<BannerImageModel> objModel = objAdminData.GetHomeBanners(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateHomeBanner(BannerImageModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objData.BannerDate = CommonUsage.GetCurrentDate();
            if (!Directory.Exists(Server.MapPath("~/Images/BannerImages/App/")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Images/BannerImages/App/"));
            }
            if (objData.OpType == -1)
            {
                System.IO.File.Delete(
                            Server.MapPath("~/Images/BannerImages/App/" + objData.ABID + "_" + objData.BImage));
            }
            if (objData.ImageFile != null)
            {
                if (objData.BImage != null && objData.ABID != 0)
                {
                    System.IO.File.Delete(
                            Server.MapPath("~/Images/BannerImages/App/" + objData.ABID + "_" + objData.BImage));
                }
                objData.BImage = objData.ImageFile.FileName.Replace(" ", "");
            }
            objData.ABID = objAdminData.InsertUpdateWebBanner(objData);
            if (objData.ImageFile != null)
            {
                objData.ImageFile.SaveAs(Server.MapPath("~/Images/BannerImages/App/" + objData.ABID + "_" + objData.BImage));
            }

            return RedirectToAction("HomeBanners", "Admin");
        }
        #endregion
        #region Report Section
        [PermissionFilter]
        public ActionResult ReportSectionClassTeacher()
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            SectionClassTeacherReportPageModel objModel = objAdminData.GetSectionClassTeacherReport(SBranchID);
            return View(objModel);
        }

        [PermissionFilter]
        public ActionResult UpdateSectionClassTeacher(SectionClassTeacherReportModel oModel)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int res = objAdminData.UpdateSectionClassTeacher(oModel);

            return RedirectToAction("ReportSectionClassTeacher", "Admin");
        }

        [PermissionFilter]
        public ActionResult ReportDailyCustomCollection(CollectionReportModel objModel)
        {
            objModel.ReportType = 2;
            if (objModel.FromDate.Year == 1)
            {
                objModel.FromDate = CommonUsage.GetCurrentDate();
                objModel.PaymentMode = -1;
            }
            if (objModel.ToDate.Year == 1)
            {
                objModel.ToDate = CommonUsage.GetCurrentDate();
            }

            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetCustomCollectionReport(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ReportDailyCollection(CollectionReportModel objModel)
        {
            objModel.ReportType = 1;
            if (objModel.FromDate.Year == 1)
            {
                objModel.FromDate = CommonUsage.GetCurrentDate();
                objModel.PaymentMode = -1;
            }
            if (objModel.ToDate.Year == 1)
            {
                objModel.ToDate = CommonUsage.GetCurrentDate();
            }

            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetCollectionReport(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult DeletePayment(PaymentModel oModel)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int res = objAdminData.DeletePayment(oModel.PaymentID);
            return RedirectToAction("ReportDailyCollection", "Admin", new { FromDate = oModel.PaymentDate, PaymentMode = oModel.PaymentMode });
        }
        [PermissionFilter]
        public ActionResult ReportDailyFeeCollection(CollectionReportModel objModel)
        {
            objModel.ReportType = 1;
            if (objModel.FromDate.Year == 1)
            {
                objModel.FromDate = CommonUsage.GetCurrentDate();
                objModel.PaymentMode = -1;
            }
            if (objModel.ToDate.Year == 1)
            {
                objModel.ToDate = CommonUsage.GetCurrentDate();
            }

            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetCollectionFeeReport(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ReportBetweenDatesCollection(CollectionReportModel objModel)
        {
            objModel.ReportType = 3;
            if (objModel.FromDate.Year == 1)
            {
                objModel.FromDate = CommonUsage.GetCurrentDate().AddDays(-7);
                objModel.PaymentMode = -1;
            }
            if (objModel.ToDate.Year == 1)
            {
                objModel.ToDate = CommonUsage.GetCurrentDate();
            }

            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetCollectionReport(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ReportBetweenDatesFeeCollection(CollectionReportModel objModel)
        {
            objModel.ReportType = 3;
            if (objModel.FromDate.Year == 1)
            {
                objModel.FromDate = CommonUsage.GetCurrentDate().AddDays(-7);
                objModel.PaymentMode = -1;
            }
            if (objModel.ToDate.Year == 1)
            {
                objModel.ToDate = CommonUsage.GetCurrentDate();
            }

            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetCollectionFeeReport(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ReportQuarterlyCollection(CollectionReportModel objModel)
        {
            objModel.ReportType = 4;
            if (objModel.QuarterID == 0)
            {
                objModel.QuarterID = 1;
                objModel.PaymentMode = -1;
            }
            if (objModel.FromDate.Year == 1)
            {
                objModel.FromDate = CommonUsage.GetCurrentDate().AddDays(-7);
            }
            if (objModel.ToDate.Year == 1)
            {
                objModel.ToDate = CommonUsage.GetCurrentDate();
            }

            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetCollectionReport(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ReportQuarterlyFeeCollection(CollectionReportModel objModel)
        {
            objModel.ReportType = 4;
            if (objModel.QuarterID == 0)
            {
                objModel.QuarterID = 1;
                objModel.PaymentMode = -1;
            }
            if (objModel.FromDate.Year == 1)
            {
                objModel.FromDate = CommonUsage.GetCurrentDate().AddDays(-7);
            }
            if (objModel.ToDate.Year == 1)
            {
                objModel.ToDate = CommonUsage.GetCurrentDate();
            }

            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetCollectionFeeReport(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ReportMonthlyCollection(CollectionReportModel objModel)
        {
            objModel.ReportType = 2;

            if (objModel.FromDate.Year == 1)
            {
                objModel.FromDate = CommonUsage.GetCurrentDate();
                objModel.PaymentMode = -1;
            }
            if (objModel.ToDate.Year == 1)
            {
                objModel.ToDate = CommonUsage.GetCurrentDate();
            }

            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetCollectionReport(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ReportMonthlyFeeCollection(CollectionReportModel objModel)
        {
            objModel.ReportType = 2;

            if (objModel.FromDate.Year == 1)
            {
                objModel.FromDate = CommonUsage.GetCurrentDate();
                objModel.PaymentMode = -1;
            }
            if (objModel.ToDate.Year == 1)
            {
                objModel.ToDate = CommonUsage.GetCurrentDate();
            }

            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetCollectionFeeReport(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ReportDatewiseFeeCollection(CollectionReportModel objModel)
        {
            objModel.ReportType = 2;

            if (objModel.FromDate.Year == 1)
            {
                objModel.FromDate = CommonUsage.GetCurrentDate();
                objModel.PaymentMode = -1;
                objModel.FromDate = new DateTime(objModel.FromDate.Year, objModel.FromDate.Month, 1);
                objModel.ToDate = objModel.FromDate.AddMonths(1).AddDays(-1);
            }
            else
            {
                objModel.FromDate = new DateTime(objModel.FromDate.Year, objModel.FromDate.Month, 1);
                objModel.ToDate = objModel.FromDate.AddMonths(1).AddDays(-1);
            }
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetDatewiseCollectionFeeReport(objModel);
            return View(objModel);
        }

        [PermissionFilter]
        public ActionResult ReportClassWiseFee(ClassWiseFeeDuePageModel objModel)
        {
            if (objModel.QDate.Year == 1)
            {
                objModel.QDate = CommonUsage.GetCurrentDate();
            }
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            objAccountData.GetClassWiseDueFeeDetailsNew(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ReportDueFee(DemandReciptListModel objData = null)
        {
            if (objData.DemandMonth.Year == 1)
            {
                objData.DemandMonth = CommonUsage.GetCurrentDate();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            //DemandReciptListModel objModel = objAccountData.GetDemandReciptData(SBranchID, objData.ClassID, objData.SectionID);

            DemandReciptListModel objModel = objAccountData.GetDemandReciptDataNew(SBranchID, objData.DemandMonth, objData.ClassID, objData.SectionID, objData.SessionID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult SessionStrengthReport()
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            SessionStrengthReport objModel = objAdminData.GetSessionStrengthReport(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult DiscountRequests(string ID = null)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int Status = CommonUsage.ConvertToInt(ID);
            AdminFeeDiscountRequestMaster objModel = new AdminFeeDiscountRequestMaster();
            objModel.Status = Status;
            objModel.Requests = objAdminData.GetFeeDiscountRequest(SBranchID, Status);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GetFeeDiscountDetails(string ID = null)
        {

            FeeDiscountRequestMaster objModel = objAdminData.GetFeeDiscountRequestDetails(ID);

            return PartialView("_FeeDiscountRequestForm", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateFeeDiscountRequest(FeeDiscountRequestMaster objModel)
        {
            objModel.RequestDate = CommonUsage.GetCurrentDate();
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAdminData.UpdateFeeDiscountRequest(objModel);
            return Redirect("~/Admin/DiscountRequests/" + objModel.Status);
        }
        [PermissionFilter]
        public ActionResult MonthlyAttendance(MonthlyAttendancePageModel objModel)
        {
            objModel.BranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objModel.RepoDate.Year == 1)
            {
                objModel.RepoDate = CommonUsage.GetCurrentDate();
                objModel.EmployeeType = 1;
            }
            objAdminData.GetMonthlyAttendance(objModel);
            return View(objModel);
        }

        [PermissionFilter]
        public ActionResult MonthlyAttendanceStatus(MonthlyAttendancePageModel objModel)
        {
            objModel.BranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objModel.RepoDate.Year == 1)
            {
                objModel.RepoDate = CommonUsage.GetCurrentDate();
                objModel.EmployeeType = 1;
            }
            objAdminData.GetMonthlyAttendanceStatus(objModel);
            return View(objModel);
        }

        #endregion

        #region Backup
        public ActionResult TakeBackup()
        {
            string backupDIR = Server.MapPath(CommonUsage.DatabasebackupDirecotry);
            if (!System.IO.Directory.Exists(backupDIR))
            {
                System.IO.Directory.CreateDirectory(backupDIR);
            }
            string filename = DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".ps";
            SMEnterpriseDB.DBModels.BackupMasterModel objbkp = objAdminData.TakeBackup();

            FileStream ms = new FileStream(backupDIR + "\\" + filename, FileMode.Create);
            //Format the object as Binary

            BinaryFormatter formatter = new BinaryFormatter();
            //It serialize the employee object
            formatter.Serialize(ms, objbkp);
            ms.Flush();
            ms.Close();
            ms.Dispose();

            return Json(filename, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RestoreBackup()
        {
            string backupDIR = Server.MapPath(CommonUsage.DatabasebackupDirecotry);
            string filename = "29102017_203330.ps";
            Deserialize(backupDIR + "\\" + filename);
            return View();

        }
        public void Deserialize(String filename)
        {
            //Format the object as Binary
            BinaryFormatter formatter = new BinaryFormatter();

            //Reading the file from the server
            FileStream fs = new FileStream(filename, FileMode.Open);
            object obj = formatter.Deserialize(fs);
            SMEnterpriseDB.DBModels.BackupMasterModel emps = (SMEnterpriseDB.DBModels.BackupMasterModel)obj;
            fs.Flush();
            fs.Close();
            fs.Dispose();
        }
        #endregion
        #region Inventory Related
        [PermissionFilter]
        public ActionResult ProductCategories()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<ProductCategoryModel> objModel = objAdminData.GetProductCategories(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateProductCategory(ProductCategoryModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            int res = objAdminData.UpdateProductCategory(objData);
            return RedirectToAction("ProductCategories", "Admin");
        }
        [PermissionFilter]
        public ActionResult Products(ProductsPageModel oModel)
        {
            oModel.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            oModel = objAdminData.GetProducts(oModel);
            return View(oModel);
        }
        [PermissionFilter]
        public ActionResult DeleteProduct(int ProductID)
        {
            ProductModel oModel = new ProductModel();
            //oModel.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());

            objAdminData.DeleteProduct(ProductID);
            return View(oModel);
        }

        [PermissionFilter]
        public ActionResult GetProductDetails(string ID = null)
        {
            int ProductID = CommonUsage.ConvertToInt(ID);
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            ProductsPageModel objModel;
            objModel = objAdminData.GetProductDetails(ProductID, SBranchID);
            if (ProductID == 0)
            {
                objModel.Product = new ProductModel();
            }

            return PartialView("_ProductDetailsPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateProduct(ProductModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            string OldImageFile = "";
            if (objData.ProductPhoto != null)
            {
                OldImageFile = objData.Photo;
                objData.Photo = objData.ProductPhoto.FileName.Replace(" ", "-");
            }
            objData.ProductID = objAdminData.UpdateProduct(objData);
            string path = Server.MapPath("~/images/Products/");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (objData.ProductPhoto != null)
            {
                try
                {
                    System.IO.File.Delete(
                        Server.MapPath("~/images/Products/" + objData.ProductID + "_" + OldImageFile));

                }
                catch (Exception ex)
                {


                }
                path = Path.Combine(Server.MapPath("~/images/Products/"),
                   objData.ProductID + "_" + objData.Photo);
                objData.ProductPhoto.SaveAs(path);
            }

            return RedirectToAction("Products", "Admin", new { CategoryID = objData.ProductCategoryID });
        }


        [PermissionFilter]
        public ActionResult LowStockProducts()
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            List<ProductModel> objModel = objAdminData.GetLowStockProducts(SBranchID);
            return View(objModel);
        }
        #endregion

        #region SMS Management
        [PermissionFilter]
        public ActionResult SMSRechargeHistory()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            List<SMSRequestModel> Model = objAccountData.GetSMSRequestHistory(SBranchID);
            return View(Model);
        }
        [PermissionFilter]
        public ActionResult UpdateSMSRequest(SMSRequestModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objAccountData.InsertUpdateSMSRequest(objData);
            if (objData.Status != 2)
            {
                string SMSText = objData.SMSCredited + " SMS Requested on " + objData.RequestDate.ToString("dd, MMM yyyy");
                //SMSSender.SendRequestSMS(SMSText, StartupModel.RequestSMSNumber, objData.SBranchID);
            }
            return RedirectToAction("SMSRechargeHistory", "Admin");
        }
        [PermissionFilter]
        public ActionResult SMSTemplates()
        {
            SMSTemplatePageModel Model = objAccountData.GetSMSTemplates();
            return View(Model);
        }
        [PermissionFilter]
        public ActionResult SMSHistory()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            List<SMSSendTaskModel> Model = objAccountData.GetSMSSendingHistory(SBranchID);
            return View(Model);
        }
        [PermissionFilter]
        public ActionResult InsertSMSTemplate(SMSTemplateModel objData)
        {
            objAccountData.InsertUpdateSMSTemplate(objData);
            string SMSText = " SMS Template Approval Requested";
            //SMSSender.SendRequestSMS(SMSText, StartupModel.RequestSMSNumber, 1);
            return RedirectToAction("SMSTemplates", "Admin");
        }

        [PermissionFilter]
        public ActionResult UpdateSMSStatus()
        {
            //var hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
            //hubContext.Clients.All.GetStatus(1, 1);

            return RedirectToAction("SendSMS", "Admin");
        }
        [PermissionFilter]
        public ActionResult SendSMSList(SMSSendTaskModel objModel)
        {
            objModel.SMSSendDate = CommonUsage.GetCurrentDate();
            if (String.IsNullOrEmpty(objModel.StartTime))
            {
                objModel.StartTime = "00:00:00";
            }
            if (String.IsNullOrEmpty(objModel.EndTime))
            {
                objModel.EndTime = "00:00:00";
            }
            objModel.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objModel.SMSSendingID = objAccountData.InsertSMSSending(objModel);
            if (Session["SMSConfiguration"] == null)
            {
                Session["SMSConfiguration"] = objAdminData.GetDefaultSMSConfigurationDetails(objModel.SBranchID);
            }
            SMSConfigirationModel SMSConfigiration = (SMSConfigirationModel)Session["SMSConfiguration"];
            DeligateTasks objDT = new DeligateTasks();
            objDT.StartSending(objModel, SMSConfigiration);
            //return Redirect("/Account/SendSMS/"+ objModel.SMSSendingID);
            return Json(1, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult CreateSMS(SMSCreateModel objModel)
        {
            if (objModel.Date.Year == 1 || objModel.SelectedSMSType == 1)
            {
                objModel.Date = CommonUsage.GetCurrentDate();
            }
            if (String.IsNullOrEmpty(objModel.ReciverCats) || objModel.ReciverCats == "null")
            {
                objModel.ReciverCats = "0";
            }
            if (objModel.SelectedSMSType == 0)
            {
                objModel.SelectedSMSType = 1;
            }
            objModel.Year = objModel.Date.Year;
            objModel.Month = objModel.Date.Month;
            objModel.Day = objModel.Date.Day;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAccountData.GetCreateSMSPageData(objModel);

            return View(objModel);
        }

        [ChildActionOnly]
        public ActionResult TaskList()
        {
            List<RunningSMSTaskModel> Model = new List<RunningSMSTaskModel>();
            List<string> TaskIDs = new List<string>();
            foreach (var task in DeligateTasks.CurrentTasks)
            {
                bool isExist = false;
                foreach (RunningSMSTaskModel r in Model)
                {
                    if (r.TaskID == task.Value.SMSSendingID)
                    {
                        isExist = true;
                        r.CompletedCount = r.CompletedCount + task.Value.ProcessCount;
                        r.TotalCount = r.TotalCount + task.Value.TotalCount;
                        r.Progress = r.CompletedCount * 100 / r.TotalCount;
                        r.RecieverList = r.RecieverList;
                    }
                }
                if (!isExist)
                {
                    RunningSMSTaskModel r = new RunningSMSTaskModel();
                    r.TaskID = task.Value.SMSSendingID;
                    r.Title = task.Value.Title;
                    r.CompletedCount = task.Value.ProcessCount;
                    r.TotalCount = task.Value.TotalCount;
                    r.Progress = r.CompletedCount * 100 / r.TotalCount;
                    r.RecieverList = task.Value.RecieverList;
                    Model.Add(r);
                }
            }
            return PartialView("~/Views/Shared/_TaskProgressesPartial.cshtml", Model);
        }
        [PermissionFilter]
        public ActionResult SendSMS(string id = null)
        {
            int SMSSendingID = CommonUsage.ConvertToInt(id);
            SMSSendTaskModel data = objAccountData.GetSMSSendingDetails(SMSSendingID);
            return View(data);
        }
        [PermissionFilter]
        public ActionResult GetRecieverList(SMSCreateModel objData)
        {
            if (String.IsNullOrEmpty(objData.ReciverCats) || objData.ReciverCats == "null")
            {
                objData.ReciverCats = "0";
            }
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            List<SMSRecieverDetailModel> objModel = objAccountData.GetSMSRecieverList(objData);
            return PartialView("_RecieverListPartial", objModel);
        }
        #endregion

        #region Student Operation

        [PermissionFilter]
        public ActionResult StudentList(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objModel = objAccountData.GetStudents(objModel.ClassID, objModel.SectionID, SBranchID, objModel.SessionID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GenerateTC(TCDetailsModel objModel)
        {
            objModel.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            if (objModel.TCDetails == null)
            {
                objAdminData.GetTCDetails(objModel);
                return View(objModel);
            }
            else
            {
                objModel.TCDetails.TCID = objAdminData.InsertUpdateTC(objModel.TCDetails);
                objAdminData.GetTCDetails(objModel);
                ViewBag.Message = "Success";
                return View(objModel);
            }
        }
        #endregion
        #region SMS Configuration
        [PermissionFilter]
        public ActionResult SMSConfigurations()
        {
            var SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            IEnumerable<SMSConfigirationModel> objModel = objAdminData.GetSMSConfigurations(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult SMSConfigurationDetails(string ID = null)
        {
            var SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            var SMSConfigID = CommonUsage.ConvertToInt(ID);
            SMSConfigirationModel objModel = objAdminData.GetSMSConfigurationDetails(SMSConfigID, SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateSMSConfiguration(SMSConfigirationModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objData.CreatedDate = CommonUsage.GetCurrentDate();
            objAdminData.UpdateSMSConfigurationSettings(objData);
            return RedirectToAction("SMSConfigurations", "Admin");
        }
        #endregion
        #region Assignments
        [PermissionFilter]
        public ActionResult Assignments(AssignmentPageModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objModel.FromDate.Year == 1)
            {
                objModel.EndDate = CommonUsage.GetCurrentDate();
                objModel.FromDate = objModel.EndDate.AddDays(-7);
            }
            objModel = objAdminData.GetAssignments(objModel);
            return View(objModel);
        }
        #endregion
        #region BlackBoard
        [PermissionFilter]
        public ActionResult BlackBoard(BlackBoardAdminPageModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objModel.BBDate.Year == 1)
            {
                objModel.BBDate = CommonUsage.GetCurrentDate();
            }
            objModel = objAdminData.GetBlackBoardPageData(objModel);
            return View(objModel);
        }
        [HttpPost]
        public ActionResult GetBlackBoardImages(string ID = null)
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());

            List<BlackBoardImageModel> oModel = objAdminData.GetBlackBoardImages(ID);
            return Json(oModel, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Youtube Videos
        [PermissionFilter]
        public ActionResult YouTubeVideos(YouTubeAdminPageModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objModel.UploadDate.Year == 1)
            {
                objModel.UploadDate = CommonUsage.GetCurrentDate();
            }
            objModel = objAdminData.GetYouTubeVideos(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult EditYouTubeVideo(string ID = null)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int VideoID = CommonUsage.ConvertToInt(ID);

            YoutubeAdminEditPageData objModel = objAdminData.GetYouTubeVideosDetails(SBranchID, VideoID);
            return View(objModel);
        }
        [HttpPost]
        public ActionResult GetSectionSubjectsStudents(string ID = null, string ID2 = null, string ID3 = null)
        {
            int SectionID = CommonUsage.ConvertToInt(ID);
            int SessionID = CommonUsage.ConvertToInt(ID2);
            int VideoID = CommonUsage.ConvertToInt(ID3);
            YoutubeAdminEditPageData oModel = objAdminData.GetSectionSubjectAndStudents(SectionID, SessionID, VideoID);
            return Json(oModel, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult UpdateYoutubeVideo(YoutubeAdminEditPageData objModel)
        {
            objModel.Video.UploadDate = CommonUsage.GetCurrentDate();
            //objModel.RequestDate = CommonUsage.GetCurrentDate();
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel.Video.VideoID = objAdminData.UpdateYoutubeVideo(objModel);
            return Redirect("~/Admin/EditYouTubeVideo/" + objModel.Video.VideoID);
        }
        #endregion
        #region Nationality/Religion/Social Category/Mother Tongue
        [PermissionFilter]
        public ActionResult NationalityManagement()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<NameIDModel> objModel = objAdminData.GetNationalities(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateNationality(NameIDModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            int res = objAdminData.InsertUpdateNationality(objData);
            return RedirectToAction("NationalityManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult ReligionManagement()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<NameIDModel> objModel = objAdminData.GetReligions(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateReligion(NameIDModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            int res = objAdminData.InsertUpdateReligion(objData);
            return RedirectToAction("ReligionManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult MotherTongueManagement()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<NameIDModel> objModel = objAdminData.GetMotherTongues(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateMotherTongue(NameIDModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            int res = objAdminData.InsertUpdateMotherTongue(objData);
            return RedirectToAction("MotherTongueManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult SocialCategoryManagement()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<NameIDModel> objModel = objAdminData.GetSocialCategories(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateSocialCategory(NameIDModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            int res = objAdminData.InsertUpdateSocialCategory(objData);
            return RedirectToAction("SocialCategoryManagement", "Admin");
        }
        [PermissionFilter]
        public ActionResult GetSocialSubCategories(string ID = null)
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            int MasterID = CommonUsage.ConvertToInt(ID);
            IEnumerable<NameIDModel> objModel = objAdminData.GetSocialSubCategories(SBranchID, MasterID);

            return PartialView("_SocialSubCategoriesPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateSocialSubCategory(NameIDModel objData)
        {
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            int res = objAdminData.InsertUpdateSocialSubCategory(objData);
            return RedirectToAction("SocialCategoryManagement", "Admin");
        }
        #endregion
        #region Extra Income Related
        [PermissionFilter]
        public ActionResult ExtraIncomeHeads()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<ExtraIncomeHeadModel> objModel = objAdminData.GetExtraIncomeHeads(SBranchID);

            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateExtraIncomeHead(ExtraIncomeHeadModel objData)
        {
            objData.CreatedDate = CommonUsage.GetCurrentDate();
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            int res = objAdminData.InsertUpdateExtraIncomeHead(objData);
            return RedirectToAction("ExtraIncomeHeads", "Admin");
        }
        #endregion

        #region Day Book
        [PermissionFilter]
        public ActionResult DayBook(CollectionReportModel objModel)
        {


            if (objModel.FromDate.Year == 1)
            {
                objModel.FromDate = CommonUsage.GetCurrentDate();
                objModel.PaymentMode = -1;
                objModel.FromDate = new DateTime(objModel.FromDate.Year, objModel.FromDate.Month, 1);
                objModel.ToDate = objModel.FromDate.AddMonths(1).AddDays(-1);
            }
            else
            {
                objModel.FromDate = new DateTime(objModel.FromDate.Year, objModel.FromDate.Month, 1);
                objModel.ToDate = objModel.FromDate.AddMonths(1).AddDays(-1);
            }
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetDayBookMonthlyReport(objModel);
            return View(objModel);
        }

        [PermissionFilter]
        public ActionResult DayBookHead()
        {
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<DayBookHeadModel> objModel = objAdminData.GetDayBookHead(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateDayBookHead(DayBookHeadModel objData)
        {
            // objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            int res = objAdminData.UpdateDayBookHead(objData);
            return RedirectToAction("DayBookHead", "Admin");
            //return View(objData);
        }


        //[PermissionFilter]
        //public ActionResult Products(ProductsPageModel oModel)
        //{
        //    oModel.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
        //    oModel = objAdminData.GetProducts(oModel);
        //    return View(oModel);
        //}
        //[PermissionFilter]
        //public ActionResult DeleteProduct(int ProductID)
        //{
        //    ProductModel oModel = new ProductModel();
        //    //oModel.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());

        //    objAdminData.DeleteProduct(ProductID);
        //    return View(oModel);
        //}

        //[PermissionFilter]
        //public ActionResult GetProductDetails(string ID = null)
        //{
        //    int ProductID = CommonUsage.ConvertToInt(ID);
        //    int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
        //    ProductsPageModel objModel;
        //    objModel = objAdminData.GetProductDetails(ProductID, SBranchID);
        //    if (ProductID == 0)
        //    {
        //        objModel.Product = new ProductModel();
        //    }

        //    return PartialView("_ProductDetailsPartial", objModel);
        //}
        //[PermissionFilter]
        //public ActionResult UpdateProduct(ProductModel objData)
        //{
        //    objData.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
        //    string OldImageFile = "";
        //    if (objData.ProductPhoto != null)
        //    {
        //        OldImageFile = objData.Photo;
        //        objData.Photo = objData.ProductPhoto.FileName.Replace(" ", "-");
        //    }
        //    objData.ProductID = objAdminData.UpdateProduct(objData);
        //    string path = Server.MapPath("~/images/Products/");
        //    if (!Directory.Exists(path))
        //    {
        //        Directory.CreateDirectory(path);
        //    }
        //    if (objData.ProductPhoto != null)
        //    {
        //        try
        //        {
        //            System.IO.File.Delete(
        //                Server.MapPath("~/images/Products/" + objData.ProductID + "_" + OldImageFile));

        //        }
        //        catch (Exception ex)
        //        {


        //        }
        //        path = Path.Combine(Server.MapPath("~/images/Products/"),
        //           objData.ProductID + "_" + objData.Photo);
        //        objData.ProductPhoto.SaveAs(path);
        //    }

        //    return RedirectToAction("Products", "Admin", new { CategoryID = objData.ProductCategoryID });
        //}
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

           
            if (oModel.ClassDate.Year == 1)
            {
                oModel.ClassDate = CommonUsage.GetCurrentDate();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            oModel.Classes = await (new BBBOnlineClassData()).GetPrincipalBBBOnlineClassesSchedules(SBranchID, oModel.ClassDate);
            return View(oModel);

        }
        [PermissionFilter]
        public async Task<ActionResult> JoinClass(string ID = null)
        {
            var onlienClassData = new BBBOnlineClassData();
            var user = PermissionManager.GetLoggedInUser();
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int OCID = CommonUsage.ConvertToInt(ID);
            BBBOnlineClassModel cModel = onlienClassData.GetOnlineClassDetailsByOCID(OCID);

            string basepath = $"{this.Request.Url.Scheme}://{this.Request.Url.Host}";
            var meetingStatus = await client.GetMeetingInfoAsync(new GetMeetingInfoRequest { meetingID = cModel.MeetingID });
            if (meetingStatus.returncode != Returncode.FAILED)
            {
                var joinDate = CommonUsage.GetCurrentDate();
                //NameIDModel student = await onlienClassData.StudentJoinOnlineClass(StudentID, joinDate, OCID, ParentID, 0);
                // StudentModel Student = objStudents.Where(x => x.StudentID == StudentID).FirstOrDefault();
               // string avatar = basepath + student.Extra1;
                var requestJoin = new JoinMeetingRequest { meetingID = cModel.MeetingID };
                requestJoin.userID =SBranchID.ToString();
                requestJoin.fullName = "Principle";
                requestJoin.password = cModel.AttPassword;
                var setConfigRequest = new SetConfigXMLRequest
                {
                    meetingID = cModel.MeetingID,
                    configXML = "<config><modules><localeversion supressWarning=\"false\">0.9.0</localeversion></modules></config>"
                };
                var setConfigResult = await client.SetConfigXMLAsync(setConfigRequest);
                if (setConfigResult.returncode == Returncode.FAILED) return Json(0);
                requestJoin.configToken = setConfigResult.configToken;
              
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