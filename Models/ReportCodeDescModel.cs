using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Api.Models
{
    public partial class ReportCodeDescModel {

        [Key]
        [Required()]
        public int ReportCodeDescNo { get; set; }

        [StringLength(64)]
        [Required()]
        public string ReportCode { get; set; }

        public string ReportDesc { get; set; }

        [StringLength(255)]
        [Required()]
        public string UserID { get; set; }
    }

}
