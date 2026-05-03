ALTER PROCEDURE [dbo].[sp_GetClassGroupFeeListOnly]
(
    @ClassID int,
    @SectionID int,
    @SBranchID int,
    @SessionID int,
    @QDate date,
    @CurDate date,
    @SStudentID int = 0
)
AS
BEGIN
    DECLARE @ActiveSessionID int
    SELECT @ActiveSessionID = SessionID
    FROM SessionMaster
    WHERE SBranchID = @SBranchID
      AND SessionStatus = 1

    IF (@SStudentID <> 0)
    BEGIN
        SELECT
            @ClassID = ClassID,
            @SectionID = SectionID
        FROM Student_Session
        WHERE SessionID = @SessionID
          AND StudentID = @SStudentID
    END

    DECLARE @GroupID int
    DECLARE @TransportFeeMode int
    DECLARE @HostelFeeMode int
    DECLARE @LastPayDay nvarchar(2)
    DECLARE @LateFeeTypeID int
    DECLARE @IsTransport int

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

    DECLARE @TransHostFeeTable TABLE
    (
        FeeTypeID int,
        MonthType int,
        NetApplicablePayment numeric(10,2),
        DiscAmt numeric(10,2),
        PaymentRecieved numeric(10,2),
        PaymentDate date,
        FeeTypeApplicable int
    )

    SELECT @IsTransport = StopID
    FROM StudentMaster
    WHERE StudentID = @SStudentID

    DECLARE @TFeeTypeID int
    SELECT @TFeeTypeID = FeeTypeID
    FROM FeeTypeMaster
    WHERE FeeTypeApplicable = 5
      AND SBranchID = @SBranchID

    DECLARE @TransportDiscount numeric(10,2) = 0

    DECLARE @SessionStartDate date, @SessionEndDate date
    SELECT @GroupID = GroupID
    FROM Class_Sections
    WHERE ID = @SectionID

    SELECT
        @SessionStartDate = SessionStartDate,
        @SessionEndDate = SessionEndDate
    FROM SessionMaster
    WHERE SessionID = @SessionID

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
        Paid numeric(10,2),
        IsCustomFee int,
        SequenceNo int
    )

    IF (@SStudentID = 0)
    BEGIN
        IF (@ActiveSessionID = @SessionID)
        BEGIN
            INSERT INTO @Students
            SELECT
                SM.StudentID, SM.Name, SS.RollNo, SM.Gender, SM.Photo, SS.ClassID, SS.FeePaymentMode,
                'STUD' + RIGHT(REPLICATE('0',6) + CAST(SS.StudentID AS varchar(6)), 6),
                SS.FromDate, SS.ToDate, SS.QuotaID, SS.SessionID, SM.VehicleRouteID, SM.HostelRoomID, SS.SectionID,
                CASE WHEN @SessionStartDate < SS.FromDate THEN SS.FromDate ELSE @SessionStartDate END,
                CASE WHEN @SessionEndDate > SS.ToDate THEN SS.ToDate ELSE @SessionEndDate END,
                ISNULL(SS.IsAdmissionFeeApplicable,0), SM.SchoolUID, 0,0,0,0,0,SS.IsCustomFee,
                ROW_NUMBER() OVER (ORDER BY SM.StudentID DESC)
            FROM Student_Session SS
            LEFT JOIN StudentMaster SM ON SM.StudentID = SS.StudentID
            WHERE SS.ClassID = @ClassID
              AND SS.SessionID = @SessionID
              AND SS.SectionID = @SectionID
              AND SS.Status = 1
        END
        ELSE
        BEGIN
            INSERT INTO @Students
            SELECT
                SM.StudentID, SM.Name, SS.RollNo, SM.Gender, SM.Photo, SS.ClassID, SS.FeePaymentMode,
                'STUD' + RIGHT(REPLICATE('0',6) + CAST(SS.StudentID AS varchar(6)), 6),
                SS.FromDate, SS.ToDate, SS.QuotaID, SS.SessionID, SM.VehicleRouteID, SM.HostelRoomID, SS.SectionID,
                CASE WHEN @SessionStartDate < SS.FromDate THEN SS.FromDate ELSE @SessionStartDate END,
                CASE WHEN @SessionEndDate > SS.ToDate THEN SS.ToDate ELSE @SessionEndDate END,
                ISNULL(SS.IsAdmissionFeeApplicable,0), SM.SchoolUID, 0,0,0,0,0,SS.IsCustomFee,
                ROW_NUMBER() OVER (ORDER BY SM.StudentID DESC)
            FROM Student_Session SS
            LEFT JOIN StudentMaster SM ON SM.StudentID = SS.StudentID
            WHERE SS.ClassID = @ClassID
              AND SS.SessionID = @SessionID
              AND SS.SectionID = @SectionID
        END
    END
    ELSE
    BEGIN
        IF (@ActiveSessionID = @SessionID)
        BEGIN
            INSERT INTO @Students
            SELECT
                SM.StudentID, SM.Name, SS.RollNo, SM.Gender, SM.Photo, SS.ClassID, SS.FeePaymentMode,
                'STUD' + RIGHT(REPLICATE('0',6) + CAST(SS.StudentID AS varchar(6)), 6),
                SS.FromDate, SS.ToDate, SS.QuotaID, SS.SessionID, SM.VehicleRouteID, SM.HostelRoomID, SS.SectionID,
                CASE WHEN @SessionStartDate < SS.FromDate THEN SS.FromDate ELSE @SessionStartDate END,
                CASE WHEN @SessionEndDate > SS.ToDate THEN SS.ToDate ELSE @SessionEndDate END,
                ISNULL(SS.IsAdmissionFeeApplicable,0), SM.SchoolUID, 0,0,0,0,0,SS.IsCustomFee,
                ROW_NUMBER() OVER (ORDER BY SM.StudentID DESC)
            FROM Student_Session SS
            LEFT JOIN StudentMaster SM ON SM.StudentID = SS.StudentID
            WHERE SS.ClassID = @ClassID
              AND SS.SessionID = @SessionID
              AND SS.SectionID = @SectionID
              AND SS.StudentID = @SStudentID
              AND SS.Status = 1
        END
        ELSE
        BEGIN
            INSERT INTO @Students
            SELECT
                SM.StudentID, SM.Name, SS.RollNo, SM.Gender, SM.Photo, SS.ClassID, SS.FeePaymentMode,
                'STUD' + RIGHT(REPLICATE('0',6) + CAST(SS.StudentID AS varchar(6)), 6),
                SS.FromDate, SS.ToDate, SS.QuotaID, SS.SessionID, SM.VehicleRouteID, SM.HostelRoomID, SS.SectionID,
                CASE WHEN @SessionStartDate < SS.FromDate THEN SS.FromDate ELSE @SessionStartDate END,
                CASE WHEN @SessionEndDate > SS.ToDate THEN SS.ToDate ELSE @SessionEndDate END,
                ISNULL(SS.IsAdmissionFeeApplicable,0), SM.SchoolUID, 0,0,0,0,0,SS.IsCustomFee,
                ROW_NUMBER() OVER (ORDER BY SM.StudentID DESC)
            FROM Student_Session SS
            LEFT JOIN StudentMaster SM ON SM.StudentID = SS.StudentID
            WHERE SS.ClassID = @ClassID
              AND SS.SessionID = @SessionID
              AND SS.SectionID = @SectionID
              AND SS.StudentID = @SStudentID
        END
    END

    DECLARE @DiscountApproved TABLE
    (
        StudentID int,
        FeeTypeID int,
        ApprovedAmount decimal(18,2),
        FeeMonth int,
        FeeYear int
    )

    INSERT INTO @DiscountApproved
    SELECT StudentID, FeeTypeID, ApprovedAmount, FeeMonth, FeeYear
    FROM FeeDiscountRequestDetails FRD
    LEFT JOIN FeeDiscountRequestMaster FRM
        ON FRD.DiscRequestID = FRM.DiscRequestID
    WHERE FRM.Status = 1
      AND ISNULL(ApprovedAmount,0) <> 0
      AND StudentID IN (SELECT StudentID FROM @Students)
      AND Status = 1
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
       AND CFS.ClassID = @ClassID
       AND CFS.GroupID = @GroupID
       AND CFS.SessionID = @SessionID
    WHERE FTM.SBranchID = @SBranchID

    DECLARE
        @StuSessionSDate date,
        @StuSessionEDate date,
        @StudentID int,
        @QuotaID int,
        @IsAdmissionFee int,
        @FeeAmount numeric(10,2),
        @PreviousDue numeric(10,2),
        @LateFee numeric(10,2),
        @Discounts numeric(10,2),
        @LMonth int,
        @LYear int,
        @MonthType int,
        @BaseDate date,
        @Paid numeric(10,2),
        @FeePaymentMode int,
        @IsCustomFee int

    DECLARE @isPaid int
    DECLARE @CurrentSequence int = 1
    DECLARE @MaxSequence int
    DECLARE @pDiscount numeric(10,2)

    SELECT @MaxSequence = MAX(SequenceNo) FROM @Students

    WHILE (@CurrentSequence <= @MaxSequence)
    BEGIN
        SELECT
            @StuSessionSDate = SessionStartDate,
            @StuSessionEDate = SessionEndDate,
            @StudentID = StudentID,
            @QuotaID = QuotaID,
            @IsAdmissionFee = IsAdmissionFee,
            @FeeAmount = FeeAmount,
            @PreviousDue = PreviousDue,
            @LateFee = LateFee,
            @Discounts = Discounts,
            @Paid = Paid,
            @FeePaymentMode = FeePaymentMode
        FROM @Students
        WHERE SequenceNo = @CurrentSequence

        DECLARE @StudentSessionUID int
        SET @Discounts = 0
        SET @pDiscount = 0

        SELECT TOP 1
            @StudentSessionUID = StudentSessionUID,
            @IsCustomFee = ISNULL(IsCustomFee,0)
        FROM Student_Session
        WHERE StudentID = @StudentID
          AND SessionID = @SessionID
        ORDER BY Status DESC

        DECLARE @CarryForwardFeeTypeID int = 0
        DECLARE @CarryForwardAmount numeric(10,2) = 0
        DECLARE @CarryForwardPaid numeric(10,2) = 0
        DECLARE @IsCarryForwardApplicable bit = 0

        SELECT @IsCarryForwardApplicable = ISNULL(IsCarryForwardApplicable, 0)
        FROM SBranchMaster
        WHERE SBranchID = @SBranchID

        IF (@IsCarryForwardApplicable = 1)
        BEGIN
            EXEC dbo.sp_GetSessionCarryForwardFeeTypeID
                @SBranchID = @SBranchID,
                @FeeTypeID = @CarryForwardFeeTypeID OUTPUT

            SELECT @CarryForwardAmount = ISNULL(FeeAmount,0)
            FROM StudentFeeDetails
            WHERE StudentID = @StudentID
              AND SessionID = @StudentSessionUID
              AND FeeTypeID = @CarryForwardFeeTypeID
              AND IsApplicable = 1
        END

        DELETE FROM @QuotaDiscounts

        INSERT INTO @QuotaDiscounts
        SELECT FeeTypeID, DiscPer
        FROM QuotaDiscountDetails
        WHERE SBranchID = @SBranchID
          AND SessionID = @SessionID
          AND QuotaID = @QuotaID

        SET @BaseDate = @StuSessionSDate
        DECLARE @LoopDate date = @QDate
        DECLARE @CMonth int = DATEPART(month,@QDate), @CYear int = DATEPART(year,@QDate)

        IF (@QDate > @StuSessionEDate)
        BEGIN
            SET @LoopDate = @StuSessionEDate
        END
        ELSE IF ((DATEDIFF(month,DATEADD(month,-1,@SessionStartDate),@LoopDate)) % @FeePaymentMode > 0)
        BEGIN
            DECLARE @remainder int = (DATEDIFF(month,DATEADD(month,-1,@SessionStartDate),@LoopDate)) % @FeePaymentMode
            DECLARE @adjustMonth int = @FeePaymentMode - @remainder
            SET @LoopDate = DATEADD(month,@adjustMonth,@LoopDate)
            SET @CMonth = DATEPART(month,DATEADD(month,1-@remainder,@QDate))
            SET @CYear = DATEPART(year,DATEADD(month,1-@remainder,@QDate))
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
            ELSE IF (@PayDate IS NULL AND @CurDate > @MonthLatePaymentDate)
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

            DECLARE @TPaid numeric(10,2) = 0
            DECLARE @CLateFee numeric(10,2) = 0
            DECLARE @CPaid numeric(10,2) = 0

            IF (@CMonth + @CYear * 12 > @LMonth + @LYear * 12)
            BEGIN
                DECLARE @PPD numeric(10,2) = 0
                DECLARE @PPR numeric(10,2) = 0

                SELECT
                    @PPD = SUM(
                        CASE
                            WHEN ISNULL(PD.PaymentID,0) != 0 THEN PD.NetApplicablePayment - ISNULL(PD.DiscAmt,0)
                            WHEN ISNULL(@IsCustomFee,0) = 1 THEN SFD.FeeAmount
                            ELSE (NULLIF(FTS.FeeAmount,0) - ISNULL(FTS.FeeAmount * NULLIF(QD.Discount,0) / 100,0))
                        END
                    ),
                    @PPR = ISNULL(SUM(PD.PaymentRecieved),0)
                FROM @FeeStructureTable FTS
                LEFT JOIN @QuotaDiscounts QD ON QD.FeeTypeID = FTS.FeeTypeID
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
                WHERE FTS.FeeTypeApplicable IN (SELECT FeeTypeID FROM @MTFT WHERE MonthTypeID = @MonthType)
                  AND FTS.FeeTypeApplicable != 5
                  AND FTS.FeeTypeApplicable <> 7
                  AND FTS.FeeTypeApplicable <> (CASE WHEN @IsAdmissionFee = 0 THEN 8 ELSE 0 END)
                  AND ISNULL(SFD.IsApplicable,FTS.Status) = 1
                  AND (FTS.FeeTypeApplicable <> 9 OR (SELECT COUNT(*) FROM dbo.SplitStringToTable(FTS.Months,',') WHERE Item = @LMonth) > 0)
                  AND (FTS.FeeTypeID NOT IN (SELECT ISNULL(NFM.FeeTypeID,FTS.FeeTypeID) FROM @NoFeeMonths NFM WHERE NFM.Month = @LMonth))
                  AND (@IsCarryForwardApplicable = 0 OR FTS.FeeTypeID <> ISNULL(@CarryForwardFeeTypeID,-1))

                DECLARE @mDiscount numeric(10,2), @mPaid numeric(10,2), @mAmount numeric(10,2), @IsPaymentDone int

                SELECT @IsPaymentDone = COUNT(*)
                FROM v_PaymentDetails PD
                WHERE PD.PayeeID = @StudentID
                  AND PD.Month = @LMonth
                  AND PD.Year = @LYear

                SELECT
                    @mDiscount = SUM(ISNULL(DiscAmt,0)),
                    @mAmount = SUM(NetApplicablePayment),
                    @mPaid = SUM(PaymentRecieved)
                FROM v_PaymentDetails PD
                WHERE PD.PayeeID = @StudentID
                  AND PD.Month = @LMonth
                  AND PD.Year = @LYear
                  AND PD.FeeTypeID != @LateFeeTypeID

                SELECT @TransportDiscount = ISNULL(ApprovedAmount,0)
                FROM FeeDiscountRequestDetails FDD
                INNER JOIN FeeDiscountRequestMaster FDM
                    ON FDM.DiscRequestID = FDD.DiscRequestID
                WHERE StudentID = @StudentID
                  AND Status = 1
                  AND FeeTypeID = @TFeeTypeID
                  AND FeeMonth = @LMonth
                  AND FeeYear = @LYear

                SELECT @pDiscount = ISNULL(SUM(
                    CASE
                        WHEN ISNULL(PD.DiscAmt,0) >= DA.ApprovedAmount THEN 0
                        ELSE DA.ApprovedAmount - ISNULL(PD.DiscAmt,0)
                    END
                ),0)
                FROM @DiscountApproved DA
                LEFT JOIN v_PaymentDetails PD
                    ON PD.FeeTypeID = DA.FeeTypeID
                   AND PD.PayeeID = @StudentID
                   AND PD.Month = @LMonth
                   AND PD.Year = @LYear
                WHERE DA.StudentID = @StudentID
                  AND DA.FeeMonth = @LMonth
                  AND DA.FeeYear = @LYear

                IF ((SELECT COUNT(*) FROM @FeeStructureTable FTS
                     WHERE FeeTypeApplicable = 5
                       AND (FeeTypeID NOT IN (SELECT ISNULL(NFM.FeeTypeID,FTS.FeeTypeID) FROM @NoFeeMonths NFM WHERE NFM.Month = @LMonth)
                            OR (SELECT COUNT(*) FROM @NoFeeMonths) = 0)) > 0)
                BEGIN
                    SELECT @PreviousDue = ISNULL(@PreviousDue,0) + ISNULL(dbo.fn_GetStudentTransportFeeAmount(@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID),0)
                END

                IF ((SELECT COUNT(*) FROM @FeeStructureTable
                     WHERE FeeTypeApplicable = 6
                       AND (FeeTypeID NOT IN (SELECT ISNULL(NFM.FeeTypeID,FeeTypeID) FROM @NoFeeMonths NFM WHERE NFM.Month = @LMonth)
                            OR (SELECT COUNT(*) FROM @NoFeeMonths) = 0)) > 0)
                BEGIN
                    SELECT @PreviousDue = ISNULL(@PreviousDue,0) + ISNULL(dbo.fn_GetStudentHostalFeeAmount(@LMonth,@LYear,@StudentID,@HostelFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID),0)
                END

                SELECT @PreviousDue = ISNULL(@PreviousDue,0) - ISNULL(SUM(ISNULL(PD.PaymentRecieved,0) + ISNULL(PD.DiscAmt,0)),0)
                FROM @FeeStructureTable FTS
                LEFT JOIN v_PaymentDetails PD
                    ON PD.FeeTypeID = FTS.FeeTypeID
                   AND PD.PayeeID = @StudentID
                   AND PD.Month = @LMonth
                   AND PD.Year = @LYear
                WHERE FTS.FeeTypeApplicable IN (5,6)
                  AND (FTS.FeeTypeID NOT IN (SELECT ISNULL(NFM.FeeTypeID,FTS.FeeTypeID) FROM @NoFeeMonths NFM WHERE NFM.Month = @LMonth))

                SELECT
                    @CLateFee = SUM(CASE WHEN ISNULL(PD.PaymentID,0) != 0 THEN PD.NetApplicablePayment - ISNULL(PD.DiscAmt,0) ELSE FTS.FeeAmount END),
                    @CPaid = SUM(ISNULL(PD.PaymentRecieved,0))
                FROM @FeeStructureTable FTS
                LEFT JOIN v_PaymentDetails PD
                    ON PD.FeeTypeID = FTS.FeeTypeID
                   AND PD.PayeeID = @StudentID
                   AND PD.Month = @LMonth
                   AND PD.Year = @LYear
                WHERE FTS.FeeTypeApplicable = (CASE WHEN @IsLatePay = 0 THEN 0 ELSE 7 END)
                  AND ISNULL(FTS.Status,0) = 1
                  AND (FTS.FeeTypeID NOT IN (SELECT ISNULL(NFM.FeeTypeID,FTS.FeeTypeID) FROM @NoFeeMonths NFM WHERE NFM.Month = @LMonth))

                SET @PreviousDue = (ISNULL(@PreviousDue,0) - ISNULL(@pDiscount,0))
                                 + (ISNULL(@CLateFee,0) - ISNULL(@CPaid,0))
                                 + (ISNULL(@PPD,0) - ISNULL(@PPR,0))
            END
            ELSE
            BEGIN
                DECLARE @pPaid numeric(10,2)

                SELECT
                    @FeeAmount = ISNULL(@FeeAmount,0) + SUM(
                        CASE
                            WHEN ISNULL(PD.PaymentID,0) != 0 THEN PD.NetApplicablePayment - ISNULL(PD.DiscAmt,0)
                            WHEN ISNULL(@IsCustomFee,0) = 1 AND PD.Amount IS NOT NULL THEN PD.Amount
                            WHEN ISNULL(@IsCustomFee,0) = 1 AND SFD.FeeAmount IS NOT NULL THEN SFD.FeeAmount
                            ELSE (NULLIF(FTS.FeeAmount,0) - ISNULL(FTS.FeeAmount * NULLIF(QD.Discount,0) / 100,0))
                        END
                    ),
                    @pPaid = SUM(ISNULL(PD.PaymentRecieved,0))
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
                WHERE FTS.FeeTypeApplicable IN (SELECT FeeTypeID FROM @MTFT WHERE MonthTypeID = @MonthType)
                  AND FTS.FeeTypeApplicable != 5
                  AND FTS.FeeTypeApplicable <> 7
                  AND FTS.FeeTypeApplicable <> (CASE WHEN @IsAdmissionFee = 0 THEN 8 ELSE 0 END)
                  AND ISNULL(SFD.IsApplicable,FTS.Status) = 1
                  AND (FTS.FeeTypeApplicable <> 9 OR (SELECT COUNT(*) FROM dbo.SplitStringToTable(FTS.Months,',') WHERE Item = @LMonth) > 0)
                  AND (FTS.FeeTypeID NOT IN (SELECT ISNULL(NFM.FeeTypeID,FTS.FeeTypeID) FROM @NoFeeMonths NFM WHERE NFM.Month = @LMonth) OR (SELECT COUNT(*) FROM @NoFeeMonths) = 0)
                  AND (@IsCarryForwardApplicable = 0 OR FTS.FeeTypeID <> ISNULL(@CarryForwardFeeTypeID,-1))

                SELECT @isPaid = COUNT(*)
                FROM v_PaymentDetails PD
                WHERE PD.PayeeID = @StudentID
                  AND PD.Month = @LMonth
                  AND PD.Year = @LYear

                IF (ISNULL(@isPaid,0) = 0)
                BEGIN
                    SELECT @Discounts = ISNULL(@Discounts,0) + ISNULL(SUM(ApprovedAmount),0)
                    FROM @DiscountApproved
                    WHERE StudentID = @StudentID
                      AND FeeMonth = @LMonth
                      AND FeeYear = @LYear
                END

                DECLARE @DiscountPT numeric(10,2)
                DECLARE @DiscountApr numeric(10,2)

                IF (ISNULL(@isPaid,0) != 0)
                BEGIN
                    SELECT @DiscountPT = ISNULL(SUM(PD.DiscAmt),0)
                    FROM v_PaymentDetails PD
                    WHERE PD.PayeeID = @StudentID
                      AND PD.Month = @LMonth
                      AND PD.Year = @LYear

                    SELECT @DiscountApr = ISNULL(SUM(ApprovedAmount),0)
                    FROM @DiscountApproved
                    WHERE StudentID = @StudentID
                      AND FeeMonth = @LMonth
                      AND FeeYear = @LYear

                    IF (@DiscountPT < @DiscountApr)
                    BEGIN
                        SELECT @Discounts = SUM(ISNULL(ApprovedAmount,0))
                        FROM @DiscountApproved
                        WHERE StudentID = @StudentID
                          AND FeeMonth = @LMonth
                          AND FeeYear = @LYear
                    END
                END

                IF (
                    SELECT COUNT(*)
                    FROM @FeeStructureTable FS
                    LEFT JOIN @NoFeeMonths NFM
                        ON FS.FeeTypeID = NFM.FeeTypeID
                       AND NFM.Month = @LMonth
                    WHERE FS.FeeTypeApplicable = 5
                      AND NFM.FeeTypeID IS NULL
                ) > 0
                BEGIN
                    SELECT @FeeAmount = ISNULL(@FeeAmount,0) + ISNULL(dbo.fn_GetStudentTransportFeeAmount(@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID),0)
                END

                IF ((SELECT COUNT(*) FROM @FeeStructureTable
                     WHERE FeeTypeApplicable = 6
                       AND (FeeTypeID NOT IN (SELECT ISNULL(NFM.FeeTypeID,FeeTypeID) FROM @NoFeeMonths NFM WHERE NFM.Month = @LMonth)
                            OR (SELECT COUNT(*) FROM @NoFeeMonths) = 0)) > 0)
                BEGIN
                    SELECT @FeeAmount = ISNULL(@FeeAmount,0) + dbo.fn_GetStudentHostalFeeAmount(@LMonth,@LYear,@StudentID,@HostelFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID)
                END

                SELECT @TPaid = SUM(ISNULL(PD.PaymentRecieved,0))
                FROM @FeeStructureTable FTS
                LEFT JOIN v_PaymentDetails PD
                    ON PD.FeeTypeID = FTS.FeeTypeID
                   AND PD.PayeeID = @StudentID
                   AND PD.Month = @LMonth
                   AND PD.Year = @LYear
                WHERE FTS.FeeTypeApplicable IN (5,6)
                  AND (FTS.FeeTypeID NOT IN (SELECT ISNULL(NFM.FeeTypeID,FTS.FeeTypeID) FROM @NoFeeMonths NFM WHERE NFM.Month = @LMonth))

                SELECT
                    @CLateFee = SUM(CASE WHEN ISNULL(PD.PaymentID,0) != 0 THEN PD.NetApplicablePayment ELSE FTS.FeeAmount END),
                    @CPaid = SUM(ISNULL(PD.PaymentRecieved,0))
                FROM @FeeStructureTable FTS
                LEFT JOIN v_PaymentDetails PD
                    ON PD.FeeTypeID = FTS.FeeTypeID
                   AND PD.PayeeID = @StudentID
                   AND PD.Month = @LMonth
                   AND PD.Year = @LYear
                WHERE FTS.FeeTypeApplicable = (CASE WHEN @IsLatePay = 0 THEN 0 ELSE 7 END)
                  AND ISNULL(FTS.Status,0) = 1
                  AND (FTS.FeeTypeID NOT IN (SELECT ISNULL(NFM.FeeTypeID,FTS.FeeTypeID) FROM @NoFeeMonths NFM WHERE NFM.Month = @LMonth))

                SET @LateFee = ISNULL(@LateFee,0) + ISNULL(@CLateFee,0)
                SET @Paid = ISNULL(@Paid,0) + ISNULL(@CPaid,0) + ISNULL(@TPaid,0) + ISNULL(@pPaid,0)
            END

            IF (@PayDate IS NULL)
            BEGIN
                SET @StuSessionSDate = DATEADD(month,1,@StuSessionSDate)
            END
            ELSE
            BEGIN
                SET @StuSessionSDate = DATEADD(month,@FeePaymentMode,@StuSessionSDate)
            END
        END

        -- Carry-forward stays in PreviousDue, but its paid portion must be included in summary paid amount.
        IF (@IsCarryForwardApplicable = 1 AND ISNULL(@CarryForwardAmount,0) > 0 AND ISNULL(@CarryForwardFeeTypeID,0) > 0)
        BEGIN
            SELECT @CarryForwardPaid = ISNULL(SUM(PD.PaymentRecieved), 0)
            FROM v_PaymentDetails PD
            WHERE PD.PayeeID = @StudentID
              AND PD.FeeTypeID = @CarryForwardFeeTypeID
              AND PD.Month = MONTH(@SessionStartDate)
              AND PD.Year = YEAR(@SessionStartDate)

            SET @PreviousDue = ISNULL(@PreviousDue,0) + ISNULL(@CarryForwardAmount,0)
            SET @Paid = ISNULL(@Paid,0) + ISNULL(@CarryForwardPaid,0)
        END

        IF (@PreviousDue < 0)
        BEGIN
            SET @PreviousDue = 0
        END

        UPDATE @Students
        SET
            FeeAmount = @FeeAmount,
            PreviousDue = @PreviousDue,
            Discounts = ISNULL(NULLIF(@Discounts,0), @pDiscount),
            Paid = @Paid,
            LateFee = @LateFee
        WHERE SequenceNo = @CurrentSequence

        SET @CurrentSequence = @CurrentSequence + 1
    END

    SELECT
        StudentID,
        Name,
        RollNo,
        Gender,
        Photo,
        ClassID,
        FeePaymentMode,
        StudentSID,
        FromDate,
        ToDate,
        QuotaID,
        SessionID,
        VehicleRouteID,
        HostelRoomID,
        SectionID,
        SessionStartDate,
        SessionEndDate,
        IsAdmissionFee,
        SchoolUID,
        FeeAmount,
        PreviousDue,
        LateFee,
        Discounts,
        Paid
    FROM @Students
    WHERE ISNULL(StudentID,0) <> 0
END
GO
