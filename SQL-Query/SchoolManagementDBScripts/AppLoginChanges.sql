Alter Table AppUsers
Add DynamicSalt nvarchar(10)
GO
CREATE procedure [dbo].[spn_InsertUpdateAppUser]   
(    
@UserType int,    
@UserID int,    
@FCMToken nvarchar(max),    
@UUID nvarchar(50),    
@LastActive Datetime,    
@SBranchID int,    
@OpType int  ,  
@DynamicSalt nvarchar(20)  
)    
as     
begin     
    
 if(@OpType=-1)    
 begin    
  --insert into [TextTable] values(@FCMToken+'$$'+cast(@UserType as nvarchar(10))+'@@'+Cast(@UserID as nvarchar(10)))    
  declare @NewTokens nvarchar(max)    
  select @NewTokens=replace(replace(replace(FCMToken,'#'+@FCMToken,''),@FCMToken+'#',''),@FCMToken,'') from AppUsers where UserID=@UserID and UserType=@UserType    
  if(@NewTokens='')    
  begin    
   Delete from AppUsers where UserID=@UserID and UserType=@UserType    
  end    
  else    
  begin    
   update AppUsers set FCMToken='' where UserID=@UserID and UserType=@UserType    
end    
 end    
 else     
 begin    
  declare @IsExist int    
  select @IsExist=count(*) from AppUsers where (UUID=@UUID)   
  if(@IsExist=0)    
  begin    
   Insert into AppUsers(UserType,UserID,UUID,Status,LastActive,FCMToken,SBranchID,DynamicSalt)    
   values(@UserType,@UserID,@UUID,1,@LastActive,@FCMToken,@SBranchID,@DynamicSalt)    
  end    
  else    
  begin    
   declare @Token nvarchar(max)    
   select @Token=FCMToken from AppUsers where UUID=@UUID  
   if(isnull(@Token,'')<>'' and  @Token not like '%'+@FCMToken+'%')    
   begin    
    set @FCMToken=@FCMToken+'#'+@Token    
   end    
   else if( @Token like '%'+@FCMToken+'%')    
   begin    
    set @FCMToken=@Token    
   end    
   Update AppUsers set UserType=@UserType,UUID=@UUID,UserID=@UserID,Status=1,LastActive=@LastActive,  
   FCMToken=@FCMToken,SBranchID=@SBranchID,DynamicSalt=@DynamicSalt    
   where  UUID=@UUID  
  end    
 end    
end    
GO
  
  
  
  