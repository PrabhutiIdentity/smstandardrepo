SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[TeacherClassSectionSubjectsByTeacher](@TeacherID int=0)  
Returns @Classes table(ClassID int,SubjectID int,SectionID int,DayID int,PeriodID int,TeacherID int,IsMerger int)     
AS  
BEGIN  
 if(@TeacherID=0)  
 begin  
  Insert into @Classes    
  select ClassID,MondaySubjectID as SubjectID,SectionID,1 as DayID,PeriodID,MondayTeacherID,0 from Time_Table_Master  where isnull(MondayTeacherID,0)!=0  
  Union      
  select ClassID,TuesdaySubjectID as SubjectID,SectionID,2 as DayID,PeriodID,TuesdayTeacherID,0 from Time_Table_Master    where isnull(TuesdayTeacherID,0)!=0  
  Union      
  select ClassID,WednesdaySubjectID as SubjectID,SectionID,3 as DayID,PeriodID,WednesdayTeacherID,0 from Time_Table_Master where isnull(wednesdayTeacherID,0)!=0  
  Union      
  select ClassID,ThursdaySubjectID as SubjectID,SectionID,4 as DayID,PeriodID,ThursdayTeacherID,0from Time_Table_Master where isnull(ThursdayTeacherID,0)!=0  
  Union      
  select ClassID,FridaySubjectID as SubjectID,SectionID,5 as DayID,PeriodID,FridayTeacherID,0 from Time_Table_Master where isnull(FridayTeacherID,0)!=0      
  Union      
  select ClassID,SaturdaySubjectID as SubjectID,SectionID,6 as DayID,PeriodID,SaturdayTeacherID,0 from Time_Table_Master where isnull(SaturdayTeacherID,0)!=0  
    end  
 else  
 begin  
 Insert into @Classes    
 select ClassID,MondaySubjectID as SubjectID,SectionID,1 as DayID,PeriodID,MondayTeacherID,0 from Time_Table_Master  where isnull(MondayTeacherID,0)=@TeacherID  
  Union      
  select ClassID,TuesdaySubjectID as SubjectID,SectionID,2 as DayID,PeriodID,TuesdayTeacherID,0 from Time_Table_Master    where isnull(TuesdayTeacherID,0)=@TeacherID  
  Union      
  select ClassID,WednesdaySubjectID as SubjectID,SectionID,3 as DayID,PeriodID,WednesdayTeacherID,0 from Time_Table_Master where isnull(wednesdayTeacherID,0)=@TeacherID  
  Union      
  select ClassID,ThursdaySubjectID as SubjectID,SectionID,4 as DayID,PeriodID,ThursdayTeacherID,0from Time_Table_Master where isnull(ThursdayTeacherID,0)=@TeacherID  
  Union      
  select ClassID,FridaySubjectID as SubjectID,SectionID,5 as DayID,PeriodID,FridayTeacherID,0 from Time_Table_Master where isnull(FridayTeacherID,0)=@TeacherID      
  Union      
  select ClassID,SaturdaySubjectID as SubjectID,SectionID,6 as DayID,PeriodID,SaturdayTeacherID,0 from Time_Table_Master where isnull(SaturdayTeacherID,0)=@TeacherID  
  
 end  
  
 insert into @Classes  
 select CS.ClassID,CMM.SubjectID,CMM.SecondSectionID,CMM.DayID,CMM.PeriodID,CL.TeacherID,1  
 from [dbo].[Class_Merge_Master] CMM left outer join Class_Sections CS on CMM.SecondSectionID=CS.ID  
 left outer join @Classes CL on CL.SectionID=CMM.FirstSectionID and CL.PeriodID=CMM.PeriodID and CL.DayID=CMM.DayID  
 where CL.ClassID is not null and isnull(CL.TeacherID,0)!=0  
   
 Return  
END  
GO
CREATE TABLE [dbo].[BBBOnlineClassMaster](
	[OCID] [int] IDENTITY(1,1) NOT NULL,
	[TeacherID] [int] NULL,
	[SectionID] [int] NULL,
	[SubjectID] [int] NULL,
	[SessionID] [int] NULL,
	[SBranchID] [int] NULL,
	[Status] [int] NULL,
	[ClassDate] [date] NULL,
	[StartTime] [nvarchar](12) NULL,
	[EndTime] [nvarchar](12) NULL,
	[CreatedDate] [datetime] NULL,
	[MeetingID] [nvarchar](50) NULL,
	[InternalMeetingID] [nvarchar](50) NULL,
	[StartedOn] [datetime] NULL,
	[EndedOn] [datetime] NULL,
	[GroupID] [int] NULL,
	[ModPassword] [nvarchar](20) NULL,
	[AttPassword] [nvarchar](20) NULL,
	[Attendees] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[BBBOnlineClassRecordings](
	[RecID] [int] IDENTITY(1,1) NOT NULL,
	[MeetingID] [nvarchar](50) NULL,
	[PlaybackURL] [nvarchar](500) NULL,
	[RecordingState] [nvarchar](50) NULL,
	[RawRecordingSize] [int] NULL,
	[ProcessedRecordingSize] [int] NULL,
	[Thumbnail] [nvarchar](1000) NULL,
	[RecordingStartTime] [numeric](18, 0) NULL,
	[RecordingEndTime] [numeric](18, 0) NULL
) ON [PRIMARY]
GO

CREATE view [dbo].[v_StudentSessionTop1Entry]  
AS  
SELECT TOP 1 WITH TIES * FROM dbo.Student_Session  
ORDER BY ROW_NUMBER() OVER(PARTITION BY StudentID,SessionID ORDER BY StudentID,SessionID,Status DESC)  
GO
CREATE  procedure [dbo].[sp_AddBBBOnlineClass]
(
@TeacherID int,
@SectionID int,
@GroupID int,
@SubjectID int,
@SessionID int,
@SBranchID int,
@Status int,
@ClassDate date,
@StartTime nvarchar(15),
@EndTime nvarchar(15),
@CreatedDate datetime,
@MeetingID nvarchar(50),
@InternalMeetingID nvarchar(50)
)
AS
Begin
	if(@SBranchID=0)
	begin
		select @SBranchID=SBranchID from EmployeeMaster where EmployeeID=@TeacherID
	end
	select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc

	Insert into [dbo].[BBBOnlineClassMaster](TeacherID,SectionID,SubjectID,GroupID,SessionID,SBranchID,Status,ClassDate,StartTime,EndTime,CreatedDate,
	MeetingID,InternalMeetingID)
	values(@TeacherID,@SectionID,@SubjectID,@GroupID,@SessionID,@SBranchID,@Status,@ClassDate,@StartTime,@EndTime,@CreatedDate,
	@MeetingID,@InternalMeetingID)
	select Cast(Scope_Identity() as int)
End
GO
CREATE procedure [dbo].[sp_GetStudentBBBWebOnlineClass] -- 5,'2020-08-26'
(    
@StudentID int,    
@CurDate date  
)    
AS    
BEGIN    
 declare @ClassID int, @SectionID int,@StudentSessionUID int,@SessionID int      
 select top 1 @SectionID=SectionID,@ClassID=ClassID,@StudentSessionUID=StudentSessionUID ,@SessionID=SessionID      
 from v_StudentSessionTop1Entry where studentID=@StudentID order by Status desc    
    
 select OCM.OCID,OCM.Status,OCM.MeetingID,OCM.ClassDate,OCM.StartTime,OCM.EndTime,OCM.StartedOn,CSN.ClassName+'/'+CSN.SectionName as ClassSection,OCM.SectionID,    
 SM.SubjectName,OCM.SubjectID,OCM.MeetingID,OCM.InternalMeetingID,EM.EmployeeName as TeacherName,    
 JSON_QUERY((Select * from BBBOnlineClassRecordings R where R.MeetingID=OCM.MeetingID For JSON Path)) as RecordingJSON    
 from BBBOnlineClassMaster OCM left outer join v_ClassSectionNames CSN on CSN.SectionID=OCM.SectionID    
 left outer join SubjectMasterAll SM on SM.SubjectID=OCM.SubjectID    
  left outer join EmployeeMaster EM on EM.EmployeeID=OCM.TeacherID    
 where cast(OCM.ClassDate as Date)=@CurDate and OCM.SectionID=@SectionID and SessionID=@SessionID    
 order by (case when OCM.Status=1 then -1 else 0 end), ClassDate,CAST(StartTime AS DATETIME)    
END
GO
GO

CREATE procedure [dbo].[sp_GetTeacherBBBWebOnlineClasses]
(
@TeacherID int,
@CurDate date,
@SBranchID int
)
AS
BEGIN
	declare @Classes table(ClassID int,SubjectID int,SectionID int)          
	Insert into @Classes           
	select ClassID,SubjectID,SectionID from dbo.TeacherClassSectionSubjectsByTeacher(@TeacherID) 
	


	select OCM.OCID,OCM.Status,OCM.MeetingID,OCM.ClassDate,OCM.StartTime,OCM.EndTime,OCM.StartedOn,CSN.ClassName+'/'+CSN.SectionName as ClassSection,OCM.SectionID,
	SM.SubjectName,OCM.SubjectID,OCM.MeetingID,OCM.InternalMeetingID,
	JSON_QUERY((Select * from BBBOnlineClassRecordings R where R.MeetingID=OCM.MeetingID For JSON Path)) as RecordingJSON
	from BBBOnlineClassMaster OCM left outer join v_ClassSectionNames CSN on CSN.SectionID=OCM.SectionID
	left outer join SubjectMasterAll SM on SM.SubjectID=OCM.SubjectID
	where OCM.ClassDate>=@CurDate and TeacherID=@TeacherID
	order by ClassDate,CAST(StartTime AS DATETIME)

	Select distinct SectionID as ID,ClassName+'/'+SectionName as Name from v_ClassSectionNames where SectionID in (select SectionID from @Classes)

	select distinct C.SubjectID as ID, SM.SubjectName as Name ,C.SectionID as Extra1
	from @Classes C left outer join SubjectMasterAll SM on SM.SubjectID=C.SubjectID
	where SM.SubjectID is not Null

	select SessionID as ID, SessionName as Name, SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID
END
GO
    
CREATE procedure [dbo].[sp_GetTeacherBBBWebOnlineClassSchedules]    
(    
@TeacherID int,    
@CurDate date,    
@SBranchID int    
)    
AS    
BEGIN      
 select OCM.OCID,OCM.Status,OCM.MeetingID,OCM.ClassDate,OCM.StartTime,OCM.EndTime,OCM.StartedOn,CSN.ClassName+'/'+CSN.SectionName as ClassSection,OCM.SectionID,    
 SM.SubjectName,OCM.SubjectID,OCM.MeetingID,OCM.InternalMeetingID,    
 JSON_QUERY((Select * from BBBOnlineClassRecordings R where R.MeetingID=OCM.MeetingID For JSON Path)) as RecordingJSON    
 from BBBOnlineClassMaster OCM left outer join v_ClassSectionNames CSN on CSN.SectionID=OCM.SectionID    
 left outer join SubjectMasterAll SM on SM.SubjectID=OCM.SubjectID    
 where OCM.ClassDate=@CurDate and TeacherID=@TeacherID    
 order by Status desc,ClassDate,CAST(StartTime AS DATETIME)      
END
GO

CREATE procedure [dbo].[sp_StartBBBOnlineClass]
(
@TeacherID int,
@OCID int,
@StartedOn datetime
)
AS
BEGIN
	Update BBBOnlineClassMaster set StartedOn=@StartedOn,Status=1 where OCID=@OCID and TeacherID=@TeacherID
	select @@rowcount
END
GO
  
CREATE procedure [dbo].[sp_UpdateBBBOnlineClassDetailonEnd]    
(    
@OCID int,    
@MeetingID nvarchar(50),    
@Status int,    
@CurDate datetime    ,
@Attendees nvarchar(max)
)    
AS    
BEGIN    
  Update BBBOnlineClassMaster set Status=2,EndedOn=@CurDate  ,Attendees=@Attendees  
  where OCID=@OCID or MeetingID=@MeetingID    
  select @@rowcount    
END
GO

CREATE procedure [dbo].[sp_UpdateBBBOnlineClassDetailonStart]
(
@OCID int,
@MeetingID nvarchar(50),
@InternalMeetingID nvarchar(50),
@AttPassword nvarchar(20),
@ModPassword nvarchar(20),
@Status int,
@CurDate datetime
)
AS
BEGIN
	Update BBBOnlineClassMaster set InternalMeetingID=@InternalMeetingID,Status=1,StartedOn=@CurDate,AttPassword=@AttPassword,
	ModPassword=@ModPassword
	where OCID=@OCID or MeetingID=@MeetingID
	select @@rowcount
END
GO

CREATE procedure [dbo].[sp_UpdateBBBRecordingStatus]
(
@MeetingID nvarchar(50),
@PlayBackURL nvarchar(500),
@RecordingState nvarchar(50),
@RawRecordingSize int,
@ProcessedRecordingSize int,
@Thumbnail nvarchar(1000),
@RecordingStartTime numeric(10,2),
@RecordingEndTime numeric(10,2)
)
AS
BEgin
	Insert into BBBOnlineClassRecordings(MeetingID,PlaybackURL,RecordingState,RawRecordingSize,ProcessedRecordingSize,Thumbnail,RecordingStartTime,RecordingEndTime)
	values(@MeetingID,@PlaybackURL,@RecordingState,@RawRecordingSize,@ProcessedRecordingSize,@Thumbnail,@RecordingStartTime,@RecordingEndTime)	
End
GO
CREATE procedure [dbo].[spn_GetRecieverListForBBBOnlineClass]     
(      
@OCID int     
)      
as       
begin        
 declare @SectionID int, @SessionID int,@SBranchID int    
    
 select @SectionID=SectionID,@SessionID=SessionID,@SBranchID=SBranchID from BBBOnlineClassMaster where OCID=@OCID    
    
   select ParentID as ID,FatherName as Name,FatherMobileNo as MobileNo, 4 as RecieverType, AU.FCMToken as deviceToken     
   from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4      
   where ParentID in (Select ParentID from StudentMaster where StudentID in    
   (select StudentID from Student_Session where SectionID=@SectionID and SessionID=@SessionID)) and isnull(AU.FCMToken,'')!=''    
     union   
 select UserID as ID,'Principal' as Name,'' as MobileNo,8 as RecieverType,AU.FCMToken as deviceToken  
 from AppUsers AU where UserType=8 and SBranchID=@SBranchID and Status=1  
end 
GO
CREATE procedure [dbo].[sp_GetTeacherBBBWebOnlineClassListOnly]  
(  
@TeacherID int,  
@CurDate date,  
@SBranchID int  
)  
AS  
BEGIN  
 select OCM.OCID,OCM.Status,OCM.MeetingID,OCM.ClassDate,OCM.StartTime,OCM.EndTime,OCM.StartedOn,CSN.ClassName+'/'+CSN.SectionName as ClassSection,OCM.SectionID,  
 SM.SubjectName,OCM.SubjectID,OCM.MeetingID,OCM.InternalMeetingID,  
 JSON_QUERY((Select * from BBBOnlineClassRecordings R where R.MeetingID=OCM.MeetingID For JSON Path)) as RecordingJSON  
 from BBBOnlineClassMaster OCM left outer join v_ClassSectionNames CSN on CSN.SectionID=OCM.SectionID  
 left outer join SubjectMasterAll SM on SM.SubjectID=OCM.SubjectID  
 where cast(OCM.ClassDate as date)>=@CurDate and TeacherID=@TeacherID  
 order by ClassDate,CAST(StartTime AS DATETIME)  
  
END  

GO

Create Procedure dbo.sp_GetStudentBasicDetailOnID
(
@StudentId int
)
AS
BEGIN
	Select SM.StudentID, Name,'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) as [StudentSID],SM.SchoolUID,
	isnull('/images/StudentImage/'+cast(SM.StudentID as nvarchar(10))+'_'+Photo,
	(case when Gender=0 then '~/Images/MaleNoImage.png' else '~/Images/FemaleNoImage.png' end)) as Photo 
	from StudentMaster SM
 where StudentID=@StudentId
END

GO

CREATE procedure dbo.GetMeetingDetailsOnOCID    
(    
@OCID int    
)    
AS    
BEGIN    
 Select OCM.OCID,OCM.Status,OCM.MeetingID,OCM.ClassDate,OCM.StartTime,OCM.EndTime,OCM.StartedOn,CSN.ClassName+'/'+CSN.SectionName as ClassSection,OCM.SectionID,    
 SM.SubjectName,OCM.SubjectID,OCM.MeetingID,OCM.InternalMeetingID,    
 JSON_QUERY((Select * from BBBOnlineClassRecordings R where R.MeetingID=OCM.MeetingID For JSON Path)) as RecordingJSON    
 ,EM.EmployeeName as TeacherName,AttPassword,ModPassword ,Attendees as AttendeesJSON   
 from  BBBOnlineClassMaster OCM    
 left outer join v_ClassSectionNames CSN on CSN.SectionID=OCM.SectionID    
 left outer join SubjectMasterAll SM on SM.SubjectID=OCM.SubjectID    
 left outer join EmployeeMaster EM on EM.EmployeeID=OCM.TeacherID    
 where OCID=@OCID    
END

GO

Create Procedure dbo.sp_GetTeacherBasicDetailOnID
(
@TeacherId int
)
AS
BEGIN
	Select SM.EmployeeID, EmployeeName,'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.[EmployeeID] AS VARCHAR(6)),6) as [EmployeeSID],
	isnull('/images/StudentImage/'+cast(SM.EmployeeID as nvarchar(10))+'_'+Photo,
	(case when Gender=0 then '~/Images/MaleNoImage.png' else '~/Images/FemaleNoImage.png' end)) as Photo 
	from EmployeeMaster SM
 where EmployeeId=@TeacherId
END

GO

Create Procedure dbo.sp_GetStudentBasicDetailOnMeetingID
(  
@OCID int,
@ParentID int
)  
AS  
BEGIN  
	declare @StudentID int,@SectionID int,@SessionID int
	select @SectionID=SectionID,@SessionID=SessionID from [BBBOnlineClassMaster] where OCID=@OCID
	
	 Select SM.StudentID, Name,'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) as [StudentSID],SM.SchoolUID,  
	 isnull('/images/StudentImage/'+cast(SM.StudentID as nvarchar(10))+'_'+Photo,  
	 (case when Gender=0 then '~/Images/MaleNoImage.png' else '~/Images/FemaleNoImage.png' end)) as Photo   
	 from StudentMaster SM  
	 where ParentID=@ParentID and StudentID in (select StudentID from Student_Session where SessionID=@SessionID and SectionID=@SectionID)  
END
GO
CREATE  procedure dbo.sp_GetPrincipalBBBOnlineClass      
(        
@SbranchID int,       
@CurDate date      
)        
AS        
BEGIN        
    
	 select OCM.OCID,OCM.Status,OCM.MeetingID,OCM.ClassDate,OCM.StartTime,OCM.EndTime,OCM.StartedOn,CSN.ClassName+'/'+CSN.SectionName as ClassSection,OCM.SectionID,    
	 SM.SubjectName,OCM.SubjectID,OCM.MeetingID,OCM.InternalMeetingID,    
	 JSON_QUERY((Select * from BBBOnlineClassRecordings R where R.MeetingID=OCM.MeetingID For JSON Path)) as RecordingJSON    
	 from BBBOnlineClassMaster OCM left outer join v_ClassSectionNames CSN on CSN.SectionID=OCM.SectionID    
	 left outer join SubjectMasterAll SM on SM.SubjectID=OCM.SubjectID    
	 where OCM.ClassDate=@CurDate and OCM.SbranchID=@SbranchID    
	 order by Status desc,ClassDate,CAST(StartTime AS DATETIME) 

END 

GO

CREATE TABLE [dbo].[BBBOnlineClassAttendees](
	[OCID] [int] NULL,
	[StudentID] [int] NULL,
	[AppType] [int] NULL,
	[StartTime] [nvarchar](12) NULL,
	[EndTime] [nvarchar](12) NULL
) ON [PRIMARY]
GO
CREATE procedure dbo.sp_JoinStudentBBBOnlineClass        
(        
@StudentID int,        
@JoinDate datetime,        
@OCID int  ,      
@AppType int,
@ParentID int=0      
)        
AS        
BEGIN        
   
  declare @SectionID int, @SessionID int,@SBranchID int ,@MeetingID nvarchar(100),@SubjectID int,@UpdatedRows int,@SubjectName nvarchar(50)    
  select @SectionID=SectionID,@SessionID=SessionID,@SBranchID=SBranchID,@MeetingID=MeetingID,@SubjectID=SubjectID from BBBOnlineClassMaster where OCID=@OCID      
  if(@StudentID=0)      
  begin      
   select top 1 @StudentID=StudentID from Student_Session where SectionID=@SectionID and SessionID=@SessionID       
   and StudentID in (Select StudentID from StudentMaster where ParentID=@ParentID)      
  end      
  Insert into BBBOnlineClassAttendees(OCID,StudentID,StartTime,AppType)        
  values(@OCID,@StudentID,cast(cast(@JoinDate as time) as nvarchar(12)),@AppType)        
  --select @UpdatedRows= @@rowcount    
  --select @SubjectName=SubjectName from SubjectMaster where SubjectID=@SubjectID    
     
 Select Name,'/Images/StudentImage/'+Cast(StudentID as nvarchar(10))+'_'+Photo as Extra1,StudentId as ID
 from StudentMaster where StudentID=@StudentID    
END 
GO
Alter procedure [dbo].[sp_GetTeacherBBBWebOnlineClassSchedules]    
(    
@TeacherID int,    
@CurDate date,    
@SBranchID int    
)    
AS    
BEGIN      
 select OCM.OCID,OCM.Status,OCM.MeetingID,OCM.ClassDate,OCM.StartTime,OCM.EndTime,OCM.StartedOn,CSN.ClassName+'/'+CSN.SectionName as ClassSection,OCM.SectionID,    
 SM.SubjectName,OCM.SubjectID,OCM.MeetingID,OCM.InternalMeetingID,    
 JSON_QUERY((Select * from BBBOnlineClassRecordings R where R.MeetingID=OCM.MeetingID For JSON Path)) as RecordingJSON ,
 JSON_QUERY((Select SM.StudentID,SM.Name,BA.AppType,BA.StartTime,BA.EndTime 
 from BBBOnlineClassAttendees BA left outer join StudentMaster SM on SM.StudentId=BA.StudentID
 where BA.OCID=OCM.OCID For JSON Path)) as AttendeesJSON     
 from BBBOnlineClassMaster OCM left outer join v_ClassSectionNames CSN on CSN.SectionID=OCM.SectionID    
 left outer join SubjectMasterAll SM on SM.SubjectID=OCM.SubjectID    
 where OCM.ClassDate=@CurDate and TeacherID=@TeacherID    
 order by Status desc,ClassDate,CAST(StartTime AS DATETIME)      
END
GO
Alter  procedure dbo.sp_GetPrincipalBBBOnlineClass      
(        
@SbranchID int,       
@CurDate date      
)        
AS        
BEGIN        
    
	 select OCM.OCID,OCM.Status,OCM.MeetingID,OCM.ClassDate,OCM.StartTime,OCM.EndTime,OCM.StartedOn,CSN.ClassName+'/'+CSN.SectionName as ClassSection,OCM.SectionID,    
	 SM.SubjectName,OCM.SubjectID,OCM.MeetingID,OCM.InternalMeetingID,    
	 JSON_QUERY((Select * from BBBOnlineClassRecordings R where R.MeetingID=OCM.MeetingID For JSON Path)) as RecordingJSON   ,
     JSON_QUERY((Select SM.StudentID,SM.Name,BA.AppType,BA.StartTime,BA.EndTime 
     from BBBOnlineClassAttendees BA left outer join StudentMaster SM on SM.StudentId=BA.StudentID
     where BA.OCID=OCM.OCID For JSON Path)) as AttendeesJSON       
	 from BBBOnlineClassMaster OCM left outer join v_ClassSectionNames CSN on CSN.SectionID=OCM.SectionID    
	 left outer join SubjectMasterAll SM on SM.SubjectID=OCM.SubjectID    
	 where OCM.ClassDate=@CurDate and OCM.SbranchID=@SbranchID    
	 order by Status desc,ClassDate,CAST(StartTime AS DATETIME) 

END 
GO
ALTER procedure [dbo].[sp_GetTeacherBBBWebOnlineClassListOnly]  
(  
@TeacherID int,  
@CurDate date,  
@SBranchID int  
)  
AS  
BEGIN  
 select OCM.OCID,OCM.Status,OCM.MeetingID,OCM.ClassDate,OCM.StartTime,OCM.EndTime,OCM.StartedOn,CSN.ClassName+'/'+CSN.SectionName as ClassSection,OCM.SectionID,  
 SM.SubjectName,OCM.SubjectID,OCM.MeetingID,OCM.InternalMeetingID,  
 JSON_QUERY((Select * from BBBOnlineClassRecordings R where R.MeetingID=OCM.MeetingID For JSON Path)) as RecordingJSON ,
 JSON_QUERY((Select SM.StudentID,SM.Name,BA.AppType,BA.StartTime,BA.EndTime 
 from BBBOnlineClassAttendees BA left outer join StudentMaster SM on SM.StudentId=BA.StudentID
 where BA.OCID=OCM.OCID For JSON Path)) as AttendeesJSON      
 from BBBOnlineClassMaster OCM left outer join v_ClassSectionNames CSN on CSN.SectionID=OCM.SectionID  
 left outer join SubjectMasterAll SM on SM.SubjectID=OCM.SubjectID  
 where cast(OCM.ClassDate as date)>=@CurDate and TeacherID=@TeacherID  
 order by ClassDate,CAST(StartTime AS DATETIME)  
  
END 

GO

CREATE procedure dbo.sp_GetBBBOnlineClassAttendees  
(  
@OCID int  
)  
AS  
BEGIN  
 Select SM.StudentID,SM.Name,BA.AppType,BA.StartTime,BA.EndTime 
 from BBBOnlineClassAttendees BA left outer join StudentMaster SM on SM.StudentId=BA.StudentID
 where BA.OCID=@OCID  
 order by StartTime   
END  
  
GO

CREATE TABLE [dbo].[BBBOnlineMeetingMaster](
	[OCID] [int] IDENTITY(1,1) NOT NULL,
	[Agenda] [nvarchar](500) NULL,
	[SBranchID] [int] NULL,
	[Status] [int] NULL,
	[ClassDate] [date] NULL,
	[StartTime] [nvarchar](12) NULL,
	[EndTime] [nvarchar](12) NULL,
	[CreatedDate] [datetime] NULL,
	[MeetingID] [nvarchar](50) NULL,
	[InternalMeetingID] [nvarchar](50) NULL,
	[StartedOn] [datetime] NULL,
	[EndedOn] [datetime] NULL,
	[GroupID] [int] NULL,
	[ModPassword] [nvarchar](20) NULL,
	[AttPassword] [nvarchar](20) NULL,
	[Attendees] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-------------------------------------------------------STAFF MEETING RELATED-----------------------------------------------------

CREATE TABLE [dbo].[BBBOnlineStaffMeetings](
	[MeetingID] [int] IDENTITY(1,1) NOT NULL,
	[MeetingDate] [date] NULL,
	[StartTime] [nvarchar](50) NULL,
	[EndTime] [nvarchar](50) NULL,
	[MeetingTitle] [nvarchar](50) NULL,
	[Status] [int] NULL,
	[SBranchID] [int] NULL,
	[BBBMeetingID] [nvarchar](50) NULL,
	[InternalMeetingID] [nvarchar](50) NULL,
	[StartedOn] [datetime] NULL,
	[EndedOn] [datetime] NULL,
	[ModPassword] [nvarchar](20) NULL,
	[AttPassword] [nvarchar](20) NULL,
	[Attendees] [nvarchar](max) NULL
) ON [PRIMARY]
GO

CREATE procedure [dbo].[spn_GetRecieverListForBBBBStaffMeeting]      
(        
@MeetingID int      
)        
as        
begin          
   select EmployeeID as ID,EmployeeName as Name,MobileNumber as MobileNo, 3 as RecieverType, AU.FCMToken as deviceToken      
   from EmployeeMaster R left outer join AppUsers AU on R.EmployeeID=AU.UserID and AU.UserType=3        
   where EmployeeType=3 and isnull(AU.FCMToken,'')!=''          
end 

GO


Create procedure dbo.sp_GetBBBStaffMeetings  
(  
@CurDate date,  
@SBranchID int  
)  
AS  
BEGIN  
Select * from [dbo].[BBBOnlineStaffMeetings] where MeetingDate=@CurDate and SBranchID=@SBranchID  
END  

GO
CREATE TABLE [dbo].[BBBStaffMeetingAttendees](
	[MeetingID] [int] NULL,
	[TeacherID] [int] NULL,
	[AppType] [int] NULL,
	[StartTime] [nvarchar](12) NULL,
	[EndTime] [nvarchar](12) NULL
) ON [PRIMARY]
GO

Create procedure [dbo].[sp_JoinBBBStaffMeetingGetDetails]        
(        
@TeacherID int,        
@JoinDate datetime,        
@MeetingID int  ,      
@AppType int     
)        
AS        
BEGIN        
   
  
  Insert into BBBStaffMeetingAttendees(MeetingID,TeacherID,StartTime,AppType)        
  values(@MeetingID,@TeacherID,cast(cast(@JoinDate as time) as nvarchar(12)),@AppType)  
     
 Select EmployeeName as Name,'/Images/EmployeeImage/'+Cast(EmployeeID as nvarchar(10))+'_'+Photo as Extra1,EmployeeID as ID
 from EmployeeMaster where EmployeeID=@TeacherID    
END 

GO
Create procedure dbo.sp_AddBBBStaffMeeting  
(  
@MeetingDate date,  
@StartTime nvarchar(50),  
@EndTime nvarchar(50),  
@MeetingTitle nvarchar(50),  
@SBranchID int  
)  
AS  
BEGIN  
Insert into [dbo].[BBBOnlineStaffMeetings](MeetingDate,StartTime,EndTime,MeetingTitle,Status,SBranchID)  
values(@MeetingDate,@StartTime,@EndTime,@MeetingTitle,0,@SBranchID)  
select Cast(Scope_Identity() as int)  
END  

GO



Create procedure [dbo].[sp_UpdateBBBStaffMeetingDetailonStart]
(
@MeetingID int,
@BBBMeetingID nvarchar(50),
@InternalMeetingID nvarchar(50),
@AttPassword nvarchar(20),
@ModPassword nvarchar(20),
@Status int,
@CurDate datetime
)
AS
BEGIN
	Update [dbo].[BBBOnlineStaffMeetings] set InternalMeetingID=@InternalMeetingID,Status=1,StartedOn=@CurDate,AttPassword=@AttPassword,
	ModPassword=@ModPassword
	where MeetingID=@MeetingID or BBBMeetingID=@BBBMeetingID
	select @@rowcount
END

GO

Create procedure dbo.sp_GetBBBStaffMeetingDetails    
(    
@MeetingID int ,
@SBranchID int    
)    
AS    
BEGIN    
Select * from [dbo].[BBBOnlineStaffMeetings] where MeetingID=@MeetingID and SBranchID=@SBranchID    
END

GO

Create procedure dbo.sp_GetBBBStaffMeetingDetailsByMeetingId   
(    
@MeetingID nvarchar(50) ,
@SBranchID int    
)    
AS    
BEGIN    
Select * from [dbo].[BBBOnlineStaffMeetings] where BBBMeetingID=@MeetingID --and SBranchID=@SBranchID    
END 
GO

Create procedure [dbo].[sp_UpdateBBBOnlineMeetionDetailonEnd]    
(    
@MeetingId int,    
@BBBMeetingID nvarchar(50), 
@CurDate datetime    ,
@Attendees nvarchar(max)
)    
AS    
BEGIN    
  Update BBBOnlineStaffMeetings set Status=2,EndedOn=@CurDate  ,Attendees=@Attendees  
  where MeetingId=@MeetingID or BBBMeetingID=@BBBMeetingID    
  select @@rowcount    
END
GO
GO

CREATE TABLE [dbo].[BBBOnlineMeetingRecordings](
	[RecID] [int] IDENTITY(1,1) NOT NULL,
	[MeetingID] [nvarchar](50) NULL,
	[PlaybackURL] [nvarchar](500) NULL,
	[RecordingState] [nvarchar](50) NULL,
	[RawRecordingSize] [int] NULL,
	[ProcessedRecordingSize] [int] NULL,
	[Thumbnail] [nvarchar](1000) NULL,
	[RecordingStartTime] [numeric](18, 0) NULL,
	[RecordingEndTime] [numeric](18, 0) NULL
) ON [PRIMARY]
GO

CREATE procedure [dbo].[sp_UpdateBBBMeetingRecordingStatus]  
(  
@MeetingID nvarchar(50),  
@PlayBackURL nvarchar(500),  
@RecordingState nvarchar(50),  
@RawRecordingSize int,  
@ProcessedRecordingSize int,  
@Thumbnail nvarchar(1000),  
@RecordingStartTime numeric(10,2),  
@RecordingEndTime numeric(10,2)  
)  
AS  
BEgin  
 Insert into [BBBOnlineMeetingRecordings](MeetingID,PlaybackURL,RecordingState,RawRecordingSize,ProcessedRecordingSize,Thumbnail,RecordingStartTime,RecordingEndTime)  
 values(@MeetingID,@PlaybackURL,@RecordingState,@RawRecordingSize,@ProcessedRecordingSize,@Thumbnail,@RecordingStartTime,@RecordingEndTime)   
End  