/*
    Session Carry Forward - Branch Wise Catch-up Notes
    ==================================================

    Use case
    --------
    - Some students were already promoted
    - Carry-forward was generated only for some of them
    - Remaining promoted students need carry-forward later
    - Run branch-wise review and catch-up safely

    Recommended rule
    ----------------
    - Keep @ForceUpdate = 0 for catch-up
    - This avoids disturbing already-correct students
    - First run missing-list query
    - Then run preview
    - Then apply
    - Then run missing-list query again

    Required inputs
    ---------------
    - @SBranchID
    - @TargetSessionID
    - optional @ClassID
    - optional @SectionID
*/

/* =========================================================
   1. Set branch/session filters
   ========================================================= */
DECLARE @SBranchID int = 34;
DECLARE @TargetSessionID int = 150;
DECLARE @ClassID int = 0;      -- keep 0 for all classes
DECLARE @SectionID int = 0;    -- keep 0 for all sections
DECLARE @CarryForwardFeeTypeID int;

EXEC dbo.sp_GetSessionCarryForwardFeeTypeID
    @SBranchID = @SBranchID,
    @FeeTypeID = @CarryForwardFeeTypeID OUTPUT;

SELECT
    @SBranchID AS SBranchID,
    @TargetSessionID AS TargetSessionID,
    @ClassID AS ClassID,
    @SectionID AS SectionID,
    @CarryForwardFeeTypeID AS CarryForwardFeeTypeID;
GO

/* =========================================================
   2. Find promoted students whose carry-forward is still missing
   Missing means:
   - mapping row not found, OR
   - StudentFeeDetails carry-forward row not found
   ========================================================= */
DECLARE @SBranchID int = 34;
DECLARE @TargetSessionID int = 150;
DECLARE @ClassID int = 0;
DECLARE @SectionID int = 0;
DECLARE @CarryForwardFeeTypeID int;

EXEC dbo.sp_GetSessionCarryForwardFeeTypeID
    @SBranchID = @SBranchID,
    @FeeTypeID = @CarryForwardFeeTypeID OUTPUT;

SELECT
    SS.StudentID,
    SS.StudentSessionUID,
    SS.ClassID,
    SS.SectionID,
    SM.Name,
    SS.IsCustomFee,
    SCM.MapID,
    SFD.SFID AS CarryForwardSFID,
    SFD.FeeAmount AS CarryForwardAmount
FROM dbo.Student_Session SS
INNER JOIN dbo.StudentMaster SM
    ON SM.StudentID = SS.StudentID
LEFT JOIN dbo.SessionCarryForwardMap SCM
    ON SCM.SBranchID = @SBranchID
   AND SCM.StudentID = SS.StudentID
   AND SCM.TargetSessionID = @TargetSessionID
   AND SCM.TargetStudentSessionUID = SS.StudentSessionUID
   AND SCM.Status = 1
LEFT JOIN dbo.StudentFeeDetails SFD
    ON SFD.StudentID = SS.StudentID
   AND SFD.SessionID = SS.StudentSessionUID
   AND SFD.FeeTypeID = @CarryForwardFeeTypeID
   AND SFD.IsApplicable = 1
WHERE SS.SBranchID = @SBranchID
  AND SS.SessionID = @TargetSessionID
  AND (@ClassID = 0 OR SS.ClassID = @ClassID)
  AND (@SectionID = 0 OR SS.SectionID = @SectionID)
  AND (SCM.MapID IS NULL OR SFD.SFID IS NULL)
ORDER BY SS.ClassID, SS.SectionID, SS.StudentID;
GO

/* =========================================================
   3. Preview catch-up for whole branch / class / section
   ========================================================= */
EXEC dbo.sp_ApplySessionCarryForward
    @SBranchID = 34,
    @TargetSessionID = 150,
    @ClassID = 0,
    @SectionID = 0,
    @StudentID = 0,
    @PreviewOnly = 1,
    @ForceUpdate = 0,
    @RunBy = 'catchup-preview';
GO

/* =========================================================
   4. Apply catch-up for whole branch / class / section
   Safe mode:
   - @ForceUpdate = 0
   ========================================================= */
EXEC dbo.sp_ApplySessionCarryForward
    @SBranchID = 34,
    @TargetSessionID = 150,
    @ClassID = 0,
    @SectionID = 0,
    @StudentID = 0,
    @PreviewOnly = 0,
    @ForceUpdate = 0,
    @RunBy = 'catchup-apply';
GO

/* =========================================================
   5. Apply catch-up for one class / section only
   ========================================================= */
EXEC dbo.sp_ApplySessionCarryForward
    @SBranchID = 34,
    @TargetSessionID = 150,
    @ClassID = 1413,
    @SectionID = 0,
    @StudentID = 0,
    @PreviewOnly = 0,
    @ForceUpdate = 0,
    @RunBy = 'catchup-class';
GO

/* =========================================================
   6. Apply catch-up for one specific student only
   ========================================================= */
EXEC dbo.sp_ApplySessionCarryForward
    @SBranchID = 34,
    @TargetSessionID = 150,
    @ClassID = 0,
    @SectionID = 0,
    @StudentID = 46093,
    @PreviewOnly = 0,
    @ForceUpdate = 0,
    @RunBy = 'catchup-single';
GO

/* =========================================================
   7. Re-check missing list after catch-up
   Expected result:
   - zero rows for the selected branch/class/section
   ========================================================= */
DECLARE @SBranchID int = 34;
DECLARE @TargetSessionID int = 150;
DECLARE @ClassID int = 0;
DECLARE @SectionID int = 0;
DECLARE @CarryForwardFeeTypeID int;

EXEC dbo.sp_GetSessionCarryForwardFeeTypeID
    @SBranchID = @SBranchID,
    @FeeTypeID = @CarryForwardFeeTypeID OUTPUT;

SELECT
    SS.StudentID,
    SS.StudentSessionUID,
    SS.ClassID,
    SS.SectionID,
    SM.Name,
    SCM.MapID,
    SFD.SFID AS CarryForwardSFID
FROM dbo.Student_Session SS
INNER JOIN dbo.StudentMaster SM
    ON SM.StudentID = SS.StudentID
LEFT JOIN dbo.SessionCarryForwardMap SCM
    ON SCM.SBranchID = @SBranchID
   AND SCM.StudentID = SS.StudentID
   AND SCM.TargetSessionID = @TargetSessionID
   AND SCM.TargetStudentSessionUID = SS.StudentSessionUID
   AND SCM.Status = 1
LEFT JOIN dbo.StudentFeeDetails SFD
    ON SFD.StudentID = SS.StudentID
   AND SFD.SessionID = SS.StudentSessionUID
   AND SFD.FeeTypeID = @CarryForwardFeeTypeID
   AND SFD.IsApplicable = 1
WHERE SS.SBranchID = @SBranchID
  AND SS.SessionID = @TargetSessionID
  AND (@ClassID = 0 OR SS.ClassID = @ClassID)
  AND (@SectionID = 0 OR SS.SectionID = @SectionID)
  AND (SCM.MapID IS NULL OR SFD.SFID IS NULL)
ORDER BY SS.ClassID, SS.SectionID, SS.StudentID;
GO

/* =========================================================
   Notes
   =========================================================
   1. This catch-up process should be used only for newly promoted students
      where carry-forward was missed.

   2. Do not use blind auto-apply on branches/students where manual custom due
      handling is already in place.

   3. If a branch does not want carry-forward:
      keep SBranchMaster.IsCarryForwardApplicable = 0

   4. If catch-up is done successfully:
      - StudentFeeDetails carry-forward row should exist
      - SessionCarryForwardMap row should exist
      - current session fee screen should show carry-forward
      - old session payment should remain blocked
*/
