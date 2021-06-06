using SMEnterprise.Models;
using SMEnterprise.Repository;
using SMEnterprise.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SMEnterprise.Services;

namespace SMEnterprise.Apiv2Controllers
{
    public class LoginController : ApiController
    {
        private UserService _userService;
        //public LoginController(IUserService userService)
        //{
        //    _userService = userService;
        //}
        [HttpPost]
        [AllowAnonymous]
        [Route("/apiv2/Login/AttemptLogin")]
        //[JsonConfigFilter]
        public CommonApiWraperModel AttemptLogin([FromBody]LoginModel userParam)
        {
            _userService = new UserService();
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            var user = _userService.Authenticate(userParam.username, userParam.password, userParam.salt);
            if (user == null)
            {
                objWraper.Code = 101;
                objWraper.Message = "User Does Not Exists";
            }
            else
            {
                CommonData objCommonData = new CommonData();
                ApiAuthenticationModel objModel = new ApiAuthenticationModel();
                objModel.SBranchID = user.SBranchID;
                objModel.OpType = 0;
                objModel.LastLoginDate = CommonUsage.GetCurrentDate();
                objModel.UserType = user.RoleID;
                objModel.UUID = userParam.UUID;
                objModel.UserID = user.UserID;
                objModel.deviceToken = userParam.deviceToken;
                objCommonData.InsertUpdateAppUser(objModel);
                objWraper.Data = user;
                objWraper.Code = 200;
            }
            return objWraper;
        }
    }
}