/*
    Purpose:
    Prevent duplicate inserts in fee structure and student custom fee save flows.

    Root cause:
    Existing procedures insert all rows where ID/SFID = 0 without checking whether
    the natural key already exists in the target table.

    Natural keys used here:
    1. ClassFeeStructureMaster:
       SBranchID + ClassID + GroupID + SessionID + FeeTypeID
    2. StudentFeeDetails:
       StudentID + SessionID + FeeTypeID
*/

ALTER PROC [dbo].[sp_UpdateFeeStructure]
(
    @StructureDetails ut_FeeStructureDetail READONLY,
    @SessionID int = 1
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE e
           SET e.FeeAmount = d.FeeAmount,
               e.Status = d.UserID
        FROM ClassFeeStructureMaster e
        INNER JOIN @StructureDetails d ON e.ID = d.ID
        WHERE d.ID <> 0;

        DECLARE @FinalNewRows TABLE
        (
            ID int,
            ClassID int,
            GroupID int,
            FeeTypeID int,
            FeeAmount numeric(18,2),
            SBranchID int,
            UserID int,
            CreatedDate datetime
        );

        ;WITH IncomingData AS
        (
            SELECT
                d.ID,
                d.ClassID,
                d.GroupID,
                d.FeeTypeID,
                d.FeeAmount,
                d.SBranchID,
                d.UserID,
                d.CreatedDate,
                ROW_NUMBER() OVER
                (
                    PARTITION BY d.SBranchID, d.ClassID, d.GroupID, d.FeeTypeID
                    ORDER BY d.CreatedDate DESC, d.ID DESC
                ) AS RowNum
            FROM @StructureDetails d
            WHERE d.ID = 0
        )
        INSERT INTO @FinalNewRows
        (
            ID,
            ClassID,
            GroupID,
            FeeTypeID,
            FeeAmount,
            SBranchID,
            UserID,
            CreatedDate
        )
        SELECT
            ID,
            ClassID,
            GroupID,
            FeeTypeID,
            FeeAmount,
            SBranchID,
            UserID,
            CreatedDate
        FROM IncomingData
        WHERE RowNum = 1;

        UPDATE T
           SET T.FeeAmount = S.FeeAmount,
               T.Status = S.UserID
        FROM ClassFeeStructureMaster T
        INNER JOIN @FinalNewRows S
            ON T.SBranchID = S.SBranchID
           AND T.ClassID = S.ClassID
           AND T.GroupID = S.GroupID
           AND T.FeeTypeID = S.FeeTypeID
           AND T.SessionID = @SessionID;

        INSERT INTO ClassFeeStructureMaster
        (
            ClassID,
            GroupID,
            FeeTypeID,
            FeeAmount,
            SBranchID,
            Status,
            CreatedDate,
            SessionID
        )
        SELECT
            S.ClassID,
            S.GroupID,
            S.FeeTypeID,
            S.FeeAmount,
            S.SBranchID,
            S.UserID,
            S.CreatedDate,
            @SessionID
        FROM @FinalNewRows S
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM ClassFeeStructureMaster T
            WHERE T.SBranchID = S.SBranchID
              AND T.ClassID = S.ClassID
              AND T.GroupID = S.GroupID
              AND T.FeeTypeID = S.FeeTypeID
              AND T.SessionID = @SessionID
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

GO
go
ALTER PROC [dbo].[sp_UpdateStudentCustomFee]
(
    @StudentSessionUID int,
    @IsCustomFee int,
    @FeeDetails ut_StudentFeeDetails READONLY
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE Student_Session
           SET IsCustomFee = @IsCustomFee
        WHERE StudentSessionUID = @StudentSessionUID;

        DECLARE @FinalRows TABLE
        (
            SFID int,
            StudentID int,
            FeeTypeID int,
            SessionID int,
            FeeAmount numeric(18,2),
            IsApplicable int
        );

        ;WITH IncomingData AS
        (
            SELECT
                d.SFID,
                d.StudentID,
                d.FeeTypeID,
                d.SessionID,
                d.FeeAmount,
                d.IsApplicable,
                ROW_NUMBER() OVER
                (
                    PARTITION BY d.StudentID, d.SessionID, d.FeeTypeID
                    ORDER BY d.SFID DESC
                ) AS RowNum
            FROM @FeeDetails d
        )
        INSERT INTO @FinalRows
        (
            SFID,
            StudentID,
            FeeTypeID,
            SessionID,
            FeeAmount,
            IsApplicable
        )
        SELECT
            SFID,
            StudentID,
            FeeTypeID,
            SessionID,
            FeeAmount,
            IsApplicable
        FROM IncomingData
        WHERE RowNum = 1;

        UPDATE T
           SET T.FeeAmount = S.FeeAmount,
               T.IsApplicable = S.IsApplicable
        FROM dbo.StudentFeeDetails T
        INNER JOIN @FinalRows S
            ON T.SFID = S.SFID
        WHERE S.SFID <> 0;

        UPDATE T
           SET T.FeeAmount = S.FeeAmount,
               T.IsApplicable = S.IsApplicable
        FROM dbo.StudentFeeDetails T
        INNER JOIN @FinalRows S
            ON T.StudentID = S.StudentID
           AND T.FeeTypeID = S.FeeTypeID
           AND T.SessionID = S.SessionID
        WHERE S.SFID = 0;

        INSERT INTO dbo.StudentFeeDetails
        (
            StudentID,
            FeeTypeID,
            SessionID,
            FeeAmount,
            IsApplicable
        )
        SELECT
            S.StudentID,
            S.FeeTypeID,
            S.SessionID,
            S.FeeAmount,
            S.IsApplicable
        FROM @FinalRows S
        WHERE S.SFID = 0
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.StudentFeeDetails T
              WHERE T.StudentID = S.StudentID
                AND T.FeeTypeID = S.FeeTypeID
                AND T.SessionID = S.SessionID
          );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
