using SMEnterprise.Filters;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMEnterprise.Controllers
{
    public class LibraryController : Controller
    {
        LibraryData libraryData = new LibraryData();
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
        public ActionResult LibraryStructure()
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            LibraryListModel objModel = libraryData.GetLibraryDetails(SBranchID, 0, 0);
            objModel.Type = 0;
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateLibraryStructure(LibraryModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            LibraryListModel objModel = libraryData.InsertUpdateLibrary(objData);
            return RedirectToAction("LibraryStructure", "Library");
        }
        [PermissionFilter]
        public ActionResult GetLibraryStructure(string ID = null,string ID2=null)
        {
            int MasterID = CommonUsage.ConvertToInt(ID);
            int Type = CommonUsage.ConvertToInt(ID2);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            LibraryListModel objModel = libraryData.GetLibraryDetails(SBranchID, MasterID, Type);

            return PartialView("_LibraryDetailsPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateLibraryDetails(LibraryModel objData)
        {
            objData.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            LibraryListModel objModel = libraryData.InsertUpdateLibrary(objData);
            return PartialView("_LibraryDetailsPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult BookCategories()
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            List<NameIDModel> objModel = libraryData.GetBookCategories(SBranchID);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult UpdateBookCategory(NameIDModel objData)
        {
            objData.Extra1 = PermissionManager.GetLoggedInUser().SBranchID.ToString();
            int result = libraryData.InsertUpdateBookCategory(objData);
            return RedirectToAction("BookCategories", "Library");
        }
        [PermissionFilter]
        public ActionResult Books(BookListModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            objModel = libraryData.GetBooks(objModel);
            return View(objModel);
        }
        [PermissionFilter]
        public ActionResult GetBookDetails(string ID = null)
        {
            int BookID = CommonUsage.ConvertToInt(ID);
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            BookDetailModel objModel = libraryData.GetBookDetails(BookID,SBranchID);

            return PartialView("_BookDetailsPartial", objModel);
        }
        [PermissionFilter]
        public ActionResult GetLibraryDetailList(string ID = null)
        {
            int iID = CommonUsage.ConvertToInt(ID);
            IEnumerable<NameIDModel> model = libraryData.GetLibraryDetailList(iID);

            return PartialView("_SelectOptionsPartial", model);
        }
        [PermissionFilter]
        [HttpPost]
        public ActionResult UpdateBookDetails(BookModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if (objModel.PhotoFile!=null)
            {
                objModel.Image = objModel.PhotoFile.FileName.Replace(" ", "_");
            }
            BookDetailModel objData = libraryData.UpdateBookDetails(objModel);
            if(objModel.OpType==-1)
            {
                try
                {
                    System.IO.File.Delete(
                        Server.MapPath(CommonUsage.BookImageBasePath + objData.Book.BookID + "_" + objModel.OldImageName));

                }
                catch (Exception ex)
                {


                }
            }
            else if (objModel.PhotoFile!=null)
            {
                
                try
                {
                    System.IO.File.Delete(
                        Server.MapPath(CommonUsage.BookImageBasePath + objData.Book.BookID + "_" + objModel.OldImageName));

                }
                catch (Exception ex)
                {


                }
                if(!Directory.Exists(Server.MapPath(CommonUsage.BookImageBasePath)))
                {
                    Directory.CreateDirectory(Server.MapPath(CommonUsage.BookImageBasePath));
                }
                var path = Path.Combine(Server.MapPath(CommonUsage.BookImageBasePath),
                   objData.Book.BookID + "_" + objModel.Image);
                objModel.PhotoFile.SaveAs(path);

            }
            // BookDetailModel objModel = libraryData.GetBookDetails(BookID, SBranchID);

            return PartialView("_BookDetailsPartial", objData);
        }
        [PermissionFilter]
        [HttpPost]
        public ActionResult DeleteBookDetails(string ID=null)
        {
            int BookID = CommonUsage.ConvertToInt(ID);
            int res = libraryData.DeleteBook(BookID);
            
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [PermissionFilter]
        public ActionResult GetBookCopies(string ID = null)
        {
            int BookID = CommonUsage.ConvertToInt(ID);
            BookCopyListModel bookCopyListModel = new BookCopyListModel();
            bookCopyListModel.Copies = libraryData.GetBookCopies(BookID);
            bookCopyListModel.BookID = BookID;
            return PartialView("_BookCopiesPartial", bookCopyListModel);
        }
        [PermissionFilter]
        public ActionResult UpdateBookCopies(BookCopyModel objModel)
        {
            BookCopyListModel bookCopyListModel = new BookCopyListModel();
            bookCopyListModel.Copies = libraryData.InsertUpdateBookCopies(objModel);
            bookCopyListModel.BookID = objModel.BookID;
            return PartialView("_BookCopiesPartial", bookCopyListModel);
        }
        [PermissionFilter]
        public ActionResult LibraryRegister(LibraryRegisterListModel objModel)
        {
            objModel.SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            if(objModel.StartDate.Year==1)
            {
                objModel.BorrowerType = -1;
                objModel.StartDate = CommonUsage.GetCurrentDate().AddDays(-7);
                objModel.EndDate = CommonUsage.GetCurrentDate();
            }
            objModel.Status = -1;
            objModel = libraryData.GetLibraryRegister(objModel);
            return View(objModel);
        }
        public ActionResult IssueDetail()
        {
            int IssueID = 1;
            int SessionID = 1;
            LibraryRegisterModel libraryRegisterModel = new LibraryRegisterModel();
            libraryRegisterModel = libraryData.GetLibraryRegisterDetails(IssueID, SessionID, 1);
            return View(libraryRegisterModel);
        }
        [PermissionFilter]
        public ActionResult GetIssueRegisterDetails(string ID=null,string ID2=null)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int IssueID = CommonUsage.ConvertToInt(ID);
            int SessionID = CommonUsage.ConvertToInt(ID2);
            LibraryRegisterModel libraryRegisterModel = new LibraryRegisterModel();
            libraryRegisterModel = libraryData.GetLibraryRegisterDetails(IssueID, SessionID, SBranchID);
            libraryRegisterModel.SessionID = SessionID;
            return PartialView("_LibraryIssueDetailPartial", libraryRegisterModel);
        }
        public JsonResult GetSearchedStudents(string query)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            List<SelectDataModel> Students = libraryData.GetSearchedStudents(SBranchID, query);
            Select2ResultModel objresults = new Select2ResultModel();
            objresults.results = Students;
            return Json(objresults, JsonRequestBehavior.AllowGet); //return the serialised results list
        }
        public ActionResult GetStudentsOnSection(string id = null,string id2=null)
        {
            int SectionID = CommonUsage.ConvertToInt(id);
            int SessionID = CommonUsage.ConvertToInt(id2);
            IEnumerable<NameIDModel> objModel = libraryData.GetSessionSectionStudents(SessionID,SectionID);

            return Json(objModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBookOnBarCode(string id=null)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            BookCopyModel book = libraryData.GetBookByBarCode(id, SBranchID);
            return Json(book, JsonRequestBehavior.AllowGet); //return the serialised results list
        }
        public JsonResult GetBooksForIssue(string id = null)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            List<SelectDataModel> book = libraryData.GetBooksForIssue(id, SBranchID);
            Select2ResultModel objresults = new Select2ResultModel();
            objresults.results = book;
            return Json(objresults, JsonRequestBehavior.AllowGet); //return the serialised results list
        }
        [PermissionFilter]
        public ActionResult UpdateIssueRegisterDetails(LibraryRegisterModel objModel)
        {
            objModel.CreatedDate = CommonUsage.GetCurrentDate();
            objModel = libraryData.InsertUpdateLibraryRegister(objModel);
            return PartialView("_LibraryRegisterRowPartial", objModel);
        }
    }
}