using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    /// <summary>
    /// 稽核事件
    /// </summary>
    public partial class AuditEventModel
    {

        /// <summary>
        /// pKey
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 資料類別
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 資料 pKey
        /// </summary>
        public string DataKey { get; set; }

        /// <summary>
        /// 資料內容
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 使用者帳號
        /// </summary>
        public string AccountNo { get; set; }

        /// <summary>
        /// 用戶端IP
        /// </summary>
        public string HostAddress { get; set; }

        /// <summary>
        /// 執行動作
        /// </summary>
        public string ProcessStack { get; set; }

        /// <summary>
        /// 記錄類型
        /// </summary>
        public int? Type { get; set; }

        /// <summary>
        /// 記錄類型名稱
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 機構名稱
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 使用者姓名
        /// </summary>
        public string AccountName { get; set; }
    }

    /// <summary>
    /// Body DateModel
    /// </summary>
    public class DateModel
    {
        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
