
using Microsoft.AspNet.SignalR;
using SMEnterprise.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace SMEnterprise.Repository
{
    public class TaskDetails
    {
        public int SMSSendingID { get; set; }
        public int DeligateID { get; set; }
        public int Status { get; set; }
        public int ProcessCount { get; set; }
        public int TotalCount { get; set; }
        public string Details { get; set; }
        public string Title { get; set; }
        public string RecieverList { get; set; }
    }
    public class DeligateTasks
    {
        private static ConcurrentDictionary<string, TaskDetails> _CurrentTasks;
        private static ConcurrentDictionary<string, string> _CanceledTasks;
        public static ConcurrentDictionary<string, TaskDetails> CurrentTasks
        {
            get
            {
                if (_CurrentTasks == null)
                    _CurrentTasks = new ConcurrentDictionary<string, TaskDetails>();

                return _CurrentTasks;
            }
        }
        private static ConcurrentDictionary<string, string> CanceledTasks
        {
            get
            {
                if (_CanceledTasks == null)
                    _CanceledTasks = new ConcurrentDictionary<string, string>();

                return _CanceledTasks;
            }
        }
        public string StartSending(SMSSendTaskModel data, SMSConfigirationModel SMSConfiguration)
        {
            List<SMSRecieverDetailModel> finalSendingList = new List<SMSRecieverDetailModel>();
            foreach (SMSRecieverDetailModel r in data.Recievers)
            {
                if (r.IsSelected == 1)
                {
                    finalSendingList.Add(r);
                }

            }
            data.Recievers = finalSendingList;

            int TotalRecievers = data.Recievers.Count;

            int TotalThreads = 1;
            int SMSPerThread = TotalRecievers / TotalThreads;
            int SMSOverHead = TotalRecievers % TotalThreads;

            TaskDetails objTask = new TaskDetails();
            objTask.DeligateID = 0;
            objTask.ProcessCount = 0;
            objTask.SMSSendingID = data.SMSSendingID;
            objTask.Status = 0;
            objTask.TotalCount = TotalRecievers;
            objTask.Title = data.Title;
            objTask.RecieverList = data.RecieverList;
            CurrentTasks.TryAdd(data.SMSSendingID + "-" + 0, objTask);

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
            hubContext.Clients.All.TaskAdded(data.SMSSendingID, data.Title, data.RecieverList, TotalRecievers, data.Content_id);
            //DelegatBulkSMS smsdeligate = SendSMSMini;
            SendSMSMini(data, 0, TotalRecievers, 0, SMSConfiguration);

            return "1";
        }
        public void SendSMSMini(SMSSendTaskModel data, int StartIndex, int EndIndex, int DeligateID, SMSConfigirationModel SMSConfiguration)
        {
            foreach (SMSRecieverDetailModel r in data.Recievers)
            {
                #region Task Cancellation Handeling
                //if (CanceledTasks.Keys.Contains(data.SMSSendingID.ToString()) && CanceledTasks[data.SMSSendingID.ToString()] == "1")
                //{
                //    _CurrentTasks[data.SMSSendingID + "-" + DeligateID].Status = -1;
                //    var isCanceled = true;
                //    for (int c = 0; c < CommonUsage.ConcurrentSMSSenderDeligateCount; c++)
                //    {
                //       if( _CurrentTasks.ContainsKey(data.SMSSendingID + "-" + c))
                //        {
                //            if(_CurrentTasks[data.SMSSendingID + "-" + c].Status!=-1)
                //            {
                //                isCanceled = false;
                //                break;
                //            }
                //        }
                //    }
                //    if(isCanceled)
                //    {
                //        var hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                //        hubContext.Clients.All.GetCancelConfirmation(data.SMSSendingID , 1);
                //    }
                //    break;
                //}
                #endregion
                CurrentTasks[data.SMSSendingID + "-" + DeligateID].ProcessCount = _CurrentTasks[data.SMSSendingID + "-" + DeligateID].ProcessCount + 1;


                data.TemplateText = data.TemplateText.Replace("[Reciever]", "Parent");

                //  data.TemplateText = data.TemplateText.Replace("[DueAmount]", r.Amount.ToString("N2"));
                string SMSText = "";
                //if (data.SMSTypeID == 1)
                //{
                //    SMSText = data.TemplateText.Replace("[StudentName]", r.Name);
                //}
                if (data.SMSTypeID == 1)
                {
                    SMSText = data.TemplateText.Replace("[StudentName]", r.Name).Replace("[AbsentDate]", data.SMSSendDate.ToString("dd MMM yyyy"));
                }
                else if (data.SMSTypeID == 2)
                {
                    SMSText = data.TemplateText.Replace("[StudentName]", r.Name).Replace("[StartDate]", data.Date.ToString("dd MMM,yyyy")).Replace("[EndDate]", data.EDate.ToString("dd MMM,yyyy")).Replace("[Reason]", data.Title);
                }
                else if (data.SMSTypeID == 6)
                {
                    SMSText = data.TemplateText.Replace("[Date]", data.Date.ToString("dd MMM,yyyy")).Replace("[StartTime]", data.StartTime.Substring(0, 5)).Replace("[EndTime]", data.EndTime.Substring(0, 5));
                }
                else if (data.SMSTypeID == 3)
                {
                    SMSText = data.TemplateText.Replace("[StudentName]", r.Name).Replace("[DueAmount]", r.Amount.ToString("N2"));
                }
                else if (data.SMSTypeID != 3)
                {
                    SMSText = data.TemplateText.Replace("[StudentName]", r.Name);
                }
                else
                {
                    SMSText = data.TemplateText.Replace("[StudentName]", r.Name).Replace("[Amount]", r.Amount.ToString("N2")).Replace("[PayDate]", r.PayDate);
                }

                if (r.Mobile != null && r.Mobile != "")
                {
                    SubmitSMS(SMSText.TrimStart(), r.Mobile, data.SBranchID, data.SMSTypeID, r.RecieverType, r.RecieverID, data.SMSSendingID, data.Content_id, SMSConfiguration);

                }
                else
                {
                    AccountData objData = new AccountData();
                    SMSStatusModel objModel = new SMSStatusModel();
                    objModel.MobileNumber = r.Mobile;
                    objModel.ReasonFailure = "Mobile Number Not Specified";
                    objModel.RecieverID = r.RecieverID;
                    objModel.RecieverType = r.RecieverType;
                    objModel.SMSDateTime = CommonUsage.GetCurrentDate();
                    objModel.SMSText = SMSText;
                    objModel.SMSType = data.SMSTypeID;
                    objModel.SMSID = data.SMSSendingID;
                    objModel.Content_id = data.Content_id;
                    objModel.DLT_TE_ID = data.Content_id;

                    objModel.Status = 1;
                    objData.UpdateSMSProcessingStatus(objModel);
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                    hubContext.Clients.All.GetStatus(data.SMSSendingID + "-" + r.RecieverID, -1);
                    int CompletedCount = 0;
                    foreach (var task in CurrentTasks)
                    {
                        if (data.SMSSendingID == task.Value.SMSSendingID)
                        {
                            CompletedCount = CompletedCount + task.Value.ProcessCount;
                        }
                    }

                    hubContext.Clients.All.GetHeadStatus(data.SMSSendingID + "-" + r.RecieverID, CompletedCount);
                }
            }
        }
        public void SubmitSMS(string text, string mobileNo, int SBranchID, int SMSType, int RecieverType, int RecieverID, int SMSID, string ContentID, SMSConfigirationModel SMSConfiguration)
        {
            if (SBranchID == 0)
            {
                SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            }

            string msg = "";
            if (SMSConfiguration == null)
            {
                SMSConfiguration = (new AdminData()).GetDefaultSMSConfigurationDetails(SBranchID);
            }

            SMSConfiguration = (new AdminData()).GetDefaultSMSConfigurationDetails(SBranchID);
            string s = "";


            WebClient httpclient = new WebClient();
            httpclient.Headers.Add("user-agent", SMSConfiguration.header);
            foreach (SMSConfigirationParamModel param in SMSConfiguration.Params)
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
                else if (param.ParamType == 3)
                {
                    httpclient.QueryString.Add(param.ParamName, Convert.ToString(ContentID).ToString());
                }
            }
            string baseurl = SMSConfiguration.baseurl;
            Stream data = httpclient.OpenRead(baseurl);
            StreamReader reader = new StreamReader(data);
            s = reader.ReadToEnd();
            data.Close();
            reader.Close();

            AccountData objData = new AccountData();

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
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
                objModel.SMSID = SMSID;
                objModel.Content_id = ContentID;
                objModel.Status = 1;
                hubContext.Clients.All.GetStatus(SMSID + "-" + RecieverID, 1);
                try
                {
                    objData.UpdateSMSProcessingStatus(objModel);
                }
                catch (Exception ex)
                {

                }
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
                objModel.SMSID = SMSID;
                objModel.Status = -1;
                objData.UpdateSMSProcessingStatus(objModel);
                hubContext.Clients.All.GetStatus(SMSID + "-" + RecieverID, -1);

            }
            int CompletedCount = 0;
            int TotalCount = 0;
            foreach (var task in DeligateTasks.CurrentTasks)
            {
                if (SMSID == task.Value.SMSSendingID)
                {
                    CompletedCount = CompletedCount + task.Value.ProcessCount;
                    TotalCount = TotalCount + task.Value.TotalCount;
                }
            }

            hubContext.Clients.All.GetHeadStatus(SMSID + "-" + RecieverID, CompletedCount, TotalCount);
        }
    }
}