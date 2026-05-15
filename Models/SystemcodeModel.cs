using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    /// <summary>
    /// 基本資料代碼表
    /// </summary>
    public partial class SystemcodeModel 
    {
        /// <summary>
        /// 類型
        /// </summary>
        [Key]
        [Required()]
        public int Category { get; set; }

        /// <summary>
        /// 代碼
        /// </summary>
        [Key]
        [StringLength(20)]
        [Required()]
        public string Code { get; set; }

        /// <summary>
        /// 顯示文字
        /// </summary>
        [StringLength(100)]
        [Required()]
        public string Display { get; set; }
    }

}
