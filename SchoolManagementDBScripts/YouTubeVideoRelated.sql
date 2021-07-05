
CREATE TABLE [dbo].[YouTubeVideos](
	[VideoID] [int] IDENTITY(1,1) NOT NULL,
	[TeacherID] [int] NULL,
	[UploadDate] [datetime] NULL,
	[SubjectID] [int] NULL,
	[Title] [nvarchar](1000) NULL,
	[Description] [nvarchar](max) NULL,
	[YouTubeID] [nvarchar](50) NULL,
	[ClassID] [int] NULL,
	[SectionID] [int] NULL,
	[SequenceNo] [int] NULL,
	[SBranchID] [int] NULL,
	[VideoDate] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


Create  Procedure [dbo].[sp_AddYouTubeVideo]
(
@TeacherID int,
@UploadDate datetime,
@SubjectID int,
@Title nvarchar(1000),
@Description nvarchar(max),
@YoutubeID nvarchar(50),
@ClassID int,
@SectionID int,
@SequenceNo int,
@SBranchID int,
@VideoDate datetime
)
AS
BEGIN
	Insert into [dbo].[YouTubeVideos](TeacherID,UploadDate,SubjectID,Title,Description,YouTubeID,ClassID,SectionID,SequenceNo,VideoDate,SBranchID)
		values(@TeacherID,@UploadDate,@SubjectID,@Title,@Description,@YouTubeID,@ClassID,@SectionID,@SequenceNo,@VideoDate,@SBranchID)

	select cast(Scope_Identity() as int)
END


GO
Create Procedure dbo.sp_GetYouTubeVideo
(
@TeacherID int,
@SubjectID int,
@ClassID int,
@SectionID int,
@SBranchID int
)
AS
BEGIN
	if(@TeacherID=0)
	begin
		select * from [dbo].[YouTubeVideos] where ClassID=@ClassID and SectionID=@SectionID and SubjectID=@SubjectID
	end
	else
	begin
		select * from [dbo].[YouTubeVideos] where ClassID=@ClassID and SectionID=@SectionID and SubjectID=@SubjectID and TeacherID=@TeacherID
	end
END

GO


Create Procedure dbo.sp_GetYouTubeVideoParent
(
@StudentID int,
@SubjectID int
)
AS
BEGIN
	declare @ClassID int, @SectionID int
	
	select top 1 @ClassID=ClassID,@SectionID=SectionID from Student_Session where StudentID=@StudentID order by Status desc
		select * from [dbo].[YouTubeVideos] where ClassID=@ClassID and SectionID=@SectionID and SubjectID=@SubjectID
	
END
GO
Create Procedure dbo.sp_GetSubjectsForStudent
(
@StudentID int
)
AS
BEGIN
	Declare @SectionID int,@GroupID int
	select top 1 @SectionID=SectionID from Student_Session where StudentID=@StudentID
	order by Status desc

	exec [dbo].[spn_GetSubjectsForSections] @SectionID
END

GO
GO
ALTER proc [dbo].[sp_GetAppGalleryList]
(
@SBranchID int
)
AS
BEGIN
	if(@SBranchID=0)
	begin
		Select GalleryID,Title,EventDate,'/images/GalleryImages/' as BasePath,
		(select top 1 ImagePath from GalleryImages GI where GI.GalleryID=GM.GalleryID order by GalleryImageID) as FeatureImage
		from GalleryMaster GM where [Status]=1
		order by EventDate desc
	end
	else
	begin
		Select GalleryID,Title,EventDate,'/images/GalleryImages/' as BasePath,
		(select top 1 ImagePath from GalleryImages GI where GI.GalleryID=GM.GalleryID order by GalleryImageID) as FeatureImage
		from GalleryMaster GM 
		where isnull(SBranchID,@SBranchID)=@SBranchID and [Status]=1
		order by EventDate desc
	end
END
GO
Create Procedure [dbo].[sp_GetStopWiseCollectionStudents] --'2019-06-04',1
(
@MStartDate date,
@SBranchID int
)
As
Begin
declare @StartDay date =DATEADD(mm, DATEDIFF(mm, 0, @MStartDate) - 1, 0)

--select @FirstDay

--Declare @MEndDate date=eomonth(@MStartDate)
Declare @MEndDate date=DATEADD(DAY, -(DAY(@MStartDate)), @MStartDate)

Select AM.AreaID,AM.AreaName,Sum(Amount)as Amount,count(users.UserID) as Students from (
select StopID,RouteID,AreaID,Rate,TAmount.Amount from [dbo].[TransportRouteDetails] TRD left outer join 
(Select KeyID,isnull(sum(Amount*(Days+1)/TotalDays),0) as Amount from (
				select StartDate,EndDate,KeyID,ChangeType,DATEDIFF(DAY, StartDate,isnull(EndDate,@MEndDate)) as Days,datepart(day,@MEndDate) as TotalDays,
				(select isnull(Rate,0) from TransportRouteDetails TRM where TRM.StopID=KeyID) as Amount from 
				(select (case when  StartDate is null or StartDate<@StartDay then @StartDay else StartDate end) as StartDate,
				(case when EndDate is null or EndDate>@MEndDate then @MEndDate else EndDate end) as EndDate
				, KeyID,ChangeType
				from TransportHostalAllocationDelocation where UserType=0 and ChangeType=0 and (StartDate<EndDate or EndDate is null) and UserID in (Select StudentID from Student_Session where SBranchID=@SBranchID)
				and ((StartDate between @StartDay and @MEndDate or StartDate<@StartDay) and (isnull(EndDate,@MEndDate) between @StartDay and @MEndDate or EndDate>@MEndDate))
				 )t)tt
				 group by KeyID) TAmount
				 on TRD.StopID=TAmount.KeyID) tab right outer join AreaMaster AM on AM.AreaID=tab.AreaID
				 left outer join (select  KeyID,UserID,Count(*)	as cnt	
					from TransportHostalAllocationDelocation where UserType=0 and ChangeType=0 and (StartDate<EndDate or EndDate is null) and
					 UserID in (Select StudentID from Student_Session where SBranchID=@SBranchID)
					and ((StartDate between @StartDay and @MEndDate or StartDate<@StartDay) and (isnull(EndDate,@MEndDate) between @StartDay and @MEndDate or EndDate>@MEndDate))
					group by KeyID,UserID) users on users.KeyID=tab.StopID
					group by AM.AreaID,AM.AreaName
					having count(users.UserID)>0
END

GO
Create procedure [dbo].[spn_GetParentAppDetails] --1,0,0,0
(
--Created By Niraj 
-- For Parent User Details
@SBranchID int,
@ClassID int,
@SectionID int,
@SessionID int=0
)
as
begin

declare @SessionStatus int=0

if(@SessionID=0)
	begin
		Select @SessionID=SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
	end
	Select @SessionStatus=SessionStatus from SessionMaster where SessionID=@SessionID

if(@ClassID=0)
begin
	Select @ClassID=min(ClassID) from ClassMaster where SBranchID=@SBranchID
	Select @SectionID=min(ID) from Class_Sections where ClassID=@ClassID
end
if(@SectionID<>0)
begin
SELECT        SM.StudentID, 'STUD' + RIGHT(REPLICATE('0', 6) + CAST(SM.StudentID AS VARCHAR(6)), 6) AS StudentSID, SM.Name,SM.SchoolUID, SM.DOB,SM.DOJ, SM.EmailID, SM.BloodGroup,SM.Gender, 
              SM.GuardianName, SM.GuardianMobileNo, SM.Photo, SS.RollNo, SM.AccessCardNo,
			  'PAR' + RIGHT(REPLICATE('0', 6) + CAST(PM.ParentID AS VARCHAR(6)), 6) AS SParentID, PM.FatherName, PM.FatherMobileNo,PM.FatherDOB,PM.MotherName, PM.MotherMobileNo, SM.AadharCardNo,
			
              SM.Gender, SS.ClassID, SS.SectionID,ISNULL((SELECT ClassName FROM dbo.ClassMaster AS CM WHERE (ClassID = SS.ClassID)), 'NA') AS ClassName, 
			  ISNULL((SELECT Name FROM dbo.Class_Sections AS CS WHERE (ID = SS.SectionID)), 'NA') AS SectionName
			
FROM            dbo.StudentMaster AS SM LEFT OUTER JOIN
                         dbo.Student_Session AS SS ON SS.StudentID = SM.StudentID AND SS.Status =isnull(nullif((case when  @SessionStatus=1 then 1 else -1 end),-1),SS.Status) LEFT OUTER JOIN
                         dbo.ParentMaster AS PM ON PM.ParentID = SM.ParentID 					
						
WHERE        (SM.SBranchID = @SBranchID) AND (SS.ClassID=@ClassID) AND (SS.SectionID=@SectionID) and  (SS.SessionID=@SessionID)
ORDER BY SM.Gender desc,SM.Name 
end
else
begin
SELECT        SM.StudentID, 'STUD' + RIGHT(REPLICATE('0', 6) + CAST(SM.StudentID AS VARCHAR(6)), 6) AS StudentSID, SM.Name,SM.SchoolUID, SM.DOB, SM.EmailID, SM.BloodGroup,SM.Gender, 
              SM.GuardianName, SM.GuardianMobileNo, SM.Photo, SS.RollNo, SM.AccessCardNo,
			   'PAR' + RIGHT(REPLICATE('0', 6) + CAST(PM.ParentID AS VARCHAR(6)), 6) AS SParentID,
			   PM.FatherName, PM.FatherMobileNo,PM.MotherName, PM.MotherMobileNo, PM.FatherDOB,
              SM.AadharCardNo,  SM.Gender, SS.ClassID, SS.SectionID,
			  ISNULL((SELECT ClassName FROM dbo.ClassMaster AS CM WHERE (ClassID = SS.ClassID)), 'NA') AS ClassName, 
			  ISNULL((SELECT Name FROM dbo.Class_Sections AS CS WHERE (ID = SS.SectionID)), 'NA') AS SectionName
			  
FROM            dbo.StudentMaster AS SM LEFT OUTER JOIN
                         dbo.Student_Session AS SS ON SS.StudentID = SM.StudentID AND  SS.Status =isnull(nullif((case when  @SessionStatus=1 then 1 else -1 end),-1),SS.Status) LEFT OUTER JOIN
                         dbo.ParentMaster AS PM ON PM.ParentID = SM.ParentID
WHERE        (SM.SBranchID = @SBranchID) and  (SS.SessionID=@SessionID)
ORDER BY SM.Gender desc,SM.Name 
end

select ClassID,ClassName from ClassMaster where SBranchID=@SBranchID

select ID,Name from Class_Sections where ClassID=@ClassID

select isnull(@ClassID,0)
select isnull(@SectionID,0)
Select SessionID as ID,SessionName as Name,SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID
select isnull(@SessionID,0)

end
-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
GO
CREATE TABLE [dbo].[YouTubeVideo_Students](
	[VideoID] [int] NULL,
	[StudentID] [int] NULL
) ON [PRIMARY]
GO

Alter Table [YouTubeVideos]
Add SessionID int

GO

Alter Table YouTubeVideos
Add IsSpecificStudent int

GO

Create Procedure [dbo].[sp_GetAdminYouTubeVideoEditData]
(
@VideoID int,
@SBranchID int
)
AS
BEGIN
	declare @ClassID int,@TeacherID int,@SubjectID int,@SectionID int,@SessionID int
	
	select @ClassID=ClassID,@TeacherID=TeacherID,@SubjectID=SubjectID,@SectionID=SectionID,@SessionID=SessionID
	from [dbo].[YouTubeVideos] where VideoID=@VideoID
	
	Select * from [dbo].[YouTubeVideos] where VideoID=@VideoID

	if(isnull(@ClassID,0)=0)
	begin
		Select top 1 @ClassID=ClassID from ClassMaster where SBranchID=@SBranchID and Status=1 order by Isnull(SequenceNo,99999)
		select top 1 @SectionID=ID from Class_Sections where ClassID=@ClassID and Status=1 order by ID
		Select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
	end
	if(isnull(@SessionID,0)=0)
	begin
		Select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
	end
	declare @GroupID int
	select @GroupID=GroupID from Class_Sections where ID=@SectionID

	if(isnull(@SubjectID,0)=0)
	begin
		select @SubjectID=min(SubjectID) from SubjectMaster where GroupID=@GroupID and Status=1
	end
	select SubjectID as ID,SubjectName as Name from SubjectMaster where GroupID=@GroupID and Status=1
	Select ClassID as ID,ClassName as Name from ClassMaster where SBranchID=@SBranchID and Status=1
	select ID,Name from Class_Sections where ClassID=@ClassID and Status=1
	Select SessionID as ID,SessionName as Name,SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID

	select SM.StudentID as ID,Name ,SchoolUID as Extra1 ,isnull(YS.StudentID,0) as Extra2
	from StudentMaster SM left outer join [dbo].[YouTubeVideo_Students] YS
	on YS.VideoID=@VideoID and YS.StudentID=SM.StudentID
	where SM.StudentID in (Select StudentID from Student_Session where SessionID=@SessionID and SectionID=@SectionID)

	Select isnull(@SessionID,0)
	Select isnull(@ClassID,0)
	Select isnull(@SectionID,0)
	Select isnull(@SubjectID,0)
	Select isnull(@TeacherID,0)
	
	select EmployeeName from EmployeeMaster where EmployeeID=@TeacherID
END
GO

Create Procedure [dbo].[sp_GetSectionSubjectsAndStudents]
(
@SectionID int,
@SessionID int,
@VideoID int=0
)
AS
BEGIN
	
	declare @GroupID int,@SubjectID int
	select @GroupID=GroupID from Class_Sections where ID=@SectionID

	if(isnull(@SubjectID,0)=0)
	begin
		select @SubjectID=min(SubjectID) from SubjectMaster where GroupID=@GroupID and Status=1
	end
	select SubjectID as ID,SubjectName as Name from SubjectMaster where GroupID=@GroupID and Status=1
	
	select SM.StudentID as ID,Name ,isnull(SchoolUID,'') as Extra1 ,isnull(YS.StudentID,0) as Extra2
	from StudentMaster SM left outer join [dbo].[YouTubeVideo_Students] YS
	on YS.VideoID=@VideoID and YS.StudentID=SM.StudentID
	where SM.StudentID in (Select StudentID from Student_Session where SessionID=@SessionID and SectionID=@SectionID)

	Select isnull(@SubjectID,0)
END
GO

Create Procedure [dbo].[sp_UpdateAdminYouTubeVideo]
(
@VideoID int,
@TeacherID int,
@UploadDate datetime,
@SubjectID int,
@Title nvarchar(1000),
@Description nvarchar(max),
@YoutubeID nvarchar(50),
@ClassID int,
@SectionID int,
@SequenceNo int,
@SBranchID int,
@VideoDate datetime,
@SessionID int,
@IsSpecificStudent int,
@Students ut_Name_ID_Utility READONLY,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from [dbo].[YouTubeVideos] where VideoID=@VideoID
		Delete from [dbo].[YouTubeVideo_Students] where VideoID=@VideoID
	end
	else
	begin
		if(@VideoID=0)
		begin
		Insert into [dbo].[YouTubeVideos](TeacherID,UploadDate,SubjectID,Title,Description,YouTubeID,ClassID,SectionID,SequenceNo,VideoDate,SBranchID,SessionID,IsSpecificStudent)
			values(@TeacherID,@UploadDate,@SubjectID,@Title,@Description,@YouTubeID,@ClassID,@SectionID,@SequenceNo,@VideoDate,@SBranchID,@SessionID,@IsSpecificStudent)
			select @VideoID=Cast(Scope_Identity() as int)
		
			if(@IsSpecificStudent=1)
			begin
				insert into [dbo].[YouTubeVideo_Students] 
				select @VideoID,ID from @Students
			end
		end
		else
		begin
			Update [dbo].[YouTubeVideos] set TeacherID=@TeacherID,UploadDate=@UploadDate,SubjectID=@SubjectID,Title=@Title,Description=@Description
			,YouTubeID=@YouTubeID,ClassID=@ClassID,SectionID=@SectionID,SequenceNo=@SequenceNo,VideoDate=@VideoDate,SBranchID=@SBranchID,
			SessionID=@SessionID,IsSpecificStudent=@IsSpecificStudent
			where VideoID=@VideoID

			delete from [dbo].[YouTubeVideo_Students]  where VideoID=@VideoID 
			if(@IsSpecificStudent=1)
			begin
				insert into [dbo].[YouTubeVideo_Students] 
				select @VideoID,ID from @Students
			end
		end
	end
	select @VideoID
END
GO

ALTER Procedure [dbo].[sp_GetYouTubeVideoParent]
(
@StudentID int,
@SubjectID int
)
AS
BEGIN
	declare @ClassID int, @SectionID int
	
	select top 1 @ClassID=ClassID,@SectionID=SectionID from Student_Session where StudentID=@StudentID order by Status desc
	
	select * from [dbo].[YouTubeVideos] where ClassID=@ClassID and SectionID=@SectionID and SubjectID=@SubjectID and isnull(IsSpecificStudent,0)=0
	union 
	select * from [dbo].[YouTubeVideos] YTV where ClassID=@ClassID and SectionID=@SectionID and SubjectID=@SubjectID and VideoID in 
	(select VideoID from [YouTubeVideo_Students] YS where YS.VideoID=YTV.VideoID) and isnull(IsSpecificStudent,0)=1
END
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
GO
GO
CREATE proc [dbo].[sp_GetTeacherPanelYouTubeVideo]
(
@TeacherID int,
@ClassID int,
@SectionID int,
@SubjectID int,
@SBranchID int
)
AS
BEGIN
		declare @ClassesTable  table (ID int,Name nvarchar(20))
		declare @SectionsTable  table (ID int,Name nvarchar(20))
		declare @EvaluationsTable  table (ID int,Name nvarchar(20))
		declare @SubjectsTable  table (ID int,Name nvarchar(50))
		INSERT INTO @SectionsTable(ID,Name) select ID,Name from Class_Sections where ID in (Select SectionID from Time_Table_Master where
		MondayTeacherID=@TeacherID or TuesdayTeacherID=@TeacherID or WednesdayTeacherID=@TeacherID 
		or ThursdayTeacherID=@TeacherID or FridayTeacherID=@TeacherID or SaturdayTeacherID=@TeacherID)

		Insert into @ClassesTable(ID,Name) select ClassID,ClassName from ClassMaster where ClassID in 
		(Select ClassID from Class_Sections where ID in (select ID from @SectionsTable))
		
		if(@SectionID=0)
		begin
			select @ClassID =min(ID) from @ClassesTable 			
			select @SectionID=min(ID) from @SectionsTable where ID in (Select ID from Class_Sections where ClassID=@ClassID)
		end
		select ID,Name from @ClassesTable 
		select ID,Name from @SectionsTable where ID in (Select ID from Class_Sections where ClassID=@ClassID)	

		Insert into @SubjectsTable(ID,Name) Select SubjectID, SubjectName  from SubjectMaster where SubjectID in
		(Select MondaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and MondayTeacherID=@TeacherID
			union
		Select TuesdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and TuesdayTeacherID=@TeacherID
			union
		Select WednesdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and WednesdayTeacherID=@TeacherID
			union
		Select ThursdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and ThursdayTeacherID=@TeacherID
			union
		Select FridaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and FridayTeacherID=@TeacherID
			union
		Select SaturdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and SaturdayTeacherID=@TeacherID)
		if(@SubjectID=0)
		begin
			select @SubjectID=min(ID) from @SubjectsTable
		end
		select ID,Name from @SubjectsTable
		
		select isnull(@ClassID,0)
		select isnull(@SectionID  ,0)
		select isnull(@SubjectID  ,0)

		exec [dbo].[sp_GetYouTubeVideo] @TeacherID,@SubjectID,@ClassID,@SectionID,@SBranchID

END
GO
GO

Create Procedure [dbo].[sp_GetTeacherSectionSubjectsAndStudents]
(
@TeacherID int,
@ClassID int,
@SectionID int,
@SessionID int,
@VideoID int=0
)
AS
BEGIN
		declare @SectionsTable  table (ID int,Name nvarchar(20))
		declare @SubjectsTable  table (ID int,Name nvarchar(50))

		INSERT INTO @SectionsTable(ID,Name) select ID,Name from Class_Sections where ID in (Select SectionID from Time_Table_Master where
		MondayTeacherID=@TeacherID or TuesdayTeacherID=@TeacherID or WednesdayTeacherID=@TeacherID 
		or ThursdayTeacherID=@TeacherID or FridayTeacherID=@TeacherID or SaturdayTeacherID=@TeacherID)
		declare @SubjectID int=0

		if(@SectionID=0)
		begin			
			select @SectionID=min(ID) from @SectionsTable where ID in (Select ID from Class_Sections where ClassID=@ClassID)			
		end

		select ID,Name from @SectionsTable where ID in (Select ID from Class_Sections where ClassID=@ClassID)	

		Insert into @SubjectsTable(ID,Name) Select SubjectID, SubjectName  from SubjectMaster where SubjectID in
		(Select MondaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and MondayTeacherID=@TeacherID
			union
		Select TuesdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and TuesdayTeacherID=@TeacherID
			union
		Select WednesdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and WednesdayTeacherID=@TeacherID
			union
		Select ThursdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and ThursdayTeacherID=@TeacherID
			union
		Select FridaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and FridayTeacherID=@TeacherID
			union
		Select SaturdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and SaturdayTeacherID=@TeacherID)
		if(@SubjectID=0)
		begin
			select @SubjectID=min(ID) from @SubjectsTable
		end
		select ID,Name from @SubjectsTable
				
		select isnull(@SectionID,0)
		select isnull(@SubjectID,0)

	
		select SM.StudentID as ID,Name ,isnull(SchoolUID,'') as Extra1 ,isnull(YS.StudentID,0) as Extra2
		from StudentMaster SM left outer join [dbo].[YouTubeVideo_Students] YS
		on YS.VideoID=@VideoID and YS.StudentID=SM.StudentID
		where SM.StudentID in (Select StudentID from Student_Session where SessionID=@SessionID and SectionID=@SectionID)

END

GO

Create Procedure [dbo].[sp_GetTeacherYouTubeVideoEditData]
(
@TeacherID int,
@VideoID int,
@SBranchID int
)
AS
BEGIN
		declare @ClassID int,@SubjectID int,@SectionID int,@SessionID int	
	
		select @ClassID=ClassID,@SubjectID=SubjectID,@SectionID=SectionID,@SessionID=SessionID
		from [dbo].[YouTubeVideos] where VideoID=@VideoID

		declare @ClassesTable  table (ID int,Name nvarchar(20))
		declare @SectionsTable  table (ID int,Name nvarchar(20),ClassID int)
		declare @EvaluationsTable  table (ID int,Name nvarchar(20))
		declare @SubjectsTable  table (ID int,Name nvarchar(50))
		
		INSERT INTO @SectionsTable(ID,Name,ClassID) select ID,Name,ClassID from Class_Sections where ID in (Select SectionID from Time_Table_Master where
		MondayTeacherID=@TeacherID or TuesdayTeacherID=@TeacherID or WednesdayTeacherID=@TeacherID 
		or ThursdayTeacherID=@TeacherID or FridayTeacherID=@TeacherID or SaturdayTeacherID=@TeacherID)

		Insert into @ClassesTable(ID,Name) select ClassID,ClassName from ClassMaster where ClassID in (select ClassID from @SectionsTable)
		
		if(isnull(@SectionID,0)=0)
		begin
			select @ClassID =min(ID) from @ClassesTable 			
			select @SectionID=min(ID) from @SectionsTable where ID in (Select ID from Class_Sections where ClassID=@ClassID)
		end
		
		if(isnull(@SessionID,0)=0)
		begin
			Select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
		end

		select ID,Name from @ClassesTable 
		select ID,Name from @SectionsTable where ID in (Select ID from Class_Sections where ClassID=@ClassID)	

		Insert into @SubjectsTable(ID,Name) Select SubjectID, SubjectName  from SubjectMaster where SubjectID in
		(Select MondaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and MondayTeacherID=@TeacherID
			union
		Select TuesdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and TuesdayTeacherID=@TeacherID
			union
		Select WednesdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and WednesdayTeacherID=@TeacherID
			union
		Select ThursdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and ThursdayTeacherID=@TeacherID
			union
		Select FridaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and FridayTeacherID=@TeacherID
			union
		Select SaturdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and SaturdayTeacherID=@TeacherID)
		if(@SubjectID=0)
		begin
			select @SubjectID=min(ID) from @SubjectsTable
		end
		select ID,Name from @SubjectsTable
				
		Select * from [dbo].[YouTubeVideos] where VideoID=@VideoID

		select SM.StudentID as ID,Name ,SchoolUID as Extra1 ,isnull(YS.StudentID,0) as Extra2
		from StudentMaster SM left outer join [dbo].[YouTubeVideo_Students] YS
		on YS.VideoID=@VideoID and YS.StudentID=SM.StudentID
		where SM.StudentID in (Select StudentID from Student_Session where SessionID=@SessionID and SectionID=@SectionID)

		Select SessionID as ID , SessionName as Name, SessionStatus as Extra1 
		from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc

		Select isnull(@SessionID,0)
		Select isnull(@ClassID,0)
		Select isnull(@SectionID,0)
		Select isnull(@SubjectID,0)
		Select isnull(@TeacherID,0)
END

