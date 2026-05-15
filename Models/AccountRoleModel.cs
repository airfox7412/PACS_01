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
    /// 帳號角色對應清單
    /// </summary>
    public partial class AccountRoleModel
    {
        
        /// <summary>
        /// 對應 account.no
        /// </summary>
        [Key]
        [StringLength(30)]
        [Required()]
        public string AccountNo { get; set; }

        /// <summary>
        /// 對應 role
        /// </summary>
        [Key]
        [StringLength(30)]
        [Required()]
        public string RoleNo { get; set; }
    }

}
