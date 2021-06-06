using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class SyllabusModel
    {
        public List<ClassModel> Classes { get; set; }
        public List<GroupModel> Groups { get; set; }
        public List<SubjectModel> Subjects { get; set; }
        public List<ChapterModel> Chapters { get; set; }
        public int ClassID { get; set; }
        public int GroupID { get; set; }
        public int SubjectID { get; set; }
        public int SBranchID { get; set; }
    }
    public class ChapterModel
    {
        public int ChapterID { get; set; }
        public int ClassID { get; set; }
        public int GroupID { get; set; }
        public int SubjectID { get; set; }
        public string ChapterName { get; set; }
        public string ChapterDescription { get; set; }
        public int IsApproved { get; set; }
        public int SequenceNo { get; set; }
        public int UserID { get; set; }
        public DateTime OperationDate { get; set; }
        public int SBranchID { get; set; }
        public int ChTopics { get; set; }
        public int OpType { get; set; }
        public List<TopicModel> Topics { get; set; }
        public List<EvaluationModel> Evaluations { get; set; }

    }
    public class TopicModel
    {
        public int TopicID { get; set; }
        public int ChapterID { get; set; }
        public string TopicName { get; set; }
        public int SequenceNo { get; set; }
        public int Marks { get; set; }
        public decimal Duration { get; set; }
        public int OpType { get; set; }
        public int EvaluationID { get; set; }
    }
}