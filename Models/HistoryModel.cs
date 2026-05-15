using System;
using System.Collections.Generic;

namespace Api.Models;

/// <summary>
/// HistoryModel
/// </summary>
public class HistoryModel
{
    public string AccessionNum { get; set; }
    public string PatientID { get; set; }
    public string Lines { get; set; }
    public string TextType { get; set; }
    public string TextCH { get; set; }
    public string TextEN { get; set; }
    public DateTime ReportUpdataTm { get; set; }
    public string ScheduledPerformingPhysician { get; set; }
    public string VerificationPhysician { get; set; }
    public DateTime ReportFinishTm { get; set; }
    public string RecommendReport { get; set; }
    public DateTime StudyDateTime { get; set; }
    public string Modality { get; set; }
    public string ItemNm { get; set; }
    public List<string> Thumbnails { get; set; }
}


/// <summary>
/// HistoryListModel
/// </summary>
public class HistoryListModel
{
    public int Count { get; set; }
    public IList<HistoryModel> Items { get; set; }
}
