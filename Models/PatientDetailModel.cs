using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Api.Models
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PatientDetailModel {
        public long DetailNo { get; set; }
        public string StudyID { get; set; }
        public string PatientID { get; set; }
        public int? SeriesNumber { get; set; }
        public DateTime? StudyDate { get; set; }
        public DateTime? ContentDate { get; set; }
        public TimeSpan? StudyTime { get; set; }
        public TimeSpan? ContentTime { get; set; }
        public string Modality { get; set; }
        public string OperatorName { get; set; }
        public string BodyPartExamined { get; set; }
        public string ProtocolName { get; set; }
        public string AccessionNumber { get; set; }

        public sbyte? InstanceNumber { get; set; }
        public string Status { get; set; }
        public string AdditionalPatientHistory { get; set; }
    }
}
