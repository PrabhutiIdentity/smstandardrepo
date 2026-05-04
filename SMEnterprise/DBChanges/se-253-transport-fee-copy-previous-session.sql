/*
Fix: Transport Fee Management should auto-carry route stop amounts to a new session.

Scenario:
- Branch 34 had transport fee rows in session 2025-26.
- New session 2026-27 is created.
- Transport Fee Management for 2026-27 shows no rows, so student transport fee becomes 0
  until rows are manually copied into TransportFeeMaster.

Change:
- sp_GetFeeForTransport now self-heals the selected session/route.
- It creates current-session rows from the current route stop list.
- For each current stop, amount is copied from the previous session when the same stop or
  same route/city/area existed before; otherwise route stop Rate/0 is used.
- If there is no previous session data, it still creates/display route stops with amount 0,
  so the user can enter and save values.
- Existing current-session rows are never overwritten; user changes stay as-is.
*/

IF OBJECT_ID(N'dbo.sp_GetFeeForTransport', N'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_GetFeeForTransport AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[sp_GetFeeForTransport]
(
    @RouteID int = 0,
    @SBranchID int,
    @SessionID int = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    IF (ISNULL(@SessionID,0) = 0)
    BEGIN
        SELECT TOP 1 @SessionID = SessionID
        FROM SessionMaster
        WHERE SBranchID = @SBranchID
        ORDER BY SessionStatus DESC, SessionStartDate DESC, SessionID DESC;
    END;

    IF (ISNULL(@RouteID,0) = 0)
    BEGIN
        SELECT TOP 1 @RouteID = RouteID
        FROM TransportRouteMaster
        WHERE SBranchID = @SBranchID
        ORDER BY RouteName, RouteID;
    END;

    DECLARE @SessionStartDate date;
    DECLARE @PreviousSessionID int;

    SELECT @SessionStartDate = SessionStartDate
    FROM SessionMaster
    WHERE SessionID = @SessionID
      AND SBranchID = @SBranchID;

    SELECT TOP 1 @PreviousSessionID = SessionID
    FROM SessionMaster
    WHERE SBranchID = @SBranchID
      AND SessionID <> @SessionID
      AND SessionEndDate < ISNULL(@SessionStartDate, '99991231')
    ORDER BY SessionEndDate DESC, SessionID DESC;

    INSERT INTO TransportFeeMaster
    (
        StopID,
        RouteID,
        CityID,
        AreaID,
        Amount,
        SBranchID,
        SessionID
    )
    SELECT
        TRD.StopID,
        TRD.RouteID,
        TRD.CityID,
        TRD.AreaID,
        COALESCE(PrevByStop.Amount, PrevByArea.Amount, TRD.Rate, 0),
        @SBranchID,
        @SessionID
    FROM TransportRouteDetails TRD
    INNER JOIN TransportRouteMaster TRM
        ON TRM.RouteID = TRD.RouteID
    OUTER APPLY
    (
        SELECT TOP 1 P.Amount
        FROM TransportFeeMaster P
        WHERE P.SBranchID = @SBranchID
          AND P.SessionID = @PreviousSessionID
          AND P.RouteID = TRD.RouteID
          AND P.StopID = TRD.StopID
        ORDER BY P.ID DESC
    ) PrevByStop
    OUTER APPLY
    (
        SELECT TOP 1 P.Amount
        FROM TransportFeeMaster P
        WHERE P.SBranchID = @SBranchID
          AND P.SessionID = @PreviousSessionID
          AND P.RouteID = TRD.RouteID
          AND ISNULL(P.CityID,0) = ISNULL(TRD.CityID,0)
          AND ISNULL(P.AreaID,0) = ISNULL(TRD.AreaID,0)
        ORDER BY P.ID DESC
    ) PrevByArea
    WHERE TRM.SBranchID = @SBranchID
      AND TRD.RouteID = @RouteID
      AND NOT EXISTS
      (
          SELECT 1
          FROM TransportFeeMaster TFM
          WHERE TFM.SBranchID = @SBranchID
            AND TFM.SessionID = @SessionID
            AND TFM.RouteID = TRD.RouteID
            AND TFM.StopID = TRD.StopID
      );

    SELECT
        TFM.ID,
        TFM.StopID,
        TFM.RouteID,
        TRM.RouteName,
        TFM.CityID,
        CM.CityName,
        TFM.AreaID,
        AM.AreaName,
        TFM.Amount,
        TFM.SBranchID,
        TFM.SessionID,
        GETDATE() AS CreatedDate,
        GETDATE() AS ModifiedDate
    FROM TransportFeeMaster TFM
    LEFT JOIN TransportRouteMaster TRM
        ON TRM.RouteID = TFM.RouteID
    LEFT JOIN CityMaster CM
        ON CM.CityID = TFM.CityID
    LEFT JOIN AreaMaster AM
        ON AM.AreaID = TFM.AreaID
    WHERE TFM.SBranchID = @SBranchID
      AND TFM.SessionID = @SessionID
      AND TFM.RouteID = @RouteID
    ORDER BY TFM.ID, TFM.StopID;

    SELECT ISNULL(@SessionID,0);

    SELECT
        SessionID,
        SessionStartDate,
        SessionEndDate,
        SessionStatus,
        SessionName,
        SBranchID
    FROM SessionMaster
    WHERE SBranchID = @SBranchID
    ORDER BY SessionStartDate DESC, SessionID DESC;

    SELECT
        RouteID,
        RouteName,
        SBranchID
    FROM TransportRouteMaster
    WHERE SBranchID = @SBranchID
    ORDER BY RouteName, RouteID;

    SELECT ISNULL(@RouteID,0);

    SET NOCOUNT OFF;
END
GO
