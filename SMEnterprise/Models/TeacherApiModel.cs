using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class ClassSectionModel
    {
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public int Status { get; set; }
    }
    public class TeacherAttandanceParamModel
    {
        public int ClassID { get; set; }
        public int SubjectID { get; set; }
        public int SectionID { get; set; }
        public int ID { get; set; }
        public string UUID { get; set; }
    }
    public class TeacherChapterParamModel
    {
        public string UUID { get; set; }
        public int ClassID { get; set; }
        public int SubjectID { get; set; }
    }
    public class TeacherLeaveParamModel
    {
        public int ID { get; set; }
        public string UUID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}