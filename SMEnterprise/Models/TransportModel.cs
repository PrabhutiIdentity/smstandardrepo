using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class StudentStopsScreenModel
    {
        public object Conductor { get; set; }
        public int VehicleRouteID { get; set; }
        public List<RouteStoppageModel> Stops { get; set; }
    }
    public class ConductorStopsScreenModel
    {
        public string Frequency_Mode { get; set; }
        public List<ConductorRoutesVehicleModel> Routes { get; set; }
    }
    public class VehicleLogModel
    {
        public int VLogID { get; set; }
        public int VehicleID { get; set; }
        public string VehicleNumber { get; set; }
        public string RegistrationNumber { get; set; }
        public int LogType { get; set; }
        public string LogTypeName { get; set; }
        public string LogTitle { get; set; }
        public DateTime LogDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal Expance { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
    }
    public class VehicleLogPageModel
    {
        public int SBranchID { get; set; }
        public int LogType { get; set; }
        public int VehicleID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<VehicleLogModel> Logs { get; set; }
        public List<VehicleModel> Vehicles { get; set; }
        public List<NameIDModel> LogTypes { get; set; }
    }
    public class TransportMapModel
    {
        public string Token { get; set; }
        public string DeviceID { get; set; }
        public List<RouteStoppageModel> Stops { get; set; }
        public TransportEmployeeModel Driver { get; set; }
        public TransportEmployeeModel Conductor { get; set; }
    }
    public class DriverConductorModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string LicenceNo { get; set; }
        public string CurrentAddress { get; set; }
        public string PermanentAddress { get; set; }
        public string ContactNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public int Type { get; set; }
        public string Image { get; set; }
        public int isVarified { get; set; }
        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
        public HttpPostedFileBase PhotoFile { get; set; }
        public int VehicleCount { get; set; }
    }
    public class VehicleModel
    {
        public int VehicleID { get; set; }
        public string VehicleNumber { get; set; }
        public string RegistrationNumber { get; set; }
        public string VehicleCapacity { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public int VehicleType { get; set; }
        public DateTime InsuranceRenewalDate { get; set; }
        public int DriverID { get; set; }
        public int ConductorID { get; set; }
        public int VehicleCondition { get; set; }
        public int IsApproved { get; set; }
        public string VehicleImage { get; set; }
        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
        public HttpPostedFileBase PhotoFile { get; set; }
        public string DeviceID { get; set; }
    }
    public class VehicleEditModel
    {
        public IEnumerable<VehicleModel> Vehicles { get; set; }
        public IEnumerable<NameIDModel> Drivers { get; set; }
        public IEnumerable<NameIDModel> Conductors { get; set; }
    }
    public class RouteModel
    {
        public int RouteID { get; set; }
        public string RouteName { get; set; }
        public int IsApproved { get; set; }
        public decimal OneWayFees { get; set; }
        public decimal TwoWayFees { get; set; }
        public int Stoppages { get; set; }
        public int VehicleCount { get; set; }
        public int UserID { get; set; }
        public int SBranchID { get; set; }
        public int OpType { get; set; }
        public DateTime OperationDate { get; set; }
        public List<RouteStoppageModel> Stops { get; set; }
    }
    public  class RouteStoppageModel
    {
        public int IsDone { get; set; }
        public int Applicable { get; set; }
        public int StopID { get; set; }
        public int RouteID { get; set; }
        public int CountryID { get; set; }
        public int StateID { get; set; }
        public int CityID { get; set; }
        public int AreaID { get; set; }
        public int VehicleRouteID { get; set; }
        public string AreaName { get; set; }
        public string Time { get; set; }
        public string TimeSecond { get; set; }
        public int HaltDuration { get; set; }
        public int SequenceNo { get; set; }
        public string TimeR { get; set; }
        public string TimeRSecond { get; set; }
        public int HaltDurationR { get; set; }
        public int SequenceNoR { get; set; }
        public int UserID { get; set; }
        public int OpType { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public decimal Rate { get; set; }
        public DateTime OperationDate { get; set; }
        public List<NameIDModel> Areas { get; set; }
        public List<NameIDModel> Cities { get; set; }
        public List<NameIDModel> States { get; set; }
        public List<NameIDModel> Countries { get; set; }
    }
    public class RouteVehicleModel
    {
        public int VehicleRouteID { get; set; }
        public int VehicleID { get; set; }
        public int RouteID { get; set; }
        public int Students { get; set; }
        public int Employees { get; set; }
        public int DelayFromRouteTime { get; set; }
        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }
        public int OpType { get; set; }
        public int SBranchID { get; set; }
        public int ShiftType { get; set; }
    }
    public class RouteVehicleListModel
    {
        public int RouteID { get; set; }
        public List<VehicleModel> Vehicles { get; set; }
        public List<RouteVehicleModel> RouteVehicles { get; set; }
    }
    public class VehicleAppModel
    {
        public string Token { get; set; }
        public int VehicleID { get; set; }
        public string VehicleNumber { get; set; }
        public string RegistrationNumber { get; set; }
        public string DeviceID { get; set; }
        public string Driver { get; set; }
        public string Conductor { get; set; }
    }
}