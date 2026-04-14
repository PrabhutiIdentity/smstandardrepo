
DROP TABLE [dbo].[QuestionBankMaster]
GO
CREATE TABLE [dbo].[QuestionBankMaster](
	[QuestionID] [int] IDENTITY(1,1) NOT NULL,
	[ClassID] [int] NULL,
	[GroupID] [int] NULL,
	[SubjectID] [int] NULL,
	[ChapterID] [int] NULL,
	[TopicID] [int] NULL,
	[QuestionType] [int] NULL,
	[Complexity] [int] NULL,
	[QuestionText] [nvarchar](max) NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [nvarchar](max) NULL,
	[Option3] [nvarchar](max) NULL,
	[Option4] [nvarchar](max) NULL,
	[Answer] [nvarchar](max) NULL,
	[Explaination] [nvarchar](max) NULL,
	[SBranchID] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

  DROP TABLE [dbo].[QuestionBankMaster]
GO

GO

CREATE TABLE [dbo].[QuestionBankMaster](
	[QuestionID] [int] IDENTITY(1,1) NOT NULL,
	[TeacherID] int NULL,
	[ClassID] [int] NULL,
	[GroupID] [int] NULL,
	[SubjectID] [int] NULL,
	[ChapterID] [int] NULL,
	[TopicID] [int] NULL,
	[QuestionType] [int] NULL,
	[Complexity] [int] NULL,
	[QuestionText] [nvarchar](max) NULL,
	[QuestionImage] [nvarchar](100) NULL,
	[Option1] [nvarchar](max) NULL,
	[Option1Image] [nvarchar](100) NULL,
	[Option2] [nvarchar](max) NULL,
	[Option2Image] [nvarchar](100) NULL,
	[Option3] [nvarchar](max) NULL,
	[Option3Image] [nvarchar](100) NULL,
	[Option4] [nvarchar](max) NULL,
	[Option4Image] [nvarchar](100) NULL,
	[Answer] [nvarchar](max) NULL,
	[Explaination] [nvarchar](max) NULL,
	[ExplainationImage] [nvarchar](100) NULL,
	[CreatedDate] datetime NULL,
	[SBranchID] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
Create Procedure dbo.sp_UpdateQuestionBank
(
@QuestionID int,
@TeacherID int,
@ClassID int,
@GroupID int,
@SubjectID int,
@ChapterID int,
@TopicID int,
@QuestionType int,
@Complexity int,
@QuestionText nvarchar(max),
@QuestionImage nvarchar(100),
@Option1 [nvarchar](max),
@Option1Image nvarchar(100),
@Option2 [nvarchar](max),
@Option2Image nvarchar(100),
@Option3 [nvarchar](max),
@Option3Image nvarchar(100),
@Option4 [nvarchar](max),
@Option4Image nvarchar(100),
@Answer [nvarchar](max),
@Explaination [nvarchar](max),
@ExplainationImage nvarchar(100),
@CreatedDate datetime,
@SBranchID int,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	Begin
		Delete from QuestionBankMaster where QuestionID=@QuestionID 
	End
	Else
	Begin
		if(@QuestionID=0)
		begin
			Insert into QuestionBankMaster(TeacherID,ClassID,GroupID,SubjectID,ChapterID,TopicID,QuestionType,Complexity,QuestionText,QuestionImage,Option1,Option1Image,Option2,Option2Image,Option3,Option3Image,Option4,Option4Image,
			Answer,Explaination,ExplainationImage,SBranchID,CreatedDate)
			values(@TeacherID,@ClassID,@GroupID,@SubjectID,@ChapterID,@TopicID,@QuestionType,@Complexity,@QuestionText,@QuestionImage,@Option1,@Option1Image,@Option2,@Option2Image,@Option3,@Option3Image,@Option4,@Option4Image,
			@Answer,@Explaination,@ExplainationImage,@SBranchID,@CreatedDate)
			select @QuestionID=Cast(Scope_Identity() as int)
		end
		else
		begin
			Update QuestionBankMaster set TeacherID=@TeacherID,ClassID=@ClassID,GroupID=@GroupID,SubjectID=@SubjectID,ChapterID=@ChapterID,TopicID=@TopicID
			,QuestionType=@QuestionType,Complexity=@Complexity,QuestionText=@QuestionText,QuestionImage=@QuestionImage,Option1=@Option1,Option2=@Option2,Option3=@Option3,
			Option4=@Option4,Option1Image=@Option1Image,Option2Image=@Option2Image,Option3Image=@Option3Image,Option4Image=@Option4Image,
			Answer=@Answer,Explaination=@Explaination,ExplainationImage=@ExplainationImage where QuestionID=@QuestionID
		end
	End
	select @QuestionID
END
GO


CREATE TABLE [dbo].[OnlineExamMaster](
	[OExamID] [int] IDENTITY(1,1) NOT NULL,
	[OExamStartDate] [datetime] NULL,
	[OExamEndDate] [datetime] NULL,
	[ClassID] [int] NULL,
	[SectionID] [int] NULL,
	[SubjectID] [int] NULL,
	[TeacherID] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[Status] [int] NULL,
	[ExamTitle] [nvarchar](50) NULL,
	[ExamDescription] [nvarchar](max) NULL,
	[ExamPriority] [int] NULL,
	[SessionID] [int] NULL,
	[SBranchID] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[OnlineExamQuestions](
	[OXQID] [int] IDENTITY(1,1) NOT NULL,
	[OXID] [int] NULL,
	[QuestionID] [int] NULL,
	[Marks] [numeric](10, 2) NULL
) ON [PRIMARY]
GO



CREATE procedure dbo.sp_GetTeacherQuestionBank  
(  
@TeacherID int,  
@ClassID int,  
@QuestionsBy int  
)  
AS  
BEGIN  
 declare @Classes table(ClassID int,SubjectID int)  
 Insert into @Classes    
 select ClassID,MondaySubjectID as SubjectID from Time_Table_Master where MondayTeacherID=@TeacherID  
 Union  
 select ClassID,TuesdaySubjectID as SubjectID from Time_Table_Master where TuesdayTeacherID=@TeacherID  
 Union  
 select ClassID,WednesdaySubjectID as SubjectID from Time_Table_Master where WednesdayTeacherID=@TeacherID  
 Union  
 select ClassID,ThursdaySubjectID as SubjectID from Time_Table_Master where ThursdayTeacherID=@TeacherID  
 Union  
 select ClassID,FridaySubjectID as SubjectID from Time_Table_Master where FridayTeacherID=@TeacherID  
 Union  
 select ClassID,SaturdaySubjectID as SubjectID from Time_Table_Master where SaturdayTeacherID=@TeacherID  
  
 if(@ClassID=0)  
 begin  
  Select @ClassID=min(ClassID) from @Classes  
 end  
 if(@QuestionsBy=0)  
 begin  
  Select QuestionID,TeacherID,ClassID,QB.GroupID,QB.SubjectID,ChapterID,TopicID,QuestionType,Complexity,QuestionText,QuestionImage,Option1,Option1Image,Option2,Option2Image,  
  Option3,Option3Image,Option4,Option4Image,Answer,Explaination,ExplainationImage,SBranchID,CreatedDate ,SM.SubjectName  
  From QuestionBankMaster QB left outer join SubjectMaster SM on SM.SubjectID=QB.SubjectID  
  where TeacherID=@TeacherID and ClassID=@ClassID  
 end  
 else  
 begin  
 Select QuestionID,TeacherID,EM.EmployeeName as TeacherName,ClassID,QB.GroupID,QB.SubjectID,ChapterID,TopicID,QuestionType,Complexity,
  QuestionText,QuestionImage,Option1,Option1Image,Option2,Option2Image,  
  Option3,Option3Image,Option4,Option4Image,Answer,Explaination,ExplainationImage,QB.SBranchID,QB.CreatedDate ,SM.SubjectName  
  From QuestionBankMaster QB left outer join SubjectMaster SM on SM.SubjectID=QB.SubjectID  
  left outer join EmployeeMaster EM on EM.EmployeeID=QB.TeacherID
  where ClassID=@ClassID   
 end  
  
 select ClassID as ID,ClassName as Name from ClassMaster where ClassID in (select ClassID from @Classes) and Status=1  
 select @ClassID  
  
 select distinct SM.SubjectID as ID,SubjectName as Name,ClassID as Extra1 from @Classes C left outer join SubjectMaster SM on SM.SubjectID=C.SubjectID  
 where SM.SubjectID is not null  
END

GO
Alter View [dbo].[v_EmployeeSalaryAmount]  
as   
  Select EM.EmployeeID,EM.EmployeeName,EM.Photo,Em.Gender,Em.EmployeeSID,EM.EmployeeType,EM.SBranchID, EM.DOJ,
  Sum(isnull(ESD.Amount,ETSD.Amount)) Amount  
  from [v_EmployeeDriversBasicDetails] EM  
  left outer join [EmployeeTypeSalaryDetails] ETSD ON EM.EmployeeType=ETSD.EmployeeTypeID  
  left outer join [EmployeeSalaryDetails] ESD on ESD.EmployeeID=EM.EmployeeID and ESD.EmployeeTypeID=EM.EmployeeType  
  and ESD.SalaryTypeID=ETSD.SalaryTypeID  
  group by EM.EmployeeID,EM.EmployeeName,EM.Photo,Em.Gender,Em.EmployeeSID,EM.EmployeeType,EM.SBranchID  ,EM.DOJ
GO
ALTER proc [dbo].[sp_GetClassTeacherClassSections]
(
@Day nvarchar(3),
@Month nvarchar(3),
@Year nvarchar(5),
@TeacherID int
)
AS
BEGIN
	declare @ClassID nvarchar(100)
	declare @SectionID  nvarchar(100)
	Declare @ClassName nvarchar(100)
	Declare @SectionName nvarchar(100)
	Declare @CurDate Date

	declare  @ClassSectionAttandanceDetails  table (ClassID int,SectionID int, ClassName nvarchar(100),SectionName nvarchar(100),Status int)

	Declare @HCount int
	Declare @DaysWorking int
	Declare @Count int
	Declare @SQL nvarchar(max)
	declare @IsExist int
	DECLARE db_cursor CURSOR FOR 
	select ClassID,SectionID,ClassName,SectionName from [v_ClassSectionNames] where SectionID in (Select ID from Class_Sections where TeacherID=@TeacherID and Status=1)
	OPEN db_cursor   
		FETCH NEXT FROM db_cursor INTO @ClassID,@SectionID,@ClassName,@SectionName
		WHILE @@FETCH_STATUS = 0   
		BEGIN 		
		set @CurDate= cast(@Month+'-'+cast(@Day as nvarchar(2))+'-'+@Year as Date)
		If datepart(dw, @CurDate) = 1 Set @Count=-1
		else
		Begin   
			select @DaysWorking= [Days] from EducationLevelMaster where ID=(select  EducationLevelID from ClassMaster where ClassID=@ClassID)
			if(@DaysWorking<datepart(dw, @CurDate)) Set @Count=-1
			else
			Begin
				Select @HCount=count(*) from HolidayMaster where cast(@Month+'-'+cast(@Day as nvarchar(2))+'-'+@Year as Date) between StartDate and EndDate 
				and Status=1 and DayDuration=1 and ((select count(*) from dbo.SplitStringToTable(Classes,',') where ITem=@ClassID or Item=0)>0) and IsStudents=1
				if(@HCount>0)
				begin
					set @Count=-1
				end
				else 
				begin	
					select @IsExist=count(*) from [dbo].[StudentAttendanceMasterT] where ClassID=@ClassID and SectionID=@SectionID and Month=@Month and FYear=@Year
					if(@IsExist<>0)
					begin
						set @SQL='select @cnt=Count(*) from [dbo].[StudentAttendanceMasterT] where ClassID='+@ClassID+' and SectionID='+@SectionID+' and isnull(D'+@Day+',3)=3 and Month='+@Month+' and FYear='+@Year	
						EXECUTE sp_executesql @SQL, N'@cnt int OUTPUT',@cnt=@Count OUTPUT
					end
					else 
					begin
						Set @Count=1
					end
					if(@Count>0)
					begin
						Set @Count=1
					end
				end
			End
		End
		insert into @ClassSectionAttandanceDetails(ClassID,SectionID,ClassName,SectionName,Status)
		values(@ClassID,@SectionID,@ClassName,@SectionName,@Count)
		FETCH NEXT FROM db_cursor INTO @ClassID,@SectionID,@ClassName,@SectionName
		END   
		CLOSE db_cursor   
		DEALLOCATE db_cursor

		Select * from @ClassSectionAttandanceDetails
END
GO
Create procedure dbo.sp_GetTeacherOnlineExams
(
@TeacherID int ,
@ClassID int,  
@SectionID int,
@SubjectID int,
@SessionID int,
@SBranchID int
)
AS
BEGIN
	if(@SessionID=0)
	begin
		select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
	end
	declare @Classes table(ClassID int,SubjectID int,SectionID int)  
	 Insert into @Classes    
	 select ClassID,MondaySubjectID as SubjectID,SectionID from Time_Table_Master where MondayTeacherID=@TeacherID  
	 Union  
	 select ClassID,TuesdaySubjectID as SubjectID,SectionID from Time_Table_Master where TuesdayTeacherID=@TeacherID  
	 Union  
	 select ClassID,WednesdaySubjectID as SubjectID,SectionID from Time_Table_Master where WednesdayTeacherID=@TeacherID  
	 Union  
	 select ClassID,ThursdaySubjectID as SubjectID,SectionID from Time_Table_Master where ThursdayTeacherID=@TeacherID  
	 Union  
	 select ClassID,FridaySubjectID as SubjectID,SectionID from Time_Table_Master where FridayTeacherID=@TeacherID  
	 Union  
	 select ClassID,SaturdaySubjectID as SubjectID,SectionID from Time_Table_Master where SaturdayTeacherID=@TeacherID  
  
	if(@ClassID=0)  
	 begin  
	  Select @ClassID=min(ClassID) from @Classes  
	  select @SectionID=min(SectionID) from @Classes
	  select @SubjectID=min(SubjectID) from @Classes
	 end 

	 Select OX.*,
	 (Select count(*) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as QuestionCount,
	 (Select sum(Marks) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as TotalMarks,
	 (Select count(*) from [dbo].OnlineExamSubmissionMaster OQ where OQ.OExamID=OX.OExamID) as SubmissionCount
	 from OnlineExamMaster OX
	 where TeacherID=@TeacherID and SubjectID=@SubjectID and SectionID=@SectionID and SessionID=@SessionID
	 
	 select ClassID as ID,ClassName as Name from ClassMaster where ClassID in (select ClassID from @Classes) and Status=1  
	 select isnull(@ClassID,0) 
  
	 select distinct SM.SubjectID as ID,SubjectName as Name,ClassID as Extra1,SectionID as Extra2 from @Classes C left outer join SubjectMaster SM on SM.SubjectID=C.SubjectID  
	 where SM.SubjectID is not null  
	 select isnull(@SubjectID,0)

	 select ID,Name,ClassID as Extra1 from Class_Sections CS where ID in (Select SectionID from @Classes)
	 select isnull(@SectionID,0)

	 Select SessionID as ID,SessionName as Name, SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID
	 select isnull(@SessionID,0)

END

GO


Create procedure dbo.sp_GetTeacherOnlineExamsDetails
(
@TeacherID int ,
@ClassID int,  
@SectionID int,
@SubjectID int,
@SessionID int,
@SBranchID int,
@OExamID int
)
AS
BEGIN
	Declare @ExamTable Table(OExamID int, OExamStartDate datetime,OExamEndDate datetime,ClassID int,SectionID int,SubjectID int,TeacherID int,CreatedDate datetime,Status int,
	ExamTitle nvarchar(max),ExamDescription nvarchar(max),ExamPriority int,SessionID int,SBranchID int)
	
	if(@OExamID!=0)
	begin
		insert into @ExamTable
		Select * from OnlineExamMaster OX where OExamID=@OExamID
	end
	else
	Begin
		insert into @ExamTable(ClassID,SectionID,SubjectID,TeacherID,SessionID) values(@ClassID,@SectionID,@SubjectID,@TeacherID,@SessionID)
	End
	
	select EX.* ,EM.EmployeeName as TeacherName,CM.ClassName,CS.Name as SectionName,
	SS.SessionName ,SM.SubjectName
	from @ExamTable EX left outer join EmployeeMaster EM on EM.EmployeeID=EX.TeacherID
	left outer join ClassMaster CM on CM.ClassID=EX.ClassID
	left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID
	left outer join Class_Sections CS on CS.ID=EX.SectionID
	left outer join SessionMaster SS on SS.SessionID=EX.SessionID
	 
	 select OQ.OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4,Answer,Explaination ,EM.EmployeeName as TeacherName
	 from [dbo].[OnlineExamQuestions] OQ
	 left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID
	left outer join EmployeeMaster EM on EM.EmployeeID=QB.TeacherID
	 where OQ.OXID=@OExamID

	 Select QuestionID,TeacherID,EM.EmployeeName as TeacherName,ClassID,QB.GroupID,QB.SubjectID,ChapterID,TopicID,QuestionType,Complexity,
	QuestionText,QuestionImage,Option1,Option1Image,Option2,Option2Image,  
	Option3,Option3Image,Option4,Option4Image,Answer,Explaination,ExplainationImage,QB.SBranchID,QB.CreatedDate 
	From QuestionBankMaster QB 
	left outer join EmployeeMaster EM on EM.EmployeeID=QB.TeacherID
	where ClassID=@ClassID   and SubjectID=@SubjectID
END

GO
GO

Create procedure dbo.sp_UpdateOnlineExam
(
@OExamID int,
@OExamStartDate datetime,
@OExamEndDate datetime,
@SessionID int,
@ClassID int,
@SectionID int,
@SubjectID int,
@TeacherID int,
@CreatedDate datetime,
@Status int,
@ExamTitle nvarchar(50),
@ExamDescription nvarchar(max),
@ExamPriority int,
@SBranchID int,
@Questions ut_Name_ID_Utility READONLY,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from OnlineExamMaster where OExamID=@OExamID
		Delete from OnlineExamQuestions where OXID=@OExamID
	end
	Else
	Begin
		if(@OExamID=0)
		begin
			Insert into dbo.OnlineExamMaster(OExamStartDate,OExamEndDate,SessionID,ClassID,SectionID,SubjectID,TeacherID,CreatedDate,Status,ExamTitle,ExamDescription,ExamPriority,SBranchID)
			values(@OExamStartDate,@OExamEndDate,@SessionID,@ClassID,@SectionID,@SubjectID,@TeacherID,@CreatedDate,@Status,@ExamTitle,@ExamDescription,@ExamPriority,@SBranchID)
			select @OExamID=Cast(Scope_Identity() as int)
		end
		else
		begin
			Update dbo.OnlineExamMaster set OExamStartDate=@OExamStartDate,OExamEndDate=@OExamEndDate,SessionID=@SessionID,ClassID=@ClassID,SectionID=@SectionID
			,SubjectID=@SubjectID,TeacherID=@TeacherID,Status=@Status,ExamTitle=@ExamTitle,ExamDescription=@ExamDescription,ExamPriority=@ExamPriority
			where OExamID=@OExamID
		end

		Delete from OnlineExamQuestions where OXID=@OExamID and OXQID in (select ID from @Questions where Extra3=-1)
		insert into OnlineExamQuestions(OXID,QuestionID,Marks) select @OExamID,Name,Extra1 from @Questions where Extra3!=-1 and ID=0
	End
END
GO

CREATE TABLE [dbo].[OnlineExamSubmissionMaster](
	[SubmissionID] [int] IDENTITY(1,1) NOT NULL,
	[OExamID] [int] NULL,
	[SubmissionDate] [datetime] NULL,
	[StartDate] [datetime] NULL,
	[StudentID] [int] NULL,
	[TeacherRemark] [nvarchar](max) NULL,
	[TeacherID] [int] NULL,
	[Status] [int] NULL,
	[CheckDate] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[OnlineExamSubmissionAnswers](
	[SubAnsID] [int] IDENTITY(1,1) NOT NULL,
	[SubmissionID] [int] NULL,
	[OXQID] [int] NULL,
	[Answer] [nvarchar](max) NULL,
	[MarksGiven] [numeric](10,2) NULL,
	[Remark] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
  
CREATE procedure dbo.sp_GetStudentOnlineExams  
(  
@StudentID int,  
@SubjectID int  
)  
AS  
BEGIN  
 Declare @ClassID int,@SectionID int,@GroupID int,@SessionID int,@StudentSessionUID int  
 Select @ClassID=ClassID,@SectionID=SectionID,@SessionID=SessionID,@StudentSessionUID=StudentSessionUID from Student_Session where StudentID=@StudentID and Status=1  
 select @GroupID=GroupID from Class_Sections where ID=@SectionID  
  
 Declare @Subjects Table(SubjectID int,SubjectName nvarchar(50))  
 insert into @Subjects select SubjectID,SubjectName from SubjectMaster where GroupID=@GroupID   
 and (IsOptionalSubject=0 or SubjectID in (select OpSubjectID from [Student_Session_OptionalSubjects] where StudentSessionID=@StudentSessionUID))  
  
 if(@SubjectID=0)  
 begin  
  Select top 1 @SubjectID=SubjectID from @Subjects  
 end  
  
  Select OX.*,  
  (Select count(*) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as QuestionCount,  
  (Select sum(Marks) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as TotalMarks,  
  OXM.SubmissionID,EM.EmployeeName as TeacherName,  
  (Select sum(MarksGiven) from OnlineExamSubmissionAnswers OXS where OXS.SubmissionID=OXM.SubmissionID) as MarksObtained  
  from OnlineExamMaster OX left outer join OnlineExamSubmissionMaster OXM on OXM.OExamID=OX.OExamID   and OXM.StudentID=@StudentID
  left outer join EmployeeMaster EM on EM.EmployeeID=OX.TeacherID  
  where SubjectID=@SubjectID and SectionID=@SectionID and SessionID=@SessionID and OX.Status=1  
    
  select SubjectID as ID,SubjectName as Name from @Subjects  
  select isnull(@SubjectID,0)  
END
GO

Create procedure dbo.sp_GetStudentOnlineExamsDetails
(
@OExamID int
)
AS
BEGIN
	select EX.* ,EM.EmployeeName as TeacherName,CM.ClassName,CS.Name as SectionName,
	SS.SessionName ,SM.SubjectName
	from OnlineExamMaster EX left outer join EmployeeMaster EM on EM.EmployeeID=EX.TeacherID
	left outer join ClassMaster CM on CM.ClassID=EX.ClassID
	left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID
	left outer join Class_Sections CS on CS.ID=EX.SectionID
	left outer join SessionMaster SS on SS.SessionID=EX.SessionID
	where OExamID=@OExamID
	 
	 select OQ.OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4
	 from [dbo].[OnlineExamQuestions] OQ
	 left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID
	left outer join EmployeeMaster EM on EM.EmployeeID=QB.TeacherID
	 where OQ.OXID=@OExamID

END
GO

Create procedure dbo.sp_StudentSubmitOnlineAnswerSheet
(
@OExamID int,
@SubmissionDate datetime,
@StudentID int,
@Answers ut_Name_ID_Utility READONLY
)
AS
BEGIN
	
	declare @SubmissionID int
	select @SubmissionID=SubmissionID from OnlineExamSubmissionMaster where StudentID=@StudentID and OExamID=@OExamID
	if(isnull(@SubmissionID,0)=0)
	begin	
		insert into OnlineExamSubmissionMaster(OExamID,SubmissionDate,StudentID,Status)values(@OExamID,@SubmissionDate,@StudentID,0)
		select @SubmissionID=cast(Scope_Identity() as int)
		insert into OnlineExamSubmissionAnswers(SubmissionID,OXQID,Answer) select @SubmissionID,ID,Name from  @Answers	
	end
	select @SubmissionID
END


GO
Create procedure dbo.sp_GetStudentOnlineExamsAnswerSheet
(
@SubmissionID int,
@OExamID int,
@StudentID int
)
AS
BEGIN
	
	select EX.* ,EM.EmployeeName as TeacherName,CM.ClassName,CS.Name as SectionName,
	SS.SessionName ,SM.SubjectName,OXM.SubmissionID,OXM.SubmissionDate,OXM.TeacherRemark,OXM.CheckDate,OXM.Status as SubmissionStatus
	from OnlineExamMaster EX left outer join EmployeeMaster EM on EM.EmployeeID=EX.TeacherID
	left outer join ClassMaster CM on CM.ClassID=EX.ClassID
	left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID
	left outer join Class_Sections CS on CS.ID=EX.SectionID
	left outer join SessionMaster SS on SS.SessionID=EX.SessionID
	left outer join OnlineExamSubmissionMaster OXM on OXM.SubmissionID=@SubmissionID
	where EX.OExamID=@OExamID
	 
	 select OQ.OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4,
	 OXA.Answer as StudentAnswer,OXA.MarksGiven,OXA.Remark as TeacherRemark,QB.Answer,QB.Explaination,QB.Complexity
	 from [dbo].[OnlineExamQuestions] OQ
	 left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID
	 left outer join OnlineExamSubmissionAnswers OXA on OXA.OXQID=OQ.OXQID
	 where OQ.OXID=@OExamID

END
GO
Create  procedure dbo.sp_GetTeacherOnlineExamSubmissions
(  
@OExamID int
)  
AS  
BEGIN  
	
	select EX.OExamID,EX.OExamStartDate,EX.OExamEndDate,EX.ExamTitle,EX.ExamDescription,EX.ExamPriority,OXM.Status
	,EM.Name +'('+'STUD'+RIGHT(REPLICATE('0',6)+CAST(EM.StudentID AS VARCHAR(6)),6)+')' as TeacherName,CM.ClassName,CS.Name as SectionName,
	SS.SessionName ,SM.SubjectName,OXM.SubmissionID,OXM.SubmissionDate,OXM.TeacherRemark,OXM.CheckDate,OXM.Status as SubmissionStatus
	from OnlineExamMaster EX 
	left outer join ClassMaster CM on CM.ClassID=EX.ClassID
	left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID
	left outer join Class_Sections CS on CS.ID=EX.SectionID
	left outer join SessionMaster SS on SS.SessionID=EX.SessionID
	left outer join OnlineExamSubmissionMaster OXM on OXM.OExamID=EX.OExamID
	left outer join StudentMaster EM on EM.StudentID=OXM.StudentID
	where OXM.SubmissionID=@SubmissionID
	 
	 select OQ.OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4,
	 OXA.Answer as StudentAnswer,OXA.MarksGiven,OXA.Remark as TeacherRemark,QB.Answer,QB.Explaination,QB.Complexity
	 from [dbo].[OnlineExamQuestions] OQ
	 left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID
	 left outer join OnlineExamSubmissionAnswers OXA on OXA.OXQID=OQ.OXQID and OXA.SubmissionID=@SubmissionID
	 where OXA.SubmissionID=@SubmissionID
END

GO

Create procedure dbo.sp_GetTeacherStudentOnlineExamsAnswerSheet
(
@SubmissionID int
)
AS
BEGIN
	
	select EX.OExamID,EX.OExamStartDate,EX.OExamEndDate,EX.ExamTitle,EX.ExamDescription,EX.ExamPriority,OXM.Status
	,EM.Name +'('+'STUD'+RIGHT(REPLICATE('0',6)+CAST(EM.StudentID AS VARCHAR(6)),6)+')' as TeacherName,CM.ClassName,CS.Name as SectionName,
	SS.SessionName ,SM.SubjectName,OXM.SubmissionID,OXM.SubmissionDate,OXM.TeacherRemark,OXM.CheckDate,OXM.Status as SubmissionStatus
	from OnlineExamMaster EX 
	left outer join ClassMaster CM on CM.ClassID=EX.ClassID
	left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID
	left outer join Class_Sections CS on CS.ID=EX.SectionID
	left outer join SessionMaster SS on SS.SessionID=EX.SessionID
	left outer join OnlineExamSubmissionMaster OXM on OXM.OExamID=EX.OExamID
	left outer join StudentMaster EM on EM.StudentID=OXM.StudentID
	where OXM.SubmissionID=@SubmissionID
	 
	 select OXA.SubAnsID as OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4,
	 OXA.Answer as StudentAnswer,OXA.MarksGiven,OXA.Remark as TeacherRemark,QB.Answer,QB.Explaination,QB.Complexity
	 from [dbo].[OnlineExamQuestions] OQ
	 left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID
	 left outer join OnlineExamSubmissionAnswers OXA on OXA.OXQID=OQ.OXQID and OXA.SubmissionID=@SubmissionID
	 where OXA.SubmissionID=@SubmissionID

END
GO
Create procedure dbo.sp_UpdateRemarkOnStudentOnlineExamSubmission
(
@SubmissionID int,
@CheckDate datetime,
@TeacherID int,
@Status int,
@TeacherRemark nvarchar(max),
@AnswerRemarks ut_Name_ID_Utility READONLY
)
AS
BEGIN
		update OnlineExamSubmissionMaster set TeacherRemark=@TeacherRemark,TeacherID=@TeacherID, CheckDate=@CheckDate,Status=@Status where SubmissionID=@SubmissionID

		UPDATE e SET e.MarksGiven=d.Name,e.Remark=d.Extra1
		FROM  OnlineExamSubmissionAnswers e, @AnswerRemarks d 
		WHERE e.SubAnsID=d.ID
END
GO

Alter procedure dbo.sp_GetTeacherStudentOnlineExamsAnswerSheet   
(  
@SubmissionID int  
)  
AS  
BEGIN  
   
 select EX.OExamID,EX.OExamStartDate,EX.OExamEndDate,EX.ExamTitle,EX.ExamDescription,EX.ExamPriority,OXM.Status  
 ,EM.Name +'('+'STUD'+RIGHT(REPLICATE('0',6)+CAST(EM.StudentID AS VARCHAR(6)),6)+')' as TeacherName,CM.ClassName,CS.Name as SectionName,  
 SS.SessionName ,SM.SubjectName,OXM.SubmissionID,OXM.SubmissionDate,OXM.TeacherRemark,OXM.CheckDate,OXM.Status as SubmissionStatus  
 from OnlineExamMaster EX   
 left outer join ClassMaster CM on CM.ClassID=EX.ClassID  
 left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID  
 left outer join Class_Sections CS on CS.ID=EX.SectionID  
 left outer join SessionMaster SS on SS.SessionID=EX.SessionID  
 left outer join OnlineExamSubmissionMaster OXM on OXM.OExamID=EX.OExamID  
 left outer join StudentMaster EM on EM.StudentID=OXM.StudentID  
 where OXM.SubmissionID=@SubmissionID  
    
  select OXA.SubAnsID as OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4,  
  OXA.Answer as StudentAnswer,OXA.MarksGiven,OXA.Remark as TeacherRemark,isnull(QB.Answer,'') as Answer,QB.Explaination,QB.Complexity  
  from [dbo].[OnlineExamQuestions] OQ  
  left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID  
  left outer join OnlineExamSubmissionAnswers OXA on OXA.OXQID=OQ.OXQID and OXA.SubmissionID=@SubmissionID  
  where OXA.SubmissionID=@SubmissionID  
  
END
GO
Alter procedure dbo.sp_GetStudentOnlineExamsAnswerSheet  
(  
@SubmissionID int,  
@OExamID int,  
@StudentID int  
)  
AS  
BEGIN  
   
 select EX.* ,EM.EmployeeName as TeacherName,CM.ClassName,CS.Name as SectionName,  
 SS.SessionName ,SM.SubjectName,OXM.SubmissionID,OXM.SubmissionDate,OXM.TeacherRemark,OXM.CheckDate,OXM.Status as SubmissionStatus  
 from OnlineExamMaster EX left outer join EmployeeMaster EM on EM.EmployeeID=EX.TeacherID  
 left outer join ClassMaster CM on CM.ClassID=EX.ClassID  
 left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID  
 left outer join Class_Sections CS on CS.ID=EX.SectionID  
 left outer join SessionMaster SS on SS.SessionID=EX.SessionID  
 left outer join OnlineExamSubmissionMaster OXM on OXM.SubmissionID=@SubmissionID  
 where EX.OExamID=@OExamID  
    
  select OQ.OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4,  
  OXA.Answer as StudentAnswer,OXA.MarksGiven,OXA.Remark as TeacherRemark,isnull(QB.Answer,'') as Answer,QB.Explaination,QB.Complexity  
  from [dbo].[OnlineExamQuestions] OQ  
  left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID  
  left outer join OnlineExamSubmissionAnswers OXA on OXA.OXQID=OQ.OXQID  
  where OQ.OXID=@OExamID  
  
END
GO
Alter procedure dbo.sp_GetTeacherOnlineExams  
(  
@TeacherID int ,  
@ClassID int,    
@SectionID int,  
@SubjectID int,  
@SessionID int,  
@SBranchID int  
)  
AS  
BEGIN  
 if(@SessionID=0)  
 begin  
  select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc  
 end  
 declare @Classes table(ClassID int,SubjectID int,SectionID int)    
  Insert into @Classes      
  select ClassID,MondaySubjectID as SubjectID,SectionID from Time_Table_Master where MondayTeacherID=@TeacherID    
  Union    
  select ClassID,TuesdaySubjectID as SubjectID,SectionID from Time_Table_Master where TuesdayTeacherID=@TeacherID    
  Union    
  select ClassID,WednesdaySubjectID as SubjectID,SectionID from Time_Table_Master where WednesdayTeacherID=@TeacherID    
  Union    
  select ClassID,ThursdaySubjectID as SubjectID,SectionID from Time_Table_Master where ThursdayTeacherID=@TeacherID    
  Union    
  select ClassID,FridaySubjectID as SubjectID,SectionID from Time_Table_Master where FridayTeacherID=@TeacherID    
  Union    
  select ClassID,SaturdaySubjectID as SubjectID,SectionID from Time_Table_Master where SaturdayTeacherID=@TeacherID    
    
 if(@ClassID=0)    
  begin    
   Select @ClassID=min(ClassID) from @Classes    
   select @SectionID=min(SectionID) from @Classes   where ClassID=@ClassID
   select @SubjectID=min(SubjectID) from @Classes    where ClassID=@ClassID and SectionID=@SectionID
  end   
  
  Select OX.*,  
  (Select count(*) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as QuestionCount,  
  (Select sum(Marks) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as TotalMarks,  
  (Select count(*) from [dbo].OnlineExamSubmissionMaster OQ where OQ.OExamID=OX.OExamID) as SubmissionCount  
  from OnlineExamMaster OX  
  where TeacherID=@TeacherID and SubjectID=@SubjectID and SectionID=@SectionID and SessionID=@SessionID  
    
  select ClassID as ID,ClassName as Name from ClassMaster where ClassID in (select ClassID from @Classes) and Status=1    
  select isnull(@ClassID,0)   
    
  select distinct SM.SubjectID as ID,SubjectName as Name,ClassID as Extra1,SectionID as Extra2 from @Classes C left outer join SubjectMaster SM on SM.SubjectID=C.SubjectID    
  where SM.SubjectID is not null    
  select isnull(@SubjectID,0)  
  
  select ID,Name,ClassID as Extra1 from Class_Sections CS where ID in (Select SectionID from @Classes)  
  select isnull(@SectionID,0)  
  
  Select SessionID as ID,SessionName as Name, SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID  
  select isnull(@SessionID,0)  
  
END
GO
ALTER procedure dbo.sp_GetStudentOnlineExamsAnswerSheet
(      
@SubmissionID int,      
@OExamID int,      
@StudentID int      
)      
AS      
BEGIN      
       
 select EX.* ,EM.EmployeeName as TeacherName,CM.ClassName,CS.Name as SectionName,      
 SS.SessionName ,SM.SubjectName,OXM.SubmissionID,OXM.SubmissionDate,OXM.TeacherRemark,OXM.CheckDate,OXM.Status as SubmissionStatus      
 from OnlineExamMaster EX left outer join EmployeeMaster EM on EM.EmployeeID=EX.TeacherID      
 left outer join ClassMaster CM on CM.ClassID=EX.ClassID      
 left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID      
 left outer join Class_Sections CS on CS.ID=EX.SectionID      
 left outer join SessionMaster SS on SS.SessionID=EX.SessionID      
 left outer join OnlineExamSubmissionMaster OXM on OXM.SubmissionID=@SubmissionID      
 where EX.OExamID=@OExamID      
        
  select OQ.OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4,      
  isnull(OXA.Answer,'') as StudentAnswer,OXA.MarksGiven,OXA.Remark as TeacherRemark,isnull(QB.Answer,'') as Answer,QB.Explaination,QB.Complexity      
  from [dbo].[OnlineExamQuestions] OQ      
  left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID      
  left outer join OnlineExamSubmissionAnswers OXA on OXA.OXQID=OQ.OXQID  and   OXA.SubmissionID=@SubmissionID
  where OQ.OXID=@OExamID      
      
END
GO
--------------------------------------------------------------------------------------------------------------------------------------------------------------
--Online Class Related


CREATE TABLE [dbo].[OnlineClassMaster](
	[OCID] [int] IDENTITY(1,1) NOT NULL,
	[TeacherID] [int] NULL,
	[SectionID] [int] NULL,
	[GroupID] [int] NULL,
	[SubjectID] [int] NULL,
	[SessionID] [int] NULL,
	[SBranchID] [int] NULL,
	[Status] [int] NULL,
	[MeetingID] [nvarchar](50) NULL,
	[ClassDate] [date] NULL,
	[StartTime] [nvarchar](10) NULL,
	[EndTime] [nvarchar](10) NULL,
	[CreatedDate] [datetime] NULL,
	[ChangeLog] [nvarchar](50) NULL,
	[StartedOn] [datetime] NULL,
	[EndedOn] [datetime] NULL
) ON [PRIMARY]
GO
GO

CREATE TABLE [dbo].[OnlineClassAttendees](
	[OCID] [int] NULL,
	[StudentID] [int] NULL,
	[StartTime] [nvarchar](12) NULL,
	[EndTime] [nvarchar](12) NULL
) ON [PRIMARY]
GO

Create procedure dbo.sp_GetTeacherOnlineClasses
(
@TeacherID int,
@CurDate date
)
AS
BEGIN
	select OCM.OCID,OCM.Status,OCM.MeetingID,OCM.ClassDate,OCM.StartTime,OCM.EndTime,OCM.StartedOn,CSN.ClassName+'/'+CSN.SectionName as ClassSection,OCM.SectionID,
	SM.SubjectName,OCM.SubjectID
	from OnlineClassMaster OCM left outer join v_ClassSectionNames CSN on CSN.SectionID=OCM.SectionID
	left outer join SubjectMaster SM on SM.SubjectID=OCM.SubjectID
	where OCM.ClassDate>=@CurDate and TeacherID=@TeacherID
	order by ClassDate,CAST(StartTime AS DATETIME)
END
GO
Create procedure dbo.sp_ScheduleTeacherOnlineClass
(
@TeacherID int,
@SectionID int,
@GroupID int,
@SubjectID int,
@ClassDate date,
@StartTime nvarchar(12),
@EndTime nvarchar(12),
@CreatedDate datetime
)
AS
BEGIN
	declare @SBranchID int,@SessionID int
	select @SBranchID=SBranchID from EmployeeMaster where EmployeeID=@TeacherID
	select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
	declare @MeetingID nvarchar(50)
	set @MeetingID='PSchool_'+cast(@TeacherID as nvarchar(10))+'_'++cast(@SectionID as nvarchar(10))+'_'++cast(@SubjectID as nvarchar(10))+'_'+CONVERT(varchar, @ClassDate, 23)
	Insert into OnlineClassMaster(TeacherID,SectionID,GroupID,SubjectID,Status,MeetingID,ClassDate,StartTime,EndTime,CreatedDate,SessionID,SBranchID)
	values(@TeacherID,@SectionID,@GroupID,@SubjectID,0,@MeetingID,@ClassDate,@StartTime,@EndTime,@CreatedDate,@SessionID,@SBranchID)
	select cast(Scope_Identity() as int) as ID,@MeetingID as Name
END

GO

Create procedure dbo.sp_StartOnlineClass
(
@TeacherID int,
@OCID int,
@StartedOn datetime
)
AS
BEGIN
	Update OnlineClassMaster set StartedOn=@StartedOn,Status=1 where OCID=@OCID and TeacherID=@TeacherID
	select @@rowcount
END
GO
Create procedure dbo.sp_GetOnlineClassAttendees
(
@OCID int
)
AS
BEGIN
	select OCA.StudentID,SM.Name as StudentName,OCA.StartTime,OCA.EndTime
	from OnlineClassAttendees OCA left outer join StudentMaster SM on SM.StudentID=OCA.StudentID	
	where OCID=@OCID
	order by StartTime 
END

GO

Create procedure dbo.sp_EndOnlineClass
(
@TeacherID int,
@OCID int,
@EndedOn datetime
)
AS
BEGIN
	Update OnlineClassMaster set EndedOn=@EndedOn,Status=2 where OCID=@OCID --and TeacherID=@TeacherID
	select @@rowcount
END
GO
Create procedure dbo.sp_GetStudentOnlineClass
(
@StudentID int,
@CurDate datetime
)
AS
BEGIN
	declare @ClassID int, @SectionID int,@StudentSessionUID int,@SessionID int
	select top 1 @SectionID=SectionID,@ClassID=ClassID,@StudentSessionUID=StudentSessionUID ,@SessionID=SessionID
	from Student_Session where studentID=@StudentID order by StudentSessionUID desc 

	select OCM.OCID,OCM.Status,OCM.MeetingID,OCM.ClassDate,OCM.StartTime,OCM.EndTime,OCM.StartedOn,OCM.SectionID,
	SM.SubjectName,OCM.SubjectID,EM.EmployeeName as TeacherName
	from OnlineClassMaster OCM left outer join EmployeeMaster EM on EM.EmployeeID=OCM.TeacherID
	left outer join SubjectMaster SM on SM.SubjectID=OCM.SubjectID
	where OCM.ClassDate>=@CurDate and SectionID=@SectionID and SessionID=@SessionID
	order by ClassDate,CAST(StartTime AS DATETIME) 
END
GO
Create procedure dbo.sp_JoinStudentOnlineClass
(
@StudentID int,
@JoinDate datetime,
@OCID int
)
AS
BEGIN
	Insert into OnlineClassAttendees(OCID,StudentID,StartTime)
	values(@OCID,@StudentID,cast(cast(@JoinDate as time) as nvarchar(12)))
	select @@rowcount
END
GO
Create procedure dbo.sp_LeaveStudentOnlineClass
(
@StudentID int,
@LeaveDate datetime,
@OCID int
)
AS
BEGIN
	WITH UpdateList_view AS (
	  SELECT TOP 1  * from OnlineClassAttendees WHERE OCID=@OCID and StudentID=@StudentID
	  ORDER BY StartTime DESC 
	)

	update UpdateList_view set EndTime=cast(cast(@LeaveDate as time) as nvarchar(12))
	select @@rowcount
END
GO
Alter procedure dbo.sp_JoinStudentOnlineClass      
(      
@StudentID int,      
@JoinDate datetime,      
@OCID int  ,    
@ParentID int=0    
)      
AS      
BEGIN      
  
  declare @SectionID int, @SessionID int,@SBranchID int ,@MeetingID nvarchar(100),@SubjectID int,@UpdatedRows int,@SubjectName nvarchar(50)  
  select @SectionID=SectionID,@SessionID=SessionID,@SBranchID=SBranchID,@MeetingID=MeetingID,@SubjectID=SubjectID from OnlineClassMaster where OCID=@OCID    
  if(@StudentID=0)    
  begin    
   select top 1 @StudentID=StudentID from Student_Session where SectionID=@SectionID and SessionID=@SessionID     
   and StudentID in (Select StudentID from StudentMaster where ParentID=@ParentID)    
  end    
  Insert into OnlineClassAttendees(OCID,StudentID,StartTime)      
  values(@OCID,@StudentID,cast(cast(@JoinDate as time) as nvarchar(12)))      
  select @UpdatedRows= @@rowcount  
  select @SubjectName=SubjectName from SubjectMaster where SubjectID=@SubjectID  
   
 Select Name,'/Images/StudentImage/'+Cast(StudentID as nvarchar(10))+'_'+Photo as Photo,@MeetingID as MeetingID,@SubjectName as SubjectName  
 from StudentMaster where StudentID=@StudentID  
END   
GO
CREATE  procedure [dbo].[spn_GetRecieverListForOnlineClass] 
(  
@OCID int 
)  
as   
begin    
	declare @SectionID int, @SessionID int,@SBranchID int

	select @SectionID=SectionID,@SessionID=SessionID,@SBranchID=SBranchID from OnlineClassMaster where OCID=@OCID

   select ParentID as ID,FatherName as Name,FatherMobileNo as MobileNo, 4 as RecieverType, AU.FCMToken as deviceToken 
   from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4  
   where ParentID in (Select ParentID from StudentMaster where StudentID in
   (select StudentID from Student_Session where SectionID=@SectionID and SessionID=@SessionID)) and isnull(AU.FCMToken,'')!=''
   
end  
  