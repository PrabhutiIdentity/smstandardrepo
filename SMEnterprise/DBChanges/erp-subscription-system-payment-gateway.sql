CREATE TABLE BranchSubscription
(
    SBranchID INT PRIMARY KEY,
    IsDue BIT NOT NULL DEFAULT(0),
    DueAmount DECIMAL(18,2) NULL,
    DueDate DATETIME NULL,
    PlanName NVARCHAR(200) NULL,
    LastPaidDate DATETIME NULL,
    LastPaymentRef NVARCHAR(200) NULL,
    NextDueDate DATETIME NULL,
    NextDueAmount DECIMAL(18,2) NULL,
    PartialCycleDays INT NULL,
    GraceDays INT NOT NULL DEFAULT(0),
    AllowPartialPayment BIT NOT NULL DEFAULT(0),
    MaxPartialPayments INT NOT NULL DEFAULT(2),
    PartialPaymentCount INT NOT NULL DEFAULT(0);
);
go
CREATE TABLE BranchSubscriptionPayment
(
    PaymentID INT IDENTITY(1,1) PRIMARY KEY,
    SBranchID INT NOT NULL,
    PaidAmount DECIMAL(18,2) NOT NULL,
    PaymentRef NVARCHAR(200) NOT NULL,
    PaymentDate DATETIME NOT NULL,
    CreatedBy INT NULL
);
go



IF OBJECT_ID('dbo.SystemPaymentGateway', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemPaymentGateway
    (
        GatewayID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SystemPaymentGateway PRIMARY KEY,
        GatewayName NVARCHAR(50) NOT NULL,
        RazorpayKeyId NVARCHAR(200) NULL,
        RazorpaySecret NVARCHAR(200) NULL,
        UseForSubscription BIT NOT NULL CONSTRAINT DF_SystemPaymentGateway_UseForSubscription DEFAULT(1),
        IsActive BIT NOT NULL CONSTRAINT DF_SystemPaymentGateway_IsActive DEFAULT(1),
        CreatedDate DATETIME NULL,
        UpdatedDate DATETIME NULL
    );
END
GO
CREATE OR ALTER PROCEDURE dbo.sp_GetBranchSubscription
    @SBranchID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SBranchID,
        IsDue,
        ISNULL(DueAmount, 0) AS DueAmount,
        DueDate,
        PlanName,
        LastPaidDate,
        LastPaymentRef,
        ISNULL(GraceDays, 0) AS GraceDays,
        ISNULL(AllowPartialPayment, 0) AS AllowPartialPayment,
        ISNULL(MaxPartialPayments, 2) AS MaxPartialPayments,
        ISNULL(PartialPaymentCount, 0) AS PartialPaymentCount,
        NextDueDate,
        ISNULL(NextDueAmount, 0) AS NextDueAmount,
        ISNULL(PartialCycleDays, 0) AS PartialCycleDays
    FROM dbo.BranchSubscription
    WHERE SBranchID = @SBranchID;
END
GO

Go

CREATE OR ALTER PROCEDURE sp_MarkBranchSubscriptionPaid
    @SBranchID INT,
    @PaidAmount DECIMAL(18,2),
    @PaymentRef NVARCHAR(200),
    @PaymentDate DATETIME,
    @CreatedBy INT
AS
BEGIN
    BEGIN TRANSACTION;

    -- Update subscription master
    UPDATE BranchSubscription
    SET IsDue = 0,
        DueAmount = 0,
        LastPaidDate = @PaymentDate,
        LastPaymentRef = @PaymentRef
    WHERE SBranchID = @SBranchID;

    -- Insert into audit/payment history
    INSERT INTO BranchSubscriptionPayment (SBranchID, PaidAmount, PaymentRef, PaymentDate, CreatedBy)
    VALUES (@SBranchID, @PaidAmount, @PaymentRef, @PaymentDate, @CreatedBy);

    COMMIT TRANSACTION;
END

go

CREATE OR ALTER PROCEDURE dbo.sp_GetSystemPaymentGateway
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        GatewayID,
        GatewayName,
        RazorpayKeyId,
        RazorpaySecret,
        UseForSubscription,
        IsActive,
        CreatedDate,
        UpdatedDate
    FROM dbo.SystemPaymentGateway
    WHERE IsActive = 1
      AND UseForSubscription = 1
    ORDER BY GatewayID DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpsertSystemPaymentGateway
    @GatewayID INT = 0,
    @GatewayName NVARCHAR(50),
    @RazorpayKeyId NVARCHAR(200),
    @RazorpaySecret NVARCHAR(200),
    @UseForSubscription BIT,
    @IsActive BIT,
    @CreatedDate DATETIME,
    @UpdatedDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@GatewayID, 0) > 0
       AND EXISTS (SELECT 1 FROM dbo.SystemPaymentGateway WHERE GatewayID = @GatewayID)
    BEGIN
        UPDATE dbo.SystemPaymentGateway
        SET GatewayName = @GatewayName,
            RazorpayKeyId = @RazorpayKeyId,
            RazorpaySecret = @RazorpaySecret,
            UseForSubscription = @UseForSubscription,
            IsActive = @IsActive,
            UpdatedDate = @UpdatedDate
        WHERE GatewayID = @GatewayID;

        SELECT @GatewayID;
        RETURN;
    END

    INSERT INTO dbo.SystemPaymentGateway
    (
        GatewayName,
        RazorpayKeyId,
        RazorpaySecret,
        UseForSubscription,
        IsActive,
        CreatedDate,
        UpdatedDate
    )
    VALUES
    (
        @GatewayName,
        @RazorpayKeyId,
        @RazorpaySecret,
        @UseForSubscription,
        @IsActive,
        @CreatedDate,
        @UpdatedDate
    );

    SELECT CONVERT(INT, SCOPE_IDENTITY());
END
GO

-- Add your ERP provider Razorpay credentials before enabling live subscription payments.
-- Example:
-- EXEC dbo.sp_UpsertSystemPaymentGateway
--     @GatewayID = 0,
--     @GatewayName = 'Razorpay',
--     @RazorpayKeyId = 'rzp_live_xxxxx',
--     @RazorpaySecret = 'xxxxx',
--     @UseForSubscription = 1,
--     @IsActive = 1,
--     @CreatedDate = GETDATE(),
--     @UpdatedDate = GETDATE();
-- GO
