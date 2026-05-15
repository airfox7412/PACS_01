using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Api.Repositories;
using Newtonsoft.Json;

namespace Api.Library;

/// <summary>
/// 共用基本資料
/// </summary>
public static class CommonHelper
{
    /// <summary>
    /// TimeSpan時間轉換
    /// </summary>
    /// <param name="ts"></param>
    /// <returns></returns>
    public static string TimeSpanFormat(TimeSpan ts)
    {
        return $"{ts:d\\:hh\\:mm\\:ss}";
    }

    public static string TransPadLeft(string date)
    {
        var sstr = date.Split('.');
        var syear = sstr[0].PadLeft(3, '0');
        var smonth = sstr[1].PadLeft(2, '0');
        return syear + smonth;
    }

    public static string GetSerialNo(string SerialNo)
    {
        if (SerialNo == null) return "";
        var sno = int.Parse(SerialNo.Substring(6, 2)) + 1;
        var snoStr = SerialNo[..5] + sno.ToString().PadLeft(2, '0');
        return snoStr;

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Age"></param>
    /// <returns></returns>
    public static string GetAgeRange(int Age)
    {
        var AgeRange = Age switch
        {
            <= 9 => "0-9",
            <= 19 => "10-19",
            <= 29 => "20-29",
            <= 39 => "30-39",
            <= 49 => "40-49",
            <= 59 => "50-59",
            <= 69 => "60-69",
            <= 79 => "70-79",
            <= 89 => "80-89",
            <= 99 => "90-99",
            <= 109 => "100-109",
            <= 119 => "110-119",
            <= 129 => "120-129",
            <= 139 => "130-139",
            _ => ""
        };
        return AgeRange;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static string GetPurpose(string code)
    {
        var name = code switch
        {
            "1" => "急診治療",
            "2" => "住院治療",
            "3" => "門診治療",
            "4" => "進一步檢查，檢查項目",
            "5" => "轉回轉出或適當之院所繼續追蹤",
            "6" => "其他",
            "7" => "加護病房治療(緊急傷病患限定)",
            "8" => "高危險妊娠、早產兒與新生兒治療(緊急傷病患限定)",
            "9" => "COVID-19 個案（含疑似）轉診治療",
            _ => ""
        };
        return name;
    }

    /// <summary>
    /// 取得類別名稱
    /// </summary>
    /// <param name="catagory"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static string GetSystemDisplay(int catagory, string code)
    {
        var scRep = new SystemcodeRepository();
        return scRep.DataSet.FirstOrDefault(c => c.Category == catagory && c.Code == code)?.Display;
    }

    /// <summary>
    /// 身分證號檢核
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool CheckTWId(string id)
    {
        id = id.ToUpper();
        // 使用「正規表達式」檢驗格式 [A~Z] {1}個數字 [0~9] {9}個數字
        var regex = new Regex("^[A-Z]{1}[0-9]{9}$");
        if (!regex.IsMatch(id))
        {
            //Regular Expression 驗證失敗，回傳 ID 錯誤
            return false;
        }

        //除了身分證數字的存放空間 
        int[] seed = new int[10];

        //建立字母陣列(A~Z)
        //A=10 B=11 C=12 D=13 E=14 F=15 G=16 H=17 J=18 K=19 L=20 M=21 N=22
        //P=23 Q=24 R=25 S=26 T=27 U=28 V=29 X=30 Y=31 W=32  Z=33 I=34 O=35            
        string[] charMapping = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "S", "T", "U", "V", "X", "Y", "W", "Z", "I", "O" };
        string target = id.Substring(0, 1); //取第一個英文數字
        for (int index = 0; index < charMapping.Length; index++)
        {
            if (charMapping[index] == target)
            {
                index += 10;
                //10進制的高位元放入存放空間   (權重*1)
                seed[0] = index / 10;

                //10進制的低位元*9後放入存放空間 (權重*9)
                seed[1] = (index % 10) * 9;

                break;
            }
        }
        for (int index = 2; index < 10; index++) //(權重*8~1)
        {   //將剩餘數字乘上權數後放入存放空間                
            seed[index] = Convert.ToInt32(id.Substring(index - 1, 1)) * (10 - index);
        }
        //檢查是否符合檢查規則，10減存放空間所有數字和除以10的餘數的個位數字是否等於檢查碼            
        //(10 - ((seed[0] + .... + seed[9]) % 10)) % 10 == 身分證字號的最後一碼   
        var result = (10 - (seed.Sum() % 10)) % 10 == Convert.ToInt32(id.Substring(9, 1));
        result = result && id != "P88882300A";
        return result;
    }
    
    /// <summary>
    /// 檢核中華民國外僑及大陸人士在台居留證(舊式+新式)
    /// </summary>
    /// <param name="idNo">身分證</param>
    /// <returns></returns>
    public static bool CheckResidentID(string idNo)
    {
        if (idNo == null)
            return false;
        idNo = idNo.ToUpper();
        var regex = new Regex(@"^([A-Z])(A|B|C|D|8|9)(\d{8})$");
        var match = regex.Match(idNo);
        if (!match.Success)
        {
            return false;
        }

        return "ABCD".IndexOf(match.Groups[2].Value, StringComparison.Ordinal) >= 0 ? 
            CheckOldResidentID(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value) : //舊式
            CheckNewResidentID(match.Groups[1].Value, match.Groups[2].Value + match.Groups[3].Value); //新式(2021/01/02)正式生效
    }

    /// <summary>
    /// 舊式檢核
    /// </summary>
    /// <param name="firstLetter">第1碼英文字母(區域碼)</param>
    /// <param name="secondLetter">第2碼英文字母(性別碼)</param>
    /// <param name="num">第3~9流水號 + 第10碼檢查碼</param>
    /// <returns></returns>
    private static bool CheckOldResidentID(string firstLetter, string secondLetter, string num)
    {
        //建立字母對應表(A~Z)
        //A=10 B=11 C=12 D=13 E=14 F=15 G=16 H=17 J=18 K=19 L=20 M=21 N=22
        //P=23 Q=24 R=25 S=26 T=27 U=28 V=29 X=30 Y=31 W=32  Z=33 I=34 O=35 
        var alphabet = "ABCDEFGHJKLMNPQRSTUVXYWZIO";
        var transferIdNo =
            $"{alphabet.IndexOf(firstLetter) + 10}" +
            $"{(alphabet.IndexOf(secondLetter) + 10) % 10}" +
            $"{num}";
        var idNoArray = transferIdNo.ToCharArray()
            .Select(c => Convert.ToInt32(c.ToString()))
            .ToArray();

        var sum = idNoArray[0];
        var weight = new[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 1 };
        for (var i = 0; i < weight.Length; i++)
        {
            sum += weight[i] * idNoArray[i + 1];
        }
        return (sum % 10 == 0);
    }
    /// <summary>
    /// 新式檢核
    /// </summary>
    /// <param name="firstLetter">第1碼英文字母(區域碼)</param>
    /// <param name="num">第2碼(性別碼) + 第3~9流水號 + 第10碼檢查碼</param>
    /// <returns></returns>
    private static bool CheckNewResidentID(string firstLetter, string num)
    {
        //建立字母對應表(A~Z)
        //A=10 B=11 C=12 D=13 E=14 F=15 G=16 H=17 J=18 K=19 L=20 M=21 N=22
        //P=23 Q=24 R=25 S=26 T=27 U=28 V=29 X=30 Y=31 W=32  Z=33 I=34 O=35 
        var alphabet = "ABCDEFGHJKLMNPQRSTUVXYWZIO";
        var transferIdNo = $"{(alphabet.IndexOf(firstLetter, StringComparison.Ordinal) + 10)}" +
                              $"{num}";
        int[] idNoArray = transferIdNo.ToCharArray()
            .Select(c => Convert.ToInt32(c.ToString()))
            .ToArray();

        var sum = idNoArray[0];
        int[] weight = new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 1 };
        for (var i = 0; i < weight.Length; i++)
        {
            sum += (weight[i] * idNoArray[i + 1]) % 10;
        }
        return (sum % 10 == 0);
    }

    /// <summary>
    /// 數字轉正楷中文
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static string FormatChineseNumber(long number)
    {
        var numberText = new List<string>
        {
            "零", "壹", "貳", "參", "肆", "伍", "陸", "柒", "捌", "玖"
        };

        var unitText = new List<string>
        {
            "", "拾", "佰", "仟"
        };

        var groupText = new List<string>
        {
            "", "萬", "億", "兆"
        };

        var result = number.ToString()
            // 反轉
            .Reverse()
            // 轉中文: 10 -> 壹零
            .Select(it => numberText[it - '0'])
            // 分組並加上單位: 壹零壹 -> 壹『佰』零壹
            .Select((it, i) => new
            {
                group = i / 4,
                text = it + (it == "零" ? "" : unitText[i % 4])
            })
            // 轉回來
            .Reverse()
            // 分組處理『億、萬』
            .GroupBy(it => it.group)
            .Aggregate(((bool prevGroupZero, string text)) (false, ""),
                (r, it) =>
                {
                    var temp = it
                        .Aggregate(((bool prevZero, string text)) (false, ""),
                            (rr, itt) =>
                            {
                                if (itt.text == "零")
                                    return (true, rr.text);

                                // 將非零項目加入字串
                                return (false, rr.text + (rr.prevZero ? "零" : "") + itt.text);
                            });

                    // 處理群組
                    if (temp.text == "")
                        return (true, r.text);

                    // 將非零群組加入字串
                    return (false, r.text + (r.prevGroupZero && !temp.text.StartsWith("零") ? "零" : "") + temp.text + groupText[it.Key]);
                });

        return result.text == "" ? "零" : result.text;
    }


    /// <summary>
    /// 星期轉數字
    /// </summary>
    /// <param name="DayOfWeek"></param>
    /// <returns></returns>
    public static byte ConvertDayofWeekToInt(string DayOfWeek)
    {
        return DayOfWeek switch
        {
            "Monday" => 1,
            "Tuesday" => 2,
            "Wednesday" => 3,
            "Thursday" => 4,
            "Friday" => 5,
            "Saturday" => 6,
            "Sunday" => 0,
            _ => 0
        };
    }

    /// <summary>
    /// 字串轉List
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static List<string> DeserializeStringToList(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return new List<string>();
        }

        // 如果字符串不包含 '[' 和 ']'，则直接作为单个元素添加到列表中
        if (!value.StartsWith("[") && !value.EndsWith("]"))
        {
            return new List<string> { value };
        }

        return JsonConvert.DeserializeObject<List<string>>(value);
    }

    /// <summary>
    /// 取得類別名稱
    /// </summary>
    /// <param name="category"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static string GetSystemcodeName(int category, string code)
    {
        var name = new SystemcodeRepository().DataSet
            .FirstOrDefault(c => c.Category == category && c.Code == code)?.Display;
        return name;
    }

    public static string ChangOName(string name)
    {
        var pname = "";
        switch (string.IsNullOrEmpty(name))
        {
            case false:
                try
                {
                    switch (name.Length)
                    {
                        case 2:
                            pname = name.Substring(0, 1) + "O";
                            break;
                        case 3:
                            pname = name.Substring(0, 1) + "O" + name.Substring(2, 1);
                            break;
                        case 4:
                            pname = name.Substring(0, 2) + "O" + name.Substring(3, 1);
                            break;
                        case 6:
                            pname = name.Substring(0, 1) + "O" + name.Substring(3, 3);
                            break;
                        default:
                            pname = name;
                            break;
                    }
                }
                catch
                {
                    // ignored
                }

                break;
        }
        return pname;
    }

    public static string ChangXXXId(string pid)
    {
        try
        {
            if (string.IsNullOrEmpty("pid") && pid.Length >= 10)
                pid = pid.Substring(0, 7) + "xxx";
        }
        catch
        {
            // ignored
        }

        return pid;
    }
    public static string GetGenderName(string code)
    {
        var ret = string.Empty;
        switch (code)
        {
            case "male":
                ret = "男";
                break;
            case "female":
                ret = "女";
                break;
            case "unknown":
                ret = "未知的性別";
                break;
        }

        return ret;
    }

    /// <summary>
    /// 取得年齡
    /// </summary>
    /// <param name="birthdate">生日</param>
    /// <param name="senddate">送件日</param>
    /// <returns>age</returns>
    public static string GetAge(DateTime birthdate, DateTime senddate)
    {
        var year = birthdate.Year;
        var _year = senddate.Year;
        var age = _year - year;
        if (birthdate > senddate.AddYears(-age)) age--;
        return age.ToString();
    }

    public static string ChangXXXTel(string phone)
    {
        try
        {
            if (!string.IsNullOrEmpty(phone))
            {
                if (phone.Length >= 10)
                    phone = phone.Substring(0, 7).Trim() + "xxx";
                else if (phone.Length <= 7 || phone.Length >= 3)
                    phone = phone.Substring(0, 3) + "xxx";
            }
        }
        catch
        {
            // ignored
        }

        return phone;
    }
    /// <summary>
    /// To the simple taiwan date.
    /// </summary>
    /// <param name="datetime">The datetime.</param>
    /// <returns></returns>
    public static string ToSimpleTaiwanDate(this DateTime datetime)
    {
        try
        {
            var taiwanCalendar = new TaiwanCalendar();
            var result = $"{taiwanCalendar.GetYear(datetime)}.{datetime.Month}.{datetime.Day.ToString().PadLeft(2, '0')}";
            return result;
        }
        catch { return ""; }
    }

    public static string ImgToBase64(string imgName)
    {
        var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
        var pathName = Path.Combine(basePath!, "Images");
        var imageArray = System.IO.File.ReadAllBytes(pathName + $"\\{imgName}");
        var base64ImageRepresentation = Convert.ToBase64String(imageArray);
        return "data:image/png;base64," + base64ImageRepresentation;
    }

    /// <summary>
    /// CheckSuper
    /// </summary>
    /// <param name="AccountNo"></param>
    /// <returns></returns>
    public static bool CheckSuper(string AccountNo)
    {
        var acc = new AccountRepository().DataSet.First(c => c.No == AccountNo);
        return acc.IsSuper;
    }
}