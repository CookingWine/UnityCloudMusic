namespace CloudMusic
{
    ///<summary>api</summary>
    public class CloudMusicAPI
    {
        /// <summary>请求的服务器地址</summary>
        public static string RequestUrl { get { return "http://findwind.cn/music/"; } }

        public static string Cookie { get; set; } = string.Empty;
    }
}
