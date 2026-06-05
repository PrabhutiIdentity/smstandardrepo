CREATE OR ALTER PROCEDURE usp_CancelStockTransactionPayment
    @StockPaymentID INT,
    @SBranchID INT,
    @CancelRemark NVARCHAR(250),
    @CancelledBy INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @STID INT;
    DECLARE @Amount DECIMAL(18,2);
    DECLARE @ActivePaidAmount DECIMAL(18,2);
    DECLARE @DueAmount DECIMAL(18,2);
    DECLARE @PaymentStatus INT;
    DECLARE @PaymentMode INT;
    DECLARE @PaymentReferanceNo NVARCHAR(100);
    DECLARE @PaymentDate DATETIME;
    DECLARE @LatestActivePaymentID INT;

    SELECT @STID = STID
    FROM dbo.StockTransactionPaymentHistory
    WHERE StockPaymentID = @StockPaymentID
      AND SBranchID = @SBranchID;

    IF ISNULL(@STID, 0) = 0
    BEGIN
        SELECT 0;
        RETURN;
    END

    SELECT TOP 1 @LatestActivePaymentID = StockPaymentID
    FROM dbo.StockTransactionPaymentHistory
    WHERE STID = @STID
      AND SBranchID = @SBranchID
      AND ISNULL(IsCancelled, 0) = 0
    ORDER BY PaymentDate DESC, StockPaymentID DESC;

    IF ISNULL(@LatestActivePaymentID, 0) <> @StockPaymentID
    BEGIN
        SELECT -2;
        RETURN;
    END

    UPDATE dbo.StockTransactionPaymentHistory
    SET IsCancelled = 1,
        CancelledDate = GETDATE(),
        CancelRemark = ISNULL(@CancelRemark, ''),
        CancelledBy = @CancelledBy,
        PaymentStatus = 3
    WHERE StockPaymentID = @StockPaymentID
      AND SBranchID = @SBranchID
      AND ISNULL(IsCancelled, 0) = 0;

    SELECT @Amount = ISNULL(SUM((STD.Quantity * STD.Cost) + (STD.Quantity * STD.Cost * (STD.SGST + STD.CGST + STD.IGST) / 100.0)), 0)
    FROM dbo.StockTransactionDetails STD
    WHERE STD.STID = @STID;

    SELECT @ActivePaidAmount = ISNULL(SUM(PaymentAmount), 0)
    FROM dbo.StockTransactionPaymentHistory
    WHERE STID = @STID
      AND SBranchID = @SBranchID
      AND ISNULL(IsCancelled, 0) = 0;

    SET @DueAmount = CASE WHEN @Amount > @ActivePaidAmount THEN @Amount - @ActivePaidAmount ELSE 0 END;
    SET @PaymentStatus = CASE
            WHEN @ActivePaidAmount <= 0 THEN 0
            WHEN @DueAmount > 0 THEN 1
            ELSE 2
        END;

    SELECT TOP 1
        @PaymentMode = PaymentMode,
        @PaymentReferanceNo = PaymentReferanceNo,
        @PaymentDate = PaymentDate
    FROM dbo.StockTransactionPaymentHistory
    WHERE STID = @STID
      AND SBranchID = @SBranchID
      AND ISNULL(IsCancelled, 0) = 0
    ORDER BY PaymentDate DESC, StockPaymentID DESC;

    UPDATE dbo.StockTransactionMaster
    SET Status = CASE WHEN @ActivePaidAmount <= 0 THEN 0 ELSE 1 END,
        PaidAmount = @ActivePaidAmount,
        DueAmount = @DueAmount,
        PaymentStatus = @PaymentStatus,
        PaymentMode = ISNULL(@PaymentMode, PaymentMode),
        PaymentReferanceNo = ISNULL(@PaymentReferanceNo, ''),
        PaymentDate = ISNULL(@PaymentDate, PaymentDate),
        CancelRemark = CASE
            WHEN @ActivePaidAmount <= 0 THEN ISNULL(@CancelRemark, '')
            ELSE ISNULL(CancelRemark, '')
        END
    WHERE STID = @STID
      AND SBranchID = @SBranchID;

    SELECT @STID;
END
GO

CREATE OR ALTER PROCEDURE sp_GetAvailableStockProducts
    @SBranchID INT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH ActiveStock AS
    (
        SELECT
            STD.ProductID,
            SUM(CASE
                    WHEN STM.TrType = 0 THEN STD.Quantity
                    WHEN STM.TrType = 1 THEN -STD.Quantity
                    ELSE 0
                END) AS AvailableQty
        FROM dbo.StockTransactionDetails STD
        INNER JOIN dbo.StockTransactionMaster STM ON STM.STID = STD.STID
        WHERE STM.SBranchID = @SBranchID
          AND ISNULL(STM.Status, 1) <> 0
        GROUP BY STD.ProductID
    )
    SELECT
        PM.ProductID,
        PM.ProductCategoryID,
        PM.Name,
        PM.Photo,
        PM.MRP,
        PM.Price,
        PM.SBranchID,
        PM.Status,
        ISNULL(PM.Quantity, 0) + ISNULL(ASQ.AvailableQty, 0) AS Quantity,
        ISNULL(PM.MinQty, 0) AS MinQty,
        ISNULL(PC.Name, '') AS ProductCategoryName,
        ISNULL(PC.SGST, 0) AS SGST,
        ISNULL(PC.CGST, 0) AS CGST,
        ISNULL(PC.IGST, 0) AS IGST,
        ISNULL(PM.Quantity, 0) + ISNULL(ASQ.AvailableQty, 0) AS Available
    FROM dbo.ProductMaster PM
    LEFT JOIN dbo.ProductCategories PC ON PC.ID = PM.ProductCategoryID
    LEFT JOIN ActiveStock ASQ ON ASQ.ProductID = PM.ProductID
    WHERE PM.SBranchID = @SBranchID
      AND ISNULL(PM.Status, 1) = 1
      AND ISNULL(PM.Quantity, 0) + ISNULL(ASQ.AvailableQty, 0) > 0
    ORDER BY PM.Name;
END
GO

CREATE OR ALTER PROCEDURE sp_GetLowStockProducts
    @SBranchID INT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH ActiveStock AS
    (
        SELECT
            STD.ProductID,
            SUM(CASE
                    WHEN STM.TrType = 0 THEN STD.Quantity
                    WHEN STM.TrType = 1 THEN -STD.Quantity
                    ELSE 0
                END) AS AvailableQty
        FROM dbo.StockTransactionDetails STD
        INNER JOIN dbo.StockTransactionMaster STM ON STM.STID = STD.STID
        WHERE STM.SBranchID = @SBranchID
          AND ISNULL(STM.Status, 1) <> 0
        GROUP BY STD.ProductID
    )
    SELECT
        PM.ProductID,
        PM.ProductCategoryID,
        PM.Name,
        PM.Photo,
        PM.MRP,
        PM.Price,
        PM.SBranchID,
        PM.Status,
        ISNULL(PM.Quantity, 0) + ISNULL(ASQ.AvailableQty, 0) AS Quantity,
        ISNULL(PM.MinQty, 0) AS MinQty,
        ISNULL(PC.Name, '') AS ProductCategoryName,
        ISNULL(PC.SGST, 0) AS SGST,
        ISNULL(PC.CGST, 0) AS CGST,
        ISNULL(PC.IGST, 0) AS IGST,
        ISNULL(PM.Quantity, 0) + ISNULL(ASQ.AvailableQty, 0) AS Available
    FROM dbo.ProductMaster PM
    LEFT JOIN dbo.ProductCategories PC ON PC.ID = PM.ProductCategoryID
    LEFT JOIN ActiveStock ASQ ON ASQ.ProductID = PM.ProductID
    WHERE PM.SBranchID = @SBranchID
      AND ISNULL(PM.Status, 1) = 1
      AND ISNULL(PM.Quantity, 0) + ISNULL(ASQ.AvailableQty, 0) <= ISNULL(PM.MinQty, 0)
    ORDER BY PM.Name;
END
GO
