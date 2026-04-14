
Delete from [dbo].[EmployeeTypeMaster] where EmployeeTypeID in (-1,-2,-3,4,1)
SET IDENTITY_INSERT [dbo].[EmployeeTypeMaster] ON 
Insert into [dbo].[EmployeeTypeMaster](EmployeeTypeID,EmployeeTypeName,SBranchID,IsDefault) values(13,'Driver',0,1)
Insert into [dbo].[EmployeeTypeMaster](EmployeeTypeID,EmployeeTypeName,SBranchID,IsDefault) values(12,'Conductor',0,1)
Insert into [dbo].[EmployeeTypeMaster](EmployeeTypeID,EmployeeTypeName,SBranchID,IsDefault) values(10,'Receptionist',0,1)
Insert into [dbo].[EmployeeTypeMaster](EmployeeTypeID,EmployeeTypeName,SBranchID,IsDefault) values(8,'Principal',0,1)
Insert into [dbo].[EmployeeTypeMaster](EmployeeTypeID,EmployeeTypeName,SBranchID,IsDefault) values(3,'Teacher',0,1)
Insert into [dbo].[EmployeeTypeMaster](EmployeeTypeID,EmployeeTypeName,SBranchID,IsDefault) values(5,'Librarian',0,1)

SET IDENTITY_INSERT [dbo].[EmployeeTypeMaster] OFF

Update EmployeeMaster set EmployeeType=3 where EmployeeType=1
Update EmployeeMaster set EmployeeType=13 where EmployeeType=-1
Update EmployeeMaster set EmployeeType=12 where EmployeeType=-2
Update EmployeeMaster set EmployeeType=10 where EmployeeType=-3
Update EmployeeMaster set EmployeeType=8 where EmployeeType=4


Update [EmployeeTypeSalaryDetails] set EmployeeTypeID=3 where EmployeeTypeID=1
Update [EmployeeTypeSalaryDetails] set EmployeeTypeID=13 where EmployeeTypeID=-1
Update [EmployeeTypeSalaryDetails] set EmployeeTypeID=12 where EmployeeTypeID=-2
Update [EmployeeTypeSalaryDetails] set EmployeeTypeID=10 where EmployeeTypeID=-3
Update [EmployeeTypeSalaryDetails] set EmployeeTypeID=8 where EmployeeTypeID=4

Update [EmployeeAttendanceMasterT] set DesignationID=3 where DesignationID=1
Update [EmployeeAttendanceMasterT] set DesignationID=13 where DesignationID=-1
Update [EmployeeAttendanceMasterT] set DesignationID=12 where DesignationID=-2
Update [EmployeeAttendanceMasterT] set DesignationID=10 where DesignationID=-3
Update [EmployeeAttendanceMasterT] set DesignationID=8 where DesignationID=4

GO
ALTER procedure [dbo].[spn_InsertUpdateEmployeeBasicDetails]
(
@EmployeeID int,
@EmployeeName nvarchar(50),
@DOB date,
@Gender int,
@Nationality int,
@ReligionID int,
@DOJ date,
@EmailID nvarchar(100),
@CategoryID int,
@MaritalStatus int,
@BloodGroup nvarchar(5),
@MotherTongueID int,
@PassportNo nvarchar(20),
@MobileNo nvarchar(12),
@PhoneNo nvarchar(20),
@AccessCardNo nvarchar(20),
@FatherHubName nvarchar(20),
@SBranchID int,
@OperationDate datetime,
@ImageName nvarchar(100),
@Status int,
@EmployeeType int,
@Password nvarchar(200),
@AadharNumber nvarchar(50),
@LicenceNumber nvarchar(50)
)
as 
begin 

	declare @EmployeeSID nvarchar(20)	
	if(@EmployeeID=0)
	begin
		insert into [dbo].[EmployeeMaster](EmployeeName,DOB,Gender,Nationality,ReligionID,DOJ,EmailID,CategoryID,MaritalStatus,BloodGroup,
		 MotherTongueID,PassportNo,MobileNumber,LandLineNumber,AccessCardNo,FatherHubName,SBranchID,CreatedDate,
		 Photo,[Status],EmployeeType,DesignationID,LicenceNumber,AadharNumber)
		 values(@EmployeeName,@DOB,@Gender,@Nationality,@ReligionID,@DOJ,@EmailID,@CategoryID,@MaritalStatus,@BloodGroup,
		 @MotherTongueID,@PassportNo,@MobileNo,@PhoneNo,@AccessCardNo,@FatherHubName,@SBranchID,@OperationDate,@ImageName,@Status,
		 @EmployeeType,@EmployeeType,@LicenceNumber,@AadharNumber)

		 SELECT @EmployeeID= CAST(SCOPE_IDENTITY() as int)
		
		 
		 --if(@EmployeeType=1)
		 -- Change for Reception Login Credential
		 if(@EmployeeType in (3,10,5,8,12))
		 begin
	
			 select @EmployeeSID='EMP'+RIGHT(REPLICATE('0',6)+CAST(@EmployeeID AS VARCHAR(6)),6) 
	   
			 insert into LoginDetails (UserName,Password,EmailID,UserID,Usertype,SBranchID)
			 values(@EmployeeSID,@Password,@EmailID,@EmployeeID,3,@SBranchID)
		end

		 declare @EmployeeAID nvarchar(20)

		 select @EmployeeAID='EMP'+RIGHT(REPLICATE('0',6)+CAST(@EmployeeID AS VARCHAR(6)),6) 
		set @AccessCardNo =isnull(@AccessCardNo,'0000')
		 exec sp_AddUserToEsslDevice @EmployeeName,@EmployeeAID,@SBranchID,@AccessCardNo,@OperationDate

		
	end
	else
	begin

	declare @OldAccessCardNumber nvarchar(50)
	
		declare @OldEmployeeType int
		select @OldEmployeeType=EmployeeType,@OldAccessCardNumber= AccessCardNo from EmployeeMaster where EmployeeID=@EmployeeID
		Update [dbo].[EmployeeMaster] set EmployeeName=@EmployeeName,DOB=@DOB,Gender=@Gender,Nationality=@Nationality
		,ReligionID=@ReligionID,DOJ=@DOJ,EmailID=@EmailID,CategoryID=@CategoryID,MaritalStatus=@MaritalStatus,BloodGroup=@BloodGroup,
		 MotherTongueID=@MotherTongueID,PassportNo=@PassportNo,MobileNumber=@MobileNo,LandLineNumber=@PhoneNo,AccessCardNo=@AccessCardNo
		 ,FatherHubName=@FatherHubName,ModifiedDate=@OperationDate,
		 Photo=@ImageName,[Status]=@Status,EmployeeType=@EmployeeType,DesignationID=@EmployeeType
		 ,LicenceNumber=@LicenceNumber,AadharNumber=@AadharNumber
		  where EmployeeID=@EmployeeID

		 if(@OldEmployeeType<>@EmployeeType)
		 begin
			if(@OldEmployeeType  in (3,10,5,8,12) and @EmployeeType not in (3,10,5,8,12))
			begin
				delete from LoginDetails where UserID=@EmployeeID and UserType=@EmployeeType
			end
			else if(@EmployeeType in (3,10,5,8,12) and @OldEmployeeType not in (3,10,5,8,12))
			begin
	
				 select @EmployeeSID='EMP'+RIGHT(REPLICATE('0',6)+CAST(@EmployeeID AS VARCHAR(6)),6) 
	   
				 insert into LoginDetails (UserName,Password,EmailID,UserID,Usertype,SBranchID)
				 values(@EmployeeSID,@Password,@EmailID,@EmployeeID,@EmployeeType,@SBranchID)
			end
		 end
		 if(isnull(@OldAccessCardNumber,'0000')<>@AccessCardNo)
		 begin
			 declare @EmployeeAUID nvarchar(20)
			 select @EmployeeAUID='EMP'+RIGHT(REPLICATE('0',6)+CAST(@EmployeeID AS VARCHAR(6)),6) 
			 exec sp_DeleteUserToEsslDevice @EmployeeAUID,@SBranchID,@OperationDate
			 set @OperationDate=DATEADD (minute , 5 , @OperationDate ) 
			 exec sp_AddUserToEsslDevice @EmployeeName,@EmployeeAUID,@SBranchID,@AccessCardNo,@OperationDate
		 end
	end
	SELECT @EmployeeID
end
GO

drop table [AttandanceDeviceDetails]

GO
ALTER proc [dbo].[sp_DeleteUserToEsslDevice]
(
@UserDeviceCode nvarchar(50),
@SBranchID	int,
@CreatedDate datetime
)
AS
BEGIN
	declare @Command nvarchar(max)
	set @Command='C:UniqueId:DATA DEL_USER PIN='+@UserDeviceCode
	declare @Title nvarchar(max)
	set @Title='Delete User '+@UserDeviceCode


		declare @DeviceID nvarchar(max)
		DECLARE db_cursor CURSOR FOR  
		SELECT SerialNumber 
		FROM Devices where SBranchID=@SBranchID

		OPEN db_cursor   
		FETCH NEXT FROM db_cursor INTO @DeviceID

		WHILE @@FETCH_STATUS = 0   
		BEGIN   
			 insert into [DeviceCommands] ([Title]  ,[DeviceCommand],[SerialNumber],[Status],[Type],[CreationDate])
			values(@Title,@Command,@DeviceID,'Pending','Delete User',@CreatedDate)
			  FETCH NEXT FROM db_cursor INTO @DeviceID
		END   

		CLOSE db_cursor   
		DEALLOCATE db_cursor
END

GO
ALTER proc [dbo].[sp_AddUserToEsslDevice]
(
@Name nvarchar(50),
@UserDeviceCode nvarchar(50),
@SBranchID	int,
@CardNumber nvarchar(50),
@CreatedDate datetime
)
AS
BEGIN
	declare @Command nvarchar(max)
	set @CardNumber=isnull(@CardNumber,'0000')
	set @Command='C:UniqueId:DATA USER PIN='+@UserDeviceCode+'	Name='+@Name+'	Pri=0	Passwd=	Card='+@CardNumber+'	Grp=1'
	declare @Title nvarchar(max)
	set @Title='Add User '+@UserDeviceCode


		declare @DeviceID nvarchar(max)
		DECLARE db_cursor CURSOR FOR  
		SELECT SerialNumber 
		FROM Devices where SBranchID=@SBranchID

		OPEN db_cursor   
		FETCH NEXT FROM db_cursor INTO @DeviceID

		WHILE @@FETCH_STATUS = 0   
		BEGIN   
			 insert into [DeviceCommands] ([Title]  ,[DeviceCommand],[SerialNumber],[Status],[Type],[CreationDate])
			values(@Title,@Command,@DeviceID,'Pending','Add User',@CreatedDate)
			  FETCH NEXT FROM db_cursor INTO @DeviceID
		END   

		CLOSE db_cursor   
		DEALLOCATE db_cursor

	
	
END
GO
ALTER proc [dbo].[sp_UpdateEmployeeAttandanceAuto]
(
@Year nvarchar(5),
@Month  nvarchar(2),
@Day nvarchar(3),
@EmployeeID int,
@DeviceSerial nvarchar(50),
@Status int
)
AS
BEGIN
declare @DesignationID int
declare @SectionID int
declare @IsExist int
declare @DaysWorking int
set @DaysWorking=6
declare @SQLQuery nvarchar(max)
declare @SBranchID int
select @SBranchID=SBranchID from [dbo].Devices where SerialNumber=@DeviceSerial

Select  @DesignationID=DesignationID from EmployeeMaster where SBranchID=@SBranchID and EmployeeID=@EmployeeID
select @IsExist=count(*) from EmployeeAttendanceMasterT where FYear=@Year and [Month]=@Month and EmployeeID=@EmployeeID
	
	if(@IsExist=1)
		begin
			set @SQLQuery='Update EmployeeAttendanceMasterT set '+@Day+'='+cast (@Status as nvarchar(5))+' where FYear='+@Year+' and Month='+@Month+' and EmployeeID='+cast (@EmployeeID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))
			exec(@SQLQuery)
		end
	else
		begin
			declare @DayNum int
			set @DayNum=1
			while(@DayNum<=31)
			begin
				declare @HCount int
				declare @LCount int
				declare @tStatus int
				set @tStatus=3
				if(isdate(@Month+'-'+cast(@DayNum as nvarchar(2))+'-'+@Year)!=0)
				begin
						Select @HCount=count(*) from HolidayMaster where cast(@Month+'-'+cast(@DayNum as nvarchar(2))+'-'+@Year as Date) between StartDate and EndDate and Status=1 and DayDuration=1 and IsEmployee=1 
						select @LCount=count(*) from LeaveMaster where cast(@Month+'-'+cast(@DayNum as nvarchar(2))+'-'+@Year as Date) between StartDate and EndDate and IsApproved=1 and ApplicantID=@EmployeeID and ApplicantType=0
							
						if(@HCount!=0)
						begin
							set @tStatus=2
						end
						else if (@LCount!=0)
						begin
							set @tStatus=-1
						end
				end
				if(@DayNum=1)
				begin
					if('D'+cast(@DayNum as nvarchar(3))=@Day)
					begin
						set @SQLQuery='Insert into EmployeeAttendanceMasterT(EmployeeID,SBranchID,DesignationID,FYear,Month,D'+cast(@DayNum as nvarchar(3))+') 
						Values ('+cast (@EmployeeID as nvarchar(5))+','+cast (@SBranchID as nvarchar(5))+','+cast (@DesignationID as nvarchar(5))+','+@Year+','+@Month+','+cast(@Status as nvarchar(2))+')'					
					end
					else
					begin
					set @SQLQuery='Insert into EmployeeAttendanceMasterT(EmployeeID,SBranchID,DesignationID,FYear,Month,D'+cast(@DayNum as nvarchar(3))+') 
						Values ('+cast (@EmployeeID as nvarchar(5))+','+cast (@SBranchID as nvarchar(5))+','+cast (@DesignationID as nvarchar(5))+','+@Year+','+@Month+','+cast(@tStatus as nvarchar(2))+')'
					end
					exec(@SQLQuery)
				end
				else
				begin
					if(isdate(@Month+'-'+cast(@DayNum as nvarchar(2))+'-'+@Year)=0)
					begin
						set @SQLQuery='Update EmployeeAttendanceMasterT set D'+cast(@DayNum as nvarchar(3))+'=-2 where FYear='+@Year+' and Month='+@Month+' and EmployeeID='+cast (@EmployeeID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))
						exec(@SQLQuery)
					end
					else
					begin
					
						declare @CurDate date
						set @CurDate= cast(@Month+'-'+cast(@DayNum as nvarchar(2))+'-'+@Year as Date)
						If datepart(dw, @CurDate) = 1 Set @tStatus=-2 
						if(@DaysWorking=6 and datepart(dw, @CurDate) = 7) Set @tStatus=-2 
						if('D'+cast(@DayNum as nvarchar(3))=@Day)
						begin
							set @SQLQuery='Update EmployeeAttendanceMasterT set D'+cast(@DayNum as nvarchar(3))+'='+cast (@Status as nvarchar(5))+' where FYear='+@Year+' and Month='+@Month+' and EmployeeID='+cast (@EmployeeID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))
							exec(@SQLQuery)
						end
						else
						begin
							set @SQLQuery='Update EmployeeAttendanceMasterT set D'+cast(@DayNum as nvarchar(3))+'='+cast (@tStatus as nvarchar(5))+' where FYear='+@Year+' and Month='+@Month+' and EmployeeID='+cast (@EmployeeID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))
							exec(@SQLQuery)
						end
					end					
				end
				set @DayNum=@DayNum+1
			end
		end	

END

GO

ALTER proc [dbo].[sp_GetEmployeeList]
(
@EmployeeTypeID int,
@SBranchID int
)
AS
BEGIN
	Select EM.[EmployeeID],EM.[EmployeeName],EM.Photo,EM.Gender,MobileNumber,VehicleRouteID,
	(case when EmployeeType in (-1,-2) then 'TRA' else 'EMP' end)+RIGHT(REPLICATE('0',6)+CAST(EM.[EmployeeID] AS VARCHAR(6)),6) as [EmployeeSID],
	DOB,DOJ,EmailID,BloodGroup,[AccessCardNo],AadharNumber,LicenceNumber
	from EmployeeMaster EM 	
	where EM.SBranchID=@SBranchID and EM.EmployeeType=@EmployeeTypeID
	order by EM.[EmployeeName]

	exec sp_GetEmployeeTypes  @SBranchID
END
GO
Alter proc [dbo].[sp_GetTeacherList]  
(  
@SBranchID int  
)  
AS  
BEGIN  
 Select [EmployeeID],EmployeeName,Photo,Extra2 as EmployeeSID,Extra2,EmployeeType,Status from   
 (Select EM.[EmployeeID] ,EM.[EmployeeName],EM.Photo ,EmployeeType,Status,  
 (case when EmployeeType in (-1,-2) then 'TRA' else 'EMP' end)+RIGHT(REPLICATE('0',6)+CAST(EM.[EmployeeID] AS VARCHAR(6)),6) as Extra2  
 from EmployeeMaster EM    
 where EM.SBranchID=@SBranchID and EM.[EmployeeType]=3 )t  
END  
GO
Alter proc [dbo].[spn_GetTeacherDayTimeTableLactures]  
(  
@TeacherID int,  
@DayID int,  
@CurrDate date   
)  
AS  
BEGIN  
  
 if(@DayID=1)  
  begin  
  Select PM.PeriodID,PM.PeriodType,PM.Name,cast(StartTime as nvarchar(10)) as StartTime,cast(EndTime as nvarchar(10)) as EndTime  
   ,isnull((Select Name from EducationLevelMaster SM where SM.ID=TTM.EducationLevelID),'NA')  as 'EducationLevelName'  
   ,isnull((Select SubjectName from SubjectMasterAll SM where SM.SubjectID=TTM.SubjectID),'No Subject')  as 'SubjectName'  
   ,isnull((Select ClassName+'/'+SectionName from v_ClassSectionNames EM where EM.ClassID=TTM.ClassID and EM.SectionID=TTM.SectionID),'No Class')  as 'ClassSection'  
   ,TTM.EducationLevelID,TTM.ClassID,TTM.SectionID,TTM.SubjectID  
   from   
  (select EducationLevelID,ClassID,SectionID,PeriodID,MondaySubjectID as SubjectID from Time_Table_Master where MondayTeacherID=@TeacherID)TTM  
  left outer join  PeriodMaster PM on PM.PeriodID=TTM.PeriodID  
  union  
  Select TSM.PeriodID,-1 as PeriodType,PM.Name,cast(PM.StartTime as nvarchar(10)) as StartTime,cast(PM.EndTime as nvarchar(10)) as EndTime   
   ,isnull((Select Name from EducationLevelMaster SM where SM.ID=PM.EducationLevelID),'NA')  as 'EducationLevelName'  
    ,isnull((Select SubjectName from SubjectMasterAll SM where SM.SubjectID=TSM.NewSubjectID),'No Subject')  as 'SubjectName'  
    ,isnull((Select ClassName+'/'+SectionName from v_ClassSectionNames EM where EM.ClassID=TSM.ClassID and EM.SectionID=TSM.SectionID),'No Class')  as 'ClassSection'  
    ,(Select EducationLevelID from ClassMaster CM where CM.ClassID=TSM.ClassID) as EducationLevelID,TSM.ClassID,TSM.SectionID,TSM.SubjectID  
  from TeacherSubstitutionMaster TSM left outer join PeriodMaster PM on PM.PeriodID=TSM.PeriodID   
  where DayID=1 and ReplacingTeacherID=@TeacherID and (FromDate between  DATEADD(day,-2,@CurrDate) and DATEADD(day,7,@CurrDate) )    
  order by StartTime desc
  
 end  
 else if(@DayID=2)  
 begin  
  Select PM.PeriodID,PM.PeriodType,PM.Name,cast(StartTime as nvarchar(10)) as StartTime,cast(EndTime as nvarchar(10)) as EndTime  
   ,isnull((Select Name from EducationLevelMaster SM where SM.ID=TTM.EducationLevelID),'NA')  as 'EducationLevelName'  
   ,isnull((Select SubjectName from SubjectMasterAll SM where SM.SubjectID=TTM.SubjectID),'No Subject')  as 'SubjectName'  
   ,isnull((Select ClassName+'/'+SectionName from v_ClassSectionNames EM where EM.ClassID=TTM.ClassID and EM.SectionID=TTM.SectionID),'No Class')  as 'ClassSection'  
   ,TTM.EducationLevelID,TTM.ClassID,TTM.SectionID,TTM.SubjectID  
  from   
 (select EducationLevelID,ClassID,SectionID,PeriodID,TuesdaySubjectID as SubjectID from Time_Table_Master where TuesdayTeacherID=@TeacherID)TTM  
 left outer join  PeriodMaster PM on PM.PeriodID=TTM.PeriodID  
 union  
 Select TSM.PeriodID,-1 as PeriodType,PM.Name,cast(PM.StartTime as nvarchar(10)) as StartTime,cast(PM.EndTime as nvarchar(10)) as EndTime   
  ,isnull((Select Name from EducationLevelMaster SM where SM.ID=PM.EducationLevelID),'NA')  as 'EducationLevelName'  
   ,isnull((Select SubjectName from SubjectMasterAll SM where SM.SubjectID=TSM.NewSubjectID),'No Subject')  as 'SubjectName'  
   ,isnull((Select ClassName+'/'+SectionName from v_ClassSectionNames EM where EM.ClassID=TSM.ClassID and EM.SectionID=TSM.SectionID),'No Class')  as 'ClassSection'  
   ,(Select EducationLevelID from ClassMaster CM where CM.ClassID=TSM.ClassID) as EducationLevelID,TSM.ClassID,TSM.SectionID,TSM.SubjectID  
 from TeacherSubstitutionMaster TSM left outer join PeriodMaster PM on PM.PeriodID=TSM.PeriodID   
 where DayID=2 and ReplacingTeacherID=@TeacherID and (FromDate between  DATEADD(day,-2,@CurrDate) and DATEADD(day,7,@CurrDate) )    
  order by StartTime desc
 end  
 else if(@DayID=3)  
 begin  
  Select PM.PeriodID,PM.PeriodType,PM.Name,cast(StartTime as nvarchar(10)) as StartTime,cast(EndTime as nvarchar(10)) as EndTime  
   ,isnull((Select Name from EducationLevelMaster SM where SM.ID=TTM.EducationLevelID),'NA')  as 'EducationLevelName'  
   ,isnull((Select SubjectName from SubjectMasterAll SM where SM.SubjectID=TTM.SubjectID),'No Subject')  as 'SubjectName'  
   ,isnull((Select ClassName+'/'+SectionName from v_ClassSectionNames EM where EM.ClassID=TTM.ClassID and EM.SectionID=TTM.SectionID),'No Class')  as 'ClassSection'  
   ,TTM.EducationLevelID,TTM.ClassID,TTM.SectionID,TTM.SubjectID  
  from   
 (select EducationLevelID,ClassID,SectionID,PeriodID,WednesdaySubjectID as SubjectID from Time_Table_Master where WednesdayTeacherID=@TeacherID)TTM  
 left outer join  PeriodMaster PM on PM.PeriodID=TTM.PeriodID  
 union  
 Select TSM.PeriodID,-1 as PeriodType,PM.Name,cast(PM.StartTime as nvarchar(10)) as StartTime,cast(PM.EndTime as nvarchar(10)) as EndTime   
  ,isnull((Select Name from EducationLevelMaster SM where SM.ID=PM.EducationLevelID),'NA')  as 'EducationLevelName'  
   ,isnull((Select SubjectName from SubjectMasterAll SM where SM.SubjectID=TSM.NewSubjectID),'No Subject')  as 'SubjectName'  
   ,isnull((Select ClassName+'/'+SectionName from v_ClassSectionNames EM where EM.ClassID=TSM.ClassID and EM.SectionID=TSM.SectionID),'No Class')  as 'ClassSection'  
   ,(Select EducationLevelID from ClassMaster CM where CM.ClassID=TSM.ClassID) as EducationLevelID,TSM.ClassID,TSM.SectionID,TSM.SubjectID  
 from TeacherSubstitutionMaster TSM left outer join PeriodMaster PM on PM.PeriodID=TSM.PeriodID   
 where DayID=3 and ReplacingTeacherID=@TeacherID and (FromDate between  DATEADD(day,-2,@CurrDate) and DATEADD(day,7,@CurrDate) )    
  order by StartTime desc
 end  
 else if(@DayID=4)  
 begin  
  Select PM.PeriodID,PM.PeriodType,PM.Name,cast(StartTime as nvarchar(10)) as StartTime,cast(EndTime as nvarchar(10)) as EndTime  
   ,isnull((Select Name from EducationLevelMaster SM where SM.ID=TTM.EducationLevelID),'NA')  as 'EducationLevelName'  
   ,isnull((Select SubjectName from SubjectMasterAll SM where SM.SubjectID=TTM.SubjectID),'No Subject')  as 'SubjectName'  
   ,isnull((Select ClassName+'/'+SectionName from v_ClassSectionNames EM where EM.ClassID=TTM.ClassID and EM.SectionID=TTM.SectionID),'No Class')  as 'ClassSection'  
   ,TTM.EducationLevelID,TTM.ClassID,TTM.SectionID,TTM.SubjectID  
  from   
 (select EducationLevelID,ClassID,SectionID,PeriodID,ThursdaySubjectID as SubjectID from Time_Table_Master where ThursdayTeacherID=@TeacherID)TTM  
 left outer join  PeriodMaster PM on PM.PeriodID=TTM.PeriodID  
 union  
 Select TSM.PeriodID,-1 as PeriodType,PM.Name,cast(PM.StartTime as nvarchar(10)) as StartTime,cast(PM.EndTime as nvarchar(10)) as EndTime   
  ,isnull((Select Name from EducationLevelMaster SM where SM.ID=PM.EducationLevelID),'NA')  as 'EducationLevelName'  
   ,isnull((Select SubjectName from SubjectMasterAll SM where SM.SubjectID=TSM.NewSubjectID),'No Subject')  as 'SubjectName'  
   ,isnull((Select ClassName+'/'+SectionName from v_ClassSectionNames EM where EM.ClassID=TSM.ClassID and EM.SectionID=TSM.SectionID),'No Class')  as 'ClassSection'  
   ,(Select EducationLevelID from ClassMaster CM where CM.ClassID=TSM.ClassID) as EducationLevelID,TSM.ClassID,TSM.SectionID,TSM.SubjectID  
 from TeacherSubstitutionMaster TSM left outer join PeriodMaster PM on PM.PeriodID=TSM.PeriodID   
 where DayID=4 and ReplacingTeacherID=@TeacherID and (FromDate between  DATEADD(day,-2,@CurrDate) and DATEADD(day,7,@CurrDate) )    
  order by StartTime desc
 end  
 else if(@DayID=5)  
 begin  
  Select PM.PeriodID,PM.PeriodType,PM.Name,cast(StartTime as nvarchar(10)) as StartTime,cast(EndTime as nvarchar(10)) as EndTime  
   ,isnull((Select Name from EducationLevelMaster SM where SM.ID=TTM.EducationLevelID),'NA')  as 'EducationLevelName'  
   ,isnull((Select SubjectName from SubjectMasterAll SM where SM.SubjectID=TTM.SubjectID),'No Subject')  as 'SubjectName'  
   ,isnull((Select ClassName+'/'+SectionName from v_ClassSectionNames EM where EM.ClassID=TTM.ClassID and EM.SectionID=TTM.SectionID),'No Class')  as 'ClassSection'  
   ,TTM.EducationLevelID,TTM.ClassID,TTM.SectionID,TTM.SubjectID  
  from   
 (select EducationLevelID,ClassID,SectionID,PeriodID,FridaySubjectID as SubjectID from Time_Table_Master where FridayTeacherID=@TeacherID)TTM  
 left outer join  PeriodMaster PM on PM.PeriodID=TTM.PeriodID  
 union  
 Select TSM.PeriodID,-1 as PeriodType,PM.Name,cast(PM.StartTime as nvarchar(10)) as StartTime,cast(PM.EndTime as nvarchar(10)) as EndTime   
  ,isnull((Select Name from EducationLevelMaster SM where SM.ID=PM.EducationLevelID),'NA')  as 'EducationLevelName'  
   ,isnull((Select SubjectName from SubjectMasterAll SM where SM.SubjectID=TSM.NewSubjectID),'No Subject')  as 'SubjectName'  
   ,isnull((Select ClassName+'/'+SectionName from v_ClassSectionNames EM where EM.ClassID=TSM.ClassID and EM.SectionID=TSM.SectionID),'No Class')  as 'ClassSection'  
   ,(Select EducationLevelID from ClassMaster CM where CM.ClassID=TSM.ClassID) as EducationLevelID,TSM.ClassID,TSM.SectionID,TSM.SubjectID  
 from TeacherSubstitutionMaster TSM left outer join PeriodMaster PM on PM.PeriodID=TSM.PeriodID   
 where DayID=5 and ReplacingTeacherID=@TeacherID and (FromDate between  DATEADD(day,-2,@CurrDate) and DATEADD(day,7,@CurrDate) )    
  order by StartTime desc
 end  
 else if(@DayID=6)  
 begin  
  Select PM.PeriodID,PM.PeriodType,PM.Name,cast(StartTime as nvarchar(10)) as StartTime,cast(EndTime as nvarchar(10)) as EndTime  
   ,isnull((Select Name from EducationLevelMaster SM where SM.ID=TTM.EducationLevelID),'NA')  as 'EducationLevelName'  
   ,isnull((Select SubjectName from SubjectMasterAll SM where SM.SubjectID=TTM.SubjectID),'No Subject')  as 'SubjectName'  
   ,isnull((Select ClassName+'/'+SectionName from v_ClassSectionNames EM where EM.ClassID=TTM.ClassID and EM.SectionID=TTM.SectionID),'No Class')  as 'ClassSection'  
   ,TTM.EducationLevelID,TTM.ClassID,TTM.SectionID,TTM.SubjectID  
   from   
  (select EducationLevelID,ClassID,SectionID,PeriodID,SaturdaySubjectID as SubjectID from Time_Table_Master where SaturdayTeacherID=@TeacherID)TTM  
  left outer join  PeriodMaster PM on PM.PeriodID=TTM.PeriodID  
  union  
  Select TSM.PeriodID,-1 as PeriodType,PM.Name,cast(PM.StartTime as nvarchar(10)) as StartTime,cast(PM.EndTime as nvarchar(10)) as EndTime   
   ,isnull((Select Name from EducationLevelMaster SM where SM.ID=PM.EducationLevelID),'NA')  as 'EducationLevelName'  
    ,isnull((Select SubjectName from SubjectMasterAll SM where SM.SubjectID=TSM.NewSubjectID),'No Subject')  as 'SubjectName'  
    ,isnull((Select ClassName+'/'+SectionName from v_ClassSectionNames EM where EM.ClassID=TSM.ClassID and EM.SectionID=TSM.SectionID),'No Class')  as 'ClassSection'  
    ,(Select EducationLevelID from ClassMaster CM where CM.ClassID=TSM.ClassID) as EducationLevelID,TSM.ClassID,TSM.SectionID,TSM.SubjectID  
  from TeacherSubstitutionMaster TSM left outer join PeriodMaster PM on PM.PeriodID=TSM.PeriodID   
  where DayID=6 and ReplacingTeacherID=@TeacherID and (FromDate between  DATEADD(day,-2,@CurrDate) and DATEADD(day,7,@CurrDate) )    
  order by StartTime desc
 end    
END  

