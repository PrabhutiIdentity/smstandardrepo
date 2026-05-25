using Dapper;
using Razorpay.Api;
using SMEnterprise.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using static System.Web.Razor.Parser.SyntaxConstants;


namespace SMEnterprise.Repository
{
    public class AccountData
    {
        private sealed class LockedStudentFeeDetail
        {
            public int SFID { get; set; }
            public int StudentID { get; set; }
            public int FeeTypeID { get; set; }
            public int SessionID { get; set; }
            public decimal FeeAmount { get; set; }
            public int IsApplicable { get; set; }
            public string FeeTypeName { get; set; }
        }

        public StudentsPageModel GetParentAppDetail(int ClassID, int SectionID, int SBranchID)
        {
            StudentsPageModel objModel = new StudentsPageModel();
            objModel.SectionID = SectionID;
            objModel.ClassID = ClassID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetParentAppDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<SectionModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }

        public IEnumerable<SBranchModel> GetBranches()
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                return con.Query<SBranchModel>("sp_GetSBranches", null, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        #region Student Management Module

        public StudentsPageModel GetStudentPaymentRecipts(int ClassID, int SectionID, int SBranchID, int SessionID)
        {

            StudentsPageModel objModel = new StudentsPageModel();
            objModel.SectionID = SectionID;
            objModel.ClassID = ClassID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("spn_GetStudentsByClassSectionFeeRecipt", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<SectionModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }

        public PaymentModel CancelPaymentReuest(int PaymentID, string OTP)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@PaymentID", PaymentID);
                paramater.Add("@OTP", OTP);
                return con.Query<PaymentModel>("sp_RequestPaymentCancellation", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int ProcessPaymentReuest(int PaymentID, string OTP)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@PaymentID", PaymentID);
                paramater.Add("@OTP", OTP);
                return con.Query<int>("sp_ProcessPaymentCancellation", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public StudentsPageModel GetStudents(int ClassID, int SectionID, int SBranchID, int SessionID)
        {

            StudentsPageModel objModel = new StudentsPageModel();
            objModel.SectionID = SectionID;
            objModel.ClassID = ClassID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("spn_GetStudentsByClassSection", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<SectionModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    try
                    {
                        objModel.BranchDetails = multi.Read<SBranchModel>().SingleOrDefault();
                        objModel.ActiveInactive = multi.Read<ActiveInactiveStudentModel>().ToList();
                    }
                    catch
                    {

                    }

                }
            }
            return objModel;
        }

        public StudentsPageModel GetStudentInactive(int ClassID, int SectionID, int SBranchID, int SessionID)
        {

            StudentsPageModel objModel = new StudentsPageModel();
            objModel.SectionID = SectionID;
            objModel.ClassID = ClassID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("spn_GetInActiveStudentsByClassSection", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<SectionModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    try
                    {
                        objModel.BranchDetails = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                    catch
                    {

                    }
                }
            }
            return objModel;
        }

        public StudentsPageModel GetStudentRecords(int ClassID, int SectionID, int SBranchID, int SessionID)
        {

            StudentsPageModel objModel = new StudentsPageModel();
            objModel.SectionID = SectionID;
            objModel.ClassID = ClassID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("spn_GetStudentRecordsClassSection", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<SectionModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public int QuickStudentUpdate(int SBranchID, StudentsPageModel model)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@Students", model.GetStudentsDataTable());
                return con.Query<int>("sp_QuickUpdateStudents", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public StudentSearchListModel SearchStudents(string SearchText, int SBranchID, int SessionID)
        {

            StudentSearchListModel objModel = new StudentSearchListModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SearchText", SearchText);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);

                using (var multi = con.QueryMultiple("spn_GetSearchedStudents", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentSearchModel>().ToList();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
                objModel.SearchText = SearchText;
                return objModel;
            }
        }

        public StudentSearchListModel SuspendedStudents(int SBranchID, int SessionID)
        {

            StudentSearchListModel objModel = new StudentSearchListModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);

                using (var multi = con.QueryMultiple("spn_GetSuspendedStudents", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentSearchModel>().ToList();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
                return objModel;
            }
        }

        public QuotaClassStudentListModel GetQuotaWiseStudents(int SBranchID, int QuotaID)
        {

            QuotaClassStudentListModel objModel = new QuotaClassStudentListModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@QuotaID", QuotaID);

                using (var multi = con.QueryMultiple("sp_GetQuotaWiseStudents", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.Quotas = multi.Read<StudentQuotaModel>().ToList();
                    objModel.ID = multi.Read<int>().SingleOrDefault();
                    try
                    {
                        objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();

                    }
                    catch
                    {

                    }
                }
                return objModel;
            }
        }
        public HouseClassStudentListModel GetHouseWiseStudents(int SBranchID, int HouseID)
        {

            HouseClassStudentListModel objModel = new HouseClassStudentListModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@HouseID", HouseID);

                using (var multi = con.QueryMultiple("sp_GetHouseWiseStudents", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.Houses = multi.Read<HouseModel>().ToList();
                    objModel.ID = multi.Read<int>().SingleOrDefault();
                    try
                    {
                        objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();

                    }
                    catch
                    {

                    }
                }
                return objModel;
            }
        }
        public BusStudentListModel GetBusWiseStudents(BusStudentListModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@BusID", objModel.ID);
                paramater.Add("@ShiftID", objModel.ShiftID);
                paramater.Add("@Type", objModel.Type);
                using (var multi = con.QueryMultiple("sp_GetBusWiseStudents", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.Busses = multi.Read<NameIDModel>().ToList();
                    objModel.ID = multi.Read<int>().SingleOrDefault();
                }
                return objModel;
            }
        }
        public BusStudentListModel GetClassWiseBusStudents(int SBranchID, int ClassID, int SectionID)
        {

            BusStudentListModel objModel = new BusStudentListModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);

                using (var multi = con.QueryMultiple("sp_GetClassWiseBusStudents", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.ClassSections = multi.Read<ClassSectionModel>().ToList();
                    objModel.ID = multi.Read<int>().SingleOrDefault();
                }
                return objModel;
            }
        }
        public HouseClassStudentListModel GetClassWiseHouseStudents(int SBranchID, int ClassID, int SectionID)
        {

            HouseClassStudentListModel objModel = new HouseClassStudentListModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);

                using (var multi = con.QueryMultiple("sp_GetClassWiseHouseStudents", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.ClassSections = multi.Read<ClassSectionModel>().ToList();
                    objModel.ID = multi.Read<int>().SingleOrDefault();
                    try
                    {
                        objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();

                    }
                    catch
                    {

                    }
                }
                return objModel;
            }
        }
        public int DeleteStudents(int StudentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                return con.Query<int>("spn_DeleteStudent", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public StudentPromotionModel GetSessionClassSectionOnBranch(int SBranchID)
        {

            StudentPromotionModel objModel = new StudentPromotionModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetSessionClassSectionForBranch", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<SectionModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public StudentPromotionModel GetPromoteStudents(int ClassID, int SectionID, int SBranchID, int SessionID)
        {

            StudentPromotionModel objModel = new StudentPromotionModel();
            objModel.SectionID = SectionID;
            objModel.ClassID = ClassID;
            objModel.SBranchID = SBranchID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("spn_GetSPromotetudentsByClassSection", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentPromoteModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<SectionModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    try
                    {
                        objModel.Branches = multi.Read<NameIDModel>().ToList();
                        objModel.ForwardSession = multi.Read<NameIDModel>().ToList();
                    }
                    catch
                    {

                    }
                }
            }
            return objModel;
        }
        public int UpdatePromotedStudents(StudentPromotionModel model)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", model.SBranchID);
                paramater.Add("@SessionID", model.SessionID);
                paramater.Add("@ClassID", model.ClassID);
                paramater.Add("@SectionID", model.SectionID);
                paramater.Add("@Students", model.GetPromotedStudentsDataTable());
                return con.Query<int>("sp_PromoteStudents", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public StudentEditModel GetStudentDetailsNew(int StudentID, int SBranchID)
        {

            StudentEditModel objModel = new StudentEditModel();
            objModel.StudentID = StudentID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetStudentsDetailsNew", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Student = multi.Read<StudentModel>().SingleOrDefault();
                    if (objModel.Student == null)
                    {
                        objModel.Student = new StudentModel();
                        objModel.Student.DOB = CommonUsage.GetCurrentDate();
                        objModel.Student.DOJ = CommonUsage.GetCurrentDate();
                    }
                    objModel.ParantDetails = multi.Read<ParentModel>().SingleOrDefault();
                    objModel.Cities = multi.Read<NameIDModel>().ToList();
                    objModel.Areas = multi.Read<NameIDModel>().ToList();
                    objModel.Student.Cadd_DistrictCode = multi.Read<int>().SingleOrDefault();
                    objModel.Student.Cadd_AreaCode = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<SessionModel>().ToList();
                    objModel.CustomeFee = multi.Read<StudentCustomFeeModel>().ToList();
                    objModel.Student.IsCustomFee = multi.Read<int>().SingleOrDefault();
                    objModel.THADDetails = multi.Read<StudentTransportHostelAllocationDelocationModel>().ToList();
                    objModel.SubReligions = multi.Read<NameIDModel>().ToList();
                    objModel.Nationalities = multi.Read<NameIDModel>().ToList();
                    objModel.Religions = multi.Read<NameIDModel>().ToList();
                    objModel.MotherTongues = multi.Read<NameIDModel>().ToList();
                    objModel.SocialCategories = multi.Read<NameIDModel>().ToList();
                    try
                    {
                        objModel.Hostel.Hostels = multi.Read<NameIDModel>().ToList();
                        objModel.Hostel.Floors = multi.Read<NameIDModel>().ToList();
                        objModel.Hostel.Rooms = multi.Read<HostalRoomModel>().ToList();
                        objModel.Hostel.HostelID = multi.Read<int>().SingleOrDefault();
                        objModel.Hostel.FloorID = multi.Read<int>().SingleOrDefault();
                        objModel.Hostel.RoomID = multi.Read<int>().SingleOrDefault();
                        objModel.Hostel.Applicable = multi.Read<int>().SingleOrDefault();
                    }
                    catch
                    {

                    }
                }
                if (objModel.ParantDetails == null)
                {
                    objModel.ParantDetails = new ParentModel();
                    objModel.ParantDetails.FatherDOB = CommonUsage.GetCurrentDate();
                    objModel.ParantDetails.MotherDOB = CommonUsage.GetCurrentDate();
                }
            }
            return objModel;
        }
        public StudentEditModel GetStudentSessionCustomFee(int StudentSessionUID, int SBranchID)
        {

            StudentEditModel objModel = new StudentEditModel();
            objModel.StudentSessionUID = StudentSessionUID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentSessionUID", StudentSessionUID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetStudentSessionCustomFee", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    if (objModel.Student == null)
                    {
                        objModel.Student = new StudentModel();
                        objModel.Student.DOB = CommonUsage.GetCurrentDate();
                        objModel.Student.DOJ = CommonUsage.GetCurrentDate();
                    }
                    objModel.CustomeFee = multi.Read<StudentCustomFeeModel>().ToList();
                    objModel.Student.IsCustomFee = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }

        public StudentTransportModel GetStudentTransportDetails(int StudentID, int SBranchID, int THChangeID)
        {

            StudentTransportModel Transport = new StudentTransportModel();
            Transport.StudentID = StudentID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@THChangeID", THChangeID);
                using (var multi = con.QueryMultiple("spn_GetStudentsTransportDetailsNew2", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    Transport.StopDetails = multi.Read<RouteStoppageModel>().SingleOrDefault();
                    Transport.Routes = multi.Read<NameIDModel>().ToList();
                    Transport.Vehicles = multi.Read<NameIDModel>().ToList();
                    Transport.Stops = multi.Read<RouteStoppageModel>().ToList();
                    Transport.THADetails = multi.Read<StudentTransportHostelAllocationDelocationModel>().SingleOrDefault();

                }
            }
            return Transport;
        }

        public int InsertUpdateStudentTransport(StudentTransportHostelAllocationDelocationModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@THChangeID", objData.THChangeID);
                paramater.Add("@VehicleRouteID", objData.VehicleRouteID);
                paramater.Add("@ChangeType", objData.ChangeType);
                paramater.Add("@UserType", objData.UserType);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@StartDate", objData.StartDate);
                paramater.Add("@EndDate", objData.EndDate);
                paramater.Add("@SessionID", objData.SessionID);
                paramater.Add("@KeyID", objData.KeyID);
                paramater.Add("@Applicable", objData.Applicable);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@SBranchID", objData.SBranchID);

                return con.Query<int>("sp_UpdateTransportAllocationDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public VendorPageModel GetVendors(int SBranchID)
        {
            VendorPageModel oModel = new VendorPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetAccountVendors", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Vendors = multi.Read<VendorModel>().ToList();
                }
            }
            return oModel;
        }
        public VendorPageModel GetVendorDetails(int SBranchID, int VendorID)
        {
            VendorPageModel oModel = new VendorPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@VendorID", VendorID);
                using (var multi = con.QueryMultiple("sp_GetVendorDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Vendor = multi.Read<VendorModel>().SingleOrDefault();
                    oModel.GSTStates = multi.Read<GSTStateModel>().ToList();
                }
            }
            return oModel;
        }
        public int InsertUpdateVendor(VendorModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@VendorID", oModel.VendorID);
                paramater.Add("@CompanyName", oModel.CompanyName);
                paramater.Add("@ContactPerson", oModel.ContactPerson);
                paramater.Add("@ContactNumber", oModel.ContactNumber);
                paramater.Add("@Address", oModel.Address);
                paramater.Add("@StateID", oModel.StateID);
                paramater.Add("@PinCode", oModel.PinCode);
                paramater.Add("@CompanyContact", oModel.CompanyContact);
                paramater.Add("@EmailID", oModel.EmailID);
                paramater.Add("@BranchName", oModel.BranchName);
                paramater.Add("@BankName", oModel.BankName);
                paramater.Add("@AccountNumber", oModel.AccountNumber);
                paramater.Add("@AccountType", oModel.AccountType);
                paramater.Add("@IFSCCode", oModel.IFSCCode);
                paramater.Add("@OtherDetails", oModel.OtherDetails);
                paramater.Add("@GSTIN", oModel.GSTIN);
                paramater.Add("@PANNumber", oModel.PANNumber);
                paramater.Add("@TINNumber", oModel.TINNumber);
                paramater.Add("@CINNumber", oModel.CINNumber);
                paramater.Add("@RegistrationNumber", oModel.RegistrationNumber);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@OpType", oModel.OpType);

                return con.Query<int>("sp_UpdateAccountVendors", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public StudentEditModel GetStudentDetails(int StudentID, int SBranchID)
        {

            StudentEditModel objModel = new StudentEditModel();
            objModel.StudentID = StudentID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetStudentsDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Student = multi.Read<StudentModel>().SingleOrDefault();
                    objModel.ParantDetails = multi.Read<ParentModel>().SingleOrDefault();
                    objModel.Countries = multi.Read<NameIDModel>().ToList();
                    objModel.States = multi.Read<NameIDModel>().ToList();
                    objModel.Cities = multi.Read<NameIDModel>().ToList();
                    objModel.Areas = multi.Read<NameIDModel>().ToList();
                    objModel.Sessions = multi.Read<SessionModel>().ToList();
                    objModel.Transport.StopDetails = multi.Read<RouteStoppageModel>().SingleOrDefault();
                    objModel.Transport.Countries = multi.Read<NameIDModel>().ToList();
                    objModel.Transport.States = multi.Read<NameIDModel>().ToList();
                    objModel.Transport.Cities = multi.Read<NameIDModel>().ToList();
                    objModel.Transport.Areas = multi.Read<NameIDModel>().ToList();
                    objModel.Transport.Routes = multi.Read<NameIDModel>().ToList();
                    objModel.Transport.Vehicles = multi.Read<NameIDModel>().ToList();
                    objModel.Transport.Stops = multi.Read<RouteStoppageModel>().ToList();

                    objModel.Hostel.Hostels = multi.Read<NameIDModel>().ToList();
                    objModel.Hostel.Floors = multi.Read<NameIDModel>().ToList();
                    objModel.Hostel.Rooms = multi.Read<HostalRoomModel>().ToList();
                    objModel.Hostel.HostelID = multi.Read<int>().SingleOrDefault();
                    objModel.Hostel.FloorID = multi.Read<int>().SingleOrDefault();
                    objModel.Hostel.RoomID = multi.Read<int>().SingleOrDefault();
                    objModel.Hostel.Applicable = multi.Read<int>().SingleOrDefault();
                    try
                    {
                        objModel.CustomeFee = multi.Read<StudentCustomFeeModel>().ToList();
                    }
                    catch { }
                }
                if (objModel.Student == null)
                {
                    objModel.Student = new StudentModel();
                    objModel.Student.DOB = CommonUsage.GetCurrentDate();
                    objModel.Student.DOJ = CommonUsage.GetCurrentDate();
                }
                if (objModel.ParantDetails == null)
                {
                    objModel.ParantDetails = new ParentModel();
                    objModel.ParantDetails.FatherDOB = CommonUsage.GetCurrentDate();
                    objModel.ParantDetails.MotherDOB = CommonUsage.GetCurrentDate();
                }
            }
            return objModel;
        }
        public int InsertUpdateStudentBasic(StudentModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", objData.StudentID);
                paramater.Add("@Name", objData.Name);
                paramater.Add("@DOB", objData.DOB);
                paramater.Add("@Gender", objData.Gender);
                paramater.Add("@Nationality", objData.Nationality);
                paramater.Add("@ReligionID", objData.ReligionID);
                paramater.Add("@DOJ", objData.DOJ);
                paramater.Add("@EmailID", objData.EmailID);
                paramater.Add("@Category", objData.Category);
                paramater.Add("@Disability", objData.Disability);
                paramater.Add("@BloodGroup", objData.BloodGroup);
                paramater.Add("@MotherTongueID", objData.MotherTongueID);
                paramater.Add("@PassportNo", objData.PassportNo);
                paramater.Add("@ExtraCurricular", objData.ExtraCurricular);
                paramater.Add("@Allergic_Medicine", objData.Allergic_Medicine);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@AdmissionBy", objData.AdmissionBy);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@ImageName", objData.Photo);
                paramater.Add("@AccessCardNo", objData.AccessCardNo);
                paramater.Add("@Password", objData.Password);
                paramater.Add("@NotificationSMSTo", objData.NotificationSMSTo);
                paramater.Add("@SchoolUID", objData.SchoolUID);
                paramater.Add("@AadharCardNo", objData.AadharCardNo);

                paramater.Add("@SSSID", objData.SSSID);
                paramater.Add("@FamilyID", objData.FamilyID);
                paramater.Add("@BankName", objData.BankName);
                paramater.Add("@AccountNumber", objData.AccountNumber);
                paramater.Add("@IFSCCode", objData.IFSCCode);
                paramater.Add("@BranchName", objData.BranchName);

                paramater.Add("@PSchoolName", objData.PSchoolName);
                paramater.Add("@PSchoolMedium", objData.PSchoolMedium);
                paramater.Add("@PClassName", objData.PClassName);
                paramater.Add("@PSResult", objData.PSResult);
                paramater.Add("@PSchoolCity", objData.PSchoolCity);
                paramater.Add("@PSchoolState", objData.PSchoolState);

                paramater.Add("@PenNo", objData.PenNo);
                paramater.Add("@ApaarID", objData.ApaarID);

                return con.Query<int>("spn_InsertUpdateStudentBasicDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public ParentModel GetParentDetailsOnParentID(string ParentID, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ParentID", ParentID);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<ParentModel>("sp_GetParentInformationOnParantID", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int InsertUpdateStudentParent(ParentModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ParentID", objData.ParentID);
                paramater.Add("@FatherName", objData.FatherName);
                paramater.Add("@FatherImage", objData.FatherImage);
                paramater.Add("@FatherOccupationID", objData.FatherOccupationID);
                paramater.Add("@FatherEducationID", objData.FatherEducationID);
                paramater.Add("@FatherMobileNo", objData.FatherMobileNo);
                paramater.Add("@FatherEmailID", objData.FatherEmailID);
                paramater.Add("@FatherBloodGroup", objData.FatherBloodGroup);
                paramater.Add("@MotherName", objData.MotherName);
                paramater.Add("@MotherImage", objData.MotherImage);
                paramater.Add("@MotherOccupationID", objData.MotherOccupationID);
                paramater.Add("@MotherEducationID", objData.MotherEducationID);
                paramater.Add("@MotherMobileNo", objData.MotherMobileNo);
                paramater.Add("@MotherEmailID", objData.MotherEmailID);
                paramater.Add("@MotherBloodGroup", objData.MotherBloodGroup);
                paramater.Add("@HomeLandLineNo", objData.HomeLandLineNo);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@StudentID", objData.StudentID);

                paramater.Add("@Padd_HouseNo", objData.Padd_HouseNo);
                paramater.Add("@Padd_Street", objData.Padd_Street);
                paramater.Add("@Padd_Area", objData.Padd_Area);
                paramater.Add("@Padd_Sector", objData.Padd_Sector);
                paramater.Add("@Padd_District", objData.Padd_District);
                paramater.Add("@Padd_State", objData.Padd_State);
                paramater.Add("@Padd_PinCode", objData.Padd_PinCode);
                paramater.Add("@Padd_Country", objData.Padd_Country);
                paramater.Add("@FatherDOB", objData.FatherDOB);
                paramater.Add("@MotherDOB", objData.MotherDOB);
                paramater.Add("@Password", objData.Password);
                paramater.Add("@AccessCardNo", objData.AccessCardNo);
                paramater.Add("@FatherAadhaar", objData.FatherAadhaar);
                paramater.Add("@MotherAadhaar", objData.MotherAadhaar);

                return con.Query<int>("sp_InsertUpdateParentDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int UpdateStudentAddress(StudentModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@StudentID", objData.StudentID);
                paramater.Add("@Cadd_HouseNo", objData.Cadd_HouseNo);
                paramater.Add("@Cadd_Street", objData.Cadd_Street);
                paramater.Add("@Cadd_AreaCode", objData.Cadd_AreaCode);
                paramater.Add("@Cadd_Sector", objData.Cadd_Sector);
                paramater.Add("@Cadd_PinCode", objData.Cadd_PinCode);
                paramater.Add("@Cadd_DistrictCode", objData.Cadd_DistrictCode);
                paramater.Add("@Cadd_StateCode", objData.Cadd_StateCode);
                paramater.Add("@Cadd_CountryCode", objData.Cadd_CountryCode);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@MiniAddress", objData.MiniAddress);
                paramater.Add("@MiniAddress2", objData.MiniAddress2);

                return con.Query<int>("spn_UpdateStudentAddress", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public IEnumerable<NameIDModel> GetCountryStates(int CountryID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@CountryID", CountryID);
                return con.Query<NameIDModel>("sp_GetCountryStates", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public IEnumerable<NameIDModel> GetStateCities(int StateID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StateID", StateID);
                return con.Query<NameIDModel>("sp_GetStateCity", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public IEnumerable<NameIDModel> GetCityAreas(int CityID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@CityID", CityID);
                return con.Query<NameIDModel>("sp_GetCityArea", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public IEnumerable<NameIDModel> AddLocation(int ID, int Type, string Name)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", ID);
                paramater.Add("@Type", Type);
                paramater.Add("@Name", Name);
                return con.Query<NameIDModel>("spn_AddLocation", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int UpdateStudentGuardian(StudentModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@StudentID", objData.StudentID);
                paramater.Add("@GuardianName", objData.GuardianName);
                paramater.Add("@RelationWithGuardian", objData.RelationWithGuardian);
                paramater.Add("@GuardianMobileNo", objData.GuardianMobileNo);
                paramater.Add("@GuardianEmail", objData.GuardianEmail);
                paramater.Add("@SBranchID", objData.SBranchID);

                return con.Query<int>("sp_UpdateStudentGuardian", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int UpdateStudentDocuments(StudentModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@StudentID", objData.StudentID);
                paramater.Add("@AddressProof", objData.AddressProof);
                paramater.Add("@BirthCertificate", objData.BirthCertificate);
                paramater.Add("@CategoryCertificate", objData.CategoryCertificate);
                paramater.Add("@TransferCertificate", objData.TransferCertificate);
                paramater.Add("@SBranchID", objData.SBranchID);

                return con.Query<int>("sp_UpdateStudentCertificateImages", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public SessionEditModel GetSessionDetails(int StudentSessionUID, int SBranchID)
        {

            SessionEditModel objModel = new SessionEditModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentSessionUID", StudentSessionUID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetStudentSessionEditDataEnt", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Session = multi.Read<SessionModel>().SingleOrDefault();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Quotas = multi.Read<NameIDModel>().ToList();
                    objModel.SessionStartDate = multi.Read<DateTime>().SingleOrDefault();
                    objModel.SessionEndDate = multi.Read<DateTime>().SingleOrDefault();
                    objModel.SchoolSessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.Houses = multi.Read<NameIDModel>().ToList();
                    try
                    {
                        objModel.OptionalSubjects = multi.Read<NameIDModel>().ToList();
                        //objModel.SubjectsOpted = multi.Read<NameIDModel>().ToList();
                    }
                    catch (Exception ex)
                    { throw ex; }
                }
                if (StudentSessionUID == 0)
                {
                    objModel.Session.FromDate = objModel.SessionStartDate;
                    objModel.Session.ToDate = objModel.SessionEndDate;
                }
            }
            return objModel;
        }
        public int UpdateStudentSession(SessionModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@StudentSessionUID", objData.StudentSessionUID);
                paramater.Add("@StudentID", objData.StudentID);
                paramater.Add("@ClassID", objData.ClassID);
                paramater.Add("@SectionID", objData.SectionID);
                paramater.Add("@FromDate", objData.FromDate);
                paramater.Add("@ToDate", objData.ToDate);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@RollNo", objData.RollNo);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@QuotaID", objData.QuotaID);
                paramater.Add("@FeePaymentMode", objData.FeePaymentMode);
                paramater.Add("@SessionID", objData.SessionID);
                paramater.Add("@HouseID", objData.HouseID);
                paramater.Add("@OptedSubjects", objData.GetSubjectOptedDataTable());
                paramater.Add("@IsAdmissionFeeApplicable", objData.IsAdmissionFeeApplicable);
                paramater.Add("@ReasonforInactive", objData.ReasonforInactive);
                paramater.Add("@IsNewSessionRequest", objData.IsNewSessionRequest);

                return con.Query<int>("spn_InsertUpdateStudentSessionDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public StudentEditModel GetStudentDetailsMini(int StudentID, int SBranchID)
        {

            StudentEditModel objModel = new StudentEditModel();
            objModel.StudentID = StudentID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetStudentsDetailsMini", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Student = multi.Read<StudentModel>().SingleOrDefault();
                    objModel.ParantDetails = multi.Read<ParentModel>().SingleOrDefault();
                    objModel.Sessions = multi.Read<SessionModel>().ToList();
                }
                if (objModel.Student == null)
                {
                    objModel.Student = new StudentModel();
                    objModel.Student.DOB = CommonUsage.GetCurrentDate();
                    objModel.Student.DOJ = CommonUsage.GetCurrentDate();
                }
                if (objModel.ParantDetails == null)
                {
                    objModel.ParantDetails = new ParentModel();
                    objModel.ParantDetails.FatherDOB = CommonUsage.GetCurrentDate();
                    objModel.ParantDetails.MotherDOB = CommonUsage.GetCurrentDate();
                }
            }
            return objModel;
        }
        public StudentEditModel GetStudentDetailsPrint(int StudentID, int SBranchID)
        {

            StudentEditModel objModel = new StudentEditModel();
            objModel.StudentID = StudentID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetStudentsDetailsPrint", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Student = multi.Read<StudentModel>().SingleOrDefault();
                    objModel.ParantDetails = multi.Read<ParentModel>().SingleOrDefault();
                    objModel.Sessions = multi.Read<SessionModel>().ToList();
                    objModel.SBranchDetails = multi.Read<SBranchModel>().SingleOrDefault();
                    objModel.SessionName = multi.Read<string>().SingleOrDefault();

                }
                if (objModel.Student == null)
                {
                    objModel.Student = new StudentModel();
                    objModel.Student.DOB = CommonUsage.GetCurrentDate();
                    objModel.Student.DOJ = CommonUsage.GetCurrentDate();
                }
                if (objModel.ParantDetails == null)
                {
                    objModel.ParantDetails = new ParentModel();
                    objModel.ParantDetails.FatherDOB = CommonUsage.GetCurrentDate();
                    objModel.ParantDetails.MotherDOB = CommonUsage.GetCurrentDate();
                }

            }
            return objModel;
        }
        public List<NameIDModel> GetOptionalSubjectsForSection(int SectionID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", SectionID);
                return con.Query<NameIDModel>("spn_GetOptionalSubjectsForSection", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public IEnumerable<NameIDModel> GetAreaRoutes(int AreaID, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@AreaID", AreaID);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<NameIDModel>("sp_GetTransportRouteForArea", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public IEnumerable<NameIDModel> GetRouteVehicles(int RouteID, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@RouteID", RouteID);
                return con.Query<NameIDModel>("spn_GetTransportRouteVehicleList", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public List<RouteStoppageModel> GetRouteVehicleStops(int VehicleRouteID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@VehicleRouteID", VehicleRouteID);
                return con.Query<RouteStoppageModel>("spn_GetVehicleRouteStoppages", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int UpdateStudentTransport(StudentTransportModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@StudentID", objData.StudentID);
                paramater.Add("@VehicleRouteID", objData.VehicleRouteID);
                paramater.Add("@StopID", objData.StopID);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@ChangeDate", objData.ChangeDate);

                return con.Query<int>("sp_UpdateStudentTransportDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public IEnumerable<NameIDModel> GetHostelFloorsForAdmission(int HostelID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@HostelID", HostelID);
                return con.Query<NameIDModel>("sp_GetHostelFloorsForAdmission", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public List<HostalRoomModel> GetHostelFloorsRoomsForAdmission(int HostelID, int FloorID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@HostelID", HostelID);
                paramater.Add("@FloorID", FloorID);
                return con.Query<HostalRoomModel>("sp_GetHostelRoomsForAdmission", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int UpdateStudentHostel(StudentHostelModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@StudentID", objData.StudentID);
                paramater.Add("@RoomID", objData.RoomID);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@ChangedDate", objData.ChangedDate);

                return con.Query<int>("sp_UpdateStudentHostelDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public int InsertUpdateStudentCustomFee(StudentEditModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                if (objData.CustomeFee == null)
                {
                    objData.CustomeFee = new List<StudentCustomFeeModel>();
                }

                var lockedCarryForwardRows = con.Query<LockedStudentFeeDetail>(
                    @"SELECT
                          SFD.SFID,
                          SFD.StudentID,
                          SFD.FeeTypeID,
                          SFD.SessionID,
                          SFD.FeeAmount,
                          SFD.IsApplicable,
                          FTM.FeeTypeName
                      FROM StudentFeeDetails SFD
                      INNER JOIN FeeTypeMaster FTM ON FTM.FeeTypeID = SFD.FeeTypeID
                      WHERE SFD.SessionID = @StudentSessionUID
                        AND LTRIM(RTRIM(FTM.FeeTypeName)) = 'Session-Carry-forward'",
                    new { objData.StudentSessionUID }).ToList();

                foreach (var lockedRow in lockedCarryForwardRows)
                {
                    var postedRow = objData.CustomeFee
                        .FirstOrDefault(x => x.FeeTypeID == lockedRow.FeeTypeID && x.SessionID == lockedRow.SessionID);

                    if (postedRow == null)
                    {
                        objData.CustomeFee.Add(new StudentCustomFeeModel
                        {
                            SFID = lockedRow.SFID,
                            StudentID = lockedRow.StudentID,
                            FeeTypeID = lockedRow.FeeTypeID,
                            SessionID = lockedRow.SessionID,
                            FeeAmount = lockedRow.FeeAmount,
                            IsApplicable = lockedRow.IsApplicable,
                            FeeTypeName = lockedRow.FeeTypeName
                        });
                        continue;
                    }

                    postedRow.SFID = lockedRow.SFID;
                    postedRow.StudentID = lockedRow.StudentID;
                    postedRow.SessionID = lockedRow.SessionID;
                    postedRow.FeeAmount = lockedRow.FeeAmount;
                    postedRow.IsApplicable = lockedRow.IsApplicable;
                    postedRow.FeeTypeName = lockedRow.FeeTypeName;
                }

                var paramater = new DynamicParameters();
                paramater.Add("@StudentSessionUID", objData.StudentSessionUID);
                paramater.Add("@IsCustomFee", objData.IsCustomFee);
                paramater.Add("@FeeDetails", objData.GetFeeStructureDataTable());

                return con.Query<int>("sp_UpdateStudentCustomFee", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #endregion

        #region Bus & House
        public BusStudentListModel GetStopWiseCollection(int SBranchID, DateTime MStartDate)
        {
            BusStudentListModel objModel = new BusStudentListModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@MStartDate", MStartDate);

                using (var multi = con.QueryMultiple("sp_GetStopWiseCollectionStudents", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.StopWiseReport = multi.Read<StopWiseStudentAmountModel>().ToList();
                    try
                    {

                        objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                    catch (Exception ex)
                    { }
                }
                objModel.SelectedDate = MStartDate;
                return objModel;
            }
        }
        public BusStudentListModel GetBusWiseStudents(int SBranchID, int BusID)
        {

            BusStudentListModel objModel = new BusStudentListModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@BusID", BusID);

                using (var multi = con.QueryMultiple("sp_GetBusWiseStudents", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.Busses = multi.Read<NameIDModel>().ToList();
                    objModel.ID = multi.Read<int>().SingleOrDefault();
                    try
                    {

                        objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                    catch (Exception ex)
                    { }
                }
                return objModel;
            }
        }
        public BusStudentListModel BusStopWiseStudents(int SBranchID, int BusID)
        {

            BusStudentListModel objModel = new BusStudentListModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@BusID", BusID);

                using (var multi = con.QueryMultiple("sp_GetBusWiseStudents", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.Busses = multi.Read<NameIDModel>().ToList();
                    objModel.ID = multi.Read<int>().SingleOrDefault();
                    try
                    {

                        objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                    catch (Exception ex)
                    { }
                }
                return objModel;
            }
        }



        #endregion
        #region Fee Module
        public StudentFeeModel GetFeePayments(StudentFeeModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Day", objModel.Day);
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SessionID", objModel.SessionID);
                using (var multi = con.QueryMultiple("spnp_GetClassSectionFeePayments", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.FeePayments = multi.Read<FeePaymentModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public StudentFeeModel GetNewFeePayments(StudentFeeModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                DateTime qdate = new DateTime(objModel.Year, objModel.Month, 1).AddMonths(1).AddDays(-1);
                if (qdate.Day < objModel.Day)
                {
                    objModel.Day = qdate.Day;
                }
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@QDate", new DateTime(objModel.Year, objModel.Month, objModel.Day));

                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());
                using (var multi = con.QueryMultiple("sp_GetClassGroupWiseStudentFeeSummery", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.FeePayments = multi.Read<FeePaymentModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public StudentFeeModel GetNewFeePaymentsParentWise(StudentFeeModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                DateTime qdate = new DateTime(objModel.Year, objModel.Month, 1).AddMonths(1).AddDays(-1);
                if (qdate.Day < objModel.Day)
                {
                    objModel.Day = qdate.Day;
                }
                var paramater = new DynamicParameters();
                paramater.Add("@ParentID", objModel.ParentID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@QDate", new DateTime(objModel.Year, objModel.Month, objModel.Day));

                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());
                using (var multi = con.QueryMultiple("sp_GetFeeSummeryForParentWise", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    //objModel.Classes = multi.Read<NameIDModel>().ToList();
                    //    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    //objModel.Sections = multi.Read<NameIDModel>().ToList();
                    //      objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.FeePayments = multi.Read<FeePaymentModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public StudentFeeModel GetFeePaymentsOld(StudentFeeModel objModel)
        {

            StartupModel objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@TransportFeeMode", objStartupModel.TransportFeeMode);
                paramater.Add("@HostelFeeMode", objStartupModel.HostelFeeMode);
                paramater.Add("@SessionStartDate", objStartupModel.SessionStartDate);
                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());
                paramater.Add("@LastPayDay", objStartupModel.FeePaymentReminderDate);
                paramater.Add("@SessionID", objModel.SessionID);
                using (var multi = con.QueryMultiple("sp_GetStudentFeeDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.FeePayments = multi.Read<FeePaymentModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public FeePaymentModel GetFeePaymentMonthwiseDetails(FeePaymentModel objModel)
        {
            StartupModel objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];
            FeePaymentModel objNew = new FeePaymentModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@PaymentID", objModel.PaymentID);
                paramater.Add("@Day", objModel.Day);
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@StudentID", objModel.StudentID);
                paramater.Add("@QDate", CommonUsage.GetCurrentDate());

                using (var multi = con.QueryMultiple("sp_GetFeePaymentMonthwiseDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objNew = multi.Read<FeePaymentModel>().SingleOrDefault();
                    objModel.PaymentDetails = multi.Read<FeeDetailsModel>().ToList();

                }
            }
            if (objNew != null)
            {
                objModel.ReferanceNumber = objNew.ReferanceNumber;
                objModel.PaymentType = objNew.PaymentType;
                objModel.PaymentMode = objNew.PaymentMode;
                objModel.Remark = objNew.Remark;
                objModel.PaymentAmount = objNew.PaymentAmount;
                objModel.PaymentStatus = objNew.PaymentStatus;
                objModel.Year = objNew.Year;
                objModel.Month = objNew.Month;
                objModel.PaymentDate = objNew.PaymentDate;
                objModel.PayMonths = objNew.PayMonths;
            }
            else
            {
                objModel.PaymentDate = CommonUsage.GetCurrentDate();
            }
            return objModel;
        }
        public FeePaymentModel GetFeePaymentDetails(FeePaymentModel objModel)
        {
            FeePaymentModel objNew = new FeePaymentModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@PaymentID", objModel.PaymentID);
                paramater.Add("@Day", objModel.Day);
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@StudentID", objModel.StudentID);
                paramater.Add("@QDate", CommonUsage.GetCurrentDate());
                paramater.Add("@SessionID", objModel.SessionID);

                using (var multi = con.QueryMultiple("sp_GetFeePaymentMonthwiseDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objNew = multi.Read<FeePaymentModel>().SingleOrDefault();
                    objModel.PaymentDetails = multi.Read<FeeDetailsModel>().ToList();
                    objModel.FeeTypes = multi.Read<NameIDModel>().ToList();
                    objModel.Months = multi.Read<PayDetailMonthsModel>().ToList();
                    objModel.SessionStartDate = multi.Read<DateTime>().SingleOrDefault();
                    objModel.SessionEndDate = multi.Read<DateTime>().SingleOrDefault();
                    objModel.FeePaymentMode = multi.Read<int>().SingleOrDefault();
                    objModel.SchoolSessionStartDate = multi.Read<DateTime>().SingleOrDefault();
                }
            }
            if (objNew != null)
            {
                objModel.ReferanceNumber = objNew.ReferanceNumber;
                objModel.PaymentType = objNew.PaymentType;
                objModel.PaymentMode = objNew.PaymentMode;
                objModel.Remark = objNew.Remark;
                objModel.PaymentAmount = objNew.PaymentAmount;
                objModel.PaymentStatus = objNew.PaymentStatus;
                objModel.PaymentDate = objNew.PaymentDate;
                objModel.FeePaymentMode = objNew.FeePaymentMode;
            }
            else
            {
                objModel.PaymentDate = CommonUsage.GetCurrentDate();
            }
            return objModel;
        }
        public async Task<FeePaymentModel> GetFeePaymentDetailsNew2(FeePaymentModel objModel)
        {
            FeePaymentModel objNew = new FeePaymentModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                DateTime qdate = new DateTime(objModel.Year, objModel.Month, 1).AddMonths(1).AddDays(-1);
                if (qdate.Day < objModel.Day)
                {
                    objModel.Day = qdate.Day;
                }
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", objModel.StudentID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@QDate", new DateTime(objModel.Year, objModel.Month, objModel.Day));
                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());

                using (var multi = await con.QueryMultipleAsync("sp_GetStudentFeeDetailViewData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.FeeTypeSummery = multi.Read<PayDetailFeeTypesModel>().ToList();
                    objModel.Months = multi.Read<PayDetailMonthsModel>().ToList();
                    objModel.PaymentDetails = multi.Read<FeeDetailsModel>().ToList();
                    objModel.SessionStartDate = multi.Read<DateTime>().SingleOrDefault();
                    objModel.SessionEndDate = multi.Read<DateTime>().SingleOrDefault();
                    objModel.FeePaymentMode = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }

        public async Task<FeePaymentModel> GetFeePaymentDetailsForPrint(FeePaymentModel objModel)
        {
            FeePaymentModel objNew = new FeePaymentModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                DateTime qdate = new DateTime(objModel.Year, objModel.Month, 1).AddMonths(1).AddDays(-1);
                if (qdate.Day < objModel.Day)
                {
                    objModel.Day = qdate.Day;
                }
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", objModel.StudentID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@QDate", new DateTime(objModel.Year, objModel.Month, objModel.Day));
                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());

                using (var multi = await con.QueryMultipleAsync("sp_GetStudentFeeDataForPrint", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.FeeTypeSummery = multi.Read<PayDetailFeeTypesModel>().ToList();
                    objModel.Months = multi.Read<PayDetailMonthsModel>().ToList();
                    objModel.PaymentDetails = multi.Read<FeeDetailsModel>().ToList();
                    objModel.SBranchDetails = multi.Read<SBranchModel>().SingleOrDefault();
                    objModel.StudentDetails = multi.Read<StudentModel>().SingleOrDefault();
                    objModel.SessionStartDate = multi.Read<DateTime>().SingleOrDefault();
                    objModel.SessionEndDate = multi.Read<DateTime>().SingleOrDefault();
                    objModel.FeePaymentMode = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public async Task<FeePaymentModel> GetFeePaymentReciptDetails(int PaymentID)
        {
            FeePaymentModel objNew = new FeePaymentModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@PaymentID", PaymentID);
                using (var multi = await con.QueryMultipleAsync("sp_FeePaymentRecieptData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objNew = multi.Read<FeePaymentModel>().SingleOrDefault();
                    if (objNew != null)
                    {
                        objNew.PaymentDetails = multi.Read<FeeDetailsModel>().ToList();
                        objNew.SBranchDetails = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                }
            }

            return objNew;
        }

        public List<FeePaymentModel> GetParentPayments(int ParentID, int SessionID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ParentID", ParentID);
                paramater.Add("@SessionID", SessionID);
                return con.Query<FeePaymentModel>("sp_GetParentFeePayments", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public FeeDiscountRequestMaster GetFeeDiscountRequestDetails(FeeDiscountRequestMaster objModel)
        {
            FeeDiscountRequestMaster objNew;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", objModel.StudentID);
                paramater.Add("@QDate", new DateTime(objModel.FeeYear, objModel.FeeMonth, 1));
                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@SessionID", objModel.SessionID);
                using (var multi = con.QueryMultiple("sp_GetFeeDiscountRequestDetailsV2", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objNew = multi.Read<FeeDiscountRequestMaster>().SingleOrDefault();
                    if (objNew == null)
                    {
                        objNew = new FeeDiscountRequestMaster();
                    }
                    objNew.Details = multi.Read<FeeDiscountRequestDetails>().ToList();
                    objNew.StudentName = multi.Read<string>().SingleOrDefault();
                    objNew.StudentID = objModel.StudentID;
                    objNew.FeeMonth = objModel.FeeMonth;
                    objNew.FeeYear = objModel.FeeYear;
                    objNew.RequestDate = objModel.RequestDate;
                }
            }

            return objNew;
        }
        public void GetStudentMonthWiseSessionFeeDetails(StudentSessionFeeStatusPageModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", objModel.StudentID);
                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());

                using (var multi = con.QueryMultiple("sp_GetParentStudentSessionFeeStatus", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.MonthlySummery = multi.Read<StudentYearMonthFeeSummeryModel>().ToList();
                    objModel.FeeWiseDetails = multi.Read<StudentYearMonthFeeTypeFeeDetailModel>().ToList();
                    try
                    {
                        objModel.SBranch = multi.Read<SBranchModel>().SingleOrDefault();

                    }
                    catch
                    {

                    }
                }


            }
        }
        public int UpdateFeeDiscountRequest(FeeDiscountRequestMaster objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@DiscRequestID", objData.DiscRequestID);
                paramater.Add("@StudentID", objData.StudentID);
                paramater.Add("@Remark", objData.Remark);
                paramater.Add("@RequestDate", objData.RequestDate);
                paramater.Add("@FeeMonth", objData.FeeMonth);
                paramater.Add("@FeeYear", objData.FeeYear);
                paramater.Add("@Status ", objData.Status);
                paramater.Add("@SBranchID ", objData.SBranchID);
                paramater.Add("@Details", objData.GetDiscountDetailsDataTable());

                return con.Query<int>("sp_InsertUpdateFeeDiscountReq", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public FeePaymentModel GetFeePaymentDetailsOld(FeePaymentModel objModel)
        {
            StartupModel objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];
            FeePaymentModel objNew = new FeePaymentModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", objModel.StudentID);
                paramater.Add("@PaymentID", objModel.PaymentID);
                paramater.Add("@TransportFeeMode", objStartupModel.TransportFeeMode);
                paramater.Add("@HostelFeeMode", objStartupModel.HostelFeeMode);
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@SessionStartDate", objStartupModel.SessionStartDate);
                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());
                paramater.Add("@LastPayDay", objStartupModel.FeePaymentReminderDate);

                using (var multi = con.QueryMultiple("sp_GetFeePaymentDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objNew = multi.Read<FeePaymentModel>().SingleOrDefault();
                    objModel.PaymentDetails = multi.Read<FeeDetailsModel>().ToList();
                    objModel.PreviourDuesDetails = multi.Read<FeeDetailsModel>().ToList();
                    objModel.StudentSID = multi.Read<string>().SingleOrDefault();
                    objModel.LateFee = multi.Read<decimal>().SingleOrDefault();
                }
            }
            if (objNew != null)
            {
                objModel.ReferanceNumber = objNew.ReferanceNumber;
                objModel.PaymentType = objNew.PaymentType;
                objModel.PaymentMode = objNew.PaymentMode;
                objModel.Remark = objNew.Remark;
                objModel.PaymentAmount = objNew.PaymentAmount;
                objModel.PaymentStatus = objNew.PaymentStatus;
                objModel.Year = objNew.Year;
                objModel.Month = objNew.Month;
                objModel.PaymentDate = objNew.PaymentDate;
            }
            else
            {
                objModel.PaymentDate = CommonUsage.GetCurrentDate();
            }
            return objModel;
        }
        public int UpdateStudentFeePayments(FeePaymentModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@PaymentID", objData.PaymentID);
                paramater.Add("@PayeeID", objData.StudentID);
                paramater.Add("@ReferanceNumber", objData.ReferanceNumber);
                paramater.Add("@PaymentType", objData.PaymentType);
                paramater.Add("@PaymentMode", objData.PaymentMode);
                paramater.Add("@Remark", objData.Remark);
                paramater.Add("@PaymentAmount", objData.PaymentAmount);
                paramater.Add("@PaymentStatus", objData.PaymentStatus);
                paramater.Add("@Year", objData.Year);
                paramater.Add("@Month", objData.Month);
                paramater.Add("@PaymentDate", objData.PaymentDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@PayeeType", objData.PayeeType);
                paramater.Add("@PaymentDetails", objData.GetFeePaymentsDataTable());

                return con.Query<int>("sp_UpdateStudentFeeDetail", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public FeePaymentModel SaveStudentFeePaymentsOld(FeePaymentModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@Day", objData.Day);
                paramater.Add("@Month", objData.Month);
                paramater.Add("@Year", objData.Year);
                paramater.Add("@StudentID", objData.StudentID);
                paramater.Add("@QDate", objData.QDate);
                paramater.Add("@PaymentAmount", objData.PaymentAmount);
                paramater.Add("@WaiverMonths", objData.WaiverMonths);
                paramater.Add("@Remark", objData.Remark);
                paramater.Add("@ReferanceNumber", objData.ReferanceNumber);
                paramater.Add("@PaymentMode", objData.PaymentMode);
                paramater.Add("@CollectedBy", objData.CollectedBy);
                paramater.Add("@SessionID", objData.SessionID);
                paramater.Add("@ExcludedFees", objData.ExcludedFees);

                return con.Query<FeePaymentModel>("spn_SaveFeePayment", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public FeePaymentRowModel SaveStudentFeePayments(FeePaymentModel objData)
        {
            FeePaymentRowModel obj = new FeePaymentRowModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@StudentID", objData.StudentID);
                paramater.Add("@QDate", new DateTime(objData.Year, objData.Month, 1));
                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());

                paramater.Add("@PaymentDate", objData.PaymentDate);


                paramater.Add("@PaymentAmount", objData.PaymentAmount);
                paramater.Add("@WaiverMonths", objData.WaiverMonths);
                paramater.Add("@Remark", objData.Remark);
                paramater.Add("@ReferanceNumber", objData.ReferanceNumber);
                paramater.Add("@PaymentMode", objData.PaymentMode);
                paramater.Add("@CollectedBy", objData.CollectedBy);
                paramater.Add("@SessionID", objData.SessionID);
                paramater.Add("@ExcludedFees", objData.ExcludedFees);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@SBranchID", objData.SBranchID);

                using (var multi = con.QueryMultiple("spn_SaveFeePaymentV2", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    obj.StudentDetails = multi.Read<FeePaymentModel>().SingleOrDefault();
                    obj.PaymentID = multi.Read<int>().SingleOrDefault();
                }

                return obj;
            }
        }
        public List<FeePaymentModel> GetStudentFeePaymentsSessionwise(int StudentID, int SessionID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SessionID", SessionID);

                return con.Query<FeePaymentModel>("spn_GetStudentFeePayments", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        public NotificationSMSModel GetFeePaymentSMSDetails(int PaymentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@PaymentID", PaymentID);

                return con.Query<NotificationSMSModel>("sp_GetStudentPaymentSMSDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #endregion
        #region Employee Management Module

        public EmployeeListPageModel GetEmployeesReport(int EmployeeType, int SBranchID)
        {

            EmployeeListPageModel objModel = new EmployeeListPageModel();
            objModel.EmployeeType = EmployeeType;
            objModel.SBranchID = SBranchID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeTypeID", EmployeeType);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetAllEmployeeList", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Employees = multi.Read<EmployeeModel>().ToList();
                    objModel.EmployeeTypes = multi.Read<EmployeeTypeModel>().ToList();
                    try
                    {
                        objModel.EmployeeType = multi.Read<int>().SingleOrDefault();
                    }
                    catch
                    { }
                }
            }
            return objModel;
        }
        public EmployeeListPageModel GetEmployees(int EmployeeType, int SBranchID)
        {

            EmployeeListPageModel objModel = new EmployeeListPageModel();
            objModel.EmployeeType = EmployeeType;
            objModel.SBranchID = SBranchID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeTypeID", EmployeeType);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetEmployeeList", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Employees = multi.Read<EmployeeModel>().ToList();
                    objModel.EmployeeTypes = multi.Read<EmployeeTypeModel>().ToList();
                    try
                    {
                        objModel.EmployeeType = multi.Read<int>().SingleOrDefault();
                    }
                    catch
                    { }
                }
            }
            return objModel;
        }

        public int DeleteEmployee(int EmployeeID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", EmployeeID);
                return con.Query<int>("spn_DeleteEmployee", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public EmployeeEditModel GetEmployeeDetails(int EmployeeID, int SBranchID)
        {

            EmployeeEditModel objModel = new EmployeeEditModel();
            objModel.EmployeeID = EmployeeID;

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", EmployeeID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetEmployeeDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.EmployeeTypes = multi.Read<EmployeeTypeModel>().ToList();
                    if (EmployeeID != 0)
                    {
                        objModel.EmployeeDetails = multi.Read<EmployeeModel>().SingleOrDefault();
                        objModel.Countries = multi.Read<NameIDModel>().ToList();
                        objModel.States = multi.Read<NameIDModel>().ToList();
                        objModel.Cities = multi.Read<NameIDModel>().ToList();
                        objModel.Areas = multi.Read<NameIDModel>().ToList();
                        objModel.Salary = multi.Read<EmployeeSalaryModel>().ToList();
                        objModel.Education = multi.Read<EmployeeEducationModel>().ToList();
                        objModel.Experience = multi.Read<EmployeeExperienceModel>().ToList();
                        objModel.THADDetails = multi.Read<EmployeeTransportHostelAllocationDelocationModel>().ToList();
                        //objModel.Transport.StopDetails = multi.Read<RouteStoppageModel>().SingleOrDefault();
                        //objModel.Transport.Countries = multi.Read<NameIDModel>().ToList();
                        //objModel.Transport.States = multi.Read<NameIDModel>().ToList();
                        //objModel.Transport.Cities = multi.Read<NameIDModel>().ToList();
                        //objModel.Transport.Areas = multi.Read<NameIDModel>().ToList();
                        //objModel.Transport.Routes = multi.Read<NameIDModel>().ToList();
                        //objModel.Transport.Vehicles = multi.Read<NameIDModel>().ToList();
                        //objModel.Transport.Stops = multi.Read<RouteStoppageModel>().ToList();
                        objModel.Leaves = multi.Read<EmployeeLeaveQuotaModel>().ToList();
                        objModel.Subjects = multi.Read<Teacher_SubjectModel>().ToList();
                        objModel.EducationLevels = multi.Read<NameIDModel>().ToList();
                        objModel.Groups = multi.Read<NameIDModel>().ToList();
                        objModel.GroupSubjects = multi.Read<NameIDModel>().ToList();
                        objModel.EmployeeDetails.DOR = CommonUsage.GetCurrentDate();

                        // objModel.SubReligions = multi.Read<NameIDModel>().ToList();
                    }
                    else
                    {
                        objModel.EmployeeDetails = new EmployeeModel();
                        objModel.EmployeeDetails.DOB = CommonUsage.GetCurrentDate();
                        objModel.EmployeeDetails.DOJ = CommonUsage.GetCurrentDate();

                    }
                    objModel.Nationalities = multi.Read<NameIDModel>().ToList();
                    objModel.Religions = multi.Read<NameIDModel>().ToList();
                    objModel.MotherTongues = multi.Read<NameIDModel>().ToList();
                    objModel.SocialCategories = multi.Read<NameIDModel>().ToList();

                }
            }
            return objModel;
        }
        public int InsertUpdateEmployeeBasic(EmployeeModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", objData.EmployeeID);
                paramater.Add("@EmployeeName", objData.EmployeeName);
                paramater.Add("@DOB", objData.DOB);
                paramater.Add("@Gender", objData.Gender);
                paramater.Add("@Nationality", objData.Nationality);
                paramater.Add("@ReligionID", objData.ReligionID);
                paramater.Add("@DOJ", objData.DOJ);
                paramater.Add("@EmailID", objData.EmailID);
                paramater.Add("@CategoryID", objData.CategoryID);
                paramater.Add("@MaritalStatus", objData.MaritalStatus);
                paramater.Add("@BloodGroup", objData.BloodGroup);
                paramater.Add("@MotherTongueID", objData.MotherTongueID);
                paramater.Add("@PassportNo", objData.PassportNo);
                paramater.Add("@MobileNo", objData.MobileNumber);
                paramater.Add("@PhoneNo", objData.LandLineNumber);
                paramater.Add("@AccessCardNo", objData.AccessCardNo);
                paramater.Add("@FatherHubName", objData.FatherHubName);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@ImageName", objData.Photo);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@EmployeeType", objData.EmployeeType);
                paramater.Add("@Password", objData.Password);
                paramater.Add("@AadharNumber", objData.AadharNumber);
                paramater.Add("@LicenceNumber", objData.LicenceNumber);

                return con.Query<int>("spn_InsertUpdateEmployeeBasicDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int InsertUpdateEmployeeAccount(EmployeeModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", objData.EmployeeID);
                paramater.Add("@PANnumber", objData.PANnumber);
                paramater.Add("@PFNumber", objData.PFNumber);
                paramater.Add("@BankName", objData.BankName);
                paramater.Add("@AccountNumber", objData.AccountNumber);
                paramater.Add("@AccountName", objData.AccountName);
                paramater.Add("@IFSCCode", objData.IFSCCode);

                return con.Query<int>("spn_UpdateEmployeeAccountDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public int InsertUpdateEmployeeLeft(EmployeeModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", objData.EmployeeID);
                paramater.Add("@IsLeft", objData.IsLeft);
                paramater.Add("@DOR", objData.DOR);


                return con.Query<int>("spn_UpdateEmployeeLeftDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int InsertUpdateEmployeeAddress(EmployeeModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", objData.EmployeeID);
                paramater.Add("@Cadd_HouseNo", objData.Cadd_HouseNo);
                paramater.Add("@Cadd_Street", objData.Cadd_Street);
                paramater.Add("@Cadd_Area", objData.Cadd_Area);
                paramater.Add("@Cadd_Sector", objData.Cadd_Sector);
                paramater.Add("@Cadd_PinCode", objData.Cadd_PinCode);
                paramater.Add("@Cadd_DistrictCode", objData.Cadd_DistrictCode);
                paramater.Add("@Cadd_StateCode", objData.Cadd_StateCode);
                paramater.Add("@Cadd_CountryCode", objData.Cadd_CountryCode);
                paramater.Add("@Padd_HouseNo", objData.Padd_HouseNo);
                paramater.Add("@Padd_Street", objData.Padd_Street);
                paramater.Add("@Padd_Area", objData.Padd_Area);
                paramater.Add("@Padd_Sector", objData.Padd_Sector);
                paramater.Add("@Padd_PinCode", objData.Padd_PinCode);
                paramater.Add("@Padd_District", objData.Padd_District);
                paramater.Add("@Padd_State", objData.Padd_State);
                paramater.Add("@Padd_Country", objData.Padd_Country);
                paramater.Add("@SBranchID", objData.SBranchID);

                return con.Query<int>("sp_UpdateEmployeeAddress", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int InsertUpdateEmployeeSalary(EmployeeEditModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SalaryDetails", objData.GetSalaryDataTable());

                return con.Query<int>("sp_UpdateEmployeeSalaryDetail", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int InsertUpdateEmployeeEducation(EmployeeEducationModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmpEduID", objData.EmpEduID);
                paramater.Add("@EmployeeID", objData.EmployeeID);
                paramater.Add("@Qualification", objData.Qualification);
                paramater.Add("@Stream", objData.Stream);
                paramater.Add("@InstituteName", objData.InstituteName);
                paramater.Add("@StartDate", objData.StartDate);
                paramater.Add("@EndDate", objData.EndDate);
                paramater.Add("@Grade", objData.Grade);
                paramater.Add("@OperationType", objData.OpType);

                return con.Query<int>("sp_UpdateEmployeeEducation", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int InsertUpdateEmployeeExperience(EmployeeExperienceModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmpExpID", objData.EmpExpID);
                paramater.Add("@EmployeeID", objData.EmployeeID);
                paramater.Add("@InstituteName", objData.InstituteName);
                paramater.Add("@Designation", objData.Designation);
                paramater.Add("@FromDate", objData.FromDate);
                paramater.Add("@EndDate", objData.EndDate);
                paramater.Add("@SubjectsClass", objData.SubjectsClasses);
                paramater.Add("@OperationType", objData.OpType);

                return con.Query<int>("sp_UpdateEmployeeExperience", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int UpdateEmployeeDocuments(EmployeeModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", objData.EmployeeID);
                paramater.Add("@BirthCertificate", objData.BirthCertificate);
                paramater.Add("@AddressProof", objData.AddressProof);
                paramater.Add("@ExperienceCertificate", objData.ExperienceCertificate);
                paramater.Add("@PFDeclaration", objData.PFDeclaration);
                paramater.Add("@PANCardCopy", objData.PANCardCopy);
                paramater.Add("@MedicalCertificate", objData.MedicalCertificate);
                paramater.Add("@CategoryCertificate", objData.CategoryCertificate);
                paramater.Add("@RelievingCertificate", objData.RelievingCertificate);
                paramater.Add("@AppointmentLetter", objData.AppointmentLetter);
                paramater.Add("@BankAccountProof", objData.BankAccountProof);
                paramater.Add("@EmpSignature", objData.EmpSignature);

                return con.Query<int>("sp_UpdateEmployeeCertificates", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public int InsertUpdateEmployeeTransport(EmployeeTransportHostelAllocationDelocationModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@THChangeID", objData.THChangeID);
                paramater.Add("@VehicleRouteID", objData.VehicleRouteID);
                paramater.Add("@ChangeType", objData.ChangeType);
                paramater.Add("@UserType", objData.UserType);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@StartDate", objData.StartDate);
                paramater.Add("@EndDate", objData.EndDate);
                paramater.Add("@SessionID", objData.SessionID);
                paramater.Add("@KeyID", objData.KeyID);
                paramater.Add("@Applicable", objData.Applicable);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@SBranchID", objData.SBranchID);

                return con.Query<int>("sp_UpdateEmployeeTransportAllocationDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public EmployeeTransportModel GetEmployeeTransportDetails(int EmployeeID, int SBranchID, int THChangeID)
        {

            EmployeeTransportModel Transport = new EmployeeTransportModel();
            Transport.EmployeeID = EmployeeID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", EmployeeID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@THChangeID", THChangeID);
                using (var multi = con.QueryMultiple("spn_GetEmployeeTransportDetailsNew2", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    Transport.StopDetails = multi.Read<RouteStoppageModel>().SingleOrDefault();
                    Transport.Routes = multi.Read<NameIDModel>().ToList();
                    Transport.Vehicles = multi.Read<NameIDModel>().ToList();
                    Transport.Stops = multi.Read<RouteStoppageModel>().ToList();
                    Transport.THADetails = multi.Read<EmployeeTransportHostelAllocationDelocationModel>().SingleOrDefault();

                }
            }
            return Transport;
        }

        public int UpdateEmployeeTransport(EmployeeTransportModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@EmployeeID", objData.EmployeeID);
                paramater.Add("@VehicleRouteID", objData.VehicleRouteID);
                paramater.Add("@StopID", objData.StopID);
                paramater.Add("@SBranchID", objData.SBranchID);

                return con.Query<int>("sp_UpdateEmployeeTransportDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int InsertUpdateEmployeeLeaves(EmployeeEditModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@LeaveDetails", objData.GetLeaveDetailsDataTable());

                return con.Query<int>("sp_UpdateEmployeeLeaveDetail", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public IEnumerable<NameIDModel> GetEducationLevelGroups(int EducationLevelID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EducationLevelID", EducationLevelID);
                return con.Query<NameIDModel>("sp_GetGroupsOnEducationLevel", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public IEnumerable<NameIDModel> GetGroupSubjectList(int GroupID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@GroupID", GroupID);
                return con.Query<NameIDModel>("sp_GetSubjectListForGroup", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdateTeacherSubject(Teacher_SubjectModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TSID", objData.TSID);
                paramater.Add("@SubjectID", objData.SubjectID);
                paramater.Add("@EducationLevelID", objData.EducationLevelID);
                paramater.Add("@TeacherID", objData.EmployeeID);
                paramater.Add("@OperationType", objData.OpType);
                paramater.Add("@GroupID", objData.GroupID);

                return con.Query<int>("sp_UpdateTeacherSubject", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public EmployeeAttandancePageModel GetEmployeesAttandance(EmployeeAttandancePageModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@Day", objModel.Day);
                paramater.Add("@EmployeeTypeID", objModel.EmployeeType);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_GetEmployeeAttandanceDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.EmployeeTypes = multi.Read<NameIDModel>().ToList();
                    objModel.EmployeeAttandances = multi.Read<EmployeeAttandanceModel>().ToList();
                }
            }
            return objModel;
        }
        public EmployeeAttandanceReviewPageModel GetEmployeesAttandanceReview(EmployeeAttandanceReviewPageModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@CDate", objModel.AttandanceDate);
                paramater.Add("@EmployeeType", objModel.EmployeeType);
                using (var multi = con.QueryMultiple("spn_GetDailyEmployeeAttandanceReview", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Attandances = multi.Read<EmployeeAttandanceReviewModel>().ToList();
                    objModel.EmployeeTypes = multi.Read<NameIDModel>().ToList();
                    objModel.EmployeeType = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public int UpdateEmployeesAttandance(EmployeeAttandancePageModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@DesignationID", objModel.EmployeeType);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Day", objModel.Day);
                paramater.Add("@List", objModel.GetAttandanceDetailsDataTable());
                paramater.Add("@SBranchID", objModel.SBranchID);
                return con.Query<int>("sp_UpdateEmployeeAttandance", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public IEnumerable<AppAttandanceModel> GetEmployeeAttandanceDetails(int EmployeeID, DateTime aDate)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", EmployeeID);
                paramater.Add("@CurrDate", aDate);
                return con.Query<AppAttandanceModel>("spn_GetAppAttandanceStatus", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public IEnumerable<AppAttandanceModel> UpdateEmployeeAttandanceReview(AppAttandanceModel model)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", model.EmployeeID);
                paramater.Add("@AttandanceDate", model.logdatetime);
                paramater.Add("@Comment", model.Comment);
                paramater.Add("@Status", model.Status);
                return con.Query<AppAttandanceModel>("spn_UpdateEmployeeAttandanceReview", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }

        public SBranchModel GetBranchPaymentProfile(int sBranchId)
        {
            using (var con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@SBranchID", sBranchId);

                // Stored Procedure call using CommandType
                return con.Query<SBranchModel>("usp_GetBranchProfile", parameters, commandType: CommandType.StoredProcedure
                ).SingleOrDefault();
            }
        }
        #endregion
        #region Employee Advance Payments
        public EmployeeAdvancePaymentListModel GetAdvancePaymentEmployeeList(int Status, int SBranchID)
        {
            EmployeeAdvancePaymentListModel objModel = new EmployeeAdvancePaymentListModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Status", Status);
                paramater.Add("@SBranchID", SBranchID);
                objModel.Employees = con.Query<EmployeeAdvancePaymentModel>("sp_GetEmployeeAdvancePayments", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
            return objModel;
        }
        public EmployeeAdvancePaymentModel GetAdvancePaymentDeductions(int AdPayID)
        {
            EmployeeAdvancePaymentModel objModel = new EmployeeAdvancePaymentModel();
            objModel.AdPaymentID = AdPayID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@AdPaymentID", AdPayID);
                objModel.Deductions = con.Query<EmployeeAdvancePaymentDeductionModel>("sp_GetEmployeeAdvancePaymentDetails", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
            return objModel;
        }
        public EmployeeAdvancePaymentEditModel GetAdvancePaymentDetails(int AdPayID, int SBranchID)
        {
            EmployeeAdvancePaymentEditModel objModel = new EmployeeAdvancePaymentEditModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@AdPaymentID", AdPayID);
                paramater.Add("@SBranchID", SBranchID);
                objModel.Employees = con.Query<EmployeeModel>("sp_GetEmployeeListForAdvancePayment", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
            objModel.Payment = new EmployeeAdvancePaymentModel();
            objModel.Payment.PaymentDate = CommonUsage.GetCurrentDate();
            return objModel;
        }
        public int AddAdvancePaymentEmployee(EmployeeAdvancePaymentModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", objData.EmployeeID);
                paramater.Add("@Amount", objData.Amount);
                paramater.Add("@PaymentDate", objData.PaymentDate);
                paramater.Add("@EMI", objData.EMI);
                paramater.Add("@Months", objData.Months);
                paramater.Add("@Title", objData.Title);
                paramater.Add("@Description", objData.Description);
                paramater.Add("@EmployeeType", objData.EmployeeType);
                paramater.Add("@SBranchID", objData.SBranchID);
                return con.Query<int>("sp_AddEmployeeAdvancePayments", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public EmployeeAdvancePaymentModel AddAdvancePaymentDeductions(EmployeeAdvancePaymentDeductionModel objData)
        {
            EmployeeAdvancePaymentModel objModel = new EmployeeAdvancePaymentModel();
            objModel.AdPaymentID = objData.AdPayID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@AdPaymentID", objData.AdPayID);
                paramater.Add("@Date", objData.Date);
                paramater.Add("@Amount", objData.Amount);
                paramater.Add("@Mode", objData.Mode);
                paramater.Add("@Remark", objData.Remark);
                objModel.Deductions = con.Query<EmployeeAdvancePaymentDeductionModel>("sp_InsertEmployeeAdvancePaymentDeduction", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
            return objModel;
        }
        #endregion
        #region Employee Leaves
        public IEnumerable<EmployeeLeaveModel> GetEmployeeLeaves(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<EmployeeLeaveModel>("sp_GetEmployeeLeaves", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public EmployeeLeaveDetailModel GetEmployeeLeavesEditDetailsNew(int LeaveID, int SBranchID, int Month, int Year, int SessionID, int EmployeeID, int EmployeeType)
        {
            EmployeeLeaveDetailModel objModel = new EmployeeLeaveDetailModel();
            objModel.LeaveID = LeaveID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@LeaveID", LeaveID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                paramater.Add("@Month", Month);
                paramater.Add("@Year", Year);
                paramater.Add("@EmployeeID", EmployeeID);
                paramater.Add("@EmployeeType", EmployeeType);
                using (var multi = con.QueryMultiple("sp_GetEmployeeLeavesAccountNew", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.LeaveBalance = multi.Read<EmployeeLeaveSummery>().ToList();
                    objModel.Employees = multi.Read<EmployeeModel>().ToList();
                    objModel.Leave = multi.Read<LeaveModel>().SingleOrDefault();
                    objModel.EmployeeID = multi.Read<int>().SingleOrDefault();
                    objModel.EmployeeType = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public List<EmployeeLeaveSummery> GetEmployeeLeaveBalanceDetails(int LeaveID, int SBranchID, int Month, int Year, int SessionID, int EmployeeID, int EmployeeType)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@LeaveID", LeaveID);
                paramater.Add("@EmployeeID", EmployeeID);
                paramater.Add("@Month", Month);
                paramater.Add("@Year", Year);
                paramater.Add("@EmployeeType", EmployeeType);
                paramater.Add("@SessionID", SessionID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetEmployeeLeaveDetailsNew", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    return multi.Read<EmployeeLeaveSummery>().ToList();
                }
            }
        }
        public List<EmployeeLeaveSummery> GetEmployeeLeaveBalanceDetailsDifferentMonths(int LeaveID, int SBranchID, DateTime StartDate, DateTime EndDate, int SessionID, int EmployeeID, int EmployeeType)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@LeaveID", LeaveID);
                paramater.Add("@EmployeeID", EmployeeID);
                paramater.Add("@StartDate", StartDate);
                paramater.Add("@EndDate", EndDate);
                paramater.Add("@EmployeeType", EmployeeType);
                paramater.Add("@SessionID", SessionID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetEmployeeLeaveDetailsNew2Months", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    return multi.Read<EmployeeLeaveSummery>().ToList();
                }
            }
        }
        public EmployeeLeaveAddModel GetEmployeeLeavesEditDetails(int LeaveID, int SBranchID, int Month, int Year, int EmployeeID, int EmployeeType)
        {
            EmployeeLeaveAddModel objModel = new EmployeeLeaveAddModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@LeaveID", LeaveID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@Month", Month);
                paramater.Add("@Year", Year);
                paramater.Add("@EmployeeID", EmployeeID);
                paramater.Add("@EmployeeType", EmployeeType);
                using (var multi = con.QueryMultiple("sp_GetEmployeeLeavesAccount", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<EmployeeLeaveAddModel>().SingleOrDefault();
                    objModel.Employees = multi.Read<EmployeeModel>().ToList();
                }
            }
            return objModel;
        }
        public int UpdateEmployeeLeaveNew(EmployeeLeaveModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@LeaveID", objData.LeaveID);
                paramater.Add("@ApplicantType", objData.ApplicantType);
                paramater.Add("@LeaveType", objData.LeaveType);
                paramater.Add("@StartDate", objData.StartDate);
                paramater.Add("@EndDate", objData.EndDate);
                paramater.Add("@LeaveReason", objData.LeaveReason);
                paramater.Add("@ApplicantID", objData.EmployeeID);
                paramater.Add("@IsApproved", objData.IsApproved);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@CreatedDate", objData.CreatedDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@EmployeeType", objData.EmployeeType);
                paramater.Add("@LeaveTypeApplied", objData.LeaveTypeApplied);
                paramater.Add("@OpType", objData.OpType);
                return con.Query<int>("sp_InsertEmployeeLeaveNew", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public int AddEmployeeLeave(EmployeeLeaveModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ApplicantType", objData.ApplicantType);
                paramater.Add("@LeaveType", objData.LeaveType);
                paramater.Add("@StartDate", objData.StartDate);
                paramater.Add("@EndDate", objData.EndDate);
                paramater.Add("@LeaveReason", objData.LeaveReason);
                paramater.Add("@ApplicantID", objData.EmployeeID);
                paramater.Add("@IsApproved", objData.IsApproved);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@CreatedDate", objData.CreatedDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@EmployeeType", objData.EmployeeType);
                paramater.Add("@LeaveTypeApplied", objData.LeaveTypeApplied);
                return con.Query<int>("sp_InsertEmployeeLeave", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public int UpdateSalaryProcessing(SalaryProcessingModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SPMID", oModel.SPMID);
                paramater.Add("@EmployeeID", oModel.EmployeeID);
                paramater.Add("@EmployeeType", oModel.EmployeeType);
                paramater.Add("@ProcessingDate", oModel.ProcessingDate);
                paramater.Add("@SMonth", oModel.SMonth);
                paramater.Add("@SYear", oModel.SYear);
                paramater.Add("@ReferanceNumber", oModel.ReferanceNumber);
                paramater.Add("@Remark", oModel.Remark);
                paramater.Add("@UserID", oModel.UserID);
                paramater.Add("@CreatedDate", oModel.CreatedDate);
                paramater.Add("@PaymentMode", oModel.PaymentMode);
                paramater.Add("@SalaryAmount", oModel.SalaryAmount);
                paramater.Add("@PaidAmount", oModel.PaidAmount);
                paramater.Add("@Status", oModel.Status);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@TotalDays", oModel.TotalDays);
                paramater.Add("@PresentDays", oModel.PresentDays);
                paramater.Add("@LWP", oModel.LWP);
                paramater.Add("@PaidLeaves", oModel.PaidLeaves);
                paramater.Add("@Holidays", oModel.Holidays);
                paramater.Add("@WeekOffs", oModel.WeekOffs);
                paramater.Add("@GrossEarning", oModel.GrossEarning);
                paramater.Add("@TotalDeductions", oModel.TotalDeductions);
                paramater.Add("@NetPayble", oModel.NetPayble);
                paramater.Add("@SalaryDetails", oModel.GetSalaryDetailsDataTable());
                return con.Query<int>("sp_UpdateEmployeeSalaryPayment", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #endregion
        #region Employee Salary 
        public EmployeeSalaryListPageModel GetEmployeesForSalaries(EmployeeSalaryListPageModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@EmployeeTypeID", objModel.EmployeeTypeID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_GetEmployeeSalaryList", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.EmployeeTypes = multi.Read<EmployeeTypeModel>().ToList();
                    objModel.Employees = multi.Read<EmployeeSalaryListModel>().ToList();
                }
            }
            return objModel;
        }
        public EmployeeSalaryDetailModel GetEmployeeSalaryDetails(EmployeeSalaryDetailModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", objModel.EmployeeID);
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@EmployeeTypeID", objModel.EmployeeTypeID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_GetEmployeeSalaryPaymentDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    objModel.Employee = multi.Read<EmployeeModel>().SingleOrDefault();
                    objModel.DaysPayble = multi.Read<decimal>().SingleOrDefault();
                    objModel.SalaryHeads = multi.Read<EmployeeSalaryDetailsListModel>().ToList();
                }
            }
            return objModel;
        }

        public SalarySlipModel GetEmployeeSalarySlipData(SalarySlipModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SPMID", objModel.SPMID);
                paramater.Add("@EmployeeID", objModel.EmployeeID);
                paramater.Add("@SalaryMonth", objModel.SalaryMonth);
                paramater.Add("@SalaryYear", objModel.SalaryYear);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@EmployeeType", objModel.EmployeeType);
                using (var multi = con.QueryMultiple("sp_GetEmployeeSalaryPaymentSlipData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    objModel.Employee = multi.Read<EmployeeModel>().SingleOrDefault();
                    objModel.Salary = multi.Read<SalaryProcessingModel>().SingleOrDefault();
                    objModel.Details = multi.Read<SalaryProcessingDetailModel>().ToList();
                }
            }
            return objModel;
        }
        public EmployeeSalaryDetailsPageModel GetEmployeeSalaryDetailsNew(EmployeeSalaryDetailsPageModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", objModel.EmployeeID);
                paramater.Add("@SalaryMonth", objModel.SalaryMonth);
                paramater.Add("@SalaryYear", objModel.SalaryYear);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@EmployeeType", objModel.EmployeeType);
                using (var multi = con.QueryMultiple("sp_GetEmployeeSalaryDetailsNew", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    objModel.Employee = multi.Read<EmployeeModel>().SingleOrDefault();
                    objModel.Categories = multi.Read<EmployeeSalaryDetailsListModel>().ToList();
                    objModel.Leaves = multi.Read<DayLeaveStatusModel>().ToList();
                    objModel.Holidays = multi.Read<DayHolidayStatusModel>().ToList();
                    objModel.Summery = multi.Read<EmployeeMonthSalarySummeryModel>().SingleOrDefault();
                    objModel.AdvancePayments = multi.Read<EmployeeSalaryDetailsListModel>().ToList();
                }
            }
            return objModel;
        }
        #endregion
        #region ExpenceManagement
        public ExpenceManagementModel GetExpences(ExpenceManagementModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@ExpenceType", objModel.ExpenceType);
                using (var multi = con.QueryMultiple("sp_GetExpences", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.ExpenceTypes = multi.Read<NameIDModel>().ToList();
                    objModel.Expences = multi.Read<ExpenceModel>().ToList();
                }
            }
            return objModel;
        }
        public ExpenceModel GetExpenceDetails(int ExpenceID, int SBranchID)
        {
            ExpenceModel objModel = new ExpenceModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ExpenceID", ExpenceID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetExpenceDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<ExpenceModel>().SingleOrDefault();
                    if (ExpenceID == 0)
                    {
                        objModel = new ExpenceModel();
                        objModel.PaymentDate = CommonUsage.GetCurrentDate();
                        objModel.EnteredDate = CommonUsage.GetCurrentDate();
                    }
                    objModel.ExpenceTypes = multi.Read<NameIDModel>().ToList();
                    objModel.ExpenceDetails = multi.Read<ExpenceDetailsModel>().ToList();
                    objModel.ExpenceBills = multi.Read<ExpenceBillModel>().ToList();
                }
            }
            return objModel;
        }
        public int InsertUpdateExpence(ExpenceModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ExpenceID", objData.ExpenceID);
                paramater.Add("@ReferanceNumber", objData.ReferanceNumber);
                paramater.Add("@ExpenceTitle", objData.ExpenceTitle);
                paramater.Add("@PaymentType", objData.PaymentType);
                paramater.Add("@PaymentMode", objData.PaymentMode);
                paramater.Add("@Remark", objData.Remark);
                paramater.Add("@PaymentStatus", objData.PaymentStatus);
                paramater.Add("@Year", objData.Year);
                paramater.Add("@Month", objData.Month);
                paramater.Add("@PaymentDate", objData.PaymentDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@EnteredDate", objData.EnteredDate);
                paramater.Add("@ExpenceTypeID", objData.ExpenceTypeID);
                paramater.Add("@ExpenceDetails", objData.GetExpenceDetailsDataTable());
                paramater.Add("@ExpenceBills", objData.GetExpenceBillsDataTable());
                return con.Query<int>("spn_InsertUpdateExpence", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public int DeleteExpence(int ExpenceID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ExpenceID", ExpenceID);
                return con.Query<int>("spn_DeleteExpence", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        #endregion
        #region DashBoard Module
        public AccountDashboardModel GetDashboardPageData(AccountDashboardModel objModel)

        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@Day", objModel.DayID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_GetAccountDashboardData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Collection = multi.Read<ClassFeeCollectionChartModel>().ToList();
                    objModel.Students = multi.Read<NameIDModel>().ToList();
                    objModel.Employees = multi.Read<NameIDModel>().ToList();
                    objModel.MonthlySale = multi.Read<decimal>().SingleOrDefault();
                    objModel.MonthlyPurchase = multi.Read<decimal>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public AccountDashboardModel GetDashboardFeeChart(AccountDashboardModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@SBranchID", objModel.SBranchID);
                objModel.Collection = con.Query<ClassFeeCollectionChartModel>("sp_GetAccountDashFeeChart", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
            return objModel;
        }
        #endregion        
        #region Employee Leaves
        public StudentLeavePageModel GetStudentLeaves(int SBranchID, int ClassID, int SectionID)
        {
            StudentLeavePageModel objModel = new StudentLeavePageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                using (var multi = con.QueryMultiple("sp_GetStudentLeaves", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentLeaveModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                }
            }
            objModel.ClassID = ClassID;
            objModel.SectionID = SectionID;
            return objModel;
        }
        public StudentLeaveAddModel GetStudentLeavesEditDetails(int SBranchID)
        {
            StudentLeaveAddModel objModel = new StudentLeaveAddModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetAddStudentLeaveDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Students = multi.Read<NameIDModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public int AddStudentLeave(StudentLeaveModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@LeaveType", objData.LeaveType);
                paramater.Add("@StartDate", objData.StartDate);
                paramater.Add("@EndDate", objData.EndDate);
                paramater.Add("@LeaveReason", objData.LeaveReason);
                paramater.Add("@ApplicantID", objData.StudentID);
                paramater.Add("@IsApproved", objData.IsApproved);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@CreatedDate", objData.CreatedDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                return con.Query<int>("sp_InsertStudentLeave", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public IEnumerable<NameIDModel> GetSectionStudentList(int SectionID, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<NameIDModel>("sp_GetSectionStudentList", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        #endregion
        #region Report

        public MonthlyAttendancePageModel GetMonthlyAttendance(MonthlyAttendancePageModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeType", oModel.EmployeeType);
                paramater.Add("@SBranchID", oModel.BranchID);
                paramater.Add("@RepoDate", oModel.RepoDate);
                using (var multi = con.QueryMultiple("sp_GetMonthlyEmployeeAttendance", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Employees = multi.Read<EmployeeModel>().ToList();
                    oModel.Attendance = multi.Read<MonthlyAttendanceLogModel>().ToList();
                    oModel.EmployeeTypes = multi.Read<EmployeeTypeModel>().ToList();
                }
            }
            return oModel;

        }

        public MonthlyAttendancePageModel GetMonthlyAttendanceStatus(MonthlyAttendancePageModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeType", oModel.EmployeeType);
                paramater.Add("@SBranchID", oModel.BranchID);
                paramater.Add("@RepoDate", oModel.RepoDate);
                using (var multi = con.QueryMultiple("sp_GetMonthlyEmployeeAttendanceStatus", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.StatusAttendance = multi.Read<SMEnterpriseDB.Models.EmployeeAttendanceMasterT>().ToList();
                    oModel.EmployeeTypes = multi.Read<EmployeeTypeModel>().ToList();
                }
            }
            return oModel;

        }

        public StudentsMonthlyAttendancePageModel GetStudentsMonthlyAttendance(int ClassID, int SectionID, int SBranchID, int SessionID, DateTime ReportDate)
        {

            StudentsMonthlyAttendancePageModel objModel = new StudentsMonthlyAttendancePageModel();
            objModel.SectionID = SectionID;
            objModel.ClassID = ClassID;
            objModel.ReportDate = ReportDate;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                paramater.Add("@ReportDate", ReportDate);
                using (var multi = con.QueryMultiple("spn_GetStudentsAttendanceByClassSection", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<SectionModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.Attendances = multi.Read<SMEnterpriseDB.Models.StudentAttendanceMasterT>().ToList();

                    try
                    {
                        objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                    catch
                    { }
                }
            }
            return objModel;
        }


        public IEnumerable<ClassWiseFeeDueModel> GetClassWiseDueFeeDetails(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Year", CommonUsage.GetCurrentDate().Year);
                paramater.Add("@Month", CommonUsage.GetCurrentDate().Month);
                paramater.Add("@Day", CommonUsage.GetCurrentDate().Day);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<ClassWiseFeeDueModel>("spn_GetClassWiseDueFeeDetails", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }

        public void GetClassWiseDueFeeDetailsNew(ClassWiseFeeDuePageModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@QDate", objModel.QDate);
                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@SessionID", objModel.SessionID);
                using (var multi = con.QueryMultiple("spn_GetClassWiseDueFeeDetailsNew", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.ClassWiseDueFee = multi.Read<ClassWiseFeeDueModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
        }
        public FeePaymentModel GetDuefeeReportMonthlyNew(int SBranchID, DateTime QDate, int ClassID, int SectionID, int SessionID)
        {
            FeePaymentModel objModel = new FeePaymentModel();
            objModel.DemandMonth = QDate;

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                paramater.Add("@QDate", QDate);
                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());
                using (var multi = con.QueryMultiple("sp_GetStudentFeeDetailsNewTemp", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.StudentList = multi.Read<StudentModel>().ToList();
                    objModel.FeeTypeSummery = multi.Read<PayDetailFeeTypesModel>().ToList();
                    objModel.FeeListDetail = multi.Read<FeeDetailsModel>().ToList();

                }
            }
            return objModel;
        }

        public FeePaymentModel GetCollectionReport(int SBranchID, DateTime QDate, int ClassID, int SectionID, int SessionID)
        {
            FeePaymentModel objModel = new FeePaymentModel();
            objModel.DemandMonth = QDate;

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                paramater.Add("@QDate", QDate);
                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());
                using (var multi = con.QueryMultiple("[sp_GetSessionCollection]", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.StudentList = multi.Read<StudentModel>().ToList();
                    //objModel.FeeTypeSummery = multi.Read<PayDetailFeeTypesModel>().ToList();
                    //objModel.FeeListDetail = multi.Read<FeeDetailsModel>().ToList();
                    try
                    {
                        objModel.SBranchDetails = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                    catch
                    { }

                }
            }
            return objModel;
        }

        #region ClassWiseCollectionSummaryReport
        public ClassWiseCollectionSummaryPageModel GetClassWiseCollectionSummaryReport(int SBranchID, int SessionID, int ClassID, int SectionID)
        {
            ClassWiseCollectionSummaryPageModel model = new ClassWiseCollectionSummaryPageModel
            {
                SBranchID = SBranchID,
                SessionID = SessionID,
                ClassID = ClassID,
                SectionID = SectionID,
                Classes = new List<NameIDModel>(),
                Sections = new List<NameIDModel>(),
                Sessions = new List<SchoolSessionModel>(),
                Rows = new List<ClassWiseCollectionSummaryRowModel>()
            };

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@SBranchID", SBranchID);
                parameters.Add("@ClassID", ClassID);

                // QueryMultiple executing the stored procedure
                using (var multi = con.QueryMultiple("usp_GetClassWiseCollectionSummary", parameters, commandType: CommandType.StoredProcedure))
                {
                    // Maintaining exact sequence of result sets from the procedure
                    model.Classes = multi.Read<NameIDModel>().ToList();
                    model.Sections = multi.Read<NameIDModel>().ToList();
                    model.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    model.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                }

                // Logic to choose the target Session
                SchoolSessionModel selectedSession = model.Sessions.FirstOrDefault(x => x.SessionID == SessionID);
                if (selectedSession == null)
                {
                    selectedSession = model.Sessions.FirstOrDefault(x => x.SessionStatus == 1)
                                      ?? model.Sessions.OrderByDescending(x => x.SessionStartDate).FirstOrDefault();
                }

                if (selectedSession == null)
                {
                    return model;
                }

                model.SessionID = selectedSession.SessionID;
                model.SessionStartDate = selectedSession.SessionStartDate;
                model.SessionEndDate = selectedSession.SessionEndDate;

                // Calling sub-method with active connection
                model.Rows = GetClassWiseCollectionSummaryRows(con, SBranchID, selectedSession.SessionID, ClassID, SectionID, selectedSession.SessionStartDate, selectedSession.SessionEndDate);
            }

            return model;
        }
        private List<ClassWiseCollectionSummaryRowModel> GetClassWiseCollectionSummaryRows(SqlConnection con, int sBranchID,
                   int sessionID, int classID, int sectionID, DateTime sessionStartDate, DateTime sessionEndDate)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@SBranchID", sBranchID);
                parameters.Add("@SessionID", sessionID);
                parameters.Add("@ClassID", classID);
                parameters.Add("@SectionID", sectionID);

                // Cleaned up the unused parameters (null, true, 0, etc.) and used named arguments for clarity
                var summaryRows = con.Query<ClassWiseCollectionSummaryResultRowModel>(
                    "sp_GetClassWiseCollectionSummary",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                if (summaryRows.Any()) // Using .Any() is slightly cleaner than .Count > 0
                {
                    return MapClassWiseCollectionSummaryRows(summaryRows, sessionStartDate, sessionEndDate);
                }
            }
            catch (SqlException ex) when (ex.Message.IndexOf("Could not find stored procedure", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Fallback logic
            }

            // Fallback if procedure returned 0 rows or procedure doesn't exist
            return BuildClassWiseCollectionSummaryRowsFallback(con, sBranchID, sessionID, classID, sectionID, sessionStartDate, sessionEndDate);
        }
        private List<ClassWiseCollectionSummaryRowModel> MapClassWiseCollectionSummaryRows(List<ClassWiseCollectionSummaryResultRowModel> summaryRows, DateTime sessionStartDate, DateTime sessionEndDate)
        {
            DateTime monthCursorStart = new DateTime(sessionStartDate.Year, sessionStartDate.Month, 1);
            DateTime lastMonth = new DateTime(sessionEndDate.Year, sessionEndDate.Month, 1);

            return summaryRows
                .GroupBy(x => new { x.ClassID, x.ClassName, x.SectionID, x.SectionName })
                .Select(group => new ClassWiseCollectionSummaryRowModel
                {
                    ClassID = group.Key.ClassID,
                    ClassName = group.Key.ClassName,
                    SectionID = group.Key.SectionID,
                    SectionName = group.Key.SectionName,
                    Months = BuildMappedMonths(group.ToList(), monthCursorStart, lastMonth)
                })
                .OrderBy(x => x.ClassID)
                .ThenBy(x => x.SectionID)
                .ToList();
        }

        private List<ClassWiseCollectionSummaryMonthModel> BuildMappedMonths(List<ClassWiseCollectionSummaryResultRowModel> summaryRows, DateTime monthCursorStart, DateTime lastMonth)
        {
            List<ClassWiseCollectionSummaryMonthModel> months = new List<ClassWiseCollectionSummaryMonthModel>();
            DateTime monthCursor = monthCursorStart;

            while (monthCursor <= lastMonth)
            {
                ClassWiseCollectionSummaryResultRowModel summary = summaryRows
                    .FirstOrDefault(x => x.FeeMonth == monthCursor.Month && x.FeeYear == monthCursor.Year);

                months.Add(new ClassWiseCollectionSummaryMonthModel
                {
                    FeeMonth = monthCursor.Month,
                    FeeYear = monthCursor.Year,
                    Amount = summary == null ? 0 : summary.Amount,
                    Discount = summary == null ? 0 : summary.Discount,
                    Paid = summary == null ? 0 : summary.Paid,
                    Balance = summary == null ? 0 : summary.Balance
                });

                monthCursor = monthCursor.AddMonths(1);
            }

            return months;
        }
        private List<ClassWiseCollectionSummaryRowModel> BuildClassWiseCollectionSummaryRowsFallback(SqlConnection con, int sBranchID, int sessionID, int classID, int sectionID, DateTime sessionStartDate, DateTime sessionEndDate)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SBranchID", sBranchID);
            parameters.Add("@ClassID", classID);
            parameters.Add("@SectionID", sectionID);

            // Inline query ki jagah Stored Procedure call kiya
            List<ClassWiseCollectionSummaryRowModel> rows = con.Query<ClassWiseCollectionSummaryRowModel>(
                "usp_GetClassSectionsForSummary", parameters, commandType: CommandType.StoredProcedure).ToList();

            // Loop abhi bhi waise hi chalega jab tak aap is pure fallback ko ek single heavy procedure me nahi badal dete
            foreach (ClassWiseCollectionSummaryRowModel row in rows)
            {
                row.Months = BuildClassWiseCollectionSummaryMonthsExact(con, sBranchID, row.ClassID, row.SectionID, sessionID, sessionStartDate, sessionEndDate);
            }

            return rows;
        }

        private List<ClassWiseCollectionSummaryMonthModel> BuildClassWiseCollectionSummaryMonthsExact(SqlConnection con, int sBranchID, int classID, int sectionID, int sessionID, DateTime sessionStartDate, DateTime sessionEndDate)
        {
            List<ClassWiseCollectionSummaryMonthModel> months = new List<ClassWiseCollectionSummaryMonthModel>();
            DateTime monthCursor = new DateTime(sessionStartDate.Year, sessionStartDate.Month, 1);
            DateTime lastMonth = new DateTime(sessionEndDate.Year, sessionEndDate.Month, 1);

            while (monthCursor <= lastMonth)
            {
                FeePaymentModel collectionData = GetCollectionReportInternal(con, sBranchID, monthCursor, classID, sectionID, sessionID);
                List<StudentModel> students = collectionData != null && collectionData.StudentList != null
                    ? collectionData.StudentList
                    : new List<StudentModel>();

                months.Add(new ClassWiseCollectionSummaryMonthModel
                {
                    FeeMonth = monthCursor.Month,
                    FeeYear = monthCursor.Year,
                    Amount = students.Sum(x => x.FeeAmount),
                    Discount = students.Sum(x => x.Discounts),
                    Paid = students.Sum(x => x.PaidAmount),
                    Balance = students.Sum(x => x.UnpaidAmount)
                });

                monthCursor = monthCursor.AddMonths(1);
            }

            return months;
        }
        private FeePaymentModel GetCollectionReportInternal(SqlConnection con, int SBranchID, DateTime QDate, int ClassID, int SectionID, int SessionID)
        {
            FeePaymentModel objModel = new FeePaymentModel();
            objModel.DemandMonth = QDate;

            var paramater = new DynamicParameters();
            paramater.Add("@ClassID", ClassID);
            paramater.Add("@SectionID", SectionID);
            paramater.Add("@SBranchID", SBranchID);
            paramater.Add("@SessionID", SessionID);
            paramater.Add("@QDate", QDate);
            paramater.Add("@CurDate", CommonUsage.GetCurrentDate());
            using (var multi = con.QueryMultiple("[sp_GetSessionCollection]", paramater, null, 0, commandType: CommandType.StoredProcedure))
            {
                objModel.Classes = multi.Read<NameIDModel>().ToList();
                objModel.Sections = multi.Read<NameIDModel>().ToList();
                objModel.ClassID = multi.Read<int>().SingleOrDefault();
                objModel.SectionID = multi.Read<int>().SingleOrDefault();
                objModel.Sessions = multi.Read<NameIDModel>().ToList();
                objModel.SessionID = multi.Read<int>().SingleOrDefault();
                objModel.StudentList = multi.Read<StudentModel>().ToList();
                try
                {
                    objModel.SBranchDetails = multi.Read<SBranchModel>().SingleOrDefault();
                }
                catch
                { }
            }

            return objModel;
        }
        #endregion ClassWiseCollectionSummaryReport
        public DemandReciptListModel GetDemandReciptDataNew(int SBranchID, DateTime QDate, int ClassID, int SectionID, int SessionID)
        {

            DemandReciptListModel objModel = new DemandReciptListModel();

            objModel.DemandMonth = QDate;

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@QDate", QDate);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SessionID", SessionID);
                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());
                using (var multi = con.QueryMultiple("spn_GetDemandReciptsNew", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Recipts = multi.Read<DemandReciptModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public DemandReciptListModel GetDemandReciptData1(int SBranchID, int Month, int Year, int ClassID, int SectionID, int SessionID)
        {

            DemandReciptListModel objModel = new DemandReciptListModel();

            objModel.DemandMonth = new DateTime(Year, Month, 1);

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Year", Year);
                paramater.Add("@Month", Month);
                //paramater.Add("@Year", CommonUsage.GetCurrentDate().Year);
                //paramater.Add("@Month", CommonUsage.GetCurrentDate().Month);
                paramater.Add("@Day", CommonUsage.GetCurrentDate().Day);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("spn_GetDemandRecipts", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Recipts = multi.Read<DemandReciptModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public DemandReciptListModel GetDemandReciptData(int SBranchID, int ClassID, int SectionID, int SessionID)
        {

            DemandReciptListModel objModel = new DemandReciptListModel();

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@Year", CommonUsage.GetCurrentDate().Year);
                paramater.Add("@Month", CommonUsage.GetCurrentDate().Month);
                paramater.Add("@Day", CommonUsage.GetCurrentDate().Day);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("spn_GetDemandRecipts", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Recipts = multi.Read<DemandReciptModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }

        #region Exam Related

        public IEnumerable<EvaluationSchemeModel> GetEvaluationSchemeExam(int SBranchID, int SessionID)
        {
            EvaluationSchemePageModel objModel = new EvaluationSchemePageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                //objModel.SessionID = SessionID;
                return con.Query<EvaluationSchemeModel>("sp_GetEvaluationSchemes", paramater, null, true, 0, CommandType.StoredProcedure).ToList();

            }
        }
        public void GetAdmitCards(AdmitCardListModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                StartupModel objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@EvaluationID", objModel.EvaluationID);
                paramater.Add("@SessionID", objModel.SessionID);

                using (var multi = con.QueryMultiple("sp_GetStudentAdmitCard", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Evaluations = multi.Read<EvaluationModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Exams = multi.Read<ExamModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.EvaluationID = multi.Read<int>().SingleOrDefault();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.AdmitCards = multi.Read<AdmitCardModel>().ToList();
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                }
            }
        }
        public void GetExamDate(AdmitCardListModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                StartupModel objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@EvaluationID", objModel.EvaluationID);
                paramater.Add("@SessionID", objModel.SessionID);

                using (var multi = con.QueryMultiple("sp_GetExamDate", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Evaluations = multi.Read<EvaluationModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Exams = multi.Read<ExamModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.EvaluationID = multi.Read<int>().SingleOrDefault();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.ExamDates = multi.Read<ExamDateNotice>().SingleOrDefault();
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                }
            }
        }

        public void GetDeskSlip(AdmitCardListModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                StartupModel objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@EvaluationID", objModel.EvaluationID);
                paramater.Add("@SessionID", objModel.SessionID);

                using (var multi = con.QueryMultiple("sp_GetDeskSlip", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Evaluations = multi.Read<EvaluationModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.EvaluationID = multi.Read<int>().SingleOrDefault();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.AdmitCards = multi.Read<AdmitCardModel>().ToList();
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                }
            }
        }
        // Shishupal Work on Exam Date Sheet For School
        // Date : 17 Nov 2021
        public IEnumerable<NameIDModel> GetEvaluationTypesExam(int SBranchID, int EvaluationSchemeID)
        {
            EvaluationTypePageModelExam objModel = new EvaluationTypePageModelExam();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);

                paramater.Add("@EvaluationSchemeID", EvaluationSchemeID);
                return con.Query<NameIDModel>("sp_GetEvaluationTypesExam", paramater, null, true, 0, CommandType.StoredProcedure).ToList();

            }
        }
        public StudentExamDatesheetModel ExamDateSheet(StudentExamDatesheetModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@EvaluationSchemeID", objModel.EvaluationSchemeID);
                paramater.Add("@EvaluationID", objModel.EvaluationID);

                using (var multi = con.QueryMultiple("sp_DateSheetTest", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.EvaluationSchemes = multi.Read<NameIDModel>().ToList();
                    objModel.Evaluations = multi.Read<NameIDModel>().ToList();
                    objModel.ExamList = multi.Read<ExamDateListModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Exams = multi.Read<ExamModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.EvaluationSchemeID = multi.Read<int>().SingleOrDefault();
                    objModel.EvaluationID = multi.Read<int>().SingleOrDefault();
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                }
            }
            return objModel;
        }
        // End Shihupal  Exam Date Sheet

        #endregion
        public ClassGenderCategoryCountPageModel GetClassGenderCategoryCount(ClassGenderCategoryCountPageModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@SBranchID", objModel.SBranchID);

                using (var multi = con.QueryMultiple("sp_ClassStudentCategoryGenderCount", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Data = multi.Read<ClassGenderCategoryCountModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    try
                    {
                        objModel.Categories = multi.Read<NameIDModel>().ToList();
                    }
                    catch
                    { }
                }
            }
            return objModel;
        }
        public ClassGenderCategoryCountPageModel GetClassGenderCategoryHouseCount(ClassGenderCategoryCountPageModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_ClassStudentCategoryGenderHouseCount", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Data = multi.Read<ClassGenderCategoryCountModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.Houses = multi.Read<NameIDModel>().ToList();
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    try
                    {
                        objModel.Categories = multi.Read<NameIDModel>().ToList();
                    }
                    catch
                    { }
                }
            }
            return objModel;
        }

        public StudentsPageModel GetSiblingReport(int SBranchID, int SessionID)
        {
            StudentsPageModel objModel = new StudentsPageModel();

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("sp_GetSiblingStudentList", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.BranchDetails = multi.Read<SBranchModel>().SingleOrDefault();

                }
            }
            return objModel;
        }
        public StudentsPageModel GetAllStudentReportEWS(int SBranchID, int SessionID)
        {
            StudentsPageModel objModel = new StudentsPageModel();
            //objModel.SectionID = SectionID;
            //  objModel.ClassID = ClassID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                //paramater.Add("@ClassID", ClassID);
                // paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("spn_GetAllStudentDetailsEWS", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Students = multi.Read<StudentModel>().ToList();
                    // objModel.Classes = multi.Read<ClassModel>().ToList();
                    //   objModel.Sections = multi.Read<SectionModel>().ToList();
                    //   objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    //  objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public StudentsPageModel GetAllStudentReport(int SBranchID, int SessionID)
        {
            StudentsPageModel objModel = new StudentsPageModel();
            //objModel.SectionID = SectionID;
            //  objModel.ClassID = ClassID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                //paramater.Add("@ClassID", ClassID);
                // paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("spn_GetAllStudentDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Students = multi.Read<StudentModel>().ToList();
                    // objModel.Classes = multi.Read<ClassModel>().ToList();
                    //   objModel.Sections = multi.Read<SectionModel>().ToList();
                    //   objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    //  objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    try
                    {

                        objModel.BranchDetails = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                    catch (Exception ex)
                    { }
                }
            }
            return objModel;
        }

        public StudentsPageModel GetStudentClassReport(int ClassID, int SectionID, int SBranchID, int SessionID)
        {
            StudentsPageModel objModel = new StudentsPageModel();
            objModel.SectionID = SectionID;
            objModel.ClassID = ClassID;
            objModel.SessionID = SessionID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("spn_GetStudentClassSectionDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<SectionModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    try
                    {

                        objModel.BranchDetails = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                    catch (Exception ex)
                    { }
                }
            }
            return objModel;
        }
        public AbsentStudentReportModel GetAbsentStudentReport(AbsentStudentReportModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Day", objModel.Day);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("spn_GetAbsentStudentListForReview", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Students = multi.Read<StudentModel>().ToList();
                }
            }
            return objModel;
        }
        public StudentFeeModel GetMonthlyFeeCollection(StudentFeeModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_GetMonthFeeCollectionReport", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.FeePayments = multi.Read<FeePaymentModel>().ToList();
                    try
                    {

                        objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                    catch (Exception ex)
                    { }
                }
            }
            return objModel;
        }

        public CollectionReportModel GetCustomCollectionReport(CollectionReportModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@FromDate", objModel.FromDate);
                paramater.Add("@ToDate", objModel.ToDate);
                paramater.Add("@ReportType", objModel.ReportType);
                paramater.Add("@PaymentMode", objModel.PaymentMode);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@QuarterID", objModel.QuarterID);
                using (var multi = con.QueryMultiple("GetCustomCollectionReport", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.PaymentModes = multi.Read<PaymentModeModel>().ToList();
                    objModel.Report = multi.Read<FeePaymentModel>().ToList();
                }
            }
            return objModel;
        }
        public CollectionReportModel GetCollectionReport(CollectionReportModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@FromDate", objModel.FromDate);
                paramater.Add("@ToDate", objModel.ToDate);
                paramater.Add("@ReportType", objModel.ReportType);
                paramater.Add("@PaymentMode", objModel.PaymentMode);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@QuarterID", objModel.QuarterID);
                paramater.Add("@SessionID", objModel.SessionID); // Pass new parameter
                using (var multi = con.QueryMultiple("sp_GetFeeCollectionReport", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.PaymentModes = multi.Read<PaymentModeModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList(); // Read the new result set
                    objModel.Report = multi.Read<FeePaymentModel>().ToList();
                    try
                    {

                        objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                    catch (Exception ex)
                    { }
                }
            }
            return objModel;
        }
        public CollectionReportModel GetCollectionFeeReport(CollectionReportModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@FromDate", objModel.FromDate);
                paramater.Add("@ToDate", objModel.ToDate);
                paramater.Add("@ReportType", objModel.ReportType);
                paramater.Add("@PaymentMode", objModel.PaymentMode);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@QuarterID", objModel.QuarterID);
                using (var multi = con.QueryMultiple("GetStudentFeeRecordDaily", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.PaymentModes = multi.Read<PaymentModeModel>().ToList();
                    objModel.FeeType = multi.Read<NameIDModel>().ToList();
                    objModel.Report = multi.Read<FeePaymentModel>().ToList();
                    objModel.FeeReportType = multi.Read<PaymentDetailsModel>().ToList();
                    try
                    {

                        objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                    catch (Exception ex)
                    { }
                }
            }
            return objModel;
        }
        public CollectionReportModel GetDatewiseCollectionFeeReport(CollectionReportModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@FromDate", objModel.FromDate);
                paramater.Add("@ToDate", objModel.ToDate);
                paramater.Add("@ReportType", objModel.ReportType);
                paramater.Add("@PaymentMode", objModel.PaymentMode);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@QuarterID", objModel.QuarterID);
                using (var multi = con.QueryMultiple("GetStudentFeeRecordDateWiseMonthly", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.PaymentModes = multi.Read<PaymentModeModel>().ToList();
                    objModel.FeeType = multi.Read<NameIDModel>().ToList();
                    objModel.FeeReportType = multi.Read<PaymentDetailsModel>().ToList();
                    try
                    {

                        objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                    catch (Exception ex)
                    { }
                }
            }
            return objModel;
        }
        public ExpenceManagementModel GetMonthlyExpenceReport(ExpenceManagementModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Year", objModel.Year);
                using (var multi = con.QueryMultiple("sp_GetExpenceReport", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Expences = multi.Read<ExpenceModel>().ToList();
                    try
                    {

                        objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                    catch (Exception ex)
                    { }
                }
            }
            return objModel;
        }
        public IEnumerable<FeePaymentModel> GetStudentFeePayments(int StudentID, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SBranchID", SBranchID);

                return con.Query<FeePaymentModel>("sp_GetStudentFeePaymentList", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        public DailyAttandanceReportModel GetDailyAbsentReport(DailyAttandanceReportModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@Year", objModel.ReportDate.Year);
                paramater.Add("@Month", objModel.ReportDate.Month);
                paramater.Add("@Day", objModel.ReportDate.Day);
                using (var multi = con.QueryMultiple("spr_GetDailyAttandanceReportMini", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Report = multi.Read<DailyAttandanceModel>().ToList();
                    try
                    {

                        objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                    catch (Exception ex)
                    { }
                }

            }
            return objModel;
        }
        #endregion
        #region Exam Result Module
        public TeacherResultPageModel GetMiniExamResults(TeacherResultPageModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EvaluationID", objModel.EvaluationID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@EvaluationMode", 0);
                paramater.Add("@SessionID", objModel.SessionID);
                using (var multi = con.QueryMultiple("sp_GetClassGroupWiseExamResultsMini", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Subjects = multi.Read<NameIDModel>().ToList();
                    objModel.Evaluations = multi.Read<NameIDModel>().ToList();
                    objModel.ExamResults = multi.Read<ExamResultDetailModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    if (objModel.Classes.Count > 0)
                    {
                        objModel.ClassID = multi.Read<int>().SingleOrDefault();
                        objModel.SectionID = multi.Read<int>().SingleOrDefault();
                        objModel.SubjectID = multi.Read<int>().SingleOrDefault();
                        objModel.EvaluationID = multi.Read<int>().SingleOrDefault();
                    }
                    objModel.IsLocked = multi.Read<int>().SingleOrDefault();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    try
                    {
                        objModel.MarkingScheme = multi.Read<int>().SingleOrDefault();

                    }
                    catch
                    {

                    }
                }
            }
            return objModel;
        }
        public IEnumerable<NameIDModel> GetSubjectsForSection(int SectionID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", SectionID);
                return con.Query<NameIDModel>("spn_GetSubjectsForSections", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public IEnumerable<NameIDModel> GetSubjectsForSectionEvalluation(int SectionID, int EvaluationID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@EvaluationID", EvaluationID);
                return con.Query<NameIDModel>("spn_GetSubjectsForSectionEvaluations", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int UpdateStudentResults(TeacherResultPageModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                DataTable dtResult = objModel.GetResultDetailsDataTable();
                paramater.Add("@ExamResultDetails", dtResult);
                return con.Query<int>("sp_UpdateExamResults", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public StudentResultScrutinyListModel GetStudentsResultScrutiny(StudentResultScrutinyListModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@EvaluationID", objModel.EvaluationID);
                paramater.Add("@SessionID", objModel.SessionID);
                using (var multi = con.QueryMultiple("sp_GetStudentEvaluationResultScrutiny", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Evaluations = multi.Read<NameIDModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.StudentResult = multi.Read<StudentResultScrutinyModel>().ToList();

                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.EvaluationID = multi.Read<int>().SingleOrDefault();
                    try
                    {
                        objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();

                    }
                    catch
                    {

                    }
                }
            }
            return objModel;
        }

        public StudentResultScrutinyListModel StudentResultList(StudentResultScrutinyListModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@EvaluationID", objModel.EvaluationID);
                paramater.Add("@SessionID", objModel.SessionID);
                using (var multi = con.QueryMultiple("sp_GetStudentResultSummery", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Evaluations = multi.Read<NameIDModel>().ToList();
                    objModel.SubEvaluations = multi.Read<NameIDModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.StudentResult = multi.Read<StudentResultScrutinyModel>().ToList();
                    objModel.Subjects2 = multi.Read<SubjectModel>().ToList();
                    objModel.Exams = multi.Read<ExamModel>().ToList();
                    objModel.Result = multi.Read<ExamResultDetailModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.EvaluationID = multi.Read<int>().SingleOrDefault();
                    objModel.ClassName = multi.Read<string>().SingleOrDefault();
                    objModel.SectionName = multi.Read<string>().SingleOrDefault();
                    objModel.SessionName = multi.Read<string>().SingleOrDefault();
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                }
            }
            return objModel;
        }


        public StudentResultScrutinyListModel GetClassResultReport(StudentResultScrutinyListModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@EvaluationID", objModel.EvaluationID);
                paramater.Add("@SessionID", objModel.SessionID);
                using (var multi = con.QueryMultiple("sp_GetClassEvaluationResultReport", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Evaluations = multi.Read<NameIDModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Result = multi.Read<ExamResultDetailModel>().ToList();
                    objModel.Subjects = multi.Read<NameIDModel>().ToList();
                    objModel.StudentResult = multi.Read<StudentResultScrutinyModel>().ToList();

                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.EvaluationID = multi.Read<int>().SingleOrDefault();
                    try
                    {
                        objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();

                    }
                    catch
                    {

                    }
                }
            }
            return objModel;
        }

        public StudentResultScrutinyListModel GetStudentsResultCheck(StudentResultScrutinyListModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@EvaluationID", objModel.EvaluationID);
                paramater.Add("@SubEvaluationID", objModel.SubEvaluationID);
                paramater.Add("@SessionID", objModel.SessionID);
                using (var multi = con.QueryMultiple("sp_GetStudentEvaluationResultCheck", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Evaluations = multi.Read<NameIDModel>().ToList();
                    objModel.SubEvaluations = multi.Read<NameIDModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Subjects = multi.Read<NameIDModel>().ToList();
                    objModel.StudentResult2 = multi.Read<StudentResultCheckModel>().ToList();
                    objModel.Students = multi.Read<StudentModel>().ToList();

                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.EvaluationID = multi.Read<int>().SingleOrDefault();
                    objModel.SubEvaluationID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }

        public StudentPerformanceListModel GetStudentsPerformanceShine(StudentPerformanceListModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EvaluationID", objModel.EvaluationID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@EvaluationMode", 0);
                paramater.Add("@SessionID", objModel.SessionID);

                using (var multi = con.QueryMultiple("sp_GetClassSectionWiseStudentPerformancesMini", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Evaluations = multi.Read<NameIDModel>().ToList();
                    objModel.StudentPerformances = multi.Read<StudentPerformanceModel>().ToList();
                    if (objModel.Classes.Count > 0)
                    {
                        objModel.ClassID = multi.Read<int>().SingleOrDefault();
                        objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    }
                    objModel.EvaluationID = multi.Read<int>().SingleOrDefault();
                    objModel.TotalParameters = multi.Read<int>().SingleOrDefault();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.SubEvaluations = multi.Read<NameIDModel>().ToList();
                }
            }
            return objModel;
        }
        public StudentPerformanceListModel GetStudentsPerformanceMini(StudentPerformanceListModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EvaluationID", objModel.EvaluationID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@EvaluationMode", 0);
                paramater.Add("@SessionID", objModel.SessionID);
                using (var multi = con.QueryMultiple("sp_GetClassSectionWiseStudentPerformancesMini", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Evaluations = multi.Read<NameIDModel>().ToList();
                    objModel.StudentPerformances = multi.Read<StudentPerformanceModel>().ToList();
                    if (objModel.Classes.Count > 0)
                    {
                        objModel.ClassID = multi.Read<int>().SingleOrDefault();
                        objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    }
                    objModel.EvaluationID = multi.Read<int>().SingleOrDefault();
                    objModel.TotalParameters = multi.Read<int>().SingleOrDefault();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.SubEvaluations = multi.Read<NameIDModel>().ToList();
                }
            }
            return objModel;
        }

        public PerformanceParameterDetailModel GetStudentPerformanceDetails(PerformanceParameterDetailModel model)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentSessionUID", model.StudentSessionUID);
                paramater.Add("@EvaluationID", model.EvaluationID);
                paramater.Add("@SBranchID", model.SBranchID);
                paramater.Add("@EvaluationMode", 0);
                using (var multi = con.QueryMultiple("sp_GetStudentEvaluationPerformanceDetail", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    model.PerformanceParameters = multi.Read<PerformanceParameterModel>().ToList();
                    model.MainEvaluations = multi.Read<EvaluationModel>().ToList();
                    model.SubEvaluations = multi.Read<EvaluationModel>().ToList();
                    model.Result = multi.Read<ExamResultDetailModel>().ToList();
                    model.Student = multi.Read<StudentModel>().SingleOrDefault();
                    model.CGPA = multi.Read<decimal>().SingleOrDefault();
                    model.EvaluationName = multi.Read<string>().SingleOrDefault();
                    model.FilledParameter = multi.Read<int>().SingleOrDefault();
                    model.TotalParameter = multi.Read<int>().SingleOrDefault();
                }
            }
            return model;
        }


        // ============================================================
        //Rank
        // ============================================================
        /*
        public PerformanceParameterDetailModel GetStudentPerformanceDetails(PerformanceParameterDetailModel model)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                // ── existing SP call (unchanged) ─────────────────────
                var paramater = new DynamicParameters();
                paramater.Add("@StudentSessionUID", model.StudentSessionUID);
                paramater.Add("@EvaluationID", model.EvaluationID);
                paramater.Add("@SBranchID", model.SBranchID);
                paramater.Add("@EvaluationMode", 0);

                using (var multi = con.QueryMultiple(
                    "sp_GetStudentEvaluationPerformanceDetail",
                    paramater, null, 0,
                    commandType: CommandType.StoredProcedure))
                {
                    model.PerformanceParameters = multi.Read<PerformanceParameterModel>().ToList();
                    model.MainEvaluations = multi.Read<EvaluationModel>().ToList();
                    model.SubEvaluations = multi.Read<EvaluationModel>().ToList();
                    model.Result = multi.Read<ExamResultDetailModel>().ToList();
                    model.Student = multi.Read<StudentModel>().SingleOrDefault();
                    model.CGPA = multi.Read<decimal>().SingleOrDefault();
                    model.EvaluationName = multi.Read<string>().SingleOrDefault();
                    model.FilledParameter = multi.Read<int>().SingleOrDefault();
                    model.TotalParameter = multi.Read<int>().SingleOrDefault();
                }

                // ── new: rank SP call ────────────────────────────────
                // SessionID comes from the existing SP result via Student,
                // but Student.SessionID may be 0 — use model.SessionID directly
                var rankParams = new DynamicParameters();
                rankParams.Add("@StudentSessionUID", model.StudentSessionUID);
                rankParams.Add("@EvaluationID", model.EvaluationID == 0 ? -1 : model.EvaluationID);
                rankParams.Add("@SBranchID", model.SBranchID);
                rankParams.Add("@SessionID", model.SessionID);

                using (var rankMulti = con.QueryMultiple(
                    "sp_GetStudentRankInSection_test",
                    rankParams,
                    commandType: CommandType.StoredProcedure))
                {
                    model.TermRanks = rankMulti.Read<StudentTermRankModel>().ToList();
                    model.OverallRank = rankMulti.Read<StudentOverallRankModel>().SingleOrDefault();
                }
            }
            return model;
        }
      */
        public int UpdateStudentPerformance(PerformanceParameterDetailModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@PerformanceDetails", objModel.GetPerformanceValuesDatatable());
                return con.Query<int>("sp_UpdateStudentPerformanceDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        #endregion
        public void GetBranchParents(ParentPageModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@SText", oModel.SText);
                paramater.Add("@SessionID", oModel.SessionID);

                using (var multi = con.QueryMultiple("sp_GetParentList", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Parents = multi.Read<ParentModel>().ToList();
                    oModel.Sessions = multi.Read<NameIDModel>().ToList();
                    oModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
        }
        public void GetParentWiseFeeDetails(ParentFeeDetailsPageModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ParentID", objModel.ParentID);
                paramater.Add("@PaymentID", objModel.PaymentID);
                paramater.Add("@Day", objModel.Day);
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@QDate", CommonUsage.GetCurrentDate());
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("spn_GetParentChildsFeeDetailsNEW", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Students = multi.Read<StudentModel>().ToList();
                    objModel.FeeDetails = multi.Read<ParentStudentFeeDetails>().ToList();
                    objModel.Session = multi.Read<SchoolSessionModel>().SingleOrDefault();
                    objModel.Parent = multi.Read<ParentModel>().SingleOrDefault();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                }
            }
        }

        public void UpdateParentFeePayment(ParentFeeDetailsPageModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@ParentID", oModel.ParentID);
                paramater.Add("@ReferanceNumber", oModel.ReferanceNumber);
                paramater.Add("@SessionID", oModel.SessionID);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@Month", oModel.Month);
                paramater.Add("@Year", oModel.Year);
                paramater.Add("@QDate", oModel.QDate);
                paramater.Add("@Remark", oModel.Remark);
                paramater.Add("@PaymentMode", oModel.PaymentMode);
                paramater.Add("@CollectedBy", oModel.CollectedBy);
                paramater.Add("@PaymentAmount", oModel.PaymentAmount);
                paramater.Add("@ApplicableAmount", oModel.ApplicableAmount);
                paramater.Add("@DiscountAmount", oModel.DiscountAmount);
                paramater.Add("@PaymentDetail", oModel.GetFeePaymentsDataTable());

                using (var multi = con.QueryMultiple("sp_SaveParentPayemnt", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    oModel.PaymentRecieptNo = multi.Read<string>().SingleOrDefault();
                    oModel.PaymentID = multi.Read<int>().SingleOrDefault();
                }
            }
        }
        #region Stock Management
        public StockManagementModel GetStockTransfers(StockManagementModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@StartDate", objModel.StartDate);
                parameters.Add("@EndDate", objModel.EndDate);
                parameters.Add("@TrType", objModel.TrType);
                parameters.Add("@SBranchID", objModel.SBranchID);

                using (var multi = con.QueryMultiple("sp_GetStockTransactions", parameters, commandType: CommandType.StoredProcedure))
                {
                    // Read 1: Transactions
                    objModel.Transactions = multi.Read<StockTransaferMasterModel>().ToList();

                    // Read 2: Branch Info
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();

                    // Read 3: Latest Payments
                    var latestPayments = multi.Read<StockTransactionPaymentModel>().ToList();

                    // Mapping: Transactions mein Payment ID set karna
                    if (objModel.Transactions != null && latestPayments != null)
                    {
                        foreach (var transaction in objModel.Transactions)
                        {
                            var lp = latestPayments.FirstOrDefault(x => x.STID == transaction.STID);
                            if (lp != null)
                            {
                                transaction.LastPaymentID = lp.StockPaymentID;
                            }
                        }
                    }
                }
            }
            return objModel;
        }
        public StockTransaferMasterModel GetVendorsStockTransfers(int STID, int SBranchID)
        {
            StockTransaferMasterModel oModel = new StockTransaferMasterModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@STID", STID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetVendorsForStockTransaction", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.VendorID = multi.Read<int>().SingleOrDefault();
                    oModel.Vendors = multi.Read<VendorModel>().ToList();
                }
            }
            return oModel;
        }
        public StockTransaferMasterModel GetEmployeesStockTransfers(int STID, int SBranchID, int EmployeeTypeID)
        {
            StockTransaferMasterModel oModel = new StockTransaferMasterModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@STID", STID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@EmployeeTypeID", EmployeeTypeID);
                using (var multi = con.QueryMultiple("sp_GetEmployeesForStockTransaction", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.EmployeeTypes = multi.Read<NameIDModel>().ToList();
                    oModel.Employees = multi.Read<NameIDModel>().ToList();
                    oModel.RefID = multi.Read<int>().SingleOrDefault();
                    oModel.EmployeeTypeID = multi.Read<int>().SingleOrDefault();
                }
            }
            return oModel;
        }
        public StockTransaferMasterModel GetStudentsStockTransfers(int STID, int SBranchID)
        {
            StockTransaferMasterModel oModel = new StockTransaferMasterModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@STID", STID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetStudentsForStockTransaction", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Sessions = multi.Read<NameIDModel>().ToList();
                    oModel.Classes = multi.Read<NameIDModel>().ToList();
                    oModel.Sections = multi.Read<NameIDModel>().ToList();
                    oModel.Students = multi.Read<NameIDModel>().ToList();
                    oModel.SessionID = multi.Read<int>().SingleOrDefault();
                    oModel.ClassID = multi.Read<int>().SingleOrDefault();
                    oModel.SectionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return oModel;
        }
        public IEnumerable<NameIDModel> GetStudentListOnSessionSection(int SessionID, int SectionID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SessionID", SessionID);
                return con.Query<NameIDModel>("getStudentListOnSessionSection", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public StockTransaferMasterModel GetStockTransferDetails(int STID, int SBranchID)
        {
            StockTransaferMasterModel objModel = new StockTransaferMasterModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@STID", STID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetStockTransactionDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<StockTransaferMasterModel>().SingleOrDefault();
                    if (objModel == null)
                    {
                        objModel = new StockTransaferMasterModel();
                    }
                    objModel.Details = multi.Read<StockTransaferDetailModel>().ToList();
                    objModel.Products = multi.Read<ProductModel>().ToList();
                    if (objModel.TrType == 0)
                    {
                        objModel.VendorID = multi.Read<int>().SingleOrDefault();
                        objModel.Vendors = multi.Read<VendorModel>().ToList();
                        objModel.Sessions = new List<NameIDModel>();
                        objModel.Classes = new List<NameIDModel>();
                        objModel.Students = new List<NameIDModel>();
                        objModel.Sections = new List<NameIDModel>();
                        objModel.EmployeeTypes = new List<NameIDModel>();
                        objModel.Employees = new List<NameIDModel>();
                    }
                    if (objModel.TrType != 0)
                    {
                        if (objModel.RefType == 0)
                        {
                            objModel.Sessions = multi.Read<NameIDModel>().ToList();
                            objModel.Classes = multi.Read<NameIDModel>().ToList();
                            objModel.Sections = multi.Read<NameIDModel>().ToList();
                            objModel.Students = multi.Read<NameIDModel>().ToList();
                            objModel.SessionID = multi.Read<int>().SingleOrDefault();
                            objModel.ClassID = multi.Read<int>().SingleOrDefault();
                            objModel.SectionID = multi.Read<int>().SingleOrDefault();

                            objModel.EmployeeTypes = new List<NameIDModel>();
                            objModel.Employees = new List<NameIDModel>();
                            objModel.Vendors = new List<VendorModel>();
                        }
                        else
                        {
                            objModel.EmployeeTypes = multi.Read<NameIDModel>().ToList();
                            objModel.Employees = multi.Read<NameIDModel>().ToList();
                            objModel.RefID = multi.Read<int>().SingleOrDefault();
                            objModel.EmployeeTypeID = multi.Read<int>().SingleOrDefault();
                            objModel.Students = new List<NameIDModel>();
                            objModel.Sessions = new List<NameIDModel>();
                            objModel.Classes = new List<NameIDModel>();
                            objModel.Sections = new List<NameIDModel>();
                            objModel.Vendors = new List<VendorModel>();
                        }
                    }
                }
                objModel.PaymentHistory = GetStockPaymentHistory(STID, SBranchID, con);
                if (objModel.PaymentHistory == null)
                {
                    objModel.PaymentHistory = new List<StockTransactionPaymentModel>();
                }
                if (objModel.PaymentHistory.Count > 0)
                {
                    objModel.LastPaymentID = objModel.PaymentHistory.Max(x => x.StockPaymentID);
                }
            }
            return objModel;
        }

        public int UpdateStockTransaction(StockTransaferMasterModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@STID", oModel.STID);
                paramater.Add("@TrDate", oModel.TrDate);
                paramater.Add("@TrType", oModel.TrType);
                paramater.Add("@RefID", oModel.RefID);
                paramater.Add("@RefType", oModel.RefType);
                paramater.Add("@Remark", oModel.Remark);
                paramater.Add("@ClassID", oModel.ClassID);
                paramater.Add("@SectionID", oModel.SectionID);
                paramater.Add("@SessionID", oModel.SessionID);
                paramater.Add("@CreatedDate", oModel.CreatedDate);
                paramater.Add("@Status", oModel.Status);
                paramater.Add("@PaymentDate", oModel.PaymentDate);
                paramater.Add("@PaymentMode", oModel.PaymentMode);
                paramater.Add("@PaymentReferanceNo", oModel.PaymentReferanceNo);
                paramater.Add("@PaidAmount", oModel.PaidAmount);
                paramater.Add("@DueAmount", oModel.DueAmount);
                paramater.Add("@PaymentStatus", oModel.PaymentStatus);
                paramater.Add("@CancelRemark", oModel.CancelRemark);
                paramater.Add("@OpType", oModel.OpType);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@Details", oModel.GetDetailsDataTable());
                paramater.Add("@EmployeeTypeID", oModel.EmployeeTypeID);
                paramater.Add("@VendorID", oModel.VendorID);
                return con.Query<int>("sp_UpdateStockTransaction", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public int InsertStockTransactionPayment(int STID, DateTime paymentDate, int paymentMode, string paymentReferanceNo, decimal paymentAmount, decimal totalPaidAmount, decimal dueAmount, int paymentStatus, int sBranchID, int createdBy)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@STID", STID);
                parameters.Add("@PaymentDate", paymentDate);
                parameters.Add("@PaymentMode", paymentMode);
                parameters.Add("@PaymentReferanceNo", paymentReferanceNo);
                parameters.Add("@PaymentAmount", paymentAmount);
                parameters.Add("@TotalPaidAmount", totalPaidAmount);
                parameters.Add("@DueAmount", dueAmount);
                parameters.Add("@PaymentStatus", paymentStatus);
                parameters.Add("@SBranchID", sBranchID);
                parameters.Add("@CreatedBy", createdBy);

                // ExecuteScalar is used here because we are expecting a single value (the new ID) back
                return con.ExecuteScalar<int>("usp_InsertStockTransactionPayment", parameters, commandType: CommandType.StoredProcedure);
            }
        }
        public int CancelStockTransactionPayment(int stockPaymentID, int sBranchID, string cancelRemark, int cancelledBy)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@StockPaymentID", stockPaymentID);
                parameters.Add("@SBranchID", sBranchID);
                parameters.Add("@CancelRemark", cancelRemark);
                parameters.Add("@CancelledBy", cancelledBy);

                return con.ExecuteScalar<int>("usp_CancelStockTransactionPayment", parameters, commandType: CommandType.StoredProcedure);
            }
        }
        public List<StockTransactionPaymentModel> GetStockPaymentHistory(int STID, int SBranchID, SqlConnection con = null)
        {
            bool closeConnection = false;
            if (con == null)
            {
                con = new SqlConnection(CommonUsage.ConnectionString);
                con.Open();
                closeConnection = true;
            }
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@STID", STID);
                parameters.Add("@SBranchID", SBranchID);

                return con.Query<StockTransactionPaymentModel>("usp_GetStockPaymentHistory", parameters, commandType: CommandType.StoredProcedure).ToList();
            }
            finally
            {
                if (closeConnection)
                {
                    con.Dispose();
                }
            }
        }
        public List<NameIDModel> GetStockProductLookup(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {

                var parameters = new DynamicParameters();
                parameters.Add("@SBranchID", SBranchID);

                return con.Query<NameIDModel>("usp_GetStockProductLookup", parameters, commandType: CommandType.StoredProcedure).ToList();
            }
        }

        public StockSaleReportPageModel GetStockSaleReport(StockSaleReportPageModel objModel, bool dueOnly = false)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                objModel.Products = GetStockProductLookup(objModel.SBranchID);
                var parameters = new DynamicParameters();
                parameters.Add("@SBranchID", objModel.SBranchID);
                parameters.Add("@StartDate", objModel.StartDate);
                parameters.Add("@EndDate", objModel.EndDate);
                parameters.Add("@ProductID", objModel.ProductID);
                parameters.Add("@DueOnly", dueOnly ? 1 : 0);

                using (var multi = con.QueryMultiple("usp_GetStockSaleReport", parameters, commandType: CommandType.StoredProcedure))
                {

                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();

                    objModel.Report = multi.Read<StockSaleReportItemModel>().ToList();
                }
            }
            return objModel;
        }
        public PrintSaleReceiptModel GetSaleTransactionPrintData(int STID, int SBranchID, int stockPaymentID = 0)
        {
            PrintSaleReceiptModel objNew = new PrintSaleReceiptModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@STID", STID);
                parameters.Add("@SBranchID", SBranchID);

                using (var multi = con.QueryMultiple("usp_GetSaleTransactionPrintData", parameters, commandType: CommandType.StoredProcedure))
                {
                    // Sequential reading as per Procedure SELECT order
                    objNew.SBranchDetails = multi.Read<SBranchModel>().SingleOrDefault();
                    objNew.Transfer = multi.Read<StockTransaferMasterModel>().SingleOrDefault();
                    objNew.Products = multi.Read<StockTransaferDetailModel>().ToList();
                    objNew.Customer = multi.Read<SaleReceiptCustomerModel>().SingleOrDefault();
                    objNew.PaymentHistory = multi.Read<StockTransactionPaymentModel>().ToList() ?? new List<StockTransactionPaymentModel>();

                    // Logic to select the specific or latest payment
                    if (stockPaymentID > 0)
                    {
                        objNew.SelectedPayment = objNew.PaymentHistory.FirstOrDefault(x => x.StockPaymentID == stockPaymentID);
                    }

                    // Default: Agar specific ID nahi mila ya stockPaymentID == 0, toh latest payment pick karein
                    if (objNew.SelectedPayment == null)
                    {
                        objNew.SelectedPayment = objNew.PaymentHistory.OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.StockPaymentID).FirstOrDefault();
                    }
                }
            }

            return objNew;
        }
        #endregion
        #region SMS Management
        public int InsertUpdateSMSRequest(SMSRequestModel ObjData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SMSBalID", ObjData.SMSBalID);
                paramater.Add("@SMSCredited", ObjData.SMSCredited);
                paramater.Add("@RequestDate", ObjData.RequestDate);
                paramater.Add("@Status", ObjData.Status);
                paramater.Add("@SBranchID", ObjData.SBranchID);

                return con.Query<int>("spn_InsertSMSBalance", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public List<SMSRequestModel> GetSMSRequestHistory(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {

                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<SMSRequestModel>("spn_GetSMSRechargeHostory", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public SMSTemplatePageModel GetSMSTemplates()
        {
            SMSTemplatePageModel objModel = new SMSTemplatePageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                using (var multi = con.QueryMultiple("spn_GetSMSTemplates", null, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Templates = multi.Read<SMSTemplateModel>().ToList();
                    objModel.Types = multi.Read<NameIDModel>().ToList();
                }
            }
            return objModel;
        }
        public int InsertUpdateSMSTemplate(SMSTemplateModel ObjData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TemplateID", ObjData.TemplateID);
                paramater.Add("@Title", ObjData.Title);
                paramater.Add("@SMSType", ObjData.SMSType);
                paramater.Add("@Template", ObjData.Template);
                paramater.Add("@OpType", ObjData.OpType);

                return con.Query<int>("spn_InsertUpdateSMSTemplate", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public int InsertSMSSending(SMSSendTaskModel objModel)
        {
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SMSTemplateID", objModel.SMSTemplateID);
                paramater.Add("@SMSTypeID", objModel.SMSTypeID);
                paramater.Add("@SMSSendDate", objModel.SMSSendDate);
                paramater.Add("@TemplateText", objModel.TemplateText);
                paramater.Add("@RecieverCatIDs", objModel.RecieverCats);
                paramater.Add("@Title", objModel.Title);
                paramater.Add("@Recievers", objModel.GetRecieverDetailsDataTable());
                //paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@content_id", objModel.Content_id);
                return con.Query<int>("sp_InsertSMSSending", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public SMSCreateModel GetCreateSMSPageData(SMSCreateModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@SMSType", objModel.SelectedSMSType);
                paramater.Add("@Recievers", objModel.ReciverCats);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Day", objModel.Day);
                using (var multi = con.QueryMultiple("spn_GetCreateSMSPageData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.SMSTypes = multi.Read<NameIDModel>().ToList();
                    objModel.Templates = multi.Read<SMSTemplateModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Recievers = multi.Read<SMSRecieverDetailModel>().ToList();
                    objModel.SelectedSMSType = multi.Read<int>().SingleOrDefault();
                    objModel.ReciverCats = multi.Read<string>().SingleOrDefault();
                }
            }
            return objModel;
        }

        public SMSSendTaskModel GetSMSSendingDetails(int SMSSendingID)
        {
            SMSSendTaskModel objModel = new SMSSendTaskModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SMSSendingID", SMSSendingID);
                using (var multi = con.QueryMultiple("spn_GetSMSSendingDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<SMSSendTaskModel>().SingleOrDefault();
                    objModel.Recievers = multi.Read<SMSRecieverDetailModel>().ToList();
                }
            }
            return objModel;
        }
        public List<SMSRecieverDetailModel> GetSMSRecieverList(SMSCreateModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@SMSType", objModel.SelectedSMSType);
                paramater.Add("@Recievers", objModel.ReciverCats);
                paramater.Add("@Year", objModel.Year);
                paramater.Add("@Month", objModel.Month);
                paramater.Add("@Day", objModel.Day);

                return con.Query<SMSRecieverDetailModel>("spn_GetRecieverListForManualSMS", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int UpdateSMSProcessingStatus(SMSStatusModel data)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SMSType", data.SMSType);
                paramater.Add("@SMSID", data.SMSID);
                paramater.Add("@RecieverType", data.RecieverType);
                paramater.Add("@RecieverID", data.RecieverID);
                paramater.Add("@MobileNumber", data.MobileNumber);
                paramater.Add("@ReasonFailure", data.ReasonFailure);
                paramater.Add("@SMSDateTime", data.SMSDateTime);
                paramater.Add("@Status", data.Status);
                paramater.Add("@Content_id", data.SMSContentID);
                return con.Query<int>("spn_UpdateSMSProcessingLog", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public List<SMSSendTaskModel> GetSMSSendingHistory(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {

                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<SMSSendTaskModel>("spn_GetSMSSendingHistory", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        #endregion

        #region Other Certificate Related
        public SOCertificateDetails GetStudentSOCDetails(int StudentID, int SessionID)
        {
            SOCertificateDetails objModel = new SOCertificateDetails();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("sp_GetStudentSOCDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<SOCertificateDetails>().SingleOrDefault();
                    if (objModel == null)
                    {
                        objModel = new SOCertificateDetails();
                        objModel.StudentID = StudentID;
                        objModel.SessionID = SessionID;
                        objModel.CertDate = CommonUsage.GetCurrentDate();
                    }
                    objModel.Session = multi.Read<SessionModel>().SingleOrDefault();
                    objModel.Student = multi.Read<StudentModel>().SingleOrDefault();
                }
                return objModel;
            }
        }
        public int InsertUpdateOCertificates(SOCertificateDetails oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", oModel.ID);
                paramater.Add("@CertID", oModel.CertID);

                paramater.Add("@StudentID", oModel.StudentID);
                paramater.Add("@SessionID", oModel.SessionID);
                paramater.Add("@DueStatus", oModel.DueStatus);
                paramater.Add("@Character", oModel.Character);
                paramater.Add("@CreatedBy", oModel.CreatedBy);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@CertDate", oModel.CertDate);
                paramater.Add("@BirthPlace", oModel.BirthPlace);
                paramater.Add("@ModifiedBy", oModel.ModifiedBy);
                paramater.Add("@OpType", 0);

                return con.Query<int>("sp_UpdateStudentSOCDetails", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public SOCertificateDetails GetStudentSOCPrintDetails(int StudentID, int SessionID)
        {
            SOCertificateDetails objModel = new SOCertificateDetails();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("sp_GetStudentSOCDetailsForPrint", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<SOCertificateDetails>().SingleOrDefault();
                    if (objModel == null)
                    {
                        objModel = new SOCertificateDetails();
                        objModel.StudentID = StudentID;
                        objModel.SessionID = SessionID;
                        objModel.CertDate = CommonUsage.GetCurrentDate();
                    }
                    objModel.Session = multi.Read<SessionModel>().SingleOrDefault();
                    objModel.Student = multi.Read<StudentModel>().SingleOrDefault();
                    objModel.Parent = multi.Read<ParentModel>().SingleOrDefault();
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                }
                return objModel;
            }
        }
        #endregion

        #region TC Related
        public StudentsPageModel GetTCStudentList(int SBranchID, int SessionID)
        {

            StudentsPageModel objModel = new StudentsPageModel();

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("spn_GetTCStudentList", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Students = multi.Read<StudentModel>().ToList();


                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    try
                    {
                        objModel.BranchDetails = multi.Read<SBranchModel>().SingleOrDefault();

                    }
                    catch
                    {

                    }

                }
            }
            return objModel;
        }

        public int InsertUpdateTC(TCModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TCID", oModel.TCID);
                paramater.Add("@StudentID", oModel.StudentID);
                paramater.Add("@SessionID", oModel.SessionID);
                paramater.Add("@IsSCBLastExamPassed", oModel.IsSCBLastExamPassed);
                paramater.Add("@SCBLastExamName", oModel.SCBLastExamName);
                paramater.Add("@SCBLastExamResult", oModel.SCBLastExamResult);
                paramater.Add("@FailedCount", oModel.FailedCount);
                paramater.Add("@PromotedToClass", oModel.PromotedToClass);
                paramater.Add("@WorkingDays", oModel.WorkingDays);
                paramater.Add("@PresentDays", oModel.PresentDays);
                paramater.Add("@IsNCC", oModel.IsNCC);
                paramater.Add("@NCCDetails", oModel.NCCDetails);
                paramater.Add("@Games", oModel.Games);
                paramater.Add("@GeneralConduct", oModel.GeneralConduct);
                paramater.Add("@DateOfApplication", oModel.DateOfApplication);
                paramater.Add("@DateOfIssue", oModel.DateOfIssue);
                paramater.Add("@ReasonForLeaving", oModel.ReasonForLeaving);
                paramater.Add("@Remarks", oModel.Remarks);
                paramater.Add("@DuePaidMonth", oModel.DuePaidMonth);
                paramater.Add("@Concession", oModel.Concession);
                paramater.Add("@OpType", oModel.OpType);
                paramater.Add("@CreatedBy", oModel.CreatedBy);
                paramater.Add("@TCSLNo", oModel.TCSLNo);
                paramater.Add("@SBranchID", oModel.SBranchID);
                return con.Query<int>("sp_InsertUpdateTCDetails", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public SLCCertificateDetails GetStudentSLCDetails(int StudentID, int SessionID)
        {
            SLCCertificateDetails objModel = new SLCCertificateDetails();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SessionID", SessionID);

                using (var multi = con.QueryMultiple("sp_GetStudentSLCDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<SLCCertificateDetails>().SingleOrDefault();
                    if (objModel == null)
                    {
                        objModel = new SLCCertificateDetails();
                        objModel.StudentID = StudentID;
                        objModel.SessionID = SessionID;
                        objModel.TCDate = CommonUsage.GetCurrentDate();
                    }
                    objModel.Sessions = multi.Read<SessionModel>().ToList();
                    objModel.Student = multi.Read<StudentModel>().SingleOrDefault();
                }
                return objModel;
            }
        }
        public SLCCertificateDetails GetStudentSLCPrintDetails(int StudentID, int SessionID)
        {

            SLCCertificateDetails objModel = new SLCCertificateDetails();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SessionID", SessionID);

                using (var multi = con.QueryMultiple("sp_GetStudentSLCPrintDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<SLCCertificateDetails>().SingleOrDefault();
                    if (objModel == null)
                    {
                        objModel = new SLCCertificateDetails();
                        objModel.StudentID = StudentID;
                        objModel.SessionID = SessionID;
                        objModel.TCDate = CommonUsage.GetCurrentDate();
                    }
                    objModel.Sessions = multi.Read<SessionModel>().ToList();
                    objModel.Student = multi.Read<StudentModel>().SingleOrDefault();
                    objModel.Parent = multi.Read<ParentModel>().SingleOrDefault();
                    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                }
                return objModel;
            }
        }

        public int InsertUpdateSLC(SLCCertificateDetails oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@CertID", oModel.CertID);
                paramater.Add("@StudentID", oModel.StudentID);
                paramater.Add("@SessionID", oModel.SessionID);
                paramater.Add("@Stream", oModel.Stream);
                paramater.Add("@DuesCleared", oModel.DuesCleared);
                paramater.Add("@Character", oModel.Character);
                paramater.Add("@Promotion", oModel.Promotion);
                paramater.Add("@TCDate", oModel.TCDate);
                paramater.Add("@CreatedDate", oModel.CreatedDate);
                paramater.Add("@CreatedBy", oModel.CreatedBy);
                paramater.Add("@TCSLNo", oModel.TCSLNo);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@SessionSLCDetails", oModel.GetStudentSLCSessionDetails());
                paramater.Add("@DOB", oModel.DOB);
                paramater.Add("@DOJ", oModel.DOJ);
                paramater.Add("@SchoolUID", oModel.SchoolUID);
                paramater.Add("@PSchoolName", oModel.PSchoolName);
                paramater.Add("@PSchoolMedium", oModel.PSchoolMedium);
                paramater.Add("@PClassName", oModel.PClassName);
                paramater.Add("@PSResult", oModel.PSResult);
                paramater.Add("@PSchoolCity", oModel.PSchoolCity);
                paramater.Add("@PSChoolState", oModel.PSChoolState);
                paramater.Add("@OpType", oModel.OpType);

                return con.Query<int>("sp_UpdateStudentSLCDetails", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public void GetTCList(TCListModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@SessionID", oModel.SessionID);
                using (var multi = con.QueryMultiple("sp_GetIssuedTCs", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Sessions = multi.Read<NameIDModel>().ToList();
                    oModel.TCList = multi.Read<TCModel>().ToList();
                    oModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
        }
        public void GetTCDetails(TCDetailsModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@SessionID", oModel.SessionID);
                paramater.Add("@StudentID", oModel.StudentID);
                using (var multi = con.QueryMultiple("sp_GetTCDetailsAdmin", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.StudentDetails = multi.Read<StudentModel>().SingleOrDefault();
                    oModel.Classes = multi.Read<ClassModel>().ToList();
                    oModel.TCDetails = multi.Read<TCModel>().SingleOrDefault();
                    if (oModel.TCDetails == null)
                    {
                        oModel.TCDetails = new TCModel();
                        oModel.TCDetails.DateOfApplication = CommonUsage.GetCurrentDate();
                        oModel.TCDetails.DateOfIssue = CommonUsage.GetCurrentDate();
                    }
                    oModel.Subjects = multi.Read<string>().ToList();
                    oModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    oModel.Sessions = multi.Read<SessionModel>().ToList();


                }
            }
        }
        #endregion
        #region Fee Refund
        public void GetFeeRefunds(FeeRefundPageModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@FromDate", oModel.FromDate);
                paramater.Add("@ToDate", oModel.ToDate);
                paramater.Add("@Status", oModel.Status);
                paramater.Add("@SBranchID", oModel.SBranchID);
                oModel.Refunds = con.Query<FeeRefundModel>("sp_GetRefunds", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int UpdateFeeRefund(FeeRefundModel objModel)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@RefundID", objModel.RefundID);
                paramater.Add("@RefundTo", objModel.RefundTo);
                paramater.Add("@RefundDate", objModel.RefundDate);
                paramater.Add("@CreatedDate", objModel.CreatedDate);
                paramater.Add("@RefundBy", objModel.RefundBy);
                paramater.Add("@Reason", objModel.Reason);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@RefundAmount", objModel.RefundAmount);
                paramater.Add("@Status", objModel.Status);
                return con.Query<int>("sp_InsertUpdateRefund", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public FeeRefundEditModel GetFeeRefundDetails(int RefundID, int SBranchID)
        {
            FeeRefundEditModel oModel = new FeeRefundEditModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@RefundID", RefundID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetRefundDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    oModel.Sessions = multi.Read<NameIDModel>().ToList();
                    oModel.Classes = multi.Read<NameIDModel>().ToList();
                    oModel.Sections = multi.Read<NameIDModel>().ToList();
                    oModel.Students = multi.Read<NameIDModel>().ToList();
                    oModel.RefundDetails = multi.Read<FeeRefundModel>().SingleOrDefault();

                    if (oModel.RefundDetails == null)
                    {
                        oModel.RefundDetails = new FeeRefundModel();
                        oModel.RefundDetails.SessionID = multi.Read<int>().SingleOrDefault();
                        oModel.RefundDetails.ClassID = multi.Read<int>().SingleOrDefault();
                        oModel.RefundDetails.SectionID = multi.Read<int>().SingleOrDefault();
                        oModel.RefundDetails.RefundTo = multi.Read<int>().SingleOrDefault();
                        oModel.RefundDetails.StudentSessionUID = multi.Read<int>().SingleOrDefault();
                        oModel.RefundDetails.RefundDate = CommonUsage.GetCurrentDate();
                    }

                }
            }
            return oModel;
        }
        public FeeRefundPrintModel GetFeeRefundPrintData(int RefundID, int SBranchID)
        {
            FeeRefundPrintModel objNew = new FeeRefundPrintModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@RefundID", RefundID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetRefundPrintData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objNew.RefundDetails = multi.Read<FeeRefundModel>().SingleOrDefault();
                    objNew.SBranchDetails = multi.Read<SBranchModel>().SingleOrDefault();
                }
            }

            return objNew;
        }
        #endregion
        #region Extra Income Related
        public void GetExtraIncomes(ExtraIncomePageModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@PaymentMode", oModel.PaymentMode);
                paramater.Add("@StartDate", oModel.StartDate);
                paramater.Add("@EndDate", oModel.EndDate);
                using (var multi = con.QueryMultiple("sp_GetExtraIncomePageData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Heads = multi.Read<ExtraIncomeHeadModel>().ToList();
                    oModel.Incomes = multi.Read<ExtraIncomeModel>().ToList();
                }
            }
        }
        public ExtraIncomePageModel GetExtraIncomeDetails(int ID, int SBranchID)
        {
            ExtraIncomePageModel oModel = new ExtraIncomePageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", ID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetExtraIncomeDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Heads = multi.Read<ExtraIncomeHeadModel>().ToList();
                    oModel.IncomeDetail = multi.Read<ExtraIncomeModel>().SingleOrDefault();
                }
            }
            return oModel;
        }
        public int UpdateExtraIncome(ExtraIncomeModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", objModel.ID);
                paramater.Add("@Title", objModel.Title);
                paramater.Add("@PaidBy", objModel.PaidBy);
                paramater.Add("@EIHeadID", objModel.EIHeadID);
                paramater.Add("@Amount", objModel.Amount);
                paramater.Add("@Description", objModel.Description);
                paramater.Add("@Status", objModel.Status);
                paramater.Add("@PaidDate", objModel.PaidDate);
                paramater.Add("@CreatedDate", objModel.CreatedDate);
                paramater.Add("@RecievedBy", objModel.RecievedBy);
                paramater.Add("@PaymentMode", objModel.PaymentMode);
                paramater.Add("@ReferanceNo", objModel.ReferanceNo);
                paramater.Add("@PayerAddress", objModel.PayerAddress);
                paramater.Add("@PayerCity", objModel.PayerCity);
                paramater.Add("@PayerState", objModel.PayerState);
                paramater.Add("@PayerPin", objModel.PayerPin);
                paramater.Add("@PayerContact", objModel.PayerContact);
                paramater.Add("@PayerPAN", objModel.PayerPAN);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@UserID", objModel.UserID);
                paramater.Add("@OpType", objModel.OpType);
                return con.Query<int>("sp_UpdateExtraIncome", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #endregion

        #region Expense Book Related
        public CollectionReportModel GetExpenseBookMonthlyReport(CollectionReportModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                ExpenseDetailsModel objexpense;
                var paramater = new DynamicParameters();
                paramater.Add("@FromDate", objModel.FromDate);
                paramater.Add("@ToDate", objModel.ToDate);
                //paramater.Add("@PaymentMode", objModel.PaymentMode);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_GetDayBookReport", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.PaymentModes = multi.Read<PaymentModeModel>().ToList();
                    objModel.FeeType = multi.Read<NameIDModel>().ToList();
                    objModel.ExpenceType = multi.Read<NameIDModel>().ToList();
                    objModel.FeeReportType = multi.Read<PaymentDetailsModel>().ToList();
                    objModel.ExpenseReportType = multi.Read<ExpenseDetailsModel>().ToList();

                    //try
                    //{

                    //    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    //}
                    //catch (Exception ex)
                    //{ }
                }
            }
            return objModel;
        }
        #endregion
        #region Day Book Related


        public CollectionReportModel GetDayBookMonthlyReport(CollectionReportModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                ExpenseDetailsModel objexpense;
                var paramater = new DynamicParameters();
                paramater.Add("@FromDate", objModel.FromDate);
                paramater.Add("@ToDate", objModel.ToDate);
                //paramater.Add("@PaymentMode", objModel.PaymentMode);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_GetDayBookReport", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.PaymentModes = multi.Read<PaymentModeModel>().ToList();
                    objModel.FeeType = multi.Read<NameIDModel>().ToList();
                    objModel.ExpenceType = multi.Read<NameIDModel>().ToList();
                    objModel.FeeReportType = multi.Read<PaymentDetailsModel>().ToList();
                    objModel.ExpenseReportType = multi.Read<ExpenseDetailsModel>().ToList();

                    //try
                    //{

                    //    objModel.Branch = multi.Read<SBranchModel>().SingleOrDefault();
                    //}
                    //catch (Exception ex)
                    //{ }
                }
            }
            return objModel;
        }
        public void GetDayBook(DayBookPageModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@PaymentMode", oModel.PaymentMode);
                paramater.Add("@StartDate", oModel.StartDate);
                paramater.Add("@EndDate", oModel.EndDate);
                using (var multi = con.QueryMultiple("sp_GetDayBookPageData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.DayBookHead = multi.Read<DayBookHeadModel>().ToList();
                    oModel.DayBooks = multi.Read<DayBookModel>().ToList();
                }
            }
        }
        public DayBookPageModel GetDayBookDetails(int DBookID, int SBranchID)
        {
            DayBookPageModel oModel = new DayBookPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@DBookID", DBookID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetDayBookDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.DayBookHead = multi.Read<DayBookHeadModel>().ToList();
                    oModel.DayBook = multi.Read<DayBookModel>().SingleOrDefault();
                }
            }
            return oModel;
        }
        public int UpdateDayBook(DayBookModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", objModel.DBookID);
                paramater.Add("@Title", objModel.Title);
                paramater.Add("@IncomeExpenceBy", objModel.IncomeExpenceBy);
                paramater.Add("@DayBookID", objModel.DayBookID);
                paramater.Add("@Amount", objModel.Amount);
                paramater.Add("@Description", objModel.Description);
                paramater.Add("@Status", objModel.Status);
                paramater.Add("@Type", objModel.Type);
                paramater.Add("@Date", objModel.Date);
                paramater.Add("@CreatedDate", objModel.CreatedDate);
                paramater.Add("@RecievedBy", objModel.RecievedBy);
                paramater.Add("@PaymentMode", objModel.PaymentMode);
                paramater.Add("@ReferanceNo", objModel.ReferanceNo);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@UserID", objModel.UserID);
                paramater.Add("@OpType", objModel.OpType);
                return con.Query<int>("sp_UpdateDayBook", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #endregion

        #region change Password


        public int UpdatePassword(int UserID, int UserType, string Password)

        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@UserID", UserID);
                paramater.Add("@UserType", UserType);
                paramater.Add("@Password", Password);

                return con.Query<int>("sp_ChangePassword", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #endregion

        #region Student Block
        public int UpdateIsBlock(int isBlock, int studentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@isBlock", isBlock);
                paramater.Add("@studentID", studentID);
                return con.Query<int>("SP_UpdateIsBlock", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }

        }
        #endregion


        #region TransportFee


        public async Task<FeePaymentModel> GetFeeDetailsNewForTransport(FeePaymentModel objModel)
        {
            FeePaymentModel objNew = new FeePaymentModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                DateTime qdate = new DateTime(objModel.Year, objModel.Month, 1).AddMonths(1).AddDays(-1);
                if (qdate.Day < objModel.Day)
                {
                    objModel.Day = qdate.Day;
                }
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", objModel.StudentID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@QDate", new DateTime(objModel.Year, objModel.Month, objModel.Day));
                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());

                using (var multi = await con.QueryMultipleAsync("sp_GetStudentFeeViewDataForTransport", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.FeeTypeSummery = multi.Read<PayDetailFeeTypesModel>().ToList();
                    objModel.Months = multi.Read<PayDetailMonthsModel>().ToList();
                    objModel.PaymentDetails = multi.Read<FeeDetailsModel>().ToList();
                    objModel.SessionStartDate = multi.Read<DateTime>().SingleOrDefault();
                    objModel.SessionEndDate = multi.Read<DateTime>().SingleOrDefault();
                    objModel.FeePaymentMode = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }


        public FeePaymentRowModel SaveStudentTransportFeePayments(FeePaymentModel objData)
        {
            FeePaymentRowModel obj = new FeePaymentRowModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@StudentID", objData.StudentID);
                paramater.Add("@QDate", new DateTime(objData.Year, objData.Month, 1));
                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());
                paramater.Add("@PaymentDate", objData.PaymentDate);
                paramater.Add("@PaymentAmount", objData.PaymentAmount);
                paramater.Add("@WaiverMonths", objData.WaiverMonths);
                paramater.Add("@Remark", objData.Remark);
                paramater.Add("@ReferanceNumber", objData.ReferanceNumber);
                paramater.Add("@PaymentMode", objData.PaymentMode);
                paramater.Add("@CollectedBy", objData.CollectedBy);
                paramater.Add("@SessionID", objData.SessionID);
                paramater.Add("@ExcludedFees", objData.ExcludedFees);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@SBranchID", objData.SBranchID);

                using (var multi = con.QueryMultiple("spn_SaveTransportFeePaymentV2", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    obj.StudentDetails = multi.Read<FeePaymentModel>().SingleOrDefault();
                    obj.PaymentID = multi.Read<int>().SingleOrDefault();
                }

                return obj;
            }
        }
        #endregion

        public async Task<AdmissionEnquiryMasterModel> GetBranchDetails(int BranchID)
        {
            AdmissionEnquiryMasterModel objNew = new AdmissionEnquiryMasterModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@BranchID", BranchID);
                using (var multi = await con.QueryMultipleAsync("sp_GetBranchDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objNew.BranchID = multi.Read<int>().SingleOrDefault();
                    objNew.BranchData = multi.Read<SBranchModel>().SingleOrDefault();
                    objNew.Religions = multi.Read<NameIDModel>().ToList();
                    objNew.Casts = multi.Read<NameIDModel>().ToList();
                    objNew.Classes = multi.Read<ClassModel>().ToList();
                    objNew.SessionNames = multi.Read<NameIDModel>().ToList();
                }
            }

            return objNew;
        }
        public AdmissionEnquiryMasterModel UpdateAdmissionEnquiry(AdmissionEnquiryMasterModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EnquiryID", objData.EnquiryID);
                paramater.Add("@EDate", CommonUsage.GetCurrentDate());
                paramater.Add("@FatherName", objData.FatherName);
                paramater.Add("@MotherName", objData.MotherName);
                paramater.Add("@FatherMobileNo", objData.FatherMobileNo);
                paramater.Add("@MotherMobileNo", objData.MotherMobileNo);
                paramater.Add("@FatherEmailID", objData.FatherEmailID);
                paramater.Add("@MotherEmailID", objData.MotherEmailID);
                paramater.Add("@FOccupation", objData.FOccupation);
                paramater.Add("@MOccupation", objData.MOccupation);
                paramater.Add("@FamilyIncome", objData.FamilyIncome);
                paramater.Add("@Address", objData.Address);
                paramater.Add("@StudentName", objData.StudentName);
                paramater.Add("@AppliedForSession", objData.AppliedForSession);
                paramater.Add("@AppliedForClass", objData.AppliedForClass);
                paramater.Add("@CurrentClass", objData.CurrentClass);
                paramater.Add("@ESource", objData.ESource);
                paramater.Add("@CurrentSchool", objData.CurrentSchool);
                paramater.Add("@CurrentSchoolAddress", objData.CurrentSchoolAddress);
                paramater.Add("@ReasonForChange", objData.ReasonForChange);
                paramater.Add("@Gender", objData.Gender);
                paramater.Add("@Nationality", objData.Nationality);
                paramater.Add("@CurrentEducationSystem", objData.CurrentEducationSystem);
                paramater.Add("@StudentDOB", objData.DateOfBirth);
                paramater.Add("@EStatus", objData.EStatus);
                paramater.Add("@EPossibility", objData.EPossibility);
                paramater.Add("@NextFollowUpDate", CommonUsage.GetCurrentDate());
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@CreatedDate", CommonUsage.GetCurrentDate());
                paramater.Add("@StudentID", objData.StudentID);
                paramater.Add("@PaymentStatus", objData.PaymentStatus);
                paramater.Add("@PaymentDetails", objData.PaymentDetails);
                paramater.Add("@AssignedTo", objData.AssignedTo);
                paramater.Add("@Image", objData.Image);
                paramater.Add("@StudentAadharNo", objData.StudentAadharNo);
                paramater.Add("@FatherAadhaarNo", objData.FatherAadhaarNo);
                paramater.Add("@MotherAadhaarNo", objData.MotherAadhaarNo);
                paramater.Add("@Caste", objData.Caste);
                paramater.Add("@DateOfBirth", objData.DateOfBirth);
                paramater.Add("@TelephoneNoOff", objData.TelephoneNoOff);
                paramater.Add("@TelephoneNoReg", objData.TelephoneNoReg);
                paramater.Add("@Sibling", objData.Sibling);
                paramater.Add("@SiblingName", objData.SiblingName);
                paramater.Add("@SiblingClass", objData.SiblingClass);
                paramater.Add("@MotherQuaAndOcc", objData.MotherQuaAndOcc);
                paramater.Add("@FatherQuaAndOcc", objData.FatherQuaAndOcc);
                paramater.Add("@TemporaryAddress", objData.TemporaryAddress);
                paramater.Add("@PermanentAddress", objData.PermanentAddress);
                paramater.Add("@Religion", objData.ReligionID);

                return con.Query<AdmissionEnquiryMasterModel>("spn_InsertUpdateAdmissionEnquiry", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public AdmissionEnquiryMasterModel GetEnquiryDetails(int EnquiryID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EnquiryID", EnquiryID);

                return con.Query<AdmissionEnquiryMasterModel>("sp_GetEnquiryDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public StudentAdmissionReportModel GetSessionAdmissionReport(StudentAdmissionReportModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SessionID", oModel.SessionID);
                paramater.Add("@SBranchID", oModel.SBranchID);
                //paramater.Add("@StartDate", oModel.StartDate);
                using (var multi = con.QueryMultiple("sp_GetSessionAdmissionsDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.StudentDetail = multi.Read<StudentAdmissionDetail>().ToList();
                    oModel.Sessions = multi.Read<NameIDModel>().ToList();
                    oModel.Branches = multi.Read<SBranchModel>().SingleOrDefault();
                }
            }
            return oModel;
        }
        #region Bulk Student Data Upload
        public BulkUploadInfoModel GetBulkUploadInfo(BulkUploadInfoModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", oModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_GetBulkUploadInfo", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Religion = multi.Read<NameIDModel>().ToList();
                    oModel.Categroy = multi.Read<NameIDModel>().ToList();
                    oModel.Quota = multi.Read<NameIDModel>().ToList();
                }
            }
            return oModel;
        }
        public BulkStudentUploadModel GetBulkUploadData(int ClassID, int SectionID, int SBranchID, int SessionID)
        {

            BulkStudentUploadModel objModel = new BulkStudentUploadModel();
            objModel.SectionID = SectionID;
            objModel.ClassID = ClassID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("spn_GetBulkUploadPageData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<SectionModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }

        public int UpdateBulkStudentsEnt(BulkStudentUploadModel objModel)
        {
            //Added for bulk upload
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@Students", objModel.GetStudentsDataTableEnt());
                return con.Query<int>("sp_BulkUploadStudentsEnt", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #endregion

        #region onlinePayment
        public BranchPaymentGatewayModel GetBranchGateway(int sBranchId)
        {
            using (var con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@SBranchID", sBranchId);

                // Saari dynamic logic database procedure ke andar secure ho chuki hai
                return con.Query<BranchPaymentGatewayModel>("sp_GetBranchGateway", parameters, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public StudentModel GetStudentDetailsForPayment(int StudentID, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SBranchID", SBranchID);

                return con.Query<StudentModel>("sp_GetStudentDetailForOnlinePayment", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public OrderModel GetOrderForPayment(string OrderID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OrderId", OrderID);


                return con.Query<OrderModel>("GetPaymentOrder", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int InsertOrderID(OrderModel obj)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OrderID", obj.OrderID);
                paramater.Add("@StudentID", obj.StudentID);
                paramater.Add("@Name", obj.Name);
                paramater.Add("@EmailID", obj.EmailID);
                paramater.Add("@ContactNumber", obj.ContactNumber);
                paramater.Add("@FeeMonth", obj.FeeMonth);
                paramater.Add("@FeeYear", obj.FeeYear);
                paramater.Add("@Amount", obj.ApplicableFee);
                paramater.Add("@SBranchID", obj.SBranchID);
                paramater.Add("@Date", obj.Date);
                paramater.Add("@PGOrderID", obj.PGOrderID);
                paramater.Add("@SessionID", obj.SessionID);
                paramater.Add("@Status", 0);
                // ── NEW parameters ────────────────────────────────────
                paramater.Add("@SelectedMonthsJson", obj.SelectedMonthsJson);  // null for single month
                paramater.Add("@IsMultiMonth", obj.IsMultiMonth);        // false for single month



                return con.Query<int>("sp_InsertOrderID", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int UpdateOrderStatus(string OrderID, string StudentID, string SessionID, string ReferanceNumber, int Status)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OrderID", OrderID);
                paramater.Add("@StudentID", StudentID);
                paramater.Add("@SessionID", SessionID);
                paramater.Add("@PGPaymentID", ReferanceNumber);
                paramater.Add("@Status", Status);
                return con.Query<int>("sp_UpdateOrderStatus", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int UpdateStudentFeePaymentStatus(OrderModel obj)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@PGOrderID", obj.PGOrderID);
                paramater.Add("@PGPaymentID", obj.PGPaymentID);
                paramater.Add("@StudentID", obj.StudentID);
                paramater.Add("@QDate", obj.QDate);
                paramater.Add("@CurDate", obj.CurDate);
                paramater.Add("@PaymentDate", obj.PaymentDate);
                paramater.Add("@PaymentAmount", obj.PaymentAmount);
                paramater.Add("@Remark", obj.Remark);
                paramater.Add("@ReferanceNumber", obj.ReferanceNumber);
                paramater.Add("@PaymentMode", obj.PaymentMode);
                paramater.Add("@CollectedBy", obj.CollectedBy);
                paramater.Add("@SBranchID", obj.SBranchID);
                paramater.Add("@Status", obj.Status);
                paramater.Add("@FeeMonth", obj.FeeMonth);
                paramater.Add("@FeeYear", obj.FeeYear);
                //paramater.Add("@ApplicableFee", obj.ApplicableFee);
                return con.Query<int>("sp_UpdateStudentFeePaymentStatus", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public FeePaymentRowModel SaveStudentFeePaymentOnline(FeePaymentModel objData)
        {
            FeePaymentRowModel obj = new FeePaymentRowModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@StudentID", objData.StudentID);
                paramater.Add("@QDate", new DateTime(objData.Year, objData.Month, 1));
                paramater.Add("@CurDate", CommonUsage.GetCurrentDate());

                paramater.Add("@PaymentDate", CommonUsage.GetCurrentDate());
                paramater.Add("@PaymentAmount", objData.PaymentAmount);
                paramater.Add("@WaiverMonths", objData.WaiverMonths);
                paramater.Add("@Remark", objData.Remark);
                paramater.Add("@ReferanceNumber", objData.ReferanceNumber);
                paramater.Add("@PaymentMode", 3);
                paramater.Add("@CollectedBy", "Payment Gateway");
                paramater.Add("@SessionID", objData.SessionID);
                paramater.Add("@ExcludedFees", objData.ExcludedFees);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@SBranchID", objData.SBranchID);

                using (var multi = con.QueryMultiple("spn_SaveFeePaymentV2", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    obj.StudentDetails = multi.Read<FeePaymentModel>().SingleOrDefault();
                    obj.PaymentID = multi.Read<int>().SingleOrDefault();
                }

                return obj;
            }
        }
        #endregion

        #region Subscription Payment Popup 
        public BranchSubscriptionModel GetBranchSubscription(int sBranchId)
        {
            using (var con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@SBranchID", sBranchId);
                return con.Query<BranchSubscriptionModel>("sp_GetBranchSubscription", parameters, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public int MarkBranchSubscriptionPaid(int sBranchId, decimal paidAmount, string paymentRef, int? paidByUserId = null)
        {
            using (var con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var now = CommonUsage.GetCurrentDate();

                var parameters = new DynamicParameters();
                parameters.Add("@SBranchID", sBranchId);
                parameters.Add("@PaidAmount", paidAmount);
                parameters.Add("@PaymentRef", paymentRef);
                parameters.Add("@PaymentDate", now);
                parameters.Add("@CreatedBy", paidByUserId);

                return con.Query<int>("sp_MarkBranchSubscriptionPaid", parameters, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        // Add this method inside the AccountData class (e.g. near other fee/report methods)
        public bool IsBranchPaymentPending(int sBranchId)
        {
            try
            {
                var sub = GetBranchSubscription(sBranchId);
                if (sub != null)
                {
                    return sub.IsDue && sub.DueAmount > 0m;
                }

            }
            catch
            {
                // do not block login on exception
            }
            return false;
        }

        public BranchSubscriptionAccountModel GetBranchSubscriptionAccount(int sBranchId)
        {
            using (var con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@SBranchID", sBranchId);

                using (var multi = con.QueryMultiple("sp_GetBranchSubscriptionAccount", parameters, commandType: CommandType.StoredProcedure))
                {
                    var model = new BranchSubscriptionAccountModel();
                    model.Subscription = multi.Read<BranchSubscriptionModel>().SingleOrDefault();
                    model.Payments = multi.Read<BranchSubscriptionPaymentModel>().ToList();

                    if (model.Subscription != null)
                    {
                        model.TotalPaid = model.Payments.Sum(x => x.PaidAmount);
                        model.CurrentDueAmount = model.Subscription.NextDueAmount > 0m
                            ? model.Subscription.NextDueAmount
                            : model.Subscription.DueAmount;
                        model.CanPayNow = model.Subscription.IsDue && model.CurrentDueAmount > 0m;
                    }

                    return model;
                }
            }
        }

        public SystemPaymentGatewayModel GetSystemPaymentGateway()
        {
            using (var con = new SqlConnection(CommonUsage.ConnectionString))
            {
                return con.Query<SystemPaymentGatewayModel>("sp_GetSystemPaymentGateway", commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public int UpsertSystemPaymentGateway(SystemPaymentGatewayModel model)
        {
            using (var con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var now = CommonUsage.GetCurrentDate();

                var parameters = new DynamicParameters();
                parameters.Add("@GatewayID", model.GatewayID);
                parameters.Add("@GatewayName", model.GatewayName);
                parameters.Add("@RazorpayKeyId", model.RazorpayKeyId);
                parameters.Add("@RazorpaySecret", model.RazorpaySecret);
                parameters.Add("@UseForSubscription", model.UseForSubscription ? 1 : 0);
                parameters.Add("@IsActive", model.IsActive ? 1 : 0);
                parameters.Add("@CreatedDate", model.CreatedDate ?? now);
                parameters.Add("@UpdatedDate", now);

                return con.Query<int>("sp_UpsertSystemPaymentGateway", parameters, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        #endregion
    }


}
