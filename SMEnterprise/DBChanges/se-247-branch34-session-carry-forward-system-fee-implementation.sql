/*
    Branch 34 implementation script
    -------------------------------
    Run only after se-246-session-carry-forward-system-fee-package.sql.

    Purpose:
    1. Mark/create branch 34 Session-Carry-forward fee as system fee.
    2. Return the branch 34 fee type id for review.
    3. Optionally attach it to every class/group in the target session with FeeAmount = 0.

    Review first. Do not run blindly on another branch.
*/

DECLARE @SBranchID int = 34;
DECLARE @TargetSessionID int = 150; -- change if needed
DECLARE @CarryForwardFeeTypeID int;

UPDATE dbo.SBranchMaster
SET IsCarryForwardApplicable = 1
WHERE SBranchID = @SBranchID;

EXEC dbo.sp_EnsureSessionCarryForwardFeeType
    @SBranchID = @SBranchID,
    @CreatedBy = 34,
    @FeeTypeID = @CarryForwardFeeTypeID OUTPUT;

-- Ensure the existing branch 34 fee head is flagged as protected system fee.
UPDATE dbo.FeeTypeMaster
SET
    SystemFeeCode = 'SESSION_CARRY_FORWARD',
    IsSystemFee = 1
WHERE FeeTypeID = @CarryForwardFeeTypeID
  AND SBranchID = @SBranchID;

SELECT
    FeeTypeID,
    FeeTypeName,
    ShortName,
    FeeTypeApplicable,
    SBranchID,
    SystemFeeCode,
    IsSystemFee
FROM dbo.FeeTypeMaster
WHERE FeeTypeID = @CarryForwardFeeTypeID;

/*
    Optional session setup:
    Add Session-Carry-forward in current target session for all class/group combinations.
    Amount stays 0; student-wise amount will continue to come from StudentFeeDetails.
*/

;WITH ClassGroups AS
(
    SELECT DISTINCT
        CS.ClassID,
        CS.GroupID
    FROM dbo.Class_Sections CS
    WHERE ISNULL(CS.ClassID, 0) <> 0
      AND ISNULL(CS.GroupID, 0) <> 0
)
MERGE dbo.ClassFeeStructureMaster AS T
USING
(
    SELECT
        CG.ClassID,
        CG.GroupID,
        @TargetSessionID AS SessionID,
        @CarryForwardFeeTypeID AS FeeTypeID,
        CAST(0 AS numeric(10,2)) AS FeeAmount,
        1 AS Status
    FROM ClassGroups CG
) AS S
ON T.ClassID = S.ClassID
AND T.GroupID = S.GroupID
AND T.SessionID = S.SessionID
AND T.FeeTypeID = S.FeeTypeID
WHEN MATCHED THEN
    UPDATE SET
        T.Status = 1
WHEN NOT MATCHED THEN
    INSERT
    (
        ClassID,
        GroupID,
        SessionID,
        FeeTypeID,
        FeeAmount,
        Status
    )
    VALUES
    (
        S.ClassID,
        S.GroupID,
        S.SessionID,
        S.FeeTypeID,
        S.FeeAmount,
        S.Status
    );

SELECT
    CFS.ClassID,
    CFS.GroupID,
    CFS.SessionID,
    CFS.FeeTypeID,
    CFS.FeeAmount,
    CFS.Status
FROM dbo.ClassFeeStructureMaster CFS
WHERE CFS.SessionID = @TargetSessionID
  AND CFS.FeeTypeID = @CarryForwardFeeTypeID
ORDER BY CFS.ClassID, CFS.GroupID;
GO
