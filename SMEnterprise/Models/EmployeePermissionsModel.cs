using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class EmployeePermissionsModel
    {
        public int isPermissionsSet { get; set; }
        public List<EmployeePermissionListModel> Permissions { get; set; }
    }
    public class EmployeePermissionListModel
    {
        public int Module { get; set; }
        public int Action { get; set; }
        public int Status { get; set; }
    }
}