/*
Parallel snapshot path for fee collection reporting.

Goal:
- Keep current live procedures untouched.
- Build a branch-wise student-month snapshot using the existing trusted source
  procedure dbo.sp_GetClassGroupFeeListOnly.
- Limit rebuild scope to the active session and its immediate previous session
  for the target branch unless a specific session is passed.

Notes:
- This table stores student-month summary totals, not fee-type detail rows.
- Class-wise / section-wise summaries can be aggregated quickly from this table.
*/

IF OBJECT_ID(N'dbo.StudentMonthFeeSnapshot', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StudentMonthFeeSnapshot
    (
        SnapshotID       bigint IDENTITY(1,1) NOT NULL,
        SBranchID        int NOT NULL,
        SessionID        int NOT NULL,
        ClassID          int NOT NULL,
        SectionID        int NOT NULL,
        StudentID        int NOT NULL,
        FeeYear          int NOT NULL,
        FeeMonth         int NOT NULL,
        Amount           decimal(18,2) NOT NULL,
        PreviousDue      decimal(18,2) NOT NULL,
        LateFee          decimal(18,2) NOT NULL,
        Discount         decimal(18,2) NOT NULL,
        Paid             decimal(18,2) NOT NULL,
        Balance          decimal(18,2) NOT NULL,
        SourceProc       nvarchar(100) NOT NULL,
        SnapshotOn       datetime NOT NULL,
        CONSTRAINT PK_StudentMonthFeeSnapshot PRIMARY KEY CLUSTERED (SnapshotID)
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.StudentMonthFeeSnapshot')
      AND name = N'UX_StudentMonthFeeSnapshot_Key'
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_StudentMonthFeeSnapshot_Key
        ON dbo.StudentMonthFeeSnapshot
        (
            SBranchID,
            SessionID,
            ClassID,
            SectionID,
            StudentID,
            FeeYear,
            FeeMonth
        );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.StudentMonthFeeSnapshot')
      AND name = N'IX_StudentMonthFeeSnapshot_BranchSessionMonth'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_StudentMonthFeeSnapshot_BranchSessionMonth
        ON dbo.StudentMonthFeeSnapshot
        (
            SBranchID,
            SessionID,
            FeeYear,
            FeeMonth,
            ClassID,
            SectionID
        )
        INCLUDE (Amount, PreviousDue, LateFee, Discount, Paid, Balance);
END
GO

IF OBJECT_ID(N'dbo.sp_RebuildStudentMonthFeeSnapshotByBranch', N'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_RebuildStudentMonthFeeSnapshotByBranch AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_RebuildStudentMonthFeeSnapshotByBranch
    @SBranchID int,
    @SessionID int = 0,
    @ClearExisting bit = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurDate date = CAST(GETDATE() AS date);

    DECLARE @TargetSessions TABLE
    (
        SessionID int PRIMARY KEY,
        SessionStartDate date NOT NULL,
        SessionEndDate date NOT NULL
    );

    IF ISNULL(@SessionID, 0) > 0
    BEGIN
        INSERT INTO @TargetSessions (SessionID, SessionStartDate, SessionEndDate)
        SELECT SessionID, CAST(SessionStartDate AS date), CAST(SessionEndDate AS date)
        FROM SessionMaster
        WHERE SBranchID = @SBranchID
          AND SessionID = @SessionID;
    END
    ELSE
    BEGIN
        ;WITH BranchSessions AS
        (
            SELECT
                SessionID,
                CAST(SessionStartDate AS date) AS SessionStartDate,
                CAST(SessionEndDate AS date) AS SessionEndDate,
                ROW_NUMBER() OVER
                (
                    ORDER BY
                        CASE WHEN SessionStatus = 1 THEN 0 ELSE 1 END,
                        SessionStartDate DESC,
                        SessionID DESC
                ) AS RowNo
            FROM SessionMaster
            WHERE SBranchID = @SBranchID
        )
        INSERT INTO @TargetSessions (SessionID, SessionStartDate, SessionEndDate)
        SELECT SessionID, SessionStartDate, SessionEndDate
        FROM BranchSessions
        WHERE RowNo <= 2;
    END

    IF NOT EXISTS (SELECT 1 FROM @TargetSessions)
    BEGIN
        RAISERROR('No session found for the supplied branch/session.', 16, 1);
        RETURN;
    END

    IF @ClearExisting = 1
    BEGIN
        DELETE SS
        FROM dbo.StudentMonthFeeSnapshot SS
        INNER JOIN @TargetSessions TS
            ON TS.SessionID = SS.SessionID
        WHERE SS.SBranchID = @SBranchID;
    END

    CREATE TABLE #ClassSections
    (
        ClassID int NOT NULL,
        ClassName nvarchar(100) NOT NULL,
        SectionID int NOT NULL,
        SectionName nvarchar(100) NOT NULL
    );

    CREATE TABLE #CollectionBase
    (
        StudentID int,
        Name nvarchar(200),
        RollNo nvarchar(100),
        Gender int,
        Photo nvarchar(200),
        ClassID int,
        FeePaymentMode int,
        StudentSID nvarchar(50),
        FromDate date,
        ToDate date,
        QuotaID int,
        SessionID int,
        VehicleRouteID int,
        HostelRoomID int,
        SectionID int,
        SessionStartDate date,
        SessionEndDate date,
        IsAdmissionFee int,
        SchoolUID nvarchar(100),
        FeeAmount decimal(18,2),
        PreviousDue decimal(18,2),
        LateFee decimal(18,2),
        Discounts decimal(18,2),
        Paid decimal(18,2)
    );

    DECLARE
        @LoopSessionID int,
        @LoopSessionStart date,
        @LoopSessionEnd date,
        @ClassID int,
        @SectionID int,
        @MonthDate date,
        @QDate date;

    DECLARE db_SessionCursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT SessionID, SessionStartDate, SessionEndDate
    FROM @TargetSessions
    ORDER BY SessionStartDate DESC, SessionID DESC;

    OPEN db_SessionCursor;
    FETCH NEXT FROM db_SessionCursor INTO @LoopSessionID, @LoopSessionStart, @LoopSessionEnd;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        DELETE FROM #ClassSections;

        INSERT INTO #ClassSections (ClassID, ClassName, SectionID, SectionName)
        SELECT
            CM.ClassID,
            CM.ClassName,
            CS.ID AS SectionID,
            CS.Name AS SectionName
        FROM ClassMaster CM
        INNER JOIN Class_Sections CS
            ON CS.ClassID = CM.ClassID
        WHERE CM.SBranchID = @SBranchID
          AND CM.Status = 1
          AND CS.Status = 1;

        DECLARE db_ClassSectionCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT ClassID, SectionID
        FROM #ClassSections
        ORDER BY ClassID, SectionID;

        OPEN db_ClassSectionCursor;
        FETCH NEXT FROM db_ClassSectionCursor INTO @ClassID, @SectionID;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @MonthDate = DATEFROMPARTS(YEAR(@LoopSessionStart), MONTH(@LoopSessionStart), 1);

            WHILE @MonthDate <= DATEFROMPARTS(YEAR(@LoopSessionEnd), MONTH(@LoopSessionEnd), 1)
            BEGIN
                SET @QDate = @MonthDate;

                DELETE FROM #CollectionBase;

                INSERT INTO #CollectionBase
                EXEC dbo.sp_GetClassGroupFeeListOnly
                    @ClassID = @ClassID,
                    @SectionID = @SectionID,
                    @SBranchID = @SBranchID,
                    @SessionID = @LoopSessionID,
                    @QDate = @QDate,
                    @CurDate = @CurDate,
                    @SStudentID = 0;

                INSERT INTO dbo.StudentMonthFeeSnapshot
                (
                    SBranchID,
                    SessionID,
                    ClassID,
                    SectionID,
                    StudentID,
                    FeeYear,
                    FeeMonth,
                    Amount,
                    PreviousDue,
                    LateFee,
                    Discount,
                    Paid,
                    Balance,
                    SourceProc,
                    SnapshotOn
                )
                SELECT
                    @SBranchID,
                    @LoopSessionID,
                    @ClassID,
                    @SectionID,
                    CB.StudentID,
                    YEAR(@QDate),
                    MONTH(@QDate),
                    ISNULL(CB.FeeAmount,0),
                    ISNULL(CB.PreviousDue,0),
                    ISNULL(CB.LateFee,0),
                    ISNULL(CB.Discounts,0),
                    ISNULL(CB.Paid,0),
                    ISNULL(CB.FeeAmount,0) + ISNULL(CB.PreviousDue,0) + ISNULL(CB.LateFee,0)
                        - ISNULL(CB.Discounts,0) - ISNULL(CB.Paid,0),
                    N'sp_GetClassGroupFeeListOnly',
                    GETDATE()
                FROM #CollectionBase CB;

                SET @MonthDate = DATEADD(MONTH, 1, @MonthDate);
            END

            FETCH NEXT FROM db_ClassSectionCursor INTO @ClassID, @SectionID;
        END

        CLOSE db_ClassSectionCursor;
        DEALLOCATE db_ClassSectionCursor;

        FETCH NEXT FROM db_SessionCursor INTO @LoopSessionID, @LoopSessionStart, @LoopSessionEnd;
    END

    CLOSE db_SessionCursor;
    DEALLOCATE db_SessionCursor;

    SELECT
        SBranchID,
        SessionID,
        COUNT(*) AS SnapshotRows,
        MIN(SnapshotOn) AS FirstSnapshotOn,
        MAX(SnapshotOn) AS LastSnapshotOn
    FROM dbo.StudentMonthFeeSnapshot
    WHERE SBranchID = @SBranchID
      AND SessionID IN (SELECT SessionID FROM @TargetSessions)
    GROUP BY SBranchID, SessionID
    ORDER BY SessionID DESC;
END
GO

/*
Usage examples:

-- Active + immediate previous session for one branch
EXEC dbo.sp_RebuildStudentMonthFeeSnapshotByBranch @SBranchID = 41;

-- Only one session for one branch
EXEC dbo.sp_RebuildStudentMonthFeeSnapshotByBranch @SBranchID = 41, @SessionID = 137;

-- Read summary from snapshot
SELECT
    SBranchID,
    SessionID,
    ClassID,
    SectionID,
    FeeYear,
    FeeMonth,
    SUM(Amount + PreviousDue + LateFee) AS Amount,
    SUM(Discount) AS Discount,
    SUM(Paid) AS Paid,
    SUM(Balance) AS Balance
FROM dbo.StudentMonthFeeSnapshot
WHERE SBranchID = 41
GROUP BY
    SBranchID,
    SessionID,
    ClassID,
    SectionID,
    FeeYear,
    FeeMonth
ORDER BY SessionID, FeeYear, FeeMonth, ClassID, SectionID;
*/
