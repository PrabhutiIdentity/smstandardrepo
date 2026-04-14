Create procedure dbo.tsp_GetTransportLocationMode  
(  
 @SBranchID int  
)  
AS  
Begin  
  select isnull(TransportLocationMode,'1') from SBranchMaster where SBranchID=@SBranchID    
END
go

CREATE proc [dbo].[spn_GetStudentRouteStoppages]   
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
go
  
  
CREATE proc [dbo].[spn_GetEmployeeRouteStoppages]     
(        
 @EmployeeID int  ,      
 @CurDate datetime      
)        
AS        
BEGIN        
 declare @VehicleRouteID int      
 select @VehicleRouteID=VehicleRouteID from EmployeeMaster where EmployeeID=@EmployeeID      
      
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