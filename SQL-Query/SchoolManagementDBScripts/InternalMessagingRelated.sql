

Create procedure [dbo].[sp_GetTeacherClassSectionWiseSubjects]
(
@TeacherID int
)
AS
BEGIN
	Declare @ClassID int, @SectionID int,@SubjectID int
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

	 select ClassID as ID,ClassName as Name from ClassMaster where ClassID in (select ClassID from @Classes) and Status=1  
	 select isnull(@ClassID,0) 
  
	 select distinct SM.SubjectID as ID,SubjectName as Name,ClassID as Extra1,SectionID as Extra2 from @Classes C left outer join SubjectMaster SM on SM.SubjectID=C.SubjectID  
	 where SM.SubjectID is not null  
	 select isnull(@SubjectID,0)

	 select ID,Name,ClassID as Extra1 from Class_Sections CS where ID in (Select SectionID from @Classes)
	 select isnull(@SectionID,0)

END

GO
Create proc [dbo].[sp_GetSectionStudentParentList]
(
@SectionID int,
@SessionID int
)
AS
BEGIN
	Select StudentID as ID,'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) +' '+ Name as Name,PM.FatherName as Extra1,PM.MotherName as Extra2
	 from StudentMaster SM left outer join ParentMaster PM on PM.ParentID=SM.ParentID
	 where StudentID in (select StudentID from dbo.v_StudentSessionTop1Entry where  SectionID=@SectionID and SessionID=@SessionID)
END
GO

Create procedure [dbo].[sp_GetTeacherMessageRecieverLists]
(
@TeacherID int ,
@ClassID int,  
@SectionID int,
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
	 end 

	 select ClassID as ID,ClassName as Name from ClassMaster where ClassID in (select ClassID from @Classes) and Status=1  
	 select isnull(@ClassID,0) 

	 select ID,Name,ClassID as Extra1 from Class_Sections CS where ID in (Select SectionID from @Classes)
	 select isnull(@SectionID,0)

	 Select SessionID as ID,SessionName as Name, SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID
	 select isnull(@SessionID,0)

	exec [dbo].[sp_GetSectionStudentParentList] @SectionID,@SessionID
END