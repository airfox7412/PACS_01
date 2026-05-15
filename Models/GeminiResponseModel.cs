using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

// 定義對應的類別 (放在 class 外面)
public class GeminiResponseModel
{
    [JsonProperty("candidates")]
    public Candidate[] Candidates { get; set; }
}

public class Candidate
{
    [JsonProperty("content")]
    public Content Content { get; set; }
}

public class Content
{
    [JsonProperty("parts")]
    public Part[] Parts { get; set; }
}

public class Part
{
    [JsonProperty("text")]
    public string Text { get; set; }
}