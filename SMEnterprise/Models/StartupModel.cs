using Dapper;
using System;
using System.Collections.Generic;
using System.Data;

namespace SMEnterprise.Models
{
    public class StartupModel
    {
        public int HostelFeeMode { get; set; }
        public int TransportFeeMode { get; set; }
        public DateTime SessionStartDate { get; set; }
        public DateTime SessionEndDate { get; set; }
        public int EvaluationMode { get; set; }
        public string AbsentNotificationTime { get; set; }
        public string AbsentSMSTemplate { get; set; }
        public int StudentInOutSMSNotification { get; set; }
        public string SchemaPrefix { get; set; }
        public string StudentInSMSTemplate { get; set; }
        public string StudentOutSMSTemplate { get; set; }
        public int SMSForAbsentStudents { get; set; }
        public int FeePaymentReminderDate { get; set; }
        public int FeePaymentReminderSMS { get; set; }
        public int MapServerKeyToken { get; set; }

        public string FeePaymentReminderSMSTemplate { get; set; }

        public string FeePaymentNotificationSMSTemplate { get; set; }
        public string TeacherRemoveSubstitutedSMSTemplate = "Dear [NamePlaceHolder],%0a,Your Substitution for [ClassPlaceHolder]/[SectionPlaceHolder] on [DatePlaceHolder]/[DayPlaceHolder]/[PeriodPlaceHolder] is Canceled.";
        public string TeacherSubstitutedSMSTemplate = "Dear [NamePlaceHolder],%0aKindly take lacture of [SubjectPlaceHolder] in [ClassPlaceHolder]/[SectionPlaceHolder] in place of [ReplacedTeacher] on [DatePlaceHolder]/[DayPlaceHolder]/[PeriodPlaceHolder]";

        public static string sTeacherRemoveSubstitutedSMSTemplate = "Dear [NamePlaceHolder],%0a,Your Substitution for [ClassPlaceHolder]/[SectionPlaceHolder] on [DatePlaceHolder]/[DayPlaceHolder]/[PeriodPlaceHolder] is Canceled.";
        public static string sTeacherSubstitutedSMSTemplate = "Dear [NamePlaceHolder],%0aKindly take lacture of [SubjectPlaceHolder] in [ClassPlaceHolder]/[SectionPlaceHolder] in place of [ReplacedTeacher] on [DatePlaceHolder]/[DayPlaceHolder]/[PeriodPlaceHolder]";


    }
    public class StartupModelUpdate
    {
        public List<Type_Value> MasterSettings { get; set; }
        public DataTable GetSettingsDataTable()
        {

            DataTable dtSetupDetails = new DataTable();
            dtSetupDetails.SetTypeName("Type_Value");
            dtSetupDetails.Columns.Add("Type");
            dtSetupDetails.Columns.Add("Value");

            foreach (Type_Value e in MasterSettings)
            {
                DataRow dr = dtSetupDetails.NewRow();
                dr["Type"] = e.Type;
                dr["Value"] = e.Value;

                dtSetupDetails.Rows.Add(dr);
            }

            return dtSetupDetails;
        }
    }
    public class Type_Value
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}