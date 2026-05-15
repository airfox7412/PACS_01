using Microsoft.AspNetCore.DataProtection;

namespace Api.Security
{
    /// <summary>
    /// 資料保護服務類
    /// </summary>
    public interface IDataProtectionService
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        string Protect(string purpose, string data);

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        string Unprotect(string purpose, string data);
    }
    
    /// <summary>
    /// 處理資料保護邏輯
    /// </summary>
    public class DataProtectionService : IDataProtectionService
    {
        private readonly IDataProtectionProvider _provider;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        public DataProtectionService(IDataProtectionProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Protect(string purpose, string data)
        {
            var protector = _provider.CreateProtector(purpose);
            return protector.Protect(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="protectedData"></param>
        /// <returns></returns>
        public string Unprotect(string purpose, string protectedData)
        {
            var protector = _provider.CreateProtector(purpose);
            return protector.Unprotect(protectedData);
        }
    }
}
