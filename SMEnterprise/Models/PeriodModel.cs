using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class ParentAPITimeTableModel
    {
        public string UUID { get; set; }
        public int Days { get; set; }
        public int DayID { get; set; }
        public int StudentID { get; set; }
        public int TeacherID { get; set; }
        public int SBranchID { get; set; }
        public List<ParentAPITTSubstitutions> Substitutions { get; set; }
        public List<PeriodModel> Periods { get; set; }
        public List<TimeTableMergedClassModel> MergedClasses { get; set; }
    }
    public class ParentAPITTSubstitutions
    {
        public int PeriodID { get; set; }
        public int DayID { get; set; }
    }
    public class PeriodModel
    {
        public int PeriodID { get; set; }
        public int EducationLevelID { get; set; }
        public string EducationLevelName { get; set; }
        public int PeriodType { get; set; }
        public int DaysCount { get; set; }
        public string Name { get; set; }
        public string PeriodTypeName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Status { get; set; }
        public int OpType { get; set; }
        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }
        public string SubjectName { get; set; }
        public string TeacherName { get; set; }
        public int SubjectID { get; set; }
        public int TeacherID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public string ClassSection { get; set; }
        public int PeriodCount { get; set; }
    }
    public class PeriodTypeModel
    {
        public int PeriodTypeID { get; set; }
        public string PeriodTypeName { get; set; }
    }
    public class EducationLevelModel
    {
        public int ID { get; set; }
        public int SBranchID { get; set; }
        public int Days { get; set; }
        public int ShiftType { get; set; }
        public string Name { get; set; }
        public int Periods { get; set; }
        public int RepSections { get; set; }
        public int Classes { get; set; }
        public string PeriodStartTime { get; set; }
        public string PeriodEndTime { get; set; }
        public List<PeriodModel> PeriodList { get; set; }
        public List<PeriodTypeModel> PeriodTypeList { get; set; }
        public List<EducationLevelSectionModel> ReportSections { get; set; }
        public int OpType { get; set; }
    }
}