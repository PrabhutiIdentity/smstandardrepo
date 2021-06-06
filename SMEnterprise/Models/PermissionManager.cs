using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class PermissionManager
    {
        static readonly string PERMISSION_SESSIONNAME = "UserPermissions";
        UserModel _loggedInUserContact;
        Role _assignedRole = new Role();
        List<Feature> _accessibleFeatures = new List<Feature>();
        List<Feature> _allFeatures = new List<Feature>();
        public int SBranchID { get; set; }
        public static bool enablePermissioningSystem
        {
            get { return true; }
        }

        public UserModel loggedInUserContact
        {
            get { return _loggedInUserContact; }
            set { _loggedInUserContact = value; }
        }

        public Role assignedRole
        {
            get { return _assignedRole; }
            set { _assignedRole = value; }
        }

        public List<Feature> allFeatures
        {
            get { return Feature.GetAll().ToList(); }
            set { _allFeatures = value; }
        }

        public List<Feature> accessibleFeatures
        {
            get { return _accessibleFeatures; }
            set { _accessibleFeatures = value; }
        }

        public bool IsInUserRole
        {
            get
            {
                return (assignedRole.ID == (int)RoleType.User);
            }
        }
        public bool IsInAccountRole
        {
            get
            {
                return (assignedRole.ID == (int)RoleType.Account);
            }
        }
        public bool IsInTechnicalRole
        {
            get
            {
                return (assignedRole.ID == (int)RoleType.Technical);
            }
        }
        public bool IsInAdminRole
        {
            get
            {
                return (assignedRole.ID == (int)RoleType.Admin);
            }
        }

        public bool IsInParentRole
        {
            get
            {
                return (assignedRole.ID == (int)RoleType.Parent);
            }
        }
        public bool IsInTeacherRole
        {
            get
            {
                return (assignedRole.ID == (int)RoleType.Teacher);
            }
        }
        public bool IsInStudentRole
        {
            get
            {
                return (assignedRole.ID == (int)RoleType.Student);
            }
        }
        public bool IsInLibraryRole
        {
            get
            {
                return (assignedRole.ID == (int)RoleType.Library);
            }
        }
        public RoleType GetUserRole()
        {
            RoleType uRole = RoleType.User;
            if (IsInAdminRole)
                uRole = RoleType.Admin;
            else if (IsInParentRole)
                uRole = RoleType.Parent;
            else if (IsInUserRole)
                uRole = RoleType.User;
            else if (IsInLibraryRole)
                uRole = RoleType.Library;
            else if (IsInTeacherRole)
                uRole = RoleType.Teacher;
            else if (IsInStudentRole)
                uRole = RoleType.Student;
            else if (IsInAccountRole)
                uRole = RoleType.Account;
            else if (IsInTechnicalRole)
                uRole = RoleType.Technical;

            return uRole;
        }

        public static PermissionManager getPermissions()
        {
            PermissionManager userPermissions = SessionManager.getSession<PermissionManager>(PERMISSION_SESSIONNAME);
            return userPermissions;
        }

        public static void setPermissions(UserModel loggedInUser)
        {
            PermissionManager userPermissions = new PermissionManager();
            Service service = new Service();
            UserModel loginContact = loggedInUser;

            userPermissions.loggedInUserContact = loginContact;
            userPermissions.assignedRole = loginContact.Role;
            userPermissions.SBranchID = loggedInUser.SBranchID;
            //userPermissions.accessibleFeatures = service.GetAccessibleFeaturesForUser(loginContact);
            userPermissions.accessibleFeatures = service.SetFeaturesToLoginUser((RoleType)loginContact.RoleID).ToList();

            SessionManager.setSession<PermissionManager>(PERMISSION_SESSIONNAME, userPermissions);
        }

        public static UserModel GetLoggedInUser()
        {
            PermissionManager userPermissions = PermissionManager.getPermissions();
            return userPermissions != null ? userPermissions.loggedInUserContact : null;
        }

        public static List<string> getAccessibleFeatureControlID()
        {
            PermissionManager userPermissions = PermissionManager.getPermissions();
            return Service.GetFeatureControlID(userPermissions != null ? userPermissions.accessibleFeatures : new List<Feature>());
        }

        public static void logout()
        {
            SessionManager.setSession<PermissionManager>(PERMISSION_SESSIONNAME, null);
        }
    }

    public class JsonPermissionManager
    {
        bool _enablePerm;
        List<string> _controlIDS = new List<string>();

        public bool enablePerm
        {
            get { return _enablePerm; }
            set { _enablePerm = value; }
        }
        public List<string> controlIDS
        {
            get { return _controlIDS; }
            set { _controlIDS = value; }
        }
    }
}