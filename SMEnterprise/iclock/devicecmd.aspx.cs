using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ESSL.Repository;
using System.Text;
using SMEnterprise.Repository;

namespace ESSL.iclock
{
    public partial class devicecmd : System.Web.UI.Page
    {
       
        EsslData objEsslData = new EsslData();
        protected void Page_Load(object sender, EventArgs e)
        {
            string SerialNumber = Context.Request.QueryString["SN"];
            Response.ContentType = "text/plain";
            this.Response.ContentType = "text/plain";
            string LogStream = "";
            try
            {
                int length = checked((int)this.Request.InputStream.Length);
                byte[] numArray = new byte[checked(length + 1)];
                int offset = 0;
                while (offset < length)
                    checked { offset += Context.Request.InputStream.Read(numArray, offset, 2048); }
                LogStream = Encoding.ASCII.GetString(numArray);
               // objEsslData.InsertTestData("Device Command:" + Context.Request.QueryString.ToString() + " : " + LogStream);
                LogStream = LogStream.Replace("\n", "\r\n");
                string[] strArray = LogStream.Split('\r');
                int index = 0;

                while (index < checked(strArray.Length - 1))
                {
                    strArray[index] = strArray[index].Replace("\n", "");
                    int integer = CommonUsage.ConvertToInt(strArray[index].Split('&')[0].Replace("ID=", ""));
                    string Status;
                    if (Convert.ToDouble(strArray[index].Split('&')[1].Replace("Return=", "")) == 0.0)
                    {
                        Status = "Success";
                        if (strArray[index].Split('&')[2].Replace("CMD=", "") == "DATA")
                        { };
                    }
                    else
                        Status = "Failure";
                    DateTime ExecutionDate = CommonUsage.GetCurrentDate();
                    objEsslData.UpdateDeviceCommandStatus(integer, Status, ExecutionDate, SerialNumber);
                    checked { ++index; }
                }
            }
            catch (Exception ex)
            {
                objEsslData.InsertTestData("Device Command:"+ SerialNumber+":" + ex.Message);


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