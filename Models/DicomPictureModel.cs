using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace Api.Models
{
    /// <summary>
    /// 影像圖檔
    /// </summary>
    public partial class DicomPictureModel {

        [Key]
        [Required()]
        public int Id { get; set; }

        [StringLength(15)]
        public string StudyID { get; set; }

        [StringLength(255)]
        public string Image { get; set; }
    }

}
