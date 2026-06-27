using System.Web.Http;

namespace SMEnterprise.App_Start
{
    public class WebApiConfig
    {
        public static string UrlPrefixRelative { get { return "~/api"; } }
        public static void Register(HttpConfiguration config)
        {
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never;

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);
            config.Routes.MapHttpRoute(
             name: "APIRoute",
             routeTemplate: "api/{controller}/{action}/{id}",
             defaults: new { controller = "ParentApi", action = "Index", id = RouteParameter.Optional }
         );
            config.Routes.MapHttpRoute(
            name: "API2Route",
            routeTemplate: "apiv2/{controller}/{action}/{id}",
            defaults: new { controller = "ParentApi", action = "Index", id = RouteParameter.Optional }
        );
        }
    }
}
