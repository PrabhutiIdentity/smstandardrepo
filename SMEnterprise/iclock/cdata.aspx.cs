using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ESSL.Repository;
using ESSL.Models;
using System.Text;
using System.Collections;
using SMEnterprise.Repository;
using SMEnterprise.Models;

namespace ESSL
{
    public partial class cdata : System.Web.UI.Page
    {

        EsslData objEsslData = new EsslData();
        protected void Page_Load(object sender, EventArgs e)
        {
            string str = "\n";
            string SerialNumber = this.Request.QueryString["SN"];
            if (!IsPostBack)
            {
                bool isProcessed = false;
                try
                {
                    string strResult = "OK:1\r";
                    Response.ContentType = "text/plain";
                    objEsslData.UpdateDeviceLastPing(SerialNumber);
                    if (Request.RequestType == "GET")
                    {

                        string PushVersion = "";
                        if (Request.QueryString["pushver"] != null)
                            PushVersion = Request.QueryString["pushver"];
                        SendOptionsToDevice(SerialNumber, PushVersion);
                        return;

                    }
                    else if (Request.RequestType == "POST")
                    {
                        int length = checked((int)Request.InputStream.Length);
                        if (Request.InputStream.Length != Request.ContentLength)
                        {
                            HttpContext.Current.Response.Write("Error Occured\r");
                            HttpContext.Current.Response.Flush();
                            HttpContext.Current.Response.End();
                            return;
                        }

                        try
                        {
                            byte[] numArray = new byte[checked(length + 1)];
                            int offset = 0;
                            while (offset < length)
                                checked { offset += Request.InputStream.Read(numArray, offset, 2048); }
                            int contentLength = Request.ContentLength;
                            str = Encoding.ASCII.GetString(numArray);

                            //objEsslData.InsertTestData("Page Load string : "+ SerialNumber+"#" + "\n" + str);
                        }
                        catch (Exception ex)
                        {
                            HttpContext.Current.Response.Write("Error Occured\r");
                            HttpContext.Current.Response.Flush();
                            HttpContext.Current.Response.End();
                            objEsslData.InsertTestData("Page Load string : " + ex.Message + "\n" + Request.ContentLength);
                        }
                        if (Request.QueryString["table"] != null && Request.QueryString["table"].Trim() == "ATTLOG")
                        {
                            string TransactionStamp = this.Request.QueryString["Stamp"];
                            if (!AddDeviceLogsForWebMonthwise(SerialNumber, str, TransactionStamp))
                            {
                                HttpContext.Current.Response.Write("Error Occured\r");
                                HttpContext.Current.Response.Flush();
                                HttpContext.Current.Response.End();
                                return;
                            }
                        }
                        else if (Request.QueryString["table"] != null && Request.QueryString["table"].Trim() == "OPERLOG")
                        {
                            if (!AddDeviceOperationLogs(str, SerialNumber))
                            {
                                HttpContext.Current.Response.Write("Error Occured\r");
                                HttpContext.Current.Response.Flush();
                                HttpContext.Current.Response.End();
                                return;
                            }
                        }
                        else
                        {
                            objEsslData.InsertTestData("CData-Else:" + Request.QueryString + "--" + str);
                        }
                    }
                    else
                    {
                        HttpContext.Current.Response.Write("Error Occured\r");
                        HttpContext.Current.Response.Flush();
                        HttpContext.Current.Response.End();
                        return;
                    }
                    HttpContext.Current.Response.Write("OK\r");
                    HttpContext.Current.Response.Flush();
                    HttpContext.Current.Response.End();

                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("Thread was being aborted"))
                    {
                        objEsslData.InsertTestData("Page Load : " + Request.QueryString + "--" + ex.Message + "$$$$$" + str);
                    }

                }
            }
        }
        public bool SendOptionsToDevice(string SerialNumber, string PushVersion)
        {
            DeviceModel objDevice = new DeviceModel();
            try
            {
                objDevice = objEsslData.GetDeviceConfigDetailsBySerialNumber(SerialNumber);
            }
            catch (Exception ex)
            {
                objEsslData.InsertTestData("Get Device Configuration : " + ex.Message);
            }

            int verComp = -1;
            try
            {
                verComp = CompareVersion(PushVersion, "2.0.0");
            }
            catch (Exception localException) { }
            String strTransFlag = "";
            if (verComp >= 0)
            {
                strTransFlag = "111111111100"; //strTransFlag = "TransData   AttLog  OpLog   AttPhoto    EnrollUser  EnrollFP    USERPIC ChgUser ChgFP   FACE";
                Response.Write("ATTLOGStamp=9999" + '\r');
                Response.Write("Stamp=9999" + '\r');
                Response.Write("OPERLOGStamp=9999" + '\r');
                Response.Write("OpStamp=" + objDevice.OpStamp + '\r');
                Response.Write("ATTPHOTOStamp=" + objDevice.OpStamp + '\r');
                Response.Write("PhotoStamp=" + objDevice.OpStamp + '\r');
            }
            else
            {
                strTransFlag = "111111111100";
                Response.Write("ATTLOGStamp=9999" + '\r');
                Response.Write("Stamp=9999" + '\r');
                Response.Write("OPERLOGStamp=9999" + '\r');
                Response.Write("OpStamp=" + objDevice.OpStamp + '\r');
                Response.Write("ATTPHOTOStamp=" + objDevice.OpStamp + '\r');
                Response.Write("PhotoStamp=" + objDevice.OpStamp + '\r');
            }
            Response.Write("ErrorDelay=60\r");
            Response.Write("Delay=30\r");
            Response.Write("TransTimes=18:20;18:25\r");
            Response.Write("TransInterval=1\r");
            Response.Write("TransFlag=" + strTransFlag + '\r');

            Response.Write("Realtime=1\r");
            Response.Write("TimeOut=60\r");
            Response.Write("TimeZone=" + objDevice.Timezone + '\r');
            Response.Write("Encrypt=0\r\r");
            Response.Write("ServerVer=2\r");

            Response.Write("ok");
            this.Response.Flush();
            this.Response.End();

            return true;
            ////DeviceModel objDevice = new DeviceModel();
            ////try
            ////{
            ////    objDevice = objEsslData.GetDeviceConfigDetailsBySerialNumber(SerialNumber);
            ////}
            ////catch (Exception ex)
            ////{
            ////    objEsslData.InsertTestData("Get Device Configuration : " + ex.Message);
            ////}
            ////string str = "GET OPTION FROM: " + SerialNumber + "\r";
            ////this.Response.Write("ATTLOGStamp=9999\r");
            ////this.Response.Write("OPERLOGStamp=9999\r");
            ////this.Response.Write("ATTPHOTOStamp=9999\r");
            ////this.Response.Write("ErrorDelay=60\r");
            ////this.Response.Write("Delay=30\r");
            ////this.Response.Write("TransTimes=18:20;18:30\r");
            ////this.Response.Write("TransInterval=1\r");
            ////this.Response.Write("TransFlag=TransData AttLog\r");
            ////this.Response.Write("TimeOut=60\r");

            ////this.Response.Write("TimeZone=330\r");
            ////this.Response.Write("Realtime=1\r");
            ////this.Response.Write("Encrypt=0\r\r");

            ////this.Response.Flush();
            ////this.Response.End();
            //if (objDevice.OpStamp == "0")
            //    PushVersion = "";
            //if (true)
            //{
            //    this.Response.Write("ATTLOGStamp=9999\r");
            //    this.Response.Write("OPERLOGStamp=9999\r");
            //    this.Response.Write("ATTPHOTOStamp=9999\r");
            //    this.Response.Write("ErrorDelay=60\r");
            //    this.Response.Write("Delay=30\r");
            //    this.Response.Write("TransTimes=18:20;18:30\r");
            //    this.Response.Write("TransInterval=1\r");
            //    this.Response.Write("TransFlag=TransData AttLog\r");
            //    this.Response.Write("TimeOut=60\r");

            //    this.Response.Write("TimeZone=330\r");
            //    this.Response.Write("Realtime=1\r");
            //    this.Response.Write("Encrypt=0\r\r");
            //    //this.Response.Write("Stamp=" + objDevice.TransactionStamp + "\r");
            //    //this.Response.Write("OpStamp=" + objDevice.OpStamp + "\r");
            //    //this.Response.Write("PhotoStamp=" + objDevice.OpStamp + "\r");
            //    //this.Response.Write("ErrorDelay=60\r");
            //    //this.Response.Write("Delay=30\r");
            //    //this.Response.Write("TransTimes=18:20;18:25\r");
            //    //this.Response.Write("TransInterval=1\r");
            //    //this.Response.Write("TransFlag=111111100000\r");
            //    //this.Response.Write("Realtime=1\r");
            //    //this.Response.Write("TimeOut=60\r");
            //    //this.Response.Write("TimeZone=" + objDevice.Timezone + "\r");
            //    //this.Response.Write("Encrypt=0\r\r");
            //}
            //else
            //{
            //    if (objDevice.TransactionStamp == "0")
            //    {
            //        this.Response.Write("Stamp=" + objDevice.TransactionStamp + "\r");
            //        this.Response.Write("ATTLOGStamp=" + objDevice.TransactionStamp + "\r");
            //        this.Response.Write("OpStamp=" + objDevice.OpStamp + "\r");
            //        this.Response.Write("OPERLOGStamp=" + objDevice.OpStamp + "\r");
            //    }
            //    else if (objDevice.OpStamp == "0")
            //    {
            //        this.Response.Write("Stamp=9999\r");
            //        this.Response.Write("ATTLOGStamp=9999\r");
            //        this.Response.Write("OpStamp=" + objDevice.OpStamp + "\r");
            //        this.Response.Write("OPERLOGStamp=" + objDevice.OpStamp + "\r");
            //    }
            //    else
            //    {
            //        this.Response.Write("Stamp=" + objDevice.TransactionStamp + "\r");
            //        this.Response.Write("ATTLOGStamp=" + objDevice.TransactionStamp + "\r");
            //        this.Response.Write("OpStamp=" + objDevice.OpStamp + "\r");
            //        this.Response.Write("OPERLOGStamp=" + objDevice.OpStamp + "\r");
            //    }
            //    this.Response.Write("ATTPHOTOStamp=9999\r");
            //    this.Response.Write("ErrorDelay=60\r");
            //    this.Response.Write("Delay=30\r");
            //    this.Response.Write("TransTimes=18:20;18:30\r");
            //    this.Response.Write("TransInterval=1\r");
            //    this.Response.Write("TransFlag=111111100000\r");
            //    this.Response.Write("TimeOut=60\r");
            //    this.Response.Write("TimeZone=" + objDevice.Timezone + "\r");
            //    this.Response.Write("Realtime=1\r");
            //    this.Response.Write("Encrypt=0\r\r");
            //    this.Response.Write("ok");
            //}

        }
        public static int CompareVersion(string version1, string version2)
        {
            if ((version1 == null) || (version2 == null))
            {
                throw new Exception("compareVersion error:illegal params.");
            }
            string[] versionArray1 = version1.Split(".".ToCharArray());
            string[] versionArray2 = version2.Split(".".ToCharArray());
            int idx = 0;
            int minLength = Math.Min(versionArray1.Length, versionArray2.Length);
            int diff = 0;
            while ((idx < minLength) &&
              ((diff = Convert.ToInt32(versionArray1[idx]) - Convert.ToInt32(versionArray2[idx])) == 0) &&
              ((diff = versionArray1[idx].CompareTo(versionArray2[idx])) == 0))
            {
                idx++;
            }
            diff = diff != 0 ? diff : versionArray1.Length - versionArray2.Length;
            return diff;
        }
        public bool AddDeviceLogsForWebMonthwise(string SerialNumber, string strData, string TransactionStamp)
        {
            DeviceAttendanceBulkModel objModel = new DeviceAttendanceBulkModel();
            objModel.Logs = new List<DeviceLogModel>();
            objModel.TransactionStamp = TransactionStamp;
            objModel.SerialNumber = SerialNumber;
            strData = strData.Replace("\n", "\r\n");
            string[] strArray1 = strData.Split('\r');
            int num1 = 0;
            int num2 = 0;
            int index = 0;
            bool finalResult = true;
            while (index < checked(strArray1.Length - 1))
            {
                strArray1[index] = strArray1[index].Replace("\t", "\t");
                string[] strArray2 = strArray1[index].Split('\t');
                if (strArray2.Length > 1)
                {
                    DeviceLogModel objDeviceLogs = new DeviceLogModel();
                    objDeviceLogs.DeviceDirection = "altinout";
                    objDeviceLogs.DeviceEmpCode = strArray2[0].Trim();
                    objDeviceLogs.DeviceEmpCode = objDeviceLogs.DeviceEmpCode.Replace("\n", "");
                    objDeviceLogs.LogDate = strArray2[1];
                    string Left1 = strArray2[2];
                    objDeviceLogs.PunchDirectionId = CommonUsage.ConvertToInt(Left1);
                    objDeviceLogs.DeviceDirection = Left1 != "0" ? "out" : "in";
                    string str1;
                    try
                    {
                        str1 = strArray2[2];
                    }
                    catch (Exception ex)
                    {

                        objEsslData.InsertTestData("Insert Attendence  start: " + ex.Message);

                    }
                    string str2;
                    try
                    {
                        str2 = strArray2[3];
                    }
                    catch (Exception ex)
                    {

                        objEsslData.InsertTestData("Insert Attendence  M1: " + ex.Message);

                    }
                    try
                    {
                        objDeviceLogs.WorkCode = strArray2[4].Trim();
                    }
                    catch (Exception ex)
                    {

                        objEsslData.InsertTestData("Insert Attendence  M2: " + ex.Message);

                    }
                    objDeviceLogs.DownloadDate = CommonUsage.GetCurrentDate();
                    objModel.Logs.Add(objDeviceLogs);
                }
                index++;
            }
            try
            {
                objEsslData.UpdateBulkAttandance(objModel);
                finalResult = true;
            }
            catch (Exception ex)
            {
                objEsslData.InsertTestData("Insert Attendence  end: " + ex.Message);
                finalResult = false;
            }
            return finalResult;
        }
        public bool AddDeviceLogsForWebMonthwiseOld(string SerialNumber, string strData, string TransactionStamp)
        {
            strData = strData.Replace("\n", "\r\n");
            string[] strArray1 = strData.Split('\r');
            int num1 = 0;
            int num2 = 0;
            int index = 0;
            bool finalResult = true;
            while (index < checked(strArray1.Length - 1))
            {
                strArray1[index] = strArray1[index].Replace("\t", "\t");
                string[] strArray2 = strArray1[index].Split('\t');
                if (strArray2.Length > 1)
                {
                    DeviceLogModel objDeviceLogs = new DeviceLogModel();
                    objDeviceLogs.SerialNumber = SerialNumber;
                    objDeviceLogs.DeviceDirection = "altinout";
                    objDeviceLogs.DeviceEmpCode = strArray2[0].Trim();
                    objDeviceLogs.DeviceEmpCode = objDeviceLogs.DeviceEmpCode.Replace("\n", "");
                    objDeviceLogs.LogDate = strArray2[1];
                    string Left1 = strArray2[2];
                    objDeviceLogs.PunchDirectionId = CommonUsage.ConvertToInt(Left1);
                    objDeviceLogs.DeviceDirection = Left1 != "0" ? "out" : "in";
                    string str1;
                    try
                    {
                        str1 = strArray2[2];
                    }
                    catch (Exception ex)
                    {

                    }
                    string str2;
                    try
                    {
                        str2 = strArray2[3];
                    }
                    catch (Exception ex)
                    {

                    }
                    try
                    {
                        objDeviceLogs.WorkCode = strArray2[4].Trim();
                    }
                    catch (Exception ex)
                    {
                    }
                    objDeviceLogs.DownloadDate = CommonUsage.GetCurrentDate();
                    int Status = 0;
                    try
                    {
                        DeviceAttandancePunchModel objModel = objEsslData.UpdateAttandanceN(objDeviceLogs, TransactionStamp);
                        Status = objModel.DeviceName != "" ? 1 : 0;
                        if (objModel.DeviceID != 0 && objModel.EmployeeType != 1)
                        {
                            //string attdata = "(" + objModel.DeviceID + ",'" + objDeviceLogs.DeviceEmpCode + "','" + objDeviceLogs.LogDate + "','" + objDeviceLogs.DownloadDate.ToString("yyy-MM-dd HH:mm:ss") + "')";

                            //BeehiveAttService.AttendanceServiceClient bAtt = new BeehiveAttService.AttendanceServiceClient();
                            //bool bStatus = bAtt.SaveAttendanceSQL(CommonUsage.BeehiveSecretCode, CommonUsage.BeehiveAPIKey, attdata);
                        }
                    }
                    catch (Exception ex)
                    {
                        objEsslData.InsertTestData("AttIns Error : " + strData + ex.Message);
                    }
                    if (Status == 0)
                    {
                        finalResult = false;
                    }
                }
                index++;
            }
            return finalResult;
        }
        //public void UpdateOperations()
        //{
        //    string str1 = this.Request.QueryString["SN"];
        //    string str2 = "";
        //    int length = checked((int)this.Request.InputStream.Length);
        //    if (this.Request.InputStream.Length != (long)this.Request.ContentLength)
        //    {
        //        this.Response.Write("Error Occured\r");
        //        this.Response.Flush();
        //        this.Response.End();
        //    }
        //    byte[] numArray = new byte[checked(length + 1)];
        //    int offset = 0;
        //    while (offset < length)
        //        checked { offset += this.Request.InputStream.Read(numArray, offset, 2048); }
        //    int contentLength = this.Request.ContentLength;
        //    string str3 = Encoding.ASCII.GetString(numArray);
        //    this.Request.QueryString.GetKey(1);
        //    string OpStamp = this.Request.QueryString["OpStamp"];
        //    if (!this.AddDeviceOperationLogs(str3, str1))
        //        this.Response.Redirect("aaaaaaaaaaa.aspx");
        //    if (!this.ExecuteOperation(str1, str3))
        //        this.Response.Redirect("aaaaaaaaaaa.aspx");
        //    if (!this.UpdateDeviceOpStamp(str1, OpStamp))
        //        this.Response.Redirect("aaaaaaaaaaa.aspx");
        //    str2 = "OK:1\r";
        //    this.Response.Write("OK\r");
        //    this.Response.Flush();
        //    this.Response.End();
        //}
        public bool ExecuteOperation(string SerialNumber, string strData)
        {
            strData = strData.Replace("\n", "\r\n");
            string[] strArray1 = strData.Split('\r');
            int index = 0;
            while (index <= checked(strArray1.Length - 1))
            {
                string[] strArray2 = strArray1[index].Split('\t');
                if (strArray2[0].Contains("USER PIN"))
                {
                    if (!AddUsers(SerialNumber, strArray1[index]))
                        return false;
                }
                else if (strArray2[0].Contains("FP PIN"))
                {
                    if (!this.AddUserFPs(strArray1[index]))
                        return false;
                }
                else if (strArray2[0].Contains("FACE PIN"))
                {
                    if (!this.AddUserFace(strArray1[index]))
                        return false;
                }
                else if (strArray2[0].Contains("OPLOG 9"))
                {
                    //if (!this.DeleteUsersByUserpin(strArray1[index]))
                    //    return false;
                }
                //else if (strArray2[0].Contains("OPLOG 13") && !this.DeleteAllUsers())
                //    return false;
                checked { ++index; }
            }
            return true;
        }
        public bool AddDeviceOperationLogs(string strdata, string SerialNo)
        {
            string strdatao = strdata;
            strdata = strdata.Replace("\n", "\r\n");
            string[] strArray1 = strdata.Split('\r');
            int index = 0;
            while (index <= checked(strArray1.Length - 1))
            {
                string[] strArray2 = strArray1[index].Split('\t');
                if (strArray2[0].Contains("OPLOG"))
                {
                    try
                    {
                        objEsslData.UpdateDeviceLog(strArray2[0].Replace("OPLOG ", ""), strArray2[2], SerialNo);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        objEsslData.InsertTestData("OpLog :" + ex.Message + "--" + strdata);
                        return false;
                    }
                }
                else
                {
                    try
                    {
                        return ExecuteOperation(SerialNo, strdatao);
                    }
                    catch (Exception ex)
                    {
                        objEsslData.InsertTestData("Execute Operation :" + ex.Message + "--" + strdatao);
                    }
                }
                checked { ++index; }
            }
            return true;
        }
        public bool AddUsers(string SerialNo, string strData)
        {
            string str1 = "";
            strData = strData.Replace("\n", "\r\n");
            string[] strArray1 = strData.Split('\r');
            int index1 = 0;
            bool flag = true;
            try
            {
                while (index1 < strArray1.Length)
                {
                    if (strArray1[index1].Split('=')[0].Contains("USER PIN"))
                    {
                        strArray1[index1] = strArray1[index1].Replace("\t", "\t");
                        string[] strArray2 = strArray1[index1].Split('\t');
                        EmployeeModel employee = new EmployeeModel();
                        employee.EmployeeSID = strArray2[0].Replace("USER PIN=", "");
                        employee.EmployeeSID = employee.EmployeeSID.Replace("\n", "").Trim();
                        employee.EmployeeName = strArray2[1].Replace("Name=", "").Trim();
                        int index2 = 0;
                        try
                        {
                            while (index2 < strArray2.Length)
                            {
                                if (strArray2[index2].Contains("Passwd="))
                                    employee.DevicePassword = strArray2[index2].Replace("Passwd=", "");
                                else if (strArray2[index2].Contains("Card="))
                                    employee.AccessCardNo = strArray2[index2].Replace("Card=", "");
                                else if (strArray2[index2].Contains("Grp="))
                                    employee.DeviceGroup = strArray2[index2].Replace("Grp=", "");
                                else if (strArray2[index2].Contains("Pri="))
                                    str1 = strArray2[index2].Replace("Pri=", "");
                                checked { ++index2; }
                            }

                            objEsslData.UpdateEmployeeFromDevice(employee);
                        }
                        catch (Exception ex)
                        {
                            objEsslData.InsertTestData("Add User 1:" + ex.Message);
                            flag = false;
                        }
                    }
                    checked { ++index1; }
                }
            }
            catch (Exception ex)
            {
                objEsslData.InsertTestData("Add User 2:" + ex.Message);
                flag = false;
            }
            return flag;
        }
        public bool AddUserFPs(string strData)
        {
            Dictionary<string, UpdateEmployeeBioModel> kvFingerprints = new Dictionary<string, UpdateEmployeeBioModel>();
            string Password = "";
            string Card = "";
            int Group = 0;
            string Face = "";
            strData = strData.Replace("\n", "\r\n");
            string[] strArray1 = strData.Split('\r');
            int index = 0;
            bool flag = true;
            try
            {
                while (index < strArray1.Length)
                {
                    if (strArray1[index].Split('=')[0].Contains("FP PIN"))
                    {
                        EmployeeModel employee = new EmployeeModel();
                        strArray1[index] = strArray1[index].Replace("\t", "\t");
                        string[] strArray2 = strArray1[index].Split('\t');
                        employee.EmployeeSID = strArray2[0].Replace("FP PIN=", "").Trim();
                        employee.EmployeeSID = employee.EmployeeSID.Replace("\n", "").Trim();
                        string str1 = strArray2[1].Replace("FID=", "");
                        string BioVer = CommonUsage.ConvertToInt(strArray2[2].Replace("Size=", "")) >= 700 ? "10" : "9";
                        string str2 = strArray2[4].Replace("TMP=", "");
                        EmployeeBioModel objFP = new EmployeeBioModel();
                        objFP.Bio = str2;
                        objFP.BioType = "Fingerprint";
                        objFP.BioVersion = BioVer;

                        if (kvFingerprints.ContainsKey(employee.EmployeeSID))
                        {
                            objFP.BioID = str1 == "-1" ? kvFingerprints[employee.EmployeeSID].Fingerprints.Count + 1 : Convert.ToInt32(str1);

                            kvFingerprints[employee.EmployeeSID].Fingerprints.Add(objFP);
                        }
                        else
                        {
                            objFP.BioID = str1 == "-1" ? 1 : Convert.ToInt32(str1);
                            UpdateEmployeeBioModel objwModel = new UpdateEmployeeBioModel();
                            objwModel.EmployeeCode = employee.EmployeeSID;
                            objwModel.UpdatedDate = CommonUsage.GetCurrentDate();
                            objwModel.Fingerprints = new List<EmployeeBioModel>();
                            objwModel.Fingerprints.Add(objFP);
                            kvFingerprints.Add(employee.EmployeeSID, objwModel);
                        }
                    }
                    checked { ++index; }
                }
                foreach (var wfp in kvFingerprints)
                {
                    try
                    {
                        objEsslData.UpdateEmployeeBioDetails(wfp.Value);
                    }
                    catch (Exception ex)
                    {
                        objEsslData.InsertTestData("Add User FP i:" + wfp.Value.EmployeeCode + "$$" + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                objEsslData.InsertTestData("Add User FP : " + ex.Message);

                flag = false;

            }
            return flag;
        }
        public bool AddUserFace(string strData)
        {
            Dictionary<string, UpdateEmployeeBioModel> kvBios = new Dictionary<string, UpdateEmployeeBioModel>();
            string Password = "";
            string Card = "";
            int Group = 0;
            string Face = "";
            strData = strData.Replace("\n", "\r\n");
            string[] strArray1 = strData.Split('\r');
            int index = 0;
            bool flag = true;
            try
            {
                while (index < strArray1.Length)
                {
                    if (strArray1[index].Split('=')[0].Contains("FACE PIN"))
                    {
                        EmployeeModel employee = new EmployeeModel();
                        strArray1[index] = strArray1[index].Replace("\t", "\t");
                        string[] strArray2 = strArray1[index].Split('\t');
                        employee.EmployeeSID = strArray2[0].Replace("FACE PIN=", "").Trim();
                        employee.EmployeeSID = employee.EmployeeSID.Replace("\n", "").Trim();
                        string str1 = strArray2[1].Replace("FID=", "");
                        string BioVer = CommonUsage.ConvertToInt(strArray2[2].Replace("Size=", "")) >= 700 ? "10" : "9";
                        string sBio = strArray2[4].Replace("TMP=", "");
                        EmployeeBioModel objFP = new EmployeeBioModel();
                        objFP.Bio = sBio;
                        objFP.BioType = "FACE";
                        objFP.BioVersion = BioVer;

                        if (kvBios.ContainsKey(employee.EmployeeSID))
                        {
                            objFP.BioID = str1 == "-1" ? kvBios[employee.EmployeeSID].Fingerprints.Count + 1 : Convert.ToInt32(str1);

                            kvBios[employee.EmployeeSID].Fingerprints.Add(objFP);
                        }
                        else
                        {
                            objFP.BioID = str1 == "-1" ? 1 : Convert.ToInt32(str1);
                            UpdateEmployeeBioModel objwModel = new UpdateEmployeeBioModel();
                            objwModel.EmployeeCode = employee.EmployeeSID;
                            objwModel.UpdatedDate = CommonUsage.GetCurrentDate();
                            objwModel.Fingerprints = new List<EmployeeBioModel>();
                            objwModel.Fingerprints.Add(objFP);
                            kvBios.Add(employee.EmployeeSID, objwModel);
                        }
                    }
                    checked { ++index; }
                }
                foreach (var wBio in kvBios)
                {
                    try
                    {
                        objEsslData.UpdateEmployeeBioDetails(wBio.Value);
                    }
                    catch (Exception ex)
                    {
                        objEsslData.InsertTestData("Add User FACE i:" + wBio.Value.EmployeeCode + "$$" + ex.Message);
                        flag = false;
                    }
                }
            }
            catch (Exception ex)
            {
                objEsslData.InsertTestData("Add User FACE : " + ex.Message);

                flag = false;

            }
            return flag;
        }
    }
}