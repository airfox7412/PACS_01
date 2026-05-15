using System.Collections.Generic;
using System;

namespace Api.Library
{
    /// <summary>
    /// 處理 Exception 的輔助工具
    /// </summary>
    public static class ExceptionHelper
    {
        /// <summary>
        /// 擷取 Exception 拋出的重要訊息
        /// </summary>
        public static string GetExceptionDetail(Exception ex)
        {
            try
            {
                // 儲存 ex.Message
                List<string> validTrace = new List<string>();
                validTrace.Add(ex.Message);

                // 去除 ex.StackTrace 的多餘訊息
                string[] stackTraceSplit = ex.StackTrace.Split('\n');
                foreach (string trace in stackTraceSplit)
                {
                    // 儲存必要的 StackTrace 訊息
                    if (trace.Contains(":line"))
                    {
                        validTrace.Add(trace.Trim());
                    }
                }

                // 儲存 ex.InnerException
                string innerException = $"{ex.InnerException}";
                validTrace.Add(innerException);

                // 重組錯誤訊息
                return string.Join("\n", validTrace);
            }
            catch (Exception)
            {
                return $"{ex.Message} & {ex.InnerException}\n{ex.StackTrace}";
            }
        }
    }
}

