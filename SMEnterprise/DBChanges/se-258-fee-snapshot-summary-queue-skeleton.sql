/*
Parallel fee snapshot architecture
----------------------------------
Purpose:
- Keep current live fee collection logic untouched.
- Create new tables and skeleton procedures for a parallel, rebuildable,
  branch-scoped fee snapshot system.
- Limit operational scope to one active branch at a time and only its
  active session + immediate previous session.

This script intentionally does NOT switch any live report/procedure to
the new tables yet.
*/

IF OBJECT_ID(N'dbo.StudentMonthFeeSnapshot', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StudentMonthFeeSnapshot
    (
        SnapshotID bigint IDENTITY(1,1) NOT NULL,
        SBranchID int NOT NULL,
        SessionID int NOT NULL,
        StudentID int NOT NULL,
        StudentSessionUID int NULL,
        ClassID int NOT NULL,
        SectionID int NOT NULL,
        FeeYear int NOT NULL,
        FeeMonth int NOT NULL,
        FeeTypeID int NOT NULL,
        FeeTypeApplicable int NULL,
        ApplicableAmount decimal(18,2) NOT NULL CONSTRAINT DF_StudentMonthFeeSnapshot_ApplicableAmount DEFAULT (0),
        ApprovedDiscountAmount decimal(18,2) NOT NULL CONSTRAINT DF_StudentMonthFeeSnapshot_ApprovedDiscountAmount DEFAULT (0),
        PaymentDiscountAmount decimal(18,2) NOT NULL CONSTRAINT DF_StudentMonthFeeSnapshot_PaymentDiscountAmount DEFAULT (0),
        EffectiveDiscountAmount decimal(18,2) NOT NULL CONSTRAINT DF_StudentMonthFeeSnapshot_EffectiveDiscountAmount DEFAULT (0),
        PaidAmount decimal(18,2) NOT NULL CONSTRAINT DF_StudentMonthFeeSnapshot_PaidAmount DEFAULT (0),
        PreviousDueAmount decimal(18,2) NOT NULL CONSTRAINT DF_StudentMonthFeeSnapshot_PreviousDueAmount DEFAULT (0),
        LateFeeAmount decimal(18,2) NOT NULL CONSTRAINT DF_StudentMonthFeeSnapshot_LateFeeAmount DEFAULT (0),
        BalanceAmount decimal(18,2) NOT NULL CONSTRAINT DF_StudentMonthFeeSnapshot_BalanceAmount DEFAULT (0),
        DueCount int NOT NULL CONSTRAINT DF_StudentMonthFeeSnapshot_DueCount DEFAULT (0),
        IsSettled bit NOT NULL CONSTRAINT DF_StudentMonthFeeSnapshot_IsSettled DEFAULT (0),
        SourceTag nvarchar(50) NOT NULL CONSTRAINT DF_StudentMonthFeeSnapshot_SourceTag DEFAULT (N'PARALLEL'),
        LastCalculatedOn datetime NOT NULL CONSTRAINT DF_StudentMonthFeeSnapshot_LastCalculatedOn DEFAULT (GETDATE()),
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
            StudentID,
            FeeYear,
            FeeMonth,
            FeeTypeID
        );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.StudentMonthFeeSnapshot')
      AND name = N'IX_StudentMonthFeeSnapshot_ClassMonth'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_StudentMonthFeeSnapshot_ClassMonth
        ON dbo.StudentMonthFeeSnapshot
        (
            SBranchID,
            SessionID,
            ClassID,
            SectionID,
            FeeYear,
            FeeMonth
        )
        INCLUDE
        (
            ApplicableAmount,
            EffectiveDiscountAmount,
            PaidAmount,
            PreviousDueAmount,
            LateFeeAmount,
            BalanceAmount,
            DueCount,
            IsSettled
        );
END
GO

IF OBJECT_ID(N'dbo.ClassMonthCollectionSummary', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClassMonthCollectionSummary
    (
        SummaryID bigint IDENTITY(1,1) NOT NULL,
        SBranchID int NOT NULL,
        SessionID int NOT NULL,
        ClassID int NOT NULL,
        SectionID int NOT NULL,
        FeeYear int NOT NULL,
        FeeMonth int NOT NULL,
        Amount decimal(18,2) NOT NULL CONSTRAINT DF_ClassMonthCollectionSummary_Amount DEFAULT (0),
        Discount decimal(18,2) NOT NULL CONSTRAINT DF_ClassMonthCollectionSummary_Discount DEFAULT (0),
        Paid decimal(18,2) NOT NULL CONSTRAINT DF_ClassMonthCollectionSummary_Paid DEFAULT (0),
        Balance decimal(18,2) NOT NULL CONSTRAINT DF_ClassMonthCollectionSummary_Balance DEFAULT (0),
        StudentCount int NOT NULL CONSTRAINT DF_ClassMonthCollectionSummary_StudentCount DEFAULT (0),
        SettledStudentCount int NOT NULL CONSTRAINT DF_ClassMonthCollectionSummary_SettledStudentCount DEFAULT (0),
        LastCalculatedOn datetime NOT NULL CONSTRAINT DF_ClassMonthCollectionSummary_LastCalculatedOn DEFAULT (GETDATE()),
        CONSTRAINT PK_ClassMonthCollectionSummary PRIMARY KEY CLUSTERED (SummaryID)
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.ClassMonthCollectionSummary')
      AND name = N'UX_ClassMonthCollectionSummary_Key'
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_ClassMonthCollectionSummary_Key
        ON dbo.ClassMonthCollectionSummary
        (
            SBranchID,
            SessionID,
            ClassID,
            SectionID,
            FeeYear,
            FeeMonth
        );
END
GO

IF OBJECT_ID(N'dbo.FeeRecalcQueue', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FeeRecalcQueue
    (
        QueueID bigint IDENTITY(1,1) NOT NULL,
        SBranchID int NOT NULL,
        SessionID int NULL,
        StudentID int NULL,
        ClassID int NULL,
        SectionID int NULL,
        FeeYear int NULL,
        FeeMonth int NULL,
        ScopeType nvarchar(30) NOT NULL,
        TriggerSource nvarchar(50) NOT NULL,
        ReferenceID bigint NULL,
        Notes nvarchar(500) NULL,
        Status nvarchar(20) NOT NULL CONSTRAINT DF_FeeRecalcQueue_Status DEFAULT (N'PENDING'),
        CreatedOn datetime NOT NULL CONSTRAINT DF_FeeRecalcQueue_CreatedOn DEFAULT (GETDATE()),
        ProcessedOn datetime NULL,
        CONSTRAINT PK_FeeRecalcQueue PRIMARY KEY CLUSTERED (QueueID)
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.FeeRecalcQueue')
      AND name = N'IX_FeeRecalcQueue_StatusBranch'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_FeeRecalcQueue_StatusBranch
        ON dbo.FeeRecalcQueue (Status, SBranchID, SessionID, ScopeType, CreatedOn);
END
GO

IF OBJECT_ID(N'dbo.sp_RecalcStudentMonthSnapshot', N'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_RecalcStudentMonthSnapshot AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_RecalcStudentMonthSnapshot
    @SBranchID int,
    @SessionID int,
    @StudentID int,
    @FeeYear int,
    @FeeMonth int
AS
BEGIN
    SET NOCOUNT ON;

    /*
    Skeleton only.

    Target behavior:
    1. Resolve student current class/section for the supplied session.
    2. Build fee-type rows for one student + month from raw source tables:
       - Student_Session
       - StudentFeeDetails
       - PaymentDetails / v_PaymentDetails
       - FeeDiscountRequestMaster / FeeDiscountRequestDetails
       - no-fee-month rules
       - transport/hostel rules
       - quota discount rules
       - late fee rules
    3. Upsert rows into dbo.StudentMonthFeeSnapshot.
    4. Effective settlement rule:
       EffectiveDiscountAmount = max(PaymentDiscountAmount, ApprovedDiscountAmount)
       BalanceAmount = max(0, ApplicableAmount + PreviousDueAmount + LateFeeAmount
                              - EffectiveDiscountAmount - PaidAmount)
    */

    RAISERROR('sp_RecalcStudentMonthSnapshot skeleton created. Business calculation not implemented yet.', 10, 1);
END
GO

IF OBJECT_ID(N'dbo.sp_RecalcClassMonthSummary', N'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_RecalcClassMonthSummary AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_RecalcClassMonthSummary
    @SBranchID int,
    @SessionID int,
    @ClassID int,
    @SectionID int,
    @FeeYear int,
    @FeeMonth int
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Src AS
    (
        SELECT
            SBranchID,
            SessionID,
            ClassID,
            SectionID,
            FeeYear,
            FeeMonth,
            ApplicableAmount + PreviousDueAmount + LateFeeAmount AS Amount,
            EffectiveDiscountAmount AS Discount,
            PaidAmount AS Paid,
            BalanceAmount AS Balance,
            StudentID,
            IsSettled
        FROM dbo.StudentMonthFeeSnapshot
        WHERE SBranchID = @SBranchID
          AND SessionID = @SessionID
          AND ClassID = @ClassID
          AND SectionID = @SectionID
          AND FeeYear = @FeeYear
          AND FeeMonth = @FeeMonth
    )
    MERGE dbo.ClassMonthCollectionSummary AS T
    USING
    (
        SELECT
            SBranchID,
            SessionID,
            ClassID,
            SectionID,
            FeeYear,
            FeeMonth,
            SUM(Amount) AS Amount,
            SUM(Discount) AS Discount,
            SUM(Paid) AS Paid,
            SUM(Balance) AS Balance,
            COUNT(DISTINCT StudentID) AS StudentCount,
            SUM(CASE WHEN IsSettled = 1 THEN 1 ELSE 0 END) AS SettledStudentCount
        FROM Src
        GROUP BY
            SBranchID,
            SessionID,
            ClassID,
            SectionID,
            FeeYear,
            FeeMonth
    ) AS S
    ON  T.SBranchID = S.SBranchID
    AND T.SessionID = S.SessionID
    AND T.ClassID = S.ClassID
    AND T.SectionID = S.SectionID
    AND T.FeeYear = S.FeeYear
    AND T.FeeMonth = S.FeeMonth
    WHEN MATCHED THEN
        UPDATE SET
            Amount = S.Amount,
            Discount = S.Discount,
            Paid = S.Paid,
            Balance = S.Balance,
            StudentCount = S.StudentCount,
            SettledStudentCount = S.SettledStudentCount,
            LastCalculatedOn = GETDATE()
    WHEN NOT MATCHED THEN
        INSERT
        (
            SBranchID,
            SessionID,
            ClassID,
            SectionID,
            FeeYear,
            FeeMonth,
            Amount,
            Discount,
            Paid,
            Balance,
            StudentCount,
            SettledStudentCount,
            LastCalculatedOn
        )
        VALUES
        (
            S.SBranchID,
            S.SessionID,
            S.ClassID,
            S.SectionID,
            S.FeeYear,
            S.FeeMonth,
            S.Amount,
            S.Discount,
            S.Paid,
            S.Balance,
            S.StudentCount,
            S.SettledStudentCount,
            GETDATE()
        );
END
GO

/*
Recommended trigger/event sources for queue insert only:

1. PaymentDetails
2. PaymentMaster
3. FeeDiscountRequestMaster
4. FeeDiscountRequestDetails
5. StudentFeeDetails
6. Student_Session
7. no-fee-month configuration tables
8. transport/hostel assignment tables

Important:
- Triggers should only enqueue rows in dbo.FeeRecalcQueue.
- Heavy recalculation should be done by a background processor or manual proc.
*/
