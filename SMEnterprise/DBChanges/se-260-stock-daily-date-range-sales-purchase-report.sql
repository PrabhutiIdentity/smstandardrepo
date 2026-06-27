CREATE OR ALTER PROCEDURE sp_GetStockTransactions
    @StartDate DATE,
    @EndDate DATE,
    @TrType INT,
    @SBranchID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        STM.STID,
        STM.TrDate,
        STM.TrType,
        STM.RefID,
        STM.RefType,
        ISNULL(STM.Remark, '') AS Remark,
        STM.Status,
        ISNULL(STM.PaymentDate, STM.TrDate) AS PaymentDate,
        ISNULL(STM.PaymentMode, 0) AS PaymentMode,
        ISNULL(STM.PaymentReferanceNo, '') AS PaymentReferanceNo,
        ISNULL(STM.PaidAmount, 0) AS PaidAmount,
        ISNULL(STM.DueAmount, 0) AS DueAmount,
        ISNULL(STM.PaymentStatus, 0) AS PaymentStatus,
        ISNULL(STM.CancelRemark, '') AS CancelRemark,
        ISNULL(STM.InvoiceNumber, CAST(STM.STID AS VARCHAR(20))) AS InvoiceNumber,
        CASE
            WHEN STM.TrType = 0 THEN ISNULL(VM.CompanyName, '')
            WHEN STM.RefType = 0 THEN ISNULL(SM.Name, '')
            ELSE ISNULL(EM.EmployeeName, '')
        END AS RefName,
        ISNULL(PN.ProductNames, '') AS ProductNames,
        ISNULL(Q.Quantity, 0) AS Quantity,
        ISNULL(Q.Amount, 0) AS Amount
    FROM dbo.StockTransactionMaster STM
    LEFT JOIN dbo.VendorMaster VM ON VM.VendorID = STM.VendorID
    LEFT JOIN dbo.StudentMaster SM ON SM.StudentID = STM.RefID AND STM.RefType = 0
    LEFT JOIN dbo.EmployeeMaster EM ON EM.EmployeeID = STM.RefID AND STM.RefType = 1
    OUTER APPLY
    (
        SELECT STUFF((
            SELECT ', ' + PM.Name
            FROM dbo.StockTransactionDetails STD2
            INNER JOIN dbo.ProductMaster PM ON PM.ProductID = STD2.ProductID
            WHERE STD2.STID = STM.STID
            GROUP BY PM.Name
            FOR XML PATH(''), TYPE
        ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS ProductNames
    ) PN
    OUTER APPLY
    (
        SELECT
            SUM(ISNULL(STD.Quantity, 0)) AS Quantity,
            SUM((ISNULL(STD.Quantity, 0) * ISNULL(STD.Cost, 0))
                + (ISNULL(STD.Quantity, 0) * ISNULL(STD.Cost, 0) * (ISNULL(STD.SGST, 0) + ISNULL(STD.CGST, 0) + ISNULL(STD.IGST, 0)) / 100.0)) AS Amount
        FROM dbo.StockTransactionDetails STD
        WHERE STD.STID = STM.STID
    ) Q
    WHERE STM.SBranchID = @SBranchID
      AND CAST(STM.TrDate AS DATE) BETWEEN @StartDate AND @EndDate
      AND
      (
          (@TrType = -1 AND STM.TrType IN (0, 1))
          OR (@TrType <> -1 AND STM.TrType = @TrType)
      )
    ORDER BY STM.TrDate DESC, STM.STID DESC;

    SELECT *
    FROM dbo.SBranchMaster
    WHERE SBranchID = @SBranchID;

    SELECT P.StockPaymentID, P.STID
    FROM dbo.StockTransactionPaymentHistory P
    INNER JOIN
    (
        SELECT STID, MAX(StockPaymentID) AS StockPaymentID
        FROM dbo.StockTransactionPaymentHistory
        WHERE SBranchID = @SBranchID
        GROUP BY STID
    ) LP ON LP.StockPaymentID = P.StockPaymentID;
END
GO
