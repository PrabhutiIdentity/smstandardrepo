
CREATE TABLE [dbo].[StockTransactionMaster](
	[STID] [int] IDENTITY(1,1) NOT NULL,
	[TrDate] [datetime] NULL,
	[TrType] [int] NULL,
	[RefID] [int] NULL,
	[RefType] [int] NULL,
	[Remark] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[Status] [int] NULL,
	[ClassID] [int] NULL,
	[SectionID] [int] NULL,
	[SessionID] [int] NULL,
	[SBranchID] [int] NULL,
	[EmployeeTypeID] [int] NULL,
	[VendorID] [int] NULL
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[StockTransactionDetails](
	[STDID] [int] IDENTITY(1,1) NOT NULL,
	[STID] [int] NULL,
	[STType] [int] NULL,
	[ProductID] [int] NULL,
	[Quantity] [numeric](10, 2) NULL,
	[MRP] [numeric](10, 2) NULL,
	[Cost] [numeric](10, 2) NULL,
	[SBranchID] [int] NULL,
	[SGST] [numeric](5, 2) NULL,
	[CGST] [numeric](5, 2) NULL,
	[IGST] [numeric](5, 2) NULL
) ON [PRIMARY]
GO
CREATE TYPE [dbo].[ut_StockTransactionDetails] AS TABLE(
	[STDID] [int] NULL,
	[STID] [int] NULL,
	[STType] [int] NULL,
	[ProductID] [int] NULL,
	[Quantity] [numeric](10, 2) NULL,
	[MRP] [numeric](10, 2) NULL,
	[Cost] [numeric](10, 2) NULL,
	[SBranchID] [int] NULL,
	[SGST] [numeric](5, 2) NULL,
	[CGST] [numeric](5, 2) NULL,
	[IGST] [numeric](5, 2) NULL
)
GO
CREATE TABLE [dbo].[BlackBoardImages](
	[BlackBoardID] [nvarchar](50) NULL,
	[BBMIID] [nvarchar](50) NULL,
	[Photo] [nvarchar](50) NULL,
	[Detail] [nvarchar](max) NULL,
	[SBranchID] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE TABLE [dbo].[BlackBoardMaster](
	[BlackBoardID] [nvarchar](50) NULL,
	[TeacherID] [int] NULL,
	[ClassID] [int] NULL,
	[SectionID] [int] NULL,
	[PeriodID] [int] NULL,
	[Title] [nvarchar](max) NULL,
	[Photo] [nvarchar](max) NULL,
	[BDate] [datetime] NULL,
	[SBranchID] [int] NULL,
	[SubjectID] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE FUNCTION [dbo].[GetBlackBoardImageList]
(
	@BlackBoardID nvarchar(50)
)
RETURNS nvarchar(max)
as
begin
declare @Images nvarchar(Max)
Set @Images= ''
declare @Photo nvarchar(max)
declare FirstCursor cursor  for select top 1 Photo from BlackBoardImages HC where HC.BlackBoardID=@BlackBoardID
OPEN FirstCursor 
	FETCH NEXT FROM FirstCursor INTO @Photo	  
	WHILE @@FETCH_STATUS = 0
	Begin
	if(@Photo is not null)
		begin
			if(@Images!='')
			begin
				set @Images=@Images+','
			end
			 set @Images=@Images+@Photo
		end
		 FETCH NEXT FROM FirstCursor INTO @Photo
	end
close FirstCursor 
deallocate FirstCursor 
return @Images
end
GO
Create view [dbo].[v_StockProductAvailability]
AS
select ProductID,sum(Case when STType!=0 then -1*Quantity else Quantity end) as AvailableQty 
from StockTransactionDetails STD left outer join StockTransactionMaster STM on STD.STID=STM.STID
where STM.Status=1
Group by ProductID
GO
Create View [dbo].[v_StockTransactionCalculated]
as
select STDID,STD.STID,STType,ProductID,STM.Status,
(Case when STType!=0 then -1*Quantity else Quantity end) as Quantity,MRP,
(Case when STType=2 then 0 else Cost end) as EffectiveCost,
Cost,STD.SBranchID,SGST,CGST,IGST,
(Case when STType=0 then -1*Quantity*Cost when STType=1 then Quantity*Cost else 0 end) as NetAmount,
(Case when STType=0 then -1*Quantity*Cost*SGST/100 when STType=1 then Quantity*Cost*SGST/100 else 0 end) as SGSTAmount,
(Case when STType=0 then -1*Quantity*Cost*CGST/100 when STType=1 then Quantity*Cost*CGST/100 else 0 end) as CGSTAmount,
(Case when STType=0 then -1*Quantity*Cost*IGST/100 when STType=1 then Quantity*Cost*IGST/100 else 0 end) as IGSTAmount
from StockTransactionDetails STD left outer join StockTransactionMaster STM on STD.STID=STM.STID
GO
CREATE TABLE [dbo].[GSTStateMaster](
	[StateID] [int] IDENTITY(1,1) NOT NULL,
	[StateName] [nvarchar](50) NULL,
	[StateCode] [nvarchar](50) NULL
) ON [PRIMARY]
GO
GO
SET IDENTITY_INSERT [dbo].[GSTStateMaster] ON 
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (1, N'JAMMU AND KASHMIR', N'1')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (2, N'HIMACHAL PRADESH', N'2')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (3, N'PUNJAB', N'3')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (4, N'CHANDIGARH', N'4')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (5, N'UTTARAKHAND', N'5')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (6, N'HARYANA', N'6')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (7, N'DELHI', N'7')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (8, N'RAJASTHAN', N'8')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (9, N'UTTAR PRADESH', N'9')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (10, N'BIHAR', N'10')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (11, N'SIKKIM', N'11')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (12, N'ARUNACHAL PRADESH', N'12')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (13, N'NAGALAND', N'13')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (14, N'MANIPUR', N'14')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (15, N'MIZORAM', N'15')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (16, N'TRIPURA', N'16')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (17, N'MEGHLAYA', N'17')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (18, N'ASSAM', N'18')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (19, N'WEST BENGAL', N'19')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (20, N'JHARKHAND', N'20')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (21, N'ODISHA', N'21')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (22, N'CHATTISGARH', N'22')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (23, N'MADHYA PRADESH', N'23')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (24, N'GUJARAT', N'24')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (25, N'DAMAN AND DIU', N'25')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (26, N'DADRA AND NAGAR HAVELI', N'26')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (27, N'MAHARASHTRA', N'27')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (28, N'ANDHRA PRADESH(BEFORE DIVISION)', N'28')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (29, N'KARNATAKA', N'29')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (30, N'GOA', N'30')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (31, N'LAKSHWADEEP', N'31')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (32, N'KERALA', N'32')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (33, N'TAMIL NADU', N'33')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (34, N'PUDUCHERRY', N'34')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (35, N'ANDAMAN AND NICOBAR ISLANDS', N'35')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (36, N'TELANGANA', N'36')
GO
INSERT [dbo].[GSTStateMaster] ([StateID], [StateName], [StateCode]) VALUES (37, N'ANDHRA PRADESH (NEW)', N'37')
GO
SET IDENTITY_INSERT [dbo].[GSTStateMaster] OFF
GO
GO
Alter Table LeaveMaster
Add LeaveTypeApplied int
GO
Alter Table LeaveMaster
Drop column CLApplied
GO
Alter Table LeaveMaster
Drop column ELApplied
GO
Alter Table LeaveMaster
Drop column PLApplied
GO
Alter Table LeaveMaster
Drop column LWPApplied
GO
Alter table LeaveTypeMaster
Add SequenceNo int
GO
Alter table LeaveTypeMaster
Add IsCarryForward int
GO
Alter table LeaveTypeMaster
Add MaxConsecutive int
GO
ALTER Table [dbo].[PaymentDetails]
Add [PaymentTitle] [nvarchar](100) 
GO
DROP TABLE [dbo].[ProductCategories]
GO
CREATE TABLE [dbo].[ProductCategories](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Status] [int] NULL,
	[SBranchID] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[HSNCode] [nvarchar](50) NULL,
	[SGST] [numeric](10, 2) NULL,
	[CGST] [numeric](10, 2) NULL,
	[IGST] [numeric](10, 2) NULL
) ON [PRIMARY]
GO
DROP TABLE [dbo].[ProductMaster]
GO
CREATE TABLE [dbo].[ProductMaster](
	[ProductID] [int] IDENTITY(1,1) NOT NULL,
	[ProductCategoryID] [int] NULL,
	[Name] [nvarchar](50) NULL,
	[Photo] [nvarchar](50) NULL,
	[MRP] [numeric](10, 2) NULL,
	[Price] [numeric](10, 2) NULL,
	[SBranchID] [int] NULL,
	[Status] [int] NULL,
	[Quantity] [numeric](10, 2) NULL,
	[MinQty] [numeric](10, 2) NULL
) ON [PRIMARY]
GO
Alter Table [dbo].[SBranchMaster] 
Add [StateID] [int]
GO
Alter table [dbo].[Student_Session]
Add [IsCustomFee] [int]
GO
CREATE TABLE [dbo].[SubReligionMaster](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[ReligionID] [int] NULL
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[VendorMaster](
	[VendorID] [int] IDENTITY(1,1) NOT NULL,
	[CompanyName] [nvarchar](50) NULL,
	[ContactPerson] [nvarchar](50) NULL,
	[ContactNumber] [nvarchar](50) NULL,
	[Address] [nvarchar](max) NULL,
	[StateID] [int] NULL,
	[PinCode] [nvarchar](10) NULL,
	[CompanyContact] [nvarchar](50) NULL,
	[EmailID] [nvarchar](50) NULL,
	[BranchName] [nvarchar](50) NULL,
	[BankName] [nvarchar](50) NULL,
	[AccountNumber] [nvarchar](50) NULL,
	[AccountType] [int] NULL,
	[IFSCCode] [nvarchar](50) NULL,
	[OtherDetails] [nvarchar](max) NULL,
	[GSTIN] [nvarchar](50) NULL,
	[PANNumber] [nvarchar](50) NULL,
	[TINNumber] [nvarchar](50) NULL,
	[CINNumber] [nvarchar](50) NULL,
	[RegistrationNumber] [nvarchar](50) NULL,
	[SBranchID] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
create procedure [dbo].[getStudentListOnSessionSection]
(
@SectionID int,
@SessionID int
)
AS
BEGIN
	
		select StudentID as ID, Name +'('+isnull(SchoolUID,'')+')' as Name from StudentMaster where 
		StudentID in (Select StudentID from Student_Session where SessionID=@SessionID and SectionID=@SectionID)
		order by Name
END
GO
CREATE Procedure [dbo].[sp_GetAccountVendors]
(
@SBranchID int
)
AS
BEGIN
		select VM.*,GS.StateName+' ('+GS.StateCode+')' as StateName from VendorMaster VM 
		left outer join GSTStateMaster GS on GS.StateID=VM.StateID
		where SBranchID=@SBranchID
		--select * from GSTStateMaster
END
GO
ALTER proc [dbo].[sp_GetAdminSBranches]
(
@SBranchID int=0
)
AS
BEGIN
	if(@SBranchID=0)
	begin
		Select SBranchID,BranchName,[Logo],[ContactNo],[EmailID],[Address],[PrincipalName],[PrincipalMobile]
		,[PrincipalEmail],[BranchSchoolName],StateID,
		(Select count(*) from Student_Session SS where Status=1 and SS.SBranchID=SBM.SBranchID) as Students,
		(Select UserName from LoginDetails LD where LD.SBranchID= SBM.SBranchID and UserType=2) as AccountUserName
		 from [dbo].[SBranchMaster] SBM

	end
	else
	begin
		Select SBranchID,BranchName,[Logo],[ContactNo],[EmailID],[Address],[PrincipalName],[PrincipalMobile]
		,[PrincipalEmail],[BranchSchoolName],StateID,
		(Select count(*) from Student_Session SS where Status=1 and SS.SBranchID=SBM.SBranchID) as Students,
		(Select UserName from LoginDetails LD where LD.SBranchID= SBM.SBranchID and UserType=2) as AccountUserName
		 from [dbo].[SBranchMaster] SBM where SBranchID=@SBranchID

	end
END
GO
CREATE Procedure [dbo].[sp_GetBlackBoardByStudent]
(
@StudentID int,
@SubjectID int,
@PageID int=0
)
AS
BEGIN
	declare @ClassID int,@SectionID int,@SBranchID int
	select top 1 @ClassID=CLassID,@SectionID=SectionID,@SBranchID=SBranchID from Student_Session where STudentID=@StudentID
	order by Status desc

	exec sp_GetBlackBoardEnteries @SBranchID,@ClassID,@SectionID,@SubjectID,@PageID
END
GO
Create procedure [dbo].[sp_GetBlackBoardDetails]
(
	@BlackBoardID nvarchar(50)
)
AS
BEGIN
	Select top 20 BBM.*,EM.EmployeeName as TeacherName,CM.ClassName,CS.Name as SectionName,
	PM.Name+'('+cast(PM.StartTime as nvarchar(5))+'-'+ cast(PM.EndTime as nvarchar(5))+')' as PeriodName,
	BBM.Title
	from dbo.BlackBoardMaster BBM 
	left outer join EmployeeMaster EM on EM.EmployeeID=BBM.TeacherID
	left outer join ClassMaster CM on CM.ClassID=BBM.ClassID
	left outer join Class_Sections CS on CS.ID=BBM.SectionID
	left outer join PeriodMaster PM on PM.PeriodID=BBM.PeriodID
	where BBM.BlackBoardID=@BlackBoardID

	select BlackBoardID,BBMIID,BlackBoardID+'_'+Photo as Photo,Detail from BlackBoardImages where BlackBoardID=@BlackBoardID
END
GO
CREATE Procedure [dbo].[sp_GetBlackBoardEnteries]
(
	@SBranchID int,
	@ClassID int,
	@SectionID int,
	@SubjectID int,
	@PageID int
)
AS
BEGIN
	SELECT *
FROM (
     Select BBM.*,EM.EmployeeName as TeacherName,CM.ClassName,CS.Name as SectionName,
	PM.Name+'('+cast(PM.StartTime as nvarchar(5))+'-'+ cast(PM.EndTime as nvarchar(5))+')' as PeriodName,
	[dbo].[GetBlackBoardImageList](BBM.BlackBoardID) as Images,SM.SubjectName, ROW_NUMBER() OVER (ORDER BY BDate desc) AS RowNum
	from dbo.BlackBoardMaster BBM 
	left outer join EmployeeMaster EM on EM.EmployeeID=BBM.TeacherID
	left outer join ClassMaster CM on CM.ClassID=BBM.ClassID
	left outer join Class_Sections CS on CS.ID=BBM.SectionID
	left outer join PeriodMaster PM on PM.PeriodID=BBM.PeriodID
	left outer join SubjectMaster SM on SM.SubjectID=BBM.SubjectID
	where BBM.ClassID=@ClassID and BBM.SectionID=@SectionID and BBM.SBranchID=@SBranchID--and BBM.SubjectID=@SubjectID
	)t where t.rownum between @PageID*20 and (@PageID+1)*20
END
GO
GO
ALTER procedure [dbo].[sp_GetClassGroupFeeListOnly]
(
@ClassID int,
@SectionID int,
@SBranchID int,
@SessionID int,
@QDate date,
@CurDate date,
@SStudentID int=0
)
as 
BEGIN
	if(@SStudentID<>0)
	begin
		select @ClassID=ClassID,@SectionID=SectionID from Student_Session where SessionID=@SessionID and StudentID=@SStudentID
	end
	Declare @GroupID int
	Declare @TransportFeeMode int
	Declare @HostelFeeMode int
	declare @LastPayDay nvarchar(2)

	Select @TransportFeeMode=details from MasterSettings where Type='TransportFeeMode' and SBranchID=@SBranchID
	Select @HostelFeeMode=details from MasterSettings where Type='HostelFeeMode' and SBranchID=@SBranchID
	Select @LastPayDay=details from MasterSettings where Type='FeePaymentReminderDate' and SBranchID=@SBranchID


	Declare @SessionStartDate date,@SessionEndDate Date
	select @GroupID=GroupID from Class_Sections where ID=@SectionID
	select @SessionStartDate=SessionStartDate,@SessionEndDate=SessionEndDate from SessionMaster where SessionID=@SessionID
		

	Declare @Students table(StudentID int,Name nvarchar(100),RollNo nvarchar(100),Gender int,Photo nvarchar(100),ClassID int,FeePaymentMode int,
	StudentSID nvarchar(15),FromDate date,ToDate Date,QuotaID int,
	SessionID int,VehicleRouteID int,HostelRoomID int,SectionID int,
	SessionStartDate date,SessionEndDate date,IsAdmissionFee int,SchoolUID nvarchar(50),FeeAmount numeric(10,2),PreviousDue numeric(10,2)
	,LateFee numeric(10,2),Discounts numeric(10,2),Paid numeric(10,2),IsCustomFee int)

	if(@SStudentID=0)
	begin
		insert into @Students
		select SM.StudentID,SM.Name,SS.RollNo,SM.Gender,SM.Photo,SS.ClassID,SS.FeePaymentMode,
		'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,SS.FromDate,SS.ToDate,
		SS.QuotaID,SS.SessionID,SM.VehicleRouteID,SM.HostelRoomID,SS.SectionID,
		(Case when @SessionStartDate<SS.FromDate then SS.FromDate else @SessionStartDate end) as SessionStartDate,
		(Case when @SessionEndDate>SS.ToDate then SS.ToDate else @SessionEndDate end) as SessionEndDate,
		SS.IsAdmissionFeeApplicable,SM.SchoolUID,0,0,0,0,0,SS.IsCustomFee
		from Student_Session SS left outer join StudentMaster SM on SM.StudentID=SS.StudentID
		where SS.ClassID=@ClassID and SessionID=@SessionID and SectionID=@SectionID
	end
	else
	begin
		insert into @Students
		select SM.StudentID,SM.Name,SS.RollNo,SM.Gender,SM.Photo,SS.ClassID,SS.FeePaymentMode,
		'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,SS.FromDate,SS.ToDate,
		SS.QuotaID,SS.SessionID,SM.VehicleRouteID,SM.HostelRoomID,SS.SectionID,
		(Case when @SessionStartDate<SS.FromDate then SS.FromDate else @SessionStartDate end) as SessionStartDate,
		(Case when @SessionEndDate>SS.ToDate then SS.ToDate else @SessionEndDate end) as SessionEndDate,
		SS.IsAdmissionFeeApplicable,SM.SchoolUID,0,0,0,0,0,SS.IsCustomFee
		from Student_Session SS left outer join StudentMaster SM on SM.StudentID=SS.StudentID
		where SS.ClassID=@ClassID and SessionID=@SessionID and SectionID=@SectionID and SS.StudentID=@SStudentID
	end
	Declare @DiscountApproved table(StudentID int,FeeMonth int,FeeYear int,ApprovedAmount decimal(18,2))

	Insert into @DiscountApproved Select StudentID,FeeMonth,FeeYear,
	(Select sum(ApprovedAmount) from  [FeeDiscountRequestDetails] FDD where FDM.DiscRequestID=FDD.DiscRequestID ) as ApprovedAmount
	from  [FeeDiscountRequestMaster] FDM 
	 where StudentID in (select StudentID from @Students) and Status=1

	Declare @QuotaDiscounts table (FeeTypeID int,Discount numeric(5,2))

	Declare @MTFT table (MonthTypeID int,FeeTypeID int)
	insert into @MTFT 
	select MonthTypeID,FeeTypeID from [MonthTypeFeeType]

	
	Declare @NoFeeMonths table (Month int)

	insert into @NoFeeMonths
	select Month from SessionClassNoFeeMonths where ClassID=@ClassID and SessionID=@SessionID
	
	Declare @FeeStructureTable table (FeeTypeID int,FeeTypeName nvarchar(50),FeeTypeApplicable int,FeeAmount numeric(10,2),Status int,Months nvarchar(50))

	insert into @FeeStructureTable
	select FTM.FeeTypeID,FTM.FeeTypeName,FTM.FeeTypeApplicable,CFS.FeeAmount,CFS.Status,FTM.Months
	from FeeTypeMaster FTM left outer join [dbo].[ClassFeeStructureMaster] CFS on CFS.FeeTypeID=FTM.FeeTypeID and ClassID=@ClassID and GroupID=@GroupID
	and CFS.SessionID=@SessionID and FTM.SBranchID=@SBranchID
	
	Declare @StuSessionSDate date,@StuSessionEDate date,@StudentID int,@QuotaID int,@IsAdmissionFee int,
	@FeeAmount numeric(10,2),@PreviousDue numeric(10,2),@LateFee numeric(10,2),@Discounts numeric(10,2)
	,@LMonth int,@LYear int,@MonthType int,@BaseDate date,@Paid numeric(10,2),@FeePaymentMode int,@IsCustomFee int

		DECLARE stu_Cursor CURSOR FOR
		SELECT SessionStartDate,SessionEndDate,StudentID,QuotaID,IsAdmissionFee,FeeAmount,PreviousDue,LateFee,Discounts,Paid,FeePaymentMode
		FROM @Students FOR UPDATE OF FeeAmount,PreviousDue,LateFee,Discounts,Paid
		OPEN stu_Cursor
		FETCH NEXT FROM stu_Cursor
		INTO @StuSessionSDate,@StuSessionEDate,@StudentID,@QuotaID,@IsAdmissionFee,@FeeAmount,@PreviousDue,@LateFee,@Discounts,@Paid,@FeePaymentMode

		WHILE (@@FETCH_STATUS = 0)
		BEGIN
			declare @StudentSessionUID int

			select top 1 @StudentSessionUID=StudentSessionUID,@IsCustomFee=isnull(IsCustomFee,0) from Student_Session where StudentID=@StudentID and SessionID=@SessionID
			order by Status desc

			Delete from @QuotaDiscounts
			Insert into @QuotaDiscounts select FeeTypeID,DiscPer from QuotaDiscountDetails where SessionID=@SessionID and QuotaID=@QuotaID
			set @BaseDate=@StuSessionSDate
			declare @LoopDate date=@QDate			
			declare @CMonth int=datepart(month,@QDate),@CYear int=datepart(year,@QDate)
			if(@QDate>@StuSessionEDate)
			begin
				set @LoopDate=@StuSessionEDate
			end
			else if((DATEDIFF(month,dateadd(month,-1,@SessionStartDate),@LoopDate))%@FeePaymentMode>0)
			begin
				declare @remainder int=(DATEDIFF(month,dateadd(month,-1,@SessionStartDate),@LoopDate))%@FeePaymentMode
				declare @adjustMonth int=@FeePaymentMode-@remainder
				set @LoopDate=DateAdd(month,@adjustMonth, @LoopDate)
				set @CMonth = datepart(month,dateadd(month,1-@remainder,@QDate))
				set @CYear = datepart(year,dateadd(month,1-@remainder,@QDate))
			end			
			
			print(cast(@CMonth as nvarchar(10))+'-'+Cast(@CYear as nvarchar(10)))
			while(datepart(year,@StuSessionSDate)*12+datepart(month,@StuSessionSDate)<=datepart(year,@LoopDate)*12+datepart(month,@LoopDate))
			begin
				set @LMonth= datepart(month,@StuSessionSDate)
				set @LYear=datepart(year,@StuSessionSDate)

				if((select count(*) from @NoFeeMonths where Month=@LMonth)=0)
				begin
					declare @PayDate date
					select @PayDate=min(PaymentDate) from PaymentDetails where PayeeID=@StudentID and Month=@LMonth and Year=@LYear and isnull(PaymentStatus,0)=0
					declare @IsLatePay int=0
					if(@PayDate is not null and @PayDate>cast(cast(@LYear as nvarchar(5))+'-'+cast(@LMonth as nvarchar(2))+'-'+@LastPayDay as date))
					begin
						set @IsLatePay=1
					end
					else if(@PayDate is null and @CurDate>cast(cast(@LYear as nvarchar(5))+'-'+cast(@LMonth as nvarchar(2))+'-'+@LastPayDay as date))
					begin
						set @IsLatePay=1
					end
					else
					begin
						set @IsLatePay=0
					end
					select @MonthType=(Case when datepart(month,@BaseDate)=@LMonth and (datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or 
							datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth) then 5 else
							(Case when datepart(month,@BaseDate)=@LMonth and (datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth) then 6 else 
							(case when datepart(month,@BaseDate)=@LMonth then (case when @IsAdmissionFee=0 then 1 else 0 end)  
							else (Case when datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth then 3
							else (case when  datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth then 4 else 2 end) end) end)end)end)
					--Update Discount Requested Details
					
					declare @TPaid numeric(10,2)=0
					declare @CLateFee numeric(10,2)=0
					declare @CPaid numeric(10,2)=0

				
					--Feed in previous dues
					if(@CMonth+@CYear*12>@LMonth+@LYear*12)
					begin
						declare @PPD numeric(10,2)=0
						declare @PPP numeric(10,2)=0

						select @PPD=sum((case when isnull(PD.PaymentID,0)!=0 then PD.NetApplicablePayment-isnull(PD.DiscAmt,0)  
						when isnull(@IsCustomFee,0)=1 then SFD.FeeAmount else (FTS.FeeAmount-isnull(FTS.FeeAmount*QD.Discount/100,0)) end)) 
						,@PPP=isnull(Sum(PD.PaymentRecieved),0)
						from @FeeStructureTable FTS left outer join @QuotaDiscounts QD on QD.FeeTypeID=FTS.FeeTypeID
						left outer join [dbo].[StudentFeeDetails] SFD on SFD.StudentID=@StudentID and SFD.SessionID=@StudentSessionUID and 
						SFD.FeeTypeID=FTS.FeeTypeID
						left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
						where FTS.FeeTypeApplicable in (select FeeTypeID from @MTFT where MonthTypeID=@MonthType) and FTS.FeeTypeApplicable!=5
						and FTS.FeeTypeApplicable <> 7 and FTS.FeeTypeApplicable<>(Case when @IsAdmissionFee=0 then 8 else 0 end)
						and isnull(SFD.IsApplicable,FTS.Status)=1 and (FTS.FeeTypeApplicable<>9 or (select count(*) from [dbo].[SplitStringToTable](FTS.Months,',') where Item=@LMonth)>0)

						
						declare @pDiscount numeric(10,2)
						if(isnull(@PPP,0)=0)
						begin
							select @pDiscount=isnull(sum(ApprovedAmount),0) from @DiscountApproved where StudentID=@StudentID and FeeMonth=@LMonth and FeeYear=@LYear				
						end
						select @PreviousDue= isnull(@PreviousDue,0)+isnull([dbo].[fn_GetStudentTransportFeeAmount](@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID),0)
						
						select @PreviousDue=isnull(@PreviousDue,0)-isnull(Sum(isnull(PD.PaymentRecieved,0)),0)
						from @FeeStructureTable FTS left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
						where FTS.FeeTypeApplicable = 5
						
						select @CLateFee=sum((case when isnull(PD.PaymentID,0)!=0 then PD.NetApplicablePayment-isnull(PD.DiscAmt,0) else FTS.FeeAmount end)) 
						,@CPaid=Sum(isnull(PD.PaymentRecieved,0))
						from @FeeStructureTable FTS left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
						where FTS.FeeTypeApplicable = (case when @IsLatePay=0 then 0 else 7 end)
						and isnull(FTS.Status,0)=1
						set @PreviousDue=isnull(@PreviousDue,0)-isnull(@pDiscount,0)+isnull(@CLateFee,0)-isnull(@CPaid,0)+isnull(@PPD,0)-isnull(@PPP,0)
						
					end
					else
					begin
						declare @pPaid numeric(10,2)
						select @FeeAmount=isnull(@FeeAmount,0)+sum((case when isnull(PD.PaymentID,0)!=0 then PD.NetApplicablePayment-isnull(PD.DiscAmt,0) 
						when isnull(@IsCustomFee,0)=1 then SFD.FeeAmount else (FTS.FeeAmount-isnull(FTS.FeeAmount*QD.Discount/100,0)) end))
						,@pPaid=Sum(isnull(PD.PaymentRecieved,0))
						from @FeeStructureTable FTS left outer join @QuotaDiscounts QD on QD.FeeTypeID=FTS.FeeTypeID
						left outer join [dbo].[StudentFeeDetails] SFD on SFD.StudentID=@StudentID and SFD.SessionID=@StudentSessionUID and SFD.FeeTypeID=FTS.FeeTypeID
						left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
						where FTS.FeeTypeApplicable in (select FeeTypeID from @MTFT where MonthTypeID=@MonthType) and FTS.FeeTypeApplicable!=5
						and FTS.FeeTypeApplicable <> 7 and FTS.FeeTypeApplicable<>(Case when @IsAdmissionFee=0 then 8 else 0 end)
						and isnull(SFD.IsApplicable,FTS.Status)=1 and (FTS.FeeTypeApplicable<>9 or (select count(*) from [dbo].[SplitStringToTable](FTS.Months,',') where Item=@LMonth)>0)

						if(isnull(@pPaid,0)=0)
						begin
							select @Discounts=isnull(@Discounts,0)+isnull(sum(ApprovedAmount),0) from @DiscountApproved where StudentID=@StudentID and FeeMonth=@LMonth and FeeYear=@LYear
						end

						select  @FeeAmount=isnull(@FeeAmount,0)+[dbo].[fn_GetStudentTransportFeeAmount](@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID)
						
						select @TPaid=Sum(isnull(PD.PaymentRecieved,0))
						from @FeeStructureTable FTS left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID 
						and PD.Month=@LMonth and PD.Year=@LYear
						where FTS.FeeTypeApplicable = 5

						select @CLateFee=sum((case when isnull(PD.PaymentID,0)!=0 then PD.NetApplicablePayment else FTS.FeeAmount end)) 
						,@CPaid=Sum(isnull(PD.PaymentRecieved,0))
						from @FeeStructureTable FTS left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
						where FTS.FeeTypeApplicable = (case when @IsLatePay=0 then 0 else 7 end)
						and isnull(FTS.Status,0)=1
						set @LateFee=isnull(@LateFee,0)+isnull(@CLateFee,0)
						set @Paid=isnull(@Paid,0)+isnull(@CPaid,0)+isnull(@TPaid,0)+isnull(@pPaid,0)
					end

				end
				set @StuSessionSDate=dateadd(month,1,@StuSessionSDate)
			end
			UPDATE @Students SET FeeAmount=@FeeAmount,PreviousDue=@PreviousDue,Discounts=@Discounts,Paid=@Paid,LateFee=@LateFee WHERE CURRENT OF stu_Cursor
    
			FETCH NEXT FROM stu_Cursor
			INTO  @StuSessionSDate,@StuSessionEDate,@StudentID,@QuotaID,@IsAdmissionFee,@FeeAmount,@PreviousDue,@LateFee,@Discounts,@Paid,@FeePaymentMode
		END

		CLOSE stu_Cursor
		DEALLOCATE stu_Cursor

		select StudentID ,Name,RollNo,Gender,Photo,ClassID,FeePaymentMode,
		StudentSID,FromDate ,ToDate ,QuotaID,SessionID,VehicleRouteID,HostelRoomID,SectionID,
		SessionStartDate,SessionEndDate,IsAdmissionFee,SchoolUID,FeeAmount,PreviousDue,LateFee,Discounts,Paid from @Students
END
GO
ALTER Procedure [dbo].[sp_GetClassGroupWiseStudentFeeSummery]
(
@ClassID int,
@SectionID int,
@SessionID int,
@SBranchID int,
@QDate date,
@CurDate Date
)
as
BEGIN
	if(@SessionID=0)
	begin
		Select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
	end
	if(@SectionID=0)
	begin
		select top 1 @ClassID=ClassID from ClassMaster where SBranchID=@SBranchID and Status=1 order by ClassID
		select top 1 @SectionID=ID from Class_Sections where ClassID=@ClassID and Status=1 order by ID
	end

	Select ClassID as ID, ClassName as Name from ClassMaster where SBranchID=@SBranchID
	Select ID,Name from Class_Sections where ClassID=@ClassID
	select @ClassID
	select @SectionID

	Declare @Students table(StudentID int,Name nvarchar(100),RollNo nvarchar(100),Gender int,Photo nvarchar(100),ClassID int,FeePaymentMode int,
	StudentSID nvarchar(15),FromDate date,ToDate Date,QuotaID int,
	SessionID int,VehicleRouteID int,HostelRoomID int,SectionID int,
	SessionStartDate date,SessionEndDate date,IsAdmissionFee int,SchoolUID nvarchar(50),FeeAmount numeric(10,2),PreviousDue numeric(10,2)
	,LateFee numeric(10,2),Discounts numeric(10,2),Paid numeric(10,2))


	insert into @Students
		exec sp_GetClassGroupFeeListOnly @ClassID,@SectionID,@SBranchID,@SessionID,@QDate,@CurDate

		select S.*,Q.QuotaName ,PM.MotherName,PM.FatherName,
		(select top 1 IsCustomFee from Student_Session SS where SS.StudentID=S.StudentID and SS.SessionID=@SessionID) as IsCustomFee
		from @Students S left outer join QuotaMaster Q on Q.QuotaID=S.QuotaID
		left outer join StudentMAster SM on SM.StudentID=S.StudentID
		left outer join ParentMaster PM on PM.ParentID=SM.ParentID
		
		select SessionID,SessionName,SessionStatus from SessionMaster where SBranchID=@SBranchID
		select isnull(@SessionID,0)
END
GO
ALTER  Procedure [dbo].[sp_GetEmployeeLeaveDetailsNew] --0,3,2,2020,1,0,1
(
@LeaveID int,
@EmployeeID int,
@Month int,
@Year int,
@EmployeeType int,
@SessionID int,
@SBranchID int
)
as
begin
  declare @StartDate date
  declare @EndDate date
  if(isnull(@SessionID,0)=0)
  begin
	select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
  end

  select @StartDate=SessionStartDate,@EndDate=SessionEndDate from SessionMaster where SessionID=@SessionID

  DECLARE @Leaves TABLE(LeaveID int,LeaveTypeID int, Applied numeric(10,2),Month int,Year int)
  DECLARE @MonthLeaves TABLE(LeaveID int,LeaveTypeID int, Applied numeric(10,2),Month int,Year int)

  insert into @Leaves
  select LeaveID,LeaveTypeID,Applied,DatePart(month,LeaveDate) as Month,datepart(year,LeaveDate) as Year from LeaveDetails LD
  where LeaveID in (select LeaveID from LeaveMaster where ApplicantID=@EmployeeID and ApplicantType=0 and EmployeeType=@EmployeeType and IsApproved in (1,2))
  and LeaveDate between @StartDate and @EndDate

  insert into @MonthLeaves
  select * from @Leaves where Month=@Month and Year=@Year


    Select LeaveTypeID,LeaveTypeName,YearlyQuota,MonthlyQuota,IsSalaryDeduct,YearlyApplied,MonthlyApplied,Applied,isnull(SequenceNo,0) as SequenceNo,MaxConsecutive from
   (select LTM.LeaveTypeID,LeaveTypeName,isnull(ELTM.YearlyQuota, LTM.[YearlyQuota]) as YearlyQuota,isnull(ELTM.MonthlyQuota,LTM.[MonthlyQuota]) as MonthlyQuota,
   IsSalaryDeduct,LY.Applied as YearlyApplied,LM.Applied as MonthlyApplied,LA.Applied,LTM.SequenceNo,LTM.MaxConsecutive
  from LeaveTypeMaster LTM left outer join [dbo].[EmployeeLeaveTypeMaster] ELTM 
	on ELTM.[LeaveTypeID]=LTM.LeaveTypeID and ELTM.EmployeeID=@EmployeeID
	left outer join 
  (select LeaveTypeID,sum(Applied) as Applied from @Leaves where LeaveID!=@LeaveID group by LeaveTypeID) LY on LY.LeaveTypeID=LTM.LeaveTypeID
   left outer join 
  (select LeaveTypeID,sum(Applied) as Applied from @MonthLeaves where LeaveID!=@LeaveID group by LeaveTypeID) LM on LM.LeaveTypeID=LTM.LeaveTypeID
   left outer join 
  (select LeaveTypeID,sum(Applied) as Applied from @Leaves where LeaveID=@LeaveID group by LeaveTypeID) LA on LA.LeaveTypeID=LTM.LeaveTypeID
  where LTM.SBranchID=@SBranchID
  union 
  select 0 as LeaveTypeID,'Un Paid',1000 as YearlyQuota,1000 as MonthlyQuota,
   1 as IsSalaryDeduct,
   (select sum(Applied) from @Leaves where LeaveID!=@LeaveID and LeaveTypeID=0) as YearlyApplied,
   (select sum(Applied) from @MonthLeaves where LeaveID!=@LeaveID and LeaveTypeID=0) as MonthlyApplied,
   (select sum(Applied) from @Leaves where LeaveID=@LeaveID and LeaveTypeID=0) as Applied,100000 as SequenceNo,10000 as MaxConsecutive)t
   order by (case when LeaveTypeID=0 then 1000000 else SequenceNo end )

 end
 GO
CREATE Procedure [dbo].[sp_GetEmployeeLeaveDetailsNew2Months]
(
@LeaveID int,
@EmployeeID int,
@StartDate date,
@EndDate date,
@EmployeeType int,
@SessionID int,
@SBranchID int
)
as
begin
	declare @SStartDate date
  declare @SEndDate date
  select @SStartDate=SessionStartDate,@SEndDate=SessionEndDate from SessionMaster where SessionID=@SessionID

  DECLARE @Leaves TABLE(LeaveID int,LeaveTypeID int, Applied numeric(10,2),Month int,Year int)
  DECLARE @MonthLeaves TABLE(LeaveID int,LeaveTypeID int, Applied numeric(10,2),Month int,Year int)

  insert into @Leaves
  select LeaveID,LeaveTypeID,Applied,DatePart(month,LeaveDate) as Month,datepart(year,LeaveDate) as Year from LeaveDetails LD
  where LeaveID in (select LeaveID from LeaveMaster where ApplicantID=@EmployeeID and ApplicantType=0 and EmployeeType=@EmployeeType and IsApproved in (1,2))
  and LeaveDate between @SStartDate and @SEndDate
  
  declare @Months int=(datepart(month,@EndDate)+datepart(Year,@EndDate)*12)-(datepart(month,@StartDate)+datepart(Year,@StartDate)*12)
  insert into @MonthLeaves
  select * from @Leaves where Month+Year*12 between datepart(month,@StartDate)+datepart(Year,@StartDate)*12 and datepart(month,@EndDate)+datepart(Year,@EndDate)*12

   Select LeaveTypeID,LeaveTypeName,YearlyQuota,MonthlyQuota,IsSalaryDeduct,YearlyApplied,MonthlyApplied,Applied from
   (select LTM.LeaveTypeID,LeaveTypeName,isnull(ELTM.YearlyQuota, LTM.[YearlyQuota]) as YearlyQuota,isnull(ELTM.MonthlyQuota,LTM.[MonthlyQuota])*(@Months+1) as MonthlyQuota,
   IsSalaryDeduct,LY.Applied as YearlyApplied,LM.Applied as MonthlyApplied,LA.Applied
  from LeaveTypeMaster LTM left outer join [dbo].[EmployeeLeaveTypeMaster] ELTM 
	on ELTM.[LeaveTypeID]=LTM.LeaveTypeID and ELTM.EmployeeID=@EmployeeID
	left outer join 
  (select LeaveTypeID,sum(Applied) as Applied from @Leaves where LeaveID!=@LeaveID group by LeaveTypeID) LY on LY.LeaveTypeID=LTM.LeaveTypeID
   left outer join 
  (select LeaveTypeID,sum(Applied) as Applied from @MonthLeaves where LeaveID!=@LeaveID group by LeaveTypeID) LM on LM.LeaveTypeID=LTM.LeaveTypeID
   left outer join 
  (select LeaveTypeID,sum(Applied) as Applied from @Leaves where LeaveID=@LeaveID group by LeaveTypeID) LA on LA.LeaveTypeID=LTM.LeaveTypeID
  where LTM.SBranchID=@SBranchID
  union 
  select 0 as LeaveTypeID,'Un Paid',1000 as YearlyQuota,1000 as MonthlyQuota,
   1 as IsSalaryDeduct,
   (select sum(Applied) from @Leaves where LeaveID!=@LeaveID and LeaveTypeID=0) as YearlyApplied,
   (select sum(Applied) from @MonthLeaves where LeaveID!=@LeaveID and LeaveTypeID=0) as MonthlyApplied,
   (select sum(Applied) from @Leaves where LeaveID=@LeaveID and LeaveTypeID=0) as Applied)t
   order by (case when LeaveTypeID=0 then 1000000 else LeaveTypeID end )
 end
 
GO
ALTER  procedure [dbo].[sp_GetEmployeeLeaves]
(
@SBranchID int
)
as 
begin 
		Select LeaveID,LeaveType,StartDate,EndDate,LeaveReason,LTM.LeaveTypeName,
		(select sum(Applied) from LeaveDetails LD where LD.LeaveID=LM.LeaveID) as Applied,
		isnull(EM.[EmployeeSID],'XXX000000') as EmployeeSID,
		ApplicantID as EmployeeID, isnull(EM.EmployeeName,'Employee Unavailable')  as 'EmployeeName',isnull(EM.Photo,'~/Images/EmployeeImage/Thumb/') as Photo,EM.EmployeeType,EM.Gender,IsApproved
		from LeaveMaster LM left outer join v_EmployeeDriversBasicDetails EM on EM.EmployeeID=LM.ApplicantID and LM.EmployeeType=EM.EmployeeType 
		left outer join LeaveTypeMaster LTM on LTM.LeaveTypeID=LM.LeaveTypeApplied
		where ApplicantType=0 and LM.SBranchID=@SBranchID
		order by StartDate desc
end
GO
ALTER  proc [dbo].[sp_GetEmployeeLeavesAccountNew] --0,1,1,2,2020,0,0
(
@LeaveID int,
@SBranchID int,
@SessionID int,
@Month int,
@Year int,
@EmployeeID int,
@EmployeeType int
)
AS
BEGIN
	if(@SessionID=0)
	begin
		select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
	end
	declare @EmployeesTable  table (EmployeeID int,EmployeeName nvarchar(500),Photo nvarchar(500),Gender int,EmployeeType int,EmployeeSID nvarchar(10))

	INSERT INTO @EmployeesTable Select EM.[EmployeeID],EM.[EmployeeName],
	 Photo,EM.Gender,EmployeeType,[EmployeeSID]
	from [v_EmployeeDriversBasicDetails] EM where SBranchID=@SBranchID	
	order by EmployeeName
	
	select @EmployeeID=ApplicantID,@EmployeeType=EmployeeType from LeaveMaster where LeaveID=@LeaveID

	if(@EmployeeID=0)
	begin
		Select top 1 @EmployeeID=EmployeeID,@EmployeeType=EmployeeType from @EmployeesTable order by EmployeeName 
	end
	exec sp_GetEmployeeLeaveDetailsNew @LeaveID,@EmployeeID,@Month,@Year,@EmployeeType,@SessionID,@SBranchID

	select * from @EmployeesTable order by EmployeeName 

	select * from LeaveMaster where LeaveID=@LeaveID
		
	select isnull(@EmployeeID,0)
	select isnull(@EmployeeType,0)
END
GO
CREATE Procedure [dbo].[sp_GetEmployeesForStockTransaction]
(
@STID int,
@SBranchID int,
@EmployeeTypeID int
)
AS
BEGIN
		declare @RefID int=0
		if(@STID!=0)
		begin
			Select @RefID=RefID,@EmployeeTypeID=EmployeeTypeID from StockTransactionMaster where STID=@STID
		end
		if(@EmployeeTypeID=0)
		begin
			Select @EmployeeTypeID=min(EmployeeTypeID) from EmployeeTypeMaster where SBranchID=@SBranchID or SBranchID=0
		end
		if(@RefID=0)
		begin
			select top 1 @RefID=EmployeeID from EmployeeMaster where SBranchID=@SBranchID and EmployeeType=@EmployeeTypeID order by EmployeeName
		end
		Select EmployeeTypeID as ID , EmployeeTypeName as Name from EmployeeTypeMaster where SBranchID=@SBranchID or SBranchID=0
		order by EmployeeTypeID


		select EmployeeID as ID,EmployeeName as Name from EmployeeMaster where SBranchID=@SBranchID and EmployeeType=@EmployeeTypeID

		select isnull(@RefID,0)
		select isnull(@EmployeeTypeID,0)
END
GO
GO
Create Procedure [dbo].[sp_GetGSTStates]
AS
BEGIN
		select * from GSTStateMaster
END
GO

CREATE Procedure [dbo].[sp_GetLowStockProducts]
(
@SBranchID int
)
AS
BEGIN
select PM.ProductID,PM.Name,PM.MRP,PM.Price,PM.MinQty,PC.Name as CategoryName,PC.HSNCode,PC.SGST,PC.IGST,PC.CGST,
		PM.Quantity+STC.AvailableQty as Quantity
		from ProductMaster PM
		left outer join ProductCategories PC on PM.ProductCategoryID=PC.ID
		left outer join v_StockProductAvailability STC on STC.ProductID=PM.ProductID and PM.Status=1
		where PM.SBranchID=@SBranchID and PM.Quantity+STC.AvailableQty<PM.MinQty

	--select PM.ProductID,PM.Name,PM.Quantity-isnull(t.Qty,0) as Quantity,PM.MinQty,Photo from ProductMaster PM
	--left outer join 
	--(Select ProductID,sum(Quantity*(Case when STType=1 then -1 else 1 end)) as Qty from StockTransactionDetails
	--where STID not in (Select STID from StockTransactionMaster where Status=0)
	--group by ProductID)t on t.ProductID=PM.ProductID
	--where PM.Quantity-isnull(t.Qty,0)<=isnull(PM.MinQty,0) and PM.SBranchID=@SBranchID
END
GO
ALTER  proc [dbo].[sp_GetMonthStudentAttandance]
(
@StudentID int,
@Year int,
@Month int
)
AS
BEGIN
	DECLARE @AttandanceTable TABLE (D1 numeric(5,2),D2 numeric(5,2),D3 numeric(5,2),D4 numeric(5,2),D5 numeric(5,2),D6 numeric(5,2),D7 numeric(5,2),D8 numeric(5,2),D9 numeric(5,2),D10 numeric(5,2),D11 numeric(5,2),D12 numeric(5,2),D13 numeric(5,2),D14 numeric(5,2),D15 numeric(5,2),
	D16 numeric(5,2), D17 numeric(5,2),D18 numeric(5,2), D19 numeric(5,2),D20 numeric(5,2),D21 numeric(5,2),D22 numeric(5,2), D23 numeric(5,2), D24 numeric(5,2), D25 numeric(5,2), D26 numeric(5,2),D27 numeric(5,2),D28 numeric(5,2), D29 numeric(5,2), D30 numeric(5,2), D31 numeric(5,2))

	declare @SBranchID int
	declare @ClassID int
	select @ClassID=ClassID,@SBranchID=SBranchID from Student_Session where StudentID=@StudentID and [Status]=1

	Declare @LeaveTable Table (DayID int,LStatus numeric(5,2))
	Declare @HolidayTable Table (DayID int,LStatus numeric(5,2))
	DECLARE db_leavecursor CURSOR FOR  
	Select StartDate,EndDate from [dbo].[LeaveMaster] where ApplicantType=1 and ApplicantID=@StudentID 	
	and ((datepart(month,StartDate)=@Month and datepart(year,StartDate)=@Year)
	or (datepart(month,EndDate)=@Month and datepart(year,EndDate)=@Year))
	Declare @StartDate date
	Declare @EndDate date
	OPEN db_leavecursor   
		FETCH NEXT FROM db_leavecursor INTO @StartDate,@EndDate

		WHILE @@FETCH_STATUS = 0   
		BEGIN   
			while(@StartDate<=@EndDate)
			begin
				if(datepart(month,@StartDate)=@Month)
				begin
					Insert into @LeaveTable(DayID,LStatus) values (datepart(day,@StartDate),-1)
				end
				Set @StartDate= DATEADD(day,1,@StartDate)
			end
		FETCH NEXT FROM db_leavecursor INTO  @StartDate,@EndDate
		END   
		CLOSE db_leavecursor   
		DEALLOCATE db_leavecursor

		DECLARE db_Holidaycursor CURSOR FOR  
	select StartDate,EndDate
	from HolidayMaster where [Status]=1 and SBranchID=@SBranchID and 
	((select count(*) from dbo.SplitStringToTable(Classes,',') where ITem=@ClassID or Item=0)>0) and IsStudents=1
	and ((datepart(month,StartDate)=@Month and datepart(year,StartDate)=@Year)
	or (datepart(month,EndDate)=@Month and datepart(year,EndDate)=@Year))
	OPEN db_Holidaycursor   
		FETCH NEXT FROM db_Holidaycursor INTO @StartDate,@EndDate

		WHILE @@FETCH_STATUS = 0   
		BEGIN   
			while(@StartDate<=@EndDate)
			begin
				if(datepart(month,@StartDate)=@Month)
				begin
					Insert into @HolidayTable(DayID,LStatus) values (datepart(day,@StartDate),2)
				end
				Set @StartDate= DATEADD(day,1,@StartDate)
			end
		FETCH NEXT FROM db_Holidaycursor INTO  @StartDate,@EndDate
		END   
		CLOSE db_Holidaycursor   
		DEALLOCATE db_Holidaycursor

	Insert into @AttandanceTable select  D1 ,D2 ,D3 ,D4 ,D5 ,D6 ,D7 ,D8 ,D9 ,D10 ,D11 ,D12 ,D13 ,D14 ,D15 ,
	D16 , D17 ,D18 , D19 ,D20 ,D21 ,D22 , D23 , D24 , D25 , D26 ,D27 ,D28 , D29 , D30 , D31
	from StudentAttendanceMaster where StudentID=@StudentID and FYear=@Year and [Month]=@Month

	Select t.DayID,isnull(isnull(HT.LStatus,LT.LStatus),[Status]) as Status from 
	(select 1 as DayID, isnull(D1,3) as [Status] from @AttandanceTable
		union select 2 as DayID, isnull(D2,3) as [Status] from @AttandanceTable
		union select 3 as DayID, isnull(D3,3) as [Status] from @AttandanceTable
		union select 4 as DayID, isnull(D4,3) as [Status] from @AttandanceTable
		union select 5 as DayID, isnull(D5,3) as [Status] from @AttandanceTable
		union select 6 as DayID, isnull(D6,3) as [Status] from @AttandanceTable
		union select 7 as DayID, isnull(D7,3) as [Status] from @AttandanceTable
		union select 8 as DayID, isnull(D8,3) as [Status] from @AttandanceTable
		union select 9 as DayID, isnull(D9,3) as [Status] from @AttandanceTable
		union select 10 as DayID, isnull(D10,3) as [Status] from @AttandanceTable
		union select 11 as DayID, isnull(D11,3) as [Status] from @AttandanceTable
		union select 12 as DayID, isnull(D12,3) as [Status] from @AttandanceTable
		union select 13 as DayID, isnull(D13,3) as [Status] from @AttandanceTable
		union select 14 as DayID, isnull(D14,3) as [Status] from @AttandanceTable
		union select 15 as DayID, isnull(D15,3) as [Status] from @AttandanceTable
		union select 16 as DayID, isnull(D16,3) as [Status] from @AttandanceTable
		union select 17 as DayID, isnull(D17,3) as [Status] from @AttandanceTable
		union select 18 as DayID, isnull(D18,3) as [Status] from @AttandanceTable
		union select 19 as DayID, isnull(D19,3) as [Status] from @AttandanceTable
		union select 20 as DayID, isnull(D20,3) as [Status] from @AttandanceTable
		union select 21 as DayID, isnull(D21,3) as [Status] from @AttandanceTable
		union select 22 as DayID, isnull(D22,3) as [Status] from @AttandanceTable
		union select 23 as DayID, isnull(D23,3) as [Status] from @AttandanceTable
		union select 24 as DayID, isnull(D24,3) as [Status] from @AttandanceTable
		union select 25 as DayID, isnull(D25,3) as [Status] from @AttandanceTable
		union select 26 as DayID, isnull(D26,3) as [Status] from @AttandanceTable
		union select 27 as DayID, isnull(D27,3) as [Status] from @AttandanceTable
		union select 28 as DayID, isnull(D28,3) as [Status] from @AttandanceTable
		union select 29 as DayID, isnull(D29,3) as [Status] from @AttandanceTable
		union select 30 as DayID, isnull(D30,3) as [Status] from @AttandanceTable
		union select 31 as DayID, isnull(D31,3) as [Status] from @AttandanceTable) t
		left outer join (Select distinct * from @LeaveTable) LT on t.DayID=LT.DayID
		left outer join (Select distinct * from @HolidayTable) HT on t.DayID=HT.DayID


END
GO
CREATE Procedure [dbo].[sp_GetProductCategories]
(
@SBranchID int
)
AS
Begin
	Select * from ProductCategories where SBranchID=@SBranchID
End
GO
CREATE Procedure [dbo].[sp_GetProductDetails]
(
@ProductID int,
@SBranchID int
)
AS
BEGIN
	Select * from ProductMaster where ProductID=@ProductID
	 Select * from ProductCategories where SBranchID=@SBranchID
END
GO
Create  Procedure [dbo].[sp_GetProducts]
(
@SBranchID int,
@CategoryID int
)
AS
BEGIN
	if(@CategoryID=0)
	begin
		select @CategoryID = min(ID) from  ProductCategories where SBranchID=@SBranchID
	end
	Select ProductID,P.Name,Photo,MRP,Price,P.Status,Quantity,MinQty,PC.Name as ProductCategoryName
	 from ProductMaster P left outer join ProductCategories PC on P.ProductCategoryID=PC.ID
	 where P.SBranchID=@SBranchID and P.ProductCategoryID=@CategoryID

	 Select ID,Name from ProductCategories where SBranchID=@SBranchID

	 select isnull(@CategoryID,0)
END

GO
CREATE  Procedure [dbo].[sp_GetStockTransactionDetails]
(
@STID int,
@SBranchID int
)
AS
BEGIN
		declare @ClassID int=0,@SessionID int=0,@SectionID int=0,@RefID int=0,@RefType int=0,@EmployeeTypeID int=0,@TrType int=0,@VendorID int=0

		if(@STID!=0)
		begin
			Select @ClassID=ClassID,@SessionID=SessionID,@SectionID=SectionID,@RefID=RefID,@RefType=RefType,@EmployeeTypeID=EmployeeTypeID,@TrType=TrType,@VendorID=VendorID from StockTransactionMaster where STID=@STID
		end
		if(@TrType=0 and @VendorID=0)
		begin
			select @VendorID=min(VendorID) from VendorMaster where SBranchID=@SBranchID
		end
		else if(@RefType=0 and @RefID=0)
		begin
			select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
			select @ClassID=min(ClassID) from ClassMaster where SBranchID=@SBranchID
			select @SectionID=min(ID) from Class_Sections where ClassID=@ClassID
		end
		else if(@RefType=1 and @RefID=0)
		begin
			Select @EmployeeTypeID=min(EmployeeTypeID) from EmployeeTypeMaster where SBranchID=@SBranchID or SBranchID=0
			select top 1 @RefID=EmployeeID from EmployeeMaster where SBranchID=@SBranchID and EmployeeType=@EmployeeTypeID order by EmployeeName 
		end

		Select * from StockTransactionMaster where STID=@STID

		Select STDID,STID,STType,ProductID,Quantity,SBranchID,Cost,MRP,SGST,CGST,IGST,
		(Select Name from ProductMaster PM where STD.ProductID=PM.ProductID) as ProductName from StockTransactionDetails STD where STID=@STID

		select PM.ProductID,PM.Name,PM.MRP,PM.Price,PM.Quantity,PM.MinQty,PC.Name as CategoryName,PC.HSNCode,PC.SGST,PC.IGST,PC.CGST,
		PM.Quantity+STC.AvailableQty as Available
		from ProductMaster PM
		left outer join ProductCategories PC on PM.ProductCategoryID=PC.ID
		left outer join v_StockProductAvailability STC on STC.ProductID=PM.ProductID and PM.Status=1
		where PM.SBranchID=@SBranchID
		
		if(@TrType=0)
		begin
			select isnull(@VendorID,0)
			select VendorID,CompanyName,ContactPerson,StateID from VendorMaster where SBranchID=@SBranchID
		end
		else if(@RefType=0)
		begin
			select SessionID as ID, SessionName as Name, SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID

			select ClassID as ID,ClassName as Name from ClassMaster where SBranchID=@SBranchID

			select ID,Name from Class_Sections where ClassID=@ClassID

			select StudentID as ID, Name,SchoolUID as Extra1 from StudentMaster where StudentID in 
			(Select StudentID from Student_Session where SessionID=@SessionID and ClassID=@ClassID and SectionID=@SectionID)
			order by name

			select isnull(@SessionID,0)
			select isnull(@ClassID,0)
			select isnull(@SectionID,0)
		end
		else
		begin
			Select EmployeeTypeID as ID , EmployeeTypeName as Name from EmployeeTypeMaster where SBranchID=@SBranchID or SBranchID=0
			order by EmployeeTypeID

			if(@RefID=0)
			begin
				Select @EmployeeTypeID=min(EmployeeTypeID) from EmployeeTypeMaster where SBranchID=@SBranchID or SBranchID=0
				select top 1 @RefID=EmployeeID from EmployeeMaster where SBranchID=@SBranchID and EmployeeType=@EmployeeTypeID order by EmployeeName 
			end

			select EmployeeID as ID,EmployeeName as Name from EmployeeMaster where SBranchID=@SBranchID and EmployeeType=@EmployeeTypeID

			select isnull(@RefID,0)
			select isnull(@EmployeeTypeID,0)
		end
END
GO
GO
CREATE Procedure [dbo].[sp_GetStockTransactions]
(
@StartDate date,
@EndDate	date,
@TrType int,
@SBranchID int
)
AS
BEGIN
	Select STID,TrDate,TrType,RefID,RefType,Remark,Status,
	(Case when TrType=0 then (Select CompanyName from VendorMaster VM where VM.VendorID=STM.VendorID) else
	(Case when RefType=0 then (Select Name from StudentMaster where StudentID=RefID) else 
	(Select EmployeeName from EmployeeMaster where EmployeeID=RefID) end) end) as RefName,
	(Select sum(Quantity) from StockTransactionDetails STD where STD.STID=STM.STID) as Quantity,
	(select sum((Quantity*Cost) +Quantity*Cost*SGST/100+Quantity*Cost*CGST/100+Quantity*Cost*IGST/100)
	from StockTransactionDetails STD  where STD.STID=STM.STID) as Amount
	 from StockTransactionMaster STM
	where cast(TrDate as date) between @StartDate and @EndDate and TrType=@TrType and SBranchID=@SBranchID
END
GO
DROP proc [dbo].[sp_GetStudentAdmitCard]
GO
CREATE proc [dbo].[sp_GetStudentAdmitCard]
(
@ClassID int,
@SectionID int,
@SBranchID int,
@EvaluationID int,
@SessionID int
)
AS
BEGIN
	declare @SchemeID int
	if(@SessionID=0)
	Begin
		Select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and SessionStatus=1
	End
	if(@SectionID=0)
	begin
		select @ClassID =min(ClassID) from ClassMaster where Status=1 and SBranchID=@SBranchID
		select @SectionID=min(ID) from Class_Sections where ClassID=@ClassID
	end
	declare @ELID int
	if(@EvaluationID=0)
	begin

		Select @ClassID=min(ClassID) from ClassMaster where Status=1 and SBranchID=@SBranchID
		select @SchemeID=SchemeID from [dbo].[ClassSessionDetails] where ClassID=@ClassID and SessionID=@SessionID
		Select @EvaluationID=min(EvaluationID) from EvaluationMaster EM where SBranchID=@SBranchID and
		EvaluationSchemeID=@SchemeID and EvaluationID not in (Select isnull(MasterID,0) from EvaluationMaster)
		Select @ELID =EducationLevelID from ClassMaster where ClassID=@ClassID

		Select @SessionID=SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
	end
	else
	begin
		Select @ELID =EducationLevelID from ClassMaster where ClassID=@ClassID
		select @SchemeID=SchemeID from [dbo].[ClassSessionDetails] where ClassID=@ClassID and SessionID=@SessionID
	end

	declare @EvaMonth nvarchar(2)
	declare @EvaYear nvarchar(4)

	select @EvaMonth=[EvaluationMonth] from [dbo].[EvaluationMaster] where EvaluationID=@EvaluationID

	select @EvaYear= YEAR(getdate())

	Select EvaluationID,
	(case when isnull(MasterID,0)=0 then EvaluationName else
	(Select EvaluationName from EvaluationMaster ED where ED.EvaluationID=EM.MasterID)+'->'+EvaluationName end) as EvaluationName
	from EvaluationMaster EM where SBranchID=@SBranchID and EvaluationSchemeID=@SchemeID and EvaluationID not in (Select isnull(MasterID,0) from EvaluationMaster)

	Select ClassID,ClassName from ClassMaster where Status=1 and SBranchID=@SBranchID
	Select ID,Name from Class_Sections where ClassID=@ClassID

	select EM.ExamID,SubM.SubjectID,SubM.SubjectName,EM.ExamDate,
	cast(isnull(EM.StartTime,'09:00:00') as nvarchar(8)) as StartTime , cast(isnull(EM.EndTime,'10:30:00') as nvarchar(8)) as EndTime
	from SubjectMasterAll SubM left outer join ExamMaster EM
	on EM.SubjectID=SubM.SubjectID  and EM.ClassID=@ClassID
	and EM.SBranchID=@SBranchID and EM.EvaluationID=@EvaluationID
	where EM.IsApplicable=1
	order by ExamDate asc

	Select SessionID,SessionName,SessionStatus from SessionMaster where SBranchID=@SBranchID
	select isnull(@EvaluationID,0)
	select isnull(@ClassID,0)
	select isnull(@SectionID,0)
	select isnull(@SessionID,0)

	declare @ClassName nvarchar(50)
	declare @SectionName nvarchar(50)
	declare @SessionName nvarchar(50)
	declare @EvaluationName nvarchar(50)
	select @ClassName=ClassName from ClassMaster where ClassID=@ClassID
	select @SectionName=Name from Class_Sections where ID=@SectionID
	select @SessionName=SessionName from SessionMaster where SessionID=@SessionID
	Select @EvaluationName=
	(case when isnull(MasterID,0)=0 then EvaluationName else
	(Select EvaluationName from EvaluationMaster ED where ED.EvaluationID=EM.MasterID)+'->'+EvaluationName end)
	from EvaluationMaster EM where EvaluationID=@EvaluationID

	Select SS.StudentSessionUID, SS.RollNo, SM.Name, SM.StudentID,SM.Gender,SM.Photo,PM.FatherName,PM.MotherName,
	@ClassName as ClassName,@SectionName as SectionName,@SessionName as SessionName,@EvaluationName as EvaluationName
	,'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6)  as StudentSID
	from StudentMaster SM right outer join Student_Session SS on SS.StudentID=SM.StudentID
	left outer join ParentMaster PM on PM.ParentID=SM.ParentID
	Where SS.SectionID=@SectionID and SS.SessionID=@SessionID
	order by SM.Name

	select * from SBranchMaster where SBranchID=@SBranchID

end
GO
ALTER procedure [dbo].[sp_GetStudentFeeDetailsNew]--250,1,1,'2020-02-01','2020-02-12'
(
@StudentID int,
@SBranchID int,
@SessionID int,
@QDate date,
@CurDate date
)
as 
BEGIN
	Declare @ClassID int
	Declare @SectionID int
	Declare @GroupID int
	Declare @TransportFeeMode int
	Declare @HostelFeeMode int
	declare @LastPayDay nvarchar(2)

	Select @TransportFeeMode=details from MasterSettings where Type='TransportFeeMode' and SBranchID=@SBranchID
	Select @HostelFeeMode=details from MasterSettings where Type='HostelFeeMode' and SBranchID=@SBranchID
	Select @LastPayDay=details from MasterSettings where Type='FeePaymentReminderDate' and SBranchID=@SBranchID


	Declare @SessionStartDate date,@SessionEndDate Date

	declare @CMonth int=datepart(month,@QDate),@CYear int=datepart(year,@QDate)
	
	
	select @SessionStartDate=SessionStartDate,@SessionEndDate=SessionEndDate from SessionMaster where SessionID=@SessionID

	Declare @StuSessionSDate date,@StuSessionEDate date,@QuotaID int,@IsAdmissionFee int,@FeePaymentMode int,
	@FeeAmount numeric(10,2),@PreviousDue numeric(10,2),@LateFee numeric(10,2),@Discounts numeric(10,2)
	,@LMonth int,@LYear int,@MonthType int,@BaseDate date,@Paid numeric(10,2),@IsCustomFee int,@StudentSessionUID int

	select top 1 @ClassID=SS.ClassID,@FeePaymentMode=FeePaymentMode,@StuSessionSDate=SS.FromDate,
	@QuotaID=SS.QuotaID,@SectionID=SS.SectionID,@StuSessionSDate=(Case when @SessionStartDate<SS.FromDate then SS.FromDate else @SessionStartDate end),
	@StuSessionEDate=(Case when @SessionEndDate>SS.ToDate then SS.ToDate else @SessionEndDate end),
	@IsAdmissionFee=SS.IsAdmissionFeeApplicable,@IsCustomFee=SS.IsCustomFee,@StudentSessionUID=StudentSessionUID
	from Student_Session SS 
	where SS.StudentID=@StudentID and SessionID=@SessionID and @QDate between FromDate and ToDate
	
	
	select @GroupID=GroupID from Class_Sections where ID=@SectionID

	Declare @DiscountApproved table(FeeTypeID int,ApprovedAmount decimal(18,2),FeeMonth int,FeeYear int)

	Insert into @DiscountApproved 
	select FeeTypeID,ApprovedAmount,FeeMonth,FeeYear from [FeeDiscountRequestDetails] FRD
	left outer join  [FeeDiscountRequestMaster] FRM on FRD.DiscRequestID=FRM.DiscRequestID
	where FRM.Status=1 and isnull(ApprovedAmount,0)<>0 and FRM.StudentID=@StudentID

	Declare @QuotaDiscounts table (FeeTypeID int,Discount numeric(5,2))

	Declare @MTFT table (MonthTypeID int,FeeTypeID int)
	insert into @MTFT 
	select MonthTypeID,FeeTypeID from [MonthTypeFeeType]

	
	Declare @NoFeeMonths table (Month int)

	insert into @NoFeeMonths
	select Month from SessionClassNoFeeMonths where ClassID=@ClassID and SessionID=@SessionID
	
	Declare @FeeStructureTable table (FeeTypeID int,FeeTypeName nvarchar(50),FeeTypeApplicable int,FeeAmount numeric(10,2),Status int,Months nvarchar(50))
	
	Declare @FeeDetailTable table (FeeMonth int,FeeYear int, FeeTypeID int,FeeTypeName nvarchar(100),FeeAmount numeric(10,2),QDiscount numeric(10,2),RDiscount numeric(10,2),
	PayApplicableAmount numeric(10,2),CustomFee numeric(10,2),PaidAmount numeric(10,2),IsCustomFee int,IsPayment int,FeeTypeApplicable int)

	insert into @FeeStructureTable
	select FTM.FeeTypeID,FTM.FeeTypeName,FTM.FeeTypeApplicable,CFS.FeeAmount,CFS.Status,FTM.Months
	from FeeTypeMaster FTM left outer join [dbo].[ClassFeeStructureMaster] CFS on CFS.FeeTypeID=FTM.FeeTypeID and ClassID=@ClassID and GroupID=@GroupID
	and CFS.SessionID=@SessionID
	
		Insert into @QuotaDiscounts select FeeTypeID,DiscPer from QuotaDiscountDetails where SessionID=@SessionID and QuotaID=@QuotaID
		set @BaseDate=@StuSessionSDate
		declare @LoopDate date=@QDate
		if(@QDate>@StuSessionEDate)
		begin
			set @LoopDate=@StuSessionEDate
		end
		else if((DATEDIFF(month,dateadd(month,-1,@SessionStartDate),@LoopDate))%@FeePaymentMode>0)
		begin
			declare @adjustMonth int=@FeePaymentMode-(DATEDIFF(month,dateadd(month,-1,@SessionStartDate),@LoopDate))%@FeePaymentMode
			set @LoopDate=DateAdd(month,@adjustMonth, @LoopDate)
		end
		while(datepart(year,@StuSessionSDate)*12+datepart(month,@StuSessionSDate)<=datepart(year,@LoopDate)*12+datepart(month,@LoopDate))
		begin
			set @LMonth = datepart(month,@StuSessionSDate)
			set @LYear = datepart(year,@StuSessionSDate)
			if((select count(*) from @NoFeeMonths where Month=@LMonth)=0)
			begin
				declare @PayDate date
				select @PayDate=min(PaymentDate) from PaymentDetails where PayeeID=@StudentID and Month=@LMonth and Year=@LYear and isnull(PaymentStatus,0)=0
				declare @IsLatePay int=0
				if(@PayDate is not null and @PayDate>cast(cast(@LYear as nvarchar(5))+'-'+cast(@LMonth as nvarchar(2))+'-'+@LastPayDay as date))
				begin
					set @IsLatePay=1
				end
				else if(@PayDate is null and @CurDate>cast(cast(@LYear as nvarchar(5))+'-'+cast(@LMonth as nvarchar(2))+'-'+@LastPayDay as date))
				begin
					set @IsLatePay=1
				end
				else
				begin
					set @IsLatePay=0
				end
				select @MonthType=(Case when datepart(month,@BaseDate)=@LMonth and (datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or 
						datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth) then 5 else
						(Case when datepart(month,@BaseDate)=@LMonth and (datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth) then 6 else 
						(case when datepart(month,@BaseDate)=@LMonth then (case when @IsAdmissionFee=0 then 1 else 0 end)  
						else (Case when datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth then 3
						else (case when  datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth then 4 else 2 end) end) end)end)end)
				--Update Discount Requested Details
					
				declare @TPaid numeric(10,2)=0
				declare @CLateFee numeric(10,2)=0
				declare @CPaid numeric(10,2)=0

				
				insert into @FeeDetailTable
				select @LMonth as FeeMonth,@LYear as FeeYear,FTS.FeeTypeID,FTS.FeeTypeName,
				(case when FTS.FeeTypeApplicable=5 then [dbo].[fn_GetStudentTransportFeeAmount](@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID) else FTS.FeeAmount end) as FeeAmount,
				QD.Discount as QuotaDiscount,
				(case when FTS.FeeTypeApplicable=7 then PD.DiscAmt when PD.PaymentID is not null then PD.DiscAmt else DA.ApprovedAmount end),PD.NetApplicablePayment,SFD.FeeAmount,PD.PaymentRecieved,
				(case when @IsCustomFee is null then 0 else 1 end) as IsCustomFee,
				(case when PD.PaymentID is null then 0 else 1 end) as IsPayment,FTS.FeeTypeApplicable
				from @FeeStructureTable FTS left outer join @QuotaDiscounts QD on QD.FeeTypeID=FTS.FeeTypeID
				left outer join [dbo].[StudentFeeDetails] SFD on SFD.StudentID=@StudentID and SFD.SessionID=@StudentSessionUID and SFD.FeeTypeID=FTS.FeeTypeID
				left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
				left outer join @DiscountApproved DA on DA.FeeTypeID=FTS.FeeTypeID and DA.FeeMonth=@LMonth and DA.FeeYear=@LYear
				where FTS.FeeTypeApplicable in (select FeeTypeID from @MTFT where MonthTypeID=@MonthType) 
				and FTS.FeeTypeID <> (case when @IsLatePay=0 then -2 else 0 end) and FTS.FeeTypeApplicable<>(Case when @IsAdmissionFee=0 then 8 else 0 end)
				and isnull(SFD.IsApplicable,FTS.Status)=1 and (FTS.FeeTypeApplicable<>9 or (select count(*) from [dbo].[SplitStringToTable](FTS.Months,',') where Item=@LMonth)>0)

			end
			set @StuSessionSDate=dateadd(month,1,@StuSessionSDate)
		end

		--select * from @FeeDetailTable 
		
		select FeeMonth,FeeYear,FeeTypeApplicable,FeeTypeID,FeeTypeName,sum(FeeAmount) as FeeAmount,avg(QDiscount) as QDiscount,sum(RDiscount) as RDiscount,sum(PayApplicableAmount) as PayApplicableAmount
		,sum((Case when FeeTypeApplicable=7 then FeeAmount else CustomFee end)) as CustomFee,sum(PaidAmount) as PaidAmount,max(IsCustomFee) as IsCustomFee,max(IsPayment) as IsPayment from
		(Select 	datepart(month,dateadd(month,-Diff,cast(cast(FeeYear as nvarchar(5))+'-'+cast(FeeMonth as nvarchar(2))+'-1' as date))) as FeeMonth,
		datepart(year,dateadd(month,-Diff,cast(cast(FeeYear as nvarchar(5))+'-'+cast(FeeMonth as nvarchar(2))+'-1' as date))) as FeeYear,
		FeeTypeApplicable,FeeTypeID,FeeTypeName,FeeAmount,QDiscount,RDiscount,PayApplicableAmount,CustomFee,PaidAmount,IsCustomFee,IsPayment
		from
		(select *,(DATEDIFF(month,@SessionStartDate,cast(cast(FeeYear as nvarchar(5))+'-'+cast(FeeMonth as nvarchar(2))+'-1' as date))%@FeePaymentMode) as Diff
		from @FeeDetailTable)t)t2
		group by FeeMonth,FeeYear,FeeTypeApplicable,FeeTypeID,FeeTypeName
		order by FeeYear,FeeMonth,case when FeeTypeApplicable=7 then 0 else FeeTypeID end,FeeTypeName
END
GO
DROP  Procedure [dbo].[sp_GetStudentSessionCustomFee]
GO
CREATE Procedure [dbo].[sp_GetStudentSessionCustomFee] --9,1
(
@StudentSessionUID int,
@SBranchID int
)
as
BEGIN
	declare @TransportFeeMode int=0
	Select @TransportFeeMode=details from MasterSettings where Type='TransportFeeMode' and SBranchID=@SBranchID

	Declare @SessionID int,@SectionID int,@QuotaID int,@ClassID int,@IsAdmissionFeeApplicable int,@GroupID int,@IsCustomFee int,@StudentID int

	select top 1 @SessionID=SessionID,@QuotaID=QuotaID,@ClassID=ClassID,@SectionID=SectionID,@IsAdmissionFeeApplicable=IsAdmissionFeeApplicable,
	@IsCustomFee=IsCustomFee,@StudentID=StudentID
	from Student_Session where StudentSessionUID=@StudentSessionUID order by Status desc,FromDate desc

	select @GroupID=GroupID from Class_Sections where ID=@SectionID

	Select @StudentSessionUID as SessionID,StudentID,SFD.SFID,SFD.IsApplicable,FTM.FeeTypeID,FTM.FeeTypeName,isnull(SFD.FeeAmount,(CFS.FeeAmount-CFS.FeeAmount*isnull(QD.DiscPer,0)/100)) as FeeAmount,
	(CFS.FeeAmount-CFS.FeeAmount*isnull(QD.DiscPer,0)/100) as ClassFee,FTM.FeeTypeApplicable
	from FeeTypeMaster FTM left outer join [ClassFeeStructureMaster] CFS on CFS.FeeTypeID=FTM.FeeTypeID and CFS.SessionID=@SessionID and CFS.ClassID=@ClassID and CFS.GroupID=@GroupID
	left outer join StudentFeeDetails SFD on SFD.FeeTypeID=FTM.FeeTypeID and SFD.SessionID=@StudentSessionUID and SFD.StudentID=@StudentID
	left outer join [QuotaDiscountDetails] QD on QD.QuotaID=@QuotaID and QD.FeeTypeID=FTM.FeeTypeID and QD.SessionID=@SessionID
	where FTM.SBranchID=@SBranchID and FTM.FeeTypeApplicable not in (7) and FTM.FeeTypeApplicable <>(case when @TransportFeeMode=0 then 5 else 0 end)

	select isnull(@IsCustomFee,0)
END
GO
Create Procedure [dbo].[sp_GetStudentsForStockTransaction]
(
@STID int,
@SBranchID int
)
AS
BEGIN
		declare @ClassID int=0,@SessionID int=0,@SectionID int=0,@RefID int=0
		declare @VendorID int=0
		if(@STID!=0)
		begin
			Select @ClassID=ClassID,@SessionID=SessionID,@SectionID=SectionID,@RefID=RefID from StockTransactionMaster where STID=@STID
		end
		if(@RefID=0)
		begin
			select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
			select @ClassID=min(ClassID) from ClassMaster where SBranchID=@SBranchID
			select @SectionID=min(ID) from Class_Sections where ClassID=@ClassID
		end
		select SessionID as ID, SessionName as Name, SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID

		select ClassID as ID,ClassName as Name from ClassMaster where SBranchID=@SBranchID

		select ID,Name from Class_Sections where ClassID=@ClassID

		select StudentID as ID, Name,SchoolUID as Extra1 from StudentMaster where StudentID in 
		(Select StudentID from Student_Session where SessionID=@SessionID and ClassID=@ClassID and SectionID=@SectionID)
		order by name

		select isnull(@SessionID,0)
		select isnull(@ClassID,0)
		select isnull(@SectionID,0)
END
GO
GO
CREATE Procedure [dbo].[sp_GetTeacherBlackBoardEditData] 
(
	@TeacherID int,
	@EntryDate datetime,
	@SubjectID int,
	@ClassID int,
	@SectionID int,
	@PeriodID int
)
AS
BEGIN
	Declare @TimePart time,@EduID int,@BlackBoardID nvarchar(50)
	declare @DayName nvarchar(10)=datename(dw,@EntryDate)
	set @TimePart=cast(@EntryDate as time)
	if(@DayName='Sunday')
	begin
		set @EntryDate = dateadd(day,-1,@EntryDate)
		set @DayName =datename(dw,@EntryDate)
	end
	if(@PeriodID=0)
	begin
		select top 1 @PeriodID=PeriodID,@EduID=EducationLevelID from PeriodMaster PM where @TimePart between Starttime and dateadd(minute,5,EndTime)
		and PM.EducationLevelID in (select EducationLevelID from Teacher_Subject where EmployeeID=@TeacherID)

		declare @SQL nvarchar(max)
		set @SQL='select @SubjectID='+@DayName+'SubjectID,@ClassID=ClassID,@SectionID=SectionID from Time_Table_Master where '+@DayName+'TeacherID='+cast(@TeacherID as nvarchar(100))+' and
		PeriodID='+cast(@PeriodID as nvarchar(100))+' and ClassID in (select ClassID from ClassMaster where EducationLevelID='+cast(@EduID as nvarchar(100))+')'
		EXECUTE sp_executesql @SQL, N'@SubjectID INTEGER OUTPUT,@ClassID INTEGER OUTPUT,@SectionID INTEGER OUTPUT', @SubjectID OUTPUT,@ClassID OUTPUT,@SectionID OUTPUT
		
	end

	select @BlackBoardID=BlackBoardID from BlackBoardMaster where TeacherID=@TeacherID and ClassID=@ClassID and 
	SectionID=@SectionID and PeriodID=@PeriodID and SubjectID=@SubjectID and Cast(BDate as date)=cast(@EntryDate as date)

	select @SubjectID as SubjectID,@ClassID as ClassID,@SectionID as SectionID, @PeriodID as PeriodID,@BlackBoardID as BlackBoardID,@EntryDate as EntryDate

	declare @ListSQL nvarchar(max)
	set @ListSQL='select TTM.'+@DayName+'SubjectID as SubjectID,TTM.PeriodID,TTM.ClassID,TTM.SectionID,
		PM.Name+'' (''+cast(PM.StartTime as nvarchar(5))+''-''+ cast(PM.EndTime as nvarchar(5))+'')'' as PeriodName,
		CM.ClassName,CS.Name as SectionName,SM.SubjectName from 
		Time_Table_Master TTM left outer join PeriodMaster PM on PM.PeriodID=TTM.PeriodID
		left outer join ClassMaster CM on CM.ClassID=TTM.ClassID
		left outer join Class_Sections CS on CS.ID=TTM.SectionID
		left outer join SubjectMaster SM on SM.SubjectID=TTM.'+@DayName+'SubjectID
		where '+@DayName+'TeacherID='+Cast(@TeacherID as nvarchar(50))
	exec(@ListSQL)
END
GO
CREATE Procedure [dbo].[sp_GetTeacherBlackBoardEntryList] 
(
	@TeacherID int,
	@EntryDate datetime,
	@SubjectID int,
	@ClassID int,
	@SectionID int,
	@PeriodID int,
	@PageID int=0
)
AS
BEGIN
	Declare @BlackBoardID nvarchar(50)
	

	select @BlackBoardID=BlackBoardID from BlackBoardMaster where TeacherID=@TeacherID and ClassID=@ClassID and 
	SectionID=@SectionID and PeriodID=@PeriodID and SubjectID=@SubjectID and Cast(BDate as date)=cast(@EntryDate as date)

	select @SubjectID as SubjectID,@ClassID as ClassID,@SectionID as SectionID, @PeriodID as PeriodID,@BlackBoardID as BlackBoardID,@EntryDate as EntryDate
	
	SELECT *
	FROM ( select BBM.*,
	PM.Name+'('+cast(PM.StartTime as nvarchar(5))+'-'+ cast(PM.EndTime as nvarchar(5))+')' as PeriodName,
	[dbo].[GetBlackBoardImageList](BBM.BlackBoardID) as Images, ROW_NUMBER() OVER (ORDER BY BDate desc) AS RowNum
	from dbo.BlackBoardMaster BBM 
	left outer join PeriodMaster PM on PM.PeriodID=BBM.PeriodID
	where BBM.ClassID=@ClassID and BBM.SectionID=@SectionID and BBM.SubjectID=@SubjectID and BBM.TeacherID=@TeacherID
	)t where t.rownum between @PageID*20 and (@PageID+1)*20
END
GO
ALTER  PROCEDURE [dbo].[sp_GetUserByLoginName]  
  (
	@LoginName nvarchar(50)
  )
AS BEGIN  
	declare @UType int
	declare @UID int
	select @UType=UserType,@UID=UserID from LoginDetails where (CONVERT(nvarchar(50),UserID)= @LoginName or EmailID=@LoginName or UserName=@LoginName) 

	if(@UType=2)
	--Accountant Login
	begin
	 SELECT UserID,UserName, [Password],isnull(UserType,2) as RoleID,1 as IsApproved,'Account' as FullName,UD.SBranchID,
	 BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName
		FROM LoginDetails UD left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID where UserType=@UType and UserID=@UID
	end
	else if(@UType=3)
	--Employee Login
	begin
	 SELECT UserID,UserName, [Password],isnull(UserType,3) as RoleID,1 as IsApproved,EM.EmployeeName as FullName,EM.Photo as UserImage,UD.SBranchID,
	 BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName
		FROM LoginDetails UD left outer join EMployeeMaster EM on EM.EmployeeID=UD.UserID
		left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID
		  where UD.UserType=@UType and UD.UserID=@UID
	end
	else if(@UType=4)
	--Parent Login
	begin
	 SELECT UserID,UserName, [Password],isnull(UserType,4) as RoleID,1 as IsApproved,EM.FatherName as FullName,EM.FatherImage as UserImage,UD.SBranchID,
	 BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName
		FROM LoginDetails UD left outer join ParentMaster EM on EM.ParentID=UD.UserID
		left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID
		  where UD.UserType=@UType and UD.UserID=@UID
	end
	else if(@UType=5)
	--Library Login
	begin
	 SELECT UserID,UserName, [Password],isnull(UserType,5) as RoleID,1 as IsApproved,isnull(EM.EmployeeName,'Library') as FullName,EM.Photo as UserImage,UD.SBranchID,
	 BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName
		FROM LoginDetails UD left outer join EMployeeMaster EM on EM.EmployeeID=UD.UserID
		left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID
		  where UD.UserType=@UType and UD.UserID=@UID
	end
	else if(@UType=6)
	--Student Login
	begin
	 SELECT UserID,UserName, [Password],isnull(UserType,6) as RoleID,1 as IsApproved,EM.Name as FullName,EM.Photo as UserImage,UD.SBranchID,
	 BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName
		FROM LoginDetails UD left outer join StudentMaster EM on EM.StudentID=UD.UserID
		left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID
		  where UD.UserType=@UType and UD.UserID=@UID
	end
	else if(@UType=8)
	--Principal Login
	begin
	 SELECT UserID,UserName, [Password],isnull(UserType,6) as RoleID,1 as IsApproved,'Principal' as FullName,UD.SBranchID,
	BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName
		FROM LoginDetails UD left outer join EMployeeMaster EM on EM.EmployeeID=UD.UserID
		left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID
		  where UD.UserType=@UType and UD.UserID=@UID
	end
	else if(@UType=10)
	--Reception Login
	begin
	 SELECT UserID,UserName, [Password],isnull(UserType,10) as RoleID,1 as IsApproved,isnull(EM.EmployeeName,'Reception') as FullName,EM.Photo as UserImage,UD.SBranchID,
	 BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName
		FROM LoginDetails UD left outer join EMployeeMaster EM on EM.EmployeeID=UD.UserID
		left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID
		  where UD.UserType=@UType and UD.UserID=@UID
	end
	else
	begin
	declare @SBranchID int
	select @SBranchID=Min(SBranchID) from SBranchMaster
	 SELECT UserID,UserName, [Password],isnull(UserType,2) as RoleID,1 as IsApproved,
	 (@SBranchID) as SBranchID,BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName
		FROM LoginDetails UD left outer join EMployeeMaster EM on EM.EmployeeID=UD.UserID
		left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID
	  where (CONVERT(nvarchar(50),UserID)= @LoginName or UD.EmailID=@LoginName or UserName=@LoginName) 
  end
END
GO
GO
Create Procedure [dbo].[sp_GetVendorDetails]
(
@SBranchID int,
@VendorID int
)
AS
BEGIN
		select VM.* from VendorMaster VM 
		where VendorID=@VendorID

		select * from GSTStateMaster
END
GO
CREATE Procedure [dbo].[sp_GetVendorsForStockTransaction]
(
@STID int,
@SBranchID int
)
AS
BEGIN
		declare @VendorID int=0
		if(@STID!=0)
		begin
			Select @VendorID=VendorID from StockTransactionMaster where STID=@STID
		end
		if(@VendorID=0)
		begin
			select @VendorID=min(VendorID) from VendorMaster where SBranchID=@SBranchID
		end
		select isnull(@VendorID,0)
		select VendorID,CompanyName,ContactPerson,StateID from VendorMaster where SBranchID=@SBranchID
END
GO
CREATE procedure [dbo].[sp_InsertBlackBoardEntry]
(
	@BlackBoardID nvarchar(50),
	@TeacherID int,
	@SectionID int,
	@ClassID int,
	@PeriodID int,
	@SubjectID int,
	@Title nvarchar(MAX),
	@BDate datetime,
	@SBranchID int
)
AS
BEGIN
	Insert into BlackBoardMaster(BlackBoardID,TeacherID,SectionID,ClassID,PeriodID,SubjectID,Title,BDate,SBranchID)
	Values(@BlackBoardID,@TeacherID,@SectionID,@ClassID,@PeriodID,@SubjectID,@Title,@BDate,@SBranchID)
	select 1
END
GO

Create procedure [dbo].[sp_InsertBlackBoardImage]
(
	@BlackBoardID nvarchar(50),
	@BBMIID nvarchar(50),
	@Photo nvarchar(50),
	@Detail nvarchar(MAX),
	@SBranchID int
)
AS
BEGIN
	Insert into BlackBoardImages(BlackBoardID,BBMIID,Photo,Detail,SBranchID)
	Values(@BlackBoardID,@BBMIID,@Photo,@Detail,@SBranchID)
END
GO
GO
CREATE procedure [dbo].[sp_InsertEmployeeLeaveNew]
(
@LeaveID int,
@ApplicantType int,
@LeaveType int,
@StartDate datetime,
@EndDate datetime,
@LeaveReason nvarchar(MAX),
@ApplicantID int,
@IsApproved int,
@UserID int,
@CreatedDate datetime,
@SBranchID int,
@EmployeeType int,
@LeaveTypeApplied int,
@OpType int
)
as 
begin 
	if(@OpType=-1)
	begin
		Delete from LeaveMaster where LeaveID=@LeaveID
		Delete from LeaveDetails where LeaveID=@LeaveID
	end
	Else 
	Begin	
		if(@LeaveID=0)
		begin		
			Insert into LeaveMaster(ApplicantType,LeaveType,StartDate,EndDate,LeaveReason,ApplicantID,IsApproved,UserID,CreatedDate,SBranchID,EmployeeType,LeaveTypeApplied)
			values (@ApplicantType,@LeaveType,@StartDate,@EndDate,@LeaveReason,@ApplicantID,@IsApproved,@UserID,@CreatedDate,@SBranchID,@EmployeeType,@LeaveTypeApplied)
			Select @LeaveID=cast(Scope_Identity() as int)
		end
		else
		begin
			Update LeaveMaster set ApplicantType=@ApplicantType,LeaveType=@LeaveType,StartDate=@StartDate,EndDate=@EndDate,LeaveTypeApplied=@LeaveTypeApplied,
			LeaveReason=@LeaveReason,ApplicantID=@ApplicantID,IsApproved=@IsApproved,EmployeeType=@EmployeeType where LeaveID=@LeaveID

			delete from LeaveDetails where LeaveID=@LeaveID
		end
		declare @LeaveSummery Table(LeaveTypeID int,LeaveTypeName nvarchar(50),YearlyQuota numeric(10,2),MonthlyQuota numeric(10,2),IsSalaryDeduct int
		,YearlyApplied numeric(10,2),MonthlyApplied numeric(10,2),Applied numeric(10,2),[Month] int,[Year] int,SequenceNo int,MaxConsecutive numeric(10,2))
		declare @LDStartMonth int=datepart(month,@StartDate),@LDEndMonth int=datepart(month,@EndDate),@AvailableLeaves numeric(10,2)
		declare  @LDStartYear int=datepart(year,@StartDate),@LDEndYear int=datepart(year,@EndDate)
		while(@LDStartYear*12+@LDStartMonth<=@LDEndYear*12+@LDEndMonth)
		begin
			Insert into @LeaveSummery(LeaveTypeID,LeaveTypeName,YearlyQuota,MonthlyQuota,IsSalaryDeduct,YearlyApplied,MonthlyApplied,Applied,SequenceNo,MaxConsecutive)
			exec sp_GetEmployeeLeaveDetailsNew @LeaveID,@ApplicantID,@LDStartMonth,@LDStartYear,@EmployeeType,0,@SBranchID			
			update @LeaveSummery set [Month]=@LDStartMonth,[Year]=@LDStartYear where [Month] is null
			set @LDStartMonth=@LDStartMonth+1
		end
		--sp_GetEmployeeLeaveDetailsNew 0,2,2,2020,1,0,1
		declare @LeaveAdded numeric(10,2)=0,@Increment numeric(10,2)=0,@MaxConsecutive numeric(10,2)=0

		if(@LeaveType!=1)
		begin
			select @AvailableLeaves=isnull(MonthlyQuota,0)-ISNULL(MonthlyApplied,0)-isnull(Applied,0),@MaxConsecutive=MaxConsecutive from @LeaveSummery
			where [Month]=datepart(month,@StartDate) and [Year]=datepart(year,@StartDate) and LeaveTypeID=@LeaveTypeApplied
			if(isnull(@AvailableLeaves,0)>0)
			begin
				insert into LeaveDetails(LeaveID,LeaveTypeID,Applied,LeaveDate) values(@LeaveID,@LeaveTypeApplied,0.5,@StartDate)
			end
			else
			begin
				insert into LeaveDetails(LeaveID,LeaveTypeID,Applied,LeaveDate) values(@LeaveID,0,0.5,@StartDate)
			end
		end
		else
		begin
			While(@StartDate<=@EndDate)
			Begin
				select @AvailableLeaves=isnull(MonthlyQuota,0)-ISNULL(MonthlyApplied,0)-isnull(Applied,0)-isnull(@LeaveAdded,0),@MaxConsecutive=MaxConsecutive 
				from @LeaveSummery where [Month]=datepart(month,@StartDate) and [Year]=datepart(year,@StartDate) and LeaveTypeID=@LeaveTypeApplied
				if(isnull(@LeaveAdded,0)<=@MaxConsecutive)
				Begin
					if(@LeaveAdded<=@AvailableLeaves)
					begin
						insert into LeaveDetails(LeaveID,LeaveTypeID,Applied,LeaveDate) values(@LeaveID,@LeaveTypeApplied,1,@StartDate)
						set @LeaveAdded=isnull(@LeaveAdded,0)+1
						set @Increment=1
					end
					else if(@AvailableLeaves=0.5)
					begin
						insert into LeaveDetails(LeaveID,LeaveTypeID,Applied,LeaveDate) values(@LeaveID,@LeaveTypeApplied,0.5,@StartDate)
						set @LeaveAdded=isnull(@LeaveAdded,0)+0.5
						set @Increment=@Increment+0.5
					end
					else
					begin
						insert into LeaveDetails(LeaveID,LeaveTypeID,Applied,LeaveDate) values(@LeaveID,0,1,@StartDate)
						set @LeaveAdded=isnull(@LeaveAdded,0)+1
						set @Increment=@Increment+1
					end
				End				
				else
				begin
					insert into LeaveDetails(LeaveID,LeaveTypeID,Applied,LeaveDate) values(@LeaveID,0,1,@StartDate)
					set @LeaveAdded=isnull(@LeaveAdded,0)+1
					set @Increment=@Increment+1
				end
				set @StartDate=dateadd(day,@Increment,@StartDate)
				if(@Increment=1)
				begin
					set @Increment=0
				end
			End
		end	
	End
end

GO
ALTER procedure [dbo].[sp_InsertUpdateLeaveType]
(
@LeaveTypeID int,
@LeaveTypeName nvarchar(50),
@YearlyQuota numeric(10,2),
@MonthlyQuota numeric(10,2),
@IsSalaryDeduct int,
@SBranchID int,
@IsCarryForward int,
@MaxConsecutive numeric(10,2),
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from LeaveTypeMaster where LeaveTypeID=@LeaveTypeID
	end
	else
	begin
		if(@LeaveTypeID=0)
		begin
			Insert into LeaveTypeMaster(LeaveTypeName,YearlyQuota,MonthlyQuota,IsSalaryDeduct,SBranchID,IsCarryForward,MaxConsecutive)
			values(@LeaveTypeName,@YearlyQuota,@MonthlyQuota,@IsSalaryDeduct,@SBranchID,@IsCarryForward,@MaxConsecutive)
			select @LeaveTypeID=cast(Scope_Identity() as int)
		end
		else
		begin
			update LeaveTypeMaster set LeaveTypeName=@LeaveTypeName,YearlyQuota=@YearlyQuota,MonthlyQuota=@MonthlyQuota,IsSalaryDeduct=@IsSalaryDeduct
			,IsCarryForward=@IsCarryForward,MaxConsecutive=@MaxConsecutive
			where LeaveTypeID=@LeaveTypeID
		end
	end
	select @LeaveTypeID
END
GO
GO
Create Procedure [dbo].[sp_UpdateAccountVendors]
(
@VendorID int,
@CompanyName nvarchar(50),
@ContactPerson nvarchar(50),
@ContactNumber nvarchar(50),
@Address nvarchar(MAX),
@StateID int,
@PinCode nvarchar(10),
@CompanyContact nvarchar(50),
@EmailID nvarchar(50),
@BranchName nvarchar(50),
@BankName nvarchar(50),
@AccountNumber nvarchar(50),
@AccountType int,
@IFSCCode nvarchar(50),
@OtherDetails nvarchar(MAX),
@GSTIN nvarchar(50),
@PANNumber nvarchar(50),
@TINNumber nvarchar(50),
@CINNumber nvarchar(50),
@RegistrationNumber nvarchar(50),
@SBranchID int,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from VendorMaster where VendorID=@VendorID
	end
	else
	begin
		if(@VendorID=0)
		begin
			Insert into VendorMaster(CompanyName,ContactPerson,ContactNumber,Address,StateID,PinCode,CompanyContact,EmailID,BranchName,BankName,AccountNumber,AccountType,
			IFSCCode,OtherDetails,GSTIN,PANNumber,TINNumber,CINNumber,RegistrationNumber,SBranchID)
			values(@CompanyName,@ContactPerson,@ContactNumber,@Address,@StateID,@PinCode,@CompanyContact,@EmailID,@BranchName,@BankName,@AccountNumber,@AccountType,
			@IFSCCode,@OtherDetails,@GSTIN,@PANNumber,@TINNumber,@CINNumber,@RegistrationNumber,@SBranchID)
			select @VendorID=Cast(Scope_Identity() as int)
		end
		else
		begin
			Update VendorMaster set CompanyName=@CompanyName,ContactPerson=@ContactPerson,ContactNumber=@ContactNumber,Address=@Address,StateID=@StateID,PinCode=@PinCode,
			CompanyContact=@CompanyContact,EmailID=@EmailID,BranchName=@BranchName,BankName=@BankName,AccountNumber=@AccountNumber,AccountType=@AccountType,IFSCCode=@IFSCCode,
			OtherDetails=@OtherDetails,GSTIN=@GSTIN,PANNumber=@PANNumber,TINNumber=@TINNumber,CINNumber=@CINNumber,RegistrationNumber=@RegistrationNumber
			where VendorID=@VendorID
		end
	end
	select @VendorID
END
GO
GO
ALTER procedure [dbo].[sp_UpdateLeaveStatus]
(
@LeaveID int,
@IsApproved int
)
as 
begin 
	update LeaveMaster set IsApproved=@IsApproved where LeaveID=@LeaveID
end

GO
Create Procedure [dbo].[sp_UpdateProduct]
(
@ProductID int,
@ProductCategoryID int,
@Name nvarchar(50),
@Photo nvarchar(50),
@MRP numeric(10, 2),
@Price numeric(10, 2),
@SBranchID int,
@Status int,
@Quantity int,
@MinQty int,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from ProductMaster where ProductID=@ProductID
	end
	else
	Begin
		if(@ProductID=0)
		begin
			Insert into ProductMaster(ProductCategoryID,Name,Photo,MRP,Price,SBranchID,Status,Quantity,MinQty)
				values(@ProductCategoryID,@Name,@Photo,@MRP,@Price,@SBranchID,@Status,@Quantity,@MinQty)
			select @ProductID=Cast(Scope_Identity() as int)
		end
		else
		begin
			Update ProductMaster set ProductCategoryID=@ProductCategoryID,Name=@Name,Photo=@Photo,MRP=@MRP,Price=@Price,SBranchID=@SBranchID,Status=@Status,Quantity=@Quantity,MinQty=@MinQty
			where ProductID=@ProductID
		end
	End
	select isnull(@ProductID,0)
END

GO
GO

CREATE Procedure [dbo].[sp_UpdateProductCategory]
(
@ID int,
@Name nvarchar(50),
@SBranchID int,
@HSNCode nvarchar(50),
@SGST numeric(10,2),
@CGST numeric(10,2),
@IGST numeric(10,2),
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from ProductCategories where ID=@ID
	end
	else
	Begin
		if(@ID=0)
		begin
			Insert into ProductCategories(Name,SBranchID,HSNCode,SGST,CGST,IGST)
				values(@Name,@SBranchID,@HSNCode,@SGST,@CGST,@IGST)
			select @ID=Cast(Scope_Identity() as int)
		end
		else
		begin
			Update ProductCategories set Name=@Name,HSNCode=@HSNCode,SGST=@SGST,CGST=@CGST,IGST=@IGST where ID=@ID
		end
	End
	select isnull(@ID,0)
END

GO
GO
CREATE Procedure [dbo].[sp_UpdateStockTransaction]
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
@OpType int,
@SBranchID int,
@Details ut_StockTransactionDetails READOnly,
@EmployeeTypeID int,
@VendorID int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from StockTransactionMaster where STID=@STID
		Delete from StockTransactionDetails where STID=@STID
	end
	else
	Begin
		if(@STID=0)
		Begin
			Insert into StockTransactionMaster(TrDate,TrType,RefID,RefType,Remark,CreatedDate,Status,SBranchID,ClassID,SectionID,SessionID,EmployeeTypeID,VendorID)
				values(@TrDate,@TrType,@RefID,@RefType,@Remark,@CreatedDate,@Status,@SBranchID,@ClassID,@SectionID,@SessionID,@EmployeeTypeID,@VendorID)
			Select @STID=cast(Scope_Identity() as int)

			INSERT INTO StockTransactionDetails(STID, STType,ProductID,Quantity,MRP,Cost,SBranchID,SGST,CGST,IGST)
			 SELECT @STID, @TrType,ProductID,Quantity,MRP,Cost,@SBranchID,SGST,CGST,IGST
			  FROM @Details PD where PD.STDID=0
		End
		else
		begin		
			update StockTransactionMaster set TrDate=@TrDate,TrType=@TrType,RefID=@RefID,RefType=@RefType,Remark=@Remark,Status=@Status
			,ClassID=@ClassID,SectionID=@SectionID,SessionID=@SessionID,EmployeeTypeID=@EmployeeTypeID,VendorID=@VendorID
			where STID=@STID

			Delete from StockTransactionDetails where STDID not in (Select STDID from @Details) and STID=@STID
			
			UPDATE e SET e.ProductID=d.ProductID,e.Quantity=d.Quantity,e.MRP=d.MRP,e.Cost=d.Cost,e.SGST=d.SGST,e.CGST=d.CGST,e.IGST=d.IGST
			 FROM  StockTransactionDetails e, @Details d 
			 WHERE d.STDID=e.STDID and d.STDID<>0

			INSERT INTO StockTransactionDetails(STID, STType,ProductID,Quantity,MRP,Cost,SBranchID,SGST,CGST,IGST)
			 SELECT @STID, @TrType,ProductID,Quantity,MRP,Cost,@SBranchID,SGST,CGST,IGST
			  FROM @Details PD where PD.STDID=0
		end
	End
	select isnull(@STID,0)
END

GO
ALTER  proc [dbo].[sp_UpdateStudentAttandance]
(
@ClassID int,
@SectionID int,
@Year nvarchar(5),
@Month  nvarchar(2),
@Day nvarchar(3),
@List AttandanceDetails READONLY,
@SBranchID int
)
AS
BEGIN

declare @DaysWorking int
declare @ID int
declare @Status numeric(10,2)
declare @IsExist int
declare @DeviceSerial nvarchar(50)
declare @SQLQuery nvarchar(max)
select @DaysWorking= [Days] from EducationLevelMaster where ID=(select  EducationLevelID from ClassMaster where ClassID=@ClassID)
DECLARE cur_List CURSOR
STATIC FOR
SELECT ID,Status,IsExist from @List
OPEN cur_List
IF @@CURSOR_ROWS > 0
BEGIN
FETCH NEXT FROM cur_List INTO @ID ,@Status,@IsExist
WHILE @@Fetch_status = 0
BEGIN
	if(@Status!=0)
	begin
		declare @AttEntryExist int
		select @AttEntryExist=count(*) from AttandanceDateTime where employeeID='STU'+RIGHT(REPLICATE('0',6)+CAST(@ID AS VARCHAR(6)),6) 
		and datepart(year,logdatetime)=@Year and  datepart(month,logdatetime)=@Month and  datepart(day,logdatetime)=replace(@Day, 'D', '')
		Select top 1 @DeviceSerial=DeviceSerial from [dbo].[AttandanceDeviceDetails] where SBranchID=@SBranchID
		if(@AttEntryExist=0)
		begin
			declare @StudentSSID nvarchar(12)
			set @StudentSSID='STU'+RIGHT(REPLICATE('0',6)+CAST(@ID AS VARCHAR(6)),6)
			declare @AttandanceDate datetime
			set @AttandanceDate=cast(@Month+'-'+replace(@Day, 'D', '')+'-'+@Year+' '+CONVERT(nvarchar(10), getdate(),108) as datetime)
			insert into   AttandanceDateTime(employeeid,logdatetime,serial_no,AttandanceType) values( @StudentSSID,@AttandanceDate,isnull(@DeviceSerial,''),1)
		end
	end
	select @IsExist=count(*) from StudentAttendanceMasterT where FYear=@Year and [Month]=@Month and StudentID=@ID
	if(@IsExist=1)
		begin
			set @SQLQuery='Update StudentAttendanceMasterT set '+@Day+'='+cast (@Status as nvarchar(5))+' where FYear='+@Year+' and Month='+@Month+' and StudentID='+cast (@ID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))
			exec(@SQLQuery)
		end
	else
		begin
			declare @DayNum int
			set @DayNum=1
			while(@DayNum<=31)
			begin
				declare @HCount int
				declare @LCount int
				declare @tStatus int
				set @tStatus=3
				if(isdate(@Month+'-'+cast(@DayNum as nvarchar(2))+'-'+@Year)!=0)
				begin
						Select @HCount=count(*) from HolidayMaster where cast(@Month+'-'+cast(@DayNum as nvarchar(2))+'-'+@Year as Date) between StartDate and EndDate and Status=1 and DayDuration=1 and 
						((select count(*) from [dbo].[SplitStringToTable](classes,',') where Item=@ClassID or ITem=0)>0)
						select @LCount=count(*) from LeaveMaster where cast(@Month+'-'+cast(@DayNum as nvarchar(2))+'-'+@Year as Date) between StartDate and EndDate and IsApproved=1 and ApplicantID=@ID and ApplicantType=1
							
						if(@HCount!=0)
						begin
							set @tStatus=2
						end
						else if (@LCount!=0)
						begin
							set @tStatus=-1
						end
				end
				if(@DayNum=1)
				begin
					if('D'+cast(@DayNum as nvarchar(3))=@Day)
					begin
						set @SQLQuery='Insert into StudentAttendanceMasterT(StudentID,SBranchID,ClassID,SectionID,FYear,Month,D'+cast(@DayNum as nvarchar(3))+') 
						Values ('+cast (@ID as nvarchar(5))+','+cast (@SBranchID as nvarchar(5))+','+cast (@ClassID as nvarchar(5))+','+cast (@SectionID as nvarchar(5))+','+@Year+','+@Month+','+cast(@Status as nvarchar(5))+')'					
					end
					else
					begin
					set @SQLQuery='Insert into StudentAttendanceMasterT(StudentID,SBranchID,ClassID,SectionID,FYear,Month,D'+cast(@DayNum as nvarchar(3))+') 
						Values ('+cast (@ID as nvarchar(5))+','+cast (@SBranchID as nvarchar(5))+','+cast (@ClassID as nvarchar(5))+','+cast (@SectionID as nvarchar(5))+','+@Year+','+@Month+','+cast(@tStatus as nvarchar(5))+')'
					end
					exec(@SQLQuery)
				end
				else
				begin
					if(isdate(@Month+'-'+cast(@DayNum as nvarchar(2))+'-'+@Year)=0)
					begin
						set @SQLQuery='Update StudentAttendanceMasterT set D'+cast(@DayNum as nvarchar(3))+'=-2 where FYear='+@Year+' and Month='+@Month+' and StudentID='+cast (@ID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))
						exec(@SQLQuery)
					end
					else
					begin
					
						declare @CurDate date
						set @CurDate= cast(@Month+'-'+cast(@DayNum as nvarchar(2))+'-'+@Year as Date)
						If datepart(dw, @CurDate) = 1 Set @tStatus=-2 
						if(@DaysWorking=6 and datepart(dw, @CurDate) = 7) Set @tStatus=-2 
						if('D'+cast(@DayNum as nvarchar(3))=@Day)
						begin
							set @SQLQuery='Update StudentAttendanceMasterT set D'+cast(@DayNum as nvarchar(3))+'='+cast (@Status as nvarchar(5))+' where FYear='+@Year+' and Month='+@Month+' and StudentID='+cast (@ID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))
							exec(@SQLQuery)
						end
						else
						begin
							set @SQLQuery='Update StudentAttendanceMasterT set D'+cast(@DayNum as nvarchar(3))+'='+cast (@tStatus as nvarchar(5))+' where FYear='+@Year+' and Month='+@Month+' and StudentID='+cast (@ID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))
							exec(@SQLQuery)
						end
					end					
				end
				set @DayNum=@DayNum+1
			end
		end
FETCH NEXT FROM cur_List INTO @ID ,@Status,@IsExist
END
END
CLOSE cur_List
DEALLOCATE cur_List

select 1
END

GO
ALTER proc [dbo].[sp_UpdateStudentCustomFee]
(
@StudentSessionUID int,
@IsCustomFee int,
@FeeDetails ut_StudentFeeDetails READONLY
)
AS
BEGIN
		update Student_Session set IsCustomFee=@IsCustomFee where StudentSessionUID=@StudentSessionUID

		UPDATE e SET   e.FeeAmount=d.FeeAmount,e.IsApplicable=d.IsApplicable
		 FROM  [dbo].StudentFeeDetails e, @FeeDetails d 
		 WHERE d.SFID=e.SFID and d.SFID<>0

		INSERT INTO  [dbo].StudentFeeDetails (StudentID, FeeTypeID,SessionID,FeeAmount,IsApplicable)
		 SELECT StudentID, FeeTypeID,SessionID,FeeAmount,IsApplicable FROM @FeeDetails PD where PD.SFID=0	
END
GO
GO
CREATE procedure [dbo].[spn_GetAdminLeaveDetails]
(
@LeaveID int
)
as 
begin 
	declare @ApplicantID int
	declare @StartDate date
	declare @SBranchID int
	declare @ApplicantType int
	declare @ClassID int
	declare @SectionID int

	select @ApplicantType=ApplicantType, @ApplicantID= ApplicantID,@SBranchID=SBranchID,@StartDate=StartDate from LeaveMaster where LeaveID=@LeaveID

	select *,(Case when ApplicantType=0 then (Select '('+EmployeeSID+') '+EmployeeName from v_EmployeeDriversBasicDetails EDB where EDB.EmployeeID=LM.ApplicantID and EDB.EmployeeType=LM.EmployeeType) 
	else (Select Name from StudentMaster where StudentID=ApplicantID) end) as 'ApplicantName' from LeaveMaster LM where LeaveID=@LeaveID

	select isnull(LM.LeaveTypeName,'Un Paid') as LeaveTypeName,LD.LeaveTypeID,sum(Applied) as Applied from LeaveDetails LD left outer join LeaveTypeMaster LM on LM.LeaveTypeID=LD.LeaveTypeID
	where LeaveID=@LeaveID
	group by LeaveTypeName,LD.LeaveTypeID

end
GO
ALTER  procedure [dbo].[spn_GetAllLeaves]
(
@ApplicantType int,
@Status int,
@SBranchID int
)
as 
begin 
	Select LeaveID,ApplicantType,LeaveType,StartDate,EndDate,LeaveReason,ApplicantID, 
	(Case when ApplicantType=0 then (Select '('+EmployeeSID+')'+EmployeeName from v_EmployeeDriversBasicDetails EDB where EDB.EmployeeID=LM.ApplicantID and EDB.EmployeeType=LM.EmployeeType) 
	else (Select Name from StudentMaster where StudentID=ApplicantID) end) as 'ApplicantName',IsApproved,EmployeeType
	from LeaveMaster LM where IsApproved=isnull(nullif(@Status,-1),IsApproved) and ApplicantType=isnull(nullif(@ApplicantType,-1),ApplicantType) and SBranchID=@SBranchID
	order by StartDate desc
end
GO
ALTER  proc [dbo].[spn_GetClasses]
(
@Status int,
@SBranchID int,
@SessionID int=0
)
AS
BEGIN
	if(@SessionID=0)
	begin
		Select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and SessionStatus=1
		
	end
if(@Status!=-1)
	begin
		Select CM.ClassID,CM.ClassName,CM.Status,CM.EducationLevelID,
		(Select Count(*) from Class_Sections CS where CS.ClassID=CM.ClassID  and CS.[Status]=1 ) as SectionCount,
		(Select count(*) from Student_Session SS where SS.ClassID=CM.ClassID and SS.SessionID=@SessionID and Status=1) as StudentCount,
		(Select SchemeID from ClassSessionDetails CSD where CSD.ClassID=CM.ClassID and CSD.SessionID=@SessionID) as EvaluationSchemeID
		 from ClassMaster CM where CM.[Status]=@Status and CM.SBranchID=@SBranchID
		 		 		
	end
	else
	begin
	Select CM.ClassID,CM.ClassName,CM.Status,CM.EducationLevelID,
		(Select Count(*) from Class_Sections CS where CS.ClassID=CM.ClassID  and CS.[Status]=1) as SectionCount,
		(Select count(*) from Student_Session SS where SS.ClassID=CM.ClassID and SS.SessionID=@SessionID and Status=1) as StudentCount,
		(Select SchemeID from ClassSessionDetails CSD where CSD.ClassID=CM.ClassID and CSD.SessionID=@SessionID) as EvaluationSchemeID
		 from ClassMaster CM where CM.SBranchID=@SBranchID
	End

	Select ID as ID, Name as Name from EducationLevelMaster where SBranchID=@SbranchID and Status=1

	Select EvaluationSchemeID,EvaluationSchemeName from EvaluationSchemeMaster where SBranchID=@SbranchID and SessionID=@SessionID

	Select SessionID,SessionName,SessionStatus from SessionMaster where SBranchID=@SBranchID

	select @SessionID
END
GO
ALTER  proc [dbo].[spn_GetExamPageModel]
(
@EvaluationID int,
@ClassID int,
@GroupID int,
@SBranchID int,
@EvaluationMode int,
@SessionID int
)
AS
BEGIN

	declare @SchemeID int
	if(@SessionID=0)
	Begin
		Select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and SessionStatus=1
	End
	declare @ELID int
	if(@EvaluationID=0)
	begin

		Select @ClassID=min(ClassID) from ClassMaster where Status=1 and SBranchID=@SBranchID
		select @SchemeID=SchemeID from [dbo].[ClassSessionDetails] where ClassID=@ClassID and SessionID=@SessionID
		Select @EvaluationID=min(EvaluationID) from EvaluationMaster EM where SBranchID=@SBranchID and
		EvaluationSchemeID=@SchemeID and EvaluationID not in (Select isnull(MasterID,0) from EvaluationMaster)

		Select @ELID =EducationLevelID from ClassMaster where ClassID=@ClassID
		Select @GroupID=GroupID from GroupMaster where EducationLevelID=@ELID
		Select @SessionID=SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
	end
	else
	begin
		Select @ELID =EducationLevelID from ClassMaster where ClassID=@ClassID
		select @SchemeID=SchemeID from [dbo].[ClassSessionDetails] where ClassID=@ClassID and SessionID=@SessionID
	end

	declare @EvaMonth nvarchar(2)
	declare @EvaYear nvarchar(4)

	select @EvaMonth=[EvaluationMonth] from [dbo].[EvaluationMaster] where EvaluationID=@EvaluationID

	select @EvaYear= YEAR(getdate())

	Select EvaluationID,
	(case when isnull(MasterID,0)=0 then EvaluationName else
	(Select EvaluationName from EvaluationMaster ED where ED.EvaluationID=EM.MasterID)+'->'+EvaluationName end) as EvaluationName
	from EvaluationMaster EM where SBranchID=@SBranchID and EvaluationSchemeID=@SchemeID and EvaluationID not in (Select isnull(MasterID,0) from EvaluationMaster)

	Select ClassID,ClassName from ClassMaster where Status=1 and SBranchID=@SBranchID
	Select GroupID,GroupName from GroupMaster where EducationLevelID=isnull(@ELID,0)

	Select EM.ExamID,SM.SubjectID,SM.SubjectName,isnull(EM.ExamDate,cast(@EvaMonth+'/01'+'/'+@EvaYear as date)) as ExamDate,EM.IsLocked
	,cast(isnull(EM.StartTime,'09:00:00') as nvarchar(8)) as StartTime , cast(isnull(EM.EndTime,'10:30:00') as nvarchar(8)) as EndTime,
	isnull(EM.IsApplicable,1) as IsApplicable,isnull(MaxMarks,100) as MaxMarks,isnull(PassMarks,35) as PassMarks from SubjectMasterAll SM
	left outer join ExamMaster EM on EM.SubjectID=SM.SubjectID and  EM.ClassID=@ClassID and
	EM.GroupID=@GroupID and EM.SBranchID=@SBranchID and EM.EvaluationID=@EvaluationID
	where SM.GroupID =@GroupID --and EM.EvaluationID<>0
	order by isnull(EM.ExamDate,cast(@EvaMonth+'/01'+'/'+@EvaYear as date)) asc


	Select isnull(@EvaluationID,0)
	Select isnull(@ClassID,0)
	select isnull(@GroupID,0)
	select isnull(@SessionID,0)

	Select SessionID,SessionName,SessionStatus from SessionMaster where SBranchID=@SBranchID

END
GO
ALTER  procedure [dbo].[spn_GetStudentsByClassSection]
(
@ClassID int,
@SectionID int,
@SBranchID int,
@SessionID int=0
)
as 
begin 
	Declare @Status int
	if(@SessionID=0)
	begin
		Select @SessionID=SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
	end
	select @Status=SessionStatus from SessionMaster where SessionID=@SessionID
	if(@SectionID<>0)
	begin
		Select SM.StudentID,'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6) as StudentSID, SM.Name,DOB, EmailID, BloodGroup, GuardianName, GuardianMobileNo, Photo,SS.RollNo,SM.AccessCardNo,SM.AadharCardNo
		,PM.FatherMobileNo,PM.MotherMobileNo,AadharCardNo ,PM.FatherName,PM.MotherName,SM.SSSID,SM.FamilyID,
		SS.FromDate as SessionStartDate, SS.ToDate as SessionEndDate,
		SM.HostelRoomID,SM.StopID,SM.Gender,SM.SchoolUID,MiniAddress,HM.Name
		from StudentMaster SM left outer join Student_Session SS on SS.StudentID=SM.StudentID
		left outer join HouseMaster HM on HM.ID=SS.HouseID
		left outer join Parentmaster PM on PM.ParentID=SM.ParentID
		where (SS.ClassID=@ClassID and SS.SectionID=@SectionID and (SS.Status=@Status or SS.Status=1) and SS.SessionID=@SessionID) and 
		SM.SBranchID=@SBranchID order by SM.Name
	end
	else
	begin		
	set @ClassID=0
			Select SM.StudentID,'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6) as StudentSID, SM.Name,DOB, EmailID, BloodGroup, GuardianName, GuardianMobileNo, Photo,'' as RollNo,SM.AccessCardNo,SM.AadharCardNo,
		SM.HostelRoomID,SM.StopID,SM.Gender,AadharCardNo,PM.FatherMobileNo,PM.MotherMobileNo,PM.FatherName,SM.SchoolUID,PM.MotherName,SM.SSSID,SM.FamilyID,
		MiniAddress
		from StudentMaster SM   left outer join Parentmaster PM on PM.ParentID=SM.ParentID
			where  SM.SBranchID=@SBranchID and StudentID not in (select StudentID from Student_Session SS where SS.Status=1) order by Name
	end
	
	select ClassID,ClassName from ClassMaster where SBranchID=@SBranchID

	select ID,Name from Class_Sections where ClassID=@ClassID

	select @ClassID
	select @SectionID

	Select SessionName as Name,SessionID as ID,SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID
	
	select @SessionID

	select * from SBranchMaster where SBranchID=@SBranchID
end


GO
ALTER  procedure [dbo].[spn_GetStudentsDetailsNew]
(
@StudentID int,
@SBranchID int
)
as 
begin 
SELECT SM.StudentID, 'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6) as StudentSID
      ,[Name]      ,[DOB]      ,[Gender]      ,[Nationality]      ,[ReligionID]      ,[DOJ]      ,[EmailID]      ,[Category]
      ,[Disability]      ,[AdmissionRoute]      ,[BloodGroup]      ,[MotherTongueID]      ,[PassportNo]      ,[House]
      ,[GuardianName]      ,[RelationWithGuardian]      ,[GuardianMobileNo]      ,[GuardianEmail]      ,[Cadd_HouseNo]
      ,[Cadd_Street]      ,[Cadd_AreaCode]      ,[Cadd_Sector]      ,[Cadd_PinCode]      ,[Cadd_DistrictCode]      ,[Cadd_StateCode]
      ,[Cadd_CountryCode]	       ,[RoleModel]      ,[Ambition]      ,[ExtraCurricular]      ,[Allergic_Medicine]
      ,[VehicleNo]      ,[DriverName]      ,[DriverMobileNo]      ,[AddressProof]      ,[BirthCertificate],AccessCardNo
      ,[CategoryCertificate]      ,[Photo]      ,[TransferCertificate]	,VehicleRouteID,StopID,HostelRoomID,NotificationSMSTo,SchoolUID,AadharCardNo,MiniAddress,MiniAddress2
	  ,SSSID,FamilyID,BankName,AccountNumber,IFSCCode,BranchName
  FROM StudentMaster SM 
  where SM.StudentID=@StudentID and SM.SBranchID=@SBranchID 
  
  select ParentID,'PAR'+RIGHT(REPLICATE('0',6)+CAST(ParentID AS VARCHAR(6)),6) as [ParentSID],[FatherName]      ,[FatherOccupationID]
      ,[FatherEducationID]      ,[FatherMobileNo]      ,[FatherEmailID]      ,[FatherBloodGroup] ,[FatherImage]     ,[MotherName]
      ,[MotherOccupationID]      ,[MotherEducationID]      ,[MotherMobileNo]      ,[MotherEmailID]      ,[MotherBloodGroup],[MotherImage]
      ,[Padd_HouseNo]      ,[Padd_Street]      ,[Padd_Area]      ,[Padd_Sector]      ,[Padd_PinCode],[AccessCardNo],FatherAadhaar,MotherAadhaar
      ,[Padd_District]      ,[Padd_State]      ,[Padd_Country]      ,[HomeLandLineNo],FatherDOB,MotherDOB from ParentMaster 
  where ParentID=(select ParentID from StudentMaster where StudentID=@StudentID)

  
	declare @AAreaID int
	declare @ACityID int

	select @ACityID=isnull(Cadd_DistrictCode,0),@AAreaID=Cadd_AreaCode from StudentMaster where StudentID=@StudentID

	if(@ACityID=0)
	begin
		select @ACityID=min(CityID) from CityMaster where StateID=@SBranchID
		select @AAreaID=min(AreaID) from AreaMaster where CityID=@ACityID
	end
	select CityID as ID, CityName as Name from CityMaster where SBranchID =@SBranchID
	select AreaID as ID, AreaName as Name from AreaMaster where CityID = @ACityID

	select isnull(@ACityID,0)
	select isnull(@AAreaID,0)

  select SS.StudentSessionUID,  SS.StudentID,   SS.ClassID	  ,SS.SectionID	  ,SS.QuotaID	  ,SS.FromDate	  ,SS.ToDate
	  ,SS.Status,	  SS.RollNo,SS.FeePaymentMode
	  ,(Select QuotaName from QuotaMaster QM where QM.QuotaID=SS.QuotaID) as QuotaName
	  ,(Select ClassName from ClassMaster CM where CM.ClassID=SS.ClassID) as ClassName
	  ,(Select Name from Class_Sections CS where CS.ID=SS.SectionID) as SectionName,
	  (Select Name from HouseMaster HM where HM.ID=SS.HouseID) as HouseName
	  ,(Select SessionName from SessionMaster CS where CS.SessionID=SS.SessionID) as SessionName
	  from Student_Session SS where SS.StudentID=@StudentID and SS.SBranchID=@SBranchID
	  order by Status desc
	  
	--exec [dbo].[spn_GetStudentsTransportDetails] @StudentID,@SBranchID	
		
	Declare @SessionID int,@SectionID int,@QuotaID int,@ClassID int,@IsAdmissionFeeApplicable int,@GroupID int,@StudentSessionUID int



	select top 1 @SessionID=SessionID,@QuotaID=QuotaID,@ClassID=ClassID,@SectionID=SectionID,@IsAdmissionFeeApplicable=IsAdmissionFeeApplicable,
	@StudentSessionUID=StudentSessionUID
	from Student_Session where StudentID=@StudentID order by Status desc,FromDate desc
	exec sp_GetStudentSessionCustomFee @StudentSessionUID,@SBranchID


	select THChangeID,ChangeType,UserType,UserID,StartDate,EndDate,SessionID,KeyID,
	(Select AreaName from AreaMaster where AreaID=(select AreaID from [dbo].[TransportRouteDetails] TRD where TRD.StopID=T.KeyID)) as Name
	from [dbo].[TransportHostalAllocationDelocation] T
	where T.UserID=@StudentID
	order by StartDate 
	
	select * from [dbo].[SubReligionMaster]

  end


GO
ALTER proc [dbo].[spn_GetVehicleRouteStoppages]
(
	@VehicleRouteID int
)
AS
BEGIN
	Select StopID,VehicleRouteID,TRD.AreaID,Rate,
	(Select [AreaName] from AreaMaster AM Where AM.AreaID=TRD.AreaID) as AreaName,
	cast(DATEADD(MINUTE,  TVR.DelayFromRouteTime, [time]  ) as nvarchar(8))as [Time],HaltDuration,HaltDurationR,SequenceNo,SequenceNoR
	,cast(DATEADD(MINUTE,  TVR.DelayFromRouteTime, [timeR]  ) as nvarchar(8)) as [TimeR],
	HaltDurationR,SequenceNoR from TransportRouteDetails TRD left outer join Transport_Vehicle_Route TVR on TVR.RouteID=TRD.RouteID
	where tvr.VehicleRouteID=@VehicleRouteID	
	order by SequenceNo
END
GO
ALTER proc [dbo].[spn_InsertUpdateSBranch]
(
@SBranchID int,
@BranchName nvarchar(50),
@Logo nvarchar(50),
@ContactNo nvarchar(50),
@EmailID nvarchar(50),
@Address nvarchar(500),
@PrincipalName nvarchar(50),
@PrincipalMobile nvarchar(50),
@PrincipalEmail nvarchar(50),
@BranchSchoolName nvarchar(100),
@OpType int,
@StateID int=0
--@AccountUserName nvarchar(50),
--@AccountPassword nvarchar(50)
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from SBranchMaster where SBranchID=@SBranchID
		Delete from LoginDetails where SBranchID=@SBranchID and UserType=2
		select @@ROWCOUNT
	end
	else
	begin
		if(@SBranchID=0)
		begin
			Insert into SBranchMaster(BranchName,[Logo],[ContactNo],[EmailID],[Address],[PrincipalName],[PrincipalMobile]
		,[PrincipalEmail],[BranchSchoolName],StateID)
			values (@BranchName,@Logo,@ContactNo,@EmailID,@Address,@PrincipalName,@PrincipalMobile,@PrincipalEmail,@BranchSchoolName,@StateID)
			SELECT @SBranchID= CAST(SCOPE_IDENTITY() as int)
			--Insert into LoginDetails(UserName,Password,UserType,SBranchID,UserID)
			--	values(@AccountUserName,@AccountPassword,2,@SBranchID,@SBranchID)
			--Insert into LoginDetails(UserType,SBranchID,UserID)
				--values(2,@SBranchID,@SBranchID)
			
			select @SBranchID
		end
		else
		begin
			Update SBranchMaster set BranchName=@BranchName,[Logo]=@Logo,[ContactNo]=@ContactNo,[EmailID]=@EmailID,[Address]=@Address
			,[PrincipalName]=@PrincipalName,[PrincipalMobile]=@PrincipalMobile,StateID=@StateID
		,[PrincipalEmail]=@PrincipalEmail,[BranchSchoolName]=@BranchSchoolName where SBranchID=@SBranchID

		--if(@AccountPassword<>'')
		--begin
		--	Update LoginDetails set UserName=@AccountUserName, [Password]=@AccountPassword where UserType=2 and SBranchID=@SBranchID
		--end
			select @SBranchID
		end
	 end
END
GO
ALTER Procedure [dbo].[spn_SaveFeePaymentV2] 
(
@StudentID int,
@QDate date,
@CurDate date,
@PaymentDate datetime,
@PaymentAmount numeric(10,2),
@WaiverMonths nvarchar(50)='',
@Remark nvarchar(max),
@ReferanceNumber nvarchar(100),
@PaymentMode int,
@CollectedBy nvarchar(100),
@SessionID int,
@ExcludedFees nvarchar(100)='',
@UserID int,
@SBranchID int
)
as
begin
	Declare @PaymentID int
	
	Declare @SessionStartDate date	
	select @SessionStartDate=SessionStartDate from SessionMaster where SessionID=@SessionID

	
	--FeeExculsion Handeling Start
		set @ExcludedFees=@ExcludedFees+','
		declare @Exclusion nvarchar(10)

		declare @ExclusionTbl  table (Month int,FeeID int)
		declare @LateFeeWaiver  table (Month int)
		
		insert into @LateFeeWaiver
		select Item from [dbo].[SplitStringToTable](@WaiverMonths,',')

		DECLARE @IND    INT
		DECLARE @SepInd int
		Declare @ExMonth nvarchar(2)
		Declare @ExFeeID nvarchar(2)
		SET @IND = CHARINDEX(',',@ExcludedFees)
		DECLARE @EIND INT set @EIND = 0
		WHILE(@IND != LEN(@ExcludedFees))
		BEGIN
			SET  @EIND = ISNULL(((CHARINDEX(',', @ExcludedFees, @IND + 1)) - @IND - 1), 0)
			SELECT @Exclusion=(SUBSTRING(@ExcludedFees, (@IND  + 1),  @EIND)) 
			SELECT @IND = ISNULL(CHARINDEX(',', @ExcludedFees, @IND + 1), 0)
			set @SepInd=ISNULL(((CHARINDEX('^', @Exclusion, 0))  - 1), 0)
			set @ExMonth=(SUBSTRING(@Exclusion, 0,  @SepInd+1)) 
			set @ExFeeID=(SUBSTRING(@Exclusion, (@SepInd  + 2),  len(@Exclusion)-1)) 

			insert into @ExclusionTbl(Month,FeeID) values(@ExMonth,@ExFeeID)

		END
	--Fee Exclusion Handeling End


	declare @Year int,@Month int,@Day int
	set @Year=datepart(year,@CurDate)
	set @Month=datepart(month,@CurDate)	
	set @Day=datepart(day,@CurDate)

	Declare @PaymentRecieptNo nvarchar(20)
	Declare @PaymentRecieptSeq int
	select @PaymentRecieptSeq=max(PaymentRecieptSeq) from PaymentMaster where SessionID=@SessionID and SBranchID=@SBranchID
	set @PaymentRecieptSeq=isnull(@PaymentRecieptSeq,0)+1
	set  @PaymentRecieptNo=right(datepart(year,@SessionStartDate),2)+cast((right(datepart(year,@SessionStartDate),2)+1) as nvarchar(3))+'/'+cast(@SBranchID as nvarchar(10))+'/'+RIGHT('000000' + cast(isnull(@PaymentRecieptSeq,0) as nvarchar(10)), 6);
	--Insert to Payment Master Start
		insert into PaymentMaster (PayeeID,ReferanceNumber,PaymentMode,Remark,PaymentAmount,[Year],[Month],
		PaymentDate,SBranchID,CollectedBy,SessionID,PaymentRecieptSeq,PaymentRecieptNo,CreatedDate)
		values (@StudentID,@ReferanceNumber,@PaymentMode,@Remark,@PaymentAmount,@Year,@Month,@PaymentDate,@SBranchID,
		@CollectedBy,@SessionID,@PaymentRecieptSeq,@PaymentRecieptNo,@CurDate)
		select @PaymentID=CAST(SCOPE_IDENTITY() as int)
	--Insert to Payment Master End

	Declare @FeeDetailTable table (FeeMonth int,FeeYear int,FeeTypeApplicable int, FeeTypeID int,FeeTypeName nvarchar(100),FeeAmount numeric(10,2),QDiscount numeric(10,2),
	RDiscount numeric(10,2),PayApplicableAmount numeric(10,2),CustomFee numeric(10,2),PaidAmount numeric(10,2),IsCustomFee int,IsPayment int)
	
	insert into @FeeDetailTable	exec sp_GetStudentFeeDetailsNew @StudentID,@SBranchID,@SessionID,@QDate,@CurDate


	Declare @FeeMonth int,@FeeYear int,@FeeTypeID int,@FeeTypeName nvarchar(100),@FeeAmount numeric(10,2),@QDiscount numeric(10,2),
	@RDiscount numeric(10,2),@PayApplicableAmount numeric(10,2),@CustomFee numeric(10,2),@PaidAmount numeric(10,2),@IsCustomFee int,@IsPayment int,@ApplicableFee numeric(10,2),
	@RDiscAmt numeric(10,2),@IsExcluded int,@FeePaid numeric(10,2),@FeeTypeApplicable int

	
	DECLARE db_Detailcursor CURSOR FOR 
	select *,(Case when IsPayment=1 then PayApplicableAmount when IsCustomFee=1 then CustomFee else (isnull(FeeAmount,0)-isnull(FeeAmount,0)*isnull(QDiscount,0)/100) end) as ApplicableFee  
	from @FeeDetailTable order by FeeYear,FeeMonth,FeeTypeID

	OPEN db_Detailcursor   
		FETCH NEXT FROM db_Detailcursor INTO @FeeMonth ,@FeeYear,@FeeTypeApplicable ,@FeeTypeID ,@FeeTypeName,@FeeAmount,@QDiscount,
		@RDiscount,@PayApplicableAmount,@CustomFee,@PaidAmount,@IsCustomFee,@IsPayment,@ApplicableFee
		WHILE @@FETCH_STATUS = 0   
		BEGIN   
			Declare @DiscountID int
			select @DiscountID=DiscRequestID from [dbo].[FeeDiscountRequestMaster] where StudentID=@StudentID and FeeYear=@FeeYear and FeeMonth=@FeeMonth and Status=0
			if(isnull(@DiscountID,0)>0)
			begin
				update [dbo].[FeeDiscountRequestMaster]  set Status=2, DirectorRemark='Discount Request Cancelled, as payment taken before Approval of Request from Principle' where DiscRequestID=@DiscountID
			end
			declare @IsInsert int=1
			select @IsExcluded=count(*) from @ExclusionTbl where Month=@FeeMonth and FeeID=@FeeTypeID
			if(@PaymentAmount>0 and isnull(@ApplicableFee,0)-isnull(@PaidAmount,0)>0 or @IsExcluded>0)
			begin
				SELECT @RDiscAmt=sum(DiscAmt) from PaymentDetails where PayeeID=@StudentID and Year=@FeeYear and Month=@FeeMonth and FeeTypeID=@FeeTypeID
				set @RDiscount=@RDiscount-isnull(@RDiscAmt,0)

				if(@FeeTypeID=-2 and @IsExcluded=0)
				begin
					if((select count(*) from @LateFeeWaiver where Month=@FeeMonth)>0)
					begin
						set @FeePaid=0
						set @RDiscount=@ApplicableFee
					end
					else
					begin
						if(isnull(@ApplicableFee,0)-isnull(@PaidAmount,0)-isnull(@RDiscount,0)<=@PaymentAmount)
						begin
							set @FeePaid=isnull(@ApplicableFee,0)-isnull(@PaidAmount,0)-isnull(@RDiscount,0)
							set @PaymentAmount=@PaymentAmount-@FeePaid
						end
						else
						begin
							set @FeePaid=@PaymentAmount
							set @PaymentAmount=0
						end
					end
				end
				else if(@IsExcluded>0)
				begin
					set @FeePaid=0
					set @IsInsert=0
				end
				else if(isnull(@ApplicableFee,0)-isnull(@PaidAmount,0)-isnull(@RDiscount,0)<=@PaymentAmount)
				begin
					set @FeePaid=isnull(@ApplicableFee,0)-isnull(@PaidAmount,0)-isnull(@RDiscount,0)
					set @PaymentAmount=@PaymentAmount-@FeePaid
				end
				else
				begin
					set @FeePaid=@PaymentAmount
					set @PaymentAmount=0
				end
				if(@IsInsert=1)
				begin
					insert into PaymentDetails(PaymentID,FeeTypeID,PayeeID,Amount,DiscAmt,NetApplicablePayment,PaymentRecieved,Year,Month,PaymentDate,SBranchID,DuesPaidCount,PaymentStatus,PaymentTitle)
					values(@PaymentID,@FeeTypeID,@StudentID,@ApplicableFee,@RDiscount,@ApplicableFee,@FeePaid,@FeeYear,@FeeMonth,@PaymentDate,@SBranchID,@IsPayment,0,@FeeTypeName)
				end
			end

			FETCH NEXT FROM db_Detailcursor INTO @FeeMonth ,@FeeYear,@FeeTypeApplicable ,@FeeTypeID ,@FeeTypeName,@FeeAmount,@QDiscount,
			@RDiscount,@PayApplicableAmount,@CustomFee,@PaidAmount,@IsCustomFee,@IsPayment,@ApplicableFee		 
		END 


		CLOSE db_Detailcursor   
		DEALLOCATE db_Detailcursor
			
	
	Declare @Students table(StudentID int,Name nvarchar(100),RollNo nvarchar(100),Gender int,Photo nvarchar(100),ClassID int,FeePaymentMode int,
	StudentSID nvarchar(15),FromDate date,ToDate Date,QuotaID int,
	SessionID int,VehicleRouteID int,HostelRoomID int,SectionID int,
	SessionStartDate date,SessionEndDate date,IsAdmissionFee int,SchoolUID nvarchar(50),FeeAmount numeric(10,2),PreviousDue numeric(10,2)
	,LateFee numeric(10,2),Discounts numeric(10,2),Paid numeric(10,2))


	insert into @Students
	exec [dbo].[sp_GetClassGroupFeeListOnly] 0,0,@SBranchID,@SessionID,@QDate,@CurDate,@StudentID

	select S.*,Q.QuotaName from @Students S left outer join QuotaMaster Q on Q.QuotaID=S.QuotaID
	
	select @PaymentID
end
GO
ALTER proc [dbo].[sp_GetEmployeeAttandanceDetails]
(
@Month nvarchar(2),
@Year nvarchar(5),
@Day nvarchar(3),
@EmployeeTypeID int,
@SBranchID int
)
AS
BEGIN

	SELECT  [EmployeeTypeID] as ID ,[EmployeeTypeName] as Name FROM [dbo].[EmployeeTypeMaster]

	declare @SQLQuery nvarchar(max)
	set @SQLQuery='Select EM.[EmployeeID],EM.[EmployeeName],''/Images/EmployeeImage/''+CAST(EM.EMPLOYEEID AS NVARCHAR(100))+''_''+ EM.Photo AS PHOTO,EM.Gender,
	(case when EM.EmployeeType in (-1,-2) then ''TRA'' else ''EMP'' end)+RIGHT(REPLICATE(''0'',6)+CAST(EM.[EmployeeID] AS VARCHAR(6)),6) as [EmployeeSID],
	isnull( LM.LeaveID,0) as LeaveID,
	isnull(EAM.D'+@Day+',3) as Status ,(case when EAM.D'+@Day+'=3 then 0 else 1 end) as IsExist
	from EmployeeMaster EM left outer join [dbo].[EmployeeAttendanceMasterT] EAM on EAM.EmployeeID=EM.EmployeeID
	and EAM.[FYear]='+@Year+' and EAM.[Month]='+@Month+'
	left outer join [dbo].[LeaveMaster] LM on LM.[ApplicantID]=EM.EmployeeID and cast('''+@Month+'/'+@Day+'/'+@Year+''' as date) between LM.[StartDate] and LM.[EndDate] and LM.ApplicantType=0 and LM.IsApproved=1
	where EM.SBranchID='+cast(@SBranchID as nvarchar(3))+' and EM.[EmployeeType]='+cast(@EmployeeTypeID as nvarchar(3))+' and [Status]=1 and EM.DOJ<=cast('''+@Month+'/'+@Day+'/'+@Year+''' as date) order by EM.[EmployeeName]'
	exec(@SQLQuery)
END
GO

ALTER proc [dbo].[sp_GetAllEmployeeTypes]
(
@SBranchID int=null
)
AS
BEGIN
	Select [EmployeeTypeID]
      , [EmployeeTypeName] ,SBranchID,IsDefault,
	  (Select Sum(isnull(amount,0)) from EmployeeTypeSalaryDetails  ETS where ETS.EmployeeTypeID=ETM.EmployeeTypeID) as NetSalary,
	  (Select count(*) from [dbo].[v_EmployeeDriversBasicDetails] EM where EM.EmployeeType=ETM.EmployeeTypeID) as EmployeeCount
	  from [dbo].[EmployeeTypeMaster] ETM 
	  order by SBranchID,[EmployeeTypeID]
END
GO