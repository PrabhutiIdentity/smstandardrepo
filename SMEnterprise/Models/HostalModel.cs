using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class HostalModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string WardenName { get; set; }
        public string ContactNumber { get; set; }
        public int HostelType { get; set; }
        public int Status { get; set; }
        public int SBranchID { get; set; }
        public string HostelTypeName { get; set; }
        public int FloorCount { get; set; }
        public int RoomCount { get; set; }
        public int RoomTypeCount { get; set; }

        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }
        public int OpType { get; set; }
        public List<HostalRoomTypeModel> RoomTypes { get; set; }
        public List<HostalFloorModel> Floors { get; set; }
    }
    public class HostalTypeModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }
        public int OpType { get; set; }
        public int Hostals { get; set; }
        public int SBranchID { get; set; }
    }
    public class HostelPageModel
    {
        public List<HostalModel> Hostals { get; set; }
        public List<HostalTypeModel> HostalTypes { get; set; }
    }
    public class HostalRoomTypeModel
    {
        public int ID { get; set; }
        public int HostelID { get; set; }
        public decimal Rate { get; set; }
        public string RoomTypeName { get; set; }
        public int Capacity { get; set; }
        public int OpType { get; set; }
        public int SBranchID { get; set; }
    }
    public  class HostalFloorModel
    {
        public int ID { get; set; }
        public int HostelID { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public int RoomCount { get; set; }
        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }
        public int OpType { get; set; }
        public List<HostalRoomModel> Rooms { get; set; }
        public List<HostalRoomTypeModel> RoomTypes { get; set; }
    }
    public class HostalRoomModel
    {
        public int ID { get; set; }
        public int FloorID { get; set; }
        public string RoomNo { get; set; }
        public string Name { get; set; }
        public string RoomTypeName { get; set; }
        public decimal Rate { get; set; }
        public int Status { get; set; }
        public int Capacity { get; set; }
        public int RoomTypeID { get; set; }
        public int Occupents { get; set; }
        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }
        public int OpType { get; set; }
        public List<HostalRoomTypeModel> RoomTypes { get; set; }
    }
}