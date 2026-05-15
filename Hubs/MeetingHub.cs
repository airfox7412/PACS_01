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
    public class MeetingHub : Hub
    {
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
        public MeetingHub(IAuditEventService auditEventService, IHttpContextAccessor accessor)
        {
            Accessor = accessor;
            AuditEventService = auditEventService;
        }

        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }

        /// <summary>
        /// TEST2
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        public async Task SendMessageToGroup(string user, string message)
            => await Clients.Group("SignalR Users").SendAsync("ReceiveMessage", user, message);

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
