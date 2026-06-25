IF COL_LENGTH('dbo.StockTransactionMaster', 'PaymentDate') IS NULL
BEGIN
    ALTER TABLE dbo.StockTransactionMaster ADD PaymentDate DATETIME NULL;
END
GO

IF COL_LENGTH('dbo.StockTransactionMaster', 'PaymentMode') IS NULL
BEGIN
    ALTER TABLE dbo.StockTransactionMaster ADD PaymentMode INT NULL;
END
GO

IF COL_LENGTH('dbo.StockTransactionMaster', 'PaymentReferanceNo') IS NULL
BEGIN
    ALTER TABLE dbo.StockTransactionMaster ADD PaymentReferanceNo NVARCHAR(100) NULL;
END
GO

IF COL_LENGTH('dbo.StockTransactionMaster', 'PaidAmount') IS NULL
BEGIN
    ALTER TABLE dbo.StockTransactionMaster ADD PaidAmount DECIMAL(18,2) NULL;
END
GO

IF COL_LENGTH('dbo.StockTransactionMaster', 'DueAmount') IS NULL
BEGIN
    ALTER TABLE dbo.StockTransactionMaster ADD DueAmount DECIMAL(18,2) NULL;
END
GO

IF COL_LENGTH('dbo.StockTransactionMaster', 'PaymentStatus') IS NULL
BEGIN
    ALTER TABLE dbo.StockTransactionMaster ADD PaymentStatus INT NULL;
END
GO

IF COL_LENGTH('dbo.StockTransactionMaster', 'CancelRemark') IS NULL
BEGIN
    ALTER TABLE dbo.StockTransactionMaster ADD CancelRemark NVARCHAR(250) NULL;
END
GO

UPDATE dbo.StockTransactionMaster
SET PaymentDate = ISNULL(PaymentDate, TrDate),
    PaymentMode = ISNULL(PaymentMode, 0),
    PaymentReferanceNo = ISNULL(PaymentReferanceNo, ''),
    PaidAmount = ISNULL(PaidAmount, 0),
    DueAmount = ISNULL(DueAmount, 0),
    PaymentStatus = ISNULL(PaymentStatus, 0),
    CancelRemark = ISNULL(CancelRemark, '');
GO

ALTER PROCEDURE [dbo].[sp_UpdateStockTransaction]
(
@STID int,
@TrDate datetime,
@TrType int,
@RefID int,
@RefType int,
@Remark nvarchar(50),
@ClassID int,
@SectionID int,
@SessionID int,
@CreatedDate datetime,
@Status int,
@PaymentDate datetime = NULL,
@PaymentMode int = 0,
@PaymentReferanceNo nvarchar(100) = NULL,
@PaidAmount decimal(18,2) = 0,
@DueAmount decimal(18,2) = 0,
@PaymentStatus int = 0,
@CancelRemark nvarchar(250) = NULL,
@OpType int,
@SBranchID int,
@Details ut_StockTransactionDetails READOnly,
@EmployeeTypeID int,
@VendorID int
)
AS
BEGIN
    IF(@OpType=-1)
    BEGIN
        DELETE FROM StockTransactionMaster WHERE STID=@STID
        DELETE FROM StockTransactionDetails WHERE STID=@STID
    END
    ELSE
    BEGIN
        SET @PaymentDate = ISNULL(@PaymentDate, @TrDate)
        SET @PaymentReferanceNo = ISNULL(@PaymentReferanceNo, '')
        SET @CancelRemark = ISNULL(@CancelRemark, '')
        SET @PaidAmount = ISNULL(@PaidAmount, 0)
        SET @DueAmount = ISNULL(@DueAmount, 0)
        SET @PaymentStatus = ISNULL(@PaymentStatus, 0)

        IF(@STID=0)
        BEGIN
            DECLARE @FYear nvarchar(10)
            IF(DATEPART(month,@TrDate)<=3)
            BEGIN
                SET @FYear= CAST((DATEPART(yyyy,@TrDate) % 100-1) AS nvarchar(4))+CAST((DATEPART(yyyy,@TrDate) % 100) AS nvarchar(4))
            END
            ELSE
            BEGIN
                SET @FYear= CAST((DATEPART(yyyy,@TrDate) % 100) AS nvarchar(4))+CAST((DATEPART(yyyy,@TrDate) % 100+1) AS nvarchar(4))
            END

            DECLARE @SlNo int
            SELECT @SlNo=MAX(SerialNo) FROM StockTransactionMaster WHERE SBranchID=@SBranchID AND TrType=@TrType AND FY=@FYear

            DECLARE @InvNumber nvarchar(50)
            SET @SlNo=ISNULL(@SlNo,0)+1
            SET @InvNumber=@FYear+'/'+CAST(@SBranchID AS nvarchar(5))+'/'+CAST(@SlNo AS nvarchar(10))

            INSERT INTO StockTransactionMaster
            (
                TrDate,TrType,RefID,RefType,Remark,CreatedDate,Status,SBranchID,ClassID,SectionID,SessionID,EmployeeTypeID,VendorID,
                SerialNo,InvoiceNumber,FY,PaymentDate,PaymentMode,PaymentReferanceNo,PaidAmount,DueAmount,PaymentStatus,CancelRemark
            )
            VALUES
            (
                @TrDate,@TrType,@RefID,@RefType,@Remark,@CreatedDate,@Status,@SBranchID,@ClassID,@SectionID,@SessionID,@EmployeeTypeID,@VendorID,
                @SlNo,@InvNumber,@FYear,@PaymentDate,@PaymentMode,@PaymentReferanceNo,@PaidAmount,@DueAmount,@PaymentStatus,@CancelRemark
            )
            SELECT @STID=CAST(SCOPE_IDENTITY() AS int)

            INSERT INTO StockTransactionDetails(STID, STType,ProductID,Quantity,MRP,Cost,SBranchID,SGST,CGST,IGST)
            SELECT @STID, @TrType,ProductID,Quantity,MRP,Cost,@SBranchID,SGST,CGST,IGST
            FROM @Details PD WHERE PD.STDID=0
        END
        ELSE
        BEGIN
            UPDATE StockTransactionMaster
            SET TrDate=@TrDate,
                TrType=@TrType,
                RefID=@RefID,
                RefType=@RefType,
                Remark=@Remark,
                Status=@Status,
                ClassID=@ClassID,
                SectionID=@SectionID,
                SessionID=@SessionID,
                EmployeeTypeID=@EmployeeTypeID,
                VendorID=@VendorID,
                PaymentDate=@PaymentDate,
                PaymentMode=@PaymentMode,
                PaymentReferanceNo=@PaymentReferanceNo,
                PaidAmount=@PaidAmount,
                DueAmount=@DueAmount,
                PaymentStatus=@PaymentStatus,
                CancelRemark=@CancelRemark
            WHERE STID=@STID

            DELETE FROM StockTransactionDetails WHERE STDID NOT IN (SELECT STDID FROM @Details) AND STID=@STID

            UPDATE e
            SET e.ProductID=d.ProductID,
                e.Quantity=d.Quantity,
                e.MRP=d.MRP,
                e.Cost=d.Cost,
                e.SGST=d.SGST,
                e.CGST=d.CGST,
                e.IGST=d.IGST
            FROM StockTransactionDetails e, @Details d
            WHERE d.STDID=e.STDID AND d.STDID<>0

            INSERT INTO StockTransactionDetails(STID, STType,ProductID,Quantity,MRP,Cost,SBranchID,SGST,CGST,IGST)
            SELECT @STID, @TrType,ProductID,Quantity,MRP,Cost,@SBranchID,SGST,CGST,IGST
            FROM @Details PD WHERE PD.STDID=0
        END
    END
    SELECT ISNULL(@STID,0)
END
GO

ALTER PROCEDURE [dbo].[sp_GetStockTransactionDetails]
(
@STID int,
@SBranchID int
)
AS
BEGIN
    DECLARE @ClassID int=0,@SessionID int=0,@SectionID int=0,@RefID int=0,@RefType int=0,@EmployeeTypeID int=0,@TrType int=0,@VendorID int=0

    IF(@STID!=0)
    BEGIN
        SELECT @ClassID=ClassID,@SessionID=SessionID,@SectionID=SectionID,@RefID=RefID,@RefType=RefType,@EmployeeTypeID=EmployeeTypeID,@TrType=TrType,@VendorID=VendorID
        FROM StockTransactionMaster WHERE STID=@STID
    END
    IF(@TrType=0 AND @VendorID=0)
    BEGIN
        SELECT @VendorID=MIN(VendorID) FROM VendorMaster WHERE SBranchID=@SBranchID
    END
    ELSE IF(@RefType=0 AND @RefID=0)
    BEGIN
        SELECT TOP 1 @SessionID=SessionID FROM SessionMaster WHERE SBranchID=@SBranchID ORDER BY SessionStatus DESC
        SELECT @ClassID=MIN(ClassID) FROM ClassMaster WHERE SBranchID=@SBranchID
        SELECT @SectionID=MIN(ID) FROM Class_Sections WHERE ClassID=@ClassID
    END
    ELSE IF(@RefType=1 AND @RefID=0)
    BEGIN
        SELECT @EmployeeTypeID=MIN(EmployeeTypeID) FROM EmployeeTypeMaster WHERE SBranchID=@SBranchID OR SBranchID=0
        SELECT TOP 1 @RefID=EmployeeID FROM EmployeeMaster WHERE SBranchID=@SBranchID AND EmployeeType=@EmployeeTypeID ORDER BY EmployeeName
    END

    SELECT *,
           ISNULL(PaymentDate, TrDate) AS PaymentDate,
           ISNULL(PaymentMode, 0) AS PaymentMode,
           ISNULL(PaymentReferanceNo, '') AS PaymentReferanceNo,
           ISNULL(PaidAmount, 0) AS PaidAmount,
           ISNULL(DueAmount, 0) AS DueAmount,
           ISNULL(PaymentStatus, 0) AS PaymentStatus,
           ISNULL(CancelRemark, '') AS CancelRemark
    FROM StockTransactionMaster
    WHERE STID=@STID

    SELECT STDID,STID,STType,ProductID,Quantity,SBranchID,Cost,MRP,SGST,CGST,IGST,
    (SELECT Name FROM ProductMaster PM WHERE STD.ProductID=PM.ProductID) AS ProductName FROM StockTransactionDetails STD WHERE STID=@STID

    ;WITH ActiveStock AS
    (
        SELECT
            STD.ProductID,
            SUM(CASE
                    WHEN STM.TrType=0 THEN ISNULL(STD.Quantity,0)
                    WHEN STM.TrType=1 THEN -ISNULL(STD.Quantity,0)
                    ELSE 0
                END) AS AvailableQty
        FROM StockTransactionDetails STD
        INNER JOIN StockTransactionMaster STM ON STM.STID=STD.STID
        WHERE STM.SBranchID=@SBranchID
          AND ISNULL(STM.Status,1)<>0
        GROUP BY STD.ProductID
    )
    SELECT PM.ProductID,PM.Name,PM.MRP,PM.Price,PM.Quantity,PM.MinQty,PC.Name AS CategoryName,PC.HSNCode,PC.SGST,PC.IGST,PC.CGST,
    ISNULL(PM.Quantity,0)+ISNULL(ASQ.AvailableQty,0) AS Available
    FROM ProductMaster PM
    LEFT OUTER JOIN ProductCategories PC ON PM.ProductCategoryID=PC.ID
    LEFT OUTER JOIN ActiveStock ASQ ON ASQ.ProductID=PM.ProductID
    WHERE PM.SBranchID=@SBranchID

    IF(@TrType=0)
    BEGIN
        SELECT ISNULL(@VendorID,0)
        SELECT VendorID,CompanyName,ContactPerson,StateID FROM VendorMaster WHERE SBranchID=@SBranchID
    END
    ELSE IF(@RefType=0)
    BEGIN
        SELECT SessionID AS ID, SessionName AS Name, SessionStatus AS Extra1 FROM SessionMaster WHERE SBranchID=@SBranchID

        SELECT ClassID AS ID,ClassName AS Name FROM ClassMaster WHERE SBranchID=@SBranchID

        SELECT ID,Name FROM Class_Sections WHERE ClassID=@ClassID

        SELECT StudentID AS ID, Name,'STUD'+RIGHT(REPLICATE('0',6)+CAST(StudentID AS VARCHAR(6)),6) AS Extra1 FROM StudentMaster WHERE StudentID IN
        (SELECT StudentID FROM Student_Session WHERE SessionID=@SessionID AND ClassID=@ClassID AND SectionID=@SectionID)
        ORDER BY Name

        SELECT ISNULL(@SessionID,0)
        SELECT ISNULL(@ClassID,0)
        SELECT ISNULL(@SectionID,0)
    END
    ELSE
    BEGIN
        SELECT EmployeeTypeID AS ID , EmployeeTypeName AS Name FROM EmployeeTypeMaster WHERE SBranchID=@SBranchID OR SBranchID=0
        ORDER BY EmployeeTypeID

        IF(@RefID=0)
        BEGIN
            SELECT @EmployeeTypeID=MIN(EmployeeTypeID) FROM EmployeeTypeMaster WHERE SBranchID=@SBranchID OR SBranchID=0
            SELECT TOP 1 @RefID=EmployeeID FROM EmployeeMaster WHERE SBranchID=@SBranchID AND EmployeeType=@EmployeeTypeID ORDER BY EmployeeName
        END

        SELECT EmployeeID AS ID,EmployeeName AS Name FROM EmployeeMaster WHERE SBranchID=@SBranchID AND EmployeeType=@EmployeeTypeID

        SELECT ISNULL(@RefID,0)
        SELECT ISNULL(@EmployeeTypeID,0)
    END
    SELECT * FROM ProductCategories AS Categories WHERE SBranchID=@SBranchID
END
GO

ALTER PROCEDURE [dbo].[sp_GetStockTransactions]
(
@StartDate date,
@EndDate date,
@TrType int,
@SBranchID int
)
AS
BEGIN
    SELECT STID,TrDate,TrType,RefID,RefType,Remark,Status,
    ISNULL(PaymentDate, TrDate) AS PaymentDate,
    ISNULL(PaymentMode, 0) AS PaymentMode,
    ISNULL(PaymentReferanceNo, '') AS PaymentReferanceNo,
    ISNULL(PaidAmount, 0) AS PaidAmount,
    ISNULL(DueAmount, 0) AS DueAmount,
    ISNULL(PaymentStatus, 0) AS PaymentStatus,
    ISNULL(CancelRemark, '') AS CancelRemark,
    'STUD'+RIGHT(REPLICATE('0',6)+CAST(RefID AS VARCHAR(6)),6) AS StudentSID,
    CAST(STM.STID AS VARCHAR(10))+'/'+CAST(STM.SbranchID AS VARCHAR(10))+'/'+CAST(STM.SessionID AS VARCHAR(10))+'/'+ CAST(STM.RefID AS VARCHAR(10)) AS RecieptNo,
    (SELECT ClassName FROM classmaster CM WHERE CM.ClassID=STM.Classid) AS ClassName,
    (SELECT Name FROM class_sections CS WHERE CS.ID=STM.SectionID AND CS.ClassID=STM.ClassID ) AS SectionName,
    (CASE WHEN TrType=0 THEN (SELECT CompanyName FROM VendorMaster VM WHERE VM.VendorID=STM.VendorID) ELSE
    (CASE WHEN RefType=0 THEN (SELECT Name FROM StudentMaster WHERE StudentID=RefID) ELSE
    (SELECT EmployeeName FROM EmployeeMaster WHERE EmployeeID=RefID) END) END) AS RefName,
    (SELECT SUM(Quantity) FROM StockTransactionDetails STD WHERE STD.STID=STM.STID) AS Quantity,
    (SELECT SUM((Quantity*Cost) +Quantity*Cost*SGST/100+Quantity*Cost*CGST/100+Quantity*Cost*IGST/100)
    FROM StockTransactionDetails STD WHERE STD.STID=STM.STID) AS Amount
    FROM StockTransactionMaster STM
    WHERE CAST(TrDate AS date) BETWEEN @StartDate AND @EndDate AND TrType=@TrType AND SBranchID=@SBranchID
    ORDER BY TrDate DESC

    SELECT * FROM sbranchmaster WHERE SBranchID=@SBranchID
END
GO

ALTER PROCEDURE [dbo].[sp_GetStockTransactionPrintDetails]
(
@STID int,
@SBranchID int
)
AS
BEGIN
    SELECT * FROM SBranchMaster WHERE SBranchID=@SBranchID

    DECLARE @ClassID int=0,@SessionID int=0,@SectionID int=0,@RefID int=0,@RefType int=0,@EmployeeTypeID int=0,@TrType int=0,@VendorID int=0,
    @Address nvarchar(max),@ContactNo nvarchar(20)

    SELECT @ClassID=ClassID,@SessionID=SessionID,@SectionID=SectionID,@RefID=RefID,@RefType=RefType,
    @EmployeeTypeID=EmployeeTypeID,@TrType=TrType,@VendorID=VendorID FROM StockTransactionMaster WHERE STID=@STID

    SELECT *,
    ISNULL(PaymentDate, TrDate) AS PaymentDate,
    ISNULL(PaymentMode, 0) AS PaymentMode,
    ISNULL(PaymentReferanceNo, '') AS PaymentReferanceNo,
    ISNULL(PaidAmount, 0) AS PaidAmount,
    ISNULL(DueAmount, 0) AS DueAmount,
    ISNULL(PaymentStatus, 0) AS PaymentStatus,
    ISNULL(CancelRemark, '') AS CancelRemark,
    'STUD'+RIGHT(REPLICATE('0',6)+CAST(RefID AS VARCHAR(6)),6) AS StudentSID,
    CAST(STM.STID AS VARCHAR(10))+'/'+CAST(STM.SbranchID AS VARCHAR(10))+'/'+CAST(STM.SessionID AS VARCHAR(10))+'/'+ CAST(STM.RefID AS VARCHAR(10)) AS InvoiceNumber,
    (SELECT ClassName FROM classmaster CM WHERE CM.ClassID=STM.Classid) AS ClassName,
    (SELECT Name FROM class_sections CS WHERE CS.ID=STM.SectionID AND CS.ClassID=STM.ClassID ) AS SectionName
    FROM StockTransactionMaster STM WHERE STID=@STID

    SELECT STDID,STID,STType,ProductID,Quantity,SBranchID,Cost,MRP,SGST,CGST,IGST,
    (SELECT Name FROM ProductMaster PM WHERE STD.ProductID=PM.ProductID) AS ProductName
    FROM StockTransactionDetails STD WHERE STID=@STID

    IF(@TrType=0)
    BEGIN
        SELECT CompanyName AS Name,ContactNumber AS ContactNo, Address,GSTIN,StateID,
        (SELECT StateName+' ('+StateCode+')' FROM GSTStateMaster GSM WHERE GSM.StateID=VM.StateID) AS StateName
        FROM VendorMaster VM WHERE VendorID=@VendorID
    END
    ELSE IF(@RefType=0)
    BEGIN
        DECLARE @StudentName nvarchar(100),@ClassSectionName nvarchar(100),
        @FatherName nvarchar(1000),@Gender int,@SessionName nvarchar(50),@ParentID int

        SELECT @StudentName=Name +' ('+ISNULL('STUD'+RIGHT(REPLICATE('0',6)+CAST(StudentID AS VARCHAR(6)),6),'')+')',@Gender=Gender,@Address=MiniAddress,@ParentID=ParentID FROM StudentMaster WHERE StudentID=@RefID

        SELECT @ClassSectionName=ClassName+'->'+SectionName FROM v_ClassSectionNames WHERE SectionID=@SectionID
        SELECT @FatherName=FatherName,@ContactNo=FatherMobileNo FROM ParentMaster WHERE ParentID=@ParentID
        SELECT @SessionName=SessionName FROM SessionMaster WHERE SessionID=@SessionID

        SELECT @StudentName+' '+(CASE WHEN @Gender=1 THEN 'D/o' ELSE 'S/o' END)+' '+@FatherName AS Name,@ContactNo AS ContactNo,'Local State' AS StateName,
        @ClassSectionName+' ('+@SessionName+')' AS ExtraData,@Address AS Address
    END
    ELSE
    BEGIN
        DECLARE @EmployeeName nvarchar(50),@EmpTypeName nvarchar(50)
        SELECT @EmpTypeName=EmployeeTypeName FROM EmployeeTypeMaster WHERE EmployeeTypeID=@EmployeeTypeID

        SELECT @EmployeeName=EmployeeName+' ('+ISNULL(@EmpTypeName,'')+')' ,@ContactNo=MobileNumber
        FROM EmployeeMaster WHERE EmployeeID=@RefID

        SELECT @EmployeeName AS Name,@ContactNo AS ContactNo,'Local State' AS StateName
    END
END
GO
