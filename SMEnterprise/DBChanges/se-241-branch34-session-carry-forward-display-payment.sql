/*
Branch 34 only patch for Session-Carry-forward display and payment flow.
Review carefully before executing.

Objects updated in this script:
- dbo.sp_GetClassGroupFeeListOnly
- dbo.sp_GetStudentFeeDetailsNew
- dbo.sp_GetStudentFeeDetailViewData
- dbo.spn_SaveFeePaymentV2

Behavior:
- Applies only when @SBranchID = 34
- Reads Session-Carry-forward as an additive fixed fee
- Does not alter quota percentage discount logic for regular class fees
- Treats carry-forward as payable in the target session start month
- Blocks old-session collection once carry-forward mapping exists
*/

/* -------------------------------------------------------------------------- */
/* 1. sp_GetClassGroupFeeListOnly                                              */
/* -------------------------------------------------------------------------- */
/*
Patch instructions:

1) Add these declarations near the existing variable declarations:

    declare @CarryForwardFeeTypeID int = 0
    declare @CarryForwardAmount numeric(10,2) = 0

2) After loading student session variables inside the per-student loop and after
   @StudentSessionUID is available, add:

    if(@SBranchID=34)
    begin
        select top 1 @CarryForwardFeeTypeID = FeeTypeID
        from FeeTypeMaster
        where SBranchID=@SBranchID
          and FeeTypeApplicable=1
          and ltrim(rtrim(FeeTypeName))='Session-Carry-forward'

        select @CarryForwardAmount = isnull(FeeAmount,0)
        from StudentFeeDetails
        where StudentID=@StudentID
          and SessionID=@StudentSessionUID
          and FeeTypeID=@CarryForwardFeeTypeID
          and IsApplicable=1

        if(isnull(@CarryForwardAmount,0)<0)
        begin
            set @CarryForwardAmount=0
        end
    end

3) Just before the final UPDATE @Students SET ... in the loop, add:

    if(@SBranchID=34 and isnull(@CarryForwardAmount,0)>0)
    begin
        set @PreviousDue = isnull(@PreviousDue,0) + isnull(@CarryForwardAmount,0)
    end

Notes:
- This keeps existing regular fee + quota discount logic unchanged.
- Carry-forward will appear in the top summary as part of previous due.
*/

/* -------------------------------------------------------------------------- */
/* 2. sp_GetStudentFeeDetailsNew                                              */
/* -------------------------------------------------------------------------- */
/*
Patch instructions:

1) Add declarations near the top, after @StudentSessionUID:

    ,@CarryForwardFeeTypeID int,@CarryForwardAmount numeric(10,2)

2) After this query:
      from Student_Session SS
      where SS.StudentID=@StudentID and SessionID=@SessionID
   add:

    set @CarryForwardFeeTypeID = 0
    set @CarryForwardAmount = 0

    if(@SBranchID=34)
    begin
        select top 1 @CarryForwardFeeTypeID = FeeTypeID
        from FeeTypeMaster
        where SBranchID=@SBranchID
          and FeeTypeApplicable=1
          and ltrim(rtrim(FeeTypeName))='Session-Carry-forward'

        select @CarryForwardAmount = isnull(FeeAmount,0)
        from StudentFeeDetails
        where StudentID=@StudentID
          and SessionID=@StudentSessionUID
          and FeeTypeID=@CarryForwardFeeTypeID
          and IsApplicable=1
    end

3) After the while loop completes, and before the final grouped SELECT result,
   append the carry-forward row into @FeeDetailTable:

    if(@SBranchID=34 and isnull(@CarryForwardAmount,0)>0 and isnull(@CarryForwardFeeTypeID,0)>0)
    begin
        insert into @FeeDetailTable
        (
            FeeMonth,FeeYear,FeeTypeApplicable,FeeTypeID,FeeTypeName,
            FeeAmount,QDiscount,RDiscount,PayApplicableAmount,
            CustomFee,PaidAmount,IsCustomFee,IsPayment
        )
        select
            month(@SessionStartDate),
            year(@SessionStartDate),
            1,
            FTM.FeeTypeID,
            FTM.FeeTypeName,
            @CarryForwardAmount,
            0,
            0,
            isnull(PD.NetApplicablePayment,@CarryForwardAmount),
            @CarryForwardAmount,
            isnull(PD.PaymentRecieved,0),
            1,
            case when PD.PaymentID is null then 0 else 1 end
        from FeeTypeMaster FTM
        outer apply
        (
            select
                min(PaymentID) as PaymentID,
                sum(isnull(NetApplicablePayment,0)) as NetApplicablePayment,
                sum(isnull(PaymentRecieved,0)) as PaymentRecieved
            from v_PaymentDetails
            where PayeeID=@StudentID
              and FeeTypeID=@CarryForwardFeeTypeID
              and Month=month(@SessionStartDate)
              and Year=year(@SessionStartDate)
        ) PD
        where FTM.FeeTypeID=@CarryForwardFeeTypeID
    end

Notes:
- QDiscount is forced to 0 so sibling quota will not reduce carry-forward.
- Carry-forward appears as a separate line item in detail view.
*/

/* -------------------------------------------------------------------------- */
/* 3. sp_GetStudentFeeDetailViewData                                          */
/* -------------------------------------------------------------------------- */
/*
No direct special logic required if sp_GetStudentFeeDetailsNew is patched as above.
This procedure consumes @FeeDetailTable output from sp_GetStudentFeeDetailsNew,
so carry-forward should automatically appear in:
- fee type summary
- month summary
- detailed line items
*/

/* -------------------------------------------------------------------------- */
/* 4. spn_SaveFeePaymentV2                                                    */
/* -------------------------------------------------------------------------- */
/*
Patch instructions:

1) Near the top, after existing declarations, add:

    declare @CFBlocked bit = 0
    declare @CFMessage nvarchar(500) = N''

2) After session dates are loaded, add:

    if(@SBranchID=34)
    begin
        exec dbo.sp_ValidateCarryForwardPaymentLock
            @SBranchID = @SBranchID,
            @StudentID = @StudentID,
            @SessionID = @SessionID,
            @IsBlocked = @CFBlocked output,
            @Message = @CFMessage output

        if(isnull(@CFBlocked,0)=1)
        begin
            raiserror(@CFMessage,16,1)
            return
        end
    end

3) No further logic changes needed in payment allocation if sp_GetStudentFeeDetailsNew
   has been patched, because this proc loads payable rows from that proc:

       insert into @FeeDetailTable
       exec sp_GetStudentFeeDetailsNew ...

Notes:
- Old session collection remains normal for all branches except where branch 34
  mapping exists.
- New session carry-forward becomes payable as part of standard allocation flow.
*/
