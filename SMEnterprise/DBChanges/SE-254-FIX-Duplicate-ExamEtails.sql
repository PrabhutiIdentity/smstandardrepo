/*
    Purpose:
    Prevent duplicate exam rows from being inserted when the same exam schedule
    is saved more than once from Admin/ExamManagement.

    Notes:
    1. Existing duplicate rows should be cleaned separately before applying this proc.
    2. This procedure uses a table variable instead of reusing a CTE across multiple
       statements, so it avoids the "Invalid object name 'FinalRows'" style issue.
*/

ALTER PROC [dbo].[sp_UpdateExamDetails]
(
    @ExamDetails ExamDetails READONLY
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE e
           SET e.EvaluationID = d.EvaluationID,
               e.ClassID = d.ClassID,
               e.GroupID = d.GroupID,
               e.SubjectID = d.SubjectID,
               e.IsLocked = d.IsLocked,
               e.ExamDate = d.ExamDate,
               e.StartTime = d.StartTime,
               e.EndTime = d.EndTime,
               e.IsApplicable = d.IsApplicable,
               e.ModifiedDate = d.ModifiedDate,
               e.UserID = d.UserID,
               e.MaxMarks = d.MaxMarks,
               e.PassMarks = d.PassMarks,
               e.MarkingScheme = d.MarkingScheme
        FROM ExamMaster e
        INNER JOIN @ExamDetails d
            ON d.ExamID = e.ExamID
        WHERE d.ExamID <> 0;

        DECLARE @FinalNewRows TABLE
        (
            ExamID int,
            EvaluationID int,
            ClassID int,
            GroupID int,
            SubjectID int,
            ExamDate date,
            StartTime nvarchar(50),
            EndTime nvarchar(50),
            SBranchID int,
            UserID int,
            CreatedDate datetime,
            ModifiedDate datetime,
            IsApplicable int,
            MaxMarks numeric(18,2),
            PassMarks numeric(18,2),
            IsLocked int,
            MarkingScheme int
        );

        ;WITH IncomingData AS
        (
            SELECT
                d.ExamID,
                d.EvaluationID,
                d.ClassID,
                d.GroupID,
                d.SubjectID,
                d.ExamDate,
                d.StartTime,
                d.EndTime,
                d.SBranchID,
                d.UserID,
                d.CreatedDate,
                d.ModifiedDate,
                d.IsApplicable,
                d.MaxMarks,
                d.PassMarks,
                d.IsLocked,
                d.MarkingScheme,
                ROW_NUMBER() OVER
                (
                    PARTITION BY d.EvaluationID, d.ClassID, d.GroupID, d.SubjectID, d.SBranchID
                    ORDER BY d.CreatedDate DESC, d.ExamID DESC
                ) AS RowNum
            FROM @ExamDetails d
            WHERE d.ExamID = 0
        )
        INSERT INTO @FinalNewRows
        (
            ExamID,
            EvaluationID,
            ClassID,
            GroupID,
            SubjectID,
            ExamDate,
            StartTime,
            EndTime,
            SBranchID,
            UserID,
            CreatedDate,
            ModifiedDate,
            IsApplicable,
            MaxMarks,
            PassMarks,
            IsLocked,
            MarkingScheme
        )
        SELECT
            ExamID,
            EvaluationID,
            ClassID,
            GroupID,
            SubjectID,
            ExamDate,
            StartTime,
            EndTime,
            SBranchID,
            UserID,
            CreatedDate,
            ModifiedDate,
            IsApplicable,
            MaxMarks,
            PassMarks,
            IsLocked,
            MarkingScheme
        FROM IncomingData
        WHERE RowNum = 1;

        UPDATE T
           SET T.ExamDate = S.ExamDate,
               T.StartTime = S.StartTime,
               T.EndTime = S.EndTime,
               T.IsApplicable = S.IsApplicable,
               T.ModifiedDate = S.ModifiedDate,
               T.UserID = S.UserID,
               T.MaxMarks = S.MaxMarks,
               T.PassMarks = S.PassMarks,
               T.IsLocked = S.IsLocked,
               T.MarkingScheme = S.MarkingScheme
        FROM ExamMaster T
        INNER JOIN @FinalNewRows S
            ON T.EvaluationID = S.EvaluationID
           AND T.ClassID = S.ClassID
           AND T.GroupID = S.GroupID
           AND T.SubjectID = S.SubjectID
           AND T.SBranchID = S.SBranchID;

        INSERT INTO ExamMaster
        (
            EvaluationID,
            ClassID,
            GroupID,
            SubjectID,
            ExamDate,
            StartTime,
            EndTime,
            SBranchID,
            UserID,
            CreatedDate,
            IsApplicable,
            MaxMarks,
            PassMarks,
            IsLocked,
            MarkingScheme
        )
        SELECT
            S.EvaluationID,
            S.ClassID,
            S.GroupID,
            S.SubjectID,
            S.ExamDate,
            S.StartTime,
            S.EndTime,
            S.SBranchID,
            S.UserID,
            S.CreatedDate,
            S.IsApplicable,
            S.MaxMarks,
            S.PassMarks,
            S.IsLocked,
            S.MarkingScheme
        FROM @FinalNewRows S
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM ExamMaster T
            WHERE T.EvaluationID = S.EvaluationID
              AND T.ClassID = S.ClassID
              AND T.GroupID = S.GroupID
              AND T.SubjectID = S.SubjectID
              AND T.SBranchID = S.SBranchID
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
