using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Api.Models
{
    public partial class ReportSpecialWordModel {

        [Key]
        [Required()]
        public int ReportSpecialWordNo { get; set; }

        [StringLength(10)]
        public string ReportSpecialCode { get; set; }

        public int? ReportSpecialGrade { get; set; }

        [StringLength(255)]
        public string ReportSpecialWord1 { get; set; }

        [StringLength(255)]
        public string UserID { get; set; }
    }

}
