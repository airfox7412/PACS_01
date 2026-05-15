using System.Collections.Generic;

namespace Api.Hubs
{
    public class ConnectionInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string Connection { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public AccountInfoModel Account { get; set; }

        /// <summary>
        /// 目前線上視訊的會診單代碼
        /// </summary>
        public virtual ICollection<long> JoinSheetIds { get; set; } = new SortedSet<long>();
    }
}
