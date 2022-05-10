using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Dapper;
using SMEnterprise.Repository;

namespace SMEnterprise.Models
{
    public class EditEvaluationModel
    {
        public int ID { get; set; }
        public List<ClassModel> Classes { get; set; }
        public int SessionID { get; set; }
        public int EvaluationSchemeID { get; set; }
        public string EvaluationSchemeName { get; set; }
    }
    public class EvaluationSchemeModel
    {
        public int EvaluationSchemeID { get; set; }
        public int SessionID { get; set; }
        public string EvaluationSchemeName { get; set; }
        public string Classes { get; set; }
        public string ClassIDs { get; set; }
        public int Status { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
        public int esCountUsed { get; set; }
    }
    public class EvaluationSchemePageModel
    {
        public int SessionID { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
        public List<EvaluationSchemeModel> Schemes { get; set; }
        public List<ClassModel> Classes { get; set; }
    }
    public class EvaluationTypePageModel
    {
       public List<EvaluationTypeModel> Evaluations { get; set; }
        public int SessionID { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
        public int EvaluationSchemeID { get; set; }
        public List<EvaluationSchemeModel> Schemes { get; set; }
    }
    public class EvaluationTypeModel
    {
        public int EvaluationTypeID { get; set; }
        public int EvaluationSchemeID { get; set; }
        public string EvaluationTypeName { get; set; }
        public int OpType { get; set; }
        public int SessionID { get; set; }
        public int SBranchID { get; set; }
        public int etCountUsed { get; set; }
    }
    public class SubEvaluationListModel
    {
        public int EvaluationID { get; set; }
        public List<EvaluationModel> SubEvaluations { get; set; }
        public List<EvaluationTypeModel> EvaluationTypes { get; set; }
    }
    public class EvaluationModel
    {
        // Add through PRo
        public decimal TotalMarks { get; set; }
        public decimal MarksObtained { get; set; }
        //
        public int MasterID { get; set; }
        public int EvaluationSchemeID { get; set; }
        public int SessionID { get; set; }
        public int SubEvaluationID { get; set; }
        public int EvaluationType { get; set; }
        public int SubEvals { get; set; }
        public int EvaluationID { get; set; }
        public string EvaluationName { get; set; }
        public decimal Weightage { get; set; }
        public int EvaluationMonth { get; set; }
        public int SBranchID { get; set; }
        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ResultDate { get; set; }
        public int evCountUsed { get; set; }
        public string EvaluationMonthName { get; set; }
        public int OpType { get; set; }
    }
    public class EvaluationPageModel
    {
        public List<EvaluationModel> Evaluations { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
        public int SessionID { get; set; }
        public List<EvaluationSchemeModel> Schemes { get; set; }
        public int EvaluationSchemeID { get; set; }
    }
    public class EducationLevelSectionModel
    {
        public int ELSectionID { get; set; }
        public int EducationLevelID { get; set; }
        public string SectionTitle { get; set; }
        public string SectionSubTitle { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
    }
    public class ExamModel
    {
        public int ExamID { get; set; }
        public int SubjectType { get; set; }
        public int EvaluationID { get; set; }
        public int ClassID { get; set; }
        public int GroupID { get; set; }
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }
        public DateTime ExamDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int SBranchID { get; set; }
        public int UserID { get; set; }
        public int IsLocked { get; set; }
        public int IsApplicable { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal PassMarks { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string EvaluationName { get; set; }
        public int MarkingScheme { get; set; }
        public int Sstatus { get; set; }
    }
    public class ExamPageModel
    {
        public int SessionID { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
        public List<ExamModel> Exams { get; set; }
        public List<EvaluationModel> Evaluations { get; set; }
        public List<ClassModel> Classes { get; set; }
        public List<GroupModel> Groups { get; set; }
        public int EvaluationID { get; set; }
        public int ClassID { get; set; }
        public int GroupID { get; set; }
        public int SBranchID { get; set; }
        public int UserID { get; set; }
        public DateTime CurrentDate { get; set; }
        public DataTable GetExamsDataTable()
        {

            DataTable dtExamDetails = new DataTable();
            dtExamDetails.SetTypeName("ExamDetails");
            dtExamDetails.Columns.Add("ExamID");
            dtExamDetails.Columns.Add("EvaluationID");
            dtExamDetails.Columns.Add("ClassID");
            dtExamDetails.Columns.Add("GroupID");
            dtExamDetails.Columns.Add("SubjectID");
            dtExamDetails.Columns.Add("ExamDate");
            dtExamDetails.Columns.Add("StartTime");
            dtExamDetails.Columns.Add("EndTime");
            dtExamDetails.Columns.Add("SBranchID");
            dtExamDetails.Columns.Add("UserID");
            dtExamDetails.Columns.Add("CreatedDate");
            dtExamDetails.Columns.Add("ModifiedDate");
            dtExamDetails.Columns.Add("IsApplicable");
            dtExamDetails.Columns.Add("MaxMarks");
            dtExamDetails.Columns.Add("PassMarks");
            dtExamDetails.Columns.Add("IsLocked");
            // For using Grading Subject
            dtExamDetails.Columns.Add("MarkingScheme");
            //
            foreach (ExamModel e in Exams)
            {
                DataRow dr = dtExamDetails.NewRow();
                dr["ExamID"] = e.ExamID;
                dr["EvaluationID"] = e.EvaluationID;
                dr["ClassID"] = e.ClassID;
                dr["GroupID"] = e.GroupID;
                dr["SubjectID"] = e.SubjectID;
                dr["ExamDate"] = e.ExamDate.ToString("yyyy-MM-dd");
                dr["StartTime"] = e.StartTime;
                dr["EndTime"] = e.EndTime;
                dr["SBranchID"] = e.SBranchID;
                dr["UserID"] = e.UserID;
                dr["CreatedDate"] = CurrentDate.ToString("yyyy-MM-dd");
                dr["ModifiedDate"] = CurrentDate.ToString("yyyy-MM-dd");
                dr["IsApplicable"] = e.IsApplicable;
                dr["MaxMarks"] = e.MaxMarks;
                dr["PassMarks"] = e.PassMarks;
                dr["IsLocked"] = e.IsLocked;
                // Use for Grading Subject 
                dr["MarkingScheme"] = e.MarkingScheme;
                //
                dtExamDetails.Rows.Add(dr);
            }

            return dtExamDetails;
        }
        public int MarkingScheme { get; set; }
    }

    // shishupal ExamDate sheet
    //17Nov2021
    public class StudentExamDatesheetModel
    {
        public SBranchModel Branch { get; set; }
        public int ClassID { get; set; }
        public int SBranchID { get; set; }
        public int SessionID { get; set; }
        public int EvaluationID { get; set; }
        public int SubjectID { get; set; }
        public int EvaluationSchemeID { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
        //public List<EvaluationSchemeModel> EvaluationSchemes { get; set; }
        public List<NameIDModel> EvaluationSchemes { get; set; }
        public List<NameIDModel> Evaluations { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public List<NameIDModel> Sections { get; set; }
        public List<SubjectModel> SubjectsE { get; set; }
        public List<ExamModel> Exams { get; set; }

        public List<ExamDateListModel> ExamList { get; set; }
        public string SubjectName { get; set; }
        public string SectionName { get; set; }
        public string ExamInTime { get; set; }
        public string ExamOutTime { get; set; }
    }

    public class ExamDateListModel
    {
        public int ExamID { get; set; }
        public DateTime ExamDate { get; set; }
        public string ExamDay { get; set; }
    }

    public class EvaluationTypePageModelExam
    {
        public List<NameIDModel> Evaluations { get; set; }
        public int SessionID { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
    }

    
}