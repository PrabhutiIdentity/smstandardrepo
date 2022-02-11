using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class SLCCertificateDetails
    {
        public DateTime DOB { get; set; }
        public DateTime DOJ { get; set; }
        public string SchoolUID { get; set; }
        public string PSchoolName { get; set; }
        public string PSchoolMedium { get; set; }
        public string PClassName { get; set; }
        public string PSResult { get; set; }
        public string PSchoolCity { get; set; }
        public string PSChoolState { get; set; }
        public int OpType { get; set; }
        public int SBranchID { get; set; }
        public int CertID { get; set; }
        public int StudentID { get; set; }
        public int SessionID { get; set; }
        public string Stream { get; set; }
        public string DuesCleared { get; set; }
        public string Character { get; set; }
        public string Promotion { get; set; }
        public DateTime TCDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public string TCSLNo { get; set; }
        public ParentModel Parent { get; set; }
        public SBranchModel Branch { get; set; }
        public StudentModel Student { get; set; }
        public List<SessionModel> Sessions { get; set; }
        public DataTable GetStudentSLCSessionDetails()
        {

            DataTable dtAttandanceDetails = new DataTable();
            dtAttandanceDetails.SetTypeName("ut_StudentSLCSessionDetails");
            dtAttandanceDetails.Columns.Add("StudentSessionUID");
            dtAttandanceDetails.Columns.Add("DateOfPromotion");
            dtAttandanceDetails.Columns.Add("DateOfRemoval");
            dtAttandanceDetails.Columns.Add("ConductWork");
            dtAttandanceDetails.Columns.Add("Work");
            dtAttandanceDetails.Columns.Add("CauseOfRemoval");
            dtAttandanceDetails.Columns.Add("IsLeft");

            foreach (var e in Sessions)
            {
                DataRow dr = dtAttandanceDetails.NewRow();
                dr["StudentSessionUID"] = e.StudentSessionUID;
                dr["DateOfPromotion"] = e.DateOfPromotion.Year == 1 ? (DateTime?)null : e.DateOfPromotion;
                dr["DateOfRemoval"] = e.DateOfRemoval.Year == 1 ? (DateTime?)null : e.DateOfRemoval;
                dr["ConductWork"] = e.ConductWork;
                dr["Work"] = e.Work;
                dr["CauseOfRemoval"] = e.CauseOfRemoval;
                dr["IsLeft"] = e.IsLeft;

                dtAttandanceDetails.Rows.Add(dr);
            }

            return dtAttandanceDetails;
        }
    }
    public class TCDetailsModel
    {
        public int SBranchID { get; set; }
        public int SessionID { get; set; }
        public DateTime DOB { get; set; }
        public string StudentName { get; set; }
        public int StudentID { get; set; }
        public TCModel TCDetails { get; set; }
        public SBranchModel Branch { get; set; }
        public StudentModel StudentDetails { get; set; }
        public List<ClassModel> Classes { get; set; }
        //public List<SubjectModel> Subjects { get; set; }
        public List<string> Subjects { get; set; }

        public List<SessionModel> Sessions { get; set; }

    }
    public class TCListModel
    {
        public int SessionID { get; set; }
        public int SBranchID { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<TCModel> TCList { get; set; }
    }
    public class TCModel
    {
        public int TCID { get; set; }
        public int StudentID { get; set; }
        public int SessionID { get; set; }
        public int IsSCBLastExamPassed { get; set; }
        public string SCBLastExamName { get; set; }
        public string SCBLastExamResult { get; set; }
        public int FailedCount { get; set; }
        public int PromotedToClass { get; set; }
        public int WorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int IsNCC { get; set; }
        public string NCCDetails { get; set; }
        public string Games { get; set; }
        public string GeneralConduct { get; set; }
        public DateTime DateOfApplication { get; set; }
        public DateTime DateOfIssue { get; set; }
        public string ReasonForLeaving { get; set; }
        public string Remarks { get; set; }
        public string DuePaidMonth { get; set; }
        public string Concession { get; set; }
        public int OpType { get; set; }

        public string StudentName { get; set; }
        public string ClassName { get; set; }

    }
    public class AcadmicModel
    {
        public static string CalculateGradeShine(decimal totalscored, decimal totalmax = 100)
        {
            string Grade;
            if (totalscored != 0)
            {
                // perform the division only if count is different than 0,
                // otherwise we know that it will throw an exception 
                // so why even attempting it?
                decimal Num;
                Num = (totalscored * 100) / totalmax;
                //numbers = numbers * 100 / maxMarks;

                if (Num >= 80 && Num <= 100)
                {
                    Grade = "A";
                }
                else if (Num >= 64)
                {
                    Grade = "B";
                }
                else if (Num >= 49)
                {
                    Grade = "C";
                }
                else if (Num >= 35)
                {
                    Grade = "D";
                }

                else
                {
                    Grade = "--";
                }

            }
            else
            {
                Grade = "--";
            }
            return Grade;
        }
        public static string CalculateGradePointsShine(decimal numbers, decimal maxMarks = 100)
        {
            numbers = numbers * 100 / maxMarks;
            string Grade = "A";
            if (numbers >= 80)
            {
                Grade = "10";
            }
            else if (numbers >= 79)
            {
                Grade = "9";
            }
            else if (numbers >= 64)
            {
                Grade = "8";
            }
            else if (numbers >= 48)
            {
                Grade = "7";
            }
            else if (numbers >= 34)
            {
                Grade = "6";
            }
            else if (numbers >= 11)
            {
                Grade = "5";
            }

            else
            {
                Grade = "--";
            }
            return Grade;
        }
        public static string CalculateGrade(decimal numbers, decimal maxMarks = 100)
        {
            if (numbers != 0)
            {
                // perform the division only if count is different than 0,
                // otherwise we know that it will throw an exception 
                // so why even attempting it?
                numbers = numbers * 100 / maxMarks;
            }
            else
            {

            }
            //numbers = numbers * 100 / maxMarks;
            string Grade = "A1";
            if (numbers >= 91)
            {
                Grade = "A1";
            }
            else if (numbers >= 81)
            {
                Grade = "A2";
            }
            else if (numbers >= 71)
            {
                Grade = "B1";
            }
            else if (numbers >= 61)
            {
                Grade = "B2";
            }
            else if (numbers >= 51)
            {
                Grade = "C1";
            }
            else if (numbers >= 41)
            {
                Grade = "C2";
            }
            else if (numbers >= 33)
            {
                Grade = "D";
            }
            else if (numbers >= 21)
            {
                Grade = "E1";
            }
            else
            {
                Grade = "E2";
            }
            return Grade;
        }
        public static string CalculateGradePoints(decimal numbers, decimal maxMarks = 100)
        {
            numbers = numbers * 100 / maxMarks;
            string Grade = "A1";
            if (numbers >= 91)
            {
                Grade = "10";
            }
            else if (numbers >= 81)
            {
                Grade = "9";
            }
            else if (numbers >= 71)
            {
                Grade = "8";
            }
            else if (numbers >= 61)
            {
                Grade = "7";
            }
            else if (numbers >= 51)
            {
                Grade = "6";
            }
            else if (numbers >= 41)
            {
                Grade = "5";
            }
            else if (numbers >= 33)
            {
                Grade = "4";
            }
            else if (numbers >= 21)
            {
                Grade = "--";
            }
            else
            {
                Grade = "--";
            }
            return Grade;
        }

        public static string CalculateGradeDivision(decimal numbers, decimal maxMarks = 100)
        {
            if (numbers != 0)
            {
                // perform the division only if count is different than 0,
                // otherwise we know that it will throw an exception 
                // so why even attempting it?
                numbers = numbers * 100 / maxMarks;
            }
            else
            {

            }

            string Division = "First";
            if (numbers >= 75)
            {
                Division = "Distinction";
            }
            else if (numbers >= 60)
            {
                Division = "First";
            }
            else if (numbers >= 45)
            {
                Division = "Second";
            }
            else if (numbers >= 35)
            {
                Division = "Third";
            }

            else
            {
                Division = "-";
            }
            return Division;
        }
        public static string CalculateGradeDivisionShine(decimal numbers, decimal maxMarks = 100)
        {
            if (numbers != 0)
            {
                // perform the division only if count is different than 0,
                // otherwise we know that it will throw an exception 
                // so why even attempting it?
                numbers = numbers * 100 / maxMarks;
            }
            else
            {

            }

            string Division = "Outstanding";

            if (numbers >= 80)
            {
                Division = "Outstanding";
            }
            else if (numbers >= 64)
            {
                Division = "Very Good";
            }
            else if (numbers >= 49)
            {
                Division = "Good";
            }
            else if (numbers >= 35)
            {
                Division = "Average";
            }
            else
            {
                Division = "Below Average";
            }
            return Division;
        }

    }

    public class AssignmentPageModel
    {
        public int SType { get; set; }
        public int SBranchID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime EndDate { get; set; }
        public string UUID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int SubjectID { get; set; }
        public int ChapterID { get; set; }
        public int TeacherID { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> Teachers { get; set; }
        public List<NameIDModel> Subjects { get; set; }
        public List<NameIDModel> Chapters { get; set; }
        public List<AssignmentModel> Assignments { get; set; }
    }
    public class AssignmentModel
    {
        public string EDetail { get; set; }
        public string UUID { get; set; }
        public int OpType { get; set; }
        public DateTime OperationDate { get; set; }
        public int Submissions { get; set; }
        public string TopicName { get; set; }
        private int iD;
        private int teacherID;
        private int classID;
        private int sectionID;
        private int subjectID;
        private int chapterID;
        private int topicID;
        private DateTime startDate;
        private DateTime endDate;
        private int gracedDayes;
        private int taskTypes;
        private string title;
        public string SubjectName { get; set; }
        public string TeacherName { get; set; }
        private string details;
        private string attachments;
        public string StudentName { get; set; }
        private string sendMail;
        public int Status { get; set; }
        public decimal Marks { get; set; }
        public string Grade { get; set; }
        public string StatusText { get; set; }
        public string StatusColor { get; set; }
        public int SubmissionStatus { get; set; }
        private DateTime createdOn;

        public int ID
        {
            get
            {
                return iD;
            }

            set
            {
                iD = value;
            }
        }

        public int TeacherID
        {
            get
            {
                return teacherID;
            }

            set
            {
                teacherID = value;
            }
        }

        public int ClassID
        {
            get
            {
                return classID;
            }

            set
            {
                classID = value;
            }
        }

        public int SectionID
        {
            get
            {
                return sectionID;
            }

            set
            {
                sectionID = value;
            }
        }

        public int SubjectID
        {
            get
            {
                return subjectID;
            }

            set
            {
                subjectID = value;
            }
        }

        public int ChapterID
        {
            get
            {
                return chapterID;
            }

            set
            {
                chapterID = value;
            }
        }

        public int TopicID
        {
            get
            {
                return topicID;
            }

            set
            {
                topicID = value;
            }
        }

        public DateTime StartDate
        {
            get
            {
                return startDate;
            }

            set
            {
                startDate = value;
            }
        }

        public DateTime EndDate
        {
            get
            {
                return endDate;
            }

            set
            {
                endDate = value;
            }
        }

        public int GracedDays
        {
            get
            {
                return gracedDayes;
            }

            set
            {
                gracedDayes = value;
            }
        }

        public int TaskType
        {
            get
            {
                return taskTypes;
            }

            set
            {
                taskTypes = value;
            }
        }

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
            }
        }

        public string Detail
        {
            get
            {
                return details;
            }

            set
            {
                details = value;
            }
        }

        public string Attachments
        {
            get
            {
                return attachments;
            }

            set
            {
                attachments = value;
            }
        }

        public string SendMail
        {
            get
            {
                return sendMail;
            }

            set
            {
                sendMail = value;
            }
        }

        public DateTime CreatedOn
        {
            get
            {
                return createdOn;
            }

            set
            {
                createdOn = value;
            }
        }
    }
    public class AssignmentSubmissionModel
    {
        public string UUID { get; set; }
        public int AssResponseID { get; set; }
        public int AssignmentID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public string Photo { get; set; }
        public string Grade { get; set; }
        public DateTime SubmissionDate { get; set; }
        public int Status { get; set; }
        public decimal Marks { get; set; }
        public string StatusText { get; set; }
        public string StatusColor { get; set; }
        public string Description { get; set; }
        public string Reason { get; set; }
        public string Comments { get; set; }
        public string Attachments { get; set; }
        public int Gender { get; set; }
        public string BasePath { get; set; }

    }
    public class AssignmentSubmissionListPage
    {
        public string BasePath { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string SubjectName { get; set; }
        public string ChapterName { get; set; }
        public string TopicName { get; set; }
        public string AssignmentTitle { get; set; }
        public List<AssignmentSubmissionModel> Submissions { get; set; }

    }
    public class ExamDateNotice
    {
        public string SessionName { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public int SectionID { get; set; }
        public string SectionName { get; set; }
        public int ExamID { get; set; }
        public int SubjectType { get; set; }
        public int SubjectID { get; set; }
        public int MainSubID { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public int IsOptionalSubject { get; set; }
        public int EvaluationID { get; set; }
        public string EvaluationName { get; set; }
        // using for branch logo
        public int Gender { get; set; }
        public string Photo { get; set; }
    }
    public class AdmitCardListModel
    {
        public SBranchModel Branch { get; set; }
        public List<ClassModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
        public List<EvaluationModel> Evaluations { get; set; }
        public List<ExamModel> Exams { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int SessionID { get; set; }
        public int EvaluationID { get; set; }
        public int SBranchID { get; set; }
        public DateTime ExamMonth { get; set; }
        public DateTime ExamYear { get; set; }
        public List<AdmitCardModel> AdmitCards { get; set; }
        public ExamDateNotice ExamDates { get; set; }
        public DateTime SelectedDate { get; set; }
    }
    public class AdmitCardModel
    {
        public int StudentID { get; set; }
        public string HouseName { get; set; }
        public string MobileNo { get; set; }
        public DateTime DOB { get; set; }
        public string FatherName { get; set; }
        public string SessionName { get; set; }
        public string EvaluationName { get; set; }
        public string MotherName { get; set; }
        public string Name { get; set; }
        public string SchoolUID { get; set; }
        public string StudentSID { get; set; }
        public string RollNo { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public int Gender { get; set; }
        public string Photo { get; set; }
        public int ExamID { get; set; }
        public int SubjectType { get; set; }
        public int SubjectID { get; set; }
        public int MainSubID { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public int IsOptionalSubject { get; set; }
        public int EvaluationID { get; set; }

    }
    public class TeacherResultPageModel
    {
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int SubjectID { get; set; }
        public int TeacherID { get; set; }
        public int EvaluationID { get; set; }
        public int SessionID { get; set; }
        public int SBranchID { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> Subjects { get; set; }
        public List<NameIDModel> Evaluations { get; set; }
        public List<ExamResultDetailModel> ExamResults { get; set; }
        public int IsLocked { get; set; }
        public int MarkingScheme { get; set; }
        public DataTable GetResultDetailsDataTable()
        {

            DataTable dtAttandanceDetails = new DataTable();
            dtAttandanceDetails.SetTypeName("ExamResultDetails");
            dtAttandanceDetails.Columns.Add("ResultID");
            dtAttandanceDetails.Columns.Add("ExamID");
            dtAttandanceDetails.Columns.Add("StudentID");
            dtAttandanceDetails.Columns.Add("MarksScored");
            dtAttandanceDetails.Columns.Add("Grade");
            dtAttandanceDetails.Columns.Add("Status");
            dtAttandanceDetails.Columns.Add("GradePoints");
            //// For Using Grading
            //dtAttandanceDetails.Columns.Add("MarksScored");
            ////
            foreach (ExamResultDetailModel e in ExamResults)
            {
                DataRow dr = dtAttandanceDetails.NewRow();
                dr["ResultID"] = e.ResultID;
                dr["ExamID"] = e.ExamID;
                dr["StudentID"] = e.StudentID;
                dr["MarksScored"] = e.MarksScored;
                dr["Grade"] = e.Grade;
                dr["Status"] = e.Status;
                dr["GradePoints"] = e.GradePoints;
                // For Using Grading
                //dr["MarksScored"] = e.MarksScored;
                //

                dtAttandanceDetails.Rows.Add(dr);
            }

            return dtAttandanceDetails;
        }
    }
    public class ExamResultDetailModel
    {
        public int MasterID { get; set; }
        public string StudentName { get; set; }
        public string RollNo { get; set; }
        public string StudentSID { get; set; }
        public string SchoolUID { get; set; }

        public decimal etMarksScored { get; set; }
        public decimal etMaxMarks { get; set; }
        public decimal rMarksScored { get; set; }
        public decimal rMaxMarks { get; set; }
        public int MaxMarks { get; set; }
        public int PassMarks { get; set; }
        public int Gender { get; set; }
        public string Photo { get; set; }
        public int ResultID { get; set; }
        public int ExamID { get; set; }
        public int StudentID { get; set; }
        public decimal MarksScored { get; set; }
        public decimal wMarksScored { get; set; }
        public decimal wMaxMarks { get; set; }
        public string Grade { get; set; }
        public int GradePoints { get; set; }
        public int Status { get; set; }
        public int SubjectType { get; set; }

        public int EvaluationID { get; set; }
        public int SubjectID { get; set; }
        public int MainSubID { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public int IsOptionalSubject { get; set; }
        public int IsApplicable { get; set; }
        public int MarkingScheme { get; set; }

    }
    public class StudentPerformanceListModel
    {
        public int StudentID { get; set; }
        public int SessionID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int EvaluationID { get; set; }
        public int TeacherID { get; set; }
        public int SBranchID { get; set; }
        public int TotalParameters { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> Evaluations { get; set; }
        public List<NameIDModel> SubEvaluations { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
        public List<StudentPerformanceModel> StudentPerformances { get; set; }
    }
    public class StudentPerformanceModel
    {
        public int StudentSessionUID { get; set; }
        public int StudentID { get; set; }
        public string StudentSID { get; set; }
        public string RollNo { get; set; }
        public int Gender { get; set; }
        public string Name { get; set; }
        public int IsExist { get; set; }
        public string Photo { get; set; }
        public decimal CGPA { get; set; }
    }
    public class StudentResultScrutinyListModel
    {
        public SBranchModel Branch { get; set; }
        public int SessionID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int EvaluationID { get; set; }
        public int SubEvaluationID { get; set; }
        public int SBranchID { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<NameIDModel> SubjectTypes { get; set; }
        public List<NameIDModel> Subjects { get; set; }
        public List<SubjectModel> Subjects2 { get; set; }
        public List<ExamResultDetailModel> Result { get; set; }
        public List<NameIDModel> Evaluations { get; set; }
        public List<NameIDModel> SubEvaluations { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
        public List<StudentModel> Students { get; set; }
        public List<NameIDModel> EvaluationSubjectTotals { get; set; }
        public List<StudentResultScrutinyModel> StudentResult { get; set; }
        public List<StudentResultCheckModel> StudentResult2 { get; set; }
        public List<ExamModel> Exams { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string SessionName { get; set; }
        public string EvaluationName { get; set; }
        public decimal CGPA { get; set; }
        public int TotalParameter { get; set; }
        public List<EvaluationTypeModel> EvaluationTypes { get; set; }

    }
    public class StudentResultCheckModel
    {
        public int SubjectID { get; set; }
        public int EvaluationID { get; set; }
        public decimal MaxMarks { get; set; }
        public int StudentID { get; set; }
        public decimal MarksScored { get; set; }
        public int SubjectType { get; set; }
        public int MainSubID { get; set; }
    }
    public class StudentResultScrutinyModel
    {
        public int StudentSessionUID { get; set; }
        public int StudentID { get; set; }
        public string StudentSID { get; set; }
        public string RollNo { get; set; }
        public string SchoolUID { get; set; }
        public int Gender { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public decimal CGPA { get; set; }
        public string ResultDetails { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
    }
    public class PerformanceParameterModel
    {
        public int ParamValID { get; set; }
        public int ParamID { get; set; }
        public int MainParamID { get; set; }
        public string Code { get; set; }
        public string ParamName { get; set; }
        public string ParamDesc { get; set; }
        public int ParamType { get; set; }
        public int EvaluationID { get; set; }
        public int StudentSessionID { get; set; }
        public string ParamKey { get; set; }
        public string ParamValue { get; set; }
        public string Grade { get; set; }
        public string DefaultVal { get; set; }
        public int Subs { get; set; }
    }
    public class ParentStudentPerformancePage
    {
        public List<EvaluationModel> Evaluations { get; set; }
        public List<NameIDModel> Exams { get; set; }
        public StudentModel Student { get; set; }
        public string ClassName { get; set; }
        public int StudentID { get; set; }
        public int StudentSessionUID { get; set; }
        public int SessionID { get; set; }
    }
    public class TotalResultModel
    {
        public List<SubjectModel> Subjects { get; set; }
        public List<EvaluationModel> Evaluations { get; set; }
        public List<ExamModel> Exams { get; set; }
        public List<ExamResultDetailModel> Result { get; set; }

        public StudentModel Student { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public SchoolSessionModel Session { get; set; }
    }
    public class StudentPerformanceResultModel
    {
        public int SBranchID { get; set; }
        public int StudentID { get; set; }
        public int SessionID { get; set; }
        public int StudentSessionUID { get; set; }
        public int EvaluationID { get; set; }
        public string SessionName { get; set; }
        public SBranchModel SBranchDetails { get; set; }
        public StudentModel Student { get; set; }
        public List<NameIDModel> SubjectTypes { get; set; }
        public List<SubSubjectTypeModel> SubjectType { get; set; }
        // public List<SubSubjectTypeModel> SubjectTypes { get; set; }
        public List<ExamModel> Exams { get; set; }
        public List<SubjectModel> Subjects { get; set; }
        public List<EvaluationModel> MainEvaluations { get; set; }
        public List<EvaluationModel> SubEvaluations { get; set; }
        public List<ExamResultDetailModel> Result { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string EvaluationName { get; set; }
        public List<EvaluationTypeModel> EvaluationTypes { get; set; }
        public DateTime ResultDate { get; set; }
        public List<PerformanceParameterModel> PerformanceParameters { get; set; }
    }
    public class PerformanceParameterDetailModel
    {
        public int SBranchID { get; set; }
        public int StudentID { get; set; }
        public int SessionID { get; set; }
        public int StudentSessionUID { get; set; }
        public int EvaluationID { get; set; }
        public string SessionName { get; set; }
        public SBranchModel SBranchDetails { get; set; }
        public List<PerformanceParameterModel> PerformanceParameters { get; set; }
        public List<EvaluationModel> MainEvaluations { get; set; }
        public List<EvaluationModel> SubEvaluations { get; set; }
        public List<ExamResultDetailModel> Result { get; set; }
        public List<EducationLevelSectionModel> ReportSections { get; set; }
        public List<NameIDModel> StudentSessions { get; set; }
        public List<NameIDModel> SubjectTypes { get; set; }
        public List<SubSubjectTypeModel> SubjectType { get; set; }
        public List<SubjectModel> Subjects { get; set; }
        public StudentModel Student { get; set; }
        public List<ExamModel> Exams { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string EvaluationName { get; set; }
        public decimal CGPA { get; set; }
        public int TotalParameter { get; set; }
        public int FilledParameter { get; set; }
        public List<EvaluationTypeModel> EvaluationTypes { get; set; }

        public DataTable GetPerformanceValuesDatatable()
        {

            DataTable dtPerformanceDetails = new DataTable();
            dtPerformanceDetails.SetTypeName("ut_PerformanceParameterValues");
            dtPerformanceDetails.Columns.Add("ParamValID");
            dtPerformanceDetails.Columns.Add("EvaluationID");
            dtPerformanceDetails.Columns.Add("StudentSessionID");
            dtPerformanceDetails.Columns.Add("ParamID");
            dtPerformanceDetails.Columns.Add("ParamKey");
            dtPerformanceDetails.Columns.Add("ParamValue");
            dtPerformanceDetails.Columns.Add("Grade");

            foreach (PerformanceParameterModel e in PerformanceParameters)
            {
                DataRow dr = dtPerformanceDetails.NewRow();
                dr["ParamValID"] = e.ParamValID;
                dr["EvaluationID"] = e.EvaluationID;
                dr["StudentSessionID"] = e.StudentSessionID;
                dr["ParamID"] = e.ParamID;
                dr["ParamKey"] = e.ParamKey;
                dr["ParamValue"] = e.ParamValue;
                dr["Grade"] = e.Grade;

                dtPerformanceDetails.Rows.Add(dr);
            }

            return dtPerformanceDetails;
        }
    }
}