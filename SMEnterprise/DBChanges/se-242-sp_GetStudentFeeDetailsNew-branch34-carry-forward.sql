ALTER PROCEDURE [dbo].[sp_GetStudentFeeDetailsNew]
(
    @StudentID int,
    @SBranchID int,
    @SessionID int,
    @QDate date,
    @CurDate date
)
AS
BEGIN
    DECLARE @ClassID int
    DECLARE @SectionID int
    DECLARE @GroupID int
    DECLARE @TransportFeeMode int
    DECLARE @HostelFeeMode int
    DECLARE @LastPayDay nvarchar(2)
    DECLARE @LateFeeTypeID int

    SELECT @TransportFeeMode = details
    FROM MasterSettings
    WHERE Type = 'TransportFeeMode'
      AND SBranchID = @SBranchID

    SELECT @HostelFeeMode = details
    FROM MasterSettings
    WHERE Type = 'HostelFeeMode'
      AND SBranchID = @SBranchID

    SELECT @LastPayDay = details
    FROM MasterSettings
    WHERE Type = 'FeePaymentReminderDate'
      AND SBranchID = @SBranchID

    SELECT @LateFeeTypeID = FeeTypeID
    FROM FeeTypeMaster
    WHERE FeeTypeApplicable = 7
      AND SBranchID = @SBranchID

    IF (@LateFeeTypeID = '' OR @LateFeeTypeID IS NULL)
    BEGIN
        SET @LateFeeTypeID = -2
    END

    DECLARE @SessionStartDate date, @SessionEndDate date

    DECLARE @CMonth int = DATEPART(month, @QDate), @CYear int = DATEPART(year, @QDate)

    SELECT
        @SessionStartDate = SessionStartDate,
        @SessionEndDate = SessionEndDate
    FROM SessionMaster
    WHERE SessionID = @SessionID

    DECLARE
        @StuSessionSDate date,
        @StuSessionEDate date,
        @QuotaID int,
        @IsAdmissionFee int,
        @FeePaymentMode int,
        @FeeAmount numeric(10,2),
        @PreviousDue numeric(10,2),
        @LateFee numeric(10,2),
        @Discounts numeric(10,2),
        @LMonth int,
        @LYear int,
        @MonthType int,
        @BaseDate date,
        @Paid numeric(10,2),
        @IsCustomFee int,
        @StudentSessionUID int

    DECLARE @CarryForwardFeeTypeID int = 0
    DECLARE @CarryForwardAmount numeric(10,2) = 0
    DECLARE @IsCarryForwardApplicable bit = 0

    SELECT TOP 1
        @ClassID = SS.ClassID,
        @FeePaymentMode = FeePaymentMode,
        @QuotaID = SS.QuotaID,
        @SectionID = SS.SectionID,
        @StuSessionSDate = (CASE WHEN @SessionStartDate < ISNULL(SS.FromDate, @SessionStartDate) THEN SS.FromDate ELSE @SessionStartDate END),
        @StuSessionEDate = (CASE WHEN @SessionEndDate > SS.ToDate THEN SS.ToDate ELSE @SessionEndDate END),
        @IsAdmissionFee = ISNULL(SS.IsAdmissionFeeApplicable, 0),
        @IsCustomFee = ISNULL(SS.IsCustomFee, 0),
        @StudentSessionUID = StudentSessionUID
    FROM Student_Session SS
    WHERE SS.StudentID = @StudentID
      AND SessionID = @SessionID
    ORDER BY (CASE WHEN @QDate BETWEEN FromDate AND ToDate THEN 0 ELSE 1 END) ASC, Status DESC

    SELECT @GroupID = GroupID
    FROM Class_Sections
    WHERE ID = @SectionID

    SELECT @IsCarryForwardApplicable = ISNULL(IsCarryForwardApplicable, 0)
    FROM SBranchMaster
    WHERE SBranchID = @SBranchID

    IF (@IsCarryForwardApplicable = 1)
    BEGIN
        EXEC dbo.sp_GetSessionCarryForwardFeeTypeID
            @SBranchID = @SBranchID,
            @FeeTypeID = @CarryForwardFeeTypeID OUTPUT

        SELECT @CarryForwardAmount = ISNULL(FeeAmount, 0)
        FROM StudentFeeDetails
        WHERE StudentID = @StudentID
          AND SessionID = @StudentSessionUID
          AND FeeTypeID = @CarryForwardFeeTypeID
          AND IsApplicable = 1
    END

    DECLARE @DiscountApproved TABLE (FeeTypeID int, ApprovedAmount decimal(18,2), FeeMonth int, FeeYear int)

    INSERT INTO @DiscountApproved
    SELECT FeeTypeID, ApprovedAmount, FeeMonth, FeeYear
    FROM FeeDiscountRequestDetails FRD
    LEFT JOIN FeeDiscountRequestMaster FRM
        ON FRD.DiscRequestID = FRM.DiscRequestID
    WHERE FRM.Status = 1
      AND ISNULL(ApprovedAmount,0) <> 0
      AND FRM.StudentID = @StudentID
      AND (
            (FRM.FeeYear > YEAR(@SessionStartDate))
            OR (FRM.FeeYear = YEAR(@SessionStartDate) AND FRM.FeeMonth >= MONTH(@SessionStartDate))
          )
      AND (
            (FRM.FeeYear < YEAR(@SessionEndDate))
            OR (FRM.FeeYear = YEAR(@SessionEndDate) AND FRM.FeeMonth <= MONTH(@SessionEndDate))
          )

    DECLARE @QuotaDiscounts TABLE (FeeTypeID int, Discount numeric(5,2))

    DECLARE @MTFT TABLE (MonthTypeID int, FeeTypeID int)
    INSERT INTO @MTFT
    SELECT MonthTypeID, FeeTypeID
    FROM MonthTypeFeeType

    DECLARE @NoFeeMonths TABLE (Month int, FeeTypeID int)
    INSERT INTO @NoFeeMonths
    SELECT Month, FeeTypeID
    FROM SessionClassNoFeeMonths
    WHERE ClassID = @ClassID
      AND SessionID = @SessionID
      AND SBranchID = @SBranchID

    DECLARE @FeeStructureTable TABLE
    (
        FeeTypeID int,
        FeeTypeName nvarchar(50),
        FeeTypeApplicable int,
        FeeAmount numeric(10,2),
        Status int,
        Months nvarchar(50)
    )

    DECLARE @FeeDetailTable TABLE
    (
        FeeMonth int,
        FeeYear int,
        FeeTypeApplicable int,
        FeeTypeID int,
        FeeTypeName nvarchar(100),
        FeeAmount numeric(10,2),
        QDiscount numeric(10,2),
        RDiscount numeric(10,2),
        PayApplicableAmount numeric(10,2),
        CustomFee numeric(10,2),
        PaidAmount numeric(10,2),
        IsCustomFee int,
        IsPayment int
    )

    INSERT INTO @FeeStructureTable
    SELECT
        FTM.FeeTypeID,
        FTM.FeeTypeName,
        FTM.FeeTypeApplicable,
        CFS.FeeAmount,
        ISNULL(CFS.Status,1),
        FTM.Months
    FROM FeeTypeMaster FTM
    LEFT JOIN ClassFeeStructureMaster CFS
        ON CFS.FeeTypeID = FTM.FeeTypeID
       AND ClassID = @ClassID
       AND GroupID = @GroupID
       AND CFS.SessionID = @SessionID
    WHERE FTM.SBranchID = @SBranchID

    INSERT INTO @QuotaDiscounts
    SELECT FeeTypeID, DiscPer
    FROM QuotaDiscountDetails
    WHERE SessionID = @SessionID
      AND QuotaID = @QuotaID

    SET @BaseDate = @StuSessionSDate
    DECLARE @LoopDate date = @QDate

    IF (@QDate > @StuSessionEDate)
    BEGIN
        SET @LoopDate = @StuSessionEDate
    END
    ELSE IF ((DATEDIFF(month, DATEADD(month, -1, @SessionStartDate), @LoopDate)) % @FeePaymentMode > 0)
    BEGIN
        DECLARE @adjustMonth int = @FeePaymentMode - (DATEDIFF(month, DATEADD(month, -1, @SessionStartDate), @LoopDate)) % @FeePaymentMode
        SET @LoopDate = DATEADD(month, @adjustMonth, @LoopDate)
    END

    WHILE (DATEPART(year,@StuSessionSDate) * 12 + DATEPART(month,@StuSessionSDate) <= DATEPART(year,@LoopDate) * 12 + DATEPART(month,@LoopDate))
    BEGIN
        SET @LMonth = DATEPART(month,@StuSessionSDate)
        SET @LYear = DATEPART(year,@StuSessionSDate)

        DECLARE @PayDate date
        SELECT @PayDate = MIN(PaymentDate)
        FROM PaymentDetails
        WHERE PayeeID = @StudentID
          AND Month = @LMonth
          AND Year = @LYear
          AND ISNULL(PaymentStatus,0) = 0
          AND ISNULL(PaymentRecieved,0) > 0

        DECLARE @IsLatePay int = 0
        DECLARE @MonthLatePaymentDate date
        SET @MonthLatePaymentDate = CAST(CAST(@LYear AS nvarchar(5)) + '-' + CAST(@LMonth AS nvarchar(2)) + '-' + @LastPayDay AS date)

        IF (@PayDate IS NOT NULL)
        BEGIN
            SELECT @IsLatePay = COUNT(*)
            FROM PaymentDetails
            WHERE PayeeID = @StudentID
              AND Month = @LMonth
              AND Year = @LYear
              AND FeeTypeID = @LateFeeTypeID
        END
        ELSE IF (@PayDate IS NULL AND @CurDate > CAST(CAST(@LYear AS nvarchar(5)) + '-' + CAST(@LMonth AS nvarchar(2)) + '-' + @LastPayDay AS date))
        BEGIN
            SET @IsLatePay = 1
        END
        ELSE
        BEGIN
            SET @IsLatePay = 0
        END

        SELECT @MonthType =
        (
            CASE
                WHEN DATEPART(month,@BaseDate) = @LMonth
                     AND (DATEPART(month,DATEADD(month,3,@SessionStartDate)) = @LMonth OR DATEPART(month,DATEADD(month,9,@SessionStartDate)) = @LMonth) THEN 5
                ELSE
                    CASE
                        WHEN DATEPART(month,@BaseDate) = @LMonth
                             AND DATEPART(month,DATEADD(month,6,@SessionStartDate)) = @LMonth THEN 6
                        ELSE
                            CASE
                                WHEN DATEPART(month,@BaseDate) = @LMonth THEN (CASE WHEN @IsAdmissionFee = 0 THEN 1 ELSE 0 END)
                                ELSE
                                    CASE
                                        WHEN DATEPART(month,DATEADD(month,3,@SessionStartDate)) = @LMonth OR DATEPART(month,DATEADD(month,9,@SessionStartDate)) = @LMonth THEN 3
                                        ELSE (CASE WHEN DATEPART(month,DATEADD(month,6,@SessionStartDate)) = @LMonth THEN 4 ELSE 2 END)
                                    END
                            END
                    END
            END
        )

        INSERT INTO @FeeDetailTable
        SELECT
            @LMonth AS FeeMonth,
            @LYear AS FeeYear,
            FTS.FeeTypeApplicable,
            FTS.FeeTypeID,
            FTS.FeeTypeName,
            CASE
                WHEN FTS.FeeTypeApplicable = 5
                    THEN dbo.fn_GetStudentTransportFeeAmount(@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID)
                ELSE FTS.FeeAmount
            END AS FeeAmount,
            QD.Discount AS QuotaDiscount,
            CASE
                WHEN FTS.FeeTypeApplicable = 7 THEN ISNULL(PD.DiscAmt,0)
                WHEN PD.PaymentID IS NOT NULL AND ISNULL(PD.DiscAmt,0) = 0 AND DA.ApprovedAmount != 0 THEN DA.ApprovedAmount
                WHEN PD.PaymentID IS NOT NULL AND ISNULL(PD.DiscAmt,0) != 0 THEN ISNULL(PD.DiscAmt,0)
                ELSE DA.ApprovedAmount
            END,
            PD.NetApplicablePayment,
            SFD.FeeAmount,
            PD.PaymentRecieved,
            CASE WHEN @IsCustomFee IS NULL THEN 0 ELSE @IsCustomFee END,
            CASE WHEN PD.PaymentID IS NULL THEN 0 ELSE 1 END
        FROM @FeeStructureTable FTS
        LEFT JOIN @QuotaDiscounts QD
            ON QD.FeeTypeID = FTS.FeeTypeID
        LEFT JOIN StudentFeeDetails SFD
            ON SFD.StudentID = @StudentID
           AND SFD.SessionID = @StudentSessionUID
           AND SFD.FeeTypeID = FTS.FeeTypeID
           AND SFD.IsApplicable = 1
        LEFT JOIN v_PaymentDetails PD
            ON PD.FeeTypeID = FTS.FeeTypeID
           AND PD.PayeeID = @StudentID
           AND PD.Month = @LMonth
           AND PD.Year = @LYear
        LEFT JOIN @DiscountApproved DA
            ON DA.FeeTypeID = FTS.FeeTypeID
           AND DA.FeeMonth = @LMonth
           AND DA.FeeYear = @LYear
        WHERE FTS.FeeTypeApplicable IN (SELECT FeeTypeID FROM @MTFT WHERE MonthTypeID = @MonthType)
          AND FTS.FeeTypeID <> (CASE WHEN @IsLatePay = 0 THEN @LateFeeTypeID ELSE 0 END)
          AND FTS.FeeTypeApplicable <> (CASE WHEN @IsAdmissionFee = 0 THEN 8 ELSE 0 END)
          AND ISNULL(SFD.IsApplicable, FTS.Status) = 1
          AND (FTS.FeeTypeApplicable <> 9 OR (SELECT COUNT(*) FROM dbo.SplitStringToTable(FTS.Months,',') WHERE Item = @LMonth) > 0)
          AND (FTS.FeeTypeID NOT IN (SELECT FeeTypeID FROM @NoFeeMonths WHERE Month = @LMonth) OR (SELECT COUNT(*) FROM @NoFeeMonths) = 0)
          AND (@IsCarryForwardApplicable = 0 OR FTS.FeeTypeID <> ISNULL(@CarryForwardFeeTypeID,-1))

        IF (@PayDate IS NULL)
        BEGIN
            SET @StuSessionSDate = DATEADD(month,1,@StuSessionSDate)
        END
        ELSE
        BEGIN
            SET @StuSessionSDate = DATEADD(month,@FeePaymentMode,@StuSessionSDate)
        END
    END

    -- Carry-forward change: inject only one carry-forward row
    IF (@IsCarryForwardApplicable = 1 AND ISNULL(@CarryForwardAmount,0) > 0 AND ISNULL(@CarryForwardFeeTypeID,0) > 0)
    BEGIN
        INSERT INTO @FeeDetailTable
        (
            FeeMonth,
            FeeYear,
            FeeTypeApplicable,
            FeeTypeID,
            FeeTypeName,
            FeeAmount,
            QDiscount,
            RDiscount,
            PayApplicableAmount,
            CustomFee,
            PaidAmount,
            IsCustomFee,
            IsPayment
        )
        SELECT
            MONTH(@SessionStartDate),
            YEAR(@SessionStartDate),
            1,
            FTM.FeeTypeID,
            FTM.FeeTypeName,
            @CarryForwardAmount,
            0,
            0,
            ISNULL(PD.NetApplicablePayment, @CarryForwardAmount),
            0,
            ISNULL(PD.PaymentRecieved, 0),
            0,
            CASE WHEN PD.PaymentID IS NULL THEN 0 ELSE 1 END
        FROM FeeTypeMaster FTM
        OUTER APPLY
        (
            SELECT
                MIN(PaymentID) AS PaymentID,
                SUM(ISNULL(NetApplicablePayment,0)) AS NetApplicablePayment,
                SUM(ISNULL(PaymentRecieved,0)) AS PaymentRecieved
            FROM v_PaymentDetails
            WHERE PayeeID = @StudentID
              AND FeeTypeID = @CarryForwardFeeTypeID
              AND Month = MONTH(@SessionStartDate)
              AND Year = YEAR(@SessionStartDate)
        ) PD
        WHERE FTM.FeeTypeID = @CarryForwardFeeTypeID
    END

    SELECT
        FeeMonth,
        FeeYear,
        FeeTypeApplicable,
        FeeTypeID,
        FeeTypeName,
        SUM(FeeAmount) AS FeeAmount,
        AVG(QDiscount) AS QDiscount,
        SUM(RDiscount) AS RDiscount,
        SUM(PayApplicableAmount) AS PayApplicableAmount,
        SUM(CASE WHEN FeeTypeApplicable IN (5,6,7) THEN FeeAmount ELSE CustomFee END) AS CustomFee,
        SUM(PaidAmount) AS PaidAmount,
        MAX(IsCustomFee) AS IsCustomFee,
        MAX(IsPayment) AS IsPayment
    FROM
    (
        SELECT
            DATEPART(month,DATEADD(month,-Diff,CAST(CAST(FeeYear AS nvarchar(5)) + '-' + CAST(FeeMonth AS nvarchar(2)) + '-1' AS date))) AS FeeMonth,
            DATEPART(year,DATEADD(month,-Diff,CAST(CAST(FeeYear AS nvarchar(5)) + '-' + CAST(FeeMonth AS nvarchar(2)) + '-1' AS date))) AS FeeYear,
            FeeTypeApplicable,
            FeeTypeID,
            FeeTypeName,
            FeeAmount,
            QDiscount,
            RDiscount,
            PayApplicableAmount,
            CustomFee,
            PaidAmount,
            IsCustomFee,
            IsPayment
        FROM
        (
            SELECT *,
                   (DATEDIFF(month,@SessionStartDate,CAST(CAST(FeeYear AS nvarchar(5)) + '-' + CAST(FeeMonth AS nvarchar(2)) + '-1' AS date)) % @FeePaymentMode) AS Diff
            FROM @FeeDetailTable
        ) t
    ) t2
    GROUP BY FeeMonth, FeeYear, FeeTypeApplicable, FeeTypeID, FeeTypeName
    ORDER BY FeeYear, FeeMonth, CASE WHEN FeeTypeApplicable = 7 THEN 0 ELSE FeeTypeID END, FeeTypeName
END
GO
