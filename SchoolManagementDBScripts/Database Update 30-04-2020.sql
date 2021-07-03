ALTER procedure [dbo].[sp_GetStudentFeeDetailsNew]
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

	select top 1 @ClassID=SS.ClassID,@FeePaymentMode=FeePaymentMode,
	@QuotaID=SS.QuotaID,@SectionID=SS.SectionID,@StuSessionSDate=(Case when @SessionStartDate<isnull(SS.FromDate,@SessionStartDate) then SS.FromDate else @SessionStartDate end),
	@StuSessionEDate=(Case when @SessionEndDate>SS.ToDate then SS.ToDate else @SessionEndDate end),
	@IsAdmissionFee=SS.IsAdmissionFeeApplicable,@IsCustomFee=isnull(SS.IsCustomFee,0),@StudentSessionUID=StudentSessionUID
	from Student_Session SS 
	where SS.StudentID=@StudentID and SessionID=@SessionID 
	order by (case when @QDate between FromDate and ToDate then 0 else 1 end)asc,Status desc
	
	
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
	select FTM.FeeTypeID,FTM.FeeTypeName,FTM.FeeTypeApplicable,CFS.FeeAmount,isnull(CFS.Status,1),FTM.Months
	from FeeTypeMaster FTM left outer join [dbo].[ClassFeeStructureMaster] CFS on CFS.FeeTypeID=FTM.FeeTypeID 
	and ClassID=@ClassID and GroupID=@GroupID
	and CFS.SessionID=@SessionID where FTM.SBranchID=@SBranchID
	
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
				(case when @IsCustomFee is null then 0 else @IsCustomFee end) as IsCustomFee,
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
		,sum((Case when FeeTypeApplicable in(5,6,7) then FeeAmount else CustomFee end)) as CustomFee,sum(PaidAmount) as PaidAmount,max(IsCustomFee) as IsCustomFee,max(IsPayment) as IsPayment from
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
	select FTM.FeeTypeID,FTM.FeeTypeName,FTM.FeeTypeApplicable,CFS.FeeAmount,isnull(CFS.Status,1),FTM.Months
	from FeeTypeMaster FTM left outer join [dbo].[ClassFeeStructureMaster] CFS on CFS.FeeTypeID=FTM.FeeTypeID and ClassID=@ClassID 
	and GroupID=@GroupID
	and CFS.SessionID=@SessionID where FTM.SBranchID=@SBranchID
	
	Declare @StuSessionSDate date,@StuSessionEDate date,@StudentID int,@QuotaID int,@IsAdmissionFee int,
	@FeeAmount numeric(10,2),@PreviousDue numeric(10,2),@LateFee numeric(10,2),@Discounts numeric(10,2)
	,@LMonth int,@LYear int,@MonthType int,@BaseDate date,@Paid numeric(10,2),@FeePaymentMode int,@IsCustomFee int
	
	declare @isPaid int
	
		DECLARE stu_Cursor CURSOR FOR
		SELECT SessionStartDate,SessionEndDate,StudentID,QuotaID,IsAdmissionFee,FeeAmount,PreviousDue,LateFee,Discounts,Paid,FeePaymentMode
		FROM @Students where isnull(StudentID,0)!=0 FOR UPDATE OF FeeAmount,PreviousDue,LateFee,Discounts,Paid
		OPEN stu_Cursor
		FETCH NEXT FROM stu_Cursor
		INTO @StuSessionSDate,@StuSessionEDate,@StudentID,@QuotaID,@IsAdmissionFee,@FeeAmount,@PreviousDue,@LateFee,@Discounts,@Paid,@FeePaymentMode

		WHILE (@@FETCH_STATUS = 0)
		BEGIN
			declare @StudentSessionUID int
			set @Discounts=0
			select top 1 @StudentSessionUID=StudentSessionUID,@IsCustomFee=isnull(IsCustomFee,0) from Student_Session where StudentID=@StudentID and SessionID=@SessionID
			order by Status desc
			print(@StudentSessionUID)
			print(@FeePaymentMode)
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
						declare @PDIsc numeric(10,2)=0

						select @PPD=sum((case when isnull(PD.PaymentID,0)!=0 then PD.NetApplicablePayment-isnull(PD.DiscAmt,0)  
						when isnull(@IsCustomFee,0)=1 then SFD.FeeAmount else (FTS.FeeAmount-isnull(FTS.FeeAmount*QD.Discount/100,0)) end)) 
						,@PPP=isnull(Sum(PD.PaymentRecieved),0)
						from @FeeStructureTable FTS left outer join @QuotaDiscounts QD on QD.FeeTypeID=FTS.FeeTypeID
						left outer join [dbo].[StudentFeeDetails] SFD on SFD.StudentID=@StudentID and SFD.SessionID=@StudentSessionUID and 
						SFD.FeeTypeID=FTS.FeeTypeID
						left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
						where FTS.FeeTypeApplicable in (select FeeTypeID from @MTFT where MonthTypeID=@MonthType) and FTS.FeeTypeApplicable!=5
						and FTS.FeeTypeApplicable <> 7 and FTS.FeeTypeApplicable<>(Case when @IsAdmissionFee=0 then 8 else 0 end)
						and isnull(SFD.IsApplicable,FTS.Status)=1 and (FTS.FeeTypeApplicable<>9 or 
						(select count(*) from [dbo].[SplitStringToTable](FTS.Months,',') where Item=@LMonth)>0)

						select @isPaid=count(*) from v_PaymentDetails PD where PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
						declare @pDiscount numeric(10,2)=0
						if(isnull(@isPaid,0)=0)
						begin
							select @pDiscount=isnull(sum(ApprovedAmount),0) from @DiscountApproved where StudentID=@StudentID and FeeMonth=@LMonth and FeeYear=@LYear				
						end
						if((select count(*) from @FeeStructureTable where FeeTypeApplicable=5)>0)
						begin
						select @PreviousDue= isnull(@PreviousDue,0)+isnull([dbo].[fn_GetStudentTransportFeeAmount](@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID),0)
						end
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
						
						select @isPaid=count(*) from v_PaymentDetails PD where PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
						
						if(isnull(@isPaid,0)=0)
						begin
							select @Discounts=isnull(@Discounts,0)+isnull(sum(ApprovedAmount),0) from @DiscountApproved where StudentID=@StudentID and FeeMonth=@LMonth and FeeYear=@LYear
						end
						
						if((select count(*) from @FeeStructureTable where FeeTypeApplicable=5)>0)
						begin
						select  @FeeAmount=isnull(@FeeAmount,0)+[dbo].[fn_GetStudentTransportFeeAmount](@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID)
						end
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
		SessionStartDate,SessionEndDate,IsAdmissionFee,SchoolUID,FeeAmount,PreviousDue,LateFee,Discounts,Paid from @Students where isnull(StudentID,0)!=0

END

GO

ALTER Procedure [dbo].[sp_GetEmployeeLeaveDetailsNew] 
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
  select LeaveID,LeaveTypeID,isnull(Applied,0),DatePart(month,LeaveDate) as Month,datepart(year,LeaveDate) as Year from LeaveDetails LD
  where LeaveID in (select LeaveID from LeaveMaster where ApplicantID=@EmployeeID and ApplicantType=0 and EmployeeType=@EmployeeType and IsApproved in (1,2))
  and LeaveDate between @StartDate and @EndDate

  insert into @MonthLeaves
  select * from @Leaves where Month=@Month and Year=@Year


    Select LeaveTypeID,LeaveTypeName,YearlyQuota,MonthlyQuota,IsSalaryDeduct,isnull(YearlyApplied,0) as YearlyApplied,isnull(MonthlyApplied,0) as MonthlyApplied,isnull(Applied,0) as Applied,isnull(SequenceNo,0) as SequenceNo,MaxConsecutive from
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
ALTER proc [dbo].[sp_GetEmployeeLeavesEmployee]
(
@SBranchID int,
@Month int,
@Year int,
@EmployeeID int
)
AS
BEGIN

	declare @EmployeeType int
	select @EmployeeType =EmployeeType from EmployeeMaster EM where EM.EmployeeID=@EmployeeID

	exec sp_GetEmployeeLeaveDetailsNew 0,@EmployeeID,@Month,@Year,@EmployeeType,0,@SBranchID
END
GO
ALTER procedure [dbo].[sp_InsertEmployeeLeaveNew]
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
	select 1
end
GO
Create Procedure [dbo].[spn_GetParentWiseStudentFeeSummeryNew]
(
@ParentID int,
@QDate date,
@CurDate Date
)
as
BEGIN
	
	Declare @MotherName nvarchar(100),@FatherName nvarchar(100)

	select @MotherName=MotherName,@FatherName=FatherName from ParentMaster where ParentID=@ParentID

	Declare @Students table(StudentID int,Name nvarchar(100),RollNo nvarchar(100),Gender int,Photo nvarchar(100),ClassID int,FeePaymentMode int,
	StudentSID nvarchar(15),FromDate date,ToDate Date,QuotaID int,
	SessionID int,VehicleRouteID int,HostelRoomID int,SectionID int,
	SessionStartDate date,SessionEndDate date,IsAdmissionFee int,SchoolUID nvarchar(50),FeeAmount numeric(10,2),PreviousDue numeric(10,2)
	,LateFee numeric(10,2),Discounts numeric(10,2),Paid numeric(10,2))

	
		DECLARE @StudentID int

		DECLARE curStudents CURSOR
		FOR select StudentID from StudentMaster where ParentID=@ParentID

		OPEN curStudents;

		FETCH NEXT FROM curStudents INTO @StudentID

		WHILE @@FETCH_STATUS = 0
			BEGIN
				declare @SessionID int,@SBranchID int,@ClassID int,@SectionID int

				Select top 1 @SessionID=SessionID,@SBranchID=SBranchID,@ClassID=ClassID,@SectionID=SectionID 
				from Student_Session where StudentID=@StudentID order by Status desc

				insert into @Students
				exec sp_GetClassGroupFeeListOnly @ClassID,@SectionID,@SBranchID,@SessionID,@QDate,@CurDate,@StudentID

				FETCH NEXT FROM curStudents INTO @StudentID
			END;

		CLOSE curStudents;

		DEALLOCATE curStudents;
		

		select S.*,Q.QuotaName ,@MotherName as MotherName,@FatherName as FatherName,VCS.ClassName+' / '+VCS.SectionName as ClassSection,
		(select top 1 IsCustomFee from Student_Session SS where SS.StudentID=S.StudentID and SS.SessionID=@SessionID) as IsCustomFee,
		(Case when isnull(S.FeeAmount,0)+isnull(S.PreviousDue,0)+isnull(S.LateFee,0)-isnull(S.Discounts,0)-isnull(S.Paid,0)>0 then 1 else 0 end) as IsPayEnable
		from @Students S left outer join QuotaMaster Q on Q.QuotaID=S.QuotaID
		left outer join StudentMaster SM on SM.StudentID=S.StudentID
		left outer join [dbo].[v_ClassSectionNames] VCS on VCS.SectionID=S.SectionID
		
END
GO
ALTER procedure [dbo].[sp_GetLeavesOnEmployee]
(
@EmployeeID int
)
as 
begin 
		Select LeaveID,LeaveType,StartDate,EndDate,LeaveReason,
		(case when EmployeeType in (-1,-2) then 'TRA' else 'EMP' end)+RIGHT(REPLICATE('0',6)+CAST(ApplicantID AS VARCHAR(6)),6) as [EmployeeSID],IsApproved		 
		from LeaveMaster where ApplicantType=0 and ApplicantID=@EmployeeID
		order by StartDate desc
end
GO

Create procedure [dbo].[sp_DeleteBlackBoardEntry]
(
	@BlackBoardID nvarchar(50)
)
AS
BEGIN
	Delete From BlackBoardMaster where BlackBoardID=@BlackBoardID
	Delete From BlackBoardImages where BlackBoardID=@BlackBoardID
	select 1
END
GO

Create procedure [dbo].[sp_DeleteBlackBoardImage]
(
	@BBMIID nvarchar(50)
)
AS
BEGIN
	Delete From BlackBoardImages where BBMIID=@BBMIID
	select 1
END
GO
Create Procedure [dbo].[sp_DeleteYouTubeVideo]
(
@TeacherID int,
@VideoID int
)
AS
BEGIN
	Delete From [dbo].[YouTubeVideos] where TeacherID=@TeacherID and VideoID=@VideoID

	select 1
END

GO

ALTER proc [dbo].[sp_DeleteAssignment]
(
@ID int
)
AS
BEGIN
	
	Delete from  [dbo].[AssignmentMaster] where ID=@ID 
	select @@rowcount
END


GO

alter table StockTransactionMaster 
Add SerialNo int

GO

alter table StockTransactionMaster 
Add FY nvarchar(5)

GO

alter table StockTransactionMaster 
Add InvoiceNumber nvarchar(50)

GO
ALTER Procedure [dbo].[sp_UpdateStockTransaction]
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
			declare @FYear nvarchar(10)
			if(datepart(month,@TrDate)<=3)
			begin
				set @FYear= cast((DATEPART("yyyy",@TrDate) % 100-1) as nvarchar(4))+cast((DATEPART("yyyy",@TrDate) % 100) as nvarchar(4))
			end
			else
			begin
				set @FYear= cast((DATEPART("yyyy",@TrDate) % 100) as nvarchar(4))+cast((DATEPART("yyyy",@TrDate) % 100+1) as nvarchar(4))
			end

			declare @SlNo int
			select @SlNo=max(SerialNo) from StockTransactionMaster where SBranchID=@SBranchID and TrType=@TrType and FY=@FYear

			declare @InvNumber nvarchar(50)
			set @SlNo=isnull(@SlNo,0)+1
			set @InvNumber=@FYear+'/'+cast(@SBranchID as nvarchar(5))+'/'+cast(@SlNo as nvarchar(10))

			Insert into StockTransactionMaster(TrDate,TrType,RefID,RefType,Remark,CreatedDate,Status,SBranchID,ClassID,SectionID,SessionID,EmployeeTypeID,VendorID
			,SerialNo,InvoiceNumber,FY)
				values(@TrDate,@TrType,@RefID,@RefType,@Remark,@CreatedDate,@Status,@SBranchID,@ClassID,@SectionID,@SessionID,@EmployeeTypeID,@VendorID,
				@SlNo,@InvNumber,@FYear)
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

CREATE  Procedure [dbo].[sp_GetStockTransactionPrintDetails]
(
@STID int,
@SBranchID int
)
AS
BEGIN
		select * from SBranchMaster where SBranchID=@SBranchID

		declare @ClassID int=0,@SessionID int=0,@SectionID int=0,@RefID int=0,@RefType int=0,@EmployeeTypeID int=0,@TrType int=0,@VendorID int=0,
		@Address nvarchar(max),@ContactNo nvarchar(20)

		Select @ClassID=ClassID,@SessionID=SessionID,@SectionID=SectionID,@RefID=RefID,@RefType=RefType,
		@EmployeeTypeID=EmployeeTypeID,@TrType=TrType,@VendorID=VendorID from StockTransactionMaster where STID=@STID
		
		Select * from StockTransactionMaster STM where STID=@STID

		Select STDID,STID,STType,ProductID,Quantity,SBranchID,Cost,MRP,SGST,CGST,IGST,
		(Select Name from ProductMaster PM where STD.ProductID=PM.ProductID) as ProductName 
		from StockTransactionDetails STD where STID=@STID

		if(@TrType=0)
		begin
			select CompanyName as Name,ContactNumber as ContactNo, Address,GSTIN,StateID,
			(select StateName+' ('+StateCode+')' from GSTStateMaster GSM where GSM.StateID=VM.StateID) as StateName
		   from VendorMaster VM where VendorID=@VendorID
		end
		else if(@RefType=0)
		begin
			Declare @StudentName nvarchar(100),@ClassSectionName nvarchar(100),
			@FatherName nvarchar(1000),@Gender int,@SessionName nvarchar(50),@ParentID int
			
			select @StudentName=Name +' ('+isnull(SchoolUID,'')+')',@Gender=Gender,@Address=MiniAddress,@ParentID=ParentID from StudentMaster where StudentID=@RefID
			
			select @ClassSectionName=ClassName+'->'+SectionName from v_ClassSectionNames where SectionID=@SectionID
			select @FatherName=FatherName,@ContactNo=FatherMobileNo from ParentMaster where ParentID=@ParentID
			select @SessionName=SessionName from SessionMaster where SessionID=@SessionID
			
			select @StudentName+' '+(case when @Gender=0 then 'D/o' else 'S/o' end)+' '+@FatherName as Name,@ContactNo as ContactNo,'Local State' as StateName,
			@ClassSectionName+' ('+@SessionName+')' as ExtraData,@Address as Address
		end
		else
		begin
			Declare @EmployeeName nvarchar(50),@EmpTypeName nvarchar(50)
			Select @EmpTypeName=EmployeeTypeName from EmployeeTypeMaster where EmployeeTypeID=@EmployeeTypeID

			select @EmployeeName=EmployeeName+' ('+isnull(@EmpTypeName,'')+')' ,@ContactNo=MobileNumber
			from EmployeeMaster where EmployeeID=@RefID 

			select @EmployeeName as Name,@ContactNo as ContactNo,'Local State' as StateName
		end
END
GO

	Create Function [dbo].[fn_GetStudentHostalFeeAmount] --4,2019,785,0,0,1,0,19,1,5,2,50,50,4
	(
	@Month int,
	@Year int,
	@StudentID int,
	@HostalFeeMode int,
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
			Declare @Amount decimal(18,2)=0,@ClassHostalFee numeric(18,2)
			Declare @Quota int
			declare @IsCustomFee int
			select top 1 @IsCustomFee=isnull(IsCustomFee,0) from Student_Session where StudentID=@StudentID and SessionID=@SessionID
			--select @Quota=QuotaID from Student_Session where StudentID=@StudentID and SessionID=@SessionID
			--select * from [StudentFeeDetails]
			if(@HostalFeeMode=1)
			begin
				select @ClassHostalFee=sum(isnull(isnull(SFD.FeeAmount,CFS.FeeAmount),0)) from ClassFeeStructureMaster CFS left outer join
				[dbo].[StudentFeeDetails] SFD on SFD.FeeTypeID=CFS.FeeTypeID and SFD.StudentID=@StudentID and SFD.SessionID=(case when @IsCustomFee=1 then @SessionID else -1 end)
				and SFD.IsApplicable=1
				where ClassID=@ClassID and GroupID=@GroupID and SBranchID=@SBranchID 
				and CFS.FeeTypeID in (select FeeTypeID from FeeTypeMaster where FeeTypeApplicable=6) and CFS.SessionID=@SessionID
			end
			if((select count(*) from TransportHostalAllocationDelocation where UserType=0 and ChangeType=1 and UserID=@StudentID)>0)
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
						(case when @HostalFeeMode=1 then 
						(@ClassHostalFee) else 
							(select isnull(Rate,0) from Hostel_RoomTypeRateMaster RTRM where RTRM.ID=(select RoomTypeID from Hostel_RoomMaster RM where RM.ID=KeyID)) end) as Amount from 
						(select (case when  StartDate is null or StartDate<@MStartDate then @MStartDate else StartDate end) as StartDate,
						(case when EndDate is null or EndDate>@MEndDate then @MEndDate else EndDate end) as EndDate
						, KeyID,ChangeType
						from TransportHostalAllocationDelocation where UserType=0 and ChangeType=1 and UserID=@StudentID and (StartDate<EndDate or EndDate is null)
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
ALTER Procedure [dbo].[sp_GetStudentFeeDetailViewData]
(
@StudentID int,
@SBranchID int,
@SessionID int,
@QDate date,
@CurDate date
)
as
BEGIN
	if(@SessionID=0)
	begin
		select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
	end

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
		select FTM.FeeTypeID,FTM.FeeTypeName,FTM.FeeTypeApplicable,CFS.FeeAmount,isnull(CFS.Status,1),FTM.Months
		from FeeTypeMaster FTM left outer join [dbo].[ClassFeeStructureMaster] CFS on CFS.FeeTypeID=FTM.FeeTypeID and ClassID=@ClassID 
		and GroupID=@GroupID
		and CFS.SessionID=@SessionID where FTM.SBranchID=@SBranchID
		
		Declare @StuSessionSDate date,@StuSessionEDate date,@StudentID int,@QuotaID int,@IsAdmissionFee int,
		@FeeAmount numeric(10,2),@PreviousDue numeric(10,2),@LateFee numeric(10,2),@Discounts numeric(10,2)
		,@LMonth int,@LYear int,@MonthType int,@BaseDate date,@Paid numeric(10,2),@FeePaymentMode int,@IsCustomFee int
		
		declare @isPaid int
		
			DECLARE stu_Cursor CURSOR FOR
			SELECT SessionStartDate,SessionEndDate,StudentID,QuotaID,IsAdmissionFee,FeeAmount,PreviousDue,LateFee,Discounts,Paid,FeePaymentMode
			FROM @Students where isnull(StudentID,0)!=0 FOR UPDATE OF FeeAmount,PreviousDue,LateFee,Discounts,Paid
			OPEN stu_Cursor
			FETCH NEXT FROM stu_Cursor
			INTO @StuSessionSDate,@StuSessionEDate,@StudentID,@QuotaID,@IsAdmissionFee,@FeeAmount,@PreviousDue,@LateFee,@Discounts,@Paid,@FeePaymentMode

			WHILE (@@FETCH_STATUS = 0)
			BEGIN
				declare @StudentSessionUID int
				set @Discounts=0
				select top 1 @StudentSessionUID=StudentSessionUID,@IsCustomFee=isnull(IsCustomFee,0) from Student_Session where StudentID=@StudentID and SessionID=@SessionID
				order by Status desc
				print(@StudentSessionUID)
				print(@FeePaymentMode)
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
							declare @PDIsc numeric(10,2)=0

							select @PPD=sum((case when isnull(PD.PaymentID,0)!=0 then PD.NetApplicablePayment-isnull(PD.DiscAmt,0)  
							when isnull(@IsCustomFee,0)=1 then SFD.FeeAmount else (FTS.FeeAmount-isnull(FTS.FeeAmount*QD.Discount/100,0)) end)) 
							,@PPP=isnull(Sum(PD.PaymentRecieved),0)
							from @FeeStructureTable FTS left outer join @QuotaDiscounts QD on QD.FeeTypeID=FTS.FeeTypeID
							left outer join [dbo].[StudentFeeDetails] SFD on SFD.StudentID=@StudentID and SFD.SessionID=@StudentSessionUID and 
							SFD.FeeTypeID=FTS.FeeTypeID
							left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
							where FTS.FeeTypeApplicable in (select FeeTypeID from @MTFT where MonthTypeID=@MonthType) and FTS.FeeTypeApplicable!=5
							and FTS.FeeTypeApplicable <> 7 and FTS.FeeTypeApplicable<>(Case when @IsAdmissionFee=0 then 8 else 0 end)
							and isnull(SFD.IsApplicable,FTS.Status)=1 and (FTS.FeeTypeApplicable<>9 or 
							(select count(*) from [dbo].[SplitStringToTable](FTS.Months,',') where Item=@LMonth)>0)

							select @isPaid=count(*) from v_PaymentDetails PD where PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
							declare @pDiscount numeric(10,2)=0
							if(isnull(@isPaid,0)=0)
							begin
								select @pDiscount=isnull(sum(ApprovedAmount),0) from @DiscountApproved where StudentID=@StudentID and FeeMonth=@LMonth and FeeYear=@LYear				
							end
							if((select count(*) from @FeeStructureTable where FeeTypeApplicable=5)>0)
							begin
							select @PreviousDue= isnull(@PreviousDue,0)+isnull([dbo].[fn_GetStudentTransportFeeAmount](@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID),0)
							end
							if((select count(*) from @FeeStructureTable where FeeTypeApplicable=6)>0)
							begin
							select @PreviousDue= isnull(@PreviousDue,0)+isnull([dbo].[fn_GetStudentHostalFeeAmount](@LMonth,@LYear,@StudentID,@HostelFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID),0)
							end
							select @PreviousDue=isnull(@PreviousDue,0)-isnull(Sum(isnull(PD.PaymentRecieved,0)),0)
							from @FeeStructureTable FTS left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
							where FTS.FeeTypeApplicable in (5,6)
							
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
							
							select @isPaid=count(*) from v_PaymentDetails PD where PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
							
							if(isnull(@isPaid,0)=0)
							begin
								select @Discounts=isnull(@Discounts,0)+isnull(sum(ApprovedAmount),0) from @DiscountApproved where StudentID=@StudentID and FeeMonth=@LMonth and FeeYear=@LYear
							end
							
							if((select count(*) from @FeeStructureTable where FeeTypeApplicable=5)>0)
							begin
							select  @FeeAmount=isnull(@FeeAmount,0)+[dbo].[fn_GetStudentTransportFeeAmount](@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID)
							end
							if((select count(*) from @FeeStructureTable where FeeTypeApplicable=6)>0)
							begin
							select  @FeeAmount=isnull(@FeeAmount,0)+[dbo].[fn_GetStudentHostalFeeAmount](@LMonth,@LYear,@StudentID,@HostelFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID)
							end
							select @TPaid=Sum(isnull(PD.PaymentRecieved,0))
							from @FeeStructureTable FTS left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID 
							and PD.Month=@LMonth and PD.Year=@LYear
							where FTS.FeeTypeApplicable in (5,6)

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
			SessionStartDate,SessionEndDate,IsAdmissionFee,SchoolUID,FeeAmount,PreviousDue,LateFee,Discounts,Paid from @Students where isnull(StudentID,0)!=0

	END
	GO
	ALTER procedure [dbo].[sp_GetStudentFeeDetailsNew]
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

		select top 1 @ClassID=SS.ClassID,@FeePaymentMode=FeePaymentMode,
		@QuotaID=SS.QuotaID,@SectionID=SS.SectionID,@StuSessionSDate=(Case when @SessionStartDate<isnull(SS.FromDate,@SessionStartDate) then SS.FromDate else @SessionStartDate end),
		@StuSessionEDate=(Case when @SessionEndDate>SS.ToDate then SS.ToDate else @SessionEndDate end),
		@IsAdmissionFee=SS.IsAdmissionFeeApplicable,@IsCustomFee=isnull(SS.IsCustomFee,0),@StudentSessionUID=StudentSessionUID
		from Student_Session SS 
		where SS.StudentID=@StudentID and SessionID=@SessionID 
		order by (case when @QDate between FromDate and ToDate then 0 else 1 end)asc,Status desc
		
		
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
		select FTM.FeeTypeID,FTM.FeeTypeName,FTM.FeeTypeApplicable,CFS.FeeAmount,isnull(CFS.Status,1),FTM.Months
		from FeeTypeMaster FTM left outer join [dbo].[ClassFeeStructureMaster] CFS on CFS.FeeTypeID=FTM.FeeTypeID 
		and ClassID=@ClassID and GroupID=@GroupID
		and CFS.SessionID=@SessionID where FTM.SBranchID=@SBranchID
		
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
					(case when FTS.FeeTypeApplicable=5 then [dbo].[fn_GetStudentTransportFeeAmount](@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID) 
					when FeeTypeApplicable=6 then [dbo].[fn_GetStudentHostalFeeAmount](@LMonth,@LYear,@StudentID,@HostelFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID)
					else FTS.FeeAmount end) as FeeAmount,
					QD.Discount as QuotaDiscount,
					(case when FTS.FeeTypeApplicable=7 then PD.DiscAmt when PD.PaymentID is not null then PD.DiscAmt else DA.ApprovedAmount end),PD.NetApplicablePayment,SFD.FeeAmount,PD.PaymentRecieved,
					(case when @IsCustomFee is null then 0 else @IsCustomFee end) as IsCustomFee,
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
			,sum((Case when FeeTypeApplicable in(5,6,7) then FeeAmount else CustomFee end)) as CustomFee,sum(PaidAmount) as PaidAmount,max(IsCustomFee) as IsCustomFee,max(IsPayment) as IsPayment from
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

	ALTER Function [dbo].[fn_GetStudentTransportFeeAmount] --4,2019,785,0,0,1,0,19,1,5,2,50,50,4
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
			declare @IsCustomFee int
			select top 1 @IsCustomFee=isnull(IsCustomFee,0) from Student_Session where StudentID=@StudentID and SessionID=@SessionID
			--select @Quota=QuotaID from Student_Session where StudentID=@StudentID and SessionID=@SessionID
			--select * from [StudentFeeDetails]
			if(@TransportFeeMode=1)
			begin
				select @ClassTransportFee=sum(isnull(isnull(SFD.FeeAmount,CFS.FeeAmount),0)) from ClassFeeStructureMaster CFS left outer join
				[dbo].[StudentFeeDetails] SFD on SFD.FeeTypeID=CFS.FeeTypeID and SFD.StudentID=@StudentID and SFD.SessionID=(case when @IsCustomFee=1 then @SessionID else -1 end) and SFD.IsApplicable=1
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
ALTER procedure [dbo].[spn_GetParentFeePayments]
(
	@ParentID int
)
as 
begin 
	Select [PaymentID],RIGHT(REPLICATE('0',6)+CAST([PaymentID] AS VARCHAR(6)),6) as [PaymentSID]
	 ,[PayeeID] ,[ReferanceNumber] ,[PaymentMode],
	 (Select SessionName from SessionMaster SM where SM.SessionID=PM.SessionID) as SessionName,
	 (Select Name from StudentMaster where StudentID=PayeeID) as PayeeName
      ,[Remark] ,[PaymentAmount] ,[PaymentStatus],[PaymentDate] ,Month,Year
      ,[PayeeType] ,[PaymentTitle] ,[EnteredDate] ,[CollectedBy],[dbo].[GetPaymentMonthNames](PaymentID) as PayMonths
	   from
	  [dbo].[PaymentMaster] PM
	  where PM.PayeeID in (select StudentID from StudentMaster where ParentID=@ParentID)

	  order by PaymentDate desc
end
GO
	----------------------------------------------------------------------------------------------------------------------------------------
	--Log Related
	CREATE TABLE [dbo].[OperationLogs](
	[LogID] [int] IDENTITY(1,1) NOT NULL,
	[LogDate] [datetime] NULL,
	[UserID] [int] NULL,
	[URL] [nvarchar](max) NULL,
	[Data] [nvarchar](max) NULL,
	[LogType] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[OperationLogs] ADD  CONSTRAINT [DF_OperationLogs_LogType]  DEFAULT ((0)) FOR [LogType]
GO


Create Procedure [dbo].[sp_AddOperationLog]
(
@LogDate datetime,
@UserID int,
@URL nvarchar(max),
@Data nvarchar(max)
)
AS
BEGIN
	
	declare @TableName nvarchar(50)='zOperationLogs_'+cast(datepart(month,@LogDate) as nvarchar(2))+'_'+cast(datepart(year,@LogDate) as nvarchar(4))
	if((select count(*) from sysobjects where type = 'U' and name like @TableName)=0)
		begin
			declare @CreateTableSQL nvarchar(max)
			set @CreateTableSQL='Select * into dbo.'+@TableName+'  from  OperationLogs '
			exec(@CreateTableSQL)			
		end
	Declare @InsertSQL nvarchar(max)
	set @InsertSQL='Insert into '+@TableName+'(LogDate,UserID,URL,Data) values('''+cast(@LogDate as nvarchar(20))+''','+cast(@UserID as nvarchar(10))+','''+@URL+''',N'''+isnull(@Data,'')+''')'
	exec(@InsertSQL)
	
END
	
-----------------------------------------------------------------------------------------------------------------------------------------------
11 May 2020
-------------------------------------------------------------------------------------------------------------------------------------------------
Alter Table [dbo].[ParentDiary]
Add Students nvarchar(max)
GO
ALTER proc [dbo].[sp_AddBulkParentDiary]
(
@TeacherID int,
@TeacherType int,
@PBDate datetime,
@Title nvarchar(500),
@Description nvarchar(max),	
@Priority	int,
@IsRead int,
@MasterID int,
@ClassID int,
@SectionID int,
@SubjectID int,
@Students ut_ParentDiary READONLY
)
AS
BEGIN
		Declare @StudentIDs nvarchar(max)
		set @StudentIDs=''
		
		select @StudentIDs=@StudentIDs+cast(StudentID as nvarchar(10))+',' from @Students

		select @MasterID=max(MasterID)+1 from [ParentDiary] 
		INSERT INTO  [dbo].[ParentDiary] (TeacherID,TeacherType,Students,PBDate,Title,Description,Priority,MasterID,ClassID,SectionID,SubjectID)
		 Values(@TeacherID,@TeacherType,@StudentIDs,@PBDate,@Title,@Description,@Priority,@MasterID,@ClassID,@SectionID,@SubjectID)

		select 1
END
GO
GO
ALTER proc [dbo].[sp_GetParentDiary]
(
@ParentID int
)
AS
BEGIN
	
	Declare @ParantDiary as Table(PBID int,TeacherID int,TeacherName nvarchar(50),TeacherType int,StudentID int,StudentName nvarchar(50)
	,PBDate datetime,Title nvarchar(max),[Description] nvarchar(max),[Priority] int,IsRead int,MasterID int,ClassID int,ClassName nvarchar(50),SectionID int,
	SectionName nvarchar(50),SubjectID int,SubjectName nvarchar(50))

	Declare @StudentID int,@ClassID int,@SectionID int,@StudentName nvarchar(100),@ClassName nvarchar(50),@SectionName nvarchar(50)

	DECLARE cursor_product CURSOR
	FOR select SM.StudentID,SM.Name,SS.ClassID,SS.SectionID from StudentMaster SM left outer join Student_Session SS on SS.StudentID=SM.StudentID 
	where SS.Status=1 and SM.ParentID=@ParentID

	OPEN cursor_product;
	FETCH NEXT FROM cursor_product INTO 
		@StudentID,@StudentName,@ClassID,@SectionID

	WHILE @@FETCH_STATUS = 0
		BEGIN
			select @ClassName=ClassName from ClassMaster where ClassID=@ClassID
			select @SectionName=Name from Class_Sections where ID=@SectionID

			Insert into @ParantDiary 
			Select [PBID] ,[TeacherID],
			(Select EmployeeName from EmployeeMaster where EmployeeID=PD.TeacherID) as TeacherName
			,[TeacherType] ,PD.StudentID	  ,@StudentName as StudentName
			  ,[PBDate]      ,[Title]      ,[Description]      ,[Priority]
			  ,[IsRead]      ,[MasterID]      ,[ClassID]   ,
			@ClassName as ClassName	 ,[SectionID]   ,@SectionName as SectionName
			   ,[SubjectID],
			(Select SubjectName from SubjectMaster where SubjectID=PD.SubjectID) as SubjectName
			  from dbo.ParentDiary PD 
			  where --PD.ClassID=@ClassID and PD.SectionID=@SectionID and 
			  ((select count(*) from [dbo].[SplitStringToTable](isnull(PD.Students,cast(PD.StudentID as nvarchar(10))+','),',') where Item=@StudentID)>0)
			
			
			FETCH NEXT FROM cursor_product INTO 
				@StudentID,@StudentName,@ClassID,@SectionID
		END;

	CLOSE cursor_product;

	DEALLOCATE cursor_product;
	
	select * from @ParantDiary
	  order by [PBDate] desc

END
GO

---------------------------------------------------------------------------------------------------------------------------------
--Assignment Submission Related
 Create Procedure dbo.sp_InsertParentAssignmentSubmission
  (
  @AssResponseID int,
  @AssignmentID int,
  @StudentID int,
  @SubmissionDate datetime,
  @Description nvarchar(max),
  @Status int,
  @Reason nvarchar(500),
  @Attachments nvarchar(max)
  )
  AS 
  BEGIN
	if(@AssResponseID=0)
	begin
		Insert into AssignmentSubmissions(AssignmentID,StudentID,SubmissionDate,Description,Status,Reason,Attachments)
		values(@AssignmentID,@StudentID,@SubmissionDate,@Description,@Status,@Reason,@Attachments)
		select @AssResponseID=Cast(Scope_Identity() as int)
	end
	else
	begin
		update AssignmentSubmissions set SubmissionDate=@SubmissionDate,Description=@Description,
		Status=@Status,Reason=@Reason,Attachments=@Attachments where AssResponseID=@AssResponseID
	end
	select @AssResponseID
  END
  GO
  
  Alter table AssignmentSubmissions
  Add Marks numeric(10,2)
  GO
  
ALTER proc [dbo].[sp_UpdateTeacherAssignmentSubmissions]
(
@AssignmentSubmissionID int,
@Status int,
@Comments nvarchar(max),
@Grade nvarchar(5)=null,
@Marks numeric(10,2)
)
AS
BEGIN
	Update [dbo].[AssignmentSubmissions] set [Status]=@Status, [Comments]=@Comments,Grade=@Grade,Marks=@Marks
	where [AssResponseID]=@AssignmentSubmissionID
	select @@ROWCOUNT
END
GO
Create Procedure dbo.sp_GetStudentAssignmentResponse
  (
  @AssignmentID int,
  @StudentID int
  )
  AS
  Begin
	Select AssResponseID,AssignmentID,AM.StudentID,SubmissionDate,Description,Status
	,SM.Name as StudentName,'/Images/StudentImage/'+Cast(SM.StudentID as nvarchar(10))+'_'+SM.Photo as Photo,
	(Case when Status=1 then 'Submitted' when Status=2 then 'Skipped' when Status=3 then 'Accepted' when Status=4 then 'Rejected' else 'Pending' end) as StatusText,
	(Case when Status=1 then '#0447F9' when Status=2 then '#F9DF04' when Status=3 then '#4EF904' when Status=4 then '#F94304' else '#F904E3' end) as StatusColor,
	Reason,Comments,Attachments,Grade,Marks
	from AssignmentSubmissions AM left outer join StudentMaster SM on AM.StudentID=SM.StudentID
	where AssignmentID=@AssignmentID and AM.StudentID=@StudentID
  End
GO

ALTER proc [dbo].[spn_GetStudentAssignments]
(
@StudentID int
)
AS
BEGIN		
		declare @ClassID int
		declare @SectionID int
		 select @SectionID=SectionID,@ClassID=ClassID from Student_Session 
		where studentID=@StudentID and Status=1 
		declare @StudentName nvarchar(100)
		select @StudentName = Name from StudentMaster where StudentID=@StudentID
		
		SELECT [ID] ,[ChapterID] ,ASS.Status,ASS.Marks,ASS.Grade,
		(Case when ASS.Status=1 then 'Submitted' when ASS.Status=2 then 'Skipped' when ASS.Status=3 then 'Accepted' when ASS.Status=4 then 'Rejected' else 'Pending' end) as StatusText,
		(Case when ASS.Status=1 then '#0447F9' when ASS.Status=2 then '#F9DF04' when ASS.Status=3 then '#4EF904' when ASS.Status=4 then '#F94304' else '#F904E3' end) as StatusColor,
		(Select SubjectName from SubjectMaster SM where SM.SubjectID=AM.SubjectID) as SubjectName,
		(Select EmployeeName from EmployeeMaster EM where EM.EmployeeID=AM.TeacherID) as TeacherName,
		[TopicID],(Select isnull(ChapterName,'No Chapter') from ChapterMaster CM where CM.ChapterID=AM.ChapterID)+' > '+(Select isnull(TopicName,'No Topic') from TopicMaster TM where TM.TopicID=AM.TopicID) as TopicName,
		[StartDate] ,[Enddate] ,[GracedDays],[TaskType] ,[Title] ,[Detail],AM.[Attachments] ,[SendMail] ,[CreatedOn],@StudentName as StudentName
		FROM [dbo].[AssignmentMaster] AM left outer join AssignmentSubmissions ASS on ASS.StudentID=@StudentID and ASS.AssignmentID=AM.ID
		
		where ClassID=@ClassID and SectionID=@SectionID
		order by StartDate desc
END

GO

ALTER proc [dbo].[sp_GetAssignmentSubmissions]
(
@AssignmentID int
)
AS
BEGIN
	declare @ClassID int
	declare @SectionID int
	declare @SubjectID int
	declare @ChapterID int
	declare @TopicID int
	declare @AssignmentTitle nvarchar(Max)

	select @AssignmentTitle=Title, @ClassID=ClassID,@SectionID=SectionID,@SubjectID=SubjectID,@ChapterID=ChapterID,@TopicID=TopicID from AssignmentMaster
	where ID=@AssignmentID

	select ClassName from ClassMaster where ClassID=@ClassID
	Select Name from Class_Sections where ID=@SectionID
	select SubjectName from SubjectMaster where SubjectID=@SubjectID
	select ChapterName from ChapterMaster where ChapterID=@ChapterID
	select TopicName from TopicMaster where TopicID=@TopicID
	select @AssignmentTitle

	SELECT [AssResponseID] ,[AssignmentID] ,SM.Gender,
	AM.[StudentID],SM.Name as StudentName,'/Images/StudentImage/'+Cast(SM.StudentID as nvarchar(10))+'_'+SM.Photo as Photo,Grade
	,[SubmissionDate] ,[Status],[Attachments] ,
	(Case when Status=1 then 'Submitted' when Status=2 then 'Skipped' when Status=3 then 'Accepted' when Status=4 then 'Rejected' else 'Pending' end) as StatusText,
	(Case when Status=1 then '#0447F9' when Status=2 then '#F9DF04' when Status=3 then '#4EF904' when Status=4 then '#F94304' else '#F904E3' end) as StatusColor,
	Marks
	FROM [dbo].[AssignmentSubmissions] AM left outer join StudentMaster SM on AM.StudentID=SM.StudentID
	where AssignmentID=@AssignmentID	
END
GO
ALTER proc [dbo].[sp_GetAssignmentSubmissions] 
(
@AssignmentID int
)
AS
BEGIN
	declare @ClassID int
	declare @SectionID int
	declare @SubjectID int
	declare @ChapterID int
	declare @TopicID int
	declare @AssignmentTitle nvarchar(Max)

	select @AssignmentTitle=Title, @ClassID=ClassID,@SectionID=SectionID,@SubjectID=SubjectID,@ChapterID=ChapterID,@TopicID=TopicID from AssignmentMaster
	where ID=@AssignmentID

	select ClassName from ClassMaster where ClassID=@ClassID
	Select Name from Class_Sections where ID=@SectionID
	select SubjectName from SubjectMaster where SubjectID=@SubjectID
	select ChapterName from ChapterMaster where ChapterID=@ChapterID
	select TopicName from TopicMaster where TopicID=@TopicID
	select @AssignmentTitle

	SELECT [AssResponseID] ,[AssignmentID] ,SM.Gender,
	AM.[StudentID],SM.Name as StudentName,'/Images/StudentImage/'+Cast(SM.StudentID as nvarchar(10))+'_'+SM.Photo as Photo,Grade
	,[SubmissionDate] ,[Status],[Attachments] ,
	(Case when Status=1 then 'Submitted' when Status=2 then 'Skipped' when Status=3 then 'Accepted' when Status=4 then 'Rejected' else 'Pending' end) as StatusText,
	(Case when Status=1 then '#0447F9' when Status=2 then '#F9DF04' when Status=3 then '#4EF904' when Status=4 then '#F94304' else '#F904E3' end) as StatusColor,
	Marks
	FROM [dbo].[AssignmentSubmissions] AM left outer join StudentMaster SM on AM.StudentID=SM.StudentID
	where AssignmentID=@AssignmentID	
END
GO
ALTER procedure [dbo].[spn_GetStudentsTransportDetailsNew2] --244,1,0
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
	where IsApproved=1 and SBranchID=@SBranchID --and RouteID in (select RouteID from TransportRouteDetails where AreaID = @AreaID)

	exec spn_GetTransportRouteVehicleList @SBranchID,@RouteID

	exec spn_GetVehicleRouteStoppages @VehicleRouteID

	select * from [TransportHostalAllocationDelocation] where THChangeID=@THChangeID

end
GO

ALTER proc [dbo].[sp_GetAssignmentSubmissions]
(
@AssignmentID int
)
AS
BEGIN
	declare @ClassID int
	declare @SectionID int
	declare @SubjectID int
	declare @ChapterID int
	declare @TopicID int
	declare @AssignmentTitle nvarchar(Max)

	select @AssignmentTitle=Title, @ClassID=ClassID,@SectionID=SectionID,@SubjectID=SubjectID,@ChapterID=ChapterID,@TopicID=TopicID from AssignmentMaster
	where ID=@AssignmentID

	select ClassName from ClassMaster where ClassID=@ClassID
	Select Name from Class_Sections where ID=@SectionID
	select SubjectName from SubjectMaster where SubjectID=@SubjectID
	select ChapterName from ChapterMaster where ChapterID=@ChapterID
	select TopicName from TopicMaster where TopicID=@TopicID
	select @AssignmentTitle

	SELECT [AssResponseID] ,[AssignmentID] ,SM.Gender,
	AM.[StudentID],SM.Name as StudentName,'/Images/StudentImage/'+Cast(SM.StudentID as nvarchar(10))+'_'+SM.Photo as Photo,Grade
	,[SubmissionDate] ,[Status],[Attachments] ,Description,
	(Case when Status=1 then 'Submitted' when Status=2 then 'Skipped' when Status=3 then 'Accepted' when Status=4 then 'Rejected' else 'Pending' end) as StatusText,
	(Case when Status=1 then '#0447F9' when Status=2 then '#F9DF04' when Status=3 then '#4EF904' when Status=4 then '#F94304' else '#F904E3' end) as StatusColor,
	Marks
	FROM [dbo].[AssignmentSubmissions] AM left outer join StudentMaster SM on AM.StudentID=SM.StudentID
	where AssignmentID=@AssignmentID	
END
GO

ALTER proc [dbo].[sp_GetAppGalleryList]
(
@SBranchID int
)
AS
BEGIN
	if(@SBranchID=0)
	begin
		Select GalleryID,Title,EventDate,Description,
		(select top 1 cast(GalleryID as nvarchar(10))+'_'+ ImagePath as ImagePath from GalleryImages GI where GI.GalleryID=GM.GalleryID order by GalleryImageID) as FeatureImage
		from GalleryMaster GM where [Status]=1
		order by EventDate desc
	end
	else
	begin
		Select GalleryID,Title,EventDate,Description,
		(select top 1 cast(GalleryID as nvarchar(10))+'_'+ ImagePath as ImagePath from GalleryImages GI where GI.GalleryID=GM.GalleryID order by GalleryImageID) as FeatureImage
		from GalleryMaster GM 
		where isnull(SBranchID,@SBranchID)=@SBranchID and [Status]=1
		order by EventDate desc
	end
END

GO

ALTER proc [dbo].[sp_GetAssignmentList]
(
@ClassID int,
@SectionID int,
@SubjectID int,
@ChapterID int,
@TeacherID int
)
AS
BEGIN		
		
	SELECT [ID] ,[ChapterID] ,
		[TopicID],(Select isnull(ChapterName,'No Chapter') from ChapterMaster CM where CM.ChapterID=AM.ChapterID)+' > '+(Select isnull(TopicName,'No Topic') from TopicMaster TM where TM.TopicID=AM.TopicID) as TopicName
		,[StartDate] ,[Enddate] ,[GracedDays],[TaskType] ,[Title] ,[Detail],[Attachments] ,[SendMail] ,[CreatedOn],
		(Select SubjectName from SubjectMasterAll SMA where SMA.SubjectID=AM.SubjectID) as SubjectName,
		(Select Count(*) from AssignmentSubmissions ASub where ASub.AssignmentID=AM.ID and ASub.Status=1) as Submissions
		FROM [dbo].[AssignmentMaster] AM where ClassID=@ClassID and SectionID=@SectionID and SubjectID=@SubjectID  and TeacherID=@TeacherID
	
	
END
GO
Create Procedure dbo.sp_GetAssignmentDetails
(
@AssignmentID int,
@TeacherID int
)
AS
BEGIN
		declare @ClassID int,@SectionID int,@SubjectID int

		SELECT @ClassID=ClassID,@SectionID=SectionID,@SubjectID=SubjectID
		FROM [dbo].[AssignmentMaster] AM where ID=@AssignmentID and TeacherID=@TeacherID

		declare @ClassesTable  table (ID int,Name nvarchar(20))
		declare @SectionsTable  table (ID int,Name nvarchar(20),ClassID int)
		declare @SubjectsTable  table (ID int,Name nvarchar(50))
		INSERT INTO @SectionsTable(ID,Name,ClassID) select ID,Name,ClassID from Class_Sections where ID in (Select SectionID from Time_Table_Master where
		(MondayTeacherID=@TeacherID or TuesdayTeacherID=@TeacherID or WednesdayTeacherID=@TeacherID 
		or ThursdayTeacherID=@TeacherID or FridayTeacherID=@TeacherID or SaturdayTeacherID=@TeacherID)
		and PeriodID in (select PeriodID from PeriodMaster))

		Insert into @ClassesTable(ID,Name) select ClassID,ClassName from ClassMaster where ClassID in 
		(select ClassID from @SectionsTable)
		
		if(isnull(@SectionID,0)=0)
		begin
			select @ClassID =min(ID) from @ClassesTable 			
			select @SectionID=min(ID) from @SectionsTable where ClassID=@ClassID
		end
		select ID,Name from @ClassesTable 
		select ID,Name from @SectionsTable where ClassID=@ClassID	

		Insert into @SubjectsTable(ID,Name) Select SubjectID, SubjectName  from SubjectMaster where SubjectID in
		(Select MondaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and MondayTeacherID=@TeacherID
			union
		Select TuesdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and TuesdayTeacherID=@TeacherID
			union
		Select WednesdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and WednesdayTeacherID=@TeacherID
			union
		Select ThursdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and ThursdayTeacherID=@TeacherID
			union
		Select FridaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and FridayTeacherID=@TeacherID
			union
		Select SaturdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and SaturdayTeacherID=@TeacherID)
		if(isnull(@SubjectID,0)=0)
		begin
			select @SubjectID=min(ID) from @SubjectsTable
		end
		select ID,Name from @SubjectsTable

		SELECT *
		FROM [dbo].[AssignmentMaster] AM where ID=@AssignmentID and TeacherID=@TeacherID
				
		select @ClassID
		select @SectionID
		select @SubjectID
END
GO
Select * into bbIBackup from BlackBoardImages
GO
ALTER TABLE BlackBoardImages 
ALTER COLUMN Photo nvarchar(500)
GO
ALTER procedure [dbo].[sp_InsertBlackBoardImage]
(
  @BlackBoardID nvarchar(50),
  @BBMIID nvarchar(50),
  @Photo nvarchar(500),
  @Detail nvarchar(MAX),
  @SBranchID int
)
AS
BEGIN
	Insert into BlackBoardImages(BlackBoardID,BBMIID,Photo,Detail,SBranchID)
	Values(@BlackBoardID,@BBMIID,@Photo,@Detail,@SBranchID)
	select 1
END
GO


ALTER proc [dbo].[sp_GetAssignmentSubmissions]
(
@AssignmentID int
)
AS
BEGIN
	declare @ClassID int
	declare @SectionID int
	declare @SubjectID int
	declare @ChapterID int
	declare @TopicID int
	declare @AssignmentTitle nvarchar(Max)
	declare @AssignmentStartDate date,@GracedDays int,@Details nvarchar(max)
	declare @AssignmentEndDate date

	select @AssignmentTitle=Title, @ClassID=ClassID,@SectionID=SectionID,@SubjectID=SubjectID,@ChapterID=ChapterID,@TopicID=TopicID ,
	@AssignmentStartDate=StartDate,@AssignmentEndDate=EndDate,@GracedDays=GracedDays,@Details=Detail
	from AssignmentMaster
	where ID=@AssignmentID


	select ClassName from ClassMaster where ClassID=@ClassID
	Select Name from Class_Sections where ID=@SectionID
	select SubjectName from SubjectMaster where SubjectID=@SubjectID
	select ChapterName from ChapterMaster where ChapterID=@ChapterID
	select TopicName from TopicMaster where TopicID=@TopicID
	select @AssignmentTitle

	SELECT [AssResponseID] ,[AssignmentID] ,SM.Gender,
	AM.[StudentID],SM.Name as StudentName,'/Images/StudentImage/'+Cast(SM.StudentID as nvarchar(10))+'_'+SM.Photo as Photo,Grade
	,[SubmissionDate] ,[Status],[Attachments] ,Description,
	(Case when Status=1 then 'Submitted' when Status=2 then 'Skipped' when Status=3 then 'Accepted' when Status=4 then 'Rejected' else 'Pending' end) as StatusText,
	(Case when Status=1 then '#0447F9' when Status=2 then '#F9DF04' when Status=3 then '#4EF904' when Status=4 then '#F94304' else '#F904E3' end) as StatusColor,
	Marks
	FROM [dbo].[AssignmentSubmissions] AM left outer join StudentMaster SM on AM.StudentID=SM.StudentID
	where AssignmentID=@AssignmentID	
	order by AssResponseID

	Select @AssignmentID as ID, @AssignmentTitle as Title,@AssignmentStartDate as StartDate,@AssignmentEndDate as EndDate,@GracedDays as GracedDays,@Details as Detail 
END
GO
---------------------------------------------------------------------------------------------------------------------------------------------
--23 May 2020
ALTER FUNCTION [dbo].[GetBlackBoardImageList]
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
			 set @Images=@Images+@BlackBoardID+'_'+@Photo
		end
		 FETCH NEXT FROM FirstCursor INTO @Photo
	end
close FirstCursor 
deallocate FirstCursor 
return @Images
end
GO
Create Procedure [dbo].[sp_GetTeacherPanelBlackBoardList] 
(
	@TeacherID int,
	@SubjectID int,
	@ClassID int,
	@SectionID int,
	@PageID int=0,
	@StartDate datetime,
	@EndDate datetime
)
AS
BEGIN
	select BBM.*,
	PM.Name+'('+cast(PM.StartTime as nvarchar(5))+'-'+ cast(PM.EndTime as nvarchar(5))+')' as PeriodName,
	[dbo].[GetBlackBoardImageList](BBM.BlackBoardID) as Images, ROW_NUMBER() OVER (ORDER BY BDate desc) AS RowNum
	from dbo.BlackBoardMaster BBM 
	left outer join PeriodMaster PM on PM.PeriodID=BBM.PeriodID
	where BBM.ClassID=@ClassID and BBM.SectionID=@SectionID and BBM.SubjectID=@SubjectID and BBM.TeacherID=@TeacherID
	and cast(BDate as date) between @StartDate and @EndDate


	--SELECT *
	--FROM ( select BBM.*,
	--PM.Name+'('+cast(PM.StartTime as nvarchar(5))+'-'+ cast(PM.EndTime as nvarchar(5))+')' as PeriodName,
	--[dbo].[GetBlackBoardImageList](BBM.BlackBoardID) as Images, ROW_NUMBER() OVER (ORDER BY BDate desc) AS RowNum
	--from dbo.BlackBoardMaster BBM 
	--left outer join PeriodMaster PM on PM.PeriodID=BBM.PeriodID
	--where BBM.ClassID=@ClassID and BBM.SectionID=@SectionID and BBM.SubjectID=@SubjectID and BBM.TeacherID=@TeacherID
	--)t where t.rownum between @PageID*20 and (@PageID+1)*20
END
GO
Create proc [dbo].[sp_GetTeacherPanelBlackBoard]
(
@TeacherID int,
@ClassID int,
@SectionID int,
@SubjectID int,
@SBranchID int,
@StartDate date,
@EndDate date,
@PageID int
)
AS
BEGIN
		declare @ClassesTable  table (ID int,Name nvarchar(20))
		declare @SectionsTable  table (ID int,Name nvarchar(20))
		declare @EvaluationsTable  table (ID int,Name nvarchar(20))
		declare @SubjectsTable  table (ID int,Name nvarchar(50))
		INSERT INTO @SectionsTable(ID,Name) select ID,Name from Class_Sections where ID in (Select SectionID from Time_Table_Master where
		MondayTeacherID=@TeacherID or TuesdayTeacherID=@TeacherID or WednesdayTeacherID=@TeacherID 
		or ThursdayTeacherID=@TeacherID or FridayTeacherID=@TeacherID or SaturdayTeacherID=@TeacherID)

		Insert into @ClassesTable(ID,Name) select ClassID,ClassName from ClassMaster where ClassID in 
		(Select ClassID from Class_Sections where ID in (select ID from @SectionsTable))
		
		if(@SectionID=0)
		begin
			select @ClassID =min(ID) from @ClassesTable 			
			select @SectionID=min(ID) from @SectionsTable where ID in (Select ID from Class_Sections where ClassID=@ClassID)
		end
		select ID,Name from @ClassesTable 
		select ID,Name from @SectionsTable where ID in (Select ID from Class_Sections where ClassID=@ClassID)	

		Insert into @SubjectsTable(ID,Name) Select SubjectID, SubjectName  from SubjectMaster where SubjectID in
		(Select MondaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and MondayTeacherID=@TeacherID
			union
		Select TuesdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and TuesdayTeacherID=@TeacherID
			union
		Select WednesdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and WednesdayTeacherID=@TeacherID
			union
		Select ThursdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and ThursdayTeacherID=@TeacherID
			union
		Select FridaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and FridayTeacherID=@TeacherID
			union
		Select SaturdaySubjectID as ID from Time_Table_Master where ClassID=@ClassID and SectionID=@SectionID and SaturdayTeacherID=@TeacherID)
		if(@SubjectID=0)
		begin
			select @SubjectID=min(ID) from @SubjectsTable
		end
		select ID,Name from @SubjectsTable
				
		select isnull(@ClassID,0)
		select isnull(@SectionID,0)
		select isnull(@SubjectID,0)

		exec [dbo].[sp_GetTeacherPanelBlackBoardList] @TeacherID,@SubjectID,@ClassID,@SectionID,@PageID,@StartDate,@EndDate
END
GO

Create procedure dbo.sp_GetTeacherSelectionListsForBB
(
@EntryDate datetime,
@TeacherID int
)
AS
BEGIN
	Declare @SelectionList as Table(SubjectID int,PeriodID int,ClassID int,SectionID int,PeriodName nvarchar(50),
	ClassName nvarchar(50),SectionName nvarchar(50),SubjectName nvarchar(50))

	Declare @TimePart time,@EduID int,@ClassID int,@SectionID int,@SubjectID int,@PeriodID int
	declare @DayName nvarchar(10)=datename(dw,@EntryDate)
	set @TimePart=cast(@EntryDate as time)
	if(@DayName='Sunday')
	begin
		set @EntryDate = dateadd(day,-1,@EntryDate)
		set @DayName =datename(dw,@EntryDate)
	end

	
	declare @ListSQL nvarchar(max)
	set @ListSQL='select TTM.'+@DayName+'SubjectID as SubjectID,TTM.PeriodID,TTM.ClassID,TTM.SectionID,
		PM.Name+'' (''+cast(PM.StartTime as nvarchar(5))+''-''+ cast(PM.EndTime as nvarchar(5))+'')'' as PeriodName,
		CM.ClassName,CS.Name as SectionName,SM.SubjectName from 
		Time_Table_Master TTM left outer join PeriodMaster PM on PM.PeriodID=TTM.PeriodID
		left outer join ClassMaster CM on CM.ClassID=TTM.ClassID
		left outer join Class_Sections CS on CS.ID=TTM.SectionID
		left outer join SubjectMasterAll SM on SM.SubjectID=TTM.'+@DayName+'SubjectID
		where '+@DayName+'TeacherID='+Cast(@TeacherID as nvarchar(50))
		insert into @SelectionList exec(@ListSQL)

		delete from @SelectionList where PeriodName is null or SubjectName is null

		select distinct ClassID as ID,ClassName as Name from @SelectionList
		select distinct SectionID as ID,SectionName as Name,ClassID as Extra1 from @SelectionList
		select distinct PeriodID as ID,isnull(PeriodName,'')+' -> '+isnull(SubjectName,'') as Name,SubjectID as Extra1,ClassID as Extra2,SectionID as Extra3 from @SelectionList

		select  top 1 @ClassID= ClassID from @SelectionList 
		select  top 1 @SectionID= SectionID from @SelectionList where ClassID=@ClassID
		select  top 1 @PeriodID= PeriodID,@SubjectID=SubjectID from @SelectionList where ClassID=@ClassID and SectionID=@SectionID

		select isnull(@ClassID,0)
		select isnull(@SectionID,0)
		select isnull(@PeriodID,0)
		select isnull(@SubjectID,0)

		select @EntryDate
END
GO
Create Procedure [dbo].[sp_GetTeacherBlackBoardEditPageData]
(
	@BlackBoardID nvarchar(50),
	@TeacherID int,
	@EntryDate datetime
)
AS
BEGIN
	
	declare @TempEntryDate datetime
	select @TempEntryDate=BDate from BlackBoardMaster where BlackBoardID=@BlackBoardID

	if(@TempEntryDate!=null)
	begin
		set @EntryDate=@TempEntryDate
	end

	

	select * from BlackBoardMaster where BlackBoardID=@BlackBoardID

	select * from BlackBoardImages where BlackBoardID=@BlackBoardID

	exec dbo.sp_GetTeacherSelectionListsForBB @EntryDate,@TeacherID
		
END
GO
CREATE TYPE [dbo].[ut_BlackBoardImages] AS TABLE(
	[BlackBoardID] [nvarchar](50) NULL,
	[BBMIID] [nvarchar](50) NULL,
	[Photo] [nvarchar](500) NULL,
	[Detail] [nvarchar](max) NULL,
	[SBranchID] [int] NULL
)
GO
Create procedure [dbo].[sp_UpdateTeacherBlackBoardEntry]
(
	@BlackBoardID nvarchar(50),
	@TeacherID int,
	@SectionID int,
	@ClassID int,
	@PeriodID int,
	@SubjectID int,
	@Title nvarchar(MAX),
	@BDate datetime,
	@SBranchID int,
	@OpType int,
	@Images ut_BlackBoardImages READONLY
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from BlackBoardMaster where BlackBoardID=@BlackBoardID
		Delete from BlackBoardImages where BlackBoardID=@BlackBoardID
	end
	else
	begin
		if(@OpType=1)
		begin
			Insert into BlackBoardMaster(BlackBoardID,TeacherID,SectionID,ClassID,PeriodID,SubjectID,Title,BDate,SBranchID)
			Values(@BlackBoardID,@TeacherID,@SectionID,@ClassID,@PeriodID,@SubjectID,@Title,@BDate,@SBranchID)			
		end
		else
		begin
			Update BlackBoardMaster set TeacherID=@TeacherID,SectionID=@SectionID,ClassID=@ClassID,PeriodID=@PeriodID,SubjectID=@SubjectID,Title=@Title
			,BDate=@BDate,SBranchID=@SBranchID where BlackBoardID=@BlackBoardID
		end
		Delete from BlackBoardImages where BlackBoardID=@BlackBoardID
		insert into BlackBoardImages select @BlackBoardID,BBMIID,Photo,Detail,@SBranchID from @Images
	end
	select 1
END
GO

alter table ParentDiary
Add SessionID int
GO

Create Procedure dbo.sp_GetTeacherParentDiaryHistory
(
@TeacherID int,
@StartDate date,
@EndDate date,
@SessionID int,
@SBranchID int
)
AS
BEGIN
	if(@SessionID=0)
	begin
		select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
	end
	select PBID,PD.TeacherID,PBDate,Title,Description ,Students as StudentList
	,(Case when PD.SubjectID=0 then 'Class Teacher' else SM.SubjectName end) as SubjectName,
	CM.ClassName,CS.Name as SectionName
	from ParentDiary PD left outer join ClassMaster CM on CM.ClassID=PD.ClassID
	left outer join Class_Sections CS on CS.ID=PD.SectionID
	left outer join SubjectMasterAll SM on SM.SubjectID=PD.SubjectID
	where PD.TeacherID=@TeacherID and Cast(PD.PBDate as date) between @StartDate and @EndDate
	and isnull(PD.SessionID,@SessionID)=@SessionID

	Select SessionID as ID,SessionName as Name, SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID
	
	select isnull(@SessionID,0)
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
	declare @EduLevel int
	select @EduLevel=EducationLevelID from ClassMaster where ClassID=@ClassID
	declare @Days int
	select @Days=Days from EducationLevelMaster where ID=@EduLevel

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
	from StudentAttendanceMasterT where StudentID=@StudentID and FYear=@Year and [Month]=@Month

	Declare @AttSummery as Table (DayID int,Status numeric(10,2))
	insert into @AttSummery
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

		declare @tDayID int,@tStatus numeric(10,2)
		DECLARE db_Attendancecursor CURSOR FOR  
		select DayID,Status from @AttSummery FOR UPDATE OF [Status]
		OPEN db_Attendancecursor   
		FETCH NEXT FROM db_Attendancecursor INTO @tDayID,@tStatus
		WHILE @@FETCH_STATUS = 0   
		BEGIN   
			declare @CDate date=cast(@Year as nvarchar(10))+'-'+cast(@Month as nvarchar(10))+'-'+cast(@tDayID as nvarchar(10))
			declare @WD int=isnull(nullif(DATEPART(dw,@CDate)-1,0),7)
			if(@WD>@Days and @tStatus=3)
			begin
				UPDATE @AttSummery SET [Status] = -2 WHERE CURRENT OF db_Attendancecursor
			end
			else if(@WD<=@Days and @tStatus=-2)
			begin
				UPDATE @AttSummery SET [Status] = 3 WHERE CURRENT OF db_Attendancecursor
			end

		FETCH NEXT FROM db_Attendancecursor INTO  @tDayID,@tStatus
		END   
		CLOSE db_Attendancecursor   
		DEALLOCATE db_Attendancecursor
		select * from @AttSummery
END
GO
ALTER proc [dbo].[sp_GetTeacherParentDiaryRecieverList]
(
@MasterID int,
@TeacherID int
)
AS
BEGIN
	Select PBDate,Title,Description,min(PBID) as PBID,
	(case when TeacherType=1 then (Select SubjectName from SubjectMaster SM where T.SubjectID=SM.SubjectID) else 'Class Teacher' end) as SubjectName
	 from ParentDiary T where MasterID=@MasterID and TeacherID=@TeacherID
	 group by PBDate,Title,Description,SubjectID,TeacherType

	Select isnull(IsRead,0) IsRead,
	(Select Name from StudentMaster SM where SM.StudentID=T.StudentID) as StudentName
	from ParentDiary T where MasterID=@MasterID and TeacherID=@TeacherID
END
GO
ALTER proc [dbo].[sp_GetTeacherParentDiaryList]
(
@TeacherID int,
@ClassID int,
@SectionID int,
@SubjectID int=0
)
AS
BEGIN
Select T.MasterID,T.Title,T.EntryDate,T.TeacherType,T.SubjectID,Description,T.Reciever,PBID,
(case when TeacherType=1 then (Select SubjectName from SubjectMaster SM where T.SubjectID=SM.SubjectID) else 'Class Teacher' end) as SubjectName from
(select MasterID,Title,convert(NVARCHAR, PBDate, 106) as EntryDate,TeacherType,SubjectID,Description,count(*) as Reciever,min(PBID) as PBID
from ParentDiary where TeacherID=@TeacherID and ClassID=@ClassID and SectionID=@SectionID and SubjectID=@SubjectID
group by MasterID,Title,PBDate,TeacherType,SubjectID,Description)T
END

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
--Start handle and clear Multiple ParentDiary entry into new single entry logic 25 may 2020 Sourabh
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
GO
Alter TABLE ParentDiary
Add SessionID int
GO
Select *  into  dbo.ParentDiaryBKP_25052020 from  ParentDiary
GO
create function dbo.fn_GetPDStudentString(@TeacherID int,@TeacherType int,@PBDate datetime,@SubjectID int,@Title nvarchar(max))
Returns nvarchar(max)
as
Begin
	declare @tmp nvarchar(max)
	SET @tmp = ''
	Select @tmp = @tmp + cast(StudentID as nvarchar(10)) + ', ' from ParentDiary where TeacherID=@TeacherID and PBDate=@PBDate 
	and Title=@Title and TeacherType=@TeacherType and SubjectID=@SubjectID
	return @tmp
End

GO

Create Table #PDTempTable(TeacherID int,TeacherType int,PBDate datetime,Title nvarchar(max),Description nvarchar(max),Priority int,IsRead int,MasterID int
,ClassID int,SectionID int,SubjectID int,SessionID int,Students nvarchar(max))

insert into #PDTempTable
select distinct TeacherID,TeacherType,PBDate,Title,Description,Priority,IsRead,MasterID,ClassID,SectionID,SubjectID
,(Case when Students is null then dbo.fn_GetPDStudentString(TeacherID,TeacherType,PBDate,SubjectID,Title) else Students end) as Students,SessionID 
from ParentDiary
GO
Drop function dbo.fn_GetPDStudentString
GO
truncate table ParentDiary
GO
insert into ParentDiary(TeacherID,TeacherType,PBDate,Title,Description,Priority,IsRead,MasterID,ClassID,SectionID,SubjectID ,Students,SessionID)
select * from #PDTempTable
Drop Table #PDTempTable

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
--End handle and clear Multiple ParentDiary entry into new single entry logic
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

GO

Create Procedure dbo.sp_DeleteParentDiary
(
@PBID int,
@TeacherID int
)
AS
BEGIN
	Delete from ParentDiary where PBID=@PBID and TeacherID=@TeacherID
	select @@Rowcount
END
GO
ALTER proc [dbo].[sp_GetTeacherParentDiaryList]
(
@TeacherID int,
@ClassID int,
@SectionID int,
@SubjectID int=0
)
AS
BEGIN
	Select T.MasterID,T.Title,convert(NVARCHAR, PBDate, 106) as EntryDate,T.TeacherType,T.SubjectID,Description,PBID,
	(case when TeacherType=1 then (Select SubjectName from SubjectMaster SM where T.SubjectID=SM.SubjectID) else 'Class Teacher' end) as SubjectName from
	ParentDiary T where TeacherID=@TeacherID and ClassID=@ClassID and SectionID=@SectionID and SubjectID=@SubjectID
END
GO

ALTER proc [dbo].[sp_GetStudentSessionEditDataEnt]
(
@StudentSessionUID int,
@SBranchID int
)
AS
BEGIN
	  declare @SessionID int
	  declare @ClassID int
	  declare @GroupID int
	  declare @SectionID int
	  if(@StudentSessionUID=0)
	  begin
		select @ClassID=min(ClassID) from ClassMaster where SBranchID=@SBranchID
		select Top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
		select @SectionID=min(ID) from Class_Sections where ClassID=@ClassID
	  end
	  else
	  begin
		select @ClassID=ClassID,@SessionID=SessionID,@SectionID=SectionID from Student_Session SS where SS.StudentSessionUID=@StudentSessionUID and SS.SBranchID=@SBranchID
	  end


	if(@StudentSessionUID=0)
	  begin
		  select 0 StudentSessionUID,  0 as StudentID,   @ClassID as ClassID	  ,@SectionID as SectionID	  ,0 QuotaID	
		  ,1 as Status
	  end
	  else
	  begin
			select SS.StudentSessionUID,  SS.StudentID,   SS.ClassID	  ,SS.SectionID	  ,SS.QuotaID	 
		 ,SS.FromDate	  ,SS.ToDate,SS.HouseID
		  ,SS.Status,	  SS.RollNo,SS.FeePaymentMode,SS.IsAdmissionFeeApplicable
		  from Student_Session SS where SS.StudentSessionUID=@StudentSessionUID and SS.SBranchID=@SBranchID
		end


	  select @GroupID=GroupID from Class_Sections where ID=@SectionID

	  select ClassID as ID, ClassName as Name from ClassMaster where SBranchID=@SBranchID order by ClassID
	  select ID,Name from Class_Sections where ClassID=@ClassID and Capacity>(select count(*) from Student_Session where SectionID=ID and SessionID=@SessionID) or ID=@SectionID

	  Select QuotaID as ID, QuotaName as Name from QuotaMaster QM where IsApproved=1 and SBranchID = @SBranchID
	  and QM.NumberOfStudents>(select count(*) from Student_Session SS where Status=1 and SS.QuotaID=QM.QuotaID)

	  declare @SessionStartDate date
	  declare @SessionEndDate date
	  if(isnull(@SessionID,0)=0)
	  begin
		  select @SessionStartDate= SS.FromDate	  ,@SessionEndDate= SS.ToDate
		  from Student_Session SS where SS.StudentSessionUID=@StudentSessionUID and SS.SBranchID=@SBranchID
	  end
	  else
	  begin
	  select @SessionStartDate=SessionStartDate,@SessionEndDate=SessionEndDate from SessionMaster where SessionID=@SessionID
	  end
	  select @SessionStartDate
	  select @SessionEndDate
	   
		  Select SessionID,SessionName,SessionStartDate,SessionEndDate,SessionStatus from SessionMaster where SBranchID=@SBranchID
		  order by SessionStatus desc

	  select isnull(@SessionID,0)

	  select Name,ID from HouseMaster where SBranchID=@SBranchID

	  select SubjectID as ID,SubjectName as Name from SubjectMaster where GroupID=@GroupID and isnull(MainSubID,0)=0 and IsOptionalSubject=1

	  select SSOSID as Extra1,StudentSessionID as Name,OpSubjectID as ID from [dbo].[Student_Session_OptionalSubjects] where StudentSessionID=@StudentSessionUID
END
GO
GO
ALTER proc [dbo].[sp_GetNotifications]
(
@RecieverID int,
@RecieverType int,
@SBranchID int
)
AS
BEGIN
	set @RecieverType=case when @RecieverType=3 then 1 else 0 end

	select NotificationID,NotificationType,NotificationDateTime,
	(case when RecieverID =0 then (Select NotificationText from NotificationRecievers NR where NR.NotificationID=NM.NotificationID 
	and NR.RecieverID=@RecieverID and NR.RecieverType=@RecieverType) else NotificationText
	 end) as NotificationText
	from NotificationMaster NM 
	where (RecieverID<>0 and NotificationID in (select NotificationID from NotificationRecievers where RecieverID=@RecieverID and RecieverType=@RecieverType))
	or (RecieverID=-1 and (RecieverType=@RecieverType) or RecieverType=0) or (RecieverID=@RecieverID and RecieverType=@RecieverType) and SBranchID=@SBranchID
	order by NotificationDateTime desc	
END
GO
GO

ALTER proc [dbo].[sp_UpdateStudentAttandance]
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
			set @SQLQuery='Insert into StudentAttendanceMasterT(StudentID,SBranchID,ClassID,SectionID,FYear,Month,'+cast(@Day as nvarchar(3))+') 
							Values ('+cast (@ID as nvarchar(5))+','+cast (@SBranchID as nvarchar(5))+','+cast (@ClassID as nvarchar(5))+','+cast (@SectionID as nvarchar(5))+','+@Year+','+@Month+','+cast(@Status as nvarchar(5))+')'					
			exec(@SQLQuery)
			end
				FETCH NEXT FROM cur_List INTO @ID ,@Status,@IsExist
		END
	END
	CLOSE cur_List
	DEALLOCATE cur_List

	select 1
END

GO

CREATE proc [dbo].[sp_GetClassStudentsDayAttandanceStatus]
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

	Declare @HolidayCnt int
	select @HolidayCnt=count(*) from HolidayMaster where @AttDate between StartDate and EndDate and 
	((select count(*) from dbo.SplitStringToTable(Classes,',') where ITem=@ClassID or Item=0)>0) and IsStudents=1
	
	declare @StudentTable as Table(StudentID int,Name nvarchar(500),Photo nvarchar(500),RollNo nvarchar(50),Gender int,StudentSID nvarchar(50),
	Status numeric(5,2),IsExist int)
	
	declare @WD int=isnull(nullif(DATEPART(dw,@AttDate)-1,0),7)
	
	declare @SQLQuery nvarchar(max)
	set @SQLQuery='Select SM.[StudentID],SM.[Name],SM.Photo,SS.RollNo,SM.Gender,
	''STUD''+RIGHT(REPLICATE(''0'',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) as [StudentSID],
	isnull(EAM.D'+@Day+',3) as Status ,(case when EAM.D'+@Day+' is null then 0 else 1 end) as IsExist
	from StudentMaster SM left outer join [dbo].[StudentAttendanceMasterT] EAM on EAM.StudentID=SM.StudentID
	and EAM.[FYear]='+@Year+' and EAM.[Month]='+@Month+ ' left outer join Student_Session SS on SS.StudentID=SM.StudentID
	where SM.SBranchID='+cast(@SBranchID as nvarchar(3))+' and SS.ClassID='+cast(@ClassID as nvarchar(3))+' and SS.SectionID='+cast(@SectionID as nvarchar(3))+' and SS.SessionID='+cast(@SessionID as nvarchar(3))+'
	and cast('''+@Year+'-'+@Month+'-'+@Day+''' as date) between SS.FromDate and SS.ToDate
	order by SM.[Name]'
	
	insert into @StudentTable 
	exec(@SQLQuery)

	declare @tStudentID int,@tStatus numeric(5,2),@lStatus numeric(5,2)
	DECLARE db_Attendancecursor CURSOR FOR  
	select StudentID,Status from @StudentTable FOR UPDATE OF [Status]
	OPEN db_Attendancecursor   
	FETCH NEXT FROM db_Attendancecursor INTO @tStudentID,@tStatus
	WHILE @@FETCH_STATUS = 0   
	BEGIN   
		if(isnull(nullif(@tStatus,3),3)=3)
		begin
			if(@WD>@NumDays and isnull(@tStatus,3)=3)
			begin
				UPDATE @StudentTable SET [Status] = -2 WHERE CURRENT OF db_Attendancecursor
			end
			else if(isnull(@HolidayCnt,0)>0)
			begin
				UPDATE @StudentTable SET [Status] = 2 WHERE CURRENT OF db_Attendancecursor
			end
			else
			begin
				Select @lStatus=max((Case when LeaveType=1 then 1 else 0.5 end)) from [dbo].[LeaveMaster] LM where LM.ApplicantType=1 and LM.IsApproved=1 and LM.ApplicantID=@tStudentID
				and @AttDate between LM.[StartDate] and LM.[EndDate]
				if(isnull(@lStatus,0)>0)
				begin
					UPDATE @StudentTable SET [Status] = -1 WHERE CURRENT OF db_Attendancecursor
				end
			end
		end

		FETCH NEXT FROM db_Attendancecursor INTO  @tStudentID,@tStatus
	END   
	CLOSE db_Attendancecursor   
	DEALLOCATE db_Attendancecursor

	select * from @StudentTable

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

	select isnull(@ClassID,0)
	select isnull(@SectionID,0)
	select case when @WD>@NumDays then -2 when @HolidayCnt>0 then 2 else 1 end
END
GO
GO
ALTER PROCEDURE [dbo].[sp_GetTeacherHolidays]
(
@TeacherID int
)
AS
begin
	declare @SBranchID int
	select @SBranchID=SBranchID from EmployeeMaster where EmployeeID=@TeacherID
	
	select HolidayID,HolidayType,(Case when HolidayType=1 then 'Local Holiday' else 'National Holiday' end) 'HolidayTypeName',
	(Case When DayDuration=1 then 'Full Day' else (Case when DayDuration=2 then 'First Half' else 'Second Half' end)end) 'DayDurationName',
	StartDate,EndDate,DATEDIFF(day,StartDate,EndDate)+1 as [Days],DayDuration, [HolidayDescription],Title,
	(case when Classes=0 then 'All Classes' else [dbo].[GetClassNames](Classes) end) as ClassesIncluded,
	[dbo].[GetEmployeeTypeNames](AssociatedIDs) as EmployeeTypes
	from HolidayMaster where [Status]=1 and SBranchID=@SBranchID and isnull(IsEmployee,1)=1
	order by EndDate desc
end
GO
ALTER PROCEDURE [dbo].[sp_GetParentHolidays]
(
@ParentID int
)
AS
begin
	declare @Classes table(ClassID int,SBranchID int)
	Insert into @Classes 
	select ClassID,SBranchID from Student_Session where [Status]=1 and StudentID in (select StudentID from StudentMaster where ParentID=@ParentID)
	
	select HolidayID,HolidayType,(Case when HolidayType=1 then 'Local Holiday' else 'National Holiday' end) 'HolidayTypeName',
	(Case When DayDuration=1 then 'Full Day' else (Case when DayDuration=2 then 'First Half' else 'Second Half' end)end) 'DayDurationName',
	StartDate,EndDate,DATEDIFF(day,StartDate,EndDate)+1 as [Days],DayDuration, [HolidayDescription],Title,
	(case when Classes=0 then 'All Classes' else [dbo].[GetClassNames](Classes) end) as ClassesIncluded,
	[dbo].[GetEmployeeTypeNames](AssociatedIDs) as EmployeeTypes
	from HolidayMaster where [Status]=1 and (SBranchID in (Select SBranchID from @Classes) and Classes=0)
	or (Select count(*) from [dbo].[SplitStringToTable](Classes,',') where Item in(Select ClassID from @Classes))>0
	order by EndDate desc
end
GO


Create procedure [dbo].[spn_GetSuspendedStudents]
(
@SBranchID int,
@SessionID int=0
)
as
begin 
	if(@SessionID=0)
	begin
		Select @SessionID=SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID
	end
		
	Select SM.StudentID,'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.StudentID AS VARCHAR(6)),6) as StudentSID, Name,DOB, EmailID, BloodGroup, GuardianName, GuardianMobileNo, Photo,SS.RollNo,SM.AccessCardNo,
		SM.GuardianMobileNo ,PM.FatherMobileNo,PM.MotherMobileNo,SM.Gender,SS.ClassID,SS.SectionID ,SM.AadharCardNo,PM.FatherName,PM.MotherName,SM.SchoolUID,SM.SSSID,SM.FamilyID,
		SM.MiniAddress, SM.MiniAddress2,
		isnull((Select ClassName from ClassMaster CM where CM.ClassID=SS.ClassID),'NA') as ClassName,
		isnull((Select Name from Class_Sections CS where CS.ID=SS.SectionID),'NA') as SectionName
		from StudentMaster SM left outer join Student_Session SS on SS.StudentID=SM.StudentID and SS.Status=0  and SS.SessionID=@SessionID
		left outer join Parentmaster PM on PM.ParentID=SM.ParentID
		where SS.SBranchID=@SBranchID  and SS.SessionID=@SessionID and SS.Status=0
		order by Name

	Select SessionName as Name,SessionID as ID,SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID
	
	select @SessionID

end
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
	declare @EduLevel int
	select @EduLevel=EducationLevelID from ClassMaster where ClassID=@ClassID
	declare @Days int
	select @Days=Days from EducationLevelMaster where ID=@EduLevel

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
	from StudentAttendanceMasterT where StudentID=@StudentID and FYear=@Year and [Month]=@Month

	if((Select count(*) from @AttandanceTable)=0)
	begin
		insert into @AttandanceTable( D1 ,D2 ,D3 ,D4 ,D5 ,D6 ,D7 ,D8 ,D9 ,D10 ,D11 ,D12 ,D13 ,D14 ,D15 ,
		D16 , D17 ,D18 , D19 ,D20 ,D21 ,D22 , D23 , D24 , D25 , D26 ,D27 ,D28 , D29 , D30 , D31)
		values(3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3)
	end

	Declare @AttSummery as Table (DayID int,Status numeric(10,2))
	insert into @AttSummery
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

		declare @tDayID int,@tStatus numeric(10,2)
		DECLARE db_Attendancecursor CURSOR FOR  
		select DayID,Status from @AttSummery FOR UPDATE OF [Status]
		OPEN db_Attendancecursor   
		FETCH NEXT FROM db_Attendancecursor INTO @tDayID,@tStatus
		WHILE @@FETCH_STATUS = 0   
		BEGIN   
			if(ISDATE(cast(@Year as nvarchar(10))+'-'+cast(@Month as nvarchar(10))+'-'+cast(@tDayID as nvarchar(10)))=1)
			begin
				declare @CDate date=cast(@Year as nvarchar(10))+'-'+cast(@Month as nvarchar(10))+'-'+cast(@tDayID as nvarchar(10))
				declare @WD int=isnull(nullif(DATEPART(dw,@CDate)-1,0),7)
				if(@WD>@Days and @tStatus=3)
				begin
					UPDATE @AttSummery SET [Status] = -2 WHERE CURRENT OF db_Attendancecursor
				end
				else if(@WD<=@Days and @tStatus=-2)
				begin
					UPDATE @AttSummery SET [Status] = 3 WHERE CURRENT OF db_Attendancecursor
				end
			end

		FETCH NEXT FROM db_Attendancecursor INTO  @tDayID,@tStatus
		END   
		CLOSE db_Attendancecursor   
		DEALLOCATE db_Attendancecursor
		select * from @AttSummery
END
GO
ALTER proc [dbo].[spn_InsertUpdateAssignment]
(
@ID int,
@TeacherID int,
@ClassID int,
@SectionID int,
@SubjectID int,
@ChapterID int,
@TopicID int,
@StartDate date,
@EndDate date,
@GracedDays int,
@TaskType int,
@Title nvarchar(100),
@Detail nvarchar(Max),
@Attachments nvarchar(500),
@SendMail int,
@OperationDate Datetime,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from AssignmentMaster where ID=@ID
	end
	else if (@ID=0)
	begin
		insert into [dbo].[AssignmentMaster] (TeacherID,ClassID,SectionID,SubjectID,ChapterID,TopicID,StartDate,EndDate,GracedDays,TaskType,Title,Detail,SendMail,CreatedOn)
		
		Values(@TeacherID,@ClassID,@SectionID,@SubjectID,@ChapterID,@TopicID,@StartDate,@EndDate,@GracedDays,@TaskType,@Title,@Detail,@SendMail,@OperationDate)
		select @ID=CAST(SCOPE_IDENTITY() as int)
	end
	else
	begin
		Update AssignmentMaster set ChapterID=@ChapterID, TopicID=@TopicID, StartDate=@StartDate,EndDate=@EndDate,GracedDays=@GracedDays
		,TaskType=@TaskType, Title=@Title, Detail=@Detail,Attachments=@Attachments,SendMail=@SendMail where ID=@ID
	end
	select @ID
END
GO
Create proc [dbo].[spn_InsertUpdateAssignmentFiles]
(
@ID int,
@Attachments nvarchar(500)
)
AS
BEGIN	
	Update AssignmentMaster set Attachments=isnull(Attachments,'')+(case when isnull(Attachments,'')='' then '' else ',' end) + @Attachments where ID=@ID	
END
GO
GO
Create Procedure [dbo].[sp_UpdateParentAssignmentSubmissionAttachment]
(
@AssResponseID int,
@Attachments nvarchar(500)
)
AS 
BEGIN
	Update AssignmentSubmissions set Attachments=isnull(Attachments,'')+(case when isnull(Attachments,'')='' then '' else ',' end) + @Attachments where AssResponseID=@AssResponseID
END
GO
ALTER Procedure [dbo].[sp_InsertParentAssignmentSubmission]
  (
  @AssResponseID int,
  @AssignmentID int,
  @StudentID int,
  @SubmissionDate datetime,
  @Description nvarchar(max),
  @Status int,
  @Reason nvarchar(500),
  @Attachments nvarchar(max)
  )
  AS 
  BEGIN
	if(@AssResponseID=0)
	begin
		Insert into AssignmentSubmissions(AssignmentID,StudentID,SubmissionDate,Description,Status,Reason)
		values(@AssignmentID,@StudentID,@SubmissionDate,@Description,@Status,@Reason)
		select @AssResponseID=Cast(Scope_Identity() as int)
	end
	else
	begin
		update AssignmentSubmissions set SubmissionDate=@SubmissionDate,Description=@Description,
		Status=@Status,Reason=@Reason,Attachments=@Attachments where AssResponseID=@AssResponseID
	end
	select @AssResponseID
  END
GO

Create procedure dbo.sp_DeletePayment
(
@PaymentID int
)
AS
BEGIN
	Delete from PaymentMaster where PaymentID=@PaymentID
	Delete from PaymentDetails where PaymentID=@PaymentID
END
GO
--------------------------------------------------------------------------------------------------------------------------------------------------------
--Salary Related
----------------------------------------------------------------------------------------------------------------------------------------------------------

DROP TABLE [dbo].[SalaryProcessingMaster]
GO

CREATE TABLE [dbo].[SalaryProcessingMaster](
	[SPMID] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeID] [int] NULL,
	[EmployeeType] [int] NULL,
	[ProcessingDate] [datetime] NULL,
	[SMonth] [int] NULL,
	[SYear] [int] NULL,
	[ReferanceNumber] [nvarchar](500) NULL,
	[Remark] [nvarchar](max) NULL,
	[UserID] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[SalaryAmount] [numeric](10, 2) NULL,
	[PaidAmount] [numeric](10, 2) NULL,
	[Status] [int] NULL,
	[SalaryReferanceNumber] [nvarchar](50) NULL,
	[SalaryReferanceSequence] [int] NULL,
	[SBranchID] [int] NULL,
	[TotalDays] [numeric](5, 2) NULL,
	[PresentDays] [numeric](5, 2) NULL,
	[LWP] [numeric](5, 2) NULL,
	[PaidLeaves] [numeric](5, 2) NULL,
	[Holidays] [numeric](5, 2) NULL,
	[WeekOffs] [numeric](5, 2) NULL,
	[GrossEarning] [numeric](10, 2) NULL,
	[TotalDeductions] [numeric](10, 2) NULL,
	[NetPayble] [numeric](10, 2) NULL,
	[PaymentMode] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
DROP TABLE [dbo].[SalaryProcessingDetails]
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
	[Title] [nvarchar](50) NULL,
	[Type] [int] NULL
) ON [PRIMARY]
GO
Drop PROCEDURE [dbo].[sp_UpdateEmployeeSalaryPayment]
GO
Drop Type [dbo].[ut_EmpSalaryProcessingDetails]
GO
CREATE TYPE [dbo].[ut_EmpSalaryProcessingDetails] AS TABLE(
	[SPDID] [int] NULL,
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
	[Title] [nvarchar](50) NULL,
	[Type] [int] NULL,
	[OpType] [int] NULL
)
GO
Create proc [dbo].[sp_UpdateEmployeeSalaryPayment]
(
@SPMID int,
@EmployeeID int,
@EmployeeType int,
@ProcessingDate datetime,
@SMonth int,
@SYear int,
@ReferanceNumber nvarchar(100),
@Remark nvarchar(max),
@UserID int,
@CreatedDate datetime,
@PaymentMode int,
@SalaryAmount numeric(10,2),
@PaidAmount numeric(10,2),
@Status int,
@SBranchID int,
@TotalDays numeric(5,2),
@PresentDays numeric(5,2),
@LWP numeric(5,2),
@PaidLeaves numeric(5,2),
@Holidays numeric(5,2),
@WeekOffs numeric(5,2),
@GrossEarning numeric(10,2),
@TotalDeductions numeric(10,2),
@NetPayble numeric(10,2),
@SalaryDetails ut_EmpSalaryProcessingDetails READONLY
)
AS
BEGIN
	if(@SPMID=0)
	begin	
		declare @SalaryReferanceSequence int,@SalaryReferanceNumber nvarchar(50)
		select @SalaryReferanceSequence= max(SalaryReferanceSequence) from SalaryProcessingMaster where SBranchID=@SBranchID
		set @SalaryReferanceSequence=isnull(@SalaryReferanceSequence,0)+1
		Insert into SalaryProcessingMaster(EmployeeID,EmployeeType,ProcessingDate,SMonth,SYear,ReferanceNumber,Remark,UserID,CreatedDate,SalaryAmount,PaidAmount,
		Status,SalaryReferanceSequence,SalaryReferanceNumber,SBranchID,TotalDays,PresentDays,LWP,PaidLeaves,Holidays,WeekOffs,GrossEarning,TotalDeductions,NetPayble)
		Values(@EmployeeID,@EmployeeType,@ProcessingDate,@SMonth,@SYear,@ReferanceNumber,@Remark,@UserID,@CreatedDate,@SalaryAmount,@PaidAmount,
		@Status,@SalaryReferanceSequence,@SalaryReferanceNumber,@SBranchID,@TotalDays,@PresentDays,@LWP,@PaidLeaves,@Holidays,@WeekOffs,@GrossEarning,@TotalDeductions,@NetPayble)
		
		Select @SPMID=cast(Scope_Identity() as int)
		
		if(@SPMID<>0)
		begin
			INSERT INTO SalaryProcessingDetails(SPMID,EmployeeID,SalaryCategoryID,RefID,Amount,Paid,IsDuePaid,SMonth,SYear,Status,Title,Type)
			SELECT @SPMID,EmployeeID,SalaryCategoryID,RefID,Amount,Paid,IsDuePaid,SMonth,SYear,Status,Title,Type FROM @SalaryDetails
			 
			Declare @AdPayID int,@AdPayAmount numeric(10,2)
			DECLARE cur_AdvancePayments CURSOR
			STATIC FOR
			SELECT  RefID,Amount FROM @SalaryDetails PD where PD.Type=2
			OPEN cur_AdvancePayments
			IF @@CURSOR_ROWS > 0
			BEGIN
				FETCH NEXT FROM cur_AdvancePayments INTO @AdPayID,@AdPayAmount
				WHILE @@Fetch_status = 0
				BEGIN
					Insert Into [dbo].[AdvancePaymentDeductions](AdPayID,[Date],Amount,Mode,Remark)
						Values (@AdPayID,@ProcessingDate,@AdPayAmount,	0,'Deducted From Salary')
				FETCH NEXT FROM cur_AdvancePayments INTO @AdPayID,@AdPayAmount	
				END
			END	
			CLOSE cur_AdvancePayments
			DEALLOCATE cur_AdvancePayments
		end

	end	
	select @SPMID
END
GO
GO
CREATE procedure [dbo].[sp_GetEmployeeSalaryDetailsNew] 
(
@EmployeeID int=4,
@SalaryMonth int=6, 
@SalaryYear int=2020,
@SBranchID int=1,
@EmployeeType int =3
)
AS
BEGIN
	
	Select BranchSchoolName,Address from SBranchMaster where SBranchID=@SBranchID

	select EmployeeID,EmployeeSID,EmployeeType,[EmployeeName],
	(Select EmployeeTypeName from EmployeeTypeMaster ETM where ETM.EmployeeTypeID=EM.EmployeeType) as EmployeeTypeName
	,DOJ from [dbo].[v_EmployeeDriversBasicDetails] EM where EmployeeID=@EmployeeID and EmployeeType=@EmployeeType

	Declare @MonthType int,@BaseDate date,@SessionStartDate date,@IsAdmissionFee int=0,@DOJ date,@SalaryMonthDate date

	Declare @HStartDate date,@HEndDate date,@HDuration int,@Month int,@DaysWorking int=6
	declare @SalarySummery Table(TypeID int,TypeName nvarchar(50),IsDeduction int,AttendanceType int,MinDays int,Amount numeric(10,2))
	declare @HolidayTable Table(DayID int,HStatus numeric(5,2),HDuration int)
	declare @LeaveSummery Table(DayID int,LStatus numeric(10,2),IsSalaryDeduct int,LDuration int)

	set @SessionStartDate=Cast(@SalaryYear as nvarchar(10))+'-1-1'
	set @SalaryMonthDate=Cast(@SalaryYear as nvarchar(10))+'-'+Cast(@SalaryMonth as nvarchar(2))+'-1'
	select @DOJ=DOJ from EmployeeMaster where EmployeeID=@EmployeeID
	set @BaseDate=@SessionStartDate
	if(@DOJ>@SessionStartDate)
	begin
		set @BaseDate=@DOJ
	end

	select @MonthType=(Case when datepart(month,@BaseDate)=@SalaryMonth and (datepart(month,dateadd(month,3,@SessionStartDate))=@SalaryMonth  or
	datepart(month,dateadd(month,9,@SessionStartDate))=@SalaryMonth) then 5 else
	(Case when datepart(month,@BaseDate)=@SalaryMonth and (datepart(month,dateadd(month,6,@SessionStartDate))=@SalaryMonth) then 6 else
	(case when datepart(month,@BaseDate)=@SalaryMonth then (case when @IsAdmissionFee=0 then 1 else 0 end)  
	else (Case when datepart(month,dateadd(month,3,@SessionStartDate))=@SalaryMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@SalaryMonth then 3
	else (case when  datepart(month,dateadd(month,6,@SessionStartDate))=@SalaryMonth then 4 else 2 end) end) end)end)end)

	Declare @MTFT table (MonthTypeID int,FeeTypeID int)
	insert into @MTFT
	select MonthTypeID,FeeTypeID from [MonthTypeFeeType]


	insert into @SalarySummery
	select distinct TypeID,TypeName,IsDeduction,AttendanceType,MinDays,isnull(ESD.Amount,ETSD.Amount) as Amount from SalaryTypeMaster STM
	left outer join [dbo].[EmployeeSalaryDetails] ESD on ESD.SalaryTypeID=STM.TypeID and ESD.EmployeeID=@EmployeeID
	left outer join [dbo].[EmployeeTypeSalaryDetails] ETSD on ETSD.SalaryTypeID=STM.TypeID and ETSD.EmployeeTypeID=@EmployeeType
	where STM.SBranchID=@SBranchID
	and STM.TypeApplicable in (select FeeTypeID from @MTFT where MonthTypeID=@MonthType)
	and (STM.TypeApplicable<>9 or (select count(*) from [dbo].[SplitStringToTable](STM.Months,',') where Item=@SalaryMonth)>0)

	select * from @SalarySummery

	Insert into @LeaveSummery
	select datepart(day,LD.LeaveDate),Applied,isnull(IsSalaryDeduct,1),LeaveType
	from LeaveDetails LD left outer join LeaveTypeMaster LTM on LTM.LeaveTypeID=LD.LeaveTypeID
	Left outer join LeaveMaster LM  on LM.LeaveID=LD.LeaveID
	where LM.ApplicantType=0 and LM.ApplicantID=@EmployeeID and LM.IsApproved=1
	and LD.LeaveDate between @SalaryMonthDate and EOMONTH(@SalaryMonthDate) --and isnull(IsSalaryDeduct,1)=0

	select * from @LeaveSummery

	DECLARE @AttandanceTable TABLE (D1 numeric(5,2),D2 numeric(5,2),D3 numeric(5,2),D4 numeric(5,2),D5 numeric(5,2),D6 numeric(5,2),D7 numeric(5,2),D8 numeric(5,2),D9 numeric(5,2),D10 numeric(5,2),D11 numeric(5,2),D12 numeric(5,2),D13 numeric(5,2),D14 numeric(5,2),D15 numeric(5,2),
	D16 numeric(5,2), D17 numeric(5,2),D18 numeric(5,2), D19 numeric(5,2),D20 numeric(5,2),D21 numeric(5,2),D22 numeric(5,2), D23 numeric(5,2), D24 numeric(5,2), D25 numeric(5,2), D26 numeric(5,2),D27 numeric(5,2),D28 numeric(5,2), D29 numeric(5,2), D30 numeric(5,2), D31 numeric(5,2))

	DECLARE @AttandanceDateTable TABLE (ADay int,Status int)

	Insert into @AttandanceTable select  D1 ,D2 ,D3 ,D4 ,D5 ,D6 ,D7 ,D8 ,D9 ,D10 ,D11 ,D12 ,D13 ,D14 ,D15 ,
	D16 , D17 ,D18 , D19 ,D20 ,D21 ,D22 , D23 , D24 , D25 , D26 ,D27 ,D28 , D29 , D30 , D31
	from EmployeeAttendanceMaster where EmployeeID=@EmployeeID and FYear=@SalaryYear and [Month]=@SalaryMonth

	insert into @AttandanceDateTable
	Select t.DayID,isnull([Status],0) as Status from
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

	DECLARE db_Holidaycursor CURSOR FOR  
	select StartDate,EndDate,DayDuration
	from HolidayMaster where [Status]=1 and SBranchID=@SBranchID and IsEmployee=1 and
	((Select count(*) from dbo.SplitStringToTable(isnull(AssociatedIDs,'0'),',') where Item=@EmployeeType or Item=0)>0)
	and ((datepart(month,StartDate)=@SalaryMonth and datepart(year,StartDate)=@SalaryYear)
	or (datepart(month,EndDate)=@SalaryMonth and datepart(year,EndDate)=@SalaryYear))
	OPEN db_Holidaycursor  
	FETCH NEXT FROM db_Holidaycursor INTO @HStartDate,@HEndDate,@HDuration
	WHILE @@FETCH_STATUS = 0  
		BEGIN  
			while(@HStartDate<=@HEndDate)
			begin
				if(datepart(month,@HStartDate)=@SalaryMonth)
				begin
					declare @tStatus int,@tHDuration int
					select @tStatus=HStatus,@tHDuration=HDuration from @HolidayTable where DayID=datepart(day,@HStartDate)
					if(@tStatus is null)
					begin
						if(@HDuration=1)
						begin
							Insert into @HolidayTable(DayID,HStatus,HDuration) values (datepart(day,@HStartDate),1,@HDuration)
						end
						else
						begin
							Insert into @HolidayTable(DayID,HStatus,HDuration) values (datepart(day,@HStartDate),0.5,@HDuration)
						end
					end
					else
					begin
						if((@tHDuration=2 and @HDuration=3) or (@tHDuration=3 and @HDuration=2) or @HDuration=1)
						begin
							Update @HolidayTable set HStatus=1,HDuration=1 where DayID=datepart(day,@HStartDate)
						end
					end
				end
				Set @HStartDate= DATEADD(day,1,@HStartDate)
			end
			FETCH NEXT FROM db_Holidaycursor INTO  @HStartDate,@HEndDate,@HDuration
		END  
		CLOSE db_Holidaycursor  
		DEALLOCATE db_Holidaycursor
		select * from @HolidayTable

	Declare @FinalAttandanceStatusTable TABLE(ADate date,AStatus numeric(10,2),LStatus numeric(10,2),HStatus numeric(10,2),LType int,HType int)
	--select * from @AttandanceDateTable
	declare @FLStatus int,@FHStatus int,@FLType int,@FHType int,@PLStatus int
	declare @EndDate date=EOMOnth(@SalaryMonthDate)
	declare @Status int

	while(@SalaryMonthDate<=@EndDate)
	begin
		set @FLType=0
		set @FHStatus=0
		set @FLType=0
		set @FHType=0
		select @Status=Status from @AttandanceDateTable where ADay=datepart(day,@SalaryMonthDate)
		if(@Status not in (1,0.5))
		begin
			if((case Datepart(dw,@SalaryMonthDate) when 1 then 8 else Datepart(dw,@SalaryMonthDate) end)>@DaysWorking+1)
			begin
				Insert into @FinalAttandanceStatusTable(ADate,AStatus,LStatus,HStatus,LType,HType)
				values(@SalaryMonthDate,2,0,0,0,0)
			end
			else
			begin
				select @FHStatus=HStatus,@FHType=HDuration from @HolidayTable where DayID=datepart(day,@SalaryMonthDate)
				select @FLStatus=LStatus,@FLType=IsSalaryDeduct from @LeaveSummery where DayID=datepart(day,@SalaryMonthDate)
				Insert into @FinalAttandanceStatusTable(ADate,AStatus,LStatus,HStatus,LType,HType)
				values(@SalaryMonthDate,0,@FLStatus,@FHStatus,@FLType,@FHType)
			end
		end
		else
		begin
			select @FHStatus=HStatus,@FHType=HDuration from @HolidayTable where DayID=datepart(day,@SalaryMonthDate)
			select @FLStatus=LStatus,@FLType=IsSalaryDeduct from @LeaveSummery where DayID=datepart(day,@SalaryMonthDate)
			Insert into @FinalAttandanceStatusTable(ADate,AStatus,LStatus,HStatus,LType,HType)
			values(@SalaryMonthDate,@Status,@FLStatus,@FHStatus,@FLType,@FHType)
		end
		set @SalaryMonthDate=dateadd(day,1,@SalaryMonthDate)
	end

	declare @WorkingDays numeric(10,2),@MonthDays numeric(10,2),@PresentDays numeric(10,2),@WeekOff numeric(10,2),
	@PubHoliday numeric(10,2),@Leaves numeric(10,2),@LWP numeric(10,2)

	select @WorkingDays=sum(Case when AStatus!=2 then 1 else 0 end),
	@MonthDays=Count(*),
	@WeekOff=sum(Case when AStatus=2 then 1 else 0 end),
	@PubHoliday=sum(HStatus),@LWP=sum(Case when LType=1 then LStatus else 0 end),
	@Leaves=sum(Case when LType=0 then LStatus else 0 end),
	@PresentDays=sum(Case when AStatus in (1,0.5) then AStatus else 0 end)
	from @FinalAttandanceStatusTable

	select @MonthDays as MonthDays,@WorkingDays-@PubHoliday as WorkingDays,@WeekOff as WeekOffs,@PubHoliday as PubHolidays,
	@Leaves as Leaves,@LWP as LWP,@PresentDays as Present

	 Select RefID as TypeID, Title+' #'+cast(DeductionsDone+1 as nvarchar(3))+'/'+cast(Months as nvarchar(3)) as TypeName, 5 as FeeTypeApplicable,
	(case when Amount-DeductedAmount<EMI then Amount-DeductedAmount else EMI end)*-1 as Amount from (
		select APM.Title, APM.Amount,EMI,APM.Months,isnull(Sum(APD.Amount),0) as DeductedAmount,count(distinct(datepart(month,APD.Date))) as DeductionsDone
	,APM.AdPaymentID as RefID from 
		[dbo].[AdvancePaymentMaster]  APM left outer join [dbo].[AdvancePaymentDeductions] APD on APM.AdPaymentID=APD.AdPayID
		and datepart(month, APD.Date)+1+12*datepart(Year, APD.Date)<=@SalaryMonth+12*@SalaryYear 
		Where EmployeeID=@EmployeeID and EmployeeType=@EmployeeType and APM.PaymentDate<=@SalaryMonthDate
		group by APM.Title,APM.Amount,EMI,APM.Months,APM.AdPaymentID
		having isnull(Sum(APD.Amount),0)<APM.Amount)t 
END

GO
Create proc [dbo].[sp_GetEmployeeSalaryList]
(
@Month int,
@Year int,
@EmployeeTypeID int,
@SBranchID int
)
AS
BEGIN
	
	exec sp_GetEmployeeTypes  @SBranchID

	Select EM.[EmployeeID],EM.[EmployeeName],EM.Photo,EM.Gender,
	Em.[EmployeeSID],EM.[EmployeeType],EM.Amount,
	SPM.SPMID as PaymentID,SPM.ProcessingDate as PaymentDate, SPM.ReferanceNumber,SPM.NetPayble as PaymentAmount,SPM.TotalDeductions as AdvanceDeduction,
	SPM.PresentDays 
	from [v_EmployeeSalaryAmount] EM left outer join SalaryProcessingMaster SPM on SPM.EmployeeID=EM.EmployeeID and SPM.SMonth=@Month and SPM.SYear=@Year and SPM.EmployeeType=@EmployeeTypeID
	
	where EM.SBranchID=@SBranchID and EM.[EmployeeType]=@EmployeeTypeID
	
END
GO
ALTER procedure [dbo].[spnp_GetStudentsForParant]
(
@ParentID nvarchar(10),
@Year nvarchar(5)=2017,
@Month nvarchar(5)=2,
@DayID nvarchar(4)='D1'
)
as
begin
update AppUsers set LastActive=getdate() where UserID=@ParentID and UserType=4
declare @SQL nvarchar(max)
set @SQL='Select SM.StudentID, Name, isnull(SS.RollNo,''--'') as RollNo,Gender,isnull(SM.BloodGroup,''Not Specified'') as BloodGroup,
isnull(AadharCardNo,''---- ---- ---- ----'') as AadharCardNo,isnull(FORMAT(SM.DOJ, ''dd MMM, yyyy''),''-- ---, ----'') as DateOfAdmission,
isnull(FORMAT(SM.DOB, ''dd MMM, yyyy''),''-- ---, ----'') as DateOfBirth,
isnull(''/images/StudentImage/''+cast(SM.StudentID as nvarchar(10))+''_''+Photo,(case when Gender=0 then ''~/Images/MaleNoImage.png'' else ''~/Images/FemaleNoImage.png'' end)) as Photo,
isnull(''~/Images/ParentImage/Father/''+cast(PM.ParentID as nvarchar(10))+''_1_''+FatherImage,''~/Images/MaleNoImage.png'') as FatherImage,
isnull(''~/Images/ParentImage/Mother/''+cast(PM.ParentID as nvarchar(10))+''_2_''+MotherImage,''~/Images/FemaleNoImage.png'') as MotherImage,
PM.FatherName,PM.MotherName,isnull(FORMAT(PM.FatherDOB, ''dd MMM, yyyy''),''-- ---, ----'') as FathersDOB,isnull(FatherEmailID,''----@---.--'') as FatherEmailID
,isnull(FORMAT(PM.MotherDOB, ''dd MMM, yyyy''),''-- ---, ----'') as MothersDOB,isnull(MotherEmailID,''----@---.--'') as MotherEmailID,
''STUD''+RIGHT(REPLICATE(''0'',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) as [StudentSID],SM.SchoolUID,
(Select ClassName from ClassMaster CM where CM.ClassID=SS.ClassID) as ClassName,
(Select Name from Class_Sections CS where CS.ID=SS.SectionID) as SectionName,isnull(PM.FatherMobileNo,''XXXXX'') as FatherMobileNo,isnull(PM.MotherMobileNo,''XXXXX'') as MotherMobileNo,
isnull((Select '+@DayID+' From StudentAttendanceMasterT where StudentID=SS.StudentID and FYear='+@Year +' and Month='+@Month+'),3) as AttandanceStatus
from StudentMaster SM left outer join Student_Session SS on SS.StudentID=SM.StudentID
left outer join ParentMaster PM on PM.ParentID=SM.ParentID
where ( SS.Status=1) and SM.ParentID='+@ParentID+' order by Name'
exec (@SQL)
end
GO
------------------------------------------------------------------------------------------------------------------------------------------
--COnductor App Related
CREATE Procedure [dbo].[sp_GetConductorRoutesVehicle]  
(  
@ConductorID int  
)  
as   
BEGIN  
 SELECT TVR.VehicleRouteID,TVR.VehicleID,isnull(VD.VehicleNumber,'')+' ('+isnull(VD.RegistrationNumber,'')+')' as Vehicle,TRM.RouteName  
  FROM  [dbo].[Transport_Vehicle_Route] TVR left outer join [dbo].[VehicleDetails] VD on VD.VehicleID=TVR.VehicleID  
 left outer join [dbo].[TransportRouteMaster] TRM on TRM.RouteID=TVR.RouteID  
 where VD.ConductorID=@ConductorID  
END  
  
CREATE proc [dbo].[spn_GetVehicleRoutePassengers] --1,'2019-07-26'  
(  
 @VehicleRouteID int,  
 @CDate date  
)  
AS  
BEGIN  
 select StudentID,'/Images/StudentImage/'+cast(StudentID as nvarchar(10))+'_'+Photo as Photo,Name,TRD.StopID,VehicleRouteID,'Student' as PType  
 ,(select AreaName from AreaMaster AM where AM.AreaID=TRD.AreaID)  as StopName,isnull(TA.TAID,0) as Status,0 as UType  
  from StudentMaster SM left outer join TransportRouteDetails TRD on TRD.StopID=SM.StopID  
  left outer join [dbo].[TransportAttendance] TA on TA.UID=StudentID and TA.UType=0 and cast(TA.UpDatetime as date)=@CDate  
 where SM.VehicleRouteID=@VehicleRouteID and SM.StopID is not null and TRD.StopID is not null  
 order by TRD.SequenceNo  
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
			 values(@EmployeeSID,@Password,@EmailID,@EmployeeID,@EmployeeType,@SBranchID)
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
			else if(@OldEmployeeType  in (3,10,5,8,12) and @EmployeeType in (3,10,5,8,12))
			begin
				Update LoginDetails set UserType=@EmployeeType where UserType=@OldEmployeeType and UserID=@EmployeeID
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

CREATE proc [dbo].[spn_UpdatePassengerAttendance]  
(  
 @CDate datetime,  
 @TRID int,  
 @StopID int,  
 @UType int,  
 @UID int  
)  
AS  
BEGIN  
 Update [TransportAttendance] set DownDateTime=@CDate where TRID=@TRID and UType=@UType and UID=@UID and UpDatetime=@CDate  
 if(@@ROWCOUNT=0)  
 begin  
  Insert into [TransportAttendance](TRID,StopID,UType,UID,UpDatetime) values(@TRID,@StopID,@UType,@UID,@CDate)  
 end  
END  
  
GO
Alter proc [dbo].[spn_GetVehicleRouteStoppages]  
(  
 @VehicleRouteID int  
)  
AS  
BEGIN  
 Select StopID,VehicleRouteID,TRD.AreaID,AM.AreaName,AM.Latitude,AM.Longitude,
 (case when SequenceNo=4 then 1 else case when SequenceNo<4 then  0 else 2 end end) as IsDone,  
 cast(DATEADD(MINUTE,  TVR.DelayFromRouteTime, [time]  ) as nvarchar(8))as [Time],HaltDuration,HaltDurationR,SequenceNo,SequenceNoR  
 ,cast(DATEADD(MINUTE,  TVR.DelayFromRouteTime, [timeR]  ) as nvarchar(8)) as [TimeR],  
 HaltDurationR,SequenceNoR from TransportRouteDetails TRD left outer join Transport_Vehicle_Route TVR on TVR.RouteID=TRD.RouteID  
 left outer join AreaMaster AM on AM.AreaID=TRD.AreaID  
 where tvr.VehicleRouteID=@VehicleRouteID   
 order by SequenceNo  
END  
GO

CREATE TABLE [dbo].[NationalityMaster](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[SBranchID] [int] NULL,
	[SchoolID] [int] NULL
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[ReligionMaster](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[SBranchID] [int] NULL,
	[SchoolID] [int] NULL
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[SocialCategory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[SBranchID] [int] NULL,
	[SchoolID] [int] NULL
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[SocialSubCategory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[SBranchID] [int] NULL,
	[SchoolID] [int] NULL,
	[Extra1] [nvarchar](10) NULL
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[MotherTongue](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[SBranchID] [int] NULL,
	[SchoolID] [int] NULL
) ON [PRIMARY]
GO
Alter Table SBranchMaster
Add SchoolID int
GO

Create procedure dbo.sp_InsertUpdateNationality
(
@ID int,
@Name nvarchar(50),
@SBranchID int,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from NationalityMaster where ID=@ID
	end
	else
	Begin
		if(@ID=0)
		begin
			declare @SchoolID int
			Select @SchoolID=SchoolID from SBranchMaster where SBranchID=@SBranchID
			Insert into NationalityMaster(Name,SBranchID,SchoolID) values(@Name,@SBranchID,@SchoolID)
			select @ID=Cast(Scope_Identity() as int)
		end
		else
		begin
			Update NationalityMaster set Name=@Name where ID=@ID
		end
	End
	select @ID
END
GO

Create procedure dbo.sp_InsertUpdateReligion
(
@ID int,
@Name nvarchar(50),
@SBranchID int,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from ReligionMaster where ID=@ID
	end
	else
	Begin
		if(@ID=0)
		begin
			declare @SchoolID int
			Select @SchoolID=SchoolID from SBranchMaster where SBranchID=@SBranchID
			Insert into ReligionMaster(Name,SBranchID,SchoolID) values(@Name,@SBranchID,@SchoolID)
			select @ID=Cast(Scope_Identity() as int)
		end
		else
		begin
			Update ReligionMaster set Name=@Name where ID=@ID
		end
	End
	select @ID
END
GO
Create procedure dbo.sp_InsertUpdateMotherTongue
(
@ID int,
@Name nvarchar(50),
@SBranchID int,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from MotherTongue where ID=@ID
	end
	else
	Begin
		if(@ID=0)
		begin
			declare @SchoolID int
			Select @SchoolID=SchoolID from SBranchMaster where SBranchID=@SBranchID
			Insert into MotherTongue(Name,SBranchID,SchoolID) values(@Name,@SBranchID,@SchoolID)
			select @ID=Cast(Scope_Identity() as int)
		end
		else
		begin
			Update MotherTongue set Name=@Name where ID=@ID
		end
	End
	select @ID
END
GO
Create procedure dbo.sp_InsertUpdateSocialCategory
(
@ID int,
@Name nvarchar(50),
@SBranchID int,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from SocialCategory where ID=@ID
	end
	else
	Begin
		if(@ID=0)
		begin
			declare @SchoolID int
			Select @SchoolID=SchoolID from SBranchMaster where SBranchID=@SBranchID
			Insert into SocialCategory(Name,SBranchID,SchoolID) values(@Name,@SBranchID,@SchoolID)
			select @ID=Cast(Scope_Identity() as int)
		end
		else
		begin
			Update SocialCategory set Name=@Name where ID=@ID
		end
	End
	select @ID
END
GO
Create procedure dbo.sp_InsertUpdateSocialSubCategory
(
@ID int,
@Name nvarchar(50),
@SBranchID int,
@Extra1 int,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from SocialSubCategory where ID=@ID
	end
	else
	Begin
		if(@ID=0)
		begin
			declare @SchoolID int
			Select @SchoolID=SchoolID from SBranchMaster where SBranchID=@SBranchID
			Insert into SocialSubCategory(Name,Extra1,SBranchID,SchoolID) values(@Name,@Extra1,@SBranchID,@SchoolID)
			select @ID=Cast(Scope_Identity() as int)
		end
		else
		begin
			Update SocialSubCategory set Name=@Name where ID=@ID
		end
	End
	select @ID
END
GO
Create Procedure dbo.sp_GetNationalities
(
@SBranchID int
)
AS
BEGIN
	Select * from NationalityMaster  where SBranchID=@SBranchID
END
GO
Create Procedure dbo.sp_GetReligions
(
@SBranchID int
)
AS
BEGIN
	Select * from ReligionMaster  where SBranchID=@SBranchID
END
GO
Create Procedure dbo.sp_GetSocialCategories
(
@SBranchID int
)
AS
BEGIN
	Select * from SocialCategory  where SBranchID=@SBranchID
END
GO
Create Procedure dbo.sp_GetSocialSubCategories
(
@SBranchID int,
@MainCategoryID int=0
)
AS
BEGIN
	if(@MainCategoryID=0)
	begin
		Select * from SocialSubCategory  where SBranchID=@SBranchID
	end
	else
	begin
		Select * from SocialSubCategory where Extra1=@MainCategoryID
	end
END
GO
Create Procedure dbo.sp_GetMotherTongues
(
@SBranchID int
)
AS
BEGIN
	Select * from MotherTongue  where SBranchID=@SBranchID
END
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

	select * from NationalityMaster where SBranchID=@SBranchID
	select * from ReligionMaster where SBranchID=@SBranchID
	select * from MotherTongue where SBranchID=@SBranchID
	select * from SocialCategory where SBranchID=@SBranchID
  end
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
			 values(@EmployeeSID,@Password,@EmailID,@EmployeeID,@EmployeeType,@SBranchID)
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
			else if(@OldEmployeeType  in (3,10,5,8,12) and @EmployeeType in (3,10,5,8,12))
			begin
				Update LoginDetails set UserType=@EmployeeType where UserType=@OldEmployeeType and UserID=@EmployeeID
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
------------------------------------------------------------------------------------------------------------------------------------------
--COnductor App Related
GO

CREATE TABLE [dbo].[TransportAttendance](
	[TAID] [int] IDENTITY(1,1) NOT NULL,
	[TRID] [int] NULL,
	[StopID] [int] NULL,
	[UType] [int] NULL,
	[UID] [int] NULL,
	[UpDatetime] [datetime] NULL,
	[DownDateTime] [datetime] NULL
) ON [PRIMARY]
GO


CREATE Procedure [dbo].[sp_GetConductorRoutesVehicle]  
(  
@ConductorID int  
)  
as   
BEGIN  
 SELECT TVR.VehicleRouteID,TVR.VehicleID,isnull(VD.VehicleNumber,'')+' ('+isnull(VD.RegistrationNumber,'')+')' as Vehicle,TRM.RouteName  
  FROM  [dbo].[Transport_Vehicle_Route] TVR left outer join [dbo].[VehicleDetails] VD on VD.VehicleID=TVR.VehicleID  
 left outer join [dbo].[TransportRouteMaster] TRM on TRM.RouteID=TVR.RouteID  
 where VD.ConductorID=@ConductorID  
END  
GO
CREATE proc [dbo].[spn_GetVehicleRoutePassengers] --1,'2019-07-26'  
(  
 @VehicleRouteID int,  
 @CDate date  
)  
AS  
BEGIN  
 select StudentID,'/Images/StudentImage/'+cast(StudentID as nvarchar(10))+'_'+Photo as Photo,Name,TRD.StopID,VehicleRouteID,'Student' as PType  
 ,(select AreaName from AreaMaster AM where AM.AreaID=TRD.AreaID)  as StopName,isnull(TA.TAID,0) as Status,0 as UType  
  from StudentMaster SM left outer join TransportRouteDetails TRD on TRD.StopID=SM.StopID  
  left outer join [dbo].[TransportAttendance] TA on TA.UID=StudentID and TA.UType=0 and cast(TA.UpDatetime as date)=@CDate  
 where SM.VehicleRouteID=@VehicleRouteID and SM.StopID is not null and TRD.StopID is not null  
 order by TRD.SequenceNo  
END  
GO

CREATE proc [dbo].[spn_UpdatePassengerAttendance]  
(  
 @CDate datetime,  
 @TRID int,  
 @StopID int,  
 @UType int,  
 @UID int  
)  
AS  
BEGIN  
 Update [TransportAttendance] set DownDateTime=@CDate where TRID=@TRID and UType=@UType and UID=@UID and UpDatetime=@CDate  
 if(@@ROWCOUNT=0)  
 begin  
  Insert into [TransportAttendance](TRID,StopID,UType,UID,UpDatetime) values(@TRID,@StopID,@UType,@UID,@CDate)  
 end  
END  
 
 GO
 
ALTER PROCEDURE [dbo].[sp_GetUserByLoginName] 
  (
	@LoginName nvarchar(50)
  )
AS BEGIN  
	declare @UType int
	declare @UID int,@SBranchID int
	select top 1 @UType=UserType,@UID=UserID,@SBranchID=SBranchID from LoginDetails where 
	(CONVERT(nvarchar(50),UserID)= @LoginName or EmailID=@LoginName or UserName=@LoginName or MobileNumber=@LoginName) 

	if(@UType=2)
	--Accountant Login
	begin
	 SELECT UserID,UserName, [Password],isnull(UserType,2) as RoleID,1 as IsApproved,'Account' as FullName,UD.SBranchID,
	 BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName,BM.NotificationServerKey
		FROM LoginDetails UD left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID where UserType=@UType and UserID=@UID and UD.SBranchID=@SBranchID
	end
	else if(@UType=3)
	--Employee Login
	begin
	 SELECT UserID,UserName, [Password],isnull(UserType,3) as RoleID,1 as IsApproved,EM.EmployeeName as FullName,EM.Photo as UserImage,UD.SBranchID,
	 BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName,BM.NotificationServerKey
		FROM LoginDetails UD left outer join EMployeeMaster EM on EM.EmployeeID=UD.UserID
		left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID
		  where UD.UserType=@UType and UD.UserID=@UID and UD.SBranchID=@SBranchID
	end
	else if(@UType=4)
	--Parent Login
	begin
	 SELECT UserID,UserName, [Password],isnull(UserType,4) as RoleID,1 as IsApproved,EM.FatherName as FullName,EM.FatherImage as UserImage,UD.SBranchID,
	 BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName,BM.NotificationServerKey
		FROM LoginDetails UD left outer join ParentMaster EM on EM.ParentID=UD.UserID
		left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID
		  where UD.UserType=@UType and UD.UserID=@UID and UD.SBranchID=@SBranchID
	end
	else if(@UType=5)
	--Library Login
	begin
	 SELECT UserID,UserName, [Password],isnull(UserType,5) as RoleID,1 as IsApproved,isnull(EM.EmployeeName,'Library') as FullName,EM.Photo as UserImage,UD.SBranchID,
	 BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName
		FROM LoginDetails UD left outer join EMployeeMaster EM on EM.EmployeeID=UD.UserID
		left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID
		  where UD.UserType=@UType and UD.UserID=@UID and UD.SBranchID=@SBranchID
	end
	else if(@UType=6)
	--Student Login
	begin
	 SELECT UserID,UserName, [Password],isnull(UserType,6) as RoleID,1 as IsApproved,EM.Name as FullName,EM.Photo as UserImage,UD.SBranchID,
	 BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName
		FROM LoginDetails UD left outer join StudentMaster EM on EM.StudentID=UD.UserID
		left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID
		  where UD.UserType=@UType and UD.UserID=@UID and UD.SBranchID=@SBranchID
	end
	else if(@UType=8)
	--Principal Login
	begin
	 SELECT UserID,UserName, [Password],isnull(UserType,6) as RoleID,1 as IsApproved,'Principal' as FullName,UD.SBranchID,
	BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName,BM.NotificationServerKey
		FROM LoginDetails UD left outer join EMployeeMaster EM on EM.EmployeeID=UD.UserID
		left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID
		  where UD.UserType=@UType and UD.UserID=@UID and UD.SBranchID=@SBranchID
	end
	else if(@UType=10)
	--Reception Login
	begin
	 SELECT UserID,UserName, [Password],isnull(UserType,10) as RoleID,1 as IsApproved,isnull(EM.EmployeeName,'Reception') as FullName,EM.Photo as UserImage,UD.SBranchID,
	 BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName
		FROM LoginDetails UD left outer join EMployeeMaster EM on EM.EmployeeID=UD.UserID
		left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID
		  where UD.UserType=@UType and UD.UserID=@UID and UD.SBranchID=@SBranchID
	end
	else if(@UType=12)
	--Conductor Login
	begin
	 SELECT UserID,UserName, [Password],isnull(UserType,12) as RoleID,1 as IsApproved,isnull(EM.EmployeeName,'Conductor') as FullName,
	 EM.Photo as UserImage,UD.SBranchID,
	 BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName
		FROM LoginDetails UD left outer join EMployeeMaster EM on EM.EmployeeID=UD.UserID
		left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID
		  where UD.UserType=@UType and UD.UserID=@UID and UD.SBranchID=@SBranchID
	end
	else
	begin
	select @SBranchID=Min(SBranchID) from SBranchMaster
	 SELECT UserID,UserName, [Password],isnull(UserType,2) as RoleID,1 as IsApproved,'Administrator' as FullName,
	 (isnull(UD.SBranchID,@SBranchID)) as SBranchID,BM.Logo as BranchLogo,BM.StateID,BM.BranchName,BM.BranchSchoolName,BM.NotificationServerKey
		FROM LoginDetails UD left outer join EMployeeMaster EM on EM.EmployeeID=UD.UserID
		left outer join SBranchMaster BM on BM.SBranchID=UD.SBranchID
	  where (CONVERT(nvarchar(50),UserID)= @LoginName or UD.EmailID=@LoginName or UserName=@LoginName or UD.MobileNumber=@LoginName) 
  end
END

Create proc [dbo].[spn_GetStudentRouteStoppages] 
(  
 @StudentID int  ,
 @CurDate datetime
)  
AS  
BEGIN  
	declare @VehicleRouteID int
	select @VehicleRouteID=VehicleRouteID from StudentMaster where StudentID=@StudentID

	 Select StopID,VehicleRouteID,TRD.AreaID,AM.AreaName,AM.Latitude,AM.Longitude, 
	 cast(DATEADD(MINUTE,  TVR.DelayFromRouteTime, [time]  ) as nvarchar(8))as [Time],HaltDuration,HaltDurationR,SequenceNo,SequenceNoR  
	 ,cast(DATEADD(MINUTE,  TVR.DelayFromRouteTime, [timeR]  ) as nvarchar(8)) as [TimeR],  
	 HaltDurationR,SequenceNoR from TransportRouteDetails TRD left outer join Transport_Vehicle_Route TVR on TVR.RouteID=TRD.RouteID  
	 left outer join AreaMaster AM on AM.AreaID=TRD.AreaID  
	 where tvr.VehicleRouteID=@VehicleRouteID   
	 order by SequenceNo  

	 
	Declare @ConductorID int
	Select @ConductorID=ConductorID from VehicleDetails where VehicleID=(Select VehicleID from [dbo].[Transport_Vehicle_Route] where VehicleRouteID=@VehicleRouteID)

	select EmployeeName,MobileNumber,'/Images/EmployeeImage/'+cast(EmployeeID as nvarchar(10))+'_'+Photo as Photo,30 as RefreshTime
	from EmployeeMaster where EmployeeID=@ConductorID

	select isnull(@VehicleRouteID,0)
END  
GO
Alter table SBranchMaster
Add TransportLocationMode int
GO

ALTER procedure [dbo].[spnp_GetStudentsForParant]
(
@ParentID nvarchar(10),
@Year nvarchar(5)=2017,
@Month nvarchar(5)=2,
@DayID nvarchar(4)='D1'
)
as
begin
	Declare @SBranchID int
	Select @SBranchID from ParentMaster where ParentID=@ParentID
	declare @TransportLocationMode nvarchar(10)
	select @TransportLocationMode=isnull(TransportLocationMode,'0') from SBranchMaster where SBranchID=@SBranchID
	update AppUsers set LastActive=getdate() where UserID=@ParentID and UserType=4
	declare @SQL nvarchar(max)
	set @SQL='Select SM.StudentID, Name, isnull(SS.RollNo,''--'') as RollNo,Gender,isnull(SM.BloodGroup,''Not Specified'') as BloodGroup,
	isnull(AadharCardNo,''---- ---- ---- ----'') as AadharCardNo,isnull(FORMAT(SM.DOJ, ''dd MMM, yyyy''),''-- ---, ----'') as DateOfAdmission,
	isnull(FORMAT(SM.DOB, ''dd MMM, yyyy''),''-- ---, ----'') as DateOfBirth,'+@TransportLocationMode+' as LocationMode,
	isnull(''/images/StudentImage/''+cast(SM.StudentID as nvarchar(10))+''_''+Photo,(case when Gender=0 then ''~/Images/MaleNoImage.png'' else ''~/Images/FemaleNoImage.png'' end)) as Photo,
	isnull(''~/Images/ParentImage/Father/''+cast(PM.ParentID as nvarchar(10))+''_1_''+FatherImage,''~/Images/MaleNoImage.png'') as FatherImage,
	isnull(''~/Images/ParentImage/Mother/''+cast(PM.ParentID as nvarchar(10))+''_2_''+MotherImage,''~/Images/FemaleNoImage.png'') as MotherImage,
	PM.FatherName,PM.MotherName,isnull(FORMAT(PM.FatherDOB, ''dd MMM, yyyy''),''-- ---, ----'') as FathersDOB,isnull(FatherEmailID,''----@---.--'') as FatherEmailID
	,isnull(FORMAT(PM.MotherDOB, ''dd MMM, yyyy''),''-- ---, ----'') as MothersDOB,isnull(MotherEmailID,''----@---.--'') as MotherEmailID,
	''STUD''+RIGHT(REPLICATE(''0'',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) as [StudentSID],SM.SchoolUID,
	(Select ClassName from ClassMaster CM where CM.ClassID=SS.ClassID) as ClassName,
	(Select Name from Class_Sections CS where CS.ID=SS.SectionID) as SectionName,isnull(PM.FatherMobileNo,''XXXXX'') as FatherMobileNo,isnull(PM.MotherMobileNo,''XXXXX'') as MotherMobileNo,
	isnull((Select '+@DayID+' From StudentAttendanceMasterT where StudentID=SS.StudentID and FYear='+@Year +' and Month='+@Month+'),3) as AttandanceStatus
	from StudentMaster SM left outer join Student_Session SS on SS.StudentID=SM.StudentID
	left outer join ParentMaster PM on PM.ParentID=SM.ParentID
	where ( SS.Status=1) and SM.ParentID='+@ParentID+' order by Name'
	exec (@SQL)
end
GO

ALTER Procedure [dbo].[sp_GetConductorRoutesVehicle]  
(  
@ConductorID int  ,
@CurDate datetime
)  
as   
BEGIN  
 SELECT TVR.VehicleRouteID,TVR.VehicleID,isnull(VD.VehicleNumber,'')+' ('+isnull(VD.RegistrationNumber,'')+')' as Vehicle,TRM.RouteName  
  FROM  [dbo].[Transport_Vehicle_Route] TVR left outer join [dbo].[VehicleDetails] VD on VD.VehicleID=TVR.VehicleID  
 left outer join [dbo].[TransportRouteMaster] TRM on TRM.RouteID=TVR.RouteID  
 where VD.ConductorID=@ConductorID  
 Declare @SBranchID int
 select @SBranchID=SBranchID from EmployeeMaster where EmployeeID=@ConductorID
 select '5-'+ isnull(cast(TransportLocationMode as nvarchar(2)),'1') from SBranchMaster where SBranchID=@SBranchID
END
GO

CREATE procedure [dbo].[spn_GetSessionClassSectionForBranch]
(
@SBranchID int
)
as 
begin 
	Declare @Status int=1,@ClassID int,@SectionID int,@SessionID int
		Select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
		select @Status=SessionStatus from SessionMaster where SessionID=@SessionID
		select top 1 @ClassID=ClassID from ClassMaster where SBranchID=@SBranchID order by ClassID 
	select ClassID,ClassName from ClassMaster where SBranchID=@SBranchID

	select top 1 @SectionID=ID from Class_Sections where ClassID=@ClassID order by ID

	select ID,Name from Class_Sections where ClassID=@ClassID

	select @ClassID
	select @SectionID

	Select SessionName as Name,SessionID as ID,SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID
	
	select @SessionID
end
GO

Alter procedure [dbo].[spnp_GetStudentsForParant]--1,2020,06,'D19'  
(  
@ParentID nvarchar(10),  
@Year nvarchar(5)=2017,  
@Month nvarchar(5)=2,  
@DayID nvarchar(4)='D1'  
)  
as  
begin  
 Declare @SBranchID int  
 Select @SBranchID=SBranchID from ParentMaster where ParentID=@ParentID  
 declare @TransportLocationMode nvarchar(10)  
 select @TransportLocationMode=isnull(TransportLocationMode,'1') from SBranchMaster where SBranchID=@SBranchID  
 update AppUsers set LastActive=getdate() where UserID=@ParentID and UserType=4  
 declare @SQL nvarchar(max)  
 set @SQL='Select SM.StudentID, Name, isnull(SS.RollNo,''--'') as RollNo,Gender,isnull(SM.BloodGroup,''Not Specified'') as BloodGroup,  
 isnull(AadharCardNo,''---- ---- ---- ----'') as AadharCardNo,isnull(FORMAT(SM.DOJ, ''dd MMM, yyyy''),''-- ---, ----'') as DateOfAdmission,  
 isnull(FORMAT(SM.DOB, ''dd MMM, yyyy''),''-- ---, ----'') as DateOfBirth,'+isnull(@TransportLocationMode,'0')+' as LocationMode,  
 isnull(''/images/StudentImage/''+cast(SM.StudentID as nvarchar(10))+''_''+Photo,(case when Gender=0 then ''~/Images/MaleNoImage.png'' else ''~/Images/FemaleNoImage.png'' end)) as Photo,  
 isnull(''~/Images/ParentImage/Father/''+cast(PM.ParentID as nvarchar(10))+''_1_''+FatherImage,''~/Images/MaleNoImage.png'') as FatherImage,  
 isnull(''~/Images/ParentImage/Mother/''+cast(PM.ParentID as nvarchar(10))+''_2_''+MotherImage,''~/Images/FemaleNoImage.png'') as MotherImage,  
 PM.FatherName,PM.MotherName,isnull(FORMAT(PM.FatherDOB, ''dd MMM, yyyy''),''-- ---, ----'') as FathersDOB,isnull(FatherEmailID,''----@---.--'') as FatherEmailID  
 ,isnull(FORMAT(PM.MotherDOB, ''dd MMM, yyyy''),''-- ---, ----'') as MothersDOB,isnull(MotherEmailID,''----@---.--'') as MotherEmailID,  
 ''STUD''+RIGHT(REPLICATE(''0'',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) as [StudentSID],SM.SchoolUID,  
 (Select ClassName from ClassMaster CM where CM.ClassID=SS.ClassID) as ClassName,  
 (select VehicleID from Transport_Vehicle_Route where VehicleRouteID =SM.VehicleRouteID) as VehicleID,
 (Select Name from Class_Sections CS where CS.ID=SS.SectionID) as SectionName,isnull(PM.FatherMobileNo,''XXXXX'') as FatherMobileNo,isnull(PM.MotherMobileNo,''XXXXX'') as MotherMobileNo,  
 isnull((Select '+@DayID+' From StudentAttendanceMasterT where StudentID=SS.StudentID and FYear='+@Year +' and Month='+@Month+'),3) as AttandanceStatus  
 from StudentMaster SM left outer join Student_Session SS on SS.StudentID=SM.StudentID  
 left outer join ParentMaster PM on PM.ParentID=SM.ParentID  
 where ( SS.Status=1) and SM.ParentID='+@ParentID+' order by Name'  
 exec (@SQL)  
end
GO
GO
ALTER proc [dbo].[spn_GetStudentRouteStoppages] 
(  
 @StudentID int  ,
 @CurDate datetime
)  
AS  
BEGIN  
	declare @VehicleRouteID int
	select @VehicleRouteID=VehicleRouteID from StudentMaster where StudentID=@StudentID

	 Select StopID,VehicleRouteID,TRD.AreaID,AM.AreaName,AM.Latitude,AM.Longitude, 
	 CONVERT(varchar(15),DATEADD(MINUTE,  TVR.DelayFromRouteTime, [time]),100) as [Time],HaltDuration,HaltDurationR,SequenceNo,SequenceNoR  
	 ,cast(DATEADD(MINUTE,  TVR.DelayFromRouteTime, [timeR]  ) as nvarchar(8)) as [TimeR],  
	 HaltDurationR,SequenceNoR from TransportRouteDetails TRD left outer join Transport_Vehicle_Route TVR on TVR.RouteID=TRD.RouteID  
	 left outer join AreaMaster AM on AM.AreaID=TRD.AreaID  
	 where tvr.VehicleRouteID=@VehicleRouteID   
	 order by SequenceNo  
	 
	Declare @ConductorID int
	Select @ConductorID=ConductorID from VehicleDetails where VehicleID=(Select VehicleID from [dbo].[Transport_Vehicle_Route] where VehicleRouteID=@VehicleRouteID)

	select EmployeeName,MobileNumber,'/Images/EmployeeImage/'+cast(EmployeeID as nvarchar(10))+'_'+Photo as Photo,30 as RefreshTime
	from EmployeeMaster where EmployeeID=@ConductorID

	select isnull(@VehicleRouteID,0)
END  
GO
Create procedure [dbo].[spnp_GetStudentsForParantApp]--1,2020,06,'D19'  
(  
@ParentID nvarchar(10),  
@Year nvarchar(5)=2017,  
@Month nvarchar(5)=2,  
@DayID nvarchar(4)='D1'  
)  
as  
begin  
 Declare @SBranchID int  
 Select @SBranchID=SBranchID from ParentMaster where ParentID=@ParentID  
 declare @TransportLocationMode nvarchar(10)  
 select @TransportLocationMode=isnull(TransportLocationMode,'1') from SBranchMaster where SBranchID=@SBranchID  
 update AppUsers set LastActive=getdate() where UserID=@ParentID and UserType=4  
 declare @SQL nvarchar(max)  
 set @SQL='Select SM.StudentID, Name, isnull(SS.RollNo,''--'') as RollNo,(case when Gender=0 then ''Male'' else ''Female'' end) as Gender,isnull(SM.BloodGroup,''Not Specified'') as BloodGroup,  
 isnull(AadharCardNo,''---- ---- ---- ----'') as AadharCardNo,isnull(FORMAT(SM.DOJ, ''dd MMM, yyyy''),''-- ---, ----'') as DateOfAdmission,  
 isnull(FORMAT(SM.DOB, ''dd MMM, yyyy''),''-- ---, ----'') as DateOfBirth,'+isnull(@TransportLocationMode,'0')+' as LocationMode,  
 isnull(''/images/StudentImage/''+cast(SM.StudentID as nvarchar(10))+''_''+Photo,(case when Gender=0 then ''/Images/MaleNoImage.png'' else ''/Images/FemaleNoImage.png'' end)) as Photo,  
 isnull(''/Images/ParentImage/Father/''+cast(PM.ParentID as nvarchar(10))+''_1_''+FatherImage,''/Images/MaleNoImage.png'') as FatherImage,  
 isnull(''/Images/ParentImage/Mother/''+cast(PM.ParentID as nvarchar(10))+''_2_''+MotherImage,''/Images/FemaleNoImage.png'') as MotherImage,  
 PM.FatherName,PM.MotherName,isnull(FORMAT(PM.FatherDOB, ''dd MMM, yyyy''),''-- ---, ----'') as FathersDOB,isnull(FatherEmailID,''----@---.--'') as FatherEmailID  
 ,isnull(FORMAT(PM.MotherDOB, ''dd MMM, yyyy''),''-- ---, ----'') as MothersDOB,isnull(MotherEmailID,''----@---.--'') as MotherEmailID,  
 ''STUD''+RIGHT(REPLICATE(''0'',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) as [StudentSID],SM.SchoolUID,  
 (Select ClassName from ClassMaster CM where CM.ClassID=SS.ClassID) as ClassName,  
 (select VehicleID from Transport_Vehicle_Route where VehicleRouteID =SM.VehicleRouteID) as VehicleID,
 (Select Name from Class_Sections CS where CS.ID=SS.SectionID) as SectionName,isnull(PM.FatherMobileNo,''XXXXX'') as FatherMobileNo,isnull(PM.MotherMobileNo,''XXXXX'') as MotherMobileNo,  
 isnull((Select '+@DayID+' From StudentAttendanceMasterT where StudentID=SS.StudentID and FYear='+@Year +' and Month='+@Month+'),3) as AttandanceStatus  
 from StudentMaster SM left outer join Student_Session SS on SS.StudentID=SM.StudentID  
 left outer join ParentMaster PM on PM.ParentID=SM.ParentID  
 where ( SS.Status=1) and SM.ParentID='+@ParentID+' order by Name'  
 exec (@SQL)  
end
GO

  alter table [Student_Session]
  Add DateOfPromotion datetime
  GO
  alter table [Student_Session]
  Add DateOfRemoval datetime
  GO
  alter table [Student_Session]
  Add ConductWork nvarchar(500)
  GO
  alter table [Student_Session]
  Add CauseOfRemoval nvarchar(500)
  
  GO

CREATE TABLE [dbo].[StudentLeavingCertificateMaster](
	[CertID] [int] IDENTITY(1,1) NOT NULL,
	[StudentID] [int] NULL,
	[SessionID] [int] NULL,
	[Stream] [nvarchar](500) NULL,
	[DuesCleared] [nvarchar](50) NULL,
	[Character] [nvarchar](50) NULL,
	[Promotion] [nvarchar](50) NULL,
	[TCDate] [date] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[TCSlNo] [nvarchar](50) NULL
) ON [PRIMARY]
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
		 (select count(*) from [dbo].[StudentLeavingCertificateMaster] SLC where SLC.StudentID=SM.StudentID) as SLCGenerated,
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
		 (select count(*) from [dbo].[StudentLeavingCertificateMaster] SLC where SLC.StudentID=SM.StudentID) as SLCGenerated,
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
Create Procedure [dbo].[sp_GetStudentSLCDetails]
(
@StudentID int,
@SessionID int
)
AS
BEGIN
	select * from [dbo].[StudentLeavingCertificateMaster] where StudentID=@StudentID and SessionID=@SessionID
	select SS.StudentSessionUID,SS.StudentID,SS.FromDate,SS.ToDate,SS.RollNo,SS.IsLeft,SS.DateOfPromotion,SS.DateOfRemoval,SS.ConductWork,SS.CauseOfRemoval
	,CM.ClassName,CS.Name as SectionName,SM.SessionName
	from Student_Session SS left outer join ClassMaster CM on CM.ClassID=SS.ClassID
	left outer join Class_Sections CS on CS.ID=SS.SectionID
	left outer join SessionMaster SM on SM.SessionID=SS.SessionID
	where StudentID=@StudentID
	order by FromDate
	
	select StudentID,Name,DOB,DOJ,SchoolUID,PSchoolName,PSchoolMedium,PClassName,PSResult,PSchoolCity,PSChoolState from StudentMaster
	where StudentID=@StudentID
END
GO
CREATE TYPE [dbo].[ut_StudentSLCSessionDetails] AS TABLE(
	[StudentSessionUID] [int] NULL,
	[DateOfPromotion] Date NULL,
	[DateOfRemoval] Date NULL,
	[ConductWork] nvarchar(500) NULL,
	[CauseOfRemoval] nvarchar(500) NULL,
	[IsLeft] int NULL
)
GO
Create  Procedure [dbo].[sp_UpdateStudentSLCDetails]
(
@CertID int,
@StudentID int,
@SessionID int,
@Stream nvarchar(50),
@DuesCleared nvarchar(50),
@Character nvarchar(50),
@Promotion nvarchar(50),
@TCDate date,
@CreatedDate datetime,
@CreatedBy int,
@TCSLNo nvarchar(50),
@SBranchID int,
@SessionSLCDetails ut_StudentSLCSessionDetails READONLY,
@DOB date,
@DOJ date,
@SchoolUID nvarchar(50),
@PSchoolName nvarchar(50),
@PSchoolMedium nvarchar(50),
@PClassName nvarchar(50),
@PSResult nvarchar(50),
@PSchoolCity nvarchar(50),
@PSChoolState nvarchar(50),
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from [dbo].[StudentLeavingCertificateMaster] where StudentID=@StudentID and SessionID=@SessionID
		Update  Student_Session set DateOfPromotion=null,DateOfRemoval=null,ConductWork=null,CauseOfRemoval=null,IsLeft=0 
		where StudentID=@StudentID and SessionID=@SessionID
	end
	else
	Begin
		if(@CertID=0)
		begin
			Insert into [dbo].[StudentLeavingCertificateMaster](StudentID,SessionID,Stream,DuesCleared,Character,Promotion,TCDate,CreatedDate,CreatedBy,TCSLNo,SBranchID)
			Values(@StudentID,@SessionID,@Stream,@DuesCleared,@Character,@Promotion,@TCDate,@CreatedDate,@CreatedBy,@TCSLNo,@SBranchID)
		end
		else
		begin
			Update [dbo].[StudentLeavingCertificateMaster] set Stream=@Stream,DuesCleared=@DuesCleared,Character=@Character,Promotion=@Promotion
			,TCDate=@TCDate,TCSLNo=@TCSLNo where CertID=@CertID
		end
		UPDATE c SET c.DateOfPromotion=t.DateOfPromotion,c.DateOfRemoval=t.DateOfRemoval,c.ConductWork=t.ConductWork
		,c.CauseOfRemoval=t.CauseOfRemoval,IsLeft=t.IsLeft
		FROM  Student_Session c INNER JOIN @SessionSLCDetails t ON c.StudentSessionUID = t.StudentSessionUID;
	End
	Update StudentMaster set DOB=@DOB,DOJ=@DOJ,SchoolUID=@SchoolUID,PSchoolName=@PSchoolName,PSchoolMedium=@PSchoolMedium
	,PClassName=@PClassName,PSResult=@PSResult,PSchoolCity=@PSchoolCity,PSChoolState=@PSChoolState
	where StudentID=@StudentID
END
GO
Create Procedure [dbo].[sp_GetStudentSLCPrintDetails]
(
@StudentID int,
@SessionID int
)
AS
BEGIN
	select * from [dbo].[StudentLeavingCertificateMaster] where StudentID=@StudentID and SessionID=@SessionID
	select SS.StudentSessionUID,SS.StudentID,SS.FromDate,SS.ToDate,SS.RollNo,SS.IsLeft,SS.DateOfPromotion,SS.DateOfRemoval,SS.ConductWork,SS.CauseOfRemoval
	,CM.ClassName,CS.Name as SectionName,SM.SessionName
	from Student_Session SS left outer join ClassMaster CM on CM.ClassID=SS.ClassID
	left outer join Class_Sections CS on CS.ID=SS.SectionID
	left outer join SessionMaster SM on SM.SessionID=SS.SessionID
	where StudentID=@StudentID
	order by FromDate
	
	declare @ParentID int,@SBranchID int
	select @ParentID=ParentID,@SBranchID=SBranchID from StudentMaster where StudentID=@StudentID

	select StudentID,Gender,Name,DOB,DOJ,SchoolUID,PSchoolName,PSchoolMedium,PClassName,PSResult,PSchoolCity,PSChoolState
	GuardianName,MiniAddress from StudentMaster
	where StudentID=@StudentID

	select FatherName,MotherName,FatherOccupationID,MotherOccupationID from ParentMaster where ParentID=@ParentID

	select * from SBranchMaster where SBranchID=@SBranchID
END
GO
  
Alter proc [dbo].[sp_GetAppSplashLogo]  
(  
@SchoolID int=0  
)  
AS  
BEGIN  
 if(@SchoolID=0)  
 begin  
  Select Top 1 '/Images/SBranchLogo/'+Cast(SBranchID as nvarchar(2))+'_'+Logo as Logo from SBranchMaster  
 end  
 else  
 begin  
  declare @SBranchID int  
  select @SBranchID=SBranchID from SBranchMaster where SchoolID=@SchoolID  
  Select Top 1 '/Images/SBranchLogo/'+Cast(SBranchID as nvarchar(2))+'_'+Logo as Logo from SBranchMaster where SBranchID=isnull(@SBranchID,SBranchID)  
 end  
END  
GO
GO
ALTER procedure [dbo].[sp_GetStudentFeeDetailsNew]
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

	select top 1 @ClassID=SS.ClassID,@FeePaymentMode=FeePaymentMode,
	@QuotaID=SS.QuotaID,@SectionID=SS.SectionID,@StuSessionSDate=(Case when @SessionStartDate<isnull(SS.FromDate,@SessionStartDate) then SS.FromDate else @SessionStartDate end),
	@StuSessionEDate=(Case when @SessionEndDate>SS.ToDate then SS.ToDate else @SessionEndDate end),
	@IsAdmissionFee=SS.IsAdmissionFeeApplicable,@IsCustomFee=isnull(SS.IsCustomFee,0),@StudentSessionUID=StudentSessionUID
	from Student_Session SS 
	where SS.StudentID=@StudentID and SessionID=@SessionID 
	order by (case when @QDate between FromDate and ToDate then 0 else 1 end)asc,Status desc
	
	
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

	
	Declare @NoFeeMonths table (Month int,FeeTypeID int)

	insert into @NoFeeMonths
	select Month,FeeTypeID from SessionClassNoFeeMonths where ClassID=@ClassID and SessionID=@SessionID
	
	Declare @FeeStructureTable table (FeeTypeID int,FeeTypeName nvarchar(50),FeeTypeApplicable int,FeeAmount numeric(10,2),Status int,Months nvarchar(50))
	
	Declare @FeeDetailTable table (FeeMonth int,FeeYear int, FeeTypeID int,FeeTypeName nvarchar(100),FeeAmount numeric(10,2),QDiscount numeric(10,2),RDiscount numeric(10,2),
	PayApplicableAmount numeric(10,2),CustomFee numeric(10,2),PaidAmount numeric(10,2),IsCustomFee int,IsPayment int,FeeTypeApplicable int)

	insert into @FeeStructureTable
	select FTM.FeeTypeID,FTM.FeeTypeName,FTM.FeeTypeApplicable,CFS.FeeAmount,isnull(CFS.Status,1),FTM.Months
	from FeeTypeMaster FTM left outer join [dbo].[ClassFeeStructureMaster] CFS on CFS.FeeTypeID=FTM.FeeTypeID 
	and ClassID=@ClassID and GroupID=@GroupID
	and CFS.SessionID=@SessionID where FTM.SBranchID=@SBranchID
	
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
				(case when FTS.FeeTypeApplicable=5 then [dbo].[fn_GetStudentTransportFeeAmount](@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID) 
				when FeeTypeApplicable=6 then [dbo].[fn_GetStudentHostalFeeAmount](@LMonth,@LYear,@StudentID,@HostelFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID)
				else FTS.FeeAmount end) as FeeAmount,
				QD.Discount as QuotaDiscount,
				(case when FTS.FeeTypeApplicable=7 then PD.DiscAmt when PD.PaymentID is not null then PD.DiscAmt else DA.ApprovedAmount end),PD.NetApplicablePayment,SFD.FeeAmount,PD.PaymentRecieved,
				(case when @IsCustomFee is null then 0 else @IsCustomFee end) as IsCustomFee,
				(case when PD.PaymentID is null then 0 else 1 end) as IsPayment,FTS.FeeTypeApplicable
				from @FeeStructureTable FTS left outer join @QuotaDiscounts QD on QD.FeeTypeID=FTS.FeeTypeID
				left outer join [dbo].[StudentFeeDetails] SFD on SFD.StudentID=@StudentID and SFD.SessionID=@StudentSessionUID and SFD.FeeTypeID=FTS.FeeTypeID
				left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
				left outer join @DiscountApproved DA on DA.FeeTypeID=FTS.FeeTypeID and DA.FeeMonth=@LMonth and DA.FeeYear=@LYear
				where FTS.FeeTypeApplicable in (select FeeTypeID from @MTFT where MonthTypeID=@MonthType) 
				and FTS.FeeTypeID <> (case when @IsLatePay=0 then -2 else 0 end) and FTS.FeeTypeApplicable<>(Case when @IsAdmissionFee=0 then 8 else 0 end)
				and isnull(SFD.IsApplicable,FTS.Status)=1 and (FTS.FeeTypeApplicable<>9 or (select count(*) from [dbo].[SplitStringToTable](FTS.Months,',') where Item=@LMonth)>0)
				and (FTS.FeeTypeID in (Select FeeTypeID from @NoFeeMonths where Month=@LMonth) or (Select count(*) from @NoFeeMonths)=0)
			
			set @StuSessionSDate=dateadd(month,1,@StuSessionSDate)
		end

		--select * from @FeeDetailTable 
		
		select FeeMonth,FeeYear,FeeTypeApplicable,FeeTypeID,FeeTypeName,sum(FeeAmount) as FeeAmount,avg(QDiscount) as QDiscount,sum(RDiscount) as RDiscount,sum(PayApplicableAmount) as PayApplicableAmount
		,sum((Case when FeeTypeApplicable in(5,6,7) then FeeAmount else CustomFee end)) as CustomFee,sum(PaidAmount) as PaidAmount,max(IsCustomFee) as IsCustomFee,max(IsPayment) as IsPayment from
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

	
	Declare @NoFeeMonths table (Month int,FeeTypeID int)

	insert into @NoFeeMonths
	select Month,FeeTypeID from SessionClassNoFeeMonths where ClassID=@ClassID and SessionID=@SessionID
	
	Declare @FeeStructureTable table (FeeTypeID int,FeeTypeName nvarchar(50),FeeTypeApplicable int,FeeAmount numeric(10,2),Status int,Months nvarchar(50))

	insert into @FeeStructureTable
	select FTM.FeeTypeID,FTM.FeeTypeName,FTM.FeeTypeApplicable,CFS.FeeAmount,isnull(CFS.Status,1),FTM.Months
	from FeeTypeMaster FTM left outer join [dbo].[ClassFeeStructureMaster] CFS on CFS.FeeTypeID=FTM.FeeTypeID and ClassID=@ClassID 
	and GroupID=@GroupID
	and CFS.SessionID=@SessionID where FTM.SBranchID=@SBranchID
	
	Declare @StuSessionSDate date,@StuSessionEDate date,@StudentID int,@QuotaID int,@IsAdmissionFee int,
	@FeeAmount numeric(10,2),@PreviousDue numeric(10,2),@LateFee numeric(10,2),@Discounts numeric(10,2)
	,@LMonth int,@LYear int,@MonthType int,@BaseDate date,@Paid numeric(10,2),@FeePaymentMode int,@IsCustomFee int
	
	declare @isPaid int
	
		DECLARE stu_Cursor CURSOR FOR
		SELECT SessionStartDate,SessionEndDate,StudentID,QuotaID,IsAdmissionFee,FeeAmount,PreviousDue,LateFee,Discounts,Paid,FeePaymentMode
		FROM @Students where isnull(StudentID,0)!=0 FOR UPDATE OF FeeAmount,PreviousDue,LateFee,Discounts,Paid
		OPEN stu_Cursor
		FETCH NEXT FROM stu_Cursor
		INTO @StuSessionSDate,@StuSessionEDate,@StudentID,@QuotaID,@IsAdmissionFee,@FeeAmount,@PreviousDue,@LateFee,@Discounts,@Paid,@FeePaymentMode

		WHILE (@@FETCH_STATUS = 0)
		BEGIN
			declare @StudentSessionUID int
			set @Discounts=0
			select top 1 @StudentSessionUID=StudentSessionUID,@IsCustomFee=isnull(IsCustomFee,0) from Student_Session where StudentID=@StudentID and SessionID=@SessionID
			order by Status desc
			print(@StudentSessionUID)
			print(@FeePaymentMode)
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
						declare @PDIsc numeric(10,2)=0

						select @PPD=sum((case when isnull(PD.PaymentID,0)!=0 then PD.NetApplicablePayment-isnull(PD.DiscAmt,0)  
						when isnull(@IsCustomFee,0)=1 then SFD.FeeAmount else (FTS.FeeAmount-isnull(FTS.FeeAmount*QD.Discount/100,0)) end)) 
						,@PPP=isnull(Sum(PD.PaymentRecieved),0)
						from @FeeStructureTable FTS left outer join @QuotaDiscounts QD on QD.FeeTypeID=FTS.FeeTypeID
						left outer join [dbo].[StudentFeeDetails] SFD on SFD.StudentID=@StudentID and SFD.SessionID=@StudentSessionUID and 
						SFD.FeeTypeID=FTS.FeeTypeID
						left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
						where FTS.FeeTypeApplicable in (select FeeTypeID from @MTFT where MonthTypeID=@MonthType) and FTS.FeeTypeApplicable!=5
						and FTS.FeeTypeApplicable <> 7 and FTS.FeeTypeApplicable<>(Case when @IsAdmissionFee=0 then 8 else 0 end)
						and isnull(SFD.IsApplicable,FTS.Status)=1 and (FTS.FeeTypeApplicable<>9 or 
						(select count(*) from [dbo].[SplitStringToTable](FTS.Months,',') where Item=@LMonth)>0)
						and (FTS.FeeTypeID in (Select FeeTypeID from @NoFeeMonths where Month=@LMonth) or (Select count(*) from @NoFeeMonths)=0)

						select @isPaid=count(*) from v_PaymentDetails PD where PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
						declare @pDiscount numeric(10,2)=0
						if(isnull(@isPaid,0)=0)
						begin
							select @pDiscount=isnull(sum(ApprovedAmount),0) from @DiscountApproved where StudentID=@StudentID and FeeMonth=@LMonth and FeeYear=@LYear				
						end
						if((select count(*) from @FeeStructureTable where FeeTypeApplicable=5 and (FeeTypeID in (Select FeeTypeID from @NoFeeMonths where Month=@LMonth) or (Select count(*) from @NoFeeMonths)=0))>0)
						begin
						select @PreviousDue= isnull(@PreviousDue,0)+isnull([dbo].[fn_GetStudentTransportFeeAmount](@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID),0)
						end
						if((select count(*) from @FeeStructureTable where FeeTypeApplicable=6 and (FeeTypeID in (Select FeeTypeID from @NoFeeMonths where Month=@LMonth) or (Select count(*) from @NoFeeMonths)=0))>0)
						begin
						select @PreviousDue= isnull(@PreviousDue,0)+isnull([dbo].[fn_GetStudentHostalFeeAmount](@LMonth,@LYear,@StudentID,@HostelFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID),0)
						end
						select @PreviousDue=isnull(@PreviousDue,0)-isnull(Sum(isnull(PD.PaymentRecieved,0)),0)
						from @FeeStructureTable FTS left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
						where FTS.FeeTypeApplicable in (5,6)
						
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
						and (FTS.FeeTypeID in (Select FeeTypeID from @NoFeeMonths where Month=@LMonth) or (Select count(*) from @NoFeeMonths)=0)
						
						select @isPaid=count(*) from v_PaymentDetails PD where PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
						
						if(isnull(@isPaid,0)=0)
						begin
							select @Discounts=isnull(@Discounts,0)+isnull(sum(ApprovedAmount),0) from @DiscountApproved where StudentID=@StudentID and FeeMonth=@LMonth and FeeYear=@LYear
						end
						
						if((select count(*) from @FeeStructureTable where FeeTypeApplicable=5 and (FeeTypeID in (Select FeeTypeID from @NoFeeMonths where Month=@LMonth) or (Select count(*) from @NoFeeMonths)=0))>0 )
						begin
						select  @FeeAmount=isnull(@FeeAmount,0)+[dbo].[fn_GetStudentTransportFeeAmount](@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID)
						end
						if((select count(*) from @FeeStructureTable where FeeTypeApplicable=6and (FeeTypeID in (Select FeeTypeID from @NoFeeMonths where Month=@LMonth) or (Select count(*) from @NoFeeMonths)=0))>0)
						begin
						select  @FeeAmount=isnull(@FeeAmount,0)+[dbo].[fn_GetStudentHostalFeeAmount](@LMonth,@LYear,@StudentID,@HostelFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID)
						end
						select @TPaid=Sum(isnull(PD.PaymentRecieved,0))
						from @FeeStructureTable FTS left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID 
						and PD.Month=@LMonth and PD.Year=@LYear
						where FTS.FeeTypeApplicable in (5,6) 

						select @CLateFee=sum((case when isnull(PD.PaymentID,0)!=0 then PD.NetApplicablePayment else FTS.FeeAmount end)) 
						,@CPaid=Sum(isnull(PD.PaymentRecieved,0))
						from @FeeStructureTable FTS left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
						where FTS.FeeTypeApplicable = (case when @IsLatePay=0 then 0 else 7 end)
						and isnull(FTS.Status,0)=1 and (FTS.FeeTypeID in (Select FeeTypeID from @NoFeeMonths where Month=@LMonth) or (Select count(*) from @NoFeeMonths)=0)

						set @LateFee=isnull(@LateFee,0)+isnull(@CLateFee,0)
						set @Paid=isnull(@Paid,0)+isnull(@CPaid,0)+isnull(@TPaid,0)+isnull(@pPaid,0)
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
		SessionStartDate,SessionEndDate,IsAdmissionFee,SchoolUID,FeeAmount,PreviousDue,LateFee,Discounts,Paid from @Students where isnull(StudentID,0)!=0

END
GO
Alter procedure [dbo].[sp_GetEmployeeDetails]  
(  
@EmployeeID int,  
@SBranchID int  
)  
as   
begin   
  
exec sp_GetEmployeeTypes @SBranchID  
if(@EmployeeID!=0)  
begin  
SELECT  (case when EmployeeType in (-1,-2) then 'TRA' else 'EMP' end)+RIGHT(REPLICATE('0',6)+CAST(EmployeeID AS VARCHAR(6)),6) as EmployeeSID,EmployeeID  
      ,[EmployeeName]      ,[DOB]      ,[Gender]      ,[Nationality]      ,[ReligionID]  ,[CategoryID]    ,[DOJ]      ,[EmailID]  
      ,[BloodGroup]      ,[MaritalStatus]      ,[MotherTongueID]      ,[MobileNumber]      ,[LandLineNumber]      ,[PassportNo]  
      ,[FatherHubName]      ,[Padd_HouseNo]      ,[Padd_Street]      ,[Padd_Area]      ,[Padd_Sector]      ,[Padd_PinCode]  
      ,[Padd_District]      ,[Padd_State]      ,[Padd_Country]      ,[Cadd_HouseNo]      ,[Cadd_Street]      ,[Cadd_Area]  
      ,[Cadd_Sector]      ,[Cadd_PinCode]      ,[Cadd_DistrictCode]      ,[Cadd_StateCode]      ,[Cadd_CountryCode]      ,[DepartmentID]  
      ,[Role]      ,[AccessCardNo]      ,[VehicleNo]      ,[DriverName]      ,[DriverMobileNo]      ,[DesignationID]      ,[AddressProof]  
      ,[BirthCertificate]      ,[CategoryCertificate]      ,[Photo]      ,[ExperienceCertificate]      ,[RelievingCertificate]  
      ,[PFDeclaration]      ,[PANCardCopy]      ,[BankAccountProof]      ,[MedicalCertificate]      ,[AppointmentLetter]  
      ,[PANnumber]      ,[PFNumber],EmployeeType     ,[BankName]      ,[AccountNumber],AccountName,IFSCCode      ,[VehicleRouteID]      ,[StopID]      ,[Status]  
   ,AadharNumber,LicenceNumber  
  FROM [dbo].[EmployeeMaster] where SBranchID=@SBranchID and EmployeeID=@EmployeeID  
   
 declare @AAreaID int  
 declare @ACityID int  
 declare @AStateID int  
 declare @ACountryID int  
  
 select @ACityID=isnull(Cadd_DistrictCode,0),@AStateID=Cadd_StateCode,@AAreaID=Cadd_Area,@ACountryID=Cadd_CountryCode from EmployeeMaster where EmployeeID=@EmployeeID  
  
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
   
 declare @EmployeeTypeID int  
 select @EmployeeTypeID=EmployeeType from EmployeeMaster where EmployeeID=@EmployeeID   
  
  Select isnull(ESD.EmployeeSalaryID,0) as EmployeeSalaryID,ETSD.SalaryTypeID,ETSD.EmployeeTypeID,  
  STM.[TypeName] as SalaryTypeName  ,STM.IsDeduction
  ,isnull(ESD.Amount,0) as Amount,isnull(ETSD.Amount,0) as SalaryStructure from   
  [dbo].[EmployeeTypeSalaryDetails] ETSD left outer join  
  [dbo].[EmployeeSalaryDetails] ESD on ESD.SalaryTypeID=ETSD.SalaryTypeID  and EmployeeID=@EmployeeID
  left outer join [dbo].[SalaryTypeMaster] STM on STM.TypeID=ETSD.SalaryTypeID
   where  ETSD.EmployeeTypeID=@EmployeeTypeID  
     
  Select [EmpEduID]      ,[Qualification]      ,[Stream]      ,[InstituteName]      ,[StartDate]  
      ,[EndDate]      ,[PassingYear]      ,[Percentage]      ,[Grade]  
  FROM [dbo].[EmployeeEducationDetails] where EmployeeID=@EmployeeID order by [StartDate] desc  
  
 SELECT [EmpExpID]      ,[InstituteName]      ,[Designation]      ,[FromDate]      ,[EndDate]      ,[SubjectsClasses]  
  FROM [dbo].[EmployeeExperienceDetails] where EmployeeID=@EmployeeID order by FromDate desc  
  
 exec [dbo].[spn_GetEmployeesTransportDetails] @EmployeeID,@SBranchID  
  
 select isnull(ELTM.ELID,0) as ELID, LTM.LeaveTypeID,LTM.[LeaveTypeName],isnull(ELTM.YearlyQuota, LTM.[YearlyQuota]) as YearlyQuota,  
 isnull(ELTM.MonthlyQuota,LTM.[MonthlyQuota] ) as MonthlyQuota  
 from LeaveTypeMaster LTM left outer join [dbo].[EmployeeLeaveTypeMaster] ELTM   
 on ELTM.[LeaveTypeID]=LTM.LeaveTypeID and ELTM.EmployeeID=@EmployeeID  
  
 Select TS.TSID,SubjectID,EducationLevelID,GroupID,  
 (Select GroupName from GroupMaster GM where GM.GroupID=TS.GroupID) as GroupName,  
 (Select SubjectName from SubjectMasterAll SM where SM.SubjectID=TS.SubjectID) as SubjectName,  
 (Select Name from EducationLevelMaster ELM where ELM.ID=TS.EducationLevelID) as EducationLevel  
 from [dbo].[Teacher_Subject] TS where TS.EmployeeID=@EmployeeID  
  
 Select  ID,  Name from EducationLevelMaster where SBranchID =@SBranchID  
 declare @ELID int  
 declare @GID int  
 select @ELID=min(ID) from EducationLevelMaster where SBranchID =@SBranchID  
 select @GID=min(GroupID) from GroupMaster where EducationLevelID=@ELID  
 select GroupID as ID, GroupName as Name from GroupMaster where EducationLevelID = @ELID  
 Select SubjectID as ID, SubjectName as Name from SubjectMaster where GroupID=@GID   
   
end  
End  
GO
ALTER procedure [dbo].[sp_GetEmployeeDetailsNew]  
(  
@EmployeeID int,  
@SBranchID int  
)  
as   
begin     
	exec sp_GetEmployeeTypes @SBranchID  
	if(@EmployeeID!=0)  
	begin  
		SELECT  (case when EmployeeType in (-1,-2) then 'TRA' else 'EMP' end)+RIGHT(REPLICATE('0',6)+CAST(EmployeeID AS VARCHAR(6)),6) as EmployeeSID,EmployeeID  
		,[EmployeeName]      ,[DOB]      ,[Gender]      ,[Nationality]      ,[ReligionID]  ,[CategoryID]    ,[DOJ]      ,[EmailID]  
		,[BloodGroup]      ,[MaritalStatus]      ,[MotherTongueID]      ,[MobileNumber]      ,[LandLineNumber]      ,[PassportNo]  
		,[FatherHubName]      ,[Padd_HouseNo]      ,[Padd_Street]      ,[Padd_Area]      ,[Padd_Sector]      ,[Padd_PinCode]  
		,[Padd_District]      ,[Padd_State]      ,[Padd_Country]      ,[Cadd_HouseNo]      ,[Cadd_Street]      ,[Cadd_Area]  
		,[Cadd_Sector]      ,[Cadd_PinCode]      ,[Cadd_DistrictCode]      ,[Cadd_StateCode]      ,[Cadd_CountryCode]      ,[DepartmentID]  
		,[Role]      ,[AccessCardNo]      ,[VehicleNo]      ,[DriverName]      ,[DriverMobileNo]      ,[DesignationID]      ,[AddressProof]  
		,[BirthCertificate]      ,[CategoryCertificate]      ,[Photo]      ,[ExperienceCertificate]      ,[RelievingCertificate]  
		,[PFDeclaration]      ,[PANCardCopy]      ,[BankAccountProof]      ,[MedicalCertificate]      ,[AppointmentLetter]  
		,[PANnumber]      ,[PFNumber],EmployeeType     ,[BankName]      ,[AccountNumber],AccountName,IFSCCode      ,[VehicleRouteID]      ,[StopID]      ,[Status]  
		,AadharNumber,LicenceNumber  
		FROM [dbo].[EmployeeMaster] where SBranchID=@SBranchID and EmployeeID=@EmployeeID  
   
		 declare @AAreaID int  
		 declare @ACityID int  
		 declare @AStateID int  
		 declare @ACountryID int  
  
		 select @ACityID=isnull(Cadd_DistrictCode,0),@AStateID=Cadd_StateCode,@AAreaID=Cadd_Area,@ACountryID=Cadd_CountryCode from EmployeeMaster where EmployeeID=@EmployeeID  
  
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
   
		 declare @EmployeeTypeID int  
		 select @EmployeeTypeID=EmployeeType from EmployeeMaster where EmployeeID=@EmployeeID   
  
		  Select isnull(ESD.EmployeeSalaryID,0) as EmployeeSalaryID,ETSD.SalaryTypeID,ETSD.EmployeeTypeID,  
		  STM.[TypeName] as SalaryTypeName  ,STM.IsDeduction
		  ,isnull(ESD.Amount,0) as Amount,isnull(ETSD.Amount,0) as SalaryStructure from   
		  [dbo].[EmployeeTypeSalaryDetails] ETSD left outer join  
		  [dbo].[EmployeeSalaryDetails] ESD on ESD.SalaryTypeID=ETSD.SalaryTypeID  and EmployeeID=@EmployeeID
		  left outer join [dbo].[SalaryTypeMaster] STM on STM.TypeID=ETSD.SalaryTypeID
		   where  ETSD.EmployeeTypeID=@EmployeeTypeID   
  
		  Select [EmpEduID]      ,[Qualification]      ,[Stream]      ,[InstituteName]      ,[StartDate]  
			  ,[EndDate]      ,[PassingYear]      ,[Percentage]      ,[Grade]  
		  FROM [dbo].[EmployeeEducationDetails] where EmployeeID=@EmployeeID order by [StartDate] desc  
  
		 SELECT [EmpExpID]      ,[InstituteName]      ,[Designation]      ,[FromDate]      ,[EndDate]      ,[SubjectsClasses]  
		  FROM [dbo].[EmployeeExperienceDetails] where EmployeeID=@EmployeeID order by FromDate desc  
  
		 select THChangeID,ChangeType,UserType,UserID,StartDate,EndDate,SessionID,KeyID,
		(Select AreaName from AreaMaster where AreaID=(select AreaID from [dbo].[TransportRouteDetails] TRD where TRD.StopID=T.KeyID)) as Name
		from [dbo].[TransportHostalAllocationDelocation] T
		where T.UserID=@EmployeeID and UserType=1 and ChangeType=0
		order by StartDate 
  
		 select isnull(ELTM.ELID,0) as ELID, LTM.LeaveTypeID,LTM.[LeaveTypeName],isnull(ELTM.YearlyQuota, LTM.[YearlyQuota]) as YearlyQuota,  
		 isnull(ELTM.MonthlyQuota,LTM.[MonthlyQuota] ) as MonthlyQuota  
		 from LeaveTypeMaster LTM left outer join [dbo].[EmployeeLeaveTypeMaster] ELTM   
		 on ELTM.[LeaveTypeID]=LTM.LeaveTypeID and ELTM.EmployeeID=@EmployeeID  
  
		 Select TS.TSID,SubjectID,EducationLevelID,GroupID,  
		 (Select GroupName from GroupMaster GM where GM.GroupID=TS.GroupID) as GroupName,  
		 (Select SubjectName from SubjectMasterAll SM where SM.SubjectID=TS.SubjectID) as SubjectName,  
		 (Select Name from EducationLevelMaster ELM where ELM.ID=TS.EducationLevelID) as EducationLevel  
		 from [dbo].[Teacher_Subject] TS where TS.EmployeeID=@EmployeeID  
  
		 Select  ID,  Name from EducationLevelMaster where SBranchID =@SBranchID  
		 declare @ELID int  
		 declare @GID int  
		 select @ELID=min(ID) from EducationLevelMaster where SBranchID =@SBranchID  
		 select @GID=min(GroupID) from GroupMaster where EducationLevelID=@ELID  
		 select GroupID as ID, GroupName as Name from GroupMaster where EducationLevelID = @ELID  
		 Select SubjectID as ID, SubjectName as Name from SubjectMaster where GroupID=@GID     
	  end  
  
	 select * from [dbo].[SubReligionMaster]  
  
	 select * from NationalityMaster where SBranchID=@SBranchID  
	 select * from ReligionMaster where SBranchID=@SBranchID  
	 select * from MotherTongue where SBranchID=@SBranchID  
	 select * from SocialCategory where SBranchID=@SBranchID  
  End  
GO


Alter proc [dbo].[sp_GetAssignmentList]  
(  
@ClassID int,  
@SectionID int,  
@SubjectID int,  
@ChapterID int,  
@TeacherID int  
)  
AS  
BEGIN    
    
 SELECT [ID] ,[ChapterID] ,  
  [TopicID],(Select isnull(ChapterName,'No Chapter') from ChapterMaster CM where CM.ChapterID=AM.ChapterID)+' > '+(Select isnull(TopicName,'No Topic') from TopicMaster TM where TM.TopicID=AM.TopicID) as TopicName  
  ,[StartDate] ,[Enddate] ,[GracedDays],[TaskType] ,[Title] ,[Detail],[Attachments] ,[SendMail] ,[CreatedOn],  
  (Select SubjectName from SubjectMasterAll SMA where SMA.SubjectID=AM.SubjectID) as SubjectName,  
  (Select Count(*) from AssignmentSubmissions ASub where ASub.AssignmentID=AM.ID and ASub.Status=1) as Submissions  
  FROM [dbo].[AssignmentMaster] AM where ClassID=@ClassID and SectionID=@SectionID and SubjectID=@SubjectID  and TeacherID=@TeacherID  
   order by StartDate desc
   
END
GO
-----------------------------------------------------------------------------------------------------------------------------------------------------------------
--Parent App Event Calender Related
-------------------------------------------------------------------------------------------------------------------------------------------------------------------

Alter proc [dbo].[sp_GetEventCalendar] 
(  
@SBranchID int,  
@Month int,  
@Year int,  
@Status int=0  ,
@ParentID int=0	
)  
AS  
BEGIN  
  
	 if(@Status=0)  
	 begin  
		  Select EventID,Title, [Description], StartDate, EndDate, EventTypeID,  
		  ETM.Name,ETM.BackGroundColor,ETM.TextColor, isnull(ClassesIncluded,'') as ClassesIncludedIDs  
		  , ClassesIncluded,dbo.GetClassNames(isnull(ClassesIncluded,'')) as ClassNames, IsApproved  
		  From EventMaster EM left outer join EventTypeMaster ETM on EM.EventTypeID=ETM.ID   
		  where IsApproved=1 and EM.SBranchID=@SBranchID and ((datepart(month,StartDate)>=@Month-1 and datepart(month,StartDate)<=@Month+1) and datepart(year,StartDate)=@Year or (CONVERT(datetime,CONVERT(nvarchar(4), @Year)+'-'+CONVERT(nvarchar(4),@Month)+'-01') 
		between StartDate and enddate))  
		
		 select HolidayID,HolidayType,(Case when HolidayType=1 then 'Local Holiday' else 'National Holiday' end) 'HolidayTypeName',  
		 Classes 'ClassesIncludedIDs',  
		 dbo.GetClassNames(Classes) 'ClassesIncluded',[dbo].[GetEmployeeTypeNames](AssociatedIDs) as EmployeeTypes,  
		 (Case When DayDuration=1 then 'Full Day' else (Case when DayDuration=2 then 'First Half' else 'Second Half' end)end) 'DayDurationName',  
		 StartDate,EndDate,DATEDIFF(day,StartDate,EndDate)+1 as [Days],DayDuration, [HolidayDescription],Title,[Status],  
		 Classes from HolidayMaster where [Status]=1 and SBranchID=@SBranchID
	 end  
	 else  
	 begin  
		if(@ParentID=0)
		begin
		  Select EventID,Title, [Description], StartDate, EndDate, EventTypeID,  
		  ETM.Name,ETM.BackGroundColor,ETM.TextColor, isnull(ClassesIncluded,'') as ClassesIncludedIDs  
		  , ClassesIncluded,dbo.GetClassNames(isnull(ClassesIncluded,'')) as ClassNames, IsApproved  
		  From EventMaster EM left outer join EventTypeMaster ETM on EM.EventTypeID=ETM.ID   
		  where  EM.SBranchID=@SBranchID and ((datepart(month,StartDate)>=@Month-1 and datepart(month,StartDate)<=@Month+1) and datepart(year,StartDate)=@Year 
		  or (CONVERT(datetime,CONVERT(nvarchar(4), @Year)+'-'+CONVERT(nvarchar(4),@Month)+'-01') between StartDate and enddate))  

		   select HolidayID,HolidayType,(Case when HolidayType=1 then 'Local Holiday' else 'National Holiday' end) 'HolidayTypeName',  
		 Classes 'ClassesIncludedIDs',  
		 dbo.GetClassNames(Classes) 'ClassesIncluded',[dbo].[GetEmployeeTypeNames](AssociatedIDs) as EmployeeTypes,  
		 (Case When DayDuration=1 then 'Full Day' else (Case when DayDuration=2 then 'First Half' else 'Second Half' end)end) 'DayDurationName',  
		 StartDate,EndDate,DATEDIFF(day,StartDate,EndDate)+1 as [Days],DayDuration, [HolidayDescription],Title,[Status],  
		 Classes from HolidayMaster where [Status]=1 and SBranchID=@SBranchID
		end
		else
		Begin
			declare @SessionID int
			select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and cast(@Year as nvarchar(4))+'-'+cast(@Month as nvarchar(2))+'-1' between SessionStartDate and SessionEndDate
			declare @Classes table(ClassID int,SBranchID int)
			Insert into @Classes 
			select ClassID,SBranchID from Student_Session where StudentID in (select StudentID from StudentMaster where ParentID=@ParentID) and SessionID=@SessionID
  
			Select EventID,Title, [Description], StartDate, EndDate, EventTypeID,  
			  ETM.Name,ETM.BackGroundColor,ETM.TextColor, isnull(ClassesIncluded,'') as ClassesIncludedIDs  
			  , ClassesIncluded,dbo.GetClassNames(isnull(ClassesIncluded,'')) as ClassNames, IsApproved  
			  From EventMaster EM left outer join EventTypeMaster ETM on EM.EventTypeID=ETM.ID   
			  where IsApproved=1 and (EM. SBranchID in (Select SBranchID from @Classes) and ClassesIncluded='0')
			or (Select count(*) from [dbo].[SplitStringToTable](ClassesIncluded,',') where Item in(Select ClassID from @Classes))>0
			order by StartDate desc

			 select HolidayID,HolidayType,(Case when HolidayType=1 then 'Local Holiday' else 'National Holiday' end) 'HolidayTypeName',  
			 Classes 'ClassesIncludedIDs',  
			 dbo.GetClassNames(Classes) 'ClassesIncluded',[dbo].[GetEmployeeTypeNames](AssociatedIDs) as EmployeeTypes,  
			 (Case When DayDuration=1 then 'Full Day' else (Case when DayDuration=2 then 'First Half' else 'Second Half' end)end) 'DayDurationName',  
			 StartDate,EndDate,DATEDIFF(day,StartDate,EndDate)+1 as [Days],DayDuration, [HolidayDescription],Title,[Status],  
			 Classes from HolidayMaster where [Status]=1 and (SBranchID in (Select SBranchID from @Classes) and Classes=0)
			or (Select count(*) from [dbo].[SplitStringToTable](Classes,',') where Item in(Select ClassID from @Classes))>0
			order by StartDate desc
		End
	 end  
	 
END  
GO
CREATE proc [dbo].[sp_GetAdminAssignmentList] -- 0,0,0,1,'2020-01-01','2020-06-01',2  
(    
@ClassID int,    
@SectionID int,     
@TeacherID int ,  
@SType int,  
@FromDate date,  
@EndDate date,  
@SBranchID int  
)    
AS    
BEGIN      
  if(@ClassID=0)  
  begin  
   select top 1 @ClassID=ClassID from ClassMaster where Status=1 and SBranchID=@SBranchID   
   order by Isnull(SequenceNo,0),ClassID  
  end  
 if(@SType=0)  
 Begin  
  if(@TeacherID=0)  
  begin  
   select Top 1 @TeacherID=EmployeeID from EmployeeMaster   
   where SBranchID=@SBranchID and EmployeeID in (select EmployeeID from [Teacher_Subject])  
  end  
  SELECT [ID] ,[ChapterID] ,[TopicID]  
  ,[StartDate] ,[Enddate] ,[GracedDays],[TaskType] ,[Title] ,[Detail],[Attachments] ,[SendMail] ,[CreatedOn],    
  (Select SubjectName from SubjectMasterAll SMA where SMA.SubjectID=AM.SubjectID) as SubjectName,  CS.ClassName+'/'+CS.SectionName as EDetail,  
  (Select Count(*) from AssignmentSubmissions ASub where ASub.AssignmentID=AM.ID and ASub.Status=1) as Submissions    
  FROM [dbo].[AssignmentMaster] AM left outer join [dbo].[v_ClassSectionNames] CS on CS.SectionID=AM.SectionID  
  where TeacherID=@TeacherID   and StartDate between @FromDate and @EndDate  
  order by StartDate desc  
 End  
  else  
  Begin  
  if(@SectionID=0)  
  begin  
   Select top 1 @SectionID=ID from Class_Sections where ClassID=@ClassID and Status=1  
  end  
  SELECT [ID] ,[ChapterID] ,[TopicID]  
  ,[StartDate] ,[Enddate] ,[GracedDays],[TaskType] ,[Title] ,[Detail],[Attachments] ,[SendMail] ,[CreatedOn],    
  (Select SubjectName from SubjectMasterAll SMA where SMA.SubjectID=AM.SubjectID) as SubjectName,  EM.EmployeeName as EDetail,  
  (Select Count(*) from AssignmentSubmissions ASub where ASub.AssignmentID=AM.ID and ASub.Status=1) as Submissions    
  FROM [dbo].[AssignmentMaster] AM Left outer join EmployeeMaster EM on EM.EmployeeID=AM.TeacherID  
  where ClassID=@ClassID and SectionID=@SectionID and StartDate between @FromDate and @EndDate   
  order by StartDate desc  
 End    
  
 select ClassID as ID, ClassName as Name from ClassMaster where Status=1 and SBranchID=@SBranchID  
   
 select ID,Name from Class_Sections where ClassID=@ClassID and Status=1  
   
 select EmployeeID as ID, EmployeeName as Name from EmployeeMaster   
 where SBranchID=@SBranchID and EmployeeID in (select EmployeeID from [Teacher_Subject])  
   
  
 select isnull(@ClassID,0)  
 select isnull(@SectionID,0)  
 select isnull(@TeacherID,0)  
END  
GO
ALTER proc [dbo].[sp_GetTeacherEvents]
(
@SBranchID int,
@TeacherID int=0
)
AS
BEGIN
	if(@TeacherID=0)
	begin
		Select EventID,Title, [Description], StartDate, EndDate, EventTypeID,
		ETM.Name,ETM.BackGroundColor,ETM.TextColor
		, ClassesIncluded,dbo.GetClassNames(ClassesIncluded) as ClassNames, IsApproved
		From EventMaster EM left outer join EventTypeMaster ETM on EM.EventTypeID=ETM.ID 
		where IsApproved=1 and EM.SBranchID=@SBranchID 
		order by StartDate desc
	end
	else
	begin		
		declare @Classes table(ClassID int)
		Insert into @Classes 
		select distinct ClassID from Time_Table_Master
		where MondayTeacherID=@TeacherID or TuesdayTeacherID=@TeacherID or WednesdayTeacherID=@TeacherID or ThursdayTeacherID=@TeacherID or FridayTeacherID=@TeacherID or SaturdayTeacherID=@TeacherID
		
		Select EventID,Title, [Description], StartDate, EndDate, EventTypeID,
		ETM.Name,ETM.BackGroundColor,ETM.TextColor
		, ClassesIncluded,dbo.GetClassNames(ClassesIncluded) as ClassNames, IsApproved
		From EventMaster EM left outer join EventTypeMaster ETM on EM.EventTypeID=ETM.ID 
		where IsApproved=1 and EM.SBranchID=@SBranchID 
		and (ClassesIncluded='0' or (Select count(*) from [dbo].[SplitStringToTable](ClassesIncluded,',') where Item in (Select ClassID from @Classes))>0)
			order by StartDate desc
	end
END
GO

  
Alter proc [dbo].[sp_GetStudentSessionEditDataEnt]  
(  
@StudentSessionUID int,  
@SBranchID int  
)  
AS  
BEGIN  
   declare @SessionID int  
   declare @ClassID int  
   declare @GroupID int  
   declare @SectionID int  
   if(@StudentSessionUID=0)  
   begin  
  select @ClassID=min(ClassID) from ClassMaster where SBranchID=@SBranchID  
  select Top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc  
  select @SectionID=min(ID) from Class_Sections where ClassID=@ClassID  
   end  
   else  
   begin  
  select @ClassID=ClassID,@SessionID=SessionID,@SectionID=SectionID from Student_Session SS where SS.StudentSessionUID=@StudentSessionUID and SS.SBranchID=@SBranchID  
   end  
  
  
 if(@StudentSessionUID=0)  
   begin  
    select 0 StudentSessionUID,  0 as StudentID,   @ClassID as ClassID   ,@SectionID as SectionID   ,0 QuotaID   
    ,1 as Status  
   end  
   else  
   begin  
   select SS.StudentSessionUID,  SS.StudentID,   SS.ClassID   ,SS.SectionID   ,SS.QuotaID    
   ,SS.FromDate   ,SS.ToDate,SS.HouseID  ,SS.SessionID
    ,SS.Status,   SS.RollNo,SS.FeePaymentMode,SS.IsAdmissionFeeApplicable  
    from Student_Session SS where SS.StudentSessionUID=@StudentSessionUID and SS.SBranchID=@SBranchID  
  end  
  
  
   select @GroupID=GroupID from Class_Sections where ID=@SectionID  
  
   select ClassID as ID, ClassName as Name from ClassMaster where SBranchID=@SBranchID order by ClassID  
   select ID,Name from Class_Sections where ClassID=@ClassID and Capacity>(select count(*) from Student_Session where SectionID=ID and SessionID=@SessionID) or ID=@SectionID  
  
   Select QuotaID as ID, QuotaName as Name from QuotaMaster QM where IsApproved=1 and SBranchID = @SBranchID  
   and QM.NumberOfStudents>(select count(*) from Student_Session SS where Status=1 and SS.QuotaID=QM.QuotaID)  
  
   declare @SessionStartDate date  
   declare @SessionEndDate date  
   if(isnull(@SessionID,0)=0)  
   begin  
    select @SessionStartDate= SS.FromDate   ,@SessionEndDate= SS.ToDate  
    from Student_Session SS where SS.StudentSessionUID=@StudentSessionUID and SS.SBranchID=@SBranchID  
   end  
   else  
   begin  
   select @SessionStartDate=SessionStartDate,@SessionEndDate=SessionEndDate from SessionMaster where SessionID=@SessionID  
   end  
   select @SessionStartDate  
   select @SessionEndDate  
      
    Select SessionID,SessionName,SessionStartDate,SessionEndDate,SessionStatus from SessionMaster where SBranchID=@SBranchID  
    order by SessionStatus desc  
  
   select isnull(@SessionID,0)  
  
   select Name,ID from HouseMaster where SBranchID=@SBranchID  
  
   select SubjectID as ID,SubjectName as Name from SubjectMaster where GroupID=@GroupID and isnull(MainSubID,0)=0 and IsOptionalSubject=1  
  
   select SSOSID as Extra1,StudentSessionID as Name,OpSubjectID as ID from [dbo].[Student_Session_OptionalSubjects] where StudentSessionID=@StudentSessionUID  
END  
GO
GO
ALTER procedure [dbo].[sp_GetStudentFeeDetailsNew]
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

	select top 1 @ClassID=SS.ClassID,@FeePaymentMode=FeePaymentMode,
	@QuotaID=SS.QuotaID,@SectionID=SS.SectionID,@StuSessionSDate=(Case when @SessionStartDate<isnull(SS.FromDate,@SessionStartDate) then SS.FromDate else @SessionStartDate end),
	@StuSessionEDate=(Case when @SessionEndDate>SS.ToDate then SS.ToDate else @SessionEndDate end),
	@IsAdmissionFee=SS.IsAdmissionFeeApplicable,@IsCustomFee=isnull(SS.IsCustomFee,0),@StudentSessionUID=StudentSessionUID
	from Student_Session SS 
	where SS.StudentID=@StudentID and SessionID=@SessionID 
	order by (case when @QDate between FromDate and ToDate then 0 else 1 end)asc,Status desc
	
	
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
	select FTM.FeeTypeID,FTM.FeeTypeName,FTM.FeeTypeApplicable,CFS.FeeAmount,isnull(CFS.Status,1),FTM.Months
	from FeeTypeMaster FTM left outer join [dbo].[ClassFeeStructureMaster] CFS on CFS.FeeTypeID=FTM.FeeTypeID 
	and ClassID=@ClassID and GroupID=@GroupID
	and CFS.SessionID=@SessionID where FTM.SBranchID=@SBranchID
	
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
				(case when FTS.FeeTypeApplicable=5 then [dbo].[fn_GetStudentTransportFeeAmount](@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID) 
				when FeeTypeApplicable=6 then [dbo].[fn_GetStudentHostalFeeAmount](@LMonth,@LYear,@StudentID,@HostelFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID)
				else FTS.FeeAmount end) as FeeAmount,
				QD.Discount as QuotaDiscount,
				(case when FTS.FeeTypeApplicable=7 then PD.DiscAmt when PD.PaymentID is not null then PD.DiscAmt else DA.ApprovedAmount end),PD.NetApplicablePayment,SFD.FeeAmount,PD.PaymentRecieved,
				(case when @IsCustomFee is null then 0 else @IsCustomFee end) as IsCustomFee,
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
		,sum((Case when FeeTypeApplicable in(5,6,7) then FeeAmount else CustomFee end)) as CustomFee,sum(PaidAmount) as PaidAmount,max(IsCustomFee) as IsCustomFee,max(IsPayment) as IsPayment from
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



ALTER procedure [dbo].[sp_FeePaymentRecieptData]
(
	@PaymentID int
)
as 
begin 
	declare @SessionID int
	declare @SBranchID int
	select @SessionID=SessionID,@SBranchID=SBranchID from PaymentMaster where PaymentID=@PaymentID

	Declare @LastPayDay int
	
	select @LastPayDay=Details from MasterSettings where Type='FeePaymentReminderDate' and SBranchID=@SBranchID 

	Select [PaymentID],RIGHT(REPLICATE('0',6)+CAST([PaymentID] AS VARCHAR(6)),6) as [PaymentSID],@LastPayDay as LastPayDay,SS.StudentID,
	(Select Name from StudentMaster where StudentID=SS.StudentID) as Name,(Select SchoolUID from StudentMaster where StudentID=SS.StudentID) as SchoolUID,
(Select FatherName from ParentMaster where ParentID=(Select ParentID from StudentMaster SM where SM.StudentID=SS.StudentID)) as FatherName,
	(select FeeAmount from [ClassFeeStructureMaster] CFM where CFM.ClassID=SS.ClassID and CFM.SessionID=SS.SessionID and FeeTypeID=-2
	and GroupID=(Select GroupID from Class_Sections CS where CS.ID=SS.SectionID)
	) as LateFee,
	'STUD'+RIGHT(REPLICATE('0',6)+CAST([PayeeID] AS VARCHAR(6)),6) as [StudentSID],
	SS.RollNo, isnull((Select isnull(ClassName,'NA') +' \ '+isnull(SectionName,'NA') from [dbo].[v_ClassSectionNames] where SectionID=SS.SectionID),'NA\NA') as ClassSection
	 ,[PayeeID] ,[ReferanceNumber] ,[PaymentMode]
      ,[Remark] ,[PaymentAmount] ,[PaymentStatus],[PaymentDate] ,PaymentRecieptNo
      ,[PayeeType] ,[PaymentTitle] ,[EnteredDate] ,[CollectedBy],[dbo].[GetPaymentMonthNames](PaymentID) as PayMonths,
	  (Select SessionName from SessionMaster where SessionID=@SessionID) as SessionName
	   from
	  [dbo].[PaymentMaster] PM left outer join Student_Session SS on SS.StudentID=PM.PayeeID  and SS.SessionID=PM.SessionID
	  where PM.PaymentID=@PaymentID
	  
	  declare @PaymentDetails Table(FeeTypeID int,FeeTypeName nvarchar(50),PaymentRecieved numeric(10,2),DiscAmt numeric(10,2),NetApplicablePayment numeric(10,2))

	  insert into @PaymentDetails
	  select PD.FeeTypeID,FTM.FeeTypeName, (PD.PaymentRecieved) as PaymentRecieved,(PD.DiscAmt) as DiscAmt,
	  case when DuesPaidCount>0 then 
	  PD.NetApplicablePayment-(select sum(isnull(PaymentRecieved,0)-isnull(DiscAmt,0)) from PaymentDetails PDt 
	  where PDt.Month=PD.Month and PDt.Year=PD.Year and PDt.PayeeID=PD.PayeeID and PDt.PaymentID<PD.PaymentID and PDt.FeeTypeID=PD.FeeTypeID)
	  else PD.NetApplicablePayment end as NetApplicablePayment
	  from PaymentDetails PD left outer join FeeTypeMaster FTM on FTM.FeeTypeID=PD.FeeTypeID and FTM.SBranchID=@SBranchID
	  where PD.PaymentID=@PaymentID

	  select FeeTypeID,FeeTypeName, sum(PaymentRecieved) as PaymentRecieved,sum(DiscAmt) as DiscAmt,sum(NetApplicablePayment) as NetApplicablePayment
	  from @PaymentDetails
	 Group by FeeTypeID,FeeTypeName

	  Select * from SBranchMaster where SBranchID=@SBranchID
end
GO
ALTER procedure [dbo].[spn_GetStudentsTransportDetailsNew2] --244,1,0
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
		(Select RouteID from TransportRouteMaster where IsApproved=1 and SBranchID=@SBranchID)
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
	where IsApproved=1 and SBranchID=@SBranchID --and RouteID in (select RouteID from TransportRouteDetails where AreaID = @AreaID)

	exec spn_GetTransportRouteVehicleList @SBranchID,@RouteID

	exec spn_GetVehicleRouteStoppages @VehicleRouteID

	select * from [TransportHostalAllocationDelocation] where THChangeID=@THChangeID

end
GO
CREATE procedure dbo.sp_GetParentListForAppSMS  
(  
@SessionID int=1,  
@SBranchID int=1  
)  
AS  
BEGIN  
 if(@SessionID=0)  
 begin  
  Select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc  
 end  
  
 Select SessionID as ID,SessionName as Name,SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc  
  
 select ParentID,FatherName,FatherMobileNo  
 ,'PAR' + RIGHT(REPLICATE('0', 6) + CAST(ParentID AS VARCHAR(6)), 6) AS ParentSID  
 from ParentMaster where ParentID in (  
 select ParentID from StudentMaster where StudentID in (  
 select StudentID from Student_Session where SessionID=@SessionID))  
  
 select isnull(@SessionID,0)  
END
GO
GO
ALTER procedure [dbo].[sp_GetStudentFeeDetailsNew]
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

	select top 1 @ClassID=SS.ClassID,@FeePaymentMode=FeePaymentMode,
	@QuotaID=SS.QuotaID,@SectionID=SS.SectionID,@StuSessionSDate=(Case when @SessionStartDate<isnull(SS.FromDate,@SessionStartDate) then SS.FromDate else @SessionStartDate end),
	@StuSessionEDate=(Case when @SessionEndDate>SS.ToDate then SS.ToDate else @SessionEndDate end),
	@IsAdmissionFee=SS.IsAdmissionFeeApplicable,@IsCustomFee=isnull(SS.IsCustomFee,0),@StudentSessionUID=StudentSessionUID
	from Student_Session SS 
	where SS.StudentID=@StudentID and SessionID=@SessionID 
	order by (case when @QDate between FromDate and ToDate then 0 else 1 end)asc,Status desc
	
	
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
	
	declare @DMonthsTable TABLE(FeeMonth int,FeeYear int)
	
	Declare @NoFeeMonths table (Month int)

	insert into @NoFeeMonths
	select Month from SessionClassNoFeeMonths where ClassID=@ClassID and SessionID=@SessionID
	
	Declare @FeeStructureTable table (FeeTypeID int,FeeTypeName nvarchar(50),FeeTypeApplicable int,FeeAmount numeric(10,2),Status int,Months nvarchar(50))
	
	Declare @FeeDetailTable table (FeeMonth int,FeeYear int, FeeTypeID int,FeeTypeName nvarchar(100),FeeAmount numeric(10,2),QDiscount numeric(10,2),RDiscount numeric(10,2),
	PayApplicableAmount numeric(10,2),CustomFee numeric(10,2),PaidAmount numeric(10,2),IsCustomFee int,IsPayment int,FeeTypeApplicable int)

	insert into @FeeStructureTable
	select FTM.FeeTypeID,FTM.FeeTypeName,FTM.FeeTypeApplicable,CFS.FeeAmount,isnull(CFS.Status,1),FTM.Months
	from FeeTypeMaster FTM left outer join [dbo].[ClassFeeStructureMaster] CFS on CFS.FeeTypeID=FTM.FeeTypeID 
	and ClassID=@ClassID and GroupID=@GroupID
	and CFS.SessionID=@SessionID where FTM.SBranchID=@SBranchID
	
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
			declare @MonthDiff int=DATEDIFF(month,@SessionStartDate,cast(cast(@LYear as nvarchar(5))+'-'+cast(@LMonth as nvarchar(2))+'-1' as date))%@FeePaymentMode
			declare @DiffDate date=dateadd(month,-@MonthDiff,cast(@LYear as nvarchar(4))+'-'+cast(@LMonth as nvarchar(2))+'-1')
			print(cast(@LMonth as nvarchar(2))+' : Month Diff : '+cast(@MonthDiff as nvarchar(10)))
			set @LMonth = datepart(month,@StuSessionSDate)
			set @LYear = datepart(year,@StuSessionSDate)
			print('Loop Month : '+cast(@LMonth as nvarchar(10)))
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
				(case when FTS.FeeTypeApplicable=5 then [dbo].[fn_GetStudentTransportFeeAmount](@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID) 
				when FeeTypeApplicable=6 then [dbo].[fn_GetStudentHostalFeeAmount](@LMonth,@LYear,@StudentID,@HostelFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID)
				else FTS.FeeAmount end) as FeeAmount,
				QD.Discount as QuotaDiscount,
				(case when FTS.FeeTypeApplicable=7 then PD.DiscAmt when PD.PaymentID is not null and isnull(PD.PaymentRecieved,0)>0 then PD.DiscAmt else DA.ApprovedAmount end),
				PD.NetApplicablePayment,SFD.FeeAmount,PD.PaymentRecieved,
				(case when @IsCustomFee is null then 0 else @IsCustomFee end) as IsCustomFee,
				(case when PD.PaymentID is null then 0 else 1 end) as IsPayment,FTS.FeeTypeApplicable
				from @FeeStructureTable FTS left outer join @QuotaDiscounts QD on QD.FeeTypeID=FTS.FeeTypeID
				left outer join [dbo].[StudentFeeDetails] SFD on SFD.StudentID=@StudentID and SFD.SessionID=@StudentSessionUID and SFD.FeeTypeID=FTS.FeeTypeID
				left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear
				left outer join @DiscountApproved DA on DA.FeeTypeID=FTS.FeeTypeID and DA.FeeMonth=DatePart(month,@DiffDate) and DA.FeeYear=DatePart(year,@DiffDate)
				where FTS.FeeTypeApplicable in (select FeeTypeID from @MTFT where MonthTypeID=@MonthType) 
				and FTS.FeeTypeID <> (case when @IsLatePay=0 then -2 else 0 end) and FTS.FeeTypeApplicable<>(Case when @IsAdmissionFee=0 then 8 else 0 end)
				and isnull(SFD.IsApplicable,FTS.Status)=1 and (FTS.FeeTypeApplicable<>9 or (select count(*) from [dbo].[SplitStringToTable](FTS.Months,',') where Item=@LMonth)>0)

			end
			set @StuSessionSDate=dateadd(month,1,@StuSessionSDate)
		end

		--select * from @FeeDetailTable 
		
		select FeeMonth,FeeYear,FeeTypeApplicable,FeeTypeID,FeeTypeName,sum(FeeAmount) as FeeAmount,avg(QDiscount) as QDiscount,sum(RDiscount) as RDiscount,sum(PayApplicableAmount) as PayApplicableAmount
		,sum((Case when FeeTypeApplicable in(5,6,7) then FeeAmount else CustomFee end)) as CustomFee,sum(PaidAmount) as PaidAmount,max(IsCustomFee) as IsCustomFee,max(IsPayment) as IsPayment from
		(Select 	datepart(month,dateadd(month,-Diff,cast(cast(FeeYear as nvarchar(5))+'-'+cast(FeeMonth as nvarchar(2))+'-1' as date))) as FeeMonth,
		datepart(year,dateadd(month,-Diff,cast(cast(FeeYear as nvarchar(5))+'-'+cast(FeeMonth as nvarchar(2))+'-1' as date))) as FeeYear,
		FeeTypeApplicable,FeeTypeID,FeeTypeName,FeeAmount,QDiscount,RDiscount,isnull(PayApplicableAmount,0) as PayApplicableAmount,CustomFee,PaidAmount,IsCustomFee,IsPayment
		from
		(select *,(DATEDIFF(month,@SessionStartDate,cast(cast(FeeYear as nvarchar(5))+'-'+cast(FeeMonth as nvarchar(2))+'-1' as date))%@FeePaymentMode) as Diff
		from @FeeDetailTable)t)t2
		group by FeeMonth,FeeYear,FeeTypeApplicable,FeeTypeID,FeeTypeName
		order by FeeYear,FeeMonth,case when FeeTypeApplicable=7 then 0 else FeeTypeID end,FeeTypeName

END
GO
DROP TABLE [dbo].[QuestionBankMaster]
GO
CREATE TABLE [dbo].[QuestionBankMaster](
	[QuestionID] [int] IDENTITY(1,1) NOT NULL,
	[ClassID] [int] NULL,
	[GroupID] [int] NULL,
	[SubjectID] [int] NULL,
	[ChapterID] [int] NULL,
	[TopicID] [int] NULL,
	[QuestionType] [int] NULL,
	[Complexity] [int] NULL,
	[QuestionText] [nvarchar](max) NULL,
	[Option1] [nvarchar](max) NULL,
	[Option2] [nvarchar](max) NULL,
	[Option3] [nvarchar](max) NULL,
	[Option4] [nvarchar](max) NULL,
	[Answer] [nvarchar](max) NULL,
	[Explaination] [nvarchar](max) NULL,
	[SBranchID] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
alter procedure [dbo].[sp_GetEmployeeDetails]    
(    
@EmployeeID int,    
@SBranchID int    
)    
as    
begin    
   
exec sp_GetEmployeeTypes @SBranchID    
if(@EmployeeID!=0)    
begin    
SELECT  (case when EmployeeType in (-1,-2) then 'TRA' else 'EMP' end)+RIGHT(REPLICATE('0',6)+CAST(EmployeeID AS VARCHAR(6)),6) as EmployeeSID,EmployeeID    
      ,[EmployeeName]      ,[DOB]      ,[Gender]      ,[Nationality]      ,[ReligionID]  ,[CategoryID]    ,[DOJ]      ,[EmailID]    
      ,[BloodGroup]      ,[MaritalStatus]      ,[MotherTongueID]      ,[MobileNumber]      ,[LandLineNumber]      ,[PassportNo]    
      ,[FatherHubName]      ,[Padd_HouseNo]      ,[Padd_Street]      ,[Padd_Area]      ,[Padd_Sector]      ,[Padd_PinCode]    
      ,[Padd_District]      ,[Padd_State]      ,[Padd_Country]      ,[Cadd_HouseNo]      ,[Cadd_Street]      ,[Cadd_Area]    
      ,[Cadd_Sector]      ,[Cadd_PinCode]      ,[Cadd_DistrictCode]      ,[Cadd_StateCode]      ,[Cadd_CountryCode]      ,[DepartmentID]    
      ,[Role]      ,[AccessCardNo]      ,[VehicleNo]      ,[DriverName]      ,[DriverMobileNo]      ,[DesignationID]      ,[AddressProof]    
      ,[BirthCertificate]      ,[CategoryCertificate]      ,[Photo]      ,[ExperienceCertificate]      ,[RelievingCertificate]    
      ,[PFDeclaration]      ,[PANCardCopy]      ,[BankAccountProof]      ,[MedicalCertificate]      ,[AppointmentLetter]    
      ,[PANnumber]      ,[PFNumber],EmployeeType     ,[BankName]      ,[AccountNumber],AccountName,IFSCCode      ,[VehicleRouteID]      ,[StopID]      ,[Status]    
   ,AadharNumber,LicenceNumber    
  FROM [dbo].[EmployeeMaster] where SBranchID=@SBranchID and EmployeeID=@EmployeeID    
     
 declare @AAreaID int    
 declare @ACityID int    
 declare @AStateID int    
 declare @ACountryID int    
   
 select @ACityID=isnull(Cadd_DistrictCode,0),@AStateID=Cadd_StateCode,@AAreaID=Cadd_Area,@ACountryID=Cadd_CountryCode from EmployeeMaster where EmployeeID=@EmployeeID    
   
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
     
 declare @EmployeeTypeID int    
 select @EmployeeTypeID=EmployeeType from EmployeeMaster where EmployeeID=@EmployeeID    
   
  Select isnull(ESD.EmployeeSalaryID,0) as EmployeeSalaryID,ETSD.SalaryTypeID,ETSD.EmployeeTypeID,    
  STM.[TypeName] as SalaryTypeName  ,STM.IsDeduction  
  ,isnull(ESD.Amount,0) as Amount,isnull(ETSD.Amount,0) as SalaryStructure from    
  [dbo].[EmployeeTypeSalaryDetails] ETSD left outer join    
  [dbo].[EmployeeSalaryDetails] ESD on ESD.SalaryTypeID=ETSD.SalaryTypeID  and EmployeeID=@EmployeeID  
  left outer join [dbo].[SalaryTypeMaster] STM on STM.TypeID=ETSD.SalaryTypeID  
   where  ETSD.EmployeeTypeID=@EmployeeTypeID  and ETSD.SBranchID=@SBranchID  
       
  Select [EmpEduID]      ,[Qualification]      ,[Stream]      ,[InstituteName]      ,[StartDate]    
      ,[EndDate]      ,[PassingYear]      ,[Percentage]      ,[Grade]    
  FROM [dbo].[EmployeeEducationDetails] where EmployeeID=@EmployeeID order by [StartDate] desc    
   
 SELECT [EmpExpID]      ,[InstituteName]      ,[Designation]      ,[FromDate]      ,[EndDate]      ,[SubjectsClasses]    
  FROM [dbo].[EmployeeExperienceDetails] where EmployeeID=@EmployeeID order by FromDate desc    
   
 exec [dbo].[spn_GetEmployeesTransportDetails] @EmployeeID,@SBranchID    
   
 select isnull(ELTM.ELID,0) as ELID, LTM.LeaveTypeID,LTM.[LeaveTypeName],isnull(ELTM.YearlyQuota, LTM.[YearlyQuota]) as YearlyQuota,    
 isnull(ELTM.MonthlyQuota,LTM.[MonthlyQuota] ) as MonthlyQuota    
 from LeaveTypeMaster LTM left outer join [dbo].[EmployeeLeaveTypeMaster] ELTM    
 on ELTM.[LeaveTypeID]=LTM.LeaveTypeID and ELTM.EmployeeID=@EmployeeID    
   
 Select TS.TSID,SubjectID,EducationLevelID,GroupID,    
 (Select GroupName from GroupMaster GM where GM.GroupID=TS.GroupID) as GroupName,    
 (Select SubjectName from SubjectMasterAll SM where SM.SubjectID=TS.SubjectID) as SubjectName,    
 (Select Name from EducationLevelMaster ELM where ELM.ID=TS.EducationLevelID) as EducationLevel    
 from [dbo].[Teacher_Subject] TS where TS.EmployeeID=@EmployeeID    
   
 Select  ID,  Name from EducationLevelMaster where SBranchID =@SBranchID    
 declare @ELID int    
 declare @GID int    
 select @ELID=min(ID) from EducationLevelMaster where SBranchID =@SBranchID    
 select @GID=min(GroupID) from GroupMaster where EducationLevelID=@ELID    
 select GroupID as ID, GroupName as Name from GroupMaster where EducationLevelID = @ELID    
 Select SubjectID as ID, SubjectName as Name from SubjectMaster where GroupID=@GID    
     
  end    
  End 
  GO
  DROP TABLE [dbo].[QuestionBankMaster]
GO

GO

CREATE TABLE [dbo].[QuestionBankMaster](
	[QuestionID] [int] IDENTITY(1,1) NOT NULL,
	[TeacherID] int NULL,
	[ClassID] [int] NULL,
	[GroupID] [int] NULL,
	[SubjectID] [int] NULL,
	[ChapterID] [int] NULL,
	[TopicID] [int] NULL,
	[QuestionType] [int] NULL,
	[Complexity] [int] NULL,
	[QuestionText] [nvarchar](max) NULL,
	[QuestionImage] [nvarchar](100) NULL,
	[Option1] [nvarchar](max) NULL,
	[Option1Image] [nvarchar](100) NULL,
	[Option2] [nvarchar](max) NULL,
	[Option2Image] [nvarchar](100) NULL,
	[Option3] [nvarchar](max) NULL,
	[Option3Image] [nvarchar](100) NULL,
	[Option4] [nvarchar](max) NULL,
	[Option4Image] [nvarchar](100) NULL,
	[Answer] [nvarchar](max) NULL,
	[Explaination] [nvarchar](max) NULL,
	[ExplainationImage] [nvarchar](100) NULL,
	[CreatedDate] datetime NULL,
	[SBranchID] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
Create Procedure dbo.sp_UpdateQuestionBank
(
@QuestionID int,
@TeacherID int,
@ClassID int,
@GroupID int,
@SubjectID int,
@ChapterID int,
@TopicID int,
@QuestionType int,
@Complexity int,
@QuestionText nvarchar(max),
@QuestionImage nvarchar(100),
@Option1 [nvarchar](max),
@Option1Image nvarchar(100),
@Option2 [nvarchar](max),
@Option2Image nvarchar(100),
@Option3 [nvarchar](max),
@Option3Image nvarchar(100),
@Option4 [nvarchar](max),
@Option4Image nvarchar(100),
@Answer [nvarchar](max),
@Explaination [nvarchar](max),
@ExplainationImage nvarchar(100),
@CreatedDate datetime,
@SBranchID int,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	Begin
		Delete from QuestionBankMaster where QuestionID=@QuestionID 
	End
	Else
	Begin
		if(@QuestionID=0)
		begin
			Insert into QuestionBankMaster(TeacherID,ClassID,GroupID,SubjectID,ChapterID,TopicID,QuestionType,Complexity,QuestionText,QuestionImage,Option1,Option1Image,Option2,Option2Image,Option3,Option3Image,Option4,Option4Image,
			Answer,Explaination,ExplainationImage,SBranchID,CreatedDate)
			values(@TeacherID,@ClassID,@GroupID,@SubjectID,@ChapterID,@TopicID,@QuestionType,@Complexity,@QuestionText,@QuestionImage,@Option1,@Option1Image,@Option2,@Option2Image,@Option3,@Option3Image,@Option4,@Option4Image,
			@Answer,@Explaination,@ExplainationImage,@SBranchID,@CreatedDate)
			select @QuestionID=Cast(Scope_Identity() as int)
		end
		else
		begin
			Update QuestionBankMaster set TeacherID=@TeacherID,ClassID=@ClassID,GroupID=@GroupID,SubjectID=@SubjectID,ChapterID=@ChapterID,TopicID=@TopicID
			,QuestionType=@QuestionType,Complexity=@Complexity,QuestionText=@QuestionText,QuestionImage=@QuestionImage,Option1=@Option1,Option2=@Option2,Option3=@Option3,
			Option4=@Option4,Option1Image=@Option1Image,Option2Image=@Option2Image,Option3Image=@Option3Image,Option4Image=@Option4Image,
			Answer=@Answer,Explaination=@Explaination,ExplainationImage=@ExplainationImage where QuestionID=@QuestionID
		end
	End
	select @QuestionID
END
GO


CREATE TABLE [dbo].[OnlineExamMaster](
	[OExamID] [int] IDENTITY(1,1) NOT NULL,
	[OExamStartDate] [datetime] NULL,
	[OExamEndDate] [datetime] NULL,
	[ClassID] [int] NULL,
	[SectionID] [int] NULL,
	[SubjectID] [int] NULL,
	[TeacherID] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[Status] [int] NULL,
	[ExamTitle] [nvarchar](50) NULL,
	[ExamDescription] [nvarchar](max) NULL,
	[ExamPriority] [int] NULL,
	[SessionID] [int] NULL,
	[SBranchID] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[OnlineExamQuestions](
	[OXQID] [int] IDENTITY(1,1) NOT NULL,
	[OXID] [int] NULL,
	[QuestionID] [int] NULL,
	[Marks] [numeric](10, 2) NULL
) ON [PRIMARY]
GO



CREATE procedure dbo.sp_GetTeacherQuestionBank  
(  
@TeacherID int,  
@ClassID int,  
@QuestionsBy int  
)  
AS  
BEGIN  
 declare @Classes table(ClassID int,SubjectID int)  
 Insert into @Classes    
 select ClassID,MondaySubjectID as SubjectID from Time_Table_Master where MondayTeacherID=@TeacherID  
 Union  
 select ClassID,TuesdaySubjectID as SubjectID from Time_Table_Master where TuesdayTeacherID=@TeacherID  
 Union  
 select ClassID,WednesdaySubjectID as SubjectID from Time_Table_Master where WednesdayTeacherID=@TeacherID  
 Union  
 select ClassID,ThursdaySubjectID as SubjectID from Time_Table_Master where ThursdayTeacherID=@TeacherID  
 Union  
 select ClassID,FridaySubjectID as SubjectID from Time_Table_Master where FridayTeacherID=@TeacherID  
 Union  
 select ClassID,SaturdaySubjectID as SubjectID from Time_Table_Master where SaturdayTeacherID=@TeacherID  
  
 if(@ClassID=0)  
 begin  
  Select @ClassID=min(ClassID) from @Classes  
 end  
 if(@QuestionsBy=0)  
 begin  
  Select QuestionID,TeacherID,ClassID,QB.GroupID,QB.SubjectID,ChapterID,TopicID,QuestionType,Complexity,QuestionText,QuestionImage,Option1,Option1Image,Option2,Option2Image,  
  Option3,Option3Image,Option4,Option4Image,Answer,Explaination,ExplainationImage,SBranchID,CreatedDate ,SM.SubjectName  
  From QuestionBankMaster QB left outer join SubjectMaster SM on SM.SubjectID=QB.SubjectID  
  where TeacherID=@TeacherID and ClassID=@ClassID  
 end  
 else  
 begin  
 Select QuestionID,TeacherID,EM.EmployeeName as TeacherName,ClassID,QB.GroupID,QB.SubjectID,ChapterID,TopicID,QuestionType,Complexity,
  QuestionText,QuestionImage,Option1,Option1Image,Option2,Option2Image,  
  Option3,Option3Image,Option4,Option4Image,Answer,Explaination,ExplainationImage,QB.SBranchID,QB.CreatedDate ,SM.SubjectName  
  From QuestionBankMaster QB left outer join SubjectMaster SM on SM.SubjectID=QB.SubjectID  
  left outer join EmployeeMaster EM on EM.EmployeeID=QB.TeacherID
  where ClassID=@ClassID   
 end  
  
 select ClassID as ID,ClassName as Name from ClassMaster where ClassID in (select ClassID from @Classes) and Status=1  
 select @ClassID  
  
 select distinct SM.SubjectID as ID,SubjectName as Name,ClassID as Extra1 from @Classes C left outer join SubjectMaster SM on SM.SubjectID=C.SubjectID  
 where SM.SubjectID is not null  
END

GO
Alter View [dbo].[v_EmployeeSalaryAmount]  
as   
  Select EM.EmployeeID,EM.EmployeeName,EM.Photo,Em.Gender,Em.EmployeeSID,EM.EmployeeType,EM.SBranchID, EM.DOJ,
  Sum(isnull(ESD.Amount,ETSD.Amount)) Amount  
  from [v_EmployeeDriversBasicDetails] EM  
  left outer join [EmployeeTypeSalaryDetails] ETSD ON EM.EmployeeType=ETSD.EmployeeTypeID  
  left outer join [EmployeeSalaryDetails] ESD on ESD.EmployeeID=EM.EmployeeID and ESD.EmployeeTypeID=EM.EmployeeType  
  and ESD.SalaryTypeID=ETSD.SalaryTypeID  
  group by EM.EmployeeID,EM.EmployeeName,EM.Photo,Em.Gender,Em.EmployeeSID,EM.EmployeeType,EM.SBranchID  ,EM.DOJ
GO
ALTER proc [dbo].[sp_GetClassTeacherClassSections]
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
						set @SQL='select @cnt=Count(*) from [dbo].[StudentAttendanceMasterT] where ClassID='+@ClassID+' and SectionID='+@SectionID+' and isnull(D'+@Day+',3)=3 and Month='+@Month+' and FYear='+@Year	
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
Create procedure dbo.sp_GetTeacherOnlineExams
(
@TeacherID int ,
@ClassID int,  
@SectionID int,
@SubjectID int,
@SessionID int,
@SBranchID int
)
AS
BEGIN
	if(@SessionID=0)
	begin
		select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
	end
	declare @Classes table(ClassID int,SubjectID int,SectionID int)  
	 Insert into @Classes    
	 select ClassID,MondaySubjectID as SubjectID,SectionID from Time_Table_Master where MondayTeacherID=@TeacherID  
	 Union  
	 select ClassID,TuesdaySubjectID as SubjectID,SectionID from Time_Table_Master where TuesdayTeacherID=@TeacherID  
	 Union  
	 select ClassID,WednesdaySubjectID as SubjectID,SectionID from Time_Table_Master where WednesdayTeacherID=@TeacherID  
	 Union  
	 select ClassID,ThursdaySubjectID as SubjectID,SectionID from Time_Table_Master where ThursdayTeacherID=@TeacherID  
	 Union  
	 select ClassID,FridaySubjectID as SubjectID,SectionID from Time_Table_Master where FridayTeacherID=@TeacherID  
	 Union  
	 select ClassID,SaturdaySubjectID as SubjectID,SectionID from Time_Table_Master where SaturdayTeacherID=@TeacherID  
  
	if(@ClassID=0)  
	 begin  
	  Select @ClassID=min(ClassID) from @Classes  
	  select @SectionID=min(SectionID) from @Classes
	  select @SubjectID=min(SubjectID) from @Classes
	 end 

	 Select OX.*,
	 (Select count(*) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as QuestionCount,
	 (Select sum(Marks) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as TotalMarks,
	 (Select count(*) from [dbo].OnlineExamSubmissionMaster OQ where OQ.OExamID=OX.OExamID) as SubmissionCount
	 from OnlineExamMaster OX
	 where TeacherID=@TeacherID and SubjectID=@SubjectID and SectionID=@SectionID and SessionID=@SessionID
	 
	 select ClassID as ID,ClassName as Name from ClassMaster where ClassID in (select ClassID from @Classes) and Status=1  
	 select isnull(@ClassID,0) 
  
	 select distinct SM.SubjectID as ID,SubjectName as Name,ClassID as Extra1,SectionID as Extra2 from @Classes C left outer join SubjectMaster SM on SM.SubjectID=C.SubjectID  
	 where SM.SubjectID is not null  
	 select isnull(@SubjectID,0)

	 select ID,Name,ClassID as Extra1 from Class_Sections CS where ID in (Select SectionID from @Classes)
	 select isnull(@SectionID,0)

	 Select SessionID as ID,SessionName as Name, SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID
	 select isnull(@SessionID,0)

END

GO


Create procedure dbo.sp_GetTeacherOnlineExamsDetails
(
@TeacherID int ,
@ClassID int,  
@SectionID int,
@SubjectID int,
@SessionID int,
@SBranchID int,
@OExamID int
)
AS
BEGIN
	Declare @ExamTable Table(OExamID int, OExamStartDate datetime,OExamEndDate datetime,ClassID int,SectionID int,SubjectID int,TeacherID int,CreatedDate datetime,Status int,
	ExamTitle nvarchar(max),ExamDescription nvarchar(max),ExamPriority int,SessionID int,SBranchID int)
	
	if(@OExamID!=0)
	begin
		insert into @ExamTable
		Select * from OnlineExamMaster OX where OExamID=@OExamID
	end
	else
	Begin
		insert into @ExamTable(ClassID,SectionID,SubjectID,TeacherID,SessionID) values(@ClassID,@SectionID,@SubjectID,@TeacherID,@SessionID)
	End
	
	select EX.* ,EM.EmployeeName as TeacherName,CM.ClassName,CS.Name as SectionName,
	SS.SessionName ,SM.SubjectName
	from @ExamTable EX left outer join EmployeeMaster EM on EM.EmployeeID=EX.TeacherID
	left outer join ClassMaster CM on CM.ClassID=EX.ClassID
	left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID
	left outer join Class_Sections CS on CS.ID=EX.SectionID
	left outer join SessionMaster SS on SS.SessionID=EX.SessionID
	 
	 select OQ.OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4,Answer,Explaination ,EM.EmployeeName as TeacherName
	 from [dbo].[OnlineExamQuestions] OQ
	 left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID
	left outer join EmployeeMaster EM on EM.EmployeeID=QB.TeacherID
	 where OQ.OXID=@OExamID

	 Select QuestionID,TeacherID,EM.EmployeeName as TeacherName,ClassID,QB.GroupID,QB.SubjectID,ChapterID,TopicID,QuestionType,Complexity,
	QuestionText,QuestionImage,Option1,Option1Image,Option2,Option2Image,  
	Option3,Option3Image,Option4,Option4Image,Answer,Explaination,ExplainationImage,QB.SBranchID,QB.CreatedDate 
	From QuestionBankMaster QB 
	left outer join EmployeeMaster EM on EM.EmployeeID=QB.TeacherID
	where ClassID=@ClassID   and SubjectID=@SubjectID
END

GO
GO

Create procedure dbo.sp_UpdateOnlineExam
(
@OExamID int,
@OExamStartDate datetime,
@OExamEndDate datetime,
@SessionID int,
@ClassID int,
@SectionID int,
@SubjectID int,
@TeacherID int,
@CreatedDate datetime,
@Status int,
@ExamTitle nvarchar(50),
@ExamDescription nvarchar(max),
@ExamPriority int,
@SBranchID int,
@Questions ut_Name_ID_Utility READONLY,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from OnlineExamMaster where OExamID=@OExamID
		Delete from OnlineExamQuestions where OXID=@OExamID
	end
	Else
	Begin
		if(@OExamID=0)
		begin
			Insert into dbo.OnlineExamMaster(OExamStartDate,OExamEndDate,SessionID,ClassID,SectionID,SubjectID,TeacherID,CreatedDate,Status,ExamTitle,ExamDescription,ExamPriority,SBranchID)
			values(@OExamStartDate,@OExamEndDate,@SessionID,@ClassID,@SectionID,@SubjectID,@TeacherID,@CreatedDate,@Status,@ExamTitle,@ExamDescription,@ExamPriority,@SBranchID)
			select @OExamID=Cast(Scope_Identity() as int)
		end
		else
		begin
			Update dbo.OnlineExamMaster set OExamStartDate=@OExamStartDate,OExamEndDate=@OExamEndDate,SessionID=@SessionID,ClassID=@ClassID,SectionID=@SectionID
			,SubjectID=@SubjectID,TeacherID=@TeacherID,Status=@Status,ExamTitle=@ExamTitle,ExamDescription=@ExamDescription,ExamPriority=@ExamPriority
			where OExamID=@OExamID
		end

		Delete from OnlineExamQuestions where OXID=@OExamID and OXQID in (select ID from @Questions where Extra3=-1)
		insert into OnlineExamQuestions(OXID,QuestionID,Marks) select @OExamID,Name,Extra1 from @Questions where Extra3!=-1 and ID=0
	End
END
GO

CREATE TABLE [dbo].[OnlineExamSubmissionMaster](
	[SubmissionID] [int] IDENTITY(1,1) NOT NULL,
	[OExamID] [int] NULL,
	[SubmissionDate] [datetime] NULL,
	[StartDate] [datetime] NULL,
	[StudentID] [int] NULL,
	[TeacherRemark] [nvarchar](max) NULL,
	[TeacherID] [int] NULL,
	[Status] [int] NULL,
	[CheckDate] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[OnlineExamSubmissionAnswers](
	[SubAnsID] [int] IDENTITY(1,1) NOT NULL,
	[SubmissionID] [int] NULL,
	[OXQID] [int] NULL,
	[Answer] [nvarchar](max) NULL,
	[MarksGiven] [numeric](10,2) NULL,
	[Remark] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
  
CREATE procedure dbo.sp_GetStudentOnlineExams  
(  
@StudentID int,  
@SubjectID int  
)  
AS  
BEGIN  
 Declare @ClassID int,@SectionID int,@GroupID int,@SessionID int,@StudentSessionUID int  
 Select @ClassID=ClassID,@SectionID=SectionID,@SessionID=SessionID,@StudentSessionUID=StudentSessionUID from Student_Session where StudentID=@StudentID and Status=1  
 select @GroupID=GroupID from Class_Sections where ID=@SectionID  
  
 Declare @Subjects Table(SubjectID int,SubjectName nvarchar(50))  
 insert into @Subjects select SubjectID,SubjectName from SubjectMaster where GroupID=@GroupID   
 and (IsOptionalSubject=0 or SubjectID in (select OpSubjectID from [Student_Session_OptionalSubjects] where StudentSessionID=@StudentSessionUID))  
  
 if(@SubjectID=0)  
 begin  
  Select top 1 @SubjectID=SubjectID from @Subjects  
 end  
  
  Select OX.*,  
  (Select count(*) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as QuestionCount,  
  (Select sum(Marks) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as TotalMarks,  
  OXM.SubmissionID,EM.EmployeeName as TeacherName,  
  (Select sum(MarksGiven) from OnlineExamSubmissionAnswers OXS where OXS.SubmissionID=OXM.SubmissionID) as MarksObtained  
  from OnlineExamMaster OX left outer join OnlineExamSubmissionMaster OXM on OXM.OExamID=OX.OExamID   and OXM.StudentID=@StudentID
  left outer join EmployeeMaster EM on EM.EmployeeID=OX.TeacherID  
  where SubjectID=@SubjectID and SectionID=@SectionID and SessionID=@SessionID and OX.Status=1  
    
  select SubjectID as ID,SubjectName as Name from @Subjects  
  select isnull(@SubjectID,0)  
END
GO

Create procedure dbo.sp_GetStudentOnlineExamsDetails
(
@OExamID int
)
AS
BEGIN
	select EX.* ,EM.EmployeeName as TeacherName,CM.ClassName,CS.Name as SectionName,
	SS.SessionName ,SM.SubjectName
	from OnlineExamMaster EX left outer join EmployeeMaster EM on EM.EmployeeID=EX.TeacherID
	left outer join ClassMaster CM on CM.ClassID=EX.ClassID
	left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID
	left outer join Class_Sections CS on CS.ID=EX.SectionID
	left outer join SessionMaster SS on SS.SessionID=EX.SessionID
	where OExamID=@OExamID
	 
	 select OQ.OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4
	 from [dbo].[OnlineExamQuestions] OQ
	 left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID
	left outer join EmployeeMaster EM on EM.EmployeeID=QB.TeacherID
	 where OQ.OXID=@OExamID

END
GO

Create procedure dbo.sp_StudentSubmitOnlineAnswerSheet
(
@OExamID int,
@SubmissionDate datetime,
@StudentID int,
@Answers ut_Name_ID_Utility READONLY
)
AS
BEGIN
	
	declare @SubmissionID int
	select @SubmissionID=SubmissionID from OnlineExamSubmissionMaster where StudentID=@StudentID and OExamID=@OExamID
	if(isnull(@SubmissionID,0)=0)
	begin	
		insert into OnlineExamSubmissionMaster(OExamID,SubmissionDate,StudentID,Status)values(@OExamID,@SubmissionDate,@StudentID,0)
		select @SubmissionID=cast(Scope_Identity() as int)
		insert into OnlineExamSubmissionAnswers(SubmissionID,OXQID,Answer) select @SubmissionID,ID,Name from  @Answers	
	end
	select @SubmissionID
END


GO
Create procedure dbo.sp_GetStudentOnlineExamsAnswerSheet
(
@SubmissionID int,
@OExamID int,
@StudentID int
)
AS
BEGIN
	
	select EX.* ,EM.EmployeeName as TeacherName,CM.ClassName,CS.Name as SectionName,
	SS.SessionName ,SM.SubjectName,OXM.SubmissionID,OXM.SubmissionDate,OXM.TeacherRemark,OXM.CheckDate,OXM.Status as SubmissionStatus
	from OnlineExamMaster EX left outer join EmployeeMaster EM on EM.EmployeeID=EX.TeacherID
	left outer join ClassMaster CM on CM.ClassID=EX.ClassID
	left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID
	left outer join Class_Sections CS on CS.ID=EX.SectionID
	left outer join SessionMaster SS on SS.SessionID=EX.SessionID
	left outer join OnlineExamSubmissionMaster OXM on OXM.SubmissionID=@SubmissionID
	where EX.OExamID=@OExamID
	 
	 select OQ.OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4,
	 OXA.Answer as StudentAnswer,OXA.MarksGiven,OXA.Remark as TeacherRemark,QB.Answer,QB.Explaination,QB.Complexity
	 from [dbo].[OnlineExamQuestions] OQ
	 left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID
	 left outer join OnlineExamSubmissionAnswers OXA on OXA.OXQID=OQ.OXQID
	 where OQ.OXID=@OExamID

END
GO
Create  procedure dbo.sp_GetTeacherOnlineExamSubmissions
(  
@OExamID int
)  
AS  
BEGIN  
	
	select EX.OExamID,EX.OExamStartDate,EX.OExamEndDate,EX.ExamTitle,EX.ExamDescription,EX.ExamPriority,OXM.Status
	,EM.Name +'('+'STUD'+RIGHT(REPLICATE('0',6)+CAST(EM.StudentID AS VARCHAR(6)),6)+')' as TeacherName,CM.ClassName,CS.Name as SectionName,
	SS.SessionName ,SM.SubjectName,OXM.SubmissionID,OXM.SubmissionDate,OXM.TeacherRemark,OXM.CheckDate,OXM.Status as SubmissionStatus
	from OnlineExamMaster EX 
	left outer join ClassMaster CM on CM.ClassID=EX.ClassID
	left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID
	left outer join Class_Sections CS on CS.ID=EX.SectionID
	left outer join SessionMaster SS on SS.SessionID=EX.SessionID
	left outer join OnlineExamSubmissionMaster OXM on OXM.OExamID=EX.OExamID
	left outer join StudentMaster EM on EM.StudentID=OXM.StudentID
	where OXM.SubmissionID=@SubmissionID
	 
	 select OQ.OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4,
	 OXA.Answer as StudentAnswer,OXA.MarksGiven,OXA.Remark as TeacherRemark,QB.Answer,QB.Explaination,QB.Complexity
	 from [dbo].[OnlineExamQuestions] OQ
	 left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID
	 left outer join OnlineExamSubmissionAnswers OXA on OXA.OXQID=OQ.OXQID and OXA.SubmissionID=@SubmissionID
	 where OXA.SubmissionID=@SubmissionID
END

GO

Create procedure dbo.sp_GetTeacherStudentOnlineExamsAnswerSheet
(
@SubmissionID int
)
AS
BEGIN
	
	select EX.OExamID,EX.OExamStartDate,EX.OExamEndDate,EX.ExamTitle,EX.ExamDescription,EX.ExamPriority,OXM.Status
	,EM.Name +'('+'STUD'+RIGHT(REPLICATE('0',6)+CAST(EM.StudentID AS VARCHAR(6)),6)+')' as TeacherName,CM.ClassName,CS.Name as SectionName,
	SS.SessionName ,SM.SubjectName,OXM.SubmissionID,OXM.SubmissionDate,OXM.TeacherRemark,OXM.CheckDate,OXM.Status as SubmissionStatus
	from OnlineExamMaster EX 
	left outer join ClassMaster CM on CM.ClassID=EX.ClassID
	left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID
	left outer join Class_Sections CS on CS.ID=EX.SectionID
	left outer join SessionMaster SS on SS.SessionID=EX.SessionID
	left outer join OnlineExamSubmissionMaster OXM on OXM.OExamID=EX.OExamID
	left outer join StudentMaster EM on EM.StudentID=OXM.StudentID
	where OXM.SubmissionID=@SubmissionID
	 
	 select OXA.SubAnsID as OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4,
	 OXA.Answer as StudentAnswer,OXA.MarksGiven,OXA.Remark as TeacherRemark,QB.Answer,QB.Explaination,QB.Complexity
	 from [dbo].[OnlineExamQuestions] OQ
	 left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID
	 left outer join OnlineExamSubmissionAnswers OXA on OXA.OXQID=OQ.OXQID and OXA.SubmissionID=@SubmissionID
	 where OXA.SubmissionID=@SubmissionID

END
GO
Create procedure dbo.sp_UpdateRemarkOnStudentOnlineExamSubmission
(
@SubmissionID int,
@CheckDate datetime,
@TeacherID int,
@Status int,
@TeacherRemark nvarchar(max),
@AnswerRemarks ut_Name_ID_Utility READONLY
)
AS
BEGIN
		update OnlineExamSubmissionMaster set TeacherRemark=@TeacherRemark,TeacherID=@TeacherID, CheckDate=@CheckDate,Status=@Status where SubmissionID=@SubmissionID

		UPDATE e SET e.MarksGiven=d.Name,e.Remark=d.Extra1
		FROM  OnlineExamSubmissionAnswers e, @AnswerRemarks d 
		WHERE e.SubAnsID=d.ID
END
GO

Alter procedure [dbo].[sp_GetStudentLeaves]  
(  
@SBranchID int,  
@ClassID int,  
@SectionID int  
)--select * from LeaveMaster  
as   
begin   
 if(@ClassID=0)  
 begin  
  Select LeaveID,LeaveType,StartDate,EndDate,LeaveReason,  
  'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) as [StudentSID],  
  SS.RollNo,  
  (Select ClassName +' \ '+SectionName from [dbo].[v_ClassSectionNames] where SectionID=SS.SectionID) as ClassSection,  
  ApplicantID as StudentID, SM.Name as StudentName,'~/Images/StudentImage/Thumb/'+isnull(SM.Photo,'') as Photo,SM.Gender,IsApproved  
  from LeaveMaster LM Left outer join Student_Session SS on SS.StudentSessionUID=LM.ApplicantID  
  left outer join  StudentMaster SM on SS.StudentID=SM.StudentID   
  where ApplicantType=1 and LM.SBranchID=@SBranchID --and SS.SessionID=(Select SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID)  
  order by StartDate desc  
 end  
 else  
 begin  
  Select LeaveID,LeaveType,StartDate,EndDate,LeaveReason,  
  'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) as [StudentSID],  
  SS.RollNo,  
  (Select ClassName +' \ '+SectionName from [dbo].[v_ClassSectionNames] where SectionID=SS.SectionID) as ClassSection,  
  ApplicantID as StudentID, SM.Name as StudentName,'~/Images/StudentImage/Thumb/'+isnull(SM.Photo,'') as Photo,SM.Gender,IsApproved  
  from LeaveMaster LM Left outer join Student_Session SS on SS.StudentSessionUID=LM.ApplicantID  
  left outer join StudentMaster SM on SS.StudentID=SM.StudentID   
  where ApplicantType=1 and LM.SBranchID=@SBranchID and SS.SectionID=@SectionID --and SS.SessionID=(Select SessionID from SessionMaster where SessionStatus=1 and SBranchID=@SBranchID)  
  order by StartDate desc  
 end  
  select ClassID as ID, ClassName as Name from ClassMaster where SBranchID=@SBranchID  
    
  Select ID,Name from Class_Sections where ClassID=@ClassID  
end  
  
GO
Alter procedure [dbo].[sp_GetEmployeeSalaryDetailsNew]
(    
@EmployeeID int=4,    
@SalaryMonth int=6,     
@SalaryYear int=2020,    
@SBranchID int=1,    
@EmployeeType int =3    
)    
AS    
BEGIN    
     
 Select BranchSchoolName,Address from SBranchMaster where SBranchID=@SBranchID    
    
 select EmployeeID,EmployeeSID,EmployeeType,[EmployeeName],    
 (Select EmployeeTypeName from EmployeeTypeMaster ETM where ETM.EmployeeTypeID=EM.EmployeeType) as EmployeeTypeName    
 ,DOJ from [dbo].[v_EmployeeDriversBasicDetails] EM where EmployeeID=@EmployeeID and EmployeeType=@EmployeeType    
    
 Declare @MonthType int,@BaseDate date,@SessionStartDate date,@IsAdmissionFee int=0,@DOJ date,@SalaryMonthDate date    
    
 Declare @HStartDate date,@HEndDate date,@HDuration int,@Month int,@DaysWorking int=6    
 declare @SalarySummery Table(TypeID int,TypeName nvarchar(50),IsDeduction int,AttendanceType int,MinDays int,Amount numeric(10,2))    
 declare @HolidayTable Table(DayID int,HStatus numeric(5,2),HDuration int)    
 declare @LeaveSummery Table(DayID int,LStatus numeric(10,2),IsSalaryDeduct int,LDuration int)    
    
 set @SessionStartDate=Cast(@SalaryYear as nvarchar(10))+'-1-1'    
 set @SalaryMonthDate=Cast(@SalaryYear as nvarchar(10))+'-'+Cast(@SalaryMonth as nvarchar(2))+'-1'    
 select @DOJ=DOJ from EmployeeMaster where EmployeeID=@EmployeeID    
 set @BaseDate=@SessionStartDate    
 if(@DOJ>@SessionStartDate)    
 begin    
  set @BaseDate=@DOJ    
 end    
    
 select @MonthType=(Case when datepart(month,@BaseDate)=@SalaryMonth and (datepart(month,dateadd(month,3,@SessionStartDate))=@SalaryMonth  or    
 datepart(month,dateadd(month,9,@SessionStartDate))=@SalaryMonth) then 5 else    
 (Case when datepart(month,@BaseDate)=@SalaryMonth and (datepart(month,dateadd(month,6,@SessionStartDate))=@SalaryMonth) then 6 else    
 (case when datepart(month,@BaseDate)=@SalaryMonth then (case when @IsAdmissionFee=0 then 1 else 0 end)      
 else (Case when datepart(month,dateadd(month,3,@SessionStartDate))=@SalaryMonth  or datepart(month,dateadd(month,9,@SessionStartDate))=@SalaryMonth then 3    
 else (case when  datepart(month,dateadd(month,6,@SessionStartDate))=@SalaryMonth then 4 else 2 end) end) end)end)end)    
    
 Declare @MTFT table (MonthTypeID int,FeeTypeID int)    
 insert into @MTFT    
 select MonthTypeID,FeeTypeID from [MonthTypeFeeType]    
    
    
 insert into @SalarySummery    
 select distinct TypeID,TypeName,IsDeduction,AttendanceType,MinDays,isnull(ESD.Amount,ETSD.Amount) as Amount from SalaryTypeMaster STM    
 left outer join [dbo].[EmployeeSalaryDetails] ESD on ESD.SalaryTypeID=STM.TypeID and ESD.EmployeeID=@EmployeeID    
 left outer join [dbo].[EmployeeTypeSalaryDetails] ETSD on ETSD.SalaryTypeID=STM.TypeID and ETSD.EmployeeTypeID=@EmployeeType    
 where STM.SBranchID=@SBranchID    
 and STM.TypeApplicable in (select FeeTypeID from @MTFT where MonthTypeID=@MonthType)    
 and (STM.TypeApplicable<>9 or (select count(*) from [dbo].[SplitStringToTable](STM.Months,',') where Item=@SalaryMonth)>0)    
    
 select * from @SalarySummery    
    
 Insert into @LeaveSummery    
 select datepart(day,LD.LeaveDate),Applied,isnull(IsSalaryDeduct,1),LeaveType    
 from LeaveDetails LD left outer join LeaveTypeMaster LTM on LTM.LeaveTypeID=LD.LeaveTypeID    
 Left outer join LeaveMaster LM  on LM.LeaveID=LD.LeaveID    
 where LM.ApplicantType=0 and LM.ApplicantID=@EmployeeID and LM.IsApproved=1    
 and LD.LeaveDate between @SalaryMonthDate and EOMONTH(@SalaryMonthDate) --and isnull(IsSalaryDeduct,1)=0    
    
 select * from @LeaveSummery    
    
 DECLARE @AttandanceTable TABLE (D1 numeric(5,2),D2 numeric(5,2),D3 numeric(5,2),D4 numeric(5,2),D5 numeric(5,2),D6 numeric(5,2),D7 numeric(5,2),D8 numeric(5,2),D9 numeric(5,2),D10 numeric(5,2),D11 numeric(5,2),D12 numeric(5,2),D13 numeric(5,2),D14 numeric(5,2),D15 numeric(5,2),    
 D16 numeric(5,2), D17 numeric(5,2),D18 numeric(5,2), D19 numeric(5,2),D20 numeric(5,2),D21 numeric(5,2),D22 numeric(5,2), D23 numeric(5,2), D24 numeric(5,2), D25 numeric(5,2), D26 numeric(5,2),D27 numeric(5,2),D28 numeric(5,2), D29 numeric(5,2), D30 numeric(5,2), D31 numeric(5,2))    
    
 DECLARE @AttandanceDateTable TABLE (ADay int,Status int)    
    
 Insert into @AttandanceTable select  D1 ,D2 ,D3 ,D4 ,D5 ,D6 ,D7 ,D8 ,D9 ,D10 ,D11 ,D12 ,D13 ,D14 ,D15 ,    
 D16 , D17 ,D18 , D19 ,D20 ,D21 ,D22 , D23 , D24 , D25 , D26 ,D27 ,D28 , D29 , D30 , D31    
 from EmployeeAttendanceMaster where EmployeeID=@EmployeeID and FYear=@SalaryYear and [Month]=@SalaryMonth    
    
 insert into @AttandanceDateTable    
 Select t.DayID,isnull([Status],0) as Status from    
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
    
 DECLARE db_Holidaycursor CURSOR FOR      
 select StartDate,EndDate,DayDuration    
 from HolidayMaster where [Status]=1 and SBranchID=@SBranchID and IsEmployee=1 and    
 ((Select count(*) from dbo.SplitStringToTable(isnull(AssociatedIDs,'0'),',') where Item=@EmployeeType or Item=0)>0)    
 and ((datepart(month,StartDate)=@SalaryMonth and datepart(year,StartDate)=@SalaryYear)    
 or (datepart(month,EndDate)=@SalaryMonth and datepart(year,EndDate)=@SalaryYear))    
 OPEN db_Holidaycursor      
 FETCH NEXT FROM db_Holidaycursor INTO @HStartDate,@HEndDate,@HDuration    
 WHILE @@FETCH_STATUS = 0      
  BEGIN      
   while(@HStartDate<=@HEndDate)    
   begin    
    if(datepart(month,@HStartDate)=@SalaryMonth)    
    begin    
     declare @tStatus int,@tHDuration int    
     select @tStatus=HStatus,@tHDuration=HDuration from @HolidayTable where DayID=datepart(day,@HStartDate)    
     if(@tStatus is null)    
     begin    
      if(@HDuration=1)    
      begin    
       Insert into @HolidayTable(DayID,HStatus,HDuration) values (datepart(day,@HStartDate),1,@HDuration)    
      end    
      else    
      begin    
       Insert into @HolidayTable(DayID,HStatus,HDuration) values (datepart(day,@HStartDate),0.5,@HDuration)    
      end    
     end    
     else    
     begin    
      if((@tHDuration=2 and @HDuration=3) or (@tHDuration=3 and @HDuration=2) or @HDuration=1)    
      begin    
       Update @HolidayTable set HStatus=1,HDuration=1 where DayID=datepart(day,@HStartDate)    
      end    
     end    
    end    
    Set @HStartDate= DATEADD(day,1,@HStartDate)    
   end    
   FETCH NEXT FROM db_Holidaycursor INTO  @HStartDate,@HEndDate,@HDuration    
  END      
  CLOSE db_Holidaycursor      
  DEALLOCATE db_Holidaycursor    
  select * from @HolidayTable    
    
 Declare @FinalAttandanceStatusTable TABLE(ADate date,AStatus numeric(10,2),LStatus numeric(10,2),HStatus numeric(10,2),LType int,HType int)    
 --select * from @AttandanceDateTable    
 declare @FLStatus int,@FHStatus int,@FLType int,@FHType int,@PLStatus int    
 declare @EndDate date=EOMOnth(@SalaryMonthDate)    
 declare @Status int    
    
 while(@SalaryMonthDate<=@EndDate)    
 begin    
  set @FLType=0    
  set @FHStatus=0    
  set @FLType=0    
  set @FHType=0    
  select @Status=Status from @AttandanceDateTable where ADay=datepart(day,@SalaryMonthDate)    
  if(@Status not in (1,0.5))    
  begin    
   if((case Datepart(dw,@SalaryMonthDate) when 1 then 8 else Datepart(dw,@SalaryMonthDate) end)>@DaysWorking+1)    
   begin    
    Insert into @FinalAttandanceStatusTable(ADate,AStatus,LStatus,HStatus,LType,HType)    
    values(@SalaryMonthDate,2,0,0,0,0)    
   end    
   else    
   begin    
    select @FHStatus=HStatus,@FHType=HDuration from @HolidayTable where DayID=datepart(day,@SalaryMonthDate)    
    select @FLStatus=LStatus,@FLType=IsSalaryDeduct from @LeaveSummery where DayID=datepart(day,@SalaryMonthDate)    
    Insert into @FinalAttandanceStatusTable(ADate,AStatus,LStatus,HStatus,LType,HType)    
    values(@SalaryMonthDate,0,@FLStatus,@FHStatus,@FLType,@FHType)   
   end    
  end    
  else    
  begin    
   select @FHStatus=HStatus,@FHType=HDuration from @HolidayTable where DayID=datepart(day,@SalaryMonthDate)    
   select @FLStatus=LStatus,@FLType=IsSalaryDeduct from @LeaveSummery where DayID=datepart(day,@SalaryMonthDate)    
   Insert into @FinalAttandanceStatusTable(ADate,AStatus,LStatus,HStatus,LType,HType)    
   values(@SalaryMonthDate,isnull(@Status,0),@FLStatus,@FHStatus,@FLType,@FHType)    
  end    
  set @SalaryMonthDate=dateadd(day,1,@SalaryMonthDate)    
 end    
    
 declare @WorkingDays numeric(10,2),@MonthDays numeric(10,2),@PresentDays numeric(10,2),@WeekOff numeric(10,2),    
 @PubHoliday numeric(10,2),@Leaves numeric(10,2),@LWP numeric(10,2)    
    
 select @WorkingDays=sum(Case when AStatus!=2 then 1 else 0 end),    
 @MonthDays=Count(*),    
 @WeekOff=sum(Case when AStatus=2 then 1 else 0 end),    
 @PubHoliday=sum(HStatus),@LWP=sum(Case when LType=1 then LStatus else 0 end),    
 @Leaves=sum(Case when LType=0 then LStatus else 0 end),    
 @PresentDays=sum(Case when AStatus in (1,0.5) then AStatus else 0 end)    
 from @FinalAttandanceStatusTable    
    
 select @MonthDays as MonthDays,@WorkingDays-@PubHoliday as WorkingDays,@WeekOff as WeekOffs,@PubHoliday as PubHolidays,    
 @Leaves as Leaves,@LWP as LWP,@PresentDays as Present    
    
  Select RefID as TypeID, Title+' #'+cast(DeductionsDone+1 as nvarchar(3))+'/'+cast(Months as nvarchar(3)) as TypeName, 5 as FeeTypeApplicable,    
 (case when Amount-DeductedAmount<EMI then Amount-DeductedAmount else EMI end)*-1 as Amount from (    
  select APM.Title, APM.Amount,EMI,APM.Months,isnull(Sum(APD.Amount),0) as DeductedAmount,count(distinct(datepart(month,APD.Date))) as DeductionsDone    
 ,APM.AdPaymentID as RefID from     
  [dbo].[AdvancePaymentMaster]  APM left outer join [dbo].[AdvancePaymentDeductions] APD on APM.AdPaymentID=APD.AdPayID    
  and datepart(month, APD.Date)+1+12*datepart(Year, APD.Date)<=@SalaryMonth+12*@SalaryYear     
  Where EmployeeID=@EmployeeID and EmployeeType=@EmployeeType and APM.PaymentDate<=@SalaryMonthDate    
  group by APM.Title,APM.Amount,EMI,APM.Months,APM.AdPaymentID    
  having isnull(Sum(APD.Amount),0)<APM.Amount)t     

END    
GO
Alter Procedure [dbo].[sp_GetClassGroupWiseStudentFeeSummery] -- 2,6,3,1,'2020-07-16','2020-07-16'
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
  
  select S.*,isnull(Q.QuotaName,'No') as QuotaName ,PM.MotherName,PM.FatherName,  
  (select top 1 IsCustomFee from Student_Session SS where SS.StudentID=S.StudentID and SS.SessionID=@SessionID) as IsCustomFee  
  from @Students S left outer join QuotaMaster Q on Q.QuotaID=S.QuotaID  
  left outer join StudentMAster SM on SM.StudentID=S.StudentID  
  left outer join ParentMaster PM on PM.ParentID=SM.ParentID  
    
  select SessionID,SessionName,SessionStatus from SessionMaster where SBranchID=@SBranchID  
  select isnull(@SessionID,0)  
END
GO

Alter proc [dbo].[sp_GetClassStudentsDayAttandanceStatus]--5,2020,24,2,6,1,4  
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
  
 Declare @HolidayCnt int  
 select @HolidayCnt=count(*) from HolidayMaster where @AttDate between StartDate and EndDate and   
 ((select count(*) from dbo.SplitStringToTable(Classes,',') where ITem=@ClassID or Item=0)>0) and IsStudents=1  and SBranchID=@SBranchID 
   
 declare @StudentTable as Table(StudentID int,Name nvarchar(500),Photo nvarchar(500),RollNo nvarchar(50),Gender int,StudentSID nvarchar(50),  
 Status numeric(5,2),IsExist int)  
   
 declare @WD int=isnull(nullif(DATEPART(dw,@AttDate)-1,0),7)  
   
 declare @SQLQuery nvarchar(max)  
 set @SQLQuery='Select SM.[StudentID],SM.[Name],SM.Photo,SS.RollNo,SM.Gender,  
 ''STUD''+RIGHT(REPLICATE(''0'',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) as [StudentSID],  
 isnull(EAM.D'+@Day+',3) as Status ,(case when EAM.D'+@Day+' is null then 0 else 1 end) as IsExist  
 from StudentMaster SM left outer join [dbo].[StudentAttendanceMasterT] EAM on EAM.StudentID=SM.StudentID  
 and EAM.[FYear]='+@Year+' and EAM.[Month]='+@Month+ ' left outer join Student_Session SS on SS.StudentID=SM.StudentID  
 where SM.SBranchID='+cast(@SBranchID as nvarchar(3))+' and SS.ClassID='+cast(@ClassID as nvarchar(3))+' and SS.SectionID='+cast(@SectionID as nvarchar(3))+' and SS.SessionID='+cast(@SessionID as nvarchar(3))+'  
 and cast('''+@Year+'-'+@Month+'-'+@Day+''' as date) between SS.FromDate and SS.ToDate  
 order by SM.[Name]'  
   
 insert into @StudentTable   
 exec(@SQLQuery)  
 --Select SM.[StudentID],SM.[Name],SM.Photo,SS.RollNo,SM.Gender,  
 --'STUD'+RIGHT(REPLICATE('0',6)+CAST(SM.[StudentID] AS VARCHAR(6)),6) as [StudentSID],  
 --isnull(nullif(EAM.D28,3),1) as Status ,(case when EAM.D28 is null then 0 else 1 end) as IsExist  
 --from StudentMaster SM left outer join [dbo].[StudentAttendanceMasterT] EAM on EAM.StudentID=SM.StudentID  
 --and EAM.[FYear]=@Year and EAM.[Month]=@Month left outer join Student_Session SS on SS.StudentID=SM.StudentID  
 --where SS.SBranchID=1 and SS.ClassID=@ClassID and SS.SectionID=@SectionID and SS.SessionID=@SessionID  
 --and cast(@AttDate as date)>SS.FromDate  
 --order by SM.[Name]  
  
 declare @tStudentID int,@tStatus numeric(5,2),@lStatus numeric(5,2)  
 DECLARE db_Attendancecursor CURSOR FOR    
 select StudentID,Status from @StudentTable FOR UPDATE OF [Status]  
 OPEN db_Attendancecursor     
 FETCH NEXT FROM db_Attendancecursor INTO @tStudentID,@tStatus  
 WHILE @@FETCH_STATUS = 0     
 BEGIN     
  if(isnull(nullif(@tStatus,3),3)=3)  
  begin  
   if(@WD>@NumDays and isnull(@tStatus,3)=3)  
   begin  
    UPDATE @StudentTable SET [Status] = -2 WHERE CURRENT OF db_Attendancecursor  
   end  
   else if(isnull(@HolidayCnt,0)>0)  
   begin  
    UPDATE @StudentTable SET [Status] = 2 WHERE CURRENT OF db_Attendancecursor  
   end  
   else  
   begin  
    Select @lStatus=max((Case when LeaveType=1 then 1 else 0.5 end)) from [dbo].[LeaveMaster] LM where LM.ApplicantType=1 and LM.IsApproved=1 and LM.ApplicantID=@tStudentID  
    and @AttDate between LM.[StartDate] and LM.[EndDate]  
    if(isnull(@lStatus,0)>0)  
    begin  
     UPDATE @StudentTable SET [Status] = -1 WHERE CURRENT OF db_Attendancecursor  
    end  
   end  
  end  
  
  FETCH NEXT FROM db_Attendancecursor INTO  @tStudentID,@tStatus  
 END     
 CLOSE db_Attendancecursor     
 DEALLOCATE db_Attendancecursor  
  
 select * from @StudentTable  
  
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
  
 select isnull(@ClassID,0)  
 select isnull(@SectionID,0)  
 select case when @WD>@NumDays then -2 when @HolidayCnt>0 then 2 else 1 end  
END
GO
  
Alter procedure dbo.sp_GetTeacherStudentOnlineExamsAnswerSheet   
(  
@SubmissionID int  
)  
AS  
BEGIN  
   
 select EX.OExamID,EX.OExamStartDate,EX.OExamEndDate,EX.ExamTitle,EX.ExamDescription,EX.ExamPriority,OXM.Status  
 ,EM.Name +'('+'STUD'+RIGHT(REPLICATE('0',6)+CAST(EM.StudentID AS VARCHAR(6)),6)+')' as TeacherName,CM.ClassName,CS.Name as SectionName,  
 SS.SessionName ,SM.SubjectName,OXM.SubmissionID,OXM.SubmissionDate,OXM.TeacherRemark,OXM.CheckDate,OXM.Status as SubmissionStatus  
 from OnlineExamMaster EX   
 left outer join ClassMaster CM on CM.ClassID=EX.ClassID  
 left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID  
 left outer join Class_Sections CS on CS.ID=EX.SectionID  
 left outer join SessionMaster SS on SS.SessionID=EX.SessionID  
 left outer join OnlineExamSubmissionMaster OXM on OXM.OExamID=EX.OExamID  
 left outer join StudentMaster EM on EM.StudentID=OXM.StudentID  
 where OXM.SubmissionID=@SubmissionID  
    
  select OXA.SubAnsID as OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4,  
  OXA.Answer as StudentAnswer,OXA.MarksGiven,OXA.Remark as TeacherRemark,isnull(QB.Answer,'') as Answer,QB.Explaination,QB.Complexity  
  from [dbo].[OnlineExamQuestions] OQ  
  left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID  
  left outer join OnlineExamSubmissionAnswers OXA on OXA.OXQID=OQ.OXQID and OXA.SubmissionID=@SubmissionID  
  where OXA.SubmissionID=@SubmissionID  
  
END
GO
Alter procedure dbo.sp_GetStudentOnlineExamsAnswerSheet  
(  
@SubmissionID int,  
@OExamID int,  
@StudentID int  
)  
AS  
BEGIN  
   
 select EX.* ,EM.EmployeeName as TeacherName,CM.ClassName,CS.Name as SectionName,  
 SS.SessionName ,SM.SubjectName,OXM.SubmissionID,OXM.SubmissionDate,OXM.TeacherRemark,OXM.CheckDate,OXM.Status as SubmissionStatus  
 from OnlineExamMaster EX left outer join EmployeeMaster EM on EM.EmployeeID=EX.TeacherID  
 left outer join ClassMaster CM on CM.ClassID=EX.ClassID  
 left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID  
 left outer join Class_Sections CS on CS.ID=EX.SectionID  
 left outer join SessionMaster SS on SS.SessionID=EX.SessionID  
 left outer join OnlineExamSubmissionMaster OXM on OXM.SubmissionID=@SubmissionID  
 where EX.OExamID=@OExamID  
    
  select OQ.OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4,  
  OXA.Answer as StudentAnswer,OXA.MarksGiven,OXA.Remark as TeacherRemark,isnull(QB.Answer,'') as Answer,QB.Explaination,QB.Complexity  
  from [dbo].[OnlineExamQuestions] OQ  
  left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID  
  left outer join OnlineExamSubmissionAnswers OXA on OXA.OXQID=OQ.OXQID  
  where OQ.OXID=@OExamID  
  
END
GO
Alter procedure dbo.sp_GetTeacherOnlineExams  
(  
@TeacherID int ,  
@ClassID int,    
@SectionID int,  
@SubjectID int,  
@SessionID int,  
@SBranchID int  
)  
AS  
BEGIN  
 if(@SessionID=0)  
 begin  
  select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc  
 end  
 declare @Classes table(ClassID int,SubjectID int,SectionID int)    
  Insert into @Classes      
  select ClassID,MondaySubjectID as SubjectID,SectionID from Time_Table_Master where MondayTeacherID=@TeacherID    
  Union    
  select ClassID,TuesdaySubjectID as SubjectID,SectionID from Time_Table_Master where TuesdayTeacherID=@TeacherID    
  Union    
  select ClassID,WednesdaySubjectID as SubjectID,SectionID from Time_Table_Master where WednesdayTeacherID=@TeacherID    
  Union    
  select ClassID,ThursdaySubjectID as SubjectID,SectionID from Time_Table_Master where ThursdayTeacherID=@TeacherID    
  Union    
  select ClassID,FridaySubjectID as SubjectID,SectionID from Time_Table_Master where FridayTeacherID=@TeacherID    
  Union    
  select ClassID,SaturdaySubjectID as SubjectID,SectionID from Time_Table_Master where SaturdayTeacherID=@TeacherID    
    
 if(@ClassID=0)    
  begin    
   Select @ClassID=min(ClassID) from @Classes    
   select @SectionID=min(SectionID) from @Classes   where ClassID=@ClassID
   select @SubjectID=min(SubjectID) from @Classes    where ClassID=@ClassID and SectionID=@SectionID
  end   
  
  Select OX.*,  
  (Select count(*) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as QuestionCount,  
  (Select sum(Marks) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as TotalMarks,  
  (Select count(*) from [dbo].OnlineExamSubmissionMaster OQ where OQ.OExamID=OX.OExamID) as SubmissionCount  
  from OnlineExamMaster OX  
  where TeacherID=@TeacherID and SubjectID=@SubjectID and SectionID=@SectionID and SessionID=@SessionID  
    
  select ClassID as ID,ClassName as Name from ClassMaster where ClassID in (select ClassID from @Classes) and Status=1    
  select isnull(@ClassID,0)   
    
  select distinct SM.SubjectID as ID,SubjectName as Name,ClassID as Extra1,SectionID as Extra2 from @Classes C left outer join SubjectMaster SM on SM.SubjectID=C.SubjectID    
  where SM.SubjectID is not null    
  select isnull(@SubjectID,0)  
  
  select ID,Name,ClassID as Extra1 from Class_Sections CS where ID in (Select SectionID from @Classes)  
  select isnull(@SectionID,0)  
  
  Select SessionID as ID,SessionName as Name, SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID  
  select isnull(@SessionID,0)  
  
END
GO
ALTER procedure dbo.sp_GetStudentOnlineExamsAnswerSheet
(      
@SubmissionID int,      
@OExamID int,      
@StudentID int      
)      
AS      
BEGIN      
       
 select EX.* ,EM.EmployeeName as TeacherName,CM.ClassName,CS.Name as SectionName,      
 SS.SessionName ,SM.SubjectName,OXM.SubmissionID,OXM.SubmissionDate,OXM.TeacherRemark,OXM.CheckDate,OXM.Status as SubmissionStatus      
 from OnlineExamMaster EX left outer join EmployeeMaster EM on EM.EmployeeID=EX.TeacherID      
 left outer join ClassMaster CM on CM.ClassID=EX.ClassID      
 left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID      
 left outer join Class_Sections CS on CS.ID=EX.SectionID      
 left outer join SessionMaster SS on SS.SessionID=EX.SessionID      
 left outer join OnlineExamSubmissionMaster OXM on OXM.SubmissionID=@SubmissionID      
 where EX.OExamID=@OExamID      
        
  select OQ.OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4,      
  isnull(OXA.Answer,'') as StudentAnswer,OXA.MarksGiven,OXA.Remark as TeacherRemark,isnull(QB.Answer,'') as Answer,QB.Explaination,QB.Complexity      
  from [dbo].[OnlineExamQuestions] OQ      
  left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID      
  left outer join OnlineExamSubmissionAnswers OXA on OXA.OXQID=OQ.OXQID  and   OXA.SubmissionID=@SubmissionID
  where OQ.OXID=@OExamID      
      
END
GO
Alter proc [dbo].[sp_GetStudentSessionEditDataEnt]--305,1  
(  
@StudentSessionUID int,  
@SBranchID int  
)  
AS  
BEGIN  
   declare @SessionID int  
   declare @ClassID int  
   declare @GroupID int  
   declare @SectionID int  
   if(@StudentSessionUID=0)  
   begin  
	  select @ClassID=min(ClassID) from ClassMaster where SBranchID=@SBranchID  
	  select Top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc  
	  select @SectionID=min(ID) from Class_Sections where ClassID=@ClassID  
   end  
   else  
   begin  
		select @ClassID=ClassID,@SessionID=SessionID,@SectionID=SectionID from Student_Session SS where SS.StudentSessionUID=@StudentSessionUID and SS.SBranchID=@SBranchID  
   end  
  
  
	 if(@StudentSessionUID=0)  
	   begin  
		select 0 StudentSessionUID,  0 as StudentID,   @ClassID as ClassID   ,@SectionID as SectionID   ,0 QuotaID  ,@SessionID as SessionID 
		,1 as Status  
	   end  
   else  
   begin  
	   select SS.StudentSessionUID,  SS.StudentID,   SS.ClassID   ,SS.SectionID   ,SS.QuotaID    
	   ,SS.FromDate   ,SS.ToDate,SS.HouseID  ,SS.SessionID
		,SS.Status,   SS.RollNo,SS.FeePaymentMode,SS.IsAdmissionFeeApplicable  
		from Student_Session SS where SS.StudentSessionUID=@StudentSessionUID and SS.SBranchID=@SBranchID  
	end  
  
  
   select @GroupID=GroupID from Class_Sections where ID=@SectionID  
  
   select ClassID as ID, ClassName as Name from ClassMaster where SBranchID=@SBranchID order by ClassID  
   select ID,Name from Class_Sections where ClassID=@ClassID and Capacity>(select count(*) from Student_Session where SectionID=ID and SessionID=@SessionID) or ID=@SectionID  
  
   Select QuotaID as ID, QuotaName as Name from QuotaMaster QM where IsApproved=1 and SBranchID = @SBranchID  
   and QM.NumberOfStudents>(select count(*) from Student_Session SS where Status=1 and SS.QuotaID=QM.QuotaID)  
  
   declare @SessionStartDate date  
   declare @SessionEndDate date  
   if(isnull(@SessionID,0)=0)  
   begin  
    select @SessionStartDate= SS.FromDate   ,@SessionEndDate= SS.ToDate  
    from Student_Session SS where SS.StudentSessionUID=@StudentSessionUID and SS.SBranchID=@SBranchID  
   end  
   else  
   begin  
   select @SessionStartDate=SessionStartDate,@SessionEndDate=SessionEndDate from SessionMaster where SessionID=@SessionID  
   end  
   select @SessionStartDate  
   select @SessionEndDate  
      
    Select SessionID,SessionName,SessionStartDate,SessionEndDate,SessionStatus from SessionMaster where SBranchID=@SBranchID  
    order by SessionStatus desc  
  
   select isnull(@SessionID,0)  
  
   select Name,ID from HouseMaster where SBranchID=@SBranchID  
  
   exec [dbo].[spn_GetOptionalSubjectsForSection] @SectionID,@StudentSessionUID  
   
   --select SSOSID as Extra1,StudentSessionID as Name,OpSubjectID as ID from [dbo].[Student_Session_OptionalSubjects] where StudentSessionID=@StudentSessionUID  
END  

------------------------------------------------------------------------------------------------------------------------------------------------------------------------
--Online Class Related 
GO

CREATE TABLE [dbo].[OnlineClassMaster](
	[OCID] [int] IDENTITY(1,1) NOT NULL,
	[TeacherID] [int] NULL,
	[SectionID] [int] NULL,
	[GroupID] [int] NULL,
	[SubjectID] [int] NULL,
	[SessionID] [int] NULL,
	[SBranchID] [int] NULL,
	[Status] [int] NULL,
	[MeetingID] [nvarchar](50) NULL,
	[ClassDate] [date] NULL,
	[StartTime] [nvarchar](10) NULL,
	[EndTime] [nvarchar](10) NULL,
	[CreatedDate] [datetime] NULL,
	[ChangeLog] [nvarchar](50) NULL,
	[StartedOn] [datetime] NULL,
	[EndedOn] [datetime] NULL
) ON [PRIMARY]
GO
GO

CREATE TABLE [dbo].[OnlineClassAttendees](
	[OCID] [int] NULL,
	[StudentID] [int] NULL,
	[StartTime] [nvarchar](12) NULL,
	[EndTime] [nvarchar](12) NULL
) ON [PRIMARY]
GO

Create procedure dbo.sp_GetTeacherOnlineClasses
(
@TeacherID int,
@CurDate date
)
AS
BEGIN
	select OCM.OCID,OCM.Status,OCM.MeetingID,OCM.ClassDate,OCM.StartTime,OCM.EndTime,OCM.StartedOn,CSN.ClassName+'/'+CSN.SectionName as ClassSection,OCM.SectionID,
	SM.SubjectName,OCM.SubjectID
	from OnlineClassMaster OCM left outer join v_ClassSectionNames CSN on CSN.SectionID=OCM.SectionID
	left outer join SubjectMaster SM on SM.SubjectID=OCM.SubjectID
	where OCM.ClassDate>=@CurDate and TeacherID=@TeacherID
	order by ClassDate,CAST(StartTime AS DATETIME)
END
GO
Create procedure dbo.sp_ScheduleTeacherOnlineClass
(
@TeacherID int,
@SectionID int,
@GroupID int,
@SubjectID int,
@ClassDate date,
@StartTime nvarchar(12),
@EndTime nvarchar(12),
@CreatedDate datetime
)
AS
BEGIN
	declare @SBranchID int,@SessionID int
	select @SBranchID=SBranchID from EmployeeMaster where EmployeeID=@TeacherID
	select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc
	declare @MeetingID nvarchar(50)
	set @MeetingID='PSchool_'+cast(@TeacherID as nvarchar(10))+'_'++cast(@SectionID as nvarchar(10))+'_'++cast(@SubjectID as nvarchar(10))+'_'+CONVERT(varchar, @ClassDate, 23)
	Insert into OnlineClassMaster(TeacherID,SectionID,GroupID,SubjectID,Status,MeetingID,ClassDate,StartTime,EndTime,CreatedDate,SessionID,SBranchID)
	values(@TeacherID,@SectionID,@GroupID,@SubjectID,0,@MeetingID,@ClassDate,@StartTime,@EndTime,@CreatedDate,@SessionID,@SBranchID)
	select cast(Scope_Identity() as int) as ID,@MeetingID as Name
END

GO

Create procedure dbo.sp_StartOnlineClass
(
@TeacherID int,
@OCID int,
@StartedOn datetime
)
AS
BEGIN
	Update OnlineClassMaster set StartedOn=@StartedOn,Status=1 where OCID=@OCID and TeacherID=@TeacherID
	select @@rowcount
END
GO
Create procedure dbo.sp_GetOnlineClassAttendees
(
@OCID int
)
AS
BEGIN
	select OCA.StudentID,SM.Name as StudentName,OCA.StartTime,OCA.EndTime
	from OnlineClassAttendees OCA left outer join StudentMaster SM on SM.StudentID=OCA.StudentID	
	where OCID=@OCID
	order by StartTime 
END

GO

Create procedure dbo.sp_EndOnlineClass
(
@TeacherID int,
@OCID int,
@EndedOn datetime
)
AS
BEGIN
	Update OnlineClassMaster set EndedOn=@EndedOn,Status=2 where OCID=@OCID --and TeacherID=@TeacherID
	select @@rowcount
END
GO
Create procedure dbo.sp_GetStudentOnlineClass
(
@StudentID int,
@CurDate datetime
)
AS
BEGIN
	declare @ClassID int, @SectionID int,@StudentSessionUID int,@SessionID int
	select top 1 @SectionID=SectionID,@ClassID=ClassID,@StudentSessionUID=StudentSessionUID ,@SessionID=SessionID
	from Student_Session where studentID=@StudentID order by StudentSessionUID desc 

	select OCM.OCID,OCM.Status,OCM.MeetingID,OCM.ClassDate,OCM.StartTime,OCM.EndTime,OCM.StartedOn,OCM.SectionID,
	SM.SubjectName,OCM.SubjectID,EM.EmployeeName as TeacherName
	from OnlineClassMaster OCM left outer join EmployeeMaster EM on EM.EmployeeID=OCM.TeacherID
	left outer join SubjectMaster SM on SM.SubjectID=OCM.SubjectID
	where OCM.ClassDate>=@CurDate and SectionID=@SectionID and SessionID=@SessionID
	order by ClassDate,CAST(StartTime AS DATETIME) 
END
GO
Create procedure dbo.sp_JoinStudentOnlineClass
(
@StudentID int,
@JoinDate datetime,
@OCID int
)
AS
BEGIN
	Insert into OnlineClassAttendees(OCID,StudentID,StartTime)
	values(@OCID,@StudentID,cast(cast(@JoinDate as time) as nvarchar(12)))
	select @@rowcount
END
GO
Create procedure dbo.sp_LeaveStudentOnlineClass
(
@StudentID int,
@LeaveDate datetime,
@OCID int
)
AS
BEGIN
	WITH UpdateList_view AS (
	  SELECT TOP 1  * from OnlineClassAttendees WHERE OCID=@OCID and StudentID=@StudentID
	  ORDER BY StartTime DESC 
	)

	update UpdateList_view set EndTime=cast(cast(@LeaveDate as time) as nvarchar(12))
	select @@rowcount
END
GO
Create function dbo.TeacherClassSectionSubjects()
Returns @Classes table(ClassID int,SubjectID int,SectionID int,DayID int,PeriodID int,TeacherID int,IsMerger int)   
AS
BEGIN
  Insert into @Classes  
	select ClassID,MondaySubjectID as SubjectID,SectionID,1 as DayID,PeriodID,MondayTeacherID,0 from Time_Table_Master  where isnull(MondayTeacherID,0)!=0
  Union    
	 select ClassID,TuesdaySubjectID as SubjectID,SectionID,2 as DayID,PeriodID,TuesdayTeacherID,0 from Time_Table_Master    where isnull(TuesdayTeacherID,0)!=0
  Union    
	 select ClassID,WednesdaySubjectID as SubjectID,SectionID,3 as DayID,PeriodID,WednesdayTeacherID,0 from Time_Table_Master where isnull(wednesdayTeacherID,0)!=0
  Union    
	 select ClassID,ThursdaySubjectID as SubjectID,SectionID,4 as DayID,PeriodID,ThursdayTeacherID,0from Time_Table_Master where isnull(ThursdayTeacherID,0)!=0
  Union    
	 select ClassID,FridaySubjectID as SubjectID,SectionID,5 as DayID,PeriodID,FridayTeacherID,0 from Time_Table_Master where isnull(FridayTeacherID,0)!=0    
  Union    
	 select ClassID,SaturdaySubjectID as SubjectID,SectionID,6 as DayID,PeriodID,SaturdayTeacherID,0 from Time_Table_Master where isnull(SaturdayTeacherID,0)!=0
    

	insert into @Classes
	select CS.ClassID,CMM.SubjectID,CMM.SecondSectionID,CMM.DayID,CMM.PeriodID,CL.TeacherID,1
	from [dbo].[Class_Merge_Master] CMM left outer join Class_Sections CS on CMM.SecondSectionID=CS.ID
	left outer join @Classes CL on CL.SectionID=CMM.FirstSectionID and CL.PeriodID=CMM.PeriodID and CL.DayID=CMM.DayID
	where CL.ClassID is not null and isnull(CL.TeacherID,0)!=0
	
	Return
END

GO

Alter FUNCTION [dbo].[GetEvaluationSchemeClassList]  
(  
@SchemeID int  ,
@SessionID int
)  
RETURNS nvarchar(max)  
as  
begin  
declare @Classes nvarchar(Max)  
Set @Classes= ''  
declare @ClassID int  
declare @ClassName nvarchar(max)  
declare FirstCursor cursor  for select HC.ClassID, CM.ClassName 
from [ClassSessionDetails] HC left Outer Join ClassMaster CM on HC.ClassID=CM.ClassID where HC.SchemeID=@SchemeID   and HC.SessionID=@SessionID
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
Alter FUNCTION [dbo].[GetSchemeClassListIDs]    
(    
@SchemeID int    ,
@SessionID int
)    
RETURNS nvarchar(max)    
as    
begin    
declare @Classes nvarchar(Max)    
Set @Classes= ''    
declare @ClassID int    
declare FirstCursor cursor  for select HC.ClassID from [ClassSessionDetails] HC where HC.SchemeID=@SchemeID    and HC.SessionID=@SessionID
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
ALTER proc [dbo].[sp_GetEvaluationSchemes]
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
 Select EvaluationSchemeID,EvaluationSchemeName,[dbo].[GetEvaluationSchemeClassList](EvaluationSchemeID,@SessionID) as Classes,
 [dbo].[GetSchemeClassListIDs](EvaluationSchemeID,@SessionID) as ClassIDs   
 from EvaluationSchemeMaster where SBranchID=@SBranchID and SessionID=@SessionID  
 select @SessionID  
 Select SessionID,SessionName,SessionStatus,SessionStartDate,SessionEndDate from SessionMaster where SBranchID=@SBranchID order by SessionStartDate   
   
 select CM.ClassID,CM.ClassName,ESM.EvaluationSchemeName as EducationLevelName, ESM.EvaluationSchemeID   
 from ClassMaster CM left outer join [ClassSessionDetails] CSD on CSD.SessionID=@SessionID and CSD.ClassID=CM.ClassID  
 left outer join EvaluationSchemeMaster ESM on ESM.EvaluationSchemeID=CSD.SchemeID  and ESM.SessionID=@SessionID
 where CM.SBranchID=@SBranchID and CM.Status=1  
END 

---------------------------------------------------------------------------------------------------------------------------------------------------------
--Merged Section Teacher Class Section Issue Related
---------------------------------------------------------------------------------------------------------------------------------------------------------
GO
Create function [dbo].[TeacherClassSectionSubjects](@TeacherID int=0)
Returns @Classes table(ClassID int,SubjectID int,SectionID int,DayID int,PeriodID int,TeacherID int,IsMerger int)   
AS
BEGIN
	if(@TeacherID=0)
	begin
  Insert into @Classes  
	select ClassID,MondaySubjectID as SubjectID,SectionID,1 as DayID,PeriodID,MondayTeacherID,0 from Time_Table_Master  where isnull(MondayTeacherID,0)!=0
  Union    
	 select ClassID,TuesdaySubjectID as SubjectID,SectionID,2 as DayID,PeriodID,TuesdayTeacherID,0 from Time_Table_Master    where isnull(TuesdayTeacherID,0)!=0
  Union    
	 select ClassID,WednesdaySubjectID as SubjectID,SectionID,3 as DayID,PeriodID,WednesdayTeacherID,0 from Time_Table_Master where isnull(wednesdayTeacherID,0)!=0
  Union    
	 select ClassID,ThursdaySubjectID as SubjectID,SectionID,4 as DayID,PeriodID,ThursdayTeacherID,0from Time_Table_Master where isnull(ThursdayTeacherID,0)!=0
  Union    
	 select ClassID,FridaySubjectID as SubjectID,SectionID,5 as DayID,PeriodID,FridayTeacherID,0 from Time_Table_Master where isnull(FridayTeacherID,0)!=0    
  Union    
	 select ClassID,SaturdaySubjectID as SubjectID,SectionID,6 as DayID,PeriodID,SaturdayTeacherID,0 from Time_Table_Master where isnull(SaturdayTeacherID,0)!=0
    end
	else
	begin
 Insert into @Classes  
	select ClassID,MondaySubjectID as SubjectID,SectionID,1 as DayID,PeriodID,MondayTeacherID,0 from Time_Table_Master  where isnull(MondayTeacherID,0)=@TeacherID
  Union    
	 select ClassID,TuesdaySubjectID as SubjectID,SectionID,2 as DayID,PeriodID,TuesdayTeacherID,0 from Time_Table_Master    where isnull(TuesdayTeacherID,0)=@TeacherID
  Union    
	 select ClassID,WednesdaySubjectID as SubjectID,SectionID,3 as DayID,PeriodID,WednesdayTeacherID,0 from Time_Table_Master where isnull(wednesdayTeacherID,0)=@TeacherID
  Union    
	 select ClassID,ThursdaySubjectID as SubjectID,SectionID,4 as DayID,PeriodID,ThursdayTeacherID,0from Time_Table_Master where isnull(ThursdayTeacherID,0)=@TeacherID
  Union    
	 select ClassID,FridaySubjectID as SubjectID,SectionID,5 as DayID,PeriodID,FridayTeacherID,0 from Time_Table_Master where isnull(FridayTeacherID,0)=@TeacherID    
  Union    
	 select ClassID,SaturdaySubjectID as SubjectID,SectionID,6 as DayID,PeriodID,SaturdayTeacherID,0 from Time_Table_Master where isnull(SaturdayTeacherID,0)=@TeacherID

	end

	insert into @Classes
	select CS.ClassID,CMM.SubjectID,CMM.SecondSectionID,CMM.DayID,CMM.PeriodID,CL.TeacherID,1
	from [dbo].[Class_Merge_Master] CMM left outer join Class_Sections CS on CMM.SecondSectionID=CS.ID
	left outer join @Classes CL on CL.SectionID=CMM.FirstSectionID and CL.PeriodID=CMM.PeriodID and CL.DayID=CMM.DayID
	where CL.ClassID is not null and isnull(CL.TeacherID,0)!=0
	
	Return
END
GO


Alter procedure [dbo].[sp_GetTeacherOnlineExams]    
(    
@TeacherID int ,    
@ClassID int,      
@SectionID int,    
@SubjectID int,    
@SessionID int,    
@SBranchID int    
)    
AS    
BEGIN    
 if(@SessionID=0)    
 begin    
  select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc    
 end    
 declare @Classes table(ClassID int,SubjectID int,SectionID int)      
  Insert into @Classes       
 select ClassID,SubjectID,SectionID from dbo.TeacherClassSectionSubjects(@TeacherID)    
      
 if(@ClassID=0)      
  begin      
   Select @ClassID=min(ClassID) from @Classes      
   select @SectionID=min(SectionID) from @Classes   where ClassID=@ClassID  
   select @SubjectID=min(SubjectID) from @Classes    where ClassID=@ClassID and SectionID=@SectionID  
  end     
    
  Select OX.*,    
  (Select count(*) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as QuestionCount,    
  (Select sum(Marks) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as TotalMarks,    
  (Select count(*) from [dbo].OnlineExamSubmissionMaster OQ where OQ.OExamID=OX.OExamID) as SubmissionCount    
  from OnlineExamMaster OX    
  where TeacherID=@TeacherID and SubjectID=@SubjectID and SectionID=@SectionID and SessionID=@SessionID    
      
  select ClassID as ID,ClassName as Name from ClassMaster where ClassID in (select ClassID from @Classes) and Status=1      
  select isnull(@ClassID,0)     
      
  select distinct SM.SubjectID as ID,SubjectName as Name,ClassID as Extra1,SectionID as Extra2 from @Classes C left outer join SubjectMaster SM on SM.SubjectID=C.SubjectID      
  where SM.SubjectID is not null      
  select isnull(@SubjectID,0)    
    
  select ID,Name,ClassID as Extra1 from Class_Sections CS where ID in (Select SectionID from @Classes)    
  select isnull(@SectionID,0)    
    
  Select SessionID as ID,SessionName as Name, SessionStatus as Extra1 from SessionMaster where SBranchID=@SBranchID    
  select isnull(@SessionID,0)    
    
END  
 GO
 Alter procedure dbo.sp_GetTeacherQuestionBank    
(    
@TeacherID int,    
@ClassID int,    
@QuestionsBy int    
)    
AS    
BEGIN    
 declare @Classes table(ClassID int,SubjectID int)    
 Insert into @Classes      
 select ClassID,SubjectID from dbo.TeacherClassSectionSubjects(@TeacherID)

 if(@ClassID=0)    
 begin    
  Select @ClassID=min(ClassID) from @Classes    
 end    
 if(@QuestionsBy=0)    
 begin    
  Select QuestionID,TeacherID,ClassID,QB.GroupID,QB.SubjectID,ChapterID,TopicID,QuestionType,Complexity,QuestionText,QuestionImage,Option1,Option1Image,Option2,Option2Image,    
  Option3,Option3Image,Option4,Option4Image,Answer,Explaination,ExplainationImage,SBranchID,CreatedDate ,SM.SubjectName    
  From QuestionBankMaster QB left outer join SubjectMaster SM on SM.SubjectID=QB.SubjectID    
  where TeacherID=@TeacherID and ClassID=@ClassID    
 end    
 else    
 begin    
 Select QuestionID,TeacherID,EM.EmployeeName as TeacherName,ClassID,QB.GroupID,QB.SubjectID,ChapterID,TopicID,QuestionType,Complexity,  
  QuestionText,QuestionImage,Option1,Option1Image,Option2,Option2Image,    
  Option3,Option3Image,Option4,Option4Image,Answer,Explaination,ExplainationImage,QB.SBranchID,QB.CreatedDate ,SM.SubjectName    
  From QuestionBankMaster QB left outer join SubjectMaster SM on SM.SubjectID=QB.SubjectID    
  left outer join EmployeeMaster EM on EM.EmployeeID=QB.TeacherID  
  where ClassID=@ClassID     
 end    
    
 select ClassID as ID,ClassName as Name from ClassMaster where ClassID in (select ClassID from @Classes) and Status=1    
 select @ClassID    
    
 select distinct SM.SubjectID as ID,SubjectName as Name,ClassID as Extra1 from @Classes C left outer join SubjectMaster SM on SM.SubjectID=C.SubjectID    
 where SM.SubjectID is not null    
END
GO

  
Alter Procedure [dbo].[sp_GetTeacherYouTubeVideoEditData]  
(  
@TeacherID int,  
@VideoID int,  
@SBranchID int  
)  
AS  
BEGIN  
  declare @ClassID int,@SubjectID int,@SectionID int,@SessionID int   
   
  select @ClassID=ClassID,@SubjectID=SubjectID,@SectionID=SectionID,@SessionID=SessionID  
  from [dbo].[YouTubeVideos] where VideoID=@VideoID  

  declare @Classes table(ClassID int,SubjectID int,SectionID int)      
  Insert into @Classes       
	 select ClassID,SubjectID,SectionID from dbo.TeacherClassSectionSubjects(@TeacherID) 
  
  declare @ClassesTable  table (ID int,Name nvarchar(20))  
  declare @SectionsTable  table (ID int,Name nvarchar(20),ClassID int)  
  declare @EvaluationsTable  table (ID int,Name nvarchar(20))  
  declare @SubjectsTable  table (ID int,Name nvarchar(50))  
    
  INSERT INTO @SectionsTable(ID,Name,ClassID) select ID,Name,ClassID from Class_Sections where ID in (Select SectionID from @Classes)  
  
  Insert into @ClassesTable(ID,Name) select ClassID,ClassName from ClassMaster where ClassID in (select ClassID from @Classes)  
    
  if(isnull(@SectionID,0)=0)  
  begin  
   select @ClassID =min(ID) from @ClassesTable      
   select @SectionID=min(SectionID) from @Classes where ClassID=@ClassID
  end  
    
  if(isnull(@SessionID,0)=0)  
  begin  
   Select top 1 @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc  
  end  
  
  select ID,Name from @ClassesTable   
  select ID,Name from @SectionsTable where ClassID=@ClassID
  
  Insert into @SubjectsTable(ID,Name) Select SubjectID, SubjectName  from SubjectMaster where SubjectID in  
  (Select SubjectID from @Classes where ClassID=@ClassID and SectionID=@SectionID)  
  if(@SubjectID=0)  
  begin  
   select @SubjectID=min(ID) from @SubjectsTable  
  end  
  select ID,Name from @SubjectsTable  
      
  Select * from [dbo].[YouTubeVideos] where VideoID=@VideoID  
  
  select SM.StudentID as ID,Name ,SchoolUID as Extra1 ,isnull(YS.StudentID,0) as Extra2  
  from StudentMaster SM left outer join [dbo].[YouTubeVideo_Students] YS  
  on YS.VideoID=@VideoID and YS.StudentID=SM.StudentID  
  where SM.StudentID in (Select StudentID from Student_Session where SessionID=@SessionID and SectionID=@SectionID)  
  
  Select SessionID as ID , SessionName as Name, SessionStatus as Extra1   
  from SessionMaster where SBranchID=@SBranchID order by SessionStatus desc  
  
  Select isnull(@SessionID,0)  
  Select isnull(@ClassID,0)  
  Select isnull(@SectionID,0)  
  Select isnull(@SubjectID,0)  
  Select isnull(@TeacherID,0)  
END  
GO

Alter Procedure [dbo].[sp_GetTeacherSectionSubjectsAndStudents]  
(  
@TeacherID int,  
@ClassID int,  
@SectionID int,  
@SessionID int,  
@VideoID int=0  
)  
AS  
BEGIN  
  declare @SectionsTable  table (ID int,Name nvarchar(20),ClassID int)  
  declare @SubjectsTable  table (ID int,Name nvarchar(50))  
  declare @Classes table(ClassID int,SubjectID int,SectionID int)      
  
  Insert into @Classes       
	select ClassID,SubjectID,SectionID from dbo.TeacherClassSectionSubjects(@TeacherID) 

  INSERT INTO @SectionsTable(ID,Name,ClassID) select ID,Name,ClassID from Class_Sections where ID in (Select SectionID from @Classes)  
  declare @SubjectID int=0  
  
  if(@SectionID=0)  
  begin     
   select @SectionID=min(ID) from @SectionsTable where ClassID=@ClassID
  end  
  
  select ID,Name from @SectionsTable where ClassID=@ClassID  
  
  Insert into @SubjectsTable(ID,Name) Select SubjectID, SubjectName  from SubjectMaster where SubjectID in  
  (Select SubjectID from @Classes where ClassID=@ClassID and SectionID=@SectionID)  
  if(@SubjectID=0)  
  begin  
   select @SubjectID=min(ID) from @SubjectsTable  
  end  
  select ID,Name from @SubjectsTable  
      
  select isnull(@SectionID,0)  
  select isnull(@SubjectID,0)  
  
   
  select SM.StudentID as ID,Name ,isnull(SchoolUID,'') as Extra1 ,isnull(YS.StudentID,0) as Extra2  
  from StudentMaster SM left outer join [dbo].[YouTubeVideo_Students] YS  
  on YS.VideoID=@VideoID and YS.StudentID=SM.StudentID  
  where SM.StudentID in (Select StudentID from Student_Session where SessionID=@SessionID and SectionID=@SectionID)  
  
END  
  
  GO
  
  Alter proc [dbo].[sp_GetTeacherTeachingSubjectsOnClassSection]  
(  
@ClassID int,  
@SectionID int,  
@TeacherID int  
)  
AS  
BEGIN  
 declare @Classes table(ClassID int,SubjectID int,SectionID int)      
  Insert into @Classes       
 select ClassID,SubjectID,SectionID from dbo.TeacherClassSectionSubjects(@TeacherID) 

 Select SubjectID as ID, SubjectName as Name from SubjectMaster where SubjectID in  
  (select SubjectID from @Classes where ClassID=@ClassID and SectionID=@SectionID)  
END  
  
  GO
  
 Alter proc [dbo].[sp_GetTeacherTeachingClassSections] 
(  
@TeacherID int  
)  
AS  
BEGIN  
	declare @Classes table(ClassID int,SubjectID int,SectionID int)      
  Insert into @Classes       
 select ClassID,SubjectID,SectionID from dbo.TeacherClassSectionSubjects(@TeacherID) 

 select CM.ClassID,CM.ClassName, t.SectionID,t.SectionName from  
 (select ClassID, ID as SectionID,Name as SectionName from Class_Sections where ID in (Select SectionID from @Classes))t  
  left outer join ClassMaster CM on t.ClassID=CM.ClassID  
END  
  GO
  
  Alter proc [dbo].[sp_GetSubjectParentDiaryList]  
(  
@TeacherID int,  
@ClassID int,  
@SectionID int,  
@SubjectID int,  
@SBranchID int  
)  
AS  
BEGIN  
  declare @ClassesTable  table (ID int,Name nvarchar(20))  
  declare @SectionsTable  table (ID int,Name nvarchar(20),ClassID int)  
  declare @EvaluationsTable  table (ID int,Name nvarchar(20))  
  declare @SubjectsTable  table (ID int,Name nvarchar(50))  
  
  declare @Classes table(ClassID int,SubjectID int,SectionID int)      
  Insert into @Classes       
 select ClassID,SubjectID,SectionID from dbo.TeacherClassSectionSubjects(@TeacherID) 

  INSERT INTO @SectionsTable(ID,Name,ClassID) select ID,Name,ClassID from Class_Sections where ID in (Select SectionID from @Classes)  
  
  Insert into @ClassesTable(ID,Name) select ClassID,ClassName from ClassMaster where ClassID in   
  (Select ClassID from @Classes)  
    
  if(@SectionID=0)  
  begin  
   select @ClassID =min(ID) from @ClassesTable      
   select @SectionID=min(ID) from @SectionsTable where ClassID=@ClassID
  end  
  select ID,Name from @ClassesTable   
  select ID,Name from @SectionsTable where ClassID=@ClassID
  
  Insert into @SubjectsTable(ID,Name) Select SubjectID, SubjectName  from SubjectMaster where SubjectID in  
  (Select SubjectID from @Classes where ClassID=@ClassID and SectionID=@SectionID)  
  if(@SubjectID=0)  
  begin  
   select @SubjectID=min(ID) from @SubjectsTable  
  end  
  select ID,Name from @SubjectsTable  
  
  exec sp_GetStudentsByClassSection @ClassID,@SectionID, @SBranchID,''  
    
  select @ClassID  
  select @SectionID  
  select @SubjectID  
  
END  
  
  GO
  
  Alter proc [dbo].[sp_GetClassGroupWiseExamResults]  
(  
@TeacherID int,  
@EvaluationID int,  
@ClassID int,  
@SectionID int,  
@SubjectID int,  
@SBranchID int,  
@EvaluationMode int,  
@SessionID int=0  
)  
AS  
BEGIN  
  if(@SessionID=0)  
  begin  
   Select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and SessionStatus=1  
  end   
  declare @ClassesTable  table (ID int,Name nvarchar(20))  
  declare @SectionsTable  table (ID int,Name nvarchar(20),ClassID int)  
  declare @EvaluationsTable  table (ID int,Name nvarchar(100))  
  declare @SubjectsTable  table (ID int,Name nvarchar(100))  
  declare @Classes table(ClassID int,SubjectID int,SectionID int)      
  Insert into @Classes       
 select ClassID,SubjectID,SectionID from dbo.TeacherClassSectionSubjects(@TeacherID) 

  INSERT INTO @SectionsTable(ID,Name,ClassID) select ID,Name,ClassID from Class_Sections where ID in (Select SectionID from @Classes)  
  
  Insert into @ClassesTable(ID,Name) select ClassID,ClassName from ClassMaster where ClassID in   
  (Select ClassID from @Classes)  
    
  if(@SectionID=0)  
  begin  
   select @ClassID =min(ID) from @ClassesTable      
   select @SectionID=min(ID) from @SectionsTable where ClassID =@ClassID
  end  
  select ID,Name from @ClassesTable   
  select ID,Name from @SectionsTable where ClassID=@ClassID 
  
  Insert into @SubjectsTable(ID,Name) Select SubjectID, SubjectName  from SubjectMasterAll where SubjectID in  
  (Select SubjectID from @Classes where ClassID=@ClassID and SectionID=@SectionID)  
  if(@SubjectID=0)  
  begin  
   select @SubjectID=min(ID) from @SubjectsTable  
  end  
  select ID,Name from @SubjectsTable  
  
  insert into @EvaluationsTable(ID,Name) exec sp_GetEvaluationList @SBranchID,@EvaluationMode,@ClassID,@SessionID  
  if(@EvaluationID=0)  
  begin  
   select @EvaluationID=min(ID) from @EvaluationsTable  
  end  
  Select ID,Name from @EvaluationsTable  
  
 declare @MaxMarks int   
 declare @PassMarks int   
 declare @GroupID int  
 declare @ExamID int  
 Select @GroupID=GroupID from Class_Sections where ID=@SectionID  
 select @MaxMarks=MaxMarks,@PassMarks=PassMarks,@ExamID=ExamID from [dbo].[ExamMaster] where SubjectID=@SubjectID and ClassID=@ClassID and   
 EvaluationID=@EvaluationID and GroupID=@GroupID and SBranchID=@SBranchID  
  
 if(@ExamID<>0)  
 begin  
 Select isnull(ERM.ResultID,0) as ResultID, @ExamID as ExamID,SS.StudentID,  
 'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.[StudentID] AS VARCHAR(6)),6) as [StudentSID]  
 ,SS.RollNo,(Select SM.Name from StudentMaster SM where SM.StudentID=SS.StudentID) as StudentName,  
 (Select SM.Photo from StudentMaster SM where SM.StudentID=SS.StudentID) as Photo,  
 (Select SM.Gender from StudentMaster SM where SM.StudentID=SS.StudentID) as Gender  
 ,@MaxMarks as MaxMarks,@PassMarks as PassMarks, ERM.MarksScored,ERM.Grade,ERM.GradePoints,isnull(ERM.[Status],0) as [Status]  
 from Student_Session SS left outer join ExamResultMaster ERM on SS.StudentID=ERM.StudentID and ERM.ExamID=@ExamID  
 where SS.ClassID=@ClassID and SS.SectionID=@SectionID and SS.[Status]=1  
   end  
   else  
   begin  
  select 0 as ResultID,0 as ExamID, 0 as StudentID,'' as StudentSID from ExamResultMaster where 1=2  
   end  
 select @ClassID  
 select @SectionID  
 select @SubjectID  
 select @EvaluationID  
  
END  
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
--Extra Income Related
GO

CREATE TABLE [dbo].[ExtraIncomeHeads](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[Status] [int] NULL,
	[SBranchID] [int] NULL
) ON [PRIMARY]
GO
GO

CREATE TABLE [dbo].[ExtraIncomes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](500) NULL,
	[PaidBy] [nvarchar](50) NULL,
	[FY] [int] NULL,
	[EIHeadID] [int] NULL,
	[Amount] [numeric](18, 2) NULL,
	[Description] [nvarchar](max) NULL,
	[Status] [int] NULL,
	[PaidDate] [datetime] NULL,
	[CreatedDate] [datetime] NULL,
	[RecievedBy] [nvarchar](50) NULL,
	[PaymentMode] [int] NULL,
	[ReferanceNo] [nvarchar](50) NULL,
	[PayerAddress] [nvarchar](50) NULL,
	[PayerCity] [nvarchar](50) NULL,
	[PayerState] [nvarchar](50) NULL,
	[PayerPin] [nvarchar](10) NULL,
	[PayerContact] [nvarchar](50) NULL,
	[PayerPAN] [nvarchar](50) NULL,
	[SBranchID] [int] NULL,
	[UserID] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
Create procedure dbo.sp_GetExtraIncomeHeads
(
@SBranchID int
)
AS
BEGIN
	select * from [ExtraIncomeHeads] where SBranchID=@SBranchID
END
GO
Create Procedure dbo.sp_UpdateExtraIncomeHead
(
@ID int,
@Name nvarchar(50),
@CreatedDate datetime,
@Status int,
@SBranchID int,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from [ExtraIncomeHeads] where ID=@ID
	end
	else
	Begin
		if(@ID=0)
		begin
			Insert into [ExtraIncomeHeads](Name,Status,CreatedDate,SBranchID)
			values(@Name,@Status,@CreatedDate,@SBranchID)
			select @ID=Cast(Scope_Identity() as int)
		end
		else
		begin
			Update [ExtraIncomeHeads] set Name=@Name,Status=@Status where ID=@ID
		end
	End
	select @ID
END
GO

Create Procedure dbo.sp_GetExtraIncomePageData
(
@SBranchID int,
@PaymentMode int,
@StartDate date,
@EndDate date
)
AS
Begin
	select * from [ExtraIncomeHeads] where SBranchID=@SBranchID and Status=1
	if(@PaymentMode=-1)
	begin
		Select EI.*,EIH.Name as HeadName
		from ExtraIncomes EI left outer join [ExtraIncomeHeads] EIH on EIH.ID=EI.EIHeadID
		where PaidDate between @StartDate and @EndDate and EI.SBranchID=@SBranchID
	end
	else
	begin
		Select EI.*,EIH.Name as HeadName
		from ExtraIncomes EI left outer join [ExtraIncomeHeads] EIH on EIH.ID=EI.EIHeadID
		where PaidDate between @StartDate and @EndDate and EI.SBranchID=@SBranchID and PaymentMode=@PaymentMode
	end
End
GO
GO
Create Procedure dbo.sp_UpdateExtraIncome
(
@ID int,
@Title nvarchar(500),
@PaidBy nvarchar(50),
@EIHeadID int,
@Amount numeric(18,2),
@Description nvarchar(max),
@Status int,
@PaidDate datetime,
@CreatedDate datetime,
@RecievedBy nvarchar(50),
@PaymentMode int,
@ReferanceNo nvarchar(50),
@PayerAddress nvarchar(50),
@PayerCity nvarchar(50),
@PayerState nvarchar(50),
@PayerPin nvarchar(50),
@PayerContact nvarchar(50),
@PayerPAN nvarchar(50),
@SBranchID int,
@UserID int,
@OpType int
)
AS
BEGIN
	if(@OpType=-1)
	begin
		Delete from [dbo].[ExtraIncomes] where ID=@ID
	end
	else
	begin
		if(@ID=0)
		begin
			Declare @FYear nvarchar(10)
			if(datepart(month,@PaidDate)<=3)
			begin
				set @FYear= cast((DATEPART("yyyy",@PaidDate) % 100-1) as nvarchar(4))+cast((DATEPART("yyyy",@PaidDate) % 100) as nvarchar(4))
			end
			else
			begin
				set @FYear= cast((DATEPART("yyyy",@PaidDate) % 100) as nvarchar(4))+cast((DATEPART("yyyy",@PaidDate) % 100+1) as nvarchar(4))
			end
			Insert into [dbo].[ExtraIncomes](Title,PaidBy,FY,EIHeadID,Amount,Description,Status,PaidDate,CreatedDate,RecievedBy,
			PaymentMode,ReferanceNo,SBranchID,PayerAddress,PayerCity,PayerState,PayerPin,PayerContact,PayerPAN,UserID)
			Values(@Title,@PaidBy,@FYear,@EIHeadID,@Amount,@Description,@Status,@PaidDate,@CreatedDate,@RecievedBy,
			@PaymentMode,@ReferanceNo,@SBranchID,@PayerAddress,@PayerCity,@PayerState,@PayerPin,@PayerContact,@PayerPAN,@UserID)
			select @ID=Cast(Scope_Identity() as int)
		end
		else
		begin
			Update [dbo].[ExtraIncomes] set Title=@Title,PaidBy=@PaidBy,EIHeadID=@EIHeadID,Amount=@Amount,Description=@Description
			,Status=@Status,PaidDate=@PaidDate,RecievedBy=@RecievedBy,PaymentMode=@PaymentMode,ReferanceNo=@ReferanceNo
			,PayerAddress=@PayerAddress,PayerCity=@PayerCity,PayerState=@PayerState,PayerPin=@PayerPin,PayerContact=@PayerContact,PayerPAN=@PayerPAN
			where ID=@ID
		end
	end
	select @ID
END
GO

Create Procedure dbo.sp_GetExtraIncomeDetails
(
@ID int,
@SBranchID int
)
AS
Begin
	select * from [ExtraIncomeHeads] where SBranchID=@SBranchID and Status=1
	
	Select EI.*,EIH.Name as HeadName
	from ExtraIncomes EI left outer join [ExtraIncomeHeads] EIH on EIH.ID=EI.EIHeadID
	where EI.ID=@ID
	
End
GO  
CREATE proc [dbo].[sp_GetAdminAssignmentList] -- 0,0,0,1,'2020-01-01','2020-06-01',2    
(      
@ClassID int,      
@SectionID int,       
@TeacherID int ,    
@SType int,    
@FromDate date,    
@EndDate date,    
@SBranchID int    
)      
AS      
BEGIN        
  if(@ClassID=0)    
  begin    
   select top 1 @ClassID=ClassID from ClassMaster where Status=1 and SBranchID=@SBranchID     
   order by Isnull(SequenceNo,0),ClassID    
  end    
 if(@SType=0)    
 Begin    
  if(@TeacherID=0)    
  begin    
   select Top 1 @TeacherID=EmployeeID from EmployeeMaster     
   where SBranchID=@SBranchID and EmployeeID in (select EmployeeID from [Teacher_Subject])    
  end    
  SELECT [ID] ,[ChapterID] ,[TopicID]    
  ,[StartDate] ,[Enddate] ,[GracedDays],[TaskType] ,[Title] ,[Detail],[Attachments] ,[SendMail] ,[CreatedOn],      
  (Select SubjectName from SubjectMasterAll SMA where SMA.SubjectID=AM.SubjectID) as SubjectName,  CS.ClassName+'/'+CS.SectionName as EDetail,    
  (Select Count(*) from AssignmentSubmissions ASub where ASub.AssignmentID=AM.ID and ASub.Status=1) as Submissions      
  FROM [dbo].[AssignmentMaster] AM left outer join [dbo].[v_ClassSectionNames] CS on CS.SectionID=AM.SectionID    
  where TeacherID=@TeacherID   and StartDate between @FromDate and @EndDate    
  order by StartDate desc    
 End    
  else    
  Begin    
  if(@SectionID=0)    
  begin    
   Select top 1 @SectionID=ID from Class_Sections where ClassID=@ClassID and Status=1    
  end    
  SELECT [ID] ,[ChapterID] ,[TopicID]    
  ,[StartDate] ,[Enddate] ,[GracedDays],[TaskType] ,[Title] ,[Detail],[Attachments] ,[SendMail] ,[CreatedOn],      
  (Select SubjectName from SubjectMasterAll SMA where SMA.SubjectID=AM.SubjectID) as SubjectName,  EM.EmployeeName as EDetail,    
  (Select Count(*) from AssignmentSubmissions ASub where ASub.AssignmentID=AM.ID and ASub.Status=1) as Submissions      
  FROM [dbo].[AssignmentMaster] AM Left outer join EmployeeMaster EM on EM.EmployeeID=AM.TeacherID    
  where ClassID=@ClassID and SectionID=@SectionID and StartDate between @FromDate and @EndDate     
  order by StartDate desc    
 End      
    
 select ClassID as ID, ClassName as Name from ClassMaster where Status=1 and SBranchID=@SBranchID    
     
 select ID,Name from Class_Sections where ClassID=@ClassID and Status=1    
     
 select EmployeeID as ID, EmployeeName as Name from EmployeeMaster     
 where SBranchID=@SBranchID and EmployeeID in (select EmployeeID from [Teacher_Subject])    
     
    
 select isnull(@ClassID,0)    
 select isnull(@SectionID,0)    
 select isnull(@TeacherID,0)    
END 
GO


Alter proc [dbo].[sp_InsertUpdateEventType]  
(  
@ID int,  
@Name nvarchar(50),  
@BackGroundColor nvarchar(20),  
@TextColor nvarchar(20),  
@OpType int  ,
@SBranchID int=0
)  
AS  
BEGIN  
 if(@OpType=-1)  
 begin  
  delete from EventTypeMaster where ID=@ID  
 end  
 else if(@ID=0)  
 begin  
  Insert into [dbo].[EventTypeMaster](Name,BackGroundColor,TextColor,SBranchID)  
  values(@Name,@BackGroundColor,@TextColor,@SBranchID)  
 end  
 else  
 begin  
  Update [dbo].[EventTypeMaster] set Name=@Name,BackGroundColor=@BackGroundColor,TextColor=@TextColor  
  where ID=@ID  
 end  
END  

GO

Alter proc [dbo].[sp_GetEventCalendar]   
(    
@SBranchID int,    
@Month int,    
@Year int,    
@Status int=0  ,  
@ParentID int=0  
)    
AS    
BEGIN    
    
  if(@Status=0)    
  begin    
    Select EventID,Title, [Description], StartDate, EndDate, EventTypeID,    
    ETM.Name,ETM.BackGroundColor,ETM.TextColor, isnull(ClassesIncluded,'') as ClassesIncludedIDs    
    , ClassesIncluded,dbo.GetClassNames(isnull(ClassesIncluded,'')) as ClassNames, IsApproved    
    From EventMaster EM left outer join EventTypeMaster ETM on EM.EventTypeID=ETM.ID     
    where IsApproved=1 and EM.SBranchID=@SBranchID and ((datepart(month,StartDate)>=@Month-1 and datepart(month,StartDate)<=@Month+1) and datepart(year,StartDate)=@Year or (CONVERT(datetime,CONVERT(nvarchar(4), @Year)+'-'+CONVERT(nvarchar(4),@Month)+'-01'
)   
  between StartDate and enddate))    
    order by StartDate desc

   select HolidayID,HolidayType,(Case when HolidayType=1 then 'Local Holiday' else 'National Holiday' end) 'HolidayTypeName',    
   Classes 'ClassesIncludedIDs',    
   dbo.GetClassNames(Classes) 'ClassesIncluded',[dbo].[GetEmployeeTypeNames](AssociatedIDs) as EmployeeTypes,    
   (Case When DayDuration=1 then 'Full Day' else (Case when DayDuration=2 then 'First Half' else 'Second Half' end)end) 'DayDurationName',    
   StartDate,EndDate,DATEDIFF(day,StartDate,EndDate)+1 as [Days],DayDuration, [HolidayDescription],Title,[Status],    
   Classes from HolidayMaster where [Status]=1 and SBranchID=@SBranchID    
    order by StartDate desc 
  end    
  else    
  begin    
  if(@ParentID=0)  
  begin  
    Select EventID,Title, [Description], StartDate, EndDate, EventTypeID,    
    ETM.Name,ETM.BackGroundColor,ETM.TextColor, isnull(ClassesIncluded,'') as ClassesIncludedIDs    
    , ClassesIncluded,dbo.GetClassNames(isnull(ClassesIncluded,'')) as ClassNames, IsApproved    
    From EventMaster EM left outer join EventTypeMaster ETM on EM.EventTypeID=ETM.ID     
    where  EM.SBranchID=@SBranchID and ((datepart(month,StartDate)>=@Month-1 and datepart(month,StartDate)<=@Month+1) and datepart(year,StartDate)=@Year   
    or (CONVERT(datetime,CONVERT(nvarchar(4), @Year)+'-'+CONVERT(nvarchar(4),@Month)+'-01') between StartDate and enddate))    
    order by StartDate desc   
  
     select HolidayID,HolidayType,(Case when HolidayType=1 then 'Local Holiday' else 'National Holiday' end) 'HolidayTypeName',    
   Classes 'ClassesIncludedIDs',    
   dbo.GetClassNames(Classes) 'ClassesIncluded',[dbo].[GetEmployeeTypeNames](AssociatedIDs) as EmployeeTypes,    
   (Case When DayDuration=1 then 'Full Day' else (Case when DayDuration=2 then 'First Half' else 'Second Half' end)end) 'DayDurationName',    
   StartDate,EndDate,DATEDIFF(day,StartDate,EndDate)+1 as [Days],DayDuration, [HolidayDescription],Title,[Status],    
   Classes from HolidayMaster where [Status]=1 and SBranchID=@SBranchID     
    order by StartDate desc
  end  
  else  
  Begin  
   declare @SessionID int  
   select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and cast(@Year as nvarchar(4))+'-'+cast(@Month as nvarchar(2))+'-1' between SessionStartDate and SessionEndDate  
   declare @Classes table(ClassID int,SBranchID int)  
   Insert into @Classes   
   select ClassID,SBranchID from Student_Session where StudentID in (select StudentID from StudentMaster where ParentID=@ParentID) and SessionID=@SessionID  
    
   Select EventID,Title, [Description], StartDate, EndDate, EventTypeID,    
     ETM.Name,ETM.BackGroundColor,ETM.TextColor, isnull(ClassesIncluded,'') as ClassesIncludedIDs    
     , ClassesIncluded,dbo.GetClassNames(isnull(ClassesIncluded,'')) as ClassNames, IsApproved    
     From EventMaster EM left outer join EventTypeMaster ETM on EM.EventTypeID=ETM.ID     
     where IsApproved=1 and (EM. SBranchID in (Select SBranchID from @Classes) and ClassesIncluded='0')  
   or (Select count(*) from [dbo].[SplitStringToTable](ClassesIncluded,',') where Item in(Select ClassID from @Classes))>0  
   order by StartDate desc  
  
    select HolidayID,HolidayType,(Case when HolidayType=1 then 'Local Holiday' else 'National Holiday' end) 'HolidayTypeName',    
    Classes 'ClassesIncludedIDs',    
    dbo.GetClassNames(Classes) 'ClassesIncluded',[dbo].[GetEmployeeTypeNames](AssociatedIDs) as EmployeeTypes,    
    (Case When DayDuration=1 then 'Full Day' else (Case when DayDuration=2 then 'First Half' else 'Second Half' end)end) 'DayDurationName',    
    StartDate,EndDate,DATEDIFF(day,StartDate,EndDate)+1 as [Days],DayDuration, [HolidayDescription],Title,[Status],    
    Classes from HolidayMaster where [Status]=1 and (SBranchID in (Select SBranchID from @Classes) and Classes=0)  
   or (Select count(*) from [dbo].[SplitStringToTable](Classes,',') where Item in(Select ClassID from @Classes))>0  
   order by StartDate desc  
  End  
  end    
    
END    

GO

  
Alter procedure dbo.sp_GetStudentOnlineExams  
(    
@StudentID int,    
@SubjectID int    
)    
AS    
BEGIN    
 Declare @ClassID int,@SectionID int,@GroupID int,@SessionID int,@StudentSessionUID int    
 Select @ClassID=ClassID,@SectionID=SectionID,@SessionID=SessionID,@StudentSessionUID=StudentSessionUID from Student_Session where StudentID=@StudentID and Status=1    
 select @GroupID=GroupID from Class_Sections where ID=@SectionID    
    
 Declare @Subjects Table(SubjectID int,SubjectName nvarchar(50))    
 insert into @Subjects select SubjectID,SubjectName from SubjectMaster where GroupID=@GroupID     
 and (IsOptionalSubject=0 or (SubjectID in (select OpSubjectID from [Student_Session_OptionalSubjects] where StudentSessionID=@StudentSessionUID) 
 or (select count(*) from [Student_Session_OptionalSubjects] where StudentSessionID=@StudentSessionUID)=0))    
    
 if(@SubjectID=0)    
 begin    
  Select top 1 @SubjectID=SubjectID from @Subjects    
 end    
    
  Select OX.*,    
  (Select count(*) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as QuestionCount,    
  (Select sum(Marks) from [dbo].[OnlineExamQuestions] OQ where OQ.OXID=OX.OExamID) as TotalMarks,    
  OXM.SubmissionID,EM.EmployeeName as TeacherName,    
  (Select sum(MarksGiven) from OnlineExamSubmissionAnswers OXS where OXS.SubmissionID=OXM.SubmissionID) as MarksObtained    
  from OnlineExamMaster OX left outer join OnlineExamSubmissionMaster OXM on OXM.OExamID=OX.OExamID and OXM.StudentID=@StudentID  
  left outer join EmployeeMaster EM on EM.EmployeeID=OX.TeacherID    
  where SubjectID=@SubjectID and SectionID=@SectionID and SessionID=@SessionID and OX.Status=1    
      
  select SubjectID as ID,SubjectName as Name from @Subjects    
  select isnull(@SubjectID,0)    
END  
GO
Alter Procedure dbo.sp_GetSubjectsForStudent
(  
@StudentID int  
)  
AS  
BEGIN  
 Declare @ClassID int,@SectionID int,@GroupID int,@SessionID int,@StudentSessionUID int    
 Select @ClassID=ClassID,@SectionID=SectionID,@SessionID=SessionID,@StudentSessionUID=StudentSessionUID 
 from Student_Session where StudentID=@StudentID and Status=1    
 select @GroupID=GroupID from Class_Sections where ID=@SectionID    
    
 Declare @Subjects Table(SubjectID int,SubjectName nvarchar(50))    
 insert into @Subjects select SubjectID,SubjectName from SubjectMaster where GroupID=@GroupID     
 and (IsOptionalSubject=0 or (SubjectID in (select OpSubjectID from [Student_Session_OptionalSubjects] where StudentSessionID=@StudentSessionUID) 
 or (select count(*) from [Student_Session_OptionalSubjects] where StudentSessionID=@StudentSessionUID)=0))  
 
 select SubjectID as ID,SubjectName  as Name from @Subjects

END   
GO
Alter table SBranchMaster
Add OnlineClassURL nvarchar(50)
GO
Update SBranchMaster set OnlineClassURL='https://meet.stridetechindia.com/' 
GO
Create procedure dbo.sp_GetSBranchesOnlineClassURL
AS
Begin
	select SBranchID as ID,OnlineClassURL as Name from SBranchMaster
End
GO
Alter proc [dbo].[sp_GetEventCalendar]
(      
@SBranchID int,      
@Month int,      
@Year int,      
@Status int=0  ,    
@ParentID int=0    
)      
AS      
BEGIN      
      
  if(@Status=0)      
  begin      
    Select EventID,Title, [Description], StartDate, EndDate, EventTypeID,      
    ETM.Name,ETM.BackGroundColor,ETM.TextColor, isnull(ClassesIncluded,'') as ClassesIncludedIDs      
    , ClassesIncluded,dbo.GetClassNames(isnull(ClassesIncluded,'')) as ClassNames, IsApproved      
    From EventMaster EM left outer join EventTypeMaster ETM on EM.EventTypeID=ETM.ID       
    where IsApproved=1 and EM.SBranchID=@SBranchID 
	--and ((datepart(month,StartDate)>=@Month-1 and datepart(month,StartDate)<=@Month+1) and datepart(year,StartDate)=@Year or 
	--(CONVERT(datetime,CONVERT(nvarchar(4), @Year)+'-'+CONVERT(nvarchar(4),@Month)+'-01') between StartDate and enddate))      
    order by StartDate desc  
  
   select HolidayID,HolidayType,(Case when HolidayType=1 then 'Local Holiday' else 'National Holiday' end) 'HolidayTypeName',      
   Classes 'ClassesIncludedIDs',      
   dbo.GetClassNames(Classes) 'ClassesIncluded',[dbo].[GetEmployeeTypeNames](AssociatedIDs) as EmployeeTypes,      
   (Case When DayDuration=1 then 'Full Day' else (Case when DayDuration=2 then 'First Half' else 'Second Half' end)end) 'DayDurationName',      
   StartDate,EndDate,DATEDIFF(day,StartDate,EndDate)+1 as [Days],DayDuration, [HolidayDescription],Title,[Status],      
   Classes from HolidayMaster where [Status]=1 and SBranchID=@SBranchID      
    order by StartDate desc   
  end      
  else      
  begin      
  if(@ParentID=0)    
  begin    
    Select EventID,Title, [Description], StartDate, EndDate, EventTypeID,      
    ETM.Name,ETM.BackGroundColor,ETM.TextColor, isnull(ClassesIncluded,'') as ClassesIncludedIDs      
    , ClassesIncluded,dbo.GetClassNames(isnull(ClassesIncluded,'')) as ClassNames, IsApproved      
    From EventMaster EM left outer join EventTypeMaster ETM on EM.EventTypeID=ETM.ID       
    where  EM.SBranchID=@SBranchID and ((datepart(month,StartDate)>=@Month-1 and datepart(month,StartDate)<=@Month+1) and datepart(year,StartDate)=@Year     
    or (CONVERT(datetime,CONVERT(nvarchar(4), @Year)+'-'+CONVERT(nvarchar(4),@Month)+'-01') between StartDate and enddate))      
    order by StartDate desc     
    
     select HolidayID,HolidayType,(Case when HolidayType=1 then 'Local Holiday' else 'National Holiday' end) 'HolidayTypeName',      
   Classes 'ClassesIncludedIDs',      
   dbo.GetClassNames(Classes) 'ClassesIncluded',[dbo].[GetEmployeeTypeNames](AssociatedIDs) as EmployeeTypes,      
   (Case When DayDuration=1 then 'Full Day' else (Case when DayDuration=2 then 'First Half' else 'Second Half' end)end) 'DayDurationName',      
   StartDate,EndDate,DATEDIFF(day,StartDate,EndDate)+1 as [Days],DayDuration, [HolidayDescription],Title,[Status],      
   Classes from HolidayMaster where [Status]=1 and SBranchID=@SBranchID       
    order by StartDate desc  
  end    
  else    
  Begin    
   declare @SessionID int    
   select @SessionID=SessionID from SessionMaster where SBranchID=@SBranchID and cast(@Year as nvarchar(4))+'-'+cast(@Month as nvarchar(2))+'-1' between SessionStartDate and SessionEndDate    
   declare @Classes table(ClassID int,SBranchID int)    
   Insert into @Classes     
   select ClassID,SBranchID from Student_Session where StudentID in 
   (select StudentID from StudentMaster where ParentID=@ParentID) and SessionID=@SessionID    
      
   Select EventID,Title, [Description], StartDate, EndDate, EventTypeID,      
     ETM.Name,ETM.BackGroundColor,ETM.TextColor, isnull(ClassesIncluded,'') as ClassesIncludedIDs      
     , ClassesIncluded,dbo.GetClassNames(isnull(ClassesIncluded,'')) as ClassNames, IsApproved      
     From EventMaster EM left outer join EventTypeMaster ETM on EM.EventTypeID=ETM.ID       
     where IsApproved=1 and (EM.SBranchID in (Select SBranchID from @Classes) and ClassesIncluded='0')    
   or (Select count(*) from [dbo].[SplitStringToTable](ClassesIncluded,',') where Item in(Select ClassID from @Classes))>0    
   order by StartDate desc    
    

    select HolidayID,HolidayType,(Case when HolidayType=1 then 'Local Holiday' else 'National Holiday' end) 'HolidayTypeName',      
    Classes 'ClassesIncludedIDs',      
    dbo.GetClassNames(Classes) 'ClassesIncluded',[dbo].[GetEmployeeTypeNames](AssociatedIDs) as EmployeeTypes,      
    (Case When DayDuration=1 then 'Full Day' else (Case when DayDuration=2 then 'First Half' else 'Second Half' end)end) 'DayDurationName',      
    StartDate,EndDate,DATEDIFF(day,StartDate,EndDate)+1 as [Days],DayDuration, [HolidayDescription],Title,[Status],      
    Classes from HolidayMaster where [Status]=1 and (SBranchID in (Select SBranchID from @Classes) and Classes='0')    
   or (Select count(*) from [dbo].[SplitStringToTable](Classes,',') where Item in(Select ClassID from @Classes))>0    
   order by StartDate desc    
  End    
  end      
      
END 
GO
Create procedure [dbo].[spn_GetRecieverListForOnlineClass]   
(    
@OCID int   
)    
as     
begin      
 declare @SectionID int, @SessionID int,@SBranchID int  
  
 select @SectionID=SectionID,@SessionID=SessionID,@SBranchID=SBranchID from OnlineClassMaster where OCID=@OCID  
  
   select ParentID as ID,FatherName as Name,FatherMobileNo as MobileNo, 4 as RecieverType, AU.FCMToken as deviceToken   
   from ParentMaster R left outer join AppUsers AU on R.ParentID=AU.UserID and AU.UserType=4    
   where ParentID in (Select ParentID from StudentMaster where StudentID in  
   (select StudentID from Student_Session where SectionID=@SectionID and SessionID=@SessionID)) and isnull(AU.FCMToken,'')!=''  
     union 
	select UserID as ID,'Principal' as Name,'' as MobileNo,8 as RecieverType,AU.FCMToken as deviceToken
	from AppUsers AU where UserType=8 and SBranchID=@SBranchID and Status=1
end    
GO
CREATE procedure dbo.sp_GetPrincipalOnlineClass    
(      
@SbranchID int,     
@CurDate date    
)      
AS      
BEGIN      
  
 select OCM.OCID,OCM.Status,OCM.MeetingID,OCM.ClassDate,OCM.StartTime,OCM.EndTime,OCM.StartedOn,  
 (select CM.ClassName+'/'+CM.SectionName  from v_ClassSectionNames CM where CM.SectionID=OCM.SectionID) as ClassSection, OCM.SectionID,      
 SM.SubjectName,OCM.SubjectID,EM.EmployeeName as TeacherName      
 from OnlineClassMaster OCM left outer join EmployeeMaster EM on EM.EmployeeID=OCM.TeacherID      
 left outer join SubjectMasterAll SM on SM.SubjectID=OCM.SubjectID   
 where OCM.ClassDate>=@CurDate and OCM.SbranchID=@SbranchID  
 order by (case when OCM.Status=1 then -1 else 0 end),  ClassDate,CAST(StartTime AS DATETIME)   desc
END 
GO
Alter proc [dbo].[sp_GetTeacherTeachingClassSections]  
(      
@TeacherID int      
)      
AS      
BEGIN      
 declare @Classes table(ClassID int,SubjectID int,SectionID int)          
  Insert into @Classes           
 select ClassID,SubjectID,SectionID from dbo.TeacherClassSectionSubjects(@TeacherID)     
    
 select CM.ClassID,CM.ClassName +'->'+isnull(t.SectionName,'') as ClassName, t.SectionID,t.SectionName from      
 (select ClassID, ID as SectionID,Name as SectionName from Class_Sections where ID in (Select SectionID from @Classes))t      
  left outer join ClassMaster CM on t.ClassID=CM.ClassID      
END 
GO
Alter procedure dbo.sp_GetStudentOnlineClass 
(  
@StudentID int,  
@CurDate datetime  
)  
AS  
BEGIN  
 declare @ClassID int, @SectionID int,@StudentSessionUID int,@SessionID int  
 select top 1 @SectionID=SectionID,@ClassID=ClassID,@StudentSessionUID=StudentSessionUID ,@SessionID=SessionID  
 from Student_Session where studentID=@StudentID order by StudentSessionUID desc   
  
 select OCM.OCID,OCM.Status,OCM.MeetingID,OCM.ClassDate,OCM.StartTime,OCM.EndTime,OCM.StartedOn,OCM.SectionID,  
 SM.SubjectName,OCM.SubjectID,EM.EmployeeName as TeacherName  
 from OnlineClassMaster OCM left outer join EmployeeMaster EM on EM.EmployeeID=OCM.TeacherID  
 left outer join SubjectMaster SM on SM.SubjectID=OCM.SubjectID  
 where OCM.ClassDate>=cast(@CurDate as date) and SectionID=@SectionID and SessionID=@SessionID  
 order by ClassDate,CAST(StartTime AS DATETIME)   
END  
GO
Alter procedure dbo.sp_JoinStudentOnlineClass        
(        
@StudentID int,        
@JoinDate datetime,        
@OCID int  ,      
@ParentID int=0      
)        
AS        
BEGIN        
    
  declare @SectionID int, @SessionID int,@SBranchID int ,@MeetingID nvarchar(100),@SubjectID int,@UpdatedRows int,@SubjectName nvarchar(50)    
  select @SectionID=SectionID,@SessionID=SessionID,@SBranchID=SBranchID,@MeetingID=MeetingID,@SubjectID=SubjectID from OnlineClassMaster where OCID=@OCID      
  if(@StudentID=0)      
  begin      
   select top 1 @StudentID=StudentID from Student_Session where SectionID=@SectionID and SessionID=@SessionID       
   and StudentID in (Select StudentID from StudentMaster where ParentID=@ParentID)      
  end      
  Insert into OnlineClassAttendees(OCID,StudentID,StartTime)        
  values(@OCID,@StudentID,cast(cast(@JoinDate as time) as nvarchar(12)))        
  select @UpdatedRows= @@rowcount    
  select @SubjectName=SubjectName from SubjectMaster where SubjectID=@SubjectID    
     
 Select Name,'/Images/StudentImage/'+Cast(StudentID as nvarchar(10))+'_'+Photo as Photo,@MeetingID as MeetingID,@SubjectName as SubjectName    
 from StudentMaster where StudentID=@StudentID    
END 
GO
Alter function [dbo].[TeacherClassSectionSubjects](@TeacherID int=0)  
Returns @Classes table(ClassID int,SubjectID int,SectionID int,DayID int,PeriodID int,TeacherID int,IsMerger int)     
AS  
BEGIN  
 if(@TeacherID=0)  
 begin  
  Insert into @Classes    
 select ClassID,MondaySubjectID as SubjectID,SectionID,1 as DayID,PeriodID,MondayTeacherID,0 from Time_Table_Master  where isnull(MondayTeacherID,0)!=0  
  Union      
  select ClassID,TuesdaySubjectID as SubjectID,SectionID,2 as DayID,PeriodID,TuesdayTeacherID,0 from Time_Table_Master    where isnull(TuesdayTeacherID,0)!=0  
  Union      
  select ClassID,WednesdaySubjectID as SubjectID,SectionID,3 as DayID,PeriodID,WednesdayTeacherID,0 from Time_Table_Master where isnull(wednesdayTeacherID,0)!=0  
  Union      
  select ClassID,ThursdaySubjectID as SubjectID,SectionID,4 as DayID,PeriodID,ThursdayTeacherID,0from Time_Table_Master where isnull(ThursdayTeacherID,0)!=0  
  Union      
  select ClassID,FridaySubjectID as SubjectID,SectionID,5 as DayID,PeriodID,FridayTeacherID,0 from Time_Table_Master where isnull(FridayTeacherID,0)!=0      
  Union      
  select ClassID,SaturdaySubjectID as SubjectID,SectionID,6 as DayID,PeriodID,SaturdayTeacherID,0 from Time_Table_Master where isnull(SaturdayTeacherID,0)!=0  
    end  
 else  
 begin  
 Insert into @Classes    
 select ClassID,MondaySubjectID as SubjectID,SectionID,1 as DayID,PeriodID,MondayTeacherID,0 from Time_Table_Master  where isnull(MondayTeacherID,0)=@TeacherID  
  Union      
  select ClassID,TuesdaySubjectID as SubjectID,SectionID,2 as DayID,PeriodID,TuesdayTeacherID,0 from Time_Table_Master    where isnull(TuesdayTeacherID,0)=@TeacherID  
  Union      
  select ClassID,WednesdaySubjectID as SubjectID,SectionID,3 as DayID,PeriodID,WednesdayTeacherID,0 from Time_Table_Master where isnull(wednesdayTeacherID,0)=@TeacherID  
  Union      
  select ClassID,ThursdaySubjectID as SubjectID,SectionID,4 as DayID,PeriodID,ThursdayTeacherID,0from Time_Table_Master where isnull(ThursdayTeacherID,0)=@TeacherID  
  Union      
  select ClassID,FridaySubjectID as SubjectID,SectionID,5 as DayID,PeriodID,FridayTeacherID,0 from Time_Table_Master where isnull(FridayTeacherID,0)=@TeacherID      
  Union      
  select ClassID,SaturdaySubjectID as SubjectID,SectionID,6 as DayID,PeriodID,SaturdayTeacherID,0 from Time_Table_Master where isnull(SaturdayTeacherID,0)=@TeacherID  
  
 end  
  
 insert into @Classes  
 select CS.ClassID,CMM.SubjectID,CMM.SecondSectionID,CMM.DayID,CMM.PeriodID,CL.TeacherID,1  
 from [dbo].[Class_Merge_Master] CMM left outer join Class_Sections CS on CMM.SecondSectionID=CS.ID  
 left outer join @Classes CL on CL.SectionID=CMM.FirstSectionID and CL.PeriodID=CMM.PeriodID and CL.DayID=CMM.DayID  
 where CL.ClassID is not null and isnull(CL.TeacherID,0)!=0  
   
 Delete from @Classes where SubjectID not in (select SubjectID from SubjectMasterAll)
 Delete from @Classes where ClassID not in (select ClassID from ClassMaster)
 Return  
END  
GO
Alter procedure dbo.sp_GetTeacherQuestionBank
(      
@TeacherID int,      
@ClassID int,      
@QuestionsBy int      
)      
AS      
BEGIN      
 declare @Classes table(ClassID int,SubjectID int,SectionID int)      
 Insert into @Classes        
 select ClassID,SubjectID,SectionID from dbo.TeacherClassSectionSubjects(@TeacherID)  
  
 if(@ClassID=0)      
 begin      
  Select @ClassID=min(ClassID) from @Classes      
 end      
 if(@QuestionsBy=0)      
 begin      
  Select QuestionID,TeacherID,ClassID,QB.GroupID,QB.SubjectID,ChapterID,TopicID,QuestionType,Complexity,QuestionText,QuestionImage,Option1,Option1Image,Option2,Option2Image,      
  Option3,Option3Image,Option4,Option4Image,Answer,Explaination,ExplainationImage ,SM.SubjectName+' ('+GM.GroupName+')' as SubjectName  
  From QuestionBankMaster QB left outer join SubjectMaster SM on SM.SubjectID=QB.SubjectID    
  left outer join GroupMaster GM on GM.GroupID=SM.GroupID
  where TeacherID=@TeacherID and ClassID=@ClassID      
 end      
 else      
 begin      
 Select QuestionID,TeacherID,EM.EmployeeName as TeacherName,ClassID,QB.GroupID,QB.SubjectID,ChapterID,TopicID,QuestionType,Complexity,    
  QuestionText,QuestionImage,Option1,Option1Image,Option2,Option2Image,      
  Option3,Option3Image,Option4,Option4Image,Answer,Explaination,ExplainationImage,QB.SBranchID,QB.CreatedDate ,SM.SubjectName+' ('+GM.GroupName+')' as SubjectName      
  From QuestionBankMaster QB left outer join SubjectMaster SM on SM.SubjectID=QB.SubjectID      
  left outer join EmployeeMaster EM on EM.EmployeeID=QB.TeacherID      
  left outer join GroupMaster GM on GM.GroupID=SM.GroupID
  where ClassID=@ClassID       
 end      
      
 select ClassID as ID,ClassName as Name from ClassMaster where ClassID in (select ClassID from @Classes) and Status=1      
 select @ClassID      
      
 select distinct SM.SubjectID as ID,SubjectName+' ('+GM.GroupName+')' as Name,C.ClassID as Extra1,C.SectionID from @Classes C 
 left outer join SubjectMaster SM on SM.SubjectID=C.SubjectID 
 left outer join GroupMaster GM on GM.GroupID=SM.GroupID
 where SM.SubjectID is not null      
END  
GO

ALTER proc [dbo].[sp_InsertUpdateEventType]  
(  
@ID int,  
@Name nvarchar(50),  
@BackGroundColor nvarchar(50),  
@TextColor nvarchar(50),  
@OpType int  ,
@SBranchID int=0
)  
AS  
BEGIN  
 if(@OpType=-1)  
 begin  
  delete from EventTypeMaster where ID=@ID  
 end  
 else if(@ID=0)  
 begin  
  Insert into [dbo].[EventTypeMaster](Name,BackGroundColor,TextColor,SBranchID)  
  values(@Name,@BackGroundColor,@TextColor,@SBranchID)  
 end  
 else  
 begin  
  Update [dbo].[EventTypeMaster] set Name=@Name,BackGroundColor=@BackGroundColor,TextColor=@TextColor  
  where ID=@ID  
 end  
END  
GO

Alter procedure [dbo].[sp_GetLeavesOnEmployee]  
(  
@EmployeeID int  
)  
as   
begin   
  Select LeaveID,LeaveType,StartDate,EndDate,LeaveReason,  isnull(LTM.LeaveTypeName,'LWP') as LeaveTypeName,
  (case when EmployeeType in (-1,-2) then 'TRA' else 'EMP' end)+RIGHT(REPLICATE('0',6)+CAST(ApplicantID AS VARCHAR(6)),6) as [EmployeeSID],IsApproved ,  
  (case when IsApproved=0 then 'Rejected' when IsApproved=1 then 'Approved' else 'Pending' end) as StatusText ,  
  (case when IsApproved=0 then '#ed553b' when IsApproved=1 then '#3caea3' else '#f6d55c' end) as StatusColor  
  from LeaveMaster LM left outer join LeaveTypeMaster LTM on LTM.LeaveTypeID=LM.LeaveTypeApplied where ApplicantType=0 and ApplicantID=@EmployeeID  
  order by StartDate desc  
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
 Select LeaveID,ApplicantType,LeaveType,StartDate,EndDate,LeaveReason,ApplicantID,isnull(LTM.LeaveTypeName,'LWP') as LeaveTypeName,   
 (Case when ApplicantType=0 then (Select '('+EmployeeSID+')'+EmployeeName from v_EmployeeDriversBasicDetails EDB where EDB.EmployeeID=LM.ApplicantID and EDB.EmployeeType=LM.EmployeeType)   
 else (Select Name from StudentMaster where StudentID=ApplicantID) end) as 'ApplicantName',IsApproved,EmployeeType  
 from LeaveMaster LM left outer join LeaveTypeMaster LTM on LTM.LeaveTypeID=LM.LeaveTypeApplied
 where IsApproved=isnull(nullif(@Status,-1),IsApproved) and ApplicantType=isnull(nullif(@ApplicantType,-1),ApplicantType) and LM.SBranchID=@SBranchID  
 order by StartDate desc    
end  
GO

-----------------------------------------------------------------------------------------------------------------------------------------------------------------
--Online Staff Meeting Related
-----------------------------------------------------------------------------------------------------------------------------------------------------------------
CREATE TABLE [dbo].[OnlineStaffMeetings](
	[MeetingID] [int] IDENTITY(1,1) NOT NULL,
	[MeetingDate] [date] NULL,
	[StartTime] [nvarchar](50) NULL,
	[EndTime] [nvarchar](50) NULL,
	[MeetingTitle] [nvarchar](50) NULL,
	[Status] [int] NULL,
	[SBranchID] [int] NULL
) ON [PRIMARY]
GO


Create procedure dbo.sp_AddStaffMeeting
(
@MeetingDate date,
@StartTime nvarchar(50),
@EndTime nvarchar(50),
@MeetingTitle nvarchar(50),
@SBranchID int
)
AS
BEGIN
	Insert into [dbo].[OnlineStaffMeetings](MeetingDate,StartTime,EndTime,MeetingTitle,Status,SBranchID)
		values(@MeetingDate,@StartTime,@EndTime,@MeetingTitle,0,@SBranchID)
		select Cast(Scope_Identity() as int)
END
GO

Create procedure dbo.sp_UpdateStaffMeeting
(
@MeetingID int,
@Status int
)
AS
BEGIN
	Update [dbo].[OnlineStaffMeetings] set Status=@Status
	where MeetingID=@MeetingID
	 select @@rowcount
END
GO

Create procedure dbo.sp_GetStaffMeetings
(
@CurDate date,
@SBranchID int
)
AS
BEGIN
	Select * from [dbo].[OnlineStaffMeetings] where MeetingDate=@CurDate and SBranchID=@SBranchID
END
GO
CREATE procedure [dbo].[spn_GetRecieverListForStaffMeeting]     
(      
@MeetingID int     
)      
as       
begin        
   select EmployeeID as ID,EmployeeName as Name,MobileNumber as MobileNo, 3 as RecieverType, AU.FCMToken as deviceToken     
   from EmployeeMaster R left outer join AppUsers AU on R.EmployeeID=AU.UserID and AU.UserType=3      
   where EmployeeType=3 and isnull(AU.FCMToken,'')!=''         
end     

-------------------------------------------------------------------------------------------------------------------------------------------------------------------

Drop Procedure [dbo].[spn_UpdateSessionNoFeeMonths] 
GO
Drop Procedure dbo.sp_StudentSubmitOnlineAnswerSheet  
GO
Drop Procedure dbo.sp_UpdateOnlineExam  
GO
Drop Procedure dbo.sp_UpdateAdminYouTubeVideo
GO
Drop Procedure	spn_InsertUpdateStudentSessionDetails
GO
Drop Procedure sp_UpdateRemarkOnStudentOnlineExamSubmission
GO
Drop Procedure [dbo].[spn_InsertUpdateStudentSessionDetailsMini]  
GO
Drop Type  [dbo].[ut_Name_ID_Utility]
GO
CREATE TYPE [dbo].[ut_Name_ID_Utility] AS TABLE(
	[ID] [int] NULL,
	[Name] [nvarchar](max) NULL,
	[Extra1] [nvarchar](max) NULL,
	[Extra2] [nvarchar](max) NULL,
	[Extra3] [nvarchar](max) NULL
)
GO
create procedure [dbo].[spn_UpdateSessionNoFeeMonths]  
(  
@SessionID int=1,  
@SBranchID int,  
@NoFeeMonths ut_Name_ID_Utility READONLY  
)  
as   
begin   
 Delete from [dbo].SessionClassNoFeeMonths where SessionID=@SessionID and SBranchID=@SBranchID  
  
 INSERT INTO [dbo].SessionClassNoFeeMonths(SessionID,ClassID,Month,SBranchID) select @SessionID,ID,Name,@SBranchID from @NoFeeMonths  
end  
GO
Create procedure dbo.sp_StudentSubmitOnlineAnswerSheet  
(  
@OExamID int,  
@SubmissionDate datetime,  
@StudentID int,  
@Answers ut_Name_ID_Utility READONLY  
)  
AS  
BEGIN  
   
 declare @SubmissionID int  
 select @SubmissionID=SubmissionID from OnlineExamSubmissionMaster where StudentID=@StudentID and OExamID=@OExamID  
 if(isnull(@SubmissionID,0)=0)  
 begin   
  insert into OnlineExamSubmissionMaster(OExamID,SubmissionDate,StudentID,Status)values(@OExamID,@SubmissionDate,@StudentID,0)  
  select @SubmissionID=cast(Scope_Identity() as int)  
  insert into OnlineExamSubmissionAnswers(SubmissionID,OXQID,Answer) select @SubmissionID,ID,Name from  @Answers   
 end  
 select @SubmissionID  
END  
GO
  
Create procedure dbo.sp_UpdateOnlineExam  
(  
@OExamID int,  
@OExamStartDate datetime,  
@OExamEndDate datetime,  
@SessionID int,  
@ClassID int,  
@SectionID int,  
@SubjectID int,  
@TeacherID int,  
@CreatedDate datetime,  
@Status int,  
@ExamTitle nvarchar(50),  
@ExamDescription nvarchar(max),  
@ExamPriority int,  
@SBranchID int,  
@Questions ut_Name_ID_Utility READONLY,  
@OpType int  
)  
AS  
BEGIN  
 if(@OpType=-1)  
 begin  
  Delete from OnlineExamMaster where OExamID=@OExamID  
  Delete from OnlineExamQuestions where OXID=@OExamID  
 end  
 Else  
 Begin  
  if(@OExamID=0)  
  begin  
   Insert into dbo.OnlineExamMaster(OExamStartDate,OExamEndDate,SessionID,ClassID,SectionID,SubjectID,TeacherID,CreatedDate,Status,ExamTitle,ExamDescription,ExamPriority,SBranchID)  
   values(@OExamStartDate,@OExamEndDate,@SessionID,@ClassID,@SectionID,@SubjectID,@TeacherID,@CreatedDate,@Status,@ExamTitle,@ExamDescription,@ExamPriority,@SBranchID)  
   select @OExamID=Cast(Scope_Identity() as int)  
  end  
  else  
  begin  
   Update dbo.OnlineExamMaster set OExamStartDate=@OExamStartDate,OExamEndDate=@OExamEndDate,SessionID=@SessionID,ClassID=@ClassID,SectionID=@SectionID  
   ,SubjectID=@SubjectID,TeacherID=@TeacherID,Status=@Status,ExamTitle=@ExamTitle,ExamDescription=@ExamDescription,ExamPriority=@ExamPriority  
   where OExamID=@OExamID  
  end  
  
  Delete from OnlineExamQuestions where OXID=@OExamID and OXQID in (select ID from @Questions where Extra3=-1)  
  insert into OnlineExamQuestions(OXID,QuestionID,Marks) select @OExamID,Name,Extra1 from @Questions where Extra3!=-1 and ID=0  
 End  
END  
GO
  
Create Procedure [dbo].[sp_UpdateAdminYouTubeVideo]  
(  
@VideoID int,  
@TeacherID int,  
@UploadDate datetime,  
@SubjectID int,  
@Title nvarchar(1000),  
@Description nvarchar(max),  
@YoutubeID nvarchar(50),  
@ClassID int,  
@SectionID int,  
@SequenceNo int,  
@SBranchID int,  
@VideoDate datetime,  
@SessionID int,  
@IsSpecificStudent int,  
@Students ut_Name_ID_Utility READONLY,  
@OpType int  
)  
AS  
BEGIN  
 if(@OpType=-1)  
 begin  
  Delete from [dbo].[YouTubeVideos] where VideoID=@VideoID  
  Delete from [dbo].[YouTubeVideo_Students] where VideoID=@VideoID  
 end  
 else  
 begin  
  if(@VideoID=0)  
  begin  
  Insert into [dbo].[YouTubeVideos](TeacherID,UploadDate,SubjectID,Title,Description,YouTubeID,ClassID,SectionID,SequenceNo,VideoDate,SBranchID,SessionID,IsSpecificStudent)  
   values(@TeacherID,@UploadDate,@SubjectID,@Title,@Description,@YouTubeID,@ClassID,@SectionID,@SequenceNo,@VideoDate,@SBranchID,@SessionID,@IsSpecificStudent)  
   select @VideoID=Cast(Scope_Identity() as int)  
    
   if(@IsSpecificStudent=1)  
   begin  
    insert into [dbo].[YouTubeVideo_Students]   
    select @VideoID,ID from @Students  
   end  
  end  
  else  
  begin  
   Update [dbo].[YouTubeVideos] set TeacherID=@TeacherID,UploadDate=@UploadDate,SubjectID=@SubjectID,Title=@Title,Description=@Description  
   ,YouTubeID=@YouTubeID,ClassID=@ClassID,SectionID=@SectionID,SequenceNo=@SequenceNo,VideoDate=@VideoDate,SBranchID=@SBranchID,  
   SessionID=@SessionID,IsSpecificStudent=@IsSpecificStudent  
   where VideoID=@VideoID  
  
   delete from [dbo].[YouTubeVideo_Students]  where VideoID=@VideoID   
   if(@IsSpecificStudent=1)  
   begin  
    insert into [dbo].[YouTubeVideo_Students]   
    select @VideoID,ID from @Students  
   end  
  end  
 end  
 select @VideoID  
END  
GO
  
Create procedure dbo.sp_UpdateRemarkOnStudentOnlineExamSubmission  
(  
@SubmissionID int,  
@CheckDate datetime,  
@TeacherID int,  
@Status int,  
@TeacherRemark nvarchar(max),  
@AnswerRemarks ut_Name_ID_Utility READONLY  
)  
AS  
BEGIN  
 update OnlineExamSubmissionMaster set TeacherRemark=@TeacherRemark,TeacherID=@TeacherID, CheckDate=@CheckDate,Status=@Status where SubmissionID=@SubmissionID  
  
 UPDATE e SET e.MarksGiven=d.Name,e.Remark=d.Extra1  
 FROM  OnlineExamSubmissionAnswers e, @AnswerRemarks d   
 WHERE e.SubAnsID=d.ID  
END  
GO
  
CREATE procedure [dbo].[spn_InsertUpdateStudentSessionDetails]  
(  
@StudentSessionUID int,  
@StudentID int,  
@ClassID int,  
@SectionID int,  
@FromDate date,  
@ToDate date,  
@Status int,  
@RollNo nvarchar(50),  
@SBranchID int,  
@QuotaID int,  
@FeePaymentMode int,  
@SessionID int=1,  
@HouseID int=1,  
@OptedSubjects ut_Name_ID_Utility READONLY,  
@IsAdmissionFeeApplicable int  
)  
as   
begin   
 if(@StudentSessionUID=0)  
 begin  
  if(@Status=1)  
  begin  
   Update [dbo].[Student_Session] set [Status]=0 where StudentID=@StudentID and SBranchID=@SBranchID  
  end  
  insert into [dbo].[Student_Session]  
  (StudentID,ClassID,SectionID,FromDate,ToDate,[Status],RollNo,SBranchID,QuotaID,FeePaymentMode,SessionID,HouseID,IsAdmissionFeeApplicable)   
  values(@StudentID,@ClassID,@SectionID,@FromDate,@ToDate,@Status,@RollNo,@SBranchID,@QuotaID,@FeePaymentMode,@SessionID,@HouseID,@IsAdmissionFeeApplicable)  
  select @StudentSessionUID=CAST(SCOPE_IDENTITY() as int)  
   select @StudentID  
    end  
 else  
 begin  
  if(@Status=1)  
  begin  
   Update [dbo].[Student_Session] set [Status]=0 where StudentID=@StudentID and SBranchID=@SBranchID  
  end  
  Update [dbo].[Student_Session]  
  set   ClassID=@ClassID, SectionID=@SectionID, HouseID=@HouseID,  
  FromDate=@FromDate, ToDate=@ToDate, [Status]=@Status, RollNo=@RollNo, SBranchID=@SBranchID, QuotaID=@QuotaID ,  
  FeePaymentMode=@FeePaymentMode,SessionID=@SessionID,IsAdmissionFeeApplicable=@IsAdmissionFeeApplicable  
  where  SBranchID=@SBranchID and StudentSessionUID=@StudentSessionUID and StudentID=@StudentID  
  select @StudentID  
 end  
  
 Delete from [dbo].[Student_Session_OptionalSubjects] where StudentSessionID=@StudentSessionUID  
  
 INSERT INTO [dbo].[Student_Session_OptionalSubjects](StudentSessionID,OpSubjectID) select @StudentSessionUID,ID from @OptedSubjects where Extra2=1  
end  
GO
CREATE procedure [dbo].[spn_InsertUpdateStudentSessionDetailsMini]  
(  
@StudentSessionUID int,  
@StudentID int,  
@ClassID int,  
@SectionID int,  
@FromDate date,  
@ToDate date,  
@Status int,  
@RollNo nvarchar(50),  
@SBranchID int,  
@QuotaID int,  
@FeePaymentMode int,  
@SessionID int=1,  
@OptedSubjects ut_Name_ID_Utility READONLY,  
@IsAdmissionFeeApplicable int  
)  
as   
begin   
 if(@StudentSessionUID=0)  
 begin  
  if(@Status=1)  
  begin  
   Update [dbo].[Student_Session] set [Status]=0 where StudentID=@StudentID and SBranchID=@SBranchID  
  end  
  insert into [dbo].[Student_Session]  
  (StudentID,ClassID,SectionID,FromDate,ToDate,[Status],RollNo,SBranchID,QuotaID,FeePaymentMode,SessionID,IsAdmissionFeeApplicable)   
  values(@StudentID,@ClassID,@SectionID,@FromDate,@ToDate,@Status,@RollNo,@SBranchID,@QuotaID,@FeePaymentMode,@SessionID,@IsAdmissionFeeApplicable)  
  select @StudentSessionUID=CAST(SCOPE_IDENTITY() as int)  
    end  
 else  
 begin  
  if(@Status=1)  
  begin  
   Update [dbo].[Student_Session] set [Status]=0 where StudentID=@StudentID and SBranchID=@SBranchID  
  end  
  Update [dbo].[Student_Session]  
  set   ClassID=@ClassID, SectionID=@SectionID,   
  FromDate=@FromDate, ToDate=@ToDate, [Status]=@Status, RollNo=@RollNo, SBranchID=@SBranchID, QuotaID=@QuotaID ,  
  FeePaymentMode=@FeePaymentMode,SessionID=@SessionID,IsAdmissionFeeApplicable=@IsAdmissionFeeApplicable  
  where  SBranchID=@SBranchID and StudentSessionUID=@StudentSessionUID and StudentID=@StudentID  
 end  
  
 Delete from [dbo].[Student_Session_OptionalSubjects] where StudentSessionID=@StudentSessionUID  
  
 INSERT INTO [dbo].[Student_Session_OptionalSubjects](StudentSessionID,OpSubjectID) select @StudentSessionUID,ID from @OptedSubjects where Extra2=1  
  
  select   
  SS.StudentSessionUID,  SS.StudentID,   SS.ClassID   ,SS.SectionID   ,SS.QuotaID   ,SS.FromDate   ,SS.ToDate  
   ,SS.Status,   SS.RollNo,SS.FeePaymentMode  
   ,(Select QuotaName from QuotaMaster QM where QM.QuotaID=SS.QuotaID) as QuotaName  
   ,(Select ClassName from ClassMaster CM where CM.ClassID=SS.ClassID) as ClassName  
   ,(Select Name from Class_Sections CS where CS.ID=SS.SectionID) as SectionName  
   ,(Select SessionName from SessionMaster CS where CS.SessionID=SS.SessionID) as SessionName  
   from Student_Session SS where SS.StudentID=@StudentID and SS.SBranchID=@SBranchID  
   order by Status desc  
end  
  
GO
alter procedure [dbo].[sp_GetStudentOnlineExamsAnswerSheet]  
(  
@SubmissionID int,  
@OExamID int,  
@StudentID int  
)  
AS  
BEGIN  
   
 select EX.* ,EM.EmployeeName as TeacherName,CM.ClassName,CS.Name as SectionName,  
 SS.SessionName ,SM.SubjectName,OXM.SubmissionID,OXM.SubmissionDate,OXM.TeacherRemark,OXM.CheckDate,OXM.Status as SubmissionStatus  
 from OnlineExamMaster EX left outer join EmployeeMaster EM on EM.EmployeeID=EX.TeacherID  
 left outer join ClassMaster CM on CM.ClassID=EX.ClassID  
 left outer join SubjectMaster SM on SM.SubjectID=EX.SubjectID  
 left outer join Class_Sections CS on CS.ID=EX.SectionID  
 left outer join SessionMaster SS on SS.SessionID=EX.SessionID  
 left outer join OnlineExamSubmissionMaster OXM on OXM.SubmissionID=@SubmissionID  
 where EX.OExamID=@OExamID  
    
  select  OQ.OXQID,OQ.OXID,OQ.QuestionID,OQ.Marks,QB.QuestionText,QB.QuestionType,Option1,Option2,Option3,Option4,QB.Complexity,  
  OXA.Answer as StudentAnswer,OXA.MarksGiven,OXA.Remark as TeacherRemark,QB.Answer,QB.Explaination  
  from [dbo].[OnlineExamQuestions] OQ  
  left outer join [dbo].[QuestionBankMaster] QB on QB.QuestionID=OQ.QuestionID  
  left outer join OnlineExamSubmissionAnswers OXA on OXA.OXQID=OQ.OXQID  
  where OQ.OXID=@OExamID  and OXA.SubmissionID=@SubmissionID
  
END