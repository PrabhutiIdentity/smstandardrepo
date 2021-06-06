using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class BlackBoardAdminPageModel
    {
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int SessionID { get; set; }
        public int TeacherID { get; set; }
        public int SBranchID { get; set; }
        public int SubjectID { get; set; }
        public DateTime BBDate { get; set; }
        public List<BlackBoardModel> BlackBoards { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<NameIDModel> Teachers { get; set; }
        public List<NameIDModel> Subjects { get; set; }
    }
    public class BlackBoardModel
    {
        public string ImageBaseURL { get; set; }
        public string SubjectName { get; set; }
        public string UUID { get; set; }
        public string BlackBoardID { get; set; }
        public int TeacherID { get; set; }
        public int SectionID { get; set; }
        public int ClassID { get; set; }
        public int PeriodID { get; set; }
        public int SubjectID { get; set; }
        public string Title { get; set; }
        public DateTime BDate { get; set; }
        public int SBranchID { get; set; }
        public string Images { get; set; }
        public string TeacherName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string PeriodName { get; set; }
        public int OpType { get; set; }
        public List<BlackBoardImageModel> BBImages { get; set; }
    }
    public class StudentBlackBoardPageModel
    {
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public List<SubjectModel> Subjects { get; set; }
        public List<BlackBoardModel> BlackBoards { get; set; }
    }
    public class TeacherBlackBoardPageModel
    {
        public string ImageBaseURL { get; set; }
        public string UUID { get; set; }
        public string BlackBoardID { get; set; }
        public int TeacherID { get; set; }
        public int SBranchID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int PeriodID { get; set; }
        public int SubjectID { get; set; }
        public DateTime EntryDate { get; set; }
        public List<object> SelectionList { get; set; }
        public List<object> BlackBoardEntries { get; set; }
    }
    public class BlackBoardImageModel
    {
        public string UUID { get; set; }
        public string BlackBoardID { get; set; }
        public string BBMIID { get; set; }
        public string Photo { get; set; }
        public string Detail { get; set; }
        public int SBranchID { get; set; }

    }
}