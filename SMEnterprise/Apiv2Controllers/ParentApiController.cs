
using SMEnterprise.Models;
using SMEnterprise.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace SMEnterprise.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize(Roles = ApiRole.Parent)]
    public class ParentApiv2Controller : ApiController
    {
        private UserModel VerifyUser(string UUID)
        {
            UserModel objUserModel = new UserModel();
            List<ApiAuthenticationModel> objapiModel = LuceneData.Search(UUID, "UUID").ToList();
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
        public CommonApiWraperModel GetParentChildList(ParentApiParamModel data)
        {
            int ParentID = 1;// CommonUsage.ConvertToInt(User.);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (ParentID != 0)
            {
                ParentData objParentData = new ParentData();
                objWraper.List = objParentData.GetChilds(ParentID).ToList<object>();
                if (objWraper.List.Count > 0)
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
        public async Task<CommonApiWraperModel> GetStudentMonthAttandance(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (VerifyUser(data.UUID) != null)
            {
                ParentData objParentData = new ParentData();
                List<Object> List = (await objParentData.GetStudentMonthlyAttandance(data.ID, data.Month, data.Year)).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
                objWraper.List = List;
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }
           
            return objWraper;
        }
       
        [HttpPost]
        public async Task<CommonApiWraperModel> GetParentDiary(ParentApiParamModel data)
        {
            int ParentID = VerifyUser(data.UUID).UserID == data.ID ? data.ID : 0;
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (ParentID != 0)
            {
                ParentData objParentData = new ParentData();
                List<Object> List =  (await objParentData.GetParentDiary(ParentID)).ToList<object>();
                if (List.Count > 0)
                {
                    objWraper.Code = 200;
                }
                else
                {
                    objWraper.Code = 404;
                }
                objWraper.Message = "Success";
                objWraper.List = List;
            }
            else
            {
                objWraper.Code = 101;
                objWraper.Message = "Unauthorized";
            }

            return objWraper;
        }

        private object await(Task<List<ParentDiaryModel>> task)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public CommonApiWraperModel GetDayPeriods(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (VerifyUser(data.UUID) != null)
            {
                ParentData objParentData = new ParentData();
                object Data = objParentData.GetParentApiTTDayLactures(data);
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
        public CommonApiWraperModel GetSchoolEvents(ParentApiParamModel data)
        {
            int ParentID = VerifyUser(data.UUID).UserID == data.ID ? data.ID : 0;
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (ParentID != 0)
            {
                int year = data.Year;
                int month = data.Month;
                int SBranchID = data.ID;
                CommonData objCommonData = new CommonData();
                Object Data = objCommonData.GetEventCalander(month, year, SBranchID, 0);
                if (Data!=null)
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
        public async Task<CommonApiWraperModel> GetPaymentDetail(ParentApiPaymentDetailParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                ParentData objParentData = new ParentData();
                int Day = CommonUsage.GetCurrentDate().Day;
                List<object> List = (await objParentData.GetPaymentDetails(data.ID, Day, data.Month,data.Year,data.StudentID)).ToList<object>();
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
        public CommonApiWraperModel GetPayments(ParentApiPaymentParamModel data)
        {
            int ParentID = VerifyUser(data.UUID).UserID == data.ID ? data.ID : 0;
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (ParentID != 0)
            {
                ParentData objParentData = new ParentData();
                int Year = CommonUsage.GetCurrentDate().Year;
                int Month = CommonUsage.GetCurrentDate().Month;
                int Day = CommonUsage.GetCurrentDate().Day;
                List<object> List= objParentData.GetPayments(ParentID,Day,Month,Year).ToList<object>();
                if (List.Count>0)
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
        public CommonApiWraperModel GetEventCalendar(ParentApiParamModel data)
        {
            UserModel objModel = VerifyUser(data.UUID);
            int SBranchID = objModel.UserID == data.ID ? objModel.SBranchID : -1;
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (SBranchID != -1)
            {
                CommonData objCommonData = new CommonData();
                Object Data = objCommonData.GetEventCalander(data.Month, data.Year,SBranchID,0);
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
        public async Task<CommonApiWraperModel> GetStudentLeaves(ParentApiParamModel data)
        {
            int StudentID = data.ID;
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (VerifyUser(data.UUID) != null)
            {
                StudentData objCommonData = new StudentData();
                List<object> List = (await objCommonData.GetStudentLeaves(StudentID)).ToList<object>();
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
        public async Task<CommonApiWraperModel> GetAssignmentForStudent(ParentApiParamModel data)
        {
            int StudentID = data.ID;
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (VerifyUser(data.UUID) != null)
            {
                StudentData objCommonData = new StudentData();
                List<object> List =(await objCommonData.GetStudentAssignments(StudentID)).ToList<object>();
                if (List.Count>0)
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
        public async Task<CommonApiWraperModel> AddStudentLeave(StudentLeaveModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            UserModel userdetail = VerifyUser(data.UUID);
            if (userdetail != null)
            {
                data.CreatedDate = CommonUsage.GetCurrentDate();
                data.ApplicantType = 1;
                data.SBranchID = userdetail.SBranchID;
                data.IsApproved = 1;
                data.LeaveType = 1;
                ParentData objParentData = new ParentData();
               int Data =(await objParentData.AddStudentLeave(data));
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
               int Data=(await objCommonData.UpdatePassword(UserID, UserType, Password));
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
        [HttpPost]
        public async Task<CommonApiWraperModel> UpdateDetails(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
           // CommonData.InsertTestLog(data.UUID+"$"+data.EmailID+"$"+data.MobileNumber);
            UserModel userdetail = VerifyUser(data.UUID);
            if (userdetail != null)
            {
               // CommonData.InsertTestLog(data.ID+"%"+data.UUID + "$" + data.EmailID + "$" + data.MobileNumber);

                data.ID = userdetail.UserID;
                ParentData objCommonData = new ParentData();
                int Data =(await objCommonData.UpdateContactDetails(data));
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
        [HttpPost]
        public async Task<CommonApiWraperModel> GetHolidays(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            UserModel user = VerifyUser(data.UUID);
            if (user != null)
            {

                ParentData objCommonData = new ParentData();
                List<object> List = (await objCommonData.GetParentHolidays(user.UserID)).ToList<object>();
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
        public async Task<CommonApiWraperModel> GetNotices(ParentApiPaymentParamModel data)
        {
            int ParentID = VerifyUser(data.UUID).UserID == data.ID ? data.ID : 0;
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (ParentID != 0)
            {
                ParentData objParentData = new ParentData();
                List<object> List = (await objParentData.GetNotices(ParentID)).ToList<object>();
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
        public CommonApiWraperModel GetStudentTransportMap(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (VerifyUser(data.UUID) != null)
            {
                ParentData objParentData = new ParentData();
                object Data = objParentData.GetStudentTransportMap(data.ID);
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
        public async Task<CommonApiWraperModel> GetImportantContacts(ParentApiParamModel data)
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
        #region Black Board
        [HttpPost]
        public CommonApiWraperModel GetBlackBoardList(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                ParentData objParentData = new ParentData();
                object data1 = objParentData.GetBlackBoardListModel(data.ID,data.ID2,data.PageID);
                if (data1 != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = data1;
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
                data.ImageBaseURL = "/images/BlackBoard/";
                data.TeacherID = user.UserID;

                data =await objTeacherData.GetBlackBoardDetail(data.BlackBoardID);
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
        #endregion
    }
}
