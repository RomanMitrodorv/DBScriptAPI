namespace DBScriptDeployment.Models
{
    public class TokenModel
    {
        public string Token { get; set; }
        public string Server { get; set; }
        public string Username { get; set; }
        public DateTime ExpiresTime { get; set; }
    }
}
