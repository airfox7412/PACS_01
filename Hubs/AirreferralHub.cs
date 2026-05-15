using DataCom.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Linq;
using Api.Models;
using Api.Repositories;
using NLog;

namespace Api.Hubs
{
    /// <summary>
    /// signalR Hub 監聽
    /// </summary>
    public class AirreferralHub : Hub
    {
        private string[] StatusStr = new[] {"Draft", "Requested", "Accepted"};
        /// <summary>
        /// 稽核記錄取得登入帳號及用戶端IP用
        /// </summary>
        protected readonly IHttpContextAccessor Accessor;

        /// <summary>
        /// 稽核記錄服務
        /// </summary>
        protected readonly IAuditEventService AuditEventService;

        private readonly ReferralrequestRepository rrRep = new();
        private static string connectionId = "";
        private static int count = 15;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="auditEventService"></param>
        /// <param name="accessor"></param>
        public AirreferralHub(IAuditEventService auditEventService, IHttpContextAccessor accessor)
        {
            Accessor = accessor;
            AuditEventService = auditEventService;
        }

        /// <summary>
        /// 檢查是否有申請單
        /// </summary>
        /// <param name="OrgId"></param>
        /// <returns></returns>
        public async Task<HubMethodResult> CheckNewEvent(string OrgId = null)
        {
            _logger.Info($"Id={connectionId}, count={count}");
            if (connectionId != Context.ConnectionId)
            {
                connectionId = Context.ConnectionId;
                count = 15;
            }
            else
            {
                count--;
                if (count <= 0)
                {
                    connectionId = "";
                    count = 15;
                }
                return new HubMethodResult {success = true, message = "檢查完成"};
            }

            var msg = "";
            var eDateTime = DateTime.Now;
            var sDateTime = eDateTime.AddMinutes(-30);
            switch (OrgId)
            {
                case null:
                    return new HubMethodResult {success = false, message = "沒有條件資料"};
                case "AEOC":
                {
                    var rrdata = rrRep.DataSet.Any(c => c.Status == "Draft" && c.DraftDate.Value.Date == eDateTime.Date);
                    if (rrdata)
                        msg = "有申請單";
                    break;
                }
                default:
                {
                    var rrdata = rrRep.DataSet.Any(c => StatusStr.Contains(c.Status) &&
                                                        c.ReferralOrganizationId.Contains(OrgId) &&
                                                        c.DraftDate >= sDateTime && c.DraftDate <= eDateTime);
                    msg = rrdata ? "有申請單" : "沒有申請單";
                    break;
                }
            }
            await Clients.Caller.SendAsync("ReceiveMessage", msg);
            _logger.Info($"ReceiveMessage=>{msg}");
            return new HubMethodResult { success = true, message = "檢查完成" };
        }
        
        /// <summary>
        /// TEST1
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        public async Task SendMessageToCaller(string user, string message)
            => await Clients.Caller.SendAsync("ReceiveMessage", user, message);
        
        /// <summary>
        /// 斷線
        /// </summary>
        /// <param name="exception"></param>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
