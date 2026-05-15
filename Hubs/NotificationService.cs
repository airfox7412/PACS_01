using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Api.Hubs
{
    /// <summary>
    /// 視訊狀態通知服務
    /// </summary>
    public class NotificationService
    {
        private readonly IHubContext<MeetingHub> _hubContext;

        /// <summary>
        /// 視訊狀態通知服務
        /// </summary>
        /// <param name="hubContext"></param>
        public NotificationService(IHubContext<MeetingHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// 通知所有客户端
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendNotification(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
