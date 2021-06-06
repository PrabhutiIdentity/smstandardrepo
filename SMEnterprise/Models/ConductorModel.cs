using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class ConductorRoutesVehicleModel
    {
        public int VehicleRouteID { get; set; }
        public int VehicleID { get; set; }
        public string Vehicle { get; set; }
        public string RouteName { get; set; }
        public int IsCompleted { get; set; }
    }
    public class ConductorModel
    {
    }
    public class PassengerAttandanceModel
    {
        public int TAID { get; set; }
        public int TRID { get; set; }
        public int StopID { get; set; }
        public int UType { get; set; }
        public int UID { get; set; }
        public DateTime CDate { get; set; }
        public DateTime UpDateTime { get; set; }
        public DateTime DownDateTime { get; set; }
        public string UUID { get; set; }
    }
    public class TransportStopUpdateModel
    {
        public int VehicleRouteID { get; set; }
        public string UUID { get; set; }
        public int StopID { get; set; }
        public DateTime UDate { get; set; }
    }
    public class TransportGeoModel
    {
        public int RouteID { get; set; }
        public int IsAll { get; set; }
        public string UUID { get; set; }
        public string Angle { get; set; }
        public int VehicleID { get; set; }
        public string GioDate { get; set; }
        public string GioTime { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
    }
}