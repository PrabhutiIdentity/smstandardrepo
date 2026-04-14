CREATE TYPE [dbo].[ut_DeviceLogs] AS TABLE(
	[DeviceLogId] [int] NULL,
	[DownloadDate] [datetime] NULL,
	[DeviceId] [int] NOT NULL,
	[UserId] [nvarchar](50) NOT NULL,
	[LogDate] [datetime] NOT NULL,
	[Direction] [nvarchar](255) NULL,
	[AttDirection] [nvarchar](255) NULL,
	[C1] [nvarchar](255) NULL,
	[C2] [nvarchar](255) NULL,
	[C3] [nvarchar](255) NULL,
	[C4] [nvarchar](255) NULL,
	[C5] [nvarchar](255) NULL,
	[C6] [nvarchar](255) NULL,
	[C7] [nvarchar](255) NULL,
	[WorkCode] [nvarchar](255) NULL
)
GO
CREATE TYPE [dbo].[ut_EmployeeLeaveDetails] AS TABLE(
	[LDID] [int] NULL,
	[LeaveTypeID] [int] NULL,
	[Applied] [numeric](10, 2) NULL
)
GO
CREATE TYPE [dbo].[ut_EmpSalaryProcessingDetails] AS TABLE(
	[SPID] [int] NULL,
	[SalaryTypeID] [int] NULL,
	[Amount] [numeric](10, 2) NULL,
	[OpType] [int] NULL
)
GO
CREATE TYPE [dbo].[ut_LibraryIssueBooks] AS TABLE(
	[BookIssueID] [int] NULL,
	[BookID] [int] NULL,
	[CopyID] [int] NULL,
	[IssueDays] [int] NULL,
	[IssueDate] [date] NULL,
	[Remark] [nvarchar](500) NULL,
	[LibraryIssueID] [int] NULL,
	[Status] [int] NULL,
	[OpType] [int] NULL,
	[Extra1] [nvarchar](500) NULL,
	[Extra2] [nvarchar](500) NULL
)
GO
CREATE TYPE [dbo].[ut_SMSConfigurationParams] AS TABLE(
	[SMSParamID] [int] NULL,
	[SMSConfigID] [int] NULL,
	[ParamType] [int] NULL,
	[ParamName] [nvarchar](50) NULL,
	[ParamValue] [nvarchar](50) NULL,
	[OpType] [int] NULL
)
GO
DROP PROCEDURE [dbo].[sp_UpdateStockTransaction]
GO
DROP TYPE [dbo].[ut_StockTransactionDetails]
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
CREATE  Procedure [dbo].[sp_UpdateStockTransaction]
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
CREATE TYPE [dbo].[ut_StudentFeeDetails] AS TABLE(
	[SFID] [int] NULL,
	[StudentID] [int] NULL,
	[FeeTypeID] [int] NULL,
	[SessionID] [int] NULL,
	[FeeAmount] [numeric](10, 2) NULL,
	[IsApplicable] [int] NULL,
	[OpType] [int] NULL
)
GO
ALTER  Function [dbo].[fn_GetEmployeeSalaryDaysPayble] 
(
@EmployeeID int,
@Month int,
@Year int,
@EmployeeType int
)
	Returns numeric(10,2)
as
begin
		declare @LeaveStartDate date
		declare @LeaveEndDate date
		declare @LeaveType int
		declare @CLApplied numeric(5,2)
		declare @SLApplied numeric(5,2)
		declare @ELApplied numeric(5,2)
		declare @LeaveAdjustment numeric(5,2)
		declare @LeaveApplied numeric(5,2)
		declare @DaysPresent numeric(5,2)
		declare @DaysAbsent numeric(5,2)
		declare @TotalMonthDays numeric(5,2)
		
		declare @StartDate date
		declare @LastDate date
		Set @StartDate=cast(cast(@Month as nvarchar(2))+ '-01-'+cast(@Year as nvarchar(5)) as Date)
		Set @LastDate=DATEADD(s,-1,DATEADD(mm, DATEDIFF(m,0,@StartDate)+1,0))
		set @LeaveAdjustment=0
		select @TotalMonthDays=TotalDays,@DaysPresent=Present+HalfDay,@DaysAbsent=[Absent] from EmployeeAttendanceMaster where FYear=@Year and Month=@Month and EmployeeID=@EmployeeID and DesignationID=@EmployeeType
		if(@TotalMonthDays<>0 and @TotalMonthDays is not null)
		begin
		DECLARE db_cursor CURSOR FOR  
		select StartDate,EndDate,LeaveType from LeaveMaster 
		where applicanttype=0 and ((StartDate between @StartDate and @LastDate) or (EndDate between @StartDate and @LastDate) or cast(cast(@Month as nvarchar(2))+ '-01-'+cast(@Year as nvarchar(5)) as date) between StartDate and EndDate) and IsApproved=1 and ApplicantID=@EmployeeID and EmployeeType=@EmployeeType		
		OPEN db_cursor   
		FETCH NEXT FROM db_cursor INTO @LeaveStartDate,@LeaveEndDate,@LeaveType

		WHILE @@FETCH_STATUS = 0   
		BEGIN   
			declare @TotalDays numeric(5,2)
			declare @DaysAfter numeric(5,2)
			set @TotalDays=DATEDIFF(DAY, @LeaveStartDate, @LeaveEndDate)+1
			set @DaysAfter=0
			declare @DaysBefore numeric(5,2)
			set @DaysBefore=0
			if(DatePart(month,@LeaveStartDate)<@Month  )
			begin
				set @DaysBefore=DATEDIFF(DAY, @LeaveStartDate, cast(@Month as nvarchar(2))+ '-01-'+cast(@Year as nvarchar(5)))
			end
			if(DatePart(month,@LeaveEndDate)>@Month  )
			begin				
				set @DaysAfter=DATEDIFF(DAY, @LastDate,@LeaveEndDate)
			end
			set @LeaveApplied=isnull(@CLApplied,0)+isnull(@SLApplied,0)+isnull(@ELApplied,0)
			if(@LeaveType<>1)
			begin
				set @DaysBefore=@DaysBefore/2
				set @DaysAfter=@DaysAfter/2
			end
			set @TotalDays=@TotalDays-@DaysAfter-@DaysBefore
			set @LeaveApplied=@LeaveApplied-@DaysBefore
			if(@TotalDays>@LeaveApplied)
			begin
				set @LeaveAdjustment=@LeaveAdjustment+@LeaveApplied
			end
			else
			begin
				set @LeaveAdjustment=@LeaveAdjustment+@TotalDays
			end
			FETCH NEXT FROM db_cursor INTO @LeaveStartDate,@LeaveEndDate,@LeaveType
		END   

		CLOSE db_cursor   
		DEALLOCATE db_cursor

		set @DaysPresent=@DaysPresent+@LeaveAdjustment
		
		end
		else
		begin
			set @DaysPresent=0
		end
		Return @DaysPresent
end
GO
ALTER FUNCTION [dbo].[fn_GetStudentFeeRelatedListOnClassSection]
(    
	@ClassID int,
	@SectionID int,
	@SessionID int,
	@LMonth int=8,
	@LYear int=8
)
RETURNS @Students table(StudentID int,Name nvarchar(100),RollNo nvarchar(100),Gender int,Photo nvarchar(100),ClassID int,FeePaymentMode int,
StudentSID nvarchar(15), ClassName nvarchar(10),SectionName nvarchar(10),FromDate date,ToDate Date, GroupID int,QuotaID int,
SessionID int,SBranchID int,VehicleRouteID int,HostelRoomID int,TransportFeeMode int,
	HostelFeeMode int,TransportFee numeric(10,2),HostalFee numeric(10,2),LateFee numeric(10,2),CustomFee numeric(10,2),SectionID int,SessionStartDate date, LastPayDay int,SessionEndDate date,IsAdmissionFee int,SchoolUID nvarchar(100))
AS
BEGIN

	Declare @SessionStartDate date
	Declare @SessionEndDate date

	Select @SessionStartDate=SessionStartDate,@SessionEndDate=SessionEndDate from SessionMaster where SessionID=@SessionID

	declare @TransportMode int
	Declare @HostelMode int
	Declare @LastPayDay int
	Declare @SBranchID int
	Declare @GroupID int
	Select @GroupID=GroupID from Class_Sections CS where CS.ID=@SectionID
	Select @SBranchID=SBranchID from SessionMaster where SessionID=@SessionID
	Select @TransportMode=details from MasterSettings where Type='TransportFeeMode' and SBranchID=@SBranchID
	Select @HostelMode=details from MasterSettings where Type='HostelFeeMode' and SBranchID=@SBranchID
	Select @LastPayDay=details from MasterSettings where Type='FeePaymentReminderDate' and SBranchID=@SBranchID
	declare @ClassTransportFee numeric(18,2)
	declare @ClassHostalFee numeric(18,2)
	--if(@TransportMode=1)
	--	begin
	--		select @ClassTransportFee=sum(isnull(FeeAmount,0)) from ClassFeeStructureMaster 
	--		where ClassID=@ClassID and GroupID=@GroupID and SBranchID=@SBranchID 
	--		and FeeTypeID in (select FeeTypeID from FeeTypeMaster where FeeTypeApplicable=5)
	--	end
	--	if(@HostelMode=1)
	--	begin
	--		select @ClassHostalFee= sum(isnull(FeeAmount,0)) from ClassFeeStructureMaster 
	--		where ClassID=@ClassID and GroupID=@GroupID and SBranchID=@SBranchID 
	--		and FeeTypeID in (select FeeTypeID from FeeTypeMaster where FeeTypeApplicable=6)
	--	end


    Insert into @Students
	Select t.StudentID,t.Name,t.RollNo,t.Gender,t.Photo,t.ClassID,t.FeePaymentMode,t.StudentSID,t.ClassName,t.SectionName,t.FromDate,t.ToDate,
	t.GroupID,t.QuotaID,t.SessionID,t.SBranchID,t.VehicleRouteID,t.HostelRoomID,t.TransportFeeMode,t.HostelFeeMode,
	(0) as TransportFee,
		(0) as HostelFee,

		(select isnull(sum(isnull(FeeAmount,0)),0) from ClassFeeStructureMaster 
		where ClassID=t.ClassID and GroupID=t.GroupID and SBranchID=t.SBranchID 
		and FeeTypeID =-2) as LateFee,null,
		t.SectionID,
		(Case when SSM.SessionStartDate<t.FromDate then t.FromDate else SSM.SessionStartDate end) as SessionStartDate,
		isnull(@LastPayDay,10) as LastPayDay,
		(Case when SSM.SessionEndDate<t.ToDate then SSM.SessionEndDate else t.ToDate end) as SessionEndDate,t.IsAdmissionFee,t.SchoolUID
	from
	(Select SM.Name,SS.RollNo,SM.Gender,SM.Photo, SS.StudentID,SS.ClassID,SS.FeePaymentMode,
	'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
	('') as ClassName,
	('') as SectionName,SS.FromDate,SS.ToDate,
	(@GroupID) as GroupID,SS.QuotaID,SS.SessionID,SS.SBranchID	,
	isnull(SM.HostelRoomID,0) as HostelRoomID,isnull(SM.VehicleRouteID,isnull(SM.StopID,0)) as VehicleRouteID,isnull(SM.StopID,0) as StopID,
	(@TransportMode) as TransportFeeMode,
	(@HostelMode) as HostelFeeMode,SS.SectionID,
	(@LastPayDay) as LastPayDay,isnull(SS.IsAdmissionFeeApplicable,0) as IsAdmissionFee,SM.SchoolUID
	from Student_Session SS Left outer join StudentMaster SM on SM.StudentID=SS.StudentID where SS.SessionID=@SessionID 
	and SS.ClassID=@ClassID and SS.SectionID=@SectionID and Status=1)t 
	Left Outer join SessionMaster SSM on SSM.SessionID=t.SessionID

      RETURN
END
GO
CREATE Function [dbo].[fn_GetStudentTransportFeeAmount] --4,2019,785,0,0,1,0,19,1,5,2,50,50,4
(
@Month int,
@Year int,
@StudentID int,
@TransportFeeMode int,
@ClassID int,
@GroupID int,
@SBranchID int,
@SessionID int=1 
)
	Returns numeric(10,2)
as
begin
		declare @MonthsYear as Table(month int,year int)
		Insert into @MonthsYear select @Month,@Year
		Declare @Amount decimal(18,2)=0,@ClassTransportFee numeric(18,2)
		Declare @Quota int
		
		if(@TransportFeeMode=1)
		begin
			select @ClassTransportFee=sum(isnull(isnull(SFD.FeeAmount,CFS.FeeAmount),0)) from ClassFeeStructureMaster CFS left outer join
			[dbo].[StudentFeeDetails] SFD on SFD.FeeTypeID=CFS.FeeTypeID and SFD.StudentID=@StudentID and SFD.SessionID=@SessionID and SFD.IsApplicable=1
			where ClassID=@ClassID and GroupID=@GroupID and SBranchID=@SBranchID 
			and CFS.FeeTypeID in (select FeeTypeID from FeeTypeMaster where FeeTypeApplicable=5) and CFS.SessionID=@SessionID
		end
		if((select count(*) from TransportHostalAllocationDelocation where UserType=0 and ChangeType=0 and UserID=@StudentID)>0)
		begin
			declare @LMonth int
			declare @LYear int
			DECLARE cur_Months CURSOR
			STATIC FOR select Month,Year from @MonthsYear
			OPEN cur_Months 
			FETCH NEXT FROM cur_Months INTO @LMonth,@LYear
			WHILE @@FETCH_STATUS = 0
			Begin		
				Declare @MStartDate date=cast(cast(@LYear as nvarchar(5))+'-'+cast(@LMonth as nvarchar(3))+'-1' as date)
				Declare @MEndDate date= EOMONTH(@MStartDate)
					Select @Amount=isnull(@Amount,0)+ isnull(sum(Amount*(Days+1)/TotalDays),0) from (
					select StartDate,EndDate,KeyID,ChangeType,DATEDIFF(DAY, StartDate,isnull(EndDate,@MEndDate)) as Days,datepart(day,@MEndDate) as TotalDays,
					(case when @TransportFeeMode=1 then 
					(@ClassTransportFee) else 
						(select isnull(Rate,0) from TransportRouteDetails TRM where TRM.StopID=KeyID) end) as Amount from 
					(select (case when  StartDate is null or StartDate<@MStartDate then @MStartDate else StartDate end) as StartDate,
					(case when EndDate is null or EndDate>@MEndDate then @MEndDate else EndDate end) as EndDate
					, KeyID,ChangeType
					from TransportHostalAllocationDelocation where UserType=0 and ChangeType=0 and UserID=@StudentID and (StartDate<EndDate or EndDate is null)
					and ((StartDate between @MStartDate and @MEndDate or StartDate<@MStartDate) and (isnull(EndDate,@MEndDate) between @MStartDate and @MEndDate or EndDate>@MEndDate))
					 )t)tt	
			
				FETCH NEXT FROM cur_Months INTO @LMonth,@LYear
			end
			close cur_Months 
			deallocate cur_Months 
		end
		Return isnull(@Amount,0)
end
GO
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
GO
Create FUNCTION [dbo].[GetBookCategoryNames]
(
	@Categories varchar(50)
)
RETURNS nvarchar(max)
AS
begin
		Declare @Names nvarchar(max)
	if(@Categories is null or @Categories='0')
	begin
		set @Names= 'All Categories'
	end
	else
	begin
		declare @CategoriNames NVARCHAR(MAX)
		if(left(@Categories,1)<>',')
		begin
			set @Categories=','+@Categories+','
		end
		DECLARE @Part NVARCHAR(MAX)
		DECLARE @IND    INT
		SET @IND = CHARINDEX(',',@Categories)
	
		set @CategoriNames=''

		DECLARE @EIND INT set @EIND = 0
		WHILE(@IND != LEN(@Categories))
		BEGIN
			declare @CategoriName nvarchar(max)
			SET  @EIND = ISNULL(((CHARINDEX(',', @Categories, @IND + 1)) - @IND - 1), 0)
			SELECT @Part=(SUBSTRING(@Categories, (@IND  + 1),  @EIND))
			SELECT @IND = ISNULL(CHARINDEX(',', @Categories, @IND + 1), 0)
			Select @CategoriName=Name from BookCategory where ID=CONVERT(int,@Part)
			Set @CategoriNames=@CategoriNames+','+@CategoriName
		END
		set @Names= substring(@CategoriNames,2,len(@CategoriNames)-1)
	end
	Return @Names
end
GO
ALTER  FUNCTION [dbo].[GetClassNames]
(
	@Classes varchar(500)
)
RETURNS nvarchar(max)
AS
begin
		Declare @Names nvarchar(max)
	if(@Classes is null or @Classes='0')
	begin
		set @Names= 'All Classes'
	end
	else
	begin
		declare @ClassNames NVARCHAR(MAX)
		set @Classes=','+@Classes+','
		DECLARE @Part NVARCHAR(MAX)
		DECLARE @IND    INT
		SET @IND = CHARINDEX(',',@Classes)
	
		set @ClassNames=''

		DECLARE @EIND INT set @EIND = 0
		WHILE(@IND != LEN(@Classes))
		BEGIN
			declare @ClassName nvarchar(max)
			SET  @EIND = ISNULL(((CHARINDEX(',', @Classes, @IND + 1)) - @IND - 1), 0)
			SELECT @Part=(SUBSTRING(@Classes, (@IND  + 1),  @EIND))
			SELECT @IND = ISNULL(CHARINDEX(',', @Classes, @IND + 1), 0)
			Select @ClassName=ClassName from ClassMaster where ClassID=CONVERT(int,@Part)
			Set @ClassNames=@ClassNames+','+@ClassName
		END
		set @Names= substring(@ClassNames,2,len(@ClassNames)-1)
	end
	Return @Names
end


GO
Create FUNCTION [dbo].[GetEmployeeTypeNames]
(
	@EmployeeTypes varchar(500)
)
RETURNS nvarchar(max)
AS
begin
		Declare @Names nvarchar(max)
	if(@EmployeeTypes is null or @EmployeeTypes='0')
	begin
		set @Names= 'All EmployeeTypees'
	end
	else
	begin
		declare @EmployeeTypeNames NVARCHAR(MAX)
		set @EmployeeTypes=','+@EmployeeTypes+','
		DECLARE @Part NVARCHAR(MAX)
		DECLARE @IND    INT
		SET @IND = CHARINDEX(',',@EmployeeTypes)
	
		set @EmployeeTypeNames=''

		DECLARE @EIND INT set @EIND = 0
		WHILE(@IND != LEN(@EmployeeTypes))
		BEGIN
			declare @EmployeeTypeName nvarchar(max)
			SET  @EIND = ISNULL(((CHARINDEX(',', @EmployeeTypes, @IND + 1)) - @IND - 1), 0)
			SELECT @Part=(SUBSTRING(@EmployeeTypes, (@IND  + 1),  @EIND))
			SELECT @IND = ISNULL(CHARINDEX(',', @EmployeeTypes, @IND + 1), 0)
			Select @EmployeeTypeName=EmployeeTypeName from EmployeeTypeMaster where EmployeeTypeID=CONVERT(int,@Part)
			Set @EmployeeTypeNames=@EmployeeTypeNames+','+@EmployeeTypeName
		END
		set @Names= substring(@EmployeeTypeNames,2,len(@EmployeeTypeNames)-1)
	end
	Return @Names
end

GO
GO
CREATE FUNCTION [dbo].[GetEvaluationSchemeClassList]
(
@SchemeID int
)
RETURNS nvarchar(max)
as
begin
declare @Classes nvarchar(Max)
Set @Classes= ''
declare @ClassID int
declare @ClassName nvarchar(max)
declare FirstCursor cursor  for select HC.ClassID, CM.ClassName from [ClassSessionDetails] HC left Outer Join ClassMaster CM on HC.ClassID=CM.ClassID where HC.SchemeID=@SchemeID
OPEN FirstCursor 
	FETCH NEXT FROM FirstCursor INTO @ClassID, @ClassName	  
	WHILE @@FETCH_STATUS = 0
	Begin
	if(@ClassName is not null)
	begin
		 set @Classes=@Classes+','+@ClassName
		 end
		 FETCH NEXT FROM FirstCursor INTO @ClassID, @ClassName
	end
close FirstCursor 
deallocate FirstCursor 
if(@Classes!='')
begin
	set @Classes= substring(@Classes,2,len(@Classes)-1)
end
return @Classes
end
GO
GO
CREATE FUNCTION [dbo].[GetSchemeClassListIDs]
(
@SchemeID int
)
RETURNS nvarchar(max)
as
begin
declare @Classes nvarchar(Max)
Set @Classes= ''
declare @ClassID int
declare FirstCursor cursor  for select HC.ClassID from [ClassSessionDetails] HC where HC.SchemeID=@SchemeID
OPEN FirstCursor 
	FETCH NEXT FROM FirstCursor INTO @ClassID  
	WHILE @@FETCH_STATUS = 0
	Begin
		 set @Classes=@Classes+','+convert(nvarchar(10), @ClassID)
		 FETCH NEXT FROM FirstCursor INTO @ClassID
	end
close FirstCursor 
deallocate FirstCursor 

if(@Classes!='')
begin
	set @Classes= substring(@Classes,2,len(@Classes)-1)
end
return @Classes
end
GO
ALTER  view [dbo].[v_PaymentDetails] as
	select PayeeID,FeeTypeID,Month,Year,Sum(DiscAmt) as DiscAmt,sum(Case when DuesPaidCount=0 then NetApplicablePayment else 0 end)as NetApplicablePayment,sum(PaymentRecieved) as PaymentRecieved 
	,min(PaymentDate) as PaymentDate,min(PaymentID) as PaymentID from PaymentDetails where  isnull(PaymentStatus,0)=0
	Group by PayeeID,FeeTypeID,Month,Year

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
CREATE View [dbo].[v_StudentCurrentClassName] as
	select SM.StudentID,SM.SBranchID,SS.SessionID,SS.Status, SM.Name+' ('+ (Select ClassName+'/'+SectionName from [v_ClassSectionNames] CM where CM.SectionID=SS.SectionID) +') '+'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6)
	As Name
	from StudentMaster SM left outer join Student_Session SS on SM.StudentID=SS.StudentID
GO
GO
CREATE TABLE [dbo].[AdmissionEnquiryFollowups](
	[FollowupID] [int] IDENTITY(1,1) NOT NULL,
	[EnquiryID] [int] NULL,
	[FollowupBy] [int] NULL,
	[FollowupByName] [nvarchar](50) NULL,
	[Remark] [nvarchar](max) NULL,
	[FDate] [datetime] NULL,
	[Status] [int] NULL,
	[Possibility] [int] NULL,
	[NextFollowupDate] [datetime] NULL,
	[CreatedDate] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
GO
CREATE TABLE [dbo].[AdmissionEnquiryMaster](
	[EnquiryID] [int] IDENTITY(1,1) NOT NULL,
	[EDate] [datetime] NULL,
	[FatherName] [nvarchar](50) NULL,
	[MotherName] [nvarchar](50) NULL,
	[FatherMobileNo] [nvarchar](50) NULL,
	[MotherMobileNo] [nvarchar](50) NULL,
	[FatherEmailID] [nvarchar](50) NULL,
	[MotherEmailID] [nvarchar](50) NULL,
	[FOccupation] [nvarchar](50) NULL,
	[MOccupation] [nvarchar](50) NULL,
	[FamilyIncome] [numeric](18, 2) NULL,
	[Address] [nvarchar](max) NULL,
	[StudentName] [nvarchar](50) NULL,
	[AppliedForSession] [int] NULL,
	[AppliedForClass] [int] NULL,
	[CurrentClass] [int] NULL,
	[ESource] [int] NULL,
	[CurrentSchool] [nvarchar](50) NULL,
	[CurrentSchoolAddress] [nvarchar](max) NULL,
	[ReasonForChange] [nvarchar](max) NULL,
	[Gender] [bit] NULL,
	[Nationality] [nvarchar](50) NULL,
	[CurrentEducationSystem] [nvarchar](50) NULL,
	[StudentDOB] [date] NULL,
	[EStatus] [int] NULL,
	[EPossibility] [int] NULL,
	[NextFollowUpDate] [datetime] NULL,
	[SBranchID] [int] NULL,
	[UserID] [int] NULL,
	[CreatedDate] [date] NULL,
	[StudentID] [int] NULL,
	[PaymentStatus] [int] NULL,
	[PaymentDetails] [nvarchar](500) NULL,
	[AssignedTo] [int] NULL,
	[EPortal] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
GO
CREATE TABLE [dbo].[BlackBoardImages](
	[BlackBoardID] [nvarchar](50) NULL,
	[BBMIID] [nvarchar](50) NULL,
	[Photo] [nvarchar](50) NULL,
	[Detail] [nvarchar](max) NULL,
	[SBranchID] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
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
GO
CREATE TABLE [dbo].[BookCategory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[SBranchID] [int] NULL
) ON [PRIMARY]
GO
GO
CREATE TABLE [dbo].[BookCopyDetail](
	[CopyID] [int] IDENTITY(1,1) NOT NULL,
	[BookID] [int] NULL,
	[BarCode] [nvarchar](50) NULL,
	[RFID] [nvarchar](50) NULL,
	[AStatus] [int] NULL,
	[Code] [nvarchar](20) NULL,
 CONSTRAINT [PK_BookCopyDetail] PRIMARY KEY CLUSTERED 
(
	[CopyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[BookMaster](
	[BookID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](500) NULL,
	[Publisher] [nvarchar](500) NULL,
	[BayID] [int] NULL,
	[Description] [nvarchar](max) NULL,
	[Copies] [int] NULL,
	[Status] [bit] NULL,
	[Price] [numeric](10, 2) NULL,
	[Author] [nvarchar](100) NULL,
	[Image] [nvarchar](500) NULL,
	[CategoryIDs] [nvarchar](50) NULL,
	[ClassesIDs] [nvarchar](50) NULL,
	[ISBN] [nvarchar](50) NULL,
	[ISBN13] [nvarchar](50) NULL,
	[SBranchID] [int] NULL,
	[BookCode] [nvarchar](50) NULL,
	[IssueDays] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[CityMaster]
ADD SBranchID int
GO
ALTER TABLE [dbo].[EmployeeTypeMaster]
ADD [IsDefault] [int]
GO
ALTER TABLE [dbo].[FeeTypeMaster]
ADD [Months] [nvarchar](50)
GO
CREATE TABLE [dbo].[GSTStateMaster](
	[StateID] [int] IDENTITY(1,1) NOT NULL,
	[StateName] [nvarchar](50) NULL,
	[StateCode] [nvarchar](50) NULL
) ON [PRIMARY]
GO
GO
Alter Table [dbo].[HolidayMaster]
ALTER COLUMN Classes [nvarchar](500);
GO
Alter Table [dbo].[HolidayMaster]
ALTER COLUMN Title [nvarchar](500);
GO
GO
CREATE TABLE [dbo].[ImportantContacts](
	[ICID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Contact] [nvarchar](50) NULL,
	[SBranchID] [int] NULL
) ON [PRIMARY]
GO
GO
CREATE TABLE [dbo].[LeaveDetails](
	[LDID] [int] IDENTITY(1,1) NOT NULL,
	[LeaveID] [int] NULL,
	[LeaveTypeID] [int] NULL,
	[Applied] [numeric](10, 2) NULL,
	[LeaveDate] [date] NULL
) ON [PRIMARY]
GO
Alter Table [dbo].[LeaveTypeMaster]
Add [SBranchID] [int]
GO
Alter Table [dbo].[LeaveTypeMaster]
Add [IsSalaryDeduct] [int]
GO
Alter Table [dbo].[LeaveTypeMaster]
Add [SequenceNo] [int]
GO
Alter Table [dbo].[LeaveTypeMaster]
Add [IsCarryForward] [int]
GO
Alter Table [dbo].[LeaveTypeMaster]
Add [MaxConsecutive] [int]
GO
CREATE TABLE [dbo].[NationalityMaster](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL
) ON [PRIMARY]
GO
ALTER Table [dbo].[PaymentMaster]
ADD [OTP] [nvarchar](10)
GOGO
CREATE TABLE [dbo].[LibraryIssueBooks](
	[BookIssueID] [int] IDENTITY(1,1) NOT NULL,
	[BookID] [int] NULL,
	[CopyID] [int] NULL,
	[IssueDays] [int] NULL,
	[IssueDate] [date] NULL,
	[Remark] [nvarchar](500) NULL,
	[LibraryIssueID] [int] NULL,
	[Status] [int] NULL
) ON [PRIMARY]
GO
GO
CREATE TABLE [dbo].[LibraryIssueRegister](
	[IssueID] [int] IDENTITY(1,1) NOT NULL,
	[BorrowerType] [int] NULL,
	[BorrowerID] [int] NULL,
	[ClassID] [int] NULL,
	[IssueDate] [date] NULL,
	[IssuedDays] [int] NULL,
	[LibraryID] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[Remark] [nvarchar](500) NULL,
	[Status] [int] NULL,
	[SessionID] [int] NULL
) ON [PRIMARY]
GO
GO
CREATE TABLE [dbo].[LibraryMaster](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NULL,
	[Status] [bit] NULL,
	[Type] [int] NULL,
	[MasterID] [int] NULL,
	[Detail] [nvarchar](500) NULL,
	[SBranchID] [int] NULL,
 CONSTRAINT [PK_LibraryMaster] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PaymentDetails]
ADD [PaymentStatus] [int] 
GO
ALTER TABLE [dbo].[PaymentDetails]
ADD [PaymentTitle] [nvarchar](100)
GO
ALTER TABLE [dbo].[PaymentMaster]
ADD [StatusRemark] [nvarchar](500)
GO
ALTER TABLE [dbo].[PaymentMaster]
ADD [Status] [int]
GO

ALTER TABLE [dbo].[PaymentMaster]
ADD [CreatedDate] [datetime]
GO
ALTER TABLE [dbo].[ProductCategories]
ADD [HSNCode] [nvarchar](50)
GO
ALTER TABLE [dbo].[ProductCategories]
ADD [SGST] [nvarchar](50)
GO
ALTER TABLE [dbo].[ProductCategories]
ADD [CGST] [nvarchar](50)
GO
ALTER TABLE [dbo].[ProductCategories]
ADD [IGST] [nvarchar](50)
GO
GO
CREATE TABLE [dbo].[RefundMaster](
	[RefundID] [int] IDENTITY(1,1) NOT NULL,
	[RefundTo] [int] NULL,
	[RefundDate] [datetime] NULL,
	[CreatedDate] [datetime] NULL,
	[RefundBy] [int] NULL,
	[Reason] [nvarchar](max) NULL,
	[SessionID] [int] NULL,
	[SBranchID] [int] NULL,
	[RefundAmount] [numeric](10, 2) NULL,
	[Status] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
GO
CREATE TABLE [dbo].[SalaryApplicableMaster](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[SalaryApplicableMaster] ON 
GO
INSERT [dbo].[SalaryApplicableMaster] ([ID], [Name]) VALUES (1, N'Annual')
GO
INSERT [dbo].[SalaryApplicableMaster] ([ID], [Name]) VALUES (2, N'Monthly')
GO
INSERT [dbo].[SalaryApplicableMaster] ([ID], [Name]) VALUES (3, N'Quaterly')
GO
INSERT [dbo].[SalaryApplicableMaster] ([ID], [Name]) VALUES (4, N'Half Yearly')
GO
INSERT [dbo].[SalaryApplicableMaster] ([ID], [Name]) VALUES (5, N'Transport')
GO
INSERT [dbo].[SalaryApplicableMaster] ([ID], [Name]) VALUES (6, N'Hostel')
GO
INSERT [dbo].[SalaryApplicableMaster] ([ID], [Name]) VALUES (7, N'Misc Fine')
GO
INSERT [dbo].[SalaryApplicableMaster] ([ID], [Name]) VALUES (9, N'Specific Months')
GO
SET IDENTITY_INSERT [dbo].[SalaryApplicableMaster] OFF
GO
GO
CREATE TABLE [dbo].[SalaryProcessingDetails](
	[SPDID] [int] IDENTITY(1,1) NOT NULL,
	[SPMID] [int] NULL,
	[EmployeeID] [int] NULL,
	[SalaryCategoryID] [int] NULL,
	[RefID] [int] NULL,
	[Amount] [numeric](10, 2) NULL,
	[Paid] [numeric](10, 2) NULL,
	[IsDuePaid] [int] NULL,
	[SMonth] [int] NULL,
	[SYear] [int] NULL,
	[Status] [int] NULL,
	[Title] [nvarchar](50) NULL
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[SalaryProcessingMaster](
	[SPMID] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeID] [int] NULL,
	[SalaryProcessingDate] [datetime] NULL,
	[SMonth] [int] NULL,
	[SYear] [int] NULL,
	[ReferanceNumber] [nvarchar](500) NULL,
	[Remark] [nvarchar](max) NULL,
	[UserID] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[PaidAmount] [numeric](10, 2) NULL,
	[Status] [int] NULL,
	[SalaryReferanceNumber] [nvarchar](50) NULL,
	[SalaryReferanceSequence] [int] NULL,
	[SBranchID] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE SalaryTypeMaster
ADD [SBranchID] [int] 
GO
ALTER TABLE SalaryTypeMaster
ADD [Months] [nvarchar](50)
GO

ALTER TABLE SalaryTypeMaster
ADD [IsDeduction] [int]
GO

ALTER TABLE SalaryTypeMaster
ADD [AttendanceType] [int]
GO

ALTER TABLE SalaryTypeMaster
ADD [MinDays] [int]
GO
ALTER TABLE [dbo].[SBranchMaster]
ADD [StateID] [int] 
GO
ALTER TABLE [dbo].[SBranchMaster]
ADD [PrincipalSignature] [nvarchar](50)
GO
CREATE TABLE [dbo].[SMSConfigurationMaster](
	[SMSConfigID] [int] IDENTITY(1,1) NOT NULL,
	[SBranchID] [int] NULL,
	[IsDefault] [int] NULL,
	[Title] [nvarchar](50) NULL,
	[Status] [int] NULL,
	[header] [nvarchar](500) NULL,
	[baseurl] [nvarchar](500) NULL,
	[balanceURL] [nvarchar](500) NULL,
	[CreatedDate] [datetime] NULL
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[SMSConfigurationParams](
	[SMSParamID] [int] IDENTITY(1,1) NOT NULL,
	[SMSConfigID] [int] NULL,
	[ParamType] [int] NULL,
	[ParamName] [nvarchar](50) NULL,
	[ParamValue] [nvarchar](50) NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[StockTransactionDetails]
ADD [MRP] [numeric](10, 2)
GO
ALTER TABLE [dbo].[StockTransactionDetails]
ADD [Cost] [numeric](10, 2)

GO
ALTER TABLE [dbo].[StockTransactionDetails]
ADD [SGST] [numeric](5, 2)

GO
ALTER TABLE [dbo].[StockTransactionDetails]
ADD [CGST] [numeric](5, 2)

GO
ALTER TABLE [dbo].[StockTransactionDetails]
ADD [IGST] [numeric](5, 2)
GO
ALTER TABLE [dbo].[StockTransactionMaster]
ADD [ClassID] [int]
GO
ALTER TABLE [dbo].[StockTransactionMaster]
ADD [SectionID] [int]
GO
ALTER TABLE [dbo].[StockTransactionMaster]
ADD [SessionID] [int]
GO
ALTER TABLE [dbo].[StockTransactionMaster]
ADD [EmployeeTypeID] [int]
GO
ALTER TABLE [dbo].[StockTransactionMaster]
ADD [VendorID] [int]
GO
ALTER  TABLE [dbo].[Student_Session]
ADD [IsCustomFee] [int]
GO
CREATE TABLE [dbo].[StudentFeeDetails](
	[SFID] [int] IDENTITY(1,1) NOT NULL,
	[StudentID] [int] NULL,
	[FeeTypeID] [int] NULL,
	[SessionID] [int] NULL,
	[FeeAmount] [numeric](10, 2) NULL,
	[IsApplicable] [int] NULL
) ON [PRIMARY]
GO
ALTER  TABLE [dbo].[StudentMaster]
ADD [PSchoolName] [nvarchar](max) 
GO
ALTER  TABLE [dbo].[StudentMaster]
ADD [PSchoolMedium] [nvarchar](100) 
GO
ALTER  TABLE [dbo].[StudentMaster]
ADD [PClassName] [nvarchar](100)
GO
ALTER  TABLE [dbo].[StudentMaster]
ADD [PSResult] [nvarchar](100)
GO
ALTER  TABLE [dbo].[StudentMaster]
ADD [PSchoolCity] [nvarchar](100)
GO
ALTER  TABLE [dbo].[StudentMaster]
ADD [PSchoolState] [nvarchar](100)
GO
GO
CREATE TABLE [dbo].[SubReligionMaster](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[ReligionID] [int] NULL
) ON [PRIMARY]
GO
DROP  TABLE [dbo].[SubSubjectTypes]
GO
CREATE TABLE [dbo].[SubSubjectTypes](
	[TypeID] [int] IDENTITY(1,1) NOT NULL,
	[TypeName] [nvarchar](50) NULL,
	[SBranchID] [int] NULL,
	[GroupID] [int] NULL
) ON [PRIMARY]
GO
GO
CREATE TABLE [dbo].[ToDoList](
	[ToDoID] [int] IDENTITY(1,1) NOT NULL,
	[Priority] [int] NULL,
	[ScheduledDateTime] [datetime] NULL,
	[Status] [int] NULL,
	[Details] [nvarchar](max) NULL,
	[UserID] [int] NULL,
	[SBranchID] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[UserType] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER  TABLE [dbo].[TransportHostalAllocationDelocation]
ADD [VehicleRouteID] [int]

--Completed till Line Number 7790
GO
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
CREATE procedure [dbo].[getStudentListOnSessionSection]
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
 GO
ALTER proc [dbo].[sp_AddUserToEsslDevice]
(
@Name nvarchar(50),
@UserDeviceCode nvarchar(50),
@SBranchID	int,
@CardNumber nvarchar(50),
@CreatedDate datetime
)
AS
BEGIN
	declare @Command nvarchar(max)
	set @CardNumber=isnull(@CardNumber,'0000')
	set @Command='C:UniqueId:DATA USER PIN='+@UserDeviceCode+'	Name='+@Name+'	Pri=0	Passwd=	Card='+@CardNumber+'	Grp=1'
	declare @Title nvarchar(max)
	set @Title='Add User '+@UserDeviceCode


		declare @DeviceID nvarchar(max)
		DECLARE db_cursor CURSOR FOR  
		SELECT SerialNumber 
		FROM Devices where SBranchID=@SBranchID

		OPEN db_cursor   
		FETCH NEXT FROM db_cursor INTO @DeviceID

		WHILE @@FETCH_STATUS = 0   
		BEGIN   
			 insert into [DeviceCommands] ([Title]  ,[DeviceCommand],[SerialNumber],[Status],[Type],[CreationDate])
			values(@Title,@Command,@DeviceID,'Pending','Add User',@CreatedDate)
			  FETCH NEXT FROM db_cursor INTO @DeviceID
		END   

		CLOSE db_cursor   
		DEALLOCATE db_cursor

	
	
END
GO

CREATE proc [dbo].[sp_DeleteBook]
(
@BookID int
)
AS
BEGIN
	Delete from BookMaster where BookID=@BookID
END
GO
ALTER proc [dbo].[sp_DeleteUserToEsslDevice]
(
@UserDeviceCode nvarchar(50),
@SBranchID	int,
@CreatedDate datetime
)
AS
BEGIN
	declare @Command nvarchar(max)
	set @Command='C:UniqueId:DATA DEL_USER PIN='+@UserDeviceCode
	declare @Title nvarchar(max)
	set @Title='Delete User '+@UserDeviceCode


		declare @DeviceID nvarchar(max)
		DECLARE db_cursor CURSOR FOR  
		SELECT SerialNumber 
		FROM Devices where SBranchID=@SBranchID

		OPEN db_cursor   
		FETCH NEXT FROM db_cursor INTO @DeviceID

		WHILE @@FETCH_STATUS = 0   
		BEGIN   
			 insert into [DeviceCommands] ([Title]  ,[DeviceCommand],[SerialNumber],[Status],[Type],[CreationDate])
			values(@Title,@Command,@DeviceID,'Pending','Delete User',@CreatedDate)
			  FETCH NEXT FROM db_cursor INTO @DeviceID
		END   

		CLOSE db_cursor   
		DEALLOCATE db_cursor
END
GO
ALTER proc [dbo].[sp_GetAccountDashboardData]
(
@Month nvarchar(2),
@Year nvarchar(5),
@Day nvarchar(3),
@SBranchID int
)
AS
BEGIN
	exec [dbo].[sp_GetAccountDashFeeChart] @Month,@Year,@SBranchID
	Declare @SessionID int
	Select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and SessionStatus=1
	Select isnull(Gender,0) as Name,count(*) as ID from StudentMaster where StudentID in 
	(select StudentID from Student_Session where Status=1 and SessionID=@SessionID) and SBranchID=@SBranchID
	group by isnull(Gender,0)
	order by isnull(Gender,0)
	
	Select isnull(Gender,0) as Name,count(*) as ID from EmployeeMaster where Status=1 and SBranchID=@SBranchID
	group by isnull(Gender,0)
	order by isnull(Gender,0)

	declare @MonthSale numeric(10,2)
	declare @MonthPurchase numeric(10,2)

	select @MonthSale=sum((Quantity*Cost) +Quantity*Cost*SGST/100+Quantity*Cost*CGST/100+Quantity*Cost*IGST/100)/1000
	from StockTransactionDetails STD left outer join StockTransactionMaster STM on STD.STID=STM.STID
	where STM.Status=1 and STType=1 and DatePart(year,STM.TrDate)=@Year and DatePart(month,STM.TrDate)=@Month

	select @MonthPurchase=sum((Quantity*Cost) +Quantity*Cost*SGST/100+Quantity*Cost*CGST/100+Quantity*Cost*IGST/100)/1000
	from StockTransactionDetails STD left outer join StockTransactionMaster STM on STD.STID=STM.STID
	where STM.Status=1 and STType=0 and DatePart(year,STM.TrDate)=@Year and DatePart(month,STM.TrDate)=@Month

	select isnull(@MonthSale,0)
	select isnull(@MonthPurchase,0)
	--select isnull(sum(SMSCredited),0) from SMSBalanceMaster where SBranchID=@SBranchID and Status=1

	--select count(*) from SMSProcessingLog where SMSID in (Select SMSSendingID from SMSSendingMaster where SBranchID=@SBranchID) and Status=1
END
GO
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
ALTER proc [dbo].[sp_GetAdminGroups]
(
@SBranchID int
)
AS
BEGIN
	
		Select GroupName,GroupID,EducationLevelID,Status,
		(select COUNT(*) from SubjectMaster SM where SM.GroupID=GM.GroupID) as Subjects ,
		(select COUNT(*) from [SubSubjectTypes] SM where SM.GroupID=GM.GroupID) as Types 
		from GroupMaster GM where  SBranchID=@SBranchID
		select ID,Name from EducationLevelMaster where Status=1 and SBranchID=@SBranchID
END
GO
ALTER  proc [dbo].[sp_GetAdminLocationList]
(
 @SBranchID int,
 @MasterID int,
 @Type int
)
AS
BEGIN
	if(@Type=1)
	begin
		Select CountryID as ID,CountryName as Name,IsApproved,1 as [Type],
		(Select Count(*) from StateMaster SM where SM.CountryID=CM.CountryID) as [Count]
		 from CountryMaster CM 
	end
	else if(@Type=2)
	begin
		Select StateID as ID,StateName as Name,IsApproved,2 as [Type],
		(Select Count(*) from CityMaster CM where CM.StateID=SM.StateID) as [Count]
		 from StateMaster SM where SM.CountryID=@MasterID  
	end
	else if(@Type=3)
	begin
		Select CityID as ID,CityName as Name,IsApproved,3 as [Type],
		(Select Count(*) from AreaMaster AM where AM.CityID=CM.CityID) as [Count]
		 from CityMaster CM where CM.SBranchID=@SBranchID  
		 order by CityName 
	end
	else if(@Type=4)
	begin
		Select AreaID as ID,AreaName as Name,IsApproved,PinCode as Other ,4 as [Type],Latitude,Longitude,
		(Select Count(*) from TransportRouteDetails TRD where TRD.AreaID=AM.AreaID) as [Count]
		from AreaMaster AM where CityID=@MasterID 
		order by AreaName
	end
END
GO
ALTER  proc [dbo].[sp_GetAdminSBranches]
(
@SBranchID int=0
)
AS
BEGIN
	if(@SBranchID=0)
	begin
		Select SBranchID,BranchName,[Logo],[ContactNo],[EmailID],[Address],[PrincipalName],[PrincipalMobile]
		,[PrincipalEmail],[BranchSchoolName],StateID,PrincipalSignature,
		(Select count(*) from Student_Session SS where Status=1 and SS.SBranchID=SBM.SBranchID) as Students,
		(Select UserName from LoginDetails LD where LD.SBranchID= SBM.SBranchID and UserType=2) as AccountUserName
		 from [dbo].[SBranchMaster] SBM

	end
	else
	begin
		Select SBranchID,BranchName,[Logo],[ContactNo],[EmailID],[Address],[PrincipalName],[PrincipalMobile]
		,[PrincipalEmail],[BranchSchoolName],StateID,PrincipalSignature,
		(Select count(*) from Student_Session SS where Status=1 and SS.SBranchID=SBM.SBranchID) as Students,
		(Select UserName from LoginDetails LD where LD.SBranchID= SBM.SBranchID and UserType=2) as AccountUserName
		 from [dbo].[SBranchMaster] SBM where SBranchID=@SBranchID

	end
END

GO
CREATE procedure [dbo].[sp_GetAdmissionEnquiryFollowups]
(
	@EnquiryID int
)
as 
begin 
	SELECT [FollowupID],[EnquiryID],[FollowupBy],[FollowupByName],[Remark],[FDate],[Status],[Possibility],[NextFollowupDate],[CreatedDate],
	(Case when FollowupBy<>0 then (Select EmployeeName from EmployeeMaster where EmployeeID=FollowupBy) else [FollowupByName] end) as [FollowupByName]
	FROM [AdmissionEnquiryFollowups] where EnquiryID=@EnquiryID 
	order by FDate desc
end
GO
ALTER  proc [dbo].[sp_GetAllEmployeeTypes]
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
Create proc [dbo].[sp_GetBookByBarCode]
(
@BarCode nvarchar(100),
@SBranchID int
)
AS
BEGIN
	Select BCD.BookID,BCD.CopyID,BM.IssueDays,BM.[Status],
	BCD.BarCode,BCD.RFID,BCD.Code, BM.Title,BM.[Image]
	from [dbo].[BookCopyDetail] BCD left outer join [dbo].[BookMaster] BM on BCD.BookID=BM.BookID
	where BarCode=@BarCode
End
GO
CREATE proc [dbo].[sp_GetBookCategories]
(
@SBranchID int
)
AS
BEGIN
	Select ID,Name,
	(Select count(*) from BookMaster where CategoryIDs like '%,'+cast(ID as nvarchar(10))+',%') as Extra1
	from BookCategory where SBranchID=@SBranchID
END
GO
Create proc [dbo].[sp_GetBookCopies]
(
@BookID int
)
AS
BEGIN
	Select CopyID,BarCode,RFID,AStatus,Code from BookCopyDetail where BookID=@BookID
END

GO
CREATE proc [dbo].[sp_GetBookDetails]
(
@BookID int,
@SBranchID int
)
AS
BEGIN
	declare @BayID int
	declare @LibraryID int
	declare @FloorID int
	declare @BlockID int
	Select BookID,Title,[Description],Copies,[Status],Price,Publisher,Author,BayID,[Image],BookCode,IssueDays,
	isnull(CategoryIDs,',0,') as CategoryIDs,isnull(ClassesIDs,',0,') as ClassesIDs,ISBN,ISBN13,[dbo].[GetClassNames](ClassesIDs) as Classes,
	dbo.GetBookCategoryNames(CategoryIDs) as CategoryNames
	from BookMaster where BookID=@BookID
	select @BayID=BayID from BookMaster where BookID=@BookID

	Select ID,name from BookCategory where SBranchID=@SBranchID

	Select ClassID as ID, ClassName as Name from ClassMaster where SBranchID=@SBranchID

	if(@BayID=0)
	begin	
		select @BayID=min(ID) from  LibraryMaster where SBranchID=@SBranchID and [Type]=3
		select @BlockID=MasterID from  LibraryMaster where ID=@BayID
		select @FloorID=MasterID from  LibraryMaster where ID=@BlockID
		select @LibraryID=MasterID from  LibraryMaster where ID=@FloorID
		
		--Library
		select ID,Name from LibraryMaster where SBranchID=@SBranchID and [Type]=0
		--Floors
		select ID,Name from LibraryMaster where SBranchID=@SBranchID and [Type]=1 and MasterID=@LibraryID
		--Blocks
		select ID,Name from LibraryMaster where SBranchID=@SBranchID and [Type]=2 and MasterID=@FloorID
		--Bays
		select ID,Name from LibraryMaster where SBranchID=@SBranchID and [Type]=3 and MasterID=@BlockID
	end
	else
	begin	
		select @BlockID=MasterID from  LibraryMaster where ID=@BayID
		select @FloorID=MasterID from  LibraryMaster where ID=@BlockID
		select @LibraryID=MasterID from  LibraryMaster where ID=@FloorID
		
		--Library
		select ID,Name from LibraryMaster where SBranchID=@SBranchID and [Type]=0
		--Floors
		select ID,Name from LibraryMaster where SBranchID=@SBranchID and [Type]=1 and MasterID=@LibraryID
		--Blocks
		select ID,Name from LibraryMaster where SBranchID=@SBranchID and [Type]=2 and MasterID=@FloorID
		--Bays
		select ID,Name from LibraryMaster where SBranchID=@SBranchID and [Type]=3 and MasterID=@BlockID
	end

	select isnull(@LibraryID,0)
	select isnull(@FloorID,0)
	select isnull(@BlockID,0)
	select isnull(@BayID,0)
END
GO
CREATE proc [dbo].[sp_GetBookForIssue]
(
@Text nvarchar(100),
@SBranchID int
)
AS
BEGIN
	Select cast(BCD.BookID as nvarchar(10))+'^'+cast(BCD.CopyID as nvarchar(10))+'^'+cast(BM.IssueDays as nvarchar(10))+'^'+
	isnull(BCD.BarCode,'')+'^'+isnull(BCD.RFID,'')+'^'+isnull(BCD.Code,'')+'^'+isnull(BM.[Image],'') as id,'('+BCD.Code+') '+ BM.Title as [text]
	from [dbo].[BookCopyDetail] BCD right outer join [dbo].[BookMaster] BM on BCD.BookID=BM.BookID 
	and BCD.CopyID not in (select CopyID from LibraryIssueBooks where [Status]=0 and BookID=BM.BookID)
	where BM.Title like '%'+@Text+'%' or BarCode like '%'+@Text+'%'
End
GO
CREATE procedure [dbo].[sp_GetClassGroupFeeListOnly]
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
GO
CREATE Procedure [dbo].[sp_GetClassGroupWiseStudentFeeSummery]
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
ALTER  proc [dbo].[sp_GetClassTeacherClassSections]
(
@Day nvarchar(3),
@Month nvarchar(3),
@Year nvarchar(5),
@TeacherID int
)
AS
BEGIN
	declare @ClassID nvarchar(100)
	declare @SectionID  nvarchar(100)
	Declare @ClassName nvarchar(100)
	Declare @SectionName nvarchar(100)
	Declare @CurDate Date

	declare  @ClassSectionAttandanceDetails  table (ClassID int,SectionID int, ClassName nvarchar(100),SectionName nvarchar(100),Status int)

	Declare @HCount int
	Declare @DaysWorking int
	Declare @Count int
	Declare @SQL nvarchar(max)
	declare @IsExist int
	DECLARE db_cursor CURSOR FOR 
	select ClassID,SectionID,ClassName,SectionName from [v_ClassSectionNames] where SectionID in (Select ID from Class_Sections where TeacherID=@TeacherID and Status=1)
	OPEN db_cursor   
		FETCH NEXT FROM db_cursor INTO @ClassID,@SectionID,@ClassName,@SectionName
		WHILE @@FETCH_STATUS = 0   
		BEGIN 
		
		set @CurDate= cast(@Month+'-'+cast(@Day as nvarchar(2))+'-'+@Year as Date)
		If datepart(dw, @CurDate) = 1 Set @Count=-1
		else
		Begin   
			select @DaysWorking= [Days] from EducationLevelMaster where ID=(select  EducationLevelID from ClassMaster where ClassID=@ClassID)
			if(@DaysWorking<datepart(dw, @CurDate)) Set @Count=-1
			else
			Begin
				Select @HCount=count(*) from HolidayMaster where cast(@Month+'-'+cast(@Day as nvarchar(2))+'-'+@Year as Date) between StartDate and EndDate 
				and Status=1 and DayDuration=1 and ((select count(*) from dbo.SplitStringToTable(Classes,',') where ITem=@ClassID or Item=0)>0) and IsStudents=1
				if(@HCount>0)
				begin
					set @Count=-1
				end
				else 
				begin	
					select @IsExist=count(*) from [dbo].[StudentAttendanceMasterT] where ClassID=@ClassID and SectionID=@SectionID and Month=@Month and FYear=@Year
					if(@IsExist<>0)
					begin
						set @SQL='select @cnt=Count(*) from [dbo].[StudentAttendanceMasterT] where ClassID='+@ClassID+' and SectionID='+@SectionID+' and D'+@Day+'=3 and Month='+@Month+' and FYear='+@Year	
						EXECUTE sp_executesql @SQL, N'@cnt int OUTPUT',@cnt=@Count OUTPUT
					end
					else 
					begin
						Set @Count=1
					end
					if(@Count>0)
					begin
						Set @Count=1
					end
				end
			End
		End
		insert into @ClassSectionAttandanceDetails(ClassID,SectionID,ClassName,SectionName,Status)
		values(@ClassID,@SectionID,@ClassName,@SectionName,@Count)
		FETCH NEXT FROM db_cursor INTO @ClassID,@SectionID,@ClassName,@SectionName
		END   
		CLOSE db_cursor   
		DEALLOCATE db_cursor

		Select * from @ClassSectionAttandanceDetails
END
GO
Create proc [dbo].[sp_GetConvertedEnquieirsForAdmission]
(
@SBranchID int,
@EStatus int
)
AS
BEGIN
	SELECT [EnquiryID],[FatherName],[MotherName],[FatherMobileNo],[MotherMobileNo],[FatherEmailID],[MotherEmailID],[FOccupation],[MOccupation],[FamilyIncome]
	,[Address],[StudentName],[AppliedForSession],[AppliedForClass],[CurrentClass],[ESource],[CurrentSchool],[CurrentSchoolAddress],[ReasonForChange],[Gender]
	,[Nationality],[CurrentEducationSystem],[StudentDOB],[EStatus],[EPossibility],[NextFollowUpDate],[SBranchID],[UserID],[CreatedDate],[StudentID],[PaymentStatus]
	,[PaymentDetails],[AssignedTo],[EPortal]
	  FROM [AdmissionEnquiryMaster] where EStatus=@EStatus and SBranchID=@SBranchID
END
GO
GO
CREATE proc [dbo].[sp_GetDeskSlip]
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

	Select SS.StudentSessionUID, SS.RollNo, SM.Name, SM.StudentID,SM.Gender,SM.Photo,PM.FatherName,PM.MotherName,PM.FatherMobileNo as MobileNo,SM.DOB,SM.SchoolUID,
	@ClassName as ClassName,@SectionName as SectionName,@SessionName as SessionName,@EvaluationName as EvaluationName
	,'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6)  as StudentSID
	from StudentMaster SM right outer join Student_Session SS on SS.StudentID=SM.StudentID
	left outer join ParentMaster PM on PM.ParentID=SM.ParentID
	Where SS.SectionID=@SectionID and SS.SessionID=@SessionID
	order by SM.Name

	select * from SBranchMaster where SBranchID=@SBranchID

end

GO
ALTER  proc [dbo].[sp_GetEducationLevelForPeriods]
(
@SBranchID int
)
AS
BEGIN
	Select ELM.ID,ELM.Name,ELM.Days,ELM.ShiftType,
	(Select Count(*) from PeriodMaster PM where PM.EducationLevelID=ELM.ID and [Status]=1) as Periods,
	(Select Count(*) from ClassMaster PM where PM.EducationLevelID=ELM.ID and [Status]=1) as Classes,
	cast ((Select Min(PM.StartTime) from PeriodMaster PM where PM.EducationLevelID=ELM.ID and [Status]=1) as nvarchar(10)) as PeriodStartTime,
	cast ((Select Max(PM.EndTime) from PeriodMaster PM where PM.EducationLevelID=ELM.ID and [Status]=1)as nvarchar(10)) as PeriodEndTime
	from EducationLevelMaster ELM where ELM.SBranchID=@SBranchID
END
GO
ALTER  proc [dbo].[sp_GetEmployeeAttandanceDetails]
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
where EM.SBranchID='+cast(@SBranchID as nvarchar(3))+' and EM.[DesignationID]='+cast(@EmployeeTypeID as nvarchar(3))+' and [Status]=1 and EM.DOJ<=cast('''+@Month+'/'+@Day+'/'+@Year+''' as date) order by EM.[EmployeeName]'
exec(@SQLQuery)
END

GO
GO
CREATE Procedure [dbo].[sp_GetEmployeeLeaveDetailsNew] --0,3,2,2020,1,0,1
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
ALTER  proc [dbo].[sp_GetEmployeeTypes]
(
@SBranchID int
)
AS
BEGIN
		Select EmployeeTypeID,EmployeeTypeName from [dbo].[EmployeeTypeMaster] where SBranchID=0 or SBranchID=@SBranchID
END


GO
ALTER  proc [dbo].[sp_GetEmployeeTypeSalaryDetails]
(
@EmployeeTypeID int,
@SBranchID int
)
AS
BEGIN
	
Select CFS.ETypeSTypeID,CFS.[EmployeeTypeID],FTM.TypeID,FTM.[TypeName],FTM.IsDeduction,FTM.TypeApplicable,isnull(CFS.Amount,0) as Amount ,FTM.Months,SAM.Name as TypeApplicableName
from [dbo].[SalaryTypeMaster] FTM left outer join [SalaryApplicableMaster] SAM on SAM.ID=FTM.TypeApplicable left OUTER JOIN
(Select [ETypeSTypeID],[EmployeeTypeID],[SalaryTypeID],[Amount] from 
 [dbo].[EmployeeTypeSalaryDetails] CFS where  isnull(EmployeeTypeID,0)=@EmployeeTypeID and SBranchID=@SBranchID) as CFS 
 on FTM.TypeID=CFS.SalaryTypeID 
	
END
GO
ALTER  proc [dbo].[sp_GetEvaluationSchemes] 
(
@SBranchID int,
@SessionID int=0
)
AS
BEGIN
	if(@SessionID=0)
	begin
		Select @SessionID=SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
	end
	Select EvaluationSchemeID,EvaluationSchemeName,[dbo].[GetEvaluationSchemeClassList](EvaluationSchemeID) as Classes,[dbo].[GetSchemeClassListIDs](EvaluationSchemeID) as ClassIDs 
	from EvaluationSchemeMaster where SBranchID=@SBranchID and SessionID=@SessionID
	select @SessionID
	Select SessionID,SessionName,SessionStatus,SessionStartDate,SessionEndDate from SessionMaster where SBranchID=@SBranchID order by SessionStartDate	
	
	select CM.ClassID,CM.ClassName,ESM.EvaluationSchemeName as EducationLevelName, ESM.EvaluationSchemeID 
	from ClassMaster CM left outer join [ClassSessionDetails] CSD on CSD.SessionID=@SessionID and CSD.ClassID=CM.ClassID
	left outer join EvaluationSchemeMaster ESM on ESM.EvaluationSchemeID=CSD.SchemeID
	where CM.SBranchID=@SBranchID and CM.Status=1
END
GO
ALTER  proc [dbo].[sp_GetFeeCollectionReport]
(
@FromDate date,
@ToDate date,
@ReportType int,
@PaymentMode int,
@SBranchID int,
@QuarterID int
)
AS
BEGIN
	Select PaymentModeID,PaymentModeName from PaymentModeMaster
	if(@ReportType=1)--Daily
	begin
		if(@PaymentMode=-1)
		begin
			select PaymentID,PaymentAmount,PaymentDate,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PaymentID) PayMonths,PaymentMode,PaymentRecieptNo,
			(Select Name from StudentMaster where StudentID=PayeeID) as Name,SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
			(Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
			 from PaymentMaster PM left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
			 left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
			 where cast(PM.PaymentDate as date)=cast(@FromDate as date) and PM.SBranchID=@SBranchID
		 end
		 else
		 begin
			select PaymentID,PaymentAmount,PaymentDate,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PaymentID) PayMonths,PaymentMode,PaymentRecieptNo,
			(Select Name from StudentMaster where StudentID=PayeeID) as Name,SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
			(Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
			 from PaymentMaster PM left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
			 left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
			 where cast(PM.PaymentDate as date)=cast(@FromDate as date) and PM.SBranchID=@SBranchID and PM.PaymentMode=@PaymentMode
		 end
	end
	else if(@ReportType=2)--Monthly
	begin
		if(@PaymentMode=-1)
		begin
			select PaymentID,PaymentAmount,PaymentDate,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PaymentID) PayMonths,PaymentMode,PaymentRecieptNo,
			(Select Name from StudentMaster where StudentID=PayeeID) as Name,SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
			(Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
			 from PaymentMaster PM left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
			 left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
			 where datepart(month,PM.PaymentDate)=datepart(month,@FromDate) and datepart(year,PM.PaymentDate)=datepart(year,@FromDate)  and PM.SBranchID=@SBranchID --and SS.SessionID<>3
		end
		else
		begin
			select PaymentID,PaymentAmount,PaymentDate,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PaymentID) PayMonths,PaymentMode,PaymentRecieptNo,
			(Select Name from StudentMaster where StudentID=PayeeID) as Name,SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
			(Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
			 from PaymentMaster PM left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
			 left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
			 where datepart(month,PM.PaymentDate)=datepart(month,@FromDate) and datepart(year,PM.PaymentDate)=datepart(year,@FromDate) and PM.SBranchID=@SBranchID and PM.PaymentMode=@PaymentMode
		end
	end
	else if(@ReportType=3)--Between Dates
	begin
		if(@PaymentMode=-1)
		begin
			select PaymentID,PaymentAmount,PaymentDate,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PaymentID) PayMonths,PaymentMode,PaymentRecieptNo,
			(Select Name from StudentMaster where StudentID=PayeeID) as Name,SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
			(Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
			 from PaymentMaster PM left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
			 left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
			 where cast(PM.PaymentDate as date) between @FromDate and @ToDate  and PM.SBranchID=@SBranchID
		end
		else
		begin
			select PaymentID,PaymentAmount,PaymentDate,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PaymentID) PayMonths,PaymentMode,PaymentRecieptNo,
			(Select Name from StudentMaster where StudentID=PayeeID) as Name,SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
			(Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
			 from PaymentMaster PM left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
			 left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
			 where cast(PM.PaymentDate as date) between @FromDate and @ToDate  and PM.SBranchID=@SBranchID and PM.PaymentMode=@PaymentMode
		end
	end
	else if(@ReportType=4)--Quarterly
		begin
		Declare @SessionStartDate date
		Declare @SessionEndDate date

		Select @SessionStartDate=SessionStartDate ,@SessionEndDate=SessionEndDate from SessionMaster where SBranchID=@SBranchID and SessionStatus=1
		if(@QuarterID=1)
		begin
			Set @FromDate=@SessionStartDate
			set @ToDate=dateadd(month,3,@SessionStartDate)
		end
		else if(@QuarterID=2)
		begin
			Set @FromDate=dateadd(month,3,@SessionStartDate)
			set @ToDate=dateadd(month,6,@SessionStartDate)
		end
		else if(@QuarterID=3)
		begin
			Set @FromDate=dateadd(month,6,@SessionStartDate)
			set @ToDate=dateadd(month,9,@SessionStartDate)
		end
		else
		begin
			Set @FromDate=dateadd(month,9,@SessionStartDate)
			set @ToDate=@SessionEndDate
		end
		if(@PaymentMode=-1)
		begin
			select PaymentID,PaymentAmount,PaymentDate,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PaymentID) PayMonths,PaymentMode,PaymentRecieptNo,
			(Select Name from StudentMaster where StudentID=PayeeID) as Name,SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
			(Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
			 from PaymentMaster PM left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
			 left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
			 where cast(PM.PaymentDate as date) between @FromDate and @ToDate  and PM.SBranchID=@SBranchID
		end
		else
		begin
			select PaymentID,PaymentAmount,PaymentDate,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PaymentID) PayMonths,PaymentMode,PaymentRecieptNo,
			(Select Name from StudentMaster where StudentID=PayeeID) as Name,SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
			(Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
			 from PaymentMaster PM left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
			 left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
			 where cast(PM.PaymentDate as date) between @FromDate and @ToDate  and PM.SBranchID=@SBranchID and PM.PaymentMode=@PaymentMode
		end
	end
select * from SBranchMaster where SBranchID=@SBranchID
End

GO

CREATE Procedure [dbo].[sp_GetFeeDiscountRequestDetailsV2] --3,'2019-07-01','2020-01-14',1,1
(
@StudentID int,
@QDate date,
@CurDate date,
@SBranchID int,
@SessionID int
)
as
begin
	declare @Month int=datepart(month,@QDate)
	declare @Year int=datepart(year,@QDate)
	Declare @FeeDetailTable table (FeeMonth int,FeeYear int, FeeTypeApplicable int, FeeTypeID int,FeeTypeName nvarchar(100),FeeAmount numeric(10,2),QDiscount numeric(10,2),
	RDiscount numeric(10,2),PayApplicableAmount numeric(10,2),CustomFee numeric(10,2),PaidAmount numeric(10,2),IsCustomFee int,IsPayment int)

	insert into @FeeDetailTable	exec sp_GetStudentFeeDetailsNew @StudentID,@SBranchID,@SessionID,@QDate,@CurDate
	
	Declare @DiscRequestID int
	select @DiscRequestID=max(DiscRequestID) from FeeDiscountRequestMaster where StudentID=@StudentID and FeeMonth=@Month and FeeYear=@Year
	 select DiscRequestID,StudentID,
	 Remark,RequestDate,FeeMonth,DATENAME(month, DATEADD(month, FeeMonth-1, CAST('2008-01-01' AS datetime))) as [MonthName],FeeYear
	 ,Status,DirectorRemark from FeeDiscountRequestMaster FDRM where DiscRequestID=@DiscRequestID
	 
	Select DiscDetailID,DiscRequestID,PMT.FeeTypeID,FDR.Amount,FDR.ApprovedAmount,PMT.FeeTypeName,
	(isnull((Case when IsPayment=1 then PayApplicableAmount when IsCustomFee=1 then CustomFee else (isnull(FeeAmount,0)-isnull(FeeAmount,0)*isnull(QDiscount,0)/100) end),0)-isnull(PMT.PaidAmount,0)) as NetApplicablePayment from FeeDiscountRequestDetails FDR
	right outer join @FeeDetailTable  PMT on PMT.FeeTypeID=FDR.FeeTypeID and DiscRequestID=@DiscRequestID
	where  PMT.FeeMonth=@Month and PMT.FeeYear=@Year and PMT.FeeTypeID<>-2
	--where (isnull(PMT.NetApplicablePayment,0)-isnull(PMT.PaymentRecieved,0))>0 
	order by FeeTypeID
	declare @StudentName nvarchar(100)
	select @StudentName=Name from StudentMaster where StudentID=@StudentID
	select isnull(@StudentName,'NA')

end
GO
ALTER Procedure [dbo].[sp_GetFeePaymentMonthwiseDetails]
(
@PaymentID int,
@Day int,
@Month int,
@Year int,
@StudentID int,
@QDate date,
@SessionID int
)
as
begin
	Declare @ClassID int
	Declare @GroupID int 
	Declare @SectionID int
	Declare @FeePaymentMode int
	Declare @SBranchID int
	Declare @QuotaID int
	Declare @SessionStartDate date
	Declare @StudentSessionDate date
	Declare @StudentSessionEndDate date
	Declare @LateFee numeric(10,2)
	Declare @LastPayDay int
	Declare @TransportFeeMode int
	Declare @HostelFeeMode int
	Declare @HostelFee numeric(10,2)
	Declare @TransportFee numeric(10,2)
	Declare @IsTransport int
	Declare @IsHostel int
	Declare @StopID int	
	Declare @PreviousDues numeric(10,2)=0
	Declare @StartDate date
	Declare @EndDate date
	Declare @BaseDate date
	Declare @IsAdmissionFee int

	select @SBranchID=SBranchID from Student_Session where StudentID=@StudentID and Status=1
	Declare @Discounts table(StudentID int,FeeTypeID int,FeeMonth int,FeeYear int,ApprovedAmount decimal(18,2))

	Declare @PreviousMonthsTable table(FeeTypeID int,FeeTypeName nvarchar(100),
	NetApplicablePayment numeric(10,2),DiscAmt numeric(10,2),PaymentRecieved numeric(10,2)
	,Year int,Month int,PaymentDate date)

	Declare @TransHostFeeTable table(FeeTypeID int,MonthType int,NetApplicablePayment numeric(10,2),DiscAmt numeric(10,2),
	PaymentRecieved numeric(10,2),PaymentDate date,FeeTypeApplicable int)
	
	Select @TransportFeeMode=details from MasterSettings where Type='TransportFeeMode' and SBranchID=@SBranchID
	Select @HostelFeeMode=details from MasterSettings where Type='HostelFeeMode' and SBranchID=@SBranchID
	Select @LastPayDay=details from MasterSettings where Type='FeePaymentReminderDate' and SBranchID=@SBranchID
	
	--Declare @CurDate date=cast(cast(@Month as nvarchar(3))+'-'+cast(1 as nvarchar(3))+'-'+cast(@Year as nvarchar(5)) as date)
	Declare @CurDate date=cast(cast(@Year as nvarchar(4))+'-'+cast(@Month as nvarchar(3))+'-'+cast(1 as nvarchar(5)) as date)
	--if(datepart(day,@QDate)>@LastPayDay)
	--begin
	--	SELECT @CurDate=EOMonth(@CurDate)-- dateadd(day,1,cast(cast(@Month as nvarchar(3))+'-'+cast(@LastPayDay as nvarchar(3))+'-'+cast(@Year as nvarchar(5)) as date))
	--end

	
	Select @IsTransport=StopID,@IsHostel=HostelRoomID,@StopID=StopID from StudentMaster where StudentID=@StudentID

	Insert into @Discounts Select StudentID,FeeTypeID,FeeMonth,FeeYear,ApprovedAmount 
	from [FeeDiscountRequestDetails] FDD left outer join [FeeDiscountRequestMaster] FDM on FDM.DiscRequestID=FDD.DiscRequestID 
	 where StudentID=@StudentID and Status=1

	Select @StudentSessionDate=FromDate,@StudentSessionEndDate=ToDate,@ClassID=ClassID,@QuotaID=QuotaID,
	@SectionID=SectionID,@SessionID=SessionID,@FeePaymentMode=FeePaymentMode,@SBranchID=SBranchID ,@IsAdmissionFee=isnull(IsAdmissionFeeApplicable,0)
	from Student_Session where StudentID=@StudentID and SessionID=@SessionID

	Select @GroupID=GroupID from Class_Sections where ID=@SectionID

	select @SessionStartDate=SessionStartDate,@EndDate=SessionEndDate from SessionMaster where SessionID=@SessionID
	if(@SessionStartDate>@StudentSessionDate)
	begin
		set @StudentSessionDate=@SessionStartDate
	end
	if(@EndDate>@StudentSessionEndDate)
	begin
		set @EndDate=@StudentSessionEndDate
	end
	if((datepart(month,@EndDate)+datepart(year,@EndDate)*12)<(datepart(month,@CurDate)+datepart(year,@CurDate)*12))
	begin
		set @CurDate=@EndDate
		set @Month=datepart(month,@CurDate)
		set @Year=datepart(year,@CurDate)
	end

	set @StartDate=@StudentSessionDate
	set @BaseDate=@StartDate
	select @PaymentID=Max(PaymentID) from PaymentDetails 
	where Month=datepart(month,@QDate) and Year=datepart(year,@QDate) and PayeeID=@StudentID
	and PaymentDate>@StartDate

	Select PaymentID,PaymentMode,ReferanceNumber,Remark,@FeePaymentMode as FeePaymentMode,
	(Select sum(PaymentRecieved) from PaymentDetails PD where PD.PaymentID=PM.PaymentID) as PaymentAmount,
	[dbo].[GetPaymentMonthNames](PM.PaymentID) as PayMonths
	 from PaymentMaster PM	
	where PaymentID=(@PaymentID) 
	
		if(@TransportFeeMode=1)
		begin
		
			Insert into @TransHostFeeTable(FeeTypeID,MonthType,NetApplicablePayment,DiscAmt,FeeTypeApplicable)
			select FeeTypeID,MonthType,ApplicableFee,Discount,FeeTypeApplicable
				from v_ClassGroupQuotaFeeTypeSessionFee where QuotaID=@QuotaID and FeeTypeApplicable=5 and ClassID=@ClassID and GroupID=@GroupID and SessionID=@SessionID  and feePaymentMode=@FeePaymentMode
			
			select @TransportFee=sum(isnull(FeeAmount,0)) from ClassFeeStructureMaster
			where ClassID=@ClassID and GroupID=@GroupID and SBranchID=@SBranchID 
			and FeeTypeID in (select FeeTypeID from FeeTypeMaster where FeeTypeApplicable=5) and SessionID=@SessionID

		end
		if(@HostelFeeMode=1)
		begin
			Insert into @TransHostFeeTable(FeeTypeID,MonthType,NetApplicablePayment,DiscAmt,FeeTypeApplicable)
			select FeeTypeID,MonthType,ApplicableFee,Discount,FeeTypeApplicable
				from v_ClassGroupQuotaFeeTypeSessionFee where QuotaID=@QuotaID and FeeTypeApplicable=6 and ClassID=@ClassID and GroupID=@GroupID and SessionID=@SessionID  and feePaymentMode=@FeePaymentMode
			
			select @HostelFee= sum(isnull(FeeAmount,0)) from ClassFeeStructureMaster 
			where ClassID=@ClassID and GroupID=@GroupID and SBranchID=@SBranchID 
			and FeeTypeID in (select FeeTypeID from FeeTypeMaster where FeeTypeApplicable=6) and SessionID=@SessionID
		end
		
		
	declare @IsLateFeeApplicable int
	Declare @LMonth int
	Declare @LYear int
	Declare @MonthType int
	While((datepart(month,@StartDate)+datepart(year,@StartDate)*12)<(datepart(month,@CurDate)+datepart(year,@CurDate)*12))
	begin
		set @IsLateFeeApplicable=0
		declare @MonthPaymentDate date
		set @LMonth=datepart(month,@StartDate)
		set @LYear=datepart(Year,@StartDate)

		select @MonthType=(Case when datepart(month,@BaseDate)=@LMonth and (datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or 
		datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth) then 5 else
		(Case when datepart(month,@BaseDate)=@LMonth and (datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth) then 6 else 
		(case when datepart(month,@BaseDate)=@LMonth then (case when @IsAdmissionFee=0 then 1 else 0 end)  
		else (Case when datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth then 3
		else (case when  datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth then 4 else 2 end) end) end)end)end)

		select @IsLateFeeApplicable=count(*) from PaymentDetails where PayeeID=@StudentID and Month=@Month and Year=@Year and FeeTypeID=-2
		--set @IsLateFeeApplicable=1
		select @MonthPaymentDate = min(PaymentDate) from PaymentDetails where PayeeID=@StudentID and Month=@Month and Year=@Year
		--if(@MonthPaymentDate is null and cast(cast(@Year as nvarchar(5))+'-'+cast(@Month as nvarchar(5))+'-'+cast(@LastPayDay as nvarchar(5)) as date)<@QDate)
		

		select @MonthPaymentDate = min(PaymentDate) from PaymentDetails where PayeeID=@StudentID and Month=@LMonth and Year=@LYear
		
		if(@MonthPaymentDate is null and cast(cast(@LYear as nvarchar(5))+'-'+cast(@LMonth as nvarchar(5))+'-'+cast(@LastPayDay as nvarchar(5)) as [date])<@QDate)
		begin			
			set @IsLateFeeApplicable=1
		end
		else if(@MonthPaymentDate is not null)
		begin
			select @IsLateFeeApplicable=count(*) from PaymentDetails where PayeeID=@StudentID and Month=@LMonth and Year=@LYear 
			and (FeeTypeID=-2)
		end
		if(@MonthPaymentDate is null and @LYear*12+@LMonth>datepart(year,@QDate)*12+Datepart(month,@QDate))
		begin
			set @MonthPaymentDate=@QDate--dateadd(month,-1,@QDate)
		end
		insert into @PreviousMonthsTable
		Select CFG.FeeTypeID ,CFG.FeeTypeName,isnull(PD.NetApplicablePayment,
		Case when CFG.FeeTypeApplicable=5 then (Case when @TransportFeeMode=1 then THFT.NetApplicablePayment else 
		[dbo].[fn_GetStudentTransportHostalFeeAmount](@LMonth,@LYear,@StudentID,0,0,@HostelFeeMode,@TransportFeeMode,@ClassID,@FeePaymentMode,@GroupID,@SBranchID,THFT.NetApplicablePayment,@HostelFee,@SessionID) end)

		 else (Case when CFG.FeeTypeApplicable=6 then  (Case when @HostelFeeMode=1 then THFT.NetApplicablePayment else 
		 [dbo].[fn_GetStudentTransportHostalFeeAmount](@LMonth,@LYear,@StudentID,0,1,@HostelFeeMode,@TransportFeeMode,@ClassID,@FeePaymentMode,@GroupID,@SBranchID,@TransportFee,THFT.NetApplicablePayment,@SessionID) end)
		  else  isnull(SFD.FeeAmount,CFG.ApplicableFee) end) end 
		) as NetApplicablePayment,isnull(PD.DiscAmt,0) as DiscAmt
		,(PD.PaymentRecieved) as PaymentRecieved,@LYear as Year,@LMonth as Month,isnull(PD.PaymentDate,isnull(@MonthPaymentDate,@CurDate)) as PaymentDate
	
		from v_ClassGroupQuotaFeeTypeSessionFee
		 CFG left outer join StudentFeeDetails SFD  on SFD.FeeTypeID=CFG.FeeTypeID and SFD.IsApplicable=1 and SFD.StudentID=@StudentID and SFD.FeeTypeID in (
		Select FeeTypeID from FeeTypeMaster FTM where FeeTypeApplicable in (Select FeeTypeID from [MonthTypeFeeType] MFT where MFT.MonthTypeID=@MonthType))
		left outer join v_PaymentDetails PD on CFG.FeeTypeID=PD.FeeTypeID and (PD.PayeeID=@StudentID) and PD.Month=@LMonth and PD.Year=@LYear
		left outer join @TransHostFeeTable THFT on THFT.FeeTypeID=CFG.FeeTypeID and THFT.MonthType=CFG.MonthType
		where isnull(CFG.QuotaID,@QuotaID)=@QuotaID
		and CFG.FeeTypeApplicable not in (Case when isnull(@IsTransport,0)=0 then 5 else 0 end) 
		and CFG.FeeTypeApplicable not in (Case when isnull(@IsHostel,0)=0 then 6 else 0 end) 		 
		 and CFG.FeeTypeID not in (case when @IsLateFeeApplicable<>0 then 0 else -2 end) 
		 and CFG.ClassID=@ClassID and CFG.GroupID=@GroupID and CFG.FeePaymentMode=@FeePaymentMode 
		and CFG.SBranchID=@SBranchID and CFG.SessionID=@SessionID 
		and CFG.MonthType=@MonthType
		--and isnull(PD.NetApplicablePayment,CFG.ApplicableFee)-isnull(PD.DiscAmt,0)-isnull(PD.PaymentRecieved,0)>0
		
		Set @StartDate= DATEADD(month,1,@StartDate)
	end
	if(@Year*12+@Month>=Datepart(year,@StudentSessionDate)*12+Datepart(month,@StudentSessionDate))
	begin
	
		select @IsLateFeeApplicable=count(*) from PaymentDetails where PayeeID=@StudentID and Month=@Month and Year=@Year and FeeTypeID=-2
		--set @IsLateFeeApplicable=1
		select @MonthPaymentDate = min(PaymentDate) from PaymentDetails where PayeeID=@StudentID and Month=@Month and Year=@Year
		if(@MonthPaymentDate is null and cast(cast(@Year as nvarchar(5))+'-'+cast(@Month as nvarchar(5))+'-'+cast(@LastPayDay as nvarchar(5)) as date)<@QDate)
		begin			
			set @IsLateFeeApplicable=1
		end
		else if(@MonthPaymentDate is not null)
		begin
			select @IsLateFeeApplicable=count(*) from PaymentDetails where PayeeID=@StudentID and Month=@Month and Year=@Year 
			and (FeeTypeID=-2 or cast(cast(@Year as nvarchar(5))+'-'+cast(@Month as nvarchar(5))+'-'+cast(@LastPayDay as nvarchar(5)) as date)<@MonthPaymentDate)
		end
		if(@MonthPaymentDate is null and @Year*12+@Month>datepart(year,@QDate)*12+Datepart(month,@QDate))
		begin
			set @MonthPaymentDate=@QDate--dateadd(month,-1,@QDate)
		end
		if(@MonthPaymentDate is null)
		begin
			set @MonthPaymentDate=@QDate
		end
		
		select @MonthType=(Case when datepart(month,@BaseDate)=@LMonth and (datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or 
		datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth) then 5 else
		(Case when datepart(month,@BaseDate)=@LMonth and (datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth) then 6 else 
		(case when datepart(month,@BaseDate)=@LMonth then (case when @IsAdmissionFee=0 then 1 else 0 end)  
		else (Case when datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth then 3
		else (case when  datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth then 4 else 2 end) end) end)end)end)

		insert into @PreviousMonthsTable
		Select CFG.FeeTypeID ,CFG.FeeTypeName,isnull(PD.NetApplicablePayment,	
			Case when CFG.FeeTypeApplicable=5 then (Case when @TransportFeeMode=1 then THFT.NetApplicablePayment else 
		[dbo].[fn_GetStudentTransportHostalFeeAmount](@Month,@Year,@StudentID,0,0,@HostelFeeMode,@TransportFeeMode,@ClassID,@FeePaymentMode,@GroupID,@SBranchID,THFT.NetApplicablePayment,@HostelFee,@SessionID) end)

		 else (Case when CFG.FeeTypeApplicable=6 then  (Case when @HostelFeeMode=1 then THFT.NetApplicablePayment else 
		 [dbo].[fn_GetStudentTransportHostalFeeAmount](@Month,@Year,@StudentID,0,1,@HostelFeeMode,@TransportFeeMode,@ClassID,@FeePaymentMode,@GroupID,@SBranchID,@TransportFee,THFT.NetApplicablePayment,@SessionID) end)
		  else isnull(SFD.FeeAmount,CFG.ApplicableFee) end) end 
			) as NetApplicablePayment,isnull(PD.DiscAmt,0) as DiscAmt
		,(PD.PaymentRecieved) as PaymentRecieved,@Year as Year,@Month as Month,isnull(PD.PaymentDate,isnull(@MonthPaymentDate,@CurDate)) as PaymentDate
	
		from v_ClassGroupQuotaFeeTypeSessionFee
		 CFG left outer join StudentFeeDetails SFD  on SFD.FeeTypeID=CFG.FeeTypeID and SFD.IsApplicable=1 and SFD.StudentID=@StudentID and SFD.FeeTypeID in (
		Select FeeTypeID from FeeTypeMaster FTM where FeeTypeApplicable in (Select FeeTypeID from [MonthTypeFeeType] MFT where MFT.MonthTypeID=@MonthType))
		left outer join v_PaymentDetails PD 
		on CFG.FeeTypeID=PD.FeeTypeID and (PD.PayeeID=@StudentID) and PD.Month=@Month and PD.Year=@Year	
		left outer join @TransHostFeeTable THFT on THFT.FeeTypeID=CFG.FeeTypeID and THFT.MonthType=CFG.MonthType
		where  isnull(CFG.QuotaID,@QuotaID)=@QuotaID
		and CFG.FeeTypeApplicable not in (Case when isnull(@IsTransport,0)=0 then 5 else 0 end) 
		and CFG.FeeTypeApplicable not in (Case when isnull(@IsHostel,0)=0 then 6 else 0 end) 		 
		 and CFG.FeeTypeID not in (case when @IsLateFeeApplicable<>0 then 0 else -2 end) 
		 and CFG.ClassID=@ClassID and CFG.GroupID=@GroupID and CFG.FeePaymentMode=@FeePaymentMode 
		and CFG.SBranchID=@SBranchID and CFG.SessionID=@SessionID 
		and CFG.FeeTypeApplicable not in (Case when isnull(@IsTransport,0)=0 then 5 else 0 end) 
		and CFG.FeeTypeApplicable not in (Case when isnull(@IsHostel,0)=0 then 6 else 0 end) 
		and CFG.MonthType=@MonthType
	end
	--was deleting some records from temp table but right now cant remember why so commenting 10-10-2017
	--delete from @PreviousMonthsTable where PaymentRecieved is null and PaymentDate<>@QDate and PaymentDate<>@CurDate

	Select p.FeeTypeID,	 FeeTypeName,	 NetApplicablePayment,	 isnull(nullif(DiscAmt,0),ApprovedAmount) as DiscAmt,ApprovedAmount,	 PaymentRecieved,	 Year,	 Month,	 PaymentDate
	from(Select FeeTypeID,	 FeeTypeName,	 NetApplicablePayment,	 DiscAmt,	 PaymentRecieved,	 Year,	 Month,	 PaymentDate
	from @PreviousMonthsTable where Month in (select Month from @PreviousMonthsTable where (NetApplicablePayment-DiscAmt-isnull(PaymentRecieved,0))>0) or Month=@Month)p
	left outer join @Discounts d on d.FeeTypeID=p.FeeTypeID and d.FeeMonth=p.Month and d.FeeYear=p.Year
	
	select Distinct FeeTypeID as ID, FeeTypeName as Name from @PreviousMonthsTable where Month in 
	(select Month from @PreviousMonthsTable where (isnull(NetApplicablePayment,0)-isnull(DiscAmt,0)-isnull(PaymentRecieved,0))>0) or Month=@Month
	
	
	Select Month ,Year ,Year*12+Month,sum(NetApplicablePayment) as ApplicableFee,sum(DiscAmt)as Waiver,Sum(PaymentRecieved) as PaymentRecieved from @PreviousMonthsTable 
	where Month in (select Month from @PreviousMonthsTable where (isnull(NetApplicablePayment,0)-isnull(DiscAmt,0)-isnull(PaymentRecieved,0))>0) or Month=@Month
	Group by Month,Year,Year*12+Month
	order by Year*12+Month

	Select @StudentSessionDate
	Select @StudentSessionEndDate
	select @FeePaymentMode
	Select @SessionStartDate

	select @MonthPaymentDate

	select * from @TransHostFeeTable
end
GO
ALTER  proc [dbo].[sp_GetFeeStructureForClassGroup]
(
@ClassID int,
@GroupID int,
@SBranchID int,
@SessionID int=0
)
AS
BEGIN
	if(@ClassID=0)
	begin
		select @ClassID=min(ClassID) from ClassMaster where Status=1 and SBranchID=@SBranchID
		select @GroupID =min(GroupID) from Class_Sections where ClassID=@ClassID and Status=1
		select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and SessionStatus=1
	end

	Select CFS.ID,CFS.ClassID,CFS.GroupID,FTM.FeeTypeApplicable,FAM.FeeApplicableName,FTM.FeeTypeID,FTM.FeeTypeName,isnull(CFS.FeeAmount,0) as FeeAmount ,Isnull(CFS.Status,1) as Status,FTM.Months
	from FeeTypeMaster FTM left outer join [dbo].[FeeApplicableMaster] FAM on FAM.[FeeApplicableID]=FTM.FeeTypeApplicable 
	left OUTER JOIN
	(Select ID,ClassID,GroupID,FeeTypeID,FeeAmount,Status from 
	 ClassFeeStructureMaster CFS where SessionID=@SessionID and isnull(SBranchID,0)=@SBranchID and isnull(ClassID,0)=@ClassID and isnull(GroupID,0)=@GroupID) as CFS 
	 on FTM.FeeTypeID=CFS.FeeTypeID where FTM.FeeTypeID<>-1 and FTM.SBranchID=@SBranchID
	
	select isnull(@ClassID,0) 
	select isnull(@GroupID,0)

	declare @ELID int
	Select @ELID =EducationLevelID from ClassMaster where ClassID=@ClassID 
	
	Select ClassID,ClassName from ClassMaster where Status=1 and SBranchID=@SBranchID
	Select GroupID,GroupName from GroupMaster where EducationLevelID=isnull(@ELID,0)

	Select SessionID,SessionName,SessionStatus from SessionMaster where SBranchID=@SBranchID

	select isnull(@SessionID,0)
END

GO
ALTER  proc [dbo].[sp_GetFeeTypes]
(
@SBranchID int=1
)
AS
BEGIN
select [FeeTypeID]
      ,[FeeTypeName]
      ,[FeeTypeApplicable],
	  (Select FeeApplicableName from FeeApplicableMaster where FeeApplicableID=FeeTypeApplicable) as FeeTypeApplicableName
      ,[ChartColor],[IsBaseType],Months
  FROM [dbo].[FeeTypeMaster] where FeeTypeID<>-1 and SBranchID=@SBranchID

  select FeeApplicableID as ID,FeeApplicableName as Name from FeeApplicableMaster
END
GO
GO
Create Procedure [dbo].[sp_GetGSTStates]
AS
BEGIN
		select * from GSTStateMaster
END
GO
ALTER  procedure [dbo].[sp_GetLeavesByStudent]
(
@StudentID int
)
as
begin

Select LeaveID,LeaveType,StartDate,EndDate,LeaveReason,
'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) as [StudentSID],
SS.RollNo,
(Select ClassName +' \ '+SectionName from [dbo].[v_ClassSectionNames] where SectionID=SS.SectionID) as ClassSection,
ApplicantID as StudentID, SM.Name as StudentName,'/images/StudentImage/'+Cast(SM.StudentID as nvarchar(50))+'_'+isnull(SM.Photo,'') as Photo,SM.Gender,IsApproved
from LeaveMaster LM Left outer join  StudentMaster SM on SM.StudentID=LM.ApplicantID
left outer join Student_Session SS on SS.StudentID=SM.StudentID
where ApplicantType=1 and LM.ApplicantID=@StudentID
order by StartDate desc

end
GO
Create procedure [dbo].[sp_GetLeaveTypes]
(
@SBranchID int
)
AS
BEGIN
	select * from LeaveTypeMaster 
	where SBranchID=@SBranchID
END

GO
CREATE proc [dbo].[sp_GetLibraryBooks]
(
@SBranchID int,
@ClassID int,
@CategoryID int
)
AS
BEGIN
	if(@ClassID=0 and @CategoryID=0)
	begin
		Select BookID,Title,[Description],Copies,[Status],Price,Publisher,Author,BayID,[Image],BookCode,IssueDays,
		CategoryIDs,ClassesIDs,ISBN,ISBN13,[dbo].[GetClassNames](ClassesIDs) as Classes,
		dbo.GetBookCategoryNames(CategoryIDs) as CategoryNames
		from BookMaster where SBranchID=@SBranchID
	end
	else if(@ClassID<>0)
	begin
		if(@CategoryID=0)
		begin
			Select BookID,Title,[Description],Copies,[Status],Price,Publisher,Author,BayID,[Image],BookCode,IssueDays,
			CategoryIDs,ClassesIDs,ISBN,ISBN13,[dbo].[GetClassNames](ClassesIDs) as Classes,
			dbo.GetBookCategoryNames(CategoryIDs) as CategoryNames
			from BookMaster where SBranchID=@SBranchID and ClassesIDs like '%,'+cast(@ClassID as nvarchar(3))+',%'
		end
		else if(@CategoryID<>0)
		begin
			Select BookID,Title,[Description],Copies,[Status],Price,Publisher,Author,BayID,[Image],BookCode,IssueDays,
			CategoryIDs,ClassesIDs,ISBN,ISBN13,[dbo].[GetClassNames](ClassesIDs) as Classes,
			dbo.GetBookCategoryNames(CategoryIDs) as CategoryNames
			from BookMaster where SBranchID=@SBranchID and ClassesIDs like '%,'+cast(@ClassID as nvarchar(3))+',%'
			and CategoryIDs like '%,'+cast(@CategoryID as nvarchar(3))+',%'
		end
	end
	else if(@CategoryID<>0)
	begin		
		Select BookID,Title,[Description],Copies,[Status],Price,Publisher,Author,BayID,[Image],BookCode,IssueDays,
		CategoryIDs,ClassesIDs,ISBN,ISBN13,[dbo].[GetClassNames](ClassesIDs) as Classes,
		dbo.GetBookCategoryNames(CategoryIDs) as CategoryNames
		from BookMaster where SBranchID=@SBranchID and CategoryIDs like '%,'+cast(@CategoryID as nvarchar(3))+',%'
	end

	Select ID,name from BookCategory where SBranchID=@SBranchID

	Select ClassID as ID, ClassName as Name from ClassMaster where SBranchID=@SBranchID
END
GO
Create proc [dbo].[sp_GetLibraryDetailList]
(
@ID int
)
AS
BEGIN
	Select ID,Name from LibraryMaster where MasterID=@ID
END
GO
CREATE proc [dbo].[sp_GetLibraryDetails]
(
@MasterID int,
@SBranchID int,
@Type int
)
AS
BEGIN
	if(@Type<>2)
	begin
		Select ID,Name,MasterID,[Status],[Type],Detail,
		(Select Count(*) from LibraryMaster LMT where LMT.MasterID=LM.ID) as [Count]
		 from LibraryMaster LM where MasterID=@MasterID and SBranchID=@SBranchID
	end
	else
	begin
		Select ID,Name,MasterID,[Status],[Type],Detail,
		(Select Count(*) from BookMaster LMT where LMT.BayID=LM.ID) as [Count]
		 from LibraryMaster LM where MasterID=@MasterID and SBranchID=@SBranchID
	end
END
GO
CREATE proc [dbo].[sp_GetLibraryIssueRegister]
(
@SBranchID int,
@LibraryID int,
@SessionID int,
@StartDate date,
@EndDate date,
@Status int,
@BorrowerType int
)
AS
BEGIN
if(@SessionID=0)
begin
	select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and SessionStatus=1
end
If(@LibraryID=0)
begin
	Select @LibraryID=Min(ID) from LibraryMaster where SBranchID=@SBranchID and Type=0
end
if(@Status=-1)
begin
	if(@BorrowerType=-1)
	begin
		select IssueID,BorrowerType,
		(case when BorrowerType=0 then (Select EmployeeName from EmployeeMaster where EmployeeID=BorrowerID) else
		(select Name from v_StudentCurrentClassName where StudentID=BorrowerID and SessionID=@SessionID) end) as BorrowerName,
		ClassID,[IssueDate],[IssuedDays],Remark,
		(Select Count(*) from [LibraryIssueBooks]where [LibraryIssueID]=IssueID and Status=1) as ReturnedBooks,
		(Select Count(*) from [LibraryIssueBooks]where [LibraryIssueID]=IssueID) as Books,Status
		 from [LibraryIssueRegister] where [LibraryID]=@LibraryID and IssueDate between @StartDate and @EndDate and SessionID=@SessionID
	 end
	 else
	 begin
		select IssueID,BorrowerType,
		(case when BorrowerType=0 then (Select EmployeeName from EmployeeMaster where EmployeeID=BorrowerID) else
		(select Name from v_StudentCurrentClassName where StudentID=BorrowerID and SessionID=@SessionID) end) as BorrowerName,
		ClassID,[IssueDate],[IssuedDays],Remark,
		(Select Count(*) from [LibraryIssueBooks]where [LibraryIssueID]=IssueID and Status=1) as ReturnedBooks,
		(Select Count(*) from [LibraryIssueBooks]where [LibraryIssueID]=IssueID) as Books,Status
		 from [LibraryIssueRegister] where [LibraryID]=@LibraryID and IssueDate between @StartDate and @EndDate and BorrowerType=@BorrowerType and SessionID=@SessionID
	 end
end
else
begin
	if(@BorrowerType=-1)
	begin
		select IssueID,BorrowerType,
		(case when BorrowerType=0 then (Select EmployeeName from EmployeeMaster where EmployeeID=BorrowerID) else
		(select Name from v_StudentCurrentClassName where StudentID=BorrowerID and SessionID=@SessionID) end) as BorrowerName,
		ClassID,[IssueDate],[IssuedDays],Remark,
		(Select Count(*) from [LibraryIssueBooks]where [LibraryIssueID]=IssueID and Status=1) as ReturnedBooks,
		(Select Count(*) from [LibraryIssueBooks]where [LibraryIssueID]=IssueID) as Books,Status
		 from [LibraryIssueRegister] where [LibraryID]=@LibraryID and IssueDate between @StartDate and @EndDate and Status=@Status and SessionID=@SessionID
	 end
	 else
	 begin
		select IssueID,BorrowerType,
		(case when BorrowerType=0 then (Select EmployeeName from EmployeeMaster where EmployeeID=BorrowerID) else
		(select Name from v_StudentCurrentClassName where StudentID=BorrowerID and SessionID=@SessionID) end) as BorrowerName,
		ClassID,[IssueDate],[IssuedDays],Remark,
		(Select Count(*) from [LibraryIssueBooks]where [LibraryIssueID]=IssueID and Status=1) as ReturnedBooks,
		(Select Count(*) from [LibraryIssueBooks]where [LibraryIssueID]=IssueID) as Books,Status
		 from [LibraryIssueRegister] where [LibraryID]=@LibraryID and IssueDate between @StartDate and @EndDate and Status=@Status and BorrowerType=@BorrowerType and SessionID=@SessionID
	 end
end
Select ID,Name from LibraryMaster where SBranchID=@SBranchID and Type=0

Select SessionID as ID,SessionName as Name, SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID

Select isnull(@LibraryID,0)
select isnull(@SessionID,0)
End
GO
CREATE proc [dbo].[sp_GetLibraryIssueRegisterDetail]
(
@IssueID int,
@SessionID int,
@SBranchID int
)
AS
BEGIN
	if(@SessionID=0)
	begin
		Select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and SessionStatus=1
	end
	select IssueID,BorrowerType,BorrowerID,
		(Select Count(*) from [LibraryIssueBooks]where [LibraryIssueID]=IssueID and Status=1) as ReturnedBooks,
		(Select Count(*) from [LibraryIssueBooks]where [LibraryIssueID]=IssueID) as Books,
	(case when BorrowerType=0 then (Select EmployeeName from EmployeeMaster where EmployeeID=BorrowerID) else
	(select Name from v_StudentCurrentClassName where StudentID=BorrowerID and SessionID=@SessionID) end) as BorrowerName,
	ClassID,[IssueDate],[IssuedDays],Remark
	from [LibraryIssueRegister] where IssueID=@IssueID
	
	Select BookIssueID,LIB.BookID,LIB.CopyID,LIB.IssueDays,LIB.IssueDate,Remark,LIB.[Status],LIB.[Status] as AStatus,
	BCD.BarCode,BCD.RFID,BCD.Code,'(' +isnull(BCD.Code,'')+') '+ BM.Title as Title,BM.[Image]
	from [LibraryIssueBooks] LIB Left outer join [dbo].[BookCopyDetail] BCD on BCD.CopyID=LIB.CopyID
	left outer join [dbo].[BookMaster] BM on LIB.BookID=BM.BookID
	where [LibraryIssueID]=@IssueID

	Select ClassID as ID, ClassName as Name from ClassMaster where SBranchID=@SBranchID and Status=1

	Select EmployeeID as ID,EmployeeName as Name from EmployeeMaster where DesignationID=1 and SBranchID=@SBranchID
End
GO
Create proc [dbo].[sp_GetLibrarySectionStudentList]
(
@SectionID int,
@SessionID int
)
AS
BEGIN
	Select StudentID as ID,'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) +' '+ Name
	 as Name
	from StudentMaster SM where SM.StudentID in (Select StudentID from Student_Session SS where SS.SectionID=@SectionID and SS.SessionID=@SessionID and Status=1)

END
GO
ALTER  Procedure [dbo].[sp_GetLowStockProducts]
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
CREATE Procedure [dbo].[sp_GetParentStudentSessionFeeStatus] 
(
@StudentID int,
@CurDate date
)
AS
BEGIN
	declare @QDate date,@SessionID int,@SBranchID int,@StudentSessionUID int

	select Top 1 @SessionID=SessionID,@SBranchID=SBranchID,@StudentSessionUID=StudentSessionUID from Student_Session where StudentID=@StudentID order by StudentSessionUID desc

	Select @QDate=SessionEndDate from SessionMaster where SessionID=@SessionID

	Declare @FeeDetailTable table (FeeMonth int,FeeYear int, FeeTypeApplicable int, FeeTypeID int,FeeTypeName nvarchar(100),FeeAmount numeric(10,2),QDiscount numeric(10,2),
	RDiscount numeric(10,2),PayApplicableAmount numeric(10,2),CustomFee numeric(10,2),PaidAmount numeric(10,2),IsCustomFee int,IsPayment int)

	insert into @FeeDetailTable	exec sp_GetStudentFeeDetailsNew @StudentID,@SBranchID,@SessionID,@QDate,@CurDate

	delete from @FeeDetailTable where (Case when IsPayment=1 then PayApplicableAmount when IsCustomFee=1 then CustomFee else (isnull(FeeAmount,0)-isnull(FeeAmount,0)*isnull(QDiscount,0)/100) end)=0

	select FeeMonth,FeeYear,sum(isnull(RDiscount,0)) as Discount,
	sum((Case when IsPayment=1 then PayApplicableAmount when IsCustomFee=1 then CustomFee else (isnull(FeeAmount,0)-isnull(FeeAmount,0)*isnull(QDiscount,0)/100) end)) as ApplicableFee,
	sum(PaidAmount) as PaymentRecieved
	from @FeeDetailTable
	group by FeeMonth,FeeYear

	select *,(Case when IsPayment=1 then PayApplicableAmount when IsCustomFee=1 then CustomFee else (isnull(FeeAmount,0)-isnull(FeeAmount,0)*isnull(QDiscount,0)/100) end) as ApplicableFee 
	from @FeeDetailTable
	order by FeeYear,FeeMonth,FeeTypeName

END
GO
ALTER  Procedure [dbo].[sp_GetProductDetails]
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
GO
CREATE Procedure [dbo].[sp_GetRefundDetails] 
(
@RefundID int,
@SBranchID int
)
AS
BEGIN
	Declare @RefundDetails Table(RefundID int,RefundTo int,RefundDate datetime,CreatedDate datetime,RefundBy int,Reason nvarchar(max),SessionID int,
	SBranchID int,RefundAmount numeric(10,2),Status int,ClassID int,SectionID int,StudentSessionUID int)
	insert into @RefundDetails
	select RM.*,t.ClassID,t.SectionID,t.StudentSessionUID
	from RefundMaster RM left outer join StudentMaster SM on SM.StudentID=RM.RefundTo
	left outer join (SELECT *, ROW_NUMBER() OVER (PARTITION BY StudentID,SessionID ORDER BY Status DESC) AS rn
   FROM Student_Session)t on t.StudentID=SM.StudentID and t.rn=1 and t.SessionID=RM.SessionID
   where RM.RefundID=@RefundID

   declare @ClassID int,@SectionID int,@SessionID int,@StudentID int,@StudentSessionUID int

   select @ClassID=ClassID,@SectionID=SectionID,@StudentSessionUID=StudentSessionUID,@SessionID=SessionID ,@StudentID=RefundTo
   from @RefundDetails

   if(isnull(@ClassID,0)=0)
   begin
		Select top 1 @ClassID = ClassID from ClassMaster where SBranchID=@SBranchID and Status=1 order by SequenceNo 
		select top 1 @SectionID= ID from Class_Sections where ClassID=@ClassID and Status=1 order by ID 
		select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
		select top 1 @StudentID=StudentID,@StudentSessionUID=StudentSessionUID from Student_Session 
		where ClassID=@ClassID and SectionID=@SectionID and SessionID=@SessionID and Status=1 order by Status desc
   end
   
   Select SessionID as ID,SessionName as Name,SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID
   Select ClassID as ID,ClassName as Name from ClassMaster where SBranchID=@SBranchID and Status=1
   select ID,Name from Class_Sections where Status=1 and ClassID=@ClassID

   Select SM.StudentID as ID,SM.Name +' ('+isnull(SM.SchoolUID,'NA')+')' as Name,t.StudentSessionUID as Extra1
   from StudentMaster SM left outer join 
   (SELECT *, ROW_NUMBER() OVER (PARTITION BY StudentID,SessionID ORDER BY Status DESC) AS rn
   FROM Student_Session)t on SM.StudentID=t.StudentID and t.SessionID=@SessionID
   where t.SessionID=@SessionID and t.ClassID=@ClassID and t.SectionID=@SectionID and t.Status=1

   select * from @RefundDetails

   select isnull(@SessionID,0)
   select isnull(@ClassID,0)
   select isnull(@SectionID,0)
   select isnull(@StudentID,0)
   select isnull(@StudentSessionUID,0)
END
GO
Create  Procedure [dbo].[sp_GetRefundPrintData]
(
@RefundID int,
@SBranchID int
)
AS
BEGIN
	select RM.*,SM.Name as RefundName,CM.ClassName,CS.Name as SectionName,SS.SessionName, SS.SessionStartDate,SS.SessionEndDate,
	'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6) as StudentSID,SM.SchoolUID,PM.FatherName
	from RefundMaster RM left outer join StudentMaster SM on SM.StudentID=RM.RefundTo
	left outer join (SELECT *, ROW_NUMBER() OVER (PARTITION BY StudentID,SessionID ORDER BY Status DESC) AS rn
	FROM Student_Session)t on t.StudentID=SM.StudentID and t.rn=1 and t.SessionID=RM.SessionID
	left outer join ClassMaster CM on CM.ClassID=t.ClassID
	left outer join Class_Sections CS on CS.ID=t.SectionID
	left outer join SessionMaster SS on SS.SessionID=t.SessionID
	left outer join ParentMaster PM on PM.ParentID=SM.ParentID
	where RM.RefundID=@RefundID and RM.SBranchID=@SBranchID
   
	Select * from SBranchMaster where SBranchID=@SBranchID

END
GO
Create Procedure [dbo].[sp_GetRefunds]
(
@FromDate date,
@ToDate date,
@Status int,
@SBranchID int
)
AS
BEGIN
	select RM.*,SM.Name as RefundName,CM.ClassName,CS.Name as SectionName,
	'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6) as StudentSID,SM.SchoolUID
	from RefundMaster RM left outer join StudentMaster SM on SM.StudentID=RM.RefundTo
	left outer join (SELECT *, ROW_NUMBER() OVER (PARTITION BY StudentID,SessionID ORDER BY Status DESC) AS rn
   FROM Student_Session)t on t.StudentID=SM.StudentID and t.rn=1 and t.SessionID=RM.SessionID
   left outer join ClassMaster CM on CM.ClassID=t.ClassID
   left outer join Class_Sections CS on CS.ID=t.SectionID
   where cast(RM.RefundDate as date) between @FromDate and @ToDate and RM.SBranchID=@SBranchID 
   and ISNULL(nullif(@Status,-1),RM.Status)=RM.Status
END
GO
ALTER  proc [dbo].[sp_GetSalaryTypes]
(
@SBranchID int
)
AS
BEGIN
select [TypeID]
      ,[TypeName]
      ,[TypeApplicable],
	  (Select Name from SalaryApplicableMaster where ID=TypeApplicable) as TypeApplicableName
      ,[ChartColor],[IsBaseType],Months,IsDeduction,AttendanceType,MinDays
  FROM [dbo].[SalaryTypeMaster] where SBranchID=@SBranchID

  select * from SalaryApplicableMaster
END
GO
GO
CREATE Procedure [dbo].[sp_GetSMSDefaultSettingDetails]
(
@SBranchID int
)
AS
BEGIN
	declare @SMSConfigID int

	select top 1 @SMSConfigID=SMSConfigID from SMSConfigurationMaster where SBranchID=@SBranchID and Status=1 order by IsDefault desc

	select * from SMSConfigurationMaster where SMSConfigID=@SMSConfigID and SBranchID=@SBranchID
	select * from SMSConfigurationParams where SMSConfigID=@SMSConfigID
END
GO
CREATE Procedure [dbo].[sp_GetSMSSettingDetails]
(
@SMSConfigID int,
@SBranchID int
)
AS
BEGIN
	select * from SMSConfigurationMaster where SMSConfigID=@SMSConfigID and SBranchID=@SBranchID
	select * from SMSConfigurationParams where SMSConfigID=@SMSConfigID
END
GO
Create Procedure [dbo].[sp_GetSMSSettings]
(
@SBranchID int
)
AS
BEGIN
	select * from SMSConfigurationMaster where SBranchID=@SBranchID
END
GO
ALTER  Procedure [dbo].[sp_GetStockTransactionDetails]
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
ALTER  Procedure [dbo].[sp_GetStockTransactions]
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
ALTER proc [dbo].[sp_GetStudentAdmitCard]
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
ALTER  proc [dbo].[sp_GetStudentAttandanceDetails]
(
@Month nvarchar(2),
@Year nvarchar(5),
@Day nvarchar(3),
@ClassID int,
@SectionID int,
@SBranchID int,
@TeacherID int
)
AS
BEGIN

Declare @AttDate date=@Year+'-'+@Month+'-'+@Day
Declare @HolidayCnt int
select @HolidayCnt=count(*) from HolidayMaster where @AttDate between StartDate and EndDate and ((select count(*) from dbo.SplitStringToTable(Classes,',') where ITem=@ClassID or Item=0)>0) and IsStudents=1
declare @TempStatus nvarchar(2)='1'

declare @SessionID nvarchar(3)
select @SessionID=SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
declare @ElID int
if(@SectionID=0 and @TeacherID<>-1)
begin
Select @ClassID=min(ClassID) from ClassMaster CM where [Status]=1 and ClassID in (Select ClassID from Class_Sections where TeacherID=@TeacherID )
Select @SectionID=min( ID) from Class_Sections where TeacherID=@TeacherID and ClassID=@ClassID and Status=1

end
else if(@TeacherID=-1 and @SectionID=0)
begin
Select @ClassID=min( ClassID) from ClassMaster CM where [Status]=1 and SBranchID=@SBranchID
Select @SectionID=min( ID) from Class_Sections where ClassID=@ClassID and Status=1
end
select @ElID=EducationLevelID from ClassMaster where ClassID=@ClassID
declare @NumDays int
select @NumDays=[Days] from EducationLevelMaster where ID = @ElID
declare @curday int = DATEPART(dw,@AttDate)
if(@curday=1)
begin
set @curday=8
end
if(@NumDays < @curday-1)
begin
set @TempStatus='2'
end
if(@HolidayCnt>0)
begin
set @TempStatus='2'
end
declare @SQLQuery nvarchar(max)
set @SQLQuery='Select SM.[StudentID],SM.[Name],SM.Photo,SS.RollNo,SM.Gender,
''STUD''+RIGHT(REPLICATE(''0'',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) as [StudentSID],isnull( LM.LeaveID,0) as LeaveID,
isnull(nullif(EAM.D'+@Day+',3),'+@TempStatus+') as Status ,(case when EAM.D'+@Day+' is null then 0 else 1 end) as IsExist
from StudentMaster SM left outer join [dbo].[StudentAttendanceMaster] EAM on EAM.StudentID=SM.StudentID
and EAM.[FYear]='+@Year+' and EAM.[Month]='+@Month+ ' left outer join Student_Session SS on SS.StudentID=SM.StudentID
left outer join [dbo].[LeaveMaster] LM on LM.[ApplicantID]=SM.StudentID and cast('''+@Month+'/'+@Day+'/'+@Year+''' as date) between LM.[StartDate] and LM.[EndDate] and LM.ApplicantType=1 and LM.IsApproved=1
where SM.SBranchID='+cast(@SBranchID as nvarchar(3))+' and SS.ClassID='+cast(@ClassID as nvarchar(3))+' and SS.SectionID='+cast(@SectionID as nvarchar(3))+' and SS.SessionID='+cast(@SessionID as nvarchar(3))+' and SS.Status=1
and cast('''+@Month+'-'+@Day+'-'+@Year+''' as date)>SS.FromDate
order by SM.[Name]'
exec(@SQLQuery)

print (@SQLQuery)
if(@TeacherID=-1)
begin
Select ClassID as ID,ClassName as Name from ClassMaster CM where [Status]=1 and SBranchID=@SBranchID
Select ID,Name
from Class_Sections where ClassID=@ClassID and Status=1
end
else
begin
Select ClassID as ID,ClassName as Name from ClassMaster CM where [Status]=1
and ClassID in (Select ClassID from Class_Sections where TeacherID=@TeacherID )

Select ID,Name
from Class_Sections where TeacherID=@TeacherID and ClassID=@ClassID and Status=1
end



select @ClassID
select @SectionID

END
GO
GO
CREATE procedure [dbo].[sp_GetStudentFeeDetailsNew]--250,1,1,'2020-02-01','2020-02-12'
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
CREATE Procedure [dbo].[sp_GetStudentFeeDetailViewData]
(
@StudentID int,
@SBranchID int,
@SessionID int,
@QDate date,
@CurDate date
)
as
BEGIN

	Declare @SessionStartDate date,@SessionEndDate Date,@FeePaymentMode int, @StuSessionSDate date,@StuSessionEDate date
		
	select @SessionStartDate=SessionStartDate,@SessionEndDate=SessionEndDate from SessionMaster where SessionID=@SessionID
	
	select top 1 @FeePaymentMode=FeePaymentMode,@StuSessionSDate=(Case when @SessionStartDate<SS.FromDate then SS.FromDate else @SessionStartDate end),
	@StuSessionEDate=(Case when @SessionEndDate>SS.ToDate then SS.ToDate else @SessionEndDate end)
	from Student_Session SS 
	where SS.StudentID=@StudentID and SessionID=@SessionID and @QDate between FromDate and ToDate

	Declare @FeeDetailTable table (FeeMonth int,FeeYear int, FeeTypeApplicable int, FeeTypeID int,FeeTypeName nvarchar(100),FeeAmount numeric(10,2),QDiscount numeric(10,2),
	RDiscount numeric(10,2),
	PayApplicableAmount numeric(10,2),CustomFee numeric(10,2),PaidAmount numeric(10,2),IsCustomFee int,IsPayment int)

	insert into @FeeDetailTable	exec sp_GetStudentFeeDetailsNew @StudentID,@SBranchID,@SessionID,@QDate,@CurDate

	
	select FeeTypeID,FeeTypeName,
	sum((Case when IsPayment=1 then PayApplicableAmount when IsCustomFee=1 then CustomFee else (isnull(FeeAmount,0)-isnull(FeeAmount,0)*isnull(QDiscount,0)/100) end)-isnull(RDiscount,0)) as ApplicableFee,
	sum(PaidAmount) as PaymentRecieved
	from @FeeDetailTable
	group by FeeTypeID,FeeTypeName
	order by FeeTypeID

	select FeeMonth,FeeYear,
	sum((Case when IsPayment=1 then PayApplicableAmount when IsCustomFee=1 then CustomFee else (isnull(FeeAmount,0)-isnull(FeeAmount,0)*isnull(QDiscount,0)/100) end)-isnull(RDiscount,0)) as ApplicableFee,
	sum(PaidAmount) as PaymentRecieved
	from @FeeDetailTable
	group by FeeMonth,FeeYear

	select *,(Case when IsPayment=1 then PayApplicableAmount when IsCustomFee=1 then CustomFee else (isnull(FeeAmount,0)-isnull(FeeAmount,0)*isnull(QDiscount,0)/100) end) as ApplicableFee 
	from @FeeDetailTable
	order by FeeTypeID,FeeYear,FeeMonth

	select @StuSessionSDate
	select @StuSessionEDate

	select @FeePaymentMode
END
GO
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
CREATE proc [dbo].[sp_GetSubSubjectEditPageModel]
(
@SubjectID int
)
AS
BEGIN
	declare @GroupID int
	select @GroupID=GroupID from SubjectMasterT where SubjectID=@SubjectID
	select * from [dbo].[SubSubjectTypes] where GroupID=@GroupID

	select SubjectID,SubjectName,IsOptionalSubject,Status,SubjectCode,MainSubID,SubjectType,SST.TypeName as SubjectTypeName from SubjectMasterT SM
	left outer join SubSubjectTypes SST on SST.TypeID=SM.SubjectType
	where MainSubID=@SubjectID

	select isnull(@GroupID,0)
END
GO
ALTER  proc [dbo].[sp_GetSubSubjects]
(
@SubjectID int
)
AS
BEGIN
	select SubjectID,SubjectName,IsOptionalSubject,Status,SubjectCode,MainSubID,SubjectType,SST.TypeName as SubjectTypeName from SubjectMasterT SM
	left outer join SubSubjectTypes SST on SST.TypeID=SM.SubjectType
	where MainSubID=@SubjectID
END

GO
Create Procedure [dbo].[sp_GetSubSubjectTypes]
  (
  @GroupID int
  )
  AS
  BEGIN
	select * from [dbo].[SubSubjectTypes] where GroupID=@GroupID
  END
GO
ALTER proc [dbo].[sp_GetTAppStudentAttandanceListOnClassSection]
(
@Month nvarchar(2),
@Year nvarchar(5),
@Day nvarchar(3),
@ClassID int,
@SectionID int,
@SBranchID int
)
AS
BEGIN
declare @SessionID nvarchar(3)
select @SessionID=SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
declare @SQLQuery nvarchar(max)
set @SQLQuery='Select SM.[StudentID],SM.[Name],''/images/StudentImage/''+Cast(SM.StudentID as nvarchar(50))+''_''+SM.Photo as Photo,SS.RollNo,SM.Gender,
''STUD''+RIGHT(REPLICATE(''0'',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) as [StudentSID],isnull( LM.LeaveID,0) as LeaveID,SM.SchoolUID,
isnull(EAM.D'+@Day+',1) as Status ,(case when EAM.D'+@Day+' is null then 0 else 1 end) as IsExist
from StudentMaster SM left outer join [dbo].[StudentAttendanceMaster] EAM on EAM.StudentID=SM.StudentID
and EAM.[FYear]='+@Year+' and EAM.[Month]='+@Month+ ' left outer join Student_Session SS on SS.StudentID=SM.StudentID
left outer join [dbo].[LeaveMaster] LM on LM.[ApplicantID]=SM.StudentID and cast('''+@Month+'/'+@Day+'/'+@Year+''' as date) between LM.[StartDate] and LM.[EndDate] and LM.ApplicantType=1 and LM.IsApproved=1
where SM.SBranchID='+cast(@SBranchID as nvarchar(3))+' and SS.ClassID='+cast(@ClassID as nvarchar(3))+' and SS.SectionID='+cast(@SectionID as nvarchar(3))+' and SS.SessionID='+cast(@SessionID as nvarchar(3))+' and SS.Status=1
and cast('''+@Month+'-'+@Day+'-'+@Year+''' as date)>SS.FromDate Order by Status Desc'
exec(@SQLQuery)

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
CREATE PROCEDURE [dbo].[sp_InsertBulkAttandance]
(
@SerialNumber nvarchar(50),
@TransactionStamp nvarchar(50),
@Attendances ut_DeviceLogs READONLY
)
AS
BEGIN
	Declare @BranchID int
	Declare @EmployeeType int
	declare @DownloadDate datetime,@LogDate datetime,@UserID nvarchar(50)
	declare @iUserID int
	Declare @ShiftID int=0

	Update Devices set LastPing =getdate(),TransactionStamp=@TransactionStamp where SerialNumber= @SerialNumber
	declare @DeviceID nvarchar(10)
	Declare @DeviceName nvarchar(50)
	select @DeviceID=DeviceID,@DeviceName=DeviceFName,@BranchID=SBranchID from Devices where SerialNumber=@SerialNumber

	DECLARE db_logCursor CURSOR FOR  
	SELECT DownloadDate,UserID,LogDate  FROM @Attendances
	OPEN db_fcursor   
	FETCH NEXT FROM db_logCursor INTO @DownloadDate,@UserID,@LogDate
	WHILE @@FETCH_STATUS = 0   
	BEGIN   
		if(left(@UserID,1)='S')
		begin
			select top 1 @iUserID=StudentID,@EmployeeType=-1 from StudentMaster SM where 'STU'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6)=@UserId
		end
		else
		begin
			select top 1 @iUserID=EmployeeID,@EmployeeType=isnull(EmployeeType,0),@BranchID=SBranchID from [v_EmployeeDriversBasicDetails] where EmployeeSID=@UserId
		end
		if(@iUserID is not null)
		Begin
			Declare @Direction int=0
			if(isnull(@DeviceID,'0')<>'0')
			begin
				declare @Month nvarchar(2)=datepart(month,@LogDate)
				declare @Year nvarchar(5)=datepart(year,@LogDate)		
					declare @TableName nvarchar(50)='zDeviceLogs_'+cast(@BranchID as nvarchar(10))+'_'+@Year+'_'+@Month
					if(not exists(SELECT *  FROM INFORMATION_SCHEMA.TABLES  WHERE  TABLE_NAME =@TableName))
					begin
						declare @CreateTableQuery nvarchar(max)
						set @CreateTableQuery ='Select * into dbo.'+@TableName+'  from  DeviceLogs'			
						exec (@CreateTableQuery)
					end
					else
					begin
						set @Direction=0
					end
					Declare @Day nvarchar(10)
					declare @DateDiff numeric(10,2)
					Declare @ShiftSTime time
					Declare @ShiftETime time
					declare @SQL nvarchar(max)
					Declare @Cnt int
					set @SQL= N'select @Cnt= count(*) from '+@TableName+' where Datediff(second,LogDate,Cast('''+convert(varchar, @LogDate, 25)+''' as DateTime))<60 and 
					Datediff(second,LogDate,Cast('''+convert(varchar, @LogDate, 25)+''' as DateTime))>-60 and UserID='+cast(@UserID as nvarchar(10))
					EXECUTE sp_executesql @SQL, N'@Cnt INTEGER OUTPUT', @Cnt OUTPUT
					if(@Cnt=0)
					begin
						declare @EMPID int
						declare @StudentID int
						declare @InsertSQL nvarchar(max)
						set @InsertSQL='Insert Into '+@TableName+'(DownloadDate,DeviceId,UserId,LogDate,Direction,C1,C2) values(cast('''+CONVERT(varchar,@DownloadDate,113)+''' as datetime),'+@DeviceID+','''+@UserId+''',cast('''+CONVERT(varchar,@LogDate,113)+''' as datetime),'+cast(@Direction as nchar(1))+','+cast(@ShiftID as nvarchar(5))+','+cast(@EmployeeType as nvarchar(2))+')'					
						exec(@InsertSQL)

						set @Year=datepart(year,@LogDate)
						set @Month=datepart(month,@LogDate) 	
						Set @Day= cast(datepart(day,@LogDate) as nvarchar(3))
	
						if(left(@UserID,3)='STU')
						begin
							select @StudentID=cast(replace(@UserID, 'STU', '') as numeric(10,0))
							exec [sp_UpdateStudentAttandanceAutoNew] @Year,@Month,@Day,@StudentID,1,@BranchID
						end
						else if(left(@UserID,3)='EMP')
						begin
							select @EMPID=cast(replace(@UserID, 'EMP', '') as numeric(10,0))
							exec sp_UpdateEmployeeAttandanceAutoNew @Year,@Month,@Day,@EMPID,1,@BranchID
						end
						else if(left(@UserID,3)='TRA')
						begin
							select @EMPID=cast(replace(@UserID, 'TRA', '') as numeric(10,0))
							exec sp_UpdateEmployeeAttandanceAutoNew @Year,@Month,@Day,@EMPID,1,@BranchID
						end
					end			
			end
		end
		FETCH NEXT FROM db_logCursor INTO @DownloadDate,@UserID,@LogDate
	END   
	CLOSE db_logCursor   
	DEALLOCATE db_logCursor
	select 1
END
GO
ALTER  procedure [dbo].[sp_InsertEmployeeLeave]
(
@ApplicantType int,
@LeaveType int,
@StartDate datetime,
@EndDate datetime,
@LeaveReason nvarchar(MAX),
@ApplicantID int,
@IsApproved int,
@UserID int,
@CreatedDate datetime,
@CLApplied numeric(5,1),
@ELApplied  numeric(5,1),
@PLApplied  numeric(5,1),
@SBranchID int,
@EmployeeType int,
@LWPApplied numeric(5,1)=0
)
as
begin
	Insert into LeaveMaster(ApplicantType,LeaveType,StartDate,EndDate,LeaveReason,ApplicantID,IsApproved,UserID,CreatedDate,CLApplied,ELApplied,PLApplied,SBranchID,EmployeeType,LWPApplied)
	values (@ApplicantType,@LeaveType,@StartDate,@EndDate,@LeaveReason,@ApplicantID,@IsApproved,@UserID,@CreatedDate,@CLApplied,@ELApplied,@PLApplied,@SBranchID,@EmployeeType,@LWPApplied)

	select 1

end

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
GO
Create proc [dbo].[sp_InsertUpdateBookCategory]
(
@ID int,
@Name nvarchar(100),
@SBranchID int,
@OpType int
)
AS
BEGIN

	if(@OpType=-1)
	begin
		Delete from BookCategory where ID=@ID 
	end
	else if(@ID=0)
	begin
		Insert into BookCategory(Name,SBranchID)
		values (@Name,@SBranchID)
	end
	else
	begin
		
		Update BookCategory set Name=@Name
		 where ID=@ID 
	end
END
GO

Create proc [dbo].[sp_InsertUpdateBookCopies]
(
@CopyID int,
@BookID int,
@BarCode nvarchar(50),
@RFID nvarchar(50),
@AStatus int,
@Code nvarchar(50),
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from BookCopyDetail where CopyID=@CopyID
	end
	else if(@CopyID=0)
	begin
		Insert into BookCopyDetail(BookID,BarCode,RFID,AStatus,Code)
		values(@BookID,@BarCode,@RFID,@AStatus,@Code)
	end
	else
	begin
		Update BookCopyDetail set BarCode=@BarCode,RFID=@RFID,AStatus=@AStatus,Code=@Code where CopyID=@CopyID
	end

	Select CopyID,BarCode,RFID,AStatus,Code from BookCopyDetail where BookID=@BookID
END

GO
CREATE proc [dbo].[sp_InsertUpdateBookCopy]
(
@CopyID int,
@BookID int,
@BarCode nvarchar(50),
@RFID nvarchar(50),
@Code nvarchar(20),
@OpType int
)
AS
BEGIN

	if(@OpType=-1)
	begin
		Delete from BookCopyDetail where CopyID=@CopyID 
	end
	else if(@CopyID=0)
	begin
		Insert into BookCopyDetail(BookID,BarCode,RFID,Code)
		values (@BookID,@BarCode,@RFID,@Code)
	end
	else
	begin
		Update BookCopyDetail set BarCode=@BarCode,RFID=@RFID,Code=@Code
		where CopyID=@CopyID
	end

	Select CopyID,BarCode,RFID,AStatus,Code from BookCopyDetail where BookID=@BookID
END

GO
CREATE proc [dbo].[sp_InsertUpdateBookDetails]
(
@BookID int,
@Title nvarchar(500),
@Publisher nvarchar(500),
@BayID int,
@Description nvarchar(MAX),
@Copies int,
@Status bit,
@Price numeric(10, 2),
@Author nvarchar(100),
@Image nvarchar(500),
@CategoryIDs nvarchar(50),
@ClassesIDs nvarchar(50),
@ISBN nvarchar(50),
@ISBN13 nvarchar(50),
@SBranchID int,
@BookCode nvarchar(50),
@IssueDays int,
@OpType int
)
AS
BEGIN

	if(@OpType=-1)
	begin
		Delete from BookMaster where BookID=@BookID 
	end
	else if(@BookID=0)
	begin
		Insert into BookMaster(Title,Publisher,BayID,[Description],Copies,[Status],Price,Author,[Image],CategoryIDs,ClassesIDs,ISBN,ISBN13,SBranchID,BookCode,IssueDays)
		values (@Title,@Publisher,@BayID,@Description,@Copies,@Status,@Price,@Author,@Image,@CategoryIDs,@ClassesIDs,@ISBN,@ISBN13,@SBranchID,@BookCode,@IssueDays)
		SELECT @BookID= CAST(SCOPE_IDENTITY() as int) 		
		exec sp_GetBookDetails @BookID,@SBranchID
	end
	else
	begin
		
		Update BookMaster set Title=@Title,Publisher=@Publisher,BayID=@BayID,[Description]=@Description,Copies=@Copies,
		[Status]=@Status,Price=@Price,Author=@Author,[Image]=@Image,CategoryIDs=@CategoryIDs,
		ClassesIDs=@ClassesIDs,ISBN=@ISBN,ISBN13=@ISBN13,SBranchID=@SBranchID,BookCode=@BookCode,IssueDays=@IssueDays
		 where BookID=@BookID
		exec sp_GetBookDetails @BookID,@SBranchID
	end
END
GO
ALTER  proc [dbo].[sp_InsertUpdateFeeTypes]
(
@FeeTypeID int,
@FeeTypeName nvarchar(50),
@FeeTypeApplicable int,
@ChartColor nvarchar(50),
@UserID int,
@OperationDate datetime,
@OpType int,
@SBranchID int,
@Months nvarchar(50)
)
AS
BEGIN
	if(@OpType=-1)
	begin
		delete from FeeTypeMaster where FeeTypeID=@FeeTypeID
	end
	else if(@FeeTypeID=0)
	begin
		insert into FeeTypeMaster ([FeeTypeName]
		  ,[FeeTypeApplicable]
		  ,[ChartColor],UserID,CreatedDate,SBranchID,Months)
		  values(@FeeTypeName,@FeeTypeApplicable,@ChartColor,@UserID,@OperationDate,@SBranchID,@Months)
		  SELECT @FeeTypeID=CAST(SCOPE_IDENTITY() as int)
		  declare @SessionID int
		  declare @QuotaID int
		  DECLARE db_cursor CURSOR FOR 
		  Select QM.QuotaID,QM.SBranchID,SM.SessionID
			from QuotaMaster QM left outer join SessionMaster SM on SM.SBranchID=QM.SBranchID
		  OPEN db_cursor   
		  FETCH NEXT FROM db_cursor INTO @QuotaID,@SBranchID,@SessionID

		  WHILE @@FETCH_STATUS = 0   
		  BEGIN   
			Insert into QuotaDiscountDetails(QuotaID,FeeTypeID,DiscPer,SBranchID,UserID,SessionID)
			values(@QuotaID,@FeeTypeID,0,@SBranchID,@UserID,@SessionID)

			FETCH NEXT FROM db_cursor INTO @QuotaID,@SBranchID,@SessionID
		  END
		  CLOSE db_cursor   
		DEALLOCATE db_cursor
	end
	else
	begin
		update FeeTypeMaster set [FeeTypeName]=@FeeTypeName,[FeeTypeApplicable]=@FeeTypeApplicable,Months=@Months,
		[ChartColor]=@ChartColor,ModifiedDate=@OperationDate,UserID=@UserID where FeeTypeID=@FeeTypeID
	end
END
GO
CREATE procedure [dbo].[sp_InsertUpdateLeaveType]
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
CREATE proc [dbo].[sp_InsertUpdateLibrary]
(
@ID int,
@Name nvarchar(100),
@Status int,
@Type int,
@MasterID int,
@Detail nvarchar(500),
@SBranchID int,
@OpType int
)
AS
BEGIN

	if(@OpType=-1)
	begin
		select  @Type=[Type] from LibraryMaster where ID=@ID
		Delete from LibraryMaster where ID=@ID 
	end
	else if(@ID=0)
	begin
		Insert into LibraryMaster(Name,[Status],[Type],MasterID,Detail,SBranchID)
		values (@Name,@Status,@Type,@MasterID,@Detail,@SBranchID)
	end
	else
	begin
		
		Update LibraryMaster set MasterID=@MasterID,Name=@Name,Detail=@Detail, [Status]=@Status
		 where ID=@ID 
	end
	set @Type=@Type+1
	exec sp_GetLibraryDetails @MasterID,@SBranchID,@Type
END
GO
Create Procedure [dbo].[sp_InsertUpdateRefund]
(
@RefundID int,
@RefundTo int,
@RefundDate datetime,
@CreatedDate datetime,
@RefundBy int,
@Reason nvarchar(max),
@SessionID int,
@SBranchID int,
@RefundAmount numeric(10,2),
@Status int
)
AS
BEGIN
	if(@RefundID=0)
	begin
		Insert into RefundMaster(RefundTo,RefundDate,CreatedDate,RefundBy,Reason,SessionID,SBranchID,RefundAmount,Status)
		values(@RefundTo,@RefundDate,@CreatedDate,@RefundBy,@Reason,@SessionID,@SBranchID,@RefundAmount,@Status)
		select @RefundID=Cast(Scope_Identity() as int)
	end
	else
	begin
		Update RefundMaster set RefundTo=@RefundTo,RefundDate=@RefundDate,RefundBy=@RefundBy,Reason=@Reason,SessionID=@SessionID
		,SBranchID=@SBranchID,RefundAmount=@RefundAmount,Status=@Status where RefundID=@RefundID
	end
	select @RefundID
END
GO
Alter  proc [dbo].[sp_InsertUpdateSalaryTypes]
(
@TypeID int,
@TypeName nvarchar(50),
@TypeApplicable int,
@ChartColor nvarchar(50),
@UserID int,
@OperationDate datetime,
@SBranchID int,
@Months nvarchar(50),
@IsDeduction int,
@AttendanceType int,
@MinDays int,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		delete from SalaryTypeMaster where TypeID=@TypeID
	end
	else if(@TypeID=0)
	begin
		insert into SalaryTypeMaster ([TypeName]
		  ,[TypeApplicable]
		  ,[ChartColor],UserID,CreatedDate,SBranchID,Months,IsDeduction,AttendanceType,MinDays)
		  values(@TypeName,@TypeApplicable,@ChartColor,@UserID,@OperationDate,@SBranchID,@Months,@IsDeduction,@AttendanceType,@MinDays)
	end
	else
	begin
		update SalaryTypeMaster set [TypeName]=@TypeName,[TypeApplicable]=@TypeApplicable,Months=@Months,IsDeduction=@IsDeduction,AttendanceType=@AttendanceType,MinDays=@MinDays,
		[ChartColor]=@ChartColor,ModifiedDate=@OperationDate,UserID=@UserID where TypeID=@TypeID
	end
END
GO
CREATE  Procedure [dbo].[sp_InsertUpdateSubSubjectTypes]
(
@TypeID int,
@TypeName nvarchar(50),
@SBranchID int,
@GroupID int,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from [dbo].[SubSubjectTypes] where TypeID=@TypeID
	end
	else
	Begin
		if(@TypeID=0)
		begin
			Insert into [dbo].[SubSubjectTypes](TypeName,SBranchID,GroupID)
			values(@TypeName,@SBranchID,@GroupID)
		end
		else
		begin
			Update [dbo].[SubSubjectTypes] set TypeName=@TypeName,SBranchID=@SBranchID,GroupID=@GroupID where TypeID=@TypeID
		end
	End
	exec [dbo].[sp_GetSubSubjectTypes] @GroupID
END
GO
create procedure [dbo].[sp_ProcessPaymentCancellation]
(
@PaymentID int,
@OTP nvarchar(10)
)
AS
BEGIN
	declare @DBOTP nvarchar(10)
	select @DBOTP=OTP from PaymentMaster where PaymentID=@PaymentID
	if(@DBOTP=@OTP)
	begin
		update PaymentMaster set PaymentAmount=0 where PaymentID=@PaymentID
		delete from PaymentDetails where PaymentID=@PaymentID
		select 1
	end
	else
	begin
		select -1
	end
END
GO
CREATE procedure [dbo].[sp_RequestPaymentCancellation]
(
@PaymentID int,
@OTP nvarchar(10)
)
AS
BEGIN
	declare @SBranchID int,@MobileNumber nvarchar(50)
	update PaymentMaster set OTP=@OTP where PaymentID=@PaymentID
	select @SBranchID=SBranchID from PaymentMaster where PaymentID=@PaymentID

	select @MobileNumber=PrincipalMobile from SBranchMaster where SBranchID=@SBranchID
	select *,@MobileNumber as Mobile  from PaymentMaster where PaymentID=@PaymentID
END
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
ALTER proc [dbo].[sp_UpdateEmployeeAttandanceAuto]
(
@Year nvarchar(5),
@Month  nvarchar(2),
@Day nvarchar(3),
@EmployeeID int,
@DeviceSerial nvarchar(50),
@Status int
)
AS
BEGIN
declare @DesignationID int
declare @SectionID int
declare @IsExist int
declare @DaysWorking int
set @DaysWorking=6
declare @SQLQuery nvarchar(max)
declare @SBranchID int
select @SBranchID=SBranchID from [dbo].Devices where SerialNumber=@DeviceSerial

Select  @DesignationID=DesignationID from EmployeeMaster where SBranchID=@SBranchID and EmployeeID=@EmployeeID
select @IsExist=count(*) from EmployeeAttendanceMasterT where FYear=@Year and [Month]=@Month and EmployeeID=@EmployeeID
	
	if(@IsExist=1)
		begin
			set @SQLQuery='Update EmployeeAttendanceMasterT set '+@Day+'='+cast (@Status as nvarchar(5))+' where FYear='+@Year+' and Month='+@Month+' and EmployeeID='+cast (@EmployeeID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))
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
						Select @HCount=count(*) from HolidayMaster where cast(@Month+'-'+cast(@DayNum as nvarchar(2))+'-'+@Year as Date) between StartDate and EndDate and Status=1 and DayDuration=1 and IsEmployee=1 
						select @LCount=count(*) from LeaveMaster where cast(@Month+'-'+cast(@DayNum as nvarchar(2))+'-'+@Year as Date) between StartDate and EndDate and IsApproved=1 and ApplicantID=@EmployeeID and ApplicantType=0
							
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
						set @SQLQuery='Insert into EmployeeAttendanceMasterT(EmployeeID,SBranchID,DesignationID,FYear,Month,D'+cast(@DayNum as nvarchar(3))+') 
						Values ('+cast (@EmployeeID as nvarchar(5))+','+cast (@SBranchID as nvarchar(5))+','+cast (@DesignationID as nvarchar(5))+','+@Year+','+@Month+','+cast(@Status as nvarchar(2))+')'					
					end
					else
					begin
					set @SQLQuery='Insert into EmployeeAttendanceMasterT(EmployeeID,SBranchID,DesignationID,FYear,Month,D'+cast(@DayNum as nvarchar(3))+') 
						Values ('+cast (@EmployeeID as nvarchar(5))+','+cast (@SBranchID as nvarchar(5))+','+cast (@DesignationID as nvarchar(5))+','+@Year+','+@Month+','+cast(@tStatus as nvarchar(2))+')'
					end
					exec(@SQLQuery)
				end
				else
				begin
					if(isdate(@Month+'-'+cast(@DayNum as nvarchar(2))+'-'+@Year)=0)
					begin
						set @SQLQuery='Update EmployeeAttendanceMasterT set D'+cast(@DayNum as nvarchar(3))+'=-2 where FYear='+@Year+' and Month='+@Month+' and EmployeeID='+cast (@EmployeeID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))
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
							set @SQLQuery='Update EmployeeAttendanceMasterT set D'+cast(@DayNum as nvarchar(3))+'='+cast (@Status as nvarchar(5))+' where FYear='+@Year+' and Month='+@Month+' and EmployeeID='+cast (@EmployeeID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))
							exec(@SQLQuery)
						end
						else
						begin
							set @SQLQuery='Update EmployeeAttendanceMasterT set D'+cast(@DayNum as nvarchar(3))+'='+cast (@tStatus as nvarchar(5))+' where FYear='+@Year+' and Month='+@Month+' and EmployeeID='+cast (@EmployeeID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))
							exec(@SQLQuery)
						end
					end					
				end
				set @DayNum=@DayNum+1
			end
		end	

END
GO
Create proc [dbo].[sp_UpdateEmployeeAttandanceAutoNew]
(
@Year nvarchar(5),
@Month  nvarchar(2),
@Day nvarchar(3),
@EmployeeID int,
@Status int,
@SBranchID int
)
AS
BEGIN
	declare @DesignationID int
	declare @SectionID int
	declare @IsExist int
	declare @DaysWorking int
	set @DaysWorking=6
	declare @SQLQuery nvarchar(max)

	Select  @DesignationID=DesignationID from EmployeeMaster where SBranchID=@SBranchID and EmployeeID=@EmployeeID
	select @IsExist=count(*) from EmployeeAttendanceMasterT where FYear=@Year and [Month]=@Month and EmployeeID=@EmployeeID
	
	if(@IsExist=1)
	begin
		set @SQLQuery='Update EmployeeAttendanceMasterT set D'+@Day+'='+cast (@Status as nvarchar(5))+' where FYear='+@Year+' and Month='+@Month+' and EmployeeID='+cast (@EmployeeID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))
		exec(@SQLQuery)
	end
	else
	begin			
		set @SQLQuery='Insert into EmployeeAttendanceMasterT(EmployeeID,SBranchID,DesignationID,FYear,Month,D'+@Day+') 
		Values ('+cast (@EmployeeID as nvarchar(5))+','+cast (@SBranchID as nvarchar(5))+','+cast (@DesignationID as nvarchar(5))+','+@Year+','+@Month+','+cast(@Status as nvarchar(2))+')'	
	end	

END
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
ALTER Procedure [dbo].[sp_UpdateProductCategory]
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
CREATE proc [dbo].[sp_UpdateSMSConfiguration]
(
@SMSConfigID int,
@SBranchID int,
@IsDefault int,
@Title nvarchar(50),
@Status int,
@header nvarchar(500),
@baseurl nvarchar(500),
@balanceURL nvarchar(500),
@CreatedDate datetime,
@Params ut_SMSConfigurationParams READONLY,
@OpType int
)
AS
BEGIN
	If(@OpType=-1)
	begin
		Delete from [dbo].[SMSConfigurationMaster] where SMSConfigID=@SMSConfigID
		Delete from [dbo].[SMSConfigurationParams] where SMSConfigID=@SMSConfigID
	end
	Else
	Begin
		if(@SMSConfigID=0)
		begin
			Insert into SMSConfigurationMaster(SBranchID,IsDefault,Title,Status,header,baseurl,balanceURL,CreatedDate)
			values(@SBranchID,@IsDefault,@Title,@Status,@header,@baseurl,@balanceURL,@CreatedDate)

			select @SMSConfigID=cast(scope_Identity() as int)

			INSERT INTO  [dbo].[SMSConfigurationParams] (SMSConfigID,ParamType,ParamName,ParamValue)
			SELECT @SMSConfigID,ParamType,ParamName,ParamValue FROM @Params PD where PD.SMSParamID=0 and PD.OpType<>-1
		end
		else
		begin
			Update SMSConfigurationMaster set SBranchID=@SBranchID,IsDefault=@IsDefault,Title=@Title,
			Status=@Status,header=@header,baseurl=@baseurl,balanceURL=@balanceURL where SMSConfigID=@SMSConfigID


			INSERT INTO  [dbo].[SMSConfigurationParams] (SMSConfigID,ParamType,ParamName,ParamValue)
			SELECT @SMSConfigID,ParamType,ParamName,ParamValue FROM @Params PD where PD.SMSParamID=0 and PD.OpType<>-1

			UPDATE e SET   e.SMSConfigID=d.SMSConfigID,e.ParamType=d.ParamType,e.ParamName=d.ParamName,e.ParamValue=d.ParamValue
			FROM  [SMSConfigurationParams] e, @Params d 
			WHERE d.SMSParamID=e.SMSParamID and d.SMSParamID<>0 and  d.OpType<>-1

			delete from [SMSConfigurationParams] where SMSParamID in (Select SMSParamID from @Params where OpType=-1 and SMSParamID<>0)
		end
	End
END
GO
ALTER  Procedure [dbo].[sp_UpdateStockTransaction]
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
		Select top 1 @DeviceSerial=SerialNumber from [dbo].Devices where SBranchID=@SBranchID
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
Create proc [dbo].[sp_UpdateStudentAttandanceAutoNew]
(
@Year nvarchar(5),
@Month  nvarchar(2),
@Day nvarchar(3),
@StudentID int,
@Status int,
@SBranchID int
)
AS
BEGIN
	declare @IsExist int
	declare @ClassID int
	declare @SectionID int
	declare @SQLQuery nvarchar(max)
	declare @DaysWorking int

	Select top 1 @SectionID=SectionID, @ClassID=ClassID from Student_Session where SBranchID=@SBranchID and StudentID=@StudentID and Cast(@Year+'-'+@Month+'-'+@Day as date) between FromDate and ToDate
	order by FromDate desc

	select @IsExist=count(*) from StudentAttendanceMasterT where FYear=@Year and [Month]=@Month and StudentID=@StudentID
	if(@IsExist=1)
	begin
		set @SQLQuery='Update StudentAttendanceMasterT set D'+@Day+'='+cast (@Status as nvarchar(5))+' where FYear='+@Year+' and Month='+@Month+' and StudentID='+cast (@StudentID as nvarchar(5))+' and SBranchID='+cast (@SBranchID as nvarchar(5))
		exec(@SQLQuery)
	end
	else
	begin		
		set @SQLQuery='Insert into StudentAttendanceMasterT(StudentID,SBranchID,ClassID,SectionID,FYear,Month,D'+@Day+') 
		Values ('+cast (@StudentID as nvarchar(5))+','+cast (@SBranchID as nvarchar(5))+','+cast (@ClassID as nvarchar(5))+','+cast (@SectionID as nvarchar(5))+','+@Year+','+@Month+','+cast(@Status as nvarchar(2))+')'									
	end
END
GO
CREATE proc [dbo].[sp_UpdateStudentCustomFee]
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
CREATE procedure [dbo].[sp_UpdateTransportAllocationDetails]
(
@THChangeID int,
@VehicleRouteID int,
@ChangeType int,
@UserType int,
@UserID int,
@StartDate date,
@EndDate date,
@SessionID int,
@KeyID int,
@Applicable int,
@OpType int,
@SBranchID int
)
as 
begin  	
	declare @OldTHChangeID int
	if(@OpType=-1)
	begin
		Delete from TransportHostalAllocationDelocation where THChangeID=@THChangeID
	end
	else
	Begin
		if(@THChangeID=0)
		begin
			if(@Applicable=1)
			begin
				set @EndDate=null
			end
			
			select @OldTHChangeID =THChangeID from TransportHostalAllocationDelocation where UserType=@UserType and UserID=@UserID and EndDate is null and StartDate<@StartDate
			if(isnull(@OldTHChangeID,0)<>0)
			begin
				update TransportHostalAllocationDelocation set EndDate=Dateadd(day,-1,@StartDate) where THChangeID=@OldTHChangeID
			end
				Insert into TransportHostalAllocationDelocation(ChangeType,UserID,UserType,StartDate,EndDate,SessionID,KeyID,VehicleRouteID)
				values(@ChangeType,@UserID,@UserType,@StartDate,@EndDate,@SessionID,@KeyID,@VehicleRouteID)
		end
		else
		begin
			if(@Applicable=1)
			begin
				set @EndDate=null
			end
			select @OldTHChangeID =THChangeID from TransportHostalAllocationDelocation where UserType=@UserType and UserID=@UserID and EndDate is null and StartDate<@StartDate
			if(isnull(@OldTHChangeID,0)<>0)
			begin
				update TransportHostalAllocationDelocation set EndDate=Dateadd(day,-1,@StartDate) where THChangeID=@OldTHChangeID
			end
			Update TransportHostalAllocationDelocation set ChangeType=@ChangeType,UserID=@UserID,UserType=@UserType,StartDate=@StartDate,EndDate=@EndDate,
			SessionID=@SessionID,KeyID=@KeyID,VehicleRouteID=@VehicleRouteID
			where THChangeID=@THChangeID
		end
	End	
	Update [dbo].[StudentMaster] set VehicleRouteID=@VehicleRouteID,StopID=@KeyID 	where StudentID=@UserID and SBranchID=@SBranchID
end
GO
ALTER  procedure [dbo].[spn_GetAbsentStudentListForReview]
(
@Year nvarchar(5),
@Month nvarchar(5),
@Day nvarchar(5),
@SBranchID int
)
as 
begin 
	declare @DayName int
	declare @SessionID nvarchar(3)
	select @SessionID=SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID

	SELECT @DayName= DatePart(dw,cast(@Year+'-'+@Month+'-'+@Day as date)) 
	if( @DayName=1)
	begin
		set @DayName=7
	end
	else
	begin
		set @DayName=@DayName-1
	end

	Select ClassID,ClassName from ClassMaster where SBRanchID=@SBranchID

	declare @HolidayCount int
	declare @ClassesTable  table (ID int)
			Insert into @ClassesTable(ID) select ClassID from ClassMaster where EducationLevelID in (Select ID from EducationLevelMaster where [Days]>=@DayName) and SBranchID=@SBranchID

	Select @HolidayCount=count(*) from HolidayMaster where cast(@Month+'-'+cast(@Day as nvarchar(2))+'-'+@Year as Date) between StartDate and EndDate and Status=1 and DayDuration=1 
	and ((select Count(*) from dbo.SplitStringToTable(Classes,',') where Item=0) >0) and SBranchID=@SBranchID and IsStudents=1
	if(@HolidayCount>0)
	begin
		delete from @ClassesTable
	end			
	else
	begin
		declare @Classes nvarchar(500)
		DECLARE db_cursor CURSOR FOR 
			Select Classes from HolidayMaster where cast(@Month+'-'+cast(@Day as nvarchar(2))+'-'+@Year as Date) between StartDate and EndDate and Status=1 and DayDuration=1 and SBranchID=@SBranchID and IsStudents=1

			OPEN db_cursor  
			FETCH NEXT FROM db_cursor INTO @Classes  

			WHILE @@FETCH_STATUS = 0  
			BEGIN  
				  delete from @ClassesTable where ID in (select Item from dbo.SplitStringToTable(@Classes,','))
				  FETCH NEXT FROM db_cursor INTO @Classes 
			END 

			CLOSE db_cursor  
			DEALLOCATE db_cursor 	
	end


	select t.StudentID,StudentSID,[Name], SS.ClassID,SS.SectionID,SS.RollNo,
	(Select Name from Class_Sections CS where CS.ID=SS.SectionID) as SectionName  from (
	SELECT SM.StudentID ,  'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6) as StudentSID
	 ,[Name]  ,SM.SBranchID
	 FROM StudentMaster SM 
	where 'STU'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6) not in 
	(select employeeid from AttandanceDateTime where datepart(year, logdatetime)=@Year and datepart(month, logdatetime)=@Month and datepart(day, logdatetime)=@Day )
	and SM.StudentID not in (Select ApplicantID from LeaveMaster where ApplicantType=1 and cast(@Month+'-'+@Day+'-'+@Year as date) between StartDate and EndDate)
	and SM.StudentID in (select StudentID from Student_Session where Status=1 and ClassID in (select ID from @ClassesTable)))t  
	left outer join Student_Session SS on SS.StudentID=t.StudentID and Status=1 and SS.SBranchID=@SBranchID and  SS.SessionID=@SessionID
	order by ClassID,SectionID,Name

		 
end
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
Alter procedure [dbo].[spn_GetAllLeaves]
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
		(Select top 1 SchemeID from ClassSessionDetails CSD where CSD.ClassID=CM.ClassID and CSD.SessionID=@SessionID) as EvaluationSchemeID
		 from ClassMaster CM where CM.[Status]=@Status and CM.SBranchID=@SBranchID
		 		 		
	end
	else
	begin
	Select CM.ClassID,CM.ClassName,CM.Status,CM.EducationLevelID,
		(Select Count(*) from Class_Sections CS where CS.ClassID=CM.ClassID  and CS.[Status]=1) as SectionCount,
		(Select count(*) from Student_Session SS where SS.ClassID=CM.ClassID and SS.SessionID=@SessionID and Status=1) as StudentCount,
		(Select top 1 SchemeID from ClassSessionDetails CSD where CSD.ClassID=CM.ClassID and CSD.SessionID=@SessionID) as EvaluationSchemeID
		 from ClassMaster CM where CM.SBranchID=@SBranchID
	End

	Select ID as ID, Name as Name from EducationLevelMaster where SBranchID=@SbranchID and Status=1

	Select EvaluationSchemeID,EvaluationSchemeName from EvaluationSchemeMaster where SBranchID=@SbranchID and SessionID=@SessionID

	Select SessionID,SessionName,SessionStatus from SessionMaster where SBranchID=@SBranchID

	select @SessionID
END
GO
CREATE proc [dbo].[spn_GetClassWiseDueFeeDetailsNew]
(
@QDate date,
@CurDate date,
@SBranchID int=0,
@SessionID int
)
AS
BEGIN
	if(@SessionID=0)
	begin
		Select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
	end

	Declare @Students table(StudentID int,Name nvarchar(100),RollNo nvarchar(100),Gender int,Photo nvarchar(100),ClassID int,FeePaymentMode int,
	StudentSID nvarchar(15),FromDate date,ToDate Date,QuotaID int,
	SessionID int,VehicleRouteID int,HostelRoomID int,SectionID int,
	SessionStartDate date,SessionEndDate date,IsAdmissionFee int,SchoolUID nvarchar(50),FeeAmount numeric(10,2),PreviousDue numeric(10,2)
	,LateFee numeric(10,2),Discounts numeric(10,2),Paid numeric(10,2))

	Declare @ClassDueFeeTable table(ClassID int,SectionID int, ClassName nvarchar(50),PreviousDues numeric(10,2),LateFee numeric(10,2),
	FeeAmount numeric(10,2),Paid numeric(10,2),Discounts numeric(10,2))

	insert into @ClassDueFeeTable
	select ClassID,ID,'',0,0,0,0,0 from Class_Sections where Status=1 and ClassID in (select ClassID from ClassMaster where SBranchID=@SBranchID)

	declare @ClassID int,@SectionID int, @PreviousDues numeric(10,2),@LateFee numeric(10,2),@FeeAmount numeric(10,2),@Paid numeric(10,2),@Discounts numeric(10,2)

	DECLARE class_cursor CURSOR FOR
		SELECT ClassID,SectionID,PreviousDues,LateFee,FeeAmount,Paid,Discounts
		FROM @ClassDueFeeTable FOR UPDATE OF PreviousDues,LateFee,FeeAmount,Paid,Discounts
		OPEN class_cursor
		FETCH NEXT FROM class_cursor
		INTO  @ClassID,@SectionID,@PreviousDues,@LateFee,@FeeAmount,@Paid,@Discounts

		WHILE (@@FETCH_STATUS = 0)
		BEGIN
			Delete from @Students

			insert into @Students
			exec sp_GetClassGroupFeeListOnly @ClassID,@SectionID,@SBranchID,@SessionID,@QDate,@CurDate
			
			select @PreviousDues=sum(PreviousDue),@LateFee=sum(LateFee),@FeeAmount=sum(FeeAmount),@Paid=sum(Paid),@Discounts=sum(Discounts) from @Students

			UPDATE @ClassDueFeeTable SET FeeAmount=@FeeAmount,PreviousDues=@PreviousDues,Discounts=@Discounts,Paid=@Paid,LateFee=@LateFee WHERE CURRENT OF class_cursor
			FETCH NEXT FROM class_cursor
			INTO   @ClassID,@SectionID,@PreviousDues,@LateFee,@FeeAmount,@Paid,@Discounts
		END

		CLOSE class_cursor
		DEALLOCATE class_cursor

		select C.ClassID,CM.ClassName,sum(PreviousDues) as PreviousDues,Sum(LateFee) as LateFee,sum(FeeAmount) as FeeAmount,sum(paid) as Paid,sum(discounts) as DiscAmt
		from @ClassDueFeeTable C left outer join ClassMaster CM on CM.ClassID=C.ClassID
		group by C.ClassID,CM.ClassName
		order by C.ClassID

		Select SessionID,SessionName,SessionStatus from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
		select isnull(@SessionID,0)
END
GO
ALTER proc [dbo].[spn_GetDemandRecipts]
(
@Year nvarchar(5),
@Month nvarchar(5),
@Day nvarchar(5),
@SBranchID int=0,
@ClassID int,
@SectionID int,
@SessionID int
)
AS
BEGIN

declare @StartDate date
declare @EndDate date
declare @SessionStartDate date
	Declare @TransportFeeMode int
	Declare @HostelFeeMode int
	Declare @HostelFee numeric(10,2)
	Declare @TransportFee numeric(10,2)
if(@ClassID=0)
	begin
		select @ClassID=min(ClassID) from ClassMaster where SBranchID=@SBranchID
		select @SectionID=min(ID) from Class_Sections where ClassID=@ClassID
		select @SessionID=(SessionID),@StartDate=(SessionStartDate),@EndDate=(SessionEndDate) from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
	end

if(@SessionID =0 )
begin
	select @SessionID=SessionID,@StartDate=(SessionStartDate),@EndDate=(SessionEndDate) from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
end
else
begin
	select @SessionID=SessionID,@StartDate=(SessionStartDate),@EndDate=(SessionEndDate) from SessionMaster where SessionID=@SessionID
end
	set @SessionStartDate=@StartDate

	
	declare @GroupID int
	select @GroupID=GroupID from Class_Sections where ID=@SectionID
	
	declare @LastPayDay int
	Select @TransportFeeMode=details from MasterSettings where Type='TransportFeeMode' and SBranchID=@SBranchID
	Select @HostelFeeMode=details from MasterSettings where Type='HostelFeeMode' and SBranchID=@SBranchID
	Select @LastPayDay=details from MasterSettings where Type='FeePaymentReminderDate' and SBranchID=@SBranchID
	set @TransportFee=-1
	set @HostelFee=-1
	if(@TransportFeeMode=1)
		begin
			select @TransportFee=sum(isnull(FeeAmount,0)) from ClassFeeStructureMaster 
			where ClassID=@ClassID and GroupID=@GroupID and SBranchID=@SBranchID 
			and FeeTypeID in (select FeeTypeID from FeeTypeMaster where FeeTypeApplicable=5)
		end
		if(@HostelFeeMode=1)
		begin
			select @HostelFee= sum(isnull(FeeAmount,0)) from ClassFeeStructureMaster 
			where ClassID=@ClassID and GroupID=@GroupID and SBranchID=@SBranchID 
			and FeeTypeID in (select FeeTypeID from FeeTypeMaster where FeeTypeApplicable=6)
		end
	Declare @Discounts table(StudentID int,FeeMonth int,FeeYear int,ApprovedAmount decimal(18,2))
	Declare @CurDate date=cast(@Month+'-28-'+@Year as date)
	if(@Day>28 and @Month<>2)
	begin
		SELECT @CurDate=EOMONTH (@CurDate )
	end
Declare @Students table(StudentID int,Name nvarchar(100),RollNo nvarchar(100),Gender int,Photo nvarchar(100),ClassID int,FeePaymentMode int,
StudentSID nvarchar(15), ClassName nvarchar(10),SectionName nvarchar(10),FromDate date,ToDate Date, GroupID int,QuotaID int,
SessionID int,SBranchID int,VehicleRouteID int,HostelRoomID int,TransportFeeMode int,
	HostelFeeMode int,TransportFee numeric(10,2),HostalFee numeric(10,2),LateFee numeric(10,2),SectionID int,SessionStartDate date,LastPayDay int,SessionEndDate date,IsAdmissionFee int,SchoolUID nvarchar(100))

	


Declare @FeeTable table(StudentID int,Name nvarchar(100),Photo nvarchar(100),Gender int,StudentSID  nvarchar(15),RollNo nvarchar(100)
	,QuotaID int,FeePaymentMode int,ClassID int,SectionID int,GroupID int, Month int,Year int,QuotaName nvarchar(50),PaymentID int,ReferanceNumber nvarchar(50),
	PaymentStatus int,ApplicableFee numeric(10,2),DiscAmt numeric(10,2),PaymentAmount numeric(10,2),LateFee numeric(10,2),TransportFee numeric(10,2),HostalFee numeric(10,2),LastPayDay int)


declare @LMonth int
declare @LYear int
	Insert into @Students select * from [dbo].[fn_GetStudentFeeRelatedListOnClassSection](@ClassID,@SectionID,@SessionID,@Month,@Year)

	Insert into @Discounts Select StudentID,FeeMonth,FeeYear,
	(Select sum(ApprovedAmount) from  [FeeDiscountRequestDetails] FDD where FDM.DiscRequestID=FDD.DiscRequestID ) as ApprovedAmount
	from  [FeeDiscountRequestMaster] FDM 
	 where StudentID in (select StudentID from @Students) and Status=1
	 
while(@StartDate<=@CurDate)
begin
	set @LMonth= datepart(month,@StartDate)
	set @LYear=datepart(year,@StartDate)
	insert into @FeeTable
	Select StudentID,Name,Photo,Gender,StudentSID,RollNo,QuotaID,FeePaymentMode,ClassID,SectionID,GroupID,Month,Year,QuotaName,PaymentID,ReferanceNumber,PaymentStatus,
	(case when PaymentID is not null then ApplicableFee else ApplicableFee+HostalFee+TransportFee end) as ApplicableFee,DiscAmt,PaymentAmount,LateFee,0 as HostalFee, 0 as TransportFee,LastPayDay from
	(Select SM.StudentID,SM.Name,SM.Photo,SM.Gender,SM.StudentSID,
		SM.RollNo,SM.QuotaID,SM.FeePaymentMode,SM.ClassID,SM.SectionID ,SM.GroupID,@LMonth as [Month],@LYear as Year,
		isnull((Select QuotaName from QuotaMaster QM where QM.QuotaID=SM.QuotaID),'No Quota') as QuotaName,
		PM.PaymentID as PaymentID,'' as ReferanceNumber,
		(case when isnull(PaymentAmount,0)=0 then 0 else (case when isnull(PM.ApplicableFee,CGQF.ApplicableFee)-isnull(PaymentAmount,0)-isnull(PM.DiscAmt,0)>0 then 2 else 1 end)end) as PaymentStatus
		,isnull(PM.ApplicableFee,CGQF.ApplicableFee) as ApplicableFee,isnull(nullif(PM.DiscAmt,0),D.ApprovedAmount) as DiscAmt,
		PaymentAmount,SM.LateFee as LateFee,
		[dbo].[fn_GetStudentTransportHostalFeeAmount](@LMonth,@LYear,SM.StudentID,0,0,@HostelFeeMode,@TransportFeeMode,@ClassID,SM.FeePaymentMode,@GroupID,@SBranchID,@TransportFee,@HostelFee,@SessionID) as TransportFee 
		,[dbo].[fn_GetStudentTransportHostalFeeAmount](@LMonth,@LYear,SM.StudentID,0,1,@HostelFeeMode,@TransportFeeMode,@ClassID,SM.FeePaymentMode,@GroupID,@SBranchID,@TransportFee,@HostelFee,@SessionID) as HostalFee
		,@LastPayDay as LastPayDay
		from @Students SM left outer join v_StudentPaymentDetails PM on SM.StudentID=PM.PayeeID and PM.Year=@LYear and PM.Month=@LMonth
		left outer join v_ClassGroupQuotaSessionFee  CGQF on CGQF.ClassID=SM.ClassID and CGQF.GroupID=SM.GroupID 
		and CGQF.QuotaID=SM.QuotaID and CGQF.SessionID=SM.SessionID 
		and CGQF.MonthType=(Case when datepart(month,SM.SessionStartDate)=@LMonth and (datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth) then 5 else
		(Case when datepart(month,SM.SessionStartDate)=@LMonth and (datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth) then 6 else 
		(case when datepart(month,SM.SessionStartDate)=@LMonth then (case when isnull(SM.IsAdmissionFee,0)=0 then 1 else 0 end)  
		else (Case when datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth then 3
		else (case when  datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth then 4 else 2 end) end) end)end)end)
		and CGQF.FeePaymentMode=SM.FeePaymentMode
		and @LMonth+@LYear*12>=datepart(month,SM.SessionStartDate)+datepart(year,SM.SessionStartDate)*12 and @LMonth+@LYear*12<=datepart(month,SM.SessionEndDate)+datepart(year,SM.SessionEndDate)*12
		left outer join @Discounts D on D.StudentID=SM.StudentID and D.FeeMonth= @LMonth and D.FeeYear=@LYear
		where CGQF.ApplicableFee is not null )t

Set @StartDate= DATEADD(month,1,@StartDate)
end


Declare @TOTStudents table(StudentID int,MaxMonthYear int)
insert into @TOTStudents select StudentID,Max(Month+Year*12) from @FeeTable group by StudentID


--For Getting Pending Month Names Start

declare @MNStudentID int
declare @MNMonth int
declare @MNYear int
declare @MNPending numeric(10,2)

Declare @DuesMonths table(StudentID int,DMonthName nvarchar(100))

 DECLARE db_MonthNamescursor CURSOR FOR 
		 Select Month,Year,StudentID,sum(TransportFee)+sum(HostalFee)+sum(ApplicableFee-isnull(DiscAmt,0)-isnull(PaymentAmount,0)+(Case when FT.PaymentStatus=0 then LateFee else 0 end))
	 from @FeeTable FT 
where  FT.Year*12+FT.Month<(Select MaxMonthYear from @TOTStudents TS where TS.StudentID=FT.StudentID)
group by Month,Year,StudentID
having sum(TransportFee)+sum(HostalFee)+sum(ApplicableFee-isnull(DiscAmt,0)-isnull(PaymentAmount,0)+(Case when FT.PaymentStatus=0 then LateFee else 0 end))>0
order by StudentID,Year,Month
		  OPEN db_MonthNamescursor   
		  FETCH NEXT FROM db_MonthNamescursor INTO @MNMonth,@MNYear,@MNStudentID,@MNPending

		  WHILE @@FETCH_STATUS = 0   
		  BEGIN   
			if(not exists(select * from @DuesMonths where StudentID=@MNStudentID))
			begin
				Insert into @DuesMonths(StudentID,DMonthName)
				values(@MNStudentID,DATENAME(month, DATEADD(month, @MNMonth-1, CAST('2008-01-01' AS datetime))))
			end
			else
			begin
				Update @DuesMonths set DMonthName=DMonthName+', '+DATENAME(month, DATEADD(month, @MNMonth-1, CAST('2008-01-01' AS datetime)))
				where StudentID=@MNSTudentID
			end

			FETCH NEXT FROM db_MonthNamescursor INTO @MNMonth,@MNYear,@MNStudentID,@MNPending
		  END
		  CLOSE db_MonthNamescursor   
		DEALLOCATE db_MonthNamescursor
	--For Getting Pending Month Names End


Select tt.StudentID,tt.StudentSID,tt.RollNo,tt.Photo,tt.Name,tt.Gender,tt.QuotaID,tt.FeePaymentMode,Month,Year, QuotaName,PaymentStatus,PaymentID,LateFee,DiscAmt,PaymentAmount,
ApplicableFee,PreviousDues ,
SM.NotificationSMSTo,(Select FatherName from ParentMaster PM where PM.ParentID=SM.ParentID) as FatherName,DM.DMonthName,
DATENAME(month, DATEADD(month, @Month-1, CAST('2008-01-01' AS datetime))) as [MonthName],
(Select ClassName from ClassMaster CM where CM.ClassID=tt.ClassID)+' / '+ (Select Name from Class_Sections CM where CM.ID=tt.SectionID) as ClassName
from
(select StudentID,StudentSID,RollNo,Photo,Name,Gender,QuotaID,FeePaymentMode,Month,Year, QuotaName,PaymentStatus,PaymentID,ClassID,SectionID,
(case when PaymentAmount is null and  (@Year*12+@Month>[Year]*12+[Month] or @Year*12+@Month=[Year]*12+[Month] and @Day>LastPayDay) then LateFee else 0 end)
 as LateFee,DiscAmt,PaymentAmount,ApplicableFee
,(Select sum(TransportFee)+sum(HostalFee)+sum(ApplicableFee-isnull(DiscAmt,0)-isnull(PaymentAmount,0)+(Case when FT.PaymentStatus=0 then LateFee else 0 end)) from @FeeTable FT 
where FT.StudentID=FTM.StudentID and FT.Year*12+FT.Month<(Select MaxMonthYear from @TOTStudents TS where TS.StudentID=FT.StudentID)) as PreviousDues
from @FeeTable FTM)tt left outer join StudentMaster SM on SM.StudentID=tt.StudentID
 left outer join @DuesMonths DM on DM.StudentID=tt.StudentID
 where Month+Year*12=(Select MaxMonthYear from @TOTStudents TS where TS.StudentID=tt.StudentID)
 and isnull(tt.PreviousDues,0)+isnull(tt.ApplicableFee,0)-isnull(tt.PaymentAmount,0)-isnull(tt.DiscAmt,0)>0

	order by Name,	ClassID,SectionID,RollNo desc

	Select ClassID as ID, ClassName as Name from ClassMaster where SBranchID=@SBranchID
	select ID,Name from Class_Sections where ClassID=@ClassID and Status=1

	select @ClassID
	select @SectionID

	Select BranchSchoolName as BranchName,Address from SBranchMaster where SBranchID=@SBranchID

	Select SessionID as ID,SessionName as Name from SessionMaster where SBranchID=@SBranchID

	select isnull(@SessionID,0)
END

GO
Create proc [dbo].[spn_GetDemandReciptsNew]
(
@QDate date,
@SBranchID int=0,
@ClassID int,
@SectionID int,
@SessionID int,
@CurDate date
)
AS
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

	declare @ClassName nvarchar(50),@SectionName as nvarchar(50)

	select @ClassName=ClassName from ClassMAster where ClassID=@ClassID
	select @SectionName =Name from Class_Sections where ID=@SectionID
	

	Declare @Students table(StudentID int,Name nvarchar(100),RollNo nvarchar(100),Gender int,Photo nvarchar(100),ClassID int,FeePaymentMode int,
	StudentSID nvarchar(15),FromDate date,ToDate Date,QuotaID int,
	SessionID int,VehicleRouteID int,HostelRoomID int,SectionID int,
	SessionStartDate date,SessionEndDate date,IsAdmissionFee int,SchoolUID nvarchar(50),FeeAmount numeric(10,2),PreviousDue numeric(10,2)
	,LateFee numeric(10,2),Discounts numeric(10,2),Paid numeric(10,2))


	insert into @Students
		exec sp_GetClassGroupFeeListOnly @ClassID,@SectionID,@SBranchID,@SessionID,@QDate,@CurDate

	select S.StudentID,SM.NotificationSMSTo,PM.FatherName,S.Name,S.SchoolUID,S.StudentSID,S.RollNo,S.LateFee,S.Discounts as DiscAmt,S.FeeAmount as ApplicableFee,S.PreviousDue as PreviousDues,
	S.Paid as PaymentAmount ,isnull(@ClassName,'')+'/'+isnull(@SectionName,'') as ClassName,DATENAME(month, @QDate)+'-'+cast(datepart(year,@QDate) as nvarchar(5)) as [MonthName]
	from @Students S left outer join StudentMAster SM on S.StudentID=SM.StudentID 
	left outer join ParentMaster PM on PM.ParentID=SM.ParentID
	where isnull(S.FeeAmount,0)+isnull(S.LateFee,0)+isnull(S.PreviousDue,0)-isnull(S.Discounts,0)-isnull(S.Paid,0)>0

	Select ClassID as ID, ClassName as Name from ClassMaster where SBranchID=@SBranchID
	Select ID,Name from Class_Sections where ClassID=@ClassID
	select @ClassID
	select @SectionID

	select * from SBranchMAster where SBranchID=@SBranchID

	select SessionID as ID,SessionName as Name,SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID
	select isnull(@SessionID,0)
END
GO
CREATE procedure [dbo].[spn_GetDueFeeStudentsForSMS]
(
@QDate date,
@CurDate date,
@SBranchID int=0,
@SessionID int
)
AS
BEGIN
	if(@SessionID=0)
	begin
		Select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
	end

	Declare @Students table(StudentID int,Name nvarchar(100),RollNo nvarchar(100),Gender int,Photo nvarchar(100),ClassID int,FeePaymentMode int,
	StudentSID nvarchar(15),FromDate date,ToDate Date,QuotaID int,
	SessionID int,VehicleRouteID int,HostelRoomID int,SectionID int,
	SessionStartDate date,SessionEndDate date,IsAdmissionFee int,SchoolUID nvarchar(50),FeeAmount numeric(10,2),PreviousDue numeric(10,2)
	,LateFee numeric(10,2),Discounts numeric(10,2),Paid numeric(10,2))

	Declare @ClassDueFeeTable table(ClassID int,SectionID int, ClassName nvarchar(50),PreviousDues numeric(10,2),LateFee numeric(10,2),
	FeeAmount numeric(10,2),Paid numeric(10,2),Discounts numeric(10,2))

	insert into @ClassDueFeeTable
	select CM.ClassID,ID,'',0,0,0,0,0 from Class_Sections CS
	left outer join ClassMaster CM on CS.ClassID=CM.ClassID
	where CS.Status=1 and CM.SBranchID=@SBranchID and CM.Status=1 
	order by CM.SequenceNo

	declare @ClassID int,@SectionID int, @PreviousDues numeric(10,2),@LateFee numeric(10,2),@FeeAmount numeric(10,2),@Paid numeric(10,2),@Discounts numeric(10,2)

	DECLARE class_cursor CURSOR FOR
		SELECT ClassID,SectionID,PreviousDues,LateFee,FeeAmount,Paid,Discounts
		FROM @ClassDueFeeTable --FOR UPDATE OF PreviousDues,LateFee,FeeAmount,Paid,Discounts
		OPEN class_cursor
		FETCH NEXT FROM class_cursor
		INTO  @ClassID,@SectionID,@PreviousDues,@LateFee,@FeeAmount,@Paid,@Discounts

		WHILE (@@FETCH_STATUS = 0)
		BEGIN
			--Delete from @Students

			insert into @Students
			exec sp_GetClassGroupFeeListOnly @ClassID,@SectionID,@SBranchID,@SessionID,@QDate,@CurDate
			
			--select @PreviousDues=sum(PreviousDue),@LateFee=sum(LateFee),@FeeAmount=sum(FeeAmount),@Paid=sum(Paid),@Discounts=sum(Discounts) from @Students

			--UPDATE @ClassDueFeeTable SET FeeAmount=@FeeAmount,PreviousDues=@PreviousDues,Discounts=@Discounts,Paid=@Paid,LateFee=@LateFee WHERE CURRENT OF class_cursor
			FETCH NEXT FROM class_cursor
			INTO   @ClassID,@SectionID,@PreviousDues,@LateFee,@FeeAmount,@Paid,@Discounts
		END

		CLOSE class_cursor
		DEALLOCATE class_cursor

		delete from @Students where round(isnull(FeeAmount,0)+isnull(LateFee,0)+isnull(PreviousDue,0)-isnull(Discounts,0)-isnull(Paid,0),0)=0

		select S.StudentID as RecieverID,1 as RecieverType,S.StudentSID,S.RollNo,S.Photo,S.Name,S.Gender,S.QuotaID,S.FeePaymentMode,DatePart(month,@QDate) as Month,DatePart(Year,@QDate) as Year
		,S.LateFee,S.Discounts as DiscAmt,S.FeeAmount as ApplicableFee,S.PreviousDue as PreviousDues,S.Paid as PaymentAmount 
		,isnull(SM.NotificationSMSTo,0) as NotificationSMSTo,
		(case when isnull(SM.NotificationSMSTo,0)=0 then PM.FatherMobileNo when isnull(SM.NotificationSMSTo,0)=1 then PM.MotherMobileNo  else SM.GuardianMobileNo end) as Mobile,
		(Select ClassName from ClassMaster CM where CM.ClassID=S.ClassID)+'/'+(Select Name from Class_Sections cs where CS.ID=S.SectionID)+' ('+
		(cast(isnull(S.FeeAmount,0)+isnull(S.PreviousDue,0)+isnull(S.LateFee,0)-isnull(S.Paid,0)-isnull(S.Discounts,0) as nvarchar(50)))+')' as Details
		from @Students S left outer join StudentMAster SM on S.StudentID=SM.StudentID 
		left outer join ParentMaster PM on PM.ParentID=SM.ParentID
		--where round(isnull(S.FeeAmount,0)+isnull(S.LateFee,0)+isnull(S.PreviousDue,0)-isnull(S.Discounts,0)-isnull(S.Paid,0),0)>0

end
GO
CREATE proc [dbo].[spn_GetDueFeeStudentsSMSDetails]
(
@Year nvarchar(5),
@Month nvarchar(5),
@Day nvarchar(5),
@HostelFeeMode int,
@TransportFeeMode int,
@LastPayDay int,
@SessionStartDate date,
@SBranchID int=0,
@SessionID int=0
)
AS
BEGIN

Declare @Discounts table(StudentID int,FeeMonth int,FeeYear int,ApprovedAmount decimal(18,2))

declare @StartDate date
declare @EndDate date
	Declare @HostelFee numeric(10,2)
	Declare @TransportFee numeric(10,2)
if(@SessionID =0 )
begin
	select @SessionID=SessionID,@StartDate=(SessionStartDate),@EndDate=(SessionEndDate) from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
end
else
begin
	select @SessionID=SessionID,@StartDate=(SessionStartDate),@EndDate=(SessionEndDate) from SessionMaster where SessionID=@SessionID
end
	set @SessionStartDate=@StartDate

	Select @TransportFeeMode=details from MasterSettings where Type='TransportFeeMode' and SBranchID=@SBranchID
	Select @HostelFeeMode=details from MasterSettings where Type='HostelFeeMode' and SBranchID=@SBranchID
	Select @LastPayDay=details from MasterSettings where Type='FeePaymentReminderDate' and SBranchID=@SBranchID
	set @TransportFee=-1
	set @HostelFee=-1
	
	Declare @QDate date=cast(@Month+'-'+@Day+'-'+@Year as date)
	Declare @CurDate date=cast(@Month+'-28-'+@Year as date)
	if(@Day>28 and @Month<>2)
	begin
		SELECT @CurDate=EOMONTH (@CurDate )
	end

Declare @Students table(StudentID int,Name nvarchar(100),RollNo nvarchar(100),Gender int,Photo nvarchar(100),ClassID int,FeePaymentMode int,
StudentSID nvarchar(15), ClassName nvarchar(10),SectionName nvarchar(10),FromDate date,ToDate Date, GroupID int,QuotaID int,
SessionID int,SBranchID int,VehicleRouteID int,HostelRoomID int,TransportFeeMode int,
	HostelFeeMode int,TransportFee numeric(10,2),HostalFee numeric(10,2),LateFee numeric(10,2),SectionID int,SessionStartDate date,LastPayDay int,SessionEndDate date)

Declare @FeeTable table(StudentID int,Name nvarchar(100),Photo nvarchar(100),Gender int,StudentSID  nvarchar(15),RollNo nvarchar(100)
	,QuotaID int,FeePaymentMode int,ClassID int,SectionID int,GroupID int, Month int,Year int,QuotaName nvarchar(50),PaymentID int,ReferanceNumber nvarchar(50),
	PaymentStatus int,ApplicableFee numeric(10,2),DiscAmt numeric(10,2),PaymentAmount numeric(10,2),LateFee numeric(10,2),TransportFee numeric(10,2),HostalFee numeric(10,2),LastPayDay int)


declare @LMonth int
declare @LYear int
	Insert into @Students select * from [dbo].[fn_GetActiveStudentFeeRelatedListOnSession](@SessionID,@Month,@Year)

	Insert into @Discounts Select StudentID,FeeMonth,FeeYear,
	(Select sum(ApprovedAmount) from  [FeeDiscountRequestDetails] FDD where FDM.DiscRequestID=FDD.DiscRequestID ) as ApprovedAmount
	from  [FeeDiscountRequestMaster] FDM 
	 where StudentID in (select StudentID from @Students) and Status=1

while(@StartDate<=@CurDate)
begin
	set @LMonth= datepart(month,@StartDate)
	set @LYear=datepart(year,@StartDate)
	insert into @FeeTable
	Select StudentID,Name,Photo,Gender,StudentSID,RollNo,QuotaID,FeePaymentMode,ClassID,SectionID,GroupID,Month,Year,QuotaName,PaymentID,ReferanceNumber,PaymentStatus,
	(case when PaymentID is not null then ApplicableFee else ApplicableFee+HostalFee+TransportFee end) as ApplicableFee,DiscAmt,PaymentAmount,LateFee,0 as HostalFee, 0 as TransportFee,LastPayDay from
	(Select SM.StudentID,SM.Name,SM.Photo,SM.Gender,SM.StudentSID,
		SM.RollNo,SM.QuotaID,SM.FeePaymentMode,SM.ClassID,SM.SectionID ,SM.GroupID,@LMonth as [Month],@LYear as Year,
		isnull((Select QuotaName from QuotaMaster QM where QM.QuotaID=SM.QuotaID),'No Quota') as QuotaName,
		PM.PaymentID as PaymentID,'' as ReferanceNumber,
		(case when isnull(PaymentAmount,0)=0 then 0 else (case when isnull(PM.ApplicableFee,CGQF.ApplicableFee)-isnull(PaymentAmount,0)-isnull(PM.DiscAmt,0)>0 then 2 else 1 end)end) as PaymentStatus
		,isnull(PM.ApplicableFee,CGQF.ApplicableFee) as ApplicableFee,isnull(nullif(PM.DiscAmt,0),D.ApprovedAmount) as DiscAmt,
		PaymentAmount,SM.LateFee as LateFee,
		[dbo].[fn_GetStudentTransportHostalFeeAmount](@LMonth,@LYear,SM.StudentID,0,0,@HostelFeeMode,@TransportFeeMode,SM.ClassID,SM.FeePaymentMode,SM.GroupID,@SBranchID,-1,-1,@SessionID) as TransportFee 
		,[dbo].[fn_GetStudentTransportHostalFeeAmount](@LMonth,@LYear,SM.StudentID,0,1,@HostelFeeMode,@TransportFeeMode,SM.ClassID,SM.FeePaymentMode,SM.GroupID,@SBranchID,-1,-1,@SessionID) as HostalFee
		,@LastPayDay as LastPayDay
		 --,dbo.fn_GetStudentDueFeeAmount(SM.StudentID,SM.ClassID,SM.QuotaID,SM.GroupID,SM.VehicleRouteID,SM.FeePaymentMode,SM.HostelFeeMode,SM.TransportFeeMode,SM.LateFee,SM.HostalFee,SM.TransportFee,SM.HostelRoomID,SSM.SessionStartDate,SSM.SessionEndDate,@CurDate,SM.LastPayDay,0) as PreviousDues

		from @Students SM left outer join v_StudentPaymentDetails PM on SM.StudentID=PM.PayeeID and PM.Year=@LYear and PM.Month=@LMonth
		left outer join v_ClassGroupQuotaSessionFee  CGQF on CGQF.ClassID=SM.ClassID and CGQF.GroupID=SM.GroupID 
		and CGQF.QuotaID=SM.QuotaID and CGQF.SessionID=SM.SessionID 
		and CGQF.MonthType=(Case when datepart(month,SM.SessionStartDate)=@LMonth and (datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth) then 5 else
		(Case when datepart(month,SM.SessionStartDate)=@LMonth and (datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth) then 6 else 
		(case when datepart(month,SM.SessionStartDate)=@LMonth then 1 
		else (Case when datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth then 3
		else (case when  datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth then 4 else 2 end) end) end)end)end)
		and CGQF.FeePaymentMode=SM.FeePaymentMode
		and @LMonth+@LYear*12>=datepart(month,SM.SessionStartDate)+datepart(year,SM.SessionStartDate)*12 and @LMonth+@LYear*12<=datepart(month,SM.SessionEndDate)+datepart(year,SM.SessionEndDate)*12
		left outer join @Discounts D on D.StudentID=SM.StudentID and D.FeeMonth= @LMonth and D.FeeYear=@LYear
		where CGQF.ApplicableFee is not null )t

Set @StartDate= DATEADD(month,1,@StartDate)
end


Declare @TOTStudents table(StudentID int,MaxMonthYear int)
insert into @TOTStudents select StudentID,Max(Month+Year*12) from @FeeTable group by StudentID

Select t.StudentID,StudentSID,RollNo,t.Photo,t.Name,t.Gender,QuotaID,FeePaymentMode,Month,Year, QuotaName,PaymentStatus,PaymentID,LateFee,DiscAmt,PaymentAmount,ApplicableFee,PreviousDues,
SM.NotificationSMSTo,
  (case when isnull(SM.NotificationSMSTo,0)=0 then (select FatherMobileNo from ParentMaster PM where PM.ParentID=SM.ParentID)
		  else (case when NotificationSMSTo=1 then (Select MotherMobileNo from ParentMaster PM where PM.ParentID=SM.ParentID) else SM.GuardianMobileNo end) end) as MobileNo,AU.FCMToken as deviceToken
from
(select StudentID,StudentSID,RollNo,Photo,Name,Gender,QuotaID,FeePaymentMode,Month,Year, QuotaName,PaymentStatus,PaymentID,ClassID,SectionID,
 (case when PaymentAmount is null and  (@Year*12+@Month>[Year]*12+[Month] or @Year*12+@Month=[Year]*12+[Month] and @Day>LastPayDay) then LateFee else 0 end)
 as LateFee,DiscAmt,PaymentAmount,ApplicableFee
,(Select sum(TransportFee)+sum(HostalFee)+sum(ApplicableFee-isnull(DiscAmt,0)-isnull(PaymentAmount,0)+(Case when FT.PaymentStatus=0 then LateFee else 0 end)) from @FeeTable FT 
where FT.StudentID=FTM.StudentID and FT.Year*12+FT.Month<(Select MaxMonthYear from @TOTStudents TS where TS.StudentID=FT.StudentID)) as PreviousDues
from @FeeTable FTM where Month=@Month and Year=@Year )t left outer join StudentMaster SM on SM.StudentID=t.StudentID left outer join AppUsers AU on AU.UserID=SM.ParentID and AU.UserType=4 and AU.LastActive>dateadd(day,-7,@QDate)
where Month+Year*12=(Select MaxMonthYear from @TOTStudents TS where TS.StudentID=t.StudentID)
	order by ClassID,SectionID,RollNo desc

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
ALTER  PROCEDURE [dbo].[spn_GetHolidayDetails]
(
@HolidayID int,
@SBranchID int
)
AS
begin
		select [HolidayID],[HolidayType],[StartDate],[EndDate],[Classes],[Title],[Days],[HolidayDescription]
		  ,[DayDuration] ,[Status] ,[IsEmployee],[IsStudents],[AssociatedIDs]
		  from dbo.HolidayMaster where HolidayID=@HolidayID

		select ClassID,ClassName from ClassMaster where [Status]=1  and SBranchID=@SBranchID
		select * from EmployeeTypeMaster where SBranchID=@SBranchID or SBranchID=0
end
GO
ALTER  PROCEDURE [dbo].[spn_GetHolidays]
(
@SBranchID int
)
AS
begin
	select HolidayID,HolidayType,(Case when HolidayType=1 then 'Local' else 'National' end) 'HolidayTypeName',
	[dbo].[GetClassNames](Classes) 'ClassesIncluded',[dbo].[GetEmployeeTypeNames](AssociatedIDs) as EmployeeTypes,
	(Case When DayDuration=1 then 'Full Day' else (Case when DayDuration=2 then 'First Half' else 'Second Half' end)end) 'DayDurationName',
	StartDate,EndDate,DATEDIFF(day,StartDate,EndDate)+1 as [Days],DayDuration, [HolidayDescription],Title,[Status],
	ISSMSSent,SMSSentTo,AssociatedIDs,IsEmployee,IsStudents,
	Classes from HolidayMaster where  SBranchID=@SBranchID order by StartDate desc
end
GO
GO
create procedure [dbo].[spn_GetParentAppDetails] --1,0,0,0
(
--Created By Niraj 
-- For Parent User Details
@SBranchID int,
@ClassID int,
@SectionID int,
@SessionID int=0
)
as
begin

declare @SessionStatus int=0

if(@SessionID=0)
	begin
		Select @SessionID=SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
	end
	Select @SessionStatus=SessionStatus from SessionMaster where SessionID=@SessionID

if(@ClassID=0)
begin
	Select @ClassID=min(ClassID) from ClassMaster where SBranchID=@SBranchID
	Select @SectionID=min(ID) from Class_Sections where ClassID=@ClassID
end
if(@SectionID<>0)
begin
SELECT        SM.StudentID, 'STUD' + RIGHT(REPLICATE('0', 6) + CAST(SM.StudentID AS VARCHAR(6)), 6) AS StudentSID, SM.Name,SM.SchoolUID, SM.DOB,SM.DOJ, SM.EmailID, SM.BloodGroup,SM.Gender, 
              SM.GuardianName, SM.GuardianMobileNo, SM.Photo, SS.RollNo, SM.AccessCardNo,
			  'PAR' + RIGHT(REPLICATE('0', 6) + CAST(PM.ParentID AS VARCHAR(6)), 6) AS SParentID, PM.FatherName, PM.FatherMobileNo,PM.FatherDOB,PM.MotherName, PM.MotherMobileNo, SM.AadharCardNo,
			
              SM.Gender, SS.ClassID, SS.SectionID,ISNULL((SELECT ClassName FROM dbo.ClassMaster AS CM WHERE (ClassID = SS.ClassID)), 'NA') AS ClassName, 
			  ISNULL((SELECT Name FROM dbo.Class_Sections AS CS WHERE (ID = SS.SectionID)), 'NA') AS SectionName
			
FROM            dbo.StudentMaster AS SM LEFT OUTER JOIN
                         dbo.Student_Session AS SS ON SS.StudentID = SM.StudentID AND SS.Status =isnull(nullif((case when  @SessionStatus=1 then 1 else -1 end),-1),SS.Status) LEFT OUTER JOIN
                         dbo.ParentMaster AS PM ON PM.ParentID = SM.ParentID 					
						
WHERE        (SM.SBranchID = @SBranchID) AND (SS.ClassID=@ClassID) AND (SS.SectionID=@SectionID) and  (SS.SessionID=@SessionID)
ORDER BY SM.Gender desc,SM.Name 
end
else
begin
SELECT        SM.StudentID, 'STUD' + RIGHT(REPLICATE('0', 6) + CAST(SM.StudentID AS VARCHAR(6)), 6) AS StudentSID, SM.Name,SM.SchoolUID, SM.DOB, SM.EmailID, SM.BloodGroup,SM.Gender, 
              SM.GuardianName, SM.GuardianMobileNo, SM.Photo, SS.RollNo, SM.AccessCardNo,
			   'PAR' + RIGHT(REPLICATE('0', 6) + CAST(PM.ParentID AS VARCHAR(6)), 6) AS SParentID,
			   PM.FatherName, PM.FatherMobileNo,PM.MotherName, PM.MotherMobileNo, PM.FatherDOB,
              SM.AadharCardNo,  SM.Gender, SS.ClassID, SS.SectionID,
			  ISNULL((SELECT ClassName FROM dbo.ClassMaster AS CM WHERE (ClassID = SS.ClassID)), 'NA') AS ClassName, 
			  ISNULL((SELECT Name FROM dbo.Class_Sections AS CS WHERE (ID = SS.SectionID)), 'NA') AS SectionName
			  
FROM            dbo.StudentMaster AS SM LEFT OUTER JOIN
                         dbo.Student_Session AS SS ON SS.StudentID = SM.StudentID AND  SS.Status =isnull(nullif((case when  @SessionStatus=1 then 1 else -1 end),-1),SS.Status) LEFT OUTER JOIN
                         dbo.ParentMaster AS PM ON PM.ParentID = SM.ParentID
WHERE        (SM.SBranchID = @SBranchID) and  (SS.SessionID=@SessionID)
ORDER BY SM.Gender desc,SM.Name 
end

select ClassID,ClassName from ClassMaster where SBranchID=@SBranchID

select ID,Name from Class_Sections where ClassID=@ClassID

select isnull(@ClassID,0)
select isnull(@SectionID,0)
Select SessionID as ID,SessionName as Name,SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID
select isnull(@SessionID,0)

end
GO
ALTER procedure [dbo].[spn_GetRecieverListForManualSMS]
(
@SBranchID int,
@SMSType int,
@Recievers nvarchar(50),
@Year nvarchar(5),
@Month nvarchar(5),
@Day nvarchar(5)
)
as 
begin 
	 declare @SessionID int
	 select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and SessionStatus=1 
	if(@SMSType=1)
	begin
		exec spn_GetAbsentStudentsDetailsManual @Year,@Month, @Day, @SBranchID
	end
	else if(@SMSType=2 or @SMSType=6 or @SMSType=-1)
	begin
		if(@Recievers='0')
			begin
				SELECT SM.StudentID as RecieverID, 'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6) as StudentSID,NotificationSMSTo
				  ,[Name]     ,SM.SBranchID,
				  (Select ClassName from ClassMaster CM where CM.ClassID=SS.ClassID)+'/'+(Select Name from Class_Sections cs where CS.ID=SS.SectionID) as Details,
				  (case when isnull(NotificationSMSTo,0)=0 then (select FatherMobileNo from ParentMaster PM where PM.ParentID=SM.ParentID)
				  else (case when NotificationSMSTo=1 then (Select MotherMobileNo from ParentMaster PM where PM.ParentID=SM.ParentID) else SM.GuardianMobileNo end) end) as Mobile
				FROM StudentMaster SM left outer join Student_Session SS on SS.StudentID=SM.StudentID and  SS.SessionID=@SessionID  where SM.SBranchID=@SBranchID
			end
			else
			begin
			select * from 
				(SELECT SM.StudentID as RecieverID, 'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6) as StudentSID,NotificationSMSTo
				  ,[Name],SM.SBranchID,ClassID,
				  (Select ClassName from ClassMaster CM where CM.ClassID=SS.ClassID)+'/'+(Select Name from Class_Sections cs where CS.ID=SS.SectionID) as Details,
				  (case when isnull(NotificationSMSTo,0)=0 then (select FatherMobileNo from ParentMaster PM where PM.ParentID=SM.ParentID)
				  else (case when NotificationSMSTo=1 then (Select MotherMobileNo from ParentMaster PM where PM.ParentID=SM.ParentID) else SM.GuardianMobileNo end) end) as Mobile
				FROM StudentMaster SM left outer join Student_Session SS on SS.StudentID=SM.StudentID and SS.SessionID=@SessionID  where SM.SBranchID=@SBranchID)t
				where t.ClassID in (select Item from dbo.SplitStringToTable(@Recievers,','))
				
			end
	end
	else if(@SMSType=3)
	begin
		declare @SessionStartDate date
		select @SessionStartDate=cast(details as date) from MasterSettings where Type='SessionDate'
		--exec spn_GetDueFeeStudentsSMSDetailsMini  @Year, @Month, @Day,1,1,10,@SessionStartDate,@SBranchID
		declare @QDate date = @Year+'-'+@Month+'-'+@Day
		exec spn_GetDueFeeStudentsForSMS @QDate,@QDate,@SBranchID,0
	end 
	else
	begin
		SELECT SM.StudentID as RecieverID, 'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6) as StudentSID,NotificationSMSTo as RecieverType,
			[Name]     ,SM.SBranchID,ClassID,
			(Select ClassName from ClassMaster CM where CM.ClassID=SS.ClassID)+'/'+(Select Name from Class_Sections cs where CS.ID=SS.SectionID) as Details,
			(case when isnull(NotificationSMSTo,0)=0 then (select FatherMobileNo from ParentMaster PM where PM.ParentID=SM.ParentID)
			else (case when NotificationSMSTo=1 then (Select MotherMobileNo from ParentMaster PM where PM.ParentID=SM.ParentID) else SM.GuardianMobileNo end) end) as Mobile
		FROM StudentMaster SM left outer join Student_Session SS on SS.StudentID=SM.StudentID and SS.SessionID=@SessionID 
		where datepart(month,DOB)=@Month and datepart(day,DOB)=@Day and SS.SessionID=@SessionID 
		and SM.SBranchID=@SBranchID 
				
	end
end
GO
ALTER procedure [dbo].[spn_GetRecieverListForSMS] 
(
@SBranchID int,
@Recievers nvarchar(40),
@RecieverType int
)
as 
begin 
	if(@RecieverType=0)
	begin
		if(@Recievers='0')
		begin
			select ParentID as ID,FatherName as Name,FatherMobileNo as MobileNo, 4 as RecieverType, AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4
			where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1) and isnull(NotificationSMSTo,0)=0) and R.SBranchID=@SBranchID
			union 
			select ParentID as ID,MotherName as Name,MotherMobileNo as MobileNo, 4 as RecieverType, AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4
			where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1) and isnull(NotificationSMSTo,0)=1) and R.SBranchID=@SBranchID
			Union
			Select StudentID as ID,GuardianName as Name,GuardianMobileNo as MobileNo,6 as RecieverType, AU.FCMToken as deviceToken from StudentMaster R left outer join AppUsers AU on R.StudentID=AU.UserID and AU.UserType=6
			where StudentID in (select StudentID from Student_Session where Status=1) and isnull(NotificationSMSTo,0)=2  and R.SBranchID=@SBranchID
			union 
			Select EmployeeID as ID,EmployeeName as Name,MobileNumber as MobileNo,3 as RecieverType , AU.FCMToken as deviceToken from EmployeeMaster R left outer join AppUsers AU on R.EmployeeID=AU.UserID and AU.UserType=3
			where R.Status=1 and R.SBranchID=@SBranchID
		end
		else
		begin
			select ParentID as ID,FatherName as Name,FatherMobileNo as MobileNo, 4 as RecieverType , AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4
			where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1 and ClassID in (select Item from dbo.SplitStringToTable(@Recievers,','))) and isnull(NotificationSMSTo,0)=0) and R.SBranchID=@SBranchID
			union 
			select ParentID as ID,MotherName as Name,MotherMobileNo as MobileNo, 4 as RecieverType , AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4
			where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1 and ClassID in (select Item from dbo.SplitStringToTable(@Recievers,','))) and isnull(NotificationSMSTo,0)=1) and R.SBranchID=@SBranchID
			Union
			Select StudentID as ID,GuardianName as Name,GuardianMobileNo as MobileNo,6 as RecieverType , AU.FCMToken as deviceToken from StudentMaster R left outer join AppUsers AU on R.StudentID=AU.UserID and AU.UserType=6
			where StudentID in (select StudentID from Student_Session where Status=1 and ClassID in (select Item from dbo.SplitStringToTable(@Recievers,','))) and isnull(NotificationSMSTo,0)=2  and R.SBranchID=@SBranchID
			union 
			Select EmployeeID as ID,EmployeeName as Name,MobileNumber as MobileNo,3 as RecieverType , AU.FCMToken as deviceToken from EmployeeMaster R left outer join AppUsers AU on R.EmployeeID=AU.UserID and AU.UserType=3
			where R.Status=1 and R.SBranchID=@SBranchID
		end
	end 
	else if(@RecieverType=1)
	begin
		Select EmployeeID as ID,EmployeeName as Name,MobileNumber as MobileNo,3 as RecieverType , AU.FCMToken as deviceToken from EmployeeMaster R left outer join AppUsers AU on R.EmployeeID=AU.UserID and AU.UserType=3
		where R.Status=1 and R.SBranchID=@SBranchID
	end
	else if(@RecieverType=3)
	begin
	if(@Recievers='0')
		begin
			select ParentID as ID,FatherName as Name,FatherMobileNo as MobileNo, 4 as RecieverType , AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4 
			where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1) and isnull(NotificationSMSTo,0)=0) and R.SBranchID=@SBranchID
			union 
			select ParentID as ID,MotherName as Name,MotherMobileNo as MobileNo, 4 as RecieverType , AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4
			where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1) and isnull(NotificationSMSTo,0)=1) and R.SBranchID=@SBranchID
			Union
			Select StudentID as ID,GuardianName as Name,GuardianMobileNo as MobileNo,6 as RecieverType , AU.FCMToken as deviceToken from StudentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=6 
			where StudentID in (select StudentID from Student_Session where Status=1) and isnull(NotificationSMSTo,0)=2  and R.SBranchID=@SBranchID
			
		end
		else
		begin
			select ParentID as ID,FatherName as Name,FatherMobileNo as MobileNo, 4 as RecieverType , AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4
			where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1 and ClassID in (select Item from dbo.SplitStringToTable(@Recievers,','))) and isnull(NotificationSMSTo,0)=0) and R.SBranchID=@SBranchID
			union 
			select ParentID as ID,MotherName as Name,MotherMobileNo as MobileNo, 4 as RecieverType , AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4 
			where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1 and ClassID in (select Item from dbo.SplitStringToTable(@Recievers,','))) and isnull(NotificationSMSTo,0)=1) and R.SBranchID=@SBranchID
			Union
			Select StudentID as ID,GuardianName as Name,GuardianMobileNo as MobileNo,6 as RecieverType , AU.FCMToken as deviceToken from StudentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=6
			where StudentID in (select StudentID from Student_Session where Status=1 and ClassID in (select Item from dbo.SplitStringToTable(@Recievers,','))) and isnull(NotificationSMSTo,0)=2  and R.SBranchID=@SBranchID
			
		end	
	end
	else if(@RecieverType=4)--Specifically for Holiday
	begin
		declare @HolidayID int =cast(@Recievers as int)
		declare @IsEmployee int 
		declare @IsStudents int 
		declare @Classes nvarchar(500)
		declare @AssociatedIDs nvarchar(500)
		select @IsEmployee=isnull(IsEmployee,1),@Classes=Classes,@IsStudents=IsStudents,@AssociatedIDs=AssociatedIDs from HolidayMaster where HolidayID=@HolidayID
		declare @RecieverList  table (ID int,Name nvarchar(50),MobileNumber nvarchar(50),RecieverType int,deviceToken nvarchar(500))
		if(@IsEmployee=1)
		begin
			if((select count(*) from [dbo].[SplitStringToTable](@AssociatedIDs,',') where Item=0)>0)
			begin
				insert into @RecieverList
				Select EmployeeID as ID,EmployeeName as Name,MobileNumber as MobileNo,3 as RecieverType , AU.FCMToken as deviceToken from EmployeeMaster R left outer join AppUsers AU on R.EmployeeID=AU.UserID and AU.UserType=3
				where R.Status=1 and R.SBranchID=@SBranchID
			end
			else
			begin
				insert into @RecieverList
				Select EmployeeID as ID,EmployeeName as Name,MobileNumber as MobileNo,3 as RecieverType , AU.FCMToken as deviceToken from EmployeeMaster R left outer join AppUsers AU on R.EmployeeID=AU.UserID and AU.UserType=3
				where R.Status=1 and R.SBranchID=@SBranchID and EmployeeType in (select Item from [dbo].[SplitStringToTable](@AssociatedIDs,','))
			end
		end
		if(@IsStudents=1)
		begin
			if((select count(*) from [dbo].[SplitStringToTable](@Classes,',') where Item=0)>0)
			begin
				insert into @RecieverList
				select ParentID as ID,FatherName as Name,FatherMobileNo as MobileNo, 4 as RecieverType , AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4 
				where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1) and isnull(NotificationSMSTo,0)=0) and R.SBranchID=@SBranchID
				union 
				select ParentID as ID,MotherName as Name,MotherMobileNo as MobileNo, 4 as RecieverType , AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4
				where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1) and isnull(NotificationSMSTo,0)=1) and R.SBranchID=@SBranchID
				Union
				Select StudentID as ID,GuardianName as Name,GuardianMobileNo as MobileNo,6 as RecieverType , AU.FCMToken as deviceToken from StudentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=6 
				where StudentID in (select StudentID from Student_Session where Status=1) and isnull(NotificationSMSTo,0)=2  and R.SBranchID=@SBranchID
			end
			else
			begin
				insert into @RecieverList
				select ParentID as ID,FatherName as Name,FatherMobileNo as MobileNo, 4 as RecieverType , AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4
				where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1 and ClassID in (select Item from [dbo].[SplitStringToTable](@Classes,','))) and isnull(NotificationSMSTo,0)=0) and R.SBranchID=@SBranchID
				union 
				select ParentID as ID,MotherName as Name,MotherMobileNo as MobileNo, 4 as RecieverType , AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4 
				where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1 and ClassID in (select Item from [dbo].[SplitStringToTable](@Classes,','))) and isnull(NotificationSMSTo,0)=1) and R.SBranchID=@SBranchID
				Union
				Select StudentID as ID,GuardianName as Name,GuardianMobileNo as MobileNo,6 as RecieverType , AU.FCMToken as deviceToken from StudentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=6
				where StudentID in (select StudentID from Student_Session where Status=1 and ClassID in (select Item from [dbo].[SplitStringToTable](@Classes,','))) and isnull(NotificationSMSTo,0)=2  and R.SBranchID=@SBranchID
		
			end
		end
		select * from @RecieverList
	end
	else if(@RecieverType=5)--Specially for Event
	begin
	if(@Recievers='0')
		begin
			select ParentID as ID,FatherName as Name,FatherMobileNo as MobileNo, 4 as RecieverType , AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4 
			where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1) and isnull(NotificationSMSTo,0)=0) and R.SBranchID=@SBranchID
			union 
			select ParentID as ID,MotherName as Name,MotherMobileNo as MobileNo, 4 as RecieverType , AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4
			where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1) and isnull(NotificationSMSTo,0)=1) and R.SBranchID=@SBranchID
			Union
			Select StudentID as ID,GuardianName as Name,GuardianMobileNo as MobileNo,6 as RecieverType , AU.FCMToken as deviceToken from StudentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=6 
			where StudentID in (select StudentID from Student_Session where Status=1) and isnull(NotificationSMSTo,0)=2  and R.SBranchID=@SBranchID
			union 
			Select EmployeeID as ID,EmployeeName as Name,MobileNumber as MobileNo,3 as RecieverType , AU.FCMToken as deviceToken from EmployeeMaster R left outer join AppUsers AU on R.EmployeeID=AU.UserID and AU.UserType=3
			where R.Status=1 and R.SBranchID=@SBranchID
			
		end
		else
		begin
			select ParentID as ID,FatherName as Name,FatherMobileNo as MobileNo, 4 as RecieverType , AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4
			where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1 and ClassID in (select Item from dbo.SplitStringToTable(@Recievers,','))) and isnull(NotificationSMSTo,0)=0) and R.SBranchID=@SBranchID
			union 
			select ParentID as ID,MotherName as Name,MotherMobileNo as MobileNo, 4 as RecieverType , AU.FCMToken as deviceToken from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4 
			where ParentID in (Select ParentID from StudentMaster where StudentID in (select StudentID from Student_Session where Status=1 and ClassID in (select Item from dbo.SplitStringToTable(@Recievers,','))) and isnull(NotificationSMSTo,0)=1) and R.SBranchID=@SBranchID
			Union
			Select StudentID as ID,GuardianName as Name,GuardianMobileNo as MobileNo,6 as RecieverType , AU.FCMToken as deviceToken from StudentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=6
			where StudentID in (select StudentID from Student_Session where Status=1 and ClassID in (select Item from dbo.SplitStringToTable(@Recievers,','))) and isnull(NotificationSMSTo,0)=2  and R.SBranchID=@SBranchID
			union 
			Select EmployeeID as ID,EmployeeName as Name,MobileNumber as MobileNo,3 as RecieverType , AU.FCMToken as deviceToken from EmployeeMaster R left outer join AppUsers AU on R.EmployeeID=AU.UserID and AU.UserType=3
			where R.Status=1 and R.SBranchID=@SBranchID			
		end	
	end
end


GO
ALTER procedure [dbo].[spn_GetSearchedStudents]
(
@SearchText nvarchar(20),
@SBranchID int,
@SessionID int=0
)
as 
begin 
	if(@SessionID=0)
	begin
		Select @SessionID=SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
	end
	set @SearchText=upper(@SearchText)
	if(@SearchText<>'')
	begin
		Select SM.StudentID,'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6) as StudentSID, Name,DOB, EmailID, BloodGroup, GuardianName, GuardianMobileNo, Photo,SS.RollNo,SM.AccessCardNo,
		SM.GuardianMobileNo ,PM.FatherMobileNo,PM.MotherMobileNo,SM.Gender,SS.ClassID,SS.SectionID ,SM.AadharCardNo,PM.FatherName,PM.MotherName,SM.SchoolUID,SM.SSSID,SM.FamilyID,
		 SM.MiniAddress, SM.MiniAddress2,
		isnull((Select ClassName from ClassMaster CM where CM.ClassID=SS.ClassID),'NA') as ClassName,
		isnull((Select Name from Class_Sections CS where CS.ID=SS.SectionID),'NA') as SectionName
		from StudentMaster SM left outer join Student_Session SS on SS.StudentID=SM.StudentID and SS.Status=1 and SS.SessionID=@SessionID
		left outer join Parentmaster PM on PM.ParentID=SM.ParentID
		where SM.SBranchID=@SBranchID and SS.SessionID=@SessionID and
		(upper(SM.Name) like '%'+@SearchText+'%' or 'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6) like  '%'+@SearchText+'%' or upper(SM.Name) like '%'+@SearchText+'%' or upper(SS.RollNo) like '%'+@SearchText+'%' or upper(PM.FatherName) like '%'+@SearchText+'%' or upper(PM.MotherName) like '%'+@SearchText+'%' or upper(SM.SSSID) like '%'+@SearchText+'%' or upper(SM.FamilyID) like '%'+@SearchText+'%' or upper(SM.AadharCardNo) like '%'+@SearchText+'%'   or upper(SM.SchoolUID) like '%'+@SearchText+'%')
		order by Name
	end
	else
	begin		
	Select top 50 SM.StudentID,'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6) as StudentSID, Name,DOB, EmailID, BloodGroup, GuardianName, GuardianMobileNo, Photo,SS.RollNo,SM.AccessCardNo,
		SM.GuardianMobileNo ,PM.FatherMobileNo,PM.MotherMobileNo,SM.Gender,SS.ClassID,SS.SectionID ,SM.AadharCardNo,PM.FatherName,PM.MotherName,SM.SchoolUID,SM.SSSID,SM.FamilyID,
		SM.MiniAddress, SM.MiniAddress2,
		isnull((Select ClassName from ClassMaster CM where CM.ClassID=SS.ClassID),'NA') as ClassName,
		isnull((Select Name from Class_Sections CS where CS.ID=SS.SectionID),'NA') as SectionName
		from StudentMaster SM left outer join Student_Session SS on SS.StudentID=SM.StudentID and SS.Status=1  and SS.SessionID=@SessionID
		left outer join Parentmaster PM on PM.ParentID=SM.ParentID
		where SM.SBranchID=@SBranchID  and SS.SessionID=@SessionID
		order by Name
	end

	Select SessionName as Name,SessionID as ID,SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID
	
	select @SessionID

end

GO
GO
Create proc [dbo].[spn_GetStudentFullTimeTable]
(
@StudentID int,
@CurrDate date,
@SBranchID int
)
AS
BEGIN
Declare @SectionID int
declare @EduLevID int
declare @GroupID int
declare @ClassID int
select @SectionID=SectionID from Student_Session where StudentID=@StudentID and Status=1
Select @ClassID=ClassID,@GroupID=GroupID from Class_Sections where ID=@SectionID
Select @EduLevID=EducationLevelID,@SBranchID=SBranchID from ClassMaster where ClassID= (@ClassID)

Select [Days],@SectionID as SectionID,@EduLevID as EducationLevelID,@ClassID as ClassID from EducationLevelMaster where ID=@EduLevID

Select PeriodID,PeriodType,Name,cast(StartTime as nvarchar(10)) as StartTime,cast(EndTime as nvarchar(10)) as EndTime,
(Select PeriodTypeName from  PeriodTypeMaster PTM where PTM.PeriodTypeID=PM.PeriodType) as PeriodTypeName
from  PeriodMaster PM where EducationLevelID=@EduLevID
order by StartTime

Select SubjectID,SubjectName,IsOptionalsubject from SubjectMasterAll SM where GroupID=@GroupID and [Status]=1

	SELECT
      TTM.PeriodID , 
	  TTM.EducationLevelID
      ,TTM.ClassID
      ,TTM.SectionID
      ,isnull(TTM.MondaySubjectID,0) as 'SubjectID1'
      ,isnull(TTM.MondayTeacherID,0) as  'TeacherID1'
	  ,isnull((Select EmployeeName from EmployeeMaster EM where EM.EmployeeID=TTM.MondayTeacherID),'No Teacher')  as 'TeacherName1'
     
      ,isnull(TTM.TuesdaySubjectID,0) as 'SubjectID2'
      ,isnull(TTM.TuesdayTeacherID,0) as  'TeacherID2'
	  ,isnull((Select EmployeeName from EmployeeMaster EM where EM.EmployeeID=TTM.TuesdayTeacherID),'No Teacher')  as 'TeacherName2'

      ,isnull(TTM.WednesdaySubjectID,0) as 'SubjectID3'
      ,isnull(TTM.WednesdayTeacherID,0) as  'TeacherID3'
	  ,isnull((Select EmployeeName from EmployeeMaster EM where EM.EmployeeID=TTM.WednesdayTeacherID),'No Teacher')  as 'TeacherName3'

      ,isnull(TTM.ThursdaySubjectID,0) as 'SubjectID4'
      ,isnull(TTM.ThursdayTeacherID,0) as  'TeacherID4'
	  ,isnull((Select EmployeeName from EmployeeMaster EM where EM.EmployeeID=TTM.ThursdayTeacherID),'No Teacher')  as 'TeacherName4'

      ,isnull(TTM.FridaySubjectID,0) as 'SubjectID5'
      ,isnull(TTM.FridayTeacherID,0) as  'TeacherID5'
	  ,isnull((Select EmployeeName from EmployeeMaster EM where EM.EmployeeID=TTM.FridayTeacherID),'No Teacher')  as 'TeacherName5'

      ,isnull(TTM.SaturdaySubjectID,0) as 'SubjectID6'
      ,isnull(TTM.SaturdayTeacherID,0) as  'TeacherID6'
	  ,isnull((Select EmployeeName from EmployeeMaster EM where EM.EmployeeID=TTM.SaturdayTeacherID),'No Teacher') as 'TeacherName6'

	FROM Time_Table_Master TTM 
	where TTM.EducationLevelID=@EduLevID and TTM.SectionID=@SectionID and TTM.ClassID=@ClassID
	
	Select PeriodID,DayID
	from TeacherSubstitutionMaster TSM where ClassID=@ClassID and SectionID=@SectionID and SBranchID=@SBranchID
	and (FromDate between DATEADD(day,-2,@CurrDate) and DATEADD(day,7,@CurrDate) )


	Select DayID,PeriodID, (Select ClassName +' -> '+SectionName from [dbo].[v_ClassSectionNames] where SectionID=FirstSectionID) as FirstClassSection,
	(Select SubjectName from SubjectMaster SM where SM.SubjectID=CMM.SubjectID) as SubjectName,
	(Select EmployeeName from EmployeeMaster EM where EM.EmployeeID=CMM.TeacherID) as TeacherName
	from Class_Merge_Master CMM where ( SecondSectionID=@SectionID) 

END

GO
ALTER procedure [dbo].[spn_GetStudentsByClassSection]
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
		SM.HostelRoomID,SM.StopID,SM.Gender,SM.SchoolUID,MiniAddress,HM.Name as HouseName
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
ALTER procedure [dbo].[spn_GetStudentsDetails]
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
      ,[CategoryCertificate]      ,[Photo]      ,[TransferCertificate]	,VehicleRouteID,StopID,HostelRoomID,NotificationSMSTo,SchoolUID,AadharCardNo
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
	declare @AStateID int
	declare @ACountryID int

	select @ACityID=isnull(Cadd_DistrictCode,0),@AStateID=Cadd_StateCode,@AAreaID=Cadd_AreaCode,@ACountryID=Cadd_CountryCode from StudentMaster where StudentID=@StudentID

	if(@ACityID=0)
	begin
		select @ACountryID=min(countryID) from CountryMaster
		select @AStateID=min(StateID) from StateMaster where CountryID=@ACountryID
		select @ACityID=min(CityID) from CityMaster where StateID=@AStateID
		select @AAreaID=min(AreaID) from AreaMaster where CityID=@ACityID
	end
	select CountryID as ID, CountryName as Name from CountryMaster
	select StateID as ID, StateName as Name from StateMaster where CountryID =@ACountryID
	select CityID as ID, CityName as Name from CityMaster where StateID =@AStateID
	select AreaID as ID, AreaName as Name from AreaMaster where CityID = @ACityID


  select 
  SS.StudentSessionUID,  SS.StudentID,   SS.ClassID	  ,SS.SectionID	  ,SS.QuotaID	  ,SS.FromDate	  ,SS.ToDate
	  ,SS.Status,	  SS.RollNo,SS.FeePaymentMode
	  ,(Select QuotaName from QuotaMaster QM where QM.QuotaID=SS.QuotaID) as QuotaName
	  ,(Select ClassName from ClassMaster CM where CM.ClassID=SS.ClassID) as ClassName
	  ,(Select Name from Class_Sections CS where CS.ID=SS.SectionID) as SectionName,
	  (Select Name from HouseMaster HM where HM.ID=SS.HouseID) as HouseName
	  ,(Select SessionName from SessionMaster CS where CS.SessionID=SS.SessionID) as SessionName
	  from Student_Session SS where SS.StudentID=@StudentID and SS.SBranchID=@SBranchID
	  order by Status desc
	  
	exec [dbo].[spn_GetStudentsTransportDetails] @StudentID,@SBranchID	

	exec [dbo].[spn_GetStudentsHostelDetails] @StudentID,@SBranchID
		
	Declare @SessionID int,@SectionID int,@QuotaID int,@ClassID int,@IsAdmissionFeeApplicable int,@GroupID int
	select top 1 @SessionID=SessionID,@QuotaID=QuotaID,@ClassID=ClassID,@SectionID=SectionID,@IsAdmissionFeeApplicable=IsAdmissionFeeApplicable
	from Student_Session where StudentID=@StudentID order by Status desc,FromDate desc

	select @GroupID=GroupID from Class_Sections where ID=@SectionID
	
	declare @TransportFeeMode int=0
	Select @TransportFeeMode=details from MasterSettings where Type='TransportFeeMode' and SBranchID=@SBranchID

	Select @SessionID as SessionID,SFD.SFID,SFD.IsApplicable,FTM.FeeTypeID,FTM.FeeTypeName,isnull(SFD.FeeAmount,(CFS.FeeAmount-CFS.FeeAmount*isnull(QD.DiscPer,0)/100)) as FeeAmount,
	(CFS.FeeAmount-CFS.FeeAmount*isnull(QD.DiscPer,0)/100) as ClassFee,FTM.FeeTypeApplicable
	from FeeTypeMaster FTM left outer join [ClassFeeStructureMaster] CFS on CFS.FeeTypeID=FTM.FeeTypeID and CFS.SessionID=@SessionID and CFS.ClassID=@ClassID and CFS.GroupID=@GroupID
	left outer join StudentFeeDetails SFD on SFD.FeeTypeID=FTM.FeeTypeID and SFD.SessionID=@SessionID and SFD.StudentID=@StudentID
	left outer join [QuotaDiscountDetails] QD on QD.QuotaID=@QuotaID and QD.FeeTypeID=FTM.FeeTypeID and QD.SessionID=@SessionID
	where FTM.SBranchID=@SBranchID and FTM.FeeTypeID not in (-1,-2) and FTM.FeeTypeApplicable <>(case when @TransportFeeMode=0 then 5 else 0 end)

  end

GO
CREATE  procedure [dbo].[spn_GetStudentsDetailsNew]
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
	  ,SSSID,FamilyID,BankName,AccountNumber,IFSCCode,BranchName,PSchoolName,PSchoolMedium,PClassName,PSResult,PSchoolCity,PSchoolState
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
CREATE procedure [dbo].[spn_GetStudentsTransportDetailsNew] --244,1,0
(
@StudentID int,
@SBranchID int,
@THChangeID int
)
as 
begin 

	declare @AreaID int
	declare @CityID int
	declare @StopID int
	declare @VehicleRouteID int
	declare @RouteID int
	declare @Applicable int

	if(@THChangeID=0)
	begin 
		select top 1 @CityID=CItyID from CityMaster where SBranchID=@SBranchID and IsApproved=1 order by CityID
		select @StopID=StopID,@AreaID=AreaID from [TransportRouteDetails] where CityID=@CItyID
		Select @VehicleRouteID=VehicleRouteID from StudentMaster where StudentID=@StudentID
	end
	else
	begin
		select @StopID=isnull(KeyID,0) from [TransportHostalAllocationDelocation] where THChangeID=@THChangeID		
		select @CityID=CityID,@AreaID=AreaID from [TransportRouteDetails] where StopID=@StopID
		Select @VehicleRouteID=VehicleRouteID from StudentMaster where StudentID=@StudentID
	end
	if(@StopID=0)
	begin
		set @Applicable=0
		select @StopID=StopID from [dbo].[TransportRouteDetails] where AreaID=(Select Cadd_AreaCode from StudentMaster where StudentID=@StudentID)
		select @AreaID=Cadd_AreaCode,@CityID=Cadd_DistrictCode from StudentMaster where StudentID=@StudentID

		Select @VehicleRouteID=min(VehicleRouteID) from Transport_Vehicle_Route 
		where RouteID in (select RouteID from TransportRouteDetails where StopID = @StopID)
	END
	else
	begin	
		set @Applicable=1
		Select @AreaID =AreaID from [dbo].[TransportRouteDetails] where StopID=@StopID
		select @CityID= CityID from AreaMaster where AreaID=@AreaID
	end
	if(@VehicleRouteID=0)
	begin		
		Select @VehicleRouteID=min(VehicleRouteID) from Transport_Vehicle_Route 
		where RouteID in (select RouteID from TransportRouteDetails where StopID = @StopID)
	end
	
	Select @RouteID =[RouteID] from [dbo].[Transport_Vehicle_Route] where [VehicleRouteID]=@VehicleRouteID	
	print(@RouteID)
	--Location DropDown Selected Values
	Select @StopID as StopID, @AreaID as AreaID,@CityID as CityID, @VehicleRouteID as VehicleRouteID,@RouteID as RouteID,@Applicable as Applicable
	--Location DropDowns For Transport
	select CityID as ID, CityName as Name from CityMaster where SBranchID =@SBranchID
	select AreaID as ID, AreaName as Name from AreaMaster where CityID = @CityID
	
	Select RouteID as ID,RouteName as Name from TransportRouteMaster 
	where IsApproved=1 and RouteID in (select RouteID from TransportRouteDetails where AreaID = @AreaID)

	exec spn_GetTransportRouteVehicleList @SBranchID,@RouteID

	exec spn_GetVehicleRouteStoppages @VehicleRouteID

	select * from [TransportHostalAllocationDelocation] where THChangeID=@THChangeID

	end

GO
CREATE procedure [dbo].[spn_GetStudentsTransportDetailsNew2] --244,1,0
(
@StudentID int,
@SBranchID int,
@THChangeID int
)
as 
begin 
	declare @StopID int
	declare @VehicleRouteID int
	declare @RouteID int
	declare @Applicable int

	if(@THChangeID=0)
	begin 
		select top 1 @RouteID=RouteID,@VehicleRouteID=VehicleRouteID from Transport_Vehicle_Route where SBranchID=@SBranchID and RouteID in
		(Select RouteID from TransportRouteMaster where IsApproved=1)
		select top 1 @StopID=StopID from [TransportRouteDetails] where RouteID=@RouteID
	end
	else
	begin
		select @StopID=isnull(KeyID,0),@VehicleRouteID=VehicleRouteID from [TransportHostalAllocationDelocation] where THChangeID=@THChangeID	
		if(isnull(@VehicleRouteID,0)=0)
		begin
			Select @VehicleRouteID=VehicleRouteID from StudentMaster where StudentID=@StudentID
		end
		select @RouteID=RouteID from Transport_Vehicle_Route where VehicleRouteID=@VehicleRouteID
	end
	if(@StopID=0)
	begin
		set @Applicable=0
	END
	else
	begin	
		set @Applicable=1
	end
	--Location DropDown Selected Values
	Select @StopID as StopID,@VehicleRouteID as VehicleRouteID,@RouteID as RouteID,@Applicable as Applicable
	
	
	Select RouteID as ID,RouteName as Name from TransportRouteMaster 
	where IsApproved=1 --and RouteID in (select RouteID from TransportRouteDetails where AreaID = @AreaID)

	exec spn_GetTransportRouteVehicleList @SBranchID,@RouteID

	exec spn_GetVehicleRouteStoppages @VehicleRouteID

	select * from [TransportHostalAllocationDelocation] where THChangeID=@THChangeID

end
GO
ALTER proc [dbo].[spn_GetTransportRouteStopDetails]
(
@StopID int,
@RouteID int,
@SBranchID int
)
AS
BEGIN
	declare @AreaID int
	declare @CityID int
	
	 if(@StopID=0)
	 begin
		select @CityID=min(CityID) from CityMaster where isApproved=1 and SBranchID=@SBranchID
		
		select @AreaID=min(AreaID) from AreaMaster where isApproved=1 and CityID=@CityID

		Select 0 as StopID,@RouteID as RouteID,@CityID,@AreaID,avg(Rate) as Rate,
		cast(DATEADD(MINUTE, 10, max([Time])) as nvarchar(8)) as [Time],avg(HaltDuration) as HaltDuration,avg(HaltDurationR) as HaltDurationR,
		max(SequenceNo)+1 as SequenceNo,1 as SequenceNoR,cast(DATEADD(MINUTE, -10, min([TimeR])) as nvarchar(8)) as TimeR
		from TransportRouteDetails TRD where RouteID=@RouteID
	 end
	 else
	 begin
	 	Select @CityID=CityID,@AreaID=AreaID
		from TransportRouteDetails TRD where StopID=@StopID
		
		Select StopID,RouteID,CityID,AreaID,Rate,
		cast([Time] as nvarchar(8)) as [Time],HaltDuration,HaltDurationR,SequenceNo,SequenceNoR,cast([TimeR] as nvarchar(8)) as TimeR
		 from TransportRouteDetails TRD where StopID=@StopID
	 end

	 Select AreaID as ID, AreaName as Name from AreaMaster where CityID=@CityID and isApproved=1 
	 select CityID as ID, CityName as Name from CityMaster where SBranchID=@SBranchID and isApproved=1 

END

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
CREATE procedure [dbo].[spn_InsertAdmissionEnquiryFollowup]
(
@FollowupID int,
@EnquiryID int,
@FollowupBy int,
@FollowupByName nvarchar(50),
@Remark nvarchar(MAX),
@FDate datetime,
@Status int,
@Possibility int,
@NextFollowupDate datetime,
@CreatedDate datetime
)
as 
begin 
		insert into AdmissionEnquiryFollowups(EnquiryID,FollowupBy,FollowupByName,Remark,FDate,Status,Possibility,NextFollowupDate,CreatedDate)
		values (@EnquiryID,@FollowupBy,@FollowupByName,@Remark,@FDate,@Status,@Possibility,@NextFollowupDate,@CreatedDate)
		
		update AdmissionEnquiryMaster set NextFollowUpDate=@NextFollowupDate where EnquiryID=@EnquiryID
		 SELECT  CAST(SCOPE_IDENTITY() as int)
end
GO
ALTER proc [dbo].[spn_InsertUpdateAdminLocation]
(
@ID int,
@Type int,
@Name nvarchar(50),
@Other nvarchar(50),
@MasterID int,
@IsApproved int,
@OpType int,
@SBranchID int
)
AS
BEGIN
	if(@Type=1)
	begin
		if(@OpType=-1)
		begin
			delete from CountryMaster where CountryID=@ID
		end
		else if(@ID=0)
		begin
			insert into CountryMaster(countryName,IsApproved)  values(@Name,@IsApproved)	
		end
		else
		begin
			Update CountryMaster set CountryName=@Name,IsApproved=@IsApproved where CountryID=@ID
		end
	end
	else if(@Type=2)
	begin
		if(@OpType=-1)
		begin
			delete from StateMaster where StateID=@ID
		end
		else if(@ID=0)
		begin
			insert into StateMaster(StateName,IsApproved,CountryID)  values(@Name,@IsApproved,@MasterID)	
		end
		else
		begin
			Update StateMaster set StateName=@Name,IsApproved=@IsApproved where StateID=@ID
		end
	end
	else if(@Type=3)
	begin
		if(@OpType=-1)
		begin
			delete from CityMaster where CityID=@ID
		end
		else if(@ID=0)
		begin
			insert into CityMaster(CityName,IsApproved,StateID,SBranchID)  values(@Name,@IsApproved,@MasterID,@SBranchID)	
		end
		else
		begin
			Update CityMaster set CityName=@Name,IsApproved=@IsApproved where CityID=@ID
		end
	end
	else if(@Type=4)
	begin
		if(@OpType=-1)
		begin
			delete from AreaMaster where AreaID=@ID
		end
		else if(@ID=0)
		begin
			insert into AreaMaster(AreaName,IsApproved,CityID,PinCode)  values(@Name,@IsApproved,@MasterID,@Other)	
		end
		else
		begin
			Update AreaMaster set AreaName=@Name,IsApproved=@IsApproved,PinCode=@Other where AreaID=@ID
		end
	end
	exec sp_GetAdminLocationList @SBranchID,@MasterID,@Type
END

GO
ALTER proc [dbo].[spn_InsertUpdateClass]
(
@ClassName nvarchar(50),
@EducationLevelID int,
@Status bit,
@UserID int,
@OperationDate Datetime,
@SBranchID int,
@ClassID int,
@OpType int,
@SessionID int=0
)
AS
BEGIN
	if(@SessionID=0)
	begin
		Select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and SessionStatus=1
	end
	if(@OpType=-1)
	begin
		delete from ClassMaster where ClassID=@ClassID
		delete from Class_Sections where ClassID=@ClassID
		delete from ClassSessionDetails where ClassID = @ClassID --and SessionID=@SessionID
	end
	else if(@ClassID=0)
	begin
		Insert into ClassMaster(ClassName,EducationLevelID,[Status],UserID,CreatedDate,SBranchID) 
		values (@ClassName,@EducationLevelID,@Status,@UserID,@OperationDate,@SBranchID)
		SELECT @ClassID= CAST(SCOPE_IDENTITY() as int)
	end
	else
	begin
		Update ClassMaster set ClassName=@ClassName, EducationLevelID=@EducationLevelID, [Status]=@Status,
		UserID=@UserID,ModifiedDate=@OperationDate
		where ClassID=@ClassID
	end
	select isnull(@ClassID,0)
END
GO
ALTER procedure [dbo].[spn_InsertUpdateEmployeeBasicDetails]
(
@EmployeeID int,
@EmployeeName nvarchar(50),
@DOB date,
@Gender int,
@Nationality int,
@ReligionID int,
@DOJ date,
@EmailID nvarchar(100),
@CategoryID int,
@MaritalStatus int,
@BloodGroup nvarchar(5),
@MotherTongueID int,
@PassportNo nvarchar(20),
@MobileNo nvarchar(12),
@PhoneNo nvarchar(20),
@AccessCardNo nvarchar(20),
@FatherHubName nvarchar(20),
@SBranchID int,
@OperationDate datetime,
@ImageName nvarchar(100),
@Status int,
@EmployeeType int,
@Password nvarchar(200),
@AadharNumber nvarchar(50),
@LicenceNumber nvarchar(50)
)
as 
begin 

	declare @EmployeeSID nvarchar(20)	
	if(@EmployeeID=0)
	begin
		insert into [dbo].[EmployeeMaster](EmployeeName,DOB,Gender,Nationality,ReligionID,DOJ,EmailID,CategoryID,MaritalStatus,BloodGroup,
		 MotherTongueID,PassportNo,MobileNumber,LandLineNumber,AccessCardNo,FatherHubName,SBranchID,CreatedDate,
		 Photo,[Status],EmployeeType,DesignationID,LicenceNumber,AadharNumber)
		 values(@EmployeeName,@DOB,@Gender,@Nationality,@ReligionID,@DOJ,@EmailID,@CategoryID,@MaritalStatus,@BloodGroup,
		 @MotherTongueID,@PassportNo,@MobileNo,@PhoneNo,@AccessCardNo,@FatherHubName,@SBranchID,@OperationDate,@ImageName,@Status,
		 @EmployeeType,@EmployeeType,@LicenceNumber,@AadharNumber)

		 SELECT @EmployeeID= CAST(SCOPE_IDENTITY() as int)
		
		 
		 --if(@EmployeeType=1)
		 -- Change for Reception Login Credential
		 if(@EmployeeType in (3,10,5,8,12))
		 begin
	
			 select @EmployeeSID='EMP'+RIGHT(REPLICATE('0',6)+CAST(@EmployeeID AS VARCHAR(6)),6) 
	   
			 insert into LoginDetails (UserName,Password,EmailID,UserID,Usertype,SBranchID)
			 values(@EmployeeSID,@Password,@EmailID,@EmployeeID,3,@SBranchID)
		end

		 declare @EmployeeAID nvarchar(20)

		 select @EmployeeAID='EMP'+RIGHT(REPLICATE('0',6)+CAST(@EmployeeID AS VARCHAR(6)),6) 
		set @AccessCardNo =isnull(@AccessCardNo,'0000')
		 exec sp_AddUserToEsslDevice @EmployeeName,@EmployeeAID,@SBranchID,@AccessCardNo,@OperationDate

		
	end
	else
	begin

	declare @OldAccessCardNumber nvarchar(50)
	
		declare @OldEmployeeType int
		select @OldEmployeeType=EmployeeType,@OldAccessCardNumber= AccessCardNo from EmployeeMaster where EmployeeID=@EmployeeID
		Update [dbo].[EmployeeMaster] set EmployeeName=@EmployeeName,DOB=@DOB,Gender=@Gender,Nationality=@Nationality
		,ReligionID=@ReligionID,DOJ=@DOJ,EmailID=@EmailID,CategoryID=@CategoryID,MaritalStatus=@MaritalStatus,BloodGroup=@BloodGroup,
		 MotherTongueID=@MotherTongueID,PassportNo=@PassportNo,MobileNumber=@MobileNo,LandLineNumber=@PhoneNo,AccessCardNo=@AccessCardNo
		 ,FatherHubName=@FatherHubName,ModifiedDate=@OperationDate,
		 Photo=@ImageName,[Status]=@Status,EmployeeType=@EmployeeType,DesignationID=@EmployeeType
		 ,LicenceNumber=@LicenceNumber,AadharNumber=@AadharNumber
		  where EmployeeID=@EmployeeID

		 if(@OldEmployeeType<>@EmployeeType)
		 begin
			if(@OldEmployeeType  in (3,10,5,8,12) and @EmployeeType not in (3,10,5,8,12))
			begin
				delete from LoginDetails where UserID=@EmployeeID and UserType=@EmployeeType
			end
			else if(@EmployeeType in (3,10,5,8,12) and @OldEmployeeType not in (3,10,5,8,12))
			begin
	
				 select @EmployeeSID='EMP'+RIGHT(REPLICATE('0',6)+CAST(@EmployeeID AS VARCHAR(6)),6) 
	   
				 insert into LoginDetails (UserName,Password,EmailID,UserID,Usertype,SBranchID)
				 values(@EmployeeSID,@Password,@EmailID,@EmployeeID,@EmployeeType,@SBranchID)
			end
		 end
		 if(isnull(@OldAccessCardNumber,'0000')<>@AccessCardNo)
		 begin
			 declare @EmployeeAUID nvarchar(20)
			 select @EmployeeAUID='EMP'+RIGHT(REPLICATE('0',6)+CAST(@EmployeeID AS VARCHAR(6)),6) 
			 exec sp_DeleteUserToEsslDevice @EmployeeAUID,@SBranchID,@OperationDate
			 set @OperationDate=DATEADD (minute , 5 , @OperationDate ) 
			 exec sp_AddUserToEsslDevice @EmployeeName,@EmployeeAUID,@SBranchID,@AccessCardNo,@OperationDate
		 end
	end
	SELECT @EmployeeID
end
GO
ALTER procedure [dbo].[spn_InsertUpdateEvaluationScheme]
(
@EvaluationSchemeID int,
@EvaluationSchemeName varchar(20),
@SBranchID int,
@SessionID int,
@ClassIDs nvarchar(500),
@OpType int
)
as 
begin 
	if(@OpType=-1)
	begin
		Delete from EvaluationSchemeMaster where EvaluationSchemeID=@EvaluationSchemeID
		delete from [ClassSessionDetails] where SchemeID=@EvaluationSchemeID
	end
	else if(@EvaluationSchemeID=0)
	begin
		Insert into EvaluationSchemeMaster(EvaluationSchemeName,SessionID,SBranchID)
		values(@EvaluationSchemeName,@SessionID,@SBranchID)
		select @EvaluationSchemeID =cast(Scope_Identity() as int)
		delete from [ClassSessionDetails] where SchemeID=@EvaluationSchemeID

		Insert into [ClassSessionDetails] (ClassID,SessionID,SchemeID) select Item,@SessionID,@EvaluationSchemeID from [dbo].[SplitStringToTable](@ClassIDs,',')
	end
	else
	begin
		Update EvaluationSchemeMaster set EvaluationSchemeName=@EvaluationSchemeName where EvaluationSchemeID=@EvaluationSchemeID

		delete from [ClassSessionDetails] where SchemeID=@EvaluationSchemeID

		Insert into [ClassSessionDetails] (ClassID,SessionID,SchemeID) select Item,@SessionID,@EvaluationSchemeID from [dbo].[SplitStringToTable](@ClassIDs,',')
	end
end
GO
CREATE proc [dbo].[spn_InsertUpdateHolidayNew]
(
@HolidayID int,
@Title nvarchar(500),
@Description nvarchar(max),
@DayDuration int,
@StartDate DateTime,
@EndDate DateTime,
@Status int,
@Classes nvarchar(500),
@AssociatedIDs nvarchar(500),
@HolidayType int,
@UserID int,
@OperationDate datetime,
@SBranchID int,
@OpType int,
@IsEmployee int=1,
@IsStudents int=1
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from  [dbo].[HolidayMaster] where HolidayID=@HolidayID
		delete from Holiday_Classes where HolidayID=@HolidayID
	end
	else if(@HolidayID=0)
	begin
		insert into [dbo].[HolidayMaster](Title,HolidayDescription,DayDuration,StartDate,EndDate,[Status],
		Classes,HolidayType,UserID,CreatedDate,SBranchID,IsEmployee,IsStudents,AssociatedIDs)
		values(@Title,@Description,@DayDuration,@StartDate,@EndDate,@Status,@Classes,@HolidayType,@UserID
		,@OperationDate,@SBranchID,@IsEmployee,@IsStudents,@AssociatedIDs) 
		SELECT @HolidayID= CAST(SCOPE_IDENTITY() as int)
	end
	else
	begin
		Update [dbo].[HolidayMaster] set Title=@Title,HolidayDescription=@Description,DayDuration=@DayDuration,StartDate=@StartDate,
		EndDate=@EndDate,[Status]=@Status,Classes=@Classes,HolidayType=@HolidayType,UserID=@UserID,ModifiedDate=@OperationDate,IsEmployee=@IsEmployee,
		IsStudents=@IsStudents,AssociatedIDs=@AssociatedIDs
		where HolidayID=@HolidayID
	end
	select @HolidayID
END
GO
CREATE proc [dbo].[spn_InsertUpdateLibraryRegister]
(
@IssueID int,
@BorrowerType int,
@BorrowerID int,
@IssueDate date,
@IssueDays int,
@LibraryID int,
@Remark nvarchar(500),
@Status int,
@SessionID int,
@CreatedDate date,
@IssuedBooks ut_LibraryIssueBooks READONLY
)
AS
BEGIN
	if(@IssueID=0)
	begin
		Insert into [dbo].[LibraryIssueRegister](BorrowerType,BorrowerID,IssueDate,LibraryID,CreatedDate,Remark,SessionID)
		values(@BorrowerType,@BorrowerID,@IssueDate,@LibraryID,@CreatedDate,@Remark,@SessionID)
		select @IssueID=CAST(SCOPE_IDENTITY() as int)
	end
	else
	begin
		Update [dbo].[LibraryIssueRegister] set BorrowerType=@BorrowerType,BorrowerID=@BorrowerID,IssueDate=@IssueDate
		,LibraryID=@LibraryID,Remark=@Remark,SessionID=@SessionID where IssueID=@IssueID
	end

	INSERT INTO  [dbo].LibraryIssueBooks (BookID, CopyID,IssueDays,IssueDate,LibraryIssueID,Status)
	SELECT BookID, CopyID,IssueDays,IssueDate,@IssueID,Status FROM @IssuedBooks PD where PD.BookIssueID=0

	UPDATE e SET   e.Status=d.Status
		FROM  [dbo].LibraryIssueBooks e, @IssuedBooks d 
		WHERE d.BookIssueID=e.BookIssueID and d.BookIssueID<>0 and d.OpType<>-1

	delete from [dbo].LibraryIssueBooks where BookIssueID in (select BookIssueID from @IssuedBooks where OpType=-1)

	select IssueID,BorrowerType,
		(case when BorrowerType=0 then (Select EmployeeName from EmployeeMaster where EmployeeID=BorrowerID) else
		(select Name from v_StudentCurrentClassName where StudentID=BorrowerID and SessionID=@SessionID) end) as BorrowerName,
		ClassID,[IssueDate],[IssuedDays],Remark,
		(Select Count(*) from [LibraryIssueBooks]where [LibraryIssueID]=IssueID and Status=1) as ReturnedBooks,
		(Select Count(*) from [LibraryIssueBooks]where [LibraryIssueID]=IssueID) as Books,Status
		 from [LibraryIssueRegister] where IssueID=@IssueID

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
@StateID int=0,
@PrincipalSignature nvarchar(50)
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
		,[PrincipalEmail],[BranchSchoolName],StateID,PrincipalSignature)
			values (@BranchName,@Logo,@ContactNo,@EmailID,@Address,@PrincipalName,@PrincipalMobile,@PrincipalEmail,@BranchSchoolName,@StateID,@PrincipalSignature)
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
			,[PrincipalName]=@PrincipalName,[PrincipalMobile]=@PrincipalMobile,StateID=@StateID,PrincipalSignature=@PrincipalSignature
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
ALTER procedure [dbo].[spn_InsertUpdateStudentBasicDetails]
(
@StudentID int,
@Name nvarchar(50),
@DOB date,
@Gender int,
@Nationality int,
@ReligionID int,
@DOJ date,
@EmailID nvarchar(100),
@Category int,
@Disability int,
@BloodGroup nvarchar(5),
@MotherTongueID int,
@PassportNo nvarchar(20),
@ExtraCurricular nvarchar(max),
@Allergic_Medicine nvarchar(max),
@SBranchID int,
@AdmissionBy int,
@OperationDate datetime,
@ImageName nvarchar(100),
@AccessCardNo nvarchar(50),
@Password nvarchar(50),
@NotificationSMSTo int=null,
@SchoolUID nvarchar(50)='',
@AadharCardNo nvarchar(100)='',
@SSSID nvarchar(50)='',
@FamilyID nvarchar(50)='',
@BankName nvarchar(100)='',
@AccountNumber nvarchar(50)='',
@IFSCCode nvarchar(50)='',
@BranchName nvarchar(100)='',
@PSchoolName nvarchar(max)='',
@PSchoolMedium nvarchar(100)='',
@PClassName nvarchar(100)='',
@PSResult nvarchar(100)='',
@PSchoolCity nvarchar(100)='',
@PSchoolState nvarchar(100)=''
)
as 
begin 
if(@StudentID=0)
begin
	insert into [dbo].[StudentMaster](Name,DOB,Gender,Nationality,ReligionID,DOJ,EmailID,Category,Disability,BloodGroup,
	 MotherTongueID,PassportNo,ExtraCurricular,Allergic_Medicine,SBranchID,AdmissionBy,CreatedDate,Photo,AccessCardNo,NotificationSMSTo,SchoolUID,AadharCardNo,SSSID,FamilyID,
	 BankName,AccountNumber,IFSCCode,BranchName,PSchoolName,PSchoolMedium,PClassName,PSResult,PSchoolCity,PSchoolState)

	 values(@Name,@DOB,@Gender,@Nationality,@ReligionID,@DOJ,@EmailID,@Category,@Disability,@BloodGroup,
	 @MotherTongueID,@PassportNo,@ExtraCurricular,@Allergic_Medicine,@SBranchID,@AdmissionBy,@OperationDate,@ImageName,@AccessCardNo,@NotificationSMSTo,@SchoolUID,@AadharCardNo,
	 @SSSID,@FamilyID,@BankName,@AccountNumber,@IFSCCode,@BranchName,@PSchoolName,@PSchoolMedium,@PClassName,@PSResult,@PSchoolCity,@PSchoolState)
	 SELECT @StudentID= CAST(SCOPE_IDENTITY() as int)

	 declare @StudentSID nvarchar(20)

	  select @StudentSID='STUD'+RIGHT(REPLICATE('0',6)+CAST(@StudentID AS VARCHAR(6)),6) 
	   
		 insert into LoginDetails (UserName,Password,EmailID,UserID,Usertype,SBranchID)
		 values(@StudentSID,@Password,@EmailID,@StudentID,6,@SBranchID)

		 SELECT @StudentID
		
		 declare @StudentAID nvarchar(20)

		 select @StudentAID='STU'+RIGHT(REPLICATE('0',6)+CAST(@StudentID AS VARCHAR(6)),6) 
	    if(@AccessCardNo is not null and @AccessCardNo!='')
			begin
				 exec sp_AddUserToEsslDevice @Name,@StudentAID,@SBranchID,@AccessCardNo,@OperationDate
			 end
 end
 else
 begin
	declare @OldAccessCardNumber nvarchar(50)
	
	select  @OldAccessCardNumber =AccessCardNo from [dbo].[StudentMaster] where StudentID=@StudentID
	 Update [dbo].[StudentMaster]
	set Name=@Name,DOB=@DOB,Gender=@Gender,Nationality=@Nationality,ReligionID=@ReligionID,DOJ=@DOJ,EmailID=@EmailID
	,Category=@Category,Disability=@Disability,BloodGroup=@BloodGroup,
	 MotherTongueID=@MotherTongueID,PassportNo=@PassportNo,ExtraCurricular=@ExtraCurricular,
	 Allergic_Medicine=@Allergic_Medicine,SBranchID=@SBranchID,ModifiedDate=@OperationDate,Photo=@ImageName
	 ,AccessCardNo=@AccessCardNo,NotificationSMSTo=@NotificationSMSTo,SchoolUID=@SchoolUID,AadharCardNo=@AadharCardNo,SSSID=@SSSID,FamilyID=@FamilyID
	 ,BankName=@BankName,AccountNumber=@AccountNumber,IFSCCode=@IFSCCode,BranchName=@BranchName,PSchoolName=@PSchoolName,PSchoolMedium=@PSchoolMedium,PClassName=@PClassName,PSResult=@PSResult,PSchoolCity=@PSchoolCity,PSchoolState=@PSchoolState
	 where StudentID=@StudentID

	 if(@OldAccessCardNumber<>@AccessCardNo)
	 begin
		 declare @StudentAUID nvarchar(20)
		 select @StudentAUID='STU'+RIGHT(REPLICATE('0',6)+CAST(@StudentID AS VARCHAR(6)),6) 
		 exec sp_DeleteUserToEsslDevice @StudentAUID,@SBranchID,@OperationDate
		 set @OperationDate=DATEADD (minute , 5 , @OperationDate ) 
		  if(@AccessCardNo is not null and @AccessCardNo!='')
			begin
				exec sp_AddUserToEsslDevice @Name,@StudentAUID,@SBranchID,@AccessCardNo,@OperationDate
			end
	 end
	 select @StudentID
 end
end

GO
CREATE procedure [dbo].[spn_InsertUpdateToDoItem]
(
@ToDoID int,
@Priority int,
@ScheduledDateTime datetime,
@Status int,
@Details nvarchar(MAX),
@UserID int,
@SBranchID int,
@CreatedDate datetime,
@UserType int,
@OpType int
)
as 
begin 
	if(@OpType=-1)
	begin
		Delete from ToDOList where ScheduledDateTime<dateadd(day,-7,@CreatedDate) and Status=1
		update ToDOList set Status=1 where ToDoID=@ToDoID
	end
	else
	begin
		if(@ToDoID=0)
		begin
			insert into ToDOList([Priority],ScheduledDateTime,[Status],Details,UserID,SBranchID,CreatedDate,UserType )
			values (@Priority,@ScheduledDateTime,@Status,@Details,@UserID,@SBranchID,@CreatedDate,@UserType )
		
			 SELECT @ToDoID= CAST(SCOPE_IDENTITY() as int)
		end
		else
		begin
			Update ToDOList set [Priority]=@Priority,ScheduledDateTime=@ScheduledDateTime,[Status]=@Status,Details=@Details,UserID=@UserID,SBranchID=@SBranchID,CreatedDate=@CreatedDate
			where ToDoID=@ToDoID

		end
	end
	select @ToDoID
end
GO
CREATE Procedure [dbo].[spn_SaveFeePaymentV2] 
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
ALTER proc [dbo].[spnp_GetClassSectionFeePayments]
(
@Day nvarchar(3),
@Month nvarchar(3),
@Year nvarchar(5),
@SBranchID int,
@ClassID int,
@SectionID int,
@SessionID int
)
AS
BEGIN

declare @StartDate date
declare @EndDate date
declare @SessionStartDate date
	Declare @TransportFeeMode int
	Declare @HostelFeeMode int
	Declare @HostelFee numeric(10,2)
	Declare @TransportFee numeric(10,2)
	if(@ClassID=0)
	begin
		select @ClassID=min(ClassID) from ClassMaster where SBranchID=@SBranchID
		select @SectionID=min(ID) from Class_Sections where ClassID=@ClassID
		select @SessionID=(SessionID),@StartDate=(SessionStartDate),@EndDate=(SessionEndDate) from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
	end
	
	if(@SessionID=0)
	begin
		select @SessionID=SessionID,@StartDate=(SessionStartDate),@EndDate=(SessionEndDate) from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
	end
	else
	begin
		select @StartDate=(SessionStartDate),@EndDate=(SessionEndDate) from SessionMaster where SessionID=@SessionID
	end
	set @SessionStartDate=@StartDate
	Select ClassID as ID, ClassName as Name from ClassMaster where SBranchID=@SBranchID
	Select ID,Name from Class_Sections where ClassID=@ClassID
	select @ClassID
	select @SectionID

	declare @GroupID int
	select @GroupID=GroupID from Class_Sections where ID=@SectionID

	declare @LastPayDay int
	Select @TransportFeeMode=details from MasterSettings where Type='TransportFeeMode' and SBranchID=@SBranchID
	Select @HostelFeeMode=details from MasterSettings where Type='HostelFeeMode' and SBranchID=@SBranchID
	Select @LastPayDay=details from MasterSettings where Type='FeePaymentReminderDate' and SBranchID=@SBranchID
	set @TransportFee=-1
	set @HostelFee=-1
	if(@TransportFeeMode=1)
		begin
			select @TransportFee=sum(isnull(FeeAmount,0)) from ClassFeeStructureMaster 
			where ClassID=@ClassID and GroupID=@GroupID and SBranchID=@SBranchID 
			and FeeTypeID in (select FeeTypeID from FeeTypeMaster where FeeTypeApplicable=5) and SessionID=@SessionID
		end
		if(@HostelFeeMode=1)
		begin
			select @HostelFee= sum(isnull(FeeAmount,0)) from ClassFeeStructureMaster 
			where ClassID=@ClassID and GroupID=@GroupID and SBranchID=@SBranchID 
			and FeeTypeID in (select FeeTypeID from FeeTypeMaster where FeeTypeApplicable=6) and SessionID=@SessionID
		end

	Declare @Discounts table(StudentID int,FeeMonth int,FeeYear int,ApprovedAmount decimal(18,2))

	Declare @CurDate date=cast(@Month+'-28-'+@Year as date)
	if(@Day>28 and @Month<>2)
	begin
		SELECT @CurDate=EOMONTH (@CurDate )
	end

	Declare @Students table(StudentID int,Name nvarchar(100),RollNo nvarchar(100),Gender int,Photo nvarchar(100),ClassID int,FeePaymentMode int,
	StudentSID nvarchar(15), ClassName nvarchar(10),SectionName nvarchar(10),FromDate date,ToDate Date, GroupID int,QuotaID int,
	SessionID int,SBranchID int,VehicleRouteID int,HostelRoomID int,TransportFeeMode int,
		HostelFeeMode int,TransportFee numeric(10,2),HostalFee numeric(10,2),LateFee numeric(10,2),CustomFee numeric(10,2),SectionID int,SessionStartDate date,LastPayDay int,
		SessionEndDate date,IsAdmissionFee int,SchoolUID nvarchar(50))

	Declare @FeeTable table(StudentID int,Name nvarchar(100),Photo nvarchar(100),Gender int,StudentSID  nvarchar(15),RollNo nvarchar(100)
	,QuotaID int,FeePaymentMode int,ClassID int,SectionID int,GroupID int, Month int,Year int,QuotaName nvarchar(50),PaymentID int,ReferanceNumber nvarchar(50),
	PaymentStatus int,ApplicableFee numeric(10,2),DiscAmt numeric(10,2),PaymentAmount numeric(10,2),LateFee numeric(10,2),TransportFee numeric(10,2),HostalFee numeric(10,2),
	LastPayDay int,SchoolUID nvarchar(50))


	declare @LMonth int
	declare @LYear int
	Insert into @Students select * from [dbo].[fn_GetStudentFeeRelatedListOnClassSection](@ClassID,@SectionID,@SessionID,@Month,@Year)
	
	Insert into @Discounts Select StudentID,FeeMonth,FeeYear,
	(Select sum(ApprovedAmount) from  [FeeDiscountRequestDetails] FDD where FDM.DiscRequestID=FDD.DiscRequestID ) as ApprovedAmount
	from  [FeeDiscountRequestMaster] FDM 
	 where StudentID in (select StudentID from @Students) and Status=1

while(@StartDate<=@CurDate)
begin

	set @LMonth= datepart(month,@StartDate)
	set @LYear=datepart(year,@StartDate)
	insert into @FeeTable
	Select StudentID,Name,Photo,Gender,StudentSID,RollNo,QuotaID,FeePaymentMode,ClassID,SectionID,GroupID,Month,Year,QuotaName,PaymentID,ReferanceNumber,PaymentStatus,
	(case when PaymentID is not null then ApplicableFee else isnull(CustomFee,ApplicableFee)+HostalFee+TransportFee end) as ApplicableFee,DiscAmt,PaymentAmount,LateFee,0 as HostalFee, 0 as TransportFee,LastPayDay,SchoolUID from
	(Select SM.StudentID,SM.Name,SM.Photo,SM.Gender,SM.StudentSID,
		SM.RollNo,SM.QuotaID,SM.FeePaymentMode,SM.ClassID,SM.SectionID ,SM.GroupID,@LMonth as [Month],@LYear as Year,
		isnull((Select QuotaName from QuotaMaster QM where QM.QuotaID=SM.QuotaID),'No Quota') as QuotaName,
		PM.PaymentID as PaymentID,'' as ReferanceNumber,
		(case when isnull(PaymentAmount,0)=0 then 0 else (case when isnull(PM.ApplicableFee,CGQF.ApplicableFee)-isnull(PaymentAmount,0)-isnull(PM.DiscAmt,0)>0 then 2 else 1 end)end) as PaymentStatus
		,isnull(PM.ApplicableFee, CGQF.ApplicableFee) as ApplicableFee,isnull(nullif(PM.DiscAmt,0),D.ApprovedAmount) as DiscAmt,
		PaymentAmount,SM.LateFee as LateFee,
		[dbo].[fn_GetStudentTransportHostalFeeAmount](@LMonth,@LYear,SM.StudentID,0,0,@HostelFeeMode,@TransportFeeMode,@ClassID,SM.FeePaymentMode,@GroupID,@SBranchID,-1,-1,@SessionID) as TransportFee 
		,[dbo].[fn_GetStudentTransportHostalFeeAmount](@LMonth,@LYear,SM.StudentID,0,1,@HostelFeeMode,@TransportFeeMode,@ClassID,SM.FeePaymentMode,@GroupID,@SBranchID,-1,-1,@SessionID) as HostalFee
		,@LastPayDay as LastPayDay,SM.SchoolUID,
		(Select Sum(FeeAmount) from StudentFeeDetails SFD where SFD.StudentID=SM.StudentID and SFD.IsApplicable=1 and SFD.FeeTypeID in 
		(Select FeeTypeID from FeeTypeMaster FTM where FeeTypeApplicable in (Select FeeTypeID from [MonthTypeFeeType] MFT where MFT.MonthTypeID=
		(Case when datepart(month,SM.FromDate)=@LMonth and (datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth) then 5 else
		(Case when datepart(month,SM.FromDate)=@LMonth and (datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth) then 6 else 
		(case when datepart(month,SM.FromDate)=@LMonth then (case when isnull(SM.IsAdmissionFee,0)=0 then 1 else 0 end) 
		else (Case when datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth then 3
		else (case when  datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth then 4 else 2 end) end) end)end)end)
		))) as CustomFee
		from @Students SM left outer join v_StudentPaymentDetails PM on isnull(PM.PayeeID,0)=SM.StudentID and PM.Year=@LYear and PM.Month=@LMonth
		left outer join 
		v_ClassGroupQuotaSessionFee  CGQF on CGQF.ClassID=SM.ClassID and CGQF.GroupID=SM.GroupID 
		and CGQF.QuotaID=SM.QuotaID and CGQF.SessionID=SM.SessionID 
		and CGQF.MonthType=(Case when datepart(month,SM.SessionStartDate)=@LMonth and (datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth) then 5 else
		(Case when datepart(month,SM.SessionStartDate)=@LMonth and (datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth) then 6 else 
		(case when datepart(month,SM.SessionStartDate)=@LMonth then (case when isnull(SM.IsAdmissionFee,0)=0 then 1 else 0 end) 
		else (Case when datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth then 3
		else (case when  datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth then 4 else 2 end) end) end)end)end)
		and CGQF.FeePaymentMode=SM.FeePaymentMode
		and @LMonth+@LYear*12>=datepart(month,SM.SessionStartDate)+datepart(year,SM.SessionStartDate)*12 and @LMonth+@LYear*12<=datepart(month,SM.SessionEndDate)+datepart(year,SM.SessionEndDate)*12
		left outer join @Discounts D on D.StudentID=SM.StudentID and D.FeeMonth= @LMonth and D.FeeYear=@LYear
		where CGQF.ApplicableFee is not null 
		)t
Set @StartDate= DATEADD(month,1,@StartDate)
end

Declare @TOTStudents table(StudentID int,MaxMonthYear int)
insert into @TOTStudents select StudentID,Max(Month+Year*12) from @FeeTable group by StudentID

Select StudentID,StudentSID,RollNo,Photo,Name,Gender,QuotaID,FeePaymentMode,Month,Year, QuotaName,PaymentStatus,PaymentID,LateFee,DiscAmt,PaymentAmount,
ApplicableFee,PreviousDues,HostalFee as HostelFee,TransportFee,
(Select FatherName from ParentMaster where ParentID=(Select ParentID from StudentMaster SM where SM.StudentID=tt.StudentID)) as FatherName,SchoolUID
from
(select StudentID,StudentSID,RollNo,Photo,Name,Gender,QuotaID,FeePaymentMode,Month,Year, QuotaName,PaymentStatus,PaymentID,
(case when PaymentAmount is null and  (@Year*12+@Month>[Year]*12+[Month] or @Year*12+@Month=[Year]*12+[Month] and @Day>LastPayDay) then LateFee else 0 end)
 as LateFee,DiscAmt,PaymentAmount,ApplicableFee,HostalFee,TransportFee,SchoolUID 
,(Select sum(TransportFee)+sum(HostalFee)+sum(ApplicableFee-isnull(DiscAmt,0)-isnull(PaymentAmount,0)+(Case when FT.PaymentStatus=0 then LateFee else 0 end))from @FeeTable FT 
where FT.StudentID=FTM.StudentID and FT.Year*12+FT.Month<(Select MaxMonthYear from @TOTStudents TS where TS.StudentID=FT.StudentID)) as PreviousDues
from @FeeTable FTM)tt
 where Month+Year*12=(Select MaxMonthYear from @TOTStudents TS where TS.StudentID=tt.StudentID)

	order by Name,Year*12+Month desc

	select SessionID,SessionName,SessionStatus from SessionMaster where SBranchID=@SBranchID
	select isnull(@SessionID,0)

END
GO
ALTER proc [dbo].[spnp_GetParentFeePayments]
(
@Day nvarchar(3),
@Month nvarchar(3),
@Year nvarchar(5),
@ParentID int
)
AS
BEGIN

Declare @Discounts table(StudentID int,FeeMonth int,FeeYear int,ApprovedAmount decimal(18,2))

	Declare @CurDate date=cast(@Month+'-1-'+@Year as date)
Declare @Students table(StudentID int,Name nvarchar(100),RollNo nvarchar(100),Gender int,Photo nvarchar(100),ClassID int,FeePaymentMode int,
StudentSID nvarchar(15), ClassName nvarchar(10),SectionName nvarchar(10),FromDate date,ToDate Date, GroupID int,QuotaID int,
SessionID int,SBranchID int,VehicleRouteID int,HostelRoomID int,TransportFeeMode int,
	HostelFeeMode int,TransportFee numeric(10,2),HostalFee numeric(10,2),LateFee numeric(10,2),SectionID int,SessionStartDate date,LastPayDay int,SessionEndDate date)

Declare @FeeTable table(StudentID int,Name nvarchar(100),Photo nvarchar(100),Gender int,StudentSID  nvarchar(15),RollNo nvarchar(100)
	,QuotaID int,FeePaymentMode int,ClassID int,SectionID int,GroupID int, Month int,Year int,QuotaName nvarchar(50),PaymentID int,ReferanceNumber nvarchar(50),
	PaymentStatus int,ApplicableFee numeric(10,2),DiscAmt numeric(10,2),PaymentAmount numeric(10,2),LateFee numeric(10,2),TransportFee numeric(10,2),HostalFee numeric(10,2),LastPayDay int,LastDate date,PaidDate date,IsPayNow int)

declare @StartDate date
declare @EndDate date
declare @SessionStartDate date
declare @SessionEndDate date
Select @StartDate=min(SessionStartDate),@EndDate=max(SessionEndDate) from SessionMaster where SessionStatus=1
set @SessionStartDate=@StartDate
set @SessionEndDate=@EndDate
declare @LMonth int
declare @LYear int
	Insert into @Students select * from [dbo].[fn_GetStudentFeeRelatedListOnParent](@ParentID,@Month,@Year)

	Insert into @Discounts Select StudentID,FeeMonth,FeeYear,
	(Select sum(ApprovedAmount) from  [FeeDiscountRequestDetails] FDD where FDM.DiscRequestID=FDD.DiscRequestID ) as ApprovedAmount
	from  [FeeDiscountRequestMaster] FDM 
	 where StudentID in (select StudentID from @Students) and Status=1

while(@StartDate<=@CurDate)
begin
	set @LMonth= datepart(month,@StartDate)
	set @LYear=datepart(year,@StartDate)
	insert into @FeeTable
	Select StudentID,Name,Photo,Gender,StudentSID,RollNo,QuotaID,FeePaymentMode,ClassID,SectionID,GroupID,Month,Year,QuotaName,PaymentID,ReferanceNumber,PaymentStatus,
	(case when PaymentID is not null then ApplicableFee else ApplicableFee+HostalFee+TransportFee end) as ApplicableFee,DiscAmt,PaymentAmount,LateFee,0 as HostalFee, 0 as TransportFee,LastPayDay,
	LastDate,PaidDate,IsPayNow from
	(Select SM.StudentID,SM.Name,SM.Photo,SM.Gender,SM.StudentSID,
		SM.RollNo,SM.QuotaID,SM.FeePaymentMode,SM.ClassID,SM.SectionID ,SM.GroupID,@LMonth as [Month],@LYear as Year,
		isnull((Select QuotaName from QuotaMaster QM where QM.QuotaID=SM.QuotaID),'No Quota') as QuotaName,
		PM.PaymentID as PaymentID,'' as ReferanceNumber,
		(case when isnull(PaymentAmount,0)=0 then 0 else (case when isnull(PM.ApplicableFee,CGQF.ApplicableFee)-isnull(PaymentAmount,0)-isnull(PM.DiscAmt,0)>0 then 2 else 1 end)end) as PaymentStatus
		,isnull(PM.ApplicableFee,CGQF.ApplicableFee) as ApplicableFee,isnull(nullif(PM.DiscAmt,0),D.ApprovedAmount) as DiscAmt,
		PaymentAmount,SM.LateFee as LateFee,
		[dbo].[fn_GetStudentTransportHostalFeeAmount](@LMonth,@LYear,SM.StudentID,0,0,SM.HostelFeeMode,SM.TransportFeeMode,SM.ClassID,SM.FeePaymentMode,SM.GroupID,SM.SBranchID,-1,-1,SM.SessionID) as TransportFee 
		,[dbo].[fn_GetStudentTransportHostalFeeAmount](@LMonth,@LYear,SM.StudentID,0,1,SM.HostelFeeMode,SM.TransportFeeMode,SM.ClassID,SM.FeePaymentMode,SM.GroupID,SM.SBranchID,-1,-1,SM.SessionID) as HostalFee
		,SM.LastPayDay,
		cast(cast(@LMonth as nvarchar(2))+'-'+cast(SM.LastPayDay as nvarchar(2))+'-'+cast(@LYear as nvarchar(5)) as date) as LastDate,
		(Select max(PaymentDate) from PaymentDetails PDt where PDt.Month=@LMonth and PDt.Year=@LYear and PDt.PayeeID=SM.StudentID) as  PaidDate
		 ,(Case when @LMonth=datepart(month,@CurDate) and @LYear=datepart(year,@CurDate) then 1 else 0 end) as IsPayNow
		
		from @Students SM left outer join v_StudentPaymentDetails PM on SM.StudentID=PM.PayeeID and PM.Year=@LYear and PM.Month=@LMonth
		left outer join v_ClassGroupQuotaSessionFee  CGQF on CGQF.ClassID=SM.ClassID and CGQF.GroupID=SM.GroupID 
		and CGQF.QuotaID=SM.QuotaID and CGQF.SessionID=SM.SessionID 
		and CGQF.MonthType=(Case when datepart(month,SM.SessionStartDate)=@LMonth and (datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth) then 5 else
		(Case when datepart(month,SM.SessionStartDate)=@LMonth and (datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth) then 6 else 
		(case when datepart(month,SM.SessionStartDate)=@LMonth then 1 
		else (Case when datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth then 3
		else (case when  datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth then 4 else 2 end) end) end)end)end)
		and CGQF.FeePaymentMode=SM.FeePaymentMode
		and @LMonth+@LYear*12>=datepart(month,SM.SessionStartDate)+datepart(year,SM.SessionStartDate)*12 and @LMonth+@LYear*12<=datepart(month,SM.SessionEndDate)+datepart(year,SM.SessionEndDate)*12
		left outer join @Discounts D on D.StudentID=SM.StudentID and D.FeeMonth= @LMonth and D.FeeYear=@LYear
		where CGQF.ApplicableFee is not null )t

Set @StartDate= DATEADD(month,1,@StartDate)
end
select StudentID,Name,Gender,QuotaID,FeePaymentMode,Month,Year, QuotaName,PaymentStatus,IsPayNow,
(case when PaymentStatus=0 and  
cast(@Month+'-'+@Day+'-'+@Year as date)>cast(cast(Month as nvarchar(2))+'-'+cast(LastPayDay as nvarchar(2))+'-'+cast(Year as nvarchar(4)) as date)
then ApplicableFee+LateFee else ApplicableFee end) as ApplicableFee,DiscAmt,PaymentAmount,LastDate,PaidDate,[dbo].[GetFeeMonthNames](FeePaymentMode,Month) as MonthNames
from @FeeTable
	order by Year*12+Month desc
END
GO
ALTER proc [dbo].[spnp_GetStudentFeePaymentRowData]--15,09,2019,1,2,2,1,8,'2019-09-15'
(
@Day nvarchar(3),
@Month nvarchar(3),
@Year nvarchar(5),
@SBranchID int,
@ClassID int,
@SectionID int,
@SessionID int,
@StudentID int,
@QDate date
)
AS
BEGIN

		declare @StartDate date
		declare @EndDate date
		set @QDate=@Year+'-'+@Month+'-'+@Day
		declare @SessionStartDate date
		if(@ClassID=0)
		begin
			select @ClassID=min(ClassID) from ClassMaster where SBranchID=@SBranchID
			select @SectionID=min(ID) from Class_Sections where ClassID=@ClassID
			select @SessionID=(SessionID),@StartDate=(SessionStartDate),@EndDate=(SessionEndDate) from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
		end

		if(@SessionID=0)
		begin
			select @SessionID=SessionID,@StartDate=(SessionStartDate),@EndDate=(SessionEndDate) from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
		end
		else
		begin
			select @StartDate=(SessionStartDate),@EndDate=(SessionEndDate) from SessionMaster where SessionID=@SessionID
		end
		set @SessionStartDate=@StartDate
		Declare @Discounts table(StudentID int,FeeMonth int,FeeYear int,ApprovedAmount decimal(18,2))

		declare @GroupID int
		select @GroupID=GroupID from Class_Sections where ID=@SectionID


		Declare @CurDate date=EOMONTH(@QDate)

		Declare @Students table(StudentID int,Name nvarchar(100),RollNo nvarchar(100),Gender int,Photo nvarchar(100),ClassID int,FeePaymentMode int,
		StudentSID nvarchar(15), ClassName nvarchar(10),SectionName nvarchar(10),FromDate date,ToDate Date, GroupID int,QuotaID int,
		SessionID int,SBranchID int,VehicleRouteID int,HostelRoomID int,TransportFeeMode int,
		HostelFeeMode int,TransportFee numeric(10,2),HostalFee numeric(10,2),LateFee numeric(10,2),SectionID int,SessionStartDate date,LastPayDay int,SessionEndDate date,IsAdmissionFee int,SchoolUID nvarchar(50))

		Declare @FeeTable table(StudentID int,Name nvarchar(100),Photo nvarchar(100),Gender int,StudentSID  nvarchar(15),RollNo nvarchar(100)
		,QuotaID int,FeePaymentMode int,ClassID int,SectionID int,GroupID int, Month int,Year int,QuotaName nvarchar(50),PaymentID int,ReferanceNumber nvarchar(50),
		PaymentStatus int,ApplicableFee numeric(10,2),DiscAmt numeric(10,2),PaymentAmount numeric(10,2),LateFee numeric(10,2),TransportFee numeric(10,2),HostalFee numeric(10,2),LastPayDay int,SchoolUID nvarchar(50))


		declare @LMonth int
		declare @LYear int
		Insert into @Students select * from [dbo].fn_GetStudentFeeRelatedRowOnStudent(@StudentID,@Month,@Year,@SessionID)

		Insert into @Discounts Select StudentID,FeeMonth,FeeYear,
		(Select sum(ApprovedAmount) from  [FeeDiscountRequestDetails] FDD where FDM.DiscRequestID=FDD.DiscRequestID ) as ApprovedAmount
		from  [FeeDiscountRequestMaster] FDM 
		where StudentID in (select StudentID from @Students) and Status=1

		declare @PaymentID int
		select @PaymentID= max(PaymentID) from PaymentMaster where PayeeID=@StudentID
		while(@StartDate<=@CurDate)
		begin
			set @LMonth= datepart(month,@StartDate)
			set @LYear=datepart(year,@StartDate)
			insert into @FeeTable
			Select StudentID,Name,Photo,Gender,StudentSID,RollNo,QuotaID,FeePaymentMode,ClassID,SectionID,GroupID,Month,Year,QuotaName,PaymentID,ReferanceNumber,PaymentStatus,
			(case when PaymentID is not null then ApplicableFee else isnull(CustomFee,ApplicableFee)+HostalFee+TransportFee end) as ApplicableFee,DiscAmt,PaymentAmount,LateFee,0 as HostalFee, 0 as TransportFee,LastPayDay,SchoolUID
			from
			(Select SM.StudentID,SM.Name,SM.Photo,SM.Gender,SM.StudentSID,
			SM.RollNo,SM.QuotaID,SM.FeePaymentMode,SM.ClassID,SM.SectionID ,SM.GroupID,@LMonth as [Month],@LYear as Year,
			isnull((Select QuotaName from QuotaMaster QM where QM.QuotaID=SM.QuotaID),'No Quota') as QuotaName,
			PM.PaymentID as PaymentID,'' as ReferanceNumber,
			(case when isnull(PaymentAmount,0)=0 then 0 else (case when isnull(PM.ApplicableFee,CGQF.ApplicableFee)-isnull(PaymentAmount,0)-isnull(PM.DiscAmt,0)>0 then 2 else 1 end)end) as PaymentStatus
			,isnull(PM.ApplicableFee,CGQF.ApplicableFee) as ApplicableFee,isnull(nullif(PM.DiscAmt,0),D.ApprovedAmount) as DiscAmt,
			PaymentAmount,SM.LateFee as LateFee,
			(Select Sum(FeeAmount) from StudentFeeDetails SFD where SFD.StudentID=SM.StudentID and SFD.IsApplicable=1 and SFD.FeeTypeID in 
			(Select FeeTypeID from FeeTypeMaster FTM where FeeTypeApplicable in (Select FeeTypeID from [MonthTypeFeeType] MFT where MFT.MonthTypeID=
			(Case when datepart(month,SM.FromDate)=@LMonth and (datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth) then 5 else
			(Case when datepart(month,SM.FromDate)=@LMonth and (datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth) then 6 else 
			(case when datepart(month,SM.FromDate)=@LMonth then (case when isnull(SM.IsAdmissionFee,0)=0 then 1 else 0 end) 
			else (Case when datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth then 3
			else (case when  datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth then 4 else 2 end) end) end)end)end)
			))) as CustomFee,
			[dbo].[fn_GetStudentTransportHostalFeeAmount](@LMonth,@LYear,SM.StudentID,0,0,SM.HostelFeeMode,SM.TransportFeeMode,@ClassID,SM.FeePaymentMode,@GroupID,@SBranchID,-1,-1,@SessionID) as TransportFee 
			,[dbo].[fn_GetStudentTransportHostalFeeAmount](@LMonth,@LYear,SM.StudentID,0,1,SM.HostelFeeMode,SM.TransportFeeMode,@ClassID,SM.FeePaymentMode,@GroupID,@SBranchID,-1,-1,@SessionID) as HostalFee
			,SM.LastPayDay
			--,dbo.fn_GetStudentDueFeeAmount(SM.StudentID,SM.ClassID,SM.QuotaID,SM.GroupID,SM.VehicleRouteID,SM.FeePaymentMode,SM.HostelFeeMode,SM.TransportFeeMode,SM.LateFee,SM.HostalFee,SM.TransportFee,SM.HostelRoomID,SSM.SessionStartDate,SSM.SessionEndDate,@CurDate,SM.LastPayDay,0) as PreviousDues
			,SM.SchoolUID
			from @Students SM left outer join v_StudentPaymentDetails PM on SM.StudentID=PM.PayeeID and PM.Year=@LYear and PM.Month=@LMonth
			left outer join v_ClassGroupQuotaSessionFee  CGQF on CGQF.ClassID=SM.ClassID and CGQF.GroupID=SM.GroupID 
			and CGQF.QuotaID=SM.QuotaID and CGQF.SessionID=SM.SessionID 
			and CGQF.MonthType=(Case when datepart(month,SM.SessionStartDate)=@LMonth and (datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth) then 5 else
			(Case when datepart(month,SM.SessionStartDate)=@LMonth and (datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth) then 6 else 
			(case when datepart(month,SM.SessionStartDate)=@LMonth then (case when isnull(SM.IsAdmissionFee,0)=0 then 1 else 0 end)  
			else (Case when datepart(month,dateadd(month,3,@SessionStartDate))=@LMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@LMonth then 3
			else (case when  datepart(month,dateadd(month,6,@SessionStartDate))=@LMonth then 4 else 2 end) end) end)end)end)
			and CGQF.FeePaymentMode=SM.FeePaymentMode
			--and @LMonth+@LYear*12>=datepart(month,SM.SessionStartDate)+datepart(year,SM.SessionStartDate)*12 and @LMonth+@LYear*12<=datepart(month,SM.SessionEndDate)+datepart(year,SM.SessionEndDate)*12
			left outer join @Discounts D on D.StudentID=SM.StudentID and D.FeeMonth= @LMonth and D.FeeYear=@LYear
			where CGQF.ApplicableFee is not null )t
			Set @StartDate= DATEADD(month,1,@StartDate)
		end

		Declare @TOTStudents table(StudentID int,MaxMonthYear int)
		insert into @TOTStudents select StudentID,Max(Month+Year*12) from @FeeTable group by StudentID

		Select StudentID,SchoolUID,StudentSID,RollNo,Photo,Name,Gender,QuotaID,FeePaymentMode,Month,Year, QuotaName,PaymentStatus,@PaymentID as PaymentID,LateFee,DiscAmt,PaymentAmount,
		ApplicableFee,PreviousDues from
		(select StudentID,StudentSID,RollNo,Photo,Name,Gender,QuotaID,FeePaymentMode,Month,Year, QuotaName,PaymentStatus,PaymentID,
		(case when PaymentAmount is null and  (@Year*12+@Month>[Year]*12+[Month] or @Year*12+@Month=[Year]*12+[Month] and @Day>LastPayDay) then LateFee else 0 end)
		 as LateFee,DiscAmt,PaymentAmount,ApplicableFee
		,(Select sum(TransportFee)+sum(HostalFee)+sum(ApplicableFee-isnull(DiscAmt,0)-isnull(PaymentAmount,0)+(Case when FT.PaymentStatus=0 then LateFee else 0 end)) from @FeeTable FT 
		where FT.StudentID=FTM.StudentID and FT.Year*12+FT.Month<(Select MaxMonthYear from @TOTStudents TS where TS.StudentID=FT.StudentID)) as PreviousDues,SchoolUID
		from @FeeTable FTM)tt
		 where Month+Year*12=(Select MaxMonthYear from @TOTStudents TS where TS.StudentID=tt.StudentID)

		order by Year*12+Month desc

END
GO

