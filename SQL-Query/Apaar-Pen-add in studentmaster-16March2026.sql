-- Add PenNo and ApparID use in Student Section

2.Account Layout (add Css) : /* Custom Modern Design System */
2. add css file in (css) student.css
3. Studentdetails page Update
4. studentQuickupdate page update


EXEC sp_help 'ut_QuickStudentEdit';


sp_helptext spn_GetStudentsDetailsNew
sp_QuickUpdateStudents
spn_GetStudentRecordsClassSection
spn_InsertUpdateStudentBasicDetails


ALTER TABLE studentmaster ADD PenNo BIGINT,ApaarID BIGINT
go

alter procedure [dbo].[spn_InsertUpdateStudentBasicDetails]        
(        
@StudentID int,        
@Name nvarchar(50),        
@DOB date,        
@Gender int,        
@Nationality int,        
@ReligionID int,        
@DOJ date,        
@EmailID nvarchar(100),        
@Category int,        
@Disability int,        
@BloodGroup nvarchar(5),        
@MotherTongueID int,        
@PassportNo nvarchar(20),        
@ExtraCurricular nvarchar(max),        
@Allergic_Medicine nvarchar(max),        
@SBranchID int,        
@AdmissionBy int,        
@OperationDate datetime,        
@ImageName nvarchar(100),        
@AccessCardNo nvarchar(50),        
@Password nvarchar(50),        
@NotificationSMSTo int=null,        
@SchoolUID nvarchar(50)='',        
@AadharCardNo nvarchar(100)='',        
@SSSID nvarchar(50)='',        
@FamilyID nvarchar(50)='',        
@BankName nvarchar(100)='',        
@AccountNumber nvarchar(50)='',        
@IFSCCode nvarchar(50)='',        
@BranchName nvarchar(100)='',        
@PSchoolName nvarchar(max)='',        
@PSchoolMedium nvarchar(100)='',        
@PClassName nvarchar(100)='',        
@PSResult nvarchar(100)='',        
@PSchoolCity nvarchar(100)='',        
@PSchoolState nvarchar(100)='',
@PenNo BIGINT = NULL,
@ApaarID BIGINT = NULL     
        
)        
as         
begin         
if(@StudentID=0)        
begin        
 insert into [dbo].[StudentMaster](Name,DOB,Gender,Nationality,ReligionID,DOJ,EmailID,Category,Disability,BloodGroup,        
  MotherTongueID,PassportNo,ExtraCurricular,Allergic_Medicine,SBranchID,AdmissionBy,CreatedDate,Photo,AccessCardNo,NotificationSMSTo,SchoolUID,AadharCardNo,SSSID,FamilyID,        
  BankName,AccountNumber,IFSCCode,BranchName,PSchoolName,PSchoolMedium,PClassName,PSResult,PSchoolCity,PSchoolState,PenNo, ApaarID)        
        
  values(@Name,@DOB,@Gender,@Nationality,@ReligionID,@DOJ,@EmailID,@Category,@Disability,@BloodGroup,        
  @MotherTongueID,@PassportNo,@ExtraCurricular,@Allergic_Medicine,@SBranchID,@AdmissionBy,@OperationDate,@ImageName,@AccessCardNo,@NotificationSMSTo,@SchoolUID,@AadharCardNo,        
  @SSSID,@FamilyID,@BankName,@AccountNumber,@IFSCCode,@BranchName,@PSchoolName,@PSchoolMedium,@PClassName,@PSResult,@PSchoolCity,@PSchoolState,@PenNo, @ApaarID)        
  SELECT @StudentID= CAST(SCOPE_IDENTITY() as int)        
        
  declare @StudentSID nvarchar(20)        
        
   select @StudentSID='STUD'+RIGHT(REPLICATE('0',6)+CAST(@StudentID AS VARCHAR(6)),6)         
    
        if(@SBranchID=17)    
  begin    
   insert into LoginDetails (UserName,Password,EmailID,UserID,Usertype,SBranchID)        
   values(@StudentSID,'9cf5bbb6267e906a8f8ad2047ff8f984',@EmailID,@StudentID,6,@SBranchID)        
    end    
 else    
 begin    
  insert into LoginDetails (UserName,Password,EmailID,UserID,Usertype,SBranchID)        
   values(@StudentSID,'9cf5bbb6267e906a8f8ad2047ff8f984',@EmailID,@StudentID,6,@SBranchID)     
   --values(@StudentSID,@Password,@EmailID,@StudentID,6,@SBranchID)       
 end    
   SELECT @StudentID        
          
   declare @StudentAID nvarchar(20)        
        
   select @StudentAID='STU'+RIGHT(REPLICATE('0',6)+CAST(@StudentID AS VARCHAR(6)),6)         
     if(@AccessCardNo is not null and @AccessCardNo!='')        
   begin        
     exec sp_AddUserToEsslDevice @Name,@StudentAID,@SBranchID,@AccessCardNo,@OperationDate        
    end        
 end        
 else        
 begin        
 declare @OldAccessCardNumber nvarchar(50)        
         
 select  @OldAccessCardNumber =AccessCardNo from [dbo].[StudentMaster] where StudentID=@StudentID        
  Update [dbo].[StudentMaster]        
 set Name=@Name,DOB=@DOB,Gender=@Gender,Nationality=@Nationality,ReligionID=@ReligionID,DOJ=@DOJ,EmailID=@EmailID        
 ,Category=@Category,Disability=@Disability,BloodGroup=@BloodGroup,        
  MotherTongueID=@MotherTongueID,PassportNo=@PassportNo,ExtraCurricular=@ExtraCurricular,        
  Allergic_Medicine=@Allergic_Medicine,SBranchID=@SBranchID,ModifiedDate=@OperationDate,Photo=@ImageName        
  ,AccessCardNo=@AccessCardNo,NotificationSMSTo=@NotificationSMSTo,SchoolUID=@SchoolUID,AadharCardNo=@AadharCardNo,SSSID=@SSSID,FamilyID=@FamilyID        
  ,BankName=@BankName,AccountNumber=@AccountNumber,IFSCCode=@IFSCCode,BranchName=@BranchName,PSchoolName=@PSchoolName,PSchoolMedium=@PSchoolMedium,PClassName=@PClassName,PSResult=@PSResult,PSchoolCity=@PSchoolCity,PSchoolState=@PSchoolState,PenNo=@PenNo, ApaarID=@ApaarID        
  where StudentID=@StudentID        
        
  if(@OldAccessCardNumber<>@AccessCardNo)        
  begin        
   declare @StudentAUID nvarchar(20)        
   select @StudentAUID='STU'+RIGHT(REPLICATE('0',6)+CAST(@StudentID AS VARCHAR(6)),6)         
   exec sp_DeleteUserToEsslDevice @StudentAUID,@SBranchID,@OperationDate        
   set @OperationDate=DATEADD (minute , 5 , @OperationDate )         
    if(@AccessCardNo is not null and @AccessCardNo!='')        
   begin        
    exec sp_AddUserToEsslDevice @Name,@StudentAUID,@SBranchID,@AccessCardNo,@OperationDate        
   end    end        
  select @StudentID        
 end        
end   
go

alter  procedure [dbo].[spn_GetStudentsDetailsNew]    
(    
@StudentID int,    
@SBranchID int    
)    
as     
begin     
SELECT SM.StudentID, 'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6) as StudentSID    
      ,[Name]      ,[DOB]      ,[Gender]      ,[Nationality]      ,[ReligionID]      ,[DOJ]      ,[EmailID]      ,[Category]    
      ,[Disability]      ,[AdmissionRoute]      ,[BloodGroup]      ,[MotherTongueID]      ,[PassportNo]      ,[House]    
      ,[GuardianName]      ,[RelationWithGuardian]      ,[GuardianMobileNo]      ,[GuardianEmail]      ,[Cadd_HouseNo]    
      ,[Cadd_Street]      ,[Cadd_AreaCode]      ,[Cadd_Sector]      ,[Cadd_PinCode]      ,[Cadd_DistrictCode]      ,[Cadd_StateCode]    
      ,[Cadd_CountryCode]        ,[RoleModel]      ,[Ambition]      ,[ExtraCurricular]      ,[Allergic_Medicine]    
      ,[VehicleNo]      ,[DriverName]      ,[DriverMobileNo]      ,[AddressProof]      ,[BirthCertificate],AccessCardNo    
      ,[CategoryCertificate]      ,[Photo]      ,[TransferCertificate] ,VehicleRouteID,StopID,HostelRoomID,NotificationSMSTo,SchoolUID,AadharCardNo,MiniAddress,MiniAddress2    
   ,SSSID,FamilyID,BankName,AccountNumber,IFSCCode,BranchName,PSchoolName,PSchoolMedium,PClassName,PSResult,PSchoolCity,PSchoolState,PenNo,ApaarID    
  FROM StudentMaster SM     
  where SM.StudentID=@StudentID --and SM.SBranchID=@SBranchID     
      
  select ParentID,'PAR'+RIGHT(REPLICATE('0',6)+CAST(ParentID AS VARCHAR(6)),6) as [ParentSID],[FatherName]      ,[FatherOccupationID]    
      ,[FatherEducationID]      ,[FatherMobileNo]      ,[FatherEmailID]      ,[FatherBloodGroup] ,[FatherImage]     ,[MotherName]    
      ,[MotherOccupationID]      ,[MotherEducationID]      ,[MotherMobileNo]      ,[MotherEmailID]      ,[MotherBloodGroup],[MotherImage]    
      ,[Padd_HouseNo]      ,[Padd_Street]      ,[Padd_Area]      ,[Padd_Sector]      ,[Padd_PinCode],[AccessCardNo],FatherAadhaar,MotherAadhaar    
      ,[Padd_District]      ,[Padd_State]      ,[Padd_Country]      ,[HomeLandLineNo],FatherDOB,MotherDOB from ParentMaster     
  where ParentID=(select ParentID from StudentMaster where StudentID=@StudentID)    
    
      
 declare @AAreaID int    
 declare @ACityID int    
    
 select @ACityID=isnull(Cadd_DistrictCode,0),@AAreaID=Cadd_AreaCode from StudentMaster where StudentID=@StudentID    
    
 if(@ACityID=0)    
 begin    
  select @ACityID=min(CityID) from CityMaster where StateID=@SBranchID    
  select @AAreaID=min(AreaID) from AreaMaster where CityID=@ACityID    
 end    
 select CityID as ID, CityName as Name from CityMaster where SBranchID =@SBranchID    
 select AreaID as ID, AreaName as Name from AreaMaster where CityID = @ACityID    
    
 select isnull(@ACityID,0)    
 select isnull(@AAreaID,0)    
    
  select SS.StudentSessionUID,  SS.StudentID,   SS.ClassID   ,SS.SectionID   ,SS.QuotaID   ,SS.FromDate   ,SS.ToDate    
   ,SS.Status,   SS.RollNo,SS.FeePaymentMode    
   ,(Select QuotaName from QuotaMaster QM where QM.QuotaID=SS.QuotaID) as QuotaName    
   ,(Select ClassName from ClassMaster CM where CM.ClassID=SS.ClassID) as ClassName    
   ,(Select Name from Class_Sections CS where CS.ID=SS.SectionID) as SectionName,    
   (Select Name from HouseMaster HM where HM.ID=SS.HouseID) as HouseName    
   ,(Select SessionName from SessionMaster CS where CS.SessionID=SS.SessionID) as SessionName    
   from Student_Session SS where SS.StudentID=@StudentID and SS.SBranchID=@SBranchID    
   order by Status desc    
       
 --exec [dbo].[spn_GetStudentsTransportDetails] @StudentID,@SBranchID     
      
 Declare @SessionID int,@SectionID int,@QuotaID int,@ClassID int,@IsAdmissionFeeApplicable int,@GroupID int,@StudentSessionUID int    
    
    
    
 select top 1 @SessionID=SessionID,@QuotaID=QuotaID,@ClassID=ClassID,@SectionID=SectionID,@IsAdmissionFeeApplicable=IsAdmissionFeeApplicable,    
 @StudentSessionUID=StudentSessionUID    
 from Student_Session where StudentID=@StudentID order by Status desc,FromDate desc    
 exec sp_GetStudentSessionCustomFee @StudentSessionUID,@SBranchID    
    
    
 select THChangeID,ChangeType,UserType,UserID,StartDate,EndDate,SessionID,KeyID,    
 (Select AreaName from AreaMaster where AreaID=(select AreaID from [dbo].[TransportRouteDetails] TRD where TRD.StopID=T.KeyID)) as Name    
 from [dbo].[TransportHostalAllocationDelocation] T    
 where T.UserID=@StudentID    
 order by StartDate     
     
 select * from [dbo].[SubReligionMaster]    
    
 select * from NationalityMaster where SBranchID=@SBranchID    
 select * from ReligionMaster where SBranchID=@SBranchID    
 select * from MotherTongue where SBranchID=@SBranchID    
 select * from SocialCategory where SBranchID=@SBranchID    
  end  

  go

alter procedure [dbo].[spn_GetStudentRecordsClassSection]    
(    
@ClassID int,    
@SectionID int,    
@SBranchID int,    
@SessionID int=0    
)    
as     
begin     
 Declare @Status int=1    
 if(@SessionID=0)    
 begin    
  Select @SessionID=SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID    
 end    
 if(@SectionID=0)    
 begin    
  select @ClassID=min(ClassID) from ClassMaster where Status=1 and SBranchID=@SBranchID    
  select @SectionID=min(ID) from Class_Sections where Status=1 and ClassID=@ClassID    
 end    
    
 Select SS.StudentSessionUID, SS.RollNo, SM.Name,SM.ParentID, SM.StudentID,PM.FatherMobileNo,PM.MotherMobileNo,PM.FatherName,PM.MotherName,SM.SchoolUID,SM.Photo    
 ,SM.DOB,SM.DOJ,SM.GuardianMobileNo,SM.MiniAddress,SM.AadharCardNo,SM.PenNo,SM.ApaarID     
  ,'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6)  as StudentSID    
  from StudentMaster SM right outer join Student_Session SS on SS.StudentID=SM.StudentID    
  left outer join ParentMaster PM on PM.ParentID=SM.ParentID    
  Where SS.SectionID=@SectionID and SS.SessionID=@SessionID  and ss.status=1  
  order by SM.Name    
     
 select ClassID,ClassName from ClassMaster where SBranchID=@SBranchID    
    
 select ID,Name from Class_Sections where ClassID=@ClassID    
    
 select isnull(@ClassID,0)    
 select isnull(@SectionID,0)    
    
 Select SessionName as Name,SessionID as ID,SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID    
     
 select isnull(@SessionID,0)    
end    

go

 --drop proc [dbo].[sp_QuickUpdateStudents] 
 --drop TYPE [dbo].[ut_QuickStudentEdit]


 create TYPE [dbo].[ut_QuickStudentEdit] AS TABLE(
	[StudentID] [int] NULL,
	[StudentSessionUID] [int] NULL,
	[ParentID] [int] NULL,
	[SchoolUID] [nvarchar](100) NULL,
	[Name] [nvarchar](100) NULL,
	[RollNo] [nvarchar](50) NULL,
	[DOB] [date] NULL,
	[DOJ] [date] NULL,
	[FatherName] [nvarchar](100) NULL,
	[FatherMobileNo] [nvarchar](15) NULL,
	[MotherName] [nvarchar](100) NULL,
	[MotherMobileNo] [nvarchar](15) NULL,
	[AadharCardNo] [nvarchar](50) NULL,
	PenNo BIGINT  NULL,
    ApaarID BIGINT  NULL  
)
GO
 
 
create proc [dbo].[sp_QuickUpdateStudents]    
(    
@SBranchID int,    
@Students ut_QuickStudentEdit READONLY    
)    
AS    
BEGIN    
    
    
   UPDATE e SET e.Name=d.Name,e.DOB=d.DOB,e.DOJ=d.DOJ,e.SchoolUID=d.SchoolUID,
   e.AadharCardNo=d.AadharCardNo,e.PenNo=d.PenNo,e.ApaarID=d.ApaarID    
   FROM  StudentMaster e, @Students d     
   WHERE d.StudentID=e.StudentID    
    
   UPDATE e SET e.RollNo=d.RollNo    
   FROM  Student_Session e, @Students d     
   WHERE d.StudentSessionUID=e.StudentSessionUID    
    
   UPDATE e SET e.FatherName=d.FatherName,e.FatherMobileNo=d.FatherMobileNo,e.MotherName=d.MotherName,e.MotherMobileNo=d.MotherMobileNo    
   FROM  ParentMaster e, @Students d     
   WHERE d.ParentID=e.ParentID    
END   
go