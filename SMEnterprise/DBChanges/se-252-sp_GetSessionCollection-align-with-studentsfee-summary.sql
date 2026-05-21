/*
Fix: Account/CollectionReport should match Account/StudentsFee.

Root cause:
- StudentsFee summary uses sp_GetClassGroupFeeListOnly / sp_GetStudentFeeDetailsNew.
- CollectionReport uses sp_GetSessionCollection.
- Older sp_GetSessionCollection logic can drift from StudentsFee when newer rules are added
  for session carry-forward, no-fee months, approved request discounts, and v_PaymentDetails.

Change:
- Keep the same result-set contract used by AccountData.GetCollectionReport.
- Build the student collection rows from sp_GetClassGroupFeeListOnly, the same summary source
  used by StudentsFee.
- Report Amount = FeeAmount + PreviousDue + LateFee.
- Report Discount = Discounts.
- Report Paid = Paid.
- Report Balance = Amount - Discount - Paid.
*/

IF OBJECT_ID(N'dbo.sp_GetSessionCollection', N'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_GetSessionCollection AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[sp_GetSessionCollection]
(
    @ClassID int,
    @SectionID int,
    @SBranchID int,
    @SessionID int,
    @QDate date,
    @CurDate date,
    @SStudentID int = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    IF (@SectionID = 0)
    BEGIN
        SELECT TOP 1 @ClassID = ClassID
        FROM ClassMaster
        WHERE SBranchID = @SBranchID
          AND Status = 1
        ORDER BY ClassID;

        SELECT TOP 1 @SectionID = ID
        FROM Class_Sections
        WHERE ClassID = @ClassID
          AND Status = 1
        ORDER BY ID;
    END;

    IF (@SStudentID <> 0)
    BEGIN
        SELECT TOP 1
            @ClassID = ClassID,
            @SectionID = SectionID
        FROM Student_Session
        WHERE SessionID = @SessionID
          AND StudentID = @SStudentID
        ORDER BY Status DESC;
    END;

    SELECT ClassID AS ID, ClassName AS Name
    FROM ClassMaster
    WHERE SBranchID = @SBranchID;

    SELECT ID, Name
    FROM Class_Sections
    WHERE ClassID = @ClassID;

    SELECT ISNULL(@ClassID, 0);
    SELECT ISNULL(@SectionID, 0);

    SELECT SessionID AS ID, SessionName AS Name, SessionStatus AS Extra1
    FROM SessionMaster
    WHERE SBranchID = @SBranchID;

    SELECT ISNULL(@SessionID, 0);

    DECLARE @Students TABLE
    (
        StudentID int,
        Name nvarchar(100),
        RollNo nvarchar(100),
        Gender int,
        Photo nvarchar(100),
        ClassID int,
        FeePaymentMode int,
        StudentSID nvarchar(15),
        FromDate date,
        ToDate date,
        QuotaID int,
        SessionID int,
        VehicleRouteID int,
        HostelRoomID int,
        SectionID int,
        SessionStartDate date,
        SessionEndDate date,
        IsAdmissionFee int,
        SchoolUID nvarchar(50),
        FeeAmount numeric(10,2),
        PreviousDue numeric(10,2),
        LateFee numeric(10,2),
        Discounts numeric(10,2),
        Paid numeric(10,2)
    );

    INSERT INTO @Students
    EXEC [dbo].[sp_GetClassGroupFeeListOnly]
        @ClassID = @ClassID,
        @SectionID = @SectionID,
        @SBranchID = @SBranchID,
        @SessionID = @SessionID,
        @QDate = @QDate,
        @CurDate = @CurDate,
        @SStudentID = @SStudentID;

    SELECT
        S.StudentID,
        S.Name,
        S.RollNo,
        S.Gender,
        S.Photo,
        S.ClassID,
        S.FeePaymentMode,
        S.StudentSID,
        S.FromDate,
        S.ToDate,
        S.QuotaID,
        S.SessionID,
        S.VehicleRouteID,
        S.HostelRoomID,
        S.SectionID,
        S.SessionStartDate,
        S.SessionEndDate,
        S.IsAdmissionFee,
        S.SchoolUID,
        Q.QuotaName,
        PM.MotherName,
        PM.FatherName,
        CAST(ISNULL(S.FeeAmount,0) + ISNULL(S.PreviousDue,0) + ISNULL(S.LateFee,0) AS numeric(10,2)) AS FeeAmount,
        CAST(ISNULL(S.Discounts,0) AS numeric(10,2)) AS Discounts,
        CAST(ISNULL(S.Paid,0) AS numeric(10,2)) AS PaidAmount,
        CAST(
            ISNULL(S.FeeAmount,0)
            + ISNULL(S.PreviousDue,0)
            + ISNULL(S.LateFee,0)
            - ISNULL(S.Discounts,0)
            - ISNULL(S.Paid,0)
            AS numeric(10,2)
        ) AS UnpaidAmount
    FROM @Students S
    LEFT JOIN StudentMaster SM
        ON SM.StudentID = S.StudentID
    LEFT JOIN ParentMaster PM
        ON PM.ParentID = SM.ParentID
    LEFT JOIN QuotaMaster Q
        ON Q.QuotaID = S.QuotaID
    WHERE ISNULL(S.StudentID,0) <> 0
    ORDER BY S.Name;

    SELECT *
    FROM SBranchMaster
    WHERE SBranchID = @SBranchID;

    SET NOCOUNT OFF;
END
GO
