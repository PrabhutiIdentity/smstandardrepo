using Dapper;
using SMEnterprise.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace SMEnterprise.Repository
{
    public class LibraryData
    {
        public LibraryListModel GetLibraryDetails(int SBranchID,int MasterID,int Type)
        {
            LibraryListModel objModel = new LibraryListModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@MasterID", MasterID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@Type", Type);
                objModel.List = con.Query<LibraryModel>("sp_GetLibraryDetails", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
            objModel.Type = Type+1;
            objModel.MasterID = MasterID;
            return objModel;
        }
        public LibraryListModel InsertUpdateLibrary(LibraryModel objData)
        {
            LibraryListModel objModel = new LibraryListModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", objData.ID);
                paramater.Add("@Name", objData.Name);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@Type", objData.Type);
                paramater.Add("@MasterID", objData.MasterID);
                paramater.Add("@Detail", objData.Detail);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OpType", objData.OpType);

                objModel.List = con.Query<LibraryModel>("sp_InsertUpdateLibrary", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
               
            }
            objModel.Type = objData.Type;
            objModel.MasterID = objData.MasterID;
            return objModel;
        }
        public List<NameIDModel> GetBookCategories(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<NameIDModel>("sp_GetBookCategories", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdateBookCategory(NameIDModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", objData.ID);
                paramater.Add("@Name", objData.Name);
                paramater.Add("@SBranchID", objData.Extra1);
                paramater.Add("@OpType", objData.Extra2);

                return con.Query<int>("sp_InsertUpdateBookCategory", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public BookListModel GetBooks(BookListModel model)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", model.SBranchID);
                paramater.Add("@ClassID", model.ClassID);
                paramater.Add("@CategoryID", model.CategoryID);

                using (var multi = con.QueryMultiple("sp_GetLibraryBooks", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    model.Books = multi.Read<BookModel>().ToList();
                    model.Categories = multi.Read<NameIDModel>().ToList();
                    model.Classes = multi.Read<NameIDModel>().ToList();
                }

            }
            return model;
        }
        public BookDetailModel GetBookDetails(int BookID,int SBranchID)
        {
            BookDetailModel model = new BookDetailModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@BookID", BookID);
                paramater.Add("@SBranchID", SBranchID);

                using (var multi = con.QueryMultiple("sp_GetBookDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    model.Book = multi.Read<BookModel>().SingleOrDefault();
                    model.Categories = multi.Read<NameIDModel>().ToList();
                    model.Classes = multi.Read<NameIDModel>().ToList();
                    model.Libraries = multi.Read<NameIDModel>().ToList();
                    model.Floors = multi.Read<NameIDModel>().ToList();
                    model.Blocks = multi.Read<NameIDModel>().ToList();
                    model.Bays = multi.Read<NameIDModel>().ToList();
                    model.LibraryID = multi.Read<int>().SingleOrDefault();
                    model.FloorID = multi.Read<int>().SingleOrDefault();
                    model.BlockID = multi.Read<int>().SingleOrDefault();
                    model.BayID = multi.Read<int>().SingleOrDefault();
                }
                if(model.Book==null)
                {
                    model.Book = new BookModel();
                }

            }
            return model;
        }
        public List<NameIDModel> GetLibraryDetailList(int ID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", ID);
                return con.Query<NameIDModel>("sp_GetLibraryDetailList", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int DeleteBook(int BookID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@BookID", BookID);
                return con.Query<int>("sp_DeleteBook", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public BookDetailModel UpdateBookDetails(BookModel objModel)
        {
            BookDetailModel model = new BookDetailModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@BookID", objModel.BookID);
                paramater.Add("@Title", objModel.Title);
                paramater.Add("@Publisher", objModel.Publisher);
                paramater.Add("@BayID", objModel.BayID);
                paramater.Add("@Description", objModel.Description);
                paramater.Add("@Copies", objModel.Copies);
                paramater.Add("@Status", objModel.Status);
                paramater.Add("@Price", objModel.Price);
                paramater.Add("@Author", objModel.Author);
                paramater.Add("@Image", objModel.Image);
                paramater.Add("@CategoryIDs", objModel.CategoryIDs);
                paramater.Add("@ClassesIDs", objModel.ClassesIDs);
                paramater.Add("@ISBN", objModel.ISBN);
                paramater.Add("@ISBN13", objModel.ISBN13);
                paramater.Add("@SBranchID", objModel.SBranchID); 
                paramater.Add("@BookCode", objModel.BookCode);
                paramater.Add("@IssueDays", objModel.IssueDays);
                paramater.Add("@OpType", objModel.OpType);

                using (var multi = con.QueryMultiple("sp_InsertUpdateBookDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    model.Book = multi.Read<BookModel>().SingleOrDefault();
                    model.Categories = multi.Read<NameIDModel>().ToList();
                    model.Classes = multi.Read<NameIDModel>().ToList();
                    model.Libraries = multi.Read<NameIDModel>().ToList();
                    model.Floors = multi.Read<NameIDModel>().ToList();
                    model.Blocks = multi.Read<NameIDModel>().ToList();
                    model.Bays = multi.Read<NameIDModel>().ToList();
                    model.LibraryID = multi.Read<int>().SingleOrDefault();
                    model.FloorID = multi.Read<int>().SingleOrDefault();
                    model.BlockID = multi.Read<int>().SingleOrDefault();
                    model.BayID = multi.Read<int>().SingleOrDefault();
                }
                if (model.Book == null)
                {
                    model.Book = new BookModel();
                }

            }
            return model;
        }
        public List<BookCopyModel> GetBookCopies(int BookID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@BookID", BookID);
                return con.Query<BookCopyModel>("sp_GetBookCopies", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public List<BookCopyModel> InsertUpdateBookCopies(BookCopyModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@CopyID", objModel.CopyID);
                paramater.Add("@BookID", objModel.BookID);
                paramater.Add("@BarCode", objModel.BarCode);
                paramater.Add("@RFID", objModel.RFID);
                paramater.Add("@AStatus", objModel.AStatus);
                paramater.Add("@Code", objModel.Code);
                paramater.Add("@OpType", objModel.OpType);
                return con.Query<BookCopyModel>("sp_InsertUpdateBookCopies", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public LibraryRegisterListModel GetLibraryRegister(LibraryRegisterListModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@LibraryID", objModel.LibraryID);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@StartDate", objModel.StartDate);
                paramater.Add("@EndDate", objModel.EndDate);
                paramater.Add("@Status", objModel.Status);
                paramater.Add("@BorrowerType", objModel.BorrowerType);

                using (var multi = con.QueryMultiple("sp_GetLibraryIssueRegister", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.IssueList = multi.Read<LibraryRegisterModel>().ToList();
                    objModel.Libraries = multi.Read<NameIDModel>().ToList();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.LibraryID = multi.Read<int>().SingleOrDefault();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public LibraryRegisterModel GetLibraryRegisterDetails(int IssueID,int SessionID,int SBranchID)
        {
            LibraryRegisterModel objModel = new LibraryRegisterModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@IssueID", IssueID);
                paramater.Add("@SessionID", SessionID);
                paramater.Add("@SBranchID", SBranchID);

                using (var multi = con.QueryMultiple("sp_GetLibraryIssueRegisterDetail", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<LibraryRegisterModel>().SingleOrDefault();
                    if (objModel == null)
                    {
                        objModel = new LibraryRegisterModel();
                        objModel.IssueDate = CommonUsage.GetCurrentDate();
                    }
                        objModel.IssuedBooks = multi.Read<BookCopyModel>().ToList();
                        objModel.Classes = multi.Read<NameIDModel>().ToList();
                        objModel.Teachers = multi.Read<NameIDModel>().ToList();
                }
            }
            return objModel;
        }
        public List<SelectDataModel> GetSearchedStudents(int SBranchID,string SearchText)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SearchText", SearchText);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<SelectDataModel>("sp_GetSearchStudentsForList", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }

        public List<SelectDataModel> GetSearchedEmployees(int SBranchID, string SearchText)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SearchText", SearchText);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<SelectDataModel>("sp_GetSearchEmployeeForList", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public List<NameIDModel> GetSessionSectionStudents(int SessionID,int SectionID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SessionID", SessionID);
                return con.Query<NameIDModel>("sp_GetLibrarySectionStudentList", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public BookCopyModel GetBookByBarCode(string BarCode,int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@BarCode", BarCode.Trim());
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<BookCopyModel>("sp_GetBookByBarCode", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public List<SelectDataModel> GetBooksForIssue(string Text, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Text", Text);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<SelectDataModel>("sp_GetBookForIssue", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public LibraryRegisterModel InsertUpdateLibraryRegister(LibraryRegisterModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@IssueID", objModel.IssueID);
                paramater.Add("@BorrowerType", objModel.BorrowerType);
                paramater.Add("@BorrowerID", objModel.BorrowerID);
                paramater.Add("@IssueDate", objModel.IssueDate);
                paramater.Add("@IssueDays", objModel.IssueDays);
                paramater.Add("@LibraryID", objModel.LibraryID);
                paramater.Add("@Remark", objModel.Remark);
                paramater.Add("@Status", objModel.Status);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@CreatedDate", objModel.CreatedDate);
                paramater.Add("@IssuedBooks", objModel.GetIssuedBooks());
                return con.Query<LibraryRegisterModel>("spn_InsertUpdateLibraryRegister", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
    }

}