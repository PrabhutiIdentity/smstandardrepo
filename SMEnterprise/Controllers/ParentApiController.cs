
using BigBlueButtonAPI.Core;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace SMEnterprise.Controllers
{
    public class ParentApiController : ApiController
    {
<<<<<<< HEAD
        private readonly BigBlueButtonAPIClient client;
        public ParentApiController() : base()
        {
            this.client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
=======
        //private readonly BigBlueButtonAPIClient client;
        public ParentApiController() : base()
        {
            //this.client = new BigBlueButtonAPIClient(MvcApplication.BigBlueButtonAPISettings, MvcApplication.HttpClient);
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
        }
        [HttpGet]
        public CommonApiWraperModel ClearVehicleLocation()
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            try
            {
                GioData.ClearLuceneIndex();
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
        [Route("api/ParentApi/UploadAttachment")]
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

                    string filename = String.Empty;
                    string directoryName = String.Empty;
                    string URL = String.Empty;
                    var thisFileName = postedFile.FileName.Trim('\"');
                    string ID = HttpContext.Current.Request.Form["ID"];
                    var path = HttpRuntime.AppDomainAppPath;
                    directoryName = System.IO.Path.Combine(path, "Attachments\\AssignmentSubmissions");

                    thisFileName = CommonUsage.GetValidFileName(thisFileName);
                    filename = System.IO.Path.Combine(directoryName, ID + "_" + thisFileName);

                    //Deletion exists file  
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }

                    postedFile.SaveAs(filename);

                    (new ParentData()).UpdateAssignmentResponseAttachment(ID, thisFileName);
                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    objWraper.Code = 200;
                    objWraper.Message = "Success";
                    objWraper.Data = "Attachments/AssignmentSubmissions/" + ID + "_" + thisFileName;
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
        //[Route("api/ParentApi/UploadAttachment")]
        //public async Task<CommonApiWraperModel> UploadAttachment()
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
        //            var path = HttpRuntime.AppDomainAppPath;
        //            directoryName = System.IO.Path.Combine(path, "Attachments\\AssignmentSubmissions");

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
        //            (new ParentData()).UpdateAssignmentResponseAttachment(ID, thisFileName);
        //            var response = Request.CreateResponse(HttpStatusCode.OK);
        //            objWraper.Code = 200;
        //            objWraper.Message = "Success";
        //            objWraper.Data = "Attachments/AssignmentSubmissions/" + ID + "_" + thisFileName;
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
        public async Task<CommonApiWraperModel> GetRouteStoppages(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            try
            {

                var oModel = await (new ParentData()).GetStudentRouteDetails(data.ID, CommonUsage.GetCurrentDate());
                var stopsdone = VehicleStopData.SearchDefault("", CommonUsage.GetCurrentDate().ToString("yyyy-MM-dd"), oModel.VehicleRouteID.ToString()).ToList();
                var isFirst = 0;
                foreach (var s in oModel.Stops)
                {
                    if (stopsdone.Where(x => x.StopID == s.StopID).Count() > 0)
                    {
                        s.IsDone = 2;
                    }
                    else
                    {
                        if (isFirst == 0)
                        {
                            s.IsDone = 1;
                            isFirst = 1;
                        }
                        else
                        {
                            s.IsDone = 0;
                        }
                    }
                }
                objWraper.List = oModel.Stops.ToList<object>();
                objWraper.Data = oModel.Conductor;
                objWraper.Code = 200;
                objWraper.Message = "Success";
            }
            catch (Exception ex)
            {
                objWraper.Code = 404;
                objWraper.Message = ex.ToString();
            }

            return objWraper;
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

        #region Login APIs need to remove after next release
        [HttpGet]
        public string GetLoginDynamicSalt(string UUID = null)
        {
            string Salt = CommonUsage.RandomString(10, false);
            List<ApiAuthenticationModel> objapiModel = LuceneData.Search(UUID, "UUID").ToList();
            if (objapiModel.Count > 0)
            {
                objapiModel[0].DynamicSalt = Salt;
                LuceneData.AddUpdateLuceneIndex(objapiModel[0]);
            }
            else
            {
                ApiAuthenticationModel objApiMod = new ApiAuthenticationModel();
                objApiMod.UUID = UUID;
                objApiMod.UserName = "";
                objApiMod.DynamicSalt = Salt;
                LuceneData.AddUpdateLuceneIndex(objApiMod);
            }
            return Salt;
        }

        [HttpPost]
        public CommonApiWraperModel Login(Login login)
        {
            LoginData objILoginData = new LoginData();
            UserModel AttemptedUser = objILoginData.GetUserByUserName(login.username);

            CommonApiWraperModel objApiWrapper = new CommonApiWraperModel();
            if (AttemptedUser != null && (AttemptedUser.Password != null || AttemptedUser.Password != ""))
            {
                List<ApiAuthenticationModel> objapiModel = LuceneData.Search(login.UUID, "UUID").ToList();
                if (objapiModel.Count > 0)
                {
                    string svalue = objapiModel[0].DynamicSalt;

                    string NewHash = CommonUsage.EncryptPassword(AttemptedUser.Password + svalue);
                    if (login.password == NewHash)
                    {
                        objapiModel[0].UserID = AttemptedUser.UserID;
                        objapiModel[0].UserName = AttemptedUser.UserName;
                        objapiModel[0].UUID = login.UUID;
                        objapiModel[0].UserType = AttemptedUser.RoleID;
                        objapiModel[0].SBranchID = AttemptedUser.SBranchID;
                        objapiModel[0].LastLoginDate = CommonUsage.GetCurrentDate();
                        LuceneData.AddUpdateLuceneIndex(objapiModel[0]);
                        AttemptedUser.Password = "";
                        objApiWrapper.Data = AttemptedUser;
                        objApiWrapper.Code = 200;
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
        #endregion
        private UserModel VerifyUser(string UUID)
        {
            UserModel objUserModel = new UserModel();
            // List<ApiAuthenticationModel> objapiModel = LuceneData.Search(UUID, "UUID").ToList();
            List<ApiAuthenticationModel> objapiModel = (new CommonData()).GetAppUser(UUID);
            bool isFound = false;
            if (objapiModel.Count > 0)
            {
                foreach (ApiAuthenticationModel a in objapiModel)
                {
                    if (a.UserType == (int)RoleType.Parent)
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
            { objUserModel = null; }
            return objUserModel;
        }
        [HttpPost]
        public async Task<CommonApiWraperModel> GetParentChildList(ParentApiParamModel data)
        {
            var usr = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (usr != null)
            {
                int ParentID = usr.UserID;
                ParentData objParentData = new ParentData();
                objWraper.List = (await objParentData.GetChildsObject(ParentID)).ToList<object>();
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
                List<Object> List = (await objParentData.GetParentDiary(ParentID)).ToList<object>();
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
        public async Task<CommonApiWraperModel> GetDayPeriods(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (VerifyUser(data.UUID) != null)
            {
                ParentData objParentData = new ParentData();
                object Data = await objParentData.GetParentApiTTDayLactures(data);
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
        public async Task<CommonApiWraperModel> GetSchoolEvents(ParentApiParamModel data)
        {
            var usr = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (usr != null)
            {
                int ParentID = usr.UserID == data.ID ? data.ID : 0;
                int year = data.Year;
                int month = data.Month;
                int SBranchID = data.ID;
                CommonData objCommonData = new CommonData();
                Object Data = await objCommonData.GetEventCalander(month, year, SBranchID, 1, ParentID);
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
        public async Task<CommonApiWraperModel> GetPaymentDetail(ParentApiPaymentDetailParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                ParentData objParentData = new ParentData();
                int Day = CommonUsage.GetCurrentDate().Day;
                List<object> List = (await objParentData.GetPaymentDetails(data.ID, Day, data.Month, data.Year, data.StudentID)).ToList<object>();
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
        public async Task<CommonApiWraperModel> GetPayments(ParentApiPaymentParamModel data)
        {
            var usr = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (usr != null)
            {
                int ParentID = usr.UserID == data.ID ? data.ID : 0;
                ParentData objParentData = new ParentData();
                int Year = CommonUsage.GetCurrentDate().Year;
                int Month = CommonUsage.GetCurrentDate().Month;
                int Day = CommonUsage.GetCurrentDate().Day;
                List<object> List = await objParentData.GetPaymentsNew(ParentID, Day, Month, Year);
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
        public async Task<CommonApiWraperModel> GetPaymentDetails(ParentApiPaymentParamModel data)
        {
            var User = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (User != null)
            {
                int ParentID = User.UserID;
                FeePaymentModel objModel = new FeePaymentModel();

                AccountData objAccountData = new AccountData();
                objModel.Year = CommonUsage.GetCurrentDate().Year;
                objModel.Month = CommonUsage.GetCurrentDate().Month;
                objModel.Day = CommonUsage.GetCurrentDate().Day;
                objModel.QDate = CommonUsage.GetCurrentDate();
                objModel.SBranchID = User.SBranchID;
                objModel.StudentID = data.ID;
                FeePaymentModel Data = await objAccountData.GetFeePaymentDetailsNew2(objModel);
                FeeDetailsAppResponseModel objResponse = new FeeDetailsAppResponseModel();
                objResponse.Months = new List<FeeDetailsAppResponseMonthModel>();
                decimal ApplicableFee = 0;
                decimal PaymentRecieved = 0;
                decimal Waiver = 0;
                foreach (var m in Data.Months)
                {
                    FeeDetailsAppResponseMonthModel oMonth = new FeeDetailsAppResponseMonthModel();
                    oMonth.FeeMonth = m.FeeMonth;
                    oMonth.FeeYear = m.FeeYear;
                    oMonth.ApplicableFee = m.ApplicableFee;
                    ApplicableFee = ApplicableFee + m.ApplicableFee;
                    oMonth.PaymentRecieved = m.PaymentRecieved;
                    PaymentRecieved = PaymentRecieved + m.PaymentRecieved;
                    oMonth.Waiver = m.Waiver;
                    Waiver = Waiver + m.Waiver;
                    oMonth.Summery = new List<FeeDetailsAppResponseMonthFeeModel>();
                    foreach (var f in Data.PaymentDetails.Where(x => x.FeeYear == m.FeeYear && x.FeeMonth == m.FeeMonth))
                    {
                        FeeDetailsAppResponseMonthFeeModel oFee = new FeeDetailsAppResponseMonthFeeModel();
                        oFee.FeeTypeName = f.FeeTypeName;
                        oFee.PaidAmount = f.PaidAmount;
                        oFee.Discount = f.RDiscount;
                        oFee.ApplicableFee = (f.IsPayment == 1 ? f.PayApplicableAmount : f.IsCustomFee == 1 ? f.CustomFee : (f.FeeAmount - f.FeeAmount * f.QDiscount / 100));
                        oFee.NetPayable = oFee.ApplicableFee - f.RDiscount - f.PaidAmount;
                        oMonth.Summery.Add(oFee);
                    }
                    objResponse.Months.Add(oMonth);
                }
                FeeDetailsAppResponseMonthModel oTotal = new FeeDetailsAppResponseMonthModel();
                oTotal.FeeMonth = 0;
                oTotal.FeeYear = 0;
                oTotal.ApplicableFee = ApplicableFee;
                oTotal.PaymentRecieved = PaymentRecieved;
                oTotal.Waiver = Waiver;
                oTotal.Summery = new List<FeeDetailsAppResponseMonthFeeModel>();
                foreach (var f in Data.FeeTypeSummery)
                {
                    FeeDetailsAppResponseMonthFeeModel oFee = new FeeDetailsAppResponseMonthFeeModel();
                    oFee.FeeTypeName = f.FeeTypeName;
                    oFee.PaidAmount = f.PaymentRecieved;
                    oFee.Discount = f.Waiver;
                    oFee.ApplicableFee = f.ApplicableFee;
                    oFee.NetPayable = oFee.ApplicableFee - f.Waiver - f.PaymentRecieved;
                    oTotal.Summery.Add(oFee);
                }
                objResponse.Months.Add(oTotal);

                if (objResponse != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = objResponse;
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
        public async Task<CommonApiWraperModel> GetFeeReciepts(ParentApiPaymentParamModel data)
        {
            var usr = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (usr != null)
            {
                int ParentID = usr.UserID == data.ID ? data.ID : 0;
                ParentData objParentData = new ParentData();
                int Year = CommonUsage.GetCurrentDate().Year;
                int Month = CommonUsage.GetCurrentDate().Month;
                int Day = CommonUsage.GetCurrentDate().Day;
                List<PaymentModel> Payments = await objParentData.GetParentPayments(ParentID, Day, Month, Year);

                if (Payments.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = Payments.ToList<object>();
                }
                else
                {
                    objWraper.Code = 404;
                    objWraper.List = new List<object>();
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
        public async Task<CommonApiWraperModel> GetFeeRecieptDetails(ParentApiParamModel data)
        {
            var usr = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (usr != null)
            {
                int ParentID = usr.UserID == data.ID ? data.ID : 0;
                AccountData objParentData = new AccountData();
                FeePaymentModel model = await objParentData.GetFeePaymentReciptDetails(data.ID2);
                model.ApplicableFee = model.PaymentDetails.Sum(x => x.NetApplicablePayment);
                if (model != null)
                {
                    objWraper.Code = 200;
                    objWraper.Data = model;
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
        public async Task<CommonApiWraperModel> GetEventCalendar(ParentApiParamModel data)
        {
            UserModel objModel = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (objModel != null)
            {
                int SBranchID = objModel.UserID == data.ID ? objModel.SBranchID : -1;
                CommonData objCommonData = new CommonData();
<<<<<<< HEAD
                Object Data = await objCommonData.GetEventCalander(data.Month, data.Year, SBranchID, 1, data.ID);
=======
                int Year = CommonUsage.GetCurrentDate().Year;
                int Month = CommonUsage.GetCurrentDate().Month;
             
                //   Object Data = await objCommonData.GetEventCalander(data.Month, data.Year, objModel.SBranchID, 1, data.ID);

                Object Data = await objCommonData.GetEventCalander(Month, Year, objModel.SBranchID, 1, data.ID);



             
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
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
                List<AssignmentModel> list;
                if (data.ID2 > 0)
                {
                    list = (await objCommonData.GetStudentAssignments(StudentID, data.ID2)).ToList();
                }
                else
                {
                    list = (await objCommonData.GetStudentAssignments(StudentID)).ToList();
                }

                list.ForEach(x => x.Attachments = x.Attachments == null ? x.Attachments : x.Attachments.Replace(",", ";"));
                if (list.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = list.ToList<object>();
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
        public async Task<CommonApiWraperModel> SubmitAssignmentResponse(AssignmentSubmissionModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (VerifyUser(data.UUID) != null)
            {
                data.SubmissionDate = CommonUsage.GetCurrentDate();
                ParentData objCommonData = new ParentData();
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
                int AssResponseID = await objCommonData.SubmitAssignmentResponse(data);
                if (AssResponseID == 0)
                {
                    objWraper.Code = 404;
                }
                else
                {
                    objWraper.Code = 200;
                    objWraper.Data = AssResponseID;
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
        public async Task<CommonApiWraperModel> GetAssignmentResponse(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (VerifyUser(data.UUID) != null)
            {
                ParentData objCommonData = new ParentData();
                AssignmentSubmissionModel oModel = await objCommonData.GetStudentAssignmentResponse(data.ID, data.ID2);
                //if(oModel ==null)
                //{
                //    oModel = new AssignmentSubmissionModel();
                //    oModel.AssignmentID = data.ID2;
                //}
                if (oModel == null)
                {
                    objWraper.Code = 404;
                }
                else
                {
                    oModel.BasePath = "/Attachments/AssignmentSubmissions/";
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
                int Data = await objParentData.AddStudentLeave(data);
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
                int Data = await objCommonData.UpdateContactDetails(data);
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
        public async Task<CommonApiWraperModel> GetStudentTransportMap(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (VerifyUser(data.UUID) != null)
            {
                ParentData objParentData = new ParentData();
                object Data = await objParentData.GetStudentTransportMap(data.ID);
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
            var usr = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (usr != null)
            {
                int SBranchID = usr.SBranchID;
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
        [HttpPost]
        public async Task<CommonApiWraperModel> GetSubjects(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            UserModel userdetail = VerifyUser(data.UUID);
            if (userdetail != null)
            {
                // CommonData.InsertTestLog(data.ID+"%"+data.UUID + "$" + data.EmailID + "$" + data.MobileNumber);

                ParentData objCommonData = new ParentData();
                List<NameIDModel> list = await objCommonData.GetSubjectsForStudent(data.ID2);
                if (list.Count > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = list.ToList<object>();
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
        public async Task<CommonApiWraperModel> GetYouTubeVideos(YouTubeVideoModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                CommonData objTeacherData = new CommonData();
                List<object> List = (await objTeacherData.GetYoutubeVideosParent(data.StudentID, data.SubjectID)).ToList<object>();
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
        public async Task<CommonApiWraperModel> GetBlackBoardList(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                ParentData objParentData = new ParentData();
                TeacherBlackBoardPageModel data1 = await objParentData.GetBlackBoardListModel(data.ID, data.ID2, data.PageID);
                data1.ImageBaseURL = "/images/BlackBoard/";
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
                data = await objTeacherData.GetBlackBoardDetail(data.BlackBoardID);
                data.ImageBaseURL = "/images/BlackBoard/";
                data.TeacherID = user.UserID;

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
        #region Online Classes Related
<<<<<<< HEAD
        [HttpPost]
        public async Task<CommonApiWraperModel> GetBBBOnlineClassList(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                // OnlineClassData objParentData = new OnlineClassData(); 
                if (data.RDate.Year == 1)
                {
                    data.RDate = CommonUsage.GetCurrentDate();
                }
                var data1 = await (new BBBOnlineClassData()).GetStudentOnlineClassesSchedules(data.ID, data.RDate);
                //objWraper.Data = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
                if (data1.Count() > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = data1.ToList<object>();
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
        public async Task<CommonApiWraperModel> GetOnlineClassList(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                if (user.SBranchID != 18)
                {
                    // OnlineClassData objParentData = new OnlineClassData(); 
                    if (data.RDate.Year == 1)
                    {
                        data.RDate = CommonUsage.GetCurrentDate();
                    }
                    var data1 = await (new BBBOnlineClassData()).GetStudentOnlineClassesSchedules(data.ID, data.RDate);
                    //objWraper.Data = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
                    if (data1.Count() > 0)
                    {
                        objWraper.Code = 200;
                        objWraper.List = data1.ToList<object>();
                    }
                    else
                    {
                        objWraper.Code = 404;
                    }
                    objWraper.Message = "Success";
                }
                else
                {
                    OnlineClassData objParentData = new OnlineClassData();
                    DateTime CurDate = CommonUsage.GetCurrentDate();
                    List<OnlineClassModel> data1 = await objParentData.GetStudentOnlineClassesSchedules(data.ID, CurDate);
                    objWraper.Data = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
                    if (data1.Count() > 0)
                    {
                        objWraper.Code = 200;
                        objWraper.List = data1.ToList<object>();
                    }
                    else
                    {
                        objWraper.Code = 404;
                    }
                    objWraper.Message = "Success";
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
        public async Task<CommonApiWraperModel> GetOnlineClassListOld(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                OnlineClassData objParentData = new OnlineClassData();
                DateTime CurDate = CommonUsage.GetCurrentDate();
                List<OnlineClassModel> data1 = await objParentData.GetStudentOnlineClassesSchedules(data.ID, CurDate);
                objWraper.Data = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
                if (data1.Count() > 0)
                {
                    objWraper.Code = 200;
                    objWraper.List = data1.ToList<object>();
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
        public async Task<CommonApiWraperModel> JoinBBBOnlineClass(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                BBBOnlineClassData onlineClassData = new BBBOnlineClassData();
                //var Student = await onlineClassData.GetStudentForOnlineClassByMeetinID(data.OCID,user.UserID);
                BBBOnlineClassModel cModel = (onlineClassData).GetOnlineClassDetailsByOCID(data.OCID);

                var meetingStatus = await client.GetMeetingInfoAsync(new GetMeetingInfoRequest { meetingID = cModel.MeetingID });
                if (meetingStatus.returncode != Returncode.FAILED)
                {

                    var joinDate = CommonUsage.GetCurrentDate();
                    NameIDModel student = await onlineClassData.StudentJoinOnlineClass(0, joinDate, data.OCID, user.UserID, 1);
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

        [HttpPost]
        public async Task<CommonApiWraperModel> JoinOnlineClass(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                DateTime JoinedOn = CommonUsage.GetCurrentDate();
                OnlineClassData objTeacherData = new OnlineClassData();
                object List = await objTeacherData.StudentJoinOnlineClass(data.ID, JoinedOn, data.OCID, user.UserID);
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
        public async Task<CommonApiWraperModel> LeaveOnlineClass(ParentApiParamModel data)
        {
            UserModel user = VerifyUser(data.UUID);
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            if (user != null)
            {
                DateTime JoinedOn = CommonUsage.GetCurrentDate();
                OnlineClassData objTeacherData = new OnlineClassData();
                int List = await objTeacherData.StudentLeaveOnlineClass(data.ID, JoinedOn, data.ID2);
                if (List > 0)
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
=======
        //[HttpPost]
        //public async Task<CommonApiWraperModel> GetBBBOnlineClassList(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        // OnlineClassData objParentData = new OnlineClassData(); 
        //        if (data.RDate.Year == 1)
        //        {
        //            data.RDate = CommonUsage.GetCurrentDate();
        //        }
        //        var data1 = await (new BBBOnlineClassData()).GetStudentOnlineClassesSchedules(data.ID, data.RDate);
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
        //public async Task<CommonApiWraperModel> GetOnlineClassList(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        if (user.SBranchID != 18)
        //        {
        //            // OnlineClassData objParentData = new OnlineClassData(); 
        //            if (data.RDate.Year == 1)
        //            {
        //                data.RDate = CommonUsage.GetCurrentDate();
        //            }
        //            var data1 = await (new BBBOnlineClassData()).GetStudentOnlineClassesSchedules(data.ID, data.RDate);
        //            //objWraper.Data = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
        //            if (data1.Count() > 0)
        //            {
        //                objWraper.Code = 200;
        //                objWraper.List = data1.ToList<object>();
        //            }
        //            else
        //            {
        //                objWraper.Code = 404;
        //            }
        //            objWraper.Message = "Success";
        //        }
        //        else
        //        {
        //            OnlineClassData objParentData = new OnlineClassData();
        //            DateTime CurDate = CommonUsage.GetCurrentDate();
        //            List<OnlineClassModel> data1 = await objParentData.GetStudentOnlineClassesSchedules(data.ID, CurDate);
        //            objWraper.Data = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
        //            if (data1.Count() > 0)
        //            {
        //                objWraper.Code = 200;
        //                objWraper.List = data1.ToList<object>();
        //            }
        //            else
        //            {
        //                objWraper.Code = 404;
        //            }
        //            objWraper.Message = "Success";
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
        //public async Task<CommonApiWraperModel> GetOnlineClassListOld(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        OnlineClassData objParentData = new OnlineClassData();
        //        DateTime CurDate = CommonUsage.GetCurrentDate();
        //        List<OnlineClassModel> data1 = await objParentData.GetStudentOnlineClassesSchedules(data.ID, CurDate);
        //        objWraper.Data = OnlineClassData.SBranchesOnlineClassURLs.Where(x => x.ID == user.SBranchID).FirstOrDefault().Name;
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
        //        BBBOnlineClassData onlineClassData = new BBBOnlineClassData();
        //        //var Student = await onlineClassData.GetStudentForOnlineClassByMeetinID(data.OCID,user.UserID);
        //        BBBOnlineClassModel cModel = (onlineClassData).GetOnlineClassDetailsByOCID(data.OCID);

        //        var meetingStatus = await client.GetMeetingInfoAsync(new GetMeetingInfoRequest { meetingID = cModel.MeetingID });
        //        if (meetingStatus.returncode != Returncode.FAILED)
        //        {

        //            var joinDate = CommonUsage.GetCurrentDate();
        //            NameIDModel student = await onlineClassData.StudentJoinOnlineClass(0, joinDate, data.OCID, user.UserID, 1);
        //            var requestJoin = new JoinMeetingRequest { meetingID = cModel.MeetingID };
        //            requestJoin.userID = student.ID.ToString();
        //            requestJoin.fullName = student.Name;
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
        //public async Task<CommonApiWraperModel> JoinOnlineClass(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        DateTime JoinedOn = CommonUsage.GetCurrentDate();
        //        OnlineClassData objTeacherData = new OnlineClassData();
        //        object List = await objTeacherData.StudentJoinOnlineClass(data.ID, JoinedOn, data.OCID, user.UserID);
        //        if (List != null)
        //        {
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
        //public async Task<CommonApiWraperModel> LeaveOnlineClass(ParentApiParamModel data)
        //{
        //    UserModel user = VerifyUser(data.UUID);
        //    CommonApiWraperModel objWraper = new CommonApiWraperModel();
        //    if (user != null)
        //    {
        //        DateTime JoinedOn = CommonUsage.GetCurrentDate();
        //        OnlineClassData objTeacherData = new OnlineClassData();
        //        int List = await objTeacherData.StudentLeaveOnlineClass(data.ID, JoinedOn, data.ID2);
        //        if (List > 0)
        //        {
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
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
        #endregion
    }
}
