using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Api.Models
{
    /// <summary>
    /// 報告
    /// </summary>
    public partial class ExamDataModel
    {

        [Key]
        [Required()]
        public long ExamDataNo { get; set; }

        [StringLength(15)]
        public string StudyID { get; set; }

        public DateTime? ExamDate { get; set; }

        public TimeSpan? ExamTime { get; set; }

        public string Memo1 { get; set; }

        public string AdditionalPatientHistory { get; set; }
    }
}
