CREATE OR ALTER PROCEDURE dbo.sp_MarkBranchSubscriptionPaid
    @SBranchID INT,
    @PaidAmount DECIMAL(18,2),
    @PaymentRef NVARCHAR(200),
    @PaymentDate DATETIME,
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    DECLARE
        @DueAmount DECIMAL(18,2),
        @AllowPartialPayment BIT,
        @MaxPartialPayments INT,
        @PartialPaymentCount INT,
        @RemainingPartialPayments INT,
        @PartialCycleDays INT,
        @RemainingAmount DECIMAL(18,2),
        @MinimumPartialAmount DECIMAL(18,2);

    SELECT
        @DueAmount = CASE
            WHEN ISNULL(NextDueAmount, 0) > 0 THEN NextDueAmount
            ELSE ISNULL(DueAmount, 0)
        END,
        @AllowPartialPayment = ISNULL(AllowPartialPayment, 0),
        @MaxPartialPayments = ISNULL(MaxPartialPayments, 0),
        @PartialPaymentCount = ISNULL(PartialPaymentCount, 0),
        @PartialCycleDays = ISNULL(PartialCycleDays, 0)
    FROM dbo.BranchSubscription WITH (UPDLOCK, ROWLOCK)
    WHERE SBranchID = @SBranchID;

    IF @DueAmount IS NULL OR @DueAmount <= 0
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 0;
        RETURN;
    END

    SET @RemainingPartialPayments = @MaxPartialPayments - @PartialPaymentCount;
    IF @RemainingPartialPayments <= 1
    BEGIN
        SET @MinimumPartialAmount = @DueAmount;
    END
    ELSE
    BEGIN
        SET @MinimumPartialAmount = ROUND(@DueAmount / @RemainingPartialPayments, 2);
    END

    IF @PaidAmount >= @DueAmount
    BEGIN
        UPDATE dbo.BranchSubscription
        SET IsDue = 0,
            DueAmount = 0,
            NextDueAmount = NULL,
            NextDueDate = NULL,
            PartialPaymentCount = 0,
            LastPaidDate = @PaymentDate,
            LastPaymentRef = @PaymentRef
        WHERE SBranchID = @SBranchID;
    END
    ELSE
    BEGIN
        IF @AllowPartialPayment = 0
           OR @RemainingPartialPayments <= 1
           OR @PaidAmount < @MinimumPartialAmount
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT -1;
            RETURN;
        END

        SET @RemainingAmount = @DueAmount - @PaidAmount;

        UPDATE dbo.BranchSubscription
        SET IsDue = 1,
            DueAmount = @RemainingAmount,
            NextDueAmount = @RemainingAmount,
            NextDueDate = DATEADD(DAY, @PartialCycleDays, @PaymentDate),
            PartialPaymentCount = @PartialPaymentCount + 1,
            LastPaidDate = @PaymentDate,
            LastPaymentRef = @PaymentRef
        WHERE SBranchID = @SBranchID;
    END

    INSERT INTO dbo.BranchSubscriptionPayment
    (
        SBranchID,
        PaidAmount,
        PaymentRef,
        PaymentDate,
        CreatedBy
    )
    VALUES
    (
        @SBranchID,
        @PaidAmount,
        @PaymentRef,
        @PaymentDate,
        @CreatedBy
    );

    COMMIT TRANSACTION;
    SELECT 1;
END
GO
