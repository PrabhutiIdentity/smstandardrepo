using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class TimeTableModel
    {
        public string[] DayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        public int Days { get; set; }
        public List<PeriodModel> Periods { get; set; }
        public List<SubjectModel> Subjects { get; set; }
        public List<TimeTablePeriodModel> Lactures { get; set; }
        public List<TeacherSubstitutionModel> Substitutions { get; set; }
        public List<TimeTableMergedClassModel> MergedClasses { get; set; }
        public int SectionID { get; set; }
        public int ClassID { get; set; }
        public int TeacherID { get; set; }
        public int EducationLevelID { get; set; }

    }
    public class TeacherSubstitutionEditModel
    {
        public List<TeacherSubstitutionModel> Substitutions { get; set; }
        public List<SubjectModel> Subjects { get; set; }
        public List<EmployeeModel> Teachers { get; set; }
        public string UUID { get; set; }
        public int TeacherID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int PeriodID { get; set; }
        public int DayID { get; set; }
        public int EducationLevelID { get; set; }
        public DateTime CurrentDate { get; set; }
        public int SBranchID { get; set; }
        public int SubjectID { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public int OpType { get; set; }
    }
    public class TimeTablePeriodModel
    {
        public int PeriodID { get; set; }
        public int EducationLevelID { get; set; }
        public int DaysCount { get; set; }
        public string EducationLevel { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public int SectionID { get; set; }
        public string SectionName { get; set; }

        public int SubjectID1 { get; set; }
        public int TeacherID1 { get; set; }
        public string TeacherName1 { get; set; }

        public int SubjectID2 { get; set; }
        public int TeacherID2 { get; set; }
        public string TeacherName2 { get; set; }

        public int SubjectID3 { get; set; }
        public int TeacherID3 { get; set; }
        public string TeacherName3 { get; set; }

        public int SubjectID4 { get; set; }
        public int TeacherID4 { get; set; }
        public string TeacherName4 { get; set; }

        public int SubjectID5 { get; set; }
        public int TeacherID5 { get; set; }
        public string TeacherName5 { get; set; }

        public int SubjectID6 { get; set; }
        public int TeacherID6 { get; set; }
        public string TeacherName6 { get; set; }
    }
    public class TimeTablePageModel
    {
        public int EducationLevelID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int TeacherID { get; set; }
        public TimeTableModel TimeTable { get; set; }
        public List<EducationLevelModel> EducationLevels { get; set; }
        public List<ClassModel> Classes { get; set; }
        public List<SectionModel> Sections { get; set; }
        public List<EmployeeModel> Teachers { get; set; }
    }
    public class TeacherSubstitutionModel
    {
        public string UUID { get; set; }
        public int SendSMS { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
        public DateTime CurrentDate { get; set; }
        public int EducationLevelID { get; set; }
        private string sectionName;
        public string TeacherName { get; set; }

        public string SectionName
        {
            get { return sectionName; }
            set { sectionName = value; }
        }
        private string className;

        public string ClassName
        {
            get { return className; }
            set { className = value; }
        }
        private string newSubject;

        public string NewSubject
        {
            get { return newSubject; }
            set { newSubject = value; }
        }
        private string oldSubject;

        public string OldSubject
        {
            get { return oldSubject; }
            set { oldSubject = value; }
        }
        private int newSubjectID;

        public int NewSubjectID
        {
            get { return newSubjectID; }
            set { newSubjectID = value; }
        }
        private int substitutionID;

        public int SubstitutionID
        {
            get { return substitutionID; }
            set { substitutionID = value; }
        }
        private int replacedTeacherID;

        public int ReplacedTeacherID
        {
            get { return replacedTeacherID; }
            set { replacedTeacherID = value; }
        }
        private string replacedTeacherName;

        public string ReplacedTeacherName
        {
            get { return replacedTeacherName; }
            set { replacedTeacherName = value; }
        }
        private int replacingTeacherID;

        public int ReplacingTeacherID
        {
            get { return replacingTeacherID; }
            set { replacingTeacherID = value; }
        }
        private string replacingTeacherName;

        public string ReplacingTeacherName
        {
            get { return replacingTeacherName; }
            set { replacingTeacherName = value; }
        }
        private int periodID;

        public int PeriodID
        {
            get { return periodID; }
            set { periodID = value; }
        }
        private int dayID;

        public int DayID
        {
            get { return dayID; }
            set { dayID = value; }
        }
        private int subjectID;

        public int SubjectID
        {
            get { return subjectID; }
            set { subjectID = value; }
        }
        private int classID;

        public int ClassID
        {
            get { return classID; }
            set { classID = value; }
        }
        private int sectionID;

        public int SectionID
        {
            get { return sectionID; }
            set { sectionID = value; }
        }
        private DateTime fromDate;

        public DateTime FromDate
        {
            get { return fromDate; }
            set { fromDate = value; }
        }
        private DateTime endDate;

        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; }
        }

    }
    public class TimeTableMergedClassModel
    {
        private string subjectName;

        public string SubjectName
        {
            get { return subjectName; }
            set { subjectName = value; }
        }
        private string teacherName;

        public string TeacherName
        {
            get { return teacherName; }
            set { teacherName = value; }
        }
        private int dayID;

        public int DayID
        {
            get { return dayID; }
            set { dayID = value; }
        }
        private int periodID;


        public int PeriodID
        {
            get { return periodID; }
            set { periodID = value; }
        }

        private string secondClassSection;

        public string SecondClassSection
        {
            get { return secondClassSection; }
            set { secondClassSection = value; }
        }
        private string firstClassSection;

        public string FirstClassSection
        {
            get { return firstClassSection; }
            set { firstClassSection = value; }
        }

        private int subjectID;

        public int SubjectID
        {
            get { return subjectID; }
            set { subjectID = value; }
        }
        private int roomID;

        public int RoomID
        {
            get { return roomID; }
            set { roomID = value; }
        }
        private int secondSectionID;

        public int SecondSectionID
        {
            get { return secondSectionID; }
            set { secondSectionID = value; }
        }
        private int firstSectionID;

        public int FirstSectionID
        {
            get { return firstSectionID; }
            set { firstSectionID = value; }
        }
        private int classMergeID;

        public int ClassMergeID
        {
            get { return classMergeID; }
            set { classMergeID = value; }
        }
        private string dayName;

        public string DayName
        {
            get { return dayName; }
            set { dayName = value; }
        }
        private int teacherID;

        public int TeacherID
        {
            get { return teacherID; }
            set { teacherID = value; }
        }
        private int educationLevelID;

        public int EducationLevelID
        {
            get { return educationLevelID; }
            set { educationLevelID = value; }
        }
        private int classID;

        public int ClassID
        {
            get { return classID; }
            set { classID = value; }
        }
    }
    public class EditTTLactureModel
    {
        public List<EmployeeModel> Teachers { get; set; }
        public List<SubjectModel> Subjects { get; set; }
        public List<ClassModel> Classes { get; set; }
        public List<SectionModel> Sections { get; set; }

        public string UUID { get; set; }
        public int SubjectID { get; set; }
        public int TeacherID { get; set; }
        public int PeriodID { get; set; }
        public int DayID { get; set; }
        public int SectionID { get; set; }
        public int ClassID { get; set; }
        public int OldSectionID { get; set; }
        public int OldClassID { get; set; }
        public int EducationLevelID { get; set; }
        public int IsMergedSubject { get; set; }
        public DateTime CurrentDate { get; set; }
        public int SenderType { get; set; }
    }
    public class EditMergedClassModel
    {
        public List<TimeTableMergedClassModel> MergedClasses { get; set; }
        public List<ClassModel> Classes { get; set; }
        public List<SectionModel> Sections { get; set; }
        public int SectionID { get; set; }
        public int SecondSectionID { get; set; }
        public int RoomID { get; set; }
        public int ClassID { get; set; }
        public int PeriodID { get; set; }
        public int TeacherID { get; set; }
        public int SubjectID { get; set; }
        public int EducationLevelID { get; set; }
        public int DayID { get; set; }
        public TeacherSubjectModel SubTeacherDetails { get; set; }
    }
    public class TeacherSubjectModel
    {
        public string SubjectName { get; set; }
        public string TeacherName { get; set; }
    }
    public class SyllabusScheduleViewModel
    {
        public int SubjectID { get; set; }
        public int SectionID { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string SubjectName { get; set; }
        public DateTime CurrentDate { get; set; }
        public List<SyllabusScheduleModel> Schedule { get; set; }
    }
    public class SyllabusScheduleModel
    {      
        public int TopicID { get; set; }      
        public string ChapterName { get; set; }
        public string TopicName { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime ComplatedDate { get; set; }
        public int IsComplete { get; set; }
        public int ChapterSequenceNo { get; set; }
        public int TopicSequenceNo { get; set; }
    }
}