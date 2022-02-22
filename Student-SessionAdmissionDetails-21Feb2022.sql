--Fuctions
		
CREATE function [dbo].[fn_GetStudentsAllSessions]      
(      
	@StudentID int      
)      
RETURNS nvarchar(max)      
AS      
BEGIN      
 Declare @DivisionNames nvarchar(max)      
 SELECT @DivisionNames= Main.Title      
  FROM      
   (      
    SELECT DISTINCT       
     (     
      SELECT +SM.SessionName+' ('+  CM.ClassName +'/' + CS.[Name] +')'+', ' AS [text()]      
      FROM dbo.Student_Session ST1 
	  left outer join SessionMaster SM on SM.SessionID = ST1.SessionID
	  left outer join ClassMaster CM on CM.ClassID = ST1.ClassID
	  left outer join Class_Sections CS on CS.ClassID = ST1.ClassID
      WHERE ST1.StudentID =@StudentID   
	  order by ST1.fromDate
      FOR XML PATH ('')      
     ) [Title]      
    FROM dbo.Student_Session ST2      
   ) [Main]      
 return @DivisionNames      
END

--Stored Procedure
		
Create procedure sp_GetSessionAdmissionsDetails --1,1                 
(         
	@SessionID int,                 
	@SBranchID int
)                  
As                  
BEGIN                  
	if @SessionID = 0	
	begin
	Select top 1 @SessionID=SessionID from SessionMaster
	end	

   declare @StartDate date        
   declare @EndDate date        
          
    Select @StartDate=SessionStartDate, @EndDate=SessionEndDate from SessionMaster where SBranchID=@SBranchID and SessionID=@SessionID
	
	 Select SM.StudentID,'STUD' + RIGHT(REPLICATE('0', 6) + CAST(SM.StudentID AS VARCHAR(6)), 6) AS StudentSID,SM.Name,SM.Gender,SM.SchoolUID, SM.DOB,SM.DOJ,            
	 SM.GuardianName, SM.GuardianMobileNo, SM.Photo, SS.RollNo, SM.AccessCardNo
	 , 'PAR' + RIGHT(REPLICATE('0', 6) + CAST(PM.ParentID AS VARCHAR(6)), 6) AS SParentID, PM.FatherName, PM.FatherMobileNo,PM.FatherDOB,PM.MotherName, 
	 PM.MotherMobileNo, SM.AadharCardNo            
	 ,SS.ClassID,CM.ClassName,SS.SectionID ,CS.Name as SectionName
	 ,(Select dbo.fn_GetStudentsAllSessions(SM.StudentID)) as StudentSessions
	 
	FROM  dbo.StudentMaster SM             
	 left Outer join Student_Session SS on SS.StudentID=SM.StudentID             
	 left Outer Join ClassMaster CM on CM.ClassID=SS.ClassID                  
	 left outer join ParentMaster PM on PM.ParentID = SM.ParentID                  
	 left outer join Class_Sections CS on CS.ClassID = CM.ClassID               
	 where SS.ClassID=CM.ClassID and SS.SectionID=CS.ID            
	  and ss.SessionID=@SessionID and SM.SBranchID=@SBranchID                 

	 order by StudentID desc               
	 
	 Select SessionID as ID, SessionName as Name,SessionStatus as Extra1,SessionStartDate as StartDate, 
	 SessionEndDate as EndDate from SessionMaster where SBranchID=@SBranchID order by SessionStartDate    desc              
					  
	 Select @SessionID=SessionID from SessionMaster where SbranchID=@SBranchID and SessionStatus=1               
             
	 Select * from SBranchMaster where SBranchID=@SBranchID
END 