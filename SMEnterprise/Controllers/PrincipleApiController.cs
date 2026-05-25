using BigBlueButtonAPI.Common;
using BigBlueButtonAPI.Core;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace SMEnterprise.Controllers
{
    public class PrincipleApiController : ApiController
    {
        private readonly BigBlueButtonAPIClient client;
        public PrincipleApiController() : base()
        {
            //this.client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
        }
        //private async Task<bool> isBigBlueButtonAPISettingsOKAsync()
        //{
        //    try
        //    {
        //        var res = await client.IsMeetingRunningAsync(new IsMeetingRunningRequest { meetingID = Guid.NewGuid().ToString() });
        //        if (res.returncode == Returncode.FAILED) return false;
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}
        private UserModel VerifyUser(string UUID)
        {
            UserModel objUserModel = new UserModel();
            //List<ApiAuthenticationModel> objapiModel = LuceneData.Search(UUID, "UUID").ToList();
            List<ApiAuthenticationModel> objapiModel = (new CommonData()).GetAppUser(UUID);
            bool isFound = false;
            if (objapiModel.Count > 0)
            {
                foreach (ApiAuthenticationModel a in objapiModel)
                {
                    if (a.UserType == (int)RoleType.Principle || a.UserType == (int)RoleType.Director)
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
        [HttpPost]
        public CommonApiWraperModel GetBranches(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                int UserID;
                if (user.RoleID == 9)
                {
                    UserID = -1;
                }
                else
                {
                    UserID = user.UserID;
                }
                AdminData objAdminData = new AdminData();
                List<object> List = objAdminData.GetAppBranches(UserID).ToList<object>();
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
        public CommonApiWraperModel GetDashboard(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                AdminData objAdminData = new AdminData();

                DateTime CurrentDate = CommonUsage.GetCurrentDate();
                AdminDashboardModel objModel = objAdminData.GetDashBoardData(data.SBranchID, CurrentDate);
                if (objModel != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = objModel;
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
        public CommonApiWraperModel GetClassTimeTable_NotUse(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                AdminData objAdminData = new AdminData();

                DateTime CurrentDate = CommonUsage.GetCurrentDate();
                TimeTablePageModel objModel = objAdminData.GetSectionTimeTable(data.ID, CurrentDate, data.SBranchID);
                if (objModel != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = objModel;
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
        public CommonApiWraperModel GetClassDayPeriods(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (VerifyUser(data.UUID) != null)
            {
                AdminData objParentData = new AdminData();
                object Data = objParentData.GetPrincipleApiTTDayLactures(data);
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
        public CommonApiWraperModel GetTeacherDayPeriods(ParentApiParamModel data)
        {
            UserModel um = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (um != null)
            {
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
        public CommonApiWraperModel GetTeacherTimeTable_NotUse(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                AdminData objAdminData = new AdminData();

                DateTime CurrentDate = CommonUsage.GetCurrentDate();
                TimeTablePageModel objModel = objAdminData.GetTeacherTimeTable(data.ID, CurrentDate, data.SBranchID);
                if (objModel != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = objModel;
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
        public CommonApiWraperModel GetDailyCollection(CollectionReportModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.ReportType = 1;
                if (data.FromDate.Year == 1)
                {
                    data.FromDate = CommonUsage.GetCurrentDate();
                    data.PaymentMode = -1;
                }
                if (data.ToDate.Year == 1)
                {
                    data.ToDate = CommonUsage.GetCurrentDate();
                }
                AccountData objAccountData = new AccountData();
                CollectionReportModel objModel = objAccountData.GetCollectionReport(data); ;
                if (objModel != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = objModel;
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
        public CommonApiWraperModel GetBetweenDatesCollection(CollectionReportModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.ReportType = 3;
                if (data.FromDate.Year == 1)
                {
                    data.FromDate = CommonUsage.GetCurrentDate().AddDays(-7);
                    data.PaymentMode = -1;
                }
                if (data.ToDate.Year == 1)
                {
                    data.ToDate = CommonUsage.GetCurrentDate();
                }
                AccountData objAccountData = new AccountData();
                CollectionReportModel objModel = objAccountData.GetCollectionReport(data); ;
                if (objModel != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = objModel;
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
        public CommonApiWraperModel GetQuarterlyCollection(CollectionReportModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.ReportType = 4;
                if (data.QuarterID == 0)
                {
                    data.QuarterID = 1;
                    data.PaymentMode = -1;
                }
                if (data.FromDate.Year == 1)
                {
                    data.FromDate = CommonUsage.GetCurrentDate().AddDays(-7);
                }
                if (data.ToDate.Year == 1)
                {
                    data.ToDate = CommonUsage.GetCurrentDate();
                }
                AccountData objAccountData = new AccountData();
                CollectionReportModel objModel = objAccountData.GetCollectionReport(data); ;
                if (objModel != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = objModel;
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
        public CommonApiWraperModel GetMonthlyCollection(CollectionReportModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                data.ReportType = 2;

                if (data.FromDate.Year == 1)
                {
                    data.FromDate = CommonUsage.GetCurrentDate();
                    data.PaymentMode = -1;
                }
                if (data.ToDate.Year == 1)
                {
                    data.ToDate = CommonUsage.GetCurrentDate();
                }
                AccountData objAccountData = new AccountData();
                CollectionReportModel objModel = objAccountData.GetCollectionReport(data); ;
                if (objModel != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = objModel;
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
        public CommonApiWraperModel GetClassFeeReport(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {

                AccountData objAccountData = new AccountData();
                List<object> objModel = objAccountData.GetClassWiseDueFeeDetails(data.SBranchID).ToList().ToList<object>();
                if (objModel.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = objModel;
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
        public CommonApiWraperModel GetEmployeeTypes(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {

                AdminData objAccountData = new AdminData();
                List<object> objModel = objAccountData.GetEmployeeTypes(user.SBranchID).ToList().ToList<object>();
                if (objModel.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = objModel;
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
        public CommonApiWraperModel UpdateEmployeeAttandance(EmployeeAttandancePageModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {

                AccountData objAccountData = new AccountData();
                //data.Day = "D" + data.Day;
                int objModel = objAccountData.UpdateEmployeesAttandance(data);
                if (objModel > 0)
                {
                    objWraper.Code = 200;
                    objWraper.Data = objModel;
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
        public CommonApiWraperModel GetDueFeeReport(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {

                AccountData objAccountData = new AccountData();
                DemandReciptListModel objModel = objAccountData.GetDemandReciptData(data.SBranchID, data.ID, data.ID2, 0);

                if (objModel != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = objModel;
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
        public CommonApiWraperModel GetEmployeeAttandance(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                EmployeeAttandancePageModel objModel = new EmployeeAttandancePageModel();
                objModel.SelectedDate = CommonUsage.GetCurrentDate();
                if (data.ID == 0)
                {
                    data.ID = 1;
                }
                objModel.EmployeeType = data.ID;
                objModel.Month = objModel.SelectedDate.Month;
                objModel.Year = objModel.SelectedDate.Year;

                objModel.Day = objModel.SelectedDate.Day.ToString();

                AccountData objAccountData = new AccountData();
                objModel.SBranchID = data.SBranchID;
                objModel = objAccountData.GetEmployeesAttandance(objModel);
                if (objModel != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = objModel;
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
        public CommonApiWraperModel GetVehicleList(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {

                AdminData objAdminData = new AdminData();
                List<object> objModel = objAdminData.GetVehicleList(data.SBranchID).ToList().ToList<object>();
                if (objModel.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = objModel;
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
        public CommonApiWraperModel GetVehicleRouteList(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {

                AdminData objAdminData = new AdminData();
                List<object> objModel = objAdminData.GetVehicleRoutes(data.SBranchID, data.ID).ToList().ToList<object>();
                if (objModel.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = objModel;
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
        public CommonApiWraperModel GetPrincipleTransportMap(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (VerifyUser(data.UUID) != null)
            {
                AdminData objAdminData = new AdminData();
                object Data = objAdminData.GetPrincipleTransportMap(data.ID);
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
        public CommonApiWraperModel GetHolidays(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                AdminData objAdminData = new AdminData();

                DateTime CurrentDate = CommonUsage.GetCurrentDate();
                List<HolidayModel> objModel = objAdminData.GetHolidays(data.SBranchID).ToList();
                if (objModel.Count > 0)
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
        public CommonApiWraperModel GetNotices(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                AdminData objAdminData = new AdminData();
                List<NoticeModel> objModel = objAdminData.GetPrincipleNotices(data.SBranchID).ToList();
                if (objModel.Count > 0)
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
        public CommonApiWraperModel GetEventTypesClass(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                AdminData objAdminData = new AdminData();
                EventCalendarModel objmodlel = new EventCalendarModel();
                objmodlel.EventTypes = objAdminData.GetEventTypeList(data.SBranchID);
                objmodlel.Classes = objAdminData.GetClasses(1, data.SBranchID, 0).Classes;
                if (objmodlel != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = objmodlel;
                    objWraper.Message = "Success";
                }
                else
                {
                    objWraper.Code = 404;
                    objWraper.Message = "No Data Found";
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
        public CommonApiWraperModel InsertUpdateEvent(CommonEvents data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {

                AdminData objAdminData = new AdminData();
                data.IsApproved = 1;
                int EventID = objAdminData.InsertUpdateEvent(data);

                if (data.IsApproved == 1)
                {
                    List<SMSRecieverModel> recievers = objAdminData.GetSMSRecieverList(data.SBranchID, data.ClassesIncludedIDs, 5);
                    NotificationModel objNModel = new NotificationModel();
                    CommonData objCommonData = new CommonData();
                    objNModel.RecieverID = -1;
                    objNModel.SBranchID = user.SBranchID;
                    objNModel.NotificationType = 6;
                    objNModel.RecieverType = 3;
                    objNModel.NotificationText = data.title;
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
                    string ServerNotificationKey = (new CommonData()).GetNotificationServerKey(user.SBranchID);
                    CommonUsage.SendNotificationFCM(Recievers, objNModel.NotificationText, "6", ServerNotificationKey);
                    objCommonData.InsertNotification(objNModel);
                }
                if (EventID != 0)
                {
                    objWraper.Code = 200;
                    objWraper.Data = EventID;

                    objWraper.Message = "Success";
                }
                else
                {
                    objWraper.Code = 404;
                    objWraper.Message = "Not Added";
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
        public CommonApiWraperModel InsertUpdateNotice(NoticeModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                AdminData objAdminData = new AdminData();
                data.OperationDate = CommonUsage.GetCurrentDate();
                data.UserID = user.UserID;
                if (data.OpType == -1)
                {

                    data.NoticeStartDate = CommonUsage.GetCurrentDate();
                    data.NoticeEndDate = CommonUsage.GetCurrentDate();
                }
                int NoticeID = objAdminData.InsertUpdateNotice(data);

                List<SMSRecieverModel> recievers = objAdminData.GetSMSRecieverList(data.SBranchID, data.ClassesIncludedIDs, data.ApplicableFor);
                NotificationModel objModel = new NotificationModel();
                CommonData objCommonData = new CommonData();
                objModel.RecieverID = -1;
                objModel.SBranchID = data.SBranchID;
                objModel.NotificationType = 1;
                objModel.RecieverType = data.ApplicableFor;
                objModel.NotificationText = data.NoticeTitle;
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
                string[] Recievers = sb.ToString().Trim().Split("#".ToCharArray());
                string ServerNotificationKey = (new CommonData()).GetNotificationServerKey(user.SBranchID);
                CommonUsage.SendNotificationFCM(Recievers, data.NoticeTitle, "1", ServerNotificationKey);
                objCommonData.InsertNotification(objModel);
                string ContentID = "0";
                if (data.SendSMS == 1 && data.OpType != -1 && data.NoticeID == 0)
                {
                    foreach (SMSRecieverModel r in recievers)
                    {
                        if (r.MobileNo != null && r.MobileNo != "")
                        {
                            SMSSender objSender = new SMSSender();
                            objSender.SendSMSAsync(data.NoticeTitle, r.MobileNo, data.SBranchID, 1, r.RecieverType, r.ID, ContentID);
                        }
                    }
                    objAdminData.UpdateSenderSMSSentStatus(1, NoticeID, data.ClassesIncludedIDs, data.ApplicableFor);
                }

                if (NoticeID != 0)
                {
                    objWraper.Code = 200;
                    objWraper.Data = NoticeID;
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
        /// <summary>
        /// Gives the list of Sections for a perticular class
        /// </summary>
        /// <param name="ID">The ID of the Class.</param>
        /// <param name="UUID">The UUID of the Sender Device.</param>
        [HttpPost]
        public CommonApiWraperModel GetSectionsOnClass(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                AdminData objAdminData = new AdminData();
                List<object> objList = objAdminData.GetSectionsOnClass(data.ID).ToList<object>();
                if (objList.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = objList;
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
        public CommonApiWraperModel GetClassSections(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                AdminData objAdminData = new AdminData();
                List<object> objList = objAdminData.GetPrincipleClassSections(data.SBranchID).ToList<object>();
                if (objList.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = objList;
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
        public CommonApiWraperModel GetTeacherSubstitutionDetails(TeacherSubstitutionEditModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                AdminData objAdminData = new AdminData();
                data.CurrentDate = CommonUsage.GetCurrentDate();
                data = objAdminData.GetTeacherSubstitutionDetails(data);
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
        public CommonApiWraperModel UpdateTeacherSubstitution(TeacherSubstitutionModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                AdminData objAdminData = new AdminData();
                if (data.OpType == -1)
                {
                    data.FromDate = CommonUsage.GetCurrentDate();
                }
                data.CurrentDate = CommonUsage.GetCurrentDate();
                data.EndDate = data.FromDate.AddHours(12);
                TeacherSubstitutionEditModel objData = objAdminData.UpdateTeacherSubstitutionDetails(data);
                TeacherSubstitutionSMSModel objSMS = objAdminData.GetTeacherSubstitutionSMSDetails(data);
                string ReplacingSMS = "";
                string ContentID = "0";
                if (data.OpType == -1)
                {
                    ReplacingSMS = StartupModel.sTeacherRemoveSubstitutedSMSTemplate.Replace("[NamePlaceHolder]", objSMS.ReplacingTeacher.EmployeeName)
                       .Replace("[ClassPlaceHolder]", objSMS.ClassName)
                       .Replace("[SectionPlaceHolder]", objSMS.SectionName)
                       .Replace("[DatePlaceHolder]", data.FromDate.ToString("dd,MMM")).Replace("[DayPlaceHolder]", AdminData.arrDays[data.DayID - 1])
                       .Replace("[PeriodPlaceHolder]", objSMS.PeriodName);
                }
                else
                {
                    ReplacingSMS = StartupModel.sTeacherSubstitutedSMSTemplate.Replace("[NamePlaceHolder]", objSMS.ReplacingTeacher.EmployeeName)
                        .Replace("[SubjectPlaceHolder]", objSMS.SubjectName).Replace("[ClassPlaceHolder]", objSMS.ClassName)
                        .Replace("[SectionPlaceHolder]", objSMS.SectionName).Replace("[ReplacedTeacher]", objSMS.ReplacedTeacher.EmployeeName)
                        .Replace("[DatePlaceHolder]", data.FromDate.ToString("dd,MMM")).Replace("[DayPlaceHolder]", AdminData.arrDays[data.DayID - 1])
                        .Replace("[PeriodPlaceHolder]", objSMS.PeriodName);
                }
                if (data.SendSMS == 1)
                {

                    if (objSMS.ReplacingTeacher.MobileNumber != null && objSMS.ReplacingTeacher.MobileNumber != "")
                    {
                        SMSSender objSender = new SMSSender();
                        objSender.SendSMSAsync(ReplacingSMS, objSMS.ReplacingTeacher.MobileNumber, data.SBranchID, 3, 3, data.ReplacingTeacherID, ContentID);
                    }

                }

                if (!String.IsNullOrEmpty(objSMS.FCMToken))
                {
                    NotificationModel objNModel = new NotificationModel();
                    CommonData objCommonData = new CommonData();
                    objNModel.RecieverID = data.ReplacingTeacherID;
                    objNModel.SBranchID = data.SBranchID;
                    objNModel.NotificationType = 8;
                    objNModel.RecieverType = 3;
                    objNModel.NotificationText = ReplacingSMS;
                    objNModel.NotificationDateTime = CommonUsage.GetCurrentDate();
                    objNModel.Recievers = new List<NotificationRecieverModel>();

                    string[] Recievers = objSMS.FCMToken.Trim().Split("#".ToCharArray());
                    string ServerNotificationKey = (new CommonData()).GetNotificationServerKey(user.SBranchID);
                    CommonUsage.SendNotificationFCM(Recievers, objNModel.NotificationText, "8", ServerNotificationKey);
                    objCommonData.InsertNotification(objNModel);
                }
                objData.DayID = data.DayID;
                objData.PeriodID = data.PeriodID;
                objData.SectionID = data.SectionID;
                objData.TeacherID = data.ReplacedTeacherID;
                objData.SubjectID = data.SubjectID;
                objData.ClassID = data.ClassID;
                objData.EducationLevelID = data.EducationLevelID;
                if (objData != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = objData;
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
        public CommonApiWraperModel GetSubjectTeachers(EditTTLactureModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                AdminData objAdminData = new AdminData();
                data.CurrentDate = CommonUsage.GetCurrentDate();
                List<EmployeeModel> Teachers = objAdminData.GetTeacherForSectionSubject(data).ToList();
                if (Teachers.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = Teachers.ToList<object>();
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
        public CommonApiWraperModel GetTeachers(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                AdminData objAdminData = new AdminData();
                List<object> Teachers = objAdminData.GetTeacherList(data.SBranchID).ToList();
                if (Teachers.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = Teachers;
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
        public async Task<CommonApiWraperModel> GetSchoolEvents(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                int year = data.Year;
                int month = data.Month;
                int SBranchID = data.SBranchID;
                CommonData objCommonData = new CommonData();
                object Data = await objCommonData.GetEventCalander(month, year, SBranchID, 0);
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
        public CommonApiWraperModel GetLeaveRequests(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {

                AdminData objAccountData = new AdminData();
                List<object> objModel = objAccountData.GetLeaves(data.SBranchID, data.ID, 0).ToList().ToList<object>();
                if (objModel.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = objModel;
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
        public CommonApiWraperModel UpdateLeaveStatus(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                LeaveModel objModel = new LeaveModel();
                objModel.LeaveID = data.ID;
                objModel.IsApproved = data.Status;
                AdminData objAdminData = new AdminData();
                //CommonData.InsertTestLog(data.ID + ":" + data.ID2);
                int Status = objAdminData.UpdateLeaveStatus(objModel);
                if (Status > 0)
                {
                    objWraper.Code = 200;
                    objWraper.Data = 1;
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
        public async Task<CommonApiWraperModel> GetImportantContacts(ParentApiParamModel data)
        {
            int SBranchID = VerifyUser(data.UUID).SBranchID;
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (SBranchID != 0)
            {
                AdminData objAdminData = new AdminData();
                List<object> List =(await objAdminData.GetImportantContact(SBranchID)).ToList<object>();
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
        //#region Online Classes Related
        //[HttpPost]
        //public async Task<CommonApiWraperModel> GetBBBOnlineClassList(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //      //  OnlineClassData objParentData = new OnlineClassData();
        //        if (data.RDate.Year == 1)
        //        {
        //            data.RDate = CommonUsage.GetCurrentDate();
        //        }
        //        var data1 = await (new BBBOnlineClassData()).GetPrincipalBBBOnlineClassesSchedules(user.SBranchID, data.RDate);
        //        //objWraper.Data = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
        //        if (data1.Count() > 0)
        //        {
        //            objWraper.Code = 200;
        //            objWraper.List = data1.ToList<object>();
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
        //[HttpPost]
        //public async Task<CommonApiWraperModel> JoinBBBOnlineClass(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        BBBOnlineClassModel cModel = (new BBBOnlineClassData()).GetOnlineClassDetailsByOCID(data.OCID);

        //        var meetingStatus = await client.GetMeetingInfoAsync(new GetMeetingInfoRequest { meetingID = cModel.MeetingID });
        //        if (meetingStatus.returncode != Returncode.FAILED)
        //        {

        //            var requestJoin = new JoinMeetingRequest { meetingID = cModel.MeetingID };
        //            requestJoin.userID = "0";
        //            requestJoin.fullName = "Principal";
        //            requestJoin.password = cModel.AttPassword;
        //            var setConfigRequest = new SetConfigXMLRequest
        //            {
        //                meetingID = cModel.MeetingID,
        //                configXML = "<config><modules><localeversion supressWarning=\"false\">0.9.0</localeversion></modules></config>"
        //            };
        //            var setConfigResult = await client.SetConfigXMLAsync(setConfigRequest);
        //            if (setConfigResult.returncode == Returncode.FAILED)
        //            {
        //                objWraper.Code = -1;
        //            }
        //            else
        //            {
        //                requestJoin.configToken = setConfigResult.configToken;
        //                // requestJoin.avatarURL = avatar;
        //                var url = client.GetJoinMeetingUrl(requestJoin);
        //                objWraper.Data = url;
        //            }
        //        }

        //        if (objWraper.Data != null)
        //        {
        //            objWraper.Code = 200;
        //            objWraper.Message = "Preparing to join class";
        //        }
        //        else
        //        {
        //            objWraper.Code = 404;
        //            objWraper.Message = "Class is not running now";
        //        }
        //    }
        //    else
        //    {
        //        objWraper.Code = 101;
        //        objWraper.Message = "Unauthorized";
        //    }
        //    return objWraper;
        //}
        //[HttpPost]
        //public CommonApiWraperModel GetOnlineClassList(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        OnlineClassData objPrincipalData = new OnlineClassData();
        //        if (data.RDate.Year == 1)
        //        {
        //            data.RDate = CommonUsage.GetCurrentDate();
        //        }
        //        List<OnlineClassModel> data1 = objPrincipalData.GetPrincipalOnlineClassesSchedules(user.SBranchID, data.RDate);
        //        try
        //        {
        //            objWraper.Data = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
        //        }
        //        catch (Exception ex)
        //        {
        //            objWraper.Data = "https://meet.stridetechindia.com/";
        //        }
        //        objWraper.RDate = data.RDate;
        //        if (data1.Count() > 0)
        //        {
        //            objWraper.Code = 200;
        //            objWraper.List = data1.ToList<object>();
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
        //[HttpPost]
        //public CommonApiWraperModel GetBBBOnlineClassAttendees(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        //OnlineClassData objPrincipalData = new OnlineClassData();
        //        //List<OnlineClassAttendeeModel> List = objPrincipalData.GetOnlineClassAttendees(data.OCID);
        //        BBBOnlineClassData objTeacherData = new BBBOnlineClassData();
        //        var List = objTeacherData.GetOnlineClassAttendees(data.OCID);
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
        //[HttpPost]
        //public CommonApiWraperModel GetOnlineClassAttendees(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        //OnlineClassData objPrincipalData = new OnlineClassData();
        //        //List<OnlineClassAttendeeModel> List = objPrincipalData.GetOnlineClassAttendees(data.OCID);
        //        BBBOnlineClassData objTeacherData = new BBBOnlineClassData();
        //        var List = objTeacherData.GetOnlineClassAttendees(data.OCID);
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
        //[HttpPost]
        //public CommonApiWraperModel JoinOnlineClass(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        objWraper.Code = 200;
        //        objWraper.Data = "Succesfully Joined";

        //        objWraper.Message = "Success";
        //    }
        //    else
        //    {
        //        objWraper.Code = 101;
        //        objWraper.Message = "Unauthorized";
        //    }
        //    return objWraper;
        //}

        //#endregion
        //#region Staf Meeting
        //[HttpPost]
        //public CommonApiWraperModel GetOnlineMeetings(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        data.SBranchID = user.SBranchID;
        //        var CurDate = CommonUsage.GetCurrentDate();
        //        OnlineClassData objTeacherData = new OnlineClassData();
        //        objWraper.Data = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
        //        List<OnlineStaffMeetingModel> Data = objTeacherData.GetOnlineStaffMeetings(user.SBranchID, CurDate);
        //        if (Data.Count > 0)
        //        {
        //            objWraper.Code = 200;
        //            objWraper.List = Data.ToList<object>();
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
        //[HttpPost]
        //public CommonApiWraperModel GetBBBOnlineMeetings(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        data.SBranchID = user.SBranchID;
        //        if(data.RDate.Year==1)
        //        {
        //            data.RDate= CommonUsage.GetCurrentDate(); ;
        //        }
        //        BBBOnlineStaffMeetingData objTeacherData = new BBBOnlineStaffMeetingData();
        //        List<OnlineStaffMeetingModel> Data = objTeacherData.GetOnlineStaffMeetings(data.RDate,user.SBranchID);
        //        if (Data.Count > 0)
        //        {
        //            objWraper.Code = 200;
        //            objWraper.List = Data.ToList<object>();
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
        //[HttpPost]
        //public CommonApiWraperModel ScheduleOnlineMeeting(OnlineStaffMeetingModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        data.SBranchID = user.SBranchID;
        //        OnlineClassData objTeacherData = new OnlineClassData();
        //        data.MeetingID = objTeacherData.AddOnlineStaffMeeting(data);
        //        if (data.MeetingID != 0)
        //        {
        //            objWraper.Code = 200;
        //            objWraper.Data = data.JitsiMeetingID;
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
        //[HttpPost]
        //public CommonApiWraperModel ScheduleBBBOnlineMeeting(OnlineStaffMeetingModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        data.SBranchID = user.SBranchID;
        //        data.BBBMeetingID = Guid.NewGuid().ToString();
        //        BBBOnlineStaffMeetingData objTeacherData = new BBBOnlineStaffMeetingData();
        //        data.MeetingID = objTeacherData.ScheduleOnlineStaffMeeting(data);
        //        if (data.MeetingID != 0)
        //        {
        //            objWraper.Code = 200;
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
        //[HttpPost]
        //public CommonApiWraperModel UpdateOnlineMeeting(OnlineStaffMeetingModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        OnlineClassData objTeacherData = new OnlineClassData();
        //        int List = objTeacherData.UpdateOnlineStaffMeeting(data);
        //        if (List > 0)
        //        {
        //            List<SMSRecieverModel> recievers = objTeacherData.GetRecieverListOnStafMeeting(data.MeetingID);
        //            OnlineClassNotificationModel message = new OnlineClassNotificationModel();
        //            message.base_url = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
        //            if (data.Status == 1)
        //            {
        //                message.message = "Online Meeting Started.";
        //                message.type = 1;
        //            }
        //            else
        //            {
        //                message.message = "Online Meeting Ended.";
        //                message.type = 2;
        //            }
        //            message.typeid = data.MeetingID;
        //            string jmessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
        //            NotificationModel objModel = new NotificationModel();
        //            objModel.RecieverID = -1;
        //            objModel.NotificationType = 10;
        //            objModel.NotificationDateTime = CommonUsage.GetCurrentDate();
        //            objModel.Recievers = new List<NotificationRecieverModel>();

        //            StringBuilder sb = new StringBuilder();
        //            foreach (SMSRecieverModel r in recievers)
        //            {
        //                if (!String.IsNullOrEmpty(r.deviceToken))
        //                {
        //                    if (r.deviceToken.Contains("\\n"))
        //                    {
        //                        r.deviceToken.Replace("\\n", "#");
        //                    }
        //                    if (sb != null && sb.ToString() != "")
        //                    {
        //                        sb.Append("#");
        //                    }
        //                    sb.Append(r.deviceToken);
        //                }
        //            }
        //            string NotificationServerKey = "AAAAFoTO4zQ:APA91bEUshyzwyxd00uGjDfvaugyo57JfKun7-QaQJkPm7XO70-x31w3BnFKAOtwHkuQnj3eTdKmeSwk2UjkzzXHyJOqUrhV0PpkQuT7_lQUBKElQ5_kv_L2LFKR004CgCOsqtoLuIFZ";
        //            string[] Recievers = sb.ToString().Trim().Split("#".ToCharArray());
        //            CommonUsage.SendNotificationFCM(Recievers, jmessage, "10", NotificationServerKey);

        //            objWraper.Code = 200;
        //            objWraper.Data = List;
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
        //[HttpPost]
        //public CommonApiWraperModel UpdateBBBOnlineMeeting(OnlineStaffMeetingModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        BBBOnlineStaffMeetingData objTeacherData = new BBBOnlineStaffMeetingData();
        //        int List = objTeacherData.UpdateOnlineStaffMeeting(data);
        //        if (List > 0)
        //        {
        //            List<SMSRecieverModel> recievers = objTeacherData.GetRecieverListOnStafMeeting(data.MeetingID);
        //            OnlineClassNotificationModel message = new OnlineClassNotificationModel();
        //            if (data.Status == 1)
        //            {
        //                message.message = "Online Meeting Started.";
        //                message.type = 1;
        //            }
        //            else
        //            {
        //                message.message = "Online Meeting Ended.";
        //                message.type = 2;
        //            }
        //            message.typeid = data.MeetingID;
        //            string jmessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
        //            NotificationModel objModel = new NotificationModel();
        //            objModel.RecieverID = -1;
        //            objModel.NotificationType = 10;
        //            objModel.NotificationDateTime = CommonUsage.GetCurrentDate();
        //            objModel.Recievers = new List<NotificationRecieverModel>();

        //            StringBuilder sb = new StringBuilder();
        //            foreach (SMSRecieverModel r in recievers)
        //            {
        //                if (!String.IsNullOrEmpty(r.deviceToken))
        //                {
        //                    if (r.deviceToken.Contains("\\n"))
        //                    {
        //                        r.deviceToken.Replace("\\n", "#");
        //                    }
        //                    if (sb != null && sb.ToString() != "")
        //                    {
        //                        sb.Append("#");
        //                    }
        //                    sb.Append(r.deviceToken);
        //                }
        //            }
        //            string NotificationServerKey = "AAAAFoTO4zQ:APA91bEUshyzwyxd00uGjDfvaugyo57JfKun7-QaQJkPm7XO70-x31w3BnFKAOtwHkuQnj3eTdKmeSwk2UjkzzXHyJOqUrhV0PpkQuT7_lQUBKElQ5_kv_L2LFKR004CgCOsqtoLuIFZ";
        //            string[] Recievers = sb.ToString().Trim().Split("#".ToCharArray());
        //            CommonUsage.SendNotificationFCM(Recievers, jmessage, "10", NotificationServerKey);

        //            objWraper.Code = 200;
        //            objWraper.Data = List;
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
        //[HttpPost]
        //public async Task<CommonApiWraperModel> StartBBBOnlineMeeting(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        string basep = $"{ this.Request.RequestUri.Scheme}://{this.Request.RequestUri.Host}";
        //        if (basep.Contains("localhost"))
        //        {
        //            basep = "https://node1.pschoolonline.com";
        //        }
        //        var onlineClassData = (new BBBOnlineStaffMeetingData());
        //        var cModel = onlineClassData.GetOnlineStaffMeetingDetails(data.ID,user.SBranchID);
        //        string basepath = basep;// $"{ this.Request.RequestUri.Scheme}://{this.Request.RequestUri.Host}";
        //        string logo = HttpUtility.UrlEncode(basepath + "/Images/SBranchLogo/" + user.SBranchID + "_" + user.BranchLogo);
        //        var setupOk = await isBigBlueButtonAPISettingsOKAsync();
        //        var meetingStatus = await this.client.GetMeetingInfoAsync(new GetMeetingInfoRequest { meetingID = cModel.BBBMeetingID });
        //        DateTime startdatetime = DateTime.Parse("2021-02-01 " + cModel.StartTime);
        //        DateTime enddatetime = DateTime.Parse("2021-02-01 " + cModel.EndTime);
        //        int duration = (enddatetime - startdatetime).Minutes;

        //        if (duration < 0)
        //        {
        //            duration = 30;
        //        }
        //        else if (duration > 60)
        //        {
        //            duration = 60;
        //        }
        //        if (meetingStatus.returncode == Returncode.FAILED)
        //        {
        //            MetaData meta = new MetaData();
        //            meta.Add("BranchID", "1");
        //            string meu = basepath + "/Home/EndOnlineMeeting";
        //            meta.Add("endCallbackUrl", meu);

        //            string reccbu = basepath + "/home/bbbrecordingreadystaff/";
        //            meta.Add("bbb-recording-ready-url", reccbu);
        //            meta.Add("bbb_skip_check_audio", "true");
        //            meta.Add("bbb_client_title", "P-School");
        //            meta.Add("bbb_enable_screen_sharing", "false");
        //            meta.Add("bbb_show_public_chat_on_login", "false");
        //            var result = await client.CreateMeetingAsync(new CreateMeetingRequest
        //            {
        //                name = cModel.MeetingTitle + " on " + cModel.MeetingDate.ToString("dd MMM, yyyy"),
        //                meetingID = cModel.BBBMeetingID,
        //                record = true,
        //                logoutURL = basepath + "/Home/ClassEnded/" + cModel.MeetingID,
        //                meta = meta,
        //                guestPolicy = "ALWAYS_ACCEPT",
        //                logo = logo,
        //                lockSettingsDisablePrivateChat = true,
        //                lockSettingsDisableNote = false,
        //                muteOnStart = true,
        //                allowModsToUnmuteUsers = true,
        //                autoStartRecording = true,
        //                duration = duration + 5
        //                //welcome="Welcome to class"
        //                //autoStartRecording = true,
        //                //bannerText = "Online Class for Subject:" + cModel.SubjectName + " Class:" + cModel.ClassSection + " By :" + cModel.TeacherName
        //            }); ;
        //            if (result.returncode == Returncode.FAILED) objWraper.Data = "BB01";
        //            if (result.returncode != Returncode.FAILED)
        //            {
        //                cModel.InternalMeetingID = result.internalMeetingID;
        //                cModel.Status = 1;
        //                cModel.StartedOn = CommonUsage.GetCurrentDate();
        //                cModel.AttPassword = result.attendeePW;
        //                cModel.ModPassword = result.moderatorPW;
        //                (onlineClassData).UpdateOnlineStaffMeeting(cModel);
        //            }
        //        }

        //        var requestJoin = new JoinMeetingRequest { meetingID = cModel.BBBMeetingID };
        //        requestJoin.userID = 0.ToString();
        //        requestJoin.fullName = "Principal";
        //        requestJoin.password = cModel.ModPassword;
        //        var setConfigRequest = new SetConfigXMLRequest
        //        {
        //            meetingID = cModel.BBBMeetingID,
        //            configXML = "<config><modules><localeversion supressWarning=\"false\">0.9.0</localeversion></modules></config>"
        //        };
        //        var setConfigResult = await client.SetConfigXMLAsync(setConfigRequest);
        //        if (setConfigResult.returncode == Returncode.FAILED)
        //        {
        //            objWraper.Data = setConfigResult;
        //        }
        //        else
        //        {
        //            requestJoin.configToken = setConfigResult.configToken;
        //            //requestJoin.avatarURL = avatar;
        //            var url = client.GetJoinMeetingUrl(requestJoin);

        //            List<SMSRecieverModel> recievers = (onlineClassData).GetRecieverListOnStafMeeting(data.ID);
        //            OnlineClassNotificationModel message = new OnlineClassNotificationModel();
        //            //message.base_url = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
        //            message.message = "Online Meeting Started.";
        //            message.type = 1;
        //            message.typeid = data.ID;
        //            string jmessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
        //            NotificationModel objModel = new NotificationModel();
        //            objModel.RecieverID = -1;
        //            objModel.NotificationType = 10;
        //            objModel.NotificationDateTime = CommonUsage.GetCurrentDate();
        //            objModel.Recievers = new List<NotificationRecieverModel>();

        //            StringBuilder sb = new StringBuilder();
        //            foreach (SMSRecieverModel r in recievers)
        //            {
        //                if (!String.IsNullOrEmpty(r.deviceToken))
        //                {
        //                    if (r.deviceToken.Contains("\\n"))
        //                    {
        //                        r.deviceToken.Replace("\\n", "#");
        //                    }
        //                    if (sb != null && sb.ToString() != "")
        //                    {
        //                        sb.Append("#");
        //                    }
        //                    sb.Append(r.deviceToken);
        //                }
        //            }
        //            string NotificationServerKey = "AAAAFoTO4zQ:APA91bEUshyzwyxd00uGjDfvaugyo57JfKun7-QaQJkPm7XO70-x31w3BnFKAOtwHkuQnj3eTdKmeSwk2UjkzzXHyJOqUrhV0PpkQuT7_lQUBKElQ5_kv_L2LFKR004CgCOsqtoLuIFZ";
        //            string[] Recievers = sb.ToString().Trim().Split("#".ToCharArray());
        //            CommonUsage.SendNotificationFCM(Recievers, jmessage, "10", NotificationServerKey);

        //            objWraper.Code = 200;
        //            objWraper.Data = url;
        //        }
        //    }
        //    else
        //    {
        //        objWraper.Code = 404;
        //    }
        //    objWraper.Message = "Success";
        //    return objWraper;
        //}
        //#endregion
    }
}