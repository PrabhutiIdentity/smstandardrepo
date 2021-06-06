using Dapper;
using SMEnterprise.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ESSL.Models
{
    public class DeviceModel
    {
        public int IsSelected { get; set; }
        public int OpType { get; set; }
        public int DeviceId { get; set; }
        public string DeviceFName { get; set; }
        public string DeviceSName { get; set; }
        public string DeviceDirection { get; set; }
        public string SerialNumber { get; set; }
        public string ConnectionType { get; set; }
        public string IpAddress { get; set; }
        public string BaudRate { get; set; }
        public string CommKey { get; set; }
        public string ComPort { get; set; }
        public DateTime LastLogDownloadDate { get; set; }
        public string C1 { get; set; }
        public string C2 { get; set; }
        public string C3 { get; set; }
        public string C4 { get; set; }
        public string C5 { get; set; }
        public string C6 { get; set; }
        public string C7 { get; set; }
        public string TransactionStamp { get; set; }
        public DateTime LastPing { get; set; }
        public string DeviceType { get; set; }
        public string OpStamp { get; set; }
        public int DownLoadType { get; set; }
        public string Timezone { get; set; }
        public string DeviceLocation { get; set; }
        public string TimeOut { get; set; }
        public int BranchID { get; set; }
        public int NetworkStrength { get; set; }
    }
    public class EsslModel
    {
    }
    public class EsslEmployeeModel
    {
        public int ShiftRosterId { get; set; }
        public string Location { get; set; }
        public string IsRecieveNotification { get; set; }
        public string Grade { get; set; }
        public string Team { get; set; }
        public string CompanyAddress { get; set; }
        public string EmployeeWorkPlace { get; set; }
        public string EmployeeExtensionNo { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string ResidentialAddress { get; set; }
        public string PermanentAddress { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Nomenee2 { get; set; }
        public string Nomenee1 { get; set; }
        public string CompanySName { get; set; }
        public string StringCode { get; set; }
        public string NumericCode { get; set; }
        public string Gender { get; set; }
        public string CategorySName { get; set; }
        public string DepartmentSName { get; set; }
        public int EmployeeId { get; set; }
        public int HolidayGroup { get; set; }
        public string EmployeeName { get; set; }
        public string EmployementType { get; set; }
        public string EmployeeCode { get; set; }
        public int DepartmentId { get; set; }
    }
    public class EEmployeeModel
    {
        public string ErrorMessage { get; set; }
        
        public int EmployeeID { get; set; }
        public string EmployeeFName { get; set; }
        public string EmployeeMName { get; set; }
        public string EmployeeLName { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeCodeInDevice { get; set; }
        public string EmployeeRFIDNumber { get; set; }
        public string Gender { get; set; }
        public string FatherName { get; set; }
        public DateTime DOJ { get; set; }
        public DateTime DOA { get; set; }
        public DateTime DOB { get; set; }
        public string ReportingManager { get; set; }
        public string ReportingManagerName { get; set; }
        public string ReportingHR { get; set; }
        public string UnitCode { get; set; }
        public string DeptCode { get; set; }
        public string ESINo { get; set; }
        public int ShiftID { get; set; }
        public DateTime ShiftChangeDate { get; set; }
        public string PFNo { get; set; }
        public string Pan_No { get; set; }
        public string EmailID { get; set; }
        public int Status { get; set; }
        public string Mob_No { get; set; }
        public int BranchID { get; set; }
        public string BranchName { get; set; }
        public List<SBranchModel> Branches { get; set; }
       
        public int OpType { get; set; }
    }
    public class EmployeeBioModel
    {
        public int EmployeeBioId { get; set; }
        public int BioID { get; set; }
        public string BioVersion { get; set; }
        public int EmployeeId { get; set; }
        public string Bio { get; set; }
        public string BioType { get; set; }
        public int OpType { get; set; }
    }
    public class LanourAttandanceReportModel
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string ContractorName { get; set; }
        public string PlantName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ShiftName { get; set; }
        public string DepartmentName { get; set; }
        public string LocationName { get; set; }
        public string DivisionName { get; set; }
        public string ZoneName { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
        public int HoursWorked { get; set; }
        public int ShiftHours { get; set; }
        public int Early { get; set; }
        public int OverStay { get; set; }
    }
    public class DeviceAttendanceBulkModel
    {
        public string SerialNumber { get; set; }
        public string TransactionStamp { get; set; }
        public List<DeviceLogModel> Logs { get; set; }
        public DataTable GetLogDetailsDataTable()
        {

            DataTable dtLogDetails = new DataTable();
            dtLogDetails.SetTypeName("ut_DeviceLogs");
            dtLogDetails.Columns.Add("DeviceLogId");
            dtLogDetails.Columns.Add("DownloadDate");
            dtLogDetails.Columns.Add("DeviceId");
            dtLogDetails.Columns.Add("UserId");
            dtLogDetails.Columns.Add("LogDate");
            dtLogDetails.Columns.Add("Direction");
            dtLogDetails.Columns.Add("AttDirection");
            dtLogDetails.Columns.Add("C1");
            dtLogDetails.Columns.Add("C2");
            dtLogDetails.Columns.Add("C3");
            dtLogDetails.Columns.Add("C4");
            dtLogDetails.Columns.Add("C5");
            dtLogDetails.Columns.Add("C6");
            dtLogDetails.Columns.Add("C7");
            dtLogDetails.Columns.Add("WorkCode");

            foreach (DeviceLogModel e in Logs)
            {
                DataRow dr = dtLogDetails.NewRow();
                dr["DeviceLogId"] = e.DeviceLogId;
                dr["DownloadDate"] = e.DownloadDate;
                dr["DeviceId"] = e.DeviceId;
                dr["UserId"] = e.DeviceEmpCode;
                dr["LogDate"] = e.LogDate;
                dr["Direction"] = e.DeviceDirection;
                dr["AttDirection"] = e.PunchDirectionId;
                dr["WorkCode"] = e.WorkCode;

                dtLogDetails.Rows.Add(dr);
            }

            return dtLogDetails;
        }
    }
    public class DeviceAttandancePunchModel
    {
        public int EmployeeType { get; set; }
        public int DeviceID { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public DateTime LogDate { get; set; }
        public string DeviceName { get; set; }
        public int Direction { get; set; }
    }
    public class UpdateEmployeeBioModel
    {
        public string EmployeeCode { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<EmployeeBioModel> Fingerprints
        {
            get;set;
        }
        public DataTable GetBioTable()
        {

            DataTable dtSchemeDetails = new DataTable();
            dtSchemeDetails.SetTypeName("ut_EmployeeBio");
            dtSchemeDetails.Columns.Add("EmployeeBioId");
            dtSchemeDetails.Columns.Add("EmployeeId");
            dtSchemeDetails.Columns.Add("BioType");
            dtSchemeDetails.Columns.Add("BioVersion");
            dtSchemeDetails.Columns.Add("BioId");
            dtSchemeDetails.Columns.Add("Bio");
            dtSchemeDetails.Columns.Add("OpType");

            foreach (EmployeeBioModel e in Fingerprints)
            {
                DataRow dr = dtSchemeDetails.NewRow();
                dr["EmployeeBioId"] = e.EmployeeBioId;
                dr["EmployeeId"] = e.EmployeeId;
                dr["BioType"] = e.BioType;
                dr["BioVersion"] = e.BioVersion;
                dr["BioId"] = e.BioID;
                dr["Bio"] = e.Bio;
                dr["OpType"] = e.OpType;

                dtSchemeDetails.Rows.Add(dr);
            }

            return dtSchemeDetails;
        }
    }
    
    public class DeviceLogModel
    {
        public int DeviceLogId { get; set; }
        public int DeviceId { get; set; }
        public string SerialNumber { get; set; }
        public string DeviceDirection { get; set; }
        public string VerifyMode { get; set; }
        public string DeviceEmpCode { get; set; }
        public string DeviceName { get; set; }
        public string LogDate { get; set; }
        public string LogDateInString { get; set; }
        public DateTime DownloadDate { get; set; }
        public int PunchDirectionId { get; set; }
        public string WorkCode { get; set; }
    }
    public class DeviceCommandModel
    {
        private int mDeviceCommandId;
        private string mType;
        private string mTitle;
        private string mDeviceCommand;

        public int DeviceCommandId
        {
            get
            {
                return this.mDeviceCommandId;
            }
            set
            {
                this.mDeviceCommandId = value;
            }
        }

        public string Title
        {
            get
            {
                return this.mTitle;
            }
            set
            {
                this.mTitle = value;
            }
        }

        public string Type
        {
            get
            {
                return this.mType;
            }
            set
            {
                this.mType = value;
            }
        }

        public string DeviceCommand
        {
            get
            {
                return this.mDeviceCommand;
            }
            set
            {
                this.mDeviceCommand = value;
            }
        }
    }
}