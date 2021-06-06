using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ESSL.Models;
using ESSL.Repository;

namespace ESSL.iclock
{
    public partial class getrequest : System.Web.UI.Page
    {
        EsslData objEsslData = new EsslData();
        protected void Page_Load(object sender, EventArgs e)
        {
            string SerialNumber = HttpContext.Current.Request.QueryString["SN"];
            try
            {
                HttpContext.Current.Response.ContentType = "text/plain";
                string SN = Request.QueryString["SN"];
                //objEsslData.InsertTestData("GR-" + SN + "--" + Request.RequestType + "##" + Request.QueryString.ToString());
                List<DeviceCommandModel> objDeviceCommands = objEsslData.GetDeviceCommands(SN);
                foreach (DeviceCommandModel deviceCommand2 in objDeviceCommands)
                {
                    deviceCommand2.DeviceCommand = deviceCommand2.DeviceCommand.ToString().Replace("UniqueId", deviceCommand2.DeviceCommandId.ToString());
                    HttpContext.Current.Response.Write(deviceCommand2.DeviceCommand);
                }
            }
            catch(Exception ex)
            {
                objEsslData.InsertTestData("Get Request: "+ SerialNumber+":" + ex.ToString());
                HttpContext.Current.Response.Write("Error Occured\r");
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.End();
                return;
            }

            HttpContext.Current.Response.Write("OK\r");
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
        }
    }
}