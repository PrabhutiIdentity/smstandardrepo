using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using SMEnterprise.Filters;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using System.Data;
using static SMEnterprise.Repository.CommonUsage;
using ICSharpCode.SharpZipLib.Zip;

namespace SMEnterprise.Controllers
{
    public class AccountController : Controller
    {
        private bool HasOverlappingStudentSession(SessionModel objData, int sBranchID)
        {
            var studentDetails = objAccountData.GetStudentDetailsNew(objData.StudentID, sBranchID);
            var existingSessions = studentDetails.Sessions ?? new List<SessionModel>();

            return existingSessions.Any(x =>
                x.SessionID == objData.SessionID &&
                x.StudentSessionUID != objData.StudentSessionUID &&
                objData.FromDate <= x.ToDate &&
                objData.ToDate >= x.FromDate);
        }

        [PermissionFilter]
        public ActionResult ParentAppDetail(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetParentAppDetail(objModel.ClassID, objModel.SectionID, SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ChangePassword()
        {
            return View();
        }
        [PermissionFilter]
        public ActionResult Vendors()
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            VendorPageModel objModel = objAccountData.GetVendors(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GetVendorDetails(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            VendorPageModel objModel = objAccountData.GetVendorDetails(SBranchID, iID);
            return PartialView("_EditVendorDetails", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateVendor(VendorModel oModel)
        {
            oModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int res = objAccountData.InsertUpdateVendor(oModel);
            return RedirectToAction("Vendors");
        }
        public ActionResult GetSessionState()
        {
            string data = "Start :" + CommonUsage.SessionStart.ToString() + " End :" + CommonUsage.SessionEnd.ToString();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        AccountData objAccountData = new AccountData();
        TeacherData objTeacherData = new TeacherData();
        AdminData objAdminData = new AdminData();
        // GET: Account
        [PermissionFilter]
        public ActionResult Dashboard(AccountDashboardModel objData)
        {
            if (objData.Year == 0)
            {
                objData.Year = CommonUsage.GetCurrentDate().Year;
                objData.Month = CommonUsage.GetCurrentDate().Month;
                objData.DayID = CommonUsage.GetCurrentDate().Day;
            }
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objData = objAccountData.GetDashboardPageData(objData);

            return View(objData);
        }
        [PermissionFilter]
        public ActionResult SchoolSubscription()
        {
            int sBranchId = PermissionManager.GetLoggedInUser().SBranchID;
            var model = objAccountData.GetBranchSubscriptionAccount(sBranchId);
            return View(model);
        }
        [PermissionFilter]
        public ActionResult GetDashboardFeeChartData(AccountDashboardModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            AccountDashboardModel objModel = objAccountData.GetDashboardFeeChart(objData);

            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult StudentQuickEdit(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStudentRecords(objModel.ClassID, objModel.SectionID, SBranchID, objModel.SessionID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult StudentQuickUpdate(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int a = objAccountData.QuickStudentUpdate(SBranchID, objModel);
            return RedirectToAction("StudentQuickEdit", "Account", new { ClassID = objModel.ClassID, SectionID = objModel.SectionID, SessionID = objModel.SessionID });
        }
        [PermissionFilter]
        public ActionResult Students(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStudents(objModel.ClassID, objModel.SectionID, SBranchID, objModel.SessionID);
            return View(objModel);
        }
       
        
        [PermissionFilter]
        public ActionResult StudentInActive(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStudentInactive(objModel.ClassID, objModel.SectionID, SBranchID, objModel.SessionID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult InactiveStudents(StudentSearchListModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentSearchListModel();
            }
            if (objModel.SearchText != null)
            {
                objModel.SearchText = objModel.SearchText.Trim();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.SuspendedStudents(SBranchID, objModel.SessionID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult SearchStudents(StudentSearchListModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentSearchListModel();
            }
            if (objModel.SearchText != null)
            {
                objModel.SearchText = objModel.SearchText.Trim();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.SearchStudents(objModel.SearchText, SBranchID, objModel.SessionID);
            return View(objModel);
        }


        [PermissionFilter]
        public ActionResult DeleteStudent(StudentsPageModel objModel)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAccountData.DeleteStudents(objModel.StudentID);
            return RedirectToAction("Students", "Account", objModel);
        }
        [PermissionFilter]
        public ActionResult GetSessionClassSectionOnBranch(string ID = null)
        {
            int SBranchID = CommonUsage.ConvertToInt(ID);
            StudentPromotionModel objModel = objAccountData.GetSessionClassSectionOnBranch(SBranchID);
            return Json(objModel);
        }
        [PermissionFilter]
        public ActionResult PromoteStudents(StudentPromotionModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentPromotionModel();
            }
            if (objModel.SBranchID == 0)
            {
                objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            }
            objModel = objAccountData.GetPromoteStudents(objModel.ClassID, objModel.SectionID, objModel.SBranchID, objModel.SessionID);
            if (objModel.SBranchID == 0)
            {
                objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            }
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdatePromotedStudents(StudentPromotionModel objModel)
        {
            if (objModel.SBranchID == 0)
            {
                objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            }
            int Status = objAccountData.UpdatePromotedStudents(objModel);
            StudentsPageModel objNewModel = new StudentsPageModel();
            objNewModel.ClassID = objModel.ClassID;
            objNewModel.SectionID = objModel.SectionID;
            objNewModel.SessionID = objModel.SessionID;
            return RedirectToAction("Students", "Account", objNewModel);
        }
        [PermissionFilter]
        public ActionResult StudentDetails(string id = null, string id2 = null)
        {
            int StudentID = CommonUsage.ConvertToInt(id);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            StudentEditModel objModel = objAccountData.GetStudentDetailsNew(StudentID, SBranchID);
            int cTab = CommonUsage.ConvertToInt(id2);
            objModel.CurrentTab = cTab == 0 ? 1 : cTab;
            ViewBag.SessionSaveMessage = TempData["SessionSaveMessage"];
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult SaveStudentPersonal(StudentModel objData)
        {
            string PassWord = objData.Name.Replace(" ", "").Replace(".", "").PadRight(4, '0').Substring(0, 4).ToUpper();
            string DateBirth = objData.DOB.Day.ToString("00") + objData.DOB.Month.ToString("00");
            PassWord = PassWord + DateBirth;
            objData.Password = CommonUsage.EncryptPassword(PassWord + CommonUsage.FixedPrimaryEncryptionSalt);
            objData.AdmissionBy = PermissionManager.GetLoggedInUser().UserID;
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            string OldImageFile = "";
            if (objData.StudentImageUploader != null)
            {
                OldImageFile = objData.Photo;
                objData.Photo = objData.StudentImageUploader.FileName.Replace(" ", "-");
            }
            objData.StudentID = objAccountData.InsertUpdateStudentBasic(objData);
            if (objData.StudentImageUploader != null)
            {
                try
                {
                    System.IO.File.Delete(
                        Server.MapPath(CommonUsage.StudentImageBasePath + objData.StudentID + "_" + OldImageFile));

                }
                catch (Exception ex)
                {


                }
                var path = Path.Combine(Server.MapPath(CommonUsage.StudentImageBasePath),
                   objData.StudentID + "_" + objData.Photo);
                objData.StudentImageUploader.SaveAs(path);

            }
            return Redirect("~/Account/StudentDetails/" + objData.StudentID + "/" + 2);
        }
        public ActionResult UpdateStudentImages()
        {
            string path = Server.MapPath(CommonUsage.StudentImageBasePath);
            string[] filePaths = Directory.GetFiles(path);
            foreach (string img in filePaths)
            {
                CommonUsage.SaveSmallImage(img);
            }
            return View();
        }

        [PermissionFilter]
        public ActionResult SaveStudentParent(ParentModel objData)
        {
            string PassWord = objData.FatherName.Replace(" ", "").Replace(".", "").Substring(0, 4).ToUpper();
            string DateBirth = objData.FatherDOB.Day.ToString("00") + objData.FatherDOB.Month.ToString("00");
            PassWord = PassWord + DateBirth;
            objData.Password = CommonUsage.EncryptPassword(PassWord + CommonUsage.FixedPrimaryEncryptionSalt);
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            string OldImageFile = "";
            if (objData.FatherImageUploader != null)
            {
                OldImageFile = objData.FatherImage;
                objData.FatherImage = objData.FatherImageUploader.FileName.Replace(" ", "-");
            }
            if (objData.MotherImageUploader != null)
            {
                OldImageFile = objData.MotherImage;
                objData.MotherImage = objData.MotherImageUploader.FileName.Replace(" ", "-");
            }
            objData.ParentID = objAccountData.InsertUpdateStudentParent(objData);
            if (objData.FatherImageUploader != null)
            {
                try
                {
                    System.IO.File.Delete(
                        Server.MapPath(CommonUsage.PFatherImageUploadBasePath + objData.ParentID + "_1_" + OldImageFile));

                }
                catch (Exception ex)
                {


                }
                var path = Path.Combine(Server.MapPath(CommonUsage.PFatherImageUploadBasePath),
                  objData.ParentID + "_1_" + objData.FatherImage);
                objData.FatherImageUploader.SaveAs(path);

            }
            if (objData.MotherImageUploader != null)
            {
                try
                {
                    System.IO.File.Delete(
                        Server.MapPath(CommonUsage.PMotherImageUploadBasePath + objData.ParentID + "_1_" + OldImageFile));

                }
                catch (Exception ex)
                {


                }
                var path = Path.Combine(Server.MapPath(CommonUsage.PMotherImageUploadBasePath),
                  objData.ParentID + "_2_" + objData.MotherImage);
                objData.MotherImageUploader.SaveAs(path);

            }
            return Redirect("~/Account/StudentDetails/" + objData.StudentID + "/" + 3);
        }
        [PermissionFilter]
        public ActionResult GetParentDetailsOnParentID(string id)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            ParentModel objModel = objAccountData.GetParentDetailsOnParentID(id, SBranchID);
            if (objModel == null)
            {

                objModel.FatherDOB = CommonUsage.GetCurrentDate();
                objModel.MotherDOB = CommonUsage.GetCurrentDate();
            }

            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult SaveStudentAddress(StudentModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            objAccountData.UpdateStudentAddress(objData);

            return Redirect("~/Account/StudentDetails/" + objData.StudentID + "/" + 4);
        }
        [PermissionFilter]
        public ActionResult StudentSessionCustomFeeDetails(string id = null)
        {
            int StudentID = CommonUsage.ConvertToInt(id);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            StudentEditModel objModel = objAccountData.GetStudentSessionCustomFee(StudentID, SBranchID);
            if (objModel.CustomeFee.Count() > 0)
            {
                objModel.Student.StudentID = objModel.CustomeFee.FirstOrDefault().StudentID;
            }
            return PartialView("_StudentCustomFeeEditPartial", objModel);
        }
        #region Location Details
        [PermissionFilter]
        public ActionResult GetCountryStates(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            IEnumerable<NameIDModel> model = objAccountData.GetCountryStates(iID);

            return PartialView("_SelectOptionsPartial", model);
        }
        [PermissionFilter]
        public ActionResult GetStateCities(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            IEnumerable<NameIDModel> model = objAccountData.GetStateCities(iID);

            return PartialView("_SelectOptionsPartial", model);
        }
        [PermissionFilter]
        public ActionResult GetCityAreas(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            IEnumerable<NameIDModel> model = objAccountData.GetCityAreas(iID);

            return PartialView("_SelectOptionsPartial", model);
        }
        [PermissionFilter]
        public ActionResult AddLocation(string ID = null, string ID2 = null, string ID3 = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            int iID2 = CommonUsage.ConvertToInt(ID2);
            string Name = ID3;
            IEnumerable<NameIDModel> model = objAccountData.AddLocation(iID, iID2, Name);

            return PartialView("_SelectOptionsPartial", model);
        }
        #endregion
        [PermissionFilter]
        public ActionResult SaveStudentGuardian(StudentModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            objAccountData.UpdateStudentGuardian(objData);

            return Redirect("~/Account/StudentDetails/" + objData.StudentID + "/" + 5);
        }
        [PermissionFilter]
        public ActionResult SaveStudentDocuments(StudentModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            string OldBirthImage = "";
            string OldAddImage = "";
            string OldCategoryImage = "";
            string OldTransferImage = "";
            if (objData.TransferCertificateUploader != null)
            {
                OldTransferImage = objData.TransferCertificate;
                objData.TransferCertificate = objData.TransferCertificateUploader.FileName.Replace(" ", "-");
                System.IO.File.Delete(
                        Server.MapPath(CommonUsage.StudentDocumentsBasePath + objData.StudentID + "_Transfer_" + OldTransferImage));

                var path = Path.Combine(Server.MapPath(CommonUsage.StudentDocumentsBasePath),
                 objData.StudentID + "_Transfer_" + objData.TransferCertificate);
                objData.TransferCertificateUploader.SaveAs(path);
            }
            if (objData.BirthCertificateUploader != null)
            {
                OldBirthImage = objData.BirthCertificate;
                objData.BirthCertificate = objData.BirthCertificateUploader.FileName.Replace(" ", "-");
                System.IO.File.Delete(
                       Server.MapPath(CommonUsage.StudentDocumentsBasePath + objData.StudentID + "_Birth_" + OldBirthImage));

                var path = Path.Combine(Server.MapPath(CommonUsage.StudentDocumentsBasePath),
                 objData.StudentID + "_Birth_" + objData.BirthCertificate);
                objData.BirthCertificateUploader.SaveAs(path);
            }
            if (objData.AddressCertificateUploader != null)
            {
                OldAddImage = objData.AddressProof;
                objData.AddressProof = objData.AddressCertificateUploader.FileName.Replace(" ", "-");
                System.IO.File.Delete(
                      Server.MapPath(CommonUsage.StudentDocumentsBasePath + objData.StudentID + "_Address_" + OldAddImage));

                var path = Path.Combine(Server.MapPath(CommonUsage.StudentDocumentsBasePath),
                 objData.StudentID + "_Address_" + objData.AddressProof);
                objData.AddressCertificateUploader.SaveAs(path);
            }
            if (objData.CategoryCertificateUploader != null)
            {
                OldCategoryImage = objData.CategoryCertificate;
                objData.CategoryCertificate = objData.CategoryCertificateUploader.FileName.Replace(" ", "-");
                System.IO.File.Delete(
                      Server.MapPath(CommonUsage.StudentDocumentsBasePath + objData.StudentID + "_Category_" + OldCategoryImage));

                var path = Path.Combine(Server.MapPath(CommonUsage.StudentDocumentsBasePath),
                 objData.StudentID + "_Category_" + objData.CategoryCertificate);
                objData.CategoryCertificateUploader.SaveAs(path);
            }
            objAccountData.UpdateStudentDocuments(objData);

            return Redirect("~/Account/StudentDetails/" + objData.StudentID + "/" + 6);
        }
        [PermissionFilter]
        public ActionResult StudentViewDetails(string id = null)
        {
            int StudentID = CommonUsage.ConvertToInt(id);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            StudentEditModel objModel = objAccountData.GetStudentDetailsPrint(StudentID, SBranchID);
            objModel.CurrentTab = 1;
            return PartialView("_StudentViewPartial", objModel);
        }





        public ActionResult TCRequest(string id = null)
        {
            int StudentID = CommonUsage.ConvertToInt(id);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            StudentEditModel objModel = objAccountData.GetStudentDetailsPrint(StudentID, SBranchID);
            objModel.CurrentTab = 1;
            return PartialView("_StudentTCPartial", objModel);
        }





        [PermissionFilter]
        public ActionResult GetSectionOptionalSubjects(string id = null)
        {
            int SectionID = CommonUsage.ConvertToInt(id);
            List<NameIDModel> objModel = objAccountData.GetOptionalSubjectsForSection(SectionID);
            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult EditSessionDetails(string ID = null, string ID2 = null)
        {
            int StudentSessionUID = CommonUsage.ConvertToInt(ID);
            int StudentID = CommonUsage.ConvertToInt(ID2);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            SessionEditModel objModel = objAccountData.GetSessionDetails(StudentSessionUID, SBranchID);
            objModel.StudentID = StudentID;
            ViewBag.IsNewSessionRequest = StudentSessionUID == 0 ? 1 : 0;
            return PartialView("_SessionEditPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult SaveStudentSession(SessionModel objData, int IsNewSessionRequest = 0)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objData.IsNewSessionRequest = IsNewSessionRequest;
            if (IsNewSessionRequest == 1 && HasOverlappingStudentSession(objData, objData.SBranchID))
            {
                TempData["SessionSaveMessage"] = "Same session/date range already exists for this student. New session was not added.";
                return Redirect("~/Account/StudentDetails/" + objData.StudentID + "/" + 6);
            }

            int result = objAccountData.UpdateStudentSession(objData);
            if (result <= 0)
            {
                TempData["SessionSaveMessage"] = "Same session/date range already exists for this student. New session was not added.";
            }
            return Redirect("~/Account/StudentDetails/" + objData.StudentID + "/" + 6);
        }
        [PermissionFilter]
        public ActionResult GetAreaRoutes(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            IEnumerable<NameIDModel> model = objAccountData.GetAreaRoutes(iID, SBranchID);

            return PartialView("_SelectOptionsPartial", model);
        }
        [PermissionFilter]
        public ActionResult GetRouteVehicles(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            IEnumerable<NameIDModel> model = objAccountData.GetRouteVehicles(iID, SBranchID);

            return Json(model, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult EditEmployeeTransportDetails(string ID = null, string ID2 = null)
        {
            int EmployeeID = CommonUsage.ConvertToInt(ID);
            int THChangeID = CommonUsage.ConvertToInt(ID2);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            EmployeeTransportModel objModel = objAccountData.GetEmployeeTransportDetails(EmployeeID, SBranchID, THChangeID);
            objModel.EmployeeID = EmployeeID;
            return PartialView("_TransportAllocationEditPartialEmp", objModel);
        }
        [PermissionFilter]
        public ActionResult EditTransportDetails(string ID = null, string ID2 = null)
        {
            int StudentID = CommonUsage.ConvertToInt(ID);
            int THChangeID = CommonUsage.ConvertToInt(ID2);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            StudentTransportModel objModel = objAccountData.GetStudentTransportDetails(StudentID, SBranchID, THChangeID);
            objModel.StudentID = StudentID;
            return PartialView("_TransportAllocationEditPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetVehicleRouteStoppages(string ID = null, string ID2 = null)
        {
            int VehicleRouteID = CommonUsage.ConvertToInt(ID);
            int StopID = CommonUsage.ConvertToInt(ID2);
            StudentTransportModel objModel = new StudentTransportModel();
            objModel.StopDetails = new RouteStoppageModel();
            objModel.StopDetails.StopID = StopID;
            objModel.Stops = objAccountData.GetRouteVehicleStops(VehicleRouteID);
            return Json(objModel, JsonRequestBehavior.AllowGet);
            // return PartialView("_TransportStoppagesPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult SaveStudentTransport(StudentTransportHostelAllocationDelocationModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            //if (objData.Applicable == 0)
            //{
            //    objData.KeyID = 0;
            //    objData.VehicleRouteID = 0;
            //}
            if (objData.OpType == -1)
            {
                objData.StartDate = CommonUsage.GetCurrentDate();
                objData.EndDate = CommonUsage.GetCurrentDate();
            }
            objAccountData.InsertUpdateStudentTransport(objData);

            return Redirect("~/Account/StudentDetails/" + objData.UserID + "/" + 7);
        }
        [PermissionFilter]
        public ActionResult GetHostelFloors(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            IEnumerable<NameIDModel> model = objAccountData.GetHostelFloorsForAdmission(iID);

            return PartialView("_SelectOptionsPartial", model);
        }
        [PermissionFilter]
        public ActionResult GetHostelFloorRooms(string ID = null, string ID2 = null, string ID3 = null)
        {
            int HostelID = CommonUsage.ConvertToInt(ID);
            int FloorID = CommonUsage.ConvertToInt(ID2);
            int RoomID = CommonUsage.ConvertToInt(ID3);
            StudentHostelModel objModel = new StudentHostelModel();
            objModel.RoomID = RoomID;
            objModel.Rooms = objAccountData.GetHostelFloorsRoomsForAdmission(HostelID, FloorID);

            return PartialView("_HostelRoomsPartial", objModel);
        }
        [PermissionFilter]
        public async Task<ActionResult> GetFeeReciept(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            FeePaymentModel model = await objAccountData.GetFeePaymentReciptDetails(iID);
          //  var xmlData = CommonUsage.SerializeToXML<FeePaymentModel>(model);
         //   var html = CommonUsage.ConvertToHTML(xmlData, "https://pschoolstorage.blob.core.windows.net/report-formats/PrintFeeReceipt.xslt");

            return PartialView("_PrintFeeRecipt", model);
        }
        [PermissionFilter]
        public ActionResult SaveStudentHostel(StudentHostelModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objData.Applicable == 0)
            {
                objData.RoomID = 0;
            }
            objAccountData.UpdateStudentHostel(objData);

            return Redirect("~/Account/StudentDetails/" + objData.StudentID + "/" + 9);
        }
        [PermissionFilter]
        public ActionResult SaveStudentCustomeFee(StudentEditModel objData)
        {
            objAccountData.InsertUpdateStudentCustomFee(objData);

            return Redirect("~/Account/StudentDetails/" + objData.StudentID + "/" + 9);
        }

        #region Bus & House Report
        [PermissionFilter]
        public ActionResult StopWiseCollection(BusStudentListModel objModel)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objModel.SelectedDate.Year == 1)
            {
                objModel.SelectedDate = CommonUsage.GetCurrentDate();
            }
            objModel = objAccountData.GetStopWiseCollection(SBranchID, objModel.SelectedDate);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult BusWiseStudents(BusStudentListModel objModel)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetBusWiseStudents(SBranchID, objModel.ID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult BusStopWiseStudents(BusStudentListModel objModel)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.BusStopWiseStudents(SBranchID, objModel.ID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ClassWiseBusStudents(BusStudentListModel objModel)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetClassWiseBusStudents(SBranchID, 0, objModel.ID);
            return View(objModel);
        }
        
            [PermissionFilter]
        public ActionResult QuotaWiseStudents(QuotaClassStudentListModel objModel)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetQuotaWiseStudents(SBranchID, objModel.ID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult HouseWiseStudents(HouseClassStudentListModel objModel)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetHouseWiseStudents(SBranchID, objModel.ID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ClassWiseHouseStudents(HouseClassStudentListModel objModel)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetClassWiseHouseStudents(SBranchID, 0, objModel.ID);
            return View(objModel);
        }
        #endregion
        #region Student Fee Module
        [PermissionFilter]
        public ActionResult StudentsFeeOld(StudentFeeModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentFeeModel();
            }
            if (objModel.SelectedDate.Year == 1)
            {
                objModel.SelectedDate = CommonUsage.GetCurrentDate();//.AddMonths(-1);
            }
            objModel.Month = objModel.SelectedDate.Month;
            objModel.Year = objModel.SelectedDate.Year;
            objModel.Day = objModel.SelectedDate.Day;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetFeePayments(objModel);
            return View(objModel);
        }

        [PermissionFilter]
        public ActionResult StudentsFee(StudentFeeModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentFeeModel();

            }
            if (objModel.SelectedDate.Year == 1)
            {
                objModel.SelectedDate = CommonUsage.GetCurrentDate();//.AddMonths(-1);
            }
            objModel.Month = objModel.SelectedDate.Month;
            objModel.Year = objModel.SelectedDate.Year;
            objModel.Day = objModel.SelectedDate.Day;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel.SchoolID = PermissionManager.GetLoggedInUser().SchoolID;
            if (objModel.SBranchID == 2 && objModel.SchoolID == 1068)
            {
               
            }
            else
            {
                objModel = objAccountData.GetNewFeePayments(objModel);
            }

            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ParentsFeeDetailsNew(StudentFeeModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentFeeModel();

            }
            if (objModel.SelectedDate.Year == 1)
            {
                objModel.SelectedDate = CommonUsage.GetCurrentDate();//.AddMonths(-1);
            }
            objModel.Month = objModel.SelectedDate.Month;
            objModel.Year = objModel.SelectedDate.Year;
            objModel.Day = objModel.SelectedDate.Day;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetNewFeePaymentsParentWise(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult Parents(ParentPageModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAccountData.GetBranchParents(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult FeeDetails(FeePaymentModel objData)
        {
            objData.Day = 10;
            objData.Month = 5;
            objData.Year = 2017;
            objData.StudentID = 4;
            objData.SessionID = 1;
            FeePaymentModel objModel = objAccountData.GetFeePaymentDetails(objData);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GetFeePaymentDetailsOld(FeePaymentModel objModel)
        {
            objModel.QDate = CommonUsage.GetCurrentDate();
            objModel.Day = objModel.QDate.Day;
            if (objModel.Month == 0)
            {
                objModel.Month = objModel.QDate.Month;
                objModel.Year = objModel.QDate.Year;
            }
            objModel = objAccountData.GetFeePaymentDetails(objModel);
            objModel.FeeDate = CommonUsage.GetCurrentDate();
            return PartialView("_FeePaymentDetailsPartialNew", objModel);
        }
        [PermissionFilter]
        public async Task<ActionResult> GetFeePaymentDetails(FeePaymentModel objModel)
        {
            objModel.QDate = CommonUsage.GetCurrentDate();
            objModel.Day = objModel.QDate.Day;
            if (objModel.Month == 0)
            {
                objModel.Month = objModel.QDate.Month;
                objModel.Year = objModel.QDate.Year;
            }
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = await objAccountData.GetFeePaymentDetailsNew2(objModel);
            objModel.FeeDate = CommonUsage.GetCurrentDate();
            return PartialView("_FeePaymentDetailsPartialNew2", objModel);
        }
        [PermissionFilter]
        public async Task<ActionResult> GetFeePaymentDetailsForPrint(FeePaymentModel objModel)
        {
            objModel.QDate = objModel.SessionEndDate;
            objModel.Day = objModel.QDate.Day;
            if (objModel.Month == 0)
            {
                objModel.Month = objModel.QDate.Month;
                objModel.Year = objModel.QDate.Year;
            }
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = await objAccountData.GetFeePaymentDetailsForPrint(objModel);
            objModel.FeeDate = CommonUsage.GetCurrentDate();
            return PartialView("_FeeDetailsViewDataPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetFeeDiscountDetails(FeeDiscountRequestMaster objModel)
        {
            objModel.RequestDate = CommonUsage.GetCurrentDate();
            if (objModel.FeeMonth == 0)
            {
                objModel.FeeMonth = objModel.RequestDate.Month;
                objModel.FeeYear = objModel.RequestDate.Year;
            }
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetFeeDiscountRequestDetails(objModel);

            return PartialView("_FeeDiscountRequestForm", objModel);
        }
        [PermissionFilter]
        public JsonResult SubmitFeeDiscountRequest(FeeDiscountRequestMaster objModel)
        {
            objModel.RequestDate = CommonUsage.GetCurrentDate();
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAccountData.UpdateFeeDiscountRequest(objModel);
            return Json(1, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult SaveFeePayment(FeePaymentModel objModel)
        {

          
                objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
               
                objModel.UserID = PermissionManager.GetLoggedInUser().UserID;
                objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
                objModel.Day = objModel.QDate.Day;
                FeePaymentRowModel objData = objAccountData.SaveStudentFeePayments(objModel);


            return PartialView("_StudentFeeRowPartial", objData);


        }
        [PermissionFilter]
        public JsonResult SendFeeCollectionSMS(string ID)
        {
            if (!String.IsNullOrEmpty(ID))
            {
                int PaymentID = CommonUsage.ConvertToInt(ID);
                int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
                string ContentID = "0";
                NotificationSMSModel objModel = objAccountData.GetFeePaymentSMSDetails(PaymentID);

                if (objModel.Number != null && objModel.Number != "" && !String.IsNullOrEmpty(objModel.SMSText) && String.IsNullOrEmpty(objModel.FCMToken))
                {
                    SMSSender objSender = new SMSSender();

                    objSender.SendSMSAsync(objModel.SMSText, objModel.Number, SBranchID, 4, 1, PaymentID, objModel.ContentID);
                }
                if (!String.IsNullOrEmpty(objModel.FCMToken))
                {
                    NotificationModel objNModel = new NotificationModel();
                    CommonData objCommonData = new CommonData();
                    objNModel.RecieverID = objModel.RecieverID;
                    objNModel.SBranchID = SBranchID;
                    objNModel.NotificationType = 7;
                    objNModel.RecieverType = objModel.RecieverType;
                    objNModel.NotificationText = objModel.SMSText.Replace("%0a", " ");
                    objNModel.NotificationDateTime = CommonUsage.GetCurrentDate();
                    objNModel.Recievers = new List<NotificationRecieverModel>();
                    string NotificationServerKey = PermissionManager.GetLoggedInUser().NotificationServerKey;
                    string[] Recievers = objModel.FCMToken.Trim().Split("#".ToCharArray());
                    NotificationDataWraperModel routeData = new NotificationDataWraperModel();
                    routeData.type = (int)CommonUsage.NotificationTypes.Fee;
                    routeData.primaryid = PaymentID;
                    CommonUsage.SendNotificationFCM(Recievers, objNModel.NotificationText, "7", NotificationServerKey);
                    objCommonData.InsertNotification(objNModel);
                }

                objAdminData.UpdateSenderSMSSentStatus(4, PaymentID, "", 1);
            }
            return Json(1, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult UpdateFeePayment(FeePaymentModel objModel)
        {
            objModel.PaymentType = 1;
            objModel.PayeeType = 1;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objModel.PaymentAmount >= objModel.ApplicableFee)
            {
                objModel.PaymentStatus = 1;
            }
            else if (objModel.PaymentAmount > 0)
            {
                objModel.PaymentStatus = 2;
            }
            else
            {
                objModel.PaymentStatus = 3;
            }
            objAccountData.UpdateStudentFeePayments(objModel);
            StudentFeeModel model = new StudentFeeModel();
            model.SBranchID = objModel.SBranchID;
            model.SelectedDate = new DateTime(objModel.Year, objModel.Month, 1);
            model.ClassID = objModel.ClassID;
            model.SectionID = objModel.SectionID;
            return RedirectToAction("StudentsFee", "Account", model);
        }
        #endregion
        #region Employee Management
        [PermissionFilter]
        public ActionResult EmployeeList(EmployeeListPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new EmployeeListPageModel();

            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetEmployeesReport(objModel.EmployeeType, SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult Employees(EmployeeListPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new EmployeeListPageModel();

            }
            //if (objModel.EmployeeType == 0)
            //{
            //    objModel.EmployeeType = 1;
            //}
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetEmployees(objModel.EmployeeType, SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult EmployeeIDCards(EmployeeListPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new EmployeeListPageModel();
            }

            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetEmployees(objModel.EmployeeType, SBranchID);
            objModel.SBranchDetails = objAccountData.GetBranchPaymentProfile(SBranchID);
            if (string.IsNullOrWhiteSpace(objModel.CurrentSessionName))
            {
                objModel.CurrentSessionName = objAdminData.GetSessions(-1, SBranchID)
                    .OrderByDescending(x => x.SessionStatus)
                    .ThenByDescending(x => x.SessionStartDate)
                    .Select(x => x.SessionName)
                    .FirstOrDefault();
            }
            ViewBag.CurrentSessionName = objModel.CurrentSessionName;
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult EmployeeIDCardsV(EmployeeListPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new EmployeeListPageModel();
            }

            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetEmployees(objModel.EmployeeType, SBranchID);
            objModel.SBranchDetails = objAccountData.GetBranchPaymentProfile(SBranchID);
            if (string.IsNullOrWhiteSpace(objModel.CurrentSessionName))
            {
                objModel.CurrentSessionName = objAdminData.GetSessions(-1, SBranchID)
                    .OrderByDescending(x => x.SessionStatus)
                    .ThenByDescending(x => x.SessionStartDate)
                    .Select(x => x.SessionName)
                    .FirstOrDefault();
            }
            ViewBag.CurrentSessionName = objModel.CurrentSessionName;
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult EmployeeDetails(string id = null, string id2 = null)
        {
            int EmployeeID = CommonUsage.ConvertToInt(id);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            EmployeeEditModel objModel = objAccountData.GetEmployeeDetails(EmployeeID, SBranchID);
            int cTab = CommonUsage.ConvertToInt(id2);
            objModel.CurrentTab = cTab == 0 ? 1 : cTab;
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult SaveEmployeePersonal(EmployeeModel objData)
        {
            string PassWord = objData.EmployeeName.Replace(" ", "").Replace(".", "").Substring(0, 4).ToUpper();
            string DateBirth = objData.DOB.Day.ToString("00") + objData.DOB.Month.ToString("00");
            PassWord = PassWord + DateBirth;
            objData.Password = CommonUsage.EncryptPassword(PassWord + CommonUsage.FixedPrimaryEncryptionSalt);
            objData.OperationDate = CommonUsage.GetCurrentDate();
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            string OldImageFile = "";
            if (objData.EmployeeImageUploader != null)
            {
                OldImageFile = objData.Photo;
                objData.Photo = objData.EmployeeImageUploader.FileName.Replace(" ", "-");
            }
            objData.EmployeeID = objAccountData.InsertUpdateEmployeeBasic(objData);
            if (objData.EmployeeImageUploader != null)
            {
                try
                {
                    System.IO.File.Delete(
                        Server.MapPath(CommonUsage.EmployeeImageBasePath + objData.EmployeeID + "_" + OldImageFile));

                }
                catch (Exception ex)
                {


                }
                var path = Path.Combine(Server.MapPath(CommonUsage.EmployeeImageBasePath),
                   objData.EmployeeID + "_" + objData.Photo);
                objData.EmployeeImageUploader.SaveAs(path);

            }
            return Redirect("~/Account/EmployeeDetails/" + objData.EmployeeID + "/" + 2);
        }
        [PermissionFilter]
        public ActionResult SaveEmployeeAccount(EmployeeModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAccountData.InsertUpdateEmployeeAccount(objData);
            return Redirect("~/Account/EmployeeDetails/" + objData.EmployeeID + "/" + 3);
        }
        [PermissionFilter]
        public ActionResult SaveEmployeeLeft(EmployeeModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAccountData.InsertUpdateEmployeeLeft(objData);
            return Redirect("~/Account/EmployeeDetails/" + objData.EmployeeID + "/" + 11);
        }
        [PermissionFilter]
        public ActionResult SaveEmployeeAddress(EmployeeModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAccountData.InsertUpdateEmployeeAddress(objData);
            return Redirect("~/Account/EmployeeDetails/" + objData.EmployeeID + "/" + 4);
        }
        [PermissionFilter]
        public ActionResult SaveEmployeeSalary(EmployeeEditModel objData)
        {
            objAccountData.InsertUpdateEmployeeSalary(objData);
            return Redirect("~/Account/EmployeeDetails/" + objData.EmployeeID + "/" + 5);
        }
        [PermissionFilter]
        public ActionResult SaveEmployeeEducation(EmployeeEducationModel objData)
        {
            objAccountData.InsertUpdateEmployeeEducation(objData);
            return Redirect("~/Account/EmployeeDetails/" + objData.EmployeeID + "/" + 5);
        }
        [PermissionFilter]
        public ActionResult DeleteEmployeeEducation(string ID = null, string ID2 = null)
        {
            EmployeeEducationModel objData = new EmployeeEducationModel();
            objData.EmpEduID = CommonUsage.ConvertToInt(ID);
            objData.EmployeeID = CommonUsage.ConvertToInt(ID2);
            objData.OpType = -1;
            objData.StartDate = CommonUsage.GetCurrentDate();
            objData.EndDate = CommonUsage.GetCurrentDate();
            objAccountData.InsertUpdateEmployeeEducation(objData);
            return Redirect("~/Account/EmployeeDetails/" + objData.EmployeeID + "/" + 5);
        }
        [PermissionFilter]
        public ActionResult SaveEmployeeExperience(EmployeeExperienceModel objData)
        {
            objAccountData.InsertUpdateEmployeeExperience(objData);
            return Redirect("~/Account/EmployeeDetails/" + objData.EmployeeID + "/" + 6);
        }
        [PermissionFilter]
        public ActionResult DeleteEmployeeExperience(string ID = null, string ID2 = null)
        {
            EmployeeExperienceModel objData = new EmployeeExperienceModel();
            objData.EmpExpID = CommonUsage.ConvertToInt(ID);
            objData.EmployeeID = CommonUsage.ConvertToInt(ID2);
            objData.OpType = -1;
            objData.FromDate = CommonUsage.GetCurrentDate();
            objData.EndDate = CommonUsage.GetCurrentDate();
            objAccountData.InsertUpdateEmployeeExperience(objData);
            return Redirect("~/Account/EmployeeDetails/" + objData.EmployeeID + "/" + 6);
        }
        [PermissionFilter]
        public ActionResult SaveEmployeeDocuments(EmployeeModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            string OldExperienceCertificate = "";
            string OldRelievingCertificate = "";
            string OldPFDeclaration = "";
            string OldPANCardCopy = "";
            string OldBankAccountProof = "";
            string OldMedicalCertificate = "";
            string OldAppointmentLetter = "";
            string OldAddressProof = "";
            string OldBirthCertificate = "";
            string OldCategoryCertificate = "";
            string OldEmployeeSignature = "";

            //Birth
            if (objData.BirthCertificateUploader != null)
            {
                OldBirthCertificate = objData.BirthCertificate;
                objData.BirthCertificate = objData.BirthCertificateUploader.FileName.Replace(" ", "-");
                System.IO.File.Delete(
                        Server.MapPath(CommonUsage.EmployeeDocumentsBasePath + objData.EmployeeID + "_Birth_" + OldBirthCertificate));

                var path = Path.Combine(Server.MapPath(CommonUsage.EmployeeDocumentsBasePath),
                 objData.EmployeeID + "_Birth_" + objData.BirthCertificate);
                objData.BirthCertificateUploader.SaveAs(path);
            }
            //Address
            if (objData.AddressCertificateUploader != null)
            {
                OldAddressProof = objData.AddressProof;
                objData.AddressProof = objData.AddressCertificateUploader.FileName.Replace(" ", "-");
                System.IO.File.Delete(
                        Server.MapPath(CommonUsage.EmployeeDocumentsBasePath + objData.EmployeeID + "_Address_" + OldAddressProof));

                var path = Path.Combine(Server.MapPath(CommonUsage.EmployeeDocumentsBasePath),
                 objData.EmployeeID + "_Address_" + objData.AddressProof);
                objData.AddressCertificateUploader.SaveAs(path);
            }
            //Category
            if (objData.CategoryCertificateUploader != null)
            {
                OldCategoryCertificate = objData.CategoryCertificate;
                objData.CategoryCertificate = objData.CategoryCertificateUploader.FileName.Replace(" ", "-");
                System.IO.File.Delete(
                        Server.MapPath(CommonUsage.EmployeeDocumentsBasePath + objData.EmployeeID + "_Category_" + OldCategoryCertificate));

                var path = Path.Combine(Server.MapPath(CommonUsage.EmployeeDocumentsBasePath),
                 objData.EmployeeID + "_Category_" + objData.CategoryCertificate);
                objData.CategoryCertificateUploader.SaveAs(path);
            }
            //Medicle
            if (objData.MedicleCertificateUploader != null)
            {
                OldMedicalCertificate = objData.MedicalCertificate;
                objData.MedicalCertificate = objData.MedicleCertificateUploader.FileName.Replace(" ", "-");
                System.IO.File.Delete(
                        Server.MapPath(CommonUsage.EmployeeDocumentsBasePath + objData.EmployeeID + "_Medicle_" + OldMedicalCertificate));

                var path = Path.Combine(Server.MapPath(CommonUsage.EmployeeDocumentsBasePath),
                 objData.EmployeeID + "_Medicle_" + objData.MedicalCertificate);
                objData.MedicleCertificateUploader.SaveAs(path);
            }
            //Experience
            if (objData.ExperienceCertificateUploader != null)
            {
                OldExperienceCertificate = objData.ExperienceCertificate;
                objData.ExperienceCertificate = objData.ExperienceCertificateUploader.FileName.Replace(" ", "-");
                System.IO.File.Delete(
                        Server.MapPath(CommonUsage.EmployeeDocumentsBasePath + objData.EmployeeID + "_Experience_" + OldExperienceCertificate));

                var path = Path.Combine(Server.MapPath(CommonUsage.EmployeeDocumentsBasePath),
                 objData.EmployeeID + "_Experience_" + objData.ExperienceCertificate);
                objData.ExperienceCertificateUploader.SaveAs(path);
            }
            //Relieving
            if (objData.RelievingCertificateUploader != null)
            {
                OldRelievingCertificate = objData.RelievingCertificate;
                objData.RelievingCertificate = objData.RelievingCertificateUploader.FileName.Replace(" ", "-");
                System.IO.File.Delete(
                        Server.MapPath(CommonUsage.EmployeeDocumentsBasePath + objData.EmployeeID + "_Relieving_" + OldRelievingCertificate));

                var path = Path.Combine(Server.MapPath(CommonUsage.EmployeeDocumentsBasePath),
                 objData.EmployeeID + "_Relieving_" + objData.RelievingCertificate);
                objData.RelievingCertificateUploader.SaveAs(path);
            }
            //PF Declaration
            if (objData.PFDeclarationUploader != null)
            {
                OldPFDeclaration = objData.PFDeclaration;
                objData.PFDeclaration = objData.PFDeclarationUploader.FileName.Replace(" ", "-");
                System.IO.File.Delete(
                        Server.MapPath(CommonUsage.EmployeeDocumentsBasePath + objData.EmployeeID + "_PF_" + OldPFDeclaration));

                var path = Path.Combine(Server.MapPath(CommonUsage.EmployeeDocumentsBasePath),
                 objData.EmployeeID + "_PF_" + objData.PFDeclaration);
                objData.PFDeclarationUploader.SaveAs(path);
            }
            //Appointment
            if (objData.AppointmentCertificateUploader != null)
            {
                OldAppointmentLetter = objData.AppointmentLetter;
                objData.AppointmentLetter = objData.AppointmentCertificateUploader.FileName.Replace(" ", "-");
                System.IO.File.Delete(
                        Server.MapPath(CommonUsage.EmployeeDocumentsBasePath + objData.EmployeeID + "_Appointment_" + OldAppointmentLetter));

                var path = Path.Combine(Server.MapPath(CommonUsage.EmployeeDocumentsBasePath),
                 objData.EmployeeID + "_Appointment_" + objData.AppointmentLetter);
                objData.AppointmentCertificateUploader.SaveAs(path);
            }
            //PAN
            if (objData.PANCardUploader != null)
            {
                OldPANCardCopy = objData.PANCardCopy;
                objData.PANCardCopy = objData.PANCardUploader.FileName.Replace(" ", "-");
                System.IO.File.Delete(
                        Server.MapPath(CommonUsage.EmployeeDocumentsBasePath + objData.EmployeeID + "_PAN_" + OldPANCardCopy));

                var path = Path.Combine(Server.MapPath(CommonUsage.EmployeeDocumentsBasePath),
                 objData.EmployeeID + "_PAN_" + objData.PANCardCopy);
                objData.PANCardUploader.SaveAs(path);
            }
            //Bank
            if (objData.BankAccountProofUploader != null)
            {
                OldBankAccountProof = objData.BankAccountProof;
                objData.BankAccountProof = objData.BankAccountProofUploader.FileName.Replace(" ", "-");
                System.IO.File.Delete(
                        Server.MapPath(CommonUsage.EmployeeDocumentsBasePath + objData.EmployeeID + "_Bank_" + OldBankAccountProof));

                var path = Path.Combine(Server.MapPath(CommonUsage.EmployeeDocumentsBasePath),
                 objData.EmployeeID + "_Bank_" + objData.BankAccountProof);
                objData.BankAccountProofUploader.SaveAs(path);
            }
            if (objData.EmpSignatureUploader != null)
            {
                OldBankAccountProof = objData.EmpSignature;
                objData.EmpSignature = objData.EmpSignatureUploader.FileName.Replace(" ", "-");
                System.IO.File.Delete(
                        Server.MapPath(CommonUsage.EmployeeDocumentsBasePath + objData.EmployeeID + "_Sign_" + OldEmployeeSignature));

                var path = Path.Combine(Server.MapPath(CommonUsage.EmployeeDocumentsBasePath),
                 objData.EmployeeID + "_Sign_" + objData.EmpSignature);
                objData.EmpSignatureUploader.SaveAs(path);
            }
            objAccountData.UpdateEmployeeDocuments(objData);

            return Redirect("~/Account/EmployeeDetails/" + objData.EmployeeID + "/" + 8);
        }
        [PermissionFilter]
        //public ActionResult SaveEmployeeTransport(EmployeeTransportModel objData)
        public ActionResult SaveEmployeeTransport(EmployeeTransportHostelAllocationDelocationModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            /*
            if (objData.Applicable == 0)
            {
                objData.StopID = 0;
                objData.VehicleRouteID = 0;
            }
             */
            if (objData.OpType == -1)
            {
                objData.StartDate = CommonUsage.GetCurrentDate();
                objData.EndDate = CommonUsage.GetCurrentDate();
            }
            // objAccountData.UpdateEmployeeTransport(objData);
            objAccountData.InsertUpdateEmployeeTransport(objData);

            //return Redirect("~/Account/EmployeeDetails/" + objData.EmployeeID + "/" + 9);
            return Redirect("~/Account/EmployeeDetails/" + objData.UserID + "/" + 9);
        }
        [PermissionFilter]
        public ActionResult SaveEmployeeLeaves(EmployeeEditModel objData)
        {
            objAccountData.InsertUpdateEmployeeLeaves(objData);
            int curTab = 9;
            if (objData.EmployeeDetails.EmployeeType == 1)
            {
                curTab = 10;
            }
            return Redirect("~/Account/EmployeeDetails/" + objData.EmployeeID + "/" + curTab);
        }
        [PermissionFilter]
        public ActionResult GetEducationLevelGroups(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            IEnumerable<NameIDModel> model = objAccountData.GetEducationLevelGroups(iID);

            return PartialView("_SelectOptionsPartial", model);
        }
        [PermissionFilter]
        public ActionResult DeleteEmployee(EmployeeListPageModel objModel)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAccountData.DeleteEmployee(objModel.EmployeeID);
            return RedirectToAction("Employees", "Account", objModel);
        }
        [PermissionFilter]
        public ActionResult GetGroupSubjectList(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            IEnumerable<NameIDModel> model = objAccountData.GetGroupSubjectList(iID);

            return PartialView("_SelectOptionsPartial", model);
        }
        [PermissionFilter]
        public ActionResult SaveTeacherSubject(Teacher_SubjectModel objData)
        {
            objAccountData.InsertUpdateTeacherSubject(objData);
            return Redirect("~/Account/EmployeeDetails/" + objData.EmployeeID + "/" + 10);
        }
        [PermissionFilter]
        public ActionResult DeleteTeacherSubject(string ID = null, string ID2 = null)
        {
            Teacher_SubjectModel objData = new Teacher_SubjectModel();
            objData.TSID = CommonUsage.ConvertToInt(ID);
            objData.EmployeeID = CommonUsage.ConvertToInt(ID2);
            objData.OpType = -1;

            objAccountData.InsertUpdateTeacherSubject(objData);
            return Redirect("~/Account/EmployeeDetails/" + objData.EmployeeID + "/" + 10);
        }
        #endregion
        #region Employee Attandance
        [PermissionFilter]
        public ActionResult EmployeeAttandance(EmployeeAttandancePageModel objModel)
        {
            if (objModel.SelectedDate.Year == 1)
            {
                objModel.SelectedDate = CommonUsage.GetCurrentDate();
                objModel.EmployeeType = 2;
            }
            objModel.Month = objModel.SelectedDate.Month;
            objModel.Year = objModel.SelectedDate.Year;

            objModel.Day = objModel.SelectedDate.Day.ToString();

            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetEmployeesAttandance(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult EmployeeAttandanceReview(EmployeeAttandanceReviewPageModel objModel)
        {

            if (objModel.AttandanceDate.Year == 1)
            {
                objModel.AttandanceDate = CommonUsage.GetCurrentDate();
                objModel.EmployeeType = 1;
            }
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetEmployeesAttandanceReview(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public JsonResult GetEmployeeAttandanceReviewDetails(EmployeeAttandanceReviewPageModel objData)
        {

            List<AppAttandanceModel> objModel = objAccountData.GetEmployeeAttandanceDetails(objData.EmployeeType, objData.AttandanceDate).ToList();

            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public JsonResult UpdateEmployeeAttandanceReview(AppAttandanceModel objData)
        {

            List<AppAttandanceModel> objModel = objAccountData.UpdateEmployeeAttandanceReview(objData).ToList();

            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult UpdateEmployeeAttandance(EmployeeAttandancePageModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAccountData.UpdateEmployeesAttandance(objModel);
            objModel.EmployeeAttandances = null;
            int day = CommonUsage.ConvertToInt(objModel.Day.Replace("D", ""));
            objModel.SelectedDate = new DateTime(objModel.Year, objModel.Month, day);
            return RedirectToAction("EmployeeAttandance", "Account", objModel);
        }

        #endregion

        #region Employee Advance Payments
        [PermissionFilter]
        public ActionResult AdvancePayments()
        {
            EmployeeAdvancePaymentListModel objModel = new EmployeeAdvancePaymentListModel();

            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel.Status = -1;
            objModel = objAccountData.GetAdvancePaymentEmployeeList(objModel.Status, objModel.SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GetAdvancePaymentDeductions(string ID = null)
        {
            int AdPayID = CommonUsage.ConvertToInt(ID);
            EmployeeAdvancePaymentModel objModel = objAccountData.GetAdvancePaymentDeductions(AdPayID);

            return PartialView("_AdvancePaymentDeductionsPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetEditAdvancePaymentForm(string ID = null)
        {
            int AdPayID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            EmployeeAdvancePaymentEditModel objModel = new EmployeeAdvancePaymentEditModel();

            objModel = objAccountData.GetAdvancePaymentDetails(AdPayID, SBranchID);

            return PartialView("_AdvancePaymentForm", objModel);
        }
        [PermissionFilter]
        public ActionResult SaveAdvancePayment(EmployeeAdvancePaymentModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            objAccountData.AddAdvancePaymentEmployee(objData);

            return RedirectToAction("AdvancePayments", "Account");
        }
        [PermissionFilter]
        public ActionResult AddAdvancePaymentDeductions(EmployeeAdvancePaymentDeductionModel objData)
        {
            EmployeeAdvancePaymentModel objModel = objAccountData.AddAdvancePaymentDeductions(objData);

            return PartialView("_AdvancePaymentDeductionsPartial", objModel);
        }
        #endregion
        #region Employee Leaves
        [PermissionFilter]
        public ActionResult EmployeeLeaves()
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            IEnumerable<EmployeeLeaveModel> objModel = objAccountData.GetEmployeeLeaves(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GetEditEmployeeLeaveForm(string ID = null)
        {
            int LeaveID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int Month = CommonUsage.GetCurrentDate().Month;
            int Year = CommonUsage.GetCurrentDate().Year;

            EmployeeLeaveDetailModel objModel = new EmployeeLeaveDetailModel();

            objModel = objAccountData.GetEmployeeLeavesEditDetailsNew(LeaveID, SBranchID, Month, Year, 0, 0, 0);
            if (objModel.Leave == null)
            {
                objModel.Leave = new LeaveModel();
            }
            objModel.Leave.StartDate = CommonUsage.GetCurrentDate();
            objModel.Leave.EndDate = CommonUsage.GetCurrentDate();
            ViewBag.EmployeeType = 1;
            if (objModel.Employees.Count > 0)
            {
                ViewBag.EmployeeType = objModel.Employees[0].EmployeeType;
            }
            return PartialView("_EmployeeLeaveForm", objModel);
        }
        [PermissionFilter]
        public ActionResult GetEmployeeLeavesDetails(LeaveModel objData)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int Month = objData.Month;
            int Year = objData.Year;

            List<EmployeeLeaveSummery> oModel = objAccountData.GetEmployeeLeaveBalanceDetails(objData.LeaveID, SBranchID, Month, Year, 0, objData.ApplicantID, objData.EmployeeType);
            return Json(oModel, JsonRequestBehavior.AllowGet);
        }

        [PermissionFilter]
        public ActionResult GetEmployeeLeavesDetailDifferentMonths(LeaveModel objData)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int Month = objData.Month;
            int Year = objData.Year;

            List<EmployeeLeaveSummery> oModel = objAccountData.GetEmployeeLeaveBalanceDetailsDifferentMonths(objData.LeaveID, SBranchID, objData.StartDate, objData.EndDate, 0, objData.ApplicantID, objData.EmployeeType);
            return Json(oModel, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult SaveEmployeeLeave(EmployeeLeaveModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.CreatedDate = CommonUsage.GetCurrentDate();
            objData.ApplicantType = 0;
            objData.IsApproved = 2;
            objAccountData.UpdateEmployeeLeaveNew(objData);

            return RedirectToAction("EmployeeLeaves", "Account");
        }
        #endregion
        #region Employee Salary
        [PermissionFilter]
        public ActionResult EmployeeSalary(EmployeeSalaryListPageModel objModel)
        {
            if (TempData["EmployeeSalaryRedirectDate"] != null)
            {
                objModel = (EmployeeSalaryListPageModel)TempData["EmployeeSalaryRedirectDate"];

            }
            else
            {
                if (objModel.EmployeeTypeID == 0)
                {
                    objModel.EmployeeTypeID = 1;
                    objModel.Month = CommonUsage.GetCurrentDate().AddMonths(-1).Month;
                    objModel.Year = CommonUsage.GetCurrentDate().AddMonths(-1).Year;
                }
                else
                {
                    objModel.Month = objModel.StartDate.Month;
                    objModel.Year = objModel.StartDate.Year;
                }
            }
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetEmployeesForSalaries(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult EmployeeSalaryDetail(EmployeeSalaryDetailsPageModel objData)
        {
            objData = objAccountData.GetEmployeeSalaryDetailsNew(objData);
            return PartialView("_EmployeeSalaryDetail", objData);
        }
        [PermissionFilter]
        public ActionResult GetSalarySlip(string ID = null)
        {
            SalarySlipModel objData = new SalarySlipModel();
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objData.SPMID = CommonUsage.ConvertToInt(ID);
            objData = objAccountData.GetEmployeeSalarySlipData(objData);
            return PartialView("_EmployeeSalarySlip", objData);
        }
        [PermissionFilter]
        public ActionResult SaveEmployeeSalaryPayment(SalaryProcessingModel objData)
        {
            objData.CreatedDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int res = objAccountData.UpdateSalaryProcessing(objData);
            // return PartialView("_EmployeeSalaryDetail", objData);
            EmployeeSalaryListPageModel objModel = new EmployeeSalaryListPageModel();
            objModel.Month = objData.SMonth;
            objModel.Year = objData.SYear;
            objModel.EmployeeTypeID = objData.EmployeeType;
            TempData["EmployeeSalaryRedirectDate"] = objModel;
            TempData["SMPID"] = res;
            return RedirectToAction("EmployeeSalary");
        }
        #endregion
        #region Expence Manegement
        [PermissionFilter]
        public ActionResult ExpanceManagement(ExpenceManagementModel objModel)
        {
            if (objModel == null)
            {
                objModel = new ExpenceManagementModel();

            }
            if (objModel.StartDate.Year == 1)
            {
                objModel.StartDate = CommonUsage.GetCurrentDate();
                objModel.Month = CommonUsage.GetCurrentDate().Month;
                objModel.Year = CommonUsage.GetCurrentDate().Year;
            }
            else
            {
                objModel.Month = objModel.StartDate.Month;
                objModel.Year = objModel.StartDate.Year;
            }
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetExpences(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ExpanceDetails(string ID = null, string ID2 = null)
        {
            int ExpenceID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            ExpenceModel objModel = objAccountData.GetExpenceDetails(ExpenceID, SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult DeleteExpence(string ID = null)
        {
            int ExpenceID = CommonUsage.ConvertToInt(ID);
            objAccountData.DeleteExpence(ExpenceID);
            string[] Files = Directory.GetFiles(Server.MapPath(CommonUsage.ExpenceBillsBasePath), ExpenceID + "_*");
            foreach (string file in Files)
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch (Exception ex) { }
            }
            return RedirectToAction("ExpanceManagement", "Account");
        }
        [PermissionFilter]
        public ActionResult SaveExpence(ExpenceModel objData)
        {
            objData.EnteredDate = CommonUsage.GetCurrentDate();
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objData.Year = objData.PaymentDate.Year;
            objData.Month = objData.PaymentDate.Month;
            objData.PaymentStatus = 1;
            objData.PaymentType = 1;
            objData.ExpenceID = objAccountData.InsertUpdateExpence(objData);

            foreach (ExpenceBillModel e in objData.ExpenceBills)
            {
                if (e.OpType == -1)
                {
                    try
                    {
                        System.IO.File.Delete(
                            Server.MapPath(CommonUsage.ExpenceBillsBasePath + objData.ExpenceID + "_" + e.OldImageName));

                    }
                    catch (Exception ex)
                    {


                    }
                }
                else if (e.ImageFile != null)
                {
                    try
                    {
                        System.IO.File.Delete(
                            Server.MapPath(CommonUsage.ExpenceBillsBasePath + objData.ExpenceID + "_" + e.OldImageName));

                    }
                    catch (Exception ex)
                    {


                    }
                    var path = Path.Combine(Server.MapPath(CommonUsage.ExpenceBillsBasePath),
                       objData.ExpenceID + "_" + e.ImageName);
                    e.ImageFile.SaveAs(path);
                }
            }
            ExpenceManagementModel objModel = new ExpenceManagementModel();
            objModel.Month = objData.PaymentDate.Month;
            objModel.Year = objData.PaymentDate.Year;
            objModel.StartDate = objData.PaymentDate;
            objModel.ExpenceType = objData.ExpenceTypeID;
            return RedirectToAction("ExpanceManagement", "Account", objModel);
        }
        #endregion
        #region Student Attandance
        public ActionResult GetSectionOnClass(string id = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            IEnumerable<SectionModel> objModel = objAdminData.GetSectionsOnClass(ID);

            return PartialView("_SectionOptionsPartial", objModel);
        }
        public ActionResult GetStudentOnSessionSection(string id = null, string id2 = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            int ID2 = CommonUsage.ConvertToInt(id2);
            IEnumerable<NameIDModel> objModel = objAccountData.GetStudentListOnSessionSection(ID, ID2);

            return PartialView("_SectionOptionsPartial", objModel);
        }
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
            objModel.TeacherID = -1;
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
            return RedirectToAction("StudentAttandance", "Account", objModel);
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

        #region Student Leaves
        [PermissionFilter]
        public ActionResult StudentLeaves(StudentLeavePageModel objData)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objData = objAccountData.GetStudentLeaves(SBranchID, objData.ClassID, objData.SectionID);
            return View(objData);
        }
        [PermissionFilter]
        public ActionResult GetEditStudentLeaveForm(string ID = null)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            StudentLeaveAddModel objModel;
            objModel = objAccountData.GetStudentLeavesEditDetails(SBranchID);
            objModel.StartDate = CommonUsage.GetCurrentDate();
            objModel.EndDate = CommonUsage.GetCurrentDate();

            return PartialView("_StudentLeaveForm", objModel);
        }

        [PermissionFilter]
        public ActionResult SaveStudentLeave(StudentLeaveModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.CreatedDate = CommonUsage.GetCurrentDate();
            objData.ApplicantType = 1;
            objData.IsApproved = 1;
            objAccountData.AddStudentLeave(objData);

            return RedirectToAction("StudentLeaves", "Account");
        }
        [PermissionFilter]
        public ActionResult GetSectionSelect2Students(string ID = null)
        {
            int SectionID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            List<NameIDModel> data = objAccountData.GetSectionStudentList(SectionID, SBranchID).ToList();

            return PartialView("_SelectOptionsPartial", data);
            //return Json(data, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult GetSectionSelect2StudentsJSON(string ID = null)
        {
            int SectionID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            List<NameIDModel> data = objAccountData.GetSectionStudentList(SectionID, SBranchID).ToList();

            //return PartialView("_SelectOptionsPartial", data);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region OtherCerificates
        [PermissionFilter]
        public ActionResult GetStudentOCDetails(string ID = null, string ID2 = null)
        {
            int StudentID = CommonUsage.ConvertToInt(ID);
            int SessionID = CommonUsage.ConvertToInt(ID2);
            //int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            var data = objAccountData.GetStudentSOCDetails(StudentID, SessionID);

            return PartialView("_EditSOCDetailsPartial", data);
            //return Json(data, JsonRequestBehavior.AllowGet);
        }

        [PermissionFilter]
        public ActionResult UpdateSOCDetails(SOCertificateDetails objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objModel.CertID < 1)
            {
                objModel.CreatedBy = PermissionManager.GetLoggedInUser().UserID;
            }
            else
            {

                objModel.ModifiedBy = PermissionManager.GetLoggedInUser().UserID;
            }
            int Res = objAccountData.InsertUpdateOCertificates(objModel);
            return Redirect("Certificates/" + objModel.StudentID + "/" + objModel.SessionID);
        }
        [PermissionFilter]
        public ActionResult Certificates(string ID = null, string ID2 = null)
        {
            int StudentID = CommonUsage.ConvertToInt(ID);
            int SessionID = CommonUsage.ConvertToInt(ID2);
            var data = objAccountData.GetStudentSOCPrintDetails(StudentID, SessionID);

            return View(data);
            //return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region TC
        [PermissionFilter]
        public ActionResult StudentTcReport(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetTCStudentList(SBranchID, objModel.SessionID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult StudentList(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            // int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objModel = objAccountData.GetStudents(objModel.ClassID, objModel.SectionID, SBranchID, objModel.SessionID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GetStudentSLCDetails(string ID = null, string ID2 = null)
        {
            int StudentID = CommonUsage.ConvertToInt(ID);
            int SessionID = CommonUsage.ConvertToInt(ID2);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            var data = objAccountData.GetStudentSLCDetails(StudentID, SessionID);

            return PartialView("_EditSLCDetailsPartial", data);
            //return Json(data, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult ViewStudentSLC(string ID = null, string ID2 = null)
        {
            int StudentID = CommonUsage.ConvertToInt(ID);
            int SessionID = CommonUsage.ConvertToInt(ID2);
            var data = objAccountData.GetStudentSLCPrintDetails(StudentID, SessionID);

            return View(data);
            //return Json(data, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult UpdateSLCDetails(SLCCertificateDetails objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel.CreatedBy = PermissionManager.GetLoggedInUser().UserID;
            objModel.CreatedDate = CommonUsage.GetCurrentDate();
            int Res = objAccountData.InsertUpdateSLC(objModel);
            return Redirect("ViewStudentSLC/" + objModel.StudentID + "/" + objModel.SessionID);
        }
        [PermissionFilter]
        public ActionResult SearchStudentsForTC(StudentSearchListModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentSearchListModel();
            }
            if (objModel.SearchText != null)
            {
                objModel.SearchText = objModel.SearchText.Trim();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objModel.TCType == 0)
            {
                objModel = objAccountData.SearchStudentsForTC(objModel.SearchText, SBranchID, objModel.SessionID);
                objAccountData.PopulateTCGeneratedStatus(objModel, SBranchID);
                return View(objModel);
            }
            else
            {
                return RedirectToAction("New", objModel);
            }

        }
        [PermissionFilter]
        public ActionResult New(StudentSearchListModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentSearchListModel();
            }
            if (objModel.SearchText != null)
            {
                objModel.SearchText = objModel.SearchText.Trim();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objModel.TCType == 1)
            {
                objModel = objAccountData.SearchStudents(objModel.SearchText, SBranchID, objModel.SessionID);
                return RedirectToAction("GenerateTC", objModel);
            }
            else
            {
                return RedirectToAction("SearchStudentsForTC", objModel);
            }
        }
        [PermissionFilter]
        public ActionResult GenerateTC(TCDetailsModel objModel)
        {
            // objModel.SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            var loggedInUser = PermissionManager.GetLoggedInUser();
            objModel.SBranchID = loggedInUser.SBranchID;
            if (objModel.TCDetails == null)
            {
                objAccountData.GetTCDetails(objModel);
                return View(objModel);
            }

            

            else
            {
                objModel.TCDetails.StudentID = objModel.TCDetails.StudentID == 0 ? objModel.StudentID : objModel.TCDetails.StudentID;
                objModel.TCDetails.SessionID = objModel.TCDetails.SessionID == 0 ? objModel.SessionID : objModel.TCDetails.SessionID;
                objModel.TCDetails.SBranchID = objModel.SBranchID;
                objModel.TCDetails.CreatedBy = loggedInUser.UserID;

                if (objModel.TCDetails.TCID <= 0)
                {
                    objModel.TCDetails.TCID = objAccountData.GetExistingTCID(objModel.TCDetails.StudentID, objModel.TCDetails.SessionID, objModel.SBranchID);
                }

                objModel.TCDetails.TCID = objAccountData.InsertUpdateTC(objModel.TCDetails);
                objAccountData.GetTCDetails(objModel);
                //ViewBag.Message = "Success";
                //return View(objModel);
                return RedirectToAction("GetTCForStudent", objModel);
                //return Json(new { newUrl = Url.Action("GetTCForStudent","Account" ) });
            }

            
   
        }
        [PermissionFilter]
        public ActionResult GetTCForStudent(TCDetailsModel oModel)
        {
            if (oModel.DOB.Year == 1)
            {
                oModel.DOB = CommonUsage.GetCurrentDate();
            }
            oModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAccountData.GetTCDetails(oModel);
            return View(oModel);
        }

        [PermissionFilter]
        public ActionResult ViewTCForStudent(string ID = null, string ID2 = null)
        {
            TCDetailsModel objModel = new TCDetailsModel();
            //int  SessionID= CommonUsage.ConvertToInt(ID);
            //int StudentID = CommonUsage.ConvertToInt(ID2);

            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel.SessionID= CommonUsage.ConvertToInt(ID);
            objModel.StudentID = CommonUsage.ConvertToInt(ID2);
            objAccountData.GetTCDetails(objModel);
            return RedirectToAction("GetTCForStudent", objModel);
        }
        #endregion
        #region Reports

        [PermissionFilter]
        public ActionResult EMonthlyAttendance(MonthlyAttendancePageModel objModel)
        {
            objModel.BranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objModel.RepoDate.Year == 1)
            {
                objModel.RepoDate = CommonUsage.GetCurrentDate();
                objModel.EmployeeType = 1;
            }
            objAccountData.GetMonthlyAttendance(objModel);
            return View(objModel);
        }

        [PermissionFilter]
        public ActionResult EMonthlyAttendanceStatus(MonthlyAttendancePageModel objModel)
        {
            objModel.BranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objModel.RepoDate.Year == 1)
            {
                objModel.RepoDate = CommonUsage.GetCurrentDate();
                objModel.EmployeeType = 1;
            }
            objAccountData.GetMonthlyAttendanceStatus(objModel);
            return View(objModel);
        }

        [PermissionFilter]
        public ActionResult StudentsAttendanceMonthly(StudentsMonthlyAttendancePageModel objModel)
        {
            if (objModel.ReportDate.Year == 1)
            {
                objModel.ReportDate = CommonUsage.GetCurrentDate();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStudentsMonthlyAttendance(objModel.ClassID, objModel.SectionID, SBranchID, objModel.SessionID, objModel.ReportDate);
            return View(objModel);
        }


        [PermissionFilter]
        public ActionResult DemandRecipt2(DemandReciptListModel objData = null)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objData.DemandMonth.Year == 1)
            {
                objData.DemandMonth = CommonUsage.GetCurrentDate();
            }


            DemandReciptListModel objModel = objAccountData.GetDemandReciptData(SBranchID, objData.ClassID, objData.SectionID, objData.SessionID);

            return View(objModel);
        }


        [PermissionFilter]
        public ActionResult DemandRecipt1(DemandReciptListModel objData = null)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            if (objData.DemandMonth.Year == 1)
            {
                objData.DemandMonth = CommonUsage.GetCurrentDate();
            }
            //   DemandReciptListModel objModel = objAccountData.GetDemandReciptData1(SBranchID, objData.DemandMonth.Month, objData.DemandMonth.Year, objData.ClassID, objData.SectionID);

            DemandReciptListModel objModel = objAccountData.GetDemandReciptData1(SBranchID, objData.DemandMonth.Month, objData.DemandMonth.Year, objData.ClassID, objData.SectionID, objData.SessionID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult DemandRecipt(DemandReciptListModel objData = null)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objData.DemandMonth.Year == 1)
            {
                objData.DemandMonth = CommonUsage.GetCurrentDate();
            }


            // DemandReciptListModel objModel = objAccountData.GetDemandReciptData(SBranchID,objData.ClassID, objData.SectionID);
            DemandReciptListModel objModel = objAccountData.GetDemandReciptDataNew(SBranchID, objData.DemandMonth, objData.ClassID, objData.SectionID, objData.SessionID);

            return View(objModel);
        }

        #region Exam Related
        [PermissionFilter]
        public ActionResult EvaluationSchemeManagementExam(string ID = null)
        {
            int SessionID = CommonUsage.ConvertToInt(ID);
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<EvaluationSchemeModel> objModel = objAccountData.GetEvaluationSchemeExam(SBranchID, SessionID);
            return PartialView("_EvaluationsSchemePartial", objModel);
        }
        [PermissionFilter]

        public ActionResult AdmitCard(AdmitCardListModel objData = null)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            objAccountData.GetAdmitCards(objData);

            return View(objData);
        }
        [PermissionFilter]
        public ActionResult AdmitCardCompact(AdmitCardListModel objData = null)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            objAccountData.GetAdmitCards(objData);

            return View(objData);
        }
        [PermissionFilter]
        public ActionResult ExamDate(AdmitCardListModel objData = null)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            objAccountData.GetExamDate(objData);

            return View(objData);
        }

        [PermissionFilter]
        public ActionResult DeskSlip(AdmitCardListModel objData = null)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            objAccountData.GetDeskSlip(objData);

            return View(objData);
        }

        // Shishupal Work on Exam Date Sheet For School
        // Date : 17 Nov 2021
        [PermissionFilter]
        public ActionResult EvaluationTypeManagementExam(string ID = null)
        {

            StudentExamDatesheetModel objSModel = new StudentExamDatesheetModel();
            int EvaluationSchemeID = CommonUsage.ConvertToInt(ID);
            EvaluationTypeModel objSs = new EvaluationTypeModel();
            //  int SessionID = objSs.SessionID;
            //if (Session["SBranchID"] == null)
            //{
            //    Session["SBranchID"] = 1;
            //}
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            IEnumerable<NameIDModel> objModel = objAccountData.GetEvaluationTypesExam(SBranchID, EvaluationSchemeID);
            return PartialView("_EvaluationsPartial", objModel);
        }

        [PermissionFilter]
        public ActionResult ExamDateSheet(StudentExamDatesheetModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.ExamDateSheet(objModel);
            return View(objModel);
        }

        // End Shihupal  Exam Date Sheet
        #endregion


        [PermissionFilter]
        public ActionResult StudentsIDCards(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStudents(objModel.ClassID, objModel.SectionID, SBranchID, objModel.SessionID);
            if (objModel.ClassID != 0)
            {
                objModel.ClassName = objModel.Classes.Where(c => c.ClassID == objModel.ClassID).SingleOrDefault().ClassName;
                objModel.SectionName = objModel.Sections.Where(c => c.ID == objModel.SectionID).SingleOrDefault().Name;
                objModel.SessionName = objModel.Sessions.Where(c => c.ID == objModel.SessionID).SingleOrDefault().Name;
            }
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult StudentsIDCardsV(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStudents(objModel.ClassID, objModel.SectionID, SBranchID, objModel.SessionID);
            if (objModel.ClassID != 0)
            {
                objModel.ClassName = objModel.Classes.Where(c => c.ClassID == objModel.ClassID).SingleOrDefault().ClassName;
                objModel.SectionName = objModel.Sections.Where(c => c.ID == objModel.SectionID).SingleOrDefault().Name;
                objModel.SessionName = objModel.Sessions.Where(c => c.ID == objModel.SessionID).SingleOrDefault().Name;
            }
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult StudentsIDCardsHorizontal(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStudents(objModel.ClassID, objModel.SectionID, SBranchID, objModel.SessionID);
            if (objModel.ClassID != 0)
            {
                objModel.ClassName = objModel.Classes.Where(c => c.ClassID == objModel.ClassID).SingleOrDefault().ClassName;
                objModel.SectionName = objModel.Sections.Where(c => c.ID == objModel.SectionID).SingleOrDefault().Name;
                objModel.SessionName = objModel.Sessions.Where(c => c.ID == objModel.SessionID).SingleOrDefault().Name;
            }
            return View(objModel);
        }
        public ActionResult StudentClassReport(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStudentClassReport(objModel.ClassID, objModel.SectionID, SBranchID, objModel.SessionID);
            return View(objModel);
        }
        public ActionResult StudentClassReportF(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStudentClassReport(objModel.ClassID, objModel.SectionID, SBranchID, objModel.SessionID);
            return View(objModel);
        }

        public ActionResult SiblingReport(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetSiblingReport(SBranchID, objModel.SessionID);
            return View(objModel);
        }
        public ActionResult AllStudentReportEWS(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetAllStudentReportEWS(SBranchID, objModel.SessionID);
            return View(objModel);
        }
        public ActionResult AllStudentReport(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetAllStudentReport(SBranchID, objModel.SessionID);
            return View(objModel);
        }
        public ActionResult AllStudentReportF(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetAllStudentReport(SBranchID, objModel.SessionID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult AbsentStudentReport(AbsentStudentReportModel objModel)
        {

            if (objModel.SelectedDate.Year == 1)
            {
                objModel.SelectedDate = CommonUsage.GetCurrentDate();
            }
            objModel.Month = objModel.SelectedDate.Month;
            objModel.Year = objModel.SelectedDate.Year;

            objModel.Day = objModel.SelectedDate.Day;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetAbsentStudentReport(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult DailyAttandanceReport(DailyAttandanceReportModel objData = null)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objData.ReportDate.Year == 1)
            {
                objData.ReportDate = CommonUsage.GetCurrentDate();
            }
            DailyAttandanceReportModel objModel = objAccountData.GetDailyAbsentReport(objData);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ClassWiseFeeReport()
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            List<ClassWiseFeeDueModel> objModel = objAccountData.GetClassWiseDueFeeDetails(SBranchID).ToList();
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult MonthlyFeeCollection(StudentFeeModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentFeeModel();

            }
            if (objModel.SelectedDate.Year == 1)
            {
                objModel.SelectedDate = CommonUsage.GetCurrentDate();//.AddMonths(-1);
            }
            objModel.Month = objModel.SelectedDate.Month;
            objModel.Year = objModel.SelectedDate.Year;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetMonthlyFeeCollection(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult MonthlyExpenceReport(ExpenceManagementModel objModel)
        {
            if (objModel == null)
            {
                objModel = new ExpenceManagementModel();

            }
            if (objModel.StartDate.Year == 1)
            {
                objModel.StartDate = CommonUsage.GetCurrentDate();//.AddMonths(-1);
            }
            objModel.Month = objModel.StartDate.Month;
            objModel.Year = objModel.StartDate.Year;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetMonthlyExpenceReport(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult StudentPayments(StudentsPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentsPageModel();
            }
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStudentPaymentRecipts(objModel.ClassID, objModel.SectionID, SBranchID, 0);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult RequestPaymentCancellation(string id = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            string ContentID = "0";
            string OTP = CommonUsage.RandomString(4, false);
            PaymentModel objModel = objAccountData.CancelPaymentReuest(ID, OTP);

            string SMS = "OTP for deletion of Payment of " + objModel.PaymentAmount.ToString("0.00") + " made on "
                + objModel.PaymentDate.ToString("dd MMM,yyyy") + " is " + OTP + ", please use the OTP if you want to process deletion.";
            if (objModel.Mobile != null && objModel.Mobile != "")
            {
                SMSSender objSender = new SMSSender();
                objSender.SendSMSAsync(SMS, objModel.Mobile, SBranchID, 4, 1, objModel.PaymentID, ContentID);
            }
            objModel.Mobile = "XXXXXX" + objModel.Mobile.Substring(objModel.Mobile.Length - 4);
            return Json(objModel, JsonRequestBehavior.AllowGet);
        }

        [PermissionFilter]
        public ActionResult ProcessPaymentCancellation(string id = null, string id2 = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            string OTP = id2;
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            int objModel = objAccountData.ProcessPaymentReuest(ID, OTP);
            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult GetStudentPaymentList(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            IEnumerable<FeePaymentModel> model = objAccountData.GetStudentFeePayments(iID, SBranchID);
            return PartialView("_StudentPaymentListPartial", model);
        }
        [PermissionFilter]
        public ActionResult GetStudentPaymentListPaymentPage(string ID = null, string ID2 = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            int iSessionID = CommonUsage.ConvertToInt(ID2);
            IEnumerable<FeePaymentModel> model = objAccountData.GetStudentFeePaymentsSessionwise(iID, iSessionID);
            return PartialView("_StudentPaymentListFeePagePartial", model);
        }

        [PermissionFilter]
        public ActionResult ClassCategoryGenderStudents(ClassGenderCategoryCountPageModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetClassGenderCategoryHouseCount(objModel);
            return View(objModel);
        }
        #endregion

        #region Exam Resulst
        [PermissionFilter]
        public ActionResult ExamResults(TeacherResultPageModel objModel)
        {

            objModel.TeacherID = PermissionManager.GetLoggedInUser().UserID;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetMiniExamResults(objModel);
            return View(objModel);
        }

        public ActionResult GetSectionEvaluationsOnClass(string id = null, string id2 = null)
        {
            int ID = CommonUsage.ConvertToInt(id);
            int ID2 = CommonUsage.ConvertToInt(id2);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            TeacherResultPageModel objModel = objAdminData.GetSectionsEvaluationsOnClass(ID, SBranchID, ID2);
            return Json(objModel, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult GetSubjectsForSection(string ID = null)
        {
            int SectionID = CommonUsage.ConvertToInt(ID);
            int TeacherID = PermissionManager.GetLoggedInUser().UserID;
            IEnumerable<NameIDModel> model = objAccountData.GetSubjectsForSection(SectionID);

            return PartialView("_SelectOptionsPartial", model);
        }
        [PermissionFilter]
        public ActionResult GetSubjectsForSectionEvaluation(string ID = null, string ID2 = null)
        {
            int SectionID = CommonUsage.ConvertToInt(ID);
            int EvaluationID = CommonUsage.ConvertToInt(ID2);
            int TeacherID = PermissionManager.GetLoggedInUser().UserID;
            IEnumerable<NameIDModel> model = objAccountData.GetSubjectsForSectionEvalluation(SectionID, EvaluationID);

            return PartialView("_SelectOptionsPartial", model);
        }
        [PermissionFilter]
        public ActionResult UpdateTeacherResults(TeacherResultPageModel objModel)
        {
            int id = objTeacherData.UpdateStudentResults(objModel);
            objModel.ExamResults = null;
            return RedirectToAction("ExamResults", "Account", objModel);
        }
        [PermissionFilter]
        public ActionResult StudentResultScrutiny(StudentResultScrutinyListModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStudentsResultScrutiny(objModel);
            return View(objModel);
        }

        [PermissionFilter]
        public ActionResult StudentResultList(StudentResultScrutinyListModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.StudentResultList(objModel);
            return View(objModel);
        }

        [PermissionFilter]
        public ActionResult StudentResultCheck(StudentResultScrutinyListModel objModel)
        {

            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStudentsResultCheck(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult ClassResultReport(StudentResultScrutinyListModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetClassResultReport(objModel);
            return View(objModel);
        }

        [PermissionFilter]
        public ActionResult StudentPerformance(StudentPerformanceListModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStudentsPerformanceMini(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult StudentTotalResult(string ID = null, string ID2 = null)
        {
            TotalResultModel objModel = new TotalResultModel();
            int StudentID = CommonUsage.ConvertToInt(ID);
            int SessionID = CommonUsage.ConvertToInt(ID2);
            objModel = objTeacherData.GetStudentTotalResult(StudentID, SessionID);
            return View(objModel);
        }

        [PermissionFilter]
        public ActionResult StudentPerformanceDetailsClassShine(StudentPerformanceListModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStudentsPerformanceShine(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult StudentPerformanceDetailsClass(StudentPerformanceListModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStudentsPerformanceMini(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult StudentPerformanceReportPartial(string ID = null, string ID2 = null, string ID3 = null)
        {
            PerformanceParameterDetailModel objModel = new PerformanceParameterDetailModel();
            objModel.StudentSessionUID = CommonUsage.ConvertToInt(ID);
            objModel.EvaluationID = CommonUsage.ConvertToInt(ID2);
            objModel.SessionID = CommonUsage.ConvertToInt(ID3);
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objTeacherData.GetStudentPerformanceDetails(objModel);
            return PartialView("_StudentPerformanceDetailsPartial", objModel);
        }

        [PermissionFilter]
        public ActionResult StudentPerformanceDetailsESS(string ID = null, string ID2 = null, string ID3 = null)
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
        public ActionResult StudentPerformanceDetailsH(string ID = null, string ID2 = null, string ID3 = null)
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
        // for shine
        [PermissionFilter]
        public ActionResult StudentPerformanceResult(string ID = null, string ID2 = null, string ID3 = null)
        {
            StudentPerformanceResultModel objModel = new StudentPerformanceResultModel();
            objModel.StudentSessionUID = CommonUsage.ConvertToInt(ID);
            objModel.EvaluationID = CommonUsage.ConvertToInt(ID2);
            objModel.SessionID = CommonUsage.ConvertToInt(ID3);
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objTeacherData.GetStudentPerformanceResult(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult StudentPerformanceResult1(string ID = null, string ID2 = null, string ID3 = null)
        {
            StudentPerformanceResultModel objModel = new StudentPerformanceResultModel();
            objModel.StudentSessionUID = CommonUsage.ConvertToInt(ID);
            objModel.EvaluationID = CommonUsage.ConvertToInt(ID2);
            objModel.SessionID = CommonUsage.ConvertToInt(ID3);
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objTeacherData.GetStudentPerformanceResult(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult StudentPerformanceReportPartialShine(string ID = null, string ID2 = null, string ID3 = null)
        {
            //  PerformanceParameterDetailModel objModel = new PerformanceParameterDetailModel();
            StudentPerformanceResultModel objModel = new StudentPerformanceResultModel();
            objModel.StudentSessionUID = CommonUsage.ConvertToInt(ID);
            objModel.EvaluationID = CommonUsage.ConvertToInt(ID2);
            objModel.SessionID = CommonUsage.ConvertToInt(ID3);
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objTeacherData.GetStudentPerformanceResult(objModel);
            return PartialView("_StudentPerformanceDetailsPartialShine", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateStudentPerformance(PerformanceParameterDetailModel objModel)
        {
            int id = objTeacherData.UpdateStudentPerformance(objModel);
            objModel.PerformanceParameters = null;
            System.Web.Routing.RouteValueDictionary route = new System.Web.Routing.RouteValueDictionary();
            route.Add("ID", objModel.StudentSessionUID);
            route.Add("ID2", objModel.EvaluationID);
            return RedirectToAction("StudentPerformanceDetails", "Account", route);
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
        [PermissionFilter]
        public ActionResult StudentResultDetails11(string ID = null, string ID2 = null, string ID3 = null)
        {
            PerformanceParameterDetailModel objModel = new PerformanceParameterDetailModel();
            objModel.StudentSessionUID = CommonUsage.ConvertToInt(ID);
            objModel.EvaluationID = CommonUsage.ConvertToInt(ID2);
            objModel.SessionID = CommonUsage.ConvertToInt(ID3);
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objTeacherData.GetStudentResultDetails(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult StudentResultDetailsH(string ID = null, string ID2 = null, string ID3 = null)
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

        [PermissionFilter]
        public ActionResult MonthlyAttendance(MonthlyAttendancePageModel objModel)
        {
            objModel.BranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objModel.RepoDate.Year == 1)
            {
                objModel.RepoDate = CommonUsage.GetCurrentDate();
            }
            objAdminData.GetMonthlyAttendance(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult AnnualResultPatrak(ClassGenderCategoryCountPageModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetClassGenderCategoryCount(objModel);
            return View(objModel);
        }

        [PermissionFilter]
        public ActionResult ClassHouseGenderStudents(ClassGenderCategoryCountPageModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetClassGenderCategoryHouseCount(objModel);
            return View(objModel);
        }

        #region StockManagement Module
        [PermissionFilter]
        public ActionResult StockManagement(StockManagementModel objModel)
        {
            if (objModel.StartDate.Year == 1)
            {
                objModel.EndDate = CommonUsage.GetCurrentDate();
                objModel.StartDate = objModel.EndDate.AddDays(-7);
            }
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStockTransfers(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GetSaleTransactionPrintData(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            PrintSaleReceiptModel model = objAccountData.GetSaleTransactionPrintData(iID, SBranchID);
            return PartialView("_PrintProductSell", model);
        }
        [PermissionFilter]





        public ActionResult GetSaleTransactionPaymentReceiptData(string ID = null, string paymentId = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            int iPaymentID = CommonUsage.ConvertToInt(paymentId);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            PrintSaleReceiptModel model = objAccountData.GetSaleTransactionPrintData(iID, SBranchID, iPaymentID);
            return PartialView("_PrintProductPaymentReceipt", model);
        }
        [PermissionFilter]
        [HttpPost]
        public ActionResult CancelStockPaymentReceipt(string id = null, string paymentId = null, string remark = null)
        {
            int iID = CommonUsage.ConvertToInt(id);
            int iPaymentID = CommonUsage.ConvertToInt(paymentId);
            int sBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int userID = PermissionManager.GetLoggedInUser().UserID;
            string mode = "receiptcancelled";
            bool success = false;
            if (iID > 0 && iPaymentID > 0)
            {
                int result = objAccountData.CancelStockTransactionPayment(iPaymentID, sBranchID, remark, userID);
                if (result == -2)
                {
                    mode = "latestreceiptonly";
                }
                else if (result > 0)
                {
                    success = true;
                }
            }
            if (!success && mode == "receiptcancelled")
            {
                mode = "receiptcancelfailed";
            }
            return Redirect("/Account/StockDetails/" + iID + "?saved=1&mode=" + mode + "&paymentId=" + iPaymentID);
        }
        [PermissionFilter]




        public ActionResult GetStudentsForStockTransfer(string id = null)
        {
            int STID = CommonUsage.ConvertToInt(id);
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            StockTransaferMasterModel model = objAccountData.GetStudentsStockTransfers(STID, SBranchID);

            return Json(model, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult GetVendorsForStockTransfer(string id = null)
        {
            int STID = CommonUsage.ConvertToInt(id);
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            StockTransaferMasterModel model = objAccountData.GetVendorsStockTransfers(STID, SBranchID);

            return Json(model, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult GetEmployeesForStockTransfer(string id = null, string id2 = null)
        {
            int STID = CommonUsage.ConvertToInt(id);
            int EmployeeTypeID = CommonUsage.ConvertToInt(id2);
            if (Session["SBranchID"] == null)
            {
                Session["SBranchID"] = 1;
            }
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            StockTransaferMasterModel model = objAccountData.GetEmployeesStockTransfers(STID, SBranchID, EmployeeTypeID);

            return Json(model, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]

             public ActionResult StockDetails(string ID = null, string saved = null, string mode = null, string paymentId = null)


        {
            int STID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            StockTransaferMasterModel objModel = objAccountData.GetStockTransferDetails(STID, SBranchID);

            ViewBag.StockSaved = saved == "1";
            ViewBag.StockSaveMode = mode ?? "";
            ViewBag.LastSavedPaymentID = CommonUsage.ConvertToInt(paymentId);



            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult SaveStock(StockTransaferMasterModel oModel)
        {
            oModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            oModel.CreatedDate = CommonUsage.GetCurrentDate();
                        int loggedInUserID = PermissionManager.GetLoggedInUser().UserID;
            StockTransaferMasterModel existingModel = null;
            decimal currentPaymentAmount = oModel.PaidAmount;
            decimal paymentReceivedNow = 0;
            bool isAdditionalPayment = false;
            int paymentReceiptID = 0;
            if (oModel.Details == null)
            {
                oModel.Details = new List<StockTransaferDetailModel>();
            }
            if (oModel.STID > 0)
            {
                existingModel = objAccountData.GetStockTransferDetails(oModel.STID, oModel.SBranchID);
            }
            if (existingModel != null
                && existingModel.STID > 0
                && existingModel.PaymentHistory != null
                && existingModel.PaymentHistory.Count > 0
                && oModel.Status == 0)
            {
                oModel.Status = existingModel.Status;
                oModel.CancelRemark = existingModel.CancelRemark;
                oModel.SelectedPaymentID = 0;
            }
            oModel.Amount = oModel.Details
                .Where(x => x.OpType != -1)
                .Sum(x => x.Cost * x.Quantity + x.Cost * x.Quantity * (x.SGST + x.CGST + x.IGST) / 100);
            if (oModel.PaymentDate == null || oModel.PaymentDate.Value.Year <= 1)
            {
                oModel.PaymentDate = oModel.TrDate;
            }
            if (oModel.TrType == 2)
            {
                oModel.PaidAmount = 0;
                oModel.DueAmount = 0;
                oModel.PaymentMode = -1;
                oModel.PaymentReferanceNo = "";
                oModel.PaymentStatus = 0;
            }
            else
            {
                if (currentPaymentAmount < 0)
                {
                    currentPaymentAmount = 0;
                }
                if (existingModel != null && existingModel.STID > 0 && existingModel.IsPaymentLocked && existingModel.Status != 0 && existingModel.DueAmount > 0)
                {
                    isAdditionalPayment = true;
                    oModel.PaidAmount = existingModel.PaidAmount + currentPaymentAmount;
                }
                else
                {
                    oModel.PaidAmount = currentPaymentAmount;
                }
                if (oModel.PaidAmount > oModel.Amount)
                {
                    oModel.PaidAmount = oModel.Amount;
                }
                oModel.DueAmount = oModel.Amount - oModel.PaidAmount;
                if (oModel.Status == 0)
                {
                    oModel.PaymentStatus = 3;
                }
                else if (oModel.PaidAmount >= oModel.Amount && oModel.Amount > 0)
                {
                    oModel.PaymentStatus = 2;
                }
                else if (oModel.PaidAmount > 0)
                {
                    oModel.PaymentStatus = 1;
                }
                else
                {
                    oModel.PaymentStatus = 0;
                }
            }

            if (existingModel != null && existingModel.STID > 0 && existingModel.IsPaymentLocked)
            {
                oModel.TrDate = existingModel.TrDate;
                oModel.TrType = existingModel.TrType;
                oModel.RefID = existingModel.RefID;
                oModel.RefType = existingModel.RefType;
                oModel.ClassID = existingModel.ClassID;
                oModel.SectionID = existingModel.SectionID;
                oModel.SessionID = existingModel.SessionID;
                oModel.EmployeeTypeID = existingModel.EmployeeTypeID;
                oModel.VendorID = existingModel.VendorID;
                oModel.Remark = existingModel.Remark;
                oModel.Details = existingModel.Details ?? new List<StockTransaferDetailModel>();
                oModel.Amount = existingModel.Amount;

                if (oModel.Status == 0)
                {
                    oModel.PaidAmount = existingModel.PaidAmount;
                    oModel.DueAmount = existingModel.DueAmount;
                    oModel.PaymentMode = existingModel.PaymentMode;
                    oModel.PaymentReferanceNo = existingModel.PaymentReferanceNo;
                    oModel.PaymentDate = existingModel.PaymentDate;
                    oModel.PaymentStatus = 3;
                }
                else if (isAdditionalPayment)
                {
                    if (currentPaymentAmount <= 0)
                    {
                        oModel.PaymentMode = existingModel.PaymentMode;
                        oModel.PaymentReferanceNo = existingModel.PaymentReferanceNo;
                        oModel.PaymentDate = existingModel.PaymentDate;
                        oModel.PaidAmount = existingModel.PaidAmount;
                        oModel.DueAmount = existingModel.DueAmount;
                        oModel.PaymentStatus = existingModel.PaymentStatus;
                    }
                }
                else
                {
                    oModel.PaidAmount = existingModel.PaidAmount;
                    oModel.DueAmount = existingModel.DueAmount;
                    oModel.PaymentMode = existingModel.PaymentMode;
                    oModel.PaymentReferanceNo = existingModel.PaymentReferanceNo;
                    oModel.PaymentDate = existingModel.PaymentDate;
                    oModel.PaymentStatus = existingModel.PaymentStatus;
                }
            }
            if (existingModel != null
                && existingModel.STID > 0
                && existingModel.PaymentHistory != null
                && existingModel.PaymentHistory.Count > 0
                && existingModel.DueAmount <= 0
                && currentPaymentAmount > 0)
            {
                oModel.PaidAmount = existingModel.PaidAmount;
                oModel.DueAmount = existingModel.DueAmount;
                oModel.PaymentStatus = existingModel.PaymentStatus;
                paymentReceivedNow = 0;
            }
            if (oModel.Status != 0 && oModel.TrType != 2)
            {
                paymentReceivedNow = oModel.PaidAmount - (existingModel == null ? 0 : existingModel.PaidAmount);
                if (paymentReceivedNow < 0)
                {
                    paymentReceivedNow = 0;
                }
            }
            int STID = objAccountData.UpdateStockTransaction(oModel);

            if (oModel.Status != 0 && oModel.TrType != 2 && paymentReceivedNow > 0)
            {
                paymentReceiptID = objAccountData.InsertStockTransactionPayment(
                    STID,
                    oModel.PaymentDate ?? oModel.TrDate,
                    oModel.PaymentMode,
                    oModel.PaymentReferanceNo,
                    paymentReceivedNow,
                    oModel.PaidAmount,
                    oModel.DueAmount,
                    oModel.PaymentStatus,
                    oModel.SBranchID,
                    loggedInUserID);
            }
            string saveMode = oModel.Status == 0 ? "cancelled" :
                oModel.PaymentStatus == 2 ? "paid" :
                oModel.PaymentStatus == 1 ? "partial" : "unpaid";
            string paymentQuery = paymentReceiptID > 0 ? "&paymentId=" + paymentReceiptID : "";
            return Redirect("/Account/StockDetails/" + STID + "?saved=1&mode=" + saveMode + paymentQuery);
        }
        [PermissionFilter]
        public ActionResult SalesDueListReport(StockSaleReportPageModel objModel)
        {
            if (objModel.StartDate.Year == 1)
            {
                objModel.EndDate = CommonUsage.GetCurrentDate();
                objModel.StartDate = objModel.EndDate.AddMonths(-1);
            }
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStockSaleReport(objModel, true);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult SalesReport(StockSaleReportPageModel objModel)
        {
            if (objModel.StartDate.Year == 1)
            {
                objModel.EndDate = CommonUsage.GetCurrentDate();
                objModel.StartDate = objModel.EndDate.AddMonths(-1);
            }
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetStockSaleReport(objModel, false);
            return View(objModel);




        }

        [PermissionFilter]
        public ActionResult LowStockProducts()
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            List<ProductModel> objModel = objAdminData.GetLowStockProducts(SBranchID);
            return View(objModel);
        }
        // Available Stock Products
        public ActionResult StockProducts()
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            List<ProductModel> objModel = objAdminData.GetAvailableStockProducts(SBranchID);
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
            return RedirectToAction("SMSRechargeHistory", "Account");
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
            return RedirectToAction("SMSTemplates", "Account");
        }

        [PermissionFilter]
        public ActionResult UpdateSMSStatus()
        {
            //var hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
            //hubContext.Clients.All.GetStatus(1, 1);

            return RedirectToAction("SendSMS", "Account");
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

        #region FeeRefund
        [PermissionFilter]
        public ActionResult FeeRefunds(FeeRefundPageModel objModel)
        {
            if (objModel.FromDate.Year == 1)
            {
                objModel.ToDate = CommonUsage.GetCurrentDate();
                objModel.FromDate = objModel.ToDate.AddDays(-7);
                objModel.Status = -1;
            }
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAccountData.GetFeeRefunds(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GetFeeRefundDetails(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            FeeRefundEditModel model = objAccountData.GetFeeRefundDetails(iID, SBranchID);
            return PartialView("_FeeRefundEditForm", model);
        }
        [PermissionFilter]
        public ActionResult GetFeeRefundPrint(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            FeeRefundPrintModel model = objAccountData.GetFeeRefundPrintData(iID, SBranchID);
            return PartialView("_PrintFeeRefund", model);
        }
        [PermissionFilter]
        public ActionResult SaveFeeRefund(FeeRefundModel oModel)
        {
            oModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            oModel.CreatedDate = CommonUsage.GetCurrentDate();
            int STID = objAccountData.UpdateFeeRefund(oModel);
            return Redirect("/Account/FeeRefunds");
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
        #region Extra Income Related
        [PermissionFilter]
        public ActionResult ExtraIncomes(ExtraIncomePageModel oModel)
        {
            if (TempData["ExtraIncomes"] != null)
            {
                oModel = (ExtraIncomePageModel)TempData["ExtraIncomes"];
            }
            if (oModel.StartDate.Year == 1)
            {
                oModel.EndDate = CommonUsage.GetCurrentDate();
                oModel.StartDate = oModel.EndDate.AddDays(-7);
                oModel.PaymentMode = -1;
            }
            oModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAccountData.GetExtraIncomes(oModel);
            return View(oModel);
        }
        [PermissionFilter]
        public ActionResult GetExtraIncomeDetails(string ID = null)
        {
            int ExtraIncomeID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            ExtraIncomePageModel model = objAccountData.GetExtraIncomeDetails(ExtraIncomeID, SBranchID);

            return PartialView("_EditExtraIncome", model);
        }
        [PermissionFilter]
        public ActionResult UpdateExtraIncome(ExtraIncomeModel oModel)
        {
            oModel.UserID = PermissionManager.GetLoggedInUser().UserID;
            oModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            oModel.CreatedDate = CommonUsage.GetCurrentDate();
            int Res = objAccountData.UpdateExtraIncome(oModel);
            ExtraIncomePageModel op = new ExtraIncomePageModel();
            op.PaymentMode = oModel.PaymentMode;
            op.StartDate = oModel.PaidDate;
            op.EndDate = oModel.PaidDate;
            TempData["ExtraIncomes"] = op;
            TempData["ExtraIncomeID"] = Res;
            return RedirectToAction("ExtraIncomes", "Account");
        }
        #endregion
        #region Expense Book

        [PermissionFilter]
        public ActionResult ExpenseBook(CollectionReportModel objModel)
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
            objModel = objAccountData.GetExpenseBookMonthlyReport(objModel);
            return View(objModel);
        }
        #endregion

        #region DayBook Related
        [PermissionFilter]
        public ActionResult DayBook(DayBookPageModel oModel)
        {
            if (TempData["DayBook"] != null)
            {
                oModel = (DayBookPageModel)TempData["DayBook"];
            }
            if (oModel.StartDate.Year == 1)
            {
                oModel.EndDate = CommonUsage.GetCurrentDate();
                oModel.StartDate = oModel.EndDate.AddDays(-7);
                oModel.PaymentMode = -1;
            }
            oModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAccountData.GetDayBook(oModel);
            return View(oModel);
        }
        [PermissionFilter]
        public ActionResult GetDayBookDetails(string ID = null)
        {
            int DBookID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            DayBookPageModel model = objAccountData.GetDayBookDetails(DBookID, SBranchID);

            return PartialView("_EditDayBook", model);
        }
        [PermissionFilter]
        public ActionResult UpdateDayBook(DayBookModel oModel)
        {
            oModel.UserID = PermissionManager.GetLoggedInUser().UserID;
            oModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            oModel.CreatedDate = CommonUsage.GetCurrentDate();
            int Res = objAccountData.UpdateDayBook(oModel);
            DayBookPageModel op = new DayBookPageModel();
            op.PaymentMode = oModel.PaymentMode;
            op.StartDate = oModel.Date;
            op.EndDate = oModel.Date;
            TempData["DayBook"] = op;
            TempData["DBookID"] = Res;
            return RedirectToAction("DayBook", "Account");
        }
        #endregion

        #region change Password
        public ActionResult UpdateStudentPassword(string ID = null, string parentid = null)
        {
            string Password = ID;
            int UserID = CommonUsage.ConvertToInt(parentid);
            int UserType = 4;
            CommonData objCommonData = new CommonData();
            int i = objAccountData.UpdatePassword(UserID, UserType, Password);
            // objAccountData.UpdatePassword(UserID, UserType, Password);
            return Json(i, JsonRequestBehavior.AllowGet);

            //  return View(objModel);
            // return RedirectToAction("Students", "Account");
        }
        public ActionResult UpdateEmployeePassword(string ID = null, string employeeid = null, string UType= null)
        {
            string Password = ID;
            int UserID = CommonUsage.ConvertToInt(employeeid);
            int UserType = CommonUsage.ConvertToInt(UType);
            CommonData objCommonData = new CommonData();
            int i = objAccountData.UpdatePassword(UserID, UserType, Password);
            return Json(i, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Student Block

        public ActionResult UpdateIsBlock(string ID = null, string ID2 = null)
        {
            int isBlock = CommonUsage.ConvertToInt(ID);
            int studentID = CommonUsage.ConvertToInt(ID2);
            int res = objAccountData.UpdateIsBlock(isBlock, studentID);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region FeeReport
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
        public ActionResult DueFeeReport(DemandReciptListModel objData = null)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            if (objData.DemandMonth.Year == 1)
            {
                objData.DemandMonth = CommonUsage.GetCurrentDate();
            }
            //   DemandReciptListModel objModel = objAccountData.GetDemandReciptData1(SBranchID, objData.DemandMonth.Month, objData.DemandMonth.Year, objData.ClassID, objData.SectionID);

            DemandReciptListModel objModel = objAccountData.GetDemandReciptDataNew(SBranchID, objData.DemandMonth, objData.ClassID, objData.SectionID, objData.SessionID);
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
        public ActionResult MonthlyDueFeeReport(FeePaymentModel objData = null)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            if (objData.DemandMonth.Year == 1)
            {
                objData.DemandMonth = CommonUsage.GetCurrentDate();
            }
            //   DemandReciptListModel objModel = objAccountData.GetDemandReciptData1(SBranchID, objData.DemandMonth.Month, objData.DemandMonth.Year, objData.ClassID, objData.SectionID);

            FeePaymentModel objModel = objAccountData.GetDuefeeReportMonthlyNew(SBranchID, objData.DemandMonth, objData.ClassID, objData.SectionID, objData.SessionID);
            return View(objModel);
        }
        [PermissionFilter]





        public ActionResult CollectionReport(FeePaymentModel objData = null)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            if (objData.DemandMonth.Year == 1)
            {
                objData.DemandMonth = CommonUsage.GetCurrentDate();
            }
         
            FeePaymentModel objModel = objAccountData.GetCollectionReport(SBranchID, objData.DemandMonth, objData.ClassID, objData.SectionID, objData.SessionID);
            return View(objModel);
        }
        [PermissionFilter]

        public ActionResult ClassWiseCollectionSummary(ClassWiseCollectionSummaryPageModel objData = null)
        {
            if (objData == null)
            {
                objData = new ClassWiseCollectionSummaryPageModel();
            }

            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objData.SessionID == 0)
            {
                objData.SessionID = -1;
            }
            ClassWiseCollectionSummaryPageModel objModel = objAccountData.GetClassWiseCollectionSummaryReport(SBranchID, objData.SessionID, objData.ClassID, objData.SectionID);
            return View(objModel);
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
                objModel.SessionID = 0;


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


            objModel.ReportType = 2;

           
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

        #endregion

        #region studentadmissionreport
        [PermissionFilter]
        public ActionResult StudentAdmissionReport(StudentAdmissionReportModel oModel)
        {
            oModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            if (oModel.StartDate.Year == 1)
            {
                oModel.StartDate = CommonUsage.GetCurrentDate();
                oModel.StartDate = new DateTime(oModel.StartDate.Year, oModel.StartDate.Month, 1);
                // oModel.StartDate = oModel.StartDate.AddDays(-oModel.StartDate.Day + 1);
              oModel.EndDate = CommonUsage.GetCurrentDate();
                //oModel.EndDate = oModel.EndDate.AddDays(-oModel.EndDate.Day + 1);
                oModel.EndDate = oModel.EndDate.AddMonths(1).AddDays(-1);
            }
            objAdminData.GetStudentAdmssionDetail(oModel);
            return View(oModel);
        }

        [PermissionFilter]
        public ActionResult SessionAdmissionReport(StudentAdmissionReportModel oModel)
        {
            oModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            objAccountData.GetSessionAdmissionReport(oModel);
            return View(oModel);
        }
        #endregion

        #region promotedstudentsreport
        [PermissionFilter]
        public ActionResult PromotedStudentsReport(StudentAdmissionReportModel oModel)
        {
            oModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;

            objAdminData.GetPromotedStudentsDetail(oModel);
            return View(oModel);
        }
        #endregion



        #region TransportFee

        [PermissionFilter]
        public ActionResult SaveTranposrtFeePayment(FeePaymentModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            //  objModel.QDate = CommonUsage.GetCurrentDate();
            objModel.QDate = CommonUsage.ConvertToDateTime(objModel.PaymentDate.ToString());
            //string FeeDate= objModel.FeeDate.ToShortDateString();
            //if (FeeDate.Length>0)
            //{
            //    objModel.FeeDate = objModel.FeeDate;
            //}
            //else
            //{
            //    objModel.FeeDate = CommonUsage.GetCurrentDate();
            //}
            objModel.UserID = PermissionManager.GetLoggedInUser().UserID;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel.Day = objModel.QDate.Day;
            FeePaymentRowModel objData = objAccountData.SaveStudentTransportFeePayments(objModel);

            return PartialView("_StudentFeeRowPartial", objData);
        }

        [PermissionFilter]
        public async Task<ActionResult> GetFeePaymentDetailsForTransport(FeePaymentModel objModel)
        {
            objModel.QDate = CommonUsage.GetCurrentDate();
            objModel.Day = objModel.QDate.Day;
            if (objModel.Month == 0)
            {
                objModel.Month = objModel.QDate.Month;
                objModel.Year = objModel.QDate.Year;
            }
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = await objAccountData.GetFeeDetailsNewForTransport(objModel);
            objModel.FeeDate = CommonUsage.GetCurrentDate();
            return PartialView("_TransportFeePaymentDetail", objModel);
        }
        [PermissionFilter]
        public ActionResult StudentTransportFee(StudentFeeModel objModel)
        {
            if (objModel == null)
            {
                objModel = new StudentFeeModel();

            }
            if (objModel.SelectedDate.Year == 1)
            {
                objModel.SelectedDate = CommonUsage.GetCurrentDate();//.AddMonths(-1);
            }
            objModel.Month = objModel.SelectedDate.Month;
            objModel.Year = objModel.SelectedDate.Year;
            objModel.Day = objModel.SelectedDate.Day;
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetNewFeePayments(objModel);
            return View(objModel);
        }

        #endregion

        #region Bulk Student Data Upload
        [PermissionFilter]

        public ActionResult BulkStudentUploadEnt(BulkStudentUploadModel objModel)
        {
            HttpPostedFileBase fileupload = objModel.ExcelFile;
            int SBranchID = CommonUsage.ConvertToInt(Session["SBranchID"].ToString());
            objModel = objAccountData.GetBulkUploadData(objModel.ClassID, objModel.SectionID, SBranchID, objModel.SessionID);
            objModel.ExcelFile = fileupload;
            if (objModel.ExcelFile != null)
            {
                string extension = System.IO.Path.GetExtension(objModel.ExcelFile.FileName).ToLower();
                string query = null;
                string connString = "";

                string[] validFileTypes = { ".xls", ".xlsx", ".csv" };

                string path1 = string.Format("{0}/{1}", Server.MapPath("~/Content/Uploads/"), objModel.ExcelFile.FileName.Replace(" ", "_"));
                if (!Directory.Exists(path1))
                {
                    Directory.CreateDirectory(Server.MapPath("~/Content/Uploads"));
                }
                if (validFileTypes.Contains(extension))
                {
                    DataTable dt = new DataTable();
                    if (System.IO.File.Exists(path1))
                    {
                        System.IO.File.Delete(path1);
                    }
                    objModel.ExcelFile.SaveAs(path1);
                    if (extension == ".csv")
                    {
                        dt = Utility.ConvertCSVtoDataTable(path1);
                        ViewBag.Data = dt;
                    }
                    //Connection String to Excel Workbook  
                    /* else if (extension.Trim() == ".xls")
                     {
                         connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path1 + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                         dt = Utility.ConvertXSLXtoDataTable(path1, connString);
                         ViewBag.Data = dt;
                     }
                     else if (extension.Trim() == ".xlsx")
                     {

                         connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                         dt = Utility.ConvertXSLXtoDataTable(path1, connString);
                         ViewBag.Data = dt;
                     }
                    */
                    else if (extension == ".xls" || extension == ".xlsx")
                    {
                        // NO MORE OLEDB PROVIDERS NEEDED!
                        // Just pass the file path to your new Utility method
                        dt = Utility.ConvertXSLXtoDataTable(path1);
                    }
                    ViewBag.Data = dt;
                    objModel.Students = CommonUsage.ConvertDataTable<StudentBulkUploadModel>(dt);
                }
                else
                {
                    ViewBag.Error = "Please Upload Files in .xls, .xlsx or .csv format";

                }

            }

            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult BulkUploadStudentsEnt(BulkStudentUploadModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objAccountData.UpdateBulkStudentsEnt(objModel);
            StudentsPageModel objNewModel = new StudentsPageModel();
            objNewModel.ClassID = objModel.ClassID;
            objNewModel.SectionID = objModel.SectionID;
            objNewModel.SessionID = objModel.SessionID;
            return RedirectToAction("Students", "Account", objNewModel);
        }

        public ActionResult BulkUploadInfo(BulkUploadInfoModel oModel)
        {
            oModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            oModel = objAccountData.GetBulkUploadInfo(oModel);
            return View(oModel);
        }
        #endregion Bulk Student Data Upload


        #region downloademployeedocumentsdetails
        [PermissionFilter]
        public ActionResult DownloadEmployeeDetails(EmployeeListPageModel objModel)
        {
            if (objModel == null)
            {
                objModel = new EmployeeListPageModel();

            }
            //if (objModel.EmployeeType == 0)
            //{
            //    objModel.EmployeeType = 1;
            //}
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = objAccountData.GetEmployees(objModel.EmployeeType, SBranchID);
            return View(objModel);
        }
        public FileResult DownloadAllEmployeesCerts(EmployeeDocumentDownloadModel oModel)
        {
            string suffix = "";
            if (oModel.DocType == 1)
            {
                suffix = "Birth_";
            }
            if (oModel.DocType == 2)
            {
                suffix = "Experience_";
            }
            var context = System.Web.HttpContext.Current;
            //var folderPath = context.Server.MapPath(string.Format("~/{0}", id));
            var folderPath = context.Server.MapPath(CommonUsage.EmployeeDocumentsBasePath);

            DirectoryInfo folder = new DirectoryInfo(folderPath);
            var baseOutputStream = new MemoryStream();
            ZipOutputStream zipOutput = new ZipOutputStream(baseOutputStream);
            zipOutput.IsStreamOwner = false;
            /* * Higher compression level will cause higher usage of reources
            * If not necessary do not use highest level 9
            */
            zipOutput.SetLevel(8);
            byte[] buffer = new byte[4096];
            foreach (var emp in oModel.Employees)
            {
                foreach (var file in folder.GetFiles(emp.ID + "_" + suffix + "*"))
                {
                    ZipEntry entry = new ZipEntry(emp.Name + "_" + Path.GetFileName(file.FullName));
                    entry.DateTime = DateTime.Now;
                    //entry.Name = emp.Name(entry);
                    zipOutput.PutNextEntry(entry);

                    using (FileStream fs = System.IO.File.OpenRead(file.FullName))
                    {
                        int sourceBytes = 0;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                            zipOutput.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                } 
            }
            zipOutput.Finish();
            zipOutput.Close();

            /* Set position to 0 so that cient start reading of the stream from the begining */
            baseOutputStream.Position = 0;

            /* Set custom headers to force browser to download the file instad of trying to open it */
            return new FileStreamResult(baseOutputStream, "application/x-zip-compressed")
            {
                FileDownloadName = "AllEmployeeDocuments.zip"
            };

        }


        public FileResult DownloadEmployeeFiles(EmployeeDocumentDownloadModel oModel)
        {
            var context = System.Web.HttpContext.Current;
            //var folderPath = context.Server.MapPath(string.Format("~/{0}", id));
            var folderPath = context.Server.MapPath(CommonUsage.EmployeeDocumentsBasePath);

            DirectoryInfo folder = new DirectoryInfo(folderPath);
            var baseOutputStream = new MemoryStream();
            ZipOutputStream zipOutput = new ZipOutputStream(baseOutputStream);
            zipOutput.IsStreamOwner = false;
            /* * Higher compression level will cause higher usage of reources
            * If not necessary do not use highest level 9
            */
            zipOutput.SetLevel(8);
            byte[] buffer = new byte[4096];
            foreach (var file in folder.GetFiles(oModel.EmployeeID + "_*"))
            {
                ZipEntry entry = new ZipEntry(Path.GetFileName(file.FullName));
                entry.DateTime = DateTime.Now;
                zipOutput.PutNextEntry(entry);

                using (FileStream fs = System.IO.File.OpenRead(file.FullName))
                {
                    int sourceBytes = 0;
                    do
                    {
                        sourceBytes = fs.Read(buffer, 0, buffer.Length);
                        zipOutput.Write(buffer, 0, sourceBytes);
                    } while (sourceBytes > 0);
                }
            }

            zipOutput.Finish();
            zipOutput.Close();

            /* Set position to 0 so that cient start reading of the stream from the begining */
            baseOutputStream.Position = 0;

            /* Set custom headers to force browser to download the file instad of trying to open it */
            return new FileStreamResult(baseOutputStream, "application/x-zip-compressed")
            {
                FileDownloadName = oModel.EmployeeName + "_Document.Zip" 
            };

        }
        #endregion

    }

}




