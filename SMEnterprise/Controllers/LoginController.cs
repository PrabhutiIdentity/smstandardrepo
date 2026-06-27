using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;
using SMEnterprise.Models;
using SMEnterprise.Filters;
using SMEnterprise.Repository;
using SMEnterpriseDB.Models;
using System.Linq;
using System.Diagnostics;

namespace SMEnterprise.Controllers
{
    public class LoginController : Controller
    {
        LoginData objILoginData;
        Service service;

        public LoginController()
        {
            objILoginData = new LoginData();
            service = new Service();
        }
        #region Subscription Payment  
        private void ClearBranchPaymentDueSession()
        {
            Session.Remove("ShowPaymentDue");
            Session.Remove("PaymentDueBranchID");
            Session.Remove("PaymentDueAmount");
            Session.Remove("PaymentDuePlanName");
            Session.Remove("PaymentDueDate");
            Session.Remove("PaymentDueGraceDays");
            Session.Remove("PaymentDueGraceEndDate");
            Session.Remove("PaymentDueCanClose");
        }

        private void SetBranchPaymentDueSession(int branchId)
        {
            var accountData = new AccountData();
            var sub = accountData.GetBranchSubscription(branchId);
            if (sub == null || !sub.IsDue)
            {
                ClearBranchPaymentDueSession();
                return;
            }

            var dueAmount = sub.NextDueAmount > 0m ? sub.NextDueAmount : sub.DueAmount;
            if (dueAmount <= 0m)
            {
                ClearBranchPaymentDueSession();
                return;
            }

            var dueDate = (sub.PartialPaymentCount > 0 && sub.NextDueDate.HasValue)
                ? sub.NextDueDate.Value.Date
                : (sub.DueDate.HasValue ? sub.DueDate.Value.Date : CommonUsage.GetCurrentDate().Date);

            var now = CommonUsage.GetCurrentDate().Date;
            var isPastDue = dueDate <= now;
            if (!isPastDue)
            {
                ClearBranchPaymentDueSession();
                return;
            }

            var graceDays = sub.GraceDays < 0 ? 0 : sub.GraceDays;
            // Ensure we are comparing dates only
            var graceEndDate = dueDate.AddDays(graceDays).Date;
            var currentDate = now.Date;

            // The popup can be closed if we are still within the grace period
            var canClosePopup = graceDays > 0 && currentDate <= graceEndDate;

            Session["ShowPaymentDue"] = true;
            Session["PaymentDueBranchID"] = branchId;
            Session["PaymentDueAmount"] = dueAmount;
            Session["PaymentDuePlanName"] = sub.PlanName ?? "";
            Session["PaymentDueDate"] = dueDate;
            Session["PaymentDueGraceDays"] = graceDays;
            Session["PaymentDueGraceEndDate"] = graceEndDate;
            Session["PaymentDueCanClose"] = canClosePopup;
        }
        #endregion

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login login)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["LoginError"] = "Please enter Username and Password.";
                    return RedirectToAction("Index", "Home");
                }

                UserModel attemptedUser = objILoginData.GetUserByUserName(login.username);
                if (attemptedUser == null || string.IsNullOrEmpty(attemptedUser.Password))
                {
                    TempData["LoginError"] = "Username or Password is incorrect.";
                    return RedirectToAction("Index", "Home");
                }

                string newHash = CommonUsage.EncryptPassword(attemptedUser.Password + login.salt);
                if (login.password != newHash)
                {
                    TempData["LoginError"] = "Username or Password is incorrect.";
                    return RedirectToAction("Index", "Home");
                }

                PermissionManager.setPermissions(attemptedUser);
                Session["UserID"] = PermissionManager.GetLoggedInUser().UserID;
                Session["SBranchID"] = PermissionManager.GetLoggedInUser().SBranchID;
                try
                {
                    Session["Permissions"] = objILoginData.GetUserPermissions(
                        PermissionManager.GetLoggedInUser().UserID,
                        PermissionManager.GetLoggedInUser().SBranchID);
                }
                catch { }

                CommonData commonData = new CommonData();
                int activeBranchId = attemptedUser.SBranchID;

                if (attemptedUser.RoleID == (int)RoleType.Admin ||
                    attemptedUser.RoleID == (int)RoleType.Principle ||
                    attemptedUser.RoleID == (int)RoleType.Director)
                {
                    var branches = (new AdminData()).GetBranches(
                        attemptedUser.UserID,
                        attemptedUser.SBranchID,
                        attemptedUser.RoleID).ToList();
                    Session["SBrancheList"] = branches;
                    activeBranchId = branches.FirstOrDefault()?.SBranchID ?? attemptedUser.SBranchID;
                    Session["SBranchID"] = activeBranchId;
                    try { SetBranchPaymentDueSession(activeBranchId); } catch { ClearBranchPaymentDueSession(); }
                    commonData.InitializeStartupSettings(activeBranchId);
                    return RedirectToAction("Dashboard", "Admin");
                }

                if (attemptedUser.RoleID == (int)RoleType.Technical)
                {
                    ClearBranchPaymentDueSession();
                    return RedirectToAction("FirstBranch", "Admin");
                }

                try { SetBranchPaymentDueSession(activeBranchId); } catch { ClearBranchPaymentDueSession(); }

                if (attemptedUser.RoleID == (int)RoleType.Parent)
                {
                    commonData.InitializeStartupSettings(activeBranchId);
                    return RedirectToAction("LandingPage", "Parent");
                }
                if (attemptedUser.RoleID == (int)RoleType.Account)
                {
                    commonData.InitializeStartupSettings(activeBranchId);
                    return RedirectToAction("Dashboard", "Account");
                }
                if (attemptedUser.RoleID == (int)RoleType.Teacher)
                {
                    commonData.InitializeStartupSettings(activeBranchId);
                    return RedirectToAction("Dashboard", "Teacher");
                }
                if (attemptedUser.RoleID == (int)RoleType.Library)
                {
                    return RedirectToAction("Dashboard", "Library");
                }
                if (attemptedUser.RoleID == (int)RoleType.Reception ||
                    attemptedUser.RoleID == (int)RoleType.Receptionist)
                {
                    return RedirectToAction("Dashboard", "Reception");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Trace.TraceError("Login failed due to an internal error: {0}", ex);
                TempData["LoginError"] = "Server is temporarily unavailable. Please try again in a moment.";
                return RedirectToAction("Index", "Home");
            }
        }
        public ActionResult LoginPartial(Login login)
        {
            if (ModelState.IsValid)
            {

                bool success = true;
                if (success == true)
                {
                    if (string.IsNullOrEmpty(Convert.ToString(1)))
                    {
                        ModelState.AddModelError("Error", "Rights to User are not Provide Contact to Admin");
                        return PartialView("_RecipePostCommentPartial", login);
                    }
                    else
                    {
                        ViewBag.UserName = login.username;
                        Session["Name"] = login.username;
                        Session["UserID"] = 1;
                        Session["LoginType"] = 1;

                        return PartialView("_RecipePostCommentPartial", login);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("Error", "Please enter valid Username and Password");
                return PartialView("_RecipePostCommentPartial", login);
            }
            return PartialView("_RecipePostCommentPartial", login);
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Register register)
        {

            if (ModelState.IsValid)
            {
                UserModel objUserModel = new UserModel();
                objUserModel.EmailID = register.EmailID;
                objUserModel.FullName = register.FullName;
                objUserModel.UserName = register.username;
                objUserModel.CreatedDate = CommonUsage.GetCurrentDate();
                objUserModel.Password = register.password;
                objUserModel.RoleID = 2;
                objUserModel.UserID = objILoginData.InsertUser(objUserModel);
                if (objUserModel.UserID > 0)
                {
                    PermissionManager.setPermissions(objUserModel);
                }
            }
            else
            {
                ModelState.AddModelError("Error", "Please enter all details");

            }
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        [MyExceptionHandler]
        [AllowAnonymous]
        public ActionResult RoleCreate()
        {
            return View();
        }

        [HttpPost]
        [MyExceptionHandler]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult RoleCreate(Role role)
        {
            if (ModelState.IsValid)
            {
                if (Roles.RoleExists(role.Name))
                {
                    ModelState.AddModelError("Error", "Rolename already exists");
                    return View(role);
                }
                else
                {
                    Roles.CreateRole(role.Name);
                    return RedirectToAction("RoleIndex", "Login");
                }
            }
            else
            {
                ModelState.AddModelError("Error", "Please enter Username and Password");
            }
            return View(role);
        }

        [HttpGet]
        [MyExceptionHandler]
        [AllowAnonymous]
        public ActionResult RoleAddToUser()
        {
            AssignRoleVM objvm = new AssignRoleVM();
            objvm.RolesList = GetAll_Roles();
            objvm.Userlist = GetAll_Users();
            return View(objvm);
        }
        [NonAction]
        public List<SelectListItem> GetAll_Users()
        {
            var Userlist = objILoginData.GetAllUsers();
            List<SelectListItem> listuser = new List<SelectListItem>();
            listuser.Add(new SelectListItem { Text = "Select", Value = "0" });
            foreach (var item in Userlist)
            {
                listuser.Add(new SelectListItem { Text = item.UserName, Value = item.Id });
            }

            return listuser;
        }

        [HttpPost]
        [MyExceptionHandler]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RoleAddToUser(AssignRoleVM objvm)
        {

            if (objvm.RoleName == "0")
            {
                ModelState.AddModelError("RoleName", "Please select RoleName");
            }

            if (objvm.UserName == "0")
            {
                ModelState.AddModelError("UserName", "Please select Username");
            }

            if (ModelState.IsValid)
            {

                if (objILoginData.Get_CheckUserRoles(objvm.UserName) == true)
                {
                    ViewBag.ResultMessage = "This user already has the role specified !";
                }
                else
                {
                    var UserName = objILoginData.GetUserName_BY_UserID(objvm.UserName);
                    Roles.AddUserToRole(UserName, objvm.RoleName);
                    ViewBag.ResultMessage = "Username added to the role succesfully !";
                }
                objvm.RolesList = GetAll_Roles();
                objvm.Userlist = GetAll_Users();

                return View(objvm);
            }
            else
            {
                objvm.RolesList = GetAll_Roles();
                objvm.Userlist = GetAll_Users();
                ModelState.AddModelError("Error", "Please enter Username and Password");
            }

            return View(objvm);
        }


        [HttpPost]
        [MyExceptionHandler]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRoleForUser(AssignRoleVM objvm)
        {
            if (objvm.RoleName == "0")
            {
                ModelState.AddModelError("RoleName", "Please select RoleName");
            }

            if (objvm.UserName == "0")
            {
                ModelState.AddModelError("UserName", "Please select Username");
            }

            objvm.RolesList = GetAll_Roles();
            objvm.Userlist = GetAll_Users();

            if (ModelState.IsValid)
            {
                if (objILoginData.Get_CheckUserRoles(objvm.UserName) == true)
                {
                    var UserName = objILoginData.GetUserName_BY_UserID(objvm.UserName);
                    Roles.RemoveUserFromRole(UserName, objvm.RoleName);
                    ViewBag.ResultMessage = "Role removed from this user successfully !";
                }
                else
                {
                    ViewBag.ResultMessage = "This user doesn't belong to selected role.";
                }
            }
            return View(objvm);
        }

        [HttpGet]
        [MyExceptionHandler]
        [AllowAnonymous]
        public ActionResult DeleteRoleForUser()
        {
            AssignRoleVM objvm = new AssignRoleVM();
            objvm.RolesList = GetAll_Roles();
            objvm.Userlist = GetAll_Users();
            return View(objvm);
        }



        [MyExceptionHandler]
        [AllowAnonymous]
        public ActionResult RoleDelete(string RoleName)
        {
            Roles.DeleteRole(RoleName);
            return RedirectToAction("RoleIndex", "Login");
        }

        [HttpPost]
        [MyExceptionHandler]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult GetRoles(string UserName)
        {
            if (!string.IsNullOrWhiteSpace(UserName))
            {
                ViewBag.RolesForThisUser = Roles.GetRolesForUser(UserName);
                SelectList list = new SelectList(Roles.GetAllRoles());
                ViewBag.Roles = list;
            }
            return View("RoleAddToUser");
        }


        [ValidateAntiForgeryToken]
        public ActionResult logout()
        {
            PermissionManager.logout();
            TempData["Message"] = "You are logged out successfully.";
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
        //[HttpGet]
        //[MyExceptionHandler]
        //public ActionResult Changepassword()
        //{
        //    return View(new ChangepasswordVM());
        //}

        //[HttpPost]
        //[MyExceptionHandler]
        //[ValidateAntiForgeryToken]
        //public ActionResult Changepassword(ChangepasswordVM VM)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (!WebSecurity.UserExists(Convert.ToString(Session["Name"])))
        //        {
        //            ModelState.AddModelError("Error", "UserName ");

        //        }
        //        else
        //        {
        //            //var token = WebSecurity.GeneratePasswordResetToken(Convert.ToString(Session["Name"]));
        //            //WebSecurity.ResetPassword(token, VM.password);
        //            //ViewBag = "Password Changed";

        //            var value = WebSecurity.ChangePassword(Session["Name"].ToString(), VM.OldPassword, VM.Newpassword);

        //            if (value == false)
        //            {
        //                ModelState.AddModelError("Error", "Incorrect Old Password");
        //                return View(VM);
        //            }
        //            else
        //            {
        //                ViewBag.ResultMessage = "Password Changed Successfully";
        //            }

        //        }
        //    }
        //    else
        //    {
        //        ModelState.AddModelError("Error", "Fill on Fields");
        //    }
        //    return View(VM);
        //}

        //[HttpGet]
        //[AllowAnonymous]
        //[Authorize(Roles = "Admin")]
        //public ActionResult AllRegisterUserDetails()
        //{
        //    var Users = objILoginData.GetAllUsers();
        //    return View(Users);
        //}

        [HttpGet]
        [AllowAnonymous]
        public ActionResult CheckUserNameExists(string username)
        {
            bool UserExists = false;

            try
            {
                var nameexits = objILoginData.Get_checkUsernameExits(username);

                if (string.Equals(nameexits, "1"))
                {
                    UserExists = true;
                }
                else
                {
                    UserExists = false;
                }
                return Json(!UserExists, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult CheckEmailExists(string EmailID)
        {
            bool UserExists = false;

            try
            {
                var nameexits = objILoginData.Get_checkEmailExits(EmailID);

                if (string.Equals(nameexits, "1"))
                {
                    UserExists = true;
                }
                else
                {
                    UserExists = false;
                }
                return Json(!UserExists, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        [NonAction]
        public List<SelectListItem> GetAll_Roles()
        {
            List<SelectListItem> listrole = new List<SelectListItem>();

            listrole.Add(new SelectListItem { Text = "Select", Value = "0" });

            foreach (var item in Roles.GetAllRoles())
            {
                listrole.Add(new SelectListItem { Text = item, Value = item });
            }

            return listrole;
        }

        //[NonAction]
        //public List<SelectListItem> GetAll_Users()
        //{
        //    var Userlist = objILoginData.GetAllUsers();
        //    List<SelectListItem> listuser = new List<SelectListItem>();
        //    listuser.Add(new SelectListItem { Text = "Select", Value = "0" });
        //    foreach (var item in Userlist)
        //    {
        //        listuser.Add(new SelectListItem { Text = item.UserName, Value = item.Id });
        //    }

        //    return listuser;
        //}

    }
}
