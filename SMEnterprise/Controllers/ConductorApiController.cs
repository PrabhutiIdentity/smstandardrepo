using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SMEnterprise.Models;
using SMEnterprise.Repository;

namespace SMEnterprise.Controllers
{
    public class ConductorApiController : ApiController
    {
        [HttpPost]
        public CommonApiWraperModel UpdatePassengerAttendance(PassengerAttandanceModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            try
            {
                data.CDate = CommonUsage.GetCurrentDate();
                objWraper.Data = (new ConductorData()).UpdatePassengerAttandance(data);
                objWraper.Code = 200;
                objWraper.Message = "Success";
            }
            catch (Exception ex)
            {
                objWraper.Code = 404;
                objWraper.Message = ex.ToString();
            }

            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetConductorVehicleRoute(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            try
            {
                ConductorStopsScreenModel oModel = (new ConductorData()).GetConductorVehicleRoutes(data.ID, CommonUsage.GetCurrentDate());
                foreach (var vr in oModel.Routes)
                {
                    List<RouteStoppageModel> stops = (new ConductorData()).GetConductorRouteDetails(vr.VehicleRouteID);
                    var stopsdone = VehicleStopData.SearchDefault("", CommonUsage.GetCurrentDate().ToString("yyyy-MM-dd"), vr.VehicleRouteID.ToString()).ToList();
                    foreach (var s in stops)
                    {
                        if (stopsdone.Where(x => x.StopID == s.StopID).Count() > 0)
                        {
                            s.IsDone = 2;
                        }
                        else
                        {
                            s.IsDone = 0;
                        }
                    }
                    if (stops.Where(x => x.IsDone == 2).Count() > 0)
                    {
                        if (stops.Where(x => x.IsDone == 0).Count() > 0)
                        {
                            vr.IsCompleted = 1;
                        }
                        else
                        {
                            vr.IsCompleted = 2;
                        }
                    }
                    else
                    {
                        vr.IsCompleted = 0;
                    }
                }
                objWraper.List = oModel.Routes.ToList<object>();
                objWraper.Code = 200;
                objWraper.Data = oModel.Frequency_Mode;
                objWraper.Message = "Success";
            }
            catch (Exception ex)
            {
                objWraper.Code = 404;
                objWraper.Message = ex.ToString();
            }

            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetConductorRoutePassengers(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            try
            {
                DateTime cDate = CommonUsage.GetCurrentDate();
                objWraper.List = (new ConductorData()).GetConductorRoutePassengers(data.ID, cDate);
                objWraper.Code = 200;
                objWraper.Message = "Success";
            }
            catch (Exception ex)
            {
                objWraper.Code = 404;
                objWraper.Message = ex.ToString();
            }

            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel GetRouteStoppages(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            try
            {
                List<RouteStoppageModel> stops = (new ConductorData()).GetConductorRouteDetails(data.ID);
                var stopsdone = VehicleStopData.SearchDefault("", CommonUsage.GetCurrentDate().ToString("yyyy-MM-dd"), data.ID.ToString()).ToList();
                var isFirst = 0;
                foreach (var s in stops)
                {
                    if (stopsdone.Where(x => x.StopID == s.StopID).Count() > 0)
                    {
                        s.IsDone = 2;
                    }
                    else
                    {
                        if (isFirst == 0)
                        {
                            s.IsDone = 1;
                            isFirst = 1;
                        }
                        else
                        {
                            s.IsDone = 0;
                        }
                    }
                }
                objWraper.List = stops.ToList<object>();
                objWraper.Code = 200;
                objWraper.Message = "Success";
            }
            catch (Exception ex)
            {
                objWraper.Code = 404;
                objWraper.Message = ex.ToString();
            }

            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel ClearVehicleStops(ParentApiParamModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            try
            {
                VehicleStopData.ClearLuceneIndex();
                objWraper.Code = 200;
                objWraper.Message = "Success";
            }
            catch (Exception ex)
            {
                objWraper.Code = 404;
                objWraper.Message = ex.ToString();
            }

            return objWraper;
        }

        [HttpGet]
        public CommonApiWraperModel GetLocationHistory()
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            try
            {
                objWraper.List = GioData.GetAllIndexRecords().ToList<object>();
                objWraper.Code = 200;
                objWraper.Message = "Success";
            }
            catch (Exception ex)
            {
                objWraper.Code = 404;
                objWraper.Message = ex.ToString();
            }

            return objWraper;
        }
        [HttpGet]
        public CommonApiWraperModel GetLocationHistoryODate(string qDate = null)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            try
            {
                objWraper.Data = GioData.SearchDefault("", qDate).OrderByDescending(o => Convert.ToDateTime(qDate + " " + o.GioTime)).ToList<object>().FirstOrDefault();
                //objWraper.List = GioData.GetAllIndexRecords().OrderByDescending(c => Convert.ToDateTime(c.GioDate + " " + c.GioTime)).ToList<object>();
                objWraper.Code = 200;
                objWraper.Message = "Success";
            }
            catch (Exception ex)
            {
                objWraper.Code = 404;
                objWraper.Message = ex.ToString();
            }

            return objWraper;
        }
        [HttpGet]
        public CommonApiWraperModel GetLocationHistoryO()
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            try
            {
                objWraper.List = GioData.GetAllIndexRecords().OrderByDescending(c => Convert.ToDateTime(c.GioDate + " " + c.GioTime)).ToList<object>();
                objWraper.Code = 200;
                objWraper.Message = "Success";
            }
            catch (Exception ex)
            {
                objWraper.Code = 404;
                objWraper.Message = ex.ToString();
            }

            return objWraper;
        }
        [HttpPost]
        public CommonApiWraperModel UpdateVehicleLocation(TransportGeoModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            try
            {
                DateTime cDate = CommonUsage.GetCurrentDate();
                data.GioDate = cDate.ToString("yyyy-MM-dd");
                data.GioTime = cDate.ToString("HH:mm:ss");
                GioData.AddUpdateLuceneIndex(data);
                objWraper.Code = 200;
                objWraper.Message = "Success";
            }
            catch (Exception ex)
            {
                objWraper.Code = 404;
                objWraper.Message = ex.InnerException.ToString();
            }

            return objWraper;
        }

        [HttpPost]
        public CommonApiWraperModel UpdateStop(TransportStopUpdateModel data)
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();

            try
            {
                data.UDate = CommonUsage.GetCurrentDate();
                VehicleStopData.AddUpdateLuceneIndex(data);
                objWraper.Code = 200;
                objWraper.Message = "Success";
            }
            catch (Exception ex)
            {
                objWraper.Code = 404;
                objWraper.Message = ex.InnerException.ToString();
            }

            return objWraper;
        }
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}