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
go
IF OBJECT_ID('dbo.StockTransactionPaymentHistory', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockTransactionPaymentHistory
    (
        StockPaymentID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        STID INT NOT NULL,
        ReceiptNumber NVARCHAR(50) NULL,
        PaymentDate DATETIME NOT NULL,
        PaymentMode INT NOT NULL,
        PaymentReferanceNo NVARCHAR(100) NULL,
        PaymentAmount DECIMAL(18,2) NOT NULL,
        TotalPaidAmount DECIMAL(18,2) NOT NULL,
        DueAmount DECIMAL(18,2) NOT NULL,
        PaymentStatus INT NOT NULL,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_StockTransactionPaymentHistory_CreatedDate DEFAULT (GETDATE()),
        SBranchID INT NOT NULL,
        CreatedBy INT NULL
    );

    CREATE INDEX IX_StockTransactionPaymentHistory_STID ON dbo.StockTransactionPaymentHistory(STID, SBranchID, StockPaymentID DESC);
END
GO

IF COL_LENGTH('dbo.StockTransactionPaymentHistory', 'IsCancelled') IS NULL
BEGIN
    ALTER TABLE dbo.StockTransactionPaymentHistory
    ADD IsCancelled BIT NOT NULL CONSTRAINT DF_StockTransactionPaymentHistory_IsCancelled DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.StockTransactionPaymentHistory', 'CancelledDate') IS NULL
BEGIN
    ALTER TABLE dbo.StockTransactionPaymentHistory
    ADD CancelledDate DATETIME NULL;
END
GO

IF COL_LENGTH('dbo.StockTransactionPaymentHistory', 'CancelRemark') IS NULL
BEGIN
    ALTER TABLE dbo.StockTransactionPaymentHistory
    ADD CancelRemark NVARCHAR(250) NULL;
END
GO

IF COL_LENGTH('dbo.StockTransactionPaymentHistory', 'CancelledBy') IS NULL
BEGIN
    ALTER TABLE dbo.StockTransactionPaymentHistory
    ADD CancelledBy INT NULL;
END
GO

CREATE OR ALTER PROCEDURE usp_GetStockProductLookup
    @SBranchID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ProductID AS ID, Name  FROM dbo.ProductMaster
    WHERE SBranchID = @SBranchID
    ORDER BY Name;
END
go
CREATE OR ALTER PROCEDURE usp_GetStockPaymentHistory
    @STID INT,
    @SBranchID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        StockPaymentID, 
        STID, 
        ISNULL(ReceiptNumber, '') AS ReceiptNumber, 
        PaymentDate, 
        PaymentMode,
        ISNULL(PaymentReferanceNo, '') AS PaymentReferanceNo, 
        PaymentAmount,
        TotalPaidAmount, 
        DueAmount, 
        PaymentStatus, 
        CreatedDate, 
        SBranchID, 
        ISNULL(CreatedBy, 0) AS CreatedBy,
        ISNULL(IsCancelled, 0) AS IsCancelled,
        CancelledDate,
        ISNULL(CancelRemark, '') AS CancelRemark,
        ISNULL(CancelledBy, 0) AS CancelledBy
    FROM dbo.StockTransactionPaymentHistory
    WHERE STID = @STID 
      AND SBranchID = @SBranchID
    ORDER BY PaymentDate DESC, StockPaymentID DESC;
END

go
CREATE OR ALTER PROCEDURE usp_InsertStockTransactionPayment
    @STID INT,
    @PaymentDate DATETIME,
    @PaymentMode INT,
    @PaymentReferanceNo NVARCHAR(100),
    @PaymentAmount DECIMAL(18,2),
    @TotalPaidAmount DECIMAL(18,2),
    @DueAmount DECIMAL(18,2),
    @PaymentStatus INT,
    @SBranchID INT,
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @PaymentID INT;

    -- 1. Insert the record
    INSERT INTO dbo.StockTransactionPaymentHistory
    (
        STID, PaymentDate, PaymentMode, PaymentReferanceNo, PaymentAmount,
        TotalPaidAmount, DueAmount, PaymentStatus, CreatedDate, SBranchID, CreatedBy
    )
    VALUES
    (
        @STID, @PaymentDate, @PaymentMode, ISNULL(@PaymentReferanceNo, ''), @PaymentAmount,
        @TotalPaidAmount, @DueAmount, @PaymentStatus, GETDATE(), @SBranchID, @CreatedBy
    );

    -- 2. Get the new ID
    SET @PaymentID = CAST(SCOPE_IDENTITY() AS INT);

    -- 3. Update the ReceiptNumber based on the new ID
    UPDATE dbo.StockTransactionPaymentHistory
    SET ReceiptNumber = 'STRP/' + CAST(@SBranchID AS VARCHAR(10)) + '/' + RIGHT('000000' + CAST(@PaymentID AS VARCHAR(10)), 6)
    WHERE StockPaymentID = @PaymentID;

    -- 4. Return the ID back to C#
    SELECT @PaymentID;
END
go

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
go

CREATE OR ALTER PROCEDURE usp_GetStockSaleReport
    @SBranchID INT,
    @StartDate DATE,
    @EndDate DATE,
    @ProductID INT = 0,
    @DueOnly BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
	-- RESULT SET 1: Branch Information
    SELECT * FROM dbo.SBranchMaster WHERE SBranchID = @SBranchID;

    -- RESULT SET 2: Report Data
    SELECT 
        STM.STID,
        ISNULL(STM.InvoiceNumber, CAST(STM.STID AS VARCHAR(20))) AS InvoiceNumber,
        STM.TrDate,
        CASE 
            WHEN STM.RefType = 0 THEN ISNULL(SM.Name, '')
            ELSE ISNULL(EM.EmployeeName, '') 
        END AS CustomerName,
        LTRIM(RTRIM(ISNULL(CM.ClassName, '') + CASE WHEN ISNULL(CS.Name, '') = '' THEN '' ELSE ' / ' + CS.Name END)) AS ClassSection,
        STUFF(
            (
                SELECT ', ' + PM.Name
                FROM dbo.StockTransactionDetails STD2
                INNER JOIN dbo.ProductMaster PM ON PM.ProductID = STD2.ProductID
                WHERE STD2.STID = STM.STID
                GROUP BY PM.Name
                FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)'), 1, 2, ''
        ) AS ProductNames,
        ISNULL((SELECT SUM(STD.Quantity) FROM dbo.StockTransactionDetails STD WHERE STD.STID = STM.STID), 0) AS Quantity,
        ISNULL((SELECT SUM((STD.Quantity * STD.Cost) + (STD.Quantity * STD.Cost * (STD.SGST + STD.CGST + STD.IGST) / 100.0))
                FROM dbo.StockTransactionDetails STD
                WHERE STD.STID = STM.STID), 0) AS Amount,
        ISNULL(STM.PaidAmount, 0) AS PaidAmount,
        ISNULL(STM.DueAmount, 0) AS DueAmount,
        ISNULL(STM.PaymentStatus, 0) AS PaymentStatus,
        CASE
            WHEN STM.Status = 0 THEN 'Cancelled'
            WHEN ISNULL(STM.PaymentStatus, 0) = 2 THEN 'Paid'
            WHEN ISNULL(STM.PaidAmount, 0) > 0 THEN 'Partial'
            ELSE 'Unpaid'
        END AS PaymentStatusText,
        ISNULL(LP.StockPaymentID, 0) AS LastPaymentID,
        ISNULL(LP.ReceiptNumber, '') AS LastReceiptNumber,
        LP.PaymentDate AS LastPaymentDate,
        ISNULL(LP.PaymentStatus, 0) AS LastPaymentStatus,
        LP.CancelledDate AS LastReceiptCancelledDate
    FROM dbo.StockTransactionMaster STM
    LEFT JOIN dbo.StudentMaster SM ON SM.StudentID = STM.RefID AND STM.RefType = 0
    LEFT JOIN dbo.EmployeeMaster EM ON EM.EmployeeID = STM.RefID AND STM.RefType = 1
    LEFT JOIN dbo.ClassMaster CM ON CM.ClassID = STM.ClassID
    LEFT JOIN dbo.class_sections CS ON CS.ID = STM.SectionID AND CS.ClassID = STM.ClassID
    OUTER APPLY
    (
        SELECT TOP 1 StockPaymentID, PaymentDate, ReceiptNumber, PaymentStatus, CancelledDate
        FROM dbo.StockTransactionPaymentHistory PH
        WHERE PH.STID = STM.STID AND PH.SBranchID = STM.SBranchID
        ORDER BY PH.PaymentDate DESC, PH.StockPaymentID DESC
    ) LP
    WHERE STM.SBranchID = @SBranchID
      AND STM.TrType = 1
      AND CAST(STM.TrDate AS DATE) BETWEEN @StartDate AND @EndDate
      AND (@ProductID = 0 OR EXISTS
      (
          SELECT 1
          FROM dbo.StockTransactionDetails STD
          WHERE STD.STID = STM.STID AND STD.ProductID = @ProductID
      ))
      AND (@DueOnly = 0 OR (STM.Status <> 0 AND ISNULL(STM.DueAmount, 0) > 0))
    ORDER BY STM.TrDate DESC, STM.STID DESC;
END
go

CREATE OR ALTER PROCEDURE usp_GetSaleTransactionPrintData
    @STID INT,
    @SBranchID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- RESULT SET 1: Branch Details
    SELECT * FROM dbo.SBranchMaster WHERE SBranchID = @SBranchID;

    -- Variables to hold data for Customer lookup
    DECLARE @RefType INT, @RefID INT, @ClassID INT, @SectionID INT;
    SELECT @RefType = RefType, @RefID = RefID, @ClassID = ClassID, @SectionID = SectionID
    FROM dbo.StockTransactionMaster
    WHERE STID = @STID AND SBranchID = @SBranchID;

    -- RESULT SET 2: Transfer/Master Data
    SELECT STM.STID, STM.TrDate, STM.TrType, STM.RefID, STM.RefType, STM.Remark, STM.Status,
           ISNULL(STM.PaymentDate, STM.TrDate) AS PaymentDate,
           ISNULL(STM.PaymentMode, 0) AS PaymentMode,
           ISNULL(STM.PaymentReferanceNo, '') AS PaymentReferanceNo,
           ISNULL(STM.PaidAmount, 0) AS PaidAmount,
           ISNULL(STM.DueAmount, 0) AS DueAmount,
           ISNULL(STM.PaymentStatus, 0) AS PaymentStatus,
           ISNULL(STM.CancelRemark, '') AS CancelRemark,
           ISNULL(STM.InvoiceNumber, CAST(STM.STID AS VARCHAR(20))) AS InvoiceNumber,
           ISNULL((SELECT TOP 1 StockPaymentID
                   FROM dbo.StockTransactionPaymentHistory PH
                   WHERE PH.STID = STM.STID AND PH.SBranchID = STM.SBranchID
                   ORDER BY PH.PaymentDate DESC, PH.StockPaymentID DESC), 0) AS LastPaymentID,
           ISNULL((SELECT SUM((STD.Quantity * STD.Cost) + (STD.Quantity * STD.Cost * (STD.SGST + STD.CGST + STD.IGST) / 100.0))
                   FROM dbo.StockTransactionDetails STD WHERE STD.STID = STM.STID), 0) AS Amount
    FROM dbo.StockTransactionMaster STM
    WHERE STM.STID = @STID AND STM.SBranchID = @SBranchID;

    -- RESULT SET 3: Product Details
    SELECT STD.STDID, STD.STID, STD.STType, STD.ProductID, PM.Name AS ProductName,
           STD.Cost, STD.MRP, STD.SGST, STD.CGST, STD.IGST, STD.Quantity
    FROM dbo.StockTransactionDetails STD
    INNER JOIN dbo.ProductMaster PM ON PM.ProductID = STD.ProductID
    WHERE STD.STID = @STID;

    -- RESULT SET 4: Customer Details
    SELECT CASE WHEN @RefType = 0 THEN ISNULL(SM.Name, '') ELSE ISNULL(EM.EmployeeName, '') END AS Name,
           '' AS Address, '' AS GSTIN, '' AS StateName, '' AS ContactNo, 0 AS StateID,
           LTRIM(RTRIM(ISNULL(CM.ClassName, '') + CASE WHEN ISNULL(CS.Name, '') = '' THEN '' ELSE ' / ' + CS.Name END)) AS ExtraData,
           ISNULL(CM.ClassName, '') AS ClassName, '' AS FatherName
    FROM (SELECT 1 AS Dummy) X
    LEFT JOIN dbo.StudentMaster SM ON SM.StudentID = @RefID AND @RefType = 0
    LEFT JOIN dbo.EmployeeMaster EM ON EM.EmployeeID = @RefID AND @RefType = 1
    LEFT JOIN dbo.ClassMaster CM ON CM.ClassID = @ClassID
    LEFT JOIN dbo.class_sections CS ON CS.ID = @SectionID AND CS.ClassID = @ClassID;

    -- RESULT SET 5: Payment History
    SELECT StockPaymentID, STID, ISNULL(ReceiptNumber, '') AS ReceiptNumber, PaymentDate, PaymentMode,
           ISNULL(PaymentReferanceNo, '') AS PaymentReferanceNo, PaymentAmount, TotalPaidAmount, DueAmount,
           PaymentStatus, CreatedDate, SBranchID, ISNULL(CreatedBy, 0) AS CreatedBy,
           ISNULL(IsCancelled, 0) AS IsCancelled, CancelledDate, ISNULL(CancelRemark, '') AS CancelRemark,
           ISNULL(CancelledBy, 0) AS CancelledBy
    FROM dbo.StockTransactionPaymentHistory
    WHERE STID = @STID AND SBranchID = @SBranchID
    ORDER BY PaymentDate ASC, StockPaymentID ASC;
END
go

IF EXISTS
(
    SELECT 1
    FROM dbo.StockTransactionMaster STM
    WHERE ISNULL(STM.PaidAmount, 0) > 0
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.StockTransactionPaymentHistory PH
          WHERE PH.STID = STM.STID
      )
)
BEGIN
    INSERT INTO dbo.StockTransactionPaymentHistory
    (
        STID, ReceiptNumber, PaymentDate, PaymentMode, PaymentReferanceNo, PaymentAmount,
        TotalPaidAmount, DueAmount, PaymentStatus, CreatedDate, SBranchID, CreatedBy
    )
    SELECT STM.STID,
           NULL,
           ISNULL(STM.PaymentDate, STM.TrDate),
           ISNULL(STM.PaymentMode, 0),
           ISNULL(STM.PaymentReferanceNo, ''),
           ISNULL(STM.PaidAmount, 0),
           ISNULL(STM.PaidAmount, 0),
           ISNULL(STM.DueAmount, 0),
           ISNULL(STM.PaymentStatus, CASE WHEN ISNULL(STM.DueAmount, 0) > 0 THEN 1 ELSE 2 END),
           GETDATE(),
           STM.SBranchID,
           NULL
    FROM dbo.StockTransactionMaster STM
    WHERE ISNULL(STM.PaidAmount, 0) > 0
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.StockTransactionPaymentHistory PH
          WHERE PH.STID = STM.STID
      );

    UPDATE PH
    SET ReceiptNumber = 'STRP/' + CAST(PH.SBranchID AS VARCHAR(10)) + '/' + RIGHT(REPLICATE('0', 6) + CAST(PH.StockPaymentID AS VARCHAR(10)), 6)
    FROM dbo.StockTransactionPaymentHistory PH
    WHERE ISNULL(PH.ReceiptNumber, '') = '';
END
GO
