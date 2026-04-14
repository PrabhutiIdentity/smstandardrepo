/*
Student upload export query

Purpose:
- Fill the columns of "Student_Upload_Template .xlsx"
- Only current active session students
- Only active student-session rows
- DOB / DOJ formatted as DD-MM-YYYY

Usage:
- Change @SBranchID before running


--Agar slash format chahiye:
'''' + ISNULL(REPLACE(CONVERT(VARCHAR(10), ST.DOB, 105), '-', '/'), '') AS DOB,
'''' + ISNULL(REPLACE(CONVERT(VARCHAR(10), ST.DOJ, 105), '-', '/'), '') AS DOJ,
*/



DECLARE @SBranchID INT = 2005;

;WITH ActiveSession AS
(
    SELECT TOP 1
        SM.SessionID
    FROM dbo.SessionMaster SM
    WHERE SM.SBranchID = @SBranchID
      AND SM.SessionStatus = 1
    ORDER BY SM.SessionStartDate DESC, SM.SessionID DESC
)
SELECT
    ISNULL(CAST(ISNULL(SS.IsAdmissionFeeApplicable, 0) AS INT), 0) AS IsAdmissionFeeApplicable,
    ISNULL(CM.ClassName, '') AS Class,
    ISNULL(CS.Name, '') AS Section,
    ISNULL(ST.SchoolUID, '') AS SchoolUID,
    ISNULL(ST.Name, '') AS Name,
    ISNULL(SS.RollNo, '') AS RollNo,
    ISNULL(ST.BloodGroup, '') AS BloodGroup,
    CASE ISNULL(ST.Gender, 0)
        WHEN 0 THEN 'Male'
        WHEN 1 THEN 'Female'      
        ELSE ''
    END AS Gender,
    ISNULL(PM.FatherName, '') AS FatherName,
    ISNULL(PM.FatherMobileNo, '') AS FatherMobileNo,
    ISNULL(PM.MotherName, '') AS MotherName,
    ISNULL(PM.MotherMobileNo, '') AS MotherMobileNo,
    --CHAR(39) + ISNULL(CONVERT(VARCHAR(10), ST.DOB, 105),'') AS DOB,
    --CHAR(39) + ISNULL(CONVERT(VARCHAR(10), ST.DOJ, 105),'') AS DOJ,
	 '' + ISNULL(REPLACE(CONVERT(VARCHAR(10), ST.DOB, 105), '-', '/'), '') AS DOB,
'' + ISNULL(REPLACE(CONVERT(VARCHAR(10), ST.DOJ, 105), '-', '/'), '') AS DOJ,
    ISNULL(ST.AadharCardNo, '') AS AadharCardNo,
    ISNULL(CAST(RM.Name AS VARCHAR(20)), '') AS Religion,
    ISNULL(CAST(SOCC.Name AS VARCHAR(20)), '') AS Category,
    ISNULL(QM.QuotaName, '') AS Quota,
    ISNULL(ST.MiniAddress, '') AS MiniAddress
FROM dbo.Student_Session SS
INNER JOIN ActiveSession AC
    ON AC.SessionID = SS.SessionID
INNER JOIN dbo.StudentMaster ST
    ON ST.StudentID = SS.StudentID
LEFT JOIN dbo.ParentMaster PM
    ON PM.ParentID = ST.ParentID
LEFT JOIN dbo.ClassMaster CM
    ON CM.ClassID = SS.ClassID
LEFT JOIN dbo.Class_Sections CS
    ON CS.ID = SS.SectionID
	LEFT JOIN dbo.[SocialCategory] SOCC
	on SOCC.ID=ST.Category
LEFT JOIN [dbo].[ReligionMaster] RM
    on RM.ID=ST.ReligionID
LEFT JOIN dbo.QuotaMaster QM
    ON QM.QuotaID = SS.QuotaID
WHERE SS.SBranchID = @SBranchID
  AND ISNULL(SS.Status, 0) = 1
  AND ISNULL(CM.Status, 1) = 1
ORDER BY
    CM.ClassID,
    CS.Name,
    TRY_CONVERT(INT, SS.RollNo),
    SS.RollNo,
    ST.Name;


