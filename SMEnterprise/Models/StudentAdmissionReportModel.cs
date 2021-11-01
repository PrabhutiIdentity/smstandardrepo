using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class StudentAdmissionReportModel
    {
        public int ID { get; set; }
        public int SBranchID { get; set; }
        public DateTime StartDate { get; set; }
        public int SessionID { get; set; }
        public List<NameIDModel> Sessions { get; set; }
        public List<StudentAdmissionDetail> StudentDetail { get; set; }


    }
    public class StudentAdmissionDetail
    {
        public int StudentID { get; set; }
        public int ParentID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string Name { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public DateTime DOB { get; set; }
        public DateTime DOJ { get; set; }

    }
}