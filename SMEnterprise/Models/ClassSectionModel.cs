using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class ClassPageModel
    {
        public List<ClassModel> Classes { get; set; }
        public List<NameIDModel> EducationLevels { get; set; }
        public List<EvaluationSchemeModel> Schemes { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
        public List<NameIDModel> NoFeeMonths { get; set; }
        public int SessionID { get; set; }
        public int SBranchID { get; set; }
   

    public DataTable GetNoFeeMonthDataTable()
    {

        DataTable dtNoFeeMonths = new DataTable();
        dtNoFeeMonths.SetTypeName("ut_Name_ID_Utility");
        dtNoFeeMonths.Columns.Add("ID");
        dtNoFeeMonths.Columns.Add("Name");
        dtNoFeeMonths.Columns.Add("Extra1");
        dtNoFeeMonths.Columns.Add("Extra2");
        dtNoFeeMonths.Columns.Add("Extra3");
        if (NoFeeMonths == null)
        {
            NoFeeMonths = new List<NameIDModel>();
        }
        foreach (NameIDModel e in NoFeeMonths)
        {
            if (e.Extra1 == "on")
            {
                DataRow dr = dtNoFeeMonths.NewRow();
                dr["ID"] = e.ID;
                dr["Name"] = e.Name;
                dr["Extra1"] = e.Extra1;
                dr["Extra2"] = e.Extra2;
                dr["Extra3"] = e.Extra3;

                dtNoFeeMonths.Rows.Add(dr);
            }
        }

        return dtNoFeeMonths;
    }
}
public class ClassModel
    {
        public int ClassID { get; set; }
        public int SessionID { get; set; }
        public int EvaluationSchemeID { get; set; }
        public int EmployeeID { get; set; }
        public string ClassName { get; set; }
        public int SectionCount { get; set; }
        public int StudentCount { get; set; }
        public int EducationLevelID { get; set; }
        public string EducationLevelName { get; set; }
        public List<SectionModel> Sections { get; set; }
        public int Status { get; set; }
        public int UserID { get; set; }
        public int OpType { get; set; }
        public DateTime OperationDate { get; set; }
        public int SBranchID { get; set; }
    }
    public class SectionModel
    {
        public int EducationLevelID { get; set; }
        public int SessionID { get; set; }
        public int OpType { get; set; }
        public int ID { get; set; }
        public int ClassID { get; set; }
        public string Name { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public int StudentCount { get; set; }
        public int TeacherID { get; set; }
        public int Capacity { get; set; }
        public int BuildingID { get; set; }
        public int FloorID { get; set; }
        public int RoomID { get; set; }
        public int Status { get; set; }
        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }
        public string TeacherName { get; set; }
       


    }
    public class SectionDetailsModel
    {
        public SectionModel Section { get; set; }
        public List<BuildingModel> Buildings { get; set; }
        public List<FloorModel> Floors { get; set; }
        public List<RoomModel> Rooms { get; set; }
        public List<GroupModel> Groups { get; set; }
        public List<EmployeeModel> Teachers { get; set; }
    }
    public class GroupModel
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public int EducationLevelID { get; set; }
        public int Status { get; set; }
        public int Subjects { get; set; }
        public int Types { get; set; }
        public List<SubjectModel> SubjectList { get; set; }
        public List<NameIDModel> RSubjects { get; set; }
        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }
        public int OpType { get; set; }
        public int SBranchID { get; set; }
        public List<EducationLevelSectionModel> ReportSections { get; set; }
    }
    public class GroupEditModel
    {
        public List<GroupModel> Groups { get; set; }
        public List<EducationLevelModel> EducationLevels { get; set; }

    }
}