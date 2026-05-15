using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Api.Models
{
    /// <summary>
    /// ExamineModel
    /// </summary>
    public class ExamineModel
    {
        /// <summary>
        /// StudyID
        /// </summary>
        public string StudyID { get; set; }
        /// <summary>
        /// 病歷號
        /// </summary>
        public string PatientID { get; set; }
        /// <summary>
        /// InstanceNumber
        /// </summary>
        public sbyte? InstanceNumber { get; set; }

        /// <summary>
        /// AccessionNumber
        /// </summary>
        public string AccessionNo { get; set; }
        /// <summary>
        /// 檢查日期時間
        /// </summary>
        public DateTime StudyDateTime { get; set; }

        /// <summary>
        /// 儀器類別
        /// </summary>
        public string Modality { get; set; }
        /// <summary>
        /// 檢查部位
        /// </summary>
        public string BodyPartExamined { get; set; }

        /// <summary>
        /// 上傳檔案
        /// </summary>
        public IList<IFormFile> Files { get; set; }
    }

    /// <summary>
    /// 輸出報告
    /// </summary>
    public class ExamDataReadModel
    {
        /// <summary>
        /// 工作單號
        /// </summary>
        public string Accession_num { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Patient_ID { get; set; }        
        /// <summary>
        /// 第幾行, 如果只有一行就1
        /// </summary>
        public string Lines { get; set; }
        /// <summary>
        /// 1診斷,2建議
        /// </summary>
        public string Text_Type { get; set; }
        /// <summary>
        /// 中文報告
        /// </summary>
        public string Text_CH { get; set; }
        /// <summary>
        /// 英文報告
        /// </summary>
        public string Text_EN { get; set; }
        /// <summary>
        /// 報告更新日期, yyyy-MM-DD
        /// </summary>
        public string Report_Updata_Tm { get; set; }
        /// <summary>
        /// 報告認證醫師（姓名）
        /// </summary>
        public string ScheduledPerformingPhysician { get; set; }
        /// <summary>
        /// 報告認證醫師（帳號）
        /// </summary>
        public string verification_physician { get; set; }
        /// <summary>
        /// 報告完成時間, yyyy-MM-DD 24hh:mm:ss
        /// </summary>
        public string Report_Finish_Tm { get; set; }

        /// <summary>
        /// 建議的報告
        /// </summary>
        public string RecommendReport { get; set; }
    }
}
