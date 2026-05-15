using DataCom.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using Api.Models;
using Api.Repositories;
using NLog;

namespace Api.Hubs
{
    /// <summary>
    /// 視訊 signalR Hub 執行方法說明，此 Hub 有監聽事件：StatusChanged(sheet):當視訊成員更動時觸發。
    /// </summary>
    //[Authorize]
    public class ConferenceHub : Hub
    {
        /// <summary>
        /// 稽核記錄取得登入帳號及用戶端IP用
        /// </summary>
        protected readonly IHttpContextAccessor Accessor;

        /// <summary>
        /// 稽核記錄服務
        /// </summary>
        protected readonly IAuditEventService AuditEventService;

        private readonly ReferralrequestRepository _repository = new();
        private readonly ConferenceGroup _conferenceGroup;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="auditEventService"></param>
        /// <param name="accessor"></param>
        public ConferenceHub(IAuditEventService auditEventService, IHttpContextAccessor accessor)
        {
            Accessor = accessor;
            AuditEventService = auditEventService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        [AllowAnonymous]
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await _conferenceGroup.RemoveMember(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 加入視訊
        /// </summary>
        /// <param name="id">轉診單代碼</param>
        /// <returns></returns>
        public async Task<HubMethodResult> Join(string id)
        {
            try
            {
                var referralrequest = await _repository.FindAsync(id);
                if (referralrequest == null)
                    return new HubMethodResult { success = false, message = "無此視訊會議代碼!" };

                await _conferenceGroup.AddMember(Context.ConnectionId, id);

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return new HubMethodResult { success = false, message = ex.Message };
            }

            return new HubMethodResult { success = true };
        }

        ///// <summary>
        ///// 退出視訊
        ///// </summary>
        ///// <param name="id">轉診單代碼</param>
        ///// <returns></returns>
        //public async Task<HubMethodResult> Exit(string id)
        //{
        //    try
        //    {
        //        var referralrequest = await _repository.FindAsync(id);
        //        if (referralrequest == null)
        //            return new HubMethodResult { success = false, message = "無此會診單號!" };

        //        await _conferenceGroup.RemoveMember(Context.ConnectionId, id);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex.Message);
        //        return new HubMethodResult { success = false, message = ex.Message };
        //    }

        //    return new HubMethodResult { success = true };
        //}
    }
}
