IF COL_LENGTH('dbo.BranchSubscription', 'GraceDays') IS NULL
BEGIN
    ALTER TABLE dbo.BranchSubscription
    ADD GraceDays INT NOT NULL CONSTRAINT DF_BranchSubscription_GraceDays DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.BranchSubscription', 'AllowPartialPayment') IS NULL
BEGIN
    ALTER TABLE dbo.BranchSubscription
    ADD AllowPartialPayment BIT NOT NULL CONSTRAINT DF_BranchSubscription_AllowPartialPayment DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.BranchSubscription', 'MaxPartialPayments') IS NULL
BEGIN
    ALTER TABLE dbo.BranchSubscription
    ADD MaxPartialPayments INT NOT NULL CONSTRAINT DF_BranchSubscription_MaxPartialPayments DEFAULT (2);
END
GO

IF COL_LENGTH('dbo.BranchSubscription', 'PartialPaymentCount') IS NULL
BEGIN
    ALTER TABLE dbo.BranchSubscription
    ADD PartialPaymentCount INT NOT NULL CONSTRAINT DF_BranchSubscription_PartialPaymentCount DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.BranchSubscription', 'NextDueDate') IS NULL
BEGIN
    ALTER TABLE dbo.BranchSubscription
    ADD NextDueDate DATETIME NULL;
END
GO

IF COL_LENGTH('dbo.BranchSubscription', 'NextDueAmount') IS NULL
BEGIN
    ALTER TABLE dbo.BranchSubscription
    ADD NextDueAmount DECIMAL(18,2) NULL;
END
GO

IF COL_LENGTH('dbo.BranchSubscription', 'PartialCycleDays') IS NULL
BEGIN
    ALTER TABLE dbo.BranchSubscription
    ADD PartialCycleDays INT NULL;
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

-- Example branch config for branch 34
-- UPDATE dbo.BranchSubscription
-- SET GraceDays = 10,
--     AllowPartialPayment = 1,
--     MaxPartialPayments = 2,
--     PartialCycleDays = 45,
--     NextDueDate = NULL,
--     NextDueAmount = NULL,
--     PartialPaymentCount = 0
-- WHERE SBranchID = 34;
