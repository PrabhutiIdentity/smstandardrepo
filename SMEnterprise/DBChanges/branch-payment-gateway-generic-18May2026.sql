IF OBJECT_ID('dbo.BranchPaymentGateway', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BranchPaymentGateway
    (
        SBranchID INT PRIMARY KEY,
        KeyId NVARCHAR(200) NULL,
        Secret NVARCHAR(200) NULL,
        PaymentGatewayName NVARCHAR(100) NULL,
        IsActive BIT NULL,
        CreatedDate DATETIME NULL,
        UpdatedDate DATETIME NULL
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetBranchGateway
    @SBranchID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        g.SBranchID,
        g.KeyId,
        g.Secret,
        g.PaymentGatewayName,
        ISNULL(g.IsActive, 1) AS IsActive,
        g.CreatedDate,
        g.UpdatedDate,
        b.BranchName,
        b.BranchSchoolName,
        b.EmailID,
        b.ContactNo,
        b.Address,
        b.Logo
    FROM dbo.BranchPaymentGateway g
    LEFT JOIN dbo.SBranchMaster b ON b.SBranchID = g.SBranchID
    WHERE g.SBranchID = @SBranchID;
END
GO
