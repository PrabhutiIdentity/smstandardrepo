using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class BuildingModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public int UserID { get; set; }
        public int OpType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int SBranchID { get; set; }
        public int FloorCount { get; set; }
        public int RoomCount { get; set; }
        public List<FloorModel> Floors = new List<FloorModel>();
    }
    public class FloorModel
    {
        public int ID { get; set; }
        public int OpType { get; set; }
        public int BuildingID { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int SBranchID { get; set; }
        public int RoomCount { get; set; }
        public List<RoomModel> Rooms = new List<RoomModel>();
        public List<RoomSizeModel> RoomSizes = new List<RoomSizeModel>();
    }
    public class RoomModel
    {
        public int ID { get; set; }
        public int OpType { get; set; }
        public int FloorID { get; set; }
        public int SizeID { get; set; }
        public string Name { get; set; }
        public string RoomSize { get; set; }
        public string RoomNo { get; set; }
        public string Section { get; set; }
        public int Status { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int SBranchID { get; set; }
    }
    public class RoomSizeModel
    {
        public int SizeID { get; set; }
        public string Name { get; set; }
    }
}