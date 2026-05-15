using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Repositories;
using Quartz;
using NLog;

namespace Api.Jobs;

/// <summary>
/// 檢查帳號是否三個月以上未使用
/// </summary>
public class AccountStatusJob : IJob
{
    private static Logger Logger => LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 檢查是否變更狀態
    /// </summary>
    /// <param name="context"></param>
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var accRep = new AccountRepository();
            var accs = accRep.DataSet.Where(c => c.Status == 100 && c.IsSuper == false).ToList();
            foreach (var acc in accs)
            {
                var disable = false;
                if (acc.LastLoginTime != null) //上次登入時間超過3個月
                {
                    var difference = DateTime.Now - acc.LastLoginTime.Value;
                    disable = difference.Days >= 92;
                }
                if (acc.Expiration != null) //帳號到期日
                    disable = DateTime.Now > acc.Expiration;

                if (!disable) continue;
                acc.Status = 0; //帳號停用
                await accRep.UpdateAsync(acc);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"帳號檢查錯誤: {ex}");
        }
    }
}