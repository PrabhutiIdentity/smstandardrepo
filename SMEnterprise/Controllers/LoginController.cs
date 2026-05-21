using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;
using SMEnterprise.Models;
using SMEnterprise.Filters;
using SMEnterprise.Repository;
<<<<<<< HEAD
using SMEnterpriseDB.Models;
using System.Linq;
=======
<<<<<<< HEAD
=======
using SMEnterpriseDB.Models;
using System.Linq;
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
>>>>>>> master

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
            string a = Request.ServerVariables["REMOTE_ADDR"];
            string b = Request.ServerVariables["REMOTE_HOST"];
            string c = Request.ServerVariables["REMOTE_USER"];
            if (ModelState.IsValid)
            {
                //  bool success = true;
                // bool rememberme = login.rememberMe;
                //  bool success = WebSecurity.Login(login.username, login.password, rememberme);
                UserModel AttemptedUser = objILoginData.GetUserByUserName(login.username);


                if (AttemptedUser != null && (AttemptedUser.Password != null || AttemptedUser.Password != ""))
                {

                    string svalue = login.salt;

                    string NewHash = CommonUsage.EncryptPassword(AttemptedUser.Password + svalue);
                    if (login.password == NewHash)
                    {
                        PermissionManager.setPermissions(AttemptedUser);
                        Session["UserID"] = PermissionManager.GetLoggedInUser().UserID;
                        Session["SBranchID"] = PermissionManager.GetLoggedInUser().SBranchID;
                        //Session["SchoolID"] = PermissionManager.GetLoggedInUser().SchoolID;
                        try
                        {
                            Session["Permissions"] = objILoginData.GetUserPermissions(PermissionManager.GetLoggedInUser().UserID, PermissionManager.GetLoggedInUser().SBranchID);
<<<<<<< HEAD
                        } 
=======
<<<<<<< HEAD
=======
                        } 
                        catch { }
                        try
                        {
                            var accountData = new AccountData();
                            int branchId = PermissionManager.GetLoggedInUser().SBranchID;
                            var sub = accountData.GetBranchSubscription(branchId);
                            if (sub != null && sub.IsDue && sub.DueAmount > 0m)
                            {
                                Session["ShowPaymentDue"] = true;
                                Session["PaymentDueBranchID"] = branchId;
                                Session["PaymentDueAmount"] = sub.DueAmount;
                                Session["PaymentDuePlanName"] = sub.PlanName ?? "";
                            }
                            else
                            {
                                Session.Remove("ShowPaymentDue");
                                Session.Remove("PaymentDueBranchID");
                                Session.Remove("PaymentDueAmount");
                                Session.Remove("PaymentDuePlanName");
                            }
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
                        }
>>>>>>> master
                        catch { }
                        CommonData objCData = new CommonData();
                        int activeBranchId = AttemptedUser.SBranchID;
                        if (AttemptedUser.RoleID == (int)RoleType.Admin || AttemptedUser.RoleID == (int)RoleType.Principle || AttemptedUser.RoleID == (int)RoleType.Director)
                        {
<<<<<<< HEAD
                           var branches= (new AdminData()).GetBranches(AttemptedUser.UserID, AttemptedUser.SBranchID,AttemptedUser.RoleID).ToList();
                            Session["SBrancheList"] = branches;
                            activeBranchId = branches.FirstOrDefault()?.SBranchID ?? AttemptedUser.SBranchID;
                            Session["SBranchID"] = activeBranchId;
                            try { SetBranchPaymentDueSession(activeBranchId); } catch { ClearBranchPaymentDueSession(); }
                            objCData.InitializeStartupSettings(activeBranchId);
=======
<<<<<<< HEAD
=======
                           var branches= (new AdminData()).GetBranches(AttemptedUser.UserID, AttemptedUser.SBranchID,AttemptedUser.RoleID).ToList();
                            Session["SBrancheList"] = branches;
                            Session["SBranchID"] = branches.FirstOrDefault()?.SBranchID;
>>>>>>> 7581125fe6277471213b8ad80ba631259c98eb8f
                            objCData.InitializeStartupSettings(AttemptedUser.SBranchID);
>>>>>>> master
                            return RedirectToAction("Dashboard", "Admin");
                        }
                        else if (AttemptedUser.RoleID == (int)RoleType.Technical)
                        {
                            ClearBranchPaymentDueSession();
                            //objCData.InitializeStartupSettings(AttemptedUser.SBranchID);
                            return RedirectToAction("FirstBranch", "Admin");
                        }
                        else if (AttemptedUser.RoleID == (int)RoleType.Parent)
                        {
                            try { SetBranchPaymentDueSession(activeBranchId); } catch { ClearBranchPaymentDueSession(); }
                            objCData.InitializeStartupSettings(activeBranchId);
                            return RedirectToAction("LandingPage", "Parent");
                        }
                        else if (AttemptedUser.RoleID == (int)RoleType.Account)
                        {
                            try { SetBranchPaymentDueSession(activeBranchId); } catch { ClearBranchPaymentDueSession(); }
                            objCData.InitializeStartupSettings(activeBranchId);
                            return RedirectToAction("Dashboard", "Account");
                        }
                        else if (AttemptedUser.RoleID == (int)RoleType.Teacher)
                        {
                            try { SetBranchPaymentDueSession(activeBranchId); } catch { ClearBranchPaymentDueSession(); }
                            objCData.InitializeStartupSettings(activeBranchId);
                            return RedirectToAction("Dashboard", "Teacher");
                        }
                        else if (AttemptedUser.RoleID == (int)RoleType.Library)
                        {
                            try { SetBranchPaymentDueSession(activeBranchId); } catch { ClearBranchPaymentDueSession(); }
                            return RedirectToAction("Dashboard", "Library");
                        }
                        else if (AttemptedUser.RoleID == (int)RoleType.Reception || AttemptedUser.RoleID == (int)RoleType.Receptionist)
                        {
                            try { SetBranchPaymentDueSession(activeBranchId); } catch { ClearBranchPaymentDueSession(); }
                            return RedirectToAction("Dashboard", "Reception");
                        }
                        else
                        {
                            try { SetBranchPaymentDueSession(activeBranchId); } catch { ClearBranchPaymentDueSession(); }
                            return RedirectToAction("Index", "Home");
                        }

                    }
                }
                else
                {
                    ViewBag.LoginError = "Username or Password is incorrect !!!!";
                    return RedirectToAction("Index", "Home");
                }

                //FormsAuthentication.SetAuthCookie("Admin", false);

                //if (success == true)
                //{
                //    if (string.IsNullOrEmpty(Convert.ToString(LoginType)))
                //    {
                //        ModelState.AddModelError("Error", "Rights to User are not Provide Contact to Admin");
                //        return View(login);
                //    }
                //    else
                //    {
                //        ViewBag.UserName = login.username;
                //        Session["Name"] = login.username;
                //        Session["UserID"] = UserID;
                //        Session["LoginType"] = LoginType;

                //        if (Roles.IsUserInRole(login.username, "Admin"))
                //        {
                //            return RedirectToAction("Blogs", "Admin");
                //        }
                //        else
                //        {
                //            string decodedUrl = "";
                //            if (!string.IsNullOrEmpty(Request.Params["ReturnUrl"]))
                //                decodedUrl = Server.UrlDecode(Request.Params["ReturnUrl"]);

                //            //Login logic...

                //            if (Url.IsLocalUrl(decodedUrl))
                //            {
                //                return Redirect(decodedUrl);
                //            }
                //            else
                //            {
                //                return RedirectToAction("Index", "Home");
                //            }
                //            //if (Request.FilePath == "Login/Login")
                //            //{
                //            //    return RedirectToAction("Home", "Index");
                //            //}
                //            //else
                //            //{
                //            //    return Redirect(Request.UrlReferrer.ToString());
                //            //}

                //        }
                //    }
                //}
                //else
                //{
                //    ModelState.AddModelError("Error", "Please enter valid Username and Password");
                //    return View(login);
                //}
            }
            else
            {
                ModelState.AddModelError("Error", "Please enter Username and Password");
            }
            return RedirectToAction("Index", "Home");

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
