using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Api.Models
{
    /// <summary>
    /// 
    /// </summary>
    public partial class UserProfileModel {

        [Key]
        [Required()]
        public int UserProfileNo { get; set; }

        [StringLength(10)]
        [Required()]
        public string UserID { get; set; }

        [StringLength(32)]
        [Required()]
        public string PassWord { get; set; }

        [StringLength(30)]
        [Required()]
        public string UserName { get; set; }

        [Required()]
        public int Level { get; set; }

        [Required()]
        public int DepartNo { get; set; }

        [StringLength(1)]
        [Required()]
        public string Status { get; set; }
    }

}
