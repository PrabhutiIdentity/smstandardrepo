using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMEnterprise.Models
{

    public class StudentOnlineExamSubmissionPageModel
    {
        public int OExamID { get; set; }
        public int SubmissionID { get; set; }
        public OnlineExamModel Exam { get; set; }
        public List<StudentOnlineExamSubmitModel> Submissions { get; set; }

        public List<QuestionBankModel> ExamQuestions { get; set; }
    }

    public class StudentOnlineExamSubmitModel
    {
        public int SubAnsID { get; set; }
        public string TeacherRemark { get; set; }
        public int TeacherID { get; set; }
        public string TeacherName { get; set; }
        public DateTime CheckDate { get; set; }
        public int Status { get; set; }
        public string StudentName { get; set; }
        public string SchoolUID { get; set; }
        public string StudentSID { get; set; }
        public int SubmissionID { get; set; }
        public int StudentID { get; set; }
        public int OExamID { get; set; }
        public int QuestionCount { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal MarksObtained { get; set; }
        public DateTime SubmissionDate { get; set; }
        public NameIDModel QuesAnswers { get; set; }
        public List<NameIDModel> Answers { get; set; }
        public DataTable GetAnswersDatatable()
        {
            DataTable dtSubjectOpted = new DataTable();
            dtSubjectOpted.SetTypeName("ut_Name_ID_Utility");
            dtSubjectOpted.Columns.Add("ID");
            dtSubjectOpted.Columns.Add("Name");
            dtSubjectOpted.Columns.Add("Extra1");
            dtSubjectOpted.Columns.Add("Extra2");
            dtSubjectOpted.Columns.Add("Extra3");
            if (Answers == null)
            {
                Answers = new List<NameIDModel>();
            }
            foreach (NameIDModel e in Answers)
            {
                DataRow dr = dtSubjectOpted.NewRow();
                dr["ID"] = e.ID;
                dr["Name"] = e.Name;
                dr["Extra1"] = e.Extra1;

                dtSubjectOpted.Rows.Add(dr);
            }

            return dtSubjectOpted;
        }



    }
    public class StudentOnlineExamListPageModel
    {
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public List<NameIDModel> Subjects { get; set; }
        public List<OnlineExamModel> Exams { get; set; }
    }
    public class OnlineExamEditModel
    {
        public StudentOnlineExamSubmitModel SubmitID { get; set; }
        public OnlineExamModel Exam { get; set; }
        public List<QuestionBankModel> ExamQuestions { get; set; }
        public List<QuestionBankModel> Questions { get; set; }
    }
    public class OnlineExamPageModel
    {
        public List<OnlineExamModel> Exams { get; set; }
        public List<NameIDModel> Subjects { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public int SessionID { get; set; }
        public int SBranchID { get; set; }
        public int ClassID { get; set; }
        public int GroupID { get; set; }
        public int SectionID { get; set; }
        public int TeacherID { get; set; }
        public int SubjectID { get; set; }
    }
    public class OnlineExamModel
    {
        public List<OnlineExamModel> Result { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int StudentID { get; set; }
        public int SubmissionStatus { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string TeacherRemark { get; set; }
        public DateTime CheckDate { get; set; }
        public int SubmissionID { get; set; }
        public decimal MarksObtained { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
        public int OExamID { get; set; }
        public DateTime OExamStartDate { get; set; }
        public DateTime OExamEndDate { get; set; }
        public int SessionID { get; set; }
        public string SessionName { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public int SectionID { get; set; }
        public string SectionName { get; set; }
        public int SubjectID { get; set; }
        public int GroupID { get; set; }
        public string SubjectName { get; set; }
        public int TeacherID { get; set; }
        public string TeacherName { get; set; }
        public int SubmissionCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        public string ExamTitle { get; set; }
        public string ExamDescription { get; set; }
        public int ExamPriority { get; set; }
        public int OnlineExamType { get; set; }
        public int QuestionCount { get; set; }
        public decimal TotalMarks { get; set; }
        public List<NameIDModel> Questions { get; set; }
        public DataTable GetQuestionsDatatable()
        {
            DataTable dtSubjectOpted = new DataTable();
            dtSubjectOpted.SetTypeName("ut_Name_ID_Utility");
            dtSubjectOpted.Columns.Add("ID");
            dtSubjectOpted.Columns.Add("Name");
            dtSubjectOpted.Columns.Add("Extra1");
            dtSubjectOpted.Columns.Add("Extra2");
            dtSubjectOpted.Columns.Add("Extra3");
            if (Questions == null)
            {
                Questions = new List<NameIDModel>();
            }
            foreach (NameIDModel e in Questions)
            {
                DataRow dr = dtSubjectOpted.NewRow();
                dr["ID"] = e.ID;
                dr["Name"] = e.Name;
                dr["Extra1"] = e.Extra1;
                dr["Extra2"] = e.Extra2;
                dr["Extra3"] = e.OpType;

                dtSubjectOpted.Rows.Add(dr);
            }

            return dtSubjectOpted;
        }
    }
    public class TeacherQuestionBankModel
    {
        public int TeacherID { get; set; }
        public int QuestionsBy { get; set; }
        public int ClassID { get; set; }
        public List<QuestionBankModel> Questions { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Subjects { get; set; }
        public int SubjectID { get; set; }
        public int SessionID { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public int GroupID { get; set; }
        public int SBranchID { get; set; }
    }



    public class QuestionBankModel
    {
        [AllowHtml]
        public string StudentAnswer { get; set; }
        public decimal MarksGiven { get; set; }
        public string TeacherRemark { get; set; }
        public int OXQID { get; set; }
        public int OXID { get; set; }
        public int Marks { get; set; }
        public int QuestionsBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int QuestionID { get; set; }
        public int TeacherID { get; set; }
        public string TeacherName { get; set; }
        public int ClassID { get; set; }
        public int GroupID { get; set; }
        //public int SubAnsID { get; set; }
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }
        public int ChapterID { get; set; }
        public int TopicID { get; set; }
        public int QuestionType { get; set; }
        public int Complexity { get; set; }
        [AllowHtml]
        public string QuestionText { get; set; }
        public string QuestionImage { get; set; }
        public HttpPostedFileBase QuestionImageFile { get; set; }
        [AllowHtml]
        public string Option1 { get; set; }
        public string Option1Image { get; set; }
        public HttpPostedFileBase Option1ImageFile { get; set; }
        [AllowHtml]
        public string Option2 { get; set; }
        public string Option2Image { get; set; }
        public HttpPostedFileBase Option2ImageFile { get; set; }
        [AllowHtml]
        public string Option3 { get; set; }
        public string Option3Image { get; set; }
        public HttpPostedFileBase Option3ImageFile { get; set; }
        [AllowHtml]
        public string Option4 { get; set; }
        public string Option4Image { get; set; }
        public HttpPostedFileBase Option4ImageFile { get; set; }
        [AllowHtml]
        public string Answer { get; set; }
        [AllowHtml]
        public string Explaination { get; set; }
        public string ExplainationImage { get; set; }
        public HttpPostedFileBase ExplainationImageFile { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
    }
}