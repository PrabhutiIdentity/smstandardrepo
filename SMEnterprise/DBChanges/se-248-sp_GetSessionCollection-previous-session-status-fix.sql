/*
Fix: Account/CollectionReport should show records for previous sessions too.

Root cause:
- dbo.sp_GetSessionCollection always filters Student_Session with SS.Status = 1
- for historical sessions, promoted students are commonly stored with Status = 0
- result: previous session report misses those students and appears to show only current-session data

Change:
- keep SS.Status = 1 only when selected session is the branch active session
- for non-active/older sessions, include historical Student_Session rows regardless of status
*/

ALTER PROCEDURE [dbo].[sp_GetSessionCollection]
(
    @ClassID int,
    @SectionID int,
    @SBranchID int,
    @SessionID int,
    @QDate date,
    @CurDate date,
    @SStudentID int=0
)
AS
BEGIN
SET NOCOUNT ON
 -- [All the initial setup code stays the same... abbreviated for space]

    IF (@SectionID = 0)
    BEGIN
        SELECT TOP 1 @ClassID = ClassID FROM ClassMaster WHERE SBranchID = @SBranchID AND Status = 1 ORDER BY ClassID;
        SELECT TOP 1 @SectionID = ID FROM Class_Sections WHERE ClassID = @ClassID AND Status = 1 ORDER BY ID;
    END;

    SELECT ClassID AS ID, ClassName AS Name FROM ClassMaster WHERE SBranchID = @SBranchID;
    SELECT ID, Name FROM Class_Sections WHERE ClassID = @ClassID;
    SELECT ISNULL(@ClassID, 0);
    SELECT ISNULL(@SectionID, 0);
    SELECT SessionID AS ID, SessionName AS Name, SessionStatus AS Extra1 FROM SessionMaster WHERE SBranchID = @SBranchID;
    SELECT ISNULL(@SessionID, 0);

    DECLARE @ActiveSessionID INT, @GroupID INT, @TransportFeeMode INT, @HostelFeeMode INT, @LastPayDay NVARCHAR(2);
    DECLARE @SessionStartDate DATE, @SessionEndDate DATE;

    SELECT @ActiveSessionID = SessionID FROM SessionMaster WHERE SBranchID = @SBranchID AND SessionStatus = 1;
    SELECT @TransportFeeMode = details FROM MasterSettings WHERE Type = 'TransportFeeMode' AND SBranchID = @SBranchID;
    SELECT @HostelFeeMode = details FROM MasterSettings WHERE Type = 'HostelFeeMode' AND SBranchID = @SBranchID;
    SELECT @LastPayDay = details FROM MasterSettings WHERE Type = 'FeePaymentReminderDate' AND SBranchID = @SBranchID;

    if(@SStudentID<>0)
    begin
        select @ClassID=ClassID,@SectionID=SectionID from Student_Session where SessionID=@SessionID and StudentID=@SStudentID
    end

    SELECT @GroupID = GroupID FROM Class_Sections WHERE ID = @SectionID;
    SELECT @SessionStartDate = SessionStartDate, @SessionEndDate = SessionEndDate FROM SessionMaster WHERE SessionID = @SessionID;

    CREATE TABLE #Students (StudentID INT, Name NVARCHAR(100), RollNo NVARCHAR(100), Gender INT, Photo NVARCHAR(100), ClassID INT, FeePaymentMode INT, StudentSID NVARCHAR(15), FromDate DATE, ToDate DATE, QuotaID INT, SessionID INT, VehicleRouteID INT, HostelRoomID INT, SectionID INT, SessionStartDate DATE, SessionEndDate DATE, IsAdmissionFee INT, SchoolUID NVARCHAR(50), FeeAmount NUMERIC(10,2), PreviousDue NUMERIC(10,2), LateFee NUMERIC(10,2), Discounts NUMERIC(10,2), Paid NUMERIC(10,2), IsCustomFee INT);

    INSERT INTO #Students
    SELECT
        SM.StudentID,
        SM.Name,
        SS.RollNo,
        SM.Gender,
        SM.Photo,
        SS.ClassID,
        SS.FeePaymentMode,
        'STUD' + RIGHT(REPLICATE('0', 6) + CAST(SS.StudentID AS VARCHAR(6)), 6),
        SS.FromDate,
        SS.ToDate,
        SS.QuotaID,
        SS.SessionID,
        SM.VehicleRouteID,
        SM.HostelRoomID,
        SS.SectionID,
        CASE WHEN @SessionStartDate < SS.FromDate THEN SS.FromDate ELSE @SessionStartDate END,
        CASE WHEN @SessionEndDate > SS.ToDate THEN SS.ToDate ELSE @SessionEndDate END,
        ISNULL(SS.IsAdmissionFeeApplicable, 0),
        SM.SchoolUID,
        0, 0, 0, 0, 0,
        SS.IsCustomFee
    FROM Student_Session SS
    LEFT OUTER JOIN StudentMaster SM ON SM.StudentID = SS.StudentID
    WHERE SS.ClassID = @ClassID
      AND SS.SessionID = @SessionID
      AND SS.SectionID = @SectionID
      AND
      (
          (@ActiveSessionID = @SessionID AND SS.Status = 1)
          OR (@ActiveSessionID <> @SessionID)
      );

    CREATE TABLE #DiscountApproved (StudentID INT, FeeTypeID INT, ApprovedAmount DECIMAL(18,2), FeeMonth INT, FeeYear INT);
    CREATE TABLE #MTFT (MonthTypeID INT, FeeTypeID INT);
    CREATE TABLE #NoFeeMonths (Month INT);

    Insert into #DiscountApproved
    select StudentID,FeeTypeID,ApprovedAmount,FeeMonth,FeeYear from [FeeDiscountRequestDetails] FRD
    left outer join  [FeeDiscountRequestMaster] FRM on FRD.DiscRequestID=FRM.DiscRequestID
    where FRM.Status=1 and isnull(ApprovedAmount,0)<>0
    and StudentID in (select StudentID from #Students) and Status=1

    insert into #MTFT select MonthTypeID,FeeTypeID from [MonthTypeFeeType]
    insert into #NoFeeMonths select Month from SessionClassNoFeeMonths where ClassID=@ClassID and SessionID=@SessionID

    CREATE TABLE #FeeStructureTable (FeeTypeID INT, FeeTypeName NVARCHAR(50), FeeTypeApplicable INT, FeeAmount NUMERIC(10,2), Status INT, Months NVARCHAR(50));

    insert into #FeeStructureTable
    select FTM.FeeTypeID,FTM.FeeTypeName,FTM.FeeTypeApplicable,CFS.FeeAmount,isnull(CFS.Status,1),FTM.Months
    from FeeTypeMaster FTM left outer join [dbo].[ClassFeeStructureMaster] CFS on CFS.FeeTypeID=FTM.FeeTypeID and ClassID=@ClassID
    and GroupID=@GroupID
    and CFS.SessionID=@SessionID where FTM.SBranchID=@SBranchID

    CREATE TABLE #FeeDetailTable (StudentID INT, FeeMonth INT, FeeYear INT, FeeTypeApplicable INT, FeeTypeID INT, FeeTypeName NVARCHAR(100), FeeAmount NUMERIC(10,2), QDiscount NUMERIC(10,2), RDiscount NUMERIC(10,2), PayApplicableAmount NUMERIC(10,2), CustomFee NUMERIC(10,2), PaidAmount NUMERIC(10,2), IsCustomFee INT, IsPayment INT);
    CREATE TABLE #QuotaDiscounts (FeeTypeID INT, Discount NUMERIC(5,2));

    CREATE TABLE #PaymentDetailsWithAmount (
        PayeeID INT,
        FeeTypeID INT,
        Month INT,
        Year INT,
        Amount NUMERIC(10,2),
        DiscAmt NUMERIC(10,2),
        NetApplicablePayment NUMERIC(10,2),
        PaymentRecieved NUMERIC(10,2),
        PaymentDate DATE,
        PaymentID INT,
        INDEX IX_Payment (PayeeID, FeeTypeID, Month, Year)
    );

    INSERT INTO #PaymentDetailsWithAmount
    SELECT
        PayeeID,
        FeeTypeID,
        Month,
        Year,
        MAX(CAST(Amount AS NUMERIC(10,2))) AS Amount,
        SUM(DiscAmt) AS DiscAmt,
        SUM(CASE WHEN DuesPaidCount = 0 THEN NetApplicablePayment ELSE 0 END) AS NetApplicablePayment,
        SUM(PaymentRecieved) AS PaymentRecieved,
        MIN(PaymentDate) AS PaymentDate,
        MIN(PaymentID) AS PaymentID
    FROM PaymentDetails
    WHERE ISNULL(PaymentStatus, 0) = 0
      AND PayeeID IN (SELECT StudentID FROM #Students)
    GROUP BY PayeeID, FeeTypeID, Month, Year;

    Declare @StuSessionSDate date,@StuSessionEDate date,@StudentID int,@QuotaID int,@IsAdmissionFee int,
    @FeeAmount numeric(10,2),@PreviousDue numeric(10,2),@LateFee numeric(10,2),@Discounts numeric(10,2)
    ,@LMonth int,@LYear int,@MonthType int,@BaseDate date,@Paid numeric(10,2),@FeePaymentMode int,@IsCustomFee int
    declare @TotalAmount  numeric(10,2)=0
    declare @TotalPaid  numeric(10,2)=0
    declare @TotalDiscount  numeric(10,2)=0
    declare @isPaid int

    DECLARE stu_Cursor CURSOR FOR
    SELECT SessionStartDate,SessionEndDate,StudentID,QuotaID,IsAdmissionFee,FeeAmount,PreviousDue,LateFee,Discounts,Paid,FeePaymentMode
    FROM #Students where isnull(StudentID,0)!=0 FOR UPDATE OF FeeAmount,PreviousDue,LateFee,Discounts,Paid

    OPEN stu_Cursor
    FETCH NEXT FROM stu_Cursor
    INTO @StuSessionSDate,@StuSessionEDate,@StudentID,@QuotaID,@IsAdmissionFee,@FeeAmount,@PreviousDue,@LateFee,@Discounts,@Paid,@FeePaymentMode

    WHILE (@@FETCH_STATUS = 0)
    BEGIN
        declare @StudentSessionUID int
        SET @PreviousDue = 0;
        SET @FeeAmount = 0;
        set @Discounts=0

        select top 1 @StudentSessionUID=StudentSessionUID,@IsCustomFee=isnull(IsCustomFee,0) from Student_Session where StudentID=@StudentID and SessionID=@SessionID order by Status desc

        Delete from #QuotaDiscounts
        Insert into #QuotaDiscounts select FeeTypeID,DiscPer from QuotaDiscountDetails where SessionID=@SessionID and QuotaID=@QuotaID
        set @BaseDate=@StuSessionSDate
        declare @LoopDate date=@QDate
        declare @CMonth int=datepart(month,@QDate),@CYear int=datepart(year,@QDate)
        if(@QDate>@StuSessionEDate)
        begin
            set @LoopDate=@StuSessionEDate
        end
        else if((DATEDIFF(month,dateadd(month,-1,@SessionStartDate),@LoopDate))%@FeePaymentMode>0)
        begin
            declare @remainder int=(DATEDIFF(month,dateadd(month,-1,@SessionStartDate),@LoopDate))%@FeePaymentMode
            declare @adjustMonth int=@FeePaymentMode-@remainder
            set @LoopDate=DateAdd(month,@adjustMonth, @LoopDate)
            set @CMonth = datepart(month,dateadd(month,1-@remainder,@QDate))
            set @CYear = datepart(year,dateadd(month,1-@remainder,@QDate))
        end

        while(datepart(year,@StuSessionSDate)*12+datepart(month,@StuSessionSDate)<=datepart(year,@LoopDate)*12+datepart(month,@LoopDate))
        begin
            set @LMonth= datepart(month,@StuSessionSDate)
            set @LYear=datepart(year,@StuSessionSDate)

            if((select count(*) from #NoFeeMonths where Month=@LMonth)=0)
            begin
                declare @PayDate date
                select @PayDate=min(PaymentDate) from PaymentDetails where PayeeID=@StudentID and Month=@LMonth and Year=@LYear and isnull(PaymentStatus,0)=0
                declare @IsLatePay int=0
                if(@PayDate is not null and @PayDate>cast(cast(@LYear as nvarchar(5))+'-'+cast(@LMonth as nvarchar(2))+'-'+@LastPayDay as date))
                begin
                    set @IsLatePay=1
                end
                else if(@PayDate is null and @CurDate>cast(cast(@LYear as nvarchar(5))+'-'+cast(@LMonth as nvarchar(2))+'-'+@LastPayDay as date))
                begin
                    set @IsLatePay=1
                end
                else
                begin
                    set @IsLatePay=0
                end

                select @MonthType=(Case when datepart(month,@BaseDate)=@LMonth and (datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or
                    datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth) then 5 else
                    (Case when datepart(month,@BaseDate)=@LMonth and (datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth) then 6 else
                    (case when datepart(month,@BaseDate)=@LMonth then (case when @IsAdmissionFee=0 then 1 else 0 end)
                    else (Case when datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth then 3
                    else (case when  datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth then 4 else 2 end) end) end)end)end)

                declare @TPaid numeric(10,2)=0
                declare @CLateFee numeric(10,2)=0
                declare @CPaid numeric(10,2)=0
                declare @PPD numeric(10,2)=0
                declare @PPR numeric(10,2)=0

                SELECT
                    @PPD = SUM(CASE
                        WHEN ISNULL(PD.PaymentID, 0) <> 0 THEN PD.NetApplicablePayment - ISNULL(PD.DiscAmt, 0)
                        WHEN ISNULL(@IsCustomFee, 0) = 1 AND PD.Amount IS NOT NULL THEN PD.Amount
                        WHEN ISNULL(@IsCustomFee, 0) = 1 AND SFD.FeeAmount IS NOT NULL THEN SFD.FeeAmount
                        ELSE FTS.FeeAmount - ISNULL(FTS.FeeAmount * QD.Discount / 100, 0)
                    END),
                    @PPR = ISNULL(SUM(PD.PaymentRecieved), 0)
                FROM #FeeStructureTable FTS
                LEFT OUTER JOIN #QuotaDiscounts QD ON QD.FeeTypeID = FTS.FeeTypeID
                LEFT OUTER JOIN StudentFeeDetails SFD ON SFD.StudentID = @StudentID AND SFD.SessionID = @StudentSessionUID AND SFD.FeeTypeID = FTS.FeeTypeID AND SFD.IsApplicable = 1
                LEFT OUTER JOIN #PaymentDetailsWithAmount PD ON PD.FeeTypeID = FTS.FeeTypeID AND PD.PayeeID = @StudentID AND PD.Month = @LMonth AND PD.Year = @LYear
                WHERE FTS.FeeTypeApplicable IN (SELECT FeeTypeID FROM #MTFT WHERE MonthTypeID = @MonthType)
                    AND FTS.FeeTypeApplicable <> 5 AND FTS.FeeTypeApplicable <> 7
                    AND FTS.FeeTypeApplicable <> CASE WHEN @IsAdmissionFee = 0 THEN 8 ELSE 0 END
                    AND ISNULL(SFD.IsApplicable, FTS.Status) = 1
                    AND (FTS.FeeTypeApplicable <> 9 OR EXISTS (SELECT 1 FROM dbo.SplitStringToTable(FTS.Months, ',') WHERE Item = @LMonth));

                declare @mDiscount numeric(10,2),@mPaid numeric(10,2),@mAmount numeric(10,2),@IsPaymentDone int
                SELECT @IsPaymentDone = COUNT(*) FROM #PaymentDetailsWithAmount WHERE PayeeID = @StudentID AND Month = @LMonth AND Year = @LYear;
                SELECT @mDiscount = SUM(ISNULL(DiscAmt, 0)), @mAmount = SUM(NetApplicablePayment), @mPaid = SUM(PaymentRecieved) FROM #PaymentDetailsWithAmount WHERE PayeeID = @StudentID AND Month = @LMonth AND Year = @LYear AND FeeTypeID <> -2;

                INSERT INTO #FeeDetailTable
                SELECT @StudentID, @LMonth, @LYear, FTS.FeeTypeApplicable, FTS.FeeTypeID, FTS.FeeTypeName,
                    CASE WHEN FTS.FeeTypeApplicable = 5 THEN dbo.fn_GetStudentTransportFeeAmount(@LMonth, @LYear, @StudentID, @TransportFeeMode, @ClassID, @GroupID, @SBranchID, @SessionID) ELSE FTS.FeeAmount END,
                    ISNULL(QD.Discount, 0),
                    CASE WHEN FTS.FeeTypeApplicable = 7 THEN ISNULL(PD.DiscAmt, 0) WHEN PD.PaymentID IS NOT NULL AND ISNULL(PD.DiscAmt, 0) = 0 AND DA.ApprovedAmount IS NOT NULL AND DA.ApprovedAmount <> 0 THEN DA.ApprovedAmount WHEN PD.PaymentID IS NOT NULL AND ISNULL(PD.DiscAmt, 0) <> 0 THEN ISNULL(PD.DiscAmt, 0) ELSE ISNULL(DA.ApprovedAmount, 0) END,
                    ISNULL(PD.NetApplicablePayment, 0),
                    CASE WHEN PD.Amount IS NOT NULL THEN PD.Amount ELSE ISNULL(SFD.FeeAmount, 0) END,
                    ISNULL(PD.PaymentRecieved, 0), ISNULL(@IsCustomFee, 0), CASE WHEN PD.PaymentID IS NULL THEN 0 ELSE 1 END
                FROM #FeeStructureTable FTS
                LEFT OUTER JOIN #QuotaDiscounts QD ON QD.FeeTypeID = FTS.FeeTypeID
                LEFT OUTER JOIN StudentFeeDetails SFD ON SFD.StudentID = @StudentID AND SFD.SessionID = @StudentSessionUID AND SFD.FeeTypeID = FTS.FeeTypeID
                LEFT OUTER JOIN #PaymentDetailsWithAmount PD ON PD.FeeTypeID = FTS.FeeTypeID AND PD.PayeeID = @StudentID AND PD.Month = @LMonth AND PD.Year = @LYear
                LEFT OUTER JOIN #DiscountApproved DA ON DA.StudentID = @StudentID AND DA.FeeTypeID = FTS.FeeTypeID AND DA.FeeMonth = @LMonth AND DA.FeeYear = @LYear
                WHERE FTS.FeeTypeApplicable IN (SELECT FeeTypeID FROM #MTFT WHERE MonthTypeID = @MonthType)
                    AND FTS.FeeTypeID <> CASE WHEN @IsLatePay = 0 THEN -2 ELSE 0 END
                    AND FTS.FeeTypeApplicable <> CASE WHEN @IsAdmissionFee = 0 THEN 8 ELSE 0 END
                    AND ISNULL(SFD.IsApplicable, FTS.Status) = 1
                    AND (FTS.FeeTypeApplicable <> 9 OR EXISTS (SELECT 1 FROM dbo.SplitStringToTable(FTS.Months, ',') WHERE Item = @LMonth));

                DECLARE @pDiscount NUMERIC(10,2) = 0;
                IF ((ISNULL(@mDiscount, 0) = 0 AND ISNULL(@mAmount, 0) - ISNULL(@mPaid, 0) >= 0) OR @IsPaymentDone = 0)
                    SELECT @pDiscount = ISNULL(SUM(ApprovedAmount), 0) FROM #DiscountApproved WHERE StudentID = @StudentID AND FeeMonth = @LMonth AND FeeYear = @LYear;

                IF EXISTS (SELECT 1 FROM #FeeStructureTable WHERE FeeTypeApplicable = 5)
                    SELECT @PreviousDue = ISNULL(@PreviousDue, 0) + ISNULL(dbo.fn_GetStudentTransportFeeAmount(@LMonth, @LYear, @StudentID, @TransportFeeMode, @ClassID, @GroupID, @SBranchID, @SessionID), 0);

                IF EXISTS (SELECT 1 FROM #FeeStructureTable WHERE FeeTypeApplicable = 6)
                    SELECT @PreviousDue = ISNULL(@PreviousDue, 0) + ISNULL(dbo.fn_GetStudentHostalFeeAmount(@LMonth, @LYear, @StudentID, @HostelFeeMode, @ClassID, @GroupID, @SBranchID, @SessionID), 0);

                SELECT @PreviousDue = ISNULL(@PreviousDue, 0) - ISNULL(SUM(ISNULL(PD.PaymentRecieved, 0) + ISNULL(PD.DiscAmt, 0)), 0)
                FROM #FeeStructureTable FTS
                LEFT OUTER JOIN #PaymentDetailsWithAmount PD ON PD.FeeTypeID = FTS.FeeTypeID AND PD.PayeeID = @StudentID AND PD.Month = @LMonth AND PD.Year = @LYear
                WHERE FTS.FeeTypeApplicable IN (5, 6);

                SELECT @CLateFee = SUM(CASE WHEN ISNULL(PD.PaymentID, 0) <> 0 THEN PD.NetApplicablePayment - ISNULL(PD.DiscAmt, 0) ELSE FTS.FeeAmount END), @CPaid = SUM(ISNULL(PD.PaymentRecieved, 0))
                FROM #FeeStructureTable FTS
                LEFT OUTER JOIN #PaymentDetailsWithAmount PD ON PD.FeeTypeID = FTS.FeeTypeID AND PD.PayeeID = @StudentID AND PD.Month = @LMonth AND PD.Year = @LYear
                WHERE FTS.FeeTypeApplicable = CASE WHEN @IsLatePay = 0 THEN 0 ELSE 7 END AND ISNULL(FTS.Status, 0) = 1;

                SET @PreviousDue = (ISNULL(@PreviousDue, 0) - ISNULL(@pDiscount, 0)) + (ISNULL(@CLateFee, 0) - ISNULL(@CPaid, 0)) + (ISNULL(@PPD, 0) - ISNULL(@PPR, 0));
            END;

            IF (@PayDate IS NULL) SET @StuSessionSDate = DATEADD(MONTH, 1, @StuSessionSDate);
            ELSE SET @StuSessionSDate = DATEADD(MONTH, @FeePaymentMode, @StuSessionSDate);
        END;

        UPDATE #Students SET FeeAmount = @FeeAmount, PreviousDue = @PreviousDue, Discounts = ISNULL(NULLIF(@Discounts, 0), @pDiscount), Paid = @Paid, LateFee = @LateFee WHERE CURRENT OF stu_Cursor;
        FETCH NEXT FROM stu_Cursor INTO @StuSessionSDate, @StuSessionEDate, @StudentID, @QuotaID, @IsAdmissionFee, @FeeAmount, @PreviousDue, @LateFee, @Discounts, @Paid, @FeePaymentMode;
    END;

    CLOSE stu_Cursor
    DEALLOCATE stu_Cursor

    ;WITH FeeCalc AS (
        SELECT studentID,
            CASE
                WHEN IsCustomFee = 1 AND CustomFee IS NOT NULL AND CustomFee <> 0 THEN CustomFee - ISNULL(RDiscount, 0)
                ELSE FeeAmount - ISNULL(RDiscount, 0)
            END AS ApplicableAmount,
            ISNULL(PaidAmount, 0) AS PaidAmount,
            ISNULL(Rdiscount, 0) AS Discounts,
            IsPayment
        FROM #FeeDetailTable
    ),
    FeeSummary AS (
        SELECT studentID,
            SUM(ApplicableAmount) AS FeeAmount,
            SUM(Discounts) AS Discounts,
            SUM(CASE WHEN IsPayment = 1 THEN PaidAmount ELSE 0 END) AS PaidAmount,
            SUM(CASE WHEN IsPayment = 0 THEN ApplicableAmount ELSE 0 END) AS UnpaidAmount
        FROM FeeCalc
        GROUP BY StudentID
    )
    SELECT S.StudentID, S.Name, S.RollNo, S.Gender, S.Photo, S.ClassID, S.FeePaymentMode, S.StudentSID, S.FromDate, S.ToDate, S.QuotaID, S.SessionID, S.VehicleRouteID, S.HostelRoomID, S.SectionID, S.SessionStartDate, S.SessionEndDate, S.IsAdmissionFee, S.SchoolUID, S.PreviousDue, S.LateFee, S.Paid, Q.QuotaName, PM.MotherName, PM.FatherName, FS.FeeAmount, FS.Discounts, FS.PaidAmount, FS.UnpaidAmount
    FROM #Students S
    LEFT OUTER JOIN QuotaMaster Q ON Q.QuotaID = S.QuotaID
    LEFT OUTER JOIN StudentMaster SM ON SM.StudentID = S.StudentID
    LEFT OUTER JOIN ParentMaster PM ON PM.ParentID = SM.ParentID
    LEFT JOIN FeeSummary FS ON FS.StudentID = S.StudentID
    WHERE ISNULL(S.StudentID, 0) <> 0;

    SELECT * FROM SBranchMaster WHERE SBranchID = @SBranchID;

    DROP TABLE #Students, #FeeDetailTable, #DiscountApproved,#QuotaDiscounts,#MTFT, #NoFeeMonths, #FeeStructureTable, #PaymentDetailsWithAmount;

    SET NOCOUNT OFF
END
GO
