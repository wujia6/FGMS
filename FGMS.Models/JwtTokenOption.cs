namespace FGMS.Models
{
    public sealed class JwtTokenOption
    {
        /// <summary>
        /// 获取或者设置接受者。
        /// </summary>
        public string? Audience { get; set; }
        /// <summary>
        /// 获取或者设置加密 key。
        /// </summary>
        public string? SecurityKey { get; set; }
        /// <summary>
        /// 获取或者设置发布者
        /// </summary>
        public string? Issuer { get; set; }
        /// <summary>
        /// TOKEN过期时间（分钟）
        /// </summary>
        public int ExpireMinutes { get; set; }
    }
}
