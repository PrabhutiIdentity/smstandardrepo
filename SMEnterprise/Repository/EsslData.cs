using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using ESSL.Models;
using System.Data.SqlClient;
using System.Data;
using SMEnterprise.Repository;
using SMEnterprise.Models;

namespace ESSL.Repository
{
    public class EsslData
    {
        public List<DeviceCommandModel> GetDeviceCommands(string SerialNumber)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SerialNumber", SerialNumber);
                return con.Query<DeviceCommandModel>("sp_getDeviceCommands", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        public int UpdateDeviceCommandStatus(int DeviceCommandId,string Status,DateTime ExecutionDate,string SerialNumber)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Status", Status);
                paramater.Add("@ExecutionDate", ExecutionDate);
                paramater.Add("@DeviceCommandId", DeviceCommandId);
                paramater.Add("@SerialNumber", SerialNumber);
                return con.Query<int>("sp_UpdateDeviceCommandStatus", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int UpdateDeviceLog(string DeviceOperationLogCode, string DeviceOperationLogExecutedOn, string SerialNumber)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SerialNumber", SerialNumber);
                paramater.Add("@DeviceOperationLogCode", DeviceOperationLogCode);
                paramater.Add("@DeviceOperationLogExecutedOn", DeviceOperationLogExecutedOn);
                return con.Query<int>("sp_UpdateDeviceOperationLog", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int UpdateDeviceLastPing(string SerialNumber)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SerialNumber", SerialNumber);
                paramater.Add("@PingDate", CommonUsage.GetCurrentDate());
                return con.Query<int>("sp_UpdateDeviceLastPing", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int UpdateAttandance(DeviceLogModel objModel, string TransactionStamp)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SerialNumber", objModel.SerialNumber);
                paramater.Add("@LogDate", objModel.LogDate);
                paramater.Add("@UserId", objModel.DeviceEmpCode);
                paramater.Add("@DownloadDate", objModel.DownloadDate);
                paramater.Add("@TransactionStamp", TransactionStamp);
                return con.Query<int>("sp_InsertAttandance", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int UpdateBulkAttandance(DeviceAttendanceBulkModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SerialNumber", objModel.SerialNumber);
                paramater.Add("@TransactionStamp", objModel.TransactionStamp);
                paramater.Add("@Attendances", objModel.GetLogDetailsDataTable());
                return con.Query<int>("sp_InsertBulkAttandance", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public DeviceAttandancePunchModel UpdateAttandanceN(DeviceLogModel objModel, string TransactionStamp)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SerialNumber", objModel.SerialNumber);
                paramater.Add("@LogDate", objModel.LogDate);
                paramater.Add("@UserId", objModel.DeviceEmpCode);
                paramater.Add("@DownloadDate", objModel.DownloadDate);
                paramater.Add("@TransactionStamp", TransactionStamp);
                return con.Query<DeviceAttandancePunchModel>("sp_InsertAttandance", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int UpdateEmployeeFromDevice(EmployeeModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeCodeInDevice", objModel.EmployeeSID);
                paramater.Add("@EmployeeName", objModel.EmployeeName);
                paramater.Add("@EmployeeDevicePassword", objModel.Password);
                paramater.Add("@EmployeeRFIDNumber", objModel.AccessCardNo);
                paramater.Add("@EmployeeDeviceGroup", 0);
                return con.Query<int>("sp_UpdateEmployeesFromDevice", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int UpdateEmployeeBioDetails(UpdateEmployeeBioModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeCode", objModel.EmployeeCode);
                paramater.Add("@UpdatedDate", objModel.UpdatedDate);
                paramater.Add("@Bios", objModel.GetBioTable());
                return con.Query<int>("sp_UploadEmployeeBioDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public DeviceModel GetDeviceConfigDetailsBySerialNumber(string SerialNumber)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SerialNumber", SerialNumber);
                return con.Query<DeviceModel>("sp_GetDeviceConfigDetailsBySerialNumber", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int InsertTestData(string Test)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Test", Test);
                return con.Query<int>("sp_InsertTestString", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
    }
}