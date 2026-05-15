using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Hubs
{
    public class ConferenceGroup
    {
        /// <summary>
        /// 一個會診單會起一個視訊
        /// </summary>
        public static readonly Dictionary<string, IList<string>> Groups = new();
        private readonly Hub _hub;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hub"></param>
        public ConferenceGroup(Hub hub)
        {
            _hub = hub;
        }

        /// <summary>
        /// 加入視訊群組
        /// </summary>
        /// <param name="meetingId">視訊會議代碼</param>
        /// <param name="connectionId"></param>
        public async Task AddMember(string connectionId, string meetingId)
        {
            if (!Groups.ContainsKey(meetingId))
                Groups.Add(meetingId, new List<string>());

            if (Groups[meetingId].All(i => i != connectionId))
            {
                Groups[meetingId].Add(connectionId);
                await _hub.Groups.AddToGroupAsync(connectionId, meetingId);
            }
        }

        /// <summary>
        /// 退出視訊群組
        /// </summary>
        /// <param name="meetingId">會診單號</param>
        /// <param name="connectionId"></param>
        public async Task<string> RemoveMember(string connectionId, string meetingId = null)
        {
            if (meetingId == null)
            {
                foreach (var group in Groups)
                {
                    var connectionInfo = group.Value.FirstOrDefault(i => i == connectionId);
                    if (connectionInfo != null)
                    {
                        meetingId = group.Key;
                        group.Value.Remove(connectionInfo);
                        await _hub.Groups.RemoveFromGroupAsync(connectionId, group.Key);
                        break;
                    }
                }
            }
            else if (Groups.ContainsKey(meetingId))
            {
                var connectionInfo = Groups[meetingId].FirstOrDefault(i => i == connectionId);
                if (connectionInfo != null)
                {
                    Groups[meetingId].Remove(connectionInfo);
                    await _hub.Groups.RemoveFromGroupAsync(connectionId, meetingId);
                }

            }

            if (meetingId != null && !Groups[meetingId].Any()) Groups.Remove(meetingId);

            return meetingId;
        }

    }
}
