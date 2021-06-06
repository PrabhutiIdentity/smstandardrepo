using SMEnterprise.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class Service 
    {
        LoginData objData = new LoginData();


        internal IEnumerable<Feature> SetFeaturesToLoginUser(RoleType role)
        {
            if (role == RoleType.Director)
            {
                return (Feature.GetAll());
            }
            else if (role == RoleType.Principle)
            {
                return (Feature.GetAll());
            }
            else if (role == RoleType.Admin)
            {
                return (Feature.GetAll());
            }
            else if(role == RoleType.Parent)
            {
                return (Feature.GetAll().Where(f => f.ControllerName == "Parent"));
            }
            else if (role == RoleType.Account)
            {
                return (Feature.GetAll().Where(f => f.ControllerName == "Account"));
            }
            else if (role == RoleType.Teacher)
            {
                return (Feature.GetAll().Where(f => f.ControllerName == "Teacher"));
            }
            else if (role == RoleType.Library)
            {
                return (Feature.GetAll().Where(f => f.ControllerName == "Library"));
            }
            else if (role == RoleType.Reception || role==RoleType.Receptionist)
            {
                return (Feature.GetAll().Where(f => f.ControllerName == "Reception"));
            }
            else if (role == RoleType.Technical)
            {
                return (Feature.GetAll().Where(f => f.ControllerName == "Admin" && f.ActionName=="FirstBranch"));
            }
            else
            {
                return (Feature.GetAll().Where(f => f.ControllerName == "Login"));
            }
        }

        internal List<Feature> GetAccessibleFeaturesForUser(UserModel loginContact)
        {
            return loginContact.Role.Features.ToList();
        }

        internal static List<string> GetFeatureControlID(List<Feature> list)
        {
            return list.Select(l => l.Name).ToList();
        }

        internal static bool isFeaturePresentInList(List<Feature> allFeatureList, string controllerName, string ActionName)
        {
            //return allFeatureList.Where(f => f.ControllerName.Equals(controllerName, StringComparison.OrdinalIgnoreCase) && f.ActionName.Equals(ActionName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() != null;
            return allFeatureList.Where(f => f.ControllerName.Equals(controllerName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() != null;
        }
    }

    public class Feature
    {
        #region Properties

        public int ID { get; set; }
        public string Name { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }

        #endregion

        #region Methods

        public static ICollection<Feature> GetAll()
        {
            List<Feature> featureList = new List<Feature>();

            Feature rT = new Feature();
            rT.ID = 1;
            rT.Name = "Dashboard";
            rT.ControllerName = "Admin";
            rT.ActionName = "Dashboard";
            featureList.Add(rT);

            Feature rT2 = new Feature();
            rT2.ID = 2;
            rT2.Name = "Index";
            rT2.ControllerName = "Home";
            rT2.ActionName = "Index";
            featureList.Add(rT2);

            Feature rT3 = new Feature();
            rT3.ID = 2;
            rT3.Name = "Index";
            rT3.ControllerName = "Login";
            rT3.ActionName = "logout";
            featureList.Add(rT3);

            Feature rT4 = new Feature();
            rT4.ID = 2;
            rT4.Name = "Parent";
            rT4.ControllerName = "Parent";
            rT4.ActionName = "Index";
            featureList.Add(rT4);

            Feature rT5 = new Feature();
            rT5.ID = 2;
            rT5.Name = "Account";
            rT5.ControllerName = "Account";
            rT5.ActionName = "Index";
            featureList.Add(rT5);

            Feature rT6 = new Feature();
            rT6.ID = 2;
            rT6.Name = "Teacher";
            rT6.ControllerName = "Teacher";
            rT6.ActionName = "Index";

            featureList.Add(rT6);

            Feature rT7 = new Feature();
            rT7.ID = 2;
            rT7.Name = "Library";
            rT7.ControllerName = "Library";
            rT7.ActionName = "Index";
            featureList.Add(rT7);

            Feature rT8 = new Feature();
            rT8.ID = 2;
            rT8.Name = "Reception";
            rT8.ControllerName = "Reception";
            rT8.ActionName = "Dashboard";
            featureList.Add(rT8);

            Feature rT9 = new Feature();
            rT9.ID = 2;
            rT9.Name = "Technical";
            rT9.ControllerName = "Technical";
            rT9.ActionName = "FirstBranch";
            featureList.Add(rT9);

            return featureList;

        }

        public static Role GetById(int id)
        {
            return Role.GetAll().Where(r => r.ID == id).FirstOrDefault();
        }

        #endregion
    }
    public class Role
    {
        #region Properties

        public int ID { get; set; }
        public string Name { get; set; }

        public IEnumerable<Feature> Features
        {
            get
            {
                var allFeatures = Feature.GetAll();
                if (ID == (int)RoleType.Admin)
                {
                    return allFeatures.Where(f => f.ControllerName=="Admin" || f.ControllerName == "Login");
                }
                else if(ID == (int)RoleType.Account)
                {
                    return allFeatures.Where(f => f.ControllerName=="Account" || f.ControllerName == "Login");
                }
                else if (ID == (int)RoleType.Parent)
                {
                    return allFeatures.Where(f => f.ControllerName == "Parent" || f.ControllerName == "Login");
                }
                else if (ID == (int)RoleType.Student)
                {
                    return allFeatures.Where(f => f.ControllerName == "Student" || f.ControllerName == "Login");
                }
                else 
                {
                    return allFeatures.Where(f => f.ControllerName == "Library" || f.ControllerName == "Login");
                }
            }
        }

        #endregion

        #region Methods

        public static ICollection<Role> GetAll()
        {
            List<Role> roleList = new List<Role>();
            Role rT = new Role();
            rT.ID = (int)RoleType.Admin;
            rT.Name = "Admin";
            roleList.Add(rT);

            rT = new Role();
            rT.ID = (int)RoleType.User;
            rT.Name = "User";
            roleList.Add(rT);

            return roleList;
        }

        public static Role GetById(int id)
        {
            return Role.GetAll().Where(r => r.ID == id).FirstOrDefault();
        }

        #endregion
    }
    public enum RoleType
    {
        Technical = -1,
        Admin = 0,
        Account = 2,
        Teacher = 3,
        Parent = 4,
        Library = 5,
        Student = 6,
        User = 7,
        Principle=8,
        Director=9,
        Reception=10,
        Receptionist = 11,
        Conductor =12
    }
}