IF OBJECT_ID(N'dbo.sp_GetClassWiseCollectionSummary', N'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_GetClassWiseCollectionSummary AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[sp_GetClassWiseCollectionSummary]
(
    @SBranchID int,
    @SessionID int,
    @ClassID int = 0,
    @SectionID int = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SessionStartDate date;
    DECLARE @SessionEndDate date;
    DECLARE @CurDate date = CAST(GETDATE() AS date);

    IF (ISNULL(@SessionID, 0) = 0)
    BEGIN
        SELECT TOP 1
            @SessionID = SessionID
        FROM SessionMaster
        WHERE SBranchID = @SBranchID
        ORDER BY CASE WHEN SessionStatus = 1 THEN 0 ELSE 1 END, SessionStartDate DESC;
    END;

    SELECT
        @SessionStartDate = SessionStartDate,
        @SessionEndDate = SessionEndDate
    FROM SessionMaster
    WHERE SessionID = @SessionID
      AND SBranchID = @SBranchID;

    IF (@SessionStartDate IS NULL OR @SessionEndDate IS NULL)
    BEGIN
        RETURN;
    END;

    DECLARE @ClassSections TABLE
    (
        RowID int IDENTITY(1,1),
        ClassID int,
        ClassName nvarchar(100),
        SectionID int,
        SectionName nvarchar(100)
    );

    INSERT INTO @ClassSections (ClassID, ClassName, SectionID, SectionName)
    SELECT
        CM.ClassID,
        CM.ClassName,
        CS.ID,
        CS.Name
    FROM ClassMaster CM
    INNER JOIN Class_Sections CS ON CS.ClassID = CM.ClassID
    WHERE CM.SBranchID = @SBranchID
      AND CM.Status = 1
      AND CS.Status = 1
      AND (@ClassID = 0 OR CM.ClassID = @ClassID)
      AND (@SectionID = 0 OR CS.ID = @SectionID)
    ORDER BY CM.ClassID, CS.ID;

    DECLARE @Summary TABLE
    (
        ClassID int,
        ClassName nvarchar(100),
        SectionID int,
        SectionName nvarchar(100),
        FeeMonth int,
        FeeYear int,
        Amount numeric(18,2),
        Discount numeric(18,2),
        Paid numeric(18,2),
        Balance numeric(18,2)
    );

    DECLARE @Index int = 1;
    DECLARE @MaxIndex int = (SELECT ISNULL(MAX(RowID), 0) FROM @ClassSections);
    DECLARE @CurrentClassID int;
    DECLARE @CurrentSectionID int;
    DECLARE @CurrentClassName nvarchar(100);
    DECLARE @CurrentSectionName nvarchar(100);

    WHILE (@Index <= @MaxIndex)
    BEGIN
        SELECT
            @CurrentClassID = ClassID,
            @CurrentClassName = ClassName,
            @CurrentSectionID = SectionID,
            @CurrentSectionName = SectionName
        FROM @ClassSections
        WHERE RowID = @Index;

        DECLARE @MonthCursor date = DATEFROMPARTS(YEAR(@SessionStartDate), MONTH(@SessionStartDate), 1);
        DECLARE @LastMonth date = DATEFROMPARTS(YEAR(@SessionEndDate), MONTH(@SessionEndDate), 1);

        WHILE (@MonthCursor <= @LastMonth)
        BEGIN
            DECLARE @Students TABLE
            (
                StudentID int,
                Name nvarchar(100),
                RollNo nvarchar(100),
                Gender int,
                Photo nvarchar(100),
                ClassID int,
                FeePaymentMode int,
                StudentSID nvarchar(15),
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
                SchoolUID nvarchar(50),
                FeeAmount numeric(10,2),
                PreviousDue numeric(10,2),
                LateFee numeric(10,2),
                Discounts numeric(10,2),
                Paid numeric(10,2)
            );

            DELETE FROM @Students;

            INSERT INTO @Students
            EXEC [dbo].[sp_GetClassGroupFeeListOnly]
                @ClassID = @CurrentClassID,
                @SectionID = @CurrentSectionID,
                @SBranchID = @SBranchID,
                @SessionID = @SessionID,
                @QDate = @MonthCursor,
                @CurDate = @CurDate,
                @SStudentID = 0;

            INSERT INTO @Summary
            (
                ClassID,
                ClassName,
                SectionID,
                SectionName,
                FeeMonth,
                FeeYear,
                Amount,
                Discount,
                Paid,
                Balance
            )
            SELECT
                @CurrentClassID,
                @CurrentClassName,
                @CurrentSectionID,
                @CurrentSectionName,
                MONTH(@MonthCursor),
                YEAR(@MonthCursor),
                CAST(SUM(ISNULL(FeeAmount, 0) + ISNULL(PreviousDue, 0) + ISNULL(LateFee, 0)) AS numeric(18,2)),
                CAST(SUM(ISNULL(Discounts, 0)) AS numeric(18,2)),
                CAST(SUM(ISNULL(Paid, 0)) AS numeric(18,2)),
                CAST(SUM(ISNULL(FeeAmount, 0) + ISNULL(PreviousDue, 0) + ISNULL(LateFee, 0) - ISNULL(Discounts, 0) - ISNULL(Paid, 0)) AS numeric(18,2))
            FROM @Students
            WHERE ISNULL(StudentID, 0) <> 0;

            SET @MonthCursor = DATEADD(month, 1, @MonthCursor);
        END;

        SET @Index = @Index + 1;
    END;

    SELECT
        ClassID,
        ClassName,
        SectionID,
        SectionName,
        FeeMonth,
        FeeYear,
        Amount,
        Discount,
        Paid,
        Balance
    FROM @Summary
    ORDER BY ClassID, SectionID, FeeYear, FeeMonth;
END
GO
