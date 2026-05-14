
ALTER PROC [dbo].[sp_GetFeeCollectionReport]
(
    @FromDate date,
    @ToDate date,
    @ReportType int,
    @PaymentMode int,
    @SBranchID int,
    @QuarterID int,
    @SessionID int = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate date = @FromDate;
    DECLARE @EndDate date = @ToDate;
    DECLARE @SessionStartDate date;
    DECLARE @SessionEndDate date;

    IF (@ReportType = 1) -- Daily
    BEGIN
        SET @StartDate = @FromDate;
        SET @EndDate = @FromDate;
    END
    ELSE IF (@ReportType = 2) -- Monthly
    BEGIN
        SET @StartDate = DATEFROMPARTS(YEAR(@FromDate), MONTH(@FromDate), 1);
        SET @EndDate = EOMONTH(@FromDate);
    END
    ELSE IF (@ReportType = 3) -- Between Dates
    BEGIN
        SET @StartDate = @FromDate;
        SET @EndDate = @ToDate;
    END
    ELSE IF (@ReportType = 4) -- Quarterly
    BEGIN
        SELECT
            @SessionStartDate = SessionStartDate,
            @SessionEndDate = SessionEndDate
        FROM SessionMaster
        WHERE SBranchID = @SBranchID
          AND SessionStatus = 1;

        IF (@QuarterID = 1)
        BEGIN
            SET @StartDate = @SessionStartDate;
            SET @EndDate = DATEADD(DAY, -1, DATEADD(MONTH, 3, @SessionStartDate));
        END
        ELSE IF (@QuarterID = 2)
        BEGIN
            SET @StartDate = DATEADD(MONTH, 3, @SessionStartDate);
            SET @EndDate = DATEADD(DAY, -1, DATEADD(MONTH, 6, @SessionStartDate));
        END
        ELSE IF (@QuarterID = 3)
        BEGIN
            SET @StartDate = DATEADD(MONTH, 6, @SessionStartDate);
            SET @EndDate = DATEADD(DAY, -1, DATEADD(MONTH, 9, @SessionStartDate));
        END
        ELSE
        BEGIN
            SET @StartDate = DATEADD(MONTH, 9, @SessionStartDate);
            SET @EndDate = @SessionEndDate;
        END
    END

    SELECT PaymentModeID, PaymentModeName
    FROM PaymentModeMaster;

    ;WITH UsedSessions AS
    (
        SELECT DISTINCT PM.SessionID
        FROM PaymentMaster PM
        WHERE PM.SBranchID = @SBranchID
          AND CAST(PM.PaymentDate AS date) BETWEEN @StartDate AND @EndDate

        UNION

        SELECT DISTINCT RM.SessionID
        FROM RefundMaster RM
        WHERE RM.SBranchID = @SBranchID
          AND CAST(RM.RefundDate AS date) BETWEEN @StartDate AND @EndDate
    )
    SELECT SM.SessionID, SM.SessionName, SM.SessionStatus, SM.SessionStartDate, SM.SessionEndDate
    FROM SessionMaster SM
    LEFT JOIN UsedSessions US ON US.SessionID = SM.SessionID
    WHERE SM.SBranchID = @SBranchID
      AND (SM.SessionStatus = 1 OR US.SessionID IS NOT NULL)
    ORDER BY SM.SessionStartDate DESC;

    SELECT
        PM.PaymentID,
        PM.PaymentAmount,
        0 AS RefundAmount,
        PM.PaymentDate,
        ISNULL(PMM.PaymentModeName, '') AS CollectedBy,
        PM.ReferanceNumber,
        PM.Remark,
        [dbo].[GetPaymentMonthNames](PM.PaymentID) AS PayMonths,
        PM.PaymentMode,
        PM.PaymentRecieptNo,
        'STUD' + RIGHT(REPLICATE('0', 6) + CAST(SS.StudentID AS varchar(6)), 6) AS StudentSID,
        SM.Name,
        SM.SchoolUID,
        SS.RollNo,
        ISNULL(VCS.ClassName, '') + '\' + ISNULL(VCS.SectionName, '') AS ClassSection,
        SMS.SessionName,
        P.FatherName,
        P.FatherMobileNo,
        'Payment' AS RecordType,
        PM.PaymentDate AS SortDate
    FROM PaymentMaster PM
    LEFT JOIN PaymentModeMaster PMM ON PMM.PaymentModeID = PM.PaymentMode
    LEFT JOIN Student_Session SS ON PM.PayeeID = SS.StudentID AND SS.SessionID = PM.SessionID
    LEFT JOIN StudentMaster SM ON SM.StudentID = PM.PayeeID
    LEFT JOIN ParentMaster P ON P.ParentID = SM.ParentID
    LEFT JOIN SessionMaster SMS ON SMS.SessionID = PM.SessionID
    LEFT JOIN [v_ClassSectionNames] VCS ON VCS.ClassID = SS.ClassID AND VCS.SectionID = SS.SectionID
    WHERE CAST(PM.PaymentDate AS date) BETWEEN @StartDate AND @EndDate
      AND PM.SBranchID = @SBranchID
      AND (@PaymentMode = -1 OR PM.PaymentMode = @PaymentMode)
      AND (@SessionID = 0 OR PM.SessionID = @SessionID)

    UNION ALL

    SELECT
        RM.RefundID AS PaymentID,
        0 AS PaymentAmount,
        RM.RefundAmount,
        RM.RefundDate AS PaymentDate,
        CAST(RM.RefundBy AS nvarchar(50)) AS CollectedBy,
        RM.Reason AS ReferanceNumber,
        NULL AS Remark,
        NULL AS PayMonths,
        NULL AS PaymentMode,
        CAST(RM.RefundID AS nvarchar(50)) AS PaymentRecieptNo,
        'STUD' + RIGHT(REPLICATE('0', 6) + CAST(SS.StudentID AS varchar(6)), 6) AS StudentSID,
        SM.Name,
        SM.SchoolUID,
        SS.RollNo,
        ISNULL(VCS.ClassName, '') + '\' + ISNULL(VCS.SectionName, '') AS ClassSection,
        SMS.SessionName,
        P.FatherName,
        P.FatherMobileNo,
        'Refund' AS RecordType,
        RM.RefundDate AS SortDate
    FROM RefundMaster RM
    LEFT JOIN Student_Session SS ON RM.RefundTo = SS.StudentID AND SS.SessionID = RM.SessionID
    LEFT JOIN StudentMaster SM ON SM.StudentID = RM.RefundTo
    LEFT JOIN ParentMaster P ON P.ParentID = SM.ParentID
    LEFT JOIN SessionMaster SMS ON SMS.SessionID = RM.SessionID
    LEFT JOIN [v_ClassSectionNames] VCS ON VCS.ClassID = SS.ClassID AND VCS.SectionID = SS.SectionID
    WHERE CAST(RM.RefundDate AS date) BETWEEN @StartDate AND @EndDate
      AND RM.SBranchID = @SBranchID
      AND (@SessionID = 0 OR RM.SessionID = @SessionID)
      AND (@PaymentMode = -1)

    ORDER BY SortDate DESC, PaymentID DESC;

    SELECT *
    FROM SBranchMaster
    WHERE SBranchID = @SBranchID;
END
GO
