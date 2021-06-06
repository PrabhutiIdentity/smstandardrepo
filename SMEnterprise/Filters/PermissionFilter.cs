using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMEnterprise.Models;

namespace SMEnterprise.Filters
{
    public class PermissionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            HttpRequestBase request = filterContext.HttpContext.Request;
            HttpResponseBase response = filterContext.HttpContext.Response;

            if ((request.AcceptTypes.Contains("application/json")) == false)
            {
                if (request.IsAjaxRequest())
                {
                    #region Preventing caching of ajax request in IE browser
                    response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                    response.Cache.SetValidUntilExpires(false);
                    response.Cache.SetCacheability(HttpCacheability.NoCache);
                    response.Cache.SetNoStore();
                    #endregion
                }

                string currentActionName = filterContext.ActionDescriptor.ActionName;
                string currentControllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

                //get LoggedInUser Permissions
                PermissionManager userPermissions = PermissionManager.getPermissions();

                //All Features are allowed for SuperAdmin - disablePermissioning
                if (userPermissions != null)
                {
                    //For logout page
                    filterContext.Controller.ViewBag.LoggedInAdministrator = PermissionManager.GetLoggedInUser();

                    if (PermissionManager.enablePermissioningSystem == true)
                    {
                        //Not all actions are feature in the application
                        bool isCurrentActionAFeature = Service.isFeaturePresentInList(userPermissions.allFeatures, currentControllerName, currentActionName);

                        if (isCurrentActionAFeature)
                        {
                            bool hasPermission = false;

                            hasPermission = Service.isFeaturePresentInList(userPermissions.accessibleFeatures, currentControllerName, currentActionName);

                            if (!hasPermission)
                            {
                                //return 'not authorized' content
                                filterContext.Controller.TempData["Message"] = "Not Authorized To Access The Content.";
                                filterContext.Result = new RedirectToRouteResult(
                                            new System.Web.Routing.RouteValueDictionary { { "controller", "Home" }, { "action", "Index" }, { "Area", "" } });
                            }
                        }
                        else
                        {
                            filterContext.Controller.TempData["Message"] = "Not Authorized To Access The Content.";
                            filterContext.Result = new RedirectToRouteResult(
                                        new System.Web.Routing.RouteValueDictionary { { "controller", "Home" }, { "action", "Index" }, { "Area", "" } });

                        }
                    }
                }
                //Redirect to login Page
                else
                {
                    filterContext.Controller.TempData["Message"] = "Please login to continue.";
                    filterContext.Result = new RedirectToRouteResult(
                                new System.Web.Routing.RouteValueDictionary { { "controller", "Home" }, { "action", "Index" }, { "Area", "" } });
                }
            }
        }
    }
}