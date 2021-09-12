using Dapper;
using System;
using System.Collections.Generic;
using System.Data;

namespace SMEnterprise.Models
{
    public class SMSConfigirationModel
    {
        public int SMSConfigID { get; set; }
        public int SBranchID { get; set; }
        public int IsDefault { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string header { get; set; }
        public string baseurl { get; set; }
        public string balanceURL { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OpType { get; set; }
        public List<SMSConfigirationParamModel> Params { get; set; }
        public DataTable GetParamsDataTable()
        {

            DataTable dtParamDetails = new DataTable();
            dtParamDetails.SetTypeName("ut_SMSConfigurationParams");
            dtParamDetails.Columns.Add("SMSParamID");
            dtParamDetails.Columns.Add("SMSConfigID");
            dtParamDetails.Columns.Add("ParamType");
            dtParamDetails.Columns.Add("ParamName");
            dtParamDetails.Columns.Add("ParamValue");
            dtParamDetails.Columns.Add("OpType");

            foreach (SMSConfigirationParamModel e in Params)
            {
                DataRow dr = dtParamDetails.NewRow();
                dr["SMSParamID"] = e.SMSParamID;
                dr["SMSConfigID"] = SMSConfigID;
                dr["ParamType"] = e.ParamType;
                dr["ParamName"] = e.ParamName;
                dr["ParamValue"] = e.ParamValue;
                dr["OpType"] = e.OpType;

                dtParamDetails.Rows.Add(dr);
            }

            return dtParamDetails;
        }
    }
    public class SMSConfigirationParamModel
    {
        public int SMSParamID { get; set; }
        public int SMSConfigID { get; set; }
        public int ParamType { get; set; }
        public string ParamName { get; set; }
        public string ParamValue { get; set; }
        public int OpType { get; set; }
    }
    public class RunningSMSTaskModel
    {
        public int TaskID { get; set; }
        public string Title { get; set; }
        public decimal Progress { get; set; }
        public int TotalCount { get; set; }
        public int CompletedCount { get; set; }
        public string RecieverList { get; set; }
    }
    public class SMSCreateModel
    {
        public List<NameIDModel> SMSTypes { get; set; }
        public List<SMSTemplateModel> Templates { get; set; }
        public List<SMSRecieverDetailModel> Recievers { get; set; }
        public List<NameIDModel> Classes { get; set; }
        public int SelectedSMSType { get; set; }
        public int SBranchID { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public DateTime Date { get; set; }
        public string ReciverCats { get; set; }
        public string RecieverList { get; set; }
        public string Title { get; set; }
        public string Content_id { get; set; }
    }
    public class SMSSendTaskModel
    {
        public List<SMSRecieverDetailModel> Recievers { get; set; }
        public int SMSTypeID { get; set; }
        public int SMSTemplateID { get; set; }
        public string TemplateText { get; set; }
        public int SMSSendingID { get; set; }
        public string RecieverCats { get; set; }
        public DateTime SMSSendDate { get; set; }
        public DateTime Date { get; set; }
        public DateTime EDate { get; set; }
        public int SelectedCount { get; set; }
        public string Title { get; set; }
        public string SMSTemplate { get; set; }
        public int Success { get; set; }
        public int Failed { get; set; }
        public int Pending { get; set; }
        public int Status { get; set; }
        public int Total { get; set; }
        public string RecieverList { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int SBranchID { get; set; }
        public string Content_id { get; set; }
        public string SMSContentID { get; set; }
        public DataTable GetRecieverDetailsDataTable()
        {

            DataTable dtDetails = new DataTable();
            dtDetails.SetTypeName("ut_SMSProcessingLog");
            dtDetails.Columns.Add("RecID");
            dtDetails.Columns.Add("SMSID");
            dtDetails.Columns.Add("SMSType");
            dtDetails.Columns.Add("RecieverType");
            dtDetails.Columns.Add("RecieverID");
            dtDetails.Columns.Add("MobileNumber");
            dtDetails.Columns.Add("ReasonFailure");
            dtDetails.Columns.Add("SMSDateTime");
            dtDetails.Columns.Add("Status");
            dtDetails.Columns.Add("Content_id");

            foreach (SMSRecieverDetailModel e in Recievers)
            {
                if (e.IsSelected == 1)
                {
                    DataRow dr = dtDetails.NewRow();
                    dr["RecieverType"] = e.RecieverType;
                    dr["RecieverID"] = e.RecieverID;
                    dr["MobileNumber"] = e.Mobile;
                    dr["Content_id"] = e.Content_id;
                    dtDetails.Rows.Add(dr);
                }
            }

            return dtDetails;
        }
    }
    public class SMSRecieverDetailModel
    {
        public int RecieverID { get; set; }
        public int RecieverType { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Photo { get; set; }
        public string Details { get; set; }
        public int IsSelected { get; set; }
        public int Gender { get; set; }
        public decimal Amount { get; set; }
        public string PayDate { get; set; }
        public int Status { get; set; }
        public string ReasonFailure { get; set; }
        public DateTime SMSDateTime { get; set; }
        public string SMSContentID { get; set; }
        public string Content_id { get; set; }
    }
    public class SMSRequestModel
    {
        public int SMSBalID { get; set; }
        public int SMSCredited { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime RechargeDate { get; set; }
        public int Status { get; set; }
        public int SBranchID { get; set; }
    }

    public class SMSTemplateModel
    {
        public int TemplateID { get; set; }
        public string Title { get; set; }
        public string Template { get; set; }
        public int SMSType { get; set; }
        public string SMSTypeName { get; set; }
        public int OpType { get; set; }
        public int IsApproved { get; set; }
        public string SMSContentID { get; set; }
        public string Content_id { get; set; }
    }
    public class SMSTemplatePageModel
    {
        public List<SMSTemplateModel> Templates { get; set; }
        public List<NameIDModel> Types { get; set; }
    }
}