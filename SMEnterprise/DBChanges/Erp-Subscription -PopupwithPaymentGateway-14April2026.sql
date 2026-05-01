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

CREATE TABLE BranchPaymentGateway
(
    SBranchID INT PRIMARY KEY,
    RazorpayKeyId NVARCHAR(200) NULL,
    RazorpaySecret NVARCHAR(200) NULL,
    UseForSubscription BIT NOT NULL CONSTRAINT DF_BranchPaymentGateway_UseForSubscription DEFAULT(1),
    CreatedDate DATETIME NULL,
    UpdatedDate DATETIME NULL
);
go
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

CREATE PROCEDURE sp_MarkBranchSubscriptionPaid
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
CREATE PROCEDURE sp_GetBranchGateway
    @SBranchID INT
AS
BEGIN
    SELECT SBranchID, RazorpayKeyId, RazorpaySecret, UseForSubscription, CreatedDate, UpdatedDate
    FROM dbo.BranchPaymentGateway
    WHERE SBranchID = @SBranchID;
END

Go

CREATE PROCEDURE sp_UpsertBranchGateway
    @SBranchID INT,
    @RazorpayKeyId NVARCHAR(200),
    @RazorpaySecret NVARCHAR(200),
    @UseForSubscription BIT,
    @CreatedDate DATETIME,
    @UpdatedDate DATETIME
AS
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.BranchPaymentGateway WHERE SBranchID = @SBranchID)
    BEGIN
        UPDATE dbo.BranchPaymentGateway
        SET RazorpayKeyId = @RazorpayKeyId,
            RazorpaySecret = @RazorpaySecret,
            UseForSubscription = @UseForSubscription,
            UpdatedDate = @UpdatedDate
        WHERE SBranchID = @SBranchID;
        SELECT 1;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.BranchPaymentGateway (SBranchID, RazorpayKeyId, RazorpaySecret, UseForSubscription, CreatedDate, UpdatedDate)
        VALUES (@SBranchID, @RazorpayKeyId, @RazorpaySecret, @UseForSubscription, @CreatedDate, @UpdatedDate);
        SELECT 1;
    END
END
go


--INSERT INTO BranchSubscription (SBranchID, IsDue, DueAmount, DueDate, PlanName, LastPaidDate, LastPaymentRef)
--VALUES 
--(34, 1, 50.00, '2026-04-01', 'Sept 2024 to Aug 2027', '2024-05-15', '');
--go

INSERT INTO BranchPaymentGateway (SBranchID, RazorpayKeyId, RazorpaySecret, UseForSubscription, CreatedDate, UpdatedDate)
VALUES (34, 'rzp_test_SVjpRhPX8rg8eE', 'naE50maGmFhq8XFTUF3fHDzE', 1, GETDATE(), GETDATE());