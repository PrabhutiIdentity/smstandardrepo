using BigBlueButtonAPI.Core;
using SMEnterprise.App_Start;
using SMEnterprise.Repository;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;

namespace SMEnterprise
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static System.Net.Http.HttpClient HttpClient = new System.Net.Http.HttpClient();
        public static BigBlueButtonAPISettings BigBlueButtonAPISettings;
        protected void Application_Start()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                BigBlueButtonAPISettings = new BigBlueButtonAPISettings
                {
                    ServerAPIUrl = ConfigurationManager.AppSettings["BigBlueButtonAPISettings:ServerAPIUrl"],
                    SharedSecret = ConfigurationManager.AppSettings["BigBlueButtonAPISettings:SharedSecret"]
                };
                AreaRegistration.RegisterAllAreas();
                GlobalConfiguration.Configure(WebApiConfig.Register);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                SMEnterprise.Repository.CommonUsage.SetConnectionString();
                SMEnterprise.Repository.CommonUsage.LoadConnectionStrings();
                SMEnterprise.Repository.CommonData.InitializeCommonConfiguration();
                //string Path = HttpRuntime.AppDomainAppPath;
                //CommonData.InitializeSMSConfiguration(Path);
            }
            catch(Exception ex)
            {

                CommonData objcd = new CommonData();
                objcd.InsertError(0, "Startup", ex.ToString());
            }
            //from accessing a cross - origin frame
            System.Web.Helpers.AntiForgeryConfig.SuppressXFrameOptionsHeader = true;
            //
        }

        protected void Application_Error(object sender, EventArgs e)
        {

            HttpContext con = HttpContext.Current;
            con.Request.Url.ToString();
            var exception = Server.GetLastError();
            //  Session["Exception"] = exception.ToString();
            if (exception.GetType().ToString() != "System.Web.HttpException")
            {
                string logFile = "~/ErrorLog.txt";
                logFile = HttpContext.Current.Server.MapPath(logFile);
                try
                {
                    // Open the log file for append and write the log
                    StreamWriter sw = new StreamWriter(logFile, true);
                    sw.WriteLine("********** {0} **********", DateTime.Now);

                    sw.WriteLine("Source: ");
                    sw.Write(con.Request.Url.ToString());

                    sw.WriteLine("Exception Type: ");
                    sw.Write(exception.GetType().ToString());
                    foreach (string key in con.Request.Form.AllKeys)
                    {
                        sw.WriteLine("{0} : {1} ", key, con.Request.Form[key].ToString());
                    }
                    sw.WriteLine("Exception: " + exception.Message);
                    sw.Flush();
                    sw.Close();

                    CommonData objcd = new CommonData();
                    string Data = "";
                    StreamReader stream = new StreamReader(con.Request.InputStream);
                    string x = stream.ReadToEnd();
                    con.Request.InputStream.Position = 0;
                    foreach (string key in con.Request.Form.AllKeys)
                    {
                        Data = Data + "ѱ" + key + ":" + Newtonsoft.Json.JsonConvert.SerializeObject(con.Request.Form[key].ToString());
                    }
                    Data = Data + "φ" + x + " Error : " + exception.ToString();
                    objcd.InsertError(0, con.Request.Url.ToString(), Data);
                }
                catch (Exception ex)
                {
                    Response.Write(ex.ToString());
                }
                // Process 404 HTTP errors
                var httpException = exception as HttpException;
                //if (httpException != null && httpException.GetHttpCode() == 404)
                //{
                Response.Clear();
                Server.ClearError();
                Response.TrySkipIisCustomErrors = true;
            }
            Response.Write("An Error Has occured.... kindly contact technical team....");
        }
        void Application_AcquireRequestState(object sender, EventArgs e)
        {
          //  Session is Available here
            HttpContext context = HttpContext.Current;
          //  context.Session["UserID"] = "100";

        }
        protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpContext con = HttpContext.Current;
            if (!con.Request.Url.ToString().Contains("KeepAlive") && !con.Request.Url.ToString().Contains("/Images/") && !con.Request.Url.ToString().Contains("/GetSessionState/"))
            {
                int UserID = 1;
                //try
                //{
                //    UserID = CommonUsage.ConvertToInt(con.Session["UserID"].ToString());
                //}
                //catch (Exception ex)
                //{
                //    UserID = -1;
                //}
                if (UserID != 0)
                {
                    DateTime dt = DateTime.Now;
                    CommonData objcd = new CommonData();
                    string Data = "";
                    StreamReader stream = new StreamReader(con.Request.InputStream);
                    string x = stream.ReadToEnd();
                    con.Request.InputStream.Position = 0;
                    foreach (string key in con.Request.Form.AllKeys)
                    {
                        Data = Data + "ѱ" + key + ":" + Newtonsoft.Json.JsonConvert.SerializeObject(con.Request.Form[key].ToString());
                    }
                    Data = Data + "φ" + x;
                    objcd.InsertLog(UserID, con.Request.Url.ToString(), Data);
                }
            }
        }
        protected void Application_PostAuthorizeRequest()
        {
            if (IsWebApiRequest())
            {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
        }

        private bool IsWebApiRequest()
        {
            return HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith(WebApiConfig.UrlPrefixRelative);
        }

    }
}
