using Newtonsoft.Json;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SMEnterprise.Controllers
{
    public class CommonApiController : ApiController
    {
        private UserModel VerifyUser(string UUID)
        {
            UserModel objUserModel = new UserModel();
            List<ApiAuthenticationModel> objapiModel = (new CommonData()).GetAppUser(UUID);
            bool isFound = false;
            if (objapiModel.Count > 0)
            {
                foreach (ApiAuthenticationModel a in objapiModel)
                {

                    objUserModel.UserID = a.UserID;
                    objUserModel.RoleID = a.UserType;
                    objUserModel.UserName = a.UserName;
                    objUserModel.SBranchID = a.SBranchID;
                    objUserModel.AppToken = a.deviceToken;
                    isFound = true;
                }
            }
            if (!isFound)
            {
                objUserModel = null;

            }
            return objUserModel;
        }
        [HttpPost]
        public async Task<CommonApiWraperModel> ChangePassword(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            UserModel userdetail = VerifyUser(data.UUID);
            if (userdetail != null)
            {

                string Password = data.Password;
                int UserID = userdetail.UserID;
                int UserType = userdetail.RoleID;
                CommonData objCommonData = new CommonData();
                int Data = await objCommonData.UpdatePassword(UserID, UserType, Password);
                if (Data > 0)
                {
                    objWraper.Code = 200;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
                objWraper.Data = Data;
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }

            return objWraper;
        }
        [HttpGet]
        public string GetLoginDynamicSalt(string UUID = null)
        {
            var commonData = new CommonData();
            //string Salt = CommonUsage.RandomString(10, false);
            var entry = (commonData).GetAppUser(UUID).FirstOrDefault();
            string salt = "";
            if (entry != null)
            {
                if (entry.DynamicSalt is null)
                {
                    entry.DynamicSalt = CommonUsage.RandomString(6, false);
                    commonData.InsertUpdateAppUser(entry);
                }
                salt = entry.DynamicSalt;
            }
            else
            {
                entry = new ApiAuthenticationModel();
                entry.SchoolID = 0;
                entry.UUID = UUID;
                entry.SBranchID = 0;
                entry.LastLoginDate = CommonUsage.GetCurrentDate();
                entry.DynamicSalt = CommonUsage.RandomString(6, false);
                commonData.InsertUpdateAppUser(entry);
                salt = entry.DynamicSalt;
            }
            return salt;
        }
        [HttpGet]
        public CommonApiWraperModel ShootClassStartNotice(string Type = null)
        {
            if (Type == null)
            {
                Type = "1";
            }
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            OnlineClassData objTeacherData = new OnlineClassData();
            List<SMSRecieverModel> recievers = objTeacherData.GetRecieverListOnOnlineClass(12);
            OnlineClassNotificationModel message = new OnlineClassNotificationModel();
            message.message = "Online Class Ended.";
            message.type = CommonUsage.ConvertToInt(Type);
            message.typeid = 12;
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
            objWraper.Data = 1;
            objWraper.Message = "success";
            return objWraper;
        }
        [HttpGet]
        public CommonApiWraperModel GetSplashLogo(string UUID = null, string name = null, string schoolid = null)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();


            AdminData loginData = new AdminData();
            if (schoolid != null)
            {
                var commonData = new CommonData();
                int SchoolID = CommonUsage.ConvertToInt(schoolid);
                var entry = (commonData).GetAppUser(UUID).FirstOrDefault();
                objWraper.Data = loginData.GetAppSplashLogo(SchoolID);
                if (entry == null)
                {
                    entry = new ApiAuthenticationModel();
                }

                entry.SchoolID = SchoolID;
                entry.UUID = UUID;
                entry.SBranchID = loginData.GetBranchIDOnSchoolID(SchoolID);
                entry.LastLoginDate = CommonUsage.GetCurrentDate();
                entry.DynamicSalt = CommonUsage.RandomString(6, false);
                commonData.InsertUpdateAppUser(entry);
            }
            else if (name != null)
            {
                objWraper.Data = loginData.GetAppSplashLogo();
                objWraper.base_url = "http://node1.pschoolonline.com/";
            }
            else
            {
                objWraper.Data = loginData.GetAppSplashLogo();
            }
            if (objWraper.Data != null)
            {
                objWraper.Code = 200;
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 404;
                objWraper.Message = "Logo Not Found";
            }

            return objWraper;
        }
        [HttpGet]
        public CommonApiWraperModel GetMenuList(string ID = null, string UUID = null, string schoolid = null)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            int SBranchID = CommonUsage.ConvertToInt(ID);
            AdminData loginData = new AdminData();
            var entry = (new CommonData()).GetAppUser(UUID).FirstOrDefault(); // CommonUsage.SplashUUIDSchoolID.Where(x => x.Name == UUID).FirstOrDefault();
            if (entry != null)
            {
                SBranchID = entry.SBranchID;
            }
            else
            {
                int SchoolID = CommonUsage.ConvertToInt(schoolid);
                entry = new ApiAuthenticationModel();
                entry.SchoolID = SchoolID;
                entry.UUID = UUID;
                entry.SBranchID = loginData.GetBranchIDOnSchoolID(SchoolID);
                entry.LastLoginDate = CommonUsage.GetCurrentDate();
                entry.DynamicSalt = CommonUsage.RandomString(6, false);
                (new CommonData()).InsertUpdateAppUser(entry);
                SBranchID = entry.SBranchID;

            }
            List<AppMenuModel> list = loginData.GetAppMenuItems(SBranchID);
            if (list.Count != 0)
            {
                objWraper.Code = 200;
                objWraper.IsOnlineClass = 0;
                AppMenuModel login = list.Where(x => x.IsDefault == 2).FirstOrDefault();
                AppMenuModel video = list.Where(x => x.IsDefault == 3).FirstOrDefault();
                AppMenuModel staffMeeting = list.Where(x => x.IsDefault == 4).FirstOrDefault();
                if (video != null)
                {
                    objWraper.IsOnlineClass = 1;
                    list.Remove(video);
                }
                if (staffMeeting != null)
                {
                    objWraper.IsStaffMeeting = 1;
                    list.Remove(staffMeeting);
                }
                if (login != null)
                {
                    objWraper.Data = 1;
                    list.Remove(login);
                }
                objWraper.List = list.ToList<object>();
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 404;
                objWraper.Message = "Menu Not Found";
            }

            return objWraper;
        }
        [HttpGet]
        public CommonApiWraperModel GetBranchList(string UUID = null)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            AdminData loginData = new AdminData();
            objWraper.List = loginData.GetAppBranchList();
            if (objWraper.List.Count != 0)
            {
                objWraper.Code = 200;
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 404;
                objWraper.Message = "Branches Not Found";
            }

            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel ForgetPassword(ForgetPasswordApiModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();


            LoginData loginData = new LoginData();
            objWraper.Data = loginData.GetUserByMobileDOB(data);
            if (objWraper.Data != null)
            {
                objWraper.Code = 200;
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 404;
                objWraper.Message = "User Not Found";
            }

            return objWraper;

        }
        [HttpPost]
        public CommonApiWraperModel UpdatePassword(ForgetPasswordApiModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();


            CommonData objCommonData = new CommonData();
            objWraper.Data = objCommonData.UpdatePassword(data.ID, data.UserType, data.Password);
            if (objWraper.Data != null)
            {
                objWraper.Code = 200;
                objWraper.Message = "Success";
            }
            else
            {
                objWraper.Code = 404;
                objWraper.Message = "Password Not Updated";
            }
            return objWraper;
        }
        [HttpGet]
        public string ClearLuceneIndex(string UUID = null)
        {
            LuceneData.ClearLuceneIndex();
            return "true";
        }
        [HttpGet]
        public IEnumerable<ApiAuthenticationModel> SendFCM()
        {
            IEnumerable<ApiAuthenticationModel> objModel = LuceneData.GetAllIndexRecords();

            string[] arrDevices = { "cs7eIT1LJX8:APA91bFn56yqHT4rfM5PkBgEkwLto2SqO-NaaMWgD6BGUXxtfs8zB1QG1rjJX2Q9i8FFkYHktP_PZdn2YluvmJupXvjSoqesC_EGRBPyFAfJR8IFE-n8MifF4VHfdPsaATR8lz3-prLt", "cjdEa_N8gC4:APA91bFkBFPnulW5OnmtAP2q5ci1O1CXzRMmvhz0UZWejJvVFOMalyAnK96mS0qSa76_plkGLc_JqZSz2thhXUgbSzF2lxRngClOYaSZY6J2JIaVpL90yHPlA-_z3knfg76ePNwN-z-A" };
            string NotificationServerKey = PermissionManager.GetLoggedInUser().NotificationServerKey;

            CommonUsage.SendNotificationFCM(arrDevices, "Test Notification", "5", NotificationServerKey);
            return objModel;
        }
        [HttpPost]
        public IEnumerable<ApiAuthenticationModel> GetLuceneData(Login login)
        {
            IEnumerable<ApiAuthenticationModel> objModel = LuceneData.GetAllIndexRecords();

            string[] arrDevices = { "cjdEa_N8gC4:APA91bFkBFPnulW5OnmtAP2q5ci1O1CXzRMmvhz0UZWejJvVFOMalyAnK96mS0qSa76_plkGLc_JqZSz2thhXUgbSzF2lxRngClOYaSZY6J2JIaVpL90yHPlA-_z3knfg76ePNwN-z-A" };
            SendNotificationFCM(arrDevices, "Test Message From PSchool", "1");
            return objModel;
        }
        public string SendNotification(string deviceId, string message)
        {
            string SERVER_API_KEY = "AAAAFoTO4zQ:APA91bEUshyzwyxd00uGjDfvaugyo57JfKun7-QaQJkPm7XO70-x31w3BnFKAOtwHkuQnj3eTdKmeSwk2UjkzzXHyJOqUrhV0PpkQuT7_lQUBKElQ5_kv_L2LFKR004CgCOsqtoLuIFZ";
            var SENDER_ID = "96717431604";
            var value = message;
            WebRequest tRequest;
            tRequest = WebRequest.Create("https://android.googleapis.com/gcm/send");
            tRequest.Method = "post";
            tRequest.ContentType = " application/x-www-form-urlencoded;charset=UTF-8";
            tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));

            tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));

            string postData = "collapse_key=score_update&time_to_live=108&delay_while_idle=1&data.message=" + value + "&data.time=" + System.DateTime.Now.ToString() + "&registration_id=" + deviceId + "";
            Console.WriteLine(postData);
            Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            tRequest.ContentLength = byteArray.Length;

            Stream dataStream = tRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse tResponse = tRequest.GetResponse();

            dataStream = tResponse.GetResponseStream();

            StreamReader tReader = new StreamReader(dataStream);

            String sResponseFromServer = tReader.ReadToEnd();


            tReader.Close();
            dataStream.Close();
            tResponse.Close();
            return sResponseFromServer;
        }
        public string SendNotificationFCM(string[] deviceId, string message, string Type)
        {
            NotificationSendModel objNotification = new NotificationSendModel();
            objNotification.data = new NotificationModel();
            objNotification.data.NotificationText = "test Message From Server";
            objNotification.data.NotificationDateTime = DateTime.Now;
            objNotification.data.Type = "1";
            objNotification.registration_ids = deviceId;
            string SERVER_API_KEY = "AAAAFoTO4zQ:APA91bEUshyzwyxd00uGjDfvaugyo57JfKun7-QaQJkPm7XO70-x31w3BnFKAOtwHkuQnj3eTdKmeSwk2UjkzzXHyJOqUrhV0PpkQuT7_lQUBKElQ5_kv_L2LFKR004CgCOsqtoLuIFZ";
            var value = message;
            WebRequest tRequest;
            tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            tRequest.Method = "post";
            tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));
            tRequest.ContentType = " application/json";
            //tRequest.Headers.Add(string.Format("Content-Type : {0}", "application/json"));


            string postData = JsonConvert.SerializeObject(objNotification);
            Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            tRequest.ContentLength = byteArray.Length;

            Stream dataStream = tRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse tResponse = tRequest.GetResponse();

            dataStream = tResponse.GetResponseStream();

            StreamReader tReader = new StreamReader(dataStream);

            String sResponseFromServer = tReader.ReadToEnd();


            tReader.Close();
            dataStream.Close();
            tResponse.Close();
            return sResponseFromServer;
        }
        [HttpPost]
        public CommonApiWraperModel Login(Login login)
        {
            LoginData objILoginData = new LoginData();
            UserModel AttemptedUser = objILoginData.GetUserByUserName(login.username);

            CommonApiWraperModel objApiWrapper = new CommonApiWraperModel();
            if (AttemptedUser != null && (AttemptedUser.Password != null || AttemptedUser.Password != ""))
            {

                List<ApiAuthenticationModel> objapiModel = (new CommonData()).GetAppUser(login.UUID).ToList();
                if (objapiModel.Count > 0)
                {
                    string svalue = objapiModel[0].DynamicSalt;

                    string NewHash = CommonUsage.EncryptPassword(AttemptedUser.Password + svalue);
                    if (login.password == NewHash)
                    {
                        //objapiModel[0].UserID = AttemptedUser.UserID;
                        //objapiModel[0].UserName = AttemptedUser.UserName;
                        //objapiModel[0].UUID = login.UUID;
                        //objapiModel[0].UserType = AttemptedUser.RoleID;
                        //objapiModel[0].SBranchID = AttemptedUser.SBranchID;
                        //objapiModel[0].LastLoginDate = CommonUsage.GetCurrentDate();
                        //objapiModel[0].deviceType = login.deviceType == null ? "" : login.deviceType;
                        //objapiModel[0].deviceToken = login.deviceToken == null ? "" : login.deviceToken;
                        //LuceneData.AddUpdateLuceneIndex(objapiModel[0]);

                        AttemptedUser.Password = "";
                        objApiWrapper.Data = AttemptedUser;
                        objApiWrapper.Code = 200;

                        CommonData objCommonData = new CommonData();
                        ApiAuthenticationModel objModel = new ApiAuthenticationModel();
                        objModel.SBranchID = AttemptedUser.SBranchID;
                        objModel.OpType = 0;
                        objModel.LastLoginDate = CommonUsage.GetCurrentDate();
                        objModel.UserType = AttemptedUser.RoleID;
                        objModel.UUID = login.UUID;
                        objModel.UserID = AttemptedUser.UserID;
                        objModel.deviceToken = login.deviceToken;
                        objCommonData.InsertUpdateAppUser(objModel);
                    }
                    else
                    {
                        objApiWrapper.Code = 101;
                        objApiWrapper.Message = "Unauthorized";
                    }
                }
                else
                {
                    objApiWrapper.Code = 101;
                    objApiWrapper.Message = "Unauthorized";

                }
            }
            else
            {
                objApiWrapper.Code = 101;
                objApiWrapper.Message = "Unauthorized";

            }

            return objApiWrapper;
        }
        [HttpPost]
        public CommonApiWraperModel LogOut(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                CommonData objCommonData = new CommonData();
                ApiAuthenticationModel objModel = new ApiAuthenticationModel();
                objModel.SBranchID = user.SBranchID;
                objModel.OpType = -1;
                objModel.LastLoginDate = CommonUsage.GetCurrentDate();
                objModel.UserType = user.RoleID;
                objModel.UUID = data.UUID;
                objModel.deviceToken = user.AppToken;
                objModel.UserID = user.UserID;
                objCommonData.InsertUpdateAppUser(objModel);
                //LuceneData.ClearLuceneIndexRecord(data.UUID);
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
            return objWraper;
        }

        [HttpPost]
        public CommonApiWraperModel GetNotifications(ParentDiaryModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                CommonData objCommonData = new CommonData();
                data.SBranchID = user.SBranchID;
                data.PBDate = CommonUsage.GetCurrentDate();
                data.TeacherID = user.UserID;
                List<object> List = objCommonData.GetNotifications(user.SBranchID, user.RoleID, user.UserID).ToList<object>();
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
    }
}