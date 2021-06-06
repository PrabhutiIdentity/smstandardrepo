using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Dapper;
using SMEnterprise.Models;

namespace SMEnterprise.Repository
{
    public class ConductorData
    {
        public List<ConductorRoutesVehicleModel> GetConductorVehicleRoutes(int ConductorID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ConductorID", ConductorID);

                return con.Query<ConductorRoutesVehicleModel>("sp_GetConductorRoutesVehicle", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public ConductorStopsScreenModel GetConductorVehicleRoutes(int ConductorID, DateTime CurDate)
        {
            ConductorStopsScreenModel objModel = new ConductorStopsScreenModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ConductorID", ConductorID);
                paramater.Add("@CurDate", CurDate);
                using (var multi = con.QueryMultiple("sp_GetConductorRoutesVehicle", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Routes = multi.Read<ConductorRoutesVehicleModel>().ToList();
                    objModel.Frequency_Mode = multi.Read<string>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public List<RouteStoppageModel> GetConductorRouteDetails(int VehicleRouteID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@VehicleRouteID", VehicleRouteID);

                return con.Query<RouteStoppageModel>("spn_GetVehicleRouteStoppages", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public List<object> GetConductorRoutePassengers(int VehicleRouteID,DateTime CDate)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@VehicleRouteID", VehicleRouteID);
                paramater.Add("@CDate", CDate);

                return con.Query<object>("spn_GetVehicleRoutePassengers", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int UpdatePassengerAttandance(PassengerAttandanceModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@CDate", objModel.CDate);
                paramater.Add("@TRID", objModel.TRID);
                paramater.Add("@StopID", objModel.StopID);
                paramater.Add("@UType", objModel.UType);
                paramater.Add("@UID", objModel.UID);
                return con.Query<int>("spn_UpdatePassengerAttendance", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
    }
}