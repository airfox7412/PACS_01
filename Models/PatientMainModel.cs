using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Api.Models
{
    /// <summary>
    /// ????
    /// </summary>
    public partial class PatientMainModel {

        [Key]
        [Required()]
        public int PatientMainNo { get; set; }

        [StringLength(20)]
        [Required()]
        public string PatientID { get; set; }

        [StringLength(20)]
        [Required()]
        public string PatientName { get; set; }

        public DateTime? PatientBirthDate { get; set; }

        [StringLength(10)]
        [Required()]
        public string PatientSex { get; set; }

        [StringLength(10)]
        public string OtherPatientID { get; set; }
    }

    /// <summary>
    /// ????
    /// </summary>
    public partial class PatientMainNewModel
    {
        [StringLength(20)]
        [Required()]
        public string PatientID { get; set; }

        [StringLength(20)]
        [Required()]
        public string PatientName { get; set; }

        public DateTime? PatientBirthDate { get; set; }

        [StringLength(10)]
        [Required()]
        public string PatientSex { get; set; }
    }
}
