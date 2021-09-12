using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using SMEnterprise.Models;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Newtonsoft.Json;

namespace SMEnterprise.Repository
{
    public class SMSBalanceModel
    {
        public string ROUTE_ID { get; set; }
        public string ROUTE { get; set; }
        public int BALANCE { get; set; }
    }
    public class SMSSender
    {
        ////Changes Regarding SMS Balance
        public static List<SMSBalanceModel> GetSMSBalance(int SBranchID, SMSConfigirationModel SMSConfigiration)
        {
           
            WebClient httpclient = new WebClient();
            httpclient.Headers.Add("user-agent", SMSConfigiration.header);

            Stream data = httpclient.OpenRead(SMSConfigiration.balanceURL);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();
            List<SMSBalanceModel> lst = JsonConvert.DeserializeObject<List<SMSBalanceModel>>(s);
            return lst;
        }
        //SMSType 1-Notice, 2-Holiday
        public delegate void DelegatSMSSender(string text, string mobile, int SBranchID,int SMSType,int RecieverType,int RecieverID, string ContentID);

        public void SendSMSAsync(string text, string mobileNo, int SBranchID, int SMSType, int RecieverType, int RecieverID, string ContentID)
        {
            
            DelegatSMSSender smsdeligate = this.SendSMS;
            smsdeligate.BeginInvoke(text, mobileNo, SBranchID,  SMSType,  RecieverType,  RecieverID, ContentID, null, null);
        }
       
        public void SendSMS(string text,string mobileNo,int SBranchID, int SMSType, int RecieverType, int RecieverID,string ContentID)
        {
            SMSConfigirationModel SMSConfigiration= (new AdminData()).GetDefaultSMSConfigurationDetails(SBranchID);
            CommonData objData = new CommonData();
            try
            {
                WebClient httpclient = new WebClient();
                try
                {
                    httpclient.Headers.Add("user-agent", SMSConfigiration.header);
                }
                catch (Exception ex)
                {
                    SMSStatusModel objModel = new SMSStatusModel();
                    objModel.MobileNumber = mobileNo;
                    objModel.ReasonFailure = ex.Message;
                    objModel.RecieverID = RecieverID;
                    objModel.RecieverType = RecieverType;
                    objModel.SMSDateTime = CommonUsage.GetCurrentDate();
                    objModel.SMSText = text;
                    objModel.SMSType = SMSType;
                    objModel.Content_id = ContentID;
                   
                    objData.UpdateSMSFailure(objModel);
                }
                foreach (SMSConfigirationParamModel param in SMSConfigiration.Params)
                {
                    if (param.ParamType == 0)
                    {
                        httpclient.QueryString.Add(param.ParamName, param.ParamValue);
                    }
                    else if (param.ParamType == 1)
                    {
                        httpclient.QueryString.Add(param.ParamName, mobileNo);
                    }
                    else if (param.ParamType ==2)
                    {
                        httpclient.QueryString.Add(param.ParamName, text);
                    }
                }
                string baseurl = SMSConfigiration.baseurl;
                Stream data = httpclient.OpenRead(baseurl);
                StreamReader reader = new StreamReader(data);
                string s = reader.ReadToEnd();
                data.Close();
                reader.Close();
                //string s = "Success";
                //System.Threading.Thread.Sleep(5000);
                if (!s.Contains("ERR"))
                {
                    SMSStatusModel objModel = new SMSStatusModel();
                    objModel.MobileNumber = mobileNo;
                    objModel.ReasonFailure = s;
                    objModel.RecieverID = RecieverID;
                    objModel.RecieverType = RecieverType;
                    objModel.SMSDateTime = CommonUsage.GetCurrentDate();
                    objModel.SMSText = text;
                    objModel.SMSType = SMSType;
                    objModel.Content_id = ContentID;
                    objData.UpdateSMSFailure(objModel);

                    objData.UpdateSMSCount(SBranchID);
                }
                else
                {
                    SMSStatusModel objModel = new SMSStatusModel();
                    objModel.MobileNumber = mobileNo;
                    objModel.ReasonFailure = s;
                    objModel.RecieverID = RecieverID;
                    objModel.RecieverType = RecieverType;
                    objModel.SMSDateTime = CommonUsage.GetCurrentDate();
                    objModel.SMSText = text;
                    objModel.SMSType = SMSType;
                    objModel.Content_id = ContentID;
                    objData.UpdateSMSFailure(objModel);
                }
            }
            catch(Exception ex)
            {
                SMSStatusModel objModel = new SMSStatusModel();
                objModel.MobileNumber = mobileNo;
                objModel.ReasonFailure = ex.Message;
                objModel.RecieverID = RecieverID;
                objModel.RecieverType = RecieverType;
                objModel.SMSDateTime = CommonUsage.GetCurrentDate();
                objModel.SMSText = ex.Message;
                objModel.SMSType = SMSType;
                objModel.Content_id = ContentID;
                objData.UpdateSMSFailure(objModel);
            }
        }
        public static void SendSMSStatic(string text, string mobileNo, int SBranchID)
        {
            SMSConfigirationModel SMSConfigiration = (new AdminData()).GetDefaultSMSConfigurationDetails(SBranchID);
            try
            {
                WebClient httpclient = new WebClient();
              
                    httpclient.Headers.Add("user-agent", SMSConfigiration.header);
                foreach (SMSConfigirationParamModel param in SMSConfigiration.Params)
                {
                    if (param.ParamType == 0)
                    {
                        httpclient.QueryString.Add(param.ParamName, param.ParamValue);
                    }
                    else if (param.ParamType == 1)
                    {
                        httpclient.QueryString.Add(param.ParamName, mobileNo);
                    }
                    else if (param.ParamType == 2)
                    {
                        httpclient.QueryString.Add(param.ParamName, text);
                    }
                }
                string baseurl = SMSConfigiration.baseurl;
                Stream data = httpclient.OpenRead(baseurl);
                StreamReader reader = new StreamReader(data);
                string s = reader.ReadToEnd();
                data.Close();
                reader.Close();
                //string s = "Success";
                //System.Threading.Thread.Sleep(5000);
               
            }
            catch (Exception ex)
            {
              
            }
        }
        public AbsentStudentListModel GetAbsentStudentSMSDetails()
        {
            AbsentStudentListModel objmodel = new AbsentStudentListModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Year", CommonUsage.GetCurrentDate().Year);
                paramater.Add("@Month", CommonUsage.GetCurrentDate().Month);
                paramater.Add("@Day", CommonUsage.GetCurrentDate().Day);
                objmodel.AbsentStudents= con.Query<AbsentStudentModel>("spn_GetAbsentStudentsDetails", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
            return objmodel;
        }
        public AbsentStudentModel GetStudentSMSNotificationDetails( int StudentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                return con.Query<AbsentStudentModel>("spn_GetStudentSMSNotificationDetails", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
    }
    public class SMSStatusModel
    {
        public int SMSType { get; set; }
        public int RecieverType { get; set; }
        public int RecieverID { get; set; }
        public string MobileNumber { get; set; }
        public string SMSText { get; set; }
        public string ReasonFailure { get; set; }
        public DateTime SMSDateTime { get; set; }
        public int Status { get; set; }
        public int SMSID { get; set; }
        public string Content_id { get; set; }
        public string SMSContentID { get; set; }
    }
    public class SMSRecieverModel
    {
        public int RecieverType { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string deviceToken { get; set; }
        public string Content_id { get; set; }
        public string SMSContentID { get; set; }
    }
    public class StudentBirthdaySMSModel
    {
        public int StudentID { get; set; }
        public string Name { get; set; }
        public int Gender { get; set; }
        public string MobileNo { get; set; }
        public string Photo { get; set; }
        public int NotificationSMSTo { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
    }
}