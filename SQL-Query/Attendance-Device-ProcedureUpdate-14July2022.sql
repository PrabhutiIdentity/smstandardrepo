alter PROCEDURE [dbo].[sp_InsertBulkAttandance]      
(      
@SerialNumber nvarchar(50),      
@TransactionStamp nvarchar(50),      
@Attendances ut_DeviceLogs READONLY      
)      
AS      
BEGIN      
 Declare @BranchID int      
 Declare @EmployeeType int      
 declare @DownloadDate datetime,@LogDate datetime,@UserID nvarchar(50)      
 declare @iUserID int      
 Declare @ShiftID int=0      
      
 Update Devices set LastPing =getdate(),TransactionStamp=@TransactionStamp where SerialNumber= @SerialNumber      
 declare @DeviceID nvarchar(10)      
 Declare @DeviceName nvarchar(50)      
 select @DeviceID=DeviceID,@DeviceName=DeviceFName,@BranchID=SBranchID from Devices where SerialNumber=@SerialNumber      
      
 DECLARE db_fcursor CURSOR FOR        
 SELECT DownloadDate,UserID,LogDate  FROM @Attendances      
 OPEN db_fcursor         
 FETCH NEXT FROM db_fcursor INTO @DownloadDate,@UserID,@LogDate      
 WHILE @@FETCH_STATUS = 0         
 BEGIN         
  if(left(@UserID,1)='S')      
  begin      
   select top 1 @iUserID=StudentID,@EmployeeType=-1 from StudentMaster SM where 'STU'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6)=@UserId      
  end      
  else      
  begin      
   select top 1 @iUserID=EmployeeID,@EmployeeType=isnull(EmployeeType,0),@BranchID=SBranchID from [v_EmployeeDriversBasicDetails] where EmployeeSID=@UserId      
  end      
  if(@iUserID is not null)      
  Begin      
   Declare @Direction int=0      
   if(isnull(@DeviceID,'0')<>'0')      
   begin      
    declare @Month nvarchar(2)=datepart(month,@LogDate)      
    declare @Year nvarchar(5)=datepart(year,@LogDate)        
     declare @TableName nvarchar(50)='zDeviceLogs_'+cast(@BranchID as nvarchar(10))+'_'+@Year+'_'+@Month      
     if(not exists(SELECT *  FROM INFORMATION_SCHEMA.TABLES  WHERE  TABLE_NAME =@TableName))      
     begin      
      declare @CreateTableQuery nvarchar(max)      
      set @CreateTableQuery ='Select * into dbo.'+@TableName+'  from  DeviceLogs'         
      exec (@CreateTableQuery)      
     end      
     else      
     begin      
      set @Direction=0      
     end      
     Declare @Day nvarchar(10)      
     declare @DateDiff numeric(10,2)      
     Declare @ShiftSTime time      
     Declare @ShiftETime time      
     declare @SQL nvarchar(max)      
     Declare @Cnt int      
     set @SQL= N'select @Cnt= count(*) from '+@TableName+' where Datediff(second,LogDate,Cast('''+convert(varchar, @LogDate, 25)+''' as DateTime))<60 and Datediff(second,LogDate,Cast('''+convert(varchar, @LogDate, 25)+''' as DateTime))>-60 and UserID='''+
cast(@UserID as nvarchar(10))+''''      
     EXECUTE sp_executesql @SQL, N'@Cnt INTEGER OUTPUT', @Cnt OUTPUT      
     if(@Cnt=0)      
     begin      
      declare @EMPID int      
      declare @StudentID int      
      declare @InsertSQL nvarchar(max)      
      set @InsertSQL='Insert Into '+@TableName+'(DownloadDate,DeviceId,UserId,LogDate,Direction,C1,C2) values(cast('''+CONVERT(varchar,@DownloadDate,113)+''' as datetime),'+@DeviceID+','''+@UserId+''',cast('''+CONVERT(varchar,@LogDate,113)+''' as datetime
),'+cast(@Direction as nchar(1))+','+cast(@ShiftID as nvarchar(5))+','+cast(@EmployeeType as nvarchar(2))+')'           
      exec(@InsertSQL)      
      
      set @Year=datepart(year,@LogDate)      
      set @Month=datepart(month,@LogDate)        
      Set @Day= cast(datepart(day,@LogDate) as nvarchar(3))      
       
      if(left(@UserID,3)='STU')      
      begin      
       select @StudentID=cast(replace(@UserID, 'STU', '') as numeric(10,0))      
       exec [sp_UpdateStudentAttandanceAutoNew] @Year,@Month,@Day,@StudentID,1,@BranchID      
      end      
      else if(left(@UserID,3)='EMP')      
      begin      
       select @EMPID=cast(replace(@UserID, 'EMP', '') as numeric(10,0))      
       exec sp_UpdateEmployeeAttandanceAutoNew @Year,@Month,@Day,@EMPID,1,@BranchID      
      end      
      else if(left(@UserID,3)='TRA')      
      begin      
       select @EMPID=cast(replace(@UserID, 'TRA', '') as numeric(10,0))      
       exec sp_UpdateEmployeeAttandanceAutoNew @Year,@Month,@Day,@EMPID,1,@BranchID      
      end      
     end         
   end      
  end      
  FETCH NEXT FROM db_fcursor INTO @DownloadDate,@UserID,@LogDate      
 END         
 CLOSE db_fcursor         
 DEALLOCATE db_fcursor      
 select 1      
END 
go


alter proc [dbo].[sp_UpdateStudentAttandanceAutoNew]    
(    
@Year nvarchar(5),    
@Month  nvarchar(2),    
@Day nvarchar(3),    
@StudentID int,    
@Status int,    
@SBranchID int    
)    
AS    
BEGIN    
 declare @IsExist int    
 declare @ClassID int    
 declare @SectionID int    
 declare @SQLQuery nvarchar(max)    
 declare @DaysWorking int    
    
 Select top 1 @SectionID=SectionID, @ClassID=ClassID from Student_Session where SBranchID=@SBranchID and StudentID=@StudentID and Cast(@Year+'-'+@Month+'-'+@Day as date) between FromDate and ToDate    
 order by FromDate desc    
    
 select @IsExist=count(*) from StudentAttendanceMasterT where FYear=@Year and [Month]=@Month and StudentID=@StudentID    
 if(@IsExist=1)    
 begin    
  set @SQLQuery='Update StudentAttendanceMasterT set D'+@Day+'='+cast (@Status as nvarchar(5))+' where FYear='+@Year+' and Month='+@Month+' and StudentID='+cast (@StudentID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))    
  exec(@SQLQuery)    
 end    
 else    
 begin      
  set @SQLQuery='Insert into StudentAttendanceMasterT(StudentID,SBranchID,ClassID,SectionID,FYear,Month,D'+@Day+')     
  Values ('+cast (@StudentID as nvarchar(5))+','+cast (@SBranchID as nvarchar(5))+','+cast (@ClassID as nvarchar(5))+','+cast (@SectionID as nvarchar(5))+','+@Year+','+@Month+','+cast(@Status as nvarchar(2))+')'             
    exec(@SQLQuery) 
 end    
END 

go


alter proc [dbo].[sp_UpdateEmployeeAttandanceAutoNew]    
(    
@Year nvarchar(5),    
@Month  nvarchar(2),    
@Day nvarchar(3),    
@EmployeeID int,    
@Status int,    
@SBranchID int    
)    
AS    
BEGIN    
 declare @DesignationID int    
 declare @SectionID int    
 declare @IsExist int    
 declare @DaysWorking int    
 set @DaysWorking=6    
 declare @SQLQuery nvarchar(max)    
    
 Select  @DesignationID=DesignationID from EmployeeMaster where SBranchID=@SBranchID and EmployeeID=@EmployeeID    
 select @IsExist=count(*) from EmployeeAttendanceMasterT where FYear=@Year and [Month]=@Month and EmployeeID=@EmployeeID    
  
 if(@IsExist=1)    
 begin    
  set @SQLQuery='Update EmployeeAttendanceMasterT set D'+@Day+'='+cast (@Status as nvarchar(5))+' where FYear='+@Year+' and Month='+@Month+' and EmployeeID='+cast (@EmployeeID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))    
  exec(@SQLQuery)    
 end    
 else    
 begin       
  set @SQLQuery='Insert into EmployeeAttendanceMasterT(EmployeeID,SBranchID,DesignationID,FYear,Month,D'+@Day+')     
  Values ('+cast (@EmployeeID as nvarchar(5))+','+cast (@SBranchID as nvarchar(5))+','+cast (@DesignationID as nvarchar(5))+','+@Year+','+@Month+','+cast(@Status as nvarchar(2))+')'     
  exec(@SQLQuery)  
  
 end     
    
END    
 go 