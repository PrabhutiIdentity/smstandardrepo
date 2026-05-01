    
              
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
 declare @ActiveSessionID int      
                                                                                                
   Select @ActiveSessionID=SessionID from SessionMaster where SBranchID=@SBranchID and SessionStatus=1                                                                                                     
                                                                                                          
  if(@SStudentID<>0)                                                                                                                  
  begin                                                                                                                  
   select @ClassID=ClassID,@SectionID=SectionID from Student_Session where SessionID=@SessionID and StudentID=@SStudentID                                                                                                                  
  end                                                                                                                  
  Declare @GroupID int                                                                                                                  
  Declare @TransportFeeMode int                                                                                                                  
  Declare @HostelFeeMode int                                                                                                                  
  declare @LastPayDay nvarchar(2)                                
    declare @LateFeeTypeID int                                
  Declare @IsTransport int                                         
                                          
  Select @TransportFeeMode=details from MasterSettings where Type='TransportFeeMode' and SBranchID=@SBranchID                                       
                                        
                                                                                                               
  Select @HostelFeeMode=details from MasterSettings where Type='HostelFeeMode' and SBranchID=@SBranchID                                                                                                                  
  Select @LastPayDay=details from MasterSettings where Type='FeePaymentReminderDate' and SBranchID=@SBranchID                                         
   select @LateFeeTypeID =FeeTypeID from FeeTypeMaster where FeeTypeApplicable=7 and SBranchID=@SBranchID                                                
                             
   if(@LateFeeTypeID='' or @LateFeeTypeID is NULL)                                             
   begin                                  
   set @LateFeeTypeID=-2                                    end                         
                                        
   Declare @TransHostFeeTable table(FeeTypeID int,MonthType int,NetApplicablePayment numeric(10,2),DiscAmt numeric(10,2),                                              
 PaymentRecieved numeric(10,2),PaymentDate date,FeeTypeApplicable int)                                             
                                        
   Select @IsTransport=StopID from StudentMaster where StudentID=@SStudentID                                              
                                        
                                        
  -- For Transport Discount                                       
 declare @TFeeTypeID int                                            
 select @TFeeTypeID=FeeTypeID from feetypemaster where FeeTypeApplicable=5 and SBranchID=@SBranchID                           
 declare @TransportDiscount numeric(10,2)=0                                         
                                        
                                                                                            
  Declare @SessionStartDate date,@SessionEndDate Date                                                                                                                  
  select @GroupID=GroupID from Class_Sections where ID=@SectionID                                                                                                                  
  select @SessionStartDate=SessionStartDate,@SessionEndDate=SessionEndDate from SessionMaster where SessionID=@SessionID                                              
                                                                                                                 
  Declare @Students table(StudentID int,Name nvarchar(100),RollNo nvarchar(100),Gender int,Photo nvarchar(100),ClassID int,FeePaymentMode int,                                                                                                                
  
  StudentSID nvarchar(15),FromDate date,ToDate Date,QuotaID int,                                                                                                                  
  SessionID int,VehicleRouteID int,HostelRoomID int,SectionID int,                                                                                                                  
  SessionStartDate date,SessionEndDate date,IsAdmissionFee int,SchoolUID nvarchar(50),FeeAmount numeric(10,2),PreviousDue numeric(10,2)                                                         
  ,LateFee numeric(10,2),Discounts numeric(10,2),Paid numeric(10,2),IsCustomFee int,SequenceNo int)                                                                    
                                                                    
  if(@SStudentID=0)                                                                                         
  begin                                                                         
      if(@ActiveSessionID=@SessionID)                                                                                                
   begin                                                                             
   insert into @Students                                                                                                                  
   select SM.StudentID,SM.Name,SS.RollNo,SM.Gender,SM.Photo,SS.ClassID,SS.FeePaymentMode,                                                                                                                  
   'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,SS.FromDate,SS.ToDate,                                                                                                                  
   SS.QuotaID,SS.SessionID,SM.VehicleRouteID,SM.HostelRoomID,SS.SectionID,                                               
   (Case when @SessionStartDate<SS.FromDate then SS.FromDate else @SessionStartDate end) as SessionStartDate,                                                                                                              
   (Case when @SessionEndDate>SS.ToDate then SS.ToDate else @SessionEndDate end) as SessionEndDate,                                                                                                               
   isnull(SS.IsAdmissionFeeApplicable,0),SM.SchoolUID,0,0,0,0,0,SS.IsCustomFee,                  
   ROW_NUMBER() OVER (ORDER BY  SM.StudentID desc)                    
   from Student_Session SS left outer join StudentMaster SM on SM.StudentID=SS.StudentID                                                                                                                  
   where SS.ClassID=@ClassID and SessionID=@SessionID and SectionID=@SectionID   and SS.Status=1                                                                                  
   end                       
   else                                                                                
                                                                                   
   begin                                                                         
   insert into @Students                                                                              
   select SM.StudentID,SM.Name,SS.RollNo,SM.Gender,SM.Photo,SS.ClassID,SS.FeePaymentMode,                                                                                         
   'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,SS.FromDate,SS.ToDate,                                 
   SS.QuotaID,SS.SessionID,SM.VehicleRouteID,SM.HostelRoomID,SS.SectionID,                                       
   (Case when @SessionStartDate<SS.FromDate then SS.FromDate else @SessionStartDate end) as SessionStartDate,                                                                                                                  
   (Case when @SessionEndDate>SS.ToDate then SS.ToDate else @SessionEndDate end) as SessionEndDate,                                                                                                                  
   isnull(SS.IsAdmissionFeeApplicable,0),SM.SchoolUID,0,0,0,0,0,SS.IsCustomFee  ,                  
   ROW_NUMBER() OVER (ORDER BY  SM.StudentID desc)                                                                                                                 
   from Student_Session SS left outer join StudentMaster SM on SM.StudentID=SS.StudentID                                                                                                                  
   where SS.ClassID=@ClassID and SessionID=@SessionID and SectionID=@SectionID   --and SS.Status=1                                                                                  
   end                                                                                                                
                                                                                                                     
  end                                                                                                                  
  else                                                                                   
  begin                                                                                   
   if(@ActiveSessionID=@SessionID)                                                                                                
   begin                                                                                  
   insert into @Students                                                                                                                  
   select SM.StudentID,SM.Name,SS.RollNo,SM.Gender,SM.Photo,SS.ClassID,SS.FeePaymentMode,         
   'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,SS.FromDate,SS.ToDate,                                                                                     
   SS.QuotaID,SS.SessionID,SM.VehicleRouteID,SM.HostelRoomID,SS.SectionID,                                                                
   (Case when @SessionStartDate<SS.FromDate then SS.FromDate else @SessionStartDate end) as SessionStartDate,                                                                                                    
   (Case when @SessionEndDate>SS.ToDate then SS.ToDate else @SessionEndDate end) as SessionEndDate,                                                                    
   isnull(SS.IsAdmissionFeeApplicable,0),SM.SchoolUID,0,0,0,0,0,SS.IsCustomFee   ,                  
   ROW_NUMBER() OVER (ORDER BY  SM.StudentID desc)                                   
   from Student_Session SS left outer join StudentMaster SM on SM.StudentID=SS.StudentID                 
   where SS.ClassID=@ClassID and SessionID=@SessionID and SectionID=@SectionID and SS.StudentID=@SStudentID  and SS.Status=1                                                                                                                                  
  
    
           
                                
   end                                                                                
   else                                                                                
   begin                                                                                
   insert into @Students                                                                                                      
   select SM.StudentID,SM.Name,SS.RollNo,SM.Gender,SM.Photo,SS.ClassID,SS.FeePaymentMode,                                                                 
   'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,SS.FromDate,SS.ToDate,                                                         
   SS.QuotaID,SS.SessionID,SM.VehicleRouteID,SM.HostelRoomID,SS.SectionID,                                                                                                                  
   (Case when @SessionStartDate<SS.FromDate then SS.FromDate else @SessionStartDate end) as SessionStartDate,                                                   
   (Case when @SessionEndDate>SS.ToDate then SS.ToDate else @SessionEndDate end) as SessionEndDate,                                                                                          
   isnull(SS.IsAdmissionFeeApplicable,0),SM.SchoolUID,0,0,0,0,0,SS.IsCustomFee   ,                  
   ROW_NUMBER() OVER (ORDER BY  SM.StudentID desc)                                       
   from Student_Session SS left outer join StudentMaster SM on SM.StudentID=SS.StudentID                                                                                                                  
   where SS.ClassID=@ClassID and SessionID=@SessionID and SectionID=@SectionID and SS.StudentID=@SStudentID  --and SS.Status=1                                                                                                       
                                                                                                             
   end                                                                                                               
                                                                                   
  end                                                          
                                                         
 Declare @DiscountApproved table(StudentID int,FeeTypeID int,ApprovedAmount decimal(18,2),FeeMonth int,FeeYear int)                                                                                          
                                                                                          
 Insert into @DiscountApproved           
 select StudentID,FeeTypeID,ApprovedAmount,FeeMonth,FeeYear from [FeeDiscountRequestDetails] FRD                                                                      
 left outer join  [FeeDiscountRequestMaster] FRM on FRD.DiscRequestID=FRM.DiscRequestID          
 where FRM.Status=1 and isnull(ApprovedAmount,0)<>0                                                                     
 and StudentID in (select StudentID from @Students) and Status=1          
 and (     (FRM.FeeYear > YEAR(@SessionStartDate))     OR (FRM.FeeYear = YEAR(@SessionStartDate)          AND FRM.FeeMonth >= MONTH(@SessionStartDate)) ) and (     (FRM.FeeYear < YEAR(@SessionEndDate))     OR (FRM.FeeYear = YEAR(@SessionEndDate)         
  
    
 AND FRM.FeeMonth <= MONTH(@SessionEndDate)) )                                                   
                                                                
  Declare @QuotaDiscounts table (FeeTypeID int,Discount numeric(5,2))                                                                                                             
   Declare @MTFT table (MonthTypeID int,FeeTypeID int)                                                                             
  insert into @MTFT                                                                         
  select MonthTypeID,FeeTypeID from [MonthTypeFeeType]                                                
                                                                                                                 
 --- changes new nofeemonth                                                                                
  Declare @NoFeeMonths table (Month int,FeeTypeID int)                                                     
                                                                                                                 
  insert into @NoFeeMonths                                                                              
   select Month,FeeTypeID as FeeTypeID  from SessionClassNoFeeMonths where ClassID=@ClassID and SessionID=@SessionID   and SBranchID=@SbranchID                                     
                                        
 --                                                                                                         
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
   declare @CurrentSequence int=1                  
   declare @MaxSequence int                  
   select @MaxSequence=max(SequenceNo) from @Students                  
   while(@CurrentSequence<=@MaxSequence)                  
   begin                  
   print('Current Index'+cast(@CurrentSequence as nvarchar(10)) +'stid'+  cast(@StudentID as nvarchar(10)))                  
  SELECT @StuSessionSDate=SessionStartDate,@StuSessionEDate=SessionEndDate,@StudentID=StudentID,@QuotaID=QuotaID,@IsAdmissionFee=IsAdmissionFee,@FeeAmount=FeeAmount,                  
  @PreviousDue=PreviousDue,@LateFee=LateFee,@Discounts=Discounts,@Paid=Paid,@FeePaymentMode=FeePaymentMode FROM @Students  where SequenceNo=@CurrentSequence                  
                                                                 
   declare @StudentSessionUID int                                                                                                                  
   set @Discounts=0                                                                                            
   select top 1 @StudentSessionUID=StudentSessionUID,@IsCustomFee=isnull(IsCustomFee,0) from Student_Session where StudentID=@StudentID and SessionID=@SessionID                      
   order by Status desc                                                      
                                  
  -- BR34 Carry-forward change: read carry-forward fee setup and amount  
       DECLARE @CarryForwardFeeTypeID int = 0         DECLARE @CarryForwardAmount numeric(10,2) = 0          IF (@SBranchID = 34)         BEGIN             SELECT TOP 1 @CarryForwardFeeTypeID = FeeTypeID             FROM FeeTypeMaster             WHERE S
BranchID = @SBranchID               AND FeeTypeApplicable = 1               AND LTRIM(RTRIM(FeeTypeName)) = 'Session-Carry-forward'              SELECT @CarryForwardAmount = ISNULL(FeeAmount,0)             FROM StudentFeeDetails             WHERE StudentI
D = @StudentID               AND SessionID = @StudentSessionUID               AND FeeTypeID = @CarryForwardFeeTypeID               AND IsApplicable = 1         END   
    
  --- end carry forworad    
                                       
    Delete from @QuotaDiscounts                                   
   Insert into @QuotaDiscounts select FeeTypeID,DiscPer from QuotaDiscountDetails where SBranchID=@SBranchID and SessionID=@SessionID and QuotaID=@QuotaID                                                                        
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
                                                                                                                      
    while(datepart(year,@StuSessionSDate)*12+datepart(month,@StuSessionSDate)<=datepart(year,@LoopDate)*12+datepart(month,@LoopDate))                                                                                                                  
    begin                                                                                                                  
   set @LMonth= datepart(month,@StuSessionSDate)                                                    
   set @LYear=datepart(year,@StuSessionSDate)                                                                                                        
                                                                                                                 
                                                                                                                 
    declare @PayDate date                                                                                                                  
   select @PayDate=min(PaymentDate) from PaymentDetails  where PayeeID=@StudentID and Month=@LMonth and Year=@LYear  and isnull(PaymentStatus,0)=0 and isnull(PaymentRecieved,0) > 0         
    declare @IsLatePay int=0                                
  declare @MonthLatePaymentDate date                                       
    set @MonthLatePaymentDate=cast(cast(@LYear as nvarchar(5))+'-'+cast(@LMonth as nvarchar(2))+'-'+@LastPayDay as date)                     
                                  
    if(@PayDate is not null)                              
  begin                   
    select @IsLatePay=count(*) from PaymentDetails where PayeeID=@StudentID and Month=@LMonth and Year=@LYear and (FeeTypeID=@LateFeeTypeID)                                                                                                                 
  end                                                                                                                            
    else if(@PayDate is null and  @CurDate>@MonthLatePaymentDate)                                                         
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
   declare @PPR numeric(10,2)=0                                                                                                                    
                                   
    SELECT                        
   @PPD = SUM(                        
    (CASE                        
     WHEN ISNULL(PD.PaymentID, 0) != 0 THEN PD.NetApplicablePayment - ISNULL(PD.DiscAmt, 0)                        
     WHEN ISNULL(@IsCustomFee, 0) = 1 THEN SFD.FeeAmount             --ELSE (FTS.FeeAmount - ISNULL(FTS.FeeAmount * NULLIF(QD.Discount, 0) / 100, 0))                        
     ELSE (NULLIF(FTS.FeeAmount, 0) - ISNULL(FTS.FeeAmount * NULLIF(QD.Discount, 0) / 100, 0))                        
    END))         
        
     ,@PPR=isnull(Sum(PD.PaymentRecieved),0)                                                                                                                  
   from @FeeStructureTable FTS left outer join @QuotaDiscounts QD on QD.FeeTypeID=FTS.FeeTypeID                                   
   left outer join [dbo].[StudentFeeDetails] SFD on SFD.StudentID=@StudentID and SFD.SessionID=@StudentSessionUID and                                                                                                                  
   SFD.FeeTypeID=FTS.FeeTypeID and SFD.IsApplicable=1                                                                             
   left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear                                                                                                                  
   where FTS.FeeTypeApplicable in (select FeeTypeID from @MTFT where MonthTypeID=@MonthType) and FTS.FeeTypeApplicable!=5                                                          
   and FTS.FeeTypeApplicable <> 7 and FTS.FeeTypeApplicable<>(Case when @IsAdmissionFee=0 then 8 else 0 end)                                                                                                                  
   and isnull(SFD.IsApplicable,FTS.Status)=1 and (FTS.FeeTypeApplicable<>9 or                                       
   (select count(*) from [dbo].[SplitStringToTable](FTS.Months,',') where Item=@LMonth)>0)                                           
     and (FTS.FeeTypeID not in (Select isnull(NFM.FeeTypeID,FTS.FeeTypeID) from @NoFeeMonths NFM   
  where NFM.Month=@LMonth) )     
  -- use previous Session due  
  AND (@SBranchID <> 34 OR FTS.FeeTypeID <> ISNULL(@CarryForwardFeeTypeID,-1))                                      
                  
     declare @mDiscount numeric(10,2),@mPaid numeric(10,2),@mAmount numeric(10,2)      ,@IsPaymentDone int                                                                                            
     select @IsPaymentDone=count(*) from v_PaymentDetails PD where PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear                                                                                            
   select @mDiscount=sum(isnull(DiscAmt,0)),@mAmount=sum(NetApplicablePayment),@mPaid=sum(PaymentRecieved) from v_PaymentDetails PD where PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear and PD.FeeTypeID!=@LateFeeTypeID                       
  
    
      
        
         
                        
    -- Transport Discount                                            
    select @TransportDiscount=isnull(ApprovedAmount,0) from [FeeDiscountRequestDetails] FDD inner join [FeeDiscountRequestMaster] FDM                                            
    on FDM.DiscRequestID=FDD.DiscRequestID                                        
    where StudentID=@StudentID and Status=1 and FeeTypeID=@TFeeTypeID  and FeeMonth=@LMonth and FeeYear=@LYear                                                                      
   --                                            
                                            
                                                  
                                                    
                                                                           
  declare @pDiscount numeric(10,2)=0                                                                                
   --if((isnull(@mDiscount,0)=0 and isnull(@mAmount,0)-isnull(@mPaid,0)>=0) or @IsPaymentDone=0)                                                                      
   --begin                                                                                                                  
   -- select @pDiscount=isnull(sum(ApprovedAmount),0) from @DiscountApproved where StudentID=@StudentID and FeeMonth=@LMonth and FeeYear=@LYear                                                                  
   --end             
   select @pDiscount = isnull(sum(     case          when isnull(PD.DiscAmt,0) >= DA.ApprovedAmount  then 0    
     -- already reflected in PaymentDetails            
   else DA.ApprovedAmount - isnull(PD.DiscAmt,0)      
   -- not yet reflected        
    end ), 0) from @DiscountApproved DA left join v_PaymentDetails PD      on PD.FeeTypeID = DA.FeeTypeID     
        and PD.PayeeID = @StudentID     and PD.Month = @LMonth      and PD.Year = @LYear where DA.StudentID = @StudentID and DA.FeeMonth = @LMonth  and DA.FeeYear = @LYear     
                                                                                                                              
                                                          
  if((select count(*) from @FeeStructureTable FTS where FeeTypeApplicable=5 and                     
   (FeeTypeID not in (Select isnull(NFM.FeeTypeID,FTS.FeeTypeID) from @NoFeeMonths NFM where NFM.Month=@LMonth) or (Select count(*) from @NoFeeMonths)=0))>0)                                 
    begin                                                  
     select @PreviousDue= isnull(@PreviousDue,0)+isnull([dbo].[fn_GetStudentTransportFeeAmount](@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID),0)                                                                  
     end                                                   
    if((select count(*) from @FeeStructureTable where FeeTypeApplicable=6 and                                               
    (FeeTypeID not in (Select isnull(NFM.FeeTypeID,FeeTypeID) from @NoFeeMonths NFM where NFM.Month=@LMonth) or (Select count(*) from @NoFeeMonths)=0))>0)                                                 
                                             
    begin                                                                                                               
     select @PreviousDue= isnull(@PreviousDue,0)+isnull([dbo].[fn_GetStudentHostalFeeAmount](@LMonth,@LYear,@StudentID,@HostelFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID),0)                                                                              
  
    
     
         
               
   end                                                          
      select @PreviousDue=isnull(@PreviousDue,0)- isnull(sum(isnull(PD.PaymentRecieved,0)+(isnull(PD.DiscAmt,0))),0)                                                                                                             
                                                                                                             
    from @FeeStructureTable FTS left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear                                                                       
     where FTS.FeeTypeApplicable in (5,6) and                                               
  (FTS.FeeTypeID not in (Select isnull(NFM.FeeTypeID,FTS.FeeTypeID) from @NoFeeMonths NFM where NFM.Month=@LMonth) )                                   
                                         
    select @CLateFee=sum((case when isnull(PD.PaymentID,0)!=0 then PD.NetApplicablePayment-isnull(PD.DiscAmt,0) else FTS.FeeAmount end))                                                                                             
    ,@CPaid=Sum(isnull(PD.PaymentRecieved,0))                                                                                                                  
    from @FeeStructureTable FTS left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear                                                                                            
 
                       
    where FTS.FeeTypeApplicable = (case when @IsLatePay=0 then 0 else 7 end)                                                                                                                  
   and isnull(FTS.Status,0)=1 and               
   (FTS.FeeTypeID not in (Select isnull(NFM.FeeTypeID,FTS.FeeTypeID) from @NoFeeMonths NFM where NFM.Month=@LMonth) )                                               
                                              
    set @PreviousDue=(isnull(@PreviousDue,0)-sum(isnull(@pDiscount,0)))+(isnull(@CLateFee,0)-isnull(@CPaid,0))+(isnull(@PPD,0)-isnull(@PPR,0))                                                                   
    end                                                                          
   else                                                                                                             
   begin                                                                          
                                                                  
    declare @pPaid numeric(10,2)                                                                      
             
  select @FeeAmount=isnull(@FeeAmount,0)      
  +sum( (case       
  when isnull(PD.PaymentID,0)!=0 then PD.NetApplicablePayment-isnull(PD.DiscAmt,0)       
  when isnull(@IsCustomFee,0)=1 AND PD.Amount IS NOT NULL  then PD.Amount  -- historical       
  when isnull(@IsCustomFee,0)=1 AND SFD.FeeAmount IS NOT NULL then SFD.FeeAmount  -- unpaid custom            
  ELSE (NULLIF(FTS.FeeAmount,0) - ISNULL(FTS.FeeAmount*NULLIF(QD.Discount,0)/100,0))           
  end))                                                                                           
   ,@pPaid=Sum(isnull(PD.PaymentRecieved,0))                                                                                                              
    from @FeeStructureTable FTS left outer join @QuotaDiscounts QD on QD.FeeTypeID=FTS.FeeTypeID                     
    left outer join [dbo].[StudentFeeDetails] SFD on SFD.StudentID=@StudentID and SFD.SessionID=@StudentSessionUID and SFD.FeeTypeID=FTS.FeeTypeID and SFD.IsApplicable=1                      
  left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear                                                                         
    where FTS.FeeTypeApplicable in (select FeeTypeID from @MTFT where MonthTypeID=@MonthType) and FTS.FeeTypeApplicable!=5                                                                                                                
    and FTS.FeeTypeApplicable <> 7 and FTS.FeeTypeApplicable<>(Case when @IsAdmissionFee=0 then 8 else 0 end)                                                                                                                  
 and isnull(SFD.IsApplicable,FTS.Status)=1 and (FTS.FeeTypeApplicable<>9 or                                                  
 (select count(*) from [dbo].[SplitStringToTable](FTS.Months,',') where Item=@LMonth)>0)                              
      and (FTS.FeeTypeID not in (Select isnull(NFM.FeeTypeID,FTS.FeeTypeID) from @NoFeeMonths NFM where NFM.Month=@LMonth) or (Select count(*) from @NoFeeMonths)=0)                                                 
      -- carry forward previous fee  
     AND (@SBranchID <> 34 OR FTS.FeeTypeID <> ISNULL(@CarryForwardFeeTypeID,-1))                                  
                                                                                                                    
    select @isPaid=count(*) from v_PaymentDetails PD where PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear                  
                                                                                                    
    if(isnull(@isPaid,0)=0)                                                                                                   
    begin                                                                                                                  
     select @Discounts=isnull(@Discounts,0)+isnull(sum(ApprovedAmount),0) from @DiscountApproved where StudentID=@StudentID and FeeMonth=@LMonth and FeeYear=@LYear                               
                                                                                                   
    end                                                                                                                  
         --                                                  
    declare @DiscountPT numeric(10,2)                                                  
 declare @DiscountApr numeric(10,2)                                                  
  if (isnull(@isPaid,0)!=0)                                                  
  begin                     
    select @DiscountPT=isnull(sum(PD.DiscAmt),0) from v_PaymentDetails PD where PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear                                                  
    select @DiscountApr=isnull(sum(ApprovedAmount),0) from @DiscountApproved where StudentID=@StudentID and FeeMonth=@LMonth and FeeYear=@LYear                                                                    
    if(@DiscountPT<@DiscountApr)                                                  
    begin                                                  
      select @Discounts=sum(isnull(ApprovedAmount,0)) from @DiscountApproved where StudentID=@StudentID and FeeMonth=@LMonth and FeeYear=@LYear                       
     end                                   
  end                                                                        
  IF (            
    SELECT COUNT(*)            
    FROM @FeeStructureTable FS            
    LEFT JOIN @NoFeeMonths NFM ON FS.FeeTypeID = NFM.FeeTypeID AND NFM.Month = @LMonth            
    WHERE FS.FeeTypeApplicable = 5 AND NFM.FeeTypeID IS NULL            
) > 0            
  begin             
    select  @FeeAmount=isnull(@FeeAmount,0)+isnull([dbo].[fn_GetStudentTransportFeeAmount](@LMonth,@LYear,@StudentID,@TransportFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID),0)                       
  end                                                                                                                          
  if((select count(*) from @FeeStructureTable where FeeTypeApplicable=6 and                    
    (FeeTypeID not in (Select isnull(NFM.FeeTypeID,FeeTypeID) from @NoFeeMonths NFM where NFM.Month=@LMonth) or (Select count(*) from @NoFeeMonths)=0))>0 )                   
   begin                                                                                                                  
    select  @FeeAmount=isnull(@FeeAmount,0)+[dbo].[fn_GetStudentHostalFeeAmount](@LMonth,@LYear,@StudentID,@HostelFeeMode,@ClassID,@GroupID,@SBranchID,@SessionID)                    
    end                                                                                                                  
    select @TPaid=Sum(isnull(PD.PaymentRecieved,0))                                                                                                                  
    from @FeeStructureTable FTS left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID                                 
    and PD.Month=@LMonth and PD.Year=@LYear                                                            
  where FTS.FeeTypeApplicable in (5,6) and                                               
    (FTS.FeeTypeID not in (Select isnull(NFM.FeeTypeID,FTS.FeeTypeID) from @NoFeeMonths NFM where NFM.Month=@LMonth) )                                            
     select @CLateFee=sum((case when isnull(PD.PaymentID,0)!=0 then PD.NetApplicablePayment else FTS.FeeAmount end))                                                                     
                                                   
    ,@CPaid=Sum(isnull(PD.PaymentRecieved,0))                                                                                                                  
    from @FeeStructureTable FTS left outer join v_PaymentDetails PD on PD.FeeTypeID=FTS.FeeTypeID and PD.PayeeID=@StudentID and PD.Month=@LMonth and PD.Year=@LYear                                                                                           
  
    
      
        
          
                      
                      
   where FTS.FeeTypeApplicable = (case when @IsLatePay=0 then 0 else 7 end)                                                                  
     and isnull(FTS.Status,0)=1 and                                               
     (FTS.FeeTypeID not in (Select isnull(NFM.FeeTypeID,FTS.FeeTypeID) from @NoFeeMonths NFM where NFM.Month=@LMonth) )                                               
                                            
                                                             
    set @LateFee=isnull(@LateFee,0)+isnull(@CLateFee,0)                                                                                                                  
    set @Paid=isnull(@Paid,0)+isnull(@CPaid,0)+isnull(@TPaid,0)+isnull(@pPaid,0)                                                                                                                  
   end                                                                                                                  
                                                       
     -- BR34 Carry-forward change: carry-forward stays in PreviousDue, not FeeAmount        IF (@SBranchID = 34 AND ISNULL(@CarryForwardAmount,0) > 0 AND ISNULL(@CarryForwardFeeTypeID,0) > 0)  
BEGIN  
    DECLARE @CarryForwardPaid numeric(10,2) = 0  
  
    SELECT @CarryForwardPaid = ISNULL(SUM(PD.PaymentRecieved), 0)  
    FROM v_PaymentDetails PD  
    WHERE PD.PayeeID = @StudentID  
      AND PD.FeeTypeID = @CarryForwardFeeTypeID  
      AND PD.Month = MONTH(@SessionStartDate)  
      AND PD.Year = YEAR(@SessionStartDate)  
  
    SET @PreviousDue = ISNULL(@PreviousDue,0) + ISNULL(@CarryForwardAmount,0)  
    SET @Paid = ISNULL(@Paid,0) + ISNULL(@CarryForwardPaid,0)  
END  
  
                
    IF @PreviousDue < 0              
BEGIN              
    SET @PreviousDue = 0;              
END              
                                              
  if(@PayDate is null)                                                                   
  begin                                                                             
   set @StuSessionSDate=dateadd(month,1,@StuSessionSDate)                                                                                                                  
  end                             
  else                                                               
  begin                              
   set @StuSessionSDate=dateadd(month,@FeePaymentMode,@StuSessionSDate)                                                                                               
  end                                                                                                              
 end                                                                       
                                                                 
                                                             
  UPDATE @Students SET FeeAmount=@FeeAmount,PreviousDue=@PreviousDue,Discounts= isnull(nullif(@Discounts,0),@pDiscount),Paid=@Paid,LateFee=@LateFee WHERE SequenceNo=@CurrentSequence                      
  set @CurrentSequence=@CurrentSequence+1                                                                                                            
   END                                                
                                                                                                           
   select StudentID ,Name,RollNo,Gender,Photo,ClassID,FeePaymentMode,                                                                                   
   StudentSID,FromDate ,ToDate ,QuotaID,SessionID,VehicleRouteID,HostelRoomID,SectionID,                                                                                                                  
   SessionStartDate,SessionEndDate,IsAdmissionFee,SchoolUID,FeeAmount,PreviousDue,LateFee,Discounts,Paid from @Students where isnull(StudentID,0)!=0                                          
                                                                                                                 
END 