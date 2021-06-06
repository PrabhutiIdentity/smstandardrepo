using ESSL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ESSL.iclock
{
    public partial class FData : System.Web.UI.Page
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
                objEsslData.InsertTestData("Device Command:" + Context.Request.QueryString.ToString() + " : " + LogStream);
            }
            catch (Exception ex)
            {
                objEsslData.InsertTestData("Device Command:" + SerialNumber + ":" + ex.Message);
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