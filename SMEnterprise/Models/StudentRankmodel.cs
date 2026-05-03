using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class StudentTermRankModel
    {
        public int EvaluationID { get; set; }
        public string EvaluationName { get; set; }
        public decimal MarksScored { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal Percentage { get; set; }
        public int RankInSection { get; set; }
        public int TotalStudentsInSection { get; set; }
        public string RankDisplay
        {
            get
            {
                switch (RankInSection)
                {
                    case 1: return "1st";
                    case 2: return "2nd";
                    case 3: return "3rd";
                    default: return RankInSection + "th";
                }
            }
        }
    }

    public class StudentOverallRankModel
    {
        public decimal MarksScored { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal Percentage { get; set; }
        public int OverallRankInSection { get; set; }
        public int TotalStudentsInSection { get; set; }
        public string RankDisplay
        {
            get
            {
                switch (OverallRankInSection)
                {
                    case 1: return "1st";
                    case 2: return "2nd";
                    case 3: return "3rd";
                    default: return OverallRankInSection + "th";
                }
            }
        }
    }
}