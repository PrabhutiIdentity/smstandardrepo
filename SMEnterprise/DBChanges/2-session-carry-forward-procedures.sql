/*
Review-first SQL script for session carry-forward generation.
This file only defines procedures. Do not execute blindly.

Primary use case:
- Generate carry-forward fee rows school-wise / branch-wise
- Support already-promoted schools
- Keep execution manual, one branch at a time
- Allow preview before apply

Suggested first review run for branch 34:
EXEC dbo.sp_PreviewSessionCarryForward
    @SBranchID = 34,
    @TargetSessionID = <target-session-id>,
    @ClassID = <target-class-id>,
    @SectionID = 0,
    @StudentID = 0;

Suggested apply run after review:
EXEC dbo.sp_ApplySessionCarryForward
    @SBranchID = 34,
    @TargetSessionID = <target-session-id>,
    @ClassID = <target-class-id>,
    @SectionID = 0,
    @StudentID = 0,
    @PreviewOnly = 0,
    @ForceUpdate = 0,
    @RunBy = 'manual-review';

Important note:
- Do not switch quota-based students into custom fee mode for carry-forward only.
- Sibling quota and other percentage-based quota discounts depend on the regular class fee flow.
- Session-Carry-forward should be treated as a separate additive fee row in fee procedures.
- Once carry-forward is applied for a student/session pair, old-session arrear collection should be blocked
  through the validation helper in this script.
*/

IF OBJECT_ID(N'dbo.SessionCarryForwardMap', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SessionCarryForwardMap]
    (
        [MapID] int IDENTITY(1,1) NOT NULL,
        [SBranchID] int NOT NULL,
        [SchoolID] int NULL,
        [StudentID] int NOT NULL,
        [PreviousSessionID] int NOT NULL,
        [TargetSessionID] int NOT NULL,
        [TargetStudentSessionUID] int NOT NULL,
        [CarryForwardFeeTypeID] int NOT NULL,
        [GeneratedAmount] numeric(10,2) NOT NULL,
        [Status] int NOT NULL CONSTRAINT [DF_SessionCarryForwardMap_Status] DEFAULT ((1)),
        [RunBy] nvarchar(100) NULL,
        [CreatedDate] datetime NOT NULL CONSTRAINT [DF_SessionCarryForwardMap_CreatedDate] DEFAULT (GETDATE()),
        [ModifiedDate] datetime NULL,
        CONSTRAINT [PK_SessionCarryForwardMap] PRIMARY KEY CLUSTERED ([MapID] ASC)
    );

    CREATE UNIQUE NONCLUSTERED INDEX [IX_SessionCarryForwardMap_StudentSession]
        ON [dbo].[SessionCarryForwardMap]([StudentID], [PreviousSessionID], [TargetSessionID], [Status]);
END;
GO

IF OBJECT_ID(N'dbo.sp_ValidateCarryForwardPaymentLock', N'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_ValidateCarryForwardPaymentLock AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[sp_ValidateCarryForwardPaymentLock]
(
    @SBranchID int,
    @StudentID int,
    @SessionID int,
    @IsBlocked bit OUTPUT,
    @Message nvarchar(500) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @IsBlocked = 0;
    SET @Message = N'';

    IF EXISTS
    (
        SELECT 1
        FROM dbo.SessionCarryForwardMap SCM
        WHERE SCM.SBranchID = @SBranchID
          AND SCM.StudentID = @StudentID
          AND SCM.PreviousSessionID = @SessionID
          AND SCM.Status = 1
    )
    BEGIN
        SET @IsBlocked = 1;
        SET @Message = N'Previous session due has already been moved to current session carry-forward. Please collect it from the current session only.';
    END;
END;
GO

IF OBJECT_ID(N'dbo.sp_PreviewSessionCarryForward', N'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_PreviewSessionCarryForward AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[sp_PreviewSessionCarryForward]
(
    @SBranchID int,
    @TargetSessionID int,
    @ClassID int = 0,
    @SectionID int = 0,
    @StudentID int = 0,
    @SchoolID int = 0,
    @IncludeZeroDue bit = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @TargetSessionStartDate date,
        @TargetSessionEndDate date,
        @TargetSessionName nvarchar(100),
        @TargetSchoolID int,
        @PreviousSessionID int,
        @PreviousSessionStartDate date,
        @PreviousSessionEndDate date,
        @PreviousSessionName nvarchar(100),
        @CarryForwardFeeTypeID int;

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

    SELECT
        @TargetSessionStartDate = SM.SessionStartDate,
        @TargetSessionEndDate = SM.SessionEndDate,
        @TargetSessionName = SM.SessionName
    FROM SessionMaster SM
    WHERE SM.SessionID = @TargetSessionID
      AND SM.SBranchID = @SBranchID;

    IF @TargetSessionStartDate IS NULL
    BEGIN
        RAISERROR('Target session not found for the selected branch.', 16, 1);
        RETURN;
    END;

    SELECT
        @TargetSchoolID = SBM.SchoolID
    FROM SBranchMaster SBM
    WHERE SBM.SBranchID = @SBranchID;

    IF @SchoolID <> 0 AND ISNULL(@TargetSchoolID, 0) <> @SchoolID
    BEGIN
        RAISERROR('Provided SchoolID does not match the selected branch.', 16, 1);
        RETURN;
    END;

    SELECT TOP 1
        @PreviousSessionID = SM.SessionID,
        @PreviousSessionStartDate = SM.SessionStartDate,
        @PreviousSessionEndDate = SM.SessionEndDate,
        @PreviousSessionName = SM.SessionName
    FROM SessionMaster SM
    WHERE SM.SBranchID = @SBranchID
      AND SM.SessionEndDate < @TargetSessionStartDate
    ORDER BY SM.SessionEndDate DESC, SM.SessionID DESC;

    IF @PreviousSessionID IS NULL
    BEGIN
        RAISERROR('Previous session could not be resolved for the selected target session.', 16, 1);
        RETURN;
    END;

    EXEC dbo.sp_GetSessionCarryForwardFeeTypeID
        @SBranchID = @SBranchID,
        @FeeTypeID = @CarryForwardFeeTypeID OUTPUT;

    CREATE TABLE #TargetStudents
    (
        StudentID int PRIMARY KEY,
        Name nvarchar(100),
        RollNo nvarchar(100),
        TargetStudentSessionUID int,
        TargetClassID int,
        TargetSectionID int,
        TargetIsCustomFee int,
        PreviousStudentSessionUID int,
        PreviousClassID int,
        PreviousSectionID int
    );

    ;WITH TargetStudentRows AS
    (
        SELECT
            TSS.StudentID,
            SM.Name,
            TSS.RollNo,
            TSS.StudentSessionUID,
            TSS.ClassID,
            TSS.SectionID,
            ISNULL(TSS.IsCustomFee, 0) AS TargetIsCustomFee,
            ROW_NUMBER() OVER
            (
                PARTITION BY TSS.StudentID
                ORDER BY TSS.StudentSessionUID DESC
            ) AS RN
        FROM Student_Session TSS
        INNER JOIN StudentMaster SM
            ON SM.StudentID = TSS.StudentID
        WHERE TSS.SessionID = @TargetSessionID
          AND TSS.SBranchID = @SBranchID
          AND TSS.Status = 1
          AND (@ClassID = 0 OR TSS.ClassID = @ClassID)
          AND (@SectionID = 0 OR TSS.SectionID = @SectionID)
          AND (@StudentID = 0 OR TSS.StudentID = @StudentID)
    )
    INSERT INTO #TargetStudents
    (
        StudentID,
        Name,
        RollNo,
        TargetStudentSessionUID,
        TargetClassID,
        TargetSectionID,
        TargetIsCustomFee,
        PreviousStudentSessionUID,
        PreviousClassID,
        PreviousSectionID
    )
    SELECT
        TSR.StudentID,
        TSR.Name,
        TSR.RollNo,
        TSR.StudentSessionUID,
        TSR.ClassID,
        TSR.SectionID,
        TSR.TargetIsCustomFee,
        PSS.StudentSessionUID,
        PSS.ClassID,
        PSS.SectionID
    FROM TargetStudentRows TSR
    OUTER APPLY
    (
        SELECT TOP 1
            PS.StudentSessionUID,
            PS.ClassID,
            PS.SectionID
        FROM Student_Session PS
        WHERE PS.StudentID = TSR.StudentID
          AND PS.SessionID = @PreviousSessionID
        ORDER BY
            CASE WHEN PS.Status = 1 THEN 0 ELSE 1 END,
            PS.StudentSessionUID DESC
    ) PSS
    WHERE TSR.RN = 1;

    CREATE TABLE #FeeSummary
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
        FeeAmount numeric(10, 2),
        PreviousDue numeric(10, 2),
        LateFee numeric(10, 2),
        Discounts numeric(10, 2),
        Paid numeric(10, 2)
    );

    CREATE TABLE #Preview
    (
        SBranchID int,
        SchoolID int,
        TargetSessionID int,
        TargetSessionName nvarchar(100),
        TargetSessionStartDate date,
        TargetSessionEndDate date,
        PreviousSessionID int,
        PreviousSessionName nvarchar(100),
        PreviousSessionStartDate date,
        PreviousSessionEndDate date,
        StudentID int,
        Name nvarchar(100),
        RollNo nvarchar(100),
        TargetStudentSessionUID int,
        TargetClassID int,
        TargetSectionID int,
        PreviousStudentSessionUID int,
        PreviousClassID int,
        PreviousSectionID int,
        TargetIsCustomFee int,
        CarryForwardFeeTypeID int,
        ExistingCarryForwardAmount numeric(10, 2),
        ExistingCarryForwardIsApplicable int,
        ExistingTargetStudentFeeRowCount int,
        HasCarryForwardPayment int,
        PreviousSessionFeeAmount numeric(10, 2),
        PreviousSessionPreviousDue numeric(10, 2),
        PreviousSessionLateFee numeric(10, 2),
        CarryForwardAmount numeric(10, 2),
        PreviewStatus nvarchar(50),
        PreviewRemarks nvarchar(500)
    );

    DECLARE
        @LoopStudentID int,
        @LoopTargetStudentSessionUID int,
        @LoopTargetClassID int,
        @LoopTargetSectionID int,
        @LoopTargetIsCustomFee int,
        @LoopPreviousStudentSessionUID int,
        @LoopPreviousClassID int,
        @LoopPreviousSectionID int,
        @LoopName nvarchar(100),
        @LoopRollNo nvarchar(100),
        @CarryForwardAmount numeric(10, 2),
        @ExistingCarryForwardAmount numeric(10, 2),
        @ExistingCarryForwardIsApplicable int,
        @ExistingTargetStudentFeeRowCount int,
        @HasCarryForwardPayment int,
        @PreviousSessionFeeAmount numeric(10, 2),
        @PreviousSessionPreviousDue numeric(10, 2),
        @PreviousSessionLateFee numeric(10, 2),
        @PreviewStatus nvarchar(50),
        @PreviewRemarks nvarchar(500);

    DECLARE cur_TargetStudents CURSOR LOCAL FAST_FORWARD FOR
    SELECT
        TS.StudentID,
        TS.Name,
        TS.RollNo,
        TS.TargetStudentSessionUID,
        TS.TargetClassID,
        TS.TargetSectionID,
        TS.TargetIsCustomFee,
        TS.PreviousStudentSessionUID,
        TS.PreviousClassID,
        TS.PreviousSectionID
    FROM #TargetStudents TS
    ORDER BY TS.StudentID;

    OPEN cur_TargetStudents;

    FETCH NEXT FROM cur_TargetStudents
    INTO
        @LoopStudentID,
        @LoopName,
        @LoopRollNo,
        @LoopTargetStudentSessionUID,
        @LoopTargetClassID,
        @LoopTargetSectionID,
        @LoopTargetIsCustomFee,
        @LoopPreviousStudentSessionUID,
        @LoopPreviousClassID,
        @LoopPreviousSectionID;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        DELETE FROM #FeeSummary;

        SET @CarryForwardAmount = 0;
        SET @ExistingCarryForwardAmount = 0;
        SET @ExistingCarryForwardIsApplicable = 0;
        SET @ExistingTargetStudentFeeRowCount = 0;
        SET @HasCarryForwardPayment = 0;
        SET @PreviousSessionFeeAmount = 0;
        SET @PreviousSessionPreviousDue = 0;
        SET @PreviousSessionLateFee = 0;
        SET @PreviewStatus = N'OK';
        SET @PreviewRemarks = N'Ready for review.';

        IF ISNULL(@LoopPreviousStudentSessionUID, 0) = 0
        BEGIN
            SET @PreviewStatus = N'SKIP';
            SET @PreviewRemarks = N'Previous session record not found for target student.';
        END;
        ELSE
        BEGIN
            INSERT INTO #FeeSummary
            EXEC dbo.sp_GetClassGroupFeeListOnly
                @ClassID = @LoopPreviousClassID,
                @SectionID = @LoopPreviousSectionID,
                @SBranchID = @SBranchID,
                @SessionID = @PreviousSessionID,
                @QDate = @PreviousSessionEndDate,
                @CurDate = @PreviousSessionEndDate,
                @SStudentID = @LoopStudentID;

            SELECT TOP 1
                @PreviousSessionFeeAmount = ISNULL(FS.FeeAmount, 0),
                @PreviousSessionPreviousDue = ISNULL(FS.PreviousDue, 0),
                @PreviousSessionLateFee = ISNULL(FS.LateFee, 0)
            FROM #FeeSummary FS
            WHERE FS.StudentID = @LoopStudentID;

            SET @CarryForwardAmount =
                ISNULL(@PreviousSessionFeeAmount, 0)
                + ISNULL(@PreviousSessionPreviousDue, 0)
                + ISNULL(@PreviousSessionLateFee, 0);

            IF @CarryForwardAmount < 0
            BEGIN
                SET @CarryForwardAmount = 0;
            END;

            IF @CarryForwardFeeTypeID IS NOT NULL
            BEGIN
                SELECT TOP 1
                    @ExistingCarryForwardAmount = ISNULL(SFD.FeeAmount, 0),
                    @ExistingCarryForwardIsApplicable = ISNULL(SFD.IsApplicable, 0)
                FROM StudentFeeDetails SFD
                WHERE SFD.StudentID = @LoopStudentID
                  AND SFD.SessionID = @LoopTargetStudentSessionUID
                  AND SFD.FeeTypeID = @CarryForwardFeeTypeID;

                SELECT
                    @HasCarryForwardPayment =
                        CASE WHEN EXISTS
                        (
                            SELECT 1
                            FROM PaymentDetails PD
                            INNER JOIN PaymentMaster PM
                                ON PM.PaymentID = PD.PaymentID
                            WHERE PD.PayeeID = @LoopStudentID
                              AND PD.FeeTypeID = @CarryForwardFeeTypeID
                              AND PM.SessionID = @TargetSessionID
                              AND ISNULL(PD.PaymentStatus, 0) = 0
                        )
                        THEN 1 ELSE 0 END;
            END;

            SELECT
                @ExistingTargetStudentFeeRowCount = COUNT(*)
            FROM StudentFeeDetails SFD
            WHERE SFD.StudentID = @LoopStudentID
              AND SFD.SessionID = @LoopTargetStudentSessionUID;

            IF @CarryForwardAmount = 0 AND @IncludeZeroDue = 0
            BEGIN
                SET @PreviewStatus = N'SKIP';
                SET @PreviewRemarks = N'No closing due found in the previous session.';
            END
            ELSE IF @CarryForwardFeeTypeID IS NULL
            BEGIN
                SET @PreviewStatus = N'REVIEW';
                SET @PreviewRemarks = N'Session-Carry-forward fee type does not exist yet for this branch. Apply proc will create it when executed.';
            END
            ELSE IF @HasCarryForwardPayment = 1
            BEGIN
                SET @PreviewStatus = N'REVIEW';
                SET @PreviewRemarks = N'Carry-forward fee payment already exists in the target session. Verify before update.';
            END
            ELSE IF @ExistingCarryForwardAmount > 0
            BEGIN
                SET @PreviewStatus = N'REVIEW';
                SET @PreviewRemarks = N'Carry-forward row already exists in target session. Verify before update.';
            END
            ELSE IF @LoopTargetIsCustomFee = 0
            BEGIN
                SET @PreviewStatus = N'REVIEW';
                SET @PreviewRemarks = N'Target student is not custom fee. Keep regular quota flow intact and read Session-Carry-forward separately in fee procedures.';
            END;
        END;

        INSERT INTO #Preview
        (
            SBranchID,
            SchoolID,
            TargetSessionID,
            TargetSessionName,
            TargetSessionStartDate,
            TargetSessionEndDate,
            PreviousSessionID,
            PreviousSessionName,
            PreviousSessionStartDate,
            PreviousSessionEndDate,
            StudentID,
            Name,
            RollNo,
            TargetStudentSessionUID,
            TargetClassID,
            TargetSectionID,
            PreviousStudentSessionUID,
            PreviousClassID,
            PreviousSectionID,
            TargetIsCustomFee,
            CarryForwardFeeTypeID,
            ExistingCarryForwardAmount,
            ExistingCarryForwardIsApplicable,
            ExistingTargetStudentFeeRowCount,
            HasCarryForwardPayment,
            PreviousSessionFeeAmount,
            PreviousSessionPreviousDue,
            PreviousSessionLateFee,
            CarryForwardAmount,
            PreviewStatus,
            PreviewRemarks
        )
        VALUES
        (
            @SBranchID,
            ISNULL(@TargetSchoolID, 0),
            @TargetSessionID,
            @TargetSessionName,
            @TargetSessionStartDate,
            @TargetSessionEndDate,
            @PreviousSessionID,
            @PreviousSessionName,
            @PreviousSessionStartDate,
            @PreviousSessionEndDate,
            @LoopStudentID,
            @LoopName,
            @LoopRollNo,
            @LoopTargetStudentSessionUID,
            @LoopTargetClassID,
            @LoopTargetSectionID,
            @LoopPreviousStudentSessionUID,
            @LoopPreviousClassID,
            @LoopPreviousSectionID,
            @LoopTargetIsCustomFee,
            ISNULL(@CarryForwardFeeTypeID, 0),
            ISNULL(@ExistingCarryForwardAmount, 0),
            ISNULL(@ExistingCarryForwardIsApplicable, 0),
            ISNULL(@ExistingTargetStudentFeeRowCount, 0),
            ISNULL(@HasCarryForwardPayment, 0),
            ISNULL(@PreviousSessionFeeAmount, 0),
            ISNULL(@PreviousSessionPreviousDue, 0),
            ISNULL(@PreviousSessionLateFee, 0),
            ISNULL(@CarryForwardAmount, 0),
            @PreviewStatus,
            @PreviewRemarks
        );

        FETCH NEXT FROM cur_TargetStudents
        INTO
            @LoopStudentID,
            @LoopName,
            @LoopRollNo,
            @LoopTargetStudentSessionUID,
            @LoopTargetClassID,
            @LoopTargetSectionID,
            @LoopTargetIsCustomFee,
            @LoopPreviousStudentSessionUID,
            @LoopPreviousClassID,
            @LoopPreviousSectionID;
    END;

    CLOSE cur_TargetStudents;
    DEALLOCATE cur_TargetStudents;

    SELECT
        P.SBranchID,
        P.SchoolID,
        P.TargetSessionID,
        P.TargetSessionName,
        P.TargetSessionStartDate,
        P.TargetSessionEndDate,
        P.PreviousSessionID,
        P.PreviousSessionName,
        P.PreviousSessionStartDate,
        P.PreviousSessionEndDate,
        P.StudentID,
        P.Name,
        P.RollNo,
        P.TargetStudentSessionUID,
        P.TargetClassID,
        P.TargetSectionID,
        P.PreviousStudentSessionUID,
        P.PreviousClassID,
        P.PreviousSectionID,
        P.TargetIsCustomFee,
        P.CarryForwardFeeTypeID,
        P.ExistingCarryForwardAmount,
        P.ExistingCarryForwardIsApplicable,
        P.ExistingTargetStudentFeeRowCount,
        P.HasCarryForwardPayment,
        P.PreviousSessionFeeAmount,
        P.PreviousSessionPreviousDue,
        P.PreviousSessionLateFee,
        P.CarryForwardAmount,
        P.PreviewStatus,
        P.PreviewRemarks,
        CASE
            WHEN EXISTS
            (
                SELECT 1
                FROM dbo.SessionCarryForwardMap SCM
                WHERE SCM.SBranchID = P.SBranchID
                  AND SCM.StudentID = P.StudentID
                  AND SCM.PreviousSessionID = P.PreviousSessionID
                  AND SCM.TargetSessionID = P.TargetSessionID
                  AND SCM.Status = 1
            )
            THEN 1 ELSE 0
        END AS IsAlreadyMapped
    FROM #Preview P
    WHERE @IncludeZeroDue = 1
       OR P.CarryForwardAmount > 0
       OR P.PreviewStatus <> N'SKIP'
    ORDER BY P.PreviewStatus DESC, P.CarryForwardAmount DESC, P.StudentID;
END;
GO

IF OBJECT_ID(N'dbo.sp_ApplySessionCarryForward', N'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_ApplySessionCarryForward AS BEGIN SET NOCOUNT ON; END');
GO

alter PROCEDURE [dbo].[sp_ApplySessionCarryForward]
(
    @SBranchID int,
    @TargetSessionID int,
    @ClassID int = 0,
    @SectionID int = 0,
    @StudentID int = 0,
    @SchoolID int = 0,
    @PreviewOnly bit = 1,
    @ForceUpdate bit = 0,
    @SkipIfCarryForwardPaymentExists bit = 1,
    @RunBy nvarchar(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE
        @TargetSessionStartDate date,
        @TargetSessionEndDate date,
        @TargetSessionName nvarchar(100),
        @TargetSchoolID int,
        @PreviousSessionID int,
        @PreviousSessionStartDate date,
        @PreviousSessionEndDate date,
        @PreviousSessionName nvarchar(100),
        @CarryForwardFeeTypeID int,
        @CarryForwardMonth int;

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

    SELECT
        @TargetSessionStartDate = SM.SessionStartDate,
        @TargetSessionEndDate = SM.SessionEndDate,
        @TargetSessionName = SM.SessionName
    FROM SessionMaster SM
    WHERE SM.SessionID = @TargetSessionID
      AND SM.SBranchID = @SBranchID;

    IF @TargetSessionStartDate IS NULL
    BEGIN
        RAISERROR('Target session not found for the selected branch.', 16, 1);
        RETURN;
    END;

    SELECT
        @TargetSchoolID = SBM.SchoolID
    FROM SBranchMaster SBM
    WHERE SBM.SBranchID = @SBranchID;

    IF @SchoolID <> 0 AND ISNULL(@TargetSchoolID, 0) <> @SchoolID
    BEGIN
        RAISERROR('Provided SchoolID does not match the selected branch.', 16, 1);
        RETURN;
    END;

    SELECT TOP 1
        @PreviousSessionID = SM.SessionID,
        @PreviousSessionStartDate = SM.SessionStartDate,
        @PreviousSessionEndDate = SM.SessionEndDate,
        @PreviousSessionName = SM.SessionName
    FROM SessionMaster SM
    WHERE SM.SBranchID = @SBranchID
      AND SM.SessionEndDate < @TargetSessionStartDate
    ORDER BY SM.SessionEndDate DESC, SM.SessionID DESC;

    IF @PreviousSessionID IS NULL
    BEGIN
        RAISERROR('Previous session could not be resolved for the selected target session.', 16, 1);
        RETURN;
    END;

    SET @CarryForwardMonth = MONTH(@TargetSessionStartDate);

    EXEC dbo.sp_GetSessionCarryForwardFeeTypeID
        @SBranchID = @SBranchID,
        @FeeTypeID = @CarryForwardFeeTypeID OUTPUT;

    IF @CarryForwardFeeTypeID IS NULL AND @PreviewOnly = 0
    BEGIN
        EXEC dbo.sp_EnsureSessionCarryForwardFeeType
            @SBranchID = @SBranchID,
            @CreatedBy = @SBranchID,
            @FeeTypeID = @CarryForwardFeeTypeID OUTPUT;
    END;

    CREATE TABLE #TargetStudents
    (
        StudentID int PRIMARY KEY,
        Name nvarchar(100),
        RollNo nvarchar(100),
        TargetStudentSessionUID int,
        TargetClassID int,
        TargetSectionID int,
        TargetIsCustomFee int,
        PreviousStudentSessionUID int,
        PreviousClassID int,
        PreviousSectionID int
    );

    ;WITH TargetStudentRows AS
    (
        SELECT
            TSS.StudentID,
            SM.Name,
            TSS.RollNo,
            TSS.StudentSessionUID,
            TSS.ClassID,
            TSS.SectionID,
            ISNULL(TSS.IsCustomFee, 0) AS TargetIsCustomFee,
            ROW_NUMBER() OVER
            (
                PARTITION BY TSS.StudentID
                ORDER BY TSS.StudentSessionUID DESC
            ) AS RN
        FROM Student_Session TSS
        INNER JOIN StudentMaster SM
            ON SM.StudentID = TSS.StudentID
        WHERE TSS.SessionID = @TargetSessionID
          AND TSS.SBranchID = @SBranchID
          AND TSS.Status = 1
          AND (@ClassID = 0 OR TSS.ClassID = @ClassID)
          AND (@SectionID = 0 OR TSS.SectionID = @SectionID)
          AND (@StudentID = 0 OR TSS.StudentID = @StudentID)
    )
    INSERT INTO #TargetStudents
    (
        StudentID,
        Name,
        RollNo,
        TargetStudentSessionUID,
        TargetClassID,
        TargetSectionID,
        TargetIsCustomFee,
        PreviousStudentSessionUID,
        PreviousClassID,
        PreviousSectionID
    )
    SELECT
        TSR.StudentID,
        TSR.Name,
        TSR.RollNo,
        TSR.StudentSessionUID,
        TSR.ClassID,
        TSR.SectionID,
        TSR.TargetIsCustomFee,
        PSS.StudentSessionUID,
        PSS.ClassID,
        PSS.SectionID
    FROM TargetStudentRows TSR
    OUTER APPLY
    (
        SELECT TOP 1
            PS.StudentSessionUID,
            PS.ClassID,
            PS.SectionID
        FROM Student_Session PS
        WHERE PS.StudentID = TSR.StudentID
          AND PS.SessionID = @PreviousSessionID
        ORDER BY
            CASE WHEN PS.Status = 1 THEN 0 ELSE 1 END,
            PS.StudentSessionUID DESC
    ) PSS
    WHERE TSR.RN = 1;

    CREATE TABLE #FeeSummary
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

    CREATE TABLE #ApplyResult
    (
        StudentID int,
        Name nvarchar(100),
        RollNo nvarchar(100),
        TargetStudentSessionUID int,
        CarryForwardFeeTypeID int,
        CarryForwardMonth int,
        CarryForwardAmount numeric(10,2),
        PreviousSessionFeeAmount numeric(10,2),
        PreviousSessionPreviousDue numeric(10,2),
        PreviousSessionLateFee numeric(10,2),
        PreviousSessionDiscounts numeric(10,2),
        PreviousSessionPaid numeric(10,2),
        ActionStatus nvarchar(50),
        ActionRemarks nvarchar(500)
    );

    DECLARE
        @LoopStudentID int,
        @LoopName nvarchar(100),
        @LoopRollNo nvarchar(100),
        @LoopTargetStudentSessionUID int,
        @LoopTargetClassID int,
        @LoopTargetSectionID int,
        @LoopPreviousStudentSessionUID int,
        @LoopPreviousClassID int,
        @LoopPreviousSectionID int,
        @LoopCarryForwardAmount numeric(10,2),
        @LoopPreviousSessionFeeAmount numeric(10,2),
        @LoopPreviousSessionPreviousDue numeric(10,2),
        @LoopPreviousSessionLateFee numeric(10,2),
        @LoopPreviousSessionDiscounts numeric(10,2),
        @LoopPreviousSessionPaid numeric(10,2),
        @LoopExistingCarryForwardAmount numeric(10,2),
        @LoopHasCarryForwardPayment int,
        @LoopActionStatus nvarchar(50),
        @LoopActionRemarks nvarchar(500);

    DECLARE cur_Apply CURSOR LOCAL FAST_FORWARD FOR
    SELECT
        TS.StudentID,
        TS.Name,
        TS.RollNo,
        TS.TargetStudentSessionUID,
        TS.TargetClassID,
        TS.TargetSectionID,
        TS.PreviousStudentSessionUID,
        TS.PreviousClassID,
        TS.PreviousSectionID
    FROM #TargetStudents TS
    ORDER BY TS.StudentID;

    OPEN cur_Apply;

    FETCH NEXT FROM cur_Apply
    INTO
        @LoopStudentID,
        @LoopName,
        @LoopRollNo,
        @LoopTargetStudentSessionUID,
        @LoopTargetClassID,
        @LoopTargetSectionID,
        @LoopPreviousStudentSessionUID,
        @LoopPreviousClassID,
        @LoopPreviousSectionID;

    IF @PreviewOnly = 0
    BEGIN
        BEGIN TRANSACTION;
    END;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        DELETE FROM #FeeSummary;

        SET @LoopCarryForwardAmount = 0;
        SET @LoopPreviousSessionFeeAmount = 0;
        SET @LoopPreviousSessionPreviousDue = 0;
        SET @LoopPreviousSessionLateFee = 0;
        SET @LoopPreviousSessionDiscounts = 0;
        SET @LoopPreviousSessionPaid = 0;
        SET @LoopExistingCarryForwardAmount = 0;
        SET @LoopHasCarryForwardPayment = 0;
        SET @LoopActionStatus = N'APPLY';
        SET @LoopActionRemarks = N'Carry-forward is ready to be generated.';

        IF ISNULL(@LoopPreviousStudentSessionUID, 0) = 0
        BEGIN
            SET @LoopActionStatus = N'SKIP';
            SET @LoopActionRemarks = N'Previous session record not found for target student.';
        END
        ELSE
        BEGIN
            INSERT INTO #FeeSummary
            EXEC dbo.sp_GetClassGroupFeeListOnly
                @ClassID = @LoopPreviousClassID,
                @SectionID = @LoopPreviousSectionID,
                @SBranchID = @SBranchID,
                @SessionID = @PreviousSessionID,
                @QDate = @PreviousSessionEndDate,
                @CurDate = @PreviousSessionEndDate,
                @SStudentID = @LoopStudentID;

            SELECT TOP 1
                @LoopPreviousSessionFeeAmount = ISNULL(FS.FeeAmount, 0),
                @LoopPreviousSessionPreviousDue = ISNULL(FS.PreviousDue, 0),
                @LoopPreviousSessionLateFee = ISNULL(FS.LateFee, 0),
                @LoopPreviousSessionDiscounts = ISNULL(FS.Discounts, 0),
                @LoopPreviousSessionPaid = ISNULL(FS.Paid, 0)
            FROM #FeeSummary FS
            WHERE FS.StudentID = @LoopStudentID;

            SET @LoopCarryForwardAmount =
                ISNULL(@LoopPreviousSessionFeeAmount, 0)
                + ISNULL(@LoopPreviousSessionPreviousDue, 0)
                + ISNULL(@LoopPreviousSessionLateFee, 0)
                - ISNULL(@LoopPreviousSessionDiscounts, 0)
                - ISNULL(@LoopPreviousSessionPaid, 0);

            IF @LoopCarryForwardAmount < 0
            BEGIN
                SET @LoopCarryForwardAmount = 0;
            END;

            IF ISNULL(@CarryForwardFeeTypeID, 0) > 0
            BEGIN
                SELECT TOP 1
                    @LoopExistingCarryForwardAmount = ISNULL(SFD.FeeAmount, 0)
                FROM StudentFeeDetails SFD
                WHERE SFD.StudentID = @LoopStudentID
                  AND SFD.SessionID = @LoopTargetStudentSessionUID
                  AND SFD.FeeTypeID = @CarryForwardFeeTypeID;

                SELECT
                    @LoopHasCarryForwardPayment =
                        CASE WHEN EXISTS
                        (
                            SELECT 1
                            FROM PaymentDetails PD
                            INNER JOIN PaymentMaster PM
                                ON PM.PaymentID = PD.PaymentID
                            WHERE PD.PayeeID = @LoopStudentID
                              AND PD.FeeTypeID = @CarryForwardFeeTypeID
                              AND PM.SessionID = @TargetSessionID
                              AND ISNULL(PD.PaymentStatus, 0) = 0
                        )
                        THEN 1 ELSE 0 END;
            END;

            IF @LoopCarryForwardAmount <= 0
            BEGIN
                IF @SkipIfCarryForwardPaymentExists = 1 AND ISNULL(@LoopHasCarryForwardPayment, 0) = 1
                BEGIN
                    SET @LoopActionStatus = N'SKIP';
                    SET @LoopActionRemarks = N'Closing due is zero, but carry-forward payment already exists in target session.';
                END
                ELSE IF ISNULL(@LoopExistingCarryForwardAmount, 0) > 0 AND @ForceUpdate = 1 AND @PreviewOnly = 0
                BEGIN
                    UPDATE StudentFeeDetails
                    SET
                        FeeAmount = 0,
                        IsApplicable = 0
                    WHERE StudentID = @LoopStudentID
                      AND SessionID = @LoopTargetStudentSessionUID
                      AND FeeTypeID = @CarryForwardFeeTypeID;

                    UPDATE dbo.SessionCarryForwardMap
                    SET
                        GeneratedAmount = 0,
                        ModifiedDate = GETDATE(),
                        RunBy = ISNULL(@RunBy, N'')
                    WHERE SBranchID = @SBranchID
                      AND StudentID = @LoopStudentID
                      AND PreviousSessionID = @PreviousSessionID
                      AND TargetSessionID = @TargetSessionID
                      AND Status = 1;

                    SET @LoopActionStatus = N'CLEAR';
                    SET @LoopActionRemarks = N'Applied. Stale carry-forward row was disabled because previous session closing due is zero.';
                END
                ELSE IF ISNULL(@LoopExistingCarryForwardAmount, 0) > 0 AND @ForceUpdate = 1
                BEGIN
                    SET @LoopActionStatus = N'CLEAR';
                    SET @LoopActionRemarks = N'Preview only. Existing carry-forward row will be disabled because previous session closing due is zero.';
                END
                ELSE
                BEGIN
                    SET @LoopActionStatus = N'SKIP';
                    SET @LoopActionRemarks = N'No closing due found in the previous session.';
                END
            END
            ELSE IF @SkipIfCarryForwardPaymentExists = 1 AND ISNULL(@LoopHasCarryForwardPayment, 0) = 1
            BEGIN
                SET @LoopActionStatus = N'SKIP';
                SET @LoopActionRemarks = N'Payment already exists against carry-forward fee in target session.';
            END
            ELSE IF @ForceUpdate = 0 AND ISNULL(@LoopExistingCarryForwardAmount, 0) > 0
            BEGIN
                SET @LoopActionStatus = N'SKIP';
                SET @LoopActionRemarks = N'Carry-forward row already exists. Use ForceUpdate after review if overwrite is required.';
            END
            ELSE IF @PreviewOnly = 0
            BEGIN
                MERGE StudentFeeDetails AS T
                USING
                (
                    SELECT
                        @LoopStudentID AS StudentID,
                        @CarryForwardFeeTypeID AS FeeTypeID,
                        @LoopTargetStudentSessionUID AS SessionID,
                        @LoopCarryForwardAmount AS FeeAmount,
                        1 AS IsApplicable
                ) AS S
                ON T.StudentID = S.StudentID
               AND T.FeeTypeID = S.FeeTypeID
               AND T.SessionID = S.SessionID
                WHEN MATCHED THEN
                    UPDATE SET
                        T.FeeAmount = S.FeeAmount,
                        T.IsApplicable = S.IsApplicable
                WHEN NOT MATCHED THEN
                    INSERT
                    (
                        StudentID,
                        FeeTypeID,
                        SessionID,
                        FeeAmount,
                        IsApplicable
                    )
                    VALUES
                    (
                        S.StudentID,
                        S.FeeTypeID,
                        S.SessionID,
                        S.FeeAmount,
                        S.IsApplicable
                    );

                MERGE dbo.SessionCarryForwardMap AS T
                USING
                (
                    SELECT
                        @SBranchID AS SBranchID,
                        ISNULL(@TargetSchoolID, 0) AS SchoolID,
                        @LoopStudentID AS StudentID,
                        @PreviousSessionID AS PreviousSessionID,
                        @TargetSessionID AS TargetSessionID,
                        @LoopTargetStudentSessionUID AS TargetStudentSessionUID,
                        @CarryForwardFeeTypeID AS CarryForwardFeeTypeID,
                        @LoopCarryForwardAmount AS GeneratedAmount,
                        ISNULL(@RunBy, N'') AS RunBy
                ) AS S
                ON T.SBranchID = S.SBranchID
               AND T.StudentID = S.StudentID
               AND T.PreviousSessionID = S.PreviousSessionID
               AND T.TargetSessionID = S.TargetSessionID
               AND T.Status = 1
                WHEN MATCHED THEN
                    UPDATE SET
                        T.TargetStudentSessionUID = S.TargetStudentSessionUID,
                        T.CarryForwardFeeTypeID = S.CarryForwardFeeTypeID,
                        T.GeneratedAmount = S.GeneratedAmount,
                        T.RunBy = S.RunBy,
                        T.ModifiedDate = GETDATE()
                WHEN NOT MATCHED THEN
                    INSERT
                    (
                        SBranchID,
                        SchoolID,
                        StudentID,
                        PreviousSessionID,
                        TargetSessionID,
                        TargetStudentSessionUID,
                        CarryForwardFeeTypeID,
                        GeneratedAmount,
                        Status,
                        RunBy
                    )
                    VALUES
                    (
                        S.SBranchID,
                        S.SchoolID,
                        S.StudentID,
                        S.PreviousSessionID,
                        S.TargetSessionID,
                        S.TargetStudentSessionUID,
                        S.CarryForwardFeeTypeID,
                        S.GeneratedAmount,
                        1,
                        S.RunBy
                    );

                SET @LoopActionRemarks = N'Applied. Session-Carry-forward fee was upserted and lock mapping was saved.';
            END
            ELSE
            BEGIN
                SET @LoopActionRemarks = N'Preview only. Carry-forward is ready to be applied.';
            END;
        END;

        INSERT INTO #ApplyResult
        (
            StudentID,
            Name,
            RollNo,
            TargetStudentSessionUID,
            CarryForwardFeeTypeID,
            CarryForwardMonth,
            CarryForwardAmount,
            PreviousSessionFeeAmount,
            PreviousSessionPreviousDue,
            PreviousSessionLateFee,
            PreviousSessionDiscounts,
            PreviousSessionPaid,
            ActionStatus,
            ActionRemarks
        )
        VALUES
        (
            @LoopStudentID,
            @LoopName,
            @LoopRollNo,
            @LoopTargetStudentSessionUID,
            ISNULL(@CarryForwardFeeTypeID, 0),
            @CarryForwardMonth,
            ISNULL(@LoopCarryForwardAmount, 0),
            ISNULL(@LoopPreviousSessionFeeAmount, 0),
            ISNULL(@LoopPreviousSessionPreviousDue, 0),
            ISNULL(@LoopPreviousSessionLateFee, 0),
            ISNULL(@LoopPreviousSessionDiscounts, 0),
            ISNULL(@LoopPreviousSessionPaid, 0),
            @LoopActionStatus,
            @LoopActionRemarks
        );

        FETCH NEXT FROM cur_Apply
        INTO
            @LoopStudentID,
            @LoopName,
            @LoopRollNo,
            @LoopTargetStudentSessionUID,
            @LoopTargetClassID,
            @LoopTargetSectionID,
            @LoopPreviousStudentSessionUID,
            @LoopPreviousClassID,
            @LoopPreviousSectionID;
    END;

    CLOSE cur_Apply;
    DEALLOCATE cur_Apply;

    IF @PreviewOnly = 0
    BEGIN
        COMMIT TRANSACTION;
    END;

    SELECT
        @SBranchID AS SBranchID,
        ISNULL(@TargetSchoolID, 0) AS SchoolID,
        @TargetSessionID AS TargetSessionID,
        ISNULL(@PreviousSessionID, 0) AS PreviousSessionID,
        ISNULL(@CarryForwardFeeTypeID, 0) AS CarryForwardFeeTypeID,
        @CarryForwardMonth AS CarryForwardMonth,
        ISNULL(@RunBy, N'') AS RunBy,
        CASE WHEN @PreviewOnly = 1 THEN N'PREVIEW' ELSE N'APPLY' END AS RunMode;

    SELECT
        AR.StudentID,
        AR.Name,
        AR.RollNo,
        AR.TargetStudentSessionUID,
        AR.CarryForwardFeeTypeID,
        AR.CarryForwardMonth,
        AR.CarryForwardAmount,
        AR.PreviousSessionFeeAmount,
        AR.PreviousSessionPreviousDue,
        AR.PreviousSessionLateFee,
        AR.PreviousSessionDiscounts,
        AR.PreviousSessionPaid,
        AR.ActionStatus,
        AR.ActionRemarks
    FROM #ApplyResult AR
    ORDER BY
        CASE AR.ActionStatus
            WHEN N'SKIP' THEN 1
            ELSE 0
        END,
        AR.CarryForwardAmount DESC,
        AR.StudentID;
END
GO

