CREATE OR ALTER PROCEDURE dbo.sp_GetBranchSubscriptionAccount
    @SBranchID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        bs.SBranchID,
        COALESCE(NULLIF(sb.BranchSchoolName, ''), sb.BranchName) AS BranchName,
        sb.Address AS BranchAddress,
        bs.IsDue,
        ISNULL(bs.DueAmount, 0) AS DueAmount,
        bs.DueDate,
        bs.PlanName,
        bs.LastPaidDate,
        bs.LastPaymentRef,
        ISNULL(bs.GraceDays, 0) AS GraceDays,
        ISNULL(bs.AllowPartialPayment, 0) AS AllowPartialPayment,
        ISNULL(bs.MaxPartialPayments, 2) AS MaxPartialPayments,
        ISNULL(bs.PartialPaymentCount, 0) AS PartialPaymentCount,
        bs.NextDueDate,
        ISNULL(bs.NextDueAmount, 0) AS NextDueAmount,
        ISNULL(bs.PartialCycleDays, 0) AS PartialCycleDays
    FROM dbo.BranchSubscription bs
    LEFT JOIN dbo.SBranchMaster sb ON sb.SBranchID = bs.SBranchID
    WHERE bs.SBranchID = @SBranchID;

    SELECT
        PaymentID,
        SBranchID,
        PaidAmount,
        PaymentRef,
        PaymentDate,
        CreatedBy
    FROM dbo.BranchSubscriptionPayment
    WHERE SBranchID = @SBranchID
    ORDER BY PaymentDate DESC, PaymentID DESC;
END
GO
