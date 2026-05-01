/*
Fix: Account/ReportDailyFeeCollection should work for both previous and current sessions.

Root cause:
- dbo.GetStudentFeeRecordDaily joins v_ClassSectionNames with "AND SS.Status = 1"
- historical Student_Session rows are often Status = 0 after promotion
- result: previous-session report rows lose class/section context and behave like current-session-only reporting

Change:
- remove the active-status-only restriction from the v_ClassSectionNames join
- keep PaymentMaster / RefundMaster session linkage unchanged
*/

ALTER PROC [dbo].[GetStudentFeeRecordDaily]
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

 select FeeTypeID as ID,FeeTypeName as Name from FeeTypeMaster where SBranchID=@SBranchID
 if(@ReportType=1)--Daily
 begin
  if(@PaymentMode=-1)
   begin
         select PM.PaymentID,PaymentAmount,0 as RefundAmount,PM.PaymentDate,PaymentRecieptNo,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PM.PaymentID) PayMonths,PaymentMode,
     'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
      (Select SchoolUID from StudentMaster where StudentID=SS.StudentID) as SchoolUID,
   (Select Name from StudentMaster where StudentID=PM.PayeeID) as Name,
   SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
    from PaymentMaster PM  left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
     where cast(PM.PaymentDate as date)=cast(@FromDate as date) and PM.SBranchID=@SBranchID
     union all
      select RefundID as PaymentID,0 as PaymentAmount,RefundAmount,Refunddate as PaymentDate,CAST(RefundBy AS NVARCHAR(50)) as CollectedBy,Reason, NULL AS Remark, NULL AS PayMonths,NULL AS PaymentMode,CAST(RefundID AS NVARCHAR(50)) AS PaymentRecieptNo,
  'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
    (Select SchoolUID from StudentMaster where StudentID=RefundTo) as SchoolUID,
      (Select Name from StudentMaster where StudentID=RefundTo) as Name,
   SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=RM.SessionID) as SessionName
     from RefundMaster RM left outer join Student_Session SS on RM.RefundTo=SS.StudentID and SS.SessionID=RM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
    where cast(RM.RefundDate as date)=cast(@FromDate as date) and RM.SBranchID=@SBranchID

     select PD.PaymentID,PD.FeeTypeID,PD.PaymentRecieved from PaymentDetails PD
     where PD.PaymentID in (select PaymentID from PaymentMaster PM where cast(PM.PaymentDate as date)=cast(@FromDate as date) and PM.SBranchID=@SBranchID)
 end
   else
   begin
    select PM.PaymentID,PaymentAmount,0 as RefundAmount,PM.PaymentDate,PaymentRecieptNo,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PM.PaymentID) PayMonths,PaymentMode,
  'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
      (Select SchoolUID from StudentMaster where StudentID=SS.StudentID) as SchoolUID,
   (Select Name from StudentMaster where StudentID=PM.PayeeID) as Name,SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
    from PaymentMaster PM  left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
    where cast(PM.PaymentDate as date)=cast(@FromDate as date) and PM.SBranchID=@SBranchID and PM.PaymentMode=@PaymentMode

     union all
      select RefundID as PaymentID,0 as PaymentAmount,RefundAmount,Refunddate as PaymentDate,CAST(RefundBy AS NVARCHAR(50)) as CollectedBy,Reason, NULL AS Remark, NULL AS PayMonths,NULL AS PaymentMode,CAST(RefundID AS NVARCHAR(50)) AS PaymentRecieptNo,
  'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
    (Select SchoolUID from StudentMaster where StudentID=RefundTo) as SchoolUID,
 (Select Name from StudentMaster where StudentID=RefundTo) as Name,
 SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=RM.SessionID) as SessionName
     from RefundMaster RM left outer join Student_Session SS on RM.RefundTo=SS.StudentID and SS.SessionID=RM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
    where cast(RM.RefundDate as date)=cast(@FromDate as date) and RM.SBranchID=@SBranchID

      select PD.PaymentID,PD.FeeTypeID,PD.PaymentRecieved from PaymentDetails PD
     where PD.PaymentID in (select PaymentID from PaymentMaster PM where cast(PM.PaymentDate as date)=cast(@FromDate as date) and PM.SBranchID=@SBranchID and PM.PaymentMode=@PaymentMode)
   end
END
  else if(@ReportType=2)--Monthly
 begin
  if(@PaymentMode=-1)
  begin
  select PM.PaymentID,PaymentAmount,0 as RefundAmount,PM.PaymentDate,PaymentRecieptNo,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PM.PaymentID) PayMonths,PaymentMode,
   'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
      (Select SchoolUID from StudentMaster where StudentID=SS.StudentID) as SchoolUID,
   (Select Name from StudentMaster where StudentID=PM.PayeeID) as Name,
   SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
    from PaymentMaster PM  left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
    where datepart(month,PM.PaymentDate)=datepart(month,@FromDate) and datepart(year,PM.PaymentDate)=datepart(year,@FromDate)  and PM.SBranchID=@SBranchID

    union all
      select RefundID as PaymentID,0 as PaymentAmount,RefundAmount,Refunddate as PaymentDate,CAST(RefundBy AS NVARCHAR(50)) as CollectedBy,Reason, NULL AS Remark, NULL AS PayMonths,NULL AS PaymentMode,CAST(RefundID AS NVARCHAR(50)) AS PaymentRecieptNo,
  'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
    (Select SchoolUID from StudentMaster where StudentID=RefundTo) as SchoolUID,
 (Select Name from StudentMaster where StudentID=RefundTo) as Name,
 SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=RM.SessionID) as SessionName
     from RefundMaster RM left outer join Student_Session SS on RM.RefundTo=SS.StudentID and SS.SessionID=RM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
    where datepart(month,RM.RefundDate)=datepart(month,@FromDate) and datepart(year,RM.RefundDate)=datepart(year,@FromDate) and RM.SBranchID=@SBranchID

      select PD.PaymentID,PD.FeeTypeID,PD.PaymentRecieved from PaymentDetails PD
     where PD.PaymentID in (select PaymentID from PaymentMaster PM where datepart(month,PM.PaymentDate)=datepart(month,@FromDate) and datepart(year,PM.PaymentDate)=datepart(year,@FromDate)  and PM.SBranchID=@SBranchID )
  end
  else
  begin
  select PM.PaymentID,PaymentAmount,0 as RefundAmount,PM.PaymentDate,PaymentRecieptNo,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PM.PaymentID) PayMonths,PaymentMode,
   'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
      (Select SchoolUID from StudentMaster where StudentID=SS.StudentID) as SchoolUID,
   (Select Name from StudentMaster where StudentID=PM.PayeeID) as Name,SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
    from PaymentMaster PM  left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
     where datepart(month,PM.PaymentDate)=datepart(month,@FromDate) and datepart(year,PM.PaymentDate)=datepart(year,@FromDate) and PM.SBranchID=@SBranchID and PM.PaymentMode=@PaymentMode

     union all
      select RefundID as PaymentID,0 as PaymentAmount,RefundAmount,Refunddate as PaymentDate,CAST(RefundBy AS NVARCHAR(50)) as CollectedBy,Reason, NULL AS Remark, NULL AS PayMonths,NULL AS PaymentMode,CAST(RefundID AS NVARCHAR(50)) AS PaymentRecieptNo,
  'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
    (Select SchoolUID from StudentMaster where StudentID=RefundTo) as SchoolUID,
 (Select Name from StudentMaster where StudentID=RefundTo) as Name,
 SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=RM.SessionID) as SessionName
     from RefundMaster RM left outer join Student_Session SS on RM.RefundTo=SS.StudentID and SS.SessionID=RM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
    where datepart(month,RM.RefundDate)=datepart(month,@FromDate) and datepart(year,RM.RefundDate)=datepart(year,@FromDate) and RM.SBranchID=@SBranchID

       select PD.PaymentID,PD.FeeTypeID,PD.PaymentRecieved from PaymentDetails PD
     where PD.PaymentID in (select PaymentID from PaymentMaster PM where datepart(month,PM.PaymentDate)=datepart(month,@FromDate) and datepart(year,PM.PaymentDate)=datepart(year,@FromDate) and PM.SBranchID=@SBranchID and PM.PaymentMode=@PaymentMode)
  end
 end

else if(@ReportType=3)--Between Dates
 begin
  if(@PaymentMode=-1)
  begin
  select PM.PaymentID,PaymentAmount,0 as RefundAmount,PM.PaymentDate,PaymentRecieptNo,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PM.PaymentID) PayMonths,PaymentMode,
   'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
      (Select SchoolUID from StudentMaster where StudentID=SS.StudentID) as SchoolUID,
   (Select Name from StudentMaster where StudentID=PM.PayeeID) as Name,SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
    from PaymentMaster PM  left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
    where cast(PM.PaymentDate as date) between @FromDate and @ToDate  and PM.SBranchID=@SBranchID

    union all
      select RefundID as PaymentID,0 as PaymentAmount,RefundAmount,Refunddate as PaymentDate,CAST(RefundBy AS NVARCHAR(50)) as CollectedBy,Reason, NULL AS Remark, NULL AS PayMonths,NULL AS PaymentMode,CAST(RefundID AS NVARCHAR(50)) AS PaymentRecieptNo,
  'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
    (Select SchoolUID from StudentMaster where StudentID=RefundTo) as SchoolUID,
 (Select Name from StudentMaster where StudentID=RefundTo) as Name,
 SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=RM.SessionID) as SessionName
     from RefundMaster RM left outer join Student_Session SS on RM.RefundTo=SS.StudentID and SS.SessionID=RM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
    where cast(RM.RefundDate as date) between @FromDate and @ToDate  and RM.SBranchID=@SBranchID

      select PD.PaymentID,PD.FeeTypeID,PD.PaymentRecieved from PaymentDetails PD
     where PD.PaymentID in (select PaymentID from PaymentMaster PM where cast(PM.PaymentDate as date) between @FromDate and @ToDate  and PM.SBranchID=@SBranchID)
  end
  else
  begin
   select PM.PaymentID,PaymentAmount,0 as RefundAmount,PM.PaymentDate,PaymentRecieptNo,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PM.PaymentID) PayMonths,PaymentMode,
    'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
      (Select SchoolUID from StudentMaster where StudentID=SS.StudentID) as SchoolUID,
   (Select Name from StudentMaster where StudentID=PM.PayeeID) as Name,SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
    from PaymentMaster PM  left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
    where cast(PM.PaymentDate as date) between @FromDate and @ToDate  and PM.SBranchID=@SBranchID and PM.PaymentMode=@PaymentMode

    union all
      select RefundID as PaymentID,0 as PaymentAmount,RefundAmount,Refunddate as PaymentDate,CAST(RefundBy AS NVARCHAR(50)) as CollectedBy,Reason, NULL AS Remark, NULL AS PayMonths,NULL AS PaymentMode,CAST(RefundID AS NVARCHAR(50)) AS PaymentRecieptNo,
  'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
    (Select SchoolUID from StudentMaster where StudentID=RefundTo) as SchoolUID,
 (Select Name from StudentMaster where StudentID=RefundTo) as Name,
 SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=RM.SessionID) as SessionName
     from RefundMaster RM left outer join Student_Session SS on RM.RefundTo=SS.StudentID and SS.SessionID=RM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
    where cast(RM.RefundDate as date) between @FromDate and @ToDate  and RM.SBranchID=@SBranchID

      select PD.PaymentID,PD.FeeTypeID,PD.PaymentRecieved from PaymentDetails PD
     where PD.PaymentID in (select PaymentID from PaymentMaster PM where cast(PM.PaymentDate as date) between @FromDate and @ToDate  and PM.SBranchID=@SBranchID and PM.PaymentMode=@PaymentMode)
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
  select PM.PaymentID,PaymentAmount,0 as RefundAmount,PM.PaymentDate,PaymentRecieptNo,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PM.PaymentID) PayMonths,PaymentMode,
   'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
      (Select SchoolUID from StudentMaster where StudentID=SS.StudentID) as SchoolUID,
   (Select Name from StudentMaster where StudentID=PM.PayeeID) as Name,SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
    from PaymentMaster PM  left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
    where cast(PM.PaymentDate as date) between @FromDate and @ToDate  and PM.SBranchID=@SBranchID

    union all
      select RefundID as PaymentID,0 as PaymentAmount,RefundAmount,Refunddate as PaymentDate,CAST(RefundBy AS NVARCHAR(50)) as CollectedBy,Reason, NULL AS Remark, NULL AS PayMonths,NULL AS PaymentMode,CAST(RefundID AS NVARCHAR(50)) AS PaymentRecieptNo,
  'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
    (Select SchoolUID from StudentMaster where StudentID=RefundTo) as SchoolUID,
 (Select Name from StudentMaster where StudentID=RefundTo) as Name,
 SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=RM.SessionID) as SessionName
     from RefundMaster RM left outer join Student_Session SS on RM.RefundTo=SS.StudentID and SS.SessionID=RM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
    where cast(RM.RefundDate as date) between @FromDate and @ToDate  and RM.SBranchID=@SBranchID

     select PD.PaymentID,PD.FeeTypeID,PD.PaymentRecieved from PaymentDetails PD
     where PD.PaymentID in (select PaymentID from PaymentMaster PM where cast(PM.PaymentDate as date) between @FromDate and @ToDate  and PM.SBranchID=@SBranchID)
  end
  else
  begin
   select PM.PaymentID,PaymentAmount,0 as RefundAmount,PM.PaymentDate,PaymentRecieptNo,CollectedBy,ReferanceNumber,Remark, [dbo].[GetPaymentMonthNames](PM.PaymentID) PayMonths,PaymentMode,
    'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
      (Select SchoolUID from StudentMaster where StudentID=SS.StudentID) as SchoolUID,
   (Select Name from StudentMaster where StudentID=PM.PayeeID) as Name,SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=PM.SessionID) as SessionName
    from PaymentMaster PM  left outer join Student_Session SS on PM.PayeeID=SS.StudentID and SS.SessionID=PM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
    where cast(PM.PaymentDate as date) between @FromDate and @ToDate  and PM.SBranchID=@SBranchID and PM.PaymentMode=@PaymentMode

    union all
      select RefundID as PaymentID,0 as PaymentAmount,RefundAmount,Refunddate as PaymentDate,CAST(RefundBy AS NVARCHAR(50)) as CollectedBy,Reason, NULL AS Remark, NULL AS PayMonths,NULL AS PaymentMode,CAST(RefundID AS NVARCHAR(50)) AS PaymentRecieptNo,
  'STUD'+RIGHT(REPLICATE('0',6)+CAST(SS.StudentID AS VARCHAR(6)),6) as StudentSID,
    (Select SchoolUID from StudentMaster where StudentID=RefundTo) as SchoolUID,
 (Select Name from StudentMaster where StudentID=RefundTo) as Name,
 SS.RollNo,isnull(VCS.ClassName,'')+'\'+isnull(VCS.SectionName,'') as ClassSection,
   (Select SessionName from SessionMaster SMS where SMS.SessionID=RM.SessionID) as SessionName
     from RefundMaster RM left outer join Student_Session SS on RM.RefundTo=SS.StudentID and SS.SessionID=RM.SessionID
    left outer join [v_ClassSectionNames] VCS on VCS.ClassID=SS.ClassID and VCS.SectionID=SS.SectionID
    where cast(RM.RefundDate as date) between @FromDate and @ToDate  and RM.SBranchID=@SBranchID

     select PD.PaymentID,PD.FeeTypeID,PD.PaymentRecieved from PaymentDetails PD
     where PD.PaymentID in (select PaymentID from PaymentMaster PM where cast(PM.PaymentDate as date) between @FromDate and @ToDate  and PM.SBranchID=@SBranchID and PM.PaymentMode=@PaymentMode)
  end
 end
 select * from SBranchMaster where SBranchID=@SBranchID
End
GO
