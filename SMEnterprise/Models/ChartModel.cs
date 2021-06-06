using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class ChartModel
    {
        public List<string> labels { get ; set; }
        public List<DataSetsModel> datasets { get; set; }

    }
    public class DataSetsModel
    {
        public string label { get; set; }
        public string backgroundColor { get; set; }
        public string borderColor { get; set; }
        public string pointBorderColor { get; set; }
        public string pointBackgroundColor { get; set; }
        public string pointHoverBackgroundColor { get; set; }
        public string pointHoverBorderColor { get; set; }
        public int pointBorderWidth = 1;
        public List<decimal> data { get; set; }
    }
}