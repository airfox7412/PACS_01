namespace Api.Hubs
{
    public class AccountInfoModel
    {
        /// <summary>
        /// 使用者帳號代碼
        /// </summary>
        public virtual string No { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 機構代碼
        /// </summary>
        public virtual string InstitutionNo { get; set; }

        /// <summary>
        /// 機構
        /// </summary>
        public virtual string InstitutionName { get; set; }
    }
}
