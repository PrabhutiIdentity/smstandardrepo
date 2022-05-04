
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SMEnterprise.Models
{
    public class EmployeeAssignPageModel
    {
        public int SessionID { get; set; }
        public List<EmployeeAssignModel> AssignedEmployee { get; set; }
        public List<SchoolSessionModel> Sessions { get; set; }
        public List<NameIDModel> EmployeeType { get; set; }
        public List<NameIDModel> Employeelist { get; set; }
        public List<ClassModel> Classes { get; set; }
        public int EmployeeTypeID { get; set; }
        public int ClassID { get; set; }
        
    }
    public class EmployeeAssignModel
    {
        public int EmployeeAssignID { get; set; }
        public int SessionID { get; set; }
        public string ClassID { get; set; }
        public string EmployeeName { get; set; }
        public string ClassNames { get; set; }
        public int EmployeeTypeID { get; set; }
        public int EmployeeID { get; set; }
         public string EmployeeTypeName { get; set; }
        public List<string> ClassesAry { get; set; }
        public int Status { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
    }
}