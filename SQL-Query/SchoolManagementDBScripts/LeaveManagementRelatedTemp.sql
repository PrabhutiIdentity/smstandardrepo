	declare @StartDate date='2020-02-19',@EndDate date='2020-03-03',@ApplicantID int=2,@LeaveID int=0,@EmployeeType int=1,@SBranchID int=1,@LeaveDuration int=0
	
	Declare @LeaveDetails  TABLE([LeaveTypeID] [int],[Applied] [numeric](10, 2),LeaveDate date)
	
	Declare @HolidayDetails TABLE(StartDate date,EndDate date,DayDuration int)
	Insert into @HolidayDetails
	select StartDate,EndDate,DayDuration from HolidayMaster where IsEmployee=1 
	and (((select count(*) from dbo.SplitStringToTable(AssociatedIDs,',') where Item=@EmployeeType)>0) or AssociatedIDs='0')
	and ((StartDate between @StartDate and @EndDate) or (EndDate between @StartDate and @EndDate) or (@StartDate between StartDate and EndDate) or (@EndDate between StartDate and EndDate))

	insert into @LeaveDetails
	select LD.LeaveTypeID,LD.Applied,LD.LeaveDate from LeaveDetails LD left outer join LeaveMaster LM on LM.LeaveID=LD.LeaveID 
	where LM.ApplicantType=0 and IsApproved=1 and LM.EmployeeType=@EmployeeType and LM.ApplicantID=@ApplicantID and LD.LeaveID!=@LeaveID
	and LD.LeaveDate between @StartDate and @EndDate

	
		declare @LeaveSummery Table(LeaveTypeID int,LeaveTypeName nvarchar(50),YearlyQuota numeric(10,2),MonthlyQuota numeric(10,2),IsSalaryDeduct int
		,YearlyApplied numeric(10,2),MonthlyApplied numeric(10,2),Applied numeric(10,2),[Month] int,[Year] int)
		declare @LDStartMonth int=datepart(month,@StartDate),@LDEndMonth int=datepart(month,@EndDate)
		declare @LDStartYear int=datepart(year,@StartDate),@LDEndYear int=datepart(year,@EndDate)
		while(@LDStartYear*12+@LDStartMonth<=@LDEndYear*12+@LDEndMonth)
		begin
			Insert into @LeaveSummery(LeaveTypeID,LeaveTypeName,YearlyQuota,MonthlyQuota,IsSalaryDeduct,YearlyApplied,MonthlyApplied,Applied)
			exec sp_GetEmployeeLeaveDetailsNew @LeaveID,@ApplicantID,@LDStartMonth,@LDStartYear,@EmployeeType,0,@SBranchID			
			update @LeaveSummery set [Month]=@LDStartMonth,[Year]=@LDStartYear where [Month] is null
			set @LDStartMonth=@LDStartMonth+1
		end
		Declare @Increment numeric(5,2)=0,@HolidayFirstCount int=0,@HolidaySecondCount int=0,@HolidayFullCount int=0,@Available numeric(10,2)=0
		while(@StartDate<=@EndDate)
		Begin
			select @HolidayFirstCount=count(*) from @HolidayDetails where @StartDate between StartDate and EndDate and DayDuration=2
			select @HolidaySecondCount=count(*) from @HolidayDetails where @StartDate between StartDate and EndDate and DayDuration=3
			select @HolidayFullCount=count(*) from @HolidayDetails where @StartDate between StartDate and EndDate and DayDuration=1
			if(@LeaveDuration=0)--FullDay
			begin
				if(@HolidayFullCount>0)
				begin
					set @Increment=@Increment+1
				end
				else if(@HolidayFirstCount>0 and @HolidaySecondCount>0)
				begin
					set @Increment=@Increment+1
				end				
				else if(@HolidayFirstCount>0 or @HolidaySecondCount>0)
				begin
					set @Increment=@Increment+0.5
					insert into LeaveDetails(LeaveID,LeaveTypeID,Applied,LeaveDate) values(@LeaveID,@LeaveTypeID,0.5,@StartDate)
				end
				else
				begin
					set @Increment=@Increment+1
					insert into LeaveDetails(LeaveID,LeaveTypeID,Applied,LeaveDate) values(@LeaveID,@LeaveTypeID,1,@StartDate)
				end
			end
			else if(@LeaveDuration=1)--First Half
			begin
				if(@HolidayFirstCount=0 and @HolidayFullCount=0)
				begin
					set @Increment=@Increment+0.5
				end
			end			
			else if(@LeaveDuration=2)--Second Half
			begin
				if(@HolidaySecondCount=0 and @HolidayFullCount=0)
				begin
					set @Increment=@Increment+0.5
				end
			end


			if(@Increment=1)
			begin
				set @StartDate=dateadd(day,1,@StartDate)
				set @Increment=@Increment-1
			end
		End

		select * from @LeaveSummery
		declare @Available numeric(10,2)
		declare @Month int=0,@Year int=0
		Declare @dc numeric(10,2)=0,@LeaveTypeID int,@Applied numeric(10,2),@Reminder numeric(10,2)=0,@IsHalf bit=0,@MonthYear int=0
		while(@StartDate<=@EndDate)
		begin
			DECLARE db_cursor CURSOR FOR 
			select LeaveTypeID,Applied as Applied from @LeaveDetails --group by LeaveTypeID 
			OPEN db_cursor  
			FETCH NEXT FROM db_cursor INTO @LeaveTypeID,@Applied 
			WHILE @@FETCH_STATUS = 0  
			BEGIN  					
				select @Month=datepart(Month,@StartDate),@Year=datepart(year,@StartDate)
				select @Available=isnull(MonthlyQuota,0)-isnull(MonthlyApplied,0)+isnull(Applied,0) from @LeaveSummery
				where [Month]=@Month and [Year]=@Year and LeaveTypeID=@LeaveTypeID
				print(@Month)
				print('0-'+cast(@LeaveTypeID as nvarchar(10))+'-'+cast(@Applied as nvarchar(10))+'-'+cast(isnull(@Available,0) as nvarchar(10))++'-'+cast(@StartDate as nvarchar(20)))
				
				while(@Applied>0 and @Available>0)
				begin
					print('1-'+cast(@Applied as nvarchar(10))+'-'+cast(@Available as nvarchar(10)))
					if(@Available>0)
					begin
						if(@Applied=0.5)
							begin
								insert into LeaveDetails(LeaveID,LeaveTypeID,Applied,LeaveDate) values(@LeaveID,@LeaveTypeID,0.5,@StartDate)
								set @Applied=0
								set @Available=@Available-@Applied
								if(@IsHalf=1)
								begin
									set @IsHalf=0
									set @dc=@dc+1
								end
								else
								begin
									set @IsHalf=1
								end
							end
							if(@Applied>=0.5)
							begin 
							if(@IsHalf=1)
							begin
								insert into LeaveDetails(LeaveID,LeaveTypeID,Applied,LeaveDate) values(@LeaveID,@LeaveTypeID,0.5,DATEADD(day,@dc,@StartDate))							
								set @Applied=@Applied-0.5
								set @dc=@dc+1
								set @IsHalf=0
								set @Available=@Available-0.5
							end
							else
							begin
								insert into LeaveDetails(LeaveID,LeaveTypeID,Applied,LeaveDate) values(@LeaveID,@LeaveTypeID,1,DATEADD(day,@dc,@StartDate))
								set @Applied=@Applied-1		
								set @Available=@Available-1			
								set @dc=@dc+1
							end
				end
					end
					update @LeaveSummery set Applied=isnull(MonthlyQuota,0)-isnull(MonthlyApplied,0)-@Available where [Month]=@Month and [Year]=@Year and LeaveTypeID=@LeaveTypeID
					set @StartDate=dateadd(day,@dc,@StartDate)
				end
				FETCH NEXT FROM db_cursor INTO @LeaveTypeID,@Applied  
			END 
			CLOSE db_cursor  
			DEALLOCATE db_cursor

			set @StartDate=DateAdd(day,1,@StartDate)
		end
		select * from LeaveDetails where LeaveID=0
		--Delete from LeaveDetails where LeaveID=0