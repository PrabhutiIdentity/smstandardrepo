using Dapper;
using SMEnterprise.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SMEnterprise.Repository
{
    public class AdminData
    {

        public int DeletePayment(int PaymentID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@PaymentID", PaymentID);

                return con.Query<int>("sp_DeletePayment", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public List<GSTStateModel> GetGSTStates()
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                return con.Query<GSTStateModel>("sp_GetGSTStates", null, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public List<SMSConfigirationModel> GetSMSConfigurations(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<SMSConfigirationModel>("sp_GetSMSSettings", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public SMSConfigirationModel GetSMSConfigurationDetails(int SMSConfigID, int SBranchID)
        {
            SMSConfigirationModel objModel = new SMSConfigirationModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SMSConfigID", SMSConfigID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetSMSSettingDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<SMSConfigirationModel>().SingleOrDefault();
                    if (objModel == null)
                    {
                        objModel = new SMSConfigirationModel();
                    }
                    objModel.Params = multi.Read<SMSConfigirationParamModel>().ToList();
                }
            }
            return objModel;
        }
        public SMSConfigirationModel GetDefaultSMSConfigurationDetails(int SBranchID)
        {
            SMSConfigirationModel objModel = CommonUsage.SMSConfigurations.Where(c => c.SBranchID == SBranchID).FirstOrDefault();
            if (objModel == null)
            {
                objModel = new SMSConfigirationModel();
                using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
                {
                    var paramater = new DynamicParameters();
                    paramater.Add("@SBranchID", SBranchID);
                    using (var multi = con.QueryMultiple("sp_GetSMSDefaultSettingDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                    {
                        objModel = multi.Read<SMSConfigirationModel>().SingleOrDefault();
                        if (objModel == null)
                        {
                            objModel = new SMSConfigirationModel();
                        }
                        objModel.Params = multi.Read<SMSConfigirationParamModel>().ToList();
                    }
                }
                CommonUsage.SMSConfigurations.Add(objModel);
            }
            return objModel;
        }
        public int UpdateSMSConfigurationSettings(SMSConfigirationModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SMSConfigID", objData.SMSConfigID);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@IsDefault", objData.IsDefault);
                paramater.Add("@Title", objData.Title);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@header", objData.header);
                paramater.Add("@baseurl", objData.baseurl);
                paramater.Add("@balanceURL", objData.balanceURL);
                paramater.Add("@CreatedDate", objData.CreatedDate);
                paramater.Add("@Params", objData.GetParamsDataTable());
                paramater.Add("@OpType", objData.OpType);

                return con.Query<int>("sp_UpdateSMSConfiguration", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
                CommonUsage.SMSConfigurations.Clear();
            }
        }
        public IEnumerable<SBranchModel> GetBranches(int UserID, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@UserID", UserID);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<SBranchModel>("sp_GetSBranches", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public List<object> GetAppBranches(int UserID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@UserID", UserID);
                return con.Query<object>("sp_GetAppSBranches", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public SBranchModel GetAppBranchContact(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<SBranchModel>("sp_GetAppContactDetails", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public List<object> GetAppBranchList()
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                return con.Query<object>("sp_GetAppSBranchList", null, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public List<AppMenuModel> GetAppMenuItems(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<AppMenuModel>("sp_GetAppMenuList", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }

        public string GetAppSplashLogo(int SchoolID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SchoolID", SchoolID);
                return con.Query<string>("sp_GetAppSplashLogo", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int GetBranchIDOnSchoolID(int SchoolID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SchoolID", SchoolID);
                return con.Query<int>("select top 1 SBranchID from SBranchMaster where SchoolID=@SchoolID", paramater, null, true, 0, CommandType.Text).SingleOrDefault();
            }
        }
        public string GetAppSplashLogo()
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                return con.Query<string>("sp_GetAppSplashLogo", null, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public IEnumerable<object> GetTeacherList(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<object>("sp_GetTeacherList", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdateMasterSettings(StartupModelUpdate objData, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@MasterSettings", objData.GetSettingsDataTable());
                paramater.Add("@SBranchID", SBranchID);

                return con.Query<int>("sp_UpdateMasterSettingList", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        #region Building
        public IEnumerable<BuildingModel> GetBuildings(int Status, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Status", Status);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<BuildingModel>("sp_GetBuildings", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public IEnumerable<FloorModel> GetBuildingFloors(int BuildingID, int Status)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@BuildingID", BuildingID);
                paramater.Add("@Status", Status);
                return con.Query<FloorModel>("sp_GetFloors", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public FloorModel GetBuildingFloorRooms(int FloorID, int Status)
        {

            FloorModel objModel = new FloorModel();
            objModel.ID = FloorID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@FloorID", FloorID);
                paramater.Add("@Status", Status);
                using (var multi = con.QueryMultiple("sp_GetRooms", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Rooms = multi.Read<RoomModel>().ToList();
                    objModel.RoomSizes = multi.Read<RoomSizeModel>().ToList();
                }
            }
            return objModel;
        }
        public int InsertUpdateBuilding(BuildingModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Name", objData.Name);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@CreatedDate", objData.CreatedDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@BuildingID", objData.ID);
                paramater.Add("@OpType", objData.OpType);
                if (objData.ID == 0)
                {
                    return con.Query<int>("nsp_ModifyBuilding", paramater, null, true, 0, commandType: CommandType.StoredProcedure).Single();
                }
                else
                {
                    var value = con.Query<int>("nsp_ModifyBuilding", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                    return objData.ID;
                }
            }
        }
        public int InsertUpdateFloor(FloorModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Name", objData.Name);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@CreatedDate", objData.CreatedDate);
                paramater.Add("@BuildingID", objData.BuildingID);
                paramater.Add("@FloorID", objData.ID);
                paramater.Add("@OpType", objData.OpType);
                if (objData.ID == 0)
                {
                    return con.Query<int>("nsp_ModifyFloor", paramater, null, true, 0, commandType: CommandType.StoredProcedure).Single();
                }
                else
                {
                    var value = con.Query<int>("nsp_ModifyFloor", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                    return objData.ID;
                }
            }
        }
        public int InsertUpdateRoom(RoomModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Name", objData.Name);
                paramater.Add("@RoomNo", objData.RoomNo);
                paramater.Add("@SizeID", objData.SizeID);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@CreatedDate", objData.CreatedDate);
                paramater.Add("@FloorID", objData.FloorID);
                paramater.Add("@RoomID", objData.ID);
                paramater.Add("@OpType", objData.OpType);
                if (objData.ID == 0)
                {
                    return con.Query<int>("nsp_ModifyRoom", paramater, null, true, 0, commandType: CommandType.StoredProcedure).Single();
                }
                else
                {
                    var value = con.Query<int>("nsp_ModifyRoom", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                    return objData.ID;
                }
            }
        }
        #endregion
        #region Class-Section
        public IEnumerable<ClassSectionModel> GetPrincipleClassSections(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<ClassSectionModel>("sp_GetPrincipleClassSections", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public ClassPageModel GetClasses(int Status, int SBranchID, int SessionID)
        {
            ClassPageModel objModel = new ClassPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Status", Status);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("spn_GetClasses", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.EducationLevels = multi.Read<NameIDModel>().ToList();
                    objModel.Schemes = multi.Read<EvaluationSchemeModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
                return objModel;
            }
        }
        public async Task<ClassPageModel> GetClassesNoFeeMonthNew(int sbranchID, int sessionID)
        {
            ClassPageModel classNoFeeMonths = new ClassPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", sbranchID);
                paramater.Add("@SessionID", sessionID);
                using (var multi = await con.QueryMultipleAsync("sp_GetClassSessionNoFeeMonthsNew", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    classNoFeeMonths.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    classNoFeeMonths.Classes = multi.Read<ClassModel>().ToList();
                    classNoFeeMonths.NoFeeMonths = multi.Read<NameIDModel>().ToList();
                    classNoFeeMonths.SessionID = multi.Read<int>().SingleOrDefault();
                    classNoFeeMonths.FeeCategories = multi.Read<FeeCategoryModel>().ToList();
                    classNoFeeMonths.Months = multi.Read<NameIDModel>().ToList();
                    classNoFeeMonths.MonthTypeFeeType = multi.Read<NameIDModel>().ToList();
                }
            }
            return classNoFeeMonths;
        }
        public async Task<int> UpdateSessionNoFeeMonthsNew(ClassPageModel noFeeMonth)
        {
            DataTable dtNoFeeMonths = new DataTable();
            dtNoFeeMonths.SetTypeName("ut_Name_ID_Utility");
            dtNoFeeMonths.Columns.Add("ID");
            dtNoFeeMonths.Columns.Add("Name");
            dtNoFeeMonths.Columns.Add("Extra1");
            dtNoFeeMonths.Columns.Add("Extra2");
            dtNoFeeMonths.Columns.Add("Extra3");

            foreach (NameIDModel e in noFeeMonth.NoFeeMonths)
            {
                if (e.Extra1 == "1")
                {
                    DataRow dr = dtNoFeeMonths.NewRow();
                    dr["ID"] = e.ID;
                    dr["Name"] = e.Name;
                    dr["Extra1"] = e.Extra1;
                    dr["Extra2"] = e.Extra2;
                    dr["Extra3"] = e.Extra3;

                    dtNoFeeMonths.Rows.Add(dr);
                }
            }
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SessionID", noFeeMonth.SessionID);
                paramater.Add("@SBranchID", noFeeMonth.SBranchID);
                paramater.Add("@NoFeeMonths", dtNoFeeMonths);

                return (await con.QueryAsync<int>("spn_UpdateSessionNoFeeMonths", paramater, null, 0, commandType: CommandType.StoredProcedure)).SingleOrDefault();
            }
        }
       


        public ClassPageModel GetClassesNoFeeMonths(int SBranchID, int SessionID)
        {
            ClassPageModel objModel = new ClassPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("sp_GetClassSessionNoFeeMonths", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.NoFeeMonths = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();

                   


                }
                return objModel;
            }
        }
        public ClassPageModel GetNoFeeMonthNewDataTable(int SBranchID, int SessionID)
        {
            ClassPageModel objModel = new ClassPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("sp_GetClassSessionNoFeeMonths", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.NoFeeMonths = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();




                }
                return objModel;
            }
        }
        public int UpdateSessionNoFeeMonths(ClassPageModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SessionID", objData.SessionID);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@NoFeeMonths", objData.GetNoFeeMonthDataTable());

                return con.Query<int>("spn_UpdateSessionNoFeeMonths", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public int InsertUpdateClass(ClassModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassName", objData.ClassName);
                paramater.Add("@EducationLevelID", objData.EducationLevelID);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@ClassID", objData.ClassID);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@SessionID", objData.SessionID);
                if (objData.ClassID == 0)
                {
                    return con.Query<int>("spn_InsertUpdateClass", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
                }
                else
                {
                    var value = con.Query<int>("spn_InsertUpdateClass", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
                    return objData.ClassID;
                }
            }
        }
        public IEnumerable<SectionModel> GetClassSections(int ClassID, int Status, int SessionID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@Status", Status);
                paramater.Add("@SessionID", SessionID);
                return con.Query<SectionModel>("spn_GetSections", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public SectionDetailsModel GetSectionDetails(int SectionID, int ClassID)
        {
            SectionDetailsModel objModel = new SectionDetailsModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@ClassID", ClassID);
                using (var multi = con.QueryMultiple("spn_GetSectionDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Section = multi.Read<SectionModel>().SingleOrDefault();
                    objModel.Groups = multi.Read<GroupModel>().ToList();
                    objModel.Buildings = multi.Read<BuildingModel>().ToList();
                    objModel.Floors = multi.Read<FloorModel>().ToList();
                    objModel.Rooms = multi.Read<RoomModel>().ToList();
                    objModel.Teachers = multi.Read<EmployeeModel>().ToList();
                }
            }
            if (objModel.Section == null)
            {
                objModel.Section = new SectionModel();
                objModel.Section.ClassID = ClassID;
            }
            return objModel;
        }
        public IEnumerable<FloorModel> GetFloorsForSection(int SectionID, int BuildingID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@BuildingID", BuildingID);
                return con.Query<FloorModel>("sp_GetFloorsForSection", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public IEnumerable<RoomModel> GetRoomsForSection(int SectionID, int FloorID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@FloorID", FloorID);
                return con.Query<RoomModel>("sp_GetRoomsForSection", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public IEnumerable<EmployeeModel> GetTeachersForSection(int EducationLevelID, int GroupID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EducationLevelID", EducationLevelID);
                paramater.Add("@GroupID", GroupID);
                return con.Query<EmployeeModel>("spn_GetSectionTeachers", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdateSection(SectionModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objData.ClassID);
                paramater.Add("@Name", objData.Name);
                paramater.Add("@GroupID", objData.GroupID);
                paramater.Add("@TeacherID", objData.TeacherID);
                paramater.Add("@Capacity", objData.Capacity);
                paramater.Add("@RoomID", objData.RoomID);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@SectionID", objData.ID);
                paramater.Add("@OpType", objData.OpType);
                if (objData.ID == 0)
                {
                    return con.Query<int>("spn_InsertUpdateSection", paramater, null, true, 0, commandType: CommandType.StoredProcedure).Single();
                }
                else
                {
                    var value = con.Query<int>("spn_InsertUpdateSection", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                    return objData.ID;
                }
            }
        }
        #endregion
        #region Holidays
        public IEnumerable<HolidayModel> GetHolidays(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<HolidayModel>("spn_GetHolidays", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public HolidayDetailModel GetHolidayDetails(int HolidayID, int SBranchID)
        {
            HolidayDetailModel objModel = new HolidayDetailModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@HolidayID", HolidayID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetHolidayDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Holiday = multi.Read<HolidayModel>().SingleOrDefault();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.EmployeeTypes = multi.Read<EmployeeTypeModel>().ToList();
                }
            }
            if (objModel.Holiday == null)
            {
                objModel.Holiday = new HolidayModel();
                objModel.Holiday.StartDate = CommonUsage.GetCurrentDate();
                objModel.Holiday.EndDate = CommonUsage.GetCurrentDate().AddDays(1);
            }
            return objModel;
        }
        public int InsertUpdateHoliday(HolidayModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@HolidayID", objData.HolidayID);
                paramater.Add("@Title", objData.Title);
                paramater.Add("@Description", objData.HolidayDescription);
                paramater.Add("@DayDuration", objData.DayDuration);
                paramater.Add("@StartDate", objData.StartDate);
                paramater.Add("@EndDate", objData.EndDate);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@Classes", objData.Classes);
                paramater.Add("@AssociatedIDs", objData.AssociatedIDs);
                paramater.Add("@HolidayType", objData.HolidayType);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@IsEmployee", objData.IsEmployee);
                paramater.Add("@IsStudents", objData.IsStudents);
                return con.Query<int>("spn_InsertUpdateHolidayNew", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }

        public int InsertUpdateHolidayOld(HolidayModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@HolidayID", objData.HolidayID);
                paramater.Add("@Title", objData.Title);
                paramater.Add("@Description", objData.HolidayDescription);
                paramater.Add("@DayDuration", objData.DayDuration);
                paramater.Add("@StartDate", objData.StartDate);
                paramater.Add("@EndDate", objData.EndDate);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@Classes", objData.Classes);
                paramater.Add("@ClassesIncludedIDs", objData.ClassesIncluded);
                paramater.Add("@HolidayType", objData.HolidayType);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@IsEmployee", objData.IsEmployee);
                if (objData.HolidayID == 0)
                {
                    return con.Query<int>("spn_InsertUpdateHoliday", paramater, null, true, 0, commandType: CommandType.StoredProcedure).Single();
                }
                else
                {
                    var value = con.Query<int>("spn_InsertUpdateHoliday", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                    return objData.HolidayID;
                }
            }
        }

        public HolidayPrintModel GetHolidayPrintDetails(int HolidayID)
        {
            HolidayPrintModel objModel = new HolidayPrintModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@HolidayID", HolidayID);
                using (var multi = con.QueryMultiple("spn_GetHolidayForPrint", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Holiday = multi.Read<HolidayModel>().SingleOrDefault();
                    objModel.SchoolDetail = multi.Read<SchoolDetailModel>().SingleOrDefault();
                    //objModel.Staff = multi.Read<EmployeeModel>().ToList();
                }
            }
            if (objModel.SchoolDetail == null)
            {
                SchoolDetailModel objSDM = new SchoolDetailModel();
                objSDM.PrincipalName = "Principal";
                objSDM.Address = "101, School \n RoadName, Sector \n CityName, District Name \n StateName-PinCode";
                objSDM.EmailID = "info@SchoolDomain.com";
                objSDM.PhoneNo = "xxx-xxxxxx";
                objSDM.SchoolName = "School Name";
                objModel.SchoolDetail = objSDM;
            }
            return objModel;
        }
        #endregion
        #region Period Management
        public IEnumerable<EducationLevelModel> GetEducationLevelsForPeriod(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<EducationLevelModel>("sp_GetEducationLevelForPeriods", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public EducationLevelModel GetPeriodsonEducationLevel(int EducationLevelID)
        {
            EducationLevelModel objModel = new EducationLevelModel();
            objModel.ID = EducationLevelID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EducationLevelID", EducationLevelID);
                using (var multi = con.QueryMultiple("spn_GetPeriodList", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.PeriodList = multi.Read<PeriodModel>().ToList();
                    objModel.PeriodTypeList = multi.Read<PeriodTypeModel>().ToList();
                }
            }
            return objModel;
        }
        public List<EducationLevelSectionModel> GetEducationLevelSections(int EducationLevelID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EducationLevelID", EducationLevelID);
                return con.Query<EducationLevelSectionModel>("spn_GetReportSectionsOnEL", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public List<EducationLevelSectionModel> InsertUpdateELSections(EducationLevelSectionModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ELSectionID", objData.ELSectionID);
                paramater.Add("@EducationLevelID", objData.EducationLevelID);
                paramater.Add("@SectionTitle", objData.SectionTitle);
                paramater.Add("@SectionSubTitle", objData.SectionSubTitle);
                paramater.Add("@OpType", objData.OpType);

                return con.Query<EducationLevelSectionModel>("spn_UpdateEducationLSection", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdatePeriod(PeriodModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@PeriodID", objData.PeriodID);
                paramater.Add("@EducationLevelID", objData.EducationLevelID);
                paramater.Add("@PeriodType", objData.PeriodType);
                paramater.Add("@Name", objData.Name);
                paramater.Add("@StartTime", objData.StartTime);
                paramater.Add("@EndTime", objData.EndTime);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@OpType", objData.OpType);
                if (objData.PeriodID == 0)
                {
                    return con.Query<int>("spn_InsertUpdatePeriod", paramater, null, true, 0, commandType: CommandType.StoredProcedure).Single();
                }
                else
                {
                    var value = con.Query<int>("spn_InsertUpdatePeriod", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                    return objData.PeriodID;
                }
            }
        }
        public int InsertUpdateEducationLevel(EducationLevelModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EducationLevelID", objData.ID);
                paramater.Add("@Name", objData.Name);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@Days", objData.Days);
                paramater.Add("@ShiftType", objData.ShiftType);
                paramater.Add("@OpType", objData.OpType);
                if (objData.ID == 0)
                {
                    return con.Query<int>("spn_InsertUpdateEducationLevel", paramater, null, true, 0, commandType: CommandType.StoredProcedure).Single();
                }
                else
                {
                    var value = con.Query<int>("spn_InsertUpdateEducationLevel", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                    return objData.ID;
                }
            }
        }
        #endregion
        #region TimeTable
        public ParentAPITimeTableModel GetPrincipleApiTTDayLactures(ParentApiParamModel objParam)
        {
            ParentAPITimeTableModel objData = new ParentAPITimeTableModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", objParam.ID);
                paramater.Add("@CurrDate", CommonUsage.GetCurrentDate());
                paramater.Add("@DayID", objParam.DayID);
                using (var multi = con.QueryMultiple("spn_GetPrincipleSectionTimeTable", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objData.Days = multi.Read<int>().SingleOrDefault();
                    objData.Substitutions = multi.Read<ParentAPITTSubstitutions>().ToList();
                    objData.Periods = multi.Read<PeriodModel>().ToList();
                    objData.MergedClasses = multi.Read<TimeTableMergedClassModel>().ToList();
                }
            }
            return objData;
        }
        public TimeTablePageModel GetSectionTimeTable(int SectionID, DateTime CurrDate, int SBranchID)
        {
            TimeTablePageModel objModel = new TimeTablePageModel();
            objModel.TimeTable = new TimeTableModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@CurrDate", CurrDate);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetSectionTimeTable", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.TimeTable = multi.Read<TimeTableModel>().SingleOrDefault();
                    objModel.TimeTable.Periods = multi.Read<PeriodModel>().ToList();
                    objModel.TimeTable.Subjects = multi.Read<SubjectModel>().ToList();
                    objModel.TimeTable.Lactures = multi.Read<TimeTablePeriodModel>().ToList();
                    objModel.TimeTable.Substitutions = multi.Read<TeacherSubstitutionModel>().ToList();
                    objModel.TimeTable.MergedClasses = multi.Read<TimeTableMergedClassModel>().ToList();
                    objModel.EducationLevels = multi.Read<EducationLevelModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<SectionModel>().ToList();
                    objModel.EducationLevelID = multi.Read<int>().SingleOrDefault();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public TimeTablePageModel GetTeacherTimeTable(int TeacherID, DateTime CurrDate, int SBranchID)
        {
            TimeTablePageModel objModel = new TimeTablePageModel();
            objModel.TimeTable = new TimeTableModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@CurrDate", CurrDate);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetTeacherTimeTable", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.TimeTable = multi.Read<TimeTableModel>().SingleOrDefault();
                    objModel.TimeTable.Periods = multi.Read<PeriodModel>().ToList();
                    objModel.TimeTable.Subjects = multi.Read<SubjectModel>().ToList();
                    objModel.TimeTable.Lactures = multi.Read<TimeTablePeriodModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<SectionModel>().ToList();
                    objModel.Teachers = multi.Read<EmployeeModel>().ToList();
                    objModel.TimeTable.Substitutions = multi.Read<TeacherSubstitutionModel>().ToList();
                    objModel.TeacherID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public TeacherSubstitutionEditModel GetSectionSubstitutionDetails(TeacherSubstitutionEditModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@CurrDate", objModel.CurrentDate);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@PeriodID", objModel.PeriodID);
                paramater.Add("@DayID", objModel.DayID);
                objModel.Substitutions = con.Query<TeacherSubstitutionModel>("spn_GetSectionSubstitutionDetails", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
            return objModel;
        }
        public SyllabusScheduleViewModel GetSyllabusSchedule(SyllabusScheduleViewModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@CurrentDate", objModel.CurrentDate);
                objModel.Schedule = con.Query<SyllabusScheduleModel>("sp_GetSyllabusSummery", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
            return objModel;
        }
        public EditTTLactureModel GetLactureEditDetails(EditTTLactureModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@IsMergeSubject", objModel.IsMergedSubject);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@DayName", arrDays[objModel.DayID - 1]);
                paramater.Add("@PeriodID", objModel.PeriodID);
                paramater.Add("@DayID", objModel.DayID);
                paramater.Add("@CurrDate", objModel.CurrentDate);
                paramater.Add("@TeacherID", objModel.TeacherID);
                using (var multi = con.QueryMultiple("spn_GetSectionTTEditDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Subjects = multi.Read<SubjectModel>().ToList();
                    objModel.Teachers = multi.Read<EmployeeModel>().ToList();
                    objModel.SubjectID = multi.Read<int>().SingleOrDefault();
                    if (objModel.Teachers.Count > 0 && objModel.TeacherID == 0)
                    {
                        objModel.TeacherID = objModel.Teachers[0].EmployeeID;
                    }
                }
            }
            return objModel;
        }
        public IEnumerable<EmployeeModel> GetTeacherForSectionSubject(EditTTLactureModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@DayName", arrDays[objModel.DayID - 1]);
                paramater.Add("@PeriodID", objModel.PeriodID);
                paramater.Add("@DayID", objModel.DayID);
                paramater.Add("@CurDate", objModel.CurrentDate);
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@SenderType", objModel.SenderType);
                return con.Query<EmployeeModel>("sp_GetTeachersForSectionSubject", paramater, null, true, 0, CommandType.StoredProcedure).ToList();

            }
        }
        public int InsertUpdateLacture(EditTTLactureModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", objData.SectionID);
                paramater.Add("@SubjectID", objData.SubjectID);
                paramater.Add("@DayName", arrDays[objData.DayID - 1]);
                paramater.Add("@PeriodID", objData.PeriodID);
                paramater.Add("@EducationLevelID", objData.EducationLevelID);
                paramater.Add("@ClassID", objData.ClassID);
                paramater.Add("@TeacherID", objData.TeacherID);

                return con.Query<int>("sp_UpdateTimeTable", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public int InsertUpdateTeacherLacture(EditTTLactureModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", objData.SectionID);
                paramater.Add("@SubjectID", objData.SubjectID);
                paramater.Add("@DayID", objData.DayID);
                paramater.Add("@PeriodID", objData.PeriodID);
                paramater.Add("@EducationLevelID", objData.EducationLevelID);
                paramater.Add("@ClassID", objData.ClassID);
                paramater.Add("@TeacherID", objData.TeacherID);
                paramater.Add("@OldClassID", objData.OldClassID);
                paramater.Add("@OldSectionID", objData.OldSectionID);

                return con.Query<int>("sp_UpdateTeacherTimeTable", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }

        public static string[] arrDays = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        public EditMergedClassModel GetEditMergeClasses(EditMergedClassModel objModel, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@PeriodID", objModel.PeriodID);
                paramater.Add("@DayID", objModel.DayID);
                paramater.Add("@DayName", arrDays[objModel.DayID - 1]);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetEditMergeClasses", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.MergedClasses = multi.Read<TimeTableMergedClassModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<SectionModel>().ToList();
                    objModel.SubTeacherDetails = multi.Read<TeacherSubjectModel>().SingleOrDefault();
                }
            }
            return objModel;
        }
        //For Managing Teacher Substituting 
        public TeacherSubstitutionEditModel GetTeacherSubstitutionDetails(TeacherSubstitutionEditModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@CurrDate", objModel.CurrentDate);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@PeriodID", objModel.PeriodID);
                paramater.Add("@DayID", objModel.DayID);
                paramater.Add("@DayName", arrDays[objModel.DayID - 1]);
                paramater.Add("@EducationLevelID", objModel.EducationLevelID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                using (var multi = con.QueryMultiple("spn_GetTeacherSubstitutionDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Substitutions = multi.Read<TeacherSubstitutionModel>().ToList();
                    objModel.Subjects = multi.Read<SubjectModel>().ToList();
                    objModel.Teachers = multi.Read<EmployeeModel>().ToList();
                }
            }
            return objModel;
        }
        public TeacherSubstitutionEditModel UpdateTeacherSubstitutionDetails(TeacherSubstitutionModel objModel)
        {
            TeacherSubstitutionEditModel objData = new TeacherSubstitutionEditModel();
            objData.SBranchID = objModel.SBranchID;
            objData.CurrentDate = objModel.CurrentDate;
            objData.ClassName = objModel.ClassName;
            objData.SectionName = objModel.SectionName;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SubstitutionID", objModel.SubstitutionID);
                paramater.Add("@ReplacedTeacherID", objModel.ReplacedTeacherID);
                paramater.Add("@ReplacingTeacherID", objModel.ReplacingTeacherID);
                paramater.Add("@PeriodID", objModel.PeriodID);
                paramater.Add("@DayID", objModel.DayID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@NewSubjectID", objModel.NewSubjectID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@FromDate", objModel.FromDate);
                paramater.Add("@EndDate", objModel.EndDate);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@OpType", objModel.OpType);
                paramater.Add("@CurDate", objModel.CurrentDate);
                paramater.Add("@DayName", arrDays[objModel.DayID - 1]);
                paramater.Add("@EducationLevelID", objModel.EducationLevelID);
                using (var multi = con.QueryMultiple("sp_InsertUpdateTeacherSubstitution", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objData.Substitutions = multi.Read<TeacherSubstitutionModel>().ToList();
                    objData.Subjects = multi.Read<SubjectModel>().ToList();
                    objData.Teachers = multi.Read<EmployeeModel>().ToList();
                }
            }
            return objData;
        }
        public TeacherSubstitutionSMSModel GetTeacherSubstitutionSMSDetails(TeacherSubstitutionModel objModel)
        {
            TeacherSubstitutionSMSModel objData = new TeacherSubstitutionSMSModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.ReplacingTeacherID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@PeriodID", objModel.PeriodID);
                paramater.Add("@SubstitutedTeacherID", objModel.ReplacedTeacherID);
                using (var multi = con.QueryMultiple("sp_GetSubstitutionSMSDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objData.ReplacingTeacher = multi.Read<EmployeeModel>().SingleOrDefault();
                    objData.ReplacedTeacher = multi.Read<EmployeeModel>().SingleOrDefault();
                    objData.ClassName = multi.Read<string>().SingleOrDefault();
                    objData.SectionName = multi.Read<string>().SingleOrDefault();
                    objData.PeriodName = multi.Read<string>().SingleOrDefault();
                    objData.SubjectName = multi.Read<string>().SingleOrDefault();
                    objData.FCMToken = multi.Read<string>().SingleOrDefault();
                }
            }
            return objData;
        }
        public int InsertMergedClasses(EditMergedClassModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@FirstSectionID", objData.SectionID);
                paramater.Add("@SecondSectionID", objData.SecondSectionID);
                paramater.Add("@RoomID", objData.RoomID);
                paramater.Add("@SubjectID", objData.SubjectID);
                paramater.Add("@DayID", objData.DayID);
                paramater.Add("@PeriodID", objData.PeriodID);
                paramater.Add("@EducationLevelID", objData.EducationLevelID);
                paramater.Add("@ClassID", objData.ClassID);
                paramater.Add("@TeacherID", objData.TeacherID);
                paramater.Add("@DayName", arrDays[objData.DayID - 1]);

                return con.Query<int>("sp_AddMergeClasses", paramater, null, true, 0, commandType: CommandType.StoredProcedure).Single();

            }
        }
        public EditMergedClassModel GetEditMergeClassSections(EditMergedClassModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@PeriodID", objModel.PeriodID);
                paramater.Add("@DayName", arrDays[objModel.DayID - 1]);
                using (var multi = con.QueryMultiple("spn_GetSectionListOnClass", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Sections = multi.Read<SectionModel>().ToList();
                    try
                    {
                        objModel.SubTeacherDetails = multi.Read<TeacherSubjectModel>().SingleOrDefault();
                    }
                    catch
                    { }
                }
            }
            return objModel;
        }
        public TeacherSubjectModel GetEditMergeClassSectionDetail(EditMergedClassModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@PeriodID", objModel.PeriodID);
                paramater.Add("@DayName", arrDays[objModel.DayID - 1]);
                objModel.SubTeacherDetails = con.Query<TeacherSubjectModel>("sp_GetTeacherSubjectForPeriodDay", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
            return objModel.SubTeacherDetails;
        }
        public int DeleteMergedClasses(int ClassMergeID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassMergeID", ClassMergeID);

                con.Query<int>("sp_DeleteMergeClasses", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                return 1;
            }
        }
        public EditTTLactureModel GetTeacherLactureEditDetails(EditTTLactureModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@EducationLevelID", objModel.EducationLevelID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@PeriodID", objModel.PeriodID);
                paramater.Add("@DayID", objModel.DayID);
                paramater.Add("@TeacherID", objModel.TeacherID);
                using (var multi = con.QueryMultiple("spn_GetTeacherTTEditDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Subjects = multi.Read<SubjectModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sections = multi.Read<SectionModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public IEnumerable<ClassModel> GetEducationLevelClasses(int ELID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EducationLevelID", ELID);
                return con.Query<ClassModel>("sp_GetClassesOnEducationLevel", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public IEnumerable<SectionModel> GetSectionsOnClass(int ClassID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                return con.Query<SectionModel>("sp_GetSectionListOnClass", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public IEnumerable<NameIDModel> GetEmployeeList(int EmployeeTypeID, int SessionID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeTypeID", EmployeeTypeID);
                paramater.Add("@SessionID", SessionID);
                return con.Query<NameIDModel>("Sp_GetemployeelistOld", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public IEnumerable<SectionModel> GetSectionsOnTeacherClass(int TeacherID, int ClassID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", TeacherID);
                paramater.Add("@ClassID", ClassID);
                return con.Query<SectionModel>("sp_GetSectionsForTeacher", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public IEnumerable<SectionModel> GetSectionsOnTeacherClassTT(EditTTLactureModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objData.TeacherID);
                paramater.Add("@ClassID", objData.ClassID);
                paramater.Add("@DayID", objData.DayID);
                paramater.Add("@PeriodID", objData.PeriodID);
                return con.Query<SectionModel>("sp_GetSectionsForTeacherTT", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        #endregion
        #region House Management
        public IEnumerable<HouseModel> GetHouses(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<HouseModel>("sp_GetHouses", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int InsertUpdateHouse(HouseModel ObjData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", ObjData.ID);
                paramater.Add("@Name", ObjData.Name);
                paramater.Add("@HouseColor", ObjData.HouseColor);
                paramater.Add("@UserID", ObjData.UserID);
                paramater.Add("@OpType", ObjData.OpType);
                paramater.Add("@OperationDate", ObjData.OperationDate);
                paramater.Add("@SBranchID", ObjData.SBranchID);

                return con.Query<int>("sp_InsertUpdateHouse", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        #endregion
        #region Transport Management

        

         public EditTransportFeeModel GetTransportRouteFee(EditTransportFeeModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@RouteID", objModel.RouteID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@SessionID", objModel.SessionID);

                using (var multi = con.QueryMultiple("sp_GetFeeForTransport", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.TransportFee = multi.Read<TransportFeeModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.Routes = multi.Read<RoteStopModel>().ToList();
                    objModel.RouteID = multi.Read<int>().SingleOrDefault();

                }
            }
            return objModel;
        }
        public int InsertUpdateTransportFee(EditTransportFeeModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TransportDetails", objData.GetTransportFeeDataTable());
                paramater.Add("@SessionID", objData.SessionID);
                paramater.Add("@SBranchID", objData.SBranchID);

                con.Query<int>("sp_UpdateTransportFee", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
                return 1;
            }
        }
        public IEnumerable<DriverConductorModel> GetDriverConductors(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<DriverConductorModel>("spn_GetAllDriverConductor", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int InsertUpdateDriverConductor(DriverConductorModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", objData.ID);
                paramater.Add("@Name", objData.Name);
                paramater.Add("@LicenceNo", objData.LicenceNo);
                paramater.Add("@CurrentAddress", objData.CurrentAddress);
                paramater.Add("@PermanentAddress", objData.PermanentAddress);
                paramater.Add("@ContactNumber", objData.ContactNumber);
                paramater.Add("@BirthDate", objData.BirthDate);
                paramater.Add("@Type", objData.Type);
                paramater.Add("@Image", objData.Image);
                paramater.Add("@isVarified", objData.isVarified);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OpType", objData.OpType);

                return con.Query<int>("spn_InsertUpdateDriverConductor", paramater, null, true, 0, commandType: CommandType.StoredProcedure).Single();

            }
        }
        public VehicleEditModel GetVehicles(int SBranchID)
        {
            VehicleEditModel objData = new VehicleEditModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetAllVehicles", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objData.Vehicles = multi.Read<VehicleModel>().ToList();
                    objData.Drivers = multi.Read<NameIDModel>().ToList();
                    objData.Conductors = multi.Read<NameIDModel>().ToList();
                }
            }
            return objData;
        }
        public int InsertUpdateVehicle(VehicleModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@VehicleID", objData.VehicleID);
                paramater.Add("@VehicleNumber", objData.VehicleNumber);
                paramater.Add("@RegistrationNumber", objData.RegistrationNumber);
                paramater.Add("@VehicleCapacity", objData.VehicleCapacity);
                paramater.Add("@ContactPerson", objData.ContactPerson);
                paramater.Add("@VehicleType", objData.VehicleType);
                paramater.Add("@InsuranceRenewalDate", objData.InsuranceRenewalDate);
                paramater.Add("@DriverID", objData.DriverID);
                paramater.Add("@ConductorID", objData.ConductorID);
                paramater.Add("@VehicleCondition", objData.VehicleCondition);
                paramater.Add("@IsApproved", objData.IsApproved);
                paramater.Add("@VehicleImage", objData.VehicleImage);
                paramater.Add("@ContactNumber", objData.ContactNumber);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@DeviceID", objData.DeviceID);

                return con.Query<int>("spn_InsertUpdateVehicle", paramater, null, true, 0, commandType: CommandType.StoredProcedure).Single();

            }
        }
        public IEnumerable<RouteModel> GetTransportRoutes(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@IsApproved", -1);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<RouteModel>("sp_GetTransportRoute", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int InsertUpdateTransportRoute(RouteModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@RouteID", objData.RouteID);
                paramater.Add("@RouteName", objData.RouteName);
                paramater.Add("@OneWayFees", objData.OneWayFees);
                paramater.Add("@TwoWayFees", objData.TwoWayFees);
                paramater.Add("@IsApproved", objData.IsApproved);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OpType", objData.OpType);

                con.Query<int>("spn_InsertUpdateTransportRoute", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                return 1;
            }
        }
        public RouteModel GetTransportRouteStops(int RouteID)
        {
            RouteModel objModel = new RouteModel();
            objModel.RouteID = RouteID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@RouteID", RouteID);
                objModel.Stops = con.Query<RouteStoppageModel>("spn_GetTransportRouteStoppages", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
            return objModel;
        }
        public RouteStoppageModel GetTransportStopEditDetails(int StopID, int RouteID, int SBranchID)
        {
            RouteStoppageModel objData;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StopID", StopID);
                paramater.Add("@RouteID", RouteID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetTransportRouteStopDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objData = multi.Read<RouteStoppageModel>().SingleOrDefault();
                    objData.Areas = multi.Read<NameIDModel>().ToList();
                    objData.Cities = multi.Read<NameIDModel>().ToList();
                }
            }
            return objData;
        }
        public RouteModel InsertUpdateTransportStop(RouteStoppageModel objData)
        {

            RouteModel objModel = new RouteModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StopID", objData.StopID);
                paramater.Add("@RouteID", objData.RouteID);
                paramater.Add("@CountryID", objData.CountryID);
                paramater.Add("@StateID", objData.StateID);
                paramater.Add("@CityID", objData.CityID);
                paramater.Add("@AreaID", objData.AreaID);
                paramater.Add("@Time", objData.Time);
                paramater.Add("@HaltDuration", objData.HaltDuration);
                paramater.Add("@SequenceNo", objData.SequenceNo);
                paramater.Add("@TimeR", objData.TimeR);
                paramater.Add("@HaltDurationR", objData.HaltDurationR);
                paramater.Add("@SequenceNoR", objData.SequenceNoR);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@Rate", objData.Rate);


                using (var multi = con.QueryMultiple("spn_InsertUpdateTransportRouteStoppages", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.RouteID = multi.Read<int>().SingleOrDefault();
                    objModel.Stops = multi.Read<RouteStoppageModel>().ToList();
                }

            }
            return objModel;
        }
        public RouteVehicleListModel GetTransportRouteVehicles(int RouteID, int SBranchID)
        {
            RouteVehicleListModel objData = new RouteVehicleListModel();
            objData.RouteID = RouteID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@RouteID", RouteID);
                using (var multi = con.QueryMultiple("spn_GetRouteVehicles", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objData.RouteVehicles = multi.Read<RouteVehicleModel>().ToList();
                    objData.Vehicles = multi.Read<VehicleModel>().ToList();
                }
            }
            return objData;
        }
        public List<RouteStoppageModel> GetTransportRouteVehicleStops(int VehicleRouteID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@VehicleRouteID", VehicleRouteID);
                return con.Query<RouteStoppageModel>("spn_GetVehicleRouteStoppages", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public RouteVehicleListModel InsertUpdateTransportRouteVehicle(RouteVehicleModel objData)
        {
            RouteVehicleListModel objModel = new RouteVehicleListModel();
            objModel.RouteID = objData.RouteID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@VehicleRouteID", objData.VehicleRouteID);
                paramater.Add("@VehicleID", objData.VehicleID);
                paramater.Add("@RouteID", objData.RouteID);
                paramater.Add("@DelayFromRouteTime", objData.DelayFromRouteTime);
                paramater.Add("@ShiftType", objData.ShiftType);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OpType", objData.OpType);
                using (var multi = con.QueryMultiple("spn_InsertUpdateVehicleRoute", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.RouteVehicles = multi.Read<RouteVehicleModel>().ToList();
                    objModel.Vehicles = multi.Read<VehicleModel>().ToList();
                }
            }

            return objModel;

        }
        #endregion

        #region Area Management
        public List<NameIDModel> GetCountryStates(int CountryID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@CountryID", CountryID);
                return con.Query<NameIDModel>("spn_GetCountryStates", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public List<NameIDModel> GetStateCities(int StateID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StateID", StateID);
                return con.Query<NameIDModel>("spn_GetStateCity", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public List<NameIDModel> GetCitieArea(int CityID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@CityID", CityID);
                return con.Query<NameIDModel>("spn_GetCityArea", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        #endregion
        #region Hostal Management
        public List<HostalTypeModel> GetHostalTypes(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<HostalTypeModel>("sp_GetHostelTypes", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int InsertUpdateHostalType(HostalTypeModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", objData.ID);
                paramater.Add("@Name", objData.Name);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@SBranchID", objData.SBranchID);

                con.Query<int>("spn_InsertUpdateHostelType", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                return 1;
            }
        }
        public HostelPageModel GetHostals(int Status, int SBranchID)
        {
            HostelPageModel objModel = new HostelPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetHostels", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Hostals = multi.Read<HostalModel>().ToList();
                    objModel.HostalTypes = multi.Read<HostalTypeModel>().ToList();
                }
            }
            return objModel;
        }
        public int InsertUpdateHostal(HostalModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", objData.ID);
                paramater.Add("@Name", objData.Name);
                paramater.Add("@Address", objData.Address);
                paramater.Add("@WardenName", objData.WardenName);
                paramater.Add("@ContactNumber", objData.ContactNumber);
                paramater.Add("@HostelType", objData.HostelType);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OpType", objData.OpType);

                con.Query<int>("spn_InsertUpdateHostel", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                return 1;
            }
        }
        public HostalModel GetHostalRoomTypes(int HostelID)
        {
            HostalModel objModel = new HostalModel();
            objModel.ID = HostelID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@HostelID", HostelID);
                objModel.RoomTypes = con.Query<HostalRoomTypeModel>("spn_GetHostelRoomTypes", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
            return objModel;
        }
        public HostalModel InsertUpdateHostelRoomType(HostalRoomTypeModel objData)
        {
            HostalModel objModel = new HostalModel();
            objModel.ID = objData.HostelID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", objData.ID);
                paramater.Add("@HostelID", objData.HostelID);
                paramater.Add("@Capacity", objData.Capacity);
                paramater.Add("@RoomTypeName", objData.RoomTypeName);
                paramater.Add("@Rate", objData.Rate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OpType", objData.OpType);
                objModel.RoomTypes = con.Query<HostalRoomTypeModel>("spn_AddHostelRoomTypes", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
            return objModel;
        }
        public HostalModel GetHostalFloors(int HostelID)
        {
            HostalModel objModel = new HostalModel();
            objModel.ID = HostelID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@HostelID", HostelID);
                objModel.Floors = con.Query<HostalFloorModel>("spn_GetHostelFloors", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
            return objModel;
        }
        public HostalModel InsertUpdateHostelFloor(HostalFloorModel objData)
        {
            HostalModel objModel = new HostalModel();
            objModel.ID = objData.HostelID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@FloorID", objData.ID);
                paramater.Add("@HostelID", objData.HostelID);
                paramater.Add("@Name", objData.Name);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@OpType", objData.OpType);
                objModel.Floors = con.Query<HostalFloorModel>("spn_InsertUpdateHostelFloor", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
            return objModel;
        }
        public HostalFloorModel GetHostalFloorRooms(int FloorID)
        {
            HostalFloorModel objModel = new HostalFloorModel();
            objModel.ID = FloorID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@FloorID", FloorID);
                using (var multi = con.QueryMultiple("spn_GetHostelRooms", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Rooms = multi.Read<HostalRoomModel>().ToList();
                    objModel.RoomTypes = multi.Read<HostalRoomTypeModel>().ToList();
                }
            }
            return objModel;
        }
        public HostalFloorModel InsertUpdateHostelFloorRoom(HostalRoomModel objData)
        {
            HostalFloorModel objModel = new HostalFloorModel();
            objModel.ID = objData.FloorID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@RoomID", objData.ID);
                paramater.Add("@FloorID", objData.FloorID);
                paramater.Add("@RoomNo", objData.RoomNo);
                paramater.Add("@Name", objData.Name);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@RoomTypeID", objData.RoomTypeID);
                paramater.Add("@OpType", objData.OpType);
                using (var multi = con.QueryMultiple("spn_InsertUpdateHostelRoom", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Rooms = multi.Read<HostalRoomModel>().ToList();
                    objModel.RoomTypes = multi.Read<HostalRoomTypeModel>().ToList();
                }
            }
            return objModel;
        }
        #endregion
        #region Notice Management
        public NoticePageModel GetNotices(int SBranchID)
        {
            NoticePageModel objModel = new NoticePageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spn_GetAllNotices", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Notices = multi.Read<NoticeModel>().ToList();
                    objModel.SelectionList = multi.Read<NameIDModel>().ToList();
                }
            }
            return objModel;
        }
        public IEnumerable<NoticeModel> GetPrincipleNotices(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<NoticeModel>("sp_GetPrincipleNotices", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int InsertUpdateNotice(NoticeModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@NoticeTitle", objData.NoticeTitle);
                paramater.Add("@NoticeDescription", objData.NoticeDescription);
                paramater.Add("@NoticeStartDate", objData.NoticeStartDate);
                paramater.Add("@NoticeEndDate", objData.NoticeEndDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@ApplicableFor", objData.ApplicableFor);
                paramater.Add("@NoticeLevel", objData.NoticeLevel);
                paramater.Add("@ClassesIncludedIDs", objData.ClassesIncludedIDs);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@NoticeID", objData.NoticeID);
                paramater.Add("@OpType", objData.OpType);

                return con.Query<int>("sp_InsertUpdateNotice", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        #endregion
        #region Leave Management
        public List<LeaveTypeModel> GetLeaveTypes(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<LeaveTypeModel>("sp_GetLeaveTypes", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int UpdateLeaveType(LeaveTypeModel Contact)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@LeaveTypeID", Contact.LeaveTypeID);
                paramater.Add("@LeaveTypeName", Contact.LeaveTypeName);
                paramater.Add("@YearlyQuota", Contact.YearlyQuota);
                paramater.Add("@MonthlyQuota", Contact.MonthlyQuota);
                paramater.Add("@IsSalaryDeduct", Contact.IsSalaryDeduct);
                paramater.Add("@SBranchID", Contact.SBranchID);
                paramater.Add("@IsCarryForward", Contact.IsCarryForward);
                paramater.Add("@MaxConsecutive", Contact.MaxConsecutive);
                paramater.Add("@OpType", Contact.OpType);
                return con.Query<int>("sp_InsertUpdateLeaveType", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public List<LeaveModel> GetLeaves(int SBranchID, int Status, int ApplicantType)
        {
            NoticePageModel objModel = new NoticePageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ApplicantType", ApplicantType);
                paramater.Add("@Status", Status);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<LeaveModel>("spn_GetAllLeaves", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        public LeaveEditModel GetLeaveDetails(int LeaveID)
        {
            LeaveEditModel objModel = new LeaveEditModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@LeaveID", LeaveID);
                using (var multi = con.QueryMultiple("spn_GetLeaveDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<LeaveEditModel>().SingleOrDefault();
                    objModel.Leave = multi.Read<LeaveModel>().SingleOrDefault();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Students = multi.Read<NameIDModel>().ToList();
                    objModel.Employees = multi.Read<NameIDModel>().ToList();
                }
            }
            if (LeaveID == 0)
            {
                objModel.Leave = new LeaveModel();
                objModel.Leave.StartDate = CommonUsage.GetCurrentDate();
                objModel.Leave.EndDate = CommonUsage.GetCurrentDate();
            }
            return objModel;
        }
        public LeaveEditModel GetAdminLeaveDetails(int LeaveID)
        {
            LeaveEditModel objModel = new LeaveEditModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@LeaveID", LeaveID);
                using (var multi = con.QueryMultiple("spn_GetAdminLeaveDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Leave = multi.Read<LeaveModel>().SingleOrDefault();
                    objModel.LeaveDetails = multi.Read<LeaveDetailModel>().ToList();
                }
            }
            if (LeaveID == 0)
            {
                objModel.Leave = new LeaveModel();
                objModel.Leave.StartDate = CommonUsage.GetCurrentDate();
                objModel.Leave.EndDate = CommonUsage.GetCurrentDate();
            }
            return objModel;
        }
        public int InsertLeave(LeaveModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ApplicantType", objData.ApplicantType);
                paramater.Add("@LeaveType", objData.LeaveType);
                paramater.Add("@StartDate", objData.StartDate);
                paramater.Add("@EndDate", objData.EndDate);
                paramater.Add("@LeaveReason", objData.LeaveReason);
                paramater.Add("@ApplicantID", objData.ApplicantID);
                paramater.Add("@IsApproved", objData.IsApproved);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@CreatedDate", objData.OperationDate);
                paramater.Add("@SBranchID", objData.SBranchID);

                var value = con.Query<int>("sp_InsertLeave", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                return 1;

            }
        }
        public int UpdateLeave(LeaveModel objData)
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
                paramater.Add("@ApplicantID", objData.ApplicantID);
                paramater.Add("@IsApproved", objData.IsApproved);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@ModifiedDate", objData.OperationDate);
                paramater.Add("@SBranchID", objData.SBranchID);

                var value = con.Query<int>("sp_UpdateLeave", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                return 1;

            }
        }
        public int UpdateLeaveStatus(LeaveModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@LeaveID", objData.LeaveID);
                paramater.Add("@IsApproved", objData.IsApproved);

                var value = con.Query<int>("sp_UpdateLeaveStatus", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                return 1;

            }
        }
        public int DeleteLeave(LeaveModel objData)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@LeaveID", objData.LeaveID);

                var value = con.Query<int>("sp_DeleteLeave", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                return 1;

            }
        }
        public IEnumerable<NameIDModel> GetSectionStudents(int SectionID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", SectionID);
                return con.Query<NameIDModel>("spn_GetSectionStudentList", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public LeaveEditModel GetEmployeeLeaveDetails(int EmployeeID, DateTime LeaveDate)
        {
            LeaveEditModel objModel = new LeaveEditModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", EmployeeID);
                paramater.Add("@LeaveDate", LeaveDate);
                objModel = con.Query<LeaveEditModel>("spn_GetEmployeeLeaveDetails", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
            objModel.Leave = new LeaveModel();
            objModel.Leave.ApplicantType = 0;
            return objModel;
        }
        #endregion

        #region Master Setting
        public IEnumerable<MasterSettingModel> GetMasterSettings(string Type, int Status, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Type", Type);
                paramater.Add("@Status", Status);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<MasterSettingModel>("sp_GetMasterSetting", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdateMasterSetting(MasterSettingModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", objData.ID);
                paramater.Add("@Name", objData.Name);
                paramater.Add("@Description", objData.Description);
                paramater.Add("@Type", objData.Type);
                paramater.Add("@Details", objData.Details);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@OpType", objData.OpType);

                con.Query<int>("sp_InsertUpdateMasterSetting", paramater, null, true, 0, commandType: CommandType.StoredProcedure);
                return 1;
            }
        }
        #endregion
        #region Evaluation Management
        public List<EvaluationSchemeModel> GetSessionSchemes(int SessionID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SessionID", SessionID);
                return con.Query<EvaluationSchemeModel>("sp_GetSessionEvaluationSchemes", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public EvaluationSchemePageModel GetEvaluationSchemes(int SBranchID, int SessionID)
        {
            EvaluationSchemePageModel objModel = new EvaluationSchemePageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("sp_GetEvaluationSchemes", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Schemes = multi.Read<EvaluationSchemeModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                }
                return objModel;

            }
        }
        public EditEvaluationModel GetEditEvaluationSchemes(int EvaluationID,int SessionID)
        {
            EditEvaluationModel objModel = new EditEvaluationModel();   
             using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EvaluationID", EvaluationID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("Sp_GetEvaluationClasses", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.EvaluationSchemeID = multi.Read<int>().SingleOrDefault();
                    objModel.EvaluationSchemeName = multi.Read<string>().SingleOrDefault();
                }
                return objModel;

            }
        }
        public IEnumerable<NameIDModel> GetEmployeeClass(int EmployeeID, int SessionID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeID", EmployeeID);
                paramater.Add("@SessionID", SessionID);
                return con.Query<NameIDModel>("Sp_GetemployeeClasslist", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public EmployeeAssignPageModel GetAssignedEmployee(int SBranchID, int SessionID)
        {
            EmployeeAssignPageModel objModel = new EmployeeAssignPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("sp_AssignedEmployee", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.EmployeeType = multi.Read<NameIDModel>().ToList();
                    objModel.Employeelist = multi.Read<NameIDModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.AssignedEmployee = multi.Read<EmployeeAssignModel>().ToList();
                }
                return objModel;

            }
        }
        public int DeleteEmployeeAssing(int ID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", ID);

                return con.Query<int>("Sp_DeleteEmployeeAssign", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int InsertAssignEmployee(EmployeeAssignModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                int res = 0;
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeAssignID", objModel.EmployeeAssignID);
                paramater.Add("@EmployeeTypeID", objModel.EmployeeTypeID);
                paramater.Add("@EmployeeID", objModel.EmployeeID);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@Status", objModel.Status);
                paramater.Add("@OpType", objModel.OpType);
                if (objModel.OpType == -1)
                {
                    paramater.Add("@ClassID", 0);
                    res = con.Query<int>("Sp_InsertUpdateEMployeeAssign", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
                }
                else
                {
                    foreach (var item in objModel.ClassesAry)
                    {
                        paramater.Add("@ClassID", item);
                        res = con.Query<int>("Sp_InsertUpdateEMployeeAssign", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
                    }
                }
                return res;

            }
        }
        public int InsertUpdateEvaluationScheme(EvaluationSchemeModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EvaluationSchemeID", objData.EvaluationSchemeID);
                paramater.Add("@EvaluationSchemeName", objData.EvaluationSchemeName);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@SessionID", objData.SessionID);
                paramater.Add("@ClassIDs", objData.ClassIDs);
                paramater.Add("@OpType", objData.OpType);

                return con.Query<int>("spn_InsertUpdateEvaluationScheme", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public EvaluationTypePageModel GetEvaluationTypes(int SBranchID, int SessionID, int EvaluationSchemeID)
        {
            EvaluationTypePageModel objModel = new EvaluationTypePageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                paramater.Add("@EvaluationSchemeID", EvaluationSchemeID);
                using (var multi = con.QueryMultiple("sp_GetEvaluationTypes", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Evaluations = multi.Read<EvaluationTypeModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.Schemes = multi.Read<EvaluationSchemeModel>().ToList();
                    objModel.EvaluationSchemeID = multi.Read<int>().SingleOrDefault();
                }
                return objModel;

            }
        }
        public int InsertUpdateEvaluationType(EvaluationTypeModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EvaluationTypeID", objData.EvaluationTypeID);
                paramater.Add("@EvaluationTypeName", objData.EvaluationTypeName);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@SessionID", objData.SessionID);
                paramater.Add("@EvaluationSchemeID", objData.EvaluationSchemeID);
                paramater.Add("@OpType", objData.OpType);

                return con.Query<int>("sp_InsertUpdateEvaluationType", paramater, null, true, 0, commandType: CommandType.StoredProcedure).Single();

            }
        }
        public ExamPageModel GetAllClasses(int Status, int SBranchID, int SessionID)
        {
            ExamPageModel objModel = new ExamPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Status", Status);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("sp_GetAllClasses", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Evaluations = multi.Read<EvaluationModel>().ToList();
                    objModel.Groups = multi.Read<GroupModel>().ToList();
                }
            }
            return objModel;
        }
        public EvaluationPageModel GetEvaluations(int SBranchID, int SessionID, int SchemeID)
        {
            EvaluationPageModel objModel = new EvaluationPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                StartupModel objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@EvaluationMode", objStartupModel.EvaluationMode);
                paramater.Add("@SessionID", SessionID);
                paramater.Add("@EvaluationSchemeID", SchemeID);
                using (var multi = con.QueryMultiple("sp_GetEvaluations", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Evaluations = multi.Read<EvaluationModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.Schemes = multi.Read<EvaluationSchemeModel>().ToList();
                    objModel.EvaluationSchemeID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public void GetSubEvaluations(SubEvaluationListModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EvaluationID", objModel.EvaluationID);
                using (var multi = con.QueryMultiple("sp_GetSubEvaluations", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.SubEvaluations = multi.Read<EvaluationModel>().ToList();
                    objModel.EvaluationTypes = multi.Read<EvaluationTypeModel>().ToList();
                }

            }
        }
        public int InsertUpdateEvaluation(EvaluationModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EvaluationID", objData.EvaluationID);
                paramater.Add("@EvaluationName", objData.EvaluationName);
                paramater.Add("@Weightage", objData.Weightage);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@EvaluationMonth", objData.EvaluationMonth);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@SessionID", objData.SessionID);
                paramater.Add("@EvaluationSchemeID", objData.EvaluationSchemeID);
                paramater.Add("@ResultDate", objData.ResultDate);

                return con.Query<int>("sp_InsertUpdateEvaluation", paramater, null, true, 0, commandType: CommandType.StoredProcedure).Single();

            }
        }
        public SubEvaluationListModel InsertUpdateSubEvaluation(EvaluationModel objData)
        {
            SubEvaluationListModel objModel = new SubEvaluationListModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@MasterID", objData.MasterID);
                paramater.Add("@EvaluationID", objData.EvaluationID);
                paramater.Add("@EvaluationType", objData.EvaluationType);
                paramater.Add("@EvaluationName", objData.EvaluationName);
                paramater.Add("@Weightage", objData.Weightage);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@EvaluationMonth", objData.EvaluationMonth);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OpType", objData.OpType);
                using (var multi = con.QueryMultiple("sp_InsertUpdateSubEvaluation", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.SubEvaluations = multi.Read<EvaluationModel>().ToList();
                    objModel.EvaluationTypes = multi.Read<EvaluationTypeModel>().ToList();
                }

            }
            objModel.EvaluationID = objData.MasterID;
            return objModel;
        }
        public ExamPageModel GetExams(ExamPageModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                StartupModel objStartupModel = (StartupModel)HttpContext.Current.Session["StartupModel"];
                var paramater = new DynamicParameters();
                paramater.Add("@EvaluationID", objModel.EvaluationID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@GroupID", objModel.GroupID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@EvaluationMode", objStartupModel.EvaluationMode);
                paramater.Add("@SessionID", objModel.SessionID);

                using (var multi = con.QueryMultiple("spn_GetExamPageModel", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Evaluations = multi.Read<EvaluationModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Groups = multi.Read<GroupModel>().ToList();
                    objModel.Exams = multi.Read<ExamModel>().ToList();
                    objModel.EvaluationID = multi.Read<int>().SingleOrDefault();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.GroupID = multi.Read<int>().SingleOrDefault();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    try
                    {
                        objModel.MarkingScheme = multi.Read<int>().SingleOrDefault();
                    }
                    catch
                    { }
                }
            }

            return objModel;

        }
        public ExamPageModel GetClassGroupEvaluations(int ClassID, int Status, int SBranchID, int SessionID)
        {
            ExamPageModel objModel = new ExamPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@Status", Status);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("sp_GetGroupEvaluations", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Evaluations = multi.Read<EvaluationModel>().ToList();
                    objModel.Groups = multi.Read<GroupModel>().ToList();
                }

            }
            return objModel;
        }
        public IEnumerable<NameIDModel> GetClassGroups(int ClassID, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<NameIDModel>("sp_GetClassGroups", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int InsertUpdateExam(ExamPageModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ExamDetails", objData.GetExamsDataTable());

                con.Query<int>("sp_UpdateExamDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
                return 1;
            }
        }
        public TeacherResultPageModel GetSectionsEvaluationsOnClass(int ClassID, int SBranchID, int SessionID)
        {
            TeacherResultPageModel objModel = new TeacherResultPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", ClassID);
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@SessionID", SessionID);
                using (var multi = con.QueryMultiple("sp_GetSectionEvaluationListOnClass", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Evaluations = multi.Read<NameIDModel>().ToList();
                }
            }
            return objModel;
        }
        #endregion

        #region Group/Subject Management
        public GroupEditModel GetGroupsForAdmin(int SBranchID)
        {
            GroupEditModel objModel = new GroupEditModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetAdminGroups", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Groups = multi.Read<GroupModel>().ToList();
                    objModel.EducationLevels = multi.Read<EducationLevelModel>().ToList();
                }
            }
            return objModel;
        }
        public int InsertUpdateGroup(GroupModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@GroupID", objData.GroupID);
                paramater.Add("@EducationLevelID", objData.EducationLevelID);
                paramater.Add("@GroupName", objData.GroupName);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OpType", objData.OpType);

                con.Query<int>("spn_InsertUpdateGroup", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
                return 1;
            }
        }
        public GroupModel GetGroupSubjects(int GroupID)
        {
            GroupModel objModel = new GroupModel();
            objModel.GroupID = GroupID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@GroupID", GroupID);
                objModel.SubjectList = con.Query<SubjectModel>("spn_GetGroupSubjectDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
            return objModel;
        }
        public List<SubSubjectTypeModel> GetGroupSubjectTypes(int GroupID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@GroupID", GroupID);
                return con.Query<SubSubjectTypeModel>("sp_GetSubSubjectTypes", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        public List<SubSubjectTypeModel> UpdateGroupSubjectTypes(SubSubjectTypeModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TypeID", oModel.TypeID);
                paramater.Add("@TypeName", oModel.TypeName);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@GroupID", oModel.GroupID);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@OpType", oModel.OpType);
                return con.Query<SubSubjectTypeModel>("sp_InsertUpdateSubSubjectTypes", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        public List<SubjectModel> GetSubSubjects(int SubjectID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SubjectID", SubjectID);
                return con.Query<SubjectModel>("sp_GetSubSubjects", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public SubSubjectEditModel GetSubSubjectsPageModel(int SubjectID)
        {
            SubSubjectEditModel oModel = new SubSubjectEditModel();
            oModel.MainSubjectID = SubjectID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SubjectID", SubjectID);
                using (var multi = con.QueryMultiple("sp_GetSubSubjectEditPageModel", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Types = multi.Read<SubSubjectTypeModel>().ToList();
                    oModel.Subjects = multi.Read<SubjectModel>().ToList();
                    oModel.GroupID = multi.Read<int>().SingleOrDefault();
                }

            }
            return oModel;
        }
        public GroupModel InsertUpdateGroupSubject(SubjectModel objData)
        {
            GroupModel objModel = new GroupModel();
            objModel.GroupID = objData.GroupID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SubjectID", objData.SubjectID);
                paramater.Add("@GroupID", objData.GroupID);
                paramater.Add("@SubjectName", objData.SubjectName);
                paramater.Add("@IsOptionalSubject", objData.IsOptionalsubject);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@SubjectCode", objData.SubjectCode);
                paramater.Add("@MainSubID", objData.MainSubID);
                paramater.Add("@SubjectType", objData.SubjectType);

                objModel.SubjectList = con.Query<SubjectModel>("spn_InsertUpdateGroupSubject", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
            return objModel;
        }
        public GroupModel InsertUpdateGroupSubjectMarkingGrade(SubjectModel objData)
        {
            GroupModel objModel = new GroupModel();
            objModel.GroupID = objData.GroupID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SubjectID", objData.SubjectID);
                paramater.Add("@GroupID", objData.GroupID);
                paramater.Add("@SubjectName", objData.SubjectName);
                paramater.Add("@IsOptionalSubject", objData.IsOptionalsubject);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@SubjectCode", objData.SubjectCode);
                paramater.Add("@MainSubID", objData.MainSubID);
                paramater.Add("@SubjectType", objData.SubjectType);

                paramater.Add("@MarkingScheme", objData.MarkingScheme);
                objModel.SubjectList = con.Query<SubjectModel>("spn_InsertUpdateGroupSubject", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
            return objModel;
        }

        #endregion
        #region Fee Management
        public FeeCategoryEditModel GetFeeTypes(int SBranchID)
        {
            FeeCategoryEditModel oModel = new FeeCategoryEditModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetFeeTypes", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.FeeCategories = multi.Read<FeeCategoryModel>().ToList();
                    oModel.FeeApplicables = multi.Read<NameIDModel>().ToList();
                }

            }
            return oModel;
        }
        public int InsertUpdateFeeType(FeeCategoryModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@FeeTypeID", objData.FeeTypeID);
                paramater.Add("@FeeTypeName", objData.FeeTypeName);
                paramater.Add("@FeeTypeApplicable", objData.FeeTypeApplicable);
                paramater.Add("@ChartColor", objData.ChartColor);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@Months", objData.Months);

                return con.Query<int>("sp_InsertUpdateFeeTypes", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public ClassFeeStructureEditModel GetFeeStructure(ClassFeeStructureEditModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@GroupID", objModel.GroupID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@SessionID", objModel.SessionID);

                using (var multi = con.QueryMultiple("sp_GetFeeStructureForClassGroup", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.FeeStructure = multi.Read<ClassFeeStructureModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.GroupID = multi.Read<int>().SingleOrDefault();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Groups = multi.Read<GroupModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public int InsertUpdateFeeStructure(ClassFeeStructureEditModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@StructureDetails", objData.GetFeeStructureDataTable());
                paramater.Add("@SessionID", objData.SessionID);

                con.Query<int>("sp_UpdateFeeStructure", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
                return 1;
            }
        }
        #endregion
     
        
        #region Salary Management
        public IEnumerable<SalaryCategoryModel> GetSalaryTypesOld(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<SalaryCategoryModel>("sp_GetSalaryTypes", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        public SalaryCategoryPageModel GetSalaryTypes(int SBranchID)
        {
            SalaryCategoryPageModel oModel = new SalaryCategoryPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetSalaryTypes", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.SalaryTypes = multi.Read<SalaryCategoryModel>().ToList();
                    oModel.SalaryApplicableTypes = multi.Read<NameIDModel>().ToList();
                }

            }
            return oModel;
        }
        public int InsertUpdateSalaryType(SalaryCategoryModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TypeID", objData.TypeID);
                paramater.Add("@TypeName", objData.TypeName);
                paramater.Add("@TypeApplicable", objData.TypeApplicable);
                paramater.Add("@ChartColor", objData.ChartColor);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@Months", objData.Months);
                paramater.Add("@IsDeduction", objData.IsDeduction);
                paramater.Add("@AttendanceType", objData.AttendanceType);
                paramater.Add("@MinDays", objData.MinDays);
                paramater.Add("@OpType", objData.OpType);

                return con.Query<int>("sp_InsertUpdateSalaryTypes", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public IEnumerable<EmployeeTypeModel> GetEmployeeTypes(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<EmployeeTypeModel>("sp_GetAllEmployeeTypes", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int InsertUpdateEmployeeType(EmployeeTypeModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeTypeID", objData.EmployeeTypeID);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@EmployeeTypeName", objData.EmployeeTypeName);
                paramater.Add("@OpType", objData.OpType);

                return con.Query<int>("spn_InsertUpdateEmployeeType", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public EmployeeTypeModel GetEmployeeSalaryDetails(int EmployeeTypeID, int SBranchID)
        {
            EmployeeTypeModel objModel = new EmployeeTypeModel();
            objModel.EmployeeTypeID = EmployeeTypeID;
            objModel.SBranchID = SBranchID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EmployeeTypeID", EmployeeTypeID);
                paramater.Add("@SBranchID", SBranchID);
                objModel.Salaries = con.Query<EmployeeTypeSalaryModel>("sp_GetEmployeeTypeSalaryDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
            return objModel;
        }
        public EmployeeTypeModel InsertUpdateSalaryStructure(EmployeeTypeModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@EmployeeTypeID", objData.EmployeeTypeID);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@SalaryDetails", objData.GetFeeStructureDataTable());

                objData.Salaries = con.Query<EmployeeTypeSalaryModel>("sp_UpdateSalaryDetails", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
            return objData;
        }
        public IEnumerable<ExpenceCategoryModel> GetExpenceTypes(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@SBranchID", SBranchID);
                return con.Query<ExpenceCategoryModel>("sp_GetExpenceTypes", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int InsertUpdateExpenceType(ExpenceCategoryModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ExpenceTypeID", objData.ExpenceTypeID);
                paramater.Add("@ExpenceTypeName", objData.ExpenceTypeName);
                paramater.Add("@ExpenceTypeApplicable", objData.ExpenceTypeApplicable);
                paramater.Add("@ChartColor", objData.ChartColor);
                paramater.Add("@UserID", objData.SBranchID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@OpType", objData.OpType);

                return con.Query<int>("sp_InsertUpdateExpenceTypes", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #endregion
        #region Quota Management
        public IEnumerable<QuotaModel> GetQuotas(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<QuotaModel>("sp_GetAdminQuotas", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int InsertUpdateQuota(QuotaModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@QuotaID", objData.QuotaID);
                paramater.Add("@QuotaName", objData.QuotaName);
                paramater.Add("@Description", objData.Description);
                paramater.Add("@NumberOfStudents", objData.NumberOfStudents);
                paramater.Add("@IsApproved", objData.IsApproved);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@OpType", objData.OpType);

                return con.Query<int>("spn_InsertUpdateQuota", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public QuotaModel GetQuotaDiscounts(QuotaModel Model)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@QuotaID", Model.QuotaID);
                paramater.Add("@SessionID", Model.SessionID);
                using (var multi = con.QueryMultiple("spn_GetQuotaDiscountDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    Model.Discounts = multi.Read<QuotaDiscountsModel>().ToList();
                    Model.Sessions = multi.Read<NameIDModel>().ToList();
                    Model.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return Model;
        }
        public QuotaModel InsertUpdateQuotaDiscounts(QuotaModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@QuotaID", objData.QuotaID);
                paramater.Add("@DiscountDetails", objData.GetQuotaDiscountDataTable());
                paramater.Add("@SessionID", objData.SessionID);

                using (var multi = con.QueryMultiple("sp_UpdateQuotaDiscounts", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objData.Discounts = multi.Read<QuotaDiscountsModel>().ToList();
                    objData.Sessions = multi.Read<NameIDModel>().ToList();
                    objData.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objData;
        }
        #endregion
        #region Event Type Management
        public IEnumerable<EventCategoryModel> GetEventTypes(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<EventCategoryModel>("sp_GetEventType", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int InsertUpdateEventType(EventCategoryModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", objData.ID);
                paramater.Add("@Name", objData.Name);
                paramater.Add("@BackGroundColor", objData.BackGroundColor);
                paramater.Add("@TextColor", objData.TextColor);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@SBranchID", objData.SBranchID);

                return con.Query<int>("sp_InsertUpdateEventType", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int InsertUpdateEvent(CommonEvents objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@EventID", objData.id);
                paramater.Add("@Title", objData.title);
                paramater.Add("@Description", objData.Description);
                paramater.Add("@StartDate", objData.start);
                paramater.Add("@EndDate", objData.end);
                paramater.Add("@EventTypeID", objData.EventTypeID);
                paramater.Add("@ClassesIncluded", objData.ClassesIncluded);
                paramater.Add("@IsApproved", objData.IsApproved);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OpType", objData.OpType);

                return con.Query<int>("sp_InsertUpdateEvent", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public List<EventCategoryModel> GetEventTypeList(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<EventCategoryModel>("sp_GetEventTypeList", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public List<VehicleAppModel> GetVehicleList(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<VehicleAppModel>("sp_GetTransportVehicles", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public List<NameIDModel> GetVehicleRoutes(int SBranchID, int VehicleID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@VehicleID", VehicleID);
                return con.Query<NameIDModel>("spn_GetTransportVehicleRouteList", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public TransportMapModel GetPrincipleTransportMap(int VehicleRouteID)
        {
            TransportMapModel objModel = new TransportMapModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@VehicleRouteID", VehicleRouteID);
                using (var multi = con.QueryMultiple("sp_GetPrincipleTransportMapDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<TransportMapModel>().SingleOrDefault();
                    if (objModel != null)
                    {
                        objModel.Stops = multi.Read<RouteStoppageModel>().ToList();
                        objModel.Driver = multi.Read<TransportEmployeeModel>().SingleOrDefault();
                        objModel.Conductor = multi.Read<TransportEmployeeModel>().SingleOrDefault();
                    }
                }

            }
            return objModel;
        }
        #endregion
        #region Syllabus Management
        public SyllabusModel GetChapters(SyllabusModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@GroupID", objModel.GroupID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@SBranchID", objModel.SBranchID);

                using (var multi = con.QueryMultiple("sp_GetAdminChapters", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Chapters = multi.Read<ChapterModel>().ToList();
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Groups = multi.Read<GroupModel>().ToList();
                    objModel.Subjects = multi.Read<SubjectModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.GroupID = multi.Read<int>().SingleOrDefault();
                    objModel.SubjectID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        public IEnumerable<SubjectModel> GetGroupSubjectList(int GroupID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@GroupID", GroupID);
                return con.Query<SubjectModel>("sp_GetGroupSubjects", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int InsertUpdateChapter(ChapterModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ChapterID", objData.ChapterID);
                paramater.Add("@ClassID", objData.ClassID);
                paramater.Add("@GroupID", objData.GroupID);
                paramater.Add("@SubjectID", objData.SubjectID);
                paramater.Add("@ChapterName", objData.ChapterName);
                paramater.Add("@ChapterDescription", objData.ChapterDescription);
                paramater.Add("@IsApproved", objData.IsApproved);
                paramater.Add("@UserID", objData.UserID);
                paramater.Add("@OperationDate", objData.OperationDate);
                paramater.Add("@SequenceNo", objData.SequenceNo);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OpType", objData.OpType);

                return con.Query<int>("spn_InsertUpdateChapter", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public ChapterModel GetChapterTopics(ChapterModel Model)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ChapterID", Model.ChapterID);

                using (var multi = con.QueryMultiple("sp_GetChapterTopics", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    Model.Topics = multi.Read<TopicModel>().ToList();
                    Model.Evaluations = multi.Read<EvaluationModel>().ToList();
                }
            }
            return Model;
        }
        public ChapterModel InsertUpdateChapterTopics(TopicModel Model)
        {
            ChapterModel objModel = new ChapterModel();
            objModel.ChapterID = Model.ChapterID;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TopicID", Model.TopicID);
                paramater.Add("@ChapterID", Model.ChapterID);
                paramater.Add("@TopicName", Model.TopicName);
                paramater.Add("@SequenceNo", Model.SequenceNo);
                paramater.Add("@Duration", Model.Duration);
                paramater.Add("@Marks", Model.Marks);
                paramater.Add("@EvaluationID", Model.EvaluationID);
                paramater.Add("@OpType", Model.OpType);

                using (var multi = con.QueryMultiple("sp_InsertUpdateChapterTopic", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Topics = multi.Read<TopicModel>().ToList();
                    objModel.Evaluations = multi.Read<EvaluationModel>().ToList();
                }
            }
            return objModel;
        }
        #endregion

        #region Admin DashBoard
        public AdminDashboardModel GetDashBoardData(int SBranchID, DateTime CurrentDate)
        {
            AdminDashboardModel objModel = new AdminDashboardModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@CurrentDate", CurrentDate);

                using (var multi = con.QueryMultiple("sp_GetAdminDashboardData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.ClassSectionData = multi.Read<ClassSectionChartModel>().ToList();
                    objModel.StudentsCount = multi.Read<int>().SingleOrDefault();
                    objModel.ClassCount = multi.Read<int>().SingleOrDefault();
                    objModel.TeacherCount = multi.Read<int>().SingleOrDefault();
                    objModel.AbsentTeachers = multi.Read<int>().SingleOrDefault();
                    objModel.MonthlyCollection = multi.Read<int>().SingleOrDefault();
                    objModel.MonthlyExpence = multi.Read<int>().SingleOrDefault();
                    objModel.AbsentTeacherList = multi.Read<EmployeeModel>().ToList();
                    objModel.EmployeeAttandance = multi.Read<AttandanceModel>().ToList();
                    objModel.FeeCategries = multi.Read<FeeCategoryModel>().ToList();
                    objModel.FeeDetails = multi.Read<FeeDetailsModel>().ToList();
                    objModel.DailyFMCollection = multi.Read<PaymentModeModel>().ToList();
                    objModel.MonthlyFMCollection = multi.Read<PaymentModeModel>().ToList();
                    objModel.QuarterlyCollection = multi.Read<PaymentModeModel>().ToList();
                    objModel.AnnualCollection = multi.Read<PaymentModeModel>().ToList();
                    try
                    {
                        objModel.DailyFTCollection = multi.Read<NameIDModel>().ToList();
                        objModel.BirthDays = multi.Read<StudentBirthdaySMSModel>().ToList();
                        objModel.SessionName = multi.Read<string>().SingleOrDefault();
                    }
                    catch
                    {
                        objModel.DailyFTCollection = new List<NameIDModel>();
                        objModel.BirthDays = new List<StudentBirthdaySMSModel>();
                    }
                }
            }
            return objModel;
        }
        #endregion
        #region Branch Management
        public IEnumerable<SBranchModel> GetAdminBranches(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<SBranchModel>("sp_GetAdminSBranches", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int InsertUpdateAdminBranch(SBranchModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@BranchName", objData.BranchName);
                paramater.Add("@Logo", objData.Logo);
                paramater.Add("@ContactNo", objData.ContactNo);
                paramater.Add("@EmailID", objData.EmailID);
                paramater.Add("@Address", objData.Address);
                paramater.Add("@PrincipalName", objData.PrincipalName);
                paramater.Add("@PrincipalMobile", objData.PrincipalMobile);
                paramater.Add("@PrincipalEmail", objData.PrincipalEmail);
                paramater.Add("@BranchSchoolName", objData.BranchSchoolName);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@StateID", objData.StateID);
                paramater.Add("@PrincipalSignature", objData.PrincipalSignature);
                paramater.Add("@NotificationServerKey", objData.NotificationServerKey);
                paramater.Add("@PlayStoreLink", objData.PlayStoreLink);
                paramater.Add("@UDISECode", objData.UDISECode);
                paramater.Add("@SchoolCode", objData.SchoolCode);
                return con.Query<int>("spn_InsertUpdateSBranch", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public int CheckUserNameExist(string UserName, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@UserName", UserName);
                paramater.Add("@SBranchID", SBranchID);

                return con.Query<int>("sp_CheckUserNameExist", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #endregion
        #region SMSReciever List
        public List<SMSRecieverModel> GetSMSRecieverList(int SBranchID, string Recievers, int RecieverType)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@Recievers", Recievers);
                paramater.Add("@RecieverType", RecieverType);
                return con.Query<SMSRecieverModel>("spn_GetRecieverListForSMS", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int UpdateSenderSMSSentStatus(int SMSType, int ID, string AssociatedIDs, int RecieverType)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SMSType", SMSType);
                paramater.Add("@ID", ID);
                paramater.Add("@RecieverType", RecieverType);
                paramater.Add("@AssociatedIDs", AssociatedIDs);

                return con.Query<int>("spn_UpdateSenderSMSSentStatus", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #endregion
        #region Location
        public IEnumerable<LocationModel> InsertUpdateLocation(LocationModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", objData.ID);
                paramater.Add("@Type", objData.Type);
                paramater.Add("@Name", objData.Name);
                paramater.Add("@Other", objData.Other);
                paramater.Add("@MasterID", objData.MasterID);
                paramater.Add("@IsApproved", objData.IsApproved);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@SBranchID", objData.SBranchID);
                return con.Query<LocationModel>("spn_InsertUpdateAdminLocation", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public IEnumerable<LocationModel> GetLocations(int ID, int Type, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@MasterID", ID);
                paramater.Add("@Type", Type);
                return con.Query<LocationModel>("sp_GetAdminLocationList", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        #endregion
        #region Session Management
        public IEnumerable<SchoolSessionModel> GetSessions(int Status, int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@Status", Status);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<SchoolSessionModel>("spn_GetSessions", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdateSessoin(SchoolSessionModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SessionID", objData.SessionID);
                paramater.Add("@SessionStartDate", objData.SessionStartDate);
                paramater.Add("@SessionEndDate", objData.SessionEndDate);
                paramater.Add("@SessionStatus", objData.SessionStatus);
                paramater.Add("@SessionName", objData.SessionName);
                paramater.Add("@SBranchID", objData.SBranchID);

                return con.Query<int>("spn_InsertUpdateSession", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #endregion
        #region News Management
        public IEnumerable<NewsModel> GetNews()
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                return con.Query<NewsModel>("sp_GetNewsList", null, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdateNews(NewsModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@NewsID", objData.NewsID);
                paramater.Add("@Title", objData.Title);
                paramater.Add("@Description", objData.Description);
                paramater.Add("@ActiveDate", objData.ActiveDate);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@Attachment", objData.Attachment);

                return con.Query<int>("sp_InsertUpdateNews", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public IEnumerable<GalleryModel> GetGallery()
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                return con.Query<GalleryModel>("sp_GetGalleryList", null, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public List<GalleryModel> GetAppGalleryList(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<GalleryModel>("sp_GetAppGalleryList", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public GalleryModel GetGalleryDetails(int GalleryID)
        {
            GalleryModel objModel = new GalleryModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@GalleryID", GalleryID);
                using (var multi = con.QueryMultiple("sp_GetGalleryDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<GalleryModel>().SingleOrDefault();
                    if (objModel == null)
                    {
                        objModel = new GalleryModel();
                        objModel.EventDate = CommonUsage.GetCurrentDate();
                    }
                    objModel.Images = multi.Read<GalleryImageModel>().ToList();
                }
            }
            return objModel;
        }
        public GalleryModel GetAppGalleryDetails(int GalleryID)
        {
            GalleryModel objModel = new GalleryModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@GalleryID", GalleryID);
                using (var multi = con.QueryMultiple("sp_GetAppGalleryDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel = multi.Read<GalleryModel>().SingleOrDefault();

                    objModel.Images = multi.Read<GalleryImageModel>().ToList();
                }
            }
            return objModel;
        }
        public int InsertUpdateGallery(GalleryModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@GalleryID", objData.GalleryID);
                paramater.Add("@Title", objData.Title);
                paramater.Add("@Description", objData.Description);
                paramater.Add("@EventDate", objData.EventDate);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@OpType", objData.OpType);
                paramater.Add("@Images", objData.GetImages());
                paramater.Add("@SbranchID", objData.SBranchID);

                return con.Query<int>("spn_InsertUpdateGallery", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public IEnumerable<NameIDModel> GetThoughts()
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                return con.Query<NameIDModel>("sp_GetAdminThouhgts", null, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdateThought(NameIDModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ThoughtID", objData.ID);
                paramater.Add("@Thought", objData.Name);
                paramater.Add("@OpType", objData.Extra1);
                return con.Query<int>("spn_InsertUpdateThought", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public IEnumerable<BannerImageModel> GetHomeBanners(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<BannerImageModel>("sp_GetAllWebBanner", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdateWebBanner(BannerImageModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ABID", objData.ABID);
                paramater.Add("@BannerDate", objData.BannerDate);
                paramater.Add("@Title", objData.Title);
                paramater.Add("@BImage", objData.BImage);
                paramater.Add("@Status", objData.Status);
                paramater.Add("@SBranchID", objData.SBranchID);
                paramater.Add("@OpType", objData.OpType);
                return con.Query<int>("sp_UpdateAppBanner", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #endregion
        #region Request Section
        public List<FeeDiscountRequestMaster> GetFeeDiscountRequest(int SBranchID, int Status)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@Status", Status);

                return con.Query<FeeDiscountRequestMaster>("sp_GetAdminFeeDiscountRequests", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        public FeeDiscountRequestMaster GetFeeDiscountRequestDetails(string RequestID)
        {
            FeeDiscountRequestMaster objNew;
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@RequestID", RequestID);
                using (var multi = con.QueryMultiple("sp_GetAdminFeeDiscountRequestDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objNew = multi.Read<FeeDiscountRequestMaster>().SingleOrDefault();
                    if (objNew == null)
                    {
                        objNew = new FeeDiscountRequestMaster();
                    }
                    objNew.Details = multi.Read<FeeDiscountRequestDetails>().ToList();
                    objNew.StudentName = multi.Read<string>().SingleOrDefault();
                }
            }

            return objNew;
        }
        public int UpdateFeeDiscountRequest(FeeDiscountRequestMaster objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();

                paramater.Add("@DiscRequestID", objData.DiscRequestID);
                paramater.Add("@DirectorRemark", objData.DirectorRemark);
                paramater.Add("@Status ", objData.Status);
                paramater.Add("@Details", objData.GetDiscountDetailsDataTable());

                return con.Query<int>("sp_AdminUpdateFeeDiscountReq", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #endregion
        #region Report Section
        public SectionClassTeacherReportPageModel GetSectionClassTeacherReport(int SBranchID)
        {
            SectionClassTeacherReportPageModel oModel = new SectionClassTeacherReportPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("spr_GetSectionClassTeacherReport", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Report = multi.Read<SectionClassTeacherReportModel>().ToList();
                    oModel.Teachers = multi.Read<NameIDModel>().ToList();
                }
            }
            return oModel;
        }
        public int UpdateSectionClassTeacher(SectionClassTeacherReportModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", objData.SectionID);
                paramater.Add("@TeacherID", objData.TeacherID);
                return con.Query<int>("sp_UpdateSectionClassTeacher", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public SessionStrengthReport GetSessionStrengthReport(int SBranchID)
        {

            SessionStrengthReport objModel = new SessionStrengthReport();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetClassSessionStrengthReport", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Classes = multi.Read<ClassModel>().ToList();
                    objModel.Sessions = multi.Read<SchoolSessionModel>().ToList();
                    objModel.Strength = multi.Read<SessionStrengthModel>().ToList();
                }
            }
            return objModel;
        }

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


        #endregion
        #region Backup
        public int TakeDatabaseBackup(string backupDir)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {

                return con.Query<int>("backup database " + con.Database + " to disk='" + backupDir + "\\" + DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".Bak'", null, null, true, 0, commandType: CommandType.Text).SingleOrDefault();

            }
        }
        public SMEnterpriseDB.DBModels.BackupMasterModel TakeBackup()
        {
            SMEnterpriseDB.DBModels.BackupMasterModel objModel = new SMEnterpriseDB.DBModels.BackupMasterModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                using (var multi = con.QueryMultiple("sp_GetDatabaseBackup", null, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.PTransportHostalAllocationDelocation = multi.Read<SMEnterpriseDB.Models.TransportHostalAllocationDelocation>().ToList();
                    objModel.PEmployees = multi.Read<SMEnterpriseDB.Models.Employees>().ToList();
                    objModel.PGalleryMaster = multi.Read<SMEnterpriseDB.Models.GalleryMaster>().ToList();
                    objModel.PBookMaster = multi.Read<SMEnterpriseDB.Models.BookMaster>().ToList();
                    objModel.PEmployeeSalaryDetails = multi.Read<SMEnterpriseDB.Models.EmployeeSalaryDetails>().ToList();
                    objModel.PLibraryIssueBooks = multi.Read<SMEnterpriseDB.Models.LibraryIssueBooks>().ToList();
                    objModel.PGalleryImages = multi.Read<SMEnterpriseDB.Models.GalleryImages>().ToList();
                    objModel.PEmployeesBio = multi.Read<SMEnterpriseDB.Models.EmployeesBio>().ToList();
                    objModel.PEmployeeTypeMaster = multi.Read<SMEnterpriseDB.Models.EmployeeTypeMaster>().ToList();
                    objModel.PEmployeeTypeSalaryDetails = multi.Read<SMEnterpriseDB.Models.EmployeeTypeSalaryDetails>().ToList();
                    objModel.PSessionMaster = multi.Read<SMEnterpriseDB.Models.SessionMaster>().ToList();
                    objModel.PEvaluationDetails = multi.Read<SMEnterpriseDB.Models.EvaluationDetails>().ToList();
                    objModel.PSMSFailureLog = multi.Read<SMEnterpriseDB.Models.SMSFailureLog>().ToList();
                    objModel.PEvaluationMaster = multi.Read<SMEnterpriseDB.Models.EvaluationMaster>().ToList();
                    objModel.PEventMaster = multi.Read<SMEnterpriseDB.Models.EventMaster>().ToList();
                    objModel.PEventTypeMaster = multi.Read<SMEnterpriseDB.Models.EventTypeMaster>().ToList();
                    objModel.PExamMaster = multi.Read<SMEnterpriseDB.Models.ExamMaster>().ToList();
                    objModel.PExamResultMaster = multi.Read<SMEnterpriseDB.Models.ExamResultMaster>().ToList();
                    objModel.PNotificationMaster = multi.Read<SMEnterpriseDB.Models.NotificationMaster>().ToList();
                    objModel.PExpenceTypeMaster = multi.Read<SMEnterpriseDB.Models.ExpenceTypeMaster>().ToList();
                    objModel.PNotificationRecievers = multi.Read<SMEnterpriseDB.Models.NotificationRecievers>().ToList();
                    objModel.PFeeApplicableMaster = multi.Read<SMEnterpriseDB.Models.FeeApplicableMaster>().ToList();
                    objModel.PFeeTypeMaster = multi.Read<SMEnterpriseDB.Models.FeeTypeMaster>().ToList();
                    objModel.PFloorMaster = multi.Read<SMEnterpriseDB.Models.FloorMaster>().ToList();
                    objModel.PGroup_Subject = multi.Read<SMEnterpriseDB.Models.Group_Subject>().ToList();
                    objModel.PAppUsers = multi.Read<SMEnterpriseDB.Models.AppUsers>().ToList();
                    objModel.PGroupMaster = multi.Read<SMEnterpriseDB.Models.GroupMaster>().ToList();
                    objModel.PGroupTypeMaster = multi.Read<SMEnterpriseDB.Models.GroupTypeMaster>().ToList();
                    objModel.PHoliday_Classes = multi.Read<SMEnterpriseDB.Models.Holiday_Classes>().ToList();
                    objModel.PHolidayMaster = multi.Read<SMEnterpriseDB.Models.HolidayMaster>().ToList();
                    objModel.PHostel_FloorMaster = multi.Read<SMEnterpriseDB.Models.Hostel_FloorMaster>().ToList();
                    objModel.PHostel_HostelTypeMaster = multi.Read<SMEnterpriseDB.Models.Hostel_HostelTypeMaster>().ToList();
                    objModel.PHostel_RoomMaster = multi.Read<SMEnterpriseDB.Models.Hostel_RoomMaster>().ToList();
                    objModel.PHostel_RoomTypeMaster = multi.Read<SMEnterpriseDB.Models.Hostel_RoomTypeMaster>().ToList();
                    objModel.PHostel_RoomTypeRateMaster = multi.Read<SMEnterpriseDB.Models.Hostel_RoomTypeRateMaster>().ToList();
                    objModel.PHostelMaster = multi.Read<SMEnterpriseDB.Models.HostelMaster>().ToList();
                    objModel.PHouseMaster = multi.Read<SMEnterpriseDB.Models.HouseMaster>().ToList();
                    objModel.PLeaveMaster = multi.Read<SMEnterpriseDB.Models.LeaveMaster>().ToList();
                    objModel.PLeaveTypeMaster = multi.Read<SMEnterpriseDB.Models.LeaveTypeMaster>().ToList();
                    objModel.PLoginDetails = multi.Read<SMEnterpriseDB.Models.LoginDetails>().ToList();
                    objModel.PBookCategory = multi.Read<SMEnterpriseDB.Models.BookCategory>().ToList();
                    objModel.PMailMaster = multi.Read<SMEnterpriseDB.Models.MailMaster>().ToList();
                    objModel.PMailProcessingStatus = multi.Read<SMEnterpriseDB.Models.MailProcessingStatus>().ToList();
                    objModel.PMailTemplates = multi.Read<SMEnterpriseDB.Models.MailTemplates>().ToList();
                    objModel.PMasterSettings = multi.Read<SMEnterpriseDB.Models.MasterSettings>().ToList();
                    objModel.PMessageMaster = multi.Read<SMEnterpriseDB.Models.MessageMaster>().ToList();
                    objModel.PNews = multi.Read<SMEnterpriseDB.Models.News>().ToList();
                    objModel.PNoticeBoard = multi.Read<SMEnterpriseDB.Models.NoticeBoard>().ToList();
                    objModel.PExpenceMaster = multi.Read<SMEnterpriseDB.Models.ExpenceMaster>().ToList();
                    objModel.PExpenceDetails = multi.Read<SMEnterpriseDB.Models.ExpenceDetails>().ToList();
                    objModel.PParallelAttLogExportDetails = multi.Read<SMEnterpriseDB.Models.ParallelAttLogExportDetails>().ToList();
                    objModel.PParallelDatabaseDetails = multi.Read<SMEnterpriseDB.Models.ParallelDatabaseDetails>().ToList();
                    objModel.PParentMaster = multi.Read<SMEnterpriseDB.Models.ParentMaster>().ToList();
                    objModel.PPaymentDetails = multi.Read<SMEnterpriseDB.Models.PaymentDetails>().ToList();
                    objModel.PPaymentMaster = multi.Read<SMEnterpriseDB.Models.PaymentMaster>().ToList();
                    objModel.PPerformanceParameterMaster = multi.Read<SMEnterpriseDB.Models.PerformanceParameterMaster>().ToList();
                    objModel.PPerformanceParameterValues = multi.Read<SMEnterpriseDB.Models.PerformanceParameterValues>().ToList();
                    objModel.PThoughtMaster = multi.Read<SMEnterpriseDB.Models.ThoughtMaster>().ToList();
                    objModel.PPeriodMaster = multi.Read<SMEnterpriseDB.Models.PeriodMaster>().ToList();
                    objModel.PPeriodTypeMaster = multi.Read<SMEnterpriseDB.Models.PeriodTypeMaster>().ToList();
                    objModel.PPermissions = multi.Read<SMEnterpriseDB.Models.Permissions>().ToList();
                    objModel.PProductMaster = multi.Read<SMEnterpriseDB.Models.ProductMaster>().ToList();
                    objModel.PQuestionBankMaster = multi.Read<SMEnterpriseDB.Models.QuestionBankMaster>().ToList();
                    objModel.PQuotaDiscountDetails = multi.Read<SMEnterpriseDB.Models.QuotaDiscountDetails>().ToList();
                    objModel.PQuotaMaster = multi.Read<SMEnterpriseDB.Models.QuotaMaster>().ToList();
                    objModel.PRoomMaster = multi.Read<SMEnterpriseDB.Models.RoomMaster>().ToList();
                    objModel.PRoomSizeMaster = multi.Read<SMEnterpriseDB.Models.RoomSizeMaster>().ToList();
                    objModel.PSalaryTypeMaster = multi.Read<SMEnterpriseDB.Models.SalaryTypeMaster>().ToList();
                    objModel.PSBranchMaster = multi.Read<SMEnterpriseDB.Models.SBranchMaster>().ToList();
                    objModel.PSchoolDetails = multi.Read<SMEnterpriseDB.Models.SchoolDetails>().ToList();
                    objModel.PSMSCountDetails = multi.Read<SMEnterpriseDB.Models.SMSCountDetails>().ToList();
                    objModel.PStateMaster = multi.Read<SMEnterpriseDB.Models.StateMaster>().ToList();
                    objModel.PEvaluationTypes = multi.Read<SMEnterpriseDB.Models.EvaluationTypes>().ToList();
                    objModel.PStudent_Session = multi.Read<SMEnterpriseDB.Models.Student_Session>().ToList();
                    objModel.PStudent_Session_OptionalSubjects = multi.Read<SMEnterpriseDB.Models.Student_Session_OptionalSubjects>().ToList();
                    objModel.PStudentAttendanceMasterT = multi.Read<SMEnterpriseDB.Models.StudentAttendanceMasterT>().ToList();
                    objModel.PStudentMaster = multi.Read<SMEnterpriseDB.Models.StudentMaster>().ToList();
                    objModel.PSubjectMasterT = multi.Read<SMEnterpriseDB.Models.SubjectMasterT>().ToList();
                    objModel.PEvaluationSchemeMaster = multi.Read<SMEnterpriseDB.Models.EvaluationSchemeMaster>().ToList();
                    objModel.PSyllabusScheduleStatus = multi.Read<SMEnterpriseDB.Models.SyllabusScheduleStatus>().ToList();
                    objModel.PTeacher_Subject = multi.Read<SMEnterpriseDB.Models.Teacher_Subject>().ToList();
                    objModel.PTeacherSubstitutionMaster = multi.Read<SMEnterpriseDB.Models.TeacherSubstitutionMaster>().ToList();
                    objModel.PTime_Table_Master = multi.Read<SMEnterpriseDB.Models.Time_Table_Master>().ToList();
                    objModel.PTopicMaster = multi.Read<SMEnterpriseDB.Models.TopicMaster>().ToList();
                    objModel.PClassSessionDetails = multi.Read<SMEnterpriseDB.Models.ClassSessionDetails>().ToList();
                    objModel.PTransport_Vehicle_Route = multi.Read<SMEnterpriseDB.Models.Transport_Vehicle_Route>().ToList();
                    objModel.PTransport_Vehicle_Route_Details = multi.Read<SMEnterpriseDB.Models.Transport_Vehicle_Route_Details>().ToList();
                    objModel.PTransportRouteDetails = multi.Read<SMEnterpriseDB.Models.TransportRouteDetails>().ToList();
                    objModel.PTransportRouteMaster = multi.Read<SMEnterpriseDB.Models.TransportRouteMaster>().ToList();
                    objModel.PUser_Details = multi.Read<SMEnterpriseDB.Models.User_Details>().ToList();
                    objModel.PUserPermissions = multi.Read<SMEnterpriseDB.Models.UserPermissions>().ToList();
                    objModel.PUsers = multi.Read<SMEnterpriseDB.Models.Users>().ToList();
                    objModel.PVehicleDetails = multi.Read<SMEnterpriseDB.Models.VehicleDetails>().ToList();
                    objModel.P_MasterSettings = multi.Read<SMEnterpriseDB.Models.C_MasterSettings>().ToList();
                    objModel.PVerificationMode = multi.Read<SMEnterpriseDB.Models.VerificationMode>().ToList();
                    objModel.PAdvancePaymentDeductions = multi.Read<SMEnterpriseDB.Models.AdvancePaymentDeductions>().ToList();
                    objModel.PBookCopyDetail = multi.Read<SMEnterpriseDB.Models.BookCopyDetail>().ToList();
                    objModel.PAdvancePaymentMaster = multi.Read<SMEnterpriseDB.Models.AdvancePaymentMaster>().ToList();
                    objModel.PAreaMaster = multi.Read<SMEnterpriseDB.Models.AreaMaster>().ToList();
                    objModel.PAssignmentMaster = multi.Read<SMEnterpriseDB.Models.AssignmentMaster>().ToList();
                    objModel.PAssignmentSubmissions = multi.Read<SMEnterpriseDB.Models.AssignmentSubmissions>().ToList();
                    objModel.PPaymentModeMaster = multi.Read<SMEnterpriseDB.Models.PaymentModeMaster>().ToList();
                    objModel.PAttandanceDeviceDetails = multi.Read<SMEnterpriseDB.Models.AttandanceDeviceDetails>().ToList();
                    objModel.PAttendanceStates = multi.Read<SMEnterpriseDB.Models.AttendanceStates>().ToList();
                    objModel.PBuildingMaster = multi.Read<SMEnterpriseDB.Models.BuildingMaster>().ToList();
                    objModel.PParentDiary = multi.Read<SMEnterpriseDB.Models.ParentDiary>().ToList();
                    objModel.PChapterMaster = multi.Read<SMEnterpriseDB.Models.ChapterMaster>().ToList();
                    objModel.PSMSTypeMaster = multi.Read<SMEnterpriseDB.Models.SMSTypeMaster>().ToList();
                    objModel.PFeeDiscountRequestMaster = multi.Read<SMEnterpriseDB.Models.FeeDiscountRequestMaster>().ToList();
                    objModel.PSMSBalanceMaster = multi.Read<SMEnterpriseDB.Models.SMSBalanceMaster>().ToList();
                    objModel.PFeeDiscountRequestDetails = multi.Read<SMEnterpriseDB.Models.FeeDiscountRequestDetails>().ToList();
                    objModel.PCityMaster = multi.Read<SMEnterpriseDB.Models.CityMaster>().ToList();
                    objModel.PSMSTemplateMaster = multi.Read<SMEnterpriseDB.Models.SMSTemplateMaster>().ToList();
                    objModel.PClass_Merge_Master = multi.Read<SMEnterpriseDB.Models.Class_Merge_Master>().ToList();
                    objModel.PClass_Sections = multi.Read<SMEnterpriseDB.Models.Class_Sections>().ToList();
                    objModel.PClassFeeStructureMaster = multi.Read<SMEnterpriseDB.Models.ClassFeeStructureMaster>().ToList();
                    objModel.PClassMaster = multi.Read<SMEnterpriseDB.Models.ClassMaster>().ToList();
                    objModel.PSMSSendingDetails = multi.Read<SMEnterpriseDB.Models.SMSSendingDetails>().ToList();
                    objModel.PLibraryIssueRegister = multi.Read<SMEnterpriseDB.Models.LibraryIssueRegister>().ToList();
                    objModel.PCountryMaster = multi.Read<SMEnterpriseDB.Models.CountryMaster>().ToList();
                    objModel.PSMSProcessingLog = multi.Read<SMEnterpriseDB.Models.SMSProcessingLog>().ToList();
                    objModel.PSMSSendingMaster = multi.Read<SMEnterpriseDB.Models.SMSSendingMaster>().ToList();
                    objModel.PExpenceBillImages = multi.Read<SMEnterpriseDB.Models.ExpenceBillImages>().ToList();
                    objModel.PSubSubjectTypes = multi.Read<SMEnterpriseDB.Models.SubSubjectTypes>().ToList();
                    objModel.PDevices = multi.Read<SMEnterpriseDB.Models.Devices>().ToList();
                    objModel.PDevicesStatus = multi.Read<SMEnterpriseDB.Models.DevicesStatus>().ToList();
                    objModel.PDriverConductorDetails = multi.Read<SMEnterpriseDB.Models.DriverConductorDetails>().ToList();
                    objModel.PEducationLevelMaster = multi.Read<SMEnterpriseDB.Models.EducationLevelMaster>().ToList();
                    objModel.PEmployeeAttendanceMasterT = multi.Read<SMEnterpriseDB.Models.EmployeeAttendanceMasterT>().ToList();
                    objModel.PEmployeeEducationDetails = multi.Read<SMEnterpriseDB.Models.EmployeeEducationDetails>().ToList();
                    objModel.PEmployeeExperienceDetails = multi.Read<SMEnterpriseDB.Models.EmployeeExperienceDetails>().ToList();
                    objModel.PNewsMaster = multi.Read<SMEnterpriseDB.Models.NewsMaster>().ToList();
                    objModel.PEmployeeLeaveTypeMaster = multi.Read<SMEnterpriseDB.Models.EmployeeLeaveTypeMaster>().ToList();
                    objModel.PLibraryMaster = multi.Read<SMEnterpriseDB.Models.LibraryMaster>().ToList();
                    objModel.PEmployeeMaster = multi.Read<SMEnterpriseDB.Models.EmployeeMaster>().ToList();
                    objModel.PAdmissionEnquiryMaster = multi.Read<SMEnterpriseDB.Models.AdmissionEnquiryMaster>().ToList();
                    objModel.PAdmissionEnquiryFollowups = multi.Read<SMEnterpriseDB.Models.AdmissionEnquiryFollowups>().ToList();
                }

            }
            return objModel;
        }
        #endregion

        public async Task<List<ImportantContactModel>> GetImportantContact(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return (await con.QueryAsync<ImportantContactModel>("spn_GetImportantContacts", paramater, null, 0, commandType: CommandType.StoredProcedure)).ToList();

            }
        }
        public int UpdateImportantContacts(ImportantContactModel Contact)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", Contact.SBranchID);
                paramater.Add("@Name", Contact.Name);
                paramater.Add("@Contact", Contact.Contact);
                paramater.Add("@ICID", Contact.ICID);
                paramater.Add("@OpType", Contact.OpType);
                return con.Query<int>("spn_UpdateImportantContacts", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        #region InventoryModel
        public List<ProductCategoryModel> GetProductCategories(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<ProductCategoryModel>("sp_GetProductCategories", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        public int UpdateProductCategory(ProductCategoryModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", oModel.ID);
                paramater.Add("@Name", oModel.Name);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@HSNCode", oModel.HSNCode);
                paramater.Add("@SGST", oModel.SGST);
                paramater.Add("@CGST", oModel.CGST);
                paramater.Add("@IGST", oModel.IGST);
                paramater.Add("@OpType", oModel.OpType);
                return con.Query<int>("sp_UpdateProductCategory", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public int DeleteProduct(int ProductID)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ProductID", ProductID);
                return con.Query<int>("sp_DeleteProduct", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        public ProductsPageModel GetProducts(ProductsPageModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@CategoryID", oModel.CategoryID);
                using (var multi = con.QueryMultiple("sp_GetProducts", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Products = multi.Read<ProductModel>().ToList();
                    oModel.Categories = multi.Read<ProductCategoryModel>().ToList();
                    oModel.CategoryID = multi.Read<int>().SingleOrDefault();
                }
            }
            return oModel;
        }
        public ProductsPageModel GetProductDetails(int ProductID, int SBranchID)
        {
            ProductsPageModel oModel = new ProductsPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ProductID", ProductID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetProductDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Product = multi.Read<ProductModel>().SingleOrDefault();
                    oModel.Categories = multi.Read<ProductCategoryModel>().ToList();
                }
            }
            return oModel;
        }
        public int UpdateProduct(ProductModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ProductID", oModel.ProductID);
                paramater.Add("@ProductCategoryID", oModel.ProductCategoryID);
                paramater.Add("@Name", oModel.Name);
                paramater.Add("@Photo", oModel.Photo);
                paramater.Add("@MRP", oModel.MRP);
                paramater.Add("@Price", oModel.Price);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@Status", oModel.Status);
                paramater.Add("@Quantity", oModel.Quantity);
                paramater.Add("@MinQty", oModel.MinQty);
                paramater.Add("@OpType", oModel.OpType);
                return con.Query<int>("sp_UpdateProduct", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public List<ProductModel> GetLowStockProducts(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<ProductModel>("sp_GetLowStockProducts", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public List<ProductModel> GetAvailableStockProducts(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<ProductModel>("sp_GetAvailableStockProducts", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        #endregion


        public void GetVehicleLogs(VehicleLogPageModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@StartDate", oModel.StartDate);
                paramater.Add("@EndDate", oModel.EndDate);
                using (var multi = con.QueryMultiple("sp_GetVehicleLogs", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Logs = multi.Read<VehicleLogModel>().ToList();
                    oModel.LogTypes = multi.Read<NameIDModel>().ToList();
                    oModel.Vehicles = multi.Read<VehicleModel>().ToList();
                }
            }
        }

        public int UpdateVehicleLog(VehicleLogModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@VLogID", oModel.VLogID);
                paramater.Add("@VehicleID", oModel.VehicleID);
                paramater.Add("@LogType", oModel.LogType);
                paramater.Add("@LogTitle", oModel.LogTitle);
                paramater.Add("@LogDate", oModel.LogDate);
                paramater.Add("@CreatedDate", oModel.CreatedDate);
                paramater.Add("@Expance", oModel.Expance);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@OpType", oModel.OpType);
                return con.Query<int>("sp_UpdateVehicleLog", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        #region TC Related

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
                return con.Query<int>("sp_InsertUpdateTCDetails", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
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
                }
            }
        }

        #endregion
        #region Assignments
        public AssignmentPageModel GetAssignments(AssignmentPageModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@SType", objModel.SType);
                paramater.Add("@FromDate", objModel.FromDate);
                paramater.Add("@EndDate", objModel.EndDate);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_GetAdminAssignmentList", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    objModel.Assignments = multi.Read<AssignmentModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.Teachers = multi.Read<NameIDModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.TeacherID = multi.Read<int>().SingleOrDefault();
                }
            }
            return objModel;
        }
        #endregion
        #region BlackBoard related
        public BlackBoardAdminPageModel GetBlackBoardPageData(BlackBoardAdminPageModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", oModel.ClassID);
                paramater.Add("@SectionID", oModel.SectionID);
                paramater.Add("@SessionID", oModel.SessionID);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@SubjectID", oModel.SubjectID);
                paramater.Add("@TeacherID", oModel.TeacherID);
                paramater.Add("@BBDate", oModel.BBDate);
                using (var multi = con.QueryMultiple("sp_GetAdminBlackBoardPageData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.BlackBoards = multi.Read<BlackBoardModel>().ToList();
                    oModel.Classes = multi.Read<NameIDModel>().ToList();
                    oModel.Sections = multi.Read<NameIDModel>().ToList();
                    oModel.Sessions = multi.Read<NameIDModel>().ToList();
                    oModel.Teachers = multi.Read<NameIDModel>().ToList();
                    oModel.ClassID = multi.Read<int>().SingleOrDefault();
                    oModel.SectionID = multi.Read<int>().SingleOrDefault();
                    oModel.SessionID = multi.Read<int>().SingleOrDefault();
                    oModel.TeacherID = multi.Read<int>().SingleOrDefault();

                }
            }
            return oModel;
        }
        public List<BlackBoardImageModel> GetBlackBoardImages(string BlackBoardID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@BlackBoardID", BlackBoardID);
                return con.Query<BlackBoardImageModel>("sp_GetAdminBlackBoardImages", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }
        #endregion
        #region YouTube Videos
        public YouTubeAdminPageModel GetYouTubeVideos(YouTubeAdminPageModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", oModel.TeacherID);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@UploadDate", oModel.UploadDate);
                using (var multi = con.QueryMultiple("sp_GetAdminYouTubeVideo", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Videos = multi.Read<YouTubeVideoModel>().ToList();
                    oModel.Teachers = multi.Read<NameIDModel>().ToList();
                }
            }
            return oModel;
        }
        public YoutubeAdminEditPageData GetYouTubeVideosDetails(int SBranchID, int VideoID)
        {
            YoutubeAdminEditPageData oModel = new YoutubeAdminEditPageData();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@VideoID", VideoID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetAdminYouTubeVideoEditData", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Video = multi.Read<YouTubeVideoModel>().SingleOrDefault();
                    if (oModel.Video == null)
                    {
                        oModel.Video = new YouTubeVideoModel();
                    }
                    oModel.Subjects = multi.Read<NameIDModel>().ToList();
                    oModel.Classes = multi.Read<NameIDModel>().ToList();
                    oModel.Sections = multi.Read<NameIDModel>().ToList();
                    oModel.Sessions = multi.Read<NameIDModel>().ToList();
                    oModel.Students = multi.Read<NameIDModel>().ToList();

                    oModel.SessionID = multi.Read<int>().SingleOrDefault();
                    oModel.ClassID = multi.Read<int>().SingleOrDefault();
                    oModel.SectionID = multi.Read<int>().SingleOrDefault();
                    oModel.SubjectID = multi.Read<int>().SingleOrDefault();
                    oModel.TeacherID = multi.Read<int>().SingleOrDefault();
                    oModel.TeacherName = multi.Read<string>().SingleOrDefault();
                }
            }
            return oModel;
        }

        public int UpdateYoutubeVideo(YoutubeAdminEditPageData oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@VideoID", oModel.Video.VideoID);
                paramater.Add("@TeacherID", oModel.Video.TeacherID);
                paramater.Add("@UploadDate", oModel.Video.UploadDate);
                paramater.Add("@SubjectID", oModel.Video.SubjectID);
                paramater.Add("@Title", oModel.Video.Title);
                paramater.Add("@Description", oModel.Video.Description);
                paramater.Add("@YoutubeID", oModel.Video.YouTubeID);
                paramater.Add("@ClassID", oModel.Video.ClassID);
                paramater.Add("@SectionID", oModel.Video.SectionID);
                paramater.Add("@SequenceNo", oModel.Video.SequenceNo);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@VideoDate", oModel.Video.VideoDate);
                paramater.Add("@SessionID", oModel.Video.SessionID);
                paramater.Add("@IsSpecificStudent", oModel.Video.IsSpecificStudent);
                paramater.Add("@Students", oModel.GetStudentsDataTable());
                paramater.Add("@OpType", oModel.OpType);
                return con.Query<int>("sp_UpdateAdminYouTubeVideo", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
        public YoutubeAdminEditPageData GetSectionSubjectAndStudents(int SectionID, int SessionID, int VideoID)
        {
            YoutubeAdminEditPageData oModel = new YoutubeAdminEditPageData();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SectionID", SectionID);
                paramater.Add("@SessionID", SessionID);
                paramater.Add("@VideoID", VideoID);
                using (var multi = con.QueryMultiple("sp_GetSectionSubjectsAndStudents", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {

                    oModel.Subjects = multi.Read<NameIDModel>().ToList();
                    oModel.Students = multi.Read<NameIDModel>().ToList();

                    oModel.SubjectID = multi.Read<int>().SingleOrDefault();
                }
            }
            return oModel;
        }
        #endregion
        public ParentAppSMSPageModel GetParentsForLoginSMS(int SessionID, int SBranchID)
        {
            ParentAppSMSPageModel oModel = new ParentAppSMSPageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SessionID", SessionID);
                paramater.Add("@SBranchID", SBranchID);
                using (var multi = con.QueryMultiple("sp_GetParentListForAppSMS", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.Sessions = multi.Read<NameIDModel>().ToList();
                    oModel.Parents = multi.Read<ParentModel>().ToList();
                    oModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return oModel;
        }
        public string GetPlayStoreLink(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<string>("sp_GetPlayStoreLink", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public string UpdateIndividualPassword(int SBranchID, string UserName, string Password, int UserID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@UserID", UserID);
                paramater.Add("@UserName", UserName);
                paramater.Add("@Password", Password);
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<string>("sp_UpdateIndividualPassword", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        #region Religion,Nationality,Mother Tongue, Social Category
        public IEnumerable<NameIDModel> GetNationalities(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<NameIDModel>("sp_GetNationalities", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdateNationality(NameIDModel oData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", oData.ID);
                paramater.Add("@Name", oData.Name);
                paramater.Add("@SBranchID", oData.SBranchID);
                paramater.Add("@OpType", oData.OpType);

                return con.Query<int>("sp_InsertUpdateNationality", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public IEnumerable<NameIDModel> GetReligions(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<NameIDModel>("sp_GetReligions", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdateReligion(NameIDModel oData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", oData.ID);
                paramater.Add("@Name", oData.Name);
                paramater.Add("@SBranchID", oData.SBranchID);
                paramater.Add("@OpType", oData.OpType);

                return con.Query<int>("sp_InsertUpdateReligion", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public IEnumerable<NameIDModel> GetMotherTongues(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<NameIDModel>("sp_GetMotherTongues", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdateMotherTongue(NameIDModel oData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", oData.ID);
                paramater.Add("@Name", oData.Name);
                paramater.Add("@SBranchID", oData.SBranchID);
                paramater.Add("@OpType", oData.OpType);

                return con.Query<int>("sp_InsertUpdateMotherTongue", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public IEnumerable<NameIDModel> GetSocialCategories(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<NameIDModel>("sp_GetSocialCategories", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdateSocialCategory(NameIDModel oData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", oData.ID);
                paramater.Add("@Name", oData.Name);
                paramater.Add("@SBranchID", oData.SBranchID);
                paramater.Add("@OpType", oData.OpType);

                return con.Query<int>("sp_InsertUpdateSocialCategory", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        public IEnumerable<NameIDModel> GetSocialSubCategories(int SBranchID, int MainCategoryID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                paramater.Add("@MainCategoryID", MainCategoryID);
                return con.Query<NameIDModel>("sp_GetSocialSubCategories", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdateSocialSubCategory(NameIDModel oData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", oData.ID);
                paramater.Add("@Name", oData.Name);
                paramater.Add("@SBranchID", oData.SBranchID);
                paramater.Add("@Extra1", oData.Extra1);
                paramater.Add("@OpType", oData.OpType);

                return con.Query<int>("sp_InsertUpdateSocialSubCategory", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        #endregion
        #region Extra IncomeRelated
        public List<ExtraIncomeHeadModel> GetExtraIncomeHeads(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<ExtraIncomeHeadModel>("sp_GetExtraIncomeHeads", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        public int InsertUpdateExtraIncomeHead(ExtraIncomeHeadModel oData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", oData.ID);
                paramater.Add("@Name", oData.Name);
                paramater.Add("@CreatedDate", oData.CreatedDate);
                paramater.Add("@Status", oData.Status);
                paramater.Add("@SBranchID", oData.SBranchID);
                paramater.Add("@OpType", oData.OpType);

                return con.Query<int>("sp_UpdateExtraIncomeHead", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
        }
        #endregion

        #region Day Book Head
        public List<DayBookHeadModel> GetDayBookHead(int SBranchID)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SBranchID", SBranchID);
                return con.Query<DayBookHeadModel>("sp_GetDayBookHead", paramater, null, true, 0, commandType: CommandType.StoredProcedure).ToList();

            }
        }

        public int UpdateDayBookHead(DayBookHeadModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", oModel.ID);
                paramater.Add("@Name", oModel.Name);
                paramater.Add("@Code", oModel.Code);
                paramater.Add("@Description", oModel.Description);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@OpType", oModel.OpType);
                return con.Query<int>("sp_UpdateDayBookHead", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        public int DeleteDayBookHead(int DayBookID)
        {

            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@DayBookID", DayBookID);
                return con.Query<int>("sp_DeleteDayBookHead", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

        }
        //public ProductsPageModel GetProducts(ProductsPageModel oModel)
        //{
        //    using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
        //    {
        //        var paramater = new DynamicParameters();
        //        paramater.Add("@SBranchID", oModel.SBranchID);
        //        paramater.Add("@CategoryID", oModel.CategoryID);
        //        using (var multi = con.QueryMultiple("sp_GetProducts", paramater, null, 0, commandType: CommandType.StoredProcedure))
        //        {
        //            oModel.Products = multi.Read<ProductModel>().ToList();
        //            oModel.Categories = multi.Read<ProductCategoryModel>().ToList();
        //            oModel.CategoryID = multi.Read<int>().SingleOrDefault();
        //        }
        //    }
        //    return oModel;
        //}
        //public ProductsPageModel GetProductDetails(int ProductID, int SBranchID)
        //{
        //    ProductsPageModel oModel = new ProductsPageModel();
        //    using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
        //    {
        //        var paramater = new DynamicParameters();
        //        paramater.Add("@ProductID", ProductID);
        //        paramater.Add("@SBranchID", SBranchID);
        //        using (var multi = con.QueryMultiple("sp_GetProductDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
        //        {
        //            oModel.Product = multi.Read<ProductModel>().SingleOrDefault();
        //            oModel.Categories = multi.Read<ProductCategoryModel>().ToList();
        //        }
        //    }
        //    return oModel;
        //}
        //public int UpdateProduct(ProductModel oModel)
        //{
        //    using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
        //    {
        //        var paramater = new DynamicParameters();
        //        paramater.Add("@ProductID", oModel.ProductID);
        //        paramater.Add("@ProductCategoryID", oModel.ProductCategoryID);
        //        paramater.Add("@Name", oModel.Name);
        //        paramater.Add("@Photo", oModel.Photo);
        //        paramater.Add("@MRP", oModel.MRP);
        //        paramater.Add("@Price", oModel.Price);
        //        paramater.Add("@SBranchID", oModel.SBranchID);
        //        paramater.Add("@Status", oModel.Status);
        //        paramater.Add("@Quantity", oModel.Quantity);
        //        paramater.Add("@MinQty", oModel.MinQty);
        //        paramater.Add("@OpType", oModel.OpType);
        //        return con.Query<int>("sp_UpdateProduct", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
        //    }
        //}

        //public List<ProductModel> GetLowStockProducts(int SBranchID)
        //{
        //    using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
        //    {
        //        var paramater = new DynamicParameters();
        //        paramater.Add("@SBranchID", SBranchID);
        //        return con.Query<ProductModel>("sp_GetLowStockProducts", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
        //    }
        //}
        #endregion

        #region OnlineExam

        public void GetTeacherQuestionBank(TeacherQuestionBankModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_GetAdminQuestionBank", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Questions = multi.Read<QuestionBankModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.Subjects = multi.Read<NameIDModel>().ToList();
                    objModel.SubjectID = multi.Read<int>().SingleOrDefault();
                    objModel.GroupID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
        }

        public void GetAdminOnlineExams(OnlineExamPageModel objModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@GroupID", objModel.GroupID);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                using (var multi = con.QueryMultiple("sp_GetAdminOnlineExams", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.Exams = multi.Read<OnlineExamModel>().ToList();
                    objModel.Classes = multi.Read<NameIDModel>().ToList();
                    objModel.ClassID = multi.Read<int>().SingleOrDefault();
                    objModel.Subjects = multi.Read<NameIDModel>().ToList();
                    objModel.SubjectID = multi.Read<int>().SingleOrDefault();
                    objModel.Sections = multi.Read<NameIDModel>().ToList();
                    objModel.SectionID = multi.Read<int>().SingleOrDefault();
                    objModel.GroupID = multi.Read<int>().SingleOrDefault();
                    objModel.Sessions = multi.Read<NameIDModel>().ToList();
                    objModel.SessionID = multi.Read<int>().SingleOrDefault();
                    //objModel.TeacherID = multi.Read<int>().SingleOrDefault();
                }
            }
        }






        public OnlineExamEditModel GetAdminOnlineExamDetails(OnlineExamModel objModel)
        {
            OnlineExamEditModel oData = new OnlineExamEditModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@TeacherID", objModel.TeacherID);
                paramater.Add("@ClassID", objModel.ClassID);
                paramater.Add("@SectionID", objModel.SectionID);
                paramater.Add("@SubjectID", objModel.SubjectID);
                paramater.Add("@SessionID", objModel.SessionID);
                paramater.Add("@SBranchID", objModel.SBranchID);
                paramater.Add("@OExamID", objModel.OExamID);
                using (var multi = con.QueryMultiple("sp_GetTeacherOnlineExamsDetails", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oData.Exam = multi.Read<OnlineExamModel>().SingleOrDefault();
                    oData.ExamQuestions = multi.Read<QuestionBankModel>().ToList();
                    oData.Questions = multi.Read<QuestionBankModel>().ToList();
                }
            }
            if (oData.Exam.OExamID == 0)
            {
                oData.Exam.OExamStartDate = CommonUsage.GetCurrentDate();
                oData.Exam.OExamEndDate = oData.Exam.OExamStartDate.AddMinutes(30);
                oData.Exam.Status = 1;
            }
            return oData;
        }



        public int UpdateOnlineExam(OnlineExamModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@OExamID", oModel.OExamID);
                paramater.Add("@OExamStartDate", oModel.OExamStartDate);
                paramater.Add("@OExamEndDate", oModel.OExamEndDate);
                paramater.Add("@SessionID", oModel.SessionID);
                paramater.Add("@ClassID", oModel.ClassID);
                paramater.Add("@SectionID", oModel.SectionID);
                paramater.Add("@SubjectID", oModel.SubjectID);
                paramater.Add("@TeacherID", oModel.TeacherID);
                paramater.Add("@CreatedDate", oModel.CreatedDate);
                paramater.Add("@Status", oModel.Status);
                paramater.Add("@ExamTitle", oModel.ExamTitle);
                paramater.Add("@ExamDescription", oModel.ExamDescription);
                paramater.Add("@ExamPriority", oModel.ExamPriority);
                paramater.Add("@OnlineExamType", oModel.OnlineExamType);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@Questions", oModel.GetQuestionsDatatable());
                paramater.Add("@OpType", oModel.OpType);

                return con.Query<int>("sp_UpdateOnlineExam", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }
        }

        #endregion
        public StudentAdmissionReportModel GetStudentAdmssionDetail(StudentAdmissionReportModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
             
                paramater.Add("@SessionID", oModel.SessionID);
                paramater.Add("@SBranchID", oModel.SBranchID);
                paramater.Add("@StartDate", oModel.StartDate);
                paramater.Add("@EndDate", oModel.EndDate);


                using (var multi = con.QueryMultiple("sp_GetSessionStudentAdmissionDetail", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.StudentDetail = multi.Read<StudentAdmissionDetail>().ToList();
                    oModel.Sessions = multi.Read<NameIDModel>().ToList();
                    // oModel.SessionID = multi.Read<int>().SingleOrDefault();
                    try
                    {

                        oModel.Branches = multi.Read<SBranchModel>().SingleOrDefault();
                    }
                    catch (Exception ex)
                    { }
                }
            }
            return oModel;
        }
        public StudentAdmissionReportModel GetPromotedStudentsDetail(StudentAdmissionReportModel oModel)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SessionID", oModel.SessionID);
                paramater.Add("@SBranchID", oModel.SBranchID);
                //paramater.Add("@StartDate", oModel.StartDate);
                using (var multi = con.QueryMultiple("sp_GetSessionPromotedStudentDetail", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    oModel.StudentDetail = multi.Read<StudentAdmissionDetail>().ToList();
                    oModel.Sessions = multi.Read<NameIDModel>().ToList();
                    oModel.Branches = multi.Read<SBranchModel>().SingleOrDefault();
                    //oModel.SessionID = multi.Read<int>().SingleOrDefault();
                }
            }
            return oModel;
        }
    }

}