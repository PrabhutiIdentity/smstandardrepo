using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SMEnterprise
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapMvcAttributeRoutes();
            routes.MapRoute(
                name: "DefaultTeacherApiUpload",
                url: "api/TeacherApi/UploadAttachment",
                defaults: new { controller = "FileUploadApi", action = "UploadAttachment", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Edit2Param",
                url: "Admin/{action}/{id}/{id2}",
                defaults: new { controller = "Admin", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Edit3Param",
                url: "Admin/{action}/{id}/{id2}/{id3}",
                defaults: new { controller = "Admin", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Get2Param",
                url: "Parent/{action}/{id}/{id2}",
                defaults: new { controller = "Parent", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
              name: "Reception2Param",
              url: "Reception/{action}/{id}/{id2}",
              defaults: new { controller = "Reception", action = "Index", id = UrlParameter.Optional }
          );
            routes.MapRoute(
              name: "Library2Param",
              url: "Library/{action}/{id}/{id2}",
              defaults: new { controller = "Library", action = "Index", id = UrlParameter.Optional }
          );
            routes.MapRoute(
               name: "Account2Param",
               url: "Account/{action}/{id}/{id2}",
               defaults: new { controller = "Account", action = "Index", id = UrlParameter.Optional }
           );
            routes.MapRoute(
              name: "Account3Param",
              url: "Account/{action}/{id}/{id2}/{id3}",
              defaults: new { controller = "Account", action = "Index", id = UrlParameter.Optional }
          );
            routes.MapRoute(
              name: "Teacher2Param",
              url: "Teacher/{action}/{id}/{id2}",
              defaults: new { controller = "Teacher", action = "Index", id = UrlParameter.Optional }
          );
            routes.MapRoute(
             name: "Teacher3Param",
             url: "Teacher/{action}/{id}/{id2}/{id3}",
             defaults: new { controller = "Teacher", action = "Index", id = UrlParameter.Optional }
         );
            routes.MapRoute(
             name: "Teacher4Param",
             url: "Teacher/{action}/{id}/{id2}/{id3}/{id4}",
             defaults: new { controller = "Teacher", action = "Index", id = UrlParameter.Optional }
         );
            routes.MapRoute(
             name: "Parent3Param",
             url: "Parent/{action}/{id}/{id2}/{id3}",
             defaults: new { controller = "Parent", action = "Index", id = UrlParameter.Optional }
         );
        }
    }
}
