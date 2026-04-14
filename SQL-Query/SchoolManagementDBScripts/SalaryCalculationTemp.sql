		Declare @SalaryMonth int=2, @SalaryYear	int=2020,@SBranchID int=1,@EmployeeType int =3,@MonthType int,
		@BaseDate date,@SessionStartDate date,@IsAdmissionFee int=0,@DOJ date,@SalaryMonthDate date
		,@EmployeeID int=2

		Declare @HStartDate date,@HEndDate date,@HDuration int,@Month int,@DaysWorking int=6
		declare @SalarySummery Table(TypeID int,TypeName nvarchar(50),IsDeduction int,AttendanceType int,MinDays int,Amount numeric(10,2))
		declare @HolidayTable Table(DayID int,HStatus numeric(5,2),HDuration int)
		declare @LeaveSummery Table(DayID int,LStatus numeric(10,2),IsSalaryDeduct int,LDuration int,SalaryTypeName nvarchar(50))

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
			select datepart(day,LD.LeaveDate),Applied,isnull(IsSalaryDeduct,1),LeaveType,isnull(LTM.LeaveTypeName,'LWP')
			from LeaveDetails LD left outer join LeaveTypeMaster LTM on LTM.LeaveTypeID=LD.LeaveTypeID
			Left outer join LeaveMaster LM  on LM.LeaveID=LD.LeaveID
			where LM.ApplicantType=0 and LM.ApplicantID=@EmployeeID and LM.IsApproved=1
			and LD.LeaveDate between @SalaryMonthDate and EOMONTH(@SalaryMonthDate) --and isnull(IsSalaryDeduct,1)=0

			
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
		
			select * from @LeaveSummery

			select @MonthDays as MonthDays,@WorkingDays-@PubHoliday as WorkingDays,@WeekOff as WeekOffs,@PubHoliday as PubHolidays,
			@Leaves as Leaves,@LWP as LWP,@PresentDays as Present