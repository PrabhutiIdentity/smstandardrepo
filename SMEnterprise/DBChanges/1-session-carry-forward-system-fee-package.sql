/*
    Session Carry Forward System Fee Package
    ---------------------------------------
    Purpose:
    0. Add branch-level carry-forward applicability flag.
    1. Add a stable system identifier on FeeTypeMaster.
    2. Mark Session-Carry-forward as a protected system fee head.
    3. Provide helper procs to get/create the branch-wise fee type safely.

    Notes:
    - FeeTypeID remains branch-wise and database-generated.
    - Identification is done by SystemFeeCode = 'SESSION_CARRY_FORWARD'.
    - Branch feature toggle is controlled by SBranchMaster.IsCarryForwardApplicable.
    - Do not run blindly on every branch without review.
*/

IF COL_LENGTH('dbo.SBranchMaster', 'IsCarryForwardApplicable') IS NULL
BEGIN
    ALTER TABLE dbo.SBranchMaster
    ADD IsCarryForwardApplicable bit NOT NULL
        CONSTRAINT DF_SBranchMaster_IsCarryForwardApplicable DEFAULT ((0));
END;
GO

IF COL_LENGTH('dbo.FeeTypeMaster', 'SystemFeeCode') IS NULL
BEGIN
    ALTER TABLE dbo.FeeTypeMaster
    ADD SystemFeeCode nvarchar(50) NULL;
END;
GO

IF COL_LENGTH('dbo.FeeTypeMaster', 'IsSystemFee') IS NULL
BEGIN
    ALTER TABLE dbo.FeeTypeMaster
    ADD IsSystemFee bit NOT NULL
        CONSTRAINT DF_FeeTypeMaster_IsSystemFee DEFAULT ((0));
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.FeeTypeMaster')
      AND name = N'IX_FeeTypeMaster_SystemFeeCode_Branch'
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_FeeTypeMaster_SystemFeeCode_Branch
    ON dbo.FeeTypeMaster (SBranchID, SystemFeeCode)
    WHERE SystemFeeCode IS NOT NULL;
END;
GO

-- Backfill already-created carry-forward fee heads.
UPDATE FTM
SET
    FTM.SystemFeeCode = 'SESSION_CARRY_FORWARD',
    FTM.IsSystemFee = 1
FROM dbo.FeeTypeMaster FTM
WHERE LTRIM(RTRIM(ISNULL(FTM.FeeTypeName, ''))) = 'Session-Carry-forward'
  AND (ISNULL(FTM.SystemFeeCode, '') = '' OR FTM.IsSystemFee = 0);
GO

IF OBJECT_ID(N'dbo.sp_GetSessionCarryForwardFeeTypeID', N'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_GetSessionCarryForwardFeeTypeID AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_GetSessionCarryForwardFeeTypeID
(
    @SBranchID int,
    @FeeTypeID int OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @FeeTypeID = NULL;

    SELECT TOP 1
        @FeeTypeID = FeeTypeID
    FROM dbo.FeeTypeMaster
    WHERE SBranchID = @SBranchID
      AND SystemFeeCode = 'SESSION_CARRY_FORWARD'
    ORDER BY FeeTypeID DESC;
END;
GO

IF OBJECT_ID(N'dbo.sp_EnsureSessionCarryForwardFeeType', N'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_EnsureSessionCarryForwardFeeType AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_EnsureSessionCarryForwardFeeType
(
    @SBranchID int,
    @CreatedBy int = 0,
    @FeeTypeID int OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.SBranchMaster
        WHERE SBranchID = @SBranchID
          AND IsCarryForwardApplicable = 1
    )
    BEGIN
        RAISERROR('Carry-forward is disabled for this branch.', 16, 1);
        RETURN;
    END;

    EXEC dbo.sp_GetSessionCarryForwardFeeTypeID
        @SBranchID = @SBranchID,
        @FeeTypeID = @FeeTypeID OUTPUT;

    IF @FeeTypeID IS NOT NULL
    BEGIN
        RETURN;
    END;

    INSERT INTO dbo.FeeTypeMaster
    (
        FeeTypeName,
        FeeTypeApplicable,
        ChartColor,
        UserID,
        IsBaseType,
        CreatedDate,
        ModifiedDate,
        ShortName,
        SBranchID,
        Months,
        SystemFeeCode,
        IsSystemFee
    )
    VALUES
    (
        'Session-Carry-forward',
        1,
        NULL,
        CASE WHEN ISNULL(@CreatedBy, 0) = 0 THEN @SBranchID ELSE @CreatedBy END,
        NULL,
        GETDATE(),
        NULL,
        'SCF',
        @SBranchID,
        NULL,
        'SESSION_CARRY_FORWARD',
        1
    );

    SET @FeeTypeID = CAST(SCOPE_IDENTITY() AS int);
END;
GO

IF OBJECT_ID(N'dbo.trg_FeeTypeMaster_ProtectSystemFee_Update', N'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_FeeTypeMaster_ProtectSystemFee_Update;
GO

CREATE TRIGGER dbo.trg_FeeTypeMaster_ProtectSystemFee_Update
ON dbo.FeeTypeMaster
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        INNER JOIN deleted d ON d.FeeTypeID = i.FeeTypeID
        WHERE d.SystemFeeCode = 'SESSION_CARRY_FORWARD'
          AND
          (
              ISNULL(i.SystemFeeCode, '') <> ISNULL(d.SystemFeeCode, '')
              OR ISNULL(i.IsSystemFee, 0) <> ISNULL(d.IsSystemFee, 0)
              OR ISNULL(i.FeeTypeApplicable, 0) <> ISNULL(d.FeeTypeApplicable, 0)
              OR ISNULL(i.SBranchID, 0) <> ISNULL(d.SBranchID, 0)
              OR LTRIM(RTRIM(ISNULL(i.FeeTypeName, ''))) <> LTRIM(RTRIM(ISNULL(d.FeeTypeName, '')))
          )
    )
    BEGIN
        RAISERROR('Session-Carry-forward system fee cannot be renamed, deleted, moved, or changed to another fee type.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;
END;
GO

IF OBJECT_ID(N'dbo.trg_FeeTypeMaster_ProtectSystemFee_Delete', N'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_FeeTypeMaster_ProtectSystemFee_Delete;
GO

CREATE TRIGGER dbo.trg_FeeTypeMaster_ProtectSystemFee_Delete
ON dbo.FeeTypeMaster
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM deleted
        WHERE SystemFeeCode = 'SESSION_CARRY_FORWARD'
    )
    BEGIN
        RAISERROR('Session-Carry-forward system fee cannot be deleted.', 16, 1);
        RETURN;
    END;

    DELETE FTM
    FROM dbo.FeeTypeMaster FTM
    INNER JOIN deleted d ON d.FeeTypeID = FTM.FeeTypeID;
END;
GO

-- Review query: find branch-wise duplicates before rollout.
SELECT
    SBranchID,
    COUNT(*) AS CarryForwardFeeHeads
FROM dbo.FeeTypeMaster
WHERE SystemFeeCode = 'SESSION_CARRY_FORWARD'
GROUP BY SBranchID
HAVING COUNT(*) > 1;
GO
