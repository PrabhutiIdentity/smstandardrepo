using SMEnterprise.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using System.Threading.Tasks;

namespace SMEnterprise.Controllers
{
    public class ReceptionController : Controller
    {
        ReceptionData receptionData = new ReceptionData();
        [PermissionFilter]
        public ActionResult EnquiryDetail(string id=null)
        {
            int EnquiryID = CommonUsage.ConvertToInt(id);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            AdmissionEnquiryPageModel objData = receptionData.GetEnquiryDetails(EnquiryID, SBranchID);
            return View(objData);
        }
        [PermissionFilter]
        public ActionResult AddAssignee(AdmissionEnquiryMasterModel oModel)
        {
            oModel = receptionData.AddEnquiryAssignee(oModel);
            return RedirectToAction("Enquiries", "Reception");
        }
        [PermissionFilter]
        public ActionResult Dashboard()
        {
            int UserID = PermissionManager.GetLoggedInUser().UserID;
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            DateTime CDate = CommonUsage.GetCurrentDate();
            int UserType = PermissionManager.GetLoggedInUser().RoleID;
            ReceptionDashboardModel objModel = receptionData.GetDashboard(UserID, CDate,SBranchID, UserType);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult Enquiries(AdmissionEnquiryPageModel objData)
        {
            if (objData.StartDate.Year == 1)
            {
                objData.EndDate = CommonUsage.GetCurrentDate();
                objData.StartDate = objData.EndDate.AddDays(-7);
            }
            objData.UserID = PermissionManager.GetLoggedInUser().RoleID == 10 ? 0 : PermissionManager.GetLoggedInUser().UserID;
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objData = receptionData.GetEnquiries(objData);
            return View(objData);
        }
        [PermissionFilter]
        public ActionResult ConvertEnquiry(AdmissionEnquiryMasterModel oModel)
        {
            oModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            oModel = receptionData.UpdateEnquiryToAdmission(oModel);
            return RedirectToAction("Enquiries","Reception");
        }
        [PermissionFilter]
        public ActionResult EnquiryFollowups(string id = null)
        {
            int EnquiryID = CommonUsage.ConvertToInt(id);
            List<AdmissionEnquiryFollowupModel> objData = receptionData.GetEnquiryFollowups(EnquiryID);
            return Json(objData, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult Students()
        {
            return View();
        }

        [PermissionFilter]
        public ActionResult Employees()
        {
            return View();
        }
        [PermissionFilter]
        public JsonResult GetSearchedStudents(string query)
        {
            LibraryData libraryData = new LibraryData();
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            List<SelectDataModel> Students = libraryData.GetSearchedStudents(SBranchID, query);
            Select2ResultModel objresults = new Select2ResultModel();
            objresults.results = Students;
            return Json(objresults, JsonRequestBehavior.AllowGet); //return the serialised results list
        }
        [PermissionFilter]
        [ValidateInput(false)]
        public ActionResult UpdateAdmissionEnquiry(AdmissionEnquiryMasterModel objData)
        {
            int SlNo = objData.SrNo;
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.CreatedDate = CommonUsage.GetCurrentDate();
            objData = receptionData.UpdateEnquiry(objData);
            objData.SrNo = SlNo;
            return PartialView("_AdmissionEnquiryRowPartial", objData);
        }
        [PermissionFilter]
        [ValidateInput(false)]
        public ActionResult AddFollowup(AdmissionEnquiryFollowupModel objData)
        {
            objData.CreatedDate = CommonUsage.GetCurrentDate();
            int FollowupID = receptionData.InsertAdmissionEnquiryFollowup(objData);
            //return Json(FollowupID, JsonRequestBehavior.AllowGet);
            return RedirectToAction("Enquiries", "Reception");
        }
        #region Calendar
        [PermissionFilter]
        public async Task<ActionResult> CalendarAsync()
        {
            int year = CommonUsage.GetCurrentDate().Year;
            int month = CommonUsage.GetCurrentDate().Month;
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            CommonData objCommonData = new CommonData();
            EventCalendarModel model =await objCommonData.GetEventCalander(month, year, SBranchID, 0);
            return View(model);
        }
        [PermissionFilter]
        public async Task<ActionResult> GetEventCalenderAsync(string id = null, string id2 = null)
        {
            int year = CommonUsage.ConvertToInt(id2);
            int month = CommonUsage.ConvertToInt(id);

            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            CommonData objCommonData = new CommonData();
            EventCalendarModel model =await objCommonData.GetEventCalander(month, year, SBranchID, 1);

            return Json(model, JsonRequestBehavior.AllowGet);
        }
        #endregion
        [PermissionFilter]
        [ValidateInput(false)]
        public ActionResult InsertUpdateToDoItem(ToDoItemModel objData)
        {
            objData.CreatedDate = CommonUsage.GetCurrentDate();
            objData.UserID = PermissionManager.GetLoggedInUser().UserID;
            objData.UserType = PermissionManager.GetLoggedInUser().RoleID;
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int ToDoID = receptionData.InsertUpdateToDoItem(objData);
            return Json(ToDoID, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult GetStudentDetails(string id=null)
        {
            int StudentID = CommonUsage.ConvertToInt(id);
            ReceptionStudentSearchModel objModel= receptionData.GetStudentSearchData(StudentID);
            return PartialView("_StudentDetailPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetStudentTimeTable(string id = null)
        {
            int StudentID = CommonUsage.ConvertToInt(id);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            DateTime CreatedDate = CommonUsage.GetCurrentDate();
            TimeTablePageModel objModel = receptionData.GetStudentTimeTable(StudentID,CreatedDate,SBranchID);
            return PartialView("_StudentTimeTablePartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetStudentParents(string id = null)
        {
            int StudentID = CommonUsage.ConvertToInt(id);
            ParentModel objModel = receptionData.GetStudentParents(StudentID);
            return PartialView("_StudentParentsPartial", objModel);
        }
        [PermissionFilter]
        public JsonResult GetSearchedEmployees(string query)
        {
            LibraryData libraryData = new LibraryData();
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            List<SelectDataModel> Students = libraryData.GetSearchedEmployees(SBranchID, query);
            Select2ResultModel objresults = new Select2ResultModel();
            objresults.results = Students;
            return Json(objresults, JsonRequestBehavior.AllowGet); //return the serialised results list
        }

        [PermissionFilter]
        public ActionResult GetEmployeeDetails(string id = null)
        {
            int EmployeeID = CommonUsage.ConvertToInt(id);
            ReceptionEmployeeSearchModel objModel = receptionData.GetEmployeeSearchData(EmployeeID);
            return PartialView("_EmployeeDetailPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetTeacherSubjects(string id = null)
        {
            int EmployeeID = CommonUsage.ConvertToInt(id);
            List< Teacher_SubjectModel> objModel = receptionData.GetTeacherSubjects(EmployeeID);
            return PartialView("_EmployeeSubjectsPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetEmployeeTimeTable(string id = null)
        {
            AdminData adminData = new AdminData();
            int EmployeeID = CommonUsage.ConvertToInt(id);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            DateTime CreatedDate = CommonUsage.GetCurrentDate();
            TimeTablePageModel objModel = adminData.GetTeacherTimeTable(EmployeeID, CreatedDate, SBranchID);
            return PartialView("_EmployeeTimeTablePartial", objModel);
        }
    }
}