           
alter proc [dbo].[sp_GetClassGroupWiseExamResults]          
(          
@TeacherID int,          
@EvaluationID int,          
@ClassID int,          
@SectionID int,          
@SubjectID int,          
@SBranchID int,          
@EvaluationMode int,          
@SessionID int=0          
)          
AS          
BEGIN        
        
  declare @IsOptionalSubject int        
  declare @MainSubID int        
  Declare @LookupSubjectID int     
    
  if(@SessionID=0)          
  begin          
   Select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and SessionStatus=1          
  end           
  declare @ClassesTable  table (ID int,Name nvarchar(20))          
  declare @SectionsTable  table (ID int,Name nvarchar(20),ClassID int)          
  declare @EvaluationsTable  table (ID int,Name nvarchar(100))          
  declare @SubjectsTable  table (ID int,Name nvarchar(100))          
  declare @Classes table(ClassID int,SubjectID int,SectionID int)              
  Insert into @Classes               
 select ClassID,SubjectID,SectionID from dbo.TeacherClassSectionSubjects(@TeacherID)         
        
  INSERT INTO @SectionsTable(ID,Name,ClassID) select ID,Name,ClassID from Class_Sections where ID in (Select SectionID from @Classes)          
          
  Insert into @ClassesTable(ID,Name) select ClassID,ClassName from ClassMaster where ClassID in           
  (Select ClassID from @Classes)          
            
  if(@SectionID=0)          
  begin          
   select @ClassID =min(ID) from @ClassesTable              
   select @SectionID=min(ID) from @SectionsTable where ClassID =@ClassID        
  end          
  select ID,Name from @ClassesTable           
  select ID,Name from @SectionsTable where ClassID=@ClassID         
          
  Insert into @SubjectsTable(ID,Name) Select SubjectID, SubjectName  from SubjectMasterAll where SubjectID in          
  (Select SubjectID from @Classes where ClassID=@ClassID and SectionID=@SectionID)          
  if(@SubjectID=0)          
  begin          
   select @SubjectID=min(ID) from @SubjectsTable          
  end         
  --for optional subject    
  select ID,Name from @SubjectsTable     
   select @MainSubID=isnull(MainSubID,0) from SubjectMasterT where SubjectID=@SubjectID        
  if(@MainSubID=0)        
  begin        
   Select @IsOptionalSubject=IsOptionalSubject from SubjectMasterT where SubjectID=@SubjectID        
   set @LookupSubjectID=@SubjectID        
  end        
  else        
  begin        
   Select @IsOptionalSubject=IsOptionalSubject from SubjectMasterT where SubjectID=@MainSubID        
   set @LookupSubjectID=@MainSubID        
  end       
    --      
  insert into @EvaluationsTable(ID,Name) exec sp_GetEvaluationList @SBranchID,@EvaluationMode,@ClassID,@SessionID          
  if(@EvaluationID=0)          
  begin          
   select @EvaluationID=min(ID) from @EvaluationsTable          
  end          
  Select ID,Name from @EvaluationsTable          
          
 declare @MaxMarks int           
 declare @PassMarks int           
 declare @GroupID int          
 declare @ExamID int          
 Select @GroupID=GroupID from Class_Sections where ID=@SectionID          
 select @MaxMarks=MaxMarks,@PassMarks=PassMarks,@ExamID=ExamID from [dbo].[ExamMaster] where SubjectID=@SubjectID and ClassID=@ClassID and           
 EvaluationID=@EvaluationID and GroupID=@GroupID and SBranchID=@SBranchID          
          
 if(@ExamID<>0)          
 begin      
  if(@IsOptionalSubject=0)        
   begin      
 Select isnull(ERM.ResultID,0) as ResultID, @ExamID as ExamID,SS.StudentID,          
 'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.[StudentID] AS VARCHAR(6)),6) as [StudentSID]          
 ,SS.RollNo,(Select SM.Name from StudentMaster SM where SM.StudentID=SS.StudentID) as StudentName,          
 (Select SM.Photo from StudentMaster SM where SM.StudentID=SS.StudentID) as Photo,          
 (Select SM.Gender from StudentMaster SM where SM.StudentID=SS.StudentID) as Gender          
 ,@MaxMarks as MaxMarks,@PassMarks as PassMarks, ERM.MarksScored,ERM.Grade,ERM.GradePoints,isnull(ERM.[Status],0) as [Status]          
 from Student_Session SS left outer join ExamResultMaster ERM on SS.StudentID=ERM.StudentID and ERM.ExamID=@ExamID          
 where SS.ClassID=@ClassID and SS.SectionID=@SectionID  and  SS.[Status]=1  and SS.SessionID=@SessionID      
 end    
 else    
 begin    
   Select isnull(ERM.ResultID,0) as ResultID, @ExamID as ExamID,SS.StudentID,        
    'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.[StudentID] AS VARCHAR(6)),6) as [StudentSID]        
    ,SS.RollNo,(Select SM.Name from StudentMaster SM where SM.StudentID=SS.StudentID) as StudentName,        
    (Select SM.Photo from StudentMaster SM where SM.StudentID=SS.StudentID) as Photo,        
    (Select SM.Gender from StudentMaster SM where SM.StudentID=SS.StudentID) as Gender        
    ,@MaxMarks as MaxMarks,@PassMarks as PassMarks, ERM.MarksScored,ERM.Grade,ERM.GradePoints,isnull(ERM.[Status],0) as [Status]        
    from Student_Session SS left outer join ExamResultMaster ERM on SS.StudentID=ERM.StudentID and ERM.ExamID=@ExamID        
    where SS.ClassID=@ClassID and SS.SectionID=@SectionID and SS.SessionID=@SessionID and SS.[Status]=1        
    and (SS.StudentSessionUID in (Select StudentSessionID from [Student_Session_OptionalSubjects] where OpSubjectID=@LookupSubjectID)        
    or not exists(select OpSubjectID from [Student_Session_OptionalSubjects] where StudentSessionID=SS.StudentSessionUID))        
    order by StudentName         
 end    
   end          
   else          
   begin          
  select 0 as ResultID,0 as ExamID, 0 as StudentID,'' as StudentSID from ExamResultMaster where 1=2          
   end          
 select isnull(@ClassID,0)          
 select isnull(@SectionID ,0)         
 select isnull(@SubjectID,0)          
 select isnull(@EvaluationID,0)    
  select isnull(IsLocked,0) from ExamMaster where ExamID= @ExamID 
          
END     