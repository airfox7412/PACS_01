using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Api.Entities;
using Database.Core;
using Database.Core.Enums;
using Database.Core.Helper;
using Database.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Api.Repositories;

/// <summary>
/// 稽核記錄
/// </summary>
public class AuditEventRepository : RepositoryBase<Db, AuditEvent>, IAuditEventService
{
    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="repository">引用其他存儲管理物件連線</param>
    /// <param name="accessor">稽核記錄取得登入帳號及用戶端IP用</param>
    public AuditEventRepository(IRepository repository = null, IHttpContextAccessor accessor = null) : base(repository, accessor)
    {
    }

    /// <summary>
    /// 更新物件實體前置作業
    /// </summary>
    /// <param name="entity">物件實體</param>
    /// <param name="entityState">更新類型</param>
    public override void BeforeModify(AuditEvent entity, ModifyType entityState)
    {
        if (entityState == ModifyType.Insert)
        {
            var Datakeys = entity.DataKey.Split('.');
            var DataKey = Datakeys.Length == 3 ? Datakeys[2] : entity.DataKey;
            entity.CreateTime = DateTime.Now;
            entity.AccountNo ??= entity.Type is (short)AuditEventType.Login or (short)AuditEventType.LoginFail
                ? DataKey
                : Accessor?.HttpContext?.User.Identity?.Name;
        }
    }

    /// <summary>
    /// 建立稽核記錄
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type">記錄類型</param>
    /// <param name="model">記錄物件</param>
    /// <param name="pKey">記錄搜尋主鍵</param>
    /// <param name="processStack"></param>
    /// <returns>ExamTracking.</returns>
    private async Task Add<T>(short type, T model, object pKey = null, string processStack = "")
    {
        if (type == (short)AuditEventType.Read)
        {
            Logger.Trace(
                $"使用者:{Accessor?.HttpContext?.User.Identity?.Name}, 用戶端IP:{ApiHelper.GetClientIp(Accessor)}, 讀取方法:{ApiHelper.GetProcessStack()}, 資料類別:{typeof(T).Name}, 資料內容:{JsonConvert.SerializeObject(model)}");
        }
        else
            await AddSerializeObject(type, JsonConvert.SerializeObject(model), typeof(T).Name, pKey, processStack);
    }

    /// <summary>
    /// 建立稽核記錄
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type">記錄類型</param>
    /// <param name="model">記錄物件</param>
    /// <param name="pKey">記錄搜尋主鍵</param>
    /// <param name="processStack"></param>
    /// <returns>ExamTracking.</returns>
    async Task IAuditEventService.Add<T>(AuditEventType type, T model, object pKey, string processStack)
    {
        await Add((short)type, model, pKey, processStack);
    }

    /// <summary>
    /// 建立稽核記錄
    /// </summary>
    /// <param name="type">記錄類型</param>
    /// <param name="modelString">資料物件值或JSON</param>
    /// <param name="dataTypeName">資料物件型別</param>
    /// <param name="pKey">記錄搜尋主鍵</param>
    /// <param name="processStack">程序堆疊(API or Function Name)</param>
    /// <returns>ExamTracking.</returns>
    private async Task AddSerializeObject(short type, string modelString, string dataTypeName = "", object pKey = null, string processStack = "")
    {
        try
        {
            if (string.IsNullOrEmpty(processStack))
            {
                var st = new StackTrace(false);
                var stackFrame = st.GetFrames()
                    .Where(s =>
                    {
                        var typeAssembly = s.GetMethod()?.ReflectedType?.Assembly;
                        var projectAssembly = GetType().Assembly;
                        return typeAssembly != null && typeAssembly == projectAssembly;
                    }).Last();
                var method = stackFrame.GetMethod();
                if (method is not null)
                    processStack = method.Name == "MoveNext"
                        ? method.ReflectedType?.Name.Split('>')[0].TrimStart('<')
                        : method.Name;
            }

            var auditEvent = new AuditEvent
            {
                Type = type,
                Data = modelString,
                AccountNo = Accessor?.HttpContext?.User.Identity?.Name,
                HostAddress = Accessor?.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                ProcessStack = processStack
            };
            if (!string.IsNullOrEmpty(dataTypeName))
                auditEvent.DataType = dataTypeName;
            if (pKey != null)
                auditEvent.DataKey =
                    pKey is IEnumerable enumerable ? string.Join(",", enumerable) : pKey.ToString();

            await InsertAsync(auditEvent);
        }
        catch (Exception ex)
        {
            Logger.Error($"寫入稽核記錄錯誤:{ex.Message}, data:{modelString}");
        }
    }

}