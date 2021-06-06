using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class LibraryModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int MasterID { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
        public string Detail { get; set; }
        public int Count { get; set; }
    }
    public class LibraryListModel
    {
        public int MasterID { get; set; }
        public int Type { get; set; }
        public List<LibraryModel> List { get; set; }
    }
    public class BookModel
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Copies { get; set; }
        public int Status { get; set; }
        public decimal Price { get; set; }
        public string Publisher { get; set; }
        public string Author { get; set; }
        public int BayID { get; set; }
        public string Image { get; set; }
        public string CategoryIDs { get; set; }
        public string ClassesIDs { get; set; }
        public string ISBN { get; set; }
        public string ISBN13 { get; set; }
        public int SBranchID { get; set; }
        public string BookCode { get; set; }
        public int OpType { get; set; }
        public int Issued { get; set; }
        public int IssueDays { get; set; }

        public HttpPostedFileBase PhotoFile { get; set; }
        public string BayName { get; set; }
        public string CategoryNames { get; set; }
        public string Classes { get; set; }
        public string OldImageName { get; set; }
    }
    public class BookDetailModel
    {
        public int ClassID { get; set; }
        public int CategoryID { get; set; }
        public int LibraryID { get; set; }
        public int FloorID { get; set; }
        public int BlockID { get; set; }
        public int BayID { get; set; }
        public BookModel Book { get; set; }
        public List<NameIDModel> Categories { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Libraries { get; set; }
        public List<NameIDModel> Floors { get; set; }
        public List<NameIDModel> Blocks { get; set; }
        public List<NameIDModel> Bays { get; set; }
    }
    public class BookListModel
    {
        public int ClassID { get; set; }
        public int SBranchID { get; set; }
        public int CategoryID { get; set; }
        public List<BookModel> Books { get; set; }
        public List<NameIDModel> Categories { get; set; }
        public List<NameIDModel> Classes { get; set; }
    }
    public class BookCopyModel
    {
        public int BookIssueID { get; set; }
        public int CopyID { get; set; }
        public int BookID { get; set; }
        public string BarCode { get; set; }
        public string RFID { get; set; }
        public int AStatus { get; set; }
        public int OpType { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public int IssueDays { get; set; }
        public DateTime IssueDate { get; set; }
    }
    public class BookCopyListModel
    {
        public int BookID { get; set; }
        public List<BookCopyModel> Copies { get; set; }
    }
    public class LibraryRegisterListModel
    {
        public int LibraryID { get; set; }
        public int BorrowerType { get; set; }
        public int Status { get; set; }
        public int SBranchID { get; set; }
        public int SessionID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<NameIDModel> Libraries { get; set; }
        public List<LibraryRegisterModel> IssueList { get; set; }
    }
    public class LibraryRegisterModel
    {
        public int IssueID { get; set; }
        public int BorrowerType { get; set; }
        public string BorrowerName { get; set; }
        public int BorrowerID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public DateTime IssueDate { get; set; }
        public int IssueDays { get; set; }
        public int LibraryID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Remark { get; set; }
        public int OpType { get; set; }
        public int ReturnedBooks { get; set; }
        public int SessionID { get; set; }
        public int Books { get; set; }
        public int Status { get; set; }
        public List<BookCopyModel> IssuedBooks { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> Students { get; set; }
        public List<NameIDModel> Teachers { get; set; }
        public DataTable GetIssuedBooks()
        {

            DataTable dtIssuedBooks = new DataTable();
            dtIssuedBooks.SetTypeName("ut_LibraryIssueBooks");
            dtIssuedBooks.Columns.Add("BookIssueID");
            dtIssuedBooks.Columns.Add("BookID");
            dtIssuedBooks.Columns.Add("CopyID");
            dtIssuedBooks.Columns.Add("IssueDays");
            dtIssuedBooks.Columns.Add("IssueDate");
            dtIssuedBooks.Columns.Add("Remark");
            dtIssuedBooks.Columns.Add("LibraryIssueID");
            dtIssuedBooks.Columns.Add("Status");
            dtIssuedBooks.Columns.Add("OpType");
            dtIssuedBooks.Columns.Add("Extra1");
            dtIssuedBooks.Columns.Add("Extra2");

            if (IssuedBooks == null)
            {
                IssuedBooks = new List<BookCopyModel>();
            }

            foreach (BookCopyModel e in IssuedBooks)
            {
                DataRow dr = dtIssuedBooks.NewRow();
                dr["BookIssueID"] = e.BookIssueID;
                dr["BookID"] = e.BookID;
                dr["CopyID"] = e.CopyID;
                dr["IssueDays"] = e.IssueDays;
                dr["IssueDate"] = e.IssueDate;
                dr["LibraryIssueID"] = IssueID;
                dr["Status"] = e.AStatus;
                dr["OpType"] = e.OpType;
                dtIssuedBooks.Rows.Add(dr);
            }
            return dtIssuedBooks;
        }
    }
    public class LibraryIssueBooksModel
    {
        public int BookIssueID { get; set; }
        public int LibraryIssueID { get; set; }
        public int BookID { get; set; }
        public int CopyID { get; set; }
        public int IssueDays { get; set; }
        public DateTime IssueDate { get; set; }
        public int Status { get; set; }
        public string Remark { get; set; }
        public int OpType { get; set; }
    }
    public class SelectDataModel
    {
        public string id { get; set; }
        public string text { get; set; }
    }
    public class Select2ResultModel
    {
        public List<SelectDataModel> results { get; set; }
    }
}