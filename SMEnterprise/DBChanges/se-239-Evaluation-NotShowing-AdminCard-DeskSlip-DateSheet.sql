  
ALTER proc [dbo].[sp_GetStudentAdmitCard]            
(            
@ClassID int,            
@SectionID int,            
@SBranchID int,            
@EvaluationID int,            
@SessionID int            
)            
AS            
BEGIN          
	declare @GroupID int            
	Select @GroupID=GroupID from Class_Sections where ClassID=@ClassID  and ID=@SectionID        
    
	declare @SchemeID int            
	if(@SessionID=0)            
	Begin            
		Select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and SessionStatus=1            
	End            
	if(@SectionID=0)            
	begin            
		select @ClassID =min(ClassID) from ClassMaster where Status=1 and SBranchID=@SBranchID            
		select @SectionID=min(ID) from Class_Sections where ClassID=@ClassID            
	end            
	declare @ELID int            
	if(@EvaluationID=0)            
	begin            
            
		select @SchemeID=SchemeID from [dbo].[ClassSessionDetails] where ClassID=@ClassID and SessionID=@SessionID            
		Select @EvaluationID=min(EvaluationID) from EvaluationMaster EM where SBranchID=@SBranchID and            
		EvaluationSchemeID=@SchemeID and EvaluationID not in (Select isnull(MasterID,0) from EvaluationMaster)            
		Select @ELID =EducationLevelID from ClassMaster where ClassID=@ClassID            
            
		--Select @SessionID=SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID            
	end            
	else            
	begin            
		Select @ELID =EducationLevelID from ClassMaster where ClassID=@ClassID            
		select @SchemeID=SchemeID from [dbo].[ClassSessionDetails] where ClassID=@ClassID and SessionID=@SessionID            
	end            
            
	declare @EvaMonth nvarchar(2)            
	declare @EvaYear nvarchar(4)            
            
	select @EvaMonth=[EvaluationMonth] from [dbo].[EvaluationMaster] where EvaluationID=@EvaluationID            
            
	select @EvaYear= YEAR(getdate())            
            
	Select EvaluationID,            
	(case when isnull(MasterID,0)=0 then EvaluationName else            
	(Select EvaluationName from EvaluationMaster ED where ED.EvaluationID=EM.MasterID)+'->'+EvaluationName end) as EvaluationName            
	from EvaluationMaster EM where SBranchID=@SBranchID and EvaluationSchemeID=@SchemeID and EvaluationID not in (Select isnull(MasterID,0) from EvaluationMaster)            
            
	Select ClassID,ClassName from ClassMaster where Status=1 and SBranchID=@SBranchID            
	Select ID,Name from Class_Sections where ClassID=@ClassID            
        
	select EM.ExamID,SubM.SubjectID,SubM.SubjectName,EM.ExamDate,       
	CONVERT(varchar(15),cast(isnull(EM.StartTime,'09:00:00') as TIME),100) as StartTime,         
	--cast(isnull(EM.StartTime,'09:00:00') as nvarchar(8)) as StartTime ,       
	CONVERT(varchar(15),cast(isnull(EM.EndTime,'10:30:00') as TIME),100) as EndTime        
	--cast(isnull(EM.EndTime,'10:30:00') as nvarchar(8)) as EndTime            
	from SubjectMasterAll SubM left outer join ExamMaster EM            
	on EM.SubjectID=SubM.SubjectID  and EM.ClassID=@ClassID   and EM.GroupID=@GroupID         
	and EM.SBranchID=@SBranchID and EM.EvaluationID=@EvaluationID            
	where EM.IsApplicable=1            
	order by ExamDate,EM.StartTime asc    
            
	Select SessionID,SessionName,SessionStatus from SessionMaster where SBranchID=@SBranchID            
	select isnull(@EvaluationID,0)            
	select isnull(@ClassID,0)            
	select isnull(@SectionID,0)            
	select isnull(@SessionID,0)            
            
	declare @ClassName nvarchar(50)            
	declare @SectionName nvarchar(50)            
	declare @SessionName nvarchar(50)            
	declare @EvaluationName nvarchar(50)            
	select @ClassName=ClassName from ClassMaster where ClassID=@ClassID            
	select @SectionName=Name from Class_Sections where ID=@SectionID            
	select @SessionName=SessionName from SessionMaster where SessionID=@SessionID            
	Select @EvaluationName=            
	(case when isnull(MasterID,0)=0 then EvaluationName else            
	(Select EvaluationName from EvaluationMaster ED where ED.EvaluationID=EM.MasterID)+'->'+EvaluationName end)            
	from EvaluationMaster EM where EvaluationID=@EvaluationID            
            
	Select SS.StudentSessionUID, SS.RollNo, SM.Name, SM.StudentID,SM.Gender,SM.Photo,PM.FatherName,PM.MotherName,            
	@ClassName as ClassName,@SectionName as SectionName,@SessionName as SessionName,@EvaluationName as EvaluationName            
	,'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6)  as StudentSID            
	from StudentMaster SM right outer join Student_Session SS on SS.StudentID=SM.StudentID            
	left outer join ParentMaster PM on PM.ParentID=SM.ParentID            
	Where SS.SectionID=@SectionID and SS.SessionID=@SessionID   and ss.status=1         
	order by SM.Name            
            
	select * from SBranchMaster where SBranchID=@SBranchID            
            
end 

GO

Alter procedure [dbo].[sp_GetDeskSlip]      
(      
@ClassID int,      
@SectionID int,      
@SBranchID int,      
@EvaluationID int,      
@SessionID int      
)      
AS      
BEGIN      
 declare @SchemeID int      
 if(@SessionID=0)      
 Begin      
  Select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and SessionStatus=1      
 End      
 if(@ClassID=0)      
 begin      
  select @ClassID =min(ClassID) from ClassMaster where Status=1 and SBranchID=@SBranchID      
  select @SectionID=min(ID) from Class_Sections where ClassID=@ClassID      
 end      
 declare @ELID int      
 if(@EvaluationID=0)      
 begin      
  select @SchemeID=SchemeID from [dbo].[ClassSessionDetails] where ClassID=@ClassID and SessionID=@SessionID      
  Select @EvaluationID=min(EvaluationID) from EvaluationMaster EM where SBranchID=@SBranchID and      
  EvaluationSchemeID=@SchemeID and EvaluationID not in (Select isnull(MasterID,0) from EvaluationMaster)      
  Select @ELID =EducationLevelID from ClassMaster where ClassID=@ClassID      
      
  --Select @SessionID=SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID      
 end      
 else      
 begin      
  Select @ELID =EducationLevelID from ClassMaster where ClassID=@ClassID      
  select @SchemeID=SchemeID from [dbo].[ClassSessionDetails] where ClassID=@ClassID and SessionID=@SessionID      
 end      
      
 declare @EvaMonth nvarchar(2)      
 declare @EvaYear nvarchar(4)      
      
 select @EvaMonth=[EvaluationMonth] from [dbo].[EvaluationMaster] where EvaluationID=@EvaluationID      
      
 select @EvaYear= YEAR(getdate())      
      
 Select EvaluationID,      
 (case when isnull(MasterID,0)=0 then EvaluationName else      
 (Select EvaluationName from EvaluationMaster ED where ED.EvaluationID=EM.MasterID)+'->'+EvaluationName end) as EvaluationName      
 from EvaluationMaster EM where SBranchID=@SBranchID and EvaluationSchemeID=@SchemeID and EvaluationID not in (Select isnull(MasterID,0) from EvaluationMaster)      
      
 Select ClassID,ClassName from ClassMaster where Status=1 and SBranchID=@SBranchID      
 Select ID,Name from Class_Sections where ClassID=@ClassID      
      
 Select SessionID,SessionName,SessionStatus from SessionMaster where SBranchID=@SBranchID      
 select isnull(@EvaluationID,0)      
 select isnull(@ClassID,0)      
 select isnull(@SectionID,0)      
 select isnull(@SessionID,0)      
      
 declare @ClassName nvarchar(50)      
 declare @SectionName nvarchar(50)      
 declare @SessionName nvarchar(50)      
 declare @EvaluationName nvarchar(50)      
 select @ClassName=ClassName from ClassMaster where ClassID=@ClassID      
 select @SectionName=Name from Class_Sections where ID=@SectionID      
 select @SessionName=SessionName from SessionMaster where SessionID=@SessionID      
 Select @EvaluationName=      
 (case when isnull(MasterID,0)=0 then EvaluationName else      
 (Select EvaluationName from EvaluationMaster ED where ED.EvaluationID=EM.MasterID)+'->'+EvaluationName end)      
 from EvaluationMaster EM where EvaluationID=@EvaluationID      
      
 Select SS.StudentSessionUID, SS.RollNo, SM.Name, SM.StudentID,SM.Gender,SM.Photo,PM.FatherName,PM.MotherName,PM.FatherMobileNo as MobileNo,SM.DOB,SM.SchoolUID,      
 @ClassName as ClassName,@SectionName as SectionName,@SessionName as SessionName,@EvaluationName as EvaluationName      
 ,'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6)  as StudentSID      
 from StudentMaster SM right outer join Student_Session SS on SS.StudentID=SM.StudentID      
 left outer join ParentMaster PM on PM.ParentID=SM.ParentID      
 Where SS.SectionID=@SectionID and SS.SessionID=@SessionID  and SS.[Status]=1  
 order by SM.Name      
      
 select * from SBranchMaster where SBranchID=@SBranchID      
      
end