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
